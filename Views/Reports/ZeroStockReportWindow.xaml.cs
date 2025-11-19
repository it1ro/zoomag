// Добавлено для SaveFileDialog

namespace Zoomag.Views.Reports;

using System.Windows;
using ClosedXML.Excel;
using Data;
using Microsoft.Win32;

/// <summary>
///     Логика взаимодействия для ZeroStockReportWindow.xaml
/// </summary>
public partial class ZeroStockReportWindow : Window
{
    private List<ProductViewModel> _zeroStockProducts;

    public ZeroStockReportWindow()
    {
        InitializeComponent();
        LoadZeroStockData();
    }

    private void LoadZeroStockData()
    {
        using var context = new AppDbContext();
        _zeroStockProducts = context.Product
            .Where(product => product.Amount == 0)
            .Select(product => new ProductViewModel
            {
                Name = product.Name,
                Price = product.Price
            })
            .ToList();

        ZeroStockDataGrid.ItemsSource = _zeroStockProducts;
    }

    private void ExportToExcel_Click(object sender, RoutedEventArgs e)
    {
        // Создаем диалог сохранения файла
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Excel файлы (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*",
            FileName = $"Товары с нулевым остатком на {DateTime.Today:yyyy-MM-dd}.xlsx", // Предлагаемое имя файла
            DefaultExt = ".xlsx",
            Title = "Сохранить отчет в Excel"
        };

        // Показываем диалог и проверяем, нажал ли пользователь "Сохранить"
        if (saveFileDialog.ShowDialog() == true)
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Товары с нулевым остатком");

                worksheet.Cell(1, 1).Value = "Товары с нулевым остатком";
                worksheet.Cell(1, 3).Value = DateTime.Today.ToString("MMMM dd yyyy");

                worksheet.Cell(3, 1).Value = "Наименование";
                worksheet.Cell(3, 3).Value = "Цена";

                var row = 4;
                foreach (var product in _zeroStockProducts)
                {
                    worksheet.Cell(row, 1).Value = product.Name;
                    worksheet.Cell(row, 3).Value = product.Price;
                    row++;
                }

                worksheet.Cell(_zeroStockProducts.Count + 2, 1).Value = $"{_zeroStockProducts.Count} товаров";

                // Сохраняем файл по выбранному пользователем пути
                workbook.SaveAs(saveFileDialog.FileName);
                MessageBox.Show($"Отчет сохранен: {saveFileDialog.FileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении отчета: {ex.Message}");
            }
        // Если пользователь нажал "Отмена", ничего не делаем
    }

    private void BackToReports_Click(object sender, RoutedEventArgs e)
    {
        var reportsWindow = new AdminReportsWindow();
        Hide();
        reportsWindow.Show();
    }

    // Вспомогательный класс для отображения данных в DataGrid
    public class ProductViewModel
    {
        public string Name { get; set; }
        public int Price { get; set; }
    }
}
