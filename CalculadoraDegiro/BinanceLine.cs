using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculadoraDegiro
{
    internal class BinanceLine
    {
        public DateTime DateTime{ get; set; }
        public string Operation { get; set; }
        public string Coin { get; set; }
        public decimal Change { get; set; }
    }
}
