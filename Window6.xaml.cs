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
using Excel = Microsoft.Office.Interop.Excel;

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
            var context = new AppDbContext();
            var q = from dd in context.Tovar
            where dd.Kategor.Typ == kat.SelectedItem.ToString()
                    select new { dd.Name, dd.Kol_vo, dd.Price, dd.Kategor,dd.Izmer };

            Excel.Application exapp = new Excel.Application();
            Excel.Workbook exwor;
            Excel.Worksheet exsheet;
            exapp.SheetsInNewWorkbook = 2;
            exwor = exapp.Workbooks.Add();

            exsheet = (Excel.Worksheet)exwor.Worksheets.get_Item(1);
            exsheet.Cells[1, 1] = "отчет по категориям на ";
            exsheet.Cells[1, 3] = DateTime.Today.ToString("MMMM d,yyyy");
            exsheet.Cells[3, 1] = "Наименование";
            exsheet.Cells[3, 3] = "Количество";
            exsheet.Cells[3, 5] = "Цена";
            exsheet.Cells[3, 7] = "Категория";
            exsheet.Cells[3, 9] = "ед/изм";

            int row = 4;
            int w = 0;
            foreach (var item in q)
            {
                
                exsheet.Cells[row, 1].Value = item.Name;
                exsheet.Cells[row, 3].Value = item.Kol_vo;
                exsheet.Cells[row, 5].Value = item.Price;
                exsheet.Cells[row, 7].Value = item.Kategor.Typ;
                exsheet.Cells[row, 9].Value = item.Izmer.Typ;

                row++;
                w++;
            }
            exapp.Visible = true;
            exsheet.Cells[w + 5, 1] = Convert.ToString(w) + " товаров";
            exwor.Saved = true;
            exapp.DisplayAlerts = false;
            exwor.SaveAs(@"C:\Users\student\Desktop/Журнал котегория " + DateTime.Today.ToString("MMMM d,yyyy") + ".xlsx");
            exapp.Quit();
        }

        private void nazad_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            this.Hide();
            main.Show();
        }

        private void kat_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
