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

public class ChatMessage(
    string? senderId,
    TransferData? content,
    DateTime timestamp,
    string? senderName,
    string? receiverName)
{
    public string? SenderId { get; init; } = senderId;
    public string? SenderName { get; init; } = senderName;
    public string? ReceiverName { get; init; } = receiverName;
    public TransferData? Content { get; init; } = content;
    public DateTime Timestamp { get; init; } = timestamp;
}

public class ChatRequestDto(UserDto? fromUser, UserDto? toUser)
{
    public UserDto? FromUser { get; init; } = fromUser;
    public UserDto? ToUser { get; init; } = toUser;
}

public class ChatResponseDto(UserDto? fromUser, UserDto? toUser, bool accepted)
{
    public UserDto? FromUser { get; init; } = fromUser;
    public UserDto? ToUser { get; init; } = toUser;
    public bool Accepted  { get; init; } = accepted;
}