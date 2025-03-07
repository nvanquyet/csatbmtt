using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models;

public class ChatMessage(ObjectId? senderId, ObjectId? receiverId, string? content, DateTime timestamp)
{
    [BsonId]
    public ObjectId? Id { get; set; }
    public ObjectId? SenderId { get; set; } = senderId;
    public ObjectId? ReceiverId { get; set; } = receiverId;
    public string? Content { get; set; } = content;
    public DateTime Timestamp { get; set; } = timestamp;
    public string? ReceiverName { get; set; }
    public string? SenderName { get; set; }
    
}