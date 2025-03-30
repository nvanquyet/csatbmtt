namespace Shared.Networking.Interfaces;

public interface INetworkProtocol : IDisposable
{
    Task Start(int port);
    void Stop();
    void Send(string data, string endpoint = "");
    
    bool IsRunning { get; }
}