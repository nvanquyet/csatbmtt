using DesServer.Networking;
using Shared.AppSettings;

namespace DesServer
{
    static class Program
    {
        private static NetworkManager? _networkManager;
        static void Main(string[] args)
        {
            _networkManager = new NetworkManager(Config.ServerTcpPort, Config.ServerUdpPort);
            
            while (true) { } // Giả lập ứng dụng chạy liên tục
        }

        static void Cleanup()
        {
            Console.WriteLine("🧹 Dọn dẹp tài nguyên...");
            _networkManager?.Dispose(); 
        }
        
    }
}