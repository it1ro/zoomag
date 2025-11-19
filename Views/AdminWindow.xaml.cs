// File: Zoomag/Views/AdminWindow.xaml.cs

namespace Zoomag.Views;

using System.Windows;
using Data;
using Models;
using Reports;

/// <summary>
///     Interaction logic for AdminWindow.xaml
/// </summary>
public partial class AdminWindow : Window, IDisposable
{
    private readonly AppDbContext _context;

    public AdminWindow()
    {
        InitializeComponent();
        _context = new AppDbContext();
        LoadUnits();
        LoadCategories();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    private void LoadUnits()
    {
        UnitSelector.ItemsSource = _context.Unit.ToList();
        UnitSelector.SelectedIndex = -1; // Очищаем выбор
    }

    private void LoadCategories()
    {
        CategorySelector.ItemsSource = _context.Category.ToList();
        CategorySelector.SelectedIndex = -1; // Очищаем выбор
    }

    private void AddCategory(object sender, RoutedEventArgs e)
    {
        var categoryName = CategoryInput.Text;
        if (string.IsNullOrWhiteSpace(categoryName)) return;

        var category = new Category { Name = categoryName };
        _context.Category.Add(category);
        _context.SaveChanges();
        LoadCategories(); // Обновляем список
        CategoryInput.Clear();
    }

    private void AddUnit(object sender, RoutedEventArgs e)
    {
        var unitName = UnitInput.Text;
        if (string.IsNullOrWhiteSpace(unitName)) return;

        var unit = new Unit { Name = unitName };
        _context.Unit.Add(unit);
        _context.SaveChanges();
        LoadUnits(); // Обновляем список
        UnitInput.Clear();
    }

    private void SaveProduct(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProductNameInput.Text) ||
            string.IsNullOrWhiteSpace(PriceInput.Text) ||
            string.IsNullOrWhiteSpace(QuantityInput.Text))
            return;

        if (UnitSelector.SelectedIndex < 0 || CategorySelector.SelectedIndex < 0)
        {
            MessageBox.Show("Пожалуйста, выберите единицу измерения и категорию.", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var unitId = UnitSelector.SelectedIndex + 1; // Индекс + 1, если Id начинаются с 1
        var categoryId = CategorySelector.SelectedIndex + 1;

        var unit = _context.Unit.Find(unitId);
        var category = _context.Category.Find(categoryId);

        if (unit == null || category == null)
        {
            MessageBox.Show("Единица измерения или категория не найдены.", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        var product = new Product
        {
            Name = ProductNameInput.Text,
            Unit = unit,
            Category = category,
            Price = Convert.ToInt32(PriceInput.Text),
            Amount = Convert.ToInt32(QuantityInput.Text)
        };

        _context.Product.Add(product);
        _context.SaveChanges();
        ClearProductForm();
    }

    private void ClearProductForm()
    {
        ProductNameInput.Clear();
        QuantityInput.Clear();
        PriceInput.Clear();
        UnitSelector.SelectedIndex = -1;
        CategorySelector.SelectedIndex = -1;
    }

    private void ShowArrival(object sender, RoutedEventArgs e)
    {
        var arrivalWindow = new ProductEditor();
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

    protected override void OnClosed(EventArgs e)
    {
        _context?.Dispose();
        base.OnClosed(e);
    }
}
