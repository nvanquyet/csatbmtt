using System.Net.Sockets;
using System.Text.Json;
using DesClient.Models;
using MongoDB.Bson;
using Shared.Models;
using Shared.Services;

namespace DesClient.Services;

public static class AuthService 
{
    public static void Register(TcpService tcpService)
    {
        Console.Clear();
        Console.WriteLine("=== Đăng ký ===");

        Console.Write("Nhập tên người dùng: ");
        string? username = Console.ReadLine();
        Console.Write("Nhập mật khẩu: ");
        string? password = Console.ReadLine();
        Console.Write("Xác nhận mật khẩu: ");
        string? confirmPassword = Console.ReadLine();

        if (password != confirmPassword)
        {
            Console.WriteLine("Mật khẩu không khớp.");
            return;
        }

        if (username != null && password != null)
        {
            var message = new MessageNetwork<AuthData>(
                type: CommandType.Registration, 
                code: StatusCode.Success,
                data: new AuthData(username, password)
            );
            
            tcpService.SendTcpMessage(message.ToJson());
        }
    }

    public static void Login(TcpService tcpService)
    {
        Console.Clear();
        Console.WriteLine("=== Đăng nhập ===");

        Console.Write("Nhập tên người dùng: ");
        string? username = Console.ReadLine();
        Console.Write("Nhập mật khẩu: ");
        string? password = Console.ReadLine();

        if (username != null && password != null)
        {
            var message = new MessageNetwork<AuthData>(
                type: CommandType.Authentication, 
                code: StatusCode.Success,
                data: new AuthData(username, password)
            );
            tcpService.SendTcpMessage(message.ToJson());
        }
    }

    #region Cache Data

    private const string AuthFile = "auth.txt";

    public static void SaveUserInfo(User? user)
    {
        try
        {
            string jsonData = JsonSerializer.Serialize(user);
            File.WriteAllText(AuthFile, jsonData);
            SessionManager.SetUser(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi lưu thông tin người dùng: {ex.Message}");
        }
    }

    public static bool TryAutoLogin(TcpClient? tcpClient)
    {
        if (!File.Exists(AuthFile)) return false;

        try
        {
            string jsonData = File.ReadAllText(AuthFile);
            User? user = JsonSerializer.Deserialize<User>(jsonData);
            if (user == null) return false;

            if (user.Id != null)
            {
                if (user.Password != null)
                {
                    var message = new MessageNetwork<AuthData>(
                        type: CommandType.Authentication,
                        code: StatusCode.Success,
                        data: new AuthData(user.Id, user.Password)
                    );

                    MsgService.SendTcpMessage(tcpClient, message.ToJson());
                }
            }

            SessionManager.SetUser(user);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi đọc thông tin đăng nhập: {ex.Message}");
            return false;
        }
    }

    public static void Logout()
    {
        try
        {
            if (File.Exists(AuthFile)) File.Delete(AuthFile);
            SessionManager.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi đăng xuất: {ex.Message}");
        }
    }

    #endregion
}