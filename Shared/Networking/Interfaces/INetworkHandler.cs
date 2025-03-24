namespace Shared.Networking.Interfaces;

public interface INetworkHandler
{
    void OnDataReceived(string message, string sourceEndpoint);
    void OnDataReceived(byte[] message, string sourceEndpoint);
    void OnClientDisconnect<T>(T? client) where T : class;
    void OnClientConnected<T>(T? client) where T : class;

    void BroadcastMessage(string message);

    void BroadcastMessageExcept<T>(T? excludedClient, string message) where T : class;
    void BroadcastMessageExcept<T>(T[] excludedClient, string message) where T : class;

}