using DesClient.Menu;
using DesClient.Network.Tcp;
using DesClient.Network.Udp;
using DesClient.Services;
using Shared.Networking.Interfaces;
using Shared.Utils.Patterns;

namespace DesClient.Network;

public class NetworkManager : Singleton<NetworkManager>
{
    public readonly INetworkProtocol TcpService = new TcpProtocol(new TcpHandler());
    public readonly INetworkProtocol UdpService = new UdpProtocol(new UdpHandler());

    public NetworkManager()
    {
        Console.WriteLine("Connecting to server...");
        TcpService.Start(0);
        Console.WriteLine("Connected to TCP Server!");
    }
}