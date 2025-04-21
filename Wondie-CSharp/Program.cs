using Discord;
using Discord.WebSocket;
using Wondie_CSharp.Commands;
using Wondie_CSharp.Events;
using Wondie_CSharp.utils;
using Exception = System.Exception;

namespace Wondie_CSharp;

public class Program
{
    private static DiscordSocketClient _client;
    private static readonly CancellationTokenSource _cancellationTokenSource = new();
    private static bool _isShuttingDown;

    public static Task Main(string[] args) => MainAsync();

    private static async Task MainAsync()
    {
        Console.CancelKeyPress += async (sender, e) =>
        {
            e.Cancel = true;
            if (!_isShuttingDown)
            {
                await ShutdownAsync();
            }
        };

        AppDomain.CurrentDomain.ProcessExit += async (sender, e) =>
        {
            if (!_isShuttingDown)
            {
                await ShutdownAsync();
            }
        };

        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | 
                             GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.DirectMessages,
        });
        
        DotNetEnv.Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));
        var token = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? throw new Exception("BOT_TOKEN not found in environment variables.");
        
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        
        _client.Ready += () => ReadyEvent.OnClientReady(_client, token);
        _client.MessageReceived += MessageEvent.MessageLogger;
        
        await CommandHandler.RegisterCommands(_client);
        
        try
        {
            await Task.Delay(-1, _cancellationTokenSource.Token);
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
            if (_client?.ConnectionState == ConnectionState.Connected)
            {
                await CommandHandler.UnregisterCommands();
                await _client.StopAsync();
                await _client.LogoutAsync();
            }
        }
        catch (Exception ex)
        {
            await Log.Error($"Error during shutdown: {ex.Message}");
        }
        finally
        {
            _cancellationTokenSource.Cancel();
        }
    }
}