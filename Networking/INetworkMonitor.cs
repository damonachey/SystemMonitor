using static Networking.NetworkMonitor;

namespace Networking;

public interface INetworkMonitor
{
    event UpdateHandler? OnUpdate;

    string LogFileName { get; }
    TimeSpan PollingInterval { get; }
    List<Log> Logs { get; }

    Task Start();
}
