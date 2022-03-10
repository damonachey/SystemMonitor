using System.Text.Json;
using System.Text.Json.Serialization;

namespace Networking;

[System.Diagnostics.DebuggerDisplay("{Time}: BytesReceived({BytesReceived}), BytesSent({BytesSent}), CumulativeBytesReceived({CumulativeBytesReceived}), CumulativeBytesSent({CumulativeBytesSent})")]
public class Log
{
    public DateTime Time { get; init; }
    public long BytesReceived { get; set; }
    public long BytesSent { get; set; }
    public long CumulativeBytesReceived { get; set; }
    public long CumulativeBytesSent { get; set; }

    [JsonIgnore]
    public long BytesTotal => BytesReceived + BytesSent;

    public override string ToString() => JsonSerializer.Serialize(this);
 
    public static Log Parse(string line) => JsonSerializer.Deserialize<Log>(line) 
        ?? throw new NullReferenceException(nameof(line));
}
