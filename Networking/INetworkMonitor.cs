using static Networking.NetworkMonitor;

namespace Networking;

public interface INetworkMonitor
{
    event UpdateHandler? OnUpdate;

    List<Log> Logs { get; }

    Task Run();
}
