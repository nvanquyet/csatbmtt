namespace Shared.Networking.Interfaces;

public interface INetworkProtocol : IDisposable
{
    void Start(int port);
    void Stop();
    void Send(byte[] data, string endpoint);
}