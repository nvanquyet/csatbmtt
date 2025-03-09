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

// public class HistoryMessage(List<ContentMessage> contentMessages, string? receiverId, string? senderId)
// {
//     [BsonId]
//     [BsonRepresentation(BsonType.ObjectId)] 
//     public string? Id { get; set; }
//     public string? SenderId { get; set; } = senderId;
//     public string? ReceiverId { get; set; } = receiverId;
//     public List<ContentMessage> ContentMessages { get; set; } = contentMessages;
// }
//
// public class ContentMessage(string? content, string? receiverName, DateTime timestamp, string? senderName)
// {
//     [BsonId]
//     [BsonRepresentation(BsonType.ObjectId)] 
//     public string? Id { get; set; }
//     public string? Content { get; set; } = content;
//     public DateTime Timestamp { get; set; } = timestamp;
//     public string? ReceiverName { get; set; } = receiverName;
//     public string? SenderName { get; set; } = senderName;
// }