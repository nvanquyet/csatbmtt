using DesServer.AppSetting;
using DesServer.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DesServer.Database;

public class ConnectionDatabase : ADatabase
{
    private static readonly IMongoCollection<UserConnection> UserConnection =
        DatabaseService.GetCollection<UserConnection>(ServerConfig.UserConnection);

    public static void UpdateConnection(UserConnection newConnection)
    {
        var currentConnection = GetConnectionByUserId(newConnection.UserId);
        if (currentConnection == null)
        {
            UserConnection.InsertOne(newConnection);
        }
        else
        {
            var update = Builders<UserConnection>.Update
                .Set(c => c.IpAddress, newConnection.IpAddress)
                .Set(c => c.Port, newConnection.Port)
                .Set(c => c.LastConnection, newConnection.LastConnection)
                .Set(c => c.TcpClient, newConnection.TcpClient);

            UserConnection.UpdateOne(c => c.UserId == newConnection.UserId, update);
        }
    }

    public static UserConnection? GetConnectionByUserId(ObjectId? userId) => UserConnection.Find(c => c.UserId == userId).FirstOrDefault();
    
}