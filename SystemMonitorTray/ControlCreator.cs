namespace SystemMonitorTray;

internal class ControlCreator
{
    private static readonly Dictionary<int, List<Button>> radioGroups = new();

    public static Button CreateRadioButton(int group, string text, object value, Size? size = null, Font? font = null)
    {
        radioGroups.TryGetValue(group, out var buttons);
        var previous = buttons?.Last();
        var button = new Button()
        {
            Text = text,
            Font = previous?.Font ?? font,
            Size = previous?.Size ?? size!.Value,
            FlatStyle = FlatStyle.Flat,
            TabStop = false,
            Tag = value,
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.BorderColor = Color.Gray;
        button.Click += (s, e) =>
        {
            foreach (var b in radioGroups[group])
            {
                b.BackColor = Properties.Settings.Default.applicationBackgroundColor;
            }

            ((Button)s!).BackColor = Color.Gray;
        };

        if (!radioGroups.ContainsKey(group))
        {
            radioGroups.Add(group, new());
            button.PerformClick();
        }

        radioGroups[group].Add(button);

        return button;
    }
}
