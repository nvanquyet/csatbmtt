using Newtonsoft.Json;

namespace Shared.Models;

public enum MessageType
{
    Authentication,
    Registration,
    General
}

public enum StatusCode
{
    Success = 1000,
    Error = 1002,       
    Failed = 1004,
    InvalidRequest = 1005 
}

public class Message(MessageType type, StatusCode code, string content, Dictionary<string, object>? data)
{
    [JsonProperty("type")]
    public MessageType Type { get; set; } = type;

    [JsonProperty("code")]
    public StatusCode Code { get; set; } = code;

    [JsonProperty("message")]
    public string Content { get; set; } = content;

    [JsonProperty("data")]
    public Dictionary<string, object>? Data { get; set; } = data;

    public string ToJson() => JsonConvert.SerializeObject(this);
    public static Message? FromJson(string json) => JsonConvert.DeserializeObject<Message>(json);
}
