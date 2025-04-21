using System.Diagnostics;
using System.Management;

namespace Wondie_CSharp.utils;

#pragma warning disable CA1416
public static class SystemUtils
{    
    public static string GetOsInfo()
    {
        return Environment.OSVersion.ToString();
    }
    
    public static float GetCpuInfo()
    {
        using var counter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total", true);
        counter.NextValue();
        Thread.Sleep(1000);
        return counter.NextValue();
    }

    public static double GetMemoryInfo()
    {
        using var memCounter = new PerformanceCounter("Memory", "Available MBytes", true);
        using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
        
        var totalMemory = 0UL;
        foreach (var obj in searcher.Get())
        {
            totalMemory = Convert.ToUInt64(obj["TotalPhysicalMemory"]) / (1024 * 1024);
        }
        
        var availableMemory = memCounter.NextValue();
        var used = totalMemory - availableMemory;
        return (used / (double)totalMemory) * 100;
    }

    public static TimeSpan GeTimeSpan()
    {
        return TimeSpan.FromMilliseconds(Environment.TickCount64);
    }
}