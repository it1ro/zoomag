namespace Zoomag.Views.Reports;

using System.Windows;
using ClosedXML.Excel;
using Zoomag.Data;
using Zoomag.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

public partial class ZeroStockReportWindow : Window
{
    private List<ProductViewModel> _zeroStockProducts;

    public ZeroStockReportWindow()
    {
        InitializeComponent();
        LoadZeroStockData();
    }

    private void LoadZeroStockData()
    {
        using var context = new AppDbContext();
        _zeroStockProducts = context.Product
            .Select(p => new
            {
                p.Name,
                Price = p.SupplyItems
                    .OrderByDescending(si => si.Supply.Date)
                    .FirstOrDefault() != null
                    ? p.SupplyItems.OrderByDescending(si => si.Supply.Date).FirstOrDefault().Price
                    : 0,
                Stock = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
            })
            .Where(x => x.Stock == 0)
            .Select(x => new ProductViewModel
            {
                Name = x.Name,
                Price = x.Price
            })
            .ToList();

        ZeroStockDataGrid.ItemsSource = _zeroStockProducts;
    }

    private void ExportToExcel_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Excel файлы (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*",
            FileName = $"Товары с нулевым остатком на {DateTime.Today:yyyy-MM-dd}.xlsx",
            DefaultExt = ".xlsx",
            Title = "Сохранить отчет в Excel"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Товары с нулевым остатком");

                worksheet.Cell(1, 1).Value = "Товары с нулевым остатком";
                worksheet.Cell(1, 3).Value = DateTime.Today.ToString("MMMM dd yyyy");

                worksheet.Cell(3, 1).Value = "Наименование";
                worksheet.Cell(3, 2).Value = "Цена"; // ← сдвинули в колонку 2 для удобства

                var row = 4;
                foreach (var product in _zeroStockProducts)
                {
                    worksheet.Cell(row, 1).Value = product.Name;
                    worksheet.Cell(row, 2).Value = product.Price;
                    row++;
                }

                worksheet.Cell(row + 1, 1).Value = $"{_zeroStockProducts.Count} товаров";
                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(saveFileDialog.FileName);
                MessageBox.Show($"Отчет сохранен: {saveFileDialog.FileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении отчета: {ex.Message}");
            }
        }
    }

    private void BackToReports_Click(object sender, RoutedEventArgs e)
    {
        var reportsWindow = new AdminReportsWindow();
        Hide();
        reportsWindow.Show();
    }

    public class ProductViewModel
    {
        public string Name { get; set; }
        public int Price { get; set; }
    }
}
