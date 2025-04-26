using Discord.WebSocket;
using Serilog;
using Wondie_CSharp.Utils;

namespace Wondie_CSharp.Events.Actions;

/// <summary>
/// Handles the Discord client's "Ready" event, which is triggered when the bot has connected and is fully operational.
/// </summary>
public class ReadyEvent
{
    /// <summary>
    /// Called when the Discord client is ready. Logs information about the client, connected guilds,
    /// and updates metrics for tracking the guild count.
    /// </summary>
    /// <param name="client">The <see cref="DiscordSocketClient"/> representing the connected Discord client.</param>
    /// <returns>A completed Task after the Ready processing is complete.</returns>
    public static Task OnClientReady(DiscordSocketClient client)
    {
        if (client.CurrentUser != null)
        {
            Log.Information($"Logged in as {client.CurrentUser.Username}");
        }
        else
        {
            Log.Warning("Client is ready, but CurrentUser is null.");
        }

        foreach (var guild in client.Guilds)
        {
            Log.Information($"Connected to guild: {guild.Name}");
        }
        
        MetricsService.UpdateGuildCount(client.Guilds.Count);

        Log.Information($"Connected to {client.Guilds.Count} guild(s).");
        
        return Task.CompletedTask;
    }
}