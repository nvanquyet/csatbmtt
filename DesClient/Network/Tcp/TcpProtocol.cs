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

    public override void Start(int port)
    {
        IsRunning = true;
        _tcpClient = new TcpClient(Config.ServerIp, Config.ServerTcpPort);
        _ = ListenForTcpMessagesAsync();
    }

    private async Task ListenForTcpMessagesAsync()
    {
        byte[] buffer = new byte[1024];
        StringBuilder messageBuilder = new StringBuilder();
        if (_tcpClient != null)
        {
            var stream = _tcpClient.GetStream();
            while (IsRunning)
            {
                try
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) continue;
                    if (bytesRead > 0)
                    {
                        messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                        // Kiểm tra kiểu dữ liệu trong "data"
                        string receivedMessage = messageBuilder.ToString();

                        // Kiểm tra nếu dữ liệu là đối tượng hoặc mảng
                        if ((receivedMessage.Contains("\"data\":\"") || receivedMessage.Contains("\"data\":{")) &&
                            receivedMessage.Contains("}"))
                        {
                            if (receivedMessage.Contains("}"))
                            {
                                DataHandler?.OnDataReceived(ByteUtils.GetBytesFromString(receivedMessage), "");
                                messageBuilder.Clear();
                            }
                        }
                        else if (receivedMessage.Contains("\"data\":[{") && receivedMessage.Contains("}]}"))
                        {
                            if (receivedMessage.Contains("}]}"))
                            {
                                DataHandler?.OnDataReceived(ByteUtils.GetBytesFromString(receivedMessage), "");
                                messageBuilder.Clear(); // Xóa dữ liệu đã xử lý
                            }
                        }
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

    public override void Send(byte[] data, string endpoint)
    {
        var tcpStream = _tcpClient?.GetStream();
        tcpStream?.Write(data, 0, data.Length);
    }
}