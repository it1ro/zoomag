using System;
using System.Linq;
using System.Windows;
using Zoomag.Data;
using Microsoft.EntityFrameworkCore;

namespace Zoomag.Views.Reports;

public partial class DailyGoodsReceiptReportWindow : Window
{
    public DailyGoodsReceiptReportWindow()
    {
        InitializeComponent();
        DeliveryDatePicker.SelectedDate = DateTime.Today;
        LoadData();
    }

    private void LoadData()
    {
        var selectedDate = DeliveryDatePicker.SelectedDate?.Date;
        if (!selectedDate.HasValue)
            return;

        try
        {
            using var context = new AppDbContext();
            var supplyItems = context.SupplyItem
                .Include(si => si.Supply)
                .Include(si => si.Product)
                .Where(si => si.Supply.Date.Date == selectedDate.Value)
                .ToList();

            var reportItems = supplyItems.Select(si => new ReportItem
            {
                Name = si.Product.Name,
                Amount = si.Quantity,
                Price = si.Price,
                Total = si.Total
            }).ToList();

            var totalSum = reportItems.Sum(r => r.Total);
            var totalQuantity = reportItems.Sum(r => r.Amount);

            TotalAmountDisplay.Text = totalSum.ToString("N0");
            TotalQuantityDisplay.Text = totalQuantity.ToString("N0");
            ReportDataGrid.ItemsSource = reportItems;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GenerateReport(object sender, RoutedEventArgs e)
    {
        LoadData();
    }

    private void ResetFilter(object sender, RoutedEventArgs e)
    {
        DeliveryDatePicker.SelectedDate = DateTime.Today;
        LoadData();
    }

    private void BackToMainMenu(object sender, RoutedEventArgs e)
    {
        var adminWindow = new AdminWindow();
        Hide();
        adminWindow.Show();
    }
}

// Вспомогательный класс для отображения
public class ReportItem
{
    public string Name { get; set; } = string.Empty;
    public int Amount { get; set; }
    public int Price { get; set; }
    public int Total { get; set; }
}
