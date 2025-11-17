using System.Windows;
using System.IO;
using System.Linq;
using Zoomag.Data;
using Zoomag.Models;
using Microsoft.Win32; // Для SaveFileDialog
using ClosedXML.Excel;

namespace Zoomag.Views.Reports
{
    using Microsoft.EntityFrameworkCore;

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
                .Select(product => new
                {
                    Name = product.Name,
                    Price = product.Price,
                    Amount = product.Amount,
                    TotalValue = product.Price * product.Amount // Вычисляем общую стоимость
                })
                .ToList();

            StockReportGrid.ItemsSource = products;
        }

        private void ExportToExcel(object sender, RoutedEventArgs e)
        {
            // Загружаем данные заново, как и при отображении
            using var context = new AppDbContext();
            var exportData = context.Product
                .Select(product => new
                {
                    Name = product.Name,
                    Price = product.Price,
                    Amount = product.Amount,
                    TotalValue = product.Price * product.Amount
                })
                .ToList();

            if (!exportData.Any())
            {
                MessageBox.Show("Нет данных для отчета.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Диалог выбора файла
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (.xlsx)|*.xlsx|All Files (*.*)|*.*",
                FileName = $"Отчет по складу на {DateTime.Today:yyyy-MM-dd}.xlsx",
                DefaultExt = ".xlsx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result != true)
            {
                return; // Выход, если пользователь отменил
            }

            string fileName = saveFileDialog.FileName;

            if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".xlsx";
            }

            // Создание Excel-файла
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Отчет по складу");

            worksheet.Cell(1, 1).Value = "Отчет по складу на";
            worksheet.Cell(1, 3).Value = DateTime.Today.ToString("MMMM dd yyyy");

            worksheet.Cell(3, 1).Value = "Наименование";
            worksheet.Cell(3, 2).Value = "Цена";
            worksheet.Cell(3, 3).Value = "Количество";
            worksheet.Cell(3, 4).Value = "Общая стоимость";

            worksheet.Range(3, 1, 3, 4).Style.Font.Bold = true; // Жирный шрифт для заголовков

            int row = 4;
            foreach (var item in exportData)
            {
                worksheet.Cell(row, 1).Value = item.Name;
                worksheet.Cell(row, 2).Value = item.Price;
                worksheet.Cell(row, 3).Value = item.Amount;
                worksheet.Cell(row, 4).Value = item.TotalValue;
                row++;
            }

            // Автоширина столбцов
            worksheet.Columns().AdjustToContents();

            try
            {
                workbook.SaveAs(fileName);
                MessageBox.Show($"Отчет сохранен: {fileName}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoToAdmin(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            this.Hide();
            adminWindow.Show();
        }
    }
}
