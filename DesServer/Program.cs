using DesServer.AppSettings;
using DesServer.Networking;

namespace DesServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var networkManager = new NetworkManager(Config.ServerIp, Config.TcpPort, Config.UdpPort);
            networkManager.Start();

            Console.WriteLine("Server is running. Press any key to exit...");
            Console.ReadKey();
        }
    }
}