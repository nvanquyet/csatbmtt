using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models;

public class User(string? userName, string? password)
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)] 
    public string? Id { get; init; }
    public string? UserName { get; init; } = userName;
    public string? Password { get; init; } = password;
}

public class UserDto(string id, string userName)
{
    public string Id { get; set; } = id;
    public string UserName { get; set; } = userName;
}