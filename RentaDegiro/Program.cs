// See https://aka.ms/new-console-template for more information
using RentaDegiro;
using System.Globalization;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

var lines = GetDegiroLines().ToList();
decimal exchangeRate = 1;
foreach (var line in lines)
{
    if (line.VariationCoin == "EUR")
    {
        line.ExchangeRate = 1;
    }
    else if (line.VariationCoin == "USD")
    {
        if (line.ExchangeRate is not null) exchangeRate = line.ExchangeRate.Value;
        else line.ExchangeRate = exchangeRate;
    }

}

var dividendoList = lines.Where(l => l.Description == "Dividendo").ToList();
var retencionList = lines.Where(l => l.Description == "Retención del dividendo").ToList();
var retencionListSpain = retencionList.Where(l => l.ISIN.Contains("ES")).ToList();
var retencionListNotSpain = retencionList.Where(l => l.ISIN.Contains("US")).ToList();
var dividendoUS = dividendoList.Where(l => l.ISIN.Contains("US")).ToList();

var sumDividendo = dividendoList.Sum(l => l.Variation * l.ExchangeRate);
var sumDividendoUS = dividendoUS.Sum(l => l.Variation * l.ExchangeRate);
var sumRetencion = retencionListSpain.Sum(l => l.Variation * l.ExchangeRate);
var sumRetencionInternacional= retencionListNotSpain.Sum(l => l.Variation * l.ExchangeRate);

Console.WriteLine($"Suma Dividendos: {sumDividendo}");

Console.WriteLine($"Suma DividendosUS: {sumDividendoUS}");
Console.WriteLine($"Suma RetencionesES: {sumRetencion}");
Console.WriteLine($"Suma RetencionesUS: {sumRetencionInternacional}");


static List<DegiroLine> GetDegiroLines()
{

    var lines = File.ReadAllLines("C:\\Working\\DegiroEstadoCuenta2023.csv");
    var linesNoHeader = lines[1..];
    var result = linesNoHeader.Select(line =>
    {
        if (!string.IsNullOrEmpty(line))
        {
            var fields = line.Split(',').Select(item => item.Replace("\"", "")).ToList();
            var datestring = fields[2];
            var date = DateTime.ParseExact(datestring, "dd-MM-yyyy", null);
            var exchangeRateSuccess = decimal.TryParse(fields[6], null, out decimal exchangeRate);
            var success = decimal.TryParse(fields[8], null, out decimal variation);
            return new DegiroLine
            {
                Date = date,
                Description = fields[5],
                ISIN = fields[4],
                Product = fields[3],
                Variation = success ? variation : null,
                VariationCoin = fields[7],
                ExchangeRate = exchangeRateSuccess ? exchangeRate : null,
            };
        }
        else
        {
            return null;
        };
    }).Where(line => line is not null).Select(_ => _!).ToList();

    var count = result.Count;

    return result;
}
