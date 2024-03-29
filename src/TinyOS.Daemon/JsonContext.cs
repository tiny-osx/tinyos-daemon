using System.Text.Json.Serialization;

namespace TinyOS.Daemon;

[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(HostInterface))]
[JsonSerializable(typeof(List<AdaptorInterface>))]
internal partial class JsonContext : JsonSerializerContext
{
}

public class HostInterface()
{
    public required string Host { get; set; }
    public required string BoardType { get; set; }
    public required List<AdaptorInterface> AdaptorInterfaces { get; set; }
}

public class AdaptorInterface()
{
    public required string Name { get; set; }
    public required List<string> IPv4Address { get; set; }
    public required List<string> IPv6Address { get; set; }
    public required int  Priority  { get; set; }
}