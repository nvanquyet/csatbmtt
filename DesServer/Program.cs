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
            
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                Console.WriteLine("🚪 Ứng dụng sắp thoát, giải phóng tài nguyên...");
                Cleanup();
            };

            Console.WriteLine("Server is running.... press Ctrl + C to exit...");
            while (true) { } // Giả lập ứng dụng chạy liên tục
        }

        static void Cleanup()
        {
            Console.WriteLine("🧹 Dọn dẹp tài nguyên...");
            _networkManager?.Dispose(); 
        }
        
    }
}