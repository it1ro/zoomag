// Zoomag/Models/ProductDisplayDto.cs
namespace Zoomag.Models;

public class ProductDisplayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public string UnitName { get; set; } = null!;
    public int Price { get; set; }
    public int Qty { get; set; }
}
