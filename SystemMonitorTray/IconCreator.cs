﻿using System.Drawing.Drawing2D;

namespace SystemMonitorTray;

internal class IconCreator
{
    public static Icon CreateIcon(double percentage)
    {
        using var bmp = new Bitmap(32, 32);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        if (percentage < 0)
        {
            g.FillEllipse(Brushes.Gray, 0, 0, bmp.Width, bmp.Height);
        }
        else
        {
            g.FillEllipse(Brushes.Green, 0, 0, bmp.Width, bmp.Height);
            g.FillPie(Brushes.Red, 0, 0, bmp.Width, bmp.Height, 270, (int)(360 * percentage));
        }

        return Icon.FromHandle(bmp.GetHicon());
    }
}