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
    /// Логика взаимодействия для Window7.xaml
    /// </summary>
    public partial class Window7 : Window
    {
        public Window7()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime selestedDate = Convert.ToDateTime(z1.SelectedDate);
            string d = selestedDate.ToString("MMMM d,yyyy");
            var context = new AppDbContext();
            var q = from dd in context.Privozs
                    select new { dd.Data, dd.Name, dd.Price, dd.Kolvo };

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Отчет по поступлению товаров за день");

                ws.Cell(1, 1).Value = "отчет о поступлении товаров за день на";
                ws.Cell(1, 4).Value = d;

                ws.Cell(3, 3).Value = "Наименование";
                ws.Cell(3, 6).Value = "Количество";
                ws.Cell(3, 5).Value = "Цена";
                ws.Cell(3, 1).Value = "Дата";

                int summ = 0;
                int kol_vo = 0;
                int row = 4;
                int w = 0;
                foreach (var item in q)
                {
                    if (d == item.Data)
                    {
                        ws.Cell(row, 3).Value = item.Name;
                        ws.Cell(row, 6).Value = item.Kolvo;
                        ws.Cell(row, 5).Value = item.Price;
                        ws.Cell(row, 1).Value = item.Data;
                        row++;
                        w++;
                        summ += item.Kolvo * item.Price;
                        kol_vo += item.Kolvo;
                    }
                }

                ws.Cell(w + 5, 1).Value = w + " товаров";
                ws.Cell(w + 7, 1).Value = "Итого=" + summ + " рубля";

                string fileName = $@"C:\Users\student\Desktop\Отчет поступления товаров за день на {DateTime.Today:MMMM d,yyyy}.xlsx";
                workbook.SaveAs(fileName);

                sum.Text = summ.ToString();
                kol_tov.Text = kol_vo.ToString();
            }
        }

        private void nazad(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            this.Hide();
            main.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Window8 dalee = new Window8();
            this.Hide();
            dalee.Show();
        }
    }
}