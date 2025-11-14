using System.Windows;
using Zoomag.Data;

namespace Zoomag.Views
{
    using Data;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Логика взаимодействия для ProductOverviewWindow.xaml
    /// </summary>
    public partial class ProductOverviewWindow : Window
    {
        public ProductOverviewWindow()
        {
            InitializeComponent();
            var context = new AppDbContext();
            dg3.ItemsSource = context.Product
            .OrderBy(x => x.Name)
            .ToList();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var context = new AppDbContext();
            dg4.ItemsSource = context.Product
                .Include(t => t.Unit)
                .Include(t => t.Category)
                .OrderByDescending(x => x.Amount)
                .ToList();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AdminWindow main = new AdminWindow();
            this.Hide();
            main.Show();
        }

    }
}
