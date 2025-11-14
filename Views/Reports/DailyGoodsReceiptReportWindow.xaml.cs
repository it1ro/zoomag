using System.Windows;
using Microsoft.EntityFrameworkCore;
using Zoomag.Data;
using System.Linq;

namespace Zoomag.Views.Reports
{
    public partial class DailyGoodsReceiptReportWindow : Window
    {
        public DailyGoodsReceiptReportWindow()
        {
            InitializeComponent();
            DeliveryDatePicker.SelectedDate = DateTime.Today;
            LoadData();
        }

        private void LoadData()
        {
            var selectedDate = DeliveryDatePicker.SelectedDate;
            if (!selectedDate.HasValue)
                return;

            DateTime date = selectedDate.Value.Date;

            try
            {
                using var context = new AppDbContext();
                var supplyItems = context.SupplyItem
                    .Include(si => si.Supply)
                    .Include(si => si.Product)
                    .Where(si => si.Supply.Date != null && si.Supply.Date == date)
                    .ToList();

                var reportItems = supplyItems
                    .Select(si => new ReportItem
                    {
                        Name = si.Product.Name,
                        Amount = si.Quantity,
                        Price = si.Price,
                        Total = si.Total
                    })
                    .ToList();

                int totalSum = reportItems.Sum(r => r.Total);
                int totalQuantity = reportItems.Sum(r => r.Amount);

                TotalAmountDisplay.Text = totalSum.ToString();
                TotalQuantityDisplay.Text = totalQuantity.ToString();
                ReportDataGrid.ItemsSource = reportItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}\nТип ошибки: {ex.GetType().Name}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateReport(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void ResetFilter(object sender, RoutedEventArgs e)
        {
            DeliveryDatePicker.SelectedDate = DateTime.Today;
            LoadData();
        }

        private void BackToMainMenu(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            this.Hide();
            adminWindow.Show();
        }

        private void NextToSalesReport(object sender, RoutedEventArgs e)
        {
            var nextWindow = new SupplyReportWindow();
            this.Hide();
            nextWindow.Show();
        }
    }

    public class ReportItem
    {
        public string Name { get; set; }
        public int Amount { get; set; }
        public int Price { get; set; }
        public int Total { get; set; }
    }
}
