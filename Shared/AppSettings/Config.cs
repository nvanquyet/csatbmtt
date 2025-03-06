namespace Shared.AppSettings;

public static class Config
{
    public const string ServerIp = "127.0.0.1";
    public const int ServerPort = 8080;
    
    public const string DatabaseConnectionString = "mongodb://localhost:27017";
    public const string DatabaseString = "Cosoantoan";

    public const int ServerTcpPort = 8000;
    public const int ServerUdpPort = 9000;
    
    public const string DefaultPassword = "123456";
    
    public const bool LogToConsole = true;
}