using DesServer.Networking;
using Shared.AppSettings;

namespace DesServer
{
    static class Program
    {
        static void Main(string[] args)
        {
            var networkManager = new NetworkManager(Config.ServerIp, Config.ServerTcpPort, Config.ServerUdpPort);
            networkManager.Start();

            Console.WriteLine("Server is running. Press any key to exit...");
            Console.ReadKey();
        }
    }
}