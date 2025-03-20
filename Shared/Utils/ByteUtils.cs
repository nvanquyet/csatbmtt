using System.Text;

namespace Shared.Utils;

public static class ByteUtils
{
    private static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }

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
            int val = ByteUtils.GetBitAt(inner, pos + i);
            ByteUtils.SetBitAt(outer, i, val);
        }

        return outer;
    }

    public static byte[] SelectBits(byte[] inner, byte[] map)
    {
        int numOfBytes = (map.Length - 1) / 8 + 1;
        byte[] outer = new byte[numOfBytes];
        for (int i = 0; i < map.Length; i++)
        {
            int val = GetBitAt(inner, map[i] - 1);
            SetBitAt(outer, i, val);
        }

        return outer;
    }

    public static int GetBitAt(byte[] data, int poz)
    {
        int posByte = poz / 8;
        int posBit = poz % 8;
        byte valByte = data[posByte];
        int valInt = valByte >> (7 - posBit) & 1;
        return valInt;
    }

    //Sets or clears a bit at the specified position in the given byte array.
    public static void SetBitAt(byte[] data, int pos, int val)
    {
        byte oldByte = data[pos / 8];
        oldByte = (byte)(((0xFF7F >> (pos % 8)) & oldByte) & 0x00FF);
        byte newByte = (byte)((val << (7 - (pos % 8))) | oldByte);
        data[pos / 8] = newByte;
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

    public static int CountBits(byte b)
    {
        var count = 0;
        while (b != 0)
        {
            count += (b & 1);
            b >>= 1;
        }
        return count;
    }
    
    public static byte[] RotateRight(byte[] inner, int len, int step)
    {
        return RotateLeft(inner, len, 28 - step);
    }

    public static byte[] GetBytesFromString(string s) => Encoding.UTF8.GetBytes(s);
    
    public static string GetStringFromBytes(byte[] bytes) => Encoding.UTF8.GetString(bytes);

    // public static String bytesToHex(byte bytes[])
    // {
    //     byte []rawData = bytes;
    //     StringBuilder hexText = new StringBuilder();
    //     String initialHex = null;
    //     int initHexLength = 0;
    //
    //     for (int i = 0; i < rawData.length; i++)
    //     {
    //         int positiveValue = rawData[i] & 0x000000FF;
    //         initialHex = Integer.toHexString(positiveValue);
    //         initHexLength = initialHex.length();
    //         while (initHexLength++ < 2)
    //         {
    //             hexText.append("0");
    //         }
    //         hexText.append(initialHex);
    //     }
    //     return hexText.toString();
    // }
}