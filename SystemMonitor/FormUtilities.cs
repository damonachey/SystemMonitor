namespace SystemMonitor;

internal class FormUtilities
{
    // https://stackoverflow.com/a/29596412/200807
    public static bool IsOnScreen(Point RecLocation, Size RecSize, double MinPercentOnScreen = 0.2)
    {
        var PixelsVisible = 0;
        var Rec = new Rectangle(RecLocation, RecSize);

        foreach (var screen in Screen.AllScreens)
        {
            var r = Rectangle.Intersect(Rec, screen.WorkingArea);

            // intersect rectangle with screen
            if (r.Width != 0 & r.Height != 0)
            {
                // tally visible pixels
                PixelsVisible += (r.Width * r.Height);
            }
        }
        return PixelsVisible >= (Rec.Width * Rec.Height) * MinPercentOnScreen;
    }
}
