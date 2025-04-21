using Discord;
using System.Diagnostics;

namespace Wondie_CSharp.utils;

public class SystemMonitor
{
    private static readonly OperatingSystem Os = Environment.OSVersion;
    private static readonly Process CurrentProcess = Process.GetCurrentProcess();
    private static (long Idle, long Total) _lastCpuTime;
    
    static SystemMonitor()
    {
        _lastCpuTime = GetLinuxCpuUsage();
    }

    private static (long Idle, long Total) GetLinuxCpuUsage()
    {
        var lines = File.ReadAllLines("/proc/stat");
        var cpuLine = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var idle = long.Parse(cpuLine[4]);
        var total = cpuLine.Skip(1).Select(long.Parse).Sum();
        
        return (idle, total);
    }

    private static (double Total, double Used) GetLinuxMemoryInfo()
    {
        var lines = File.ReadAllLines("/proc/meminfo");
        var total = 0.0;
        var free = 0.0;
        var cached = 0.0;
        var buffers = 0.0;

        foreach (var line in lines)
        {
            if (line.StartsWith("MemTotal:"))
                total = double.Parse(line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
            else if (line.StartsWith("MemFree:"))
                free = double.Parse(line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
            else if (line.StartsWith("Cached:"))
                cached = double.Parse(line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
            else if (line.StartsWith("Buffers:"))
                buffers = double.Parse(line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
        }

        var used = total - free - cached - buffers;
        return (total, used);
    }

    public static Embed GetSystemReport()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        
        var (lastIdle, lastTotal) = _lastCpuTime;
        var (currentIdle, currentTotal) = GetLinuxCpuUsage();
        
        var idleDelta = currentIdle - lastIdle;
        var totalDelta = currentTotal - lastTotal;
        var cpuUsage = 100.0 * (1.0 - (double)idleDelta / totalDelta);
        
        _lastCpuTime = (currentIdle, currentTotal);

        var (totalMem, usedMem) = GetLinuxMemoryInfo();
        var memoryUsagePercent = (usedMem / totalMem) * 100;

        var embed = new EmbedBuilder()
            .WithAuthor("Wondie's Report", "https://avatars.githubusercontent.com/u/45337741?v=4", "https://github.com/QuackieMackie")
            .WithTitle("System Monitoring Dashboard")
            .WithDescription("Latest system statistics report, taken at `unixtime stamp`.\n\n" +
                             "**System Metrics**\n" +
                             $"**CPU:** {cpuUsage:F2}%\n" +
                             $"**Memory:** {memoryUsagePercent:F2}%\n" +
                             "**Disk (C:)** 00.00% \n" +
                             "**Network I/O** Sent: 00000.0 MB, Received: 00000.0 MB\n\n" +
                             "**Active Logged-in User(s)**")
            .WithColor(Color.DarkPurple)
            .WithFooter(footer => footer.Text = $"I have been on `{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s` now.")
            .Build();

        return embed;
    }
}