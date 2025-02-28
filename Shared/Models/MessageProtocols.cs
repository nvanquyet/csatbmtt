using System.Net.Sockets;
using Newtonsoft.Json;

namespace Shared.Models;

public enum MessageType
{
    Authentication,
    Registration,
    RoomManagement,
    General
}

public enum StatusCode
{
    // Authentication & Registration (1000-1999)
    Success = 1000,
    UsernameAlreadyExists = 1001,
    UsernameNotFound = 1002,
    InvalidPassword = 1003,
    LoginFailed = 1004,

    // Room Management (2000-2999)
    RoomCreated = 2000,
    RoomIdExists = 2001,
    RoomJoined = 2002,
    RoomLeft = 2003,
    RoomNotFound = 2004,

    // General Errors (3000-3999)
    Error = 3000,       
    InvalidRequest = 3001,  
    ServerError = 3002    
}

public class Message(MessageType type, StatusCode code, string content, Dictionary<string, object> data)
{
    [JsonProperty("type")]
    public MessageType Type { get; set; } = type;

    [JsonProperty("code")]
    public StatusCode Code { get; set; } = code;

    [JsonProperty("message")]
    public string Content { get; set; } = content;

    [JsonProperty("data")]
    public Dictionary<string, object> Data { get; set; } = data;

    public string ToJson() => JsonConvert.SerializeObject(this);
    public static Message? FromJson(string json) => JsonConvert.DeserializeObject<Message>(json);
}
