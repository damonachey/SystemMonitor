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
        if (previous is not null)
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
            var lines = File.ReadLines(LogFileName);

            foreach (var line in lines)
            {
                try
                {
                    Logs.Add(Log.Parse(line));
                }
                catch { } // if log file is corrupt or has bad data just ignore it.
            }
        }
    }

    private void PadLogsForMissingTime()
    {
        var lastStart = Uptime.LastStart().RoundDown(PollingInterval);
        var last = Logs.LastOrDefault() ?? new();

        if (last.Time <= lastStart)
        {
            // a restart occurred, use that time
            last = new() { Time = lastStart };
        }

        var current = GetCurrentLog(PollingInterval);

        if (current.CumulativeBytesReceived < last.CumulativeBytesReceived || current.CumulativeBytesSent < last.CumulativeBytesSent)
        {
            // the network totals have been reset, start as if new from last time
            last = new() { Time = last.Time };
            previous = last;
        }

        var intervals = (int)((current.Time - last.Time) / PollingInterval);

        if (intervals == 0)
        {
            // restarted within the same polling interval, leave
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
            .ToArray();

        if (duplicates.Any())
        {
            foreach (var duplicate in duplicates)
            {
                System.Diagnostics.Debug.WriteLine($"WARNING Duplicate: {duplicate}");
            }
        }

        for (var i = 1; i < Logs.Count; i++)
        {
            if (Logs[i].BytesReceived < 0 || Logs[i].BytesSent < 0)
            {
                System.Diagnostics.Debug.WriteLine($"WARNING Negative(s): {Logs[i]}");
            }

            var diffBytesReceived = Logs[i].CumulativeBytesReceived - Logs[i - 1].CumulativeBytesReceived;
            var diffBytesSent = Logs[i].CumulativeBytesSent - Logs[i - 1].CumulativeBytesSent;

            // probably a restart, ignore
            if (diffBytesReceived < 0 && diffBytesSent < 0)
            {
                continue;
            }

            if (diffBytesReceived != Logs[i].BytesReceived || diffBytesSent != Logs[i].BytesSent)
            {
                System.Diagnostics.Debug.WriteLine($"          Previous: {Logs[i - 1]}");
                System.Diagnostics.Debug.WriteLine($"WARNING Math error: {Logs[i]}");
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

        if (previous is null) previous = current;

        var log = new Log
        {
            Time = current.Time,
            BytesReceived = current.CumulativeBytesReceived - previous.CumulativeBytesReceived,
            BytesSent = current.CumulativeBytesSent - previous.CumulativeBytesSent,
            CumulativeBytesReceived = current.CumulativeBytesReceived,
            CumulativeBytesSent = current.CumulativeBytesSent
        };

        if (log.BytesReceived < 0 || log.BytesSent < 0)
        {
            // the network connection may have been reset, start as if new
            log.BytesReceived = log.CumulativeBytesReceived;
            log.BytesSent = log.CumulativeBytesSent;
        }

        previous = current;

        return log;
    }
}
