namespace Shared.Models;

public class ClientInfo(string id, byte[] publicKey)
{
    public string Id { get; init; } = id;
    public byte[] PublicKey { get; init; } = publicKey; 
}