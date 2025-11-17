using System.Windows;
using System.IO;
using ClosedXML.Excel;
using Zoomag.Data;
using System.Linq;
using System;
using Zoomag.Models;
// Добавлено для SaveFileDialog
using Microsoft.Win32;

namespace Zoomag.Views.Reports
{
    using Microsoft.EntityFrameworkCore;

    public partial class IncomingGoodsJournalWindow : Window
    {
        public IncomingGoodsJournalWindow()
        {
            InitializeComponent();
            LoadSupplies();
        }

        private void LoadSupplies()
        {
            using var context = new AppDbContext();
            var supplies = context.Supply
                .Include(s => s.SupplyItems)
                    .ThenInclude(si => si.Product)
                .OrderBy(s => s.Date)
                .ToList();

            var displayList = supplies.SelectMany(s => s.SupplyItems.Select(si => new
            {
                SupplyDate = s.Date,
                ProductName = si.Product.Name,
                Quantity = si.Quantity,
                Price = si.Price,
                Total = si.Quantity * si.Price
            })).ToList();

            GoodsReceiptsGrid.ItemsSource = displayList;
        }

        private void ExportToExcel(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            var supplies = context.Supply
                .Include(s => s.SupplyItems)
                    .ThenInclude(si => si.Product)
                .OrderBy(s => s.Date)
                .ToList();

            var exportData = supplies.SelectMany(s => s.SupplyItems.Select(si => new
            {
                SupplyDate = s.Date,
                ProductName = si.Product.Name,
                Quantity = si.Quantity,
                Price = si.Price,
                Total = si.Quantity * si.Price
            })).ToList();

            if (!exportData.Any())
            {
                MessageBox.Show("Нет данных для отчёта", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // --- Новый код для выбора места сохранения ---
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (.xlsx)|*.xlsx|All Files (*.*)|*.*", // Фильтр для типов файлов
                FileName = $"Журнал поступления товаров {DateTime.Now:yyyy-MM-dd}.xlsx", // Предлагаемое имя файла
                DefaultExt = ".xlsx", // Расширение по умолчанию
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) // Начальная папка - рабочий стол
            };

            // Показываем диалоговое окно
            bool? result = saveFileDialog.ShowDialog();

            // Проверяем, выбрал ли пользователь файл для сохранения
            if (result != true)
            {
                // Пользователь отменил операцию
                return;
            }

            string fileName = saveFileDialog.FileName; // Получаем выбранный путь к файлу

            // Проверка, чтобы пользователь действительно выбрал .xlsx
            if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                // Если пользователь не указал .xlsx, добавляем его
                fileName += ".xlsx";
            }

            // --- Конец нового кода ---

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Журнал поступления товаров");

            worksheet.Cell(1, 1).Value = "Журнал поступления товаров на";
            worksheet.Cell(1, 3).Value = DateTime.Now.ToString("MMMM d, yyyy");

            worksheet.Cell(3, 1).Value = "Дата";
            worksheet.Cell(3, 2).Value = "Наименование";
            worksheet.Cell(3, 3).Value = "Количество";
            worksheet.Cell(3, 4).Value = "Цена";
            worksheet.Cell(3, 5).Value = "Общая сумма";

            worksheet.Range(3, 1, 3, 5).Style.Font.Bold = true;

            int row = 4;
            foreach (var item in exportData)
            {
                worksheet.Cell(row, 1).Value = item.SupplyDate;
                worksheet.Cell(row, 2).Value = item.ProductName;
                worksheet.Cell(row, 3).Value = item.Quantity;
                worksheet.Cell(row, 4).Value = item.Price;
                worksheet.Cell(row, 5).Value = item.Total;
                row++;
            }

            worksheet.Column(1).Style.NumberFormat.SetFormat("dd/MM/yyyy");
            worksheet.Column(3).Style.NumberFormat.SetFormat("#,##0.00");


            worksheet.Columns().AdjustToContents();

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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            this.Hide();
            adminWindow.Show();
        }
    }
}
