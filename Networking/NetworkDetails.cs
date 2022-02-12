namespace Networking;

public record NetworkDetails(
    TimeSpan SystemUptime,
    long BytesReceivedSinceStartup,
    long BytesSentSinceStartup,
    long ByteReceivedLast1m,
    long ByteSentLast1m);
