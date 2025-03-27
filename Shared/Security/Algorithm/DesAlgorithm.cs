using Shared.AppSettings;
using Shared.Security.Cryptography.Des;
using Shared.Security.Interface;

namespace Shared.Security.Algorithm;

public class DesAlgorithm : IEncryptionAlgorithm
{
    private readonly byte[] _desKey = GenerateRandomKey(Config.KeyDesEncryptionLength);
    public byte[] EncryptKey => _desKey;
    public byte[] DecryptKey => _desKey;
    public byte[] Encrypt(byte[] data, byte[] key) => DesCrypto.Encrypt(data, key);

    public byte[] Decrypt(byte[] encryptedData, byte[] key) => DesCrypto.Decrypt(encryptedData, key);

    private static byte[] GenerateRandomKey(int length)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
        var random = new Random();
        var key = new byte[length];

        for (int i = 0; i < length; i++)
        {
            key[i] = (byte)chars[random.Next(chars.Length)];
        }

        return key;
    }
}