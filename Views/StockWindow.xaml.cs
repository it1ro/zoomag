using System;
using System.Linq;
using System.Windows;
using Zoomag.Data;
using Zoomag.Models;
using Microsoft.EntityFrameworkCore;

namespace Zoomag.Views;

public partial class StockWindow : Window
{
    public StockWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
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

            var displayProducts = products.Select(p => new ProductDisplayDto
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category?.Name ?? "Без категории",
                UnitName = p.Unit?.Name ?? "шт",
                Price = p.SupplyItems
                    .Where(si => si.Supply != null) // ← защита от NullReferenceException
                    .OrderByDescending(si => si.Supply.Date)
                    .FirstOrDefault()?.Price ?? 0,
                Qty = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
            }).ToList();

            StockDataGrid.ItemsSource = displayProducts;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке склада: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GoBack_Click(object sender, RoutedEventArgs e)
    {
        var adminWindow = new AdminWindow();
        Hide();
        adminWindow.Show();
    }
}
