﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Shared.Models;

public enum CommandType
{
    None,
    Login,
    Registration,
    GetAvailableClients,
    DispatchMessage,
    ClientDisconnect,
    HandshakeRequest,
    HandshakeResponse,
    HandshakeCancel,
    GetHandshakeUsers,
    UpdateStatusUsers,
    CancelDispatchMessage,
}

public enum StatusCode
{
    Success = 1000,
    Error = 1002,       
    Failed = 1004,
    InvalidRequest = 1005 
}

public class MessageNetwork<T>(CommandType type, StatusCode code, T data)
{
    [JsonProperty("type")]
    public CommandType Type { get; set; } = type;

    [JsonProperty("code")]
    public StatusCode Code { get; set; } = code;

    [JsonProperty("data")]
    public T Data { get; set; } = data;

    public string ToJson() => JsonConvert.SerializeObject(this) + "\n";
    
    public static MessageNetwork<T>? FromJson(string json) => JsonConvert.DeserializeObject<MessageNetwork<T>>(json);
    
    public bool TryParseData<TV>(out TV? newData)
    {
        newData = default;
        try
        {
            if (Data is JObject jObject)
            {
                newData = jObject.ToObject<TV>();
                return newData != null;
            }
        
            if (Data is JToken token)
            {
                newData = token.ToObject<TV>();
                return newData != null;
            }
            
            if(Data is TV data)
            {
                newData = data;
                return true;
            }
            
        } catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing JObject: {ex.Message}");
            Console.WriteLine($"Failed to parse data. Expected type: {typeof(TV)}, but got: {Data?.GetType()}");
            return false;
        }
        return false;
    }
}