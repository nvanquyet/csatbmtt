using System.Security.Cryptography;
using Shared.AppSettings;
using Shared.Security.Interface;
using Shared.Utils;

namespace Shared.Security.Algorithm;

public class AesAlgorithm : IEncryptionAlgorithm
{
    private readonly Aes _aes = Aes.Create();

    public AesAlgorithm()
    {
        //GenerateKey(Config.KeyEncryptionLength);
    }
    
    public byte[] EncryptKey => _aes.Key;
    public byte[] DecryptKey => _aes.Key;

    public byte[] GenerateKey(int length)
    {
        _aes.GenerateKey();
        _aes.GenerateIV();
        return _aes.Key;
    }

    public byte[] Encrypt(byte[] data, byte[] key)
    {
        return ByteUtils.GetBytesFromString("anhyeuem");
    }

    public byte[] Decrypt(byte[] encryptedData, byte[] key)
    {
        return ByteUtils.GetBytesFromString("anhyeuem");
    }
}