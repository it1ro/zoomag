// File: Zoomag/Views/AdminWindow.xaml.cs

namespace Zoomag.Views;

using System.Windows;
using System.Windows.Controls; // Добавлено для ComboBox
using Data;
using Models;
using Reports;

/// <summary>
/// Interaction logic for AdminWindow.xaml
/// </summary>
public partial class AdminWindow : Window, IDisposable
{
    // Удаляем поле _context из класса.

    public AdminWindow()
    {
        InitializeComponent();
        LoadUnits();
        LoadCategories();
    }

    public void Dispose()
    {
        // Не нужно больше уничтожать _context здесь, так как он больше не поле класса.
        // Однако, если у окна есть другие ресурсы, их можно освободить.
        GC.SuppressFinalize(this);
    }

    private void LoadUnits()
    {
        // Создаём контекст только для этой операции
        using var context = new AppDbContext();
        // Привязываем список объектов Unit к ComboBox
        UnitSelector.ItemsSource = context.Unit.ToList();
        UnitSelector.SelectedIndex = -1; // Очищаем выбор
    }

    private void LoadCategories()
    {
        // Создаём контекст только для этой операции
        using var context = new AppDbContext();
        // Привязываем список объектов Category к ComboBox
        CategorySelector.ItemsSource = context.Category.ToList();
        CategorySelector.SelectedIndex = -1; // Очищаем выбор
    }

    private void AddCategory(object sender, RoutedEventArgs e)
    {
        var categoryName = CategoryInput.Text; // Получаем исходный ввод
        // --- ИЗМЕНЕНО: Добавлено Trim() ---
        var trimmedCategoryName = categoryName?.Trim(); // Убираем пробелы в начале и конце

        // --- ИЗМЕНЕНО: Проверяем trimmedCategoryName ---
        if (string.IsNullOrWhiteSpace(trimmedCategoryName))
        {
            MessageBox.Show("Название категории не может быть пустым или содержать только пробелы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            CategoryInput.Clear(); // Очищаем поле ввода
            return;
        }

        using var context = new AppDbContext();
        // --- ИЗМЕНЕНО: Используем trimmedCategoryName для проверки дубликата ---
        if (context.Category.Any(c => c.Name == trimmedCategoryName))
        {
            MessageBox.Show($"Категория с названием '{trimmedCategoryName}' уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            CategoryInput.Clear(); // Очищаем поле ввода
            return;
        }

        try
        {
            // --- ИЗМЕНЕНО: Используем trimmedCategoryName для создания объекта ---
            var category = new Category { Name = trimmedCategoryName };
            context.Category.Add(category);
            context.SaveChanges();
            LoadCategories(); // Обновляем список
            CategoryInput.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при добавлении категории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddUnit(object sender, RoutedEventArgs e)
    {
        var unitName = UnitInput.Text; // Получаем исходный ввод
        // --- ИЗМЕНЕНО: Добавлено Trim() ---
        var trimmedUnitName = unitName?.Trim(); // Убираем пробелы в начале и конце

        // --- ИЗМЕНЕНО: Проверяем trimmedUnitName ---
        if (string.IsNullOrWhiteSpace(trimmedUnitName))
        {
            MessageBox.Show("Название единицы измерения не может быть пустым или содержать только пробелы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            UnitInput.Clear(); // Очищаем поле ввода
            return;
        }

        using var context = new AppDbContext();
        // --- ИЗМЕНЕНО: Используем trimmedUnitName для проверки дубликата ---
        if (context.Unit.Any(u => u.Name == trimmedUnitName))
        {
            MessageBox.Show($"Единица измерения с названием '{trimmedUnitName}' уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            UnitInput.Clear(); // Очищаем поле ввода
            return;
        }

        try
        {
            // --- ИЗМЕНЕНО: Используем trimmedUnitName для создания объекта ---
            var unit = new Unit { Name = trimmedUnitName };
            context.Unit.Add(unit);
            context.SaveChanges();
            LoadUnits(); // Обновляем список
            UnitInput.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при добавлении единицы измерения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // File: Zoomag/Views/AdminWindow.xaml.cs

private void SaveProduct(object sender, RoutedEventArgs e)
{
    // Проверяем, что введены имя и цена
    if (string.IsNullOrWhiteSpace(ProductNameInput.Text) ||
        string.IsNullOrWhiteSpace(PriceInput.Text)) // Убедимся, что цена не пустая
    {
        MessageBox.Show("Пожалуйста, заполните все обязательные поля (Имя, Стоимость).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    // Проверяем, что выбраны единица измерения и категория
    if (UnitSelector.SelectedItem == null || CategorySelector.SelectedItem == null)
    {
        MessageBox.Show("Пожалуйста, выберите единицу измерения и категорию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    // Валидация цены
    if (!int.TryParse(PriceInput.Text, out int price) || price < 0)
    {
        MessageBox.Show("Введите корректную цену (неотрицательное число).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }

    // --- ИЗМЕНЕНО: Получаем ID выбранных объектов ---
    var selectedUnitId = ((Unit)UnitSelector.SelectedItem).Id; // Получаем ID
    var selectedCategoryId = ((Category)CategorySelector.SelectedItem).Id; // Получаем ID

    using var context = new AppDbContext();
    try
    {
        // --- ИЗМЕНЕНО: Загружаем Unit и Category по ID в текущем контексте ---
        // Это гарантирует, что EF Core знает об этих сущностях и может установить связь
        var selectedUnit = context.Unit.Find(selectedUnitId);
        var selectedCategory = context.Category.Find(selectedCategoryId);

        if (selectedUnit == null || selectedCategory == null)
        {
            MessageBox.Show("Выбранная единица измерения или категория не найдены в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Создаём новый товар
        var product = new Product
        {
            Name = ProductNameInput.Text,
            // --- ИЗМЕНЕНО: Устанавливаем НАВИГАЦИОННЫЕ СВОЙСТВА ---
            Unit = selectedUnit, // Передаём объект, отслеживаемый текущим контекстом
            Category = selectedCategory, // Передаём объект, отслеживаемый текущим контекстом
            Price = price, // Устанавливаем валидированную цену
            Amount = 0 // Устанавливаем начальное количество 0
        };

        context.Product.Add(product);
        context.SaveChanges(); // Теперь EF Core корректно установит внешние ключи
        ClearProductForm();
        MessageBox.Show("Товар успешно сохранён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Ошибка при сохранении товара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
    private void ClearProductForm()
    {
        ProductNameInput.Clear();
        PriceInput.Clear();
        UnitSelector.SelectedIndex = -1; // Это теперь правильно очищает ComboBox
        CategorySelector.SelectedIndex = -1; // Это теперь правильно очищает ComboBox
    }

    private void ShowArrival(object sender, RoutedEventArgs e)
    {
        var arrivalWindow = new SupplyWindow();
        Hide();
        arrivalWindow.Show();
    }

    private void ShowSale(object sender, RoutedEventArgs e)
    {
        var saleWindow = new SaleWindow();
        Hide();
        saleWindow.Show();
    }

    private void ShowStock(object sender, RoutedEventArgs e)
    {
        var stockWindow = new ProductOverviewWindow();
        Hide();
        stockWindow.Show();
    }

    private void ShowReports(object sender, RoutedEventArgs e)
    {
        var reportsWindow = new AdminReportsWindow();
        Hide();
        reportsWindow.Show();
    }
}
