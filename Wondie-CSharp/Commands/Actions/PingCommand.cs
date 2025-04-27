using Discord;
using Discord.WebSocket;
using Wondie_CSharp.Commands.Models;

namespace Wondie_CSharp.Commands.Actions;

/// <summary>
/// Represents a slash command that responds with "Pong!" when executed.
/// Implements the <see cref="ISlashCommand"/> interface to define its structure,
/// behaviour, and execution logic specifically for the "ping" command.
/// </summary>
public class PingCommand : ISlashCommand
{
    public string Name => "ping";
    public string Description => "Replies with Pong!";

    public SlashCommandProperties BuildCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription(Description)
            .Build();
    }

    /// <summary>
    /// Executes the slash command asynchronously. This method handles the command logic
    /// for a given <see cref="SocketSlashCommand"/> instance.
    /// </summary>
    /// <param name="command">The <see cref="SocketSlashCommand"/> instance containing the context of the trigger.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExecuteAsync(SocketSlashCommand command)
    {
        await command.RespondAsync("Pong!");
    }
}