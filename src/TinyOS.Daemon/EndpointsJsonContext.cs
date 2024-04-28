using System.Net;
using System.Text.Json.Serialization;

namespace TinyOS.Daemon;

[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string[]))]
internal partial class EndpointsJsonContext : JsonSerializerContext
{
}
