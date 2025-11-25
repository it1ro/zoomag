using System;
using System.Linq;
using System.Windows;
using ClosedXML.Excel;
using Zoomag.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace Zoomag.Views.Reports;

public partial class StockReportWindow : Window
{
    public StockReportWindow()
    {
        InitializeComponent();
        LoadStockReport();
    }

    private void LoadStockReport()
    {
        try
        {
            using var context = new AppDbContext();
            var reportItems = context.Product
                .Select(p => new
                {
                    p.Name,
                    Price = p.SupplyItems
                        .OrderByDescending(si => si.Supply.Date)
                        .Select(si => si.Price)
                        .FirstOrDefault(), // автоматически 0, если нет поставок
                    Amount = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
                })
                .AsEnumerable() // переключаемся в LINQ to Objects для TotalValue
                .Select(p => new
                {
                    p.Name,
                    p.Price,
                    p.Amount,
                    TotalValue = p.Price * p.Amount
                })
                .Where(x => x.Amount != 0 || x.Price != 0) // опционально
                .ToList();

            StockReportGrid.ItemsSource = reportItems;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке отчёта: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportToExcel(object sender, RoutedEventArgs e)
    {
        try
        {
            using var context = new AppDbContext();
            var reportItems = context.Product
                .Select(p => new
                {
                    p.Name,
                    Price = p.SupplyItems
                        .OrderByDescending(si => si.Supply.Date)
                        .Select(si => si.Price)
                        .FirstOrDefault(),
                    Amount = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
                })
                .AsEnumerable()
                .Select(p => new
                {
                    p.Name,
                    p.Price,
                    p.Amount,
                    TotalValue = p.Price * p.Amount
                })
                .ToList();

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

            worksheet.Cell(1, 1).Value = "Отчет по складу на:";
            worksheet.Cell(1, 2).Value = DateTime.Today.ToString("dd MMMM yyyy",
                System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU"));

            worksheet.Cell(3, 1).Value = "Наименование";
            worksheet.Cell(3, 2).Value = "Цена (руб.)";
            worksheet.Cell(3, 3).Value = "Остаток";
            worksheet.Cell(3, 4).Value = "Общая стоимость (руб.)";
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
            workbook.SaveAs(fileName);

            MessageBox.Show($"Отчёт успешно сохранён:\n{fileName}", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при экспорте: {ex.Message}",
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
