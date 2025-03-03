namespace DesServer.Models;

public class AuthResult(bool success, string message)
{
    public bool Success { get; set; } = success;
    public string Message { get; set; } = message;
}