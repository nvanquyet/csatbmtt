using MongoDB.Bson;

namespace Shared.Models;

public class ChatHistoryRequest(ObjectId? senderId, ObjectId? receiverId)
{
    public ObjectId? SenderId { get; set; } = senderId;
    public ObjectId? ReceiverId { get; set; } = receiverId;
}