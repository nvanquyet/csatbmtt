using Shared.Networking;
using Shared.Networking.Interfaces;

namespace DesClient.Network.Udp;

public class UdpProtocol(INetworkHandler dataHandler) : ANetworkProtocol(dataHandler)
{
    public override Task Start(int port)
    {
        throw new NotImplementedException();
    }


    public override void Send(string data, string endpoint = "")
    {
        throw new NotImplementedException();
    }
}