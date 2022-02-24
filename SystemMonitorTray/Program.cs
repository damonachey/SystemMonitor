using Networking;

using System.Configuration;
using System.Diagnostics;

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

        // Add the event handler for handling UI thread exceptions to the event.
        Application.ThreadException += Application_ThreadException;

        // Set the unhandled exception mode to force all Windows Forms errors to go through our handler.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        // Add the event handler for handling non-UI thread exceptions to the event. 
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        var logPath = Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);

        var networkMonitor = new NetworkMonitor(logPath!);

        // if this call ever returns it will only be for an error condition
        networkMonitor
            .Start()
            .ContinueWith(task => UserCrashMessage(task.Exception!));

        var application = new Tray(networkMonitor);

        Application.Run(application);
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
        UserCrashMessage(e.Exception);
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        UserCrashMessage((Exception)e.ExceptionObject);
    }

    private static void UserCrashMessage(Exception ex)
    {
        var dialogResult = MessageBox.Show(
            "!!! SystemMonitor failed and will exit !!!\n\n" +
            "Would you like to send a crash report?\n\n" +
            "(If you choose 'OK' your email application will open with a crash message.  Just hit 'Send' to complete the report submission.)",
            "System Monitor Error",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Error);

        if (dialogResult == DialogResult.OK)
        {
            var str = $"mailto:SystemMonitor@Achey.Net?subject=Crash Report&body={Uri.EscapeDataString(ex.ToString())}";
            Process.Start(new ProcessStartInfo(str) { UseShellExecute = true });
        }

        Application.Exit();
    }
}
