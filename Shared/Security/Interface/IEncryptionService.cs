
namespace Shared.Security.Interface;

public interface IEncryptionService
{
    byte[] EncryptData(EncryptionType type, byte[] data);
    byte[] DecryptData(EncryptionType type, byte[] encryptedData);
}