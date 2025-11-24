using System.Windows;
using Zoomag.Data;
using Zoomag.Models;

namespace Zoomag.Views;

public partial class UnitsEditorWindow : Window
{
    private readonly AppDbContext _context = new();

    public UnitsEditorWindow()
    {
        InitializeComponent();
        LoadUnits();
    }

    private void LoadUnits()
    {
        var units = _context.Unit.ToList();
        UnitsGrid.ItemsSource = units;
    }

    private void NewUnitButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new UnitEditDialog { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            var newName = dialog.Result.Name.Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Название не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_context.Unit.Any(u => u.Name == newName))
            {
                MessageBox.Show("Единица измерения с таким названием уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _context.Unit.Add(new Unit { Name = newName });
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Единица измерения добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUnits();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void EditUnitButton_Click(object sender, RoutedEventArgs e)
    {
        if (UnitsGrid.SelectedItem is not Unit selected)
        {
            MessageBox.Show("Выберите единицу измерения для редактирования.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var unitCopy = new Unit { Id = selected.Id, Name = selected.Name };
        var dialog = new UnitEditDialog(unitCopy) { Owner = this };

        if (dialog.ShowDialog() == true)
        {
            var newName = dialog.Result.Name.Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Название не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_context.Unit.Any(u => u.Id != selected.Id && u.Name == newName))
            {
                MessageBox.Show("Единица измерения с таким названием уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            selected.Name = newName;
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Единица измерения обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUnits();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (UnitsGrid.SelectedItem is not Unit selected)
        {
            MessageBox.Show("Выберите единицу измерения для удаления.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (MessageBox.Show($"Удалить единицу измерения «{selected.Name}»?\nВсе связанные товары будут удалены.", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            _context.Unit.Remove(selected);
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Единица измерения удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUnits();
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
