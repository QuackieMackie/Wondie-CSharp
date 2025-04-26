using Discord;
using Discord.WebSocket;
using Wondie_CSharp.Commands.Models;

namespace Wondie_CSharp.Commands.Actions;

/// <summary>
/// Represents a slash command that calculates the number of steps required to reach 1
/// when running a given positive integer through the Collatz conjecture.
/// </summary>
public class CollatzCommand : ISlashCommand
{
    public string Name => "collatz";
    public string Description => "Run a number through the Collatz conjecture";
    
    public SlashCommandProperties BuildCommand()
    {
        return new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription(Description)
            .AddOption("number", ApplicationCommandOptionType.Integer, "The number to run through Collatz conjecture", isRequired: true)
            .Build();
    }

    /// <summary>
    /// Executes the logic for the Collatz conjecture command when triggered by a user.
    /// Responds with the number of steps it took to reach 1.
    /// </summary>
    /// <param name="command">The <see cref="SocketSlashCommand"/> containing the user's input.</param>
    public async Task ExecuteAsync(SocketSlashCommand command)
    {
        var numberOption = command.Data.Options.FirstOrDefault();
        if (numberOption == null)
        {
            await command.RespondAsync("Please provide a number.", ephemeral: true);
            return;
        }

        var number = (long)numberOption.Value;
        
        if (number <= 0)
        {
            await command.RespondAsync("Please provide a positive integer.", ephemeral: true);
            return;
        }
        
        long steps = 0;
        var originalNumber = number;

        while (number != 1)
        {
            if (number % 2 == 0)
            {
                number /= 2; // If even, divide by 2.
            }
            else
            {
                number = 3 * number + 1; // If odd, multiply by 3 and add 1.
            }
            steps++;
        }
        
        await command.RespondAsync($"Run {originalNumber} through the Collatz conjecture, {steps} times, and it has reached 1.");
    }
}