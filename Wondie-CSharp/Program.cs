using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Wondie_CSharp;

public class Program
{
    public static Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/wondie_.log",
                rollingInterval: RollingInterval.Day
            )
            .CreateLogger();
        
        try
        {
            Log.Information("Starting bot service...");

            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services.AddHostedService<WondieWorker>();
                })
                .UseSerilog()
                .Build()
                .RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "The bot service terminated unexpectedly.");
            return Task.FromException(ex);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}