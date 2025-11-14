using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Zoomag.Data;
using Zoomag.Models;

namespace Zoomag.Views.Reports
{
    using ClosedXML.Excel;

    /// <summary>
    /// Логика взаимодействия для AdminReportsWindow.xaml
    /// </summary>
    public partial class AdminReportsWindow : Window
    {
        public AdminReportsWindow()
        {
            InitializeComponent();
        }

        private void GoToAdmin(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            this.Hide();
            adminWindow.Show();
        }

        private void GeneratePriceList(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var products = context.Product
                .Where(product => product.Amount > 0)
                .Select(product => new { product.Name, product.Price, product.Amount })
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Прайс-лист");

            worksheet.Cell(1, 1).Value = "Прайс-лист на";
            worksheet.Cell(1, 3).Value = DateTime.Today.ToString("MMMM dd yyyy");

            worksheet.Cell(3, 1).Value = "Наименование";
            worksheet.Cell(3, 3).Value = "Цена";
            worksheet.Cell(3, 5).Value = "Количество";

            int row = 4;
            foreach (var product in products)
            {
                worksheet.Cell(row, 1).Value = product.Name;
                worksheet.Cell(row, 3).Value = product.Price;
                worksheet.Cell(row, 5).Value = product.Amount;
                row++;
            }

            worksheet.Cell(products.Count + 5, 1).Value = $"{products.Count} товаров";

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = Path.Combine(desktopPath, $"Прайс-лист на {DateTime.Today:MMMM dd yyyy}.xlsx");
                workbook.SaveAs(fileName);
                MessageBox.Show($"Отчет сохранен: {fileName}");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении отчета: {ex.Message}");
            }
        }

        private void ViewIncomingGoodsJournal(object sender, RoutedEventArgs e)
        {
            var journalWindow = new IncomingGoodsJournalWindow();
            this.Hide();
            journalWindow.Show();
        }

        private void GenerateStockReport(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var products = context.Product
                .Select(product => new { product.Name, product.Price, product.Amount })
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Отчет по складу");

            worksheet.Cell(1, 1).Value = "Отчет по складу на";
            worksheet.Cell(1, 3).Value = DateTime.Today.ToString("MMMM dd yyyy");

            worksheet.Cell(3, 1).Value = "Наименование";
            worksheet.Cell(3, 3).Value = "Цена";
            worksheet.Cell(3, 5).Value = "Количество";

            int row = 4;
            int totalStockValue = 0; // Локальная переменная вместо поля класса

            foreach (var product in products)
            {
                worksheet.Cell(row, 1).Value = product.Name;
                worksheet.Cell(row, 3).Value = product.Price;
                worksheet.Cell(row, 5).Value = product.Amount;
                totalStockValue += product.Price * product.Amount;
                row++;
            }

            worksheet.Cell(products.Count + 5, 1).Value = $"{products.Count} товаров";
            worksheet.Cell(products.Count + 7, 1).Value = $"Общая стоимость: {totalStockValue}";

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = Path.Combine(desktopPath, $"Отчет по складу на {DateTime.Today:MMMM dd yyyy}.xlsx");
                workbook.SaveAs(fileName);
                MessageBox.Show($"Отчет сохранен: {fileName}");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении отчета: {ex.Message}");
            }
        }

        private void ViewDailyReceiptReport(object sender, RoutedEventArgs e)
        {
            var reportWindow = new DailyGoodsReceiptReportWindow();
            this.Hide();
            reportWindow.Show();
        }

        private void ViewCategoryReport(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var reportData = context.Product
                .GroupJoin(context.Category,
                    p => p.Category.Id,
                    c => c.Id,
                    (p, c) => new { Product = p, Categories = c })
                .SelectMany(x => x.Categories.DefaultIfEmpty(),
                    (x, c) => new { x.Product, Category = c })
                .GroupBy(pc => pc.Category.Name)
                .Select(g => new { CategoryName = g.Key, Count = g.Count(), TotalValue = g.Sum(pc => pc.Product.Price * pc.Product.Amount) })
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Отчет по категориям");

            worksheet.Cell(1, 1).Value = "Отчет по категориям на";
            worksheet.Cell(1, 3).Value = DateTime.Today.ToString("MMMM dd yyyy");

            worksheet.Cell(3, 1).Value = "Категория";
            worksheet.Cell(3, 3).Value = "Количество товаров";
            worksheet.Cell(3, 5).Value = "Общая стоимость";

            int row = 4;
            foreach (var item in reportData)
            {
                worksheet.Cell(row, 1).Value = item.CategoryName;
                worksheet.Cell(row, 3).Value = item.Count;
                worksheet.Cell(row, 5).Value = item.TotalValue;
                row++;
            }

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = Path.Combine(desktopPath, $"Отчет по категориям на {DateTime.Today:MMMM dd yyyy}.xlsx");
                workbook.SaveAs(fileName);
                MessageBox.Show($"Отчет сохранен: {fileName}");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении отчета: {ex.Message}");
            }
        }

        private void ViewZeroStockReport(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var products = context.Product
                .Where(product => product.Amount == 0)
                .Select(product => new { product.Name, product.Price })
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Товары с нулевым остатком");

            worksheet.Cell(1, 1).Value = "Товары с нулевым остатком";
            worksheet.Cell(1, 3).Value = DateTime.Today.ToString("MMMM dd yyyy");

            worksheet.Cell(3, 1).Value = "Наименование";
            worksheet.Cell(3, 3).Value = "Цена";

            int row = 4;
            foreach (var product in products)
            {
                worksheet.Cell(row, 1).Value = product.Name;
                worksheet.Cell(row, 3).Value = product.Price;
                row++;
            }

            worksheet.Cell(products.Count + 2, 1).Value = $"{products.Count} товаров";

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = Path.Combine(desktopPath, $"Товары с нулевым остатком на {DateTime.Today:MMMM dd yyyy}.xlsx");
                workbook.SaveAs(fileName);
                MessageBox.Show($"Отчет сохранен: {fileName}");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении отчета: {ex.Message}");
            }
        }
    }
}
