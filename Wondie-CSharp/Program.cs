using Discord;
using Discord.WebSocket;
using Serilog;
using Wondie_CSharp.Commands;
using Wondie_CSharp.Utils;
using EventHandler = Wondie_CSharp.Events.EventHandler;
using Exception = System.Exception;

namespace Wondie_CSharp;

public class Program
{
    private static DiscordSocketClient _client = null!;
    private static EventHandler _eventHandler = null!;
    
    private static readonly CancellationTokenSource CancelTokenSource = new();
    private static bool _isShuttingDown;

    public static Task Main(string[] _) => MainAsync();

    private static async Task MainAsync()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/wondie.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("Starting up...");

            Log.Debug("Serilog is set up.");

            SetupGracefulShutdown();
        
            Log.Debug("Before Discord client initialization.");

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged |
                                 GatewayIntents.MessageContent |
                                 GatewayIntents.Guilds |
                                 GatewayIntents.GuildMembers |
                                 GatewayIntents.DirectMessages
            });

            MetricsService.StartMetricsServer();

            var token = File.ReadAllText("/run/secrets/bot_token");
            Log.Debug($"BOT_TOKEN: {token.Substring(0, 5)}...");

            _eventHandler = new EventHandler(_client);
            _eventHandler.RegisterEvents();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await CommandHandler.RegisterCommands(_client);

            await Task.Delay(-1, CancelTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            Log.Information("Shutdown triggered.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception during bot runtime");
        }
        finally
        {
            MetricsService.StopMetricsServer();
            Log.CloseAndFlush();
        }
    }


    private static async Task ShutdownAsync()
    {
        if (_isShuttingDown) return;
        _isShuttingDown = true;

        Log.Warning("Shutting down...");

        try
        {
            _eventHandler.UnregisterEvents();
            await CommandHandler.UnregisterCommands();
            await _client.LogoutAsync();
            await _client.StopAsync();
            MetricsService.StopMetricsServer();
        }
        catch (Exception ex)
        {
            Log.Error($"Shutdown encountered an error: {ex.Message}");
        }
        finally
        {
            await CancelTokenSource.CancelAsync();
        }
    }

    private static void SetupGracefulShutdown()
    {
        Console.CancelKeyPress += async (_, e) =>
        {
            e.Cancel = true;
            if (!_isShuttingDown)
                await ShutdownAsync();
        };

        AppDomain.CurrentDomain.ProcessExit += async (_, _) =>
        {
            if (!_isShuttingDown)
                await ShutdownAsync();
        };
    }
}