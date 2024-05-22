// See https://aka.ms/new-console-template for more information
using CalculadoraDegiro;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;


var binanceLines = GetDegiroLines();
var coinValueDictionary = GetCoinValuesDictionary();

var GroubedBinanceLines = binanceLines.GroupBy(line => line.Coin);
decimal totalDollar = 0;
foreach (var CoinLines in GroubedBinanceLines)
{
    decimal totalCoin = 0;
    var DollarValues = coinValueDictionary[CoinLines.Key];
    var dailyValue = GetDailyValues(CoinLines, DollarValues);

    var byMonth = dailyValue.GroupBy(item => item.Item1.Month);
    foreach (var Month in byMonth)
    {
        var sum = Month.Sum(item => item.Item2);
        totalDollar += sum;
        totalCoin += sum;
        Console.WriteLine($"Month:{Month.Key} $ {sum}  coin: {CoinLines.Key}");

    }
    Console.WriteLine($"TotalCoin {totalCoin}$  {CoinLines.Key}");
}
Console.WriteLine($"TotalDollar {totalDollar}$");
Console.ReadLine();




static List<(DateTime, decimal, string)> GetDailyValues(IGrouping<string, BinanceLine> coinLines, List<(DateTime, decimal)> dollarValues)
{
    var result = coinLines.Select(coin =>
    {
        var value = dollarValues.Find(dv => dv.Item1.Date == coin.DateTime.Date).Item2;
        return (coin.DateTime.Date, value * coin.Change, coin.Coin);
    }).ToList();

    return result;
}


static List<BinanceLine> GetDegiroLines()
{
    var lines = File.ReadAllLines("C:\\Working\\BinanceDataAdri.csv");
    var linesNoHeader = lines[1..];
    return linesNoHeader.Select(line =>
    {
        var fields = line.Split(',').Select(item => item.Replace("\"", "")).ToList();

        return new BinanceLine
        {
            DateTime = DateTime.Parse(fields[1], CultureInfo.InvariantCulture),
            Operation = fields[3],
            Coin = fields[4],
            Change = decimal.Parse(fields[5], CultureInfo.InvariantCulture),
        };
    }).ToList();
}


static Dictionary<string, List<(DateTime, decimal)>> GetCoinValuesDictionary()
{
    var result = new Dictionary<string, List<(DateTime, decimal)>>();
    var files = Directory.GetFiles("C:\\Working\\Coins");
    foreach (var file in files)
    {
        var coin = ExtractCoinFromFilename(file);
        var lines = File.ReadAllLines(file);
        var linesNoHeader = lines[1..];
        var cosa = linesNoHeader.Select(line =>
        {
            var fields = line.Split(',').Select(item => item.Replace("\"", "")).ToList();

            return (DateTime.Parse(fields[0], CultureInfo.InvariantCulture), decimal.Parse(fields[4], CultureInfo.InvariantCulture));
        }).ToList();
        result.Add(coin, cosa);
    }
    return result;
}


static string ExtractCoinFromFilename(string filename)
{
    const string directory = "C:\\Working\\Coins\\";
    var withoutDirectory = filename[directory.Length..];
    var index = withoutDirectory.IndexOf("-");
    return withoutDirectory[..index];
}
