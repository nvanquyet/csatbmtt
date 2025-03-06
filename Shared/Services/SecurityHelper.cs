using System.Security.Cryptography;
using System.Text;
using Shared.AppSettings;

namespace Shared.Services;

public static class SecurityHelper
{
    public static string HashPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return Config.DefaultPassword;

        using SHA256 sha256Hash = SHA256.Create();
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }
}