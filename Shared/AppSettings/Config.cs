namespace Shared.AppSettings;

public static class Config
{
    //public const string ServerIp = "127.0.0.1";
    public const string ServerIp = "20.5.131.240";

    public const int ServerTcpPort = 8000;
    public const int ServerUdpPort = 9000;
    
    public const bool LogToConsole = true;
    public const string DefaultPassword = "123456";
    
    public const int KeyEncryptionLength = 256;
}