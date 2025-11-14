using System.Windows;
using Zoomag.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Zoomag.Views.Reports
{
    /// <summary>
    /// Логика взаимодействия для ZeroStockReportWindow.xaml
    /// </summary>
    public partial class ZeroStockReportWindow : Window
    {
        public ZeroStockReportWindow()
        {
            InitializeComponent();
            LoadZeroStockData();
        }

        private void LoadZeroStockData()
        {
            try
            {
                using var context = new AppDbContext();
                var zeroStockItems = context.Product
                    .Include(product => product.Category)
                    .Include(product => product.Unit)
                    .Where(product => product.Amount == 0)
                    .Select(product => new
                    {
                        Name = product.Name,
                        Category = product.Category != null ? product.Category.Name : "Без категории",
                        Unit = product.Unit != null ? product.Unit.Name : "Без ед.изм.",
                        Price = product.Price
                    })
                    .ToList();

                ZeroStockGrid.ItemsSource = zeroStockItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToExcel(object sender, RoutedEventArgs e)
        {
            try
            {
                using var context = new AppDbContext();
                var generator = new Services.ZeroStockReportGenerator(context);
                string filePath = generator.GenerateReport();

                MessageBox.Show($"Отчёт сгенерирован:\n{filePath}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации отчёта:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoToAdminReports(object sender, RoutedEventArgs e)
        {
            var adminReportsWindow = new AdminReportsWindow();
            this.Hide();
            adminReportsWindow.Show();
        }
    }
}
