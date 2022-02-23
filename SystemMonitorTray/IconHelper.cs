namespace SystemMonitorTray;

internal class IconHelper
{
    public static Icon GenerateIcon(int i)
    {
        using var bmp = new Bitmap(32, 32);
        using var g = Graphics.FromImage(bmp);
        using var font = new Font("tahoma", 14);

        var str = i.ToString();
        var size = g.MeasureString(str, font);

        g.DrawString(str, font, Brushes.White, 0, 0);

        return Icon.FromHandle(bmp.GetHicon());
    }
}
