using System.Windows;
using Microsoft.Win32;
using ClosedXML.Excel;
using Zoomag.Data;
using Zoomag.Models;
using System.Linq;

namespace Zoomag.Views
{
    public partial class ProductEditor : Window
    {
        public ProductEditor()
        {
            InitializeComponent();
        }

        private void ImportFromExcel(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Файлы Excel|*.xlsx;*.xls;*.xlsm|Все файлы|*.*"
            };

            if (openDialog.ShowDialog() != true) return;

            using var workbook = new XLWorkbook(openDialog.FileName);
            var worksheet = workbook.Worksheets.Worksheet(1);
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            if (lastRow < 2)
            {
                MessageBox.Show("Файл не содержит данных для импорта.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            for (int row = 2; row <= lastRow; row++)
            {
                var name = ReadCell(worksheet, row, 1);
                var unit = ReadCell(worksheet, row, 2);
                var quantityStr = ReadCell(worksheet, row, 3);
                var priceStr = ReadCell(worksheet, row, 4);

                if (!int.TryParse(quantityStr, out int quantity) || !int.TryParse(priceStr, out int price))
                {
                    MessageBox.Show($"Неверный формат данных в строке {row}.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

                var supplyItem = new SupplyImportViewModel
                {
                    Date = DateTime.Today,
                    Name = name,
                    Unit = unit,
                    Quantity = quantity,
                    Price = price
                };

                SupplyDataGrid.Items.Add(supplyItem);
            }
        }

        private string ReadCell(IXLWorksheet worksheet, int row, int column)
        {
            return worksheet.Cell(row, column).IsEmpty() ? string.Empty : worksheet.Cell(row, column).GetString();
        }

        private void SaveToDatabase(object sender, RoutedEventArgs e)
        {
            if (SupplyDataGrid.Items.Count == 0)
            {
                MessageBox.Show("Нет данных для сохранения.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            using var context = new AppDbContext();

            for (int i = 0; i < SupplyDataGrid.Items.Count; i++)
            {
                var item = SupplyDataGrid.Items[i] as SupplyImportViewModel;
                if (item == null) continue;

                // Создаём поставку
                var supply = new Supply
                {
                    Date = item.Date,
                    Name = $"Поставка {item.Name} от {item.Date:dd.MM.yyyy}"
                };

                context.Supply.Add(supply);
                context.SaveChanges(); // Сохраняем, чтобы получить Id

                // Ищем существующий товар
                var product = context.Product.FirstOrDefault(x => x.Name == item.Name);

                if (product != null)
                {
                    // Обновляем существующий товар
                    product.Amount += item.Quantity;
                    if (product.Price < item.Price)
                        product.Price = item.Price;
                }
                else
                {
                    // Создаём новый товар
                    var unit = context.Unit.Find(1);
                    if (unit == null)
                    {
                        MessageBox.Show("Единица измерения не найдена!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var categoryId = item.Price < 100 ? 1 : 2; // Логика определения категории
                    var category = context.Category.Find(categoryId);
                    if (category == null)
                    {
                        MessageBox.Show($"Категория с Id {categoryId} не найдена!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
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
                    context.SaveChanges(); // Сохраняем, чтобы получить Id
                    product = newProduct;
                }

                // Создаём элемент поставки
                var supplyItem = new SupplyItem
                {
                    SupplyId = supply.Id,
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    Price = item.Price
                };

                context.SupplyItem.Add(supplyItem);
            }

            context.SaveChanges();
            MessageBox.Show("Данные успешно сохранены в базу.", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public class SupplyImportViewModel
        {
            public DateTime Date { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Unit { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public int Price { get; set; }
        }
    }
}
