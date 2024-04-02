namespace TinyOS.Build.Serialization
{
    public class AdaptorInterface()
    {
        public string Name { get; set; } = "unknown";
        public List<string> IPv4Address { get; set; } = new List<string>() { "192.168.7.1" };
        public List<string> IPv6Address { get; set; } = new List<string>() { "::" };
        public int  Priority  { get; set; } = 0;
    }
}