namespace Networking;

internal class RunningDiff
{
    public long Value { get; private set; } = long.MinValue;

    public long Next(long current)
    {
        if (Value == long.MinValue) Value = current;

        var diff = current - Value;

        Value = current;

        return diff;
    }
}
