namespace DesServer.Models
{
    public enum ProtocolType
    {
        Tcp,
        Udp
    }

    public class ProtocolRequest(string message, ProtocolType protocol)
    {
        public ProtocolType Protocol { get; set; } = protocol;
        public string Message { get; set; } = message;
    }
}