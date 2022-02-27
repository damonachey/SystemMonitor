using static Networking.NetworkMonitor;

namespace Networking;

public interface INetworkMonitor
{
    event UpdateHandler? OnUpdate;

    string LogFile { get; }
    TimeSpan PollInterval { get; }
    List<Log> Logs { get; }

    Task Start();
}
