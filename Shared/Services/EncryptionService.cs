using Shared.Security.Algorithm;
using Shared.Security.Interface;
using Shared.Utils.Patterns;

namespace Shared.Services;

public class EncryptionService : Singleton<EncryptionService>, IEncryptionService
{
    private readonly Dictionary<EncryptionType, IEncryptionAlgorithm> _algorithms;

    public EncryptionService()
    {
        _algorithms ??= new Dictionary<EncryptionType, IEncryptionAlgorithm>()
        {
            { EncryptionType.Aes, new AesAlgorithm() },
            { EncryptionType.Des, new DesAlgorithm() },
            { EncryptionType.Rsa, new RsaAlgorithm() }
        };
    }


    public IEncryptionAlgorithm GetAlgorithm(EncryptionType encryptionType) => _algorithms[encryptionType];
    
    public byte[] EncryptData(EncryptionType type, byte[] data, byte[] key)
    {
        if (!_algorithms.TryGetValue(type, out var algorithm))
            throw new ArgumentException($"Algorithm {type} is not supported");

        return algorithm.Encrypt(data, key);
    }

    public byte[] DecryptData(EncryptionType type, byte[] encryptedData, byte[] key)
    {
        if (!_algorithms.TryGetValue(type, out var algorithm))
            throw new ArgumentException($"Algorithm {type} is not supported");

        return algorithm.Decrypt(encryptedData, key);
    }
}