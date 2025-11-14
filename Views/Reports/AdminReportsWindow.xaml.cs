using System.Windows;
using Zoomag.Data;
using Zoomag.Views.Reports;

namespace Zoomag.Views.Reports
{
    using ClosedXML.Excel;

    /// <summary>
    /// Логика взаимодействия для AdminReportsWindow.xaml
    /// </summary>
    public partial class AdminReportsWindow : Window
    {
        private int _totalStockValue = 0;

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
            var context = new AppDbContext();
            var products = context.Product
                .Where(product => product.Amount > 0)
                .Select(product => new { product.Name, product.Price, product.Amount });

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Прайс-лист");

                worksheet.Cell(1, 1).Value = "Прайс-лист на";
                worksheet.Cell(1, 3).Value = DateTime.Today.ToString("MMMM d,yyyy");

                worksheet.Cell(3, 1).Value = "Наименование";
                worksheet.Cell(3, 3).Value = "Цена";
                worksheet.Cell(3, 5).Value = "Количество";

                int row = 4;
                int productCount = 0;

                foreach (var product in products)
                {
                    worksheet.Cell(row, 1).Value = product.Name;
                    worksheet.Cell(row, 3).Value = product.Price;
                    worksheet.Cell(row, 5).Value = product.Amount;
                    row++;
                    productCount++;
                }

                worksheet.Cell(productCount + 5, 1).Value = $"{productCount} товаров";

                string fileName = $@"C:\Users\student\Desktop\Прайс-лист на {DateTime.Today:MMMM d,yyyy}.xlsx";
                workbook.SaveAs(fileName);
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
            var context = new AppDbContext();
            var products = context.Product
                .Select(product => new { product.Name, product.Price, product.Amount });

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Отчет по складу");

                worksheet.Cell(1, 1).Value = "Отчет по складу на";
                worksheet.Cell(1, 3).Value = DateTime.Today.ToString("MMMM d,yyyy");

                worksheet.Cell(3, 1).Value = "Наименование";
                worksheet.Cell(3, 3).Value = "Цена";
                worksheet.Cell(3, 5).Value = "Количество";

                int row = 4;
                int productCount = 0;
                _totalStockValue = 0;

                foreach (var product in products)
                {
                    worksheet.Cell(row, 1).Value = product.Name;
                    worksheet.Cell(row, 3).Value = product.Price;
                    worksheet.Cell(row, 5).Value = product.Amount;
                    _totalStockValue += product.Price * product.Amount;
                    row++;
                    productCount++;
                }

                worksheet.Cell(productCount + 5, 1).Value = $"{productCount} товаров";
                worksheet.Cell(productCount + 7, 1).Value = $"{_totalStockValue} сумма";

                string fileName = $@"C:\Users\student\Desktop\Отчет по складу на {DateTime.Today:MMMM d,yyyy}.xlsx";
                workbook.SaveAs(fileName);
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
            var reportWindow = new CategoryReportWindow();
            this.Hide();
            reportWindow.Show();
        }

        private void ViewZeroStockReport(object sender, RoutedEventArgs e)
        {
            var reportWindow = new ZeroStockReportWindow();
            this.Hide();
            reportWindow.Show();
        }
    }
}
