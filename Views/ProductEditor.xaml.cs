// File: Zoomag/Views/ProductEditor.xaml.cs
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
            using var transaction = context.Database.BeginTransaction(); // Используем транзакцию
            try
            {
                var suppliesToSave = new List<Supply>();
                var productsToSave = new List<Product>();
                var supplyItemsToSave = new List<SupplyItem>();

                for (int i = 0; i < SupplyDataGrid.Items.Count; i++)
                {
                    var item = SupplyDataGrid.Items[i] as SupplyImportViewModel;
                    if (item == null) continue;

                    // Создаём поставку (пока без Id, он появится после SaveChanges)
                    var supply = new Supply
                    {
                        Date = item.Date,
                        Name = $"Поставка {item.Name} от {item.Date:dd.MM.yyyy}"
                    };
                    suppliesToSave.Add(supply);

                    // Ищем существующий товар
                    var product = context.Product.FirstOrDefault(x => x.Name == item.Name);
                    if (product != null)
                    {
                        // Обновляем существующий товар
                        product.Amount += item.Quantity;
                        if (product.Price < item.Price)
                            product.Price = item.Price;
                        // Объект product уже отслеживается контекстом, изменения будут сохранены
                    }
                    else
                    {
                        // Создаём новый товар
                        var unit = context.Unit.Find(1); // Хардкод
                        if (unit == null)
                        {
                            MessageBox.Show("Единица измерения не найдена!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            transaction.Rollback(); // Откатываем транзакцию
                            return;
                        }
                        var categoryId = item.Price < 100 ? 1 : 2; // Логика определения категории
                        var category = context.Category.Find(categoryId); // Хардкод
                        if (category == null)
                        {
                            MessageBox.Show($"Категория с Id {categoryId} не найдена!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            transaction.Rollback(); // Откатываем транзакцию
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
                        productsToSave.Add(newProduct);
                        product = newProduct; // Обновляем ссылку
                    }

                    // Создаём элемент поставки (ссылки на supply и product пока не установлены)
                    var supplyItem = new SupplyItem
                    {
                        // SupplyId будет установлен после сохранения Supply
                        // ProductId будет установлен после сохранения Product
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Total = item.Quantity * item.Price // Устанавливаем Total
                    };
                    // Связываем с объектами в памяти, EF установит Id при SaveChanges
                    supplyItem.Supply = supply;
                    supplyItem.Product = product;
                    supplyItemsToSave.Add(supplyItem);
                }

                // Добавляем все сущности в контекст
                if (suppliesToSave.Any()) context.Supply.AddRange(suppliesToSave);
                if (productsToSave.Any()) context.Product.AddRange(productsToSave);
                if (supplyItemsToSave.Any()) context.SupplyItem.AddRange(supplyItemsToSave);

                // Сохраняем все изменения в транзакции
                context.SaveChanges();
                transaction.Commit(); // Подтверждаем транзакцию

                MessageBox.Show("Данные успешно сохранены в базу.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                transaction.Rollback(); // Откатываем транзакцию в случае ошибки
                MessageBox.Show($"Ошибка при сохранении в базу: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
