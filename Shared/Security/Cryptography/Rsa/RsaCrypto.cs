using System.Numerics;
using Shared.AppSettings;
using Shared.Utils;
using Shared.Utils.Rsa;

namespace Shared.Security.Cryptography.Rsa;

public static class RsaCrypto
{
    public static KeyPair GenerateKeyPair(int bitLength)
    {
        //Generating primes, checking if the GCD of (n-1)(p-1) and e is 1.
        BigInteger q, p;
        do
        {
            q = FindPrime(bitLength / 2);
        } while (q % Constants.e == 1);

        do
        {
            p = FindPrime(bitLength / 2);
        } while (p % Constants.e == 1);

        //Setting n as QP, phi (represented here as x) to tortiary.
        var n = q * p;
        var x = (p - 1) * (q - 1);

        //Computing D such that ed = 1%x.
        var d = Maths.ModularInverse(Constants.e, x);

        //Returning results.
        return KeyPair.Generate(n, d);
    }

    //Finds a prime of the given bit length, to be used as n and p in RSA key calculations.
    private static BigInteger FindPrime(int bitLength)
    {
        //Generating a random number of bit length.
        if (bitLength % 8 != 0)
        {
            throw new Exception("Invalid bit length for key given, cannot generate primes.");
        }

        //Filling bytes with pseudorandom.
        var randomBytes = new byte[(bitLength / 8) + 1];
        Maths.Rand.NextBytes(randomBytes);
        //Making the extra byte 0x0 so the BigInts are unsigned (little endian).
        randomBytes[^1] = 0x0;

        //Setting the bottom bit and top two bits of the number.
        //This ensures the number is odd, and ensures the high bit of N is set when generating keys.
        ByteUtils.SetBitInByte(0, ref randomBytes[0]);
        ByteUtils.SetBitInByte(7, ref randomBytes[^2]);
        ByteUtils.SetBitInByte(6, ref randomBytes[^2]);

        while (true)
        {
            //Performing a Rabin-Miller primality test.
            var isPrime = Maths.RabinMillerTest(randomBytes, 40);
            if (isPrime)
            {
                break;
            }
            else
            {
                ByteUtils.IncrementByteArrayLe(ref randomBytes, 2);
                var upperLimit = new byte[randomBytes.Length];

                //Clearing upper bit for unsigned, creating upper and lower bounds.
                upperLimit[randomBytes.Length - 1] = 0x0;
                var upperLimitBi = new BigInteger(upperLimit);
                var lowerLimit = upperLimitBi - 20;
                var current = new BigInteger(randomBytes);

                if (lowerLimit < current && current < upperLimitBi)
                {
                    //Failed to find a prime, returning -1.
                    //Reached limit with no solutions.
                    return new BigInteger(-1);
                }
            }
        }

        //Returning working BigInt.
        return new BigInteger(randomBytes);
    }

    private static byte[]? Encrypt(byte[]? bytes, Key publicKey)
    {
        //Checking that the size of the bytes is less than n, and greater than 1.
        if (1 > bytes.Length || bytes.Length >= publicKey.n.ToByteArray().Length)
        {
            throw new Exception($"Bytes given are longer than length of key element n ({bytes.Length} bytes). Current Length{publicKey.n.ToByteArray().Length}");
        }

        //Padding the array to unsign.
        var bytesPadded = new byte[bytes.Length + 2];
        Array.Copy(bytes, bytesPadded, bytes.Length);
        bytesPadded[^1] = 0x00;

        //Setting high byte right before the data, to prevent data loss.
        bytesPadded[^2] = 0xFF;

        //Computing as a BigInteger the encryption operation.
        var paddedBigint = new BigInteger(bytesPadded);
        var cipherBigint = BigInteger.ModPow(paddedBigint, publicKey.e, publicKey.n);

        //Returning the byte array of encrypted bytes.
        return cipherBigint.ToByteArray();
    }

    //Decrypts a set of bytes when given a private key.
    private static byte[]? Decrypt(byte[]? bytes, Key privateKey)
    {
        //Checking that the private key is legitimate, and contains d.
        if (privateKey.type != KeyType.PRIVATE)
        {
            throw new Exception("Private key given for decrypt is classified as non-private in instance.");
        }

        //Decrypting.
        var paddedBigint = new BigInteger(bytes);
        var plainBigint = BigInteger.ModPow(paddedBigint, privateKey.d, privateKey.n);

        //Removing all padding bytes, including the marker 0xFF.
        var plainBytes = plainBigint.ToByteArray();
        var lengthToCopy = -1;
        for (int i = plainBytes.Length - 1; i >= 0; i--)
        {
            if (plainBytes[i] != 0xFF) continue;
            lengthToCopy = i;
            break;
        }

        //Checking for a failure to find marker byte.
        if (lengthToCopy == -1)
        {
            throw new Exception(
                "Marker byte for padding (0xFF) not found in plain bytes.\nPossible Reasons:\n1: PAYLOAD TOO LARGE\n2: KEYS INVALID\n3: ENCRYPT/DECRYPT FUNCTIONS INVALID");
        }

        //Copying into return array, returning.
        var returnArray = new byte[lengthToCopy];
        Array.Copy(plainBytes, returnArray, lengthToCopy);
        return returnArray;
    }

    public static byte[]? Encrypt(byte[]? data, byte[] key)
    {
        var keyEncrypt = Key.FromBytes(key);
        return Encrypt(data, keyEncrypt);
    }

    public static byte[]? Decrypt(byte[]? encryptedData, byte[] key)
    {
        var keyDecrypt = Key.FromBytes(key);
        return Decrypt(encryptedData, keyDecrypt);
    }

}