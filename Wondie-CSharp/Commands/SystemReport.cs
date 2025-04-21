using Discord.WebSocket;
using Wondie_CSharp.utils;

namespace Wondie_CSharp.Commands;

public class SystemReport
{
    public static string Name => "report";
    public static string Description => "Gives a report of the current system's workings.";

    public static async Task HandleAsync(SocketSlashCommand command)
    {
        await command.RespondAsync(embed: SystemMonitor.GetSystemReport());
    }
}