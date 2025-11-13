using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace prriva_10
{
    /// <summary>
    /// Логика взаимодействия для Window9.xaml
    /// </summary>
    public partial class Window9 : Window
    {
        public Window9()
        {
            InitializeComponent();
            LoadZeroStockData();
        }

        private void LoadZeroStockData()
        {
            try
            {
                var context = new AppDbContext();
                var zeroStockItems = context.Tovar
                    .Include(t => t.Kategor) // Подгружаем связанные данные категории
                    .Include(t => t.Izmer)   // Подгружаем связанные данные единицы измерения
                    .Where(t => t.Kol_vo == 0)
                    .Select(t => new
                    {
                        // Создаём анонимный тип с нужными полями
                        Name = t.Name,
                        Category = t.Kategor != null ? t.Kategor.Typ : "Без категории",
                        Unit = t.Izmer != null ? t.Izmer.Typ : "Без ед.изм.",
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
            // Возвращаемся в главное меню (Window4)
            Window4 main = new Window4();
            this.Hide();
            main.Show();
        }
    }
}