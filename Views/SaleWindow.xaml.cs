using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Zoomag.Data;
using Zoomag.Models;
using System.Linq;

namespace Zoomag.Views
{
    public partial class SaleWindow : Window
    {
        private List<Zoomag.Models.Product> allProducts;
        private List<ReceiptItem> receiptItems;  // ✅ ReceiptItem, не SupplyItem
        private List<Category> allCategories;
        private List<Unit> allUnits;

        public SaleWindow()
        {
            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            receiptItems = new List<ReceiptItem>();
            LoadAllData();
        }

        private void LoadAllData()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    allProducts = context.Product.ToList();
                    allCategories = context.Category.ToList();
                    allUnits = context.Unit.ToList();

                    // Добавляем информацию о категориях и единицах измерения
                    foreach (var product in allProducts)
                    {
                        var category = allCategories.FirstOrDefault(c => c.Id == product.Category.Id);
                        var unit = allUnits.FirstOrDefault(u => u.Id == product.Unit.Id);
                        product.Category.Name = category?.Name ?? "Без категории";
                        product.Unit.Name = unit?.Name ?? "шт";
                    }

                    ProductsGrid.ItemsSource = allProducts;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterProducts();
        }

        private void FilterProducts()
        {
            string searchTerm = SearchTextBox.Text?.Trim().ToLower() ?? "";

            var filteredProducts = allProducts.Where(p =>
                string.IsNullOrEmpty(searchTerm) ||
                p.Name.ToLower().Contains(searchTerm) ||
                p.Category.Name.ToLower().Contains(searchTerm)  // ✅ CategoryName
            ).ToList();

            ProductsGrid.ItemsSource = filteredProducts;
        }

        private void AddToReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите товар для добавления в чек!",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectedProduct = (Zoomag.Models.Product)ProductsGrid.SelectedItem;

            if (selectedProduct.Amount <= 0)
            {
                MessageBox.Show("Товар закончился на складе!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, есть ли уже этот товар в чеке
            var existingItem = receiptItems.FirstOrDefault(r => r.ProductId == selectedProduct.Id);

            if (existingItem != null)
            {
                // Если товар уже есть в чеке, увеличиваем количество
                existingItem.Quantity++;
                existingItem.Total = existingItem.Quantity * existingItem.Price;
            }
            else
            {
                // Если товара нет в чеке, добавляем новую позицию
                receiptItems.Add(new ReceiptItem  // ✅ ReceiptItem
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

        private void RemoveFromReceipt_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dataGridRow = FindParent<DataGridRow>(button);
            var receiptItem = dataGridRow?.DataContext as ReceiptItem;  // ✅ ReceiptItem

            if (receiptItem != null)
            {
                receiptItems.Remove(receiptItem);
                UpdateReceiptDisplay();
            }
        }

        private void ClearReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (receiptItems.Count == 0)
            {
                MessageBox.Show("Чек пуст!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите очистить чек?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                receiptItems.Clear();
                UpdateReceiptDisplay();
            }
        }

        private void Buy_Click(object sender, RoutedEventArgs e)
        {
            if (receiptItems.Count == 0)
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

                foreach (var receiptItem in receiptItems)
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
                foreach (var receiptItem in receiptItems)
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
                    Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),  // ✅ Правильное имя
                    Price = receiptItems.Sum(r => r.Total),  // ✅ int
                    Amount = receiptItems.Sum(r => r.Quantity)  // ✅ int, не строка
                };

                context.Sale.Add(sale);
                context.SaveChanges();

                // Создаем связи продажа-товар
                foreach (var receiptItem in receiptItems)
                {
                    var saleItem = new SaleItem  // ✅ SaleItem
                    {
                        SaleId = sale.Id,
                        ProductId = receiptItem.ProductId
                    };
                    context.SaleItem.Add(saleItem);  // ✅ DbSet<SaleItem>
                }

                context.SaveChanges();
            }

            MessageBox.Show($"Покупка успешно оформлена!\nОбщая сумма: {receiptItems.Sum(r => r.Total)} руб.",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            receiptItems.Clear();
            UpdateReceiptDisplay();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow main = new AdminWindow();
            this.Hide();
            main.Show();
        }

        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            LoadAllData();
            SearchTextBox.Clear();
        }

        private void FilterByCategory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция фильтрации по категории будет реализована в следующей версии.",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateReceiptDisplay()
        {
            ReceiptGrid.ItemsSource = receiptItems;

            int itemCount = receiptItems.Sum(r => r.Quantity);
            int totalAmount = receiptItems.Sum(r => r.Total);

            ItemCountText.Text = itemCount.ToString();
            TotalAmountText.Text = totalAmount.ToString();
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
