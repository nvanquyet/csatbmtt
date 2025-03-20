namespace Shared.Networking.Interfaces;

public interface INetworkHandler
{
    void OnDataReceived(string message, string sourceEndpoint);
    void OnDataReceived(byte[] message, string sourceEndpoint);
    void OnClientDisconnect<T>(T? client) where T : class;
    void OnClientConnected<T>(T? client) where T : class;
}