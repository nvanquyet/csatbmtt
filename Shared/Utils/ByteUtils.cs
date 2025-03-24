using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Shared.Utils;

public partial class ByteUtils
{
    public static byte[] GetBytesFromString(string s) => Encoding.UTF8.GetBytes(s);
    
    public static string GetStringFromBytes(byte[] bytes) => Encoding.UTF8.GetString(bytes);
    
    public static byte[] GetBytes<T>(T value) where T : class
    {
        var size = Marshal.SizeOf(value);
        var bytes = new byte[size];

        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return bytes;
    }

    public static T? GetFromBytes<T>(byte[] bytes) where T : class
    {
        var size = Marshal.SizeOf<T>();
        if (bytes.Length != size)
            throw new ArgumentException($"Byte array length does not match size of {typeof(T)}");

        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(bytes, 0, ptr, size);
            return Marshal.PtrToStructure<T>(ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}