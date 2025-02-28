namespace Shared.Models;

public class LoginData(string username, string password)
{
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
}

public class RegisterData(string username, string password, string? confirmPassword) : LoginData(username, password)
{
    public string? ConfirmPassword { get; set; } = confirmPassword;
}


public class JoinRoomData(int roomId)
{
    public int RoomId { get; set; } = roomId;
}