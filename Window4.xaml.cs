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
using Excel = Microsoft.Office.Interop.Excel;

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
                    select new { dd.Name, dd.Price, dd.Kol_vo };

            Excel.Application exapp = new Excel.Application();
            Excel.Workbook exwor;
            Excel.Worksheet exsheet;
            exapp.SheetsInNewWorkbook = 2;
            exwor = exapp.Workbooks.Add();

            exsheet = (Excel.Worksheet)exwor.Worksheets.get_Item(1);
            exsheet.Cells[1, 1] = "Прайс-лист на";
            exsheet.Cells[1, 3] = DateTime.Today.ToString("MMMM d,yyyy");
            exsheet.Cells[3, 1] = "Наименование";
            exsheet.Cells[3, 3] = "Цена";
            exsheet.Cells[3, 5] = "Количество";

            int row = 4;
            int w = 0;
            foreach (var item in q)
            {
                if (item.Kol_vo > 0)
                {
                    exsheet.Cells[row, 1].Value = item.Name;
                    exsheet.Cells[row, 3].Value = item.Price;
                    exsheet.Cells[row, 5].Value = item.Kol_vo;
                    row++;
                    w++;
                }
            }
            exapp.Visible = true;
            exsheet.Cells[w + 5, 1] = Convert.ToString(w) + " товаров";
            exwor.Saved = true;
            exapp.DisplayAlerts = false;
            exwor.SaveAs(@"C:\Users\student\Desktop/Прайс-лист на " + DateTime.Today.ToString("MMMM d,yyyy") + ".xlsx");
            exapp.Quit();
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

            Excel.Application exapp = new Excel.Application();
            Excel.Workbook exwor;
            Excel.Worksheet exsheet;
            exapp.SheetsInNewWorkbook = 2;
            exwor = exapp.Workbooks.Add();

            exsheet = (Excel.Worksheet)exwor.Worksheets.get_Item(1);
            exsheet.Cells[1, 1] = "Отчет по складу на ";
            exsheet.Cells[1, 3] = DateTime.Today.ToString("MMMM d,yyyy");
            exsheet.Cells[3, 1] = "Наименование";
            exsheet.Cells[3, 3] = "Цена";
            exsheet.Cells[3, 5] = "Количество";

            int row = 4;
            int w = 0;
            foreach (var item in q)
            {
                if (item.Kol_vo >= 0)
                {
                    exsheet.Cells[row, 1].Value = item.Name;
                    exsheet.Cells[row, 3].Value = item.Price;
                    exsheet.Cells[row, 5].Value = item.Kol_vo;
                    sum += item.Price * item.Kol_vo;
                    row++;
                    w++;
                }
            }
            exapp.Visible = true;
            exsheet.Cells[w + 5, 1] = Convert.ToString(w) + " товаров";
            exsheet.Cells[w + 7, 1] = Convert.ToString(sum) + "сумма";
            exwor.Saved = true;
            exapp.DisplayAlerts = false;
            exwor.SaveAs(@"C:\Users\student\Desktop/Отчет по складу на " + DateTime.Today.ToString("MMMM d,yyyy") + ".xlsx");
            exapp.Quit();
        }

        private void post_day(object sender, RoutedEventArgs e)
        {
            Window7 main = new Window7();
            this.Hide();
            main.Show();
        }

        private void null_poz(object sender, RoutedEventArgs e)
        {
            var context = new AppDbContext();
            var q = from dd in context.Tovar
                    select new { dd.Name, dd.Kol_vo };

            Excel.Application exapp = new Excel.Application();
            Excel.Workbook exwor;
            Excel.Worksheet exsheet;
            exapp.SheetsInNewWorkbook = 2;
            exwor = exapp.Workbooks.Add();

            exsheet = (Excel.Worksheet)exwor.Worksheets.get_Item(1);
            exsheet.Cells[1, 1] = "Нулевые позиции на ";
            exsheet.Cells[1, 3] = DateTime.Today.ToString("MMMM d,yyyy");
            exsheet.Cells[3, 1] = "Наименование";

            int row = 4;
            int w = 0;
            foreach (var item in q)
            {
                if (item.Kol_vo == 0)
                {
                    exsheet.Cells[row, 1].Value = item.Name;
                    row++;
                    w++;
                }
            }
            exapp.Visible = true;
            exsheet.Cells[w + 5, 1] = Convert.ToString(w) + " наименования";
            exwor.Saved = true;
            exapp.DisplayAlerts = false;
            exwor.SaveAs(@"C:\Users\student\Desktop/Нулевые позиции на " + DateTime.Today.ToString("MMMM d,yyyy") + ".xlsx");
            exapp.Quit();
        }

        private void cat(object sender, RoutedEventArgs e)
        {
            Window6 main = new Window6();
            this.Hide();
            main.Show();
        }
    }
}