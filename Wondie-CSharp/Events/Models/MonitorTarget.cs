﻿using Wondie_CSharp.Events.Models;

namespace Wondie_CSharp.Events.Models;

/// <summary>
/// Represents a target to be monitored, including its address, monitoring type, and online status.
/// </summary>
public class MonitorTarget
{
    /// <summary>
    /// Gets the public address used to connect to the monitored service (e.g., a URL or IP address visible externally).
    /// </summary>
    public string PublicAddress { get; }
    
    /// <summary>
    /// Gets the address of the target to be monitored (e.g., an IP address, hostname, or domain).
    /// </summary>
    public string Address { get; }

    /// <summary>
    /// Gets the type of monitoring task to perform on the target.
    /// </summary>
    public MonitorType Type { get; }

    /// <summary>
    /// Gets or sets whether the target is currently online or reachable.
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// Initialises a new instance of the <see cref="MonitorTarget"/> class with the specified address and monitoring type.
    /// </summary>
    /// <param name="publicAddress">The address people use to connect to the service.</param>
    /// <param name="address">The address of the target to monitor.</param>
    /// <param name="type">The type of monitoring task to perform.</param>
    public MonitorTarget(string publicAddress, string address, MonitorType type)
    {
        PublicAddress = publicAddress;
        Address = address;
        Type = type;
    }
}