using Networking;
using System.Configuration;
using System.Diagnostics;

namespace SystemMonitorTray;

public partial class SettingsForm : Form
{
    private INetworkMonitor networkMonitor = default!;

    public SettingsForm(INetworkMonitor networkMonitor)
    {
        BackColor = Properties.Settings.Default.applicationBackgroundColor;
        ForeColor = Properties.Settings.Default.applicationForegroundColor;
        MinimumSize = new Size(600, 300);
        StartPosition = FormStartPosition.Manual;

        this.networkMonitor = networkMonitor;

        InitializeComponent();
        InitializeSettingsControls();

        Load += (s, e) => OnLoad();
    }

    private void OnLoad()
    {
        Location = Properties.Settings.Default.settingsFormLocation;
        Size = Properties.Settings.Default.settingsFormSize;

        if (!FormUtilities.IsOnScreen(Location, Size))
        {
            SetDesktopLocation(0, 0);
        }

        FormClosing += (s, e) => SaveWindowPosition();
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

    private void InitializeSettingsControls()
    {
        var width = 120;
        var valueOffset = new Size(width, 0);

        var alertEnabledLabel = new Label
        {
            Location = new(10, 10),
            Text = "Alerts Enabled:",
        };
        Controls.Add(alertEnabledLabel);

        var alertEnabledCheckBox = new CheckBox
        {

        };
        Controls.Add(alertEnabledCheckBox);

        var alertValueLabel = new Label
        {

        };
        Controls.Add(alertValueLabel);

        var alertValueTextBox = new TextBox
        {

        };
        Controls.Add(alertValueTextBox);

        var alertValueUnitListBox = new ListBox
        {

        };
        Controls.Add(alertValueUnitListBox);

        var graphStyleLabel = new Label
        {
            Location = 
        };
        Controls.Add(graphStyleLabel);

        var graphStyleListBox = new ListBox
        {
            Location = 
        }

        var soundLabel = new Label
        {
            Location = new(10, 10),
            Text = "Sound Enabled:",
            Width = width,
        };
        Controls.Add(soundLabel);

        var soundCheckBox = new CheckBox
        {
            CheckAlign = ContentAlignment.TopLeft,
            Checked = Properties.Settings.Default.applicationSound,
            Location = soundLabel.Location + valueOffset,
            Text = "",
        };
        soundCheckBox.CheckedChanged += (s, e) => Properties.Settings.Default.applicationSound = soundCheckBox.Checked;
        Controls.Add(soundCheckBox);

        var startWithWindowsLabel = new Label
        {
            Location = soundLabel.Location + new Size(0, soundLabel.Height),
            Text = "Start With Windows:",
            Width = width,
        };
        Controls.Add(startWithWindowsLabel);

        var startWithWindowsCheckBox = new CheckBox
        {
            CheckAlign = ContentAlignment.TopLeft,
            Checked = Properties.Settings.Default.applicationStartWithWindows,
            Location = startWithWindowsLabel.Location + valueOffset,
            Text = "",
        };
        startWithWindowsCheckBox.CheckedChanged += (s, e) => Properties.Settings.Default.applicationStartWithWindows = startWithWindowsCheckBox.Checked;
        Controls.Add(startWithWindowsCheckBox);

        var pollingIntervalLabel = new Label
        {
            Location = startWithWindowsLabel.Location + new Size(0, startWithWindowsLabel.Size.Height),
            Text = "Polling Interval:",
            Width = width,
        };
        Controls.Add(pollingIntervalLabel);

        var pollingIntervalValueLabel = new Label
        {
            Location = pollingIntervalLabel.Location + valueOffset,
            Text = $"{networkMonitor.PollingInterval.Minutes} minutes",
        };
        Controls.Add(pollingIntervalValueLabel);

        var settingsLabel = new Label
        {
            Location = pollingIntervalLabel.Location + new Size(0, pollingIntervalLabel.Size.Height),
            Text = "Settings File:",
            Width = width,
        };
        Controls.Add(settingsLabel);

        var settingsFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
        var settingsLink = new LinkLabel
        {
            Location = settingsLabel.Location + valueOffset,
            Text = Path.GetFileName(settingsFile),
        };
        settingsLink.LinkClicked += (s, e) => Process.Start(new ProcessStartInfo
        {
            FileName = settingsFile,
            UseShellExecute = true,
        });
        Controls.Add(settingsLink);

        var logsLabel = new Label
        {
            Location = settingsLabel.Location + new Size(0, settingsLabel.Size.Height),
            Text = "Logs File:",
            Width = width,
        };
        Controls.Add(logsLabel);

        var logsLink = new LinkLabel
        {
            AutoSize = true,
            Location = logsLabel.Location + valueOffset,
            Text = Path.GetFileName(networkMonitor.LogFileName),
        };
        logsLink.LinkClicked += (s, e) => Process.Start(new ProcessStartInfo
        {
            FileName = networkMonitor.LogFileName,
            UseShellExecute = true,
        });
        Controls.Add(logsLink);
    }
}
