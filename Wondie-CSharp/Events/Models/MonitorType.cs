namespace Wondie_CSharp.Events.Models;

/// <summary>
/// Represents different types of monitoring tasks that can be performed.
/// </summary>
public enum MonitorType
{
    /// <summary>
    /// Check if a host is reachable via ICMP.
    /// </summary>
    Ping,
    /// <summary>
    /// Check if a website or API responds successfully to HTTP requests.
    /// </summary>
    Http,
    /// <summary>
    /// Check if a specific port on a host is open and accepting connections.
    /// </summary>
    Tcp,
    /// <summary>
    /// Check if the DNS resolution works for a host.
    /// </summary>
    Dns,
    /// <summary>
    /// Check if a specific process is running on the local machine.
    /// </summary>
    Process,
    /// <summary>
    /// Check if a Minecraft server is reachable and accepting connections.
    /// </summary>
    Minecraft
}