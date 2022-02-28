using Microsoft.Win32;

using Networking;

using System.Reflection;

namespace SystemMonitorTray;

public partial class Tray : Form
{
    private readonly string startWithWindowsKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private readonly string startWithWindowsName = @"Achey\SystemMonitor";

    private readonly INetworkMonitor networkMonitor;
    private readonly NotifyIcon trayIcon;

    public Tray(INetworkMonitor networkMonitor)
    {
        this.networkMonitor = networkMonitor;
        this.networkMonitor.OnUpdate += UpdateNetworkData;

        trayIcon = new()
        {
            Text = "System Monitor",
            Icon = IconCreator.CreateIcon(-1),
            ContextMenuStrip = GetContextMenu(),
            Visible = true,
        };
        trayIcon.DoubleClick += (s, e) => new DetailsForm(networkMonitor).Show();
        Properties.Settings.Default.PropertyChanged += (s, e) => Properties.Settings.Default.Save();

        InitializeOptions();
    }

    private ContextMenuStrip GetContextMenu()
    {
        var contextMenuStrip = new ContextMenuStrip();

        var detailsMenuItem = new ToolStripMenuItem("Details");
        var settingsMenuItem = new ToolStripMenuItem("Settings");
        var soundMenuItem = new ToolStripMenuItem("Sound") { Checked = true, CheckOnClick = true };
        var startWithWindowsItem = new ToolStripMenuItem("Start With Windows") { Checked = false, CheckOnClick = true };

        detailsMenuItem.Click += OnDetails;
        settingsMenuItem.Click += OnSettings;
        soundMenuItem.Click += OnSound;
        startWithWindowsItem.Click += OnStartWithWindows;

        contextMenuStrip.Items.Add(detailsMenuItem);
        contextMenuStrip.Items.Add("-");
        contextMenuStrip.Items.Add(settingsMenuItem);
        contextMenuStrip.Items.Add(soundMenuItem);
        contextMenuStrip.Items.Add(startWithWindowsItem);
        contextMenuStrip.Items.Add("-");
        contextMenuStrip.Items.Add("Exit", null, OnExit);

        return contextMenuStrip;
    }

    private void InitializeOptions()
    {
        var key = Registry.CurrentUser.CreateSubKey(startWithWindowsKey);
        Properties.Settings.Default.applicationStartWithWindows = key.GetValue(startWithWindowsName) != null;

        foreach (object item in trayIcon.ContextMenuStrip.Items)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                if (menuItem.Text == "Start With Windows")
                {
                    menuItem.Checked = Properties.Settings.Default.applicationStartWithWindows;
                }

                if (menuItem.Text == "Sound")
                {
                    menuItem.Checked = Properties.Settings.Default.applicationSound;
                }
            }
        }
     
        UpdateNetworkData();
    }

    private void UpdateNetworkData()
    {
        var limit = Properties.Settings.Default.settingsFormAlertValue;

        if (limit == 0)
        {
            trayIcon.Icon.Dispose();
            trayIcon.Icon = IconCreator.CreateIcon(-1);
            return;
        }

        var unit = Enum.Parse<Unit>(Properties.Settings.Default.settingsFormAlertUnit);
        var range = Enum.Parse<Range>(Properties.Settings.Default.settingsFormAlertRange);

        var logs = range switch
        {
            Range.Hour => networkMonitor.Logs.Where(log => log.Time >= DateTime.Now.RoundDown(TimeSpan.FromHours(1))),
            Range.Day => networkMonitor.Logs.Where(log => log.Time > DateTime.Today),
            Range.Month => networkMonitor.Logs.Where(log => log.Time >= DateTime.Now.StartOfMonth()),
            _ => throw new ArgumentOutOfRangeException("alertRange"),
        };

        var value = logs.Sum(log => log.BytesTotal) / (double)unit;

        trayIcon.Icon.Dispose();
        trayIcon.Icon = IconCreator.CreateIcon(value / limit);

        //
        //    // TODO: allow show balloon every... 5GB used?
        //    trayIcon.ShowBalloonTip(10000, "Test Title", $"Used {current} MB", ToolTipIcon.Info);
        //
        //}
    }

    protected override void OnLoad(EventArgs e)
    {
        Visible = false; // Hide form window.
        ShowInTaskbar = false; // Remove from task bar.

        base.OnLoad(e);
    }

    private void OnDetails(object? sender, EventArgs e)
    {
        new DetailsForm(networkMonitor).Show();
    }

    private void OnSettings(object? sender, EventArgs e)
    {
        var settingsForm = new SettingsForm(networkMonitor);

        settingsForm.FormClosed += (s, e) =>InitializeOptions();
        settingsForm.ShowDialog();
    }

    private void OnSound(object? sender, EventArgs e)
    {
        var item = (ToolStripMenuItem)sender!;

        Properties.Settings.Default.applicationSound = item.Checked;

        if (Properties.Settings.Default.applicationSound)
        {
            Console.Beep();
        }
    }

    private void OnStartWithWindows(object? sender, EventArgs e)
    {
        var item = (ToolStripMenuItem)sender!;

        Properties.Settings.Default.applicationStartWithWindows = item.Checked;

        var key = Registry.CurrentUser.CreateSubKey(startWithWindowsKey);
        if (Properties.Settings.Default.applicationStartWithWindows)
        {
            key.SetValue(startWithWindowsName, Assembly.GetEntryAssembly()?.Location!);
        }
        else
        {
            key.DeleteValue(startWithWindowsName);
        }
    }

    private void OnExit(object? sender, EventArgs e)
    {
        Application.Exit();
    }

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing) trayIcon.Dispose();
        if (isDisposing) networkMonitor.OnUpdate -= UpdateNetworkData;

        base.Dispose(isDisposing);
    }
}
