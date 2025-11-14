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
            LoadUnits();
            LoadCategories();
        }

        private void LoadUnits()
        {
            var context = new AppDbContext();
            foreach (var item in context.Unit.ToList())
            {
                UnitSelector.Items.Add(item.Name);
            }
        }

        private void LoadCategories()
        {
            var context = new AppDbContext();
            foreach (var item in context.Category.ToList())
            {
                CategorySelector.Items.Add(item.Name);
            }
        }

        private void AddCategory(object sender, RoutedEventArgs e)
        {
            var categoryName = CategoryInput.Text;
            if (string.IsNullOrWhiteSpace(categoryName)) return;

            var context = new AppDbContext();
            var category = new Category { Name = categoryName };
            context.Category.Add(category);
            context.SaveChanges();
            CategorySelector.Items.Add(categoryName);
            CategoryInput.Clear();
        }

        private void AddUnit(object sender, RoutedEventArgs e)
        {
            var unitName = UnitInput.Text;
            if (string.IsNullOrWhiteSpace(unitName)) return;

            var context = new AppDbContext();
            var unit = new Unit { Name = unitName };
            context.Unit.Add(unit);
            context.SaveChanges();
            UnitSelector.Items.Add(unitName);
            UnitInput.Clear();
        }

        private void SaveProduct(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProductNameInput.Text) ||
                string.IsNullOrWhiteSpace(PriceInput.Text) ||
                string.IsNullOrWhiteSpace(QuantityInput.Text))
                return;

            var context = new AppDbContext();
            var unit = context.Unit.Find(UnitSelector.SelectedIndex + 1);
            var category = context.Category.Find(CategorySelector.SelectedIndex + 1);

            var product = new Product
            {
                Name = ProductNameInput.Text,
                Unit = unit,
                Category = category,
                Price = Convert.ToInt32(PriceInput.Text),
                Amount = Convert.ToInt32(QuantityInput.Text)
            };

            context.Product.Add(product);
            context.SaveChanges();

            ClearProductForm();
        }

        private void ClearProductForm()
        {
            ProductNameInput.Clear();
            QuantityInput.Clear();
            PriceInput.Clear();
            UnitSelector.SelectedIndex = -1;
            CategorySelector.SelectedIndex = -1;
        }

        private void ShowArrival(object sender, RoutedEventArgs e)
        {
            var arrivalWindow = new ProductEditor();
            this.Hide();
            arrivalWindow.Show();
        }

        private void ShowSale(object sender, RoutedEventArgs e)
        {
            var saleWindow = new SaleWindow();
            this.Hide();
            saleWindow.Show();
        }

        private void ShowStock(object sender, RoutedEventArgs e)
        {
            var stockWindow = new ProductOverviewWindow();
            this.Hide();
            stockWindow.Show();
        }

        private void ShowReports(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new AdminReportsWindow();
            this.Hide();
            reportsWindow.Show();
        }
    }
}
