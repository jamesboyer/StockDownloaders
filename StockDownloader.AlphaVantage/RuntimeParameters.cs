using CommandLine;

namespace StockDownloader.AlphaVantage
{
  public sealed class RuntimeParameters
  {
    [Option('k', "apiKey", Default = null, HelpText = "AlphaVantage API key", Required = true)]
    public string ApiKey { get; set; }

    [Option('i', "interval", Default = Interval.Daily, HelpText = "Interval of daily data", Required = false)]
    public Interval Interval { get; set; }

    [Option('s', "symbols", Default = null, HelpText = "Comma separated symbols to download (null or empty will download all)", Required = false)]
    public string Symbols { get; set; }

    [Option('o', "output", Default = null, HelpText = "Desired output directory (default will auto-generate one)", Required = false)]
    public string OutputDirectory { get; set; }

    [Option("outputSize", Default = OutputSize.Full, HelpText = "The size of the data to collect", Required = false)]
    public OutputSize OutputSize { get; set; } 

    [Option("order", Default = DateOrder.Decending, HelpText = "The order for dates to be recorded to output", Required = false)]
    public DateOrder DateOrder { get; set; }

    [Option('t', "timestampFiles", Default = false, HelpText = "Flag to timestamp downloaded files", Required = false)]
    public bool TimestampFiles { get; set; }
  }
}
