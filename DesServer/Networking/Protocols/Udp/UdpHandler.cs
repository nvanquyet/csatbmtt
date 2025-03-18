using Shared.Networking.Interfaces;

namespace DesServer.Networking.Protocols.Udp;

public class UdpHandler : INetworkHandler
{
    public void OnDataReceived(byte[] data, string sourceEndpoint)
    {
        throw new NotImplementedException();
    }

    public void OnDataReceived(string message, string sourceEndpoint)
    {
        throw new NotImplementedException();
    }

    public void OnClientConnected<T>(string id,T? client) where T : class
    {
        throw new NotImplementedException();
    }

    public void OnClientConnected<T>(T? client) where T : class
    {
        throw new NotImplementedException();
    }
}