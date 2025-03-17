using System.Net;
using System.Net.Sockets;
using Shared.Networking;
using Shared.Networking.Interfaces;
using Shared.Utils;

namespace DesServer.Networking.Protocols.Udp;

public class UdpProtocol(INetworkHandler dataHandler) : ANetworkProtocol(dataHandler)
{
    private UdpClient? _udpClient;
    public override Task Start(int port)
    {
        IsRunning = true;
        _udpClient = new UdpClient(port);
        Logs.Logger.Log($"UDP server started on port {port}");

        var receiveThread = new Thread(ReceiveData);
        receiveThread.Start();
        return Task.CompletedTask;
    }
    
    private void ReceiveData()
    {
        try
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (IsRunning)
            {
                var data = _udpClient?.Receive(ref remoteEndPoint);
                var sourceEndpoint = remoteEndPoint.ToString();
                if(data != null) DataHandler.OnDataReceived(data, sourceEndpoint);
            }
        }
        catch (SocketException ex)
        {
            Logs.Logger.Log($"UDP receive error: {ex.Message}");
        }
    }
    

    public override void Send(string data, string endpoint)
    {
        try
        {
            var parts = endpoint.Split(':');
            var ip = IPAddress.Parse(parts[0]);
            var port = int.Parse(parts[1]);
            var targetEndpoint = new IPEndPoint(ip, port);
            var bData = ByteUtils.GetBytesFromString(data);
            _udpClient?.Send(bData, bData.Length, targetEndpoint);
        }
        catch (Exception ex)
        {
            Logs.Logger.Log($"Failed to send to {endpoint}: {ex.Message}");
        }
    }
}