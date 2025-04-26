using Discord.WebSocket;
using Serilog;
using Wondie_CSharp.Events.Actions;

namespace Wondie_CSharp.Events;

/// <summary>
/// Handles the registration and unregistration of Discord client events.
/// This class centralizes event handling logic to improve maintainability and modularity.
/// </summary>
public class EventHandler(DiscordSocketClient client)
{
    /// <summary>
    /// Registers the necessary events for the Discord client, such as handling logs, client readiness, and message receipt.
    /// </summary>
    public void RegisterEvents()
    {
        client.Ready += OnReadyEventHandler;
        client.MessageReceived += OnMessageEventHandler;
    }

    /// <summary>
    /// Unregisters all previously registered events to allow for a graceful shutdown or cleanup.
    /// </summary>
    public void UnregisterEvents()
    {
        client.Ready -= OnReadyEventHandler;
        client.MessageReceived -= OnMessageEventHandler;
    }

    /// <summary>
    /// Handles messages received in Discord channels. 
    /// Currently logs the content of the message.
    /// </summary>
    /// <param name="message">The message object received from Discord.</param>
    private Task OnMessageEventHandler(SocketMessage message)
    {
        Log.Information(message.Content);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Fired when the Discord client is marked as "ready."
    /// This event sets up additional services like monitoring and logs client readiness.
    /// </summary>
    private async Task OnReadyEventHandler()
    {
        await ReadyEvent.OnClientReady(client);
        await StatusEvent.StartMonitoring(client);
    }
}