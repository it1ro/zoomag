using System.Windows;
using Zoomag.Views;

namespace Zoomag.Views;

public partial class AdminWindow : Window
{
    public AdminWindow()
    {
        InitializeComponent();
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

    private void ShowArrival(object sender, RoutedEventArgs e)
    {
        var arrival = new ArrivalWindow();
        Hide();
        arrival.Show();
    }

    private void ShowSale(object sender, RoutedEventArgs e)
    {
        var sale = new SaleWindow();
        Hide();
        sale.Show();
    }

    private void ShowStock(object sender, RoutedEventArgs e)
    {
        var stock = new StockWindow();
        Hide();
        stock.Show();
    }

    private void ShowReports(object sender, RoutedEventArgs e)
    {
        var reports = new Reports.AdminReportsWindow();
        Hide();
        reports.Show();
    }

    private void Logout(object sender, RoutedEventArgs e)
    {
        var login = new LoginWindow();
        Hide();
        login.Show();
    }
}
