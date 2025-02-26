using System.Security.Cryptography;
using System.Text;
using DesServer.AppSettings;

namespace DesServer.Models
{
    public class User(string? userName, string? password)
    {
        public string? UserName { get; set; } = userName;
        private string? Password { get; set; } = HashPassword(password);


        private static string HashPassword(string? password)
        {
            if (string.IsNullOrEmpty(password))
                return Config.DefaultUserPassword;

            using SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        public bool VerifyPassword(string password)
        {
            var passCheck = HashPassword(password);
            return Password == passCheck;
        }
    }
}