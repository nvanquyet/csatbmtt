using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Shared.Models;

public static class MessageParser
{
    public static bool TryParse<T>(JObject data, out T? result) where T : class
    {
        try
        {
            result = data.ToObject<T>();
            return result != null;
        }
        catch (JsonException)
        {
            result = null;
            return false;
        }
    }
}