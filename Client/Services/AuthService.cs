using System.Text.Json;
using Client.Models;
using Client.Network;
using Shared;
using Shared.Models;
using Shared.Networking.Interfaces;

namespace Client.Services;

public static class AuthService 
{
    public static void Login(string username, string password)
    {
        var message = new MessageNetwork<User>(
            type: CommandType.Login, 
            code: StatusCode.Success,
            data: new User(username, password)
        );
        NetworkManager.Instance.TcpService.Send(message.ToJson(), "");
    }

    public static void Register(string username, string password)
    {
        var message = new MessageNetwork<User>(
            type: CommandType.Registration, 
            code: StatusCode.Success,
            data: new User(username, password)
        );
            
        NetworkManager.Instance.TcpService.Send(message.ToJson(), "");
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
            Logger.LogError($"Lỗi khi lưu thông tin người dùng: {ex.Message}");
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

            if (user.Password == null) return true;
            var message = new MessageNetwork<User>(
                type: CommandType.Login,
                code: StatusCode.Success,
                data: new User(user.UserName, user.Password)
            );
            Logger.LogInfo($"User {message.ToJson()}");
            protocol?.Send(message.ToJson(), "");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Lỗi khi đọc thông tin đăng nhập: {ex.Message}");
            return false;
        }
    }

    public static void Logout()
    {
        try
        {
            var message = new MessageNetwork<UserDto>(
                type: CommandType.ClientDisconnect,
                code: StatusCode.Success,
                data: SessionManager.GetUserDto()
            );
            Logger.LogInfo($"User {message.ToJson()}");
            NetworkManager.Instance.TcpService.Send(message.ToJson(), "");
            if (File.Exists(AuthFile)) File.Delete(AuthFile);
            SessionManager.Clear();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Lỗi khi đăng xuất: {ex.Message}");
        }
    }

    #endregion
}