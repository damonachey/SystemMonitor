using static Networking.NetworkMonitor;

namespace Networking;

public interface INetworkMonitor
{
    event UpdateHandler? OnUpdate;

    IList<Log> Logs { get; }

    Task Run();
}
