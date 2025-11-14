using System.Windows;
using Microsoft.EntityFrameworkCore;
using Zoomag.Data;

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
            var categories = context.Category.ToList();
            foreach (var category in categories)
            {
                cb.Items.Add(category.Name);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            var previousWindow = new DailyGoodsReceiptReportWindow();
            previousWindow.Show();
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedDate = GetSelectedDate();
                if (!selectedDate.HasValue)
                {
                    MessageBox.Show("Please select a date.", "Date Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedCategory = cb.Text;
                var firstLetter = tbbuk.Text?.Trim();

                if (string.IsNullOrEmpty(selectedCategory))
                {
                    MessageBox.Show("Please select a category.", "Category Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(firstLetter))
                {
                    MessageBox.Show("Please enter the first letter of product name.", "Name Letter Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var reportData = GetSupplyReport(selectedDate.Value, selectedCategory, firstLetter);
                dg8.ItemsSource = reportData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DateTime? GetSelectedDate()
        {
            if (DatePicker.SelectedDate != null)  // ✅ Правильное имя
            {
                return DatePicker.SelectedDate.Value.Date;
            }
            return null;
        }

        private List<SupplyReportItem> GetSupplyReport(DateTime date, string category, string nameStartsWith)
        {
            using var context = new AppDbContext();

            var reportData = context.SupplyItem
                .Include(si => si.Supply)
                .Include(si => si.Product)
                .ThenInclude(p => p.Category)
                .Where(si => si.Supply.Date == date &&  // ✅ SupplyDate
                             si.Product.Category.Name == category &&
                             si.Product.Name.StartsWith(nameStartsWith, StringComparison.OrdinalIgnoreCase))
                .OrderBy(si => si.Product.Amount)
                .Select(si => new SupplyReportItem
                {
                    Id = si.SupplyId,
                    Data = si.Supply.Date,  // ✅ SupplyDate
                    Naim = si.Product.Name,
                    Amount = si.Quantity,
                    Price = si.Price
                })
                .ToList();

            return reportData;
        }

        private class SupplyReportItem
        {
            public int Id { get; set; }
            public DateTime Data { get; set; }  // ✅ DateTime, не string
            public string Naim { get; set; }
            public int Amount { get; set; }
            public int Price { get; set; }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }
}
