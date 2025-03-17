namespace Shared.Networking.Interfaces;

public interface INetworkProtocol : IDisposable
{
    Task Start(int port);
    void Stop();
    void Send(byte[] data, string endpoint);
    void Send(string data, string endpoint);
}