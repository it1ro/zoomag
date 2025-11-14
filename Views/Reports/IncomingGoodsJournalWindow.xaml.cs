using System.Windows;
using System.IO;
using ClosedXML.Excel;
using Zoomag.Data;
using System.Linq;

namespace Zoomag.Views.Reports
{
    public partial class IncomingGoodsJournalWindow : Window
    {
        public IncomingGoodsJournalWindow()
        {
            InitializeComponent();
            LoadSupplies();
        }

        private void LoadSupplies()
        {
            using var context = new AppDbContext();
            var supplies = context.Supply
                .OrderBy(supply => supply.Name)
                .ToList();
            GoodsReceiptsGrid.ItemsSource = supplies;
        }

        private void ExportToExcel(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var supplies = context.Supply
                .Select(supply => new {
                    supply.Name,
                    supply.Date
                })
                .OrderBy(supply => supply.Name)
                .ToList();

            if (!supplies.Any())
            {
                MessageBox.Show("Нет данных для отчёта", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Журнал поступления товаров");

            worksheet.Cell(1, 1).Value = "Журнал поступления товаров на";
            worksheet.Cell(1, 3).Value = DateTime.Now.ToString("MMMM d, yyyy");

            worksheet.Cell(3, 1).Value = "Наименование";
            worksheet.Cell(3, 3).Value = "Количество";
            worksheet.Cell(3, 5).Value = "Цена";
            worksheet.Cell(3, 7).Value = "Дата";

            worksheet.Range(3, 1, 3, 7).Style.Font.Bold = true;

            int row = 4;
            foreach (var supply in supplies)
            {
                worksheet.Cell(row, 1).Value = supply.Name;
                worksheet.Cell(row, 7).Value = supply.Date;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            string fileName = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"Журнал поступления товаров {DateTime.Now:yyyy-MM-dd}.xlsx"
            );

            try
            {
                workbook.SaveAs(fileName);
                MessageBox.Show($"Файл сохранён: {fileName}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            this.Hide();
            adminWindow.Show();
        }
    }
}
