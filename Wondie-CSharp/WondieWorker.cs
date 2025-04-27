using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wondie_CSharp.Commands;
using Wondie_CSharp.Events.Actions;
using Wondie_CSharp.Utils;
using EventHandler = Wondie_CSharp.Events.EventHandler;


namespace Wondie_CSharp;

public class WondieWorker : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly ILogger<WondieWorker> _logger;
    
    private CommandHandler _commandHandler;
    private EventHandler _eventHandler;

    public WondieWorker(ILogger<WondieWorker> logger)
    {
        _logger = logger;
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | 
                             GatewayIntents.Guilds | 
                             GatewayIntents.GuildMessages | 
                             GatewayIntents.MessageContent  
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Log += msg =>
        {
            _logger.LogInformation(msg.ToString());
            return Task.CompletedTask;
        };
        
        _commandHandler = new CommandHandler(_client, _logger);
        _eventHandler = new EventHandler(_client);

        _eventHandler.RegisterEvents();

        _client.Ready += async () =>
        {
            _logger.LogInformation($"{_client.CurrentUser.GlobalName} is connected and ready!");
            await _commandHandler.RegisterCommandsAsync();
            MetricsService.StartMetricsServer();
        };
        
        _client.SlashCommandExecuted += _commandHandler.HandleSlashCommandAsync;

        var token = await File.ReadAllTextAsync("/run/secrets/bot_token", stoppingToken);
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Shutdown requested
        }
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Discord bot...");
        
        _eventHandler.UnregisterEvents();
        
        await StatusEvent.SetBotStatusToOffline(_client);
        StatusEvent.StopMonitoring(new LoggerFactory().CreateLogger("StatusEvent"));
        
        MetricsService.StopMetricsServer();
        
        await _client.StopAsync();
        await base.StopAsync(cancellationToken);
    }
}