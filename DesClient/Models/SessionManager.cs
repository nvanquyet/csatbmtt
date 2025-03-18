﻿using Shared.Models;

namespace DesClient.Models;

public static class SessionManager
{
    private static User? _currentUser;

    public static void SetUser(User? user) => _currentUser = user;

    public static void Clear() => _currentUser = null;
    
    public static string? GetUserId() => _currentUser?.Id;
    public static string? GetUserName() => _currentUser?.UserName;
    
}