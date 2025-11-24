using System.Windows;
using Zoomag.Data;
using Zoomag.Models;
using Microsoft.EntityFrameworkCore;

namespace Zoomag.Views;

public partial class ProductEditorWindow : Window
{
    private readonly AppDbContext _context = new();

    public ProductEditorWindow()
    {
        InitializeComponent();
        LoadProducts();
    }

    private void LoadProducts()
    {
        var products = _context.Product
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .ToList();
        ProductsGrid.ItemsSource = products;
    }

    private void NewProductButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProductEditDialog { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _context.Product.Add(dialog.Result);
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Товар добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void EditProductButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProductsGrid.SelectedItem is not Product selected)
        {
            MessageBox.Show("Выберите товар для редактирования.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var productCopy = new Product
        {
            Id = selected.Id,
            Name = selected.Name,
            Price = selected.Price,
            CategoryId = selected.CategoryId,
            UnitId = selected.UnitId,
            Amount = selected.Amount
        };

        var dialog = new ProductEditDialog(productCopy) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _context.Entry(selected).CurrentValues.SetValues(dialog.Result);
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Товар обновлён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                LoadProducts();
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
