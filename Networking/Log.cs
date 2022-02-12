using System.Diagnostics;

namespace Networking;

[DebuggerDisplay("{Time}: Received({BytesReceived}), Sent({BytesSent})")]
public class Log
{
    public DateTime Time { get; internal set; }
    public long BytesReceived { get; internal set; }
    public long BytesSent { get; internal set; }
}
