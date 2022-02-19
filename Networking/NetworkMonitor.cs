using System.Net.NetworkInformation;

namespace Networking;

public class NetworkMonitor : INetworkMonitor
{
    public delegate void UpdateHandler();
    public event UpdateHandler? OnUpdate;

    public List<Log> Logs { get; } = new List<Log>();

    private readonly string logFile = GetLogFilenmame();
    private readonly TimeSpan pollInterval = TimeSpan.FromSeconds(60);

    public NetworkMonitor()
    {
        LoadNetworkMonitorLog();
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

    private void LoadNetworkMonitorLog()
    {
        if (File.Exists(logFile))
        {
            var logs = new List<Log>();
            var lines = File.ReadLines(logFile);

            foreach (var line in lines)
            {
                var log = Log.Parse(line);

                logs.Add(log);
            }

            logs = logs
                // only store one years worth of data
                .Where(log => log.Time > DateTime.Today.AddDays(-365))
                // filter duplicate logs in case of restart in the same minute
                .GroupBy(logs => logs.Time)
                .Select(group => group.First())
                .ToList();

            Logs.AddRange(logs);

            File.WriteAllLines(logFile, Logs.Select(log => log.ToString()));
        }
    }

    private static string GetLogFilenmame()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var networkId = NetworkInterface
            .GetAllNetworkInterfaces()
            .FirstOrDefault()
            ?.Id;

        return @$"{baseDirectory}\NetworkMonitor.{networkId}.log";
    }

    private static Log previous = default!;

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

    private static async Task DelayNext(TimeSpan interval)
    {
        var now = DateTime.Now;
        var nextMinute = now.RoundUp(interval) - now;

        await Task.Delay(nextMinute);
    }
}
