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
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;

    public override async Task Start(int port)
    {
        IsRunning = true;
        _tcpClient = new TcpClient();

        try
        {
            await _tcpClient.ConnectAsync(Config.ServerIp, Config.ServerTcpPort);
            _stream = _tcpClient.GetStream();
            _cts = new CancellationTokenSource();

            Console.WriteLine($"✅ Kết nối đến server: {_tcpClient.Client.RemoteEndPoint}");

            _ = Task.Run(() => ListenForTcpMessagesAsync(_cts.Token)); // Khởi chạy lắng nghe
        }
        catch (Exception e)
        {
            Console.WriteLine($"❌ Lỗi kết nối TCP: {e.Message}");
            throw;
        }
    }

    private async Task ListenForTcpMessagesAsync(CancellationToken token)
    {
        var buffer = new byte[1024];
        var messageBuilder = new StringBuilder();

        if (_tcpClient is null || _stream is null)
        {
            Console.WriteLine("❌ Client chưa được kết nối!");
            return;
        }

        try
        {
            while (IsRunning && _tcpClient.Connected && !token.IsCancellationRequested)
            {
                if (!_stream.DataAvailable)
                {
                    await Task.Delay(10, token);
                    continue;
                }

                var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                if (bytesRead <= 0)
                {
                    Console.WriteLine("⚠️ Kết nối bị đóng bởi server!");
                    break;
                }

                messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                var receivedMessage = messageBuilder.ToString().Trim();
                Console.WriteLine($"📥 Nhận dữ liệu từ server: {receivedMessage}");

                if (!IsCompleteJson(receivedMessage)) continue;

                var data = ByteUtils.GetBytesFromString(receivedMessage);
                DataHandler?.OnDataReceived(data, "");

                messageBuilder.Clear();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Lỗi khi đọc TCP: {ex.Message}");
        }
        finally
        {
            CloseConnection();
        }
    }

    private bool IsCompleteJson(string receivedMessage)
    {
        try
        {
            JsonDocument.Parse(receivedMessage);
            return receivedMessage.TrimEnd().EndsWith("}") || receivedMessage.TrimEnd().EndsWith("}]}");
        }
        catch
        {
            return false;
        }
    }

    public override async void Send(string data, string endpoint = "")
    {
        if (_tcpClient is not { Connected: true } || _stream == null)
        {
            Console.WriteLine("❌ Không có kết nối TCP để gửi dữ liệu!");
            return;
        }

        try
        {
            var bytes = ByteUtils.GetBytesFromString(data);
            await _stream.WriteAsync(bytes, 0, bytes.Length);
            await _stream.FlushAsync();

            Console.WriteLine($"📤 Gửi dữ liệu ({bytes.Length} bytes) thành công!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Lỗi khi gửi dữ liệu: {ex.Message}");
        }
    }

    public override void Stop()
    {
        CloseConnection();
    }

    private void CloseConnection()
    {
        IsRunning = false;
        _cts?.Cancel();
        _stream?.Close();
        _tcpClient?.Close();
        Console.WriteLine("🛑 Kết nối TCP đã đóng.");
    }
}
