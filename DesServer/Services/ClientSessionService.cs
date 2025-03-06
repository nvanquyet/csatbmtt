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
            if (DbController.Instance.UsernameValidation(username))
            {
                return (false, "Username already exists.");
            }

            DbController.Instance.RegisterUser(username, password);
            return (true, "User registered successfully!");
        }

        public (bool, string) LoginUser(string? username, string? password)
        {
            if (username == null || password == null)
                return (false, "Username or password must not empty");
            
            
            if (DbController.Instance.Login(username, password))
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
