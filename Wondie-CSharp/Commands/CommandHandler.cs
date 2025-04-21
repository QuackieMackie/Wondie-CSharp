using Discord;
using Discord.WebSocket;
using Wondie_CSharp.utils;

namespace Wondie_CSharp.Commands;

public class CommandHandler
{
    private static DiscordSocketClient _client;

    public static async Task RegisterCommands(DiscordSocketClient client)
    {
        _client = client;

        if (_client.ConnectionState != ConnectionState.Connected)
        {
            var ready = new TaskCompletionSource<bool>();
            _client.Ready += () =>
            {
                ready.SetResult(true);
                return Task.CompletedTask;
            };
            await ready.Task;
        }

        var pingCommand = new SlashCommandBuilder()
            .WithName(PingCommand.Name)
            .WithDescription(PingCommand.Description)
            .Build();

        var calculateCommand = CalculateCommand.Build();
        
        var systemReport = new SlashCommandBuilder()
            .WithName(SystemReport.Name)
            .WithDescription(SystemReport.Description)
            .Build();

        try
        {
            await _client.CreateGlobalApplicationCommandAsync(pingCommand);
            await _client.CreateGlobalApplicationCommandAsync(calculateCommand);
            await _client.CreateGlobalApplicationCommandAsync(systemReport);

            _client.SlashCommandExecuted += HandleSlashCommand;
            
            await Log.Info("Successfully registered commands.");
        }
        catch (Exception ex)
        {
            await Log.Error($"Error registering commands: {ex.Message}");
        }
    }

    private static async Task HandleSlashCommand(SocketSlashCommand command)
    {
        try
        {
            switch (command.CommandName)
            {
                case "ping":
                    await PingCommand.HandleAsync(command);
                    break;
                case "calculate":
                    await CalculateCommand.HandleAsync(command);
                    break;
                case "report":
                    await SystemReport.HandleAsync(command);
                    break;
                default:
                    await command.RespondAsync("Unknown command");
                    break;
            }
        }
        catch (Exception ex)
        {
            await command.RespondAsync($"Error executing command: {ex.Message}");
        }
    }

    public static async Task UnregisterCommands()
    {
        if (_client == null) return;

        try
        {
            var commands = await _client.GetGlobalApplicationCommandsAsync();

            foreach (var command in commands)
            {
                await command.DeleteAsync();
            }

            _client.SlashCommandExecuted -= HandleSlashCommand;
            await Log.Info($"Unregistered {commands.Count} commands.");
        }
        catch (Exception ex)
        {
            await Log.Error($"Error unregistering commands: {ex.Message}");
        }
    }
}