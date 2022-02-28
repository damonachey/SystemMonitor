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
        var label = new Control
        {
            Location = new(10, 10),
            Width = 120,
        };
        var value = new Control
        {
            Location = new(120, 10),
        };

        label = new Label
        {
            Location = label.Location + new Size(0, label.Height),
            Text = "Alerts Enabled:",
            Width = label.Width,
        };
        Controls.Add(label);

        value = new CheckBox
        {
            CheckAlign = ContentAlignment.TopLeft,
            //Checked = Properties.Settings.Default.applicationAlert,
            Location = label.Location + new Size(label.Width, 0),
        };
        Controls.Add(value);

        //  ***********************************************************************
        label = new Label
        {
            Location = label.Location + new Size(0, label.Height),
            Text = "Alert Value:",
            Width = label.Width,
        };
        Controls.Add(label);

        value = new TextBox
        {
            Location = label.Location + new Size(label.Width, 0),
            Text = "none",
        };
        Controls.Add(value);

        value = new ListBox
        {
            Location = label.Location + new Size(label.Width * 2, 0),
            Text = "MB",
        };
        Controls.Add(value);

        //  ***********************************************************************
        label = new Label
        {
            Location = label.Location + new Size(0, label.Height),
            Text = "Graph Style:",
            Width = label.Width,
        };
        Controls.Add(label);

        value = new ListBox
        {
            Location = value.Location + new Size(label.Width, 0),
            Text = "None",
        };
        Controls.Add(value);

        //  ***********************************************************************
        label = new Label
        {
            Location = label.Location + new Size(0, label.Height),
            Text = "Sound Enabled:",
            Width = label.Width,
        };
        Controls.Add(label);

        value = new CheckBox
        {
            CheckAlign = ContentAlignment.TopLeft,
            Checked = Properties.Settings.Default.applicationSound,
            Location = label.Location + new Size(label.Width, 0),
        };
        var soundCheckBox = (CheckBox)value;
        soundCheckBox.CheckedChanged += (s, e) => Properties.Settings.Default.applicationSound = soundCheckBox.Checked;
        Controls.Add(value);

        //  ***********************************************************************
        label = new Label
        {
            Location = label.Location + new Size(0, label.Height),
            Text = "Start With Windows:",
            Width = label.Width,
        };
        Controls.Add(label);

        value = new CheckBox
        {
            CheckAlign = ContentAlignment.TopLeft,
            Checked = Properties.Settings.Default.applicationStartWithWindows,
            Location = label.Location + new Size(label.Width, 0),
        };
        var startWithWindowsCheckBox = (CheckBox)value;
        startWithWindowsCheckBox.CheckedChanged += (s, e) => Properties.Settings.Default.applicationStartWithWindows = startWithWindowsCheckBox.Checked;
        // TODO: requires more to flip this value
        Controls.Add(value);

        //  ***********************************************************************
        label = new Label
        {
            Location = label.Location + new Size(0, label.Height),
            Text = "Polling Interval:",
            Width = label.Width,
        };
        Controls.Add(label);

        value = new Label
        {
            Location = label.Location + new Size(label.Width, 0),
            Text = $"{networkMonitor.PollingInterval.Minutes} minutes (not editable)",
            Width = label.Width * 2,
        };
        Controls.Add(value);

        //  ***********************************************************************
        label = new Label
        {
            Location = label.Location + new Size(0, label.Height * 2),
            Text = "!!! Below files are listed for debugging and should not be edited by hand !!!",
            Width = label.Width * 4,
        };
        Controls.Add(label);

        //  ***********************************************************************
        label = new Label
        {
            Location = label.Location + new Size(0, label.Height),
            Text = "Settings File:",
            Width = label.Width / 4,
        };
        Controls.Add(label);

        var settingsFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
        value = new LinkLabel
        {
            LinkColor = Color.White,
            Location = label.Location + new Size(label.Width, 0),
            Text = Path.GetFileName(settingsFile),
        };
        var settingsFileLink = (LinkLabel)value;
        ((LinkLabel)value).LinkClicked += (s, e) => Process.Start(new ProcessStartInfo
        {
            FileName = settingsFile,
            UseShellExecute = true,
        });
        Controls.Add(value);

        //  ***********************************************************************
        label = new Label
        {
            Location = label.Location + new Size(0, label.Height),
            Text = "Logs File:",
            Width = label.Width,
        };
        Controls.Add(label);

        value = new LinkLabel
        {
            AutoSize = true,
            LinkColor = Color.White,
            Location = label.Location + new Size(label.Width, 0),
            Text = Path.GetFileName(networkMonitor.LogFileName),
        };
        ((LinkLabel)value).LinkClicked += (s, e) => Process.Start(new ProcessStartInfo
        {
            FileName = networkMonitor.LogFileName,
            UseShellExecute = true,
        });
        Controls.Add(value);
    }
}
