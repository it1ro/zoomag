using System.Windows;
using Zoomag.Models;

namespace Zoomag.Views;

public partial class UnitEditDialog : Window
{
    public Unit Result { get; private set; }
    private readonly bool _isEditMode;

    public UnitEditDialog()
    {
        InitializeComponent();
        _isEditMode = false;
    }

    public UnitEditDialog(Unit unitToEdit)
    {
        InitializeComponent();
        _isEditMode = true;
        NameInput.Text = unitToEdit.Name;
        Result = unitToEdit;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var name = NameInput.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Укажите название единицы измерения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_isEditMode)
        {
            Result.Name = name;
        }
        else
        {
            Result = new Unit { Name = name };
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
