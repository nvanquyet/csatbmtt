using DesServer.Networking;
using Shared.AppSettings;

namespace DesServer
{
    static class Program
    {
        static void Main(string[] args)
        {
            _ = new NetworkManager(Config.ServerTcpPort, Config.ServerUdpPort);
            
            Console.WriteLine("Server is running. Press any key to exit...");
            Console.ReadKey();
        }
    }
}