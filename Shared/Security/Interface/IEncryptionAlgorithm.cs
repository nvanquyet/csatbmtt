namespace Shared.Security.Interface;

public enum EncryptionType
{
    Aes,
    Des,
    Rsa
}

public interface IEncryptionAlgorithm
{
    byte[] EncryptKey { get; }
    byte[] DecryptKey { get; }
    protected byte[] GenerateKey(int length);
    byte[] Encrypt(byte[] data, byte[] key);
    byte[] Decrypt(byte[] encryptedData, byte[] key);
}