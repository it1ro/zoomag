using System;
using System.Linq;
using System.Windows;
using ClosedXML.Excel;
using Zoomag.Data; // ← Исправь, если namespace другой!
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace Zoomag.Views.Reports;

public partial class IncomingGoodsJournalWindow : Window
{
    public IncomingGoodsJournalWindow()
    {
        InitializeComponent();
        LoadSupplies();
    }

    private void LoadSupplies()
    {
        try
        {
            using var context = new AppDbContext();
            var displayList = context.Supply
                .Include(s => s.SupplyItems)
                .ThenInclude(si => si.Product)
                .OrderBy(s => s.Date)
                .SelectMany(s => s.SupplyItems.Select(si => new
                {
                    SupplyDate = s.Date,
                    ProductName = si.Product.Name,
                    Quantity = si.Quantity,
                    Price = si.Price,
                    Total = si.Quantity * si.Price
                }))
                .ToList();

            GoodsReceiptsGrid.ItemsSource = displayList;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportToExcel(object sender, RoutedEventArgs e)
    {
        try
        {
            using var context = new AppDbContext();
            var exportData = context.Supply
                .Include(s => s.SupplyItems)
                .ThenInclude(si => si.Product)
                .OrderBy(s => s.Date)
                .SelectMany(s => s.SupplyItems.Select(si => new
                {
                    SupplyDate = s.Date,
                    ProductName = si.Product.Name,
                    Quantity = si.Quantity,
                    Price = si.Price,
                    Total = si.Quantity * si.Price
                }))
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
                FileName = $"Журнал поступления товаров {DateTime.Now:yyyy-MM-dd}.xlsx",
                DefaultExt = ".xlsx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (saveFileDialog.ShowDialog() != true) return;

            var fileName = saveFileDialog.FileName;
            if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                fileName += ".xlsx";

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Журнал поступления");

            // Заголовок
            worksheet.Cell(1, 1).Value = "Журнал поступления товаров на:";
            worksheet.Cell(1, 2).Value = DateTime.Now.ToString("dd.MM.yyyy");

            // Шапка таблицы
            worksheet.Cell(3, 1).Value = "Дата";
            worksheet.Cell(3, 2).Value = "Наименование";
            worksheet.Cell(3, 3).Value = "Количество";
            worksheet.Cell(3, 4).Value = "Цена, ₽";
            worksheet.Cell(3, 5).Value = "Сумма, ₽";
            worksheet.Range(3, 1, 3, 5).Style.Font.Bold = true;

            // Данные
            var row = 4;
            foreach (var item in exportData)
            {
                worksheet.Cell(row, 1).Value = item.SupplyDate;
                worksheet.Cell(row, 2).Value = item.ProductName;
                worksheet.Cell(row, 3).Value = item.Quantity;
                worksheet.Cell(row, 4).Value = item.Price;
                worksheet.Cell(row, 5).Value = item.Total;
                row++;
            }

            // Форматирование
            worksheet.Column(1).Style.DateFormat.Format = "dd.MM.yyyy";
            worksheet.Column(3).Style.NumberFormat.Format = "#,##0";
            worksheet.Column(4).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Column(5).Style.NumberFormat.Format = "#,##0.00";

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(fileName);
            MessageBox.Show($"Отчёт сохранён:\n{fileName}", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при экспорте: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var adminWindow = new AdminWindow();
        Hide();
        adminWindow.Show();
    }
}
