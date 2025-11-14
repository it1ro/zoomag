using System.Windows;
using System.Windows.Controls;
using Zoomag.Data;
using System.Linq;

namespace Zoomag.Views.Reports
{
    public partial class SupplyReportWindow : Window
    {
        public SupplyReportWindow()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            using var context = new AppDbContext();
            foreach (var category in context.Category.ToList())
            {
                CategoryFilter.Items.Add(category.Name);
            }
        }

        private void GenerateReport(object sender, RoutedEventArgs e)
        {
            var context = new AppDbContext();

            var query = context.SupplyItem
                .AsQueryable();

            // Фильтр по категории
            if (CategoryFilter.SelectedItem != null)
            {
                var selectedCategory = CategoryFilter.SelectedItem.ToString();
                query = query.Where(si => si.Product.Category.Name == selectedCategory);
            }

            // Фильтр по первой букве названия
            if (!string.IsNullOrWhiteSpace(FirstLetterFilter.Text))
            {
                var firstLetter = FirstLetterFilter.Text.Trim().ToLower();
                query = query.Where(si => si.Product.Name.ToLower().StartsWith(firstLetter));
            }

            // Фильтр по дате
            if (DateFilter.SelectedDate != null)
            {
                var selectedDate = DateFilter.SelectedDate.Value.Date;
                query = query.Where(si => si.Supply.Date.Date == selectedDate);
            }

            var reportData = query
                .Select(si => new SupplyReportItem
                {
                    Date = si.Supply.Date,
                    Name = si.Product.Name,
                    Quantity = si.Quantity,
                    Price = si.Price
                })
                .ToList();

            SupplyReportGrid.ItemsSource = reportData;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            this.Hide();
            adminWindow.Show();
        }
    }

    public class SupplyReportItem
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
