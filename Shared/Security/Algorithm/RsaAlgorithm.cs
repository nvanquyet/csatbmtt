using Shared.AppSettings;
using Shared.Security.Cryptography.Rsa;
using Shared.Security.Interface;

namespace Shared.Security.Algorithm;

class RsaAlgorithm : IEncryptionAlgorithm
{
    private KeyPair _key = RsaCrypto.GenerateKeyPair(Config.KeyRsaEncryptionLength);
    public byte[] EncryptKey => _key.public_.GetBytes();
    public byte[] DecryptKey => _key.private_.GetBytes();
    public byte[]? Encrypt(byte[]? data, byte[] key) => RsaCrypto.Encrypt(data, key);

    public byte[]? Decrypt(byte[]? encryptedData, byte[] key) => RsaCrypto.Decrypt(encryptedData, key);
    public void GenerateKey()
    {
        _key = RsaCrypto.GenerateKeyPair(Config.KeyRsaEncryptionLength);
    }
}