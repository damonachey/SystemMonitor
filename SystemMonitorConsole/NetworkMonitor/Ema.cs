namespace SystemMonitorConsole.NetworkMonitor;

internal class Ema
{
    private readonly double Alpha;

    public double Periods { get; }
    public double Value { get; private set; } = double.NaN;

    public Ema(int periods) => Alpha = 2d / ((Periods = periods) + 1);

    public double Next(double value)
    {
        if (double.IsNaN(Value)) Value = value;
        else Value = (value - Value) * Alpha + Value;

        return Value;
    }
}
