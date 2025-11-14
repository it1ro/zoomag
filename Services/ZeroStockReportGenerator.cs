// Файл: Zoomag/Services/ZeroStockReportGenerator.cs

using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore; // <- Добавь это
using Microsoft.Win32; // <- Для SaveFileDialog
using System;
using System.Linq;
using System.Windows;
using Zoomag.Data; // <- Для MessageBox

namespace Zoomag.Services
{
    using Data;

    public class ZeroStockReportGenerator
    {
        private readonly AppDbContext _context;

        public ZeroStockReportGenerator(AppDbContext context)
        {
            _context = context;
        }

        public string GenerateReport(string outputPath = null)
        {
            // 1. Получаем товары с нулевым остатком, подгружая связи
            var zeroStockItems = _context.Product
                .Include(t => t.Category) // Подгружаем категории
                .Include(t => t.Unit)   // Подгружаем единицы измерения
                .Where(t => t.Amount == 0)
                .Select(t => new
                {
                    t.Name,
                    // Теперь безопасно обращаемся к Name, т.к. связи подгружены
                    Category = t.Category != null ? t.Category.Name : "Без категории",
                    Unit = t.Unit != null ? t.Unit.Name : "Без ед.изм.",
                    t.Price
                })
                .OrderBy(x => x.Name)
                .ToList();

            // 2. Создаём Excel-файл
            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Нулевые позиции");

            // Заголовок
            ws.Cell(1, 1).Value = "Отчёт по нулевым позициям";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 16;
            ws.Cell(1, 3).Value = $"Сгенерировано: {DateTime.Now:dd.MM.yyyy HH:mm}";
            ws.Row(1).Height = 30;

            // Подзаголовки
            ws.Cell(3, 1).Value = "Наименование";
            ws.Cell(3, 2).Value = "Категория";
            ws.Cell(3, 3).Value = "Ед. изм.";
            ws.Cell(3, 4).Value = "Цена (₽)";
            ws.Row(3).Style.Font.Bold = true;
            ws.Row(3).Height = 25;

            // Данные
            int row = 4;
            foreach (var item in zeroStockItems)
            {
                ws.Cell(row, 1).Value = item.Name;
                ws.Cell(row, 2).Value = item.Category;
                ws.Cell(row, 3).Value = item.Unit;
                ws.Cell(row, 4).Value = item.Price;
                row++;
            }

            // Итог
            ws.Cell(row + 1, 1).Value = $"Всего нулевых позиций: {zeroStockItems.Count}";
            ws.Cell(row + 1, 1).Style.Font.Bold = true;
            ws.Cell(row + 1, 1).Style.Font.FontColor = XLColor.Red;

            // Автоподбор ширины столбцов
            ws.Columns().AdjustToContents();

            // 3. Выбираем путь через диалог, если outputPath не задан
            if (string.IsNullOrEmpty(outputPath))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx;*.xlsm",
                    FileName = $"Нулевые позиции {DateTime.Now:yyyy-MM-dd HH-mm}.xlsx", // Предлагаемое имя файла
                    DefaultExt = ".xlsx",
                    AddExtension = true
                };

                if (saveFileDialog.ShowDialog() == true) // true, если пользователь нажал "Сохранить"
                {
                    outputPath = saveFileDialog.FileName;
                }
                else
                {
                    // Пользователь отменил сохранение
                    MessageBox.Show("Сохранение отчёта отменено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return null; // Возвращаем null, если не сохранили
                }
            }

            // 4. Сохраняем файл
            workbook.SaveAs(outputPath);

            return outputPath; // Возвращаем путь к сохранённому файлу
        }
    }
}
