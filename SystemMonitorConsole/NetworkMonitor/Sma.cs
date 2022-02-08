namespace SystemMonitorConsole.NetworkMonitor;

internal class Sma
{
    private readonly Queue<double> queue = new();

    public double Value => queue.DefaultIfEmpty(double.NaN).Average();
    public int Periods { get; }

    public Sma(int periods) => Periods = periods;

    public double Next(double value)
    {
        queue.Enqueue(value);
        while (queue.Count > Periods) queue.Dequeue();

        return Value;
    }
}
