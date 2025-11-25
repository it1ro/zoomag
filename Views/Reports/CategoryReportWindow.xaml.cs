// Добавлено для SaveFileDialog
namespace Zoomag.Views.Reports;

using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Zoomag.Data;
using Zoomag.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

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
        foreach (var category in context.Category.OrderBy(c => c.Name).ToList())
            CategorySelector.Items.Add(category.Name);
    }

    private void ExportToExcel(object sender, RoutedEventArgs e)
    {
        if (CategorySelector.SelectedItem == null)
        {
            MessageBox.Show("Пожалуйста, выберите категорию.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Excel Files (.xlsx)|*.xlsx|All Files (*.*)|*.*",
            FileName = $"Отчет по категории {CategorySelector.SelectedItem} {DateTime.Now:yyyy-MM-dd}.xlsx",
            DefaultExt = ".xlsx",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (saveFileDialog.ShowDialog() != true) return;

        var fileName = saveFileDialog.FileName;
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            fileName += ".xlsx";

        using var context = new AppDbContext();
        var selectedCategoryName = CategorySelector.SelectedItem.ToString();

        // 🔁 Вычисляем остаток и цену динамически
        var products = context.Product
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.SupplyItems).ThenInclude(si => si.Supply)
            .Include(p => p.SaleItems)
            .Where(p => p.Category.Name == selectedCategoryName)
            .Select(p => new
            {
                p.Name,
                CategoryName = p.Category.Name,
                UnitName = p.Unit.Name,
                Price = p.SupplyItems
                    .OrderByDescending(si => si.Supply.Date)
                    .FirstOrDefault() != null
                    ? p.SupplyItems.OrderByDescending(si => si.Supply.Date).FirstOrDefault().Price
                    : 0,
                Stock = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
            })
            .OrderBy(x => x.Name)
            .ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Отчет по категории");

        worksheet.Cell(1, 1).Value = "Отчет по категории на";
        worksheet.Cell(1, 3).Value = DateTime.Today.ToString("MMMM d, yyyy");

        worksheet.Cell(3, 1).Value = "Наименование";
        worksheet.Cell(3, 2).Value = "Количество";
        worksheet.Cell(3, 3).Value = "Цена";
        worksheet.Cell(3, 4).Value = "Категория";
        worksheet.Cell(3, 5).Value = "Ед/изм";

        var row = 4;
        foreach (var p in products)
        {
            worksheet.Cell(row, 1).Value = p.Name;
            worksheet.Cell(row, 2).Value = p.Stock;
            worksheet.Cell(row, 3).Value = p.Price;
            worksheet.Cell(row, 4).Value = p.CategoryName;
            worksheet.Cell(row, 5).Value = p.UnitName;
            row++;
        }

        worksheet.Cell(row + 1, 1).Value = $"{products.Count} товаров";
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

    private void GoToAdmin(object sender, RoutedEventArgs e)
    {
        var adminWindow = new AdminWindow();
        Hide();
        adminWindow.Show();
    }

    private void OnCategoryChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategorySelector.SelectedItem == null) return;

        using var context = new AppDbContext();
        var categoryName = CategorySelector.SelectedItem.ToString();

        // 🔁 То же самое для отображения в DataGrid
        var products = context.Product
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.SupplyItems).ThenInclude(si => si.Supply)
            .Include(p => p.SaleItems)
            .Where(p => p.Category.Name == categoryName)
            .Select(p => new
            {
                p.Name,
                CategoryName = p.Category.Name,
                UnitName = p.Unit.Name,
                Price = p.SupplyItems
                    .OrderByDescending(si => si.Supply.Date)
                    .FirstOrDefault() != null
                    ? p.SupplyItems.OrderByDescending(si => si.Supply.Date).FirstOrDefault().Price
                    : 0,
                Stock = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
            })
            .ToList();

        CategoryProductsGrid.ItemsSource = products;
    }
}
