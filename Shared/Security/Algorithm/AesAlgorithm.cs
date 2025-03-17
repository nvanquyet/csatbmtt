using System.Security.Cryptography;
using Shared.AppSettings;
using Shared.Security.Interface;

namespace Shared.Security.Algorithm;

public class AesAlgorithm : IEncryptionAlgorithm
{
    private readonly Aes _aes = Aes.Create();

    public AesAlgorithm()
    {
        GenerateKey(Config.KeyEncryptionLength);
    }
    
    public byte[] Key => _aes.Key;

    public byte[] GenerateKey(int length)
    {
        _aes.GenerateKey();
        _aes.GenerateIV();
        return _aes.Key;
    }

    public byte[] Encrypt(byte[] data, byte[] key)
    {
        throw new NotImplementedException();
    }

    public byte[] Decrypt(byte[] encryptedData, byte[] key)
    {
        throw new NotImplementedException();
    }
}