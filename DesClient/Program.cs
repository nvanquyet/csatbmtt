using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared.AppSettings;
using Shared.Models;
using Shared.Services;

namespace DesClient;

static class Program
{
    private static TcpClient? _tcpClient;
    private static UdpClient? _udpClient;
    private static NetworkStream? _tcpStream;

    static async Task Main()
    {
        _tcpClient = new TcpClient();
        _udpClient = new UdpClient();

        Console.WriteLine("Connecting to server...");
        await _tcpClient.ConnectAsync(Config.ServerIp, Config.ServerTcpPort);
        _tcpStream = _tcpClient.GetStream();
        Console.WriteLine("Connected to TCP Server!");
        
        // Bắt đầu lắng nghe các thông điệp từ TCP server
        _ = Task.Run(ListenForTcpMessages);
        
        while (true)
        {
            Console.WriteLine("\nMENU:");
            Console.WriteLine("1. Register");
            Console.WriteLine("2. Login");
            Console.WriteLine("3. Create Room");
            Console.WriteLine("4. Join Room");
            Console.WriteLine("5. Send TCP Message");
            Console.WriteLine("6. Send UDP Message");
            Console.WriteLine("7. Exit");
            Console.Write("Choose an option: ");

            string? choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.Write("Enter username: ");
                    string? regUser = Console.ReadLine();
                    Console.Write("Enter password: ");
                    string? regPass = Console.ReadLine();
                    if (regUser != null && regPass != null) {
                        var message = new Message(type: MessageType.Registration, StatusCode.Success, "Authentication", data: new Dictionary<string, object>
                        {
                            { "Username", regUser },
                            { "Password", regPass }
                        });

                        MsgService.SendTcpMessage(_tcpClient, message.ToJson());
                    }
                    break;
                case "2":
                    Console.Write("Enter username: ");
                    string? logUser = Console.ReadLine();
                    Console.Write("Enter password: ");
                    string? logPass = Console.ReadLine();
                    if (logUser != null && logPass != null) {
                        var message = new Message(type: MessageType.Authentication, StatusCode.Success, "Authentication", data: new Dictionary<string, object>
                        {
                            { "Username", logUser },
                            { "Password", logPass }
                        });
                        MsgService.SendTcpMessage(_tcpClient, message.ToJson());
                    }
                    break;
                case "3":
                    Console.Write("Enter Room ID: ");
                    string? roomId = Console.ReadLine();
                    break;
                case "4":
                    Console.Write("Enter Room ID: ");
                    string? joinRoomId = Console.ReadLine();
                    break;
                case "5":
                    Console.Write("Enter message: ");
                    string? tcpMessage = Console.ReadLine();
                    break;
                case "6":
                    Console.Write("Enter message: ");
                    string? udpMessage = Console.ReadLine();
                    //SendUdpMessage(udpMessage);
                    break;
                case "7":
                    Console.WriteLine("Exiting...");
                    return;
                default:
                    Console.WriteLine("Invalid option, try again.");
                    break;
            }
        }
    }

    private static void SendUdpMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        _udpClient?.Send(data, data.Length, Config.ServerIp, Config.ServerUdpPort);
        Console.WriteLine("Sent UDP: " + message);
    }

    // Hàm để lắng nghe các thông điệp TCP
    private static void ListenForTcpMessages()
    {
        byte[] buffer = new byte[1024]; // Đọc tối đa 1024 byte mỗi lần

        while (true)
        {
            try
            {
                int bytesRead = _tcpStream?.Read(buffer, 0, buffer.Length) ?? 0;
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("[TCP] Received: " + message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading TCP message: " + ex.Message);
            }
        }
    }
}
