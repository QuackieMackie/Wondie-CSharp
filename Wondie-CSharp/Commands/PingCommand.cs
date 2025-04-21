using Discord.WebSocket;

namespace Wondie_CSharp.Commands;

public class PingCommand
{
    public static string Name => "ping";
    public static string Description => "Replies with pong!";

    public static async Task HandleAsync(SocketSlashCommand command)
    {
        await command.RespondAsync("Pong! 🏓");
    }
}