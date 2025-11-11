using Microsoft.Win32;
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
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Файл Excel|*XLSX;*.XLS";
            if (openDialog.ShowDialog() == true)
            {
                Excel.Application ExcelApp = new Excel.Application();
                Excel.Workbook WorkBookExcel = ExcelApp.Workbooks.Open(openDialog.FileName);
                Excel.Worksheet WorkSheetExcel = (Excel.Worksheet)WorkBookExcel.Sheets[1];

                Excel.Range ExcelRange = WorkSheetExcel.UsedRange;
                int rowCount = ExcelRange.Rows.Count;
                string[] list = new string[100];
                for (int i = 1; i < (int)rowCount; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        ExcelRange = WorkSheetExcel.Cells[i + 1, j + 1] as Excel.Range;
                        if (ExcelRange != null && ExcelRange.Value2 != null)
                        {
                            list[j] = ExcelRange.Value2.ToString();
                        }
                        else list[j] = "";

                    }
                        var data = new Test { data = DateTime.Today.ToString("MMMM d,yyyy"), naim = list[0], ed = list[1], kol_vo = Convert.ToInt32(list[2]), cena = Convert.ToInt32(list[3])};
                        dg.Items.Add(data);
                }
                WorkBookExcel.Close(false, Type.Missing, Type.Missing);
                ExcelApp.Quit();
                GC.Collect();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var Context = new AppDbContext();
            for (int i = 0; i < dg.Items.Count; i++)
            {
                Test path = dg.Items[i] as Test;
                var priv1 = new Privozs { Data = path.data, Name = path.naim,Kolvo =path.kol_vo , Price = path.cena };
                Context.Privozs.Add(priv1);
                var tovar = Context.Tovar.Where(x => x.Name == path.naim).FirstOrDefault();
                if (tovar != null)
                {
                    tovar.Kol_vo += path.kol_vo;
                    if (tovar.Price < path.cena) tovar.Price = path.cena;
                }
                else
                {
                    var ed = Context.Izmers.Find(1);

                    if (path.cena < 100)
                    {
                        var kat = Context.Kategors.Find(1);
                        var newtovar1 = new Tovars
                        {
                            Name = path.naim,
                            Price = path.cena,
                            Kol_vo = path.kol_vo,
                            Izmer = ed,
                            Kategor = kat
                        };
                        Context.Tovar.Add(newtovar1);
                        Context.SaveChanges();

                    }
                    else
                    {
                        var kat = Context.Kategors.Find(2);

                        var newtovar2 = new Tovars
                        {
                            Name = path.naim,
                            Price = path.cena,
                            Kol_vo = path.kol_vo,
                            Izmer = ed,
                            Kategor = kat
                        };
                        Context.Tovar.Add(newtovar2);
                       
                    }

                }
                Context.SaveChanges();
            }
            
            }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            this.Hide();
            main.Show();
        }
    }
}
