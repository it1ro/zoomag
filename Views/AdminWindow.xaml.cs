// File: Zoomag/Views/AdminWindow.xaml.cs

namespace Zoomag.Views;

using System.Windows;
using System.Windows.Controls; // Добавлено для ComboBox
using Data;
using Models;
using Reports;

/// <summary>
/// Interaction logic for AdminWindow.xaml
/// </summary>
public partial class AdminWindow : Window, IDisposable
{
    // Удаляем поле _context из класса.

    public AdminWindow()
    {
        InitializeComponent();
    }

    public void Dispose()
    {
        // Не нужно больше уничтожать _context здесь, так как он больше не поле класса.
        // Однако, если у окна есть другие ресурсы, их можно освободить.
        GC.SuppressFinalize(this);
    }



    private void ShowArrival(object sender, RoutedEventArgs e)
    {
        var arrivalWindow = new ArrivalWindow();
        Hide();
        arrivalWindow.Show();
    }

    private void ShowSale(object sender, RoutedEventArgs e)
    {
        var saleWindow = new SaleWindow();
        Hide();
        saleWindow.Show();
    }

    private void ShowStock(object sender, RoutedEventArgs e)
    {
        var stockWindow = new StockWindow();
        Hide();
        stockWindow.Show();
    }

    private void ShowReports(object sender, RoutedEventArgs e)
    {
        var reportsWindow = new AdminReportsWindow();
        Hide();
        reportsWindow.Show();
    }

    private void OpenProductEditor(object sender, RoutedEventArgs e)
    {
        var editor = new ProductEditorWindow();
        Hide();
        editor.Show();
    }

    private void OpenCategoriesEditor(object sender, RoutedEventArgs e)
    {
        var editor = new CategoriesEditorWindow();
        Hide();
        editor.Show();
    }

    private void OpenUnitsEditor(object sender, RoutedEventArgs e)
    {
        var editor = new UnitsEditorWindow();
        Hide();
        editor.Show();
    }
}
