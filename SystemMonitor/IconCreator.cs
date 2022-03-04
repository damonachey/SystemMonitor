using System.Drawing.Drawing2D;

namespace SystemMonitor;

internal class IconCreator
{
    public static Icon CreateIcon(double percentage)
    {
        using var bmp = new Bitmap(32, 32);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var brush = percentage switch
        {
            < 0 => Brushes.Green,
            0 => Brushes.Gray,
            <= 0.75 => Brushes.Green,
            < 1 => Brushes.Yellow,
            _ => Brushes.Red,
        };

        g.FillEllipse(Brushes.Gray, 0, 0, bmp.Width, bmp.Height);
        g.FillPie(brush, 0, 0, bmp.Width, bmp.Height, 270, (int)(360 * percentage));

        return Icon.FromHandle(bmp.GetHicon());
    }
}
