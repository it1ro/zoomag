using System.Windows;
using Zoomag.Data;
using Zoomag.Models;

namespace Zoomag.Views;

public partial class ProductEditDialog : Window
{
    public Product Result { get; private set; }
    private readonly bool _isEditMode;
    private readonly AppDbContext _context;

    public ProductEditDialog()
    {
        InitializeComponent();
        _isEditMode = false;
        _context = new AppDbContext();
        LoadReferences();
    }

    public ProductEditDialog(Product productToEdit)
    {
        InitializeComponent();
        _isEditMode = true;
        _context = new AppDbContext();
        LoadReferences();

        NameInput.Text = productToEdit.Name;
        CategorySelector.SelectedValue = productToEdit.CategoryId;
        UnitSelector.SelectedValue = productToEdit.UnitId;
        Result = productToEdit;
    }

    private void LoadReferences()
    {
        CategorySelector.ItemsSource = _context.Category.ToList();
        UnitSelector.ItemsSource = _context.Unit.ToList();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameInput.Text))
        {
            MessageBox.Show("Укажите название товара.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CategorySelector.SelectedItem == null || UnitSelector.SelectedItem == null)
        {
            MessageBox.Show("Выберите категорию и единицу измерения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var categoryId = (int)CategorySelector.SelectedValue;
        var unitId = (int)UnitSelector.SelectedValue;

        if (_isEditMode)
        {
            Result.Name = NameInput.Text.Trim();
            Result.CategoryId = categoryId;
            Result.UnitId = unitId;
        }
        else
        {
            Result = new Product
            {
                Name = NameInput.Text.Trim(),
                CategoryId = categoryId,
                UnitId = unitId
                // ❌ НЕТ Price, ❌ НЕТ Amount
            };
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _context?.Dispose();
        base.OnClosed(e);
    }
}
