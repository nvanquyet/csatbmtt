using System.Numerics;
using Shared.Security.Interface;

namespace Shared.Security.Algorithm;

class RsaAlgorithm : IEncryptionAlgorithm
{
    public BigInteger p, q, n, phi, e, d;
    public (BigInteger, BigInteger) PublicKey;  
    public (BigInteger, BigInteger) PrivateKey; 

    public RsaAlgorithm(int bitLength = 512)
    {
        GenerateKeys(bitLength);
    }

    private void GenerateKeys(int bitLength)
    {
        
        p = GenerateLargePrime(bitLength / 2);
        q = GenerateLargePrime(bitLength / 2);
        n = p * q;
        phi = (p - 1) * (q - 1);

        e = 65537;
        d = ModInverse(e, phi);

        PublicKey = (n, e);
        PrivateKey = (n, d);
    }

    private BigInteger GenerateLargePrime(int bits)
    {
        Random rand = new Random();
        BigInteger prime;
        do
        {
            prime = RandomBigInteger(bits, rand);
        } while (true);//!IsPrime(prime, 10));
        return prime;
    }


    private BigInteger RandomBigInteger(int bits, Random rand)
    {
        byte[] bytes = new byte[bits / 8];
        rand.NextBytes(bytes);
        bytes[^1] |= 0x01;
        return new BigInteger(bytes);
    }

    // private bool IsPrime(BigInteger number, int rounds)
    // {
    //     if (number < 2) return false;
    //     if (number % 2 == 0) return number == 2;
    //
    //     for (var i = 0; i < rounds; i++)
    //     {
    //         var a = RandomBigInteger(2, number - 1, new Random());
    //         if (BigInteger.ModPow(a, number - 1, number) != 1)
    //             return false;
    //     }
    //     return true;
    // }

    private static BigInteger ModInverse(BigInteger a, BigInteger m)
    {
        BigInteger m0 = m, t, q;
        BigInteger x0 = 0, x1 = 1;

        while (a > 1)
        {
            q = a / m;
            t = m;
            m = a % m;
            a = t;
            t = x0;
            x0 = x1 - q * x0;
            x1 = t;
        }

        return x1 < 0 ? x1 + m0 : x1;
    }
    
    public static BigInteger Encrypt(BigInteger message, (BigInteger n, BigInteger e) publicKey)
    {
        return BigInteger.ModPow(message, publicKey.e, publicKey.n);
    }

    public static BigInteger Decrypt(BigInteger ciphertext, (BigInteger n, BigInteger d) privateKey)
    {
        return BigInteger.ModPow(ciphertext, privateKey.d, privateKey.n);
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