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

namespace prriva_10
{
    /// <summary>
    /// Логика взаимодействия для Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
            var context = new AppDbContext();
            {
                var data = context.Tovar
                .Include(t => t.Izmer)
                .ToList();
                dg2.ItemsSource = data;
            }
        }

        private void Kupit(object sender, RoutedEventArgs e)
        {
            var koli = kolich.Text;
            var koll = int.Parse(koli);
            var value = dg2.SelectedCells[0].Column.GetCellContent(dg2.SelectedCells[0].Item);
            string tovar_naim = (value as TextBlock)?.Text;
            var context = new AppDbContext();
            var tovar = context.Tovar.Where(x=>x.Name == tovar_naim).First();
            if (tovar != null)
            {
                if (tovar.Kol_vo != 0)
                {
                    if (tovar.Kol_vo < koll)
                    {
                        MessageBox.Show("Превышает количество товаров");
                        return;
                    }
                    else
                    {
                        tovar.Kol_vo -= koll;
                        context.SaveChanges();
                    }

                }
                else
                {
                    MessageBox.Show("Нет в наличии");
                    return;
                }
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            this.Hide();
            main.Show();
        }
    }
}
