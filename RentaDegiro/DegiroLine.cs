using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentaDegiro
{
    internal class DegiroLine
    {

        [Name("Fecha valor")]
        public DateTime Date { get; set; }
      
        [Name("Producto")]
        public string Product { get; set; }
        [Name("ISIN")]
        public string ISIN { get; set; }
        [Name("Descripción")]
        public string Description { get; set; }

        [Name("Tipo")]
        public decimal? ExchangeRate { get; set; }

        [Name("Variación")]
        public decimal? Variation { get; set;}

        public string VariationCoin { get; set; }
    }
}
