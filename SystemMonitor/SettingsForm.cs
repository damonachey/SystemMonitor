using Networking;

using System.Configuration;
using System.Diagnostics;

namespace SystemMonitor;

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

        // ***********************************************************************
        label = new Label
        {
            Location = label.Location + new Size(0, label.Height),
            Text = "Alert Value:",
            Width = label.Width,
        };
        Controls.Add(label);

        value = new NumericUpDown
        {
            Location = label.Location + new Size(label.Width, 0),
            Maximum = int.MaxValue,
            Value = Properties.Settings.Default.settingsFormAlertValue,
            Width = label.Width / 2,
        };
        var alertValueTextBox = (NumericUpDown)value;
        alertValueTextBox.ValueChanged += (s, e) => Properties.Settings.Default.settingsFormAlertValue = (long)alertValueTextBox.Value;
        Controls.Add(value);

        value = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = value.Location + new Size(value.Width , 0),
            Width = 50,
        };
        var alertUnitComboBox = (ComboBox)value;
        alertUnitComboBox.Items.AddRange(new[]
        {
            nameof(Unit.B),
            nameof(Unit.KB),
            nameof(Unit.MB),
            nameof(Unit.GB),
            nameof(Unit.TB),
            nameof(Unit.PB),
        });
        alertUnitComboBox.SelectedItem = Properties.Settings.Default.settingsFormAlertUnit;
        alertUnitComboBox.SelectedValueChanged += (s, e) => Properties.Settings.Default.settingsFormAlertUnit = (string)alertUnitComboBox.SelectedItem;
        Controls.Add(value);

        value = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = value.Location + new Size(value.Width + 1, 0),
            Width = 80,
        };
        var alertRangeComboBox = (ComboBox)value;
        alertRangeComboBox.Items.AddRange(new[]
        {
            nameof(Range.Hour),
            nameof(Range.Day),
            nameof(Range.Month),
        });
        alertRangeComboBox.SelectedItem = Properties.Settings.Default.settingsFormAlertRange;
        alertRangeComboBox.SelectedValueChanged += (s, e) => Properties.Settings.Default.settingsFormAlertRange = (string)alertRangeComboBox.SelectedItem;
        Controls.Add(value);

        // ***********************************************************************
        label = new Label
        {
            Location = label.Location + new Size(0, label.Height),
            Text = "Graph Style:",
            Width = label.Width,
        };
        Controls.Add(label);

        value = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,   
            Location = label.Location + new Size(label.Width, 0),
            Width = 80,
        };
        var graphStyleComboBox = (ComboBox)value;
        graphStyleComboBox.Items.AddRange(new[]
        {
            "SplineArea",
            "Column",
        });
        graphStyleComboBox.SelectedItem = Properties.Settings.Default.detailsFormGraphStyle;
        graphStyleComboBox.SelectedValueChanged += (s, e) => Properties.Settings.Default.detailsFormGraphStyle = (string)graphStyleComboBox.SelectedItem;
        Controls.Add(value);

        value = new Label
        {
            Location = value.Location + new Size(value.Width, 0),
            Text = "(takes effect on new window)",
            Width = label.Width * 2,
        };
        Controls.Add(value);

        // ***********************************************************************
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
            Height = 15,
            Location = label.Location + new Size(label.Width, 0),
            Width = 12,
        };
        var soundCheckBox = (CheckBox)value;
        soundCheckBox.CheckedChanged += (s, e) => Properties.Settings.Default.applicationSound = soundCheckBox.Checked;
        Controls.Add(value);

        // ***********************************************************************
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
            Height = 15,
            Location = label.Location + new Size(label.Width, 0),
            Width = 12,
        };
        var startWithWindowsCheckBox = (CheckBox)value;
        startWithWindowsCheckBox.CheckedChanged += (s, e) => Properties.Settings.Default.applicationStartWithWindows = startWithWindowsCheckBox.Checked;
        // TODO: requires more to flip this value
        Controls.Add(value);

        // ***********************************************************************
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
            Text = $"{networkMonitor.PollingInterval.Minutes} minute (not editable)",
            Width = label.Width * 2,
        };
        Controls.Add(value);

        // ***********************************************************************
        label = new Label
        {
            Location = label.Location + new Size(0, label.Height * 2),
            Text = "!!! Below files are listed for debugging and should not be edited by hand !!!",
            Width = label.Width * 4,
        };
        Controls.Add(label);

        // ***********************************************************************
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

        // ***********************************************************************
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
