using System.Net.NetworkInformation;

namespace Networking;

public class NetworkMonitor : INetworkMonitor
{
    public delegate void UpdateHandler();
    public event UpdateHandler? OnUpdate;

    public List<Log> Logs { get; internal set; } = new();

    private readonly string logFile = default!;
    private readonly TimeSpan pollInterval = TimeSpan.FromSeconds(60);

    public NetworkMonitor(string logPath)
    {
        logFile = GetLogFilename(logPath);
    }

    public async Task Start()
    {
        InitializeLogs();
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

    private static string GetLogFilename(string logPath)
    {
        var networkId = NetworkInterface
            .GetAllNetworkInterfaces()
            .FirstOrDefault()
            ?.Id;

        return @$"{logPath}\NetworkMonitor.{networkId}.log";
    }

    private void InitializeLogs()
    {
        LoadNetworkMonitorLogs();
        PadLogsForMissingTime();
        FilterLogs();
    }

    private void LoadNetworkMonitorLogs()
    {
        if (File.Exists(logFile))
        {
            Logs = File.ReadLines(logFile)
                .Select(line => Log.Parse(line))
                .ToList();
        }
    }

    private void PadLogsForMissingTime()
    {
        var lastStart = LastStart().RoundDown(pollInterval);
        var last = Logs.LastOrDefault() ?? new();

        if (last.Time < lastStart) last = new() { Time = lastStart };

        var current = GetCurrentLog(pollInterval);
        var intervals = (int)((current.Time - last.Time) / pollInterval);

        if (intervals == 0) return;

        var deltaBytesReceived = (current.CumulativeBytesReceived - last.CumulativeBytesReceived) / intervals;
        var deltaBytesSent = (current.CumulativeBytesSent - last.CumulativeBytesSent) / intervals;

        for (var time = last.Time + pollInterval; time <= current.Time; time += pollInterval)
        {
            Logs.Add(last = new Log
            {
                Time = time,
                BytesReceived = deltaBytesReceived,
                BytesSent = deltaBytesSent,
                CumulativeBytesReceived = last.CumulativeBytesReceived + deltaBytesReceived,
                CumulativeBytesSent = last.CumulativeBytesSent + deltaBytesSent,
            });
        }
    }

    private void FilterLogs()
    {
        // only store one years worth of data
        Logs = Logs
            .Where(log => log.Time > DateTime.Today.AddDays(-365))
            .ToList();

        var duplicates = Logs
            .GroupBy(logs => logs.Time)
            .Where(group => group.Count() > 1)
            .ToList();

        // check for duplicate logs in case of restart in the same minute
        if (duplicates.Any())
        {
            System.Diagnostics.Debugger.Break();
        }

        // write clean logs file
        File.WriteAllLines(logFile, Logs.Select(log => log.ToString()));
    }

    private static Log previous = default!;

    private static Log GetCurrentLog(TimeSpan round)
    {
        var current = new Log { Time = DateTime.Now.RoundDown(round) };

        // Aggregate data from all interfaces
        foreach (var intr in NetworkInterface.GetAllNetworkInterfaces())
        {
            var stats = intr.GetIPStatistics();

            current.CumulativeBytesReceived += stats.BytesReceived;
            current.CumulativeBytesSent += stats.BytesSent;
        }

        if (previous == null) previous = current;

        var log = new Log
        {
            Time = current.Time,
            BytesReceived = current.CumulativeBytesReceived - previous.CumulativeBytesReceived,
            BytesSent = current.CumulativeBytesSent - previous.CumulativeBytesSent,
            CumulativeBytesReceived = current.CumulativeBytesReceived,
            CumulativeBytesSent = current.CumulativeBytesSent
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

    private static DateTime LastStart()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);

        return DateTime.Now.Add(-uptime);
    }
}
