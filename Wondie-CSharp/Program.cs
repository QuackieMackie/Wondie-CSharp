using Discord;
using Discord.WebSocket;
using Wondie_CSharp.Commands;
using Wondie_CSharp.utils;
using EventHandler = Wondie_CSharp.Events.EventHandler;
using Exception = System.Exception;

namespace Wondie_CSharp;

public class Program
{
    private static DiscordSocketClient _client;
    private static EventHandler _eventHandler;
    
    private static readonly CancellationTokenSource _cancelTokenSource = new();
    private static bool _isShuttingDown;

    public static Task Main(string[] _) => MainAsync();

    private static async Task MainAsync()
    {
        SetupGracefulShutdown();

        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged |
                             GatewayIntents.MessageContent |
                             GatewayIntents.Guilds |
                             GatewayIntents.GuildMembers |
                             GatewayIntents.DirectMessages
        });

        DotNetEnv.Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));
        var token = Environment.GetEnvironmentVariable("BOT_TOKEN") 
                    ?? throw new Exception("BOT_TOKEN is not found in environment variables.");

        _eventHandler = new EventHandler(_client);
        _eventHandler.RegisterEvents();

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await CommandHandler.RegisterCommands(_client);

        try
        {
            await Task.Delay(-1, _cancelTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            // Normal shutdown
        }
    }

    private static async Task ShutdownAsync()
    {
        if (_isShuttingDown) return;
        _isShuttingDown = true;

        await Log.Warn("Shutting down...");

        try
        {
            _eventHandler.UnregisterEvents();
            await CommandHandler.UnregisterCommands();
            await _client.LogoutAsync();
            await _client.StopAsync();
        }
        catch (Exception ex)
        {
            await Log.Error($"Shutdown encountered an error: {ex.Message}");
        }
        finally
        {
            _cancelTokenSource.Cancel();
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