using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared.AppSettings;
using Shared.Networking;
using Shared.Networking.Interfaces;
using Shared.Utils;

namespace DesClient.Network.Tcp;

public class TcpProtocol(INetworkHandler dataHandler) : ANetworkProtocol(dataHandler)
{
    private TcpClient? _tcpClient;

    public override Task Start(int port)
    {
        IsRunning = true;
        _tcpClient = new TcpClient(Config.ServerIp, Config.ServerTcpPort);
        _ = ListenForTcpMessagesAsync();
        return Task.CompletedTask;
    }

    private async Task ListenForTcpMessagesAsync()
    {
        var buffer = new byte[1024];
        var messageBuilder = new StringBuilder();
        if (_tcpClient != null)
        {
            var stream = _tcpClient.GetStream();
            while (IsRunning)
            {
                try
                {
                    var bytesRead = await stream.ReadAsync(buffer);
                    if (bytesRead <= 0) continue;
                    messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    // Kiểm tra kiểu dữ liệu trong "data"
                    var receivedMessage = messageBuilder.ToString();

                    // Kiểm tra nếu dữ liệu là đối tượng hoặc mảng
                    if ((receivedMessage.Contains("\"data\":\"") || receivedMessage.Contains("\"data\":{")) &&
                        receivedMessage.Contains('}'))
                    {
                        if (!receivedMessage.Contains('}')) continue;
                        DataHandler?.OnDataReceived(ByteUtils.GetBytesFromString(receivedMessage), "");
                        messageBuilder.Clear();
                    }
                    else if (receivedMessage.Contains("\"data\":[{") && receivedMessage.Contains("}]}"))
                    {
                        if (!receivedMessage.Contains("}]}")) continue;
                        DataHandler?.OnDataReceived(ByteUtils.GetBytesFromString(receivedMessage), "");
                        messageBuilder.Clear(); // Xóa dữ liệu đã xử lý
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading TCP message: " + ex.Message);
                    break;
                }
            }
        }
    }


    public override void Send(string data, string endpoint)
    {
        var tcpStream = _tcpClient?.GetStream();
        tcpStream?.WriteAsync(ByteUtils.GetBytesFromString(data), 0, data.Length);
    }
    
}