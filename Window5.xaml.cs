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
                    select new { dd.Name, dd.Kolvo, dd.Price, dd.Data };

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Журнал поступления товаров");

                ws.Cell(1, 1).Value = "Журнал поступления товаров на ";
                ws.Cell(1, 3).Value = DateTime.Today.ToString("MMMM d,yyyy");

                ws.Cell(3, 7).Value = "Дата";
                ws.Cell(3, 1).Value = "Наименование";
                ws.Cell(3, 3).Value = "Количество";
                ws.Cell(3, 5).Value = "Цена";

                int row = 4;
                int w = 0;
                foreach (var item in q)
                {
                    ws.Cell(row, 1).Value = item.Name;
                    ws.Cell(row, 3).Value = item.Kolvo;
                    ws.Cell(row, 5).Value = item.Price;
                    ws.Cell(row, 7).Value = item.Data;
                    row++;
                    w++;
                }

                ws.Cell(w + 5, 1).Value = w + " товаров";

                string fileName = $@"C:\Users\student\Desktop\Журнал поступления товаров {DateTime.Today:MMMM d,yyyy}.xlsx";
                workbook.SaveAs(fileName);
            }
        }
        private void nazad_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            this.Hide();
            main.Show();
        }
    }
}