using DesServer.AppSetting;
using MongoDB.Driver;
using Shared.Models;

namespace DesServer.Database;

public class MessageDatabase : ADatabase
{
    private static readonly IMongoCollection<ChatMessage> ChatMessagesCollection = DatabaseService.GetCollection<ChatMessage>(ServerConfig.ChatMessagesCollection);
    
    public static void SaveMessage(ChatMessage message)
    {
        ChatMessagesCollection.InsertOne(message);
    }
    
    public static List<ChatMessage> LoadMessages(string? senderId, string? receiverId)
    {
        var filter = Builders<ChatMessage>.Filter.Or(
            Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(m => m.SenderId, senderId),
                Builders<ChatMessage>.Filter.Eq(m => m.ReceiverId, receiverId)
            ),
            Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(m => m.SenderId, receiverId),
                Builders<ChatMessage>.Filter.Eq(m => m.ReceiverId, senderId)
            )
        );

        return ChatMessagesCollection.Find(filter).SortBy(m => m.Timestamp).ToList();
    }
}
