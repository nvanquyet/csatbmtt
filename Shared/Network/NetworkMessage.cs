using Newtonsoft.Json;

namespace Shared.Network;

public class NetworkMessage<T>(string type, T data)
{
    [JsonProperty("type")]
    public string Type { get; set; } = type;

    [JsonProperty("data")]
    public T Data { get; set; } = data;

    // Serialize object to JSON
    public string ToJson() => JsonConvert.SerializeObject(this);
    // Deserialize JSON to object
    public static NetworkMessage<T>? FromJson(string json) => JsonConvert.DeserializeObject<NetworkMessage<T>>(json);
}