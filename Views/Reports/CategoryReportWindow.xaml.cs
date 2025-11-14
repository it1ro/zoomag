using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Zoomag.Data;

// Заменили using Excel = Microsoft.Office.Interop.Excel;

namespace Zoomag.Views.Reports
{
    using Data;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Логика взаимодействия для CategoryReportWindow.xaml
    /// </summary>
    public partial class CategoryReportWindow : Window
    {
        public CategoryReportWindow()
        {
            InitializeComponent();
            var context = new AppDbContext();
            foreach (var item in context.Category.ToList())
            {
                kat.Items.Add(item.Name);
            }
        }
        private void viv_v_excel_Click(object sender, RoutedEventArgs e)
        {
            if (kat.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите категорию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var context = new AppDbContext();
            var selectedCategory = kat.SelectedItem.ToString();
            var q = from dd in context.Product
                    where dd.Category.Name == selectedCategory
                    select new { dd.Name, dd.Amount, dd.Price, dd.Category, dd.Unit };

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Отчет по категории");

                ws.Cell(1, 1).Value = "Отчет по категориям на ";
                ws.Cell(1, 3).Value = DateTime.Today.ToString("MMMM d,yyyy");

                ws.Cell(3, 1).Value = "Наименование";
                ws.Cell(3, 3).Value = "Количество";
                ws.Cell(3, 5).Value = "Цена";
                ws.Cell(3, 7).Value = "Категория";
                ws.Cell(3, 9).Value = "ед/изм";

                int row = 4;
                int w = 0;
                foreach (var item in q)
                {
                    ws.Cell(row, 1).Value = item.Name;
                    ws.Cell(row, 3).Value = item.Amount;
                    ws.Cell(row, 5).Value = item.Price;
                    ws.Cell(row, 7).Value = item.Category.Name;
                    ws.Cell(row, 9).Value = item.Unit.Name;
                    row++;
                    w++;
                }

                ws.Cell(w + 5, 1).Value = w + " товаров";

                string fileName = $@"C:\Users\student\Desktop\Журнал категории {DateTime.Today:MMMM d,yyyy}.xlsx";
                workbook.SaveAs(fileName);
            }
        }
        private void nazad_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow main = new AdminWindow();
            this.Hide();
            main.Show();
        }
        private void kat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (kat.SelectedItem != null)
            {
                var context = new AppDbContext();
                dg6.ItemsSource = context.Product
                    .Include(t => t.Category)
                    .Include(t => t.Unit)
                    .Where(t => t.Category.Name == kat.SelectedItem.ToString())
                    .ToList();
            }
        }
    }
}
