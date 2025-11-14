using System.Windows;
using Microsoft.Win32;
using ClosedXML.Excel;
using Zoomag.Data;
using Zoomag.Models;

namespace Zoomag.Views
{
    public partial class ProductEditor : Window
    {
        public ProductEditor()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Файлы Excel|*.xlsx;*.xls;*.xlsm|Все файлы|*.*";
            if (openDialog.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook(openDialog.FileName))
                {
                    var ws = workbook.Worksheets.Worksheet(1);
                    var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

                    for (int i = 2; i <= lastRow; i++)
                    {
                        string name = ws.Cell(i, 1).IsEmpty() ? "" : ws.Cell(i, 1).GetString();
                        string unit = ws.Cell(i, 2).IsEmpty() ? "" : ws.Cell(i, 2).GetString();
                        string amountStr = ws.Cell(i, 3).IsEmpty() ? "0" : ws.Cell(i, 3).GetString();
                        string priceStr = ws.Cell(i, 4).IsEmpty() ? "0" : ws.Cell(i, 4).GetString();

                        if (int.TryParse(amountStr, out int quantity) && int.TryParse(priceStr, out int price))
                        {
                            var item = new SupplyImportViewModel
                            {
                                Date = DateTime.Today,  // ✅ DateTime
                                Name = name,
                                Unit = unit,
                                Quantity = quantity,
                                Price = price
                            };
                            dg.Items.Add(item);
                        }
                        else
                        {
                            MessageBox.Show($"Неверный формат данных в строке {i}.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using var context = new AppDbContext();

            for (int i = 0; i < dg.Items.Count; i++)
            {
                var item = dg.Items[i] as SupplyImportViewModel;

                // Создаём поставку
                var supply = new Supply
                {
                    Date = item.Date,  // ✅ DateTime
                    Name = $"Поставка {item.Name} от {item.Date:dd.MM.yyyy}",
                };

                context.Supply.Add(supply);
                context.SaveChanges(); // Нужно, чтобы получить supply.Id

                var product = context.Product.Where(x => x.Name == item.Name).FirstOrDefault();

                if (product != null)
                {
                    product.Amount += item.Quantity;
                    if (product.Price < item.Price) product.Price = item.Price;
                }
                else
                {
                    var unit = context.Unit.Find(1);
                    if (unit == null)
                    {
                        MessageBox.Show("Единица измерения не найдена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var category = context.Category.Find(item.Price < 100 ? 1 : 2);
                    if (category == null)
                    {
                        MessageBox.Show("Категория не найдена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var newProduct = new Product
                    {
                        Name = item.Name,
                        Price = item.Price,
                        Amount = item.Quantity,
                        Unit = unit,
                        Category = category
                    };
                    context.Product.Add(newProduct);
                    context.SaveChanges(); // Чтобы получить Id
                    product = newProduct;
                }

                // Создаём элемент поставки
                var supplyItem = new SupplyItem
                {
                    SupplyId = supply.Id,
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    Price = item.Price
                    // Total вычисляется автоматически
                };

                context.SupplyItem.Add(supplyItem);

                // ✅ Обновляем общую сумму поставки
            }

            context.SaveChanges(); // Сохраняем всё
        }

        public class SupplyImportViewModel
        {
            public DateTime Date { get; set; }  // ✅ DateTime
            public string Name { get; set; }
            public string Unit { get; set; }
            public int Quantity { get; set; }
            public int Price { get; set; }
        }
    }
}
