using Networking;

using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;

namespace SystemMonitor;

public partial class SettingsForm : Form
{
    private readonly INetworkMonitor networkMonitor = default!;

    public SettingsForm(INetworkMonitor networkMonitor)
    {
        BackColor = Settings.Default.ApplicationBackgroundColor;
        ForeColor = Settings.Default.ApplicationForegroundColor;
        MinimumSize = new Size(600, 300);
        StartPosition = FormStartPosition.Manual;

        this.networkMonitor = networkMonitor;

        InitializeComponent();
        InitializeSettingsControls();

        Load += (s, e) => OnLoad();
    }

    private void OnLoad()
    {
        Location = Settings.Default.SettingsFormLocation;
        Size = Settings.Default.SettingsFormSize;

        if (!FormUtilities.IsOnScreen(Location, Size))
        {
            SetDesktopLocation(0, 0);
        }

        FormClosing += (s, e) => { SaveWindowPosition(); Settings.Save(); };
        LocationChanged += (s, e) => SaveWindowPosition();
        SizeChanged += (s, e) => SaveWindowPosition();
    }

    private void SaveWindowPosition()
    {
        if (Size.Width >= MinimumSize.Width && Size.Height >= MinimumSize.Height)
        {
            Settings.Default.SettingsFormLocation = Location;
            Settings.Default.SettingsFormSize = Size;
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
            Value = Settings.Default.SettingsFormAlertValue,
            Width = label.Width / 2,
        };
        var alertValueTextBox = (NumericUpDown)value;
        // using validating in case the window is closed before leaving the control which won't trigger a ValueChanged event
        alertValueTextBox.Validating += (s, e) => Settings.Default.SettingsFormAlertValue = (long)alertValueTextBox.Value;
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
        alertUnitComboBox.SelectedItem = Settings.Default.SettingsFormAlertUnit.ToString();
        alertUnitComboBox.SelectedValueChanged += (s, e) => Settings.Default.SettingsFormAlertUnit = Enum.Parse<Unit>((string)alertUnitComboBox.SelectedItem);
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
        alertRangeComboBox.SelectedItem = Settings.Default.SettingsFormAlertRange.ToString();
        alertRangeComboBox.SelectedValueChanged += (s, e) => Settings.Default.SettingsFormAlertRange = Enum.Parse<Range>((string)alertRangeComboBox.SelectedItem);
        Controls.Add(value);

        value = new Label
        {
            Location = value.Location + new Size(value.Width, 0),
            Text = "(zero to disable)",
            Width = label.Width * 2,
        };
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
        graphStyleComboBox.SelectedItem = Settings.Default.DetailsFormChartType.ToString();
        graphStyleComboBox.SelectedValueChanged += (s, e) => Settings.Default.DetailsFormChartType = Enum.Parse<SeriesChartType>((string)graphStyleComboBox.SelectedItem);
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
            Checked = Settings.Default.ApplicationSound,
            Height = 15,
            Location = label.Location + new Size(label.Width, 0),
            Width = 12,
        };
        var soundCheckBox = (CheckBox)value;
        soundCheckBox.CheckedChanged += (s, e) => Settings.Default.ApplicationSound = soundCheckBox.Checked;
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
            Checked = Settings.Default.ApplicationStartWithWindows,
            Height = 15,
            Location = label.Location + new Size(label.Width, 0),
            Width = 12,
        };
        var startWithWindowsCheckBox = (CheckBox)value;
        startWithWindowsCheckBox.CheckedChanged += (s, e) => Settings.Default.ApplicationStartWithWindows = startWithWindowsCheckBox.Checked;
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

        var settingsFile = Settings.FileName;
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

        // ***********************************************************************
        label = new Label
        {
            Location = label.Location + new Size(0, label.Height),
            Text = $"Version:",
            Width = label.Width,
        };
        Controls.Add(label);

        value = new Label
        {
            Location = label.Location + new Size(label.Width, 0),
            Text = "v0.2.4",
            Width = label.Width * 2,
        };
        Controls.Add(value);
    }
}
