namespace Zoomag.Views;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Data;
using Microsoft.EntityFrameworkCore;
using Models;

public partial class SaleWindow : Window
{
    private List<Category> _allCategories;
    private List<ProductDisplayDto> _allProducts;
    private List<Unit> _allUnits;
    private List<ReceiptItem> _receiptItems;

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
                var products = context.Product
                    .Include(p => p.Category)
                    .Include(p => p.Unit)
                    .ToList();

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
        var searchTerm = SearchInput.Text?.Trim().ToLower() ?? "";
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

        var existingItem = _receiptItems.FirstOrDefault(r => r.ProductId == selectedProductDto.Id);
        if (existingItem != null)
        {
            existingItem.Quantity++;
            existingItem.Total = existingItem.Quantity * existingItem.Price;
            ReceiptItemsGrid.Items.Refresh();
        }
        else
        {
            _receiptItems.Add(new ReceiptItem
            {
                ProductId = selectedProductDto.Id,
                Name = selectedProductDto.Name,
                Price = selectedProductDto.Price,
                Quantity = 1,
                Total = selectedProductDto.Price
            });
            ReceiptItemsGrid.Items.Refresh();
        }

        selectedProductDto.Qty--;
        ProductListGrid.Items.Refresh();

        UpdateReceiptDisplay();
    }

    private void RemoveItemFromReceipt(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var dataGridRow = FindParent<DataGridRow>(button);
        var receiptItem = dataGridRow?.DataContext as ReceiptItem;
        if (receiptItem != null)
        {
            var productDto = _allProducts.FirstOrDefault(p => p.Id == receiptItem.ProductId);
            if (productDto != null) productDto.Qty += receiptItem.Quantity;

            _receiptItems.Remove(receiptItem);
            ReceiptItemsGrid.Items.Refresh();
            ProductListGrid.Items.Refresh();
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
            foreach (var receiptItem in _receiptItems)
            {
                var productDto = _allProducts.FirstOrDefault(p => p.Id == receiptItem.ProductId);
                if (productDto != null) productDto.Qty += receiptItem.Quantity;
            }

            _receiptItems.Clear();
            ReceiptItemsGrid.Items.Refresh();
            ProductListGrid.Items.Refresh();
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
            var hasInsufficientStock = false;
            var insufficientProducts = new List<string>();

            foreach (var receiptItem in _receiptItems)
            {
                var product = context.Product.FirstOrDefault(p => p.Id == receiptItem.ProductId);
                if (product != null && product.Amount < receiptItem.Quantity)
                {
                    hasInsufficientStock = true;
                    insufficientProducts.Add(
                        $"{product.Name} (требуется {receiptItem.Quantity}, доступно {product.Amount})");
                }
            }

            if (hasInsufficientStock)
            {
                var message = "Недостаточно товара на складе:\n" + string.Join("\n", insufficientProducts);
                MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var receiptItem in _receiptItems)
            {
                var product = context.Product.FirstOrDefault(p => p.Id == receiptItem.ProductId);
                if (product != null) product.Amount -= receiptItem.Quantity;
            }

            var sale = new Sale
            {
                Name = $"Продажа от {DateTime.Now:dd.MM.yyyy HH:mm}",
                Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                Price = _receiptItems.Sum(r => r.Total),
                Amount = _receiptItems.Sum(r => r.Quantity)
            };

            context.Sale.Add(sale);
            context.SaveChanges();

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

            MessageBox.Show($"Покупка успешно оформлена!\nОбщая сумма: {_receiptItems.Sum(r => r.Total)} руб.",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            _receiptItems.Clear();
            UpdateReceiptDisplay();
            LoadAllData();
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
        Hide();
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
        var itemCount = _receiptItems.Sum(r => r.Quantity);
        var totalAmount = _receiptItems.Sum(r => r.Total);
        ItemCountDisplay.Text = itemCount.ToString();
        TotalAmountDisplay.Text = totalAmount.ToString();
    }

    private static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parentObject = VisualTreeHelper.GetParent(child);
        if (parentObject == null) return null;
        var parent = parentObject as T;
        if (parent != null) return parent;
        return FindParent<T>(parentObject);
    }
}

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
