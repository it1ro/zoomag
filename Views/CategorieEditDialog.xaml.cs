using System.Windows;
using Zoomag.Models;

namespace Zoomag.Views;

public partial class CategoryEditDialog : Window
{
    public Category Result { get; private set; }
    private readonly bool _isEditMode;

    // Для создания новой категории
    public CategoryEditDialog()
    {
        InitializeComponent();
        _isEditMode = false;
    }

    // Для редактирования существующей
    public CategoryEditDialog(Category categoryToEdit)
    {
        InitializeComponent();
        _isEditMode = true;
        NameInput.Text = categoryToEdit.Name;
        Result = categoryToEdit;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var name = NameInput.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Укажите название категории.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_isEditMode)
        {
            Result.Name = name;
        }
        else
        {
            Result = new Category { Name = name };
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
