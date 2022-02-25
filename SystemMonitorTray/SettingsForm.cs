namespace SystemMonitorTray;

public partial class SettingsForm : Form
{
    public SettingsForm()
    {
        BackColor = Properties.Settings.Default.applicationBackgroundColor;
        ForeColor = Properties.Settings.Default.applicationForegroundColor;
        StartPosition = FormStartPosition.Manual;

        InitializeComponent();

        FormClosing += (s, e) => OnFormClosing();
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
    }
}
