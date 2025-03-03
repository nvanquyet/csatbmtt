namespace Shared.Models;

public class AuthData(string username, string password)
{
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
}



public class JoinRoomData(int roomId)
{
    public int RoomId { get; set; } = roomId;
}