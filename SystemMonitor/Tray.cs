using Microsoft.Win32;

using Networking;

using System.Runtime.InteropServices;

namespace SystemMonitor;

public partial class Tray : Form
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    extern static bool DestroyIcon(IntPtr handle);

    private readonly string startWithWindowsKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private readonly string startWithWindowsName = @"Achey\SystemMonitor";

    private readonly INetworkMonitor networkMonitor;
    private readonly NotifyIcon trayIcon;

    private Form? settingsForm = default;

    public Tray(INetworkMonitor networkMonitor)
    {
        this.networkMonitor = networkMonitor;
        this.networkMonitor.OnUpdate += UpdateNetworkData;

        trayIcon = new()
        {
            Text = "System Monitor",
            ContextMenuStrip = GetContextMenu(),
            Visible = true,
        };
        trayIcon.DoubleClick += OnDetails;

        UpdateIcon(-1);

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
        SetRegistryRunStatus();

        foreach (object item in trayIcon.ContextMenuStrip.Items)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                if (menuItem.Text == "Start With Windows")
                {
                    menuItem.Checked = Settings.Default.ApplicationStartWithWindows;
                }

                if (menuItem.Text == "Sound")
                {
                    menuItem.Checked = Settings.Default.ApplicationSound;
                }
            }
        }

        UpdateNetworkData();
    }

    private void UpdateNetworkData()
    {
        var limit = Settings.Default.SettingsFormAlertValue;

        if (limit == 0)
        {
            UpdateIcon(-1);
            return;
        }

        var unit = Settings.Default.SettingsFormAlertUnit;
        var range = Settings.Default.SettingsFormAlertRange;

        var logs = range switch
        {
            Range.Hour => networkMonitor.Logs.Where(log => log.Time >= DateTime.Now.RoundDown(TimeSpan.FromHours(1))),
            Range.Day => networkMonitor.Logs.Where(log => log.Time > DateTime.Today),
            Range.Month => networkMonitor.Logs.Where(log => log.Time >= DateTime.Now.StartOfMonth()),
            _ => throw new ArgumentOutOfRangeException("alertRange"),
        };

        var value = logs.Sum(log => log.BytesTotal) / (double)unit;

        UpdateIcon(value / limit);
    }

    private void UpdateIcon(double percentage)
    {
        if (trayIcon.Icon is not null)
        {
            trayIcon.Icon?.Dispose();
            DestroyIcon(trayIcon.Icon!.Handle);
        }

        trayIcon.Icon = IconCreator.CreateIcon(percentage);
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
        if (settingsForm is null)
        {
            settingsForm = new SettingsForm(networkMonitor);

            settingsForm.FormClosed += (s, e) =>
            {
                settingsForm = null;
                InitializeOptions();
                UpdateNetworkData();
            };

            settingsForm.Show();
        }
        else
        {
            settingsForm.BringToFront();
        }
    }

    private void OnSound(object? sender, EventArgs e)
    {
        var item = sender as ToolStripMenuItem
            ?? throw new NullReferenceException(nameof(sender));

        Settings.Default.ApplicationSound = item.Checked;
        Settings.Save();
    }

    private void OnStartWithWindows(object? sender, EventArgs e)
    {
        var item = sender as ToolStripMenuItem
            ?? throw new NullReferenceException(nameof(sender));

        Settings.Default.ApplicationStartWithWindows = item.Checked;
        Settings.Save();

        SetRegistryRunStatus();
    }

    private void SetRegistryRunStatus()
    {
        var key = Registry.CurrentUser.CreateSubKey(startWithWindowsKey);

        if (Settings.Default.ApplicationStartWithWindows)
        {
            key.SetValue(startWithWindowsName, Environment.ProcessPath!);
        }
        else
        {
            key.DeleteValue(startWithWindowsName, false);
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
