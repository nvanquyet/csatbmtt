using System.Net.Sockets;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DesServer.Models;

public class UserConnection(
    string? userId,
    TcpClient tcpClient,
    string ipAddress,
    int port,
    DateTime lastConnection)
{
    [BsonId] 
    [BsonRepresentation(BsonType.ObjectId)] 
    public string? Id { get; set; }
    public string? UserId { get; set; } = userId;
    public TcpClient TcpClient { get; set; } = tcpClient;
    public string IpAddress { get; set; } = ipAddress;
    public int Port { get; set; } = port;
    public DateTime LastConnection { get; set; } = lastConnection;
}