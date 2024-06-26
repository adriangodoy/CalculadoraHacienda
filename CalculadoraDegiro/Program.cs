﻿// See https://aka.ms/new-console-template for more information
using CalculadoraDegiro;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

var binanceLines = GetDegiroLines();
var coinValueDictionary = GetCoinValuesDictionary();

var GroubedBinanceLines = binanceLines.GroupBy(line => line.Coin);
var DollarValueInEuros= GetDollarValue();
decimal totalDollar = 0;
foreach (var CoinLines in GroubedBinanceLines)
{
    decimal totalCoin = 0;
    var DollarValues = coinValueDictionary[CoinLines.Key];
    var dailyValue = GetDailyValues(CoinLines, DollarValues, DollarValueInEuros);
    var DailyStrings= new List<string>
    {
        "Fecha,Valor,Moneda"
    };
    dailyValue.ForEach(dv => DailyStrings.Add($"{dv.Item1},{dv.Item2.ToString(CultureInfo.InvariantCulture)},{dv.Item3}"));
    File.WriteAllLines($"C:\\Working\\{CoinLines.Key}Diario.csv", DailyStrings);
    var byMonth = dailyValue.GroupBy(item => item.Item1.Month);
    var MonthlyStrings = new List<string>
    {
        "Mes,Valor,Moneda"
    };

    foreach (var Month in byMonth)
    {
        var sum = Month.Sum(item => item.Item2);
        totalDollar += sum;
        totalCoin += sum;
        Console.WriteLine($"Month:{Month.Key} € {sum}  coin: {CoinLines.Key}");
        MonthlyStrings.Add($"{Month.Key},{sum.ToString(CultureInfo.InvariantCulture)},{CoinLines.Key}");
    }
    File.WriteAllLines($"C:\\Working\\{CoinLines.Key}Mensual.csv", MonthlyStrings);
    Console.WriteLine($"TotalCoin {totalCoin.ToString(CultureInfo.InvariantCulture)}€  {CoinLines.Key}");
}
Console.WriteLine($"TotalEur {totalDollar}€");
Console.ReadLine();




static List<(DateTime, decimal, string)> GetDailyValues(IGrouping<string, BinanceLine> coinLines, List<(DateTime, decimal)> coinDollarValue, Dictionary<DateTime,decimal> DollarInEuroValue)
{
    var result = coinLines.Select((Func<BinanceLine, (DateTime Date, decimal, string Coin)>)(coin =>
    {
        var value = coinDollarValue.Find(dv => dv.Item1.Date == coin.DateTime.Date).Item2;
        var dollarValueInEurosOfTheDay = GetDollarValueInEurosOfTheDay(DollarInEuroValue, coin.DateTime.Date);
        return ((DateTime Date, decimal, string Coin))(coin.DateTime.Date, (decimal)(value * coin.Change * dollarValueInEurosOfTheDay), coin.Coin);
    })).ToList();

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
            Change = decimal.Parse(fields[5], NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint),
        };
    }).Where(transaction=>!transaction.Operation.Contains("Redem")).ToList();
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

static  Dictionary<DateTime, decimal> GetDollarValue()
{
    var lines = File.ReadAllLines("C:\\Working\\DollarEuro.csv");
    var linesNoHeader = lines[1..];
    return linesNoHeader.Select(line =>
    {
        var fields = line.Split(',').Select(item => item.Replace("\"", "")).ToList();

        return (DateTime.Parse(fields[0], CultureInfo.InvariantCulture), decimal.Parse(fields[4], CultureInfo.InvariantCulture));
    }).ToDictionary();
}

static string ExtractCoinFromFilename(string filename)
{
    const string directory = "C:\\Working\\Coins\\";
    var withoutDirectory = filename[directory.Length..];
    var index = withoutDirectory.IndexOf("-");
    return withoutDirectory[..index];
}

static decimal GetDollarValueInEurosOfTheDay(Dictionary<DateTime, decimal> DollarInEuroValue, DateTime date)
{
    if (DollarInEuroValue.ContainsKey(date.Date))
        return DollarInEuroValue[date.Date];
    else return GetDollarValueInEurosOfTheDay(DollarInEuroValue, date.AddDays(-1));
}