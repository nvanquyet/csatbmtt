using System.Security.Cryptography;
using System.Text;

namespace Shared.AppSettings;

public static class Config
{
    private static string _serverIp = "20.5.131.240";
    private static int _serverTcpPort = 8000;
    private static int _serverUdpPort = 9000;
    private static bool _logToConsole = true;
    private static string _defaultPassword = "123456";
    private static int _keyEncryptionLength = 256;

    public static string ServerIp
    {
        get => _serverIp;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Server IP không được để trống");
            _serverIp = value;
        }
    }

    public static int ServerTcpPort
    {
        get => _serverTcpPort;
        set
        {
            if (value <= 0 || value > 65535)
                throw new ArgumentException("Port TCP phải nằm trong khoảng 1-65535");
            _serverTcpPort = value;
        }
    }

    public static int ServerUdpPort
    {
        get => _serverUdpPort;
        set
        {
            if (value <= 0 || value > 65535)
                throw new ArgumentException("Port UDP phải nằm trong khoảng 1-65535");
            _serverUdpPort = value;
        }
    }

    public static bool LogToConsole
    {
        get => _logToConsole;
        set => _logToConsole = value;
    }

    public static string DefaultPassword
    {
        get => _defaultPassword;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 6)
                throw new ArgumentException("Mật khẩu mặc định phải có ít nhất 6 ký tự");
            _defaultPassword = HashPassword(value);
        }
    }

    public static int KeyEncryptionLength
    {
        get => _keyEncryptionLength;
        set
        {
            if (value < 128 || value > 512)
                throw new ArgumentException("Độ dài khóa mã hóa phải nằm trong khoảng 128-512 bits");
            _keyEncryptionLength = value;
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}