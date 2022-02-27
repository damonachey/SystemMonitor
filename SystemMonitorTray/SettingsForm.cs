namespace SystemMonitorTray;

public partial class SettingsForm : Form
{
    public SettingsForm()
    {
        BackColor = Properties.Settings.Default.applicationBackgroundColor;
        ForeColor = Properties.Settings.Default.applicationForegroundColor;
        MinimumSize = new Size(600, 300);
        StartPosition = FormStartPosition.Manual;

        InitializeComponent();

        Load += (s, e) => OnLoad();
    }

    private void OnFormClosing()
    {
        Properties.Settings.Default.settingsFormLocation = Location;
        Properties.Settings.Default.settingsFormSize = Size;
    }

    private void OnLoad()
    {
        Location = Properties.Settings.Default.settingsFormLocation;
        Size = Properties.Settings.Default.settingsFormSize;

        FormClosing += (s, e) => OnFormClosing();
        LocationChanged += (s, e) => SaveWindowPosition();
        SizeChanged += (s, e) => SaveWindowPosition();
    }

    private void SaveWindowPosition()
    {
        if (Size.Width >= MinimumSize.Width && Size.Height >= MinimumSize.Height)
        {
            Properties.Settings.Default.settingsFormLocation = Location;
            Properties.Settings.Default.settingsFormSize = Size;
        }
    }
}
