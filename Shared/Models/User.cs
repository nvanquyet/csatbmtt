using System.Net.Sockets;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Services;

namespace Shared.Models
{
    public class AuthResult(bool success, string message)
    {
        public bool Success { get; set; } = success;
        public string Message { get; set; } = message;
    }
    
    public class User(string? userName, string? password)
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string? UserName { get; init; } = userName;
        public string? Password { get; init; } = password;
        
        [BsonIgnore]
        public string StringId
        {
            get => Id.ToString();  // Trả về giá trị ObjectId dưới dạng chuỗi
            set => Id = ObjectId.Parse(value);  // Chuyển chuỗi thành ObjectId khi gán
        }
    }
}