using System.Windows;
using Microsoft.EntityFrameworkCore;
using Zoomag.Data;

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
            var selecteDeliverydDate = DeliveryDatePicker.SelectedDate;
            if (!selecteDeliverydDate.HasValue)
                return;

            DateTime date = selecteDeliverydDate.Value.Date;

            try
            {
                using var context = new AppDbContext();
                var supplyItems = context.SupplyItem
                    .Include(si => si.Supply)  // Загрузить поставку
                    .Include(si => si.Product) // Загрузить товар
                    .Where(si => si.Supply.Date != null && selecteDeliverydDate == date)
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

                sum.Text = totalSum.ToString();
                kol_tov.Text = totalQuantity.ToString();
                dataGrid.ItemsSource = reportItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}\nТип ошибки: {ex.GetType().Name}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            DeliveryDatePicker.SelectedDate = DateTime.Today;
            LoadData();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow main = new AdminWindow();
            this.Hide();
            main.Show();
        }

        private void NextWindowButton_Click(object sender, RoutedEventArgs e)
        {
            SupplyReportWindow nextWindow = new SupplyReportWindow();
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
