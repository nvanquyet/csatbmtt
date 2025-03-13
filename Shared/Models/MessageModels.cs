using System.Net.Sockets;

namespace Shared.Models;

public class AuthData(string? username, string? password)
{
    public string? Username { get; set; } = username;
    public string? Password { get; set; } = password;
}

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