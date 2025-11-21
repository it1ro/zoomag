namespace Zoomag.Views;

using System.Collections.ObjectModel;
using System.Windows;
using ClosedXML.Excel;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Models;
// Добавлено
// Добавлено для кнопок, если нужно

public partial class ArrivalWindow : Window
{
    // --- КОНЕЦ НОВОГО СВОЙСТВА ---

    public ArrivalWindow()
    {
        InitializeComponent();
        LoadReferenceData(); // Загружаем справочники
        // --- УСТАНОВКА DataContext ---
        DataContext = this; // Привязка свойств ImportedItems, UnitsList, CategoriesList к окну
        // --- КОНЕЦ УСТАНОВКИ DataContext ---
    }

    // --- НОВЫЕ СВОЙСТВА ДЛЯ ComboBox ---
    public ObservableCollection<Unit> UnitsList { get; set; } = new();

    public ObservableCollection<Category> CategoriesList { get; set; } = new();
    // --- КОНЕЦ НОВЫХ СВОЙСТВ ---

    // --- НОВОЕ СВОЙСТВО ---
    public ObservableCollection<SupplyImportViewModel> ImportedItems { get; set; } = new();

    // --- НОВЫЙ МЕТОД ЗАГРУЗКИ СПРАВОЧНИКОВ ---
    private void LoadReferenceData()
    {
        using var context = new AppDbContext();
        UnitsList.Clear();
        CategoriesList.Clear();

        foreach (var unit in context.Unit.ToList()) UnitsList.Add(unit);
        foreach (var category in context.Category.ToList()) CategoriesList.Add(category);
    }
    // --- КОНЕЦ МЕТОДА ---

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

