using Discord.WebSocket;
using Wondie_CSharp.utils;

namespace Wondie_CSharp.Events.Actions;

public class MessageEvent
{
    private static readonly ulong AuthorizedUserId = 779274101180727297;
    private const string ReportCommand = "report";
    
    public static async Task MessageChecker(SocketMessage message)
    {
        if (message.Author.IsBot) return;
        
        await ReportCommandEvent(message);
    }
    
    private static async Task ReportCommandEvent(SocketMessage message)
    {
        if (!message.Content.Trim().Equals(ReportCommand, StringComparison.OrdinalIgnoreCase))
            return;

        if (message.Channel is SocketDMChannel && message.Author.Id == AuthorizedUserId)
        {
            var embed = SystemMonitor.GetSystemReport();
            await message.Channel.SendMessageAsync(embed: embed);
            return;
        }

        if (!(message.Channel is SocketDMChannel))
        {
            await message.Channel.SendMessageAsync("This command can only be used in DMs.");
            return;
        }
        
        await message.Channel.SendMessageAsync("You are not authorized to use this command.");
    }
}