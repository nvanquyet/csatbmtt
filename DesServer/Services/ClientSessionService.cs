using System.Net.Sockets;
using System.Text;
using DesServer.Models;

namespace DesServer.Services
{
    public class ClientSessionService
    {
        private readonly Dictionary<string, Session?> _sessions = new();
        
        public void RegisterUser(TcpClient client, string username, string password)
        {
            if (_sessions.ContainsKey(username))
            {
                MessageService.SendTcpMessage(client, "Username already exists.");
                return;
            }

            // Create Session and User
            var session = new Session(new User(username, password));
            _sessions[username] = session;

            MessageService.SendTcpMessage(client, "User registered successfully!");
        }

        public void LoginUser(TcpClient client, string username, string password)
        {
            if (_sessions.TryGetValue(username, out var session))
            {
                if ((bool)session?.User.VerifyPassword(password))
                {
                    MessageService.SendTcpMessage(client, $"Login successful! Session ID: {session.SessionId}");
                }
                else
                {
                    MessageService.SendTcpMessage(client, "Invalid password.");
                }
            }
            else
            {
                MessageService.SendTcpMessage(client, "Username not found.");
            }
        }

        // End Session when user delete accounts
        public void EndSession(string username)
        {
            if (_sessions.ContainsKey(username))
            {
                _sessions[username]?.EndSession();
                _sessions.Remove(username);
            }
        }
    }
}
