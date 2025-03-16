using System.Security.Cryptography;
using System.Text;
using Shared.Utils.Patterns;

namespace Shared.Models;

public class EncryptionService : Singleton<EncryptionService>
{
    private byte[] DesKey { get; set; }
    private byte[] AesKey { get; set; }
    private RSAParameters RsaPrivateKey { get; set; }
    private RSAParameters RsaPublicKey { get; set; }

    [Obsolete("Obsolete")]
    public string EncryptDes(string plainText)
    {
        using var des = new DESCryptoServiceProvider();
        des.Key = DesKey;
        des.IV = new byte[des.BlockSize / 8]; // Khởi tạo IV bằng 0
        using var encryptor = des.CreateEncryptor();
        var data = Encoding.UTF8.GetBytes(plainText);
        var encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);
        return Convert.ToBase64String(encryptedData);
    }
    
    [Obsolete("Obsolete")]
    public string DecryptDes(string cipherText)
    {
        using var des = new DESCryptoServiceProvider();
        des.Key = DesKey;
        des.IV = new byte[des.BlockSize / 8]; // Khởi tạo IV bằng 0
        using var cryptoTransform = des.CreateDecryptor();
        var data = Convert.FromBase64String(cipherText);
        var decryptedData = cryptoTransform.TransformFinalBlock(data, 0, data.Length);
        return Encoding.UTF8.GetString(decryptedData);
    }
    
    [Obsolete("Obsolete")]
    public string EncryptAes(string plainText)
    {
        using var aes = new AesCryptoServiceProvider();
        aes.Key = AesKey;
        aes.IV = new byte[aes.BlockSize / 8]; 
        using var encryptor = aes.CreateEncryptor();
        var data = Encoding.UTF8.GetBytes(plainText);
        var encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);
        return Convert.ToBase64String(encryptedData);
    }
    [Obsolete("Obsolete")]
    public string DecryptAes(string cipherText)
    {
        using var aes = new AesCryptoServiceProvider();
        aes.Key = AesKey;
        aes.IV = new byte[aes.BlockSize / 8]; 
        using var cryptoTransform = aes.CreateDecryptor();
        var data = Convert.FromBase64String(cipherText);
        var decryptedData = cryptoTransform.TransformFinalBlock(data, 0, data.Length);
        return Encoding.UTF8.GetString(decryptedData);
    }
    
    public string EncryptRsa(string plainText)
    {
        using var rsa = new RSACryptoServiceProvider();
        rsa.ImportParameters(RsaPublicKey);
        var data = Encoding.UTF8.GetBytes(plainText);
        var encryptedData = rsa.Encrypt(data, false);
        return Convert.ToBase64String(encryptedData);
    }

    public string DecryptRsa(string cipherText)
    {
        using var rsa = new RSACryptoServiceProvider();
        rsa.ImportParameters(RsaPrivateKey);
        var data = Convert.FromBase64String(cipherText);
        var decryptedData = rsa.Decrypt(data, false);
        return Encoding.UTF8.GetString(decryptedData);
    }

    [Obsolete("Obsolete")]
    public EncryptionService()
    {
        DesKey = new byte[8]; 
        AesKey = new byte[32]; 
        RsaPrivateKey = new RSAParameters();
        RsaPublicKey = new RSAParameters();

        new RNGCryptoServiceProvider().GetBytes(DesKey);
        new RNGCryptoServiceProvider().GetBytes(AesKey);
    }
}