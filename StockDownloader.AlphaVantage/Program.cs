using CommandLine;
using DryIoc;
using MoreLinq;
using Nito.AsyncEx;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StockDownloader.AlphaVantage
{
  class Program
  {
    static void Main(string[] args)
    {
      var container = new Container();
      var result = Parser
        .Default
        .ParseArguments<RuntimeParameters>(args)
        .WithParsed(runtimeParams =>
        {
          container.RegisterInstance(runtimeParams);
        })
        .WithNotParsed(errors =>
        {
          errors
            .Where(error => error.StopsProcessing)
            .ForEach(error =>
            {
              Console.WriteLine(error.ToString());
            });
        });
      if (result.Tag == ParserResultType.NotParsed)
      {
        Console.WriteLine("Resolve errors");
        Console.ReadLine();
        return;
      }
      container.Register<Downloader>(Reuse.Singleton);
      container.Validate();

      AsyncContext.Run(() =>
      {
        var runtimeParams = container.Resolve<RuntimeParameters>();
        var downloader = container.Resolve<Downloader>();
        var downloadTasks = (runtimeParams.Symbols ?? "AA")
          .Split(',')
          .Select(symbol =>
          {
            return downloader.DownloadAsync(
              symbol,
              runtimeParams.Interval,
              runtimeParams.OutputSize,
              new DirectoryInfo(runtimeParams.OutputDirectory ?? $"Data_{DateTime.UtcNow.ToFileTimeUtc()}"),
              runtimeParams.DateOrder,
              runtimeParams.TimestampFiles,
              runtimeParams.ApiKey);
          });
        return Task.WhenAll(downloadTasks);
      });

      Console.WriteLine("Hit enter to close");
      Console.ReadLine();
    }
  }
}
