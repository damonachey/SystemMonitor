namespace Networking;

internal class Uptime
{
    public static DateTime LastStart()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);

        return DateTime.Now.Add(-uptime);
    }
}
