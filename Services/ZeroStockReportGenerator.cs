namespace Zoomag.Services;

using System.Windows;
using ClosedXML.Excel;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

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
            .Include(t => t.Unit) // Подгружаем единицы измерения
            .Where(t => t.Amount == 0)
            .Select(t => new
            {
                t.Name,
                Category = t.Category != null ? t.Category.Name : "Без категории",
                Unit = t.Unit != null ? t.Unit.Name : "Без ед.изм.",
                t.Price
            })
            .OrderBy(x => x.Name)
            .ToList();

        var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Нулевые позиции");

        ws.Cell(1, 1).Value = "Отчёт по нулевым позициям";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 16;
        ws.Cell(1, 3).Value = $"Сгенерировано: {DateTime.Now:dd.MM.yyyy HH:mm}";
        ws.Row(1).Height = 30;

        ws.Cell(3, 1).Value = "Наименование";
        ws.Cell(3, 2).Value = "Категория";
        ws.Cell(3, 3).Value = "Ед. изм.";
        ws.Cell(3, 4).Value = "Цена (₽)";
        ws.Row(3).Style.Font.Bold = true;
        ws.Row(3).Height = 25;

        var row = 4;
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

        ws.Columns().AdjustToContents();

        if (string.IsNullOrEmpty(outputPath))
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xlsm",
                FileName = $"Нулевые позиции {DateTime.Now:yyyy-MM-dd HH-mm}.xlsx",
                DefaultExt = ".xlsx",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                outputPath = saveFileDialog.FileName;
            }
            else
            {
                MessageBox.Show("Сохранение отчёта отменено.", "Информация", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return null;
            }
        }

        workbook.SaveAs(outputPath);

        return outputPath;
    }
}
