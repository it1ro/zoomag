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
using ClosedXML.Excel; // Заменили using Excel = Microsoft.Office.Interop.Excel;

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
            openDialog.Filter = "Файлы Excel|*.xlsx;*.xls;*.xlsm|Все файлы|*.*";
            if (openDialog.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook(openDialog.FileName))
                {
                    var ws = workbook.Worksheets.Worksheet(1); // или .Worksheet("ИмяЛиста")
                    var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

                    for (int i = 2; i <= lastRow; i++) // начинаем с 2, т.к. 1 — заголовки
                    {
                        string naim = ws.Cell(i, 1).IsEmpty() ? "" : ws.Cell(i, 1).GetString();
                        string ed = ws.Cell(i, 2).IsEmpty() ? "" : ws.Cell(i, 2).GetString();
                        string kol_voStr = ws.Cell(i, 3).IsEmpty() ? "0" : ws.Cell(i, 3).GetString();
                        string cenaStr = ws.Cell(i, 4).IsEmpty() ? "0" : ws.Cell(i, 4).GetString();

                        if (int.TryParse(kol_voStr, out int kol_vo) && int.TryParse(cenaStr, out int cena))
                        {
                            var data = new Test
                            {
                                data = DateTime.Today.ToString("MMMM d,yyyy"),
                                naim = naim,
                                ed = ed,
                                kol_vo = kol_vo,
                                cena = cena
                            };
                            dg.Items.Add(data);
                        }
                        else
                        {
                            MessageBox.Show($"Неверный формат данных в строке {i}.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var Context = new AppDbContext();
            for (int i = 0; i < dg.Items.Count; i++)
            {
                Test path = dg.Items[i] as Test;
                var priv1 = new Privozs { Data = path.data, Name = path.naim, Kolvo = path.kol_vo, Price = path.cena };
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