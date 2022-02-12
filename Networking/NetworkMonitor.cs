using System.Collections.Concurrent;
using System.Net.NetworkInformation;

namespace Networking;

public class NetworkMonitor : INetworkMonitor
{
    public delegate void UpdateHandler();
    public event UpdateHandler? OnUpdate;

    public readonly ConcurrentDictionary<DateTime, Log> logs = new();

    private readonly TimeSpan refreshPeriod = TimeSpan.FromSeconds(5);
    private readonly TimeSpan granularity = TimeSpan.FromMinutes(1);

    public async Task Run()
    {
        while (true)
        {
            var window = DateTime.Now.RoundUp(granularity);
            var current = GetLog(window);

            if (logs.TryGetValue(window, out var log))
            {
                log.BytesReceived += current.BytesReceived;
                log.BytesSent += current.BytesSent;
            }
            else
            {
                logs.TryAdd(window, current);
            }

            OnUpdate?.Invoke();

            await Task.Delay(refreshPeriod);
        }
    }

    private static Log GetLog(DateTime time)
    {
        var log = new Log { Time = time };

        // Aggregate data from all interfaces
        foreach (var intr in NetworkInterface.GetAllNetworkInterfaces())
        {
            var stats = intr.GetIPStatistics();

            log.BytesReceived += stats.BytesReceived;
            log.BytesSent += stats.BytesSent;
        }

        return log;
    }

    private static TimeSpan Uptime()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);

        return uptime;
    }
}
