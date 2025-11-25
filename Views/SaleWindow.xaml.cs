using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Zoomag.Data;
using Zoomag.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace Zoomag.Views;

public partial class SaleWindow : Window
{
    private List<ProductDisplayDto> _allProducts = new();
    private ObservableCollection<ReceiptItem> _receiptItems;

    public SaleWindow()
    {
        InitializeComponent();
        _receiptItems = new ObservableCollection<ReceiptItem>();
        ReceiptItemsGrid.ItemsSource = _receiptItems; // Привязка один раз
        InitializeData();
    }

    private void InitializeData()
    {
        _receiptItems.Clear();
        LoadAllData();
    }

    private void LoadAllData()
    {
        try
        {
            using var context = new AppDbContext();
            var products = context.Product
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .Include(p => p.SupplyItems).ThenInclude(si => si.Supply)
                .Include(p => p.SaleItems)
                .ToList();

            _allProducts = products.Select(p => new ProductDisplayDto
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category?.Name ?? "Без категории",
                UnitName = p.Unit?.Name ?? "шт",
                Price = p.SupplyItems
                    .Where(si => si.Supply != null)
                    .OrderByDescending(si => si.Supply.Date)
                    .FirstOrDefault()?.Price ?? 0,
                Qty = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
            }).ToList();

            ProductListGrid.ItemsSource = _allProducts;
            UpdateReceiptSummary();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SearchInput_TextChanged(object sender, TextChangedEventArgs e) => FilterProducts();

    private void FilterProducts()
    {
        var term = SearchInput.Text?.Trim().ToLower() ?? "";
        var filtered = _allProducts.Where(p =>
            string.IsNullOrEmpty(term) ||
            p.Name.ToLower().Contains(term) ||
            p.CategoryName.ToLower().Contains(term)
        ).ToList();
        ProductListGrid.ItemsSource = filtered;
    }

    private void AddSelectedProductToReceipt(object sender, RoutedEventArgs e)
    {
        if (ProductListGrid.SelectedItem is not ProductDisplayDto dto)
        {
            MessageBox.Show("Выберите товар для добавления в чек!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (dto.Qty <= 0)
        {
            MessageBox.Show("Товар закончился на складе!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var existing = _receiptItems.FirstOrDefault(r => r.ProductId == dto.Id);
        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            _receiptItems.Add(new ReceiptItem
            {
                ProductId = dto.Id,
                Name = dto.Name,
                Price = dto.Price,
                Quantity = 1
            });
        }

        dto.Qty--;
        ProductListGrid.Items.Refresh(); // ProductDisplayDto не INPC — обновляем вручную
        UpdateReceiptSummary();
    }

    private void RemoveItemFromReceipt(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var row = FindParent<DataGridRow>(button);
        var item = row?.DataContext as ReceiptItem;
        if (item == null) return;

        var dto = _allProducts.First(p => p.Id == item.ProductId);
        dto.Qty += item.Quantity;
        _receiptItems.Remove(item);
        ProductListGrid.Items.Refresh();
        UpdateReceiptSummary();
    }

    private void ClearCurrentReceipt(object sender, RoutedEventArgs e)
    {
        if (_receiptItems.Count == 0) return;

        if (MessageBox.Show("Очистить чек?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            foreach (var item in _receiptItems.ToList()) // ToList() — избегаем модификации при итерации
            {
                var dto = _allProducts.First(p => p.Id == item.ProductId);
                dto.Qty += item.Quantity;
            }
            _receiptItems.Clear();
            ProductListGrid.Items.Refresh();
            UpdateReceiptSummary();
        }
    }

    private void FinalizeCurrentSale(object sender, RoutedEventArgs e)
    {
        if (_receiptItems.Count == 0)
        {
            MessageBox.Show("Чек пуст!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();
        try
        {
            foreach (var receiptItem in _receiptItems)
            {
                var currentStock = context.Product
                    .Where(p => p.Id == receiptItem.ProductId)
                    .Select(p => p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity))
                    .FirstOrDefault();

                if (currentStock < receiptItem.Quantity)
                {
                    var name = context.Product.Find(receiptItem.ProductId)?.Name ?? "Неизвестно";
                    MessageBox.Show($"Недостаточно товара: {name} (требуется {receiptItem.Quantity}, доступно {currentStock})", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var sale = new Sale
            {
                Name = $"Продажа от {DateTime.Now:dd.MM.yyyy HH:mm}",
                Date = DateTime.Now
            };
            context.Sale.Add(sale);
            context.SaveChanges();

            foreach (var item in _receiptItems)
            {
                context.SaleItem.Add(new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            context.SaveChanges();

            MessageBox.Show($"Покупка оформлена! Сумма: {_receiptItems.Sum(r => r.Total)} руб.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            _receiptItems.Clear();
            LoadAllData(); // перезагружаем остатки
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при оформлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ReturnToMainMenu(object sender, RoutedEventArgs e)
    {
        new AdminWindow().Show();
        Close();
    }

    private void RefreshProductList(object sender, RoutedEventArgs e)
    {
        LoadAllData();
        SearchInput.Clear();
    }

    private void UpdateReceiptSummary()
    {
        ItemCountDisplay.Text = _receiptItems.Sum(r => r.Quantity).ToString();
        TotalAmountDisplay.Text = _receiptItems.Sum(r => r.Total).ToString();
    }

    private static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        return parent switch
        {
            null => null,
            T p => p,
            _ => FindParent<T>(parent)
        };
    }
}

public class ProductDisplayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public string UnitName { get; set; } = null!;
    public int Price { get; set; }
    public int Qty { get; set; }
}
