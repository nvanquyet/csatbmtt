using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models;

public class ChatConversation(string? senderId, string? receiverId, List<ChatMessage> messages)
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? SenderId { get; set; } = senderId;
    public string? ReceiverId { get; set; } = receiverId;   

    public List<ChatMessage> Messages { get; set; } = messages;
}

public class ChatMessage(string? senderId, string? content, DateTime timestamp, string? senderName, string? receiverName)
{
    public string? SenderId { get; init; } = senderId;
    public string? SenderName { get; init; } = senderName;
    public string? ReceiverName { get; init; } = receiverName;
    public string? Content { get; init; } = content;
    public DateTime Timestamp { get; init; } = timestamp;
}
