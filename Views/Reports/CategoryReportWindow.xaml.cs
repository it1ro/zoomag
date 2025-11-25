using System.Windows;
using ClosedXML.Excel;
using Zoomag.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Linq;

namespace Zoomag.Views.Reports;

using System.Windows.Controls;

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
        var categories = context.Category
            .OrderBy(c => c.Name)
            .Select(c => c.Name)
            .ToList();

        CategorySelector.Items.Clear();
        foreach (var name in categories)
            CategorySelector.Items.Add(name);
    }

    private void OnCategoryChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategorySelector.SelectedItem == null) return;

        var categoryName = CategorySelector.SelectedItem.ToString();

        using var context = new AppDbContext();
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
                    .Select(si => si.Price)
                    .FirstOrDefault(),
                Stock = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
            })
            .OrderBy(x => x.Name)
            .ToList();

        CategoryProductsGrid.ItemsSource = products;
    }

    private void ExportToExcel(object sender, RoutedEventArgs e)
    {
        if (CategorySelector.SelectedItem == null)
        {
            MessageBox.Show("Пожалуйста, выберите категорию.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();
        var categoryName = CategorySelector.SelectedItem.ToString();

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
                    .Select(si => si.Price)
                    .FirstOrDefault(),
                Stock = p.SupplyItems.Sum(si => si.Quantity) - p.SaleItems.Sum(si => si.Quantity)
            })
            .OrderBy(x => x.Name)
            .ToList();

        if (!products.Any())
        {
            MessageBox.Show("В выбранной категории нет товаров.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Excel Files (.xlsx)|*.xlsx|All Files (*.*)|*.*",
            FileName = $"Отчет по категории {categoryName} {DateTime.Now:yyyy-MM-dd}.xlsx",
            DefaultExt = ".xlsx",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (saveFileDialog.ShowDialog() != true) return;

        var fileName = saveFileDialog.FileName;
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            fileName += ".xlsx";

        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Отчет по категории");

            worksheet.Cell(1, 1).Value = "Отчет по категории:";
            worksheet.Cell(1, 2).Value = categoryName;
            worksheet.Cell(2, 1).Value = "На дату:";
            worksheet.Cell(2, 2).Value = DateTime.Today.ToString("dd MMMM yyyy",
                System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU"));

            worksheet.Cell(4, 1).Value = "Наименование";
            worksheet.Cell(4, 2).Value = "Категория";
            worksheet.Cell(4, 3).Value = "Ед. изм.";
            worksheet.Cell(4, 4).Value = "Цена";
            worksheet.Cell(4, 5).Value = "Остаток";
            worksheet.Range(4, 1, 4, 5).Style.Font.Bold = true;

            var row = 5;
            foreach (var p in products)
            {
                worksheet.Cell(row, 1).Value = p.Name;
                worksheet.Cell(row, 2).Value = p.CategoryName;
                worksheet.Cell(row, 3).Value = p.UnitName;
                worksheet.Cell(row, 4).Value = p.Price;
                worksheet.Cell(row, 5).Value = p.Stock;
                row++;
            }

            worksheet.Cell(row + 1, 1).Value = $"Всего: {products.Count} товаров";
            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(fileName);

            MessageBox.Show($"Отчёт сохранён:\n{fileName}", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении Excel: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GoToAdmin(object sender, RoutedEventArgs e)
    {
        var adminWindow = new AdminWindow();
        Hide();
        adminWindow.Show();
    }
}
