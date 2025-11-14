using System.Windows;
using ClosedXML.Excel;
using Zoomag.Data;

// Заменили using Excel = Microsoft.Office.Interop.Excel;

namespace Zoomag.Views.Reports
{
    /// <summary>
    /// Логика взаимодействия для AdminReportsWindow.xaml
    /// </summary>
    public partial class AdminReportsWindow : Window
    {
        int sum = 0;
        public AdminReportsWindow()
        {
            InitializeComponent();
        }
        private void nazad(object sender, RoutedEventArgs e)
        {
            AdminWindow main = new AdminWindow();
            this.Hide();
            main.Show();
        }
        private void price_list(object sender, RoutedEventArgs e)
        {
            var context = new AppDbContext();
            var q = from dd in context.Product
                    where dd.Amount > 0 // фильтруем в LINQ, как и было
                    select new { dd.Name, dd.Price, dd.Amount };

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Прайс-лист");

                ws.Cell(1, 1).Value = "Прайс-лист на";
                ws.Cell(1, 3).Value = DateTime.Today.ToString("MMMM d,yyyy");

                ws.Cell(3, 1).Value = "Наименование";
                ws.Cell(3, 3).Value = "Цена";
                ws.Cell(3, 5).Value = "Количество";

                int row = 4;
                int w = 0;
                foreach (var item in q)
                {
                    ws.Cell(row, 1).Value = item.Name;
                    ws.Cell(row, 3).Value = item.Price;
                    ws.Cell(row, 5).Value = item.Amount;
                    row++;
                    w++;
                }

                ws.Cell(w + 5, 1).Value = w + " товаров";

                string fileName = $@"C:\Users\student\Desktop\Прайс-лист на {DateTime.Today:MMMM d,yyyy}.xlsx";
                workbook.SaveAs(fileName);
                // Excel автоматически откроется, если пользователь откроет файл по умолчанию для .xlsx
            }
        }
        private void post_tov(object sender, RoutedEventArgs e)
        {
            IncomingGoodsJournalWindow main = new IncomingGoodsJournalWindow();
            this.Hide();
            main.Show();
        }
        private void otch_sklad(object sender, RoutedEventArgs e)
        {
            var context = new AppDbContext();
            var q = from dd in context.Product
                    select new { dd.Name, dd.Price, dd.Amount };

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Отчет по складу");

                ws.Cell(1, 1).Value = "Отчет по складу на ";
                ws.Cell(1, 3).Value = DateTime.Today.ToString("MMMM d,yyyy");

                ws.Cell(3, 1).Value = "Наименование";
                ws.Cell(3, 3).Value = "Цена";
                ws.Cell(3, 5).Value = "Количество";

                int row = 4;
                int w = 0;
                sum = 0; // обнуляем сумму перед подсчетом
                foreach (var item in q)
                {
                    ws.Cell(row, 1).Value = item.Name;
                    ws.Cell(row, 3).Value = item.Price;
                    ws.Cell(row, 5).Value = item.Amount;
                    sum += item.Price * item.Amount;
                    row++;
                    w++;
                }

                ws.Cell(w + 5, 1).Value = w + " товаров";
                ws.Cell(w + 7, 1).Value = sum + " сумма";

                string fileName = $@"C:\Users\student\Desktop\Отчет по складу на {DateTime.Today:MMMM d,yyyy}.xlsx";
                workbook.SaveAs(fileName);
            }
        }
        private void post_day(object sender, RoutedEventArgs e)
        {
            DailyGoodsReceiptReportWindow main = new DailyGoodsReceiptReportWindow();
            this.Hide();
            main.Show();
        }
        private void cat(object sender, RoutedEventArgs e)
        {
            CategoryReportWindow main = new CategoryReportWindow();
            this.Hide();
            main.Show();
        }

        // Файл: Zoomag/AdminReportsWindow.xaml.cs

        private void null_poz(object sender, RoutedEventArgs e)
        {
            ZeroStockReportWindow nullWindow = new ZeroStockReportWindow();
            this.Hide(); // Скрываем текущее окно (AdminReportsWindow)
            nullWindow.Show(); // Показываем новое окно
        }


    }
}
