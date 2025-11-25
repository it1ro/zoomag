using System;
using System.Linq;
using System.Windows;
using ClosedXML.Excel;
using Zoomag.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace Zoomag.Views.Reports;

// 👇 Вынесем модель отдельно (лучше в Zoomag/Models/, но можно и тут)
public class ProductViewModel
{
    public string Name { get; set; } = string.Empty;
    public int Price { get; set; }
}

public partial class ZeroStockReportWindow : Window
{
    private List<ProductViewModel> _zeroStockProducts = new();

    public ZeroStockReportWindow()
    {
        InitializeComponent();
        LoadZeroStockData();
    }

    private void LoadZeroStockData()
    {
        try
        {
            using var context = new AppDbContext();
            _zeroStockProducts = context.Product
                .Where(p => p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity) == 0)
                .Select(p => new ProductViewModel
                {
                    Name = p.Name,
                    Price = p.SupplyItems
                        .OrderByDescending(si => si.Supply.Date)
                        .Select(si => si.Price)
                        .FirstOrDefault() // 0, если нет поставок
                })
                .ToList();

            ZeroStockDataGrid.ItemsSource = _zeroStockProducts;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportToExcel_Click(object sender, RoutedEventArgs e)
    {
        if (!_zeroStockProducts.Any())
        {
            MessageBox.Show("Нет данных для экспорта.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Excel файлы (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*",
            FileName = $"Товары с нулевым остатком на {DateTime.Today:yyyy-MM-dd}.xlsx",
            DefaultExt = ".xlsx",
            Title = "Сохранить отчет в Excel",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (saveFileDialog.ShowDialog() != true) return;

        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Товары с нулевым остатком");

            worksheet.Cell(1, 1).Value = "Товары с нулевым остатком на:";
            worksheet.Cell(1, 2).Value = DateTime.Today.ToString("dd.MM.yyyy");

            worksheet.Cell(3, 1).Value = "Наименование";
            worksheet.Cell(3, 2).Value = "Цена, ₽";
            worksheet.Range(3, 1, 3, 2).Style.Font.Bold = true;

            var row = 4;
            foreach (var product in _zeroStockProducts)
            {
                worksheet.Cell(row, 1).Value = product.Name;
                worksheet.Cell(row, 2).Value = product.Price;
                row++;
            }

            worksheet.Cell(row + 1, 1).Value = $"Всего: {_zeroStockProducts.Count} товаров";
            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(saveFileDialog.FileName);

            MessageBox.Show($"Отчёт успешно сохранён:\n{saveFileDialog.FileName}",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BackToReports_Click(object sender, RoutedEventArgs e)
    {
        var reportsWindow = new AdminReportsWindow();
        Hide();
        reportsWindow.Show();
    }
}
