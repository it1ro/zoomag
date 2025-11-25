namespace Zoomag.Models;

using System.ComponentModel.DataAnnotations.Schema;

public class Supply
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }

    public List<SupplyItem> SupplyItems { get; set; } = new();
}
