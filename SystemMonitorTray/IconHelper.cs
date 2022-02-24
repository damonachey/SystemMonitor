using System.Drawing.Drawing2D;

namespace SystemMonitorTray;

internal class IconHelper
{
    public static Icon GenerateIcon(double percentage)
    {
        using var bmp = new Bitmap(32, 32);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        g.FillEllipse(Brushes.Green, 0, 0, bmp.Width, bmp.Height);
        g.FillPie(Brushes.Red, 0, 0, bmp.Width, bmp.Height, 270, (int)(360 * percentage));

        return Icon.FromHandle(bmp.GetHicon());
    }
}
