namespace Shared.Utils;

public static class SecurityUtils
{
    /// <summary>
    /// Generates a random DES key consisting of 8 bytes.
    /// Each byte in the key is adjusted to ensure the last bit is a parity bit.
    /// </summary>
    /// <returns>A byte array containing the randomly generated DES key (8 bytes).</returns>
    public static byte[] GenerateRandomDesKey()
    {
        var random = new Random();
        var key = new byte[8]; //Des need 8 byte

        random.NextBytes(key);

        for (var i = 0; i < key.Length; i++)
        {
            key[i] &= 0xFE;
            key[i] |= (byte)((ByteUtils.CountBits(key[i]) % 2 == 0) ? 1 : 0);
        }
        return key;
    }
}