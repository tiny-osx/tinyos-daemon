using System.Text.Json.Serialization;

namespace TinyOS.Build.Serialization
{
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(HostInterface))]
    [JsonSerializable(typeof(List<AdaptorInterface>))]
    public partial class InterfaceContext : JsonSerializerContext
    {
    }
}