namespace Shared.Models;

public class MessageDto(
    string? receiverId,
    TransferData? data)
{
    public string? ReceiverId { get; init; } = receiverId;
    public TransferData? Data { get; init; } = data;
}