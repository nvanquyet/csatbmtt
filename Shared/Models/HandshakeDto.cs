namespace Shared.Models;

public class HandshakeDto(UserDto? fromUser, UserDto? toUser)
{
    public UserDto? FromUser { get; init; } = fromUser;
    public UserDto? ToUser { get; init; } = toUser;
    public string Description { get; set; } = string.Empty;
    public bool Accepted  { get; set; } = false;
}
