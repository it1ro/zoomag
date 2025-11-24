using System.Windows;
using System.Windows.Controls;
using Zoomag.Data;
using Zoomag.Models;

namespace Zoomag.Views;

using Microsoft.EntityFrameworkCore;

public partial class ProductEditorWindow : Window
{
    private AppDbContext _context;
    private Product _editingProduct;

    public ProductEditorWindow()
    {
        InitializeComponent();
        _context = new AppDbContext();
        LoadReferenceData();
        LoadProducts();
    }

    private void LoadReferenceData()
    {
        CategorySelector.ItemsSource = _context.Category.ToList();
        UnitSelector.ItemsSource = _context.Unit.ToList();
    }

    private void LoadProducts()
    {
        var products = _context.Product
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .ToList();
        ProductsGrid.ItemsSource = products;
    }

    private void ProductsGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
    {
        if (ProductsGrid.SelectedItem is not Product selected) return;

        _editingProduct = selected;
        NameInput.Text = selected.Name;
        PriceInput.Text = selected.Price.ToString();
        CategorySelector.SelectedValue = selected.CategoryId;
        UnitSelector.SelectedValue = selected.UnitId;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameInput.Text) ||
            !int.TryParse(PriceInput.Text, out var price) || price < 0)
        {
            MessageBox.Show("Проверьте корректность ввода: название и цена (целое ≥0).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CategorySelector.SelectedItem == null || UnitSelector.SelectedItem == null)
        {
            MessageBox.Show("Выберите категорию и единицу измерения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var category = (Category)CategorySelector.SelectedItem;
        var unit = (Unit)UnitSelector.SelectedItem;

        if (_editingProduct == null)
        {
            // Создаём новый товар
            var product = new Product
            {
                Name = NameInput.Text.Trim(),
                Price = price,
                CategoryId = category.Id,
                UnitId = unit.Id,
                Amount = 0 // можно позже обновить при поставке
            };
            _context.Product.Add(product);
        }
        else
        {
            // Редактируем существующий
            _editingProduct.Name = NameInput.Text.Trim();
            _editingProduct.Price = price;
            _editingProduct.CategoryId = category.Id;
            _editingProduct.UnitId = unit.Id;
        }

        try
        {
            _context.SaveChanges();
            MessageBox.Show("Товар сохранён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm();
            LoadProducts(); // обновляем таблицу
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProductsGrid.SelectedItem is not Product selected)
        {
            MessageBox.Show("Выберите товар для удаления.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (MessageBox.Show($"Удалить товар «{selected.Name}»?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            _context.Product.Remove(selected);
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Товар удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ClearForm()
    {
        _editingProduct = null;
        NameInput.Clear();
        PriceInput.Clear();
        CategorySelector.SelectedIndex = -1;
        UnitSelector.SelectedIndex = -1;
        ProductsGrid.UnselectAll();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        _context.Dispose();
        var admin = new AdminWindow();
        Hide();
        admin.Show();
    }

    protected override void OnClosed(EventArgs e)
    {
        _context?.Dispose();
        base.OnClosed(e);
    }
}
