
namespace Shared.Security.Interface;

public interface IEncryptionService
{
    byte[] EncryptData(byte[] data, byte[] key);
    byte[] DecryptData(byte[] encryptedData, byte[] key);
}