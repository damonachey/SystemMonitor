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
        if (previous != null)
        {
            throw new NotSupportedException("Only one instance of NetworkMonitor can be run at a time");
        }

        InitializeLogs();
        await Delay.Next(pollInterval);

        while (true)
        {
            var log = GetCurrentLog(pollInterval);

            Logs.Add(log);
            File.AppendAllLines(logFile, new[] { log.ToString() });

            _ = Task.Run(() => OnUpdate?.Invoke());

            await Delay.Next(pollInterval);
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
        DebugValidateLogs();
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
        var lastStart = Uptime.LastStart().RoundDown(pollInterval);
        var last = Logs.LastOrDefault() ?? new();

        if (last.Time <= lastStart)
        {
            last = new() { Time = lastStart };
        }

        var current = GetCurrentLog(pollInterval);
        var intervals = (int)((current.Time - last.Time) / pollInterval);

        if (intervals == 0)
        {
            previous = last; 
            return;
        }

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

        previous = Logs.Last();
    }

    private void FilterLogs()
    {
        // only store one years worth of data
        Logs = Logs
            .Where(log => log.Time > DateTime.Today.AddDays(-365))
            .ToList();

        // write clean logs file
        File.WriteAllLines(logFile, Logs.Select(log => log.ToString()));
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

            System.Diagnostics.Debugger.Break();
        }

        for (var i = 1; i < Logs.Count; i++)
        {
            var diffBytesReceived = Logs[i].CumulativeBytesReceived - Logs[i - 1].CumulativeBytesReceived;
            var diffBytesSent = Logs[i].CumulativeBytesSent - Logs[i - 1].CumulativeBytesSent;

            if (diffBytesReceived != Logs[i].BytesReceived || diffBytesSent != Logs[i].BytesSent)
            {
                var logStr = System.Text.Json.JsonSerializer.Serialize(Logs[i]);

                System.Diagnostics.Debug.WriteLine($"WARNING Math error: {logStr}");
         
                System.Diagnostics.Debugger.Break();
            }
        }
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
}
