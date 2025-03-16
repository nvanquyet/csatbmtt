using System.Security.Cryptography;
using Shared.Security.Interface;

namespace Shared.Security.Algorithm;

public class AesAlgorithm : IEncryptionAlgorithm
{
    private readonly Aes _aes = Aes.Create();
    private RSA _rsa = RSA.Create();

    // Tạo khóa AES ngẫu nhiên
    private byte[] GenerateAesKey()
    {
        _aes.GenerateKey();
        _aes.GenerateIV();
        return _aes.Key;
    }

    // Mã hóa khóa AES bằng RSA
    private static byte[] EncryptAesKeyWithRsa(byte[] aesKey, RSA rsaPublicKey)
    {
        return rsaPublicKey.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);
    }

    // Mã hóa dữ liệu bằng AES
    private byte[] EncryptData(byte[] data)
    {
        using var encryptor = _aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        cs.Write(data, 0, data.Length);
        cs.FlushFinalBlock();
        return ms.ToArray();
    }

    public void SendEncryptedAesKey(RSA serverPublicKey)
    {
        var aesKey = GenerateAesKey();
        var encryptedAesKey = EncryptAesKeyWithRsa(aesKey, serverPublicKey);
        Console.WriteLine("Khóa AES mã hóa đã được gửi: " + Convert.ToBase64String(encryptedAesKey));

        // Mã hóa dữ liệu với AES và gửi dữ liệu mã hóa (Giả sử gửi qua mạng)
        string message = "Hello, Server!";
        byte[] encryptedMessage = EncryptData(System.Text.Encoding.UTF8.GetBytes(message));
        Console.WriteLine("Dữ liệu mã hóa đã được gửi: " + Convert.ToBase64String(encryptedMessage));
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