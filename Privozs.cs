using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prriva_10
{
    public class Privozs
    {
        public int ID { get; set; }
        public string Data { get; set; }
        public string Name { get; set; }
        public int Kolvo { get; set; }
        public int Price { get; set; }
        public List<Tovars> Tovar { get; set; } = new List<Tovars>();
    }
}
