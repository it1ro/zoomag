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
    /// Логика взаимодействия для Window5.xaml
    /// </summary>
    public partial class Window5 : Window
    {
        public Window5()
        {
            InitializeComponent();
            var context = new AppDbContext();
            dg5.ItemsSource = context.Privozs
            .OrderBy(x => x.Name)
            .ToList();
        }

        private void viv_v_excel_Click(object sender, RoutedEventArgs e)
        {
            var context = new AppDbContext();
            var q = from dd in context.Privozs
                    select new { dd.Name, dd.Kolvo, dd.Price,dd.Data};

            Excel.Application exapp = new Excel.Application();
            Excel.Workbook exwor;
            Excel.Worksheet exsheet;
            exapp.SheetsInNewWorkbook = 2;
            exwor = exapp.Workbooks.Add();

            exsheet = (Excel.Worksheet)exwor.Worksheets.get_Item(1);
            exsheet.Cells[1, 1] = "Журнал поступления товаров на ";
            exsheet.Cells[1, 3] = DateTime.Today.ToString("MMMM d,yyyy");
            exsheet.Cells[3, 7] = "Дата";
            exsheet.Cells[3, 1] = "Наименование";
            exsheet.Cells[3, 3] = "Количество";
            exsheet.Cells[3, 5] = "Цена";

            int row = 4;
            int w = 0;
            foreach (var item in q)
            {
                exsheet.Cells[row, 1].Value = item.Name;
                exsheet.Cells[row, 3].Value = item.Kolvo;
                exsheet.Cells[row, 5].Value = item.Price;
                exsheet.Cells[row, 7].Value = item.Data;
                row++;
                w++;
            }
            exapp.Visible = true;
            exsheet.Cells[w + 5, 1] = Convert.ToString(w) + " товаров";
            exwor.Saved = true;
            exapp.DisplayAlerts = false;
            exwor.SaveAs(@"C:\Users\student\Desktop/Журнал поступления товаров" + DateTime.Today.ToString("MMMM d,yyyy") + ".xlsx");
            exapp.Quit();
        }

        private void nazad_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            this.Hide();
            main.Show();
        }
    }
}
