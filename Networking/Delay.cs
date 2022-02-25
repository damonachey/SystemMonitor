﻿namespace Networking;

internal class Delay
{
    public static async Task Next(TimeSpan interval)
    {
        var now = DateTime.Now;
        var nextMinute = now.RoundUp(interval) - now;

        await Task.Delay(nextMinute);
    }
}
