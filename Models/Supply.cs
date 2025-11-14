using System;
using System.Collections.Generic;

namespace Zoomag.Models
{
    public class Supply
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int TotalAmount { get; }

        public List<SupplyItem> SupplyItems { get; set; } = new();
    }
}
