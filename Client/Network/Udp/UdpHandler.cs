using Shared.Networking.Interfaces;

namespace Client.Network.Udp;

public class UdpHandler : INetworkHandler
{
    public void OnDataReceived(byte[] data, string sourceEndpoint)
    {
        throw new NotImplementedException();
    }

    public void OnClientDisconnect<T>(T? client) where T : class
    {
        throw new NotImplementedException();
    }

    public void OnDataReceived(string message, string sourceEndpoint)
    {
        throw new NotImplementedException();
    }

    public void OnClientDisconnect<T>(string id, T? client) where T : class
    {
        throw new NotImplementedException();
    }

    public void OnClientConnected<T>(T? client) where T : class
    {
        throw new NotImplementedException();
    }

    public void BroadcastMessage(string message)
    {
        throw new NotImplementedException();
    }

    public void BroadcastMessageExcept<T>(T? excludedClient, string message) where T : class
    {
        throw new NotImplementedException();
    }

    public void BroadcastMessageExcept<T>(T[] excludedClient, string message) where T : class
    {
        throw new NotImplementedException();
    }
}