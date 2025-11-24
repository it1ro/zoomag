using System.Collections.ObjectModel;
using System.Windows;
using ClosedXML.Excel;
using Zoomag.Data;
using Zoomag.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace Zoomag.Views;

public partial class ArrivalWindow : Window
{
    public ObservableCollection<Category> AllCategories { get; set; } = new();
    public ObservableCollection<Unit> AllUnits { get; set; } = new();
    public ObservableCollection<SupplyImportViewModel> ImportedItems { get; set; } = new();

    public ArrivalWindow()
    {
        InitializeComponent();
        LoadReferenceData();
        DataContext = this;
    }

    private void LoadReferenceData()
    {
        using var context = new AppDbContext();
        AllCategories.Clear();
        AllUnits.Clear();

        foreach (var cat in context.Category.OrderBy(c => c.Name)) AllCategories.Add(cat);
        foreach (var unit in context.Unit.OrderBy(u => u.Name)) AllUnits.Add(unit);
    }

    private void ImportFromExcel(object sender, RoutedEventArgs e)
    {
        var openDialog = new OpenFileDialog
        {
            Filter = "Файлы Excel|*.xlsx;*.xls;*.xlsm|Все файлы|*.*"
        };
        if (openDialog.ShowDialog() != true) return;

        using var workbook = new XLWorkbook(openDialog.FileName);
        var worksheet = workbook.Worksheets.Worksheet(1);
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        if (lastRow < 2)
        {
            MessageBox.Show("Файл не содержит данных для импорта.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        ImportedItems.Clear();
        var deliveryDate = DeliveryDatePicker.SelectedDate ?? DateTime.Today;

        for (var row = 2; row <= lastRow; row++)
        {
            var categoryName = ReadCell(worksheet, row, 1);
            var name = ReadCell(worksheet, row, 2);
            var unitName = ReadCell(worksheet, row, 3);
            var quantityStr = ReadCell(worksheet, row, 4);
            var priceStr = ReadCell(worksheet, row, 5);

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(unitName) ||
                string.IsNullOrWhiteSpace(categoryName))
            {
                MessageBox.Show($"Пропущены обязательные поля в строке {row}.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                continue;
            }

            if (!int.TryParse(quantityStr, out var quantity) || quantity <= 0 ||
                !int.TryParse(priceStr, out var price) || price <= 0)
            {
                MessageBox.Show($"Неверный формат количества или цены в строке {row}.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                continue;
            }

            var category = AllCategories.FirstOrDefault(c => c.Name == categoryName);
            var unit = AllUnits.FirstOrDefault(u => u.Name == unitName);

            if (category == null || unit == null)
            {
                MessageBox.Show($"В строке {row} не найдены категория '{categoryName}' или ед.изм. '{unitName}'.\nУбедитесь, что справочники заполнены.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                continue;
            }

            ImportedItems.Add(new SupplyImportViewModel
            {
                Date = deliveryDate,
                Name = name.Trim(),
                CategoryId = category.Id,
                UnitId = unit.Id,
                Quantity = quantity,
                Price = price
            });
        }
    }

    private string ReadCell(IXLWorksheet worksheet, int row, int column)
    {
        return worksheet.Cell(row, column).IsEmpty() ? string.Empty : worksheet.Cell(row, column).GetString();
    }

    private async void SaveToDatabase(object sender, RoutedEventArgs e)
    {
        if (ImportedItems.Count == 0)
        {
            MessageBox.Show("Нет данных для сохранения.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // 🔒 Валидация: отсекаем строки с нулевым или отрицательным количеством/ценой
        var invalidItems = ImportedItems.Where(i => i.Quantity <= 0 || i.Price <= 0).ToList();
        if (invalidItems.Any())
        {
            var names = string.Join(", ", invalidItems.Take(5).Select(i => i.Name));
            MessageBox.Show($"Невозможно сохранить записи с нулевой или отрицательной ценой/количеством.\nПримеры: {names}", "Ошибка валидации",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var context = new AppDbContext();
        var strategy = context.Database.CreateExecutionStrategy();

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = context.Database.BeginTransaction();

                try
                {
                    var deliveryDate = DeliveryDatePicker.SelectedDate ?? DateTime.Today;

                    foreach (var item in ImportedItems)
                    {
                        var category = await context.Category.FindAsync(item.CategoryId);
                        var unit = await context.Unit.FindAsync(item.UnitId);
                        if (category == null || unit == null)
                        {
                            throw new InvalidOperationException("Категория или единица измерения не найдены при сохранении.");
                        }

                        var supply = new Supply
                        {
                            Date = deliveryDate,
                            Name = $"Поставка от {deliveryDate:dd.MM.yyyy}"
                        };

                        var product = await context.Product
                            .Include(p => p.Category)
                            .Include(p => p.Unit)
                            .FirstOrDefaultAsync(p => p.Name == item.Name);

                        if (product == null)
                        {
                            product = new Product
                            {
                                Name = item.Name,
                                Price = item.Price,
                                Amount = item.Quantity,
                                CategoryId = item.CategoryId,
                                UnitId = item.UnitId
                            };
                            context.Product.Add(product);
                        }
                        else
                        {
                            if (product.Price < item.Price) product.Price = item.Price;
                            product.Amount += item.Quantity;
                        }

                        var supplyItem = new SupplyItem
                        {
                            Supply = supply,
                            Product = product,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            Total = item.Quantity * item.Price
                        };

                        context.Supply.Add(supply);
                        context.SupplyItem.Add(supplyItem);
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    ImportedItems.Clear();
                    MessageBox.Show("Данные успешно сохранены в базу.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"Ошибка при сохранении: {ex.Message}", ex);
                }
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при выполнении операции: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var admin = new AdminWindow();
        Hide();
        admin.Show();
    }

    public class SupplyImportViewModel
    {
        public DateTime Date { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int UnitId { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
