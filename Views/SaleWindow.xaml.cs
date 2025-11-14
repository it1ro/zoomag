using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Zoomag.Data;
using Zoomag.Models;
using System.Linq;
using System.Collections.Generic;

namespace Zoomag.Views
{
    public partial class SaleWindow : Window
    {
        private List<Zoomag.Models.Product> _allProducts;
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
                    _allProducts = context.Product.ToList();
                    _allCategories = context.Category.ToList();
                    _allUnits = context.Unit.ToList();

                    // Добавляем информацию о категориях и единицах измерения
                    foreach (var product in _allProducts)
                    {
                        var category = _allCategories.FirstOrDefault(c => c.Id == product.Category.Id);
                        var unit = _allUnits.FirstOrDefault(u => u.Id == product.Unit.Id);
                        product.Category.Name = category?.Name ?? "Без категории";
                        product.Unit.Name = unit?.Name ?? "шт";
                    }

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
                p.Category.Name.ToLower().Contains(searchTerm)
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

            var selectedProduct = (Zoomag.Models.Product)ProductListGrid.SelectedItem;

            if (selectedProduct.Amount <= 0)
            {
                MessageBox.Show("Товар закончился на складе!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, есть ли уже этот товар в чеке
            var existingItem = _receiptItems.FirstOrDefault(r => r.ProductId == selectedProduct.Id);

            if (existingItem != null)
            {
                // Если товар уже есть в чеке, увеличиваем количество
                existingItem.Quantity++;
                existingItem.Total = existingItem.Quantity * existingItem.Price;
            }
            else
            {
                // Если товара нет в чеке, добавляем новую позицию
                _receiptItems.Add(new ReceiptItem
                {
                    ProductId = selectedProduct.Id,
                    Name = selectedProduct.Name,
                    Price = selectedProduct.Price,
                    Quantity = 1,
                    Total = selectedProduct.Price
                });
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

            // Проверяем наличие товаров на складе
            using (var context = new AppDbContext())
            {
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
                    return;
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

                // Создаем запись о продаже
                var sale = new Sale
                {
                    Name = $"Продажа от {DateTime.Now:dd.MM.yyyy HH:mm}",
                    Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                    Price = _receiptItems.Sum(r => r.Total),
                    Amount = _receiptItems.Sum(r => r.Quantity)
                };

                context.Sale.Add(sale);
                context.SaveChanges();

                // Создаем связи продажа-товар
                foreach (var receiptItem in _receiptItems)
                {
                    var saleItem = new SaleItem
                    {
                        SaleId = sale.Id,
                        ProductId = receiptItem.ProductId
                    };
                    context.SaleItem.Add(saleItem);
                }

                context.SaveChanges();
            }

            MessageBox.Show($"Покупка успешно оформлена!\nОбщая сумма: {_receiptItems.Sum(r => r.Total)} руб.",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            _receiptItems.Clear();
            UpdateReceiptDisplay();
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

        private void ShowCategoryFilter(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция фильтрации по категории будет реализована в следующей версии.",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
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

    public class ReceiptItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public int Total { get; set; }
    }
}
