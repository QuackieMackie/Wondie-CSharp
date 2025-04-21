using Discord;
using Discord.WebSocket;
using Wondie_CSharp.utils;

namespace Wondie_CSharp.Events;

public class ReadyEvent
{
    public static async Task OnClientReady(DiscordSocketClient client, string token)
    {
        if (client.CurrentUser != null) 
            await Log.Info($"Logged in to account {client.CurrentUser.Username} using the provided token (partial) '{token.Substring(0, 10)}...'");
        else 
            await Log.Warn("Client is ready, but CurrentUser is still null.");

        var guilds = client.Guilds;

        foreach (var guild in guilds)
        {
            await Log.Info($"Connected to guild: {guild.Name}.");
        }

        await Log.Info($"Connected to {guilds.Count} guilds.");
        
        await client.SetActivityAsync(new CustomStatusGame($"{DateTime.Now:HH:mm:ss}"));
    }
}