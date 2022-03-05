using System.Net.NetworkInformation;

namespace Networking;

public class NetworkMonitor : INetworkMonitor
{
    public delegate void UpdateHandler();
    public event UpdateHandler? OnUpdate;

    public string LogFileName { get; }
    public TimeSpan PollingInterval { get; } = TimeSpan.FromSeconds(60);
    public List<Log> Logs { get; internal set; } = new();

    public NetworkMonitor(string logPath)
    {
        LogFileName = GetLogFileName(logPath);
    }

    public async Task Start()
    {
        if (previous != null)
        {
            throw new NotSupportedException("Only one instance of NetworkMonitor can be run at a time");
        }

        InitializeLogs();
        await Delay.Next(PollingInterval);

        while (true)
        {
            var log = GetCurrentLog(PollingInterval);

            Logs.Add(log);
            File.AppendAllLines(LogFileName, new[] { log.ToString() });

            _ = Task.Run(() => OnUpdate?.Invoke());

            await Delay.Next(PollingInterval);
        }
    }

    private static string GetLogFileName(string logPath)
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
        DebugValidateLogs();
    }

    private void LoadNetworkMonitorLogs()
    {
        if (File.Exists(LogFileName))
        {
            Logs = File.ReadLines(LogFileName)
                .Select(line => Log.Parse(line))
                .ToList();
        }
    }

    private void PadLogsForMissingTime()
    {
        var lastStart = Uptime.LastStart().RoundDown(PollingInterval);
        var last = Logs.LastOrDefault() ?? new();

        if (last.Time <= lastStart)
        {
            last = new() { Time = lastStart };
        }

        var current = GetCurrentLog(PollingInterval);
        var intervals = (int)((current.Time - last.Time) / PollingInterval);

        if (intervals == 0)
        {
            previous = last;
            return;
        }

        var deltaBytesReceived = (current.CumulativeBytesReceived - last.CumulativeBytesReceived) / intervals;
        var deltaBytesSent = (current.CumulativeBytesSent - last.CumulativeBytesSent) / intervals;

        for (var time = last.Time + PollingInterval; time <= current.Time; time += PollingInterval)
        {
            Logs.Add(last = new()
            {
                Time = time,
                BytesReceived = deltaBytesReceived,
                BytesSent = deltaBytesSent,
                CumulativeBytesReceived = last.CumulativeBytesReceived + deltaBytesReceived,
                CumulativeBytesSent = last.CumulativeBytesSent + deltaBytesSent,
            });
        }

        previous = Logs.Last();
    }

    private void FilterLogs()
    {
        // only store one years worth of data
        Logs = Logs
            .Where(log => log.Time > DateTime.Today.AddDays(-365))
            .ToList();

        var directory = Path.GetDirectoryName(LogFileName)
            ?? throw new NullReferenceException(nameof(LogFileName));

        Directory.CreateDirectory(directory);
        File.WriteAllLines(LogFileName, Logs.Select(log => log.ToString()));
    }

    private void DebugValidateLogs()
    {
        var duplicates = Logs
            .GroupBy(logs => logs.Time)
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicates.Any())
        {
            foreach (var duplicate in duplicates)
            {
                var logStr = System.Text.Json.JsonSerializer.Serialize(duplicate);

                System.Diagnostics.Debug.WriteLine($"WARNING Duplicate: {logStr}");
            }
        }

        for (var i = 1; i < Logs.Count; i++)
        {
            var diffBytesReceived = Logs[i].CumulativeBytesReceived - Logs[i - 1].CumulativeBytesReceived;
            var diffBytesSent = Logs[i].CumulativeBytesSent - Logs[i - 1].CumulativeBytesSent;

            // probably a restart, ignore
            if (diffBytesReceived < 0 && diffBytesSent < 0)
            {
                continue;
            }

            if (diffBytesReceived != Logs[i].BytesReceived || diffBytesSent != Logs[i].BytesSent)
            {
                var prevStr = System.Text.Json.JsonSerializer.Serialize(Logs[i - 1]);
                var logStr = System.Text.Json.JsonSerializer.Serialize(Logs[i]);

                System.Diagnostics.Debug.WriteLine($"          Previous: {prevStr}");
                System.Diagnostics.Debug.WriteLine($"WARNING Math error: {logStr}");
            }
        }
    }

    private static Log? previous;

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
}
