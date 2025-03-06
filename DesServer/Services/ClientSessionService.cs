using DesServer.Database;
using Shared.Models;
using Shared.Services;

namespace DesServer.Services
{
    public class ClientSessionService : Singleton<ClientSessionService>
    {
        public AuthResult RegisterUser(string? username, string? password)
        {
            if (username == null || password == null)
                return new AuthResult(false, "Username or password must not empty");
            if (DbController.Instance.UsernameValidation(username))
            {
                return new AuthResult(false, "Username already exists.");
            }

            DbController.Instance.RegisterUser(username, SecurityHelper.HashPassword(password));
            return new AuthResult(true, "User registered successfully!");
        }

        public AuthResult LoginUser(string? username, string? password)
        {
            if (username == null || password == null)
                return new AuthResult(false, "Username or password must not empty");
            
            
            if (DbController.Instance.Login(username, password))
            {
                return new AuthResult(true, "Login Successful!");
            }
            else
            {
                return new AuthResult(false, "Invalid username or password.");
            }
        }
    }
}
