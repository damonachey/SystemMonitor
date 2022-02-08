namespace SystemMonitorTray;

public partial class Tray : Form
{
    private readonly NotifyIcon trayIcon;
    private bool sound = true;

    public Tray()
    {
        var contextMenuStrip = new ContextMenuStrip();
        var soundMenuItem = new ToolStripMenuItem("Sound") { Checked = true, CheckOnClick = true };
        soundMenuItem.Click += OnSound;

        contextMenuStrip.Items.Add(soundMenuItem);
        contextMenuStrip.Items.Add("Exit", null, OnExit);

        trayIcon = new()
        {
            Text = "SysTrayApp",
            
            Icon = Properties.Resources.graph,

            ContextMenuStrip = contextMenuStrip,
            Visible = true,
        };
    }

    protected override void OnLoad(EventArgs e)
    {
        Visible = false; // Hide form window.
        ShowInTaskbar = false; // Remove from task bar.

        base.OnLoad(e);
    }

    private void OnSound(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem soundMenuItem)
            sound = soundMenuItem?.Checked ?? false;

        if (sound) Console.Beep();
    }

    private void OnExit(object? sender, EventArgs e)
    {
        Application.Exit();
    }

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing) trayIcon.Dispose();

        base.Dispose(isDisposing);
    }
}
