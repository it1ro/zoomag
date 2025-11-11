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
    /// Логика взаимодействия для Window8.xaml
    /// </summary>
    public partial class Window8 : Window
    {
        public Window8()
        {
            InitializeComponent();
            var context = new AppDbContext();
            foreach (var item in context.Kategors.ToList())
            {
                cb.Items.Add(item.Typ);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Window7 window = new Window7();
            window.Show();
        }


        private void bt1_Click(object sender, RoutedEventArgs e)
        {
            DateTime sDate = Convert.ToDateTime(calll.SelectedDate);
            string q = sDate.ToString("MMMM d, yyyy");
            string w = tbbuk.Text;
            var context = new AppDbContext();
            var kt = cb.Text;
            var data = context.Tovar
                .Include(t => t.Kategor)
                .Where(x => x.Kategor.Typ == kt)
                .Where(t => t.Name.StartsWith(w))
                .OrderBy(t => t.Kol_vo)
                .ToList();
            var d = context.Privozs
                .Where(x => x.Data == q)
                .ToList();
            var r = from t in data join p in d on t.Name equals p.Name select new { p.ID, p.Data, p.Name, p.Kolvo, p.Price };
            dg8.ItemsSource = r.ToList();
        }
    }
}
