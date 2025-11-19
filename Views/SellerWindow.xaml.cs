using System.Windows;


namespace Zoomag.Views;

public partial class SellerWindow : Window
{
    public SellerWindow()
    {
        InitializeComponent();
    }

    private void ShowSale(object sender, RoutedEventArgs e)
    {
        var saleWindow = new SaleWindow();
        saleWindow.Show();

    }

    private void ShowStock(object sender, RoutedEventArgs e)
    {
        var stockWindow = new ProductOverviewWindow();
        stockWindow.Show();
    }

    private void ShowProductOverview(object sender, RoutedEventArgs e)
    {
        var productOverviewWindow = new ProductOverviewWindow();
        productOverviewWindow.Show();
    }


}
