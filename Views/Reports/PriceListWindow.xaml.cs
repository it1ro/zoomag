using System.Windows;
using System.IO;
using System.Linq;
using Zoomag.Data;
using Zoomag.Models;
using Microsoft.Win32; // Для SaveFileDialog
using ClosedXML.Excel;

namespace Zoomag.Views.Reports
{
    using Microsoft.EntityFrameworkCore;

    public partial class PriceListWindow : Window
    {
        public PriceListWindow()
        {
            InitializeComponent();
            LoadPriceList();
        }

        private void LoadPriceList()
        {
            using var context = new AppDbContext();
            // Загружаем только товары с количеством > 0, как в оригинальном методе
            var products = context.Product
                .Where(product => product.Amount > 0)
                .Select(product => new
                {
                    Name = product.Name,
                    Price = product.Price,
                    Amount = product.Amount
                })
                .ToList();

            PriceListGrid.ItemsSource = products;
        }

        private void ExportToExcel(object sender, RoutedEventArgs e)
        {
            // Загружаем данные заново, как и при отображении
            using var context = new AppDbContext();
            var exportData = context.Product
                .Where(product => product.Amount > 0)
                .Select(product => new
                {
                    Name = product.Name,
                    Price = product.Price,
                    Amount = product.Amount
                })
                .ToList();

            if (!exportData.Any())
            {
                MessageBox.Show("Нет данных для отчета.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Диалог выбора файла
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (.xlsx)|*.xlsx|All Files (*.*)|*.*",
                FileName = $"Прайс-лист на {DateTime.Today:yyyy-MM-dd}.xlsx", // Имя файла по умолчанию
                DefaultExt = ".xlsx",
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

            // Создание Excel-файла
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Прайс-лист");

            worksheet.Cell(1, 1).Value = "Прайс-лист на";
            worksheet.Cell(1, 3).Value = DateTime.Today.ToString("MMMM dd yyyy");

            worksheet.Cell(3, 1).Value = "Наименование";
            worksheet.Cell(3, 2).Value = "Цена";
            worksheet.Cell(3, 3).Value = "Количество";

            worksheet.Range(3, 1, 3, 3).Style.Font.Bold = true; // Жирный шрифт для заголовков

            int row = 4;
            foreach (var item in exportData)
            {
                worksheet.Cell(row, 1).Value = item.Name;
                worksheet.Cell(row, 2).Value = item.Price;
                worksheet.Cell(row, 3).Value = item.Amount;
                row++;
            }

            worksheet.Cell(exportData.Count + 5, 1).Value = $"{exportData.Count} товаров";

            // Автоширина столбцов
            worksheet.Columns().AdjustToContents();

            try
            {
                workbook.SaveAs(fileName); // Сохраняем в выбранный файл
                MessageBox.Show($"Отчет сохранен: {fileName}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoToAdmin(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            this.Hide();
            adminWindow.Show();
        }
    }
}
