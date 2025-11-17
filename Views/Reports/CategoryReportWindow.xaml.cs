using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Zoomag.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
// Добавлено для SaveFileDialog
using Microsoft.Win32;
using System.IO;

namespace Zoomag.Views.Reports
{
    /// <summary>
    /// Логика взаимодействия для CategoryReportWindow.xaml
    /// </summary>
    public partial class CategoryReportWindow : Window
    {
        public CategoryReportWindow()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            using var context = new AppDbContext();
            foreach (var category in context.Category.ToList())
            {
                CategorySelector.Items.Add(category.Name);
            }
        }

        private void ExportToExcel(object sender, RoutedEventArgs e)
        {
            if (CategorySelector.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите категорию.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- Новый код для выбора места сохранения ---
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (.xlsx)|*.xlsx|All Files (*.*)|*.*", // Фильтр типов файлов
                // Используем выбранную категорию в имени файла
                FileName = $"Отчет по категории {CategorySelector.SelectedItem.ToString()} {DateTime.Now:yyyy-MM-dd}.xlsx",
                DefaultExt = ".xlsx", // Расширение по умолчанию
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) // Начальная папка
            };

            bool? result = saveFileDialog.ShowDialog(); // Показываем диалог

            if (result != true) // Проверяем, подтвердил ли пользователь
            {
                return; // Выходим из метода, если отменено
            }

            string fileName = saveFileDialog.FileName; // Получаем выбранный путь

            if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".xlsx"; // Добавляем .xlsx, если пользователь не указал
            }
            // --- Конец нового кода ---

            using var context = new AppDbContext();
            var selectedCategory = CategorySelector.SelectedItem.ToString();
            var products = context.Product
                .Where(product => product.Category.Name == selectedCategory)
                .Select(product => new {
                    product.Name,
                    product.Amount,
                    product.Price,
                    product.Category,
                    product.Unit
                });

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Отчет по категории");

            worksheet.Cell(1, 1).Value = "Отчет по категориям на";
            worksheet.Cell(1, 3).Value = DateTime.Today.ToString("MMMM d,yyyy");

            worksheet.Cell(3, 1).Value = "Наименование";
            worksheet.Cell(3, 3).Value = "Количество";
            worksheet.Cell(3, 5).Value = "Цена";
            worksheet.Cell(3, 7).Value = "Категория";
            worksheet.Cell(3, 9).Value = "Ед/изм";

            int row = 4;
            int productCount = 0;

            foreach (var product in products)
            {
                worksheet.Cell(row, 1).Value = product.Name;
                worksheet.Cell(row, 3).Value = product.Amount;
                worksheet.Cell(row, 5).Value = product.Price;
                worksheet.Cell(row, 7).Value = product.Category.Name;
                worksheet.Cell(row, 9).Value = product.Unit.Name;
                row++;
                productCount++;
            }

            worksheet.Cell(productCount + 5, 1).Value = $"{productCount} товаров";

            // Сохраняем в выбранный пользователем файл
            try
            {
                workbook.SaveAs(fileName);
                MessageBox.Show($"Файл сохранён: {fileName}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoToAdmin(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            this.Hide();
            adminWindow.Show();
        }

        private void OnCategoryChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategorySelector.SelectedItem == null) return;

            using var context = new AppDbContext();
            CategoryProductsGrid.ItemsSource = context.Product
                .Include(product => product.Category)
                .Include(product => product.Unit)
                .Where(product => product.Category.Name == CategorySelector.SelectedItem.ToString())
                .ToList();
        }
    }
}
