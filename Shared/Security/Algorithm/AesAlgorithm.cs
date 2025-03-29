using Shared.AppSettings;
using Shared.Security.Interface;
using Shared.Utils;

namespace Shared.Security.Algorithm;

public class AesAlgorithm : IEncryptionAlgorithm
{
    private readonly byte[] _key = GenerateKey(Config.KeyDesEncryptionLength);
    public byte[] EncryptKey => _key;
    public byte[] DecryptKey => _key;
    public byte[]? Encrypt(byte[]? data, byte[] key)
    {
        throw new NotImplementedException();
    }
    
    private static byte[] GenerateKey(int length)
    {
        return ByteUtils.GetBytesFromString("testAesAlgorithm");
    }

    public byte[]? Decrypt(byte[]? encryptedData, byte[] key)
    {
        throw new NotImplementedException();
    }

    public void GenerateKey()
    {
        throw new NotImplementedException();
    }
}