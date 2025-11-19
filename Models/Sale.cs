namespace Zoomag.Models;

public class Sale
{
    public int Id { get; set; }
    public string Date { get; set; }
    public string Name { get; set; }
    public int Amount { get; set; }
    public int Price { get; set; }

    public List<SaleItem> SaleItems { get; set; } = new();
}
