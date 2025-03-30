using Shared.Networking.Interfaces;

namespace Shared.Networking;

public abstract class ANetworkProtocol(INetworkHandler dataHandler) : INetworkProtocol
{
    protected readonly INetworkHandler DataHandler = dataHandler;
    protected bool _isRunning;

    public abstract Task Start(int port);

    public virtual bool IsRunning => _isRunning;
    
    public virtual void Stop()
    {
        _isRunning = false;
    }

    public abstract void Send(string data, string endpoint = "");
    public virtual void Dispose() => Stop();

    protected static void ValidateData(byte[]? data)
    {
        if(data == null || data.Length == 0)
            throw new NullReferenceException();
    }
}