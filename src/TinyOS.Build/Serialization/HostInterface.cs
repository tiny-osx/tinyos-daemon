namespace TinyOS.Build.Serialization
{
    public class HostInterface()
    {
        public string Host { get; set; } = "unknown";
        public int Port { get; set; } = 8920;
        public string BoardType { get; set; } = "unknown";
        public List<AdaptorInterface>? AdaptorInterfaces { get; set; } = new List<AdaptorInterface>();
    }
}