namespace Shared.Networking.Interfaces;

public interface INetworkHandler
{
    void OnDataReceived(byte[] data, string sourceEndpoint);
    
    void OnDataReceived(string message, string sourceEndpoint);

    void OnClientConnected<T>(string id,T? client) where T : class;
}