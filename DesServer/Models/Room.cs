using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using DesServer.Services;
using Shared.AppSettings;
using Shared.Services;

namespace DesServer.Models;

public class Room(string? id, string password)
{
    public string? Id { get; set; } = id;
    private string? Password { get; set; } = HashPassword(password);
    
    private readonly List<TcpClient?> _allClient = new List<TcpClient?>();
    public List<TcpClient?> AllClient => _allClient;

    public void BroadcastMessage(string message, TcpClient senderClient)
    {
        foreach (var client in _allClient.Where(client => !client.Equals(senderClient)))
        {
            MsgService.SendTcpMessage(target: client, message: message);
        }
    }
    
    public void AddClient(TcpClient? client)
    {
        if (_allClient.Contains(client)) return;
        _allClient.Add(client);
    }

    public bool RemoveClient(TcpClient? client, Action? callback = null)
    {
        if (_allClient.Contains(client))
        {
            _allClient.Remove(client);
            if(_allClient.Count <= 0) callback?.Invoke();
            return true;
        }
        if(_allClient.Count <= 0) callback?.Invoke();
        return false;   
    }
    
    private static string HashPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return Config.DefaultPassword;

        using SHA256 sha256Hash = SHA256.Create();
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }

    public bool VerifyPassword(string password)
    {
        var passCheck = HashPassword(password);
        return Password == passCheck;
    }
}