namespace Zoomag.Models;

public class Sale
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; }
    public List<SaleItem> SaleItems { get; set; } = new();
}
