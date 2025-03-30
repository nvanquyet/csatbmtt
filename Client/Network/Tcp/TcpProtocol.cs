using System.Net.Sockets;
using System.Text;
using Client.Form;
using Shared;
using Shared.AppSettings;
using Shared.Networking;
using Shared.Networking.Interfaces;
using Shared.Services;
using Shared.Utils;

namespace Client.Network.Tcp;

public class TcpProtocol(INetworkHandler dataHandler) : ANetworkProtocol(dataHandler)
{
    public bool IsConnected => _tcpClient?.Client?.Connected ?? false; 
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;

    
    private Thread? _listenThread;
    
    public override async Task Start(int port)
    {
        _isRunning = true;
        _tcpClient = new TcpClient();

        try
        {
            await _tcpClient.ConnectAsync(Config.ServerIp, Config.ServerTcpPort);
            Logger.LogInfo("Connected to TCP Server!");
            _stream = _tcpClient.GetStream();
            _cts = new CancellationTokenSource();

            Logger.LogInfo($"✅ Kết nối đến server: {_tcpClient.Client.RemoteEndPoint}");
            LoginForm.TryLogin();
            _listenThread = new Thread(ListenForTcpMessagesAsync);
            _listenThread.Start(); 
        }
        catch (Exception e)
        {
            Logger.LogError($"❌ Lỗi kết nối TCP: {e.Message}");
            throw;
        }
    }

    private void ListenForTcpMessagesAsync()
    {
        var buffer = new byte[Config.BufferSizeLimit];
        var messageBuilder = new StringBuilder();

        if (_tcpClient is null || _stream is null)
        {
            Logger.LogError("Client chưa được kết nối!");
            return;
        }

        try
        {
            while (IsRunning && _tcpClient.Connected)
            {
                if (!_stream.DataAvailable) continue;
                var bytesRead = _stream.Read(buffer);
                if (bytesRead <= 0)
                {
                    Logger.LogWarning("Kết nối bị đóng bởi server!");
                    break;
                }

                messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                while (true)
                {
                    var endIndex = messageBuilder.ToString().IndexOf('\n');
                    if(endIndex == -1) break;
                    var completeJson = messageBuilder.ToString(0, Math.Max(endIndex, 0));
                    messageBuilder.Remove(0, Math.Max(endIndex + 1, 0));

                    var data = ByteUtils.GetBytesFromString(completeJson.Trim());
                    DataHandler?.OnDataReceived(data, "");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Lỗi khi đọc TCP: {ex.Message}");
        }
        finally
        {
            CloseConnection();
        }
    }


    public override void Send(string data, string endpoint = "") => MsgService.SendTcpMessage(_tcpClient, data);
    public void Send(string data, Action<byte[]>? progressCallback = null) => MsgService.SendTcpMessage(_tcpClient, data, progressCallback);
    public override void Stop()
    {
        _isRunning = false;
        CloseConnection();
    }

    private void CloseConnection()
    {
        try
        {
            // Kiểm tra nếu có TcpListener thì dừng lại
            _tcpClient?.Close();
        
            // Hủy token để dừng luồng an toàn
            _cts?.Cancel();

            // Đợi luồng kết thúc nhưng không để nó bị treo
            _listenThread?.Join(2000); // Timeout là 2 giây để tránh treo mãi mãi
        
            // Đóng luồng stream và client nếu còn mở
            _stream?.Close();
            _tcpClient?.Close();
        
            Logger.LogError("🛑 Kết nối TCP đã đóng.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Lỗi khi đóng kết nối: {ex.Message}");
        }
    }

}