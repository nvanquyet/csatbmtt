using System.Net.Sockets;
using System.Text;
using System.Text.Json;
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
                // Trong phương thức ListenForTcpMessagesAsync()
                try
                {
                    var bytesRead = await stream.ReadAsync(buffer);
                    if (bytesRead <= 0) continue;

                    // Thêm dữ liệu mới vào messageBuilder
                    messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    // Kiểm tra nếu JSON hợp lệ
                    var receivedMessage = messageBuilder.ToString().Trim();
                    Console.WriteLine(receivedMessage);

                    if (!IsCompleteJson(receivedMessage)) continue;
                    var data = ByteUtils.GetBytesFromString(receivedMessage);
                    DataHandler?.OnDataReceived(data, "");
                    messageBuilder.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Error reading TCP message: " + ex.Message);
                    break;
                }
            }
        }
    }

    // 🛠 Hàm kiểm tra JSON hợp lệ (chấp nhận cả object {} và array [])
    private bool IsCompleteJson(string receivedMessage)
    {
        try
        {
            // Thử phân tích JSON
            JsonDocument.Parse(receivedMessage);
        
            // Kiểm tra cụ thể cho cấu trúc với data là mảng
            if (receivedMessage.Contains("\"data\":["))
            {
                return receivedMessage.TrimEnd().EndsWith("}]}");
            }
            // Kiểm tra cho cấu trúc với data là chuỗi hoặc object
            if (receivedMessage.Contains("\"data\":\"") || receivedMessage.Contains("\"data\":{"))
            {
                return receivedMessage.TrimEnd().EndsWith("}");
            }

            return true; // Nếu JSON hợp lệ nhưng không thuộc các trường hợp cụ thể trên
        }
        catch
        {
            Console.WriteLine("Json invalid");
            return false; // JSON không hợp lệ
        }
    }

    public override void Send(string data, string endpoint = "")
    {
        Console.WriteLine($"data: {data} {_tcpClient?.Client.RemoteEndPoint}");
        var tcpStream = _tcpClient?.GetStream();
        _ = tcpStream?.WriteAsync(ByteUtils.GetBytesFromString(data), 0, data.Length);
        tcpStream?.Flush();
    }
}