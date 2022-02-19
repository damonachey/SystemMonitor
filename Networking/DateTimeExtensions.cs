namespace Networking;

public static class DateTimeExtensions
{
    public static DateTime RoundUp(this DateTime dt, TimeSpan d)
    {
        return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
    }

    public static DateTime RoundDown(this DateTime dt, TimeSpan d)
    {
        return new DateTime(dt.Ticks / d.Ticks * d.Ticks, dt.Kind);
    }

    public static DateTime StartOfWeek(this DateTime dt)
    {
        return dt.Date.AddDays(-(int)dt.Date.DayOfWeek);
    }

    public static DateTime StartOfMonth(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, 1);
    }
}
