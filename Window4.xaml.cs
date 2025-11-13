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
    /// Логика взаимодействия для Window4.xaml
    /// </summary>
    public partial class Window4 : Window
    {
        int sum = 0;
        public Window4()
        {
            InitializeComponent();
        }
        private void nazad(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            this.Hide();
            main.Show();
        }
        private void price_list(object sender, RoutedEventArgs e)
        {
            var context = new AppDbContext();
            var q = from dd in context.Tovar
                    where dd.Kol_vo > 0 // фильтруем в LINQ, как и было
                    select new { dd.Name, dd.Price, dd.Kol_vo };

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
                    ws.Cell(row, 5).Value = item.Kol_vo;
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
            Window5 main = new Window5();
            this.Hide();
            main.Show();
        }
        private void otch_sklad(object sender, RoutedEventArgs e)
        {
            var context = new AppDbContext();
            var q = from dd in context.Tovar
                    select new { dd.Name, dd.Price, dd.Kol_vo };

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
                    ws.Cell(row, 5).Value = item.Kol_vo;
                    sum += item.Price * item.Kol_vo;
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
            Window7 main = new Window7();
            this.Hide();
            main.Show();
        }
        private void cat(object sender, RoutedEventArgs e)
        {
            Window6 main = new Window6();
            this.Hide();
            main.Show();
        }
        
        // Файл: prriva_10/Window4.xaml.cs

        private void null_poz(object sender, RoutedEventArgs e)
        {
            Window9 nullWindow = new Window9();
            this.Hide(); // Скрываем текущее окно (Window4)
            nullWindow.Show(); // Показываем новое окно
        }
        
        
    }
}