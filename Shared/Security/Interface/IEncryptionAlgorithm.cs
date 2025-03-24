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
    byte[] Encrypt(byte[] data, byte[] key);
    byte[] Decrypt(byte[] encryptedData, byte[] key);
}