using System.Windows;
using System.IO;
using ClosedXML.Excel;
using Zoomag.Data;

namespace Zoomag.Views.Reports
{
    public partial class IncomingGoodsJournalWindow : Window
    {
        public IncomingGoodsJournalWindow()
        {
            InitializeComponent();
            using var context = new AppDbContext();
            var goodsReceipts = context.Supply
                .OrderBy(x => x.Name)
                .ToList();
            dg5.ItemsSource = goodsReceipts;
        }

        private void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var goodsReceipts = context.Supply
                .Select(gr => new { gr.Name, gr.TotalAmount, gr.Date })
                .OrderBy(gr => gr.Name)
                .ToList();

            if (!goodsReceipts.Any())
            {
                MessageBox.Show("Нет данных для отчёта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Журнал поступления товаров");

            ws.Cell(1, 1).Value = "Журнал поступления товаров на ";
            ws.Cell(1, 3).Value = DateTime.Now.ToString("MMMM d, yyyy");

            ws.Cell(3, 1).Value = "Наименование";
            ws.Cell(3, 3).Value = "Количество";
            ws.Cell(3, 5).Value = "Цена";
            ws.Cell(3, 7).Value = "Дата";

            ws.Range(3, 1, 3, 7).Style.Font.Bold = true;

            int row = 4;
            foreach (var item in goodsReceipts)
            {
                ws.Cell(row, 1).Value = item.Name;
                ws.Cell(row, 5).Value = item.TotalAmount;
                ws.Cell(row, 7).Value = item.Date;
                row++;
            }

            ws.Columns().AdjustToContents();

            string fileName = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"Журнал поступления товаров {DateTime.Now:yyyy-MM-dd}.xlsx"
            );

            try
            {
                workbook.SaveAs(fileName);
                MessageBox.Show($"Файл сохранён: {fileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow main = new AdminWindow();
            this.Hide();
            main.Show();
        }
    }
}
