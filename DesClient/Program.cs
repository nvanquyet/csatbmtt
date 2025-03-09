using DesClient.Menu;
using DesClient.Services;
namespace DesClient
{
    static class Program
    {
        private static readonly TcpService TcpService = new();
        private static readonly UdpService UdpService = new();

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Connecting to server...");
            await TcpService.ConnectAsync();
            Console.WriteLine("Connected to TCP Server!");

            _ = TcpService.ListenForTcpMessagesAsync();
            if (TcpService.GetTcpClient() != null && AuthService.TryAutoLogin(TcpService.GetTcpClient()))
            {
                Console.WriteLine("Đang thử đăng nhập với tài khoản đã lưu...");
            }
            else MainMenu.ShowMenu(TcpService);
            while (true)
            {
                
            }
        }
    }
}