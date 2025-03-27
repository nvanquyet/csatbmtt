using System.Collections;

namespace Shared.Security.Cryptography.Des;

public static class ByteArrayExtensions
{
    public static BitArray ToBitArray(this byte[] bytes, int bitCount)
    {
        BitArray bits = new BitArray(bitCount);
        for (int i = 0; i < bitCount; i++)
        {
            int byteIndex = i / 8;
            int bitInByte = 7 - (i % 8); // MSB first
            if (byteIndex < bytes.Length)
                bits[i] = (bytes[byteIndex] & (1 << bitInByte)) != 0;
        }

        return bits;
    }

    public static byte[] ToByteArray(this BitArray bits)
    {
        int byteCount = (bits.Length + 7) / 8;
        byte[] bytes = new byte[byteCount];
        for (int i = 0; i < bits.Length; i++)
        {
            int byteIndex = i / 8;
            int bitInByte = 7 - (i % 8);
            if (bits[i])
                bytes[byteIndex] |= (byte)(1 << bitInByte);
        }

        return bytes;
    }
}