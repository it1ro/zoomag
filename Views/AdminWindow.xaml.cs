using System.Windows;
using Zoomag.Data;
using Zoomag.Models;
using Zoomag.Views.Reports;

namespace Zoomag.Views
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            var context = new AppDbContext();
            foreach (var item in context.Unit.ToList())
            {
                ed_izmm.Items.Add(item.Name);
            }
            foreach (var item in context.Category.ToList())
            {
                kat.Items.Add(item.Name);
            }

        }



        private void Dob_kat(object sender, RoutedEventArgs e)
        {
            var a = new_kat.Text;
            var context = new AppDbContext();
            var kateg = new Category { Name = a };
            context.Category.Add(kateg);
            context.SaveChanges();
            kat.Items.Add(a);

        }

        private void Zap(object sender, RoutedEventArgs e)
        {
            var b = new_ed.Text;
            var context = new AppDbContext();
            var ed_izm = new Unit { Name = b };
            context.Unit.Add(ed_izm);
            context.SaveChanges();
            ed_izmm.Items.Add(b);
        }

        private void Vvod(object sender, RoutedEventArgs e)
        {

            var Product = tovar.Text;
            var Price = price.Text;
            var Kol = kol_vo.Text;
            var context = new AppDbContext();
            var Edizm = context.Unit.Find(ed_izmm.SelectedIndex + 1);
            var kateg = context.Category.Find(kat.SelectedIndex + 1);
            var tovari = new Product { Name = Product, Unit = Edizm, Category = kateg, Price = Convert.ToInt32(Price), Amount = Convert.ToInt32(Kol) };
            context.Product.Add(tovari);
            context.SaveChanges();
        }

        private void Privoz(object sender, RoutedEventArgs e)
        {
            ProductEditor priv = new ProductEditor();
            this.Hide();
            priv.Show();
        }

        private void Sale(object sender, RoutedEventArgs e)
        {
            SaleWindow prod = new SaleWindow();
            this.Hide();
            prod.Show();
        }

        private void Sklad(object sender, RoutedEventArgs e)
        {
            ProductOverviewWindow sklad = new ProductOverviewWindow();
            this.Hide();
            sklad.Show();
        }

        private void Otchet(object sender, RoutedEventArgs e)
        {
            AdminReportsWindow sklad = new AdminReportsWindow();
            this.Hide();
            sklad.Show();
        }
    }
}
