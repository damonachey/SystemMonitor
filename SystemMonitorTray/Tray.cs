using Microsoft.Win32;

using Networking;

using System.Reflection;

namespace SystemMonitorTray;

public partial class Tray : Form
{
    private readonly string appKey = @"Software\Achey\SystemMonitor";
    private readonly string runKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private readonly string runName = @"Achey\SystemMonitor";

    private readonly INetworkMonitor networkMonitor;
    private readonly NotifyIcon trayIcon;
    
    private bool sound = true;
    private bool startWithWindows = false;

    public Tray(INetworkMonitor networkMonitor)
    {
        this.networkMonitor = networkMonitor;
        this.networkMonitor.OnUpdate += UpdateNetworkData;

        trayIcon = new()
        {
            Text = "System Monitor",
            Icon = IconHelper.GenerateIcon("N/A"),
            ContextMenuStrip = GetContextMenu(),
            Visible = true,
        };

        InitializeOptions();

        // TODO: remove after debugging
        new Details(networkMonitor).Show();
    }

    private void InitializeOptions()
    {
        var key = Registry.CurrentUser.CreateSubKey(appKey);
        sound = bool.Parse((string)key.GetValue("Sound", true.ToString())!);
        
        key = Registry.CurrentUser.CreateSubKey(runKey);
        startWithWindows = key.GetValue(runName) != null;

        foreach (object item in trayIcon.ContextMenuStrip.Items)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                if (menuItem.Text == "Start With Windows")
                {
                    menuItem.Checked = startWithWindows;
                }

                if (menuItem.Text == "Sound")
                {
                    menuItem.Checked = sound;
                }
            }
        }
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

    protected override void OnLoad(EventArgs e)
    {
        Visible = false; // Hide form window.
        ShowInTaskbar = false; // Remove from task bar.

        base.OnLoad(e);
    }

    long count = 0;

    private void UpdateNetworkData()
    {
        count++;

        trayIcon.Icon = IconHelper.GenerateIcon($"{count}");

        //var current = (long)(NetworkMonitor.GetDetails().ByteReceivedLast1m / 1e6);
        //
        //if (current != last)
        //{
        //    last = current;
        //
        //    trayIcon.Icon = IconHelper.GenerateIcon($"{current}");
        //
        //    // TODO: allow show balloon every... 5GB used?
        //    trayIcon.ShowBalloonTip(10000, "Test Title", $"Used {current} MB", ToolTipIcon.Info);
        //
        //}
    }

    private void OnDetails(object? sender, EventArgs e)
    {
        new Details(networkMonitor).Show();
    }

    private void OnSettings(object? sender, EventArgs e)
    {
        new Settings().Show();
    }

    private void OnSound(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item)
        {
            sound = item?.Checked ?? false;
        }

        var key = Registry.CurrentUser.CreateSubKey(appKey);
        key.SetValue("Sound", sound.ToString());

        if (sound)
        {
            Console.Beep();
        }
    }

    private void OnStartWithWindows(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item)
        {
            startWithWindows = item?.Checked ?? false;
        }

        var key = Registry.CurrentUser.CreateSubKey(runKey);
        if (startWithWindows)
        {
            key.SetValue(runName, Assembly.GetEntryAssembly()?.Location!);
        }
        else
        {
            key.DeleteValue(runName);
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
