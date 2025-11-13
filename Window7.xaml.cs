using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;

namespace prriva_10
{
    public partial class Window7 : Window
    {
        public Window7()
        {
            InitializeComponent();
            // Установка текущей даты по умолчанию
            DeliveryDatePicker.SelectedDate = DateTime.Today;
            LoadData();
        }

        private void LoadData()
        {
            if (DeliveryDatePicker.SelectedDate == null)
                return;

            DateTime selectedDate = DeliveryDatePicker.SelectedDate.Value.Date;

            try
            {
                using (var context = new AppDbContext())
                {
                    var deliveries = context.Privozs.ToList();

                    var reportItems = new List<ReportItem>();
                    int totalSum = 0;
                    int totalQuantity = 0;

                    foreach (var delivery in deliveries)
                    {
                        DateTime deliveryDate;
                        // Парсим дату из строки - предполагаем формат "dd.MM.yyyy" или подобный
                        if (DateTime.TryParse(delivery.Data, out deliveryDate) && 
                            deliveryDate.Date == selectedDate.Date)
                        {
                            int itemTotal = delivery.Price * delivery.Kolvo;
                            reportItems.Add(new ReportItem
                            {
                                Name = delivery.Name,
                                Kolvo = delivery.Kolvo,
                                Price = delivery.Price,
                                Total = itemTotal
                            });

                            totalSum += itemTotal;
                            totalQuantity += delivery.Kolvo;
                        }
                    }

                    // Обновление интерфейса
                    sum.Text = totalSum.ToString();
                    kol_tov.Text = totalQuantity.ToString();
                    dataGrid.ItemsSource = reportItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            DeliveryDatePicker.SelectedDate = DateTime.Today;
            LoadData();
        }

        private void nazad(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            this.Hide();
            main.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Window8 nextWindow = new Window8();
            this.Hide();
            nextWindow.Show();
        }
    }

    public class ReportItem
    {
        public string Name { get; set; }
        public int Kolvo { get; set; }
        public int Price { get; set; }
        public int Total { get; set; }
    }
}