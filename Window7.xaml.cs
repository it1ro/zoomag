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
                    select new { dd.Data, dd.Name, dd.Price, dd.Kolvo};

            Excel.Application exapp = new Excel.Application();
            Excel.Workbook exwor;
            Excel.Worksheet exsheet;
            exapp.SheetsInNewWorkbook = 2;
            exwor = exapp.Workbooks.Add();

            exsheet = (Excel.Worksheet)exwor.Worksheets.get_Item(1);
            exsheet.Cells[1, 1] = "отчет о поступлении товаров за день на";
            exsheet.Cells[1, 4] = d;
            exsheet.Cells[3, 3] = "Наименование";
            exsheet.Cells[3, 6] = "Количество";
            exsheet.Cells[3, 5] = "Цена";
            exsheet.Cells[3, 1] = "Дата";
            int summ = 0;
            int shtuk = 0;
            int kol_vo = 0;
            int row = 4;
            int w = 0;
            foreach (var item in q)
            {
                if (d == item.Data)
                {
                    exsheet.Cells[row, 3].Value = item.Name;
                    exsheet.Cells[row, 6].Value = item.Kolvo;
                    exsheet.Cells[row, 5].Value = item.Price;
                    exsheet.Cells[row, 1].Value = item.Data;
                    shtuk += item.Kolvo;
                    row++;
                    w++;
                    summ += item.Kolvo * item.Price;
                    kol_vo += item.Kolvo;
                }
            }
            exapp.Visible = true;
            exsheet.Cells[w + 5, 1] = Convert.ToString(w) + " товаров";
            exsheet.Cells[w + 7, 1] = "Итог=" + Convert.ToString(summ) + " рубля";
            exwor.Saved = true;
            exapp.DisplayAlerts = false;
            sum.Text = Convert.ToString(summ);
            kol_tov.Text = Convert.ToString(kol_vo);

            exwor.SaveAs(@"C:\Users\student\Desktop/Отчет поступления товаров за день на " + DateTime.Today.ToString("MMMM d,yyyy") + ".xlsx");
            exapp.Quit();

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
