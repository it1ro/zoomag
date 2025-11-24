using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Zoomag.Data;
using Zoomag.Models;
using Microsoft.EntityFrameworkCore;

namespace Zoomag.Views;

public partial class StockWindow : Window
{
    private readonly AppDbContext _context;
    private List<StockItemView> _allItems = new();

    public StockWindow()
    {
        InitializeComponent();
        _context = new AppDbContext();
        LoadData();
        LoadCategories();
    }

    private void LoadData()
    {
        var products = _context.Product
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .OrderBy(p => p.Name)
            .ToList();

        _allItems = products.Select(p => new StockItemView
        {
            Id = p.Id,
            Name = p.Name,
            CategoryName = p.Category?.Name ?? "—",
            UnitName = p.Unit?.Name ?? "—",
            Price = p.Price,
            Amount = p.Amount,
            CategoryId = p.CategoryId
        }).ToList();

        ApplyFilter();
    }

    private void LoadCategories()
    {
        var categories = _context.Category.OrderBy(c => c.Name).ToList();
        CategoryFilter.Items.Clear();
        CategoryFilter.Items.Add(new ComboBoxItem { Content = "Все категории", Tag = -1 });
        foreach (var cat in categories)
        {
            CategoryFilter.Items.Add(new ComboBoxItem { Content = cat.Name, Tag = cat.Id });
        }
        CategoryFilter.SelectedIndex = 0;
    }

    private void ApplyFilter()
    {
        var search = SearchBox.Text?.ToLower().Trim();
        var selectedCategory = (CategoryFilter.SelectedItem as ComboBoxItem)?.Tag as int?;

        var filtered = _allItems.AsEnumerable();

        if (!string.IsNullOrEmpty(search))
        {
            filtered = filtered.Where(p => p.Name.ToLower().Contains(search));
        }

        if (selectedCategory.HasValue && selectedCategory != -1)
        {
            filtered = filtered.Where(p => p.CategoryId == selectedCategory.Value);
        }

        var result = filtered.ToList();
        StockGrid.ItemsSource = result;

        var total = result.Sum(item => item.TotalValue);
        TotalSummaryText.Text = $"Общая стоимость: {total:N0} ₽";
    }

    private void OnFilterChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void RefreshData(object sender, RoutedEventArgs e)
    {
        LoadData();
    }

    private void GoToAdmin(object sender, RoutedEventArgs e)
    {
        var admin = new AdminWindow();
        Hide();
        admin.Show();
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _context?.Dispose();
        base.OnClosed(e);
    }
}

// Вспомогательный DTO для отображения + вычисления TotalValue
public class StockItemView
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CategoryName { get; set; }
    public string UnitName { get; set; }
    public int Price { get; set; }
    public int Amount { get; set; }
    public int CategoryId { get; set; }
    public int TotalValue => Amount * Price;
}
