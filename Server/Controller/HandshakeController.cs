using System.Collections.Concurrent;

namespace Server.Controller;

public static class HandshakeController
{
    // ConcurrentDictionary to store handshake state:
    // Key: UserId, Value: Partner UserId with whom the user is currently in handshake.
    // If a user is not engaged in a handshake, their key will not exist.
    private static readonly ConcurrentDictionary<string, string> HandshakeMapping = new();

    /// <summary>
    /// Checks if the user is available for a handshake.
    /// </summary>
    /// <param name="userId">The user id to check.</param>
    /// <returns>True if the user is not currently in a handshake; otherwise, false.</returns>
    public static bool IsUserAvailableForHandshake(string userId)
    {
        // If the user is not in the mapping, then they are available (i.e., not in a handshake)
        return !HandshakeMapping.ContainsKey(userId);
    }

    /// <summary>
    /// Initiates a handshake between two users.
    /// If both users are available, stores the handshake state and returns true.
    /// Otherwise, returns false.
    /// </summary>
    /// <param name="userId">The user id initiating the handshake.</param>
    /// <param name="partnerUserId">The partner user id for the handshake.</param>
    /// <returns>True if handshake is successfully started; otherwise, false.</returns>
    public static bool StartHandshake(string userId, string partnerUserId)
    {
        // Check if both users are available for handshake
        if (!IsUserAvailableForHandshake(userId) || !IsUserAvailableForHandshake(partnerUserId))
        {
            return false;
        }

        // Attempt to add the handshake state for both users
        var addedUser = HandshakeMapping.TryAdd(userId, partnerUserId);
        var addedPartner = HandshakeMapping.TryAdd(partnerUserId, userId);

        // If unable to add for both users, rollback any partial addition
        if (addedUser && addedPartner) return true;
        HandshakeMapping.TryRemove(userId, out _);
        HandshakeMapping.TryRemove(partnerUserId, out _);
        return false;
    }

    /// <summary>
    /// Ends the handshake for a user and also removes the handshake state for the partner if present.
    /// </summary>
    /// <param name="userId">The user id whose handshake is to be ended.</param>
    public static void EndHandshake(string userId)
    {
        if (HandshakeMapping.TryRemove(userId, out var partnerUserId))
        {
            // Also remove the mapping for the partner if they are in a handshake with this user
            HandshakeMapping.TryRemove(partnerUserId, out _);
        }
    }

    /// <summary>
    /// Retrieves the handshake partner of the given user, if any.
    /// </summary>
    /// <param name="userId">The user id to query.</param>
    /// <returns>The partner user id if found; otherwise, null.</returns>
    public static string? GetHandshakePartner(string userId) => HandshakeMapping.TryGetValue(userId, out var partner) ? partner : null;
}