// Для SaveFileDialog
namespace Zoomag.Views.Reports;

using System.Windows;
using ClosedXML.Excel;
using Zoomag.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

public partial class StockReportWindow : Window
{
    public StockReportWindow()
    {
        InitializeComponent();
        LoadStockReport();
    }

    private void LoadStockReport()
    {
        using var context = new AppDbContext();
        var products = context.Product
            .Select(p => new
            {
                p.Name,
                Price = p.SupplyItems
                    .OrderByDescending(si => si.Supply.Date)
                    .FirstOrDefault() != null
                    ? p.SupplyItems.OrderByDescending(si => si.Supply.Date).FirstOrDefault().Price
                    : 0,
                Amount = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
            })
            .Where(x => x.Amount != 0 || x.Price != 0) // опционально: показывать даже нулевые, если были операции
            .ToList();

        var reportItems = products.Select(p => new
        {
            p.Name,
            p.Price,
            p.Amount,
            TotalValue = p.Price * p.Amount
        }).ToList();

        StockReportGrid.ItemsSource = reportItems;
    }

    private void ExportToExcel(object sender, RoutedEventArgs e)
    {
        using var context = new AppDbContext();
        var exportData = context.Product
            .Select(p => new
            {
                p.Name,
                Price = p.SupplyItems
                    .OrderByDescending(si => si.Supply.Date)
                    .FirstOrDefault() != null
                    ? p.SupplyItems.OrderByDescending(si => si.Supply.Date).FirstOrDefault().Price
                    : 0,
                Amount = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
            })
            .ToList();

        var reportItems = exportData.Select(p => new
        {
            p.Name,
            p.Price,
            p.Amount,
            TotalValue = p.Price * p.Amount
        }).ToList();

        if (!reportItems.Any())
        {
            MessageBox.Show("Нет данных для отчёта.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Excel Files (.xlsx)|*.xlsx|All Files (*.*)|*.*",
            FileName = $"Отчет по складу на {DateTime.Today:yyyy-MM-dd}.xlsx",
            DefaultExt = ".xlsx",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (saveFileDialog.ShowDialog() != true) return;

        var fileName = saveFileDialog.FileName;
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            fileName += ".xlsx";

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Отчет по складу");

        worksheet.Cell(1, 1).Value = "Отчет по складу на";
        worksheet.Cell(1, 3).Value = DateTime.Today.ToString("MMMM dd yyyy");

        worksheet.Cell(3, 1).Value = "Наименование";
        worksheet.Cell(3, 2).Value = "Цена";
        worksheet.Cell(3, 3).Value = "Количество";
        worksheet.Cell(3, 4).Value = "Общая стоимость";
        worksheet.Range(3, 1, 3, 4).Style.Font.Bold = true;

        var row = 4;
        foreach (var item in reportItems)
        {
            worksheet.Cell(row, 1).Value = item.Name;
            worksheet.Cell(row, 2).Value = item.Price;
            worksheet.Cell(row, 3).Value = item.Amount;
            worksheet.Cell(row, 4).Value = item.TotalValue;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        try
        {
            workbook.SaveAs(fileName);
            MessageBox.Show($"Отчёт сохранён: {fileName}", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении отчёта: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GoToAdmin(object sender, RoutedEventArgs e)
    {
        var adminWindow = new AdminWindow();
        Hide();
        adminWindow.Show();
    }
}
