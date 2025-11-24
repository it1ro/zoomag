using System.Windows;
using Zoomag.Data;
using Zoomag.Models;

namespace Zoomag.Views;

public partial class CategoriesEditorWindow : Window
{
    private readonly AppDbContext _context = new();

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

    private void NewCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CategoryEditDialog { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            var newName = dialog.Result.Name.Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Название категории не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка дубликата (регистронезависимо — за счёт COLLATION БД)
            bool isDuplicate = _context.Category.Any(c => c.Name == newName);
            if (isDuplicate)
            {
                MessageBox.Show("Категория с таким названием уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _context.Category.Add(new Category { Name = newName });
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Категория добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void EditCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (CategoriesGrid.SelectedItem is not Category selected)
        {
            MessageBox.Show("Выберите категорию для редактирования.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var categoryCopy = new Category { Id = selected.Id, Name = selected.Name };
        var dialog = new CategoryEditDialog(categoryCopy) { Owner = this };

        if (dialog.ShowDialog() == true)
        {
            var newName = dialog.Result.Name.Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Название категории не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка дубликата, кроме текущей категории
            bool isDuplicate = _context.Category.Any(c => c.Id != selected.Id && c.Name == newName);
            if (isDuplicate)
            {
                MessageBox.Show("Категория с таким названием уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            selected.Name = newName;
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Категория обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
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
