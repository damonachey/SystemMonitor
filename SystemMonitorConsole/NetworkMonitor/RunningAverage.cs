namespace SystemMonitorConsole.NetworkMonitor;

internal class RunningAverage
{
    private long Count;

    public double Value => Sum / Count;
    public double Sum;

    public double Next(double value)
    {
        Sum += value;
        Count++;

        return Value;
    }
}
