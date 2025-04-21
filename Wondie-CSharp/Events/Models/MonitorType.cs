namespace Wondie_CSharp.Events.Models;

public enum MonitorType
{
    Ping,
    Http,      // Check if a website/API is responding
    Tcp,       // Check if a port is open
    Dns,       // Check if DNS resolution works
    Process    // Check if a local process is running
}