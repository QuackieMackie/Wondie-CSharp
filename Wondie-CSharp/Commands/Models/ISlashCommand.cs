using Discord;
using Discord.WebSocket;

namespace Wondie_CSharp.Commands.Models;

/// <summary>
/// Defines the structure of a slash command that can be implemented by specific command classes.
/// Provides properties and methods to describe, build, and execute the command.
/// </summary>
public interface ISlashCommand
{
    /// <summary>
    /// Gets the name of the slash command.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the slash command.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Builds the slash command for registration with Discord, specifying its structure.
    /// </summary>
    /// <returns>A <see cref="SlashCommandProperties"/> object describing the command.</returns>
    SlashCommandProperties BuildCommand();

    /// <summary>
    /// Executes the slash command using the provided <see cref="SocketSlashCommand"/> data.
    /// </summary>
    /// <param name="command">The <see cref="SocketSlashCommand"/> containing the command's context and options.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ExecuteAsync(SocketSlashCommand command);
}