    for (var row = 2; row <= lastRow; row++)
    {
        var name = ReadCell(worksheet, row, 1);
        var unitIdStr = ReadCell(worksheet, row, 2); // Это ID единицы измерения
        var categoryName = ReadCell(worksheet, row, 5); // Это название категории (может быть пустым)
        var quantityStr = ReadCell(worksheet, row, 3);
        var priceStr = ReadCell(worksheet, row, 4);

        if (!int.TryParse(quantityStr, out var quantity) || !int.TryParse(priceStr, out var price))
        {
            MessageBox.Show($"Неверный формат данных в строке {row}.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            continue;
        }

        // Попытка преобразовать ID единицы измерения из строки
        if (!int.TryParse(unitIdStr, out var unitId))
        {
            MessageBox.Show($"Неверный формат ID единицы измерения в строке {row}.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            continue;
        }

        // Найти объект Unit по ID
        var unit = UnitsList.FirstOrDefault(u => u.Id == unitId);
        if (unit == null)
        {
            MessageBox.Show($"Единица измерения с ID '{unitId}' не найдена в справочнике.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            unit = UnitsList.FirstOrDefault(); // fallback, если не найдена
            if (unit == null) continue; // Если и fallback не сработал, пропускаем строку
        }

        // Найти объект Category по имени, если имя не пустое
        Category category = null;
        if (!string.IsNullOrWhiteSpace(categoryName)) // Проверяем, что имя категории не пустое
        {
            category = CategoriesList.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            // Убираем сообщение об ошибке, если категория не найдена
            // if (category == null)
            // {
            //      MessageBox.Show($"Категория '{categoryName}' не найдена в справочнике.", "Ошибка",
            //         MessageBoxButton.OK, MessageBoxImage.Warning);
            //     category = CategoriesList.FirstOrDefault(); // fallback
            // }
        }
        // Если categoryName пустое или категория не найдена, category останется null

        var supplyItem = new SupplyImportViewModel
        {
            Date = DateTime.Today,
            Name = name,
            Unit = unit, // Присваиваем найденный объект Unit
            Category = category, // Присваиваем найденный объект Category или null
            Quantity = quantity,
            Price = price
        };
        ImportedItems.Add(supplyItem);
    }
}
    private string ReadCell(IXLWorksheet worksheet, int row, int column)
    {
        return worksheet.Cell(row, column).IsEmpty() ? string.Empty : worksheet.Cell(row, column).GetString();
    }

    private async void SaveToDatabase(object sender, RoutedEventArgs e)
{
    // --- ПРОВЕРКА КОЛЛЕКЦИИ ---
    if (ImportedItems.Count == 0)
    {
        MessageBox.Show("Нет данных для сохранения.", "Информация",
            MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }
    // --- КОНЕЦ ПРОВЕРКИ ---

    using var context = new AppDbContext();
    // 1. Получаем стратегию выполнения от контекста
    var strategy = context.Database.CreateExecutionStrategy();

    try
    {
        // 2. Выполняем всю операцию (включая транзакцию) через стратегию
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = context.Database.BeginTransaction(); // Теперь BeginTransaction вызывается внутри стратегии

            try
            {
                var suppliesToSave = new List<Supply>();
                var productsToSave = new List<Product>();
                var supplyItemsToSave = new List<SupplyItem>();

                // --- ИТЕРАЦИЯ ПО КОЛЛЕКЦИИ ---
                foreach (var item in ImportedItems)
                {
                    // --- КОНЕЦ ИТЕРАЦИИ ---
                    // Создаём поставку (пока без Id, он появится после SaveChanges)
                    var supply = new Supply
                    {
                        Date = item.Date,
                        Name = $"Поставка {item.Name} от {item.Date:dd.MM.yyyy}"
                    };
                    suppliesToSave.Add(supply);

                    // Ищем существующий товар *внутри транзакции*
                    var product = await context.Product
                        .Include(p => p.Unit) // Убедимся, что Unit загружен
                        .Include(p => p.Category) // Убедимся, что Category загружен
                        .FirstOrDefaultAsync(x => x.Name == item.Name); // Используем Async метод

                    if (product != null)
                    {
                        // Обновляем существующий товар
                        product.Amount += item.Quantity;
                        if (product.Price < item.Price)
                            product.Price = item.Price;
                        // Объект product уже отслеживается контекстом, изменения будут сохранены
                    }
                    else
                    {
                        // --- ИСПРАВЛЕНО: Используем объекты из ViewModel (item.Unit, item.Category) ---
                        // Убедимся, что объекты Unit и Category отслеживаются текущим контекстом
                        // или получим их из базы по ID, если они были загружены в окне
                        var unit = context.Unit.Local.FirstOrDefault(u => u.Id == item.Unit.Id) ?? await context.Unit.FindAsync(item.Unit.Id);
                        var category = context.Category.Local.FirstOrDefault(c => c.Id == item.Category.Id) ?? await context.Category.FindAsync(item.Category.Id);

                        if (unit == null || category == null)
                        {
                            MessageBox.Show("Ошибка сопоставления единицы измерения или категории при сохранении.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            // Вызов Rollback выбросит исключение, которое перехватит стратегия
                            await transaction.RollbackAsync();
                            // Бросаем исключение для остановки выполнения внутри стратегии
                            throw new InvalidOperationException("Не удалось найти Unit или Category для сохранения.");
                        }

                        var newProduct = new Product
                        {
                            Name = item.Name,
                            Price = item.Price,
                            Amount = item.Quantity,
                            // --- ИСПРАВЛЕНО: Использование найденных объектов из БД ---
                            Unit = unit,
                            Category = category
                            // --- КОНЕЦ ИСПРАВЛЕНИЯ ---
                        };
                        productsToSave.Add(newProduct);
                        product = newProduct; // Обновляем ссылку
                    }

                    // Создаём элемент поставки (ссылки на supply и product пока не установлены)
                    var supplyItem = new SupplyItem
                    {
                        // SupplyId будет установлен после сохранения Supply
                        // ProductId будет установлен после сохранения Product
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Total = item.Quantity * item.Price // Устанавливаем Total
                    };
                    // Связываем с объектами в памяти, EF установит Id при SaveChanges
                    supplyItem.Supply = supply;
                    supplyItem.Product = product;
                    supplyItemsToSave.Add(supplyItem);
                }

                // Добавляем все сущности в контекст
                if (suppliesToSave.Any()) context.Supply.AddRange(suppliesToSave);
                if (productsToSave.Any()) context.Product.AddRange(productsToSave);
                if (supplyItemsToSave.Any()) context.SupplyItem.AddRange(supplyItemsToSave);

                // Сохраняем все изменения в транзакции
                await context.SaveChangesAsync(); // Используем Async
                await transaction.CommitAsync(); // Используем Async

                // --- ОЧИСТКА КОЛЛЕКЦИИ ПОСЛЕ УСПЕШНОГО СОХРАНЕНИЯ ---
                ImportedItems.Clear();
                // --- КОНЕЦ ОЧИСТКИ ---

                MessageBox.Show("Данные успешно сохранены в базу.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Откатываем транзакцию в случае ошибки
                // Бросаем исключение дальше, чтобы стратегия повтора могла его обработать
                // или чтобы оно дошло до основного catch в методе, если стратегия не будет повторять
                throw new InvalidOperationException($"Ошибка при сохранении в базу: {ex.Message}", ex);
            }
        });
    }
    catch (Exception ex) // Этот catch перехватывает исключение, если стратегия исчерпала попытки повтора
    {
        MessageBox.Show($"Ошибка при выполнении операции: {ex.Message}", "Ошибка",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

    // --- ОБРАБОТКА КНОПКИ НАЗАД ---
    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        var adminWindow = new AdminWindow();
        Hide();
        adminWindow.Show();
    }
    // --- КОНЕЦ ОБРАБОТКИ ---

    public class SupplyImportViewModel
    {
        public DateTime Date { get; set; }

        public string Name { get; set; } = string.Empty;

        // --- ИЗМЕНЕНО: Unit и Category теперь объекты, а не строки ---
        public Unit Unit { get; set; } // Инициализируем null

        public Category Category { get; set; } // Инициализируем null

        // --- КОНЕЦ ИЗМЕНЕНИЯ ---
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
