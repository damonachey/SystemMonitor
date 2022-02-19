using System.Text.Json;
using System.Text.Json.Serialization;

namespace Networking;

[System.Diagnostics.DebuggerDisplay("{Time}: Received({BytesReceived}), Sent({BytesSent})")]
public class Log
{
    public DateTime Time { get; set; }
    public long BytesReceived { get; set; }
    public long BytesSent { get; set; }

    [JsonIgnore]
    public long BytesTotal => BytesReceived + BytesSent;

    public override string ToString() => JsonSerializer.Serialize(this);
 
    public static Log Parse(string line) => JsonSerializer.Deserialize<Log>(line)!;
}
