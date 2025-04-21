using Discord;

namespace Wondie_CSharp.utils;

public class SystemMonitor
{
    public static Embed GetSystemReport()
    {
        Log.Info("Getting system report...");
        
        var osInfo = SystemUtils.GetOsInfo();
        var cpuInfo = SystemUtils.GetCpuInfo();
        var memoryInfo = SystemUtils.GetMemoryInfo();
        var uptime = SystemUtils.GeTimeSpan();
        
        var embed = new EmbedBuilder()
            .WithAuthor("Wondie's Report", "https://avatars.githubusercontent.com/u/45337741?v=4", "https://github.com/QuackieMackie")
            .WithTitle("System Monitoring Dashboard")
            .WithDescription($"Latest system statistics report, taken at <t:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:F>\n\n" +
                           "**System Metrics**\n" +
                           $"**OS:** {osInfo}\n" +
                           $"**CPU:** {cpuInfo:F2}%\n" +
                           $"**Memory:** {memoryInfo:F2}%\n")
            .WithColor(Color.DarkPurple)
            .WithFooter(footer => footer.Text = $"Uptime: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s")
            .Build();

        return embed;
    }
}