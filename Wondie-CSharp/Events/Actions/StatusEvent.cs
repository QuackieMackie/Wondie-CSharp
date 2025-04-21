using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Discord;
using Discord.WebSocket;
using Wondie_CSharp.Events.Models;
using Wondie_CSharp.utils;

namespace Wondie_CSharp.Events.Actions;

public class StatusEvent
{
    private static readonly Dictionary<string, MonitorTarget> Targets = new()
    {
        //{ "Title", new MonitorTarget("IpHere", MonitorType.Ping) },
        //{ "Minecraft Server", new MonitorTarget("IpHere:25565", MonitorType.Tcp) },
        //{ "Title", new MonitorTarget("example.com", MonitorType.Dns) },
        //{ "Spotify", new MonitorTarget("Spotify", MonitorType.Process) }
    };

    private static IUserMessage? _statusMessage;
    private const ulong StatusChannelId = 1363956343924457663;
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromMinutes(1);

    public static async Task StartMonitoring(DiscordSocketClient client)
    {
        var channel = client.GetChannel(StatusChannelId) as ITextChannel;
        if (channel == null)
        {
            await Log.Error("Status channel not found!");
            return;
        }

        var messages = await channel.GetMessagesAsync(1).FlattenAsync();
        _statusMessage = messages.FirstOrDefault() as IUserMessage;
        
        if (_statusMessage == null)
        {
            _statusMessage = await channel.SendMessageAsync(embed: CreateStatusEmbed());
        }
        
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await UpdateStatus();
                await Task.Delay(UpdateInterval);
            }
        });
    }

    private static async Task UpdateStatus()
    {
        if (_statusMessage == null) return;
        
        await UpdateTargetStatuses();
        await _statusMessage.ModifyAsync(msg => msg.Embed = CreateStatusEmbed());
    }

    private static async Task UpdateTargetStatuses()
    {
        foreach (var target in Targets.Values)
        {
            target.LastCheckTime = DateTime.UtcNow;
            target.IsOnline = await CheckTarget(target);
        }
    }

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

    private static Embed CreateStatusEmbed()
    {
        var embed = new EmbedBuilder()
            .WithTitle("QuackieMackie's Status Dashboard")
            .WithDescription("Provides Real-time status of services.")
            .WithColor(Color.Blue);

        foreach (var (name, target) in Targets)
        {
            var status = target.IsOnline ? "✅ Online" : "❌ Offline";
            embed.AddField(name, $"{status}\n\nLast checked: <t:{target.LastCheckTime.ToUnixTimeSeconds()}:R>");
        }

        return embed.Build();
    }
}