using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Zoomag.Data;
using Zoomag.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Zoomag.Views
{
    public partial class SaleWindow : Window
    {
        private List<ProductDisplayDto> _allProducts;
        private List<ReceiptItem> _receiptItems;
        private List<Category> _allCategories;
        private List<Unit> _allUnits;

        public SaleWindow()
        {
            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            _receiptItems = new List<ReceiptItem>();
            LoadAllData();
        }

        private void LoadAllData()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Загружаем продукты с категорией и единицей измерения
                    var products = context.Product
                        .Include(p => p.Category)
                        .Include(p => p.Unit)
                        .ToList();

                    // Создаем DTO для отображения, чтобы не изменять оригинальные сущности
                    _allProducts = products.Select(p => new ProductDisplayDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CategoryName = p.Category?.Name ?? "Без категории",
                        UnitName = p.Unit?.Name ?? "шт",
                        Price = p.Price,
                        Qty = p.Amount
                    }).ToList();

                    _allCategories = context.Category.ToList();
                    _allUnits = context.Unit.ToList();

                    ProductListGrid.ItemsSource = _allProducts;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterProducts();
        }

        private void FilterProducts()
        {
            string searchTerm = SearchInput.Text?.Trim().ToLower() ?? "";
            var filteredProducts = _allProducts.Where(p =>
                string.IsNullOrEmpty(searchTerm) ||
                p.Name.ToLower().Contains(searchTerm) ||
                p.CategoryName.ToLower().Contains(searchTerm)
            ).ToList();
            ProductListGrid.ItemsSource = filteredProducts;
        }

        private void AddSelectedProductToReceipt(object sender, RoutedEventArgs e)
        {
            if (ProductListGrid.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите товар для добавления в чек!",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectedProductDto = (ProductDisplayDto)ProductListGrid.SelectedItem;
            if (selectedProductDto.Qty <= 0)
            {
                MessageBox.Show("Товар закончился на складе!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, есть ли уже этот товар в чеке
            var existingItem = _receiptItems.FirstOrDefault(r => r.ProductId == selectedProductDto.Id);
            if (existingItem != null)
            {
                // Если товар уже есть в чеке, увеличиваем количество
                existingItem.Quantity++;
                existingItem.Total = existingItem.Quantity * existingItem.Price;
                ReceiptItemsGrid.Items.Refresh(); // Обновляем отображение
            }
            else
            {
                // Если товара нет в чеке, добавляем новую позицию
                _receiptItems.Add(new ReceiptItem
                {
                    ProductId = selectedProductDto.Id,
                    Name = selectedProductDto.Name,
                    Price = selectedProductDto.Price,
                    Quantity = 1,
                    Total = selectedProductDto.Price
                });
                ReceiptItemsGrid.Items.Refresh(); // Обновляем отображение
            }

            UpdateReceiptDisplay();
        }

        private void RemoveItemFromReceipt(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dataGridRow = FindParent<DataGridRow>(button);
            var receiptItem = dataGridRow?.DataContext as ReceiptItem;
            if (receiptItem != null)
            {
                _receiptItems.Remove(receiptItem);
                ReceiptItemsGrid.Items.Refresh(); // Обновляем отображение
                UpdateReceiptDisplay();
            }
        }

        private void ClearCurrentReceipt(object sender, RoutedEventArgs e)
        {
            if (_receiptItems.Count == 0)
            {
                MessageBox.Show("Чек пуст!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (MessageBox.Show("Вы уверены, что хотите очистить чек?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _receiptItems.Clear();
                ReceiptItemsGrid.Items.Refresh(); // Обновляем отображение
                UpdateReceiptDisplay();
            }
        }

        private void FinalizeCurrentSale(object sender, RoutedEventArgs e)
        {
            if (_receiptItems.Count == 0)
            {
                MessageBox.Show("Чек пуст! Добавьте товары перед покупкой.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var context = new AppDbContext();
            try
            {
                // Проверяем наличие товара на складе
                bool hasInsufficientStock = false;
                var insufficientProducts = new List<string>();

                foreach (var receiptItem in _receiptItems)
                {
                    var product = context.Product.FirstOrDefault(p => p.Id == receiptItem.ProductId);
                    if (product != null && product.Amount < receiptItem.Quantity)
                    {
                        hasInsufficientStock = true;
                        insufficientProducts.Add($"{product.Name} (требуется {receiptItem.Quantity}, доступно {product.Amount})");
                    }
                }

                if (hasInsufficientStock)
                {
                    string message = "Недостаточно товара на складе:\n" + string.Join("\n", insufficientProducts);
                    MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Не завершаем продажу
                }

                // Обновляем количество на складе
                foreach (var receiptItem in _receiptItems)
                {
                    var product = context.Product.FirstOrDefault(p => p.Id == receiptItem.ProductId);
                    if (product != null)
                    {
                        product.Amount -= receiptItem.Quantity;
                    }
                }

                // Создаём запись о продаже
                var sale = new Sale
                {
                    Name = $"Продажа от {DateTime.Now:dd.MM.yyyy HH:mm}",
                    Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                    Price = _receiptItems.Sum(r => r.Total),
                    Amount = _receiptItems.Sum(r => r.Quantity)
                };

                context.Sale.Add(sale);
                context.SaveChanges(); // Сохраняем продажу, чтобы получить Id

                // Создаём связи продажа-товар
                foreach (var receiptItem in _receiptItems)
                {
                    var saleItem = new SaleItem
                    {
                        SaleId = sale.Id,
                        ProductId = receiptItem.ProductId
                    };
                    context.SaleItem.Add(saleItem);
                }

                context.SaveChanges(); // Сохраняем связи

                MessageBox.Show($"Покупка успешно оформлена!\nОбщая сумма: {_receiptItems.Sum(r => r.Total)} руб.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                _receiptItems.Clear();
                UpdateReceiptDisplay();
                LoadAllData(); // Обновляем список товаров
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении покупки: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReturnToMainMenu(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            this.Hide();
            adminWindow.Show();
        }

        private void RefreshProductList(object sender, RoutedEventArgs e)
        {
            LoadAllData();
            SearchInput.Clear();
        }


        private void UpdateReceiptDisplay()
        {
            ReceiptItemsGrid.ItemsSource = _receiptItems;
            int itemCount = _receiptItems.Sum(r => r.Quantity);
            int totalAmount = _receiptItems.Sum(r => r.Total);
            ItemCountDisplay.Text = itemCount.ToString();
            TotalAmountDisplay.Text = totalAmount.ToString();
        }

        // Вспомогательный метод для поиска родительского элемента
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null) return parent;
            else return FindParent<T>(parentObject);
        }
    }

    // DTO для отображения товара в DataGrid
    public class ProductDisplayDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public string UnitName { get; set; }
        public int Price { get; set; }
        public int Qty { get; set; }
    }

    public class ReceiptItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public int Total { get; set; }
    }
}
