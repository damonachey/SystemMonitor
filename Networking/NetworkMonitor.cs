using System.Net.NetworkInformation;
using System.Reflection;

namespace Networking;

public class NetworkMonitor : INetworkMonitor
{
    public delegate void UpdateHandler();
    public event UpdateHandler? OnUpdate;

    public IList<Log> Logs { get; } = new List<Log>();

    private readonly string logFile = @$"{AppContext.BaseDirectory}\NetworkMonitor.log";
    private readonly TimeSpan pollInterval = TimeSpan.FromSeconds(60);

    public NetworkMonitor()
    {
        if (File.Exists(logFile))
        {
            var lines = File.ReadLines(logFile);

            foreach (var line in lines)
            {
                var log = Log.Parse(line);

                Logs.Add(log);
            }

            Logs = Logs
                .GroupBy(logs => logs.Time)
                .Select(group => group.First())
                .ToList();

            File.WriteAllLines(logFile, Logs.Select(log => log.ToString()));
        }
    }

    public async Task Run()
    {
        GetCurrentLog(pollInterval);
        await DelayNext(pollInterval);

        while (true)
        {
            var log = GetCurrentLog(pollInterval);

            Logs.Add(log);
            File.AppendAllLines(logFile, new[] { log.ToString() });

            _ = Task.Run(() => OnUpdate?.Invoke());

            await DelayNext(pollInterval);
        }
    }

    static Log previous = default!;

    private static Log GetCurrentLog(TimeSpan round)
    {
        var current = new Log { Time = DateTime.Now.RoundDown(round) };

        // Aggregate data from all interfaces
        foreach (var intr in NetworkInterface.GetAllNetworkInterfaces())
        {
            var stats = intr.GetIPStatistics();

            current.BytesReceived += stats.BytesReceived;
            current.BytesSent += stats.BytesSent;
        }

        if (previous == null) previous = current;

        var log = new Log
        {
            Time = current.Time,
            BytesReceived = current.BytesReceived - previous.BytesReceived,
            BytesSent = current.BytesSent - previous.BytesSent
        };

        previous = current;

        return log;
    }

    static async Task DelayNext(TimeSpan interval)
    {
        var now = DateTime.Now;
        var nextMinute = now.RoundUp(interval) - now;

        await Task.Delay(nextMinute);
    }
}
