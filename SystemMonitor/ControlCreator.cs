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
            foreach (var button in this)
            {
                button.BackColor = Settings.Default.ApplicationBackgroundColor;
            }

            var clickedButton = s as Button
                ?? throw new NullReferenceException(nameof(s));

            clickedButton.BackColor = Color.Gray;
        };

        Add(button);

        return button;
    }

    public static implicit operator Control[](RadioButtonGroup group)
    {
        return group.ToArray();
    }
}
