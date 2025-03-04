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
    
    public static async Task Main(string[] args)
    {
        _tcpClient = new TcpClient();
        _udpClient = new UdpClient();

        Console.WriteLine("Connecting to server...");
        await _tcpClient.ConnectAsync(Config.ServerIp, Config.ServerTcpPort);
        _tcpStream = _tcpClient.GetStream();
        Console.WriteLine("Connected to TCP Server!");
        
        // Bắt đầu lắng nghe các thông điệp từ TCP server
        _ = Task.Run(ListenForTcpMessages);
        ShowMenu1();
        while (true)
        {
            
        }
    }

    
    private static void ShowMenu1()
    {
        Console.Clear();
        Console.WriteLine("=== Menu 1 ===");
        Console.WriteLine("1. Đăng nhập");
        Console.WriteLine("2. Đăng ký");
        Console.WriteLine("3. Thoát");
        Console.Write("Chọn một tùy chọn (1-3): ");
        
        string? choice = Console.ReadLine();
        
        switch (choice)
        {
            case "1":
                Login();
                break;
            case "2":
                Register();
                break;
            case "3":
                Environment.Exit(0);
                return;
            default:
                Console.WriteLine("Lựa chọn không hợp lệ.");
                break;
        }
    }

    private static void Register()
    {
        Console.Clear();
        Console.WriteLine("=== Đăng ký ===");
        
        Console.Write("Nhập tên người dùng: ");
        string? username = Console.ReadLine();

        if (!CheckValidUsername(username))
        {
            Console.WriteLine("Tên người dùng đã tồn tại.");
            ShowMenu1();
            return;
        }

        Console.Write("Nhập mật khẩu: ");
        string? password = Console.ReadLine();
        Console.Write("Xác nhận mật khẩu: ");
        string? confirmPassword = Console.ReadLine();

        if (password != confirmPassword)
        {
            Console.WriteLine("Mật khẩu không khớp.");
            ShowMenu1();
        }else if (username != null && password != null) {
            var message = new Message(type: MessageType.Registration, StatusCode.Success, "Authentication", data: new Dictionary<string, object>
            {
                { "Username", username },
                { "Password", password }
            });
            MsgService.SendTcpMessage(_tcpClient, message.ToJson());
        }
        else
        {
            ShowMenu1();
        }
    }

    private static bool CheckValidUsername(string? username) => true;
    
    private static void Login()
    {
        Console.Clear();
        Console.WriteLine("=== Đăng nhập ===");
        
        Console.Write("Nhập tên người dùng: ");
        string? username = Console.ReadLine();
        
        Console.Write("Nhập mật khẩu: ");
        string? password = Console.ReadLine();

        if (username != null && password != null) {
            var message = new Message(type: MessageType.Authentication, StatusCode.Success, "Authentication", data: new Dictionary<string, object>
            {
                { "Username", username },
                { "Password", password }
            });

            MsgService.SendTcpMessage(_tcpClient, message.ToJson());
        }
        else
        {
            ShowMenu1();
        }
    }

    private static void ShowMenu2()
    {
        Console.Clear();
        Console.WriteLine("=== Menu 2 ===");
        Console.WriteLine("1. Chat với người dùng khác");
        Console.WriteLine("2. Đăng xuất");
        Console.Write("Chọn một tùy chọn (1-2): ");
        
        string? choice = Console.ReadLine();
        
        switch (choice)
        {
            case "1":
                ChatWith();
                break;
            case "2":
                Console.WriteLine("Đã đăng xuất.");
                ShowMenu1();
                break;
            default:
                Console.WriteLine("Lựa chọn không hợp lệ.");
                ShowMenu2();
                break;
        }
    }

    private static List<String> GetAllUserValid()
    {
        return new List<String>(){ "Hoang" , "Anh"};
    }

    private static void ChatWith()
    {
        Console.Clear();
        Console.WriteLine("=== Chọn người để chat ===");
        int index = 0;
        foreach (var user in GetAllUserValid())
        {
            Console.WriteLine($"{index}: {user}");
            index++;
        }

        Console.Write($"{index} Nhập tên người bạn muốn chat: ");
        string? targetUser = Console.ReadLine();
        
        //Connect with other
    }

    // Menu 3: 
    private static void ShowMenu3(string? targetUser)
    {
        Console.Clear();
        Console.WriteLine("=== Menu 3: Chat với " + targetUser + " ===");
        Console.WriteLine("1. Gửi tin nhắn");
        Console.WriteLine("2. Quay lại");
        Console.Write("Chọn một tùy chọn (1-2): ");
        
        string? choice = Console.ReadLine();
        
        switch (choice)
        {
            case "1":
                SendMessage(targetUser);
                break;
            case "2":
                ShowMenu2();
                break;
            default:
                Console.WriteLine("Lựa chọn không hợp lệ.");
                ShowMenu3(targetUser);
                break;
        }
    }

    // Gửi tin nhắn
    private static void SendMessage(string? targetUser)
    {
        Console.Clear();
        Console.WriteLine("=== Gửi tin nhắn cho " + targetUser + " ===");
        Console.Write("Nhập tin nhắn của bạn: ");
        string? message = Console.ReadLine();

        Console.WriteLine($"Tin nhắn đã gửi cho {targetUser}: {message}");
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
                    //Handle message
                    var msg = Message.FromJson(message);
                    if (msg?.Code == StatusCode.Success)
                    {
                        switch (msg.Type)
                        {
                            case MessageType.Authentication:
                                //Cache username and password to local and save token
                                ShowMenu2();
                                break;
                            case MessageType.Registration:
                                Console.WriteLine("Register Success");
                                ShowMenu1();
                                break;
                            default:
                                ShowMenu1();
                                break;
                        }
                    }
                    else
                    {
                        ShowMenu1();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading TCP message: " + ex.Message);
            }
        }
    }
}
