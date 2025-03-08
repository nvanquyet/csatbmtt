using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models;

public class ChatMessage(string? senderId, string? receiverId, string? content, DateTime timestamp)
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)] 
    public string? Id { get; set; }
    public string? SenderId { get; set; } = senderId;
    public string? ReceiverId { get; set; } = receiverId;
    public string? Content { get; set; } = content;
    public DateTime Timestamp { get; set; } = timestamp;
    public string? ReceiverName { get; set; }
    public string? SenderName { get; set; }
}