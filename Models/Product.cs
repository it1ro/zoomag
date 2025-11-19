namespace Zoomag.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Unit Unit { get; set; }
    public Category Category { get; set; }
    public int Price { get; set; }
    public int Amount { get; set; }

    public List<SupplyItem> SupplyItems { get; set; } = new();
    public List<SaleItem> SaleItems { get; set; } = new();
}
