using System.Windows;
using Zoomag.Data;
using Zoomag.Models;

namespace Zoomag.Views;

public partial class CategoriesEditorWindow : Window
{
    private readonly AppDbContext _context = new();
    private Category _editingCategory;

    public CategoriesEditorWindow()
    {
        InitializeComponent();
        LoadCategories();
    }

    private void LoadCategories()
    {
        var categories = _context.Category.ToList();
        CategoriesGrid.ItemsSource = categories;
    }

    private void CategoriesGrid_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
    {
        if (CategoriesGrid.SelectedItem is not Category selected) return;

        _editingCategory = selected;
        NameInput.Text = selected.Name;
    }

    private void NewCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var name = NameInput.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Укажите название категории.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Проверка дубликата (игнорируем регистр)
        int editingId = _editingCategory?.Id ?? 0;

        bool isDuplicate = _context.Category.Any(c =>
            c.Id != editingId &&
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (isDuplicate)
        {
            MessageBox.Show("Категория с таким названием уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_editingCategory == null)
        {
            // Создание
            var category = new Category { Name = name };
            _context.Category.Add(category);
        }
        else
        {
            // Редактирование
            _editingCategory.Name = name;
        }

        try
        {
            _context.SaveChanges();
            MessageBox.Show("Категория сохранена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm();
            LoadCategories();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (CategoriesGrid.SelectedItem is not Category selected)
        {
            MessageBox.Show("Выберите категорию для удаления.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (MessageBox.Show($"Удалить категорию «{selected.Name}»?\nВсе связанные товары будут удалены.", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            _context.Category.Remove(selected);
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Категория удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ClearForm()
    {
        _editingCategory = null;
        NameInput.Clear();
        CategoriesGrid.UnselectAll();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        _context.Dispose();
        var admin = new AdminWindow();
        Hide();
        admin.Show();
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _context?.Dispose();
        base.OnClosed(e);
    }
}
