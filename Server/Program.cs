using Server.Networking;
using Shared;
using Shared.AppSettings;

namespace Server
{
    static class Program
    {
        private static NetworkManager? _networkManager;
        static void Main(string[] args)
        {
            _networkManager = new NetworkManager(Config.ServerTcpPort, Config.ServerUdpPort);
            
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                Logger.LogInfo("🚪 Ứng dụng sắp thoát, giải phóng tài nguyên...");
                Cleanup();
            };

            Logger.LogInfo("Server is running.... press Ctrl + C to exit...");
            while (true) { } // Giả lập ứng dụng chạy liên tục
        }

        static void Cleanup()
        {
            Logger.LogInfo("🧹 Dọn dẹp tài nguyên...");
            _networkManager?.Stop(); 
        }
        
    }
}