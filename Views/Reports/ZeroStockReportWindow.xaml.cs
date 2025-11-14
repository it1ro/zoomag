using System.Windows;
using Zoomag.Data;

namespace Zoomag.Views.Reports
{
    using Data;
    using Microsoft.EntityFrameworkCore;

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
                var context = new AppDbContext();
                var zeroStockItems = context.Product
                    .Include(t => t.Category) // Подгружаем связанные данные категории
                    .Include(t => t.Unit)   // Подгружаем связанные данные единицы измерения
                    .Where(t => t.Amount == 0)
                    .Select(t => new
                    {
                        // Создаём анонимный тип с нужными полями
                        Name = t.Name,
                        Category = t.Category != null ? t.Category.Name : "Без категории",
                        Unit = t.Unit != null ? t.Unit.Name : "Без ед.изм.",
                        Price = t.Price
                    })
                    .ToList();

                dgZeroStock.ItemsSource = zeroStockItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаём экземпляр генератора
                var context = new AppDbContext();
                var generator = new Services.ZeroStockReportGenerator(context);

                // Генерируем отчёт
                string filePath = generator.GenerateReport();

                // Показываем сообщение
                MessageBox.Show($" Отчёт сгенерирован:\n{filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($" Ошибка при генерации отчёта:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // Возвращаемся в главное меню (AdminReportsWindow)
            AdminReportsWindow main = new AdminReportsWindow();
            this.Hide();
            main.Show();
        }
    }
}
