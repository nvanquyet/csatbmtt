
namespace Shared.Models;

public class ErrorData(string message)
{
    public string Message { get; set; } = message;
}

public enum TransferType
{
    Text,
    Image,
    Video,
    Audio,
    File,
    Folder,
}
public struct TransferData(TransferType transferType, byte[]? rawData)
{
    public TransferType TransferType { get; set; } = transferType;
    public byte[]? RawData { get; set; } = rawData;
}