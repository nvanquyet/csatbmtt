using Server.Database.Repositories;
using Shared.Models;
using Shared.Utils.Patterns;

namespace Server.Services
{
    public static class ClientSessionService
    {
        public static (bool, string) RegisterUser(string? username, string? password)
        {
            if (username == null || password == null)
                return (false, "Username or password must not empty");
            if (UserRepository.UsernameValidation(username))
            {
                return (false, "Username already exists.");
            }

            if(UserRepository.RegisterUser(username, password))
            {
                return (true, "User registered successfully!");
            }
            return (false, "Failed to register user.");
        }

        public static (bool, string) LoginUser(string? username, string? password, out User? user)
        {
            user = null;
            if (username == null || password == null)
                return (false, "Username or password must not empty");
            
            user = UserRepository.Login(username, password);
            
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
