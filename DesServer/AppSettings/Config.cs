namespace DesServer.AppSettings;

public static class Config
{
    public const string ServerIp = "127.0.0.1";
    public const int ServerPort = 8080;
    
    public const string DatabaseConnectionString = "mongodb+srv://dbVawnWuyet:Quyet8102003@cluster0.nouhj.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";

    public const int TcpPort = 8000;
    public const int UdpPort = 9000;
    
    public const string DefaultUserPassword = "123456";
}