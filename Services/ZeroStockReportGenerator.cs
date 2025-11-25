namespace Zoomag.Services;

using System.Windows;
using ClosedXML.Excel;
using Zoomag.Data;
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
        // Вычисляем остаток на лету: поставки - продажи
        var zeroStockItems = _context.Product
            .Select(p => new
            {
                p.Id,
                p.Name,
                CategoryName = p.Category != null ? p.Category.Name : "Без категории",
                UnitName = p.Unit != null ? p.Unit.Name : "Без ед.изм.",
                // Цена из последней поставки
                Price = p.SupplyItems
                    .OrderByDescending(si => si.Supply.Date)
                    .FirstOrDefault() != null
                    ? p.SupplyItems.OrderByDescending(si => si.Supply.Date).FirstOrDefault().Price
                    : 0,
                // Остаток: поставки - продажи
                Stock = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
            })
            .Where(x => x.Stock == 0)
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
            ws.Cell(row, 2).Value = item.CategoryName;
            ws.Cell(row, 3).Value = item.UnitName;
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
