using DesServer.AppSetting;
using MongoDB.Driver;
using Shared.Models;

namespace DesServer.Database;

public abstract class MessageDatabase : ADatabase
{
    private static readonly IMongoCollection<ChatConversation> ChatMessagesCollection = DatabaseService.GetCollection<ChatConversation>(ServerConfig.ChatMessagesCollection);
    
    //public static void SaveMessage(ChatMessage message) =>  ChatMessagesCollection.InsertOne(message);
    
    public static async Task SaveChatMessage(string? senderId, string? receiverId, ChatMessage message)
    {
        if (senderId == null || receiverId == null || string.IsNullOrWhiteSpace(message.Content)) return;

        (senderId, receiverId) = NormalizeParticipants(senderId, receiverId);

        var filter = Builders<ChatConversation>.Filter.And(
            Builders<ChatConversation>.Filter.Eq(c => c.SenderId, senderId),
            Builders<ChatConversation>.Filter.Eq(c => c.ReceiverId, receiverId)
        );

        var update = Builders<ChatConversation>.Update.Push(c => c.Messages, message);

        await ChatMessagesCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public static async Task<ChatMessage[]> LoadChatMessages(string senderId, string receiverId, int limit = 50)
    {
        (senderId, receiverId) = NormalizeParticipants(senderId, receiverId);

        var filter = Builders<ChatConversation>.Filter.And(
            Builders<ChatConversation>.Filter.Eq(c => c.SenderId, senderId),
            Builders<ChatConversation>.Filter.Eq(c => c.ReceiverId, receiverId)
        );

        var chat = await ChatMessagesCollection
            .Find(filter)
            .FirstOrDefaultAsync();

        return chat?.Messages
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToArray() ?? [];
    }

    
    private static (string, string) NormalizeParticipants(string senderId, string receiverId)
    {
        return string.CompareOrdinal(senderId, receiverId) < 0
            ? (senderId, receiverId)
            : (receiverId, senderId);
    }

}
