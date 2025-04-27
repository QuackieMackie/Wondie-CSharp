using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Wondie_CSharp.Events.Models;

namespace Wondie_CSharp.Events.Actions;

public static class StatusEvent
{
    private static readonly Dictionary<string, MonitorTarget> Targets = new()
    {
        { "Sylphian Proxy", new MonitorTarget("mc.sylphian.net", "sylphian-proxy:25565", MonitorType.Tcp) },
        { "Sylphian Hub", new MonitorTarget("mc.sylphian.net", "sylphian-hub:25565", MonitorType.Tcp) },
        { "Sylphian Survival", new MonitorTarget("mc.sylphian.net", "sylphian-survival:25565", MonitorType.Tcp) },
    };

    private static readonly Dictionary<ulong, IUserMessage> StatusMessages = new();
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromMinutes(1);
    private static CancellationTokenSource _monitoringTokenSource = new();

    /// <summary>
    /// Starts monitoring the targets and posting the embed to all "server-status" channels.
    /// </summary>
    public static async Task StartMonitoring(DiscordSocketClient client, ILogger logger)
    {
        logger.LogInformation("Starting the StatusEvent monitoring loop...");

        _monitoringTokenSource = new CancellationTokenSource();

        // Start the monitoring loop
        _ = Task.Run(async () =>
        {
            while (!_monitoringTokenSource.Token.IsCancellationRequested)
            {
                await EnsureStatusChannels(client, logger);
                await UpdateStatus(logger);
                logger.LogInformation("Status check and update cycle completed.");
                await Task.Delay(UpdateInterval, _monitoringTokenSource.Token);
            }
        }, _monitoringTokenSource.Token);
    }

    /// <summary>
    /// Stops monitoring and cancels the monitoring loop.
    /// </summary>
    public static void StopMonitoring(ILogger logger)
    {
        logger.LogInformation("Stopping the StatusEvent monitoring loop...");
        _monitoringTokenSource.Cancel();
    }

    /// <summary>
    /// Ensures that all guilds with a "server-status" channel have a corresponding status message.
    /// </summary>
    private static async Task EnsureStatusChannels(DiscordSocketClient client, ILogger logger)
    {
        foreach (var guild in client.Guilds)
        {
            try
            {
                var channel = guild.TextChannels.FirstOrDefault(c => c.Name == "server-status");
                if (channel is ITextChannel textChannel)
                {
                    // Get or send the initial status message
                    if (!StatusMessages.ContainsKey(guild.Id))
                    {
                        var messages = await textChannel.GetMessagesAsync(1).FlattenAsync();
                        var statusMessage = messages.FirstOrDefault() as IUserMessage ?? await textChannel.SendMessageAsync(embed: CreateStatusEmbed());

                        StatusMessages[guild.Id] = statusMessage;
                        logger.LogInformation($"Initialized status message for guild '{guild.Name}' in channel '{textChannel.Name}'.");
                    }
                }
                else
                {
                    // Remove guild from tracked status messages if the channel no longer exists
                    if (StatusMessages.Remove(guild.Id))
                    {
                        logger.LogWarning($"Removed tracked status for guild '{guild.Name}' because no 'server-status' channel was found.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error ensuring status channel for guild '{guild.Name}'.");
            }
        }
    }

    /// <summary>
    /// Updates the status messages across all guilds with a "server-status" channel.
    /// </summary>
    private static async Task UpdateStatus(ILogger logger)
    {
        foreach (var (guildId, message) in StatusMessages)
        {
            try
            {
                await UpdateTargetStatuses(logger);
                await message.ModifyAsync(msg => msg.Embed = CreateStatusEmbed());
                logger.LogInformation($"Updated status message for guild ID: {guildId}.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to update status message for guild ID: {guildId}.");
            }
        }
    }

    /// <summary>
    /// Checks all monitor targets and updates their online status.
    /// </summary>
    private static async Task UpdateTargetStatuses(ILogger logger)
    {
        foreach (var target in Targets.Values)
        {
            try
            {
                target.IsOnline = await CheckTarget(target);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking target {TargetPublicAddress}", target.PublicAddress);
                target.IsOnline = false;
            }
        }
    }

    /// <summary>
    /// Performs the target-specific status check.
    /// </summary>
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
            .WithTitle("Status Dashboard")
            .WithDescription($"Provides real-time status of services.\n\n" +
                             $"**Last checked:** <t:{now.AddSeconds(-30).ToUnixTimeSeconds()}:R>\n" +
                             $"**Next check:** <t:{now.AddSeconds(30).ToUnixTimeSeconds()}:R>")
            .WithColor(Color.Blue);

        embed.AddField("🟢 **Am I Online?**", $"Yes, I'm **online**! (as of <t:{now.AddSeconds(-30).ToUnixTimeSeconds()}:R>)");

        foreach (var (name, target) in Targets)
        {
            var status = target.IsOnline ? "🟢 **Online**" : "🔴 **Offline**";
            var publicAddress = target.PublicAddress;

            embed.AddField(
                $"**{name}**",
                $"{status}\n" +
                $"To connect: `{publicAddress}`\n"
            );
        }

        return embed.Build();
    }

    /// <summary>
    /// Updates all "server-status" channels to indicate that the bot is offline.
    /// </summary>
    public static async Task SetBotStatusToOffline(DiscordSocketClient client)
    {
        var now = DateTimeOffset.UtcNow;

        var offlineEmbed = new EmbedBuilder()
            .WithTitle("Status Dashboard")
            .WithDescription($"Provides real-time status of services.")
            .WithColor(Color.Red)
            .AddField("🔴 **Am I Online?**", $"No! I'm **offline**! (as of <t:{now.AddSeconds(-30).ToUnixTimeSeconds()}:R>)")
            .Build();

        foreach (var guild in client.Guilds)
        {
            var channel = guild.TextChannels.FirstOrDefault(c => c.Name == "server-status");
            if (channel is ITextChannel textChannel)
            {
                try
                {
                    var messages = await textChannel.GetMessagesAsync(1).FlattenAsync();
                    var statusMessage = messages.FirstOrDefault() as IUserMessage ?? await textChannel.SendMessageAsync(embed: offlineEmbed);

                    await statusMessage.ModifyAsync(msg => msg.Embed = offlineEmbed);
                }
                catch
                {
                    // Ignore exceptions in shutdown
                }
            }
        }
    }
}