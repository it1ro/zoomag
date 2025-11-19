namespace Zoomag.Views.Reports;

using System.Windows;

public partial class AdminReportsWindow : Window
{
    public AdminReportsWindow()
    {
        InitializeComponent();
    }

    private void GoToAdmin(object sender, RoutedEventArgs e)
    {
        var adminWindow = new AdminWindow();
        Hide();
        adminWindow.Show();
    }

    // Измененный метод: теперь открывает окно
    private void GeneratePriceList(object sender, RoutedEventArgs e)
    {
        var priceListWindow = new PriceListWindow();
        Hide();
        priceListWindow.Show();
    }

    private void ViewIncomingGoodsJournal(object sender, RoutedEventArgs e)
    {
        var journalWindow = new IncomingGoodsJournalWindow();
        Hide();
        journalWindow.Show();
    }

    private void GenerateStockReport(object sender, RoutedEventArgs e)
    {
        var stockReportWindow = new StockReportWindow();
        Hide();
        stockReportWindow.Show();
    }

    private void ViewDailyReceiptReport(object sender, RoutedEventArgs e)
    {
        var reportWindow = new DailyGoodsReceiptReportWindow();
        Hide();
        reportWindow.Show();
    }

    private void ViewCategoryReport(object sender, RoutedEventArgs e)
    {
        var categoryReportWindow = new CategoryReportWindow();
        Hide();
        categoryReportWindow.Show();
    }

    private void ViewZeroStockReport(object sender, RoutedEventArgs e)
    {
        var zeroStockWindow = new ZeroStockReportWindow();
        Hide();
        zeroStockWindow.Show();
    }
}
