using Client.Network.Tcp;
using Client.Network.Udp;
using Shared;
using Shared.Networking.Interfaces;
using Shared.Utils.Patterns;

namespace Client.Network;

public class NetworkManager : Singleton<NetworkManager>
{
    public readonly INetworkProtocol TcpService = new TcpProtocol(new TcpHandler());
    public readonly INetworkProtocol UdpService = new UdpProtocol(new UdpHandler());

    public NetworkManager()
    {
        Logger.LogInfo("Connecting to server...");
        TcpService.Start(0);
    }
}