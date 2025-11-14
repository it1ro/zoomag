using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zoomag.Models
{
    public class SaleItem
    {
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public Sale Sale { get; set; }
        public Product Product { get; set; }

    }
}
