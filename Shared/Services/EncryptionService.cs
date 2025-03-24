using Shared.Security.Algorithm;
using Shared.Security.Interface;
using Shared.Utils.Patterns;

namespace Shared.Services;

public class EncryptionService : Singleton<EncryptionService>
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
}