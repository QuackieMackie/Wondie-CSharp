using Discord;
using Discord.WebSocket;
using Wondie_CSharp.Events.Actions;
using Wondie_CSharp.utils;

namespace Wondie_CSharp.Events;

public class EventHandler
{
    private readonly DiscordSocketClient _client;

    public EventHandler(DiscordSocketClient client)
    {
        _client = client;
    }

    public void RegisterEvents()
    {
        _client.Ready += OnReadyEventHandler;
        _client.Log += OnLogEventHandler;
        _client.MessageReceived += MessageEvent.MessageChecker;
    }

    public void UnregisterEvents()
    {
        _client.Ready -= OnReadyEventHandler;
        _client.Log -= OnLogEventHandler;
        _client.MessageReceived -= MessageEvent.MessageChecker;
    }

    private async Task OnReadyEventHandler()
    {
        await ReadyEvent.OnClientReady(_client);
        await StatusEvent.StartMonitoring(_client);
    }

    private Task OnLogEventHandler(LogMessage log)
    {
        return log.Severity switch
        {
            LogSeverity.Critical => Log.Error(log.Message),
            LogSeverity.Error => Log.Error(log.Message),
            LogSeverity.Warning => Log.Warn(log.Message),
            LogSeverity.Info => Log.Info(log.Message),
            LogSeverity.Debug => Log.Debug(log.Message),
            _ => Log.Info(log.Message)
        };
    }
}