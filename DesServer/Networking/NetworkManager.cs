using DesServer.Services;

namespace DesServer.Networking
{
    public class NetworkManager(string ipAddress, int tcpPort, int udpPort)
    {
        private readonly TcpServerService _tcpServerService = new(ipAddress, tcpPort);
        private readonly UdpServerService _udpServerService = new(ipAddress, udpPort);

        public void Start()
        {
            // Start both TCP and UDP services
            Task.Run(() => _tcpServerService.Start());
            Task.Run(() => _udpServerService.Start());
        }
    }
}