// See https://aka.ms/new-console-template for more information

using SystemMonitorConsole;
using SystemMonitorConsole.NetworkMonitor;

var refreshDelay = TimeSpan.FromSeconds(5);

var monitors = new[]
{
    new NetworkMonitor()
};

while (true)
{
    Console.Clear();

    foreach (var monitor in monitors)
    {
        switch (monitor.Status())
        {
            case Statuses.Warning: 
                Console.Beep(); 
                break;

            case Statuses.Error: 
                Console.Beep(); 
                Console.Beep(); 
                Console.Beep(); 
                break;

            default: 
                break;
        }

        Console.WriteLine();
    }

    Console.WriteLine($"Updated: {DateTime.Now}");

    await Task.Delay(refreshDelay);
}
