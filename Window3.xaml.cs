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
    /// Логика взаимодействия для Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        public Window3()
        {
            InitializeComponent();
            var context = new AppDbContext();
            dg3.ItemsSource = context.Tovar
            .OrderBy(x => x.Name)
            .ToList ();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var context = new AppDbContext();
            dg4.ItemsSource = context.Tovar
                .Include(t => t.Izmer)
                .ToList();
            dg4.ItemsSource = context.Tovar
                .Include(t => t.Kategor)
                .ToList();
            dg4.ItemsSource = context.Tovar
                .OrderByDescending(x => x.Kol_vo)
                .ToList ();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            this.Hide();
            main.Show();
        }

    }
}
