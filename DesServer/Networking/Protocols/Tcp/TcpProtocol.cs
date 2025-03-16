using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Shared.Networking;
using Shared.Networking.Interfaces;

namespace DesServer.Networking.Protocols.Tcp;

public class TcpProtocol(INetworkHandler dataHandler) : ANetworkProtocol(dataHandler)
{
    private TcpListener? _listener;
    private Thread? _listenerThread;
    private readonly ConcurrentDictionary<string, TcpClient> _clients = new();

    public override void Start(int port)
    {
        IsRunning = true;
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();

        _listenerThread = new Thread(ListenForClients);
        _listenerThread.Start();

        Logs.Logger.Log($"TCP server started on port {port}");
    }

    private void ListenForClients()
    {
        try
        {
            while (IsRunning)
            {
                var client = _listener?.AcceptTcpClient();
                var endpoint = client?.Client.RemoteEndPoint?.ToString();
                if (client == null) continue;
                if (endpoint != null) _clients.TryAdd(endpoint, client);
                new Thread(() => HandleClient(client)).Start();
            }
        }
        catch (SocketException ex)
        {
            Logs.Logger.Log($"TCP listener error: {ex.Message}");
        }
    }

    private void HandleClient(TcpClient client)
    {
        //Client Connected
        DataHandler?.OnClientConnected(client);
        
        var endpoint = client.Client.RemoteEndPoint?.ToString();
        var stream = client.GetStream();
        
        try
        {
            byte[] buffer = new byte[4096];
            while (IsRunning && client.Connected)
            {
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) continue;

                var receivedData = new byte[bytesRead];
                Array.Copy(buffer, receivedData, bytesRead);
                if (endpoint != null) DataHandler?.OnDataReceived(receivedData, endpoint);
            }
        }
        catch (Exception ex)
        {
            Logs.Logger.Log($"Client {endpoint} error: {ex.Message}");
        }
        finally
        {
            if (endpoint != null) _clients.TryRemove(endpoint, out _);
            client.Dispose();
        }
    }

    public override void Send(byte[] data, string endpoint)
    {
        ValidateData(data);

        if (!_clients.TryGetValue(endpoint, out var client) || !client.Connected) return;
        try
        {
            client.GetStream().Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Logs.Logger.Log($"Failed to send to {endpoint}: {ex.Message}");
        }
    }

    public override void Stop()
    {
        base.Stop();
        _listener?.Stop();
        foreach (var client in _clients.Values) client.Dispose();
        _clients.Clear();
    }
}