using Server.Networking.Protocols.Tcp;
using Server.Networking.Protocols.Udp;
using Shared;
using Shared.Networking.Interfaces;
namespace Server.Networking
{
    public class NetworkManager
    {
        private readonly List<INetworkProtocol> _protocols = new();

        public NetworkManager(int tcpPort, int udpPort)
        {
            // Khởi tạo TCP và UDP
            var tcpProtocol = new TcpProtocol(new TcpHandler());
            var udpProtocol = new UdpProtocol(new UdpHandler());

            _protocols.Add(tcpProtocol);
            _protocols.Add(udpProtocol);

            tcpProtocol.Start(tcpPort);
            udpProtocol.Start(udpPort);
        }

        public void Stop()
        {
            foreach (var protocol in _protocols)
            {
                protocol.Stop();
            }

            foreach (var protocol in _protocols)
            {
                protocol.Stop();
            }

            Logger.LogWarning("Network services stopped.");
        }
    }
}