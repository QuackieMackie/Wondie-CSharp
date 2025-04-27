using System.Reflection;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Wondie_CSharp.Commands.Models;

namespace Wondie_CSharp.Commands;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly ILogger _logger;
    private readonly Dictionary<string, ISlashCommand> _commands = new();

    public CommandHandler(DiscordSocketClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <summary>
    /// Dynamically discovers and registers all commands implementing <see cref="ISlashCommand"/>.
    /// </summary>
    public async Task RegisterCommandsAsync()
    {
        DiscoverCommands();

        try
        {
            foreach (var command in _commands.Values)
            {
                await _client.CreateGlobalApplicationCommandAsync(command.BuildCommand());
                _logger.LogInformation($"Registered command: {command.Name}");
            }

            _logger.LogInformation("All slash commands registered globally!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering slash commands.");
        }
    }

    /// <summary>
    /// Handles dynamic execution of slash commands.
    /// </summary>
    /// <param name="command">The command invoked by the user.</param>
    public async Task HandleSlashCommandAsync(SocketSlashCommand command)
    {
        if (_commands.TryGetValue(command.CommandName, out var slashCommand))
        {
            try
            {
                await slashCommand.ExecuteAsync(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing command: {command.CommandName}");
                await command.RespondAsync("An error occurred while executing this command.", ephemeral: true);
            }
        }
        else
        {
            _logger.LogWarning($"Unknown command: {command.CommandName}");
            await command.RespondAsync("Unknown command.", ephemeral: true);
        }
    }

    /// <summary>
    /// Discovers all classes that implement the <see cref="ISlashCommand"/> interface
    /// and adds them to the Commands dictionary.
    /// </summary>
    private void DiscoverCommands()
    {
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => typeof(ISlashCommand).IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false });

        foreach (var type in commandTypes)
        {
            if (Activator.CreateInstance(type) is not ISlashCommand command) continue;
            _commands[command.Name] = command;
            _logger.LogDebug($"Discovered command: {command.Name}");
        }
    }
}