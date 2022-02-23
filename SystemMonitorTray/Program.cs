using Networking;

namespace SystemMonitorTray;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        var networkMonitor = new NetworkMonitor();
        _ = networkMonitor.Start();
        
        var application = new Tray(networkMonitor);

        Application.Run(application);
    }
}
