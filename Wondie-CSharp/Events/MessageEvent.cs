using Discord.WebSocket;
using Wondie_CSharp.utils;

namespace Wondie_CSharp.Events;

public class MessageEvent
{
    public static async Task MessageLogger(SocketMessage message)
    {
        if (message.Author.IsBot) return;
        
        if (message.Channel is SocketGuildChannel guildChannel)
        {
            await Log.Info($"[{guildChannel.Guild.Name}#{guildChannel.Name}] {message.Author.Username}: {message.Content}");
        }
    }
}