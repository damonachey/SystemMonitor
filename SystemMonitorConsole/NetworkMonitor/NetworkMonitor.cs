using System.Net.NetworkInformation;
using System.Text.Json;

namespace SystemMonitorConsole.NetworkMonitor;

internal class NetworkMonitor : IMonitor
{
    private const double KB = 1024;
    private const double MB = KB * KB;
    private const double GB = MB * KB;
    private const double TB = GB * KB;

    private readonly string logFile = @$"{Path.GetTempPath()}\Network Usage.log";
    private readonly record struct Entry(DateTime Time, long Bytes);
    private readonly List<Entry> entries;

    private readonly int groupSize = 1;
    private readonly double threshold = 30 * GB / 10 / 60;

    public NetworkMonitor()
    {
        entries = LoadEntries(logFile);
    }

    public Statuses Status()
    {
        entries.Add(new(DateTime.Now, GetBytes()));
        //File.AppendAllLines(logFile, new[] { JsonSerializer.Serialize(entries.Last(), options: new() { IncludeFields = true }) });

        var runningDiff = new RunningDiff();
        var runningAverage = new RunningAverage();
        var sma = new Sma(15);
        var ema = new Ema(15);

        var histories = new Dictionary<DateTime, double>();

        for (var time = DateTime.Today; time < DateTime.Today.AddDays(1); time = time.AddMinutes(1))
            histories.Add(time, 0);

        foreach (var entry in entries)
            histories[entry.Time.RoundDown()] += runningDiff.Next(entry.Bytes);

        var currents = histories
            .Where(h => h.Key > DateTime.Today.AddHours(7.5))
            .Where(h => h.Key < DateTime.Now);

        foreach (var current in currents)
        {
            runningAverage.Next(current.Value);
            sma.Next(current.Value);
            ema.Next(current.Value);
        }

        Console.WriteLine($"Current rate: {histories[DateTime.Now.RoundDown().AddMinutes(-1)] / GB * 60 / groupSize:0.00} GB/hour");
        Console.WriteLine($"Total: {runningAverage.Sum / GB:0.00} GB/day");
        Console.WriteLine($"Running average: {runningAverage.Value / MB:0.00} MB");
        Console.WriteLine($"SMA({sma.Periods}): {sma.Value / MB:0.00} MB");
        Console.WriteLine($"EMA({ema.Periods}): {ema.Value / MB:0.00} MB");

        if (histories[DateTime.Now.RoundDown().AddMinutes(-1)] > threshold) return Statuses.Warning;
        if (runningAverage.Value > threshold) return Statuses.Error;

        return Statuses.Normal;
    }

    private static List<Entry> LoadEntries(string logFile)
    {
        var entries = new List<Entry>();

        if (File.Exists(logFile))
            entries.AddRange(File
                .ReadLines(logFile)
                .Select(line => JsonSerializer.Deserialize<Entry>(line, options: new() { IncludeFields = true }))
                .Where(log => log.Time.Date == DateTime.Today));

        return entries;
    }

    private static long GetBytes(int interfaceNumber = 0)
        => NetworkInterface
            .GetAllNetworkInterfaces()[interfaceNumber]
            .GetIPv4Statistics()
            .BytesReceived;
}
