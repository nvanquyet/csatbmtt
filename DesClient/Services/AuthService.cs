using System.Text.Json;
using DesClient.Menu;
using DesClient.Models;
using Shared.Models;
using Shared.Networking.Interfaces;

namespace DesClient.Services;

public static class AuthService 
{
    public static void Register(INetworkProtocol protocol)
    {
        Console.Clear();
        Console.WriteLine("=== Đăng ký ===");

        Console.Write("Nhập tên người dùng: ");
        var username = Console.ReadLine();
        Console.Write("Nhập mật khẩu: ");
        var password = Console.ReadLine();
        Console.Write("Xác nhận mật khẩu: ");
        var confirmPassword = Console.ReadLine();

        if (password != confirmPassword)
        {
            Console.WriteLine("Mật khẩu không khớp.");
            MainMenu.ShowMenu(protocol);
            return;
        }

        if (username == null || password == null) return;
        var message = new MessageNetwork<User>(
            type: CommandType.Registration, 
            code: StatusCode.Success,
            data: new User(username, password)
        );
            
        protocol.Send(message.ToJson(),"");
    }

    public static void Login(INetworkProtocol protocol)
    {
        Console.Clear();
        Console.WriteLine("=== Đăng nhập ===");

        Console.Write("Nhập tên người dùng: ");
        var username = Console.ReadLine();
        Console.Write("Nhập mật khẩu: ");
        var password = Console.ReadLine();

        if (username == null || password == null) return;
        var message = new MessageNetwork<User>(
            type: CommandType.Login, 
            code: StatusCode.Success,
            data: new User(username, password)
        );
        protocol.Send(message.ToJson(), "");
    }

    #region Cache Data

    private const string AuthFile = "auth.txt";

    public static void SaveUserInfo(User? user)
    {
        try
        {
            var jsonData = JsonSerializer.Serialize(user);
            File.WriteAllText(AuthFile, jsonData);
            SessionManager.SetUser(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi lưu thông tin người dùng: {ex.Message}");
        }
    }

    public static bool TryAutoLogin(INetworkProtocol? protocol)
    {
        if (!File.Exists(AuthFile)) return false;

        try
        {
            var jsonData = File.ReadAllText(AuthFile);
            var user = JsonSerializer.Deserialize<User>(jsonData);
            if (user == null) return false;

            if (user.Password != null)
            {
                var message = new MessageNetwork<User>(
                    type: CommandType.Login,
                    code: StatusCode.Success,
                    data: new User(user.UserName, user.Password)
                );
                Console.WriteLine($"User {message.ToJson()}");
                protocol?.Send(message.ToJson(), "");
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