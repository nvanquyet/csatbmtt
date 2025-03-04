using System.Net.Sockets;

namespace Shared.Models;

public class AuthData(string username, string password)
{
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
}

public class CommunicationData(TcpClient targetClient, string message, DateTime timeSend)
{
    public TcpClient TargetClient { get; set; } = targetClient;
    public string Message { get; set; } = message;
    public DateTime TimeSend { get; set; } = timeSend;
}