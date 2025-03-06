namespace Shared.Models;

public class ChatHistoryRequest(string? senderId, string? receiverId)
{
    public string? SenderId { get; set; } = senderId;
    public string? ReceiverId { get; set; } = receiverId;
}