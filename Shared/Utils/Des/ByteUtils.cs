namespace Shared.Utils;

public partial class ByteUtils
{
    public static byte[] XORBytes(byte[] a, byte[] b)
    {
        byte[] outer = new byte[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            outer[i] = (byte)(a[i] ^ b[i]);
        }

        return outer;
    }
    
    public static string GetFileSize(long byteLength)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        var order = 0;
        double size = byteLength;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }

    public static byte[] SelectBits(byte[] inner, int pos, int len)
    {
        int numOfBytes = (len - 1) / 8 + 1;
        byte[] outer = new byte[numOfBytes];
        for (int i = 0; i < len; i++)
        {
            int val = GetBitAt(inner, pos + i);
            SetBitAt(outer, i, val);
        }

        return outer;
    }

    public static int GetBitAt(byte[] data, int position)
    {
        int byteIndex = position / 8;
        int bitIndex = 7 - (position % 8); // MSB first
        return (data[byteIndex] >> bitIndex) & 0x01;
    }

    public static void SetBitAt(byte[] data, int position, int value)
    {
        int byteIndex = position / 8;
        int bitIndex = 7 - (position % 8); // MSB first
        if (value == 1)
            data[byteIndex] |= (byte)(1 << bitIndex);
        else
            data[byteIndex] &= (byte)~(1 << bitIndex);
    }

    public static byte[] RotateLeft(byte[] inner, int len, int step)
    {
        byte[] outer = new byte[(len - 1) / 8 + 1];
        for (int i = 0; i < len; i++)
        {
            int val = GetBitAt(inner, (i + step) % len);
            SetBitAt(outer, i, val);
        }

        return outer;
    }
}