namespace SystemMonitorConsole.NetworkMonitor;

static class ExtensionMethods
{
    public static DateTime RoundDown(this DateTime dt, int minutes = 1)
        => new(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, dt.Kind);
}
