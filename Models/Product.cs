namespace Zoomag.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CategoryId { get; set; }
    public int UnitId { get; set; }

    public Category Category { get; set; } = null!;
    public Unit Unit { get; set; } = null!;

    // Связи с транзакциями — ОСТАВЛЯЕМ
    public List<SupplyItem> SupplyItems { get; set; } = new();
    public List<SaleItem> SaleItems { get; set; } = new();
}
