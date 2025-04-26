using Discord;
using Discord.WebSocket;
using Wondie_CSharp.Commands.Models;
using System.Reflection;
using Serilog;

namespace Wondie_CSharp.Commands;

/// <summary>
/// The CommandHandler class is responsible for managing and handling slash commands for the Discord bot.
/// It discovers, registers, executes, and unregisters commands as needed.
/// </summary>
public class CommandHandler
{
    private static DiscordSocketClient _client = null!;
    private static readonly Dictionary<string, ISlashCommand> Commands = new();

    /// <summary>
    /// A set of allowed channel IDs where slash commands can be executed.
    /// </summary>
    private static readonly HashSet<ulong> AllowedChannelIds = new()
    {
        1365747310641811487, // #General/bot-commands
        1365751028120555622  // #Dev/bot-commands
    };
    
    /// <summary>
    /// Registers all the slash commands for the bot and initialises the CommandHandler with the given Discord client.
    /// </summary>
    /// <param name="client">The <see cref="DiscordSocketClient"/> to register commands for.</param>
    public static async Task RegisterCommands(DiscordSocketClient client)
    {
        _client = client;

        if (_client.ConnectionState != ConnectionState.Connected)
        {
            var readyTask = new TaskCompletionSource<bool>();
            _client.Ready += () =>
            {
                readyTask.SetResult(true);
                return Task.CompletedTask;
            };
            await readyTask.Task;
        }

        DiscoverCommands();

        try
        {
            foreach (var command in Commands.Values)
            {
                await _client.CreateGlobalApplicationCommandAsync(command.BuildCommand());
            }

            _client.SlashCommandExecuted += HandleSlashCommand;

            Log.Information($"Successfully registered {Commands.Count} commands.");
        }
        catch (Exception ex)
        {
            Log.Error($"Error registering commands: {ex.Message}");
        }
    }

    /// <summary>
    /// Discovers all classes that implement the <see cref="ISlashCommand"/> interface
    /// and adds them to the Commands dictionary.
    /// </summary>
    private static void DiscoverCommands()
    {
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => typeof(ISlashCommand).IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false });

        foreach (var type in commandTypes)
        {
            if (Activator.CreateInstance(type) is not ISlashCommand command) continue;
            Commands[command.Name] = command;
            Log.Debug($"Discovered command: {command.Name}");
        }
    }

    /// <summary>
    /// Handles the execution of slash commands when triggered by users.
    /// Verifies the command is valid and executes it in the allowed channels.
    /// </summary>
    /// <param name="command">The <see cref="SocketSlashCommand"/> instance representing the command invocation.</param>
    private static async Task HandleSlashCommand(SocketSlashCommand command)
    {
        try
        {
            if (!AllowedChannelIds.Contains(command.Channel.Id))
            {
                await command.RespondAsync("This command cannot be used in this channel.", ephemeral: true);
                return;
            }

            if (Commands.TryGetValue(command.CommandName, out var slashCommand))
            {
                await slashCommand.ExecuteAsync(command);
            }
            else
            {
                await command.RespondAsync("Unknown command", ephemeral: true);
            }
        }
        catch (Exception ex)
        {
            await command.RespondAsync($"Error executing command: {ex.Message}", ephemeral: true);
        }
    }

    /// <summary>
    /// Unregisters all previously registered commands from the Discord client.
    /// This method also detaches the event handler for slash command execution.
    /// </summary>
    public static async Task UnregisterCommands()
    {
        try
        {
            var commands = await _client.GetGlobalApplicationCommandsAsync();

            foreach (var command in commands)
            {
                await command.DeleteAsync();
            }

            _client.SlashCommandExecuted -= HandleSlashCommand;
            Commands.Clear();

            Log.Information($"Unregistered {commands.Count} commands.");
        }
        catch (Exception ex)
        {
            Log.Error($"Error unregistering commands: {ex.Message}");
        }
    }
}