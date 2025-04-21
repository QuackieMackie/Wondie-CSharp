using Discord.WebSocket;
using Wondie_CSharp.utils;

namespace Wondie_CSharp.Events.Actions;

public class ReadyEvent
{
    public static async Task OnClientReady(DiscordSocketClient client)
    {
        if (client.CurrentUser != null)
        {
            await Log.Info($"Logged in as {client.CurrentUser.Username}");
        }
        else
        {
            await Log.Warn("Client is ready, but CurrentUser is null.");
        }

        foreach (var guild in client.Guilds)
        {
            await Log.Info($"Connected to guild: {guild.Name}");
        }

        await Log.Info($"Connected to {client.Guilds.Count} guild(s).");
    }
}