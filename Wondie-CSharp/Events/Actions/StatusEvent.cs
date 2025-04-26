using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Discord;
using Discord.WebSocket;
using Serilog;
using Wondie_CSharp.Events.Models;

namespace Wondie_CSharp.Events.Actions;

/// <summary>
/// Provides functionality to monitor the status of various services and update a status dashboard on Discord.
/// </summary>
public static class StatusEvent
{
    /// <summary>
    /// Dictionary containing the targets to monitor, with their names as keys and <see cref="MonitorTarget"/> objects as values.
    /// </summary>
    private static readonly Dictionary<string, MonitorTarget> Targets = new()
    {
        { "Sylphian Proxy", new MonitorTarget("sylphian-proxy:25565", MonitorType.Minecraft) },
        { "Sylphian Hub", new MonitorTarget("sylphian-hub:25565", MonitorType.Minecraft) },
        { "Sylphian Survival", new MonitorTarget("sylphian-survival:25565", MonitorType.Minecraft) },
    };

    private static IUserMessage? _statusMessage;
    private const ulong StatusChannelId = 1363956343924457663;
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Starts monitoring the status of targets and periodically updates the Discord status channel.
    /// </summary>
    /// <param name="client">The DiscordSocketClient instance to interact with Discord.</param>
    public static async Task StartMonitoring(DiscordSocketClient client)
    {
        if (client.GetChannel(StatusChannelId) is not ITextChannel channel)
        {
            Log.Error("Status channel not found!");
            return;
        }

        var messages = await channel.GetMessagesAsync(1).FlattenAsync();
        _statusMessage = messages.FirstOrDefault() as IUserMessage ?? await channel.SendMessageAsync(embed: CreateStatusEmbed());

        _ = Task.Run(async () =>
        {
            while (true)
            {
                await UpdateStatus();
                await Task.Delay(UpdateInterval);
            }
        });
    }

    /// <summary>
    /// Updates the Discord status message with the latest information.
    /// </summary>
    private static async Task UpdateStatus()
    {
        if (_statusMessage == null) return;

        await UpdateTargetStatuses();
        await _statusMessage.ModifyAsync(msg => msg.Embed = CreateStatusEmbed());
    }

    /// <summary>
    /// Updates the "IsOnline" state of all defined monitor targets by performing relevant checks.
    /// </summary>
    private static async Task UpdateTargetStatuses()
    {
        foreach (var target in Targets.Values)
        {
            var isOnline = await CheckTarget(target);
            target.IsOnline = isOnline;
        }
    }

    /// <summary>
    /// Checks whether a given monitor target is online, based on its type.
    /// </summary>
    /// <param name="target">The <see cref="MonitorTarget"/> to check.</param>
    /// <returns>A boolean indicating whether the target is online.</returns>
    private static async Task<bool> CheckTarget(MonitorTarget target)
    {
        try
        {
            switch (target.Type)
            {
                case MonitorType.Ping:
                    using (var ping = new Ping())
                    {
                        var reply = await ping.SendPingAsync(target.Address, 1000);
                        return reply.Status == IPStatus.Success;
                    }

                case MonitorType.Http:
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(target.Address);
                        return response.IsSuccessStatusCode;
                    }

                case MonitorType.Tcp:
                    using (var client = new TcpClient())
                    {
                        var parts = target.Address.Split(':');
                        if (parts.Length != 2) return false;

                        await client.ConnectAsync(parts[0], int.Parse(parts[1]));
                        return client.Connected;
                    }

                case MonitorType.Minecraft:
                    using (var client = new TcpClient())
                    {
                        var parts = target.Address.Split(':');
                        if (parts.Length != 2) return false;

                        var host = parts[0];
                        var port = int.Parse(parts[1]);

                        await client.ConnectAsync(host, port);
                        return client.Connected;
                    }

                case MonitorType.Dns:
                    var result = await Dns.GetHostEntryAsync(target.Address);
                    return result.AddressList.Length > 0;

                case MonitorType.Process:
                    return Process.GetProcessesByName(target.Address).Length > 0;

                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates an embed representing the current status of all monitored targets.
    /// </summary>
    /// <returns>A Discord <see cref="Embed"/> object representing the status dashboard.</returns>
    private static Embed CreateStatusEmbed()
    {
        var now = DateTimeOffset.UtcNow;
        var embed = new EmbedBuilder()
            .WithTitle("QuackieMackie's Status Dashboard")
            .WithDescription($"Provides real-time status of services.\n\n**Last checked:** <t:{now.AddSeconds(-30).ToUnixTimeSeconds()}:R>\n**Next checked:** <t:{now.AddSeconds(30).ToUnixTimeSeconds()}:R>")
            .WithColor(Color.Blue);

        embed.AddField("Am I Online?", $"✅ **Yes, I am!** (as of <t:{now.AddSeconds(-30).ToUnixTimeSeconds()}:R>)");

        foreach (var (name, target) in Targets)
        {
            var status = target.IsOnline ? "✅ Online" : "❌ Offline";
            embed.AddField(name, status);
        }

        return embed.Build();
    }
}