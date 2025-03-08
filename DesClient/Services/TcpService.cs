using System.Net.Sockets;
using System.Text;
using DesClient.Menu;
using Shared.AppSettings;
using Shared.Models;

namespace DesClient.Services;

public class TcpService
{
    private TcpClient? _tcpClient;
    private NetworkStream? _tcpStream;
    
    public TcpClient? GetTcpClient() => _tcpClient;
    
    public async Task ConnectAsync()
    {
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(Config.ServerIp, Config.ServerTcpPort);
        _tcpStream = _tcpClient.GetStream();
    }

    public void SendTcpMessage(string message)
    {
        if (_tcpStream == null) return;
        byte[] data = Encoding.UTF8.GetBytes(message);
        _tcpStream.Write(data, 0, data.Length);
    }

    public void ListenForTcpMessages()
    {
        byte[] buffer = new byte[1024];

        while (true)
        {
            try
            {
                int bytesRead = _tcpStream?.Read(buffer, 0, buffer.Length) ?? 0;
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("[TCP] Received: " + message);
                    HandleMessage(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading TCP message: " + ex.Message);
            }
        }
    }

    private void HandleMessage(string message)
    {
        var msg = MessageNetwork<dynamic>.FromJson(message);
        if (msg?.Code == StatusCode.Success)
        {
            switch (msg.Type)
            {
                case CommandType.Authentication:
                    if (msg.TryParseData(out User? user))
                    {
                        AuthService.SaveUserInfo(user);
                        MainMenu.ShowMenu2(this);
                    }
                    break;
                case CommandType.Registration:
                    Console.WriteLine("Register Success");
                    MainMenu.ShowMenu(this);
                    break;
                case CommandType.GetAllUsers:
                    if (msg.TryParseData(out List<User>? allUsers))
                        if (allUsers != null)
                            ChatMenu.ChatWith(allUsers, this);
                    break;
                case CommandType.ReceiveMessage:
                    if (msg.TryParseData(out ChatMessage? cM))
                    {
                        ChatMenu.LoadMessage(cM);
                    }
                    break;
                case CommandType.LoadMessage:
                    if (msg.TryParseData(out List<ChatMessage>? allMessages))
                    {
                        ChatMenu.LoadAllMessage(allMessages);
                    }
                    break;
                default:
                    Console.WriteLine("Unknown Command");
                    break;
            }
        }
        else
        {
            MainMenu.ShowMenu(this);
        }
    }
}