using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Prometheus.DotNetRuntime;

namespace Wondie_CSharp.Utils;

/// <summary>
/// The MetricsService class provides functionality for tracking and exposing application metrics
/// such as uptime, memory usage, network activity, and more via Prometheus.
/// </summary>
public static class MetricsService
{
    // Prometheus metrics definitions
    private static readonly Gauge GuildCountGauge = Metrics.CreateGauge("discord_guild_count", "Number of Discord guilds connected");
    private static readonly Gauge UptimeGauge = Metrics.CreateGauge("bot_uptime_seconds", "Bot uptime in seconds");
    private static readonly Gauge WorkingSetGauge = Metrics.CreateGauge("process_working_set_bytes", "Amount of physical memory allocated for the process");
    private static readonly Gauge PrivateMemoryGauge = Metrics.CreateGauge("process_private_memory_bytes", "Private memory size of the process");
    private static readonly Gauge CpuUsageGauge = Metrics.CreateGauge("process_cpu_percent", "Approximate CPU usage percentage for the process");
    private static readonly Gauge NetworkBytesSent = Metrics.CreateGauge("network_bytes_sent_total", "Total bytes sent across all network interfaces");
    private static readonly Gauge NetworkBytesReceived = Metrics.CreateGauge("network_bytes_received_total", "Total bytes received across all network interfaces");

    // Stopwatch to track bot uptime
    private static Stopwatch _uptimeStopwatch = new();

    // Task and cancellation management for the metrics server
    private static Task? _metricsTask;
    private static CancellationTokenSource? _metricsCts;

    // Fields for tracking CPU usage
    private static TimeSpan _previousCpuTime;
    private static DateTime _lastCpuMeasurement;

    /// <summary>
    /// Starts the Prometheus metrics server and begins collecting metrics.
    /// </summary>
    public static void StartMetricsServer()
    {
        if (_metricsTask != null) return;

        _metricsCts = new CancellationTokenSource();
        var cancellationToken = _metricsCts.Token;

        _metricsTask = Task.Run(() =>
        {
            // Initialize ASP.NET Core for serving metrics
            var metricsServer = WebApplication.CreateBuilder();
            metricsServer.Services.AddHealthChecks();
            var app = metricsServer.Build();

            // Start dotnet runtime metrics collection
            DotNetRuntimeStatsBuilder.Default().StartCollecting();
            app.UseHttpMetrics();

            // Expose metrics endpoint
            app.MapMetrics(pattern: "/wondie/metrics");

            _uptimeStopwatch.Start();

            // Start monitoring loop
            _ = Task.Run(async () =>
            {
                var process = Process.GetCurrentProcess();
                _previousCpuTime = process.TotalProcessorTime;
                _lastCpuMeasurement = DateTime.UtcNow;

                while (!cancellationToken.IsCancellationRequested)
                {
                    UpdateUptime();
                    UpdateMemoryMetrics(process);
                    UpdateCpuUsage(process);
                    UpdateNetworkMetrics();

                    await Task.Delay(5000, cancellationToken);
                }
            }, cancellationToken);

            app.Run();
            return Task.CompletedTask;
        }, cancellationToken);
    }

    /// <summary>
    /// Stops the Prometheus metrics server and cancels any ongoing monitoring tasks.
    /// </summary>
    public static void StopMetricsServer()
    {
        if (_metricsCts == null) return;

        _metricsCts.Cancel(); // Cancel the task
        _metricsCts.Dispose(); // Dispose of the cancellation token source
        _metricsCts = null;
        _metricsTask = null;
    }

    /// <summary>
    /// Updates the current count of connected Discord guilds.
    /// </summary>
    /// <param name="count">The number of guilds connected to the bot.</param>
    public static void UpdateGuildCount(int count)
    {
        GuildCountGauge.Set(count);
    }

    /// <summary>
    /// Updates the bot's uptime metric based on the stopwatch.
    /// </summary>
    private static void UpdateUptime()
    {
        UptimeGauge.Set(_uptimeStopwatch.Elapsed.TotalSeconds);
    }

    /// <summary>
    /// Updates memory usage metrics including working set and private memory size.
    /// </summary>
    /// <param name="process">The current process to retrieve memory metrics from.</param>
    private static void UpdateMemoryMetrics(Process process)
    {
        process.Refresh();
        WorkingSetGauge.Set(process.WorkingSet64);
        PrivateMemoryGauge.Set(process.PrivateMemorySize64);
    }

    /// <summary>
    /// Updates CPU usage as a percentage based on the elapsed CPU and wall-clock time.
    /// </summary>
    /// <param name="process">The current process to retrieve CPU metrics from.</param>
    private static void UpdateCpuUsage(Process process)
    {
        var currentCpuTime = process.TotalProcessorTime;
        var currentTime = DateTime.UtcNow;

        var elapsedCpuTime = (currentCpuTime - _previousCpuTime).TotalMilliseconds;
        var elapsedTime = (currentTime - _lastCpuMeasurement).TotalMilliseconds;

        if (elapsedTime > 0)
        {
            var cpuUsagePercentage = (elapsedCpuTime / (Environment.ProcessorCount * elapsedTime)) * 100.0;
            CpuUsageGauge.Set(cpuUsagePercentage);
        }

        _previousCpuTime = currentCpuTime;
        _lastCpuMeasurement = currentTime;
    }

    /// <summary>
    /// Updates network metrics such as total bytes sent and received across all network interfaces.
    /// </summary>
    private static void UpdateNetworkMetrics()
    {
        long totalSent = 0;
        long totalReceived = 0;

        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            var stats = ni.GetIPv4Statistics();
            totalSent += stats.BytesSent;
            totalReceived += stats.BytesReceived;
        }

        NetworkBytesSent.Set(totalSent);
        NetworkBytesReceived.Set(totalReceived);
    }
}