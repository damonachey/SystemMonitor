namespace SystemMonitor;

internal class RadioButtonGroup : List<Button>
{
    public Button CreateRadioButton(string text, object value)
    {
        var button = new Button()
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            TabStop = false,
            Tag = value,
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.BorderColor = Color.Gray;
        button.Click += (s, e) =>
        {
            foreach (var b in this)
            {
                b.BackColor = Properties.Settings.Default.applicationBackgroundColor;
            }

            ((Button)s!).BackColor = Color.Gray;
        };

        Add(button);

        return button;
    }

    public static implicit operator Control[](RadioButtonGroup group)
    {
        return group.ToArray();
    }
}
