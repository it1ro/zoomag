using System.Windows;
using Zoomag.Data;
using Zoomag.Models;

namespace Zoomag.Views;

public partial class UnitsEditorWindow : Window
{
    private readonly AppDbContext _context = new();
    private Unit _editingUnit;

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

    private void UnitsGrid_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
    {
        if (UnitsGrid.SelectedItem is not Unit selected) return;

        _editingUnit = selected;
        NameInput.Text = selected.Name;
    }

    private void NewUnitButton_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var name = NameInput.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Укажите название единицы измерения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int editingId = _editingUnit?.Id ?? 0;

        bool isDuplicate = _context.Unit.Any(u =>
            u.Id != editingId &&
            u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (isDuplicate)
        {
            MessageBox.Show("Единица измерения с таким названием уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_editingUnit == null)
        {
            // Создание
            var unit = new Unit { Name = name };
            _context.Unit.Add(unit);
        }
        else
        {
            // Редактирование
            _editingUnit.Name = name;
        }

        try
        {
            _context.SaveChanges();
            MessageBox.Show("Единица измерения сохранена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearForm();
            LoadUnits();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                ClearForm();
                LoadUnits();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ClearForm()
    {
        _editingUnit = null;
        NameInput.Clear();
        UnitsGrid.UnselectAll();
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
