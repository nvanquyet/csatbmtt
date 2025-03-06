using Newtonsoft.Json;

namespace Shared.Models;

public enum CommandType
{
    None,
    Authentication,
    Registration,
    GetAllUsers,
}

public enum StatusCode
{
    Success = 1000,
    Error = 1002,       
    Failed = 1004,
    InvalidRequest = 1005 
}

public class MessageNetwork<T>(CommandType type, StatusCode code, T data) where T : class
{
    [JsonProperty("type")]
    public CommandType Type { get; set; } = type;

    [JsonProperty("code")]
    public StatusCode Code { get; set; } = code;

    [JsonProperty("data")]
    public T Data { get; set; } = data;

    public string ToJson() => JsonConvert.SerializeObject(this);
    public static MessageNetwork<T>? FromJson(string json) => JsonConvert.DeserializeObject<MessageNetwork<T>>(json);
    
    public bool TryParseData<TV>(out TV? newData) where TV : class
    {
        newData = null;
    
        if (Data is TV data)
        {
            newData = data;
            return true;
        }

        return false;
    }

}