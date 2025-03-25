using Shared.Models;

namespace Client.Models;

public static class SessionManager
{
    private static User? _currentUser;
    private static UserDto? _currentUserDto;
    public static void SetUser(User? user) => _currentUser = user;

    public static void Clear() => _currentUser = null;
    
    public static string? GetUserId() => _currentUser?.Id;
    private static string? GetUserName() => _currentUser?.UserName;
    
    public static UserDto GetUserDto() => _currentUserDto ??= new UserDto(GetUserId(), GetUserName(), UserStatus.Available);
    
}