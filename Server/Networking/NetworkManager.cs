using Server.Logs;
using Server.Networking.Protocols.Tcp;
using Server.Networking.Protocols.Udp;
using Shared.Networking.Interfaces;

namespace Server.Networking
{
    public class NetworkManager : IDisposable
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

            Logger.Log("Network services started...");
        }

        private void Stop()
        {
            foreach (var protocol in _protocols)
            {
                protocol.Stop();
            }

            Logger.Log("Network services stopped.");
        }

        public void Dispose() => Stop();
    }
}