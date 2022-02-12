namespace Networking;

internal static class DateTimeExtensions
{
    public static DateTime RoundUp(this DateTime dt, TimeSpan d)
    {
        return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
    }

    public static DateTime RoundDown(this DateTime dt, TimeSpan d)
    {
        return new DateTime(dt.Ticks / d.Ticks * d.Ticks, dt.Kind);
    }
}
