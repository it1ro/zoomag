using System.Windows;
using Zoomag.Views; // Для доступа к SaleWindow, StockWindow, ProductOverviewWindow

namespace Zoomag.Views;

public partial class SellerWindow : Window
{
    public SellerWindow()
    {
        InitializeComponent();
    }

    private void ShowSale(object sender, RoutedEventArgs e)
    {
        var saleWindow = new SaleWindow(); // Открывает окно продажи
        saleWindow.Show();
        // this.Close(); // Раскомментируй, если хочешь закрыть текущее окно
    }

    private void ShowStock(object sender, RoutedEventArgs e)
    {
        var stockWindow = new ProductOverviewWindow(); // Открывает окно склада
        stockWindow.Show();
        // this.Close(); // Раскомментируй, если хочешь закрыть текущее окно
    }

    private void ShowProductOverview(object sender, RoutedEventArgs e)
    {
        var productOverviewWindow = new ProductOverviewWindow(); // Открывает окно каталога товаров
        productOverviewWindow.Show();
        // this.Close(); // Раскомментируй, если хочешь закрыть текущое окно
    }

    private void Logout(object sender, RoutedEventArgs e)
    {
        // Закрывает текущее окно продавца и возвращается к окну входа
        this.Close();
        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }
}
