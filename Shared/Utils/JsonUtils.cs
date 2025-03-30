using Newtonsoft.Json;

namespace Shared.Utils;
public static class JsonUtils
{
    public static byte[] Serialize<T>(T data)
    {
        var s = JsonConvert.SerializeObject(data);
        return ByteUtils.GetBytesFromString(s);
    }
    
    public static T? Deserialize<T>(byte[] fullData)
    {
        var data = ByteUtils.GetStringFromBytes(fullData);
        return JsonConvert.DeserializeObject<T>(data);
    }
}