using Shared.AppSettings;
using Shared.Security.Cryptography.Rsa;
using Shared.Security.Interface;

namespace Shared.Security.Algorithm;

class RsaAlgorithm : IEncryptionAlgorithm
{
    private static readonly KeyPair Key = RsaCrypto.GenerateKeyPair(Config.KeyRsaEncryptionLength);
    public byte[] EncryptKey => Key.public_.GetBytes();
    public byte[] DecryptKey => Key.private_.GetBytes();
    public byte[] Encrypt(byte[] data, byte[] key) => RsaCrypto.Encrypt(data, key);

    public byte[] Decrypt(byte[] encryptedData, byte[] key) => RsaCrypto.Decrypt(encryptedData, key);
}