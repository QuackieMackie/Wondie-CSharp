using Discord;
using Discord.WebSocket;

namespace Wondie_CSharp.Commands;

public class CalculateCommand
{
    public static string Name => "calculate";
    public static string Description => "Performs basic arithmetic operations";

    public static SlashCommandProperties Build()
    {
        return new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription(Description)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("operation")
                .WithDescription("The operation to perform")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)
                .AddChoice("add", "add")
                .AddChoice("subtract", "subtract")
                .AddChoice("multiply", "multiply")
                .AddChoice("divide", "divide"))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("first")
                .WithDescription("First number")
                .WithType(ApplicationCommandOptionType.Number)
                .WithRequired(true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("second")
                .WithDescription("Second number")
                .WithType(ApplicationCommandOptionType.Number)
                .WithRequired(true))
            .Build();
    }

    public static async Task HandleAsync(SocketSlashCommand command)
    {
        var operation = command.Data.Options.First(x => x.Name == "operation").Value as string;
        var first = Convert.ToDouble(command.Data.Options.First(x => x.Name == "first").Value);
        var second = Convert.ToDouble(command.Data.Options.First(x => x.Name == "second").Value);

        double result = operation switch
        {
            "add" => first + second,
            "subtract" => first - second,
            "multiply" => first * second,
            "divide" => second != 0 ? first / second : double.NaN,
            _ => double.NaN
        };

        await command.RespondAsync($"Result: {result}");
    }
}