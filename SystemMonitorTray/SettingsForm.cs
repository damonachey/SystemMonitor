namespace SystemMonitorTray;

public partial class SettingsForm : Form
{
    public SettingsForm()
    {
        BackColor = Properties.Settings.Default.applicationBackgroundColor;
        ForeColor = Properties.Settings.Default.applicationForegroundColor;
        Location = Properties.Settings.Default.settingsFormLocation;
        Size = Properties.Settings.Default.settingsFormSize;
        StartPosition = FormStartPosition.Manual;

        InitializeComponent();

        FormClosing += (o, e) => OnFormClosing();
    }

    private void OnFormClosing()
    {
        Properties.Settings.Default.settingsFormLocation = Location;
        Properties.Settings.Default.settingsFormSize = Size;
    }
}
