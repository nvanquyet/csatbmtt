using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;
using Shared.AppSettings;
using Shared.Networking;
using Shared.Networking.Interfaces;
namespace Server.Networking.Protocols.Tcp;

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

        Logger.LogInfo($"TCP server started on port {port}");
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
            Logger.LogError($"TCP listener error: {ex.Message}");
        }
    }
    
    private void HandleClient(TcpClient client)
    {
        var stream = client.GetStream();
        var endPoint = client?.Client.RemoteEndPoint?.ToString();
        var messageBuilder = new StringBuilder();
        DataHandler.OnClientConnected(client);
        try
        {
            var buffer = new byte[Config.BufferSizeLimit];
            while (IsRunning && client is { Connected: true })
            {
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) continue;
                messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                while (true)
                {
                    var temp = messageBuilder.ToString();
                    var endIndex = temp.IndexOf('\n');
                    if (endIndex == -1) break; 
                    var completeJson = temp[..endIndex].Trim();
                    messageBuilder.Remove(0, endIndex + 1);

                    if (string.IsNullOrEmpty(completeJson)) continue;
                    if (endPoint != null)
                        DataHandler.OnDataReceived(completeJson, endPoint);
                }
            }
        }
        catch (Exception ex)
        {
            DataHandler?.OnClientDisconnect(client);
            Logger.LogError($"Client {client?.Client.RemoteEndPoint} error: {ex.Message}");
        }
    }

    public override void Send(string data, string endpoint = "") { }

    public override void Stop()
    {
        base.Stop();
        _listener?.Stop();
    }
}