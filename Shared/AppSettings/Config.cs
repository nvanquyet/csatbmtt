﻿using System.Security.Cryptography;
using System.Text;
using Shared.Models;
using Shared.Utils;

namespace Shared.AppSettings;

public static class Config
{
    private static string _serverIp = "20.5.131.240";
    private static int _serverTcpPort = 8000;
    private static int _serverUdpPort = 9000;
    private static string _defaultPassword = "123456";
    private static int _keyDesEncryptionLength = (int)KeySize.KMin;
    private static int _keyRsaEncryptionLength = (int)KeySize.K1024;
    private static int _bufferSize = (int)BufferSize.Medium;

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

    public static bool LogToConsole { get; } = true;

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

    public static int KeyDesEncryptionLength => _keyDesEncryptionLength;

    public static int KeyRsaEncryptionLength => _keyRsaEncryptionLength;
    public static int BufferSizeLimit => _bufferSize;

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}

