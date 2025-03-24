using Shared.Utils;

namespace Shared.Models;

public class ErrorMessage(string message)
{
    public string Message { get; set; } = message;
}
public class TransferData(TransferType transferType, byte[]? rawData)
{
    public TransferType TransferType { get; set; } = transferType;
    public byte[]? RawData { get; set; } = rawData;
    
    public byte[]? KeyDecrypt { get; set; }
}