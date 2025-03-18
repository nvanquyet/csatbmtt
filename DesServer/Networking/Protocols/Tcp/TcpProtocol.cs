using System.Net;
using System.Net.Sockets;
using Shared.Networking;
using Shared.Networking.Interfaces;

namespace DesServer.Networking.Protocols.Tcp;

public class TcpProtocol(INetworkHandler dataHandler) : ANetworkProtocol(dataHandler)
{
    private TcpListener? _listener;
    private Thread? _listenerThread;

    public override Task Start(int port)
    {
        IsRunning = true;
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();

        _listenerThread = new Thread(ListenForClients);
        _listenerThread.Start();

        Logs.Logger.Log($"TCP server started on port {port}");
        return Task.CompletedTask;
    }

    private void ListenForClients()
    {
        try
        {
            while (IsRunning)
            {
                var client = _listener?.AcceptTcpClient();
                if (client == null) continue;
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
        var stream = client.GetStream();
        try
        {
            var buffer = new byte[4096];
            while (IsRunning && client.Connected)
            {
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) continue;
    
                var receivedData = new byte[bytesRead];
                Array.Copy(buffer, receivedData, bytesRead);
                Console.WriteLine($"Received {receivedData.Length} bytes from {client.Client.RemoteEndPoint}");
                DataHandler.OnDataReceived(receivedData, "");
            }
        }
        catch (Exception ex)
        {
            Logs.Logger.Log($"Client {client.Client.RemoteEndPoint} error: {ex.Message}");
        }
    }

    public override void Send(string data, string endpoint = "")
    {
    }

    public override void Stop()
    {
        base.Stop();
        _listener?.Stop();
    }
}