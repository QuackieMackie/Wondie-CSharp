namespace Wondie_CSharp.Events.Models;

public class MonitorTarget
{
    public string Address { get; }
    public MonitorType Type { get; }
    public bool IsOnline { get; set; }
    public DateTimeOffset LastCheckTime { get; set; }

    public MonitorTarget(string address, MonitorType type)
    {
        Address = address;
        Type = type;
        LastCheckTime = DateTimeOffset.UtcNow;
    }
}