using System.Net.Sockets;
using DesServer.Models;
using Shared.Models;

namespace DesServer.Controllers;

public class ConnectionController : Singleton<ConnectionController>
{
    private readonly Dictionary<string, UserConnection?> _connectedClients = new();

    /// <summary>
    /// Add Client
    /// </summary>
    public void AddClient(string userId, UserConnection? client)
    {
        if (!_connectedClients.TryAdd(userId, client))
        {
            _connectedClients[userId]?.TcpClient.Close();
            _connectedClients[userId] = client;
        }
    }

    /// <summary>
    /// Get UserConnection
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public UserConnection? GetUserConnection(string userId)
    {
        if (_connectedClients.TryGetValue(userId, out var connection))
        {
           return connection;
        }
        return null;
    } 
    
    /// <summary>
    /// Remove Client
    /// </summary>
    public void RemoveClient(string userId)
    {
        if (_connectedClients.ContainsKey(userId))
        {
            _connectedClients[userId]?.TcpClient.Close();
            _connectedClients.Remove(userId);
        }
    }

    /// <summary>
    /// Check is Connection
    /// </summary>
    public bool IsClientConnected(string userId)
    {
        return _connectedClients.ContainsKey(userId) && _connectedClients[userId]!.TcpClient.Connected;
    }
}