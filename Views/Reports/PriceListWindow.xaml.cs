using System.Windows;
using ClosedXML.Excel;
using Zoomag.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Linq;

namespace Zoomag.Views.Reports;

public partial class PriceListWindow : Window
{
    public PriceListWindow()
    {
        InitializeComponent();
        LoadPriceList();
    }

    private void LoadPriceList()
    {
        try
        {
            using var context = new AppDbContext();
            var products = context.Product
                .Select(p => new
                {
                    p.Name,
                    Price = p.SupplyItems
                        .OrderByDescending(si => si.Supply.Date)
                        .Select(si => si.Price)
                        .FirstOrDefault(), // для int: 0 если нет поставок
                    Stock = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
                })
                .Where(x => x.Stock > 0)
                .ToList();

            PriceListGrid.ItemsSource = products;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке прайс-листа: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportToExcel(object sender, RoutedEventArgs e)
    {
        try
        {
            using var context = new AppDbContext();
            var exportData = context.Product
                .Select(p => new
                {
                    p.Name,
                    Price = p.SupplyItems
                        .OrderByDescending(si => si.Supply.Date)
                        .Select(si => si.Price)
                        .FirstOrDefault(),
                    Stock = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
                })
                .Where(x => x.Stock > 0)
                .ToList();

            if (!exportData.Any())
            {
                MessageBox.Show("Нет данных для отчёта.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (.xlsx)|*.xlsx|All Files (*.*)|*.*",
                FileName = $"Прайс-лист на {DateTime.Today:yyyy-MM-dd}.xlsx",
                DefaultExt = ".xlsx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (saveFileDialog.ShowDialog() != true) return;

            var fileName = saveFileDialog.FileName;
            if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                fileName += ".xlsx";

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Прайс-лист");

            // Заголовок отчёта
            worksheet.Cell(1, 1).Value = "Прайс-лист на:";
            worksheet.Cell(1, 2).Value = DateTime.Today.ToString("dd MMMM yyyy",
                System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU"));

            // Шапка таблицы
            worksheet.Cell(3, 1).Value = "Наименование";
            worksheet.Cell(3, 2).Value = "Цена (руб.)";
            worksheet.Cell(3, 3).Value = "Остаток";
            worksheet.Range(3, 1, 3, 3).Style.Font.Bold = true;

            // Данные
            var row = 4;
            foreach (var item in exportData)
            {
                worksheet.Cell(row, 1).Value = item.Name;
                worksheet.Cell(row, 2).Value = item.Price;
                worksheet.Cell(row, 3).Value = item.Stock;
                row++;
            }

            // Итог
            worksheet.Cell(row + 1, 1).Value = $"Всего товаров: {exportData.Count}";
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(fileName);
            MessageBox.Show($"Отчёт успешно сохранён:\n{fileName}", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}",
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
