using Discord;
using Discord.WebSocket;

namespace Wondie_CSharp.Commands.Models;

public interface ISlashCommand
{
    string Name { get; }
    string Description { get; }
    SlashCommandProperties BuildCommand();
    Task ExecuteAsync(SocketSlashCommand command);
}