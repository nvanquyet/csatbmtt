using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Shared.Utils;

public partial class ByteUtils
{
    public static byte[] GetBytesFromString(string plainText) 
        => Encoding.ASCII.GetBytes(plainText);
    
    public static string GetStringFromBytes(byte[] bytes) 
        => Encoding.ASCII.GetString(bytes);
    
    public static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }
    
    public static string ByteArrayToString(byte[] ba)
    {
        StringBuilder hex = new StringBuilder(ba.Length * 2);
        foreach (byte b in ba)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }

}