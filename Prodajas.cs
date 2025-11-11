using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prriva_10
{
    public class Prodajas
    {
        public int ID { get; set; }

        public string Data { get; set; }

        public string Name { get; set; }

        public string Kolvo { get; set; }

        public string Price { get; set; }

        public List<Tovars> Tovars { get; set; } = new List<Tovars>();


    }
}
