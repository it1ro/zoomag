using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace prriva_10
{
    public class Tovars
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Izmers Izmer { get; set; }
        public Kategors Kategor { get; set; }
        public int Price { get; set; }
        public int Kol_vo { get; set; }
        public List<Privozs> Privozs { get; set; } = new List<Privozs>();
        public List<Prodajas> Prodajas { get; set; } = new List<Prodajas>();
    }
}
