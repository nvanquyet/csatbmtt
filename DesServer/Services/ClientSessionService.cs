using System.Net.Sockets;
using System.Text;
using DesServer.Models;
using Shared.Models;

namespace DesServer.Services
{
    public class ClientSessionService
    {
        private readonly Dictionary<string, Session?> _sessions = new();
        
        public AuthResult RegisterUser(string? username, string? password)
        {
            if (username != null && _sessions.ContainsKey(username))
            {
                return new AuthResult(false, "Username already exists.");
            }

            // Create Session and User
            var session = new Session(new User(username, password));
            if (username != null) _sessions[username] = session;

            return new AuthResult(true, "User registered successfully!");
        }

        public AuthResult LoginUser(string? username, string? password)
        {
            if (username != null && _sessions.TryGetValue(username, out var session))
            {
                if ((bool)session?.User.VerifyPassword(password))
                {
                    return new AuthResult(true, $"Login successful! Session ID: {session.SessionId}");
                }
                else
                {
                    return new AuthResult(false, "Invalid password.");
                }
            }
            else
            {
                return new AuthResult(false, "Username not found.");
            }
        }

        // End Session when user delete accounts
        public void EndSession(string? username)
        {
            if (username != null && _sessions.ContainsKey(username))
            {
                _sessions[username]?.EndSession();
                _sessions.Remove(username);
            }
        }
    }
}
