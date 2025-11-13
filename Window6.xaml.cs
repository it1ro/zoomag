using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ClosedXML.Excel; // Заменили using Excel = Microsoft.Office.Interop.Excel;

namespace prriva_10
{
    /// <summary>
    /// Логика взаимодействия для Window6.xaml
    /// </summary>
    public partial class Window6 : Window
    {
        public Window6()
        {
            InitializeComponent();
            var context = new AppDbContext();
            foreach (var item in context.Kategors.ToList())
            {
                kat.Items.Add(item.Typ);
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
            var q = from dd in context.Tovar
                    where dd.Kategor.Typ == selectedCategory
                    select new { dd.Name, dd.Kol_vo, dd.Price, dd.Kategor, dd.Izmer };

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
                    ws.Cell(row, 3).Value = item.Kol_vo;
                    ws.Cell(row, 5).Value = item.Price;
                    ws.Cell(row, 7).Value = item.Kategor.Typ;
                    ws.Cell(row, 9).Value = item.Izmer.Typ;
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
            MainWindow main = new MainWindow();
            this.Hide();
            main.Show();
        }
        private void kat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (kat.SelectedItem != null)
            {
                var context = new AppDbContext();
                dg6.ItemsSource = context.Tovar
                    .Include(t => t.Kategor)
                    .Include(t => t.Izmer)
                    .Where(t => t.Kategor.Typ == kat.SelectedItem.ToString())
                    .ToList();
            }
        }
    }
}