using DesServer.Database;
using Shared.Models;
using Shared.Services;

namespace DesServer.Services
{
    public class ClientSessionService : Singleton<ClientSessionService>
    {
        public (bool, string) RegisterUser(string? username, string? password)
        {
            if (username == null || password == null)
                return (false, "Username or password must not empty");
            if (UserDatabase.UsernameValidation(username))
            {
                return (false, "Username already exists.");
            }

            UserDatabase.RegisterUser(username, password);
            return (true, "User registered successfully!");
        }

        public (bool, string) LoginUser(string? username, string? password, out User? user)
        {
            user = null;
            if (username == null || password == null)
                return (false, "Username or password must not empty");
            
            user = UserDatabase.Login(username, password);
            
            if (user != null)
            {
                return (true, "Login Successful!");
            }
            else
            {
                return (false, "Invalid username or password.");
            }
        }
    }
}
