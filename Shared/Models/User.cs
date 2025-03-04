using Shared.Services;

namespace Shared.Models
{
    public class User(string? userName, string? password)
    {
        public string? UserName { get; set; } = userName;
        private string? Password { get; set; } = SecurityHelper.HashPassword(password);
        public bool VerifyPassword(string? password) => SecurityHelper.VerifyPassword(Password, password);
    }
}