using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StockDownloader.AlphaVantage
{
  public sealed class Downloader : IDisposable
  {
    private readonly HttpClient _client;

    public Downloader()
    {
      _client = new HttpClient();
    }

    public void Dispose()
    {
      _client.Dispose();
    }

    public async Task DownloadAsync(
      string symbol,
      Interval interval,
      OutputSize outputSize,
      DirectoryInfo directoryInfo,
      DateOrder dateOrder,
      bool timestampFiles = false,
      string apiKey = "demo")
    {
      Console.WriteLine($"Downloading: {symbol}");
      var func = ToFunctionIntervalQueryValues(interval);
      var output = outputSize.ToString().ToLower();
      var uri = $"https://www.alphavantage.co/query?{func}&symbol={symbol}&outputsize={output}&datatype=csv&apikey={apiKey}";
      var response = await _client.GetAsync(uri);
      if (!response.IsSuccessStatusCode)
      {
        Console.WriteLine($"Failed download for: {symbol}. StatusCode: {response.StatusCode}, Content: {response.Content}");
        return;
      }

      Console.WriteLine($"Saving: {symbol}");
      if (!directoryInfo.Exists)
        directoryInfo.Create();

      var data = await response.Content.ReadAsStringAsync();

      if (dateOrder == DateOrder.Ascending)
      {
        var dataRows = data.Split(Environment.NewLine);
        data = $"{dataRows[0]}{string.Join(Environment.NewLine, dataRows.Skip(1).Reverse())}";
      }

      var timestampString = timestampFiles ? "_" + DateTime.UtcNow.ToFileTimeUtc() : string.Empty;
      var fileName = $"{symbol}{timestampString}";
      var path = Path.Combine(directoryInfo.FullName, $"{fileName}.csv");
      await File.WriteAllTextAsync(path, data);
      Console.WriteLine($"Stored: {symbol}");
    }

    private static string ToFunctionIntervalQueryValues(Interval interval)
    {
      if (interval == Interval.Daily)
        return "function=TIME_SERIES_DAILY";

      return $"function=TIME_SERIES_INTRADAY&interval={ToIntervalQueryValue(interval)}";
    }

    private static string ToIntervalQueryValue(Interval interval)
    {
      switch (interval)
      {
        case Interval.OneMin:
          return "1min";
        case Interval.FiveMin:
          return "5min";
        case Interval.FifteenMin:
          return "15min";
        case Interval.ThirtyMin:
          return "30min";
        case Interval.SixtyMin:
          return "60min";
      }
      return null;
    }
  }
}
