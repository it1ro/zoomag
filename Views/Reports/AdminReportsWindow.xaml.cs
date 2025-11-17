using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Zoomag.Data;
using Zoomag.Models;

namespace Zoomag.Views.Reports
{
    using ClosedXML.Excel;

    public partial class AdminReportsWindow : Window
    {
        public AdminReportsWindow()
        {
            InitializeComponent();
        }

        private void GoToAdmin(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            this.Hide();
            adminWindow.Show();
        }

        // Измененный метод: теперь открывает окно
        private void GeneratePriceList(object sender, RoutedEventArgs e)
        {
            var priceListWindow = new PriceListWindow();
            this.Hide();
            priceListWindow.Show();
        }

        private void ViewIncomingGoodsJournal(object sender, RoutedEventArgs e)
        {
            var journalWindow = new IncomingGoodsJournalWindow();
            this.Hide();
            journalWindow.Show();
        }

        private void GenerateStockReport(object sender, RoutedEventArgs e)
        {
            var stockReportWindow = new StockReportWindow();
            this.Hide();
            stockReportWindow.Show();
        }

        private void ViewDailyReceiptReport(object sender, RoutedEventArgs e)
        {
            var reportWindow = new DailyGoodsReceiptReportWindow();
            this.Hide();
            reportWindow.Show();
        }

        private void ViewCategoryReport(object sender, RoutedEventArgs e)
        {
            var categoryReportWindow = new CategoryReportWindow();
            this.Hide();
            categoryReportWindow.Show();
        }

        private void ViewZeroStockReport(object sender, RoutedEventArgs e)
        {
            var zeroStockWindow = new ZeroStockReportWindow();
            this.Hide();
            zeroStockWindow.Show();
        }
    }
}
