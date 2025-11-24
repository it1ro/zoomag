namespace Zoomag.Views;

using System.Windows;

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
        var stockWindow = new StockWindow();
        stockWindow.Show();
    }

    private void ShowProducts(object sender, RoutedEventArgs e)
    {
        var productEditorWindow = new ProductEditorWindow();
        productEditorWindow.Show();
    }

    private void Logout(object sender, RoutedEventArgs e)
    {
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        Close(); // закрываем окно продавца
    }
}
