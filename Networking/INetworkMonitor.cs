using static Networking.NetworkMonitor;

namespace Networking;

public interface INetworkMonitor
{
    event UpdateHandler? OnUpdate;

    Task Run();
}
