using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prriva_10
{
    public class Kategors
    {
        public int ID { get; set; }
        public string Typ { get; set; }
  
        public List<Tovars> Tovars { get; set; }
    }
}
