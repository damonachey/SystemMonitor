using System.Net.NetworkInformation;

namespace Networking;

public class NetworkMonitor : INetworkMonitor
{
    public delegate void UpdateHandler();
    public event UpdateHandler? OnUpdate;

    public string LogFile { get; } = default!;
    public TimeSpan PollInterval { get; } = TimeSpan.FromSeconds(60);
    public List<Log> Logs { get; internal set; } = new();

    public NetworkMonitor(string logPath)
    {
        LogFile = GetLogFilename(logPath);
    }

    public async Task Start()
    {
        if (previous != null)
        {
            throw new NotSupportedException("Only one instance of NetworkMonitor can be run at a time");
        }

        InitializeLogs();
        await Delay.Next(PollInterval);

        while (true)
        {
            var log = GetCurrentLog(PollInterval);

            Logs.Add(log);
            File.AppendAllLines(LogFile, new[] { log.ToString() });

            _ = Task.Run(() => OnUpdate?.Invoke());

            await Delay.Next(PollInterval);
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
        if (File.Exists(LogFile))
        {
            Logs = File.ReadLines(LogFile)
                .Select(line => Log.Parse(line))
                .ToList();
        }
    }

    private void PadLogsForMissingTime()
    {
        var lastStart = Uptime.LastStart().RoundDown(PollInterval);
        var last = Logs.LastOrDefault() ?? new();

        if (last.Time <= lastStart)
        {
            last = new() { Time = lastStart };
        }

        var current = GetCurrentLog(PollInterval);
        var intervals = (int)((current.Time - last.Time) / PollInterval);

        if (intervals == 0)
        {
            previous = last; 
            return;
        }

        var deltaBytesReceived = (current.CumulativeBytesReceived - last.CumulativeBytesReceived) / intervals;
        var deltaBytesSent = (current.CumulativeBytesSent - last.CumulativeBytesSent) / intervals;

        for (var time = last.Time + PollInterval; time <= current.Time; time += PollInterval)
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
        File.WriteAllLines(LogFile, Logs.Select(log => log.ToString()));
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
                // probably a restart, ignore
                if (diffBytesReceived < 0 && diffBytesSent < 0)
                    continue;

                var prevStr = System.Text.Json.JsonSerializer.Serialize(Logs[i - 1]);
                var logStr = System.Text.Json.JsonSerializer.Serialize(Logs[i]);

                System.Diagnostics.Debug.WriteLine($"          Previous: {prevStr}");
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
