namespace Zoomag.Views;

using System.Windows;
using Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
///     Логика взаимодействия для ProductOverviewWindow.xaml
/// </summary>
public partial class ProductOverviewWindow : Window
{
    private readonly AppDbContext _context;

    public ProductOverviewWindow()
    {
        InitializeComponent();
        _context = new AppDbContext();
        LoadProductNames();
    }

    private void LoadProductNames()
    {
        ProductNameGrid.ItemsSource = _context.Product
            .OrderBy(product => product.Name)
            .ToList();
    }

    private void LoadProductDetails(object sender, RoutedEventArgs e)
    {
        ProductDetailsGrid.ItemsSource = _context.Product
            .Include(product => product.Unit)
            .Include(product => product.Category)
            .OrderByDescending(product => product.Amount)
            .ToList();
    }

    private void GoToAdmin(object sender, RoutedEventArgs e)
    {
        var adminWindow = new AdminWindow();
        Hide();
        adminWindow.Show();
    }

    protected override void OnClosed(EventArgs e)
    {
        _context?.Dispose();
        base.OnClosed(e);
    }
}
