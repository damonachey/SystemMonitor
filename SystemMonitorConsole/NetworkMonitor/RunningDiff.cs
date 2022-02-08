namespace SystemMonitorConsole.NetworkMonitor;

internal class RunningDiff
{
    private long Previous = long.MinValue;

    public double Next(long value)
    {
        if (Previous == long.MinValue) Previous = value;

        var diff = value - Previous;

        Previous = value;

        return diff;
    }
}
