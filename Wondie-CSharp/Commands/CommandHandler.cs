using Discord;
using Discord.WebSocket;
using Wondie_CSharp.Commands.Models;
using Wondie_CSharp.utils;
using System.Reflection;

namespace Wondie_CSharp.Commands;

public class CommandHandler
{
    private static DiscordSocketClient _client;
    private static readonly Dictionary<string, ISlashCommand> Commands = new();

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

            await Log.Info($"Successfully registered {Commands.Count} commands.");
        }
        catch (Exception ex)
        {
            await Log.Error($"Error registering commands: {ex.Message}");
        }
    }

    private static void DiscoverCommands()
    {
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => typeof(ISlashCommand).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

        foreach (var type in commandTypes)
        {
            if (Activator.CreateInstance(type) is ISlashCommand command)
            {
                Commands[command.Name] = command;
                Log.Debug($"Discovered command: {command.Name}");
            }
        }
    }

    private static async Task HandleSlashCommand(SocketSlashCommand command)
    {
        try
        {
            if (Commands.TryGetValue(command.CommandName, out var slashCommand))
            {
                await slashCommand.ExecuteAsync(command);
            }
            else
            {
                await command.RespondAsync("Unknown command");
            }
        }
        catch (Exception ex)
        {
            await command.RespondAsync($"Error executing command: {ex.Message}");
        }
    }

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
            await Log.Info($"Unregistered {commands.Count} commands.");
        }
        catch (Exception ex)
        {
            await Log.Error($"Error unregistering commands: {ex.Message}");
        }
    }
}