using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Serilog;
using Wondie_CSharp.Events.Actions;

namespace Wondie_CSharp.Events;

/// <summary>
/// Handles the registration and unregistration of Discord client events.
/// This class centralises event handling logic to improve maintainability and modularity.
/// </summary>
public class EventHandler
{
    private readonly DiscordSocketClient _client;

    /// <summary>
    /// Initializes the EventHandler with the provided DiscordSocketClient.
    /// </summary>
    public EventHandler(DiscordSocketClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Registers the necessary events for the Discord client.
    /// </summary>
    public void RegisterEvents()
    {
        _client.Ready += OnReadyEventHandler;
        _client.ReactionAdded += RuleComplianceEvent.HandleReactionAdded;
        _client.ReactionRemoved += RuleComplianceEvent.HandleReactionRemoved;

        Log.Information("Events registered successfully.");
    }

    /// <summary>
    /// Unregisters all previously registered events.
    /// </summary>
    public void UnregisterEvents()
    {
        _client.Ready -= OnReadyEventHandler;
        _client.ReactionAdded -= RuleComplianceEvent.HandleReactionAdded;
        _client.ReactionRemoved -= RuleComplianceEvent.HandleReactionRemoved;

        Log.Information("Events unregistered successfully.");
    }

    /// <summary>
    /// Fires when the Discord client is "ready."
    /// </summary>
    private async Task OnReadyEventHandler()
    {
        await ReadyEvent.OnClientReady(_client);
        await StatusEvent.StartMonitoring(_client, new LoggerFactory().CreateLogger("StatusEvent"));
    }
}