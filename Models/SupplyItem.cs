namespace Zoomag.Models;

public class SupplyItem
{
    public int SupplyId { get; set; }
    public int ProductId { get; set; }

    public Supply Supply { get; set; }
    public Product Product { get; set; }

    public int Quantity { get; set; }
    public int Price { get; set; }
    public int Total { get; set; }
}
