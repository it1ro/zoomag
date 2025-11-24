namespace Zoomag.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Внешние ключи (явные)
    public int CategoryId { get; set; }
    public int UnitId { get; set; }

    // Навигационные свойства
    public Category Category { get; set; } = null!;
    public Unit Unit { get; set; } = null!;

    public int Price { get; set; }
    public int Amount { get; set; }

    public List<SupplyItem> SupplyItems { get; set; } = new();
    public List<SaleItem> SaleItems { get; set; } = new();
}
