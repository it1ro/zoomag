using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace prriva_10
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var context = new AppDbContext();
            foreach (var item in context.Izmers.ToList())
            {
                ed_izmm.Items.Add(item.Typ);
            }
            foreach (var item in context.Kategors.ToList())
            {
                kat.Items.Add(item.Typ);
            }

        }



        private void Dob_kat(object sender, RoutedEventArgs e)
        {
            var a = new_kat.Text;
            var context = new AppDbContext();
            var kateg = new Kategors { Typ = a };
            context.Kategors.Add(kateg);
            context.SaveChanges();
            kat.Items.Add(a);
            
        }

        private void Zap(object sender, RoutedEventArgs e)
        {
            var b = new_ed.Text;
            var context = new AppDbContext();
            var ed_izm = new Izmers { Typ = b };
            context.Izmers.Add(ed_izm);
            context.SaveChanges();
            ed_izmm.Items.Add(b);
        }

        private void Vvod(object sender, RoutedEventArgs e)
        {

            var Tovar = tovar.Text;
            var Price = price.Text;
            var Kol = kol_vo.Text;
            var context = new AppDbContext();
            var Edizm = context.Izmers.Find(ed_izmm.SelectedIndex + 1);
            var kateg = context.Kategors.Find(kat.SelectedIndex + 1);
            var tovari = new Tovars { Name = Tovar, Izmer = Edizm, Kategor = kateg, Price = Convert.ToInt32(Price), Kol_vo = Convert.ToInt32(Kol) };
            context.Tovar.Add(tovari);
            context.SaveChanges();
        }

        private void Privoz(object sender, RoutedEventArgs e)
        {
            Window1 priv = new Window1();
            this.Hide();
            priv.Show();
        }

        private void Prodajas(object sender, RoutedEventArgs e)
        {
            Window2 prod = new Window2();
            this.Hide();
            prod.Show();
        }

        private void Sklad(object sender, RoutedEventArgs e)
        {
            Window3 sklad = new Window3();
            this.Hide();
            sklad.Show();
        }

        private void Otchet(object sender, RoutedEventArgs e)
        {
            Window4 sklad = new Window4();
            this.Hide();
            sklad.Show();
        }
    }
}