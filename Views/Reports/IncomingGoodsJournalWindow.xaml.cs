using System.Windows;
using System.IO;
using ClosedXML.Excel;
using Zoomag.Data;
using System.Linq;
using System;
using Zoomag.Models;

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
            // Загружаем поставки и их элементы (SupplyItem), включая продукты
            var supplies = context.Supply
                .Include(s => s.SupplyItems)
                    .ThenInclude(si => si.Product)
                .OrderBy(s => s.Date) // Сортировка по дате, как более логичная для журнала
                .ToList();

            // Преобразуем в список анонимных объектов или создайте ViewModel для отображения
            // В данном случае, создадим временный список для DataGrid
            var displayList = supplies.SelectMany(s => s.SupplyItems.Select(si => new
            {
                SupplyDate = s.Date,
                ProductName = si.Product.Name,
                Quantity = si.Quantity,
                Price = si.Price, // Цена из SupplyItem
                Total = si.Quantity * si.Price // Вычисляем общую сумму
            })).ToList();

            GoodsReceiptsGrid.ItemsSource = displayList;
        }


        private void ExportToExcel(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();
            // Загружаем данные, аналогично LoadSupplies
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

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Журнал поступления товаров");

            worksheet.Cell(1, 1).Value = "Журнал поступления товаров на";
            worksheet.Cell(1, 3).Value = DateTime.Now.ToString("MMMM d, yyyy");

            // Заголовки столбцов
            worksheet.Cell(3, 1).Value = "Дата";
            worksheet.Cell(3, 2).Value = "Наименование";
            worksheet.Cell(3, 3).Value = "Количество";
            worksheet.Cell(3, 4).Value = "Цена";
            worksheet.Cell(3, 5).Value = "Общая сумма";

            worksheet.Range(3, 1, 3, 5).Style.Font.Bold = true; // Жирный шрифт для заголовков

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

            // Форматирование столбцов
            worksheet.Column(1).Style.NumberFormat.SetFormat("dd/MM/yyyy"); // Формат даты
            worksheet.Column(3).Style.NumberFormat.SetFormat("#,##0.00"); // Формат для количества
            worksheet.Column(4).Style.NumberFormat.SetFormat("#,##0.00 [$€-419]"); // Пример формата валюты (евро, русская локализация)
            worksheet.Column(5).Style.NumberFormat.SetFormat("#,##0.00 [$€-419]"); // Формат для общей суммы

            worksheet.Columns().AdjustToContents(); // Автоширина столбцов

            string fileName = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"Журнал поступления товаров {DateTime.Now:yyyy-MM-dd}.xlsx"
            );

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
