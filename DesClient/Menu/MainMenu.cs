using DesClient.Services;
using Shared.Models;

namespace DesClient.Menu;

public static class MainMenu
{
    public static void ShowMenu(TcpService tcpService)
    {
        Console.Clear();
        Console.WriteLine("=== Menu 1 ===");
        Console.WriteLine("1. Đăng nhập");
        Console.WriteLine("2. Đăng ký");
        Console.WriteLine("3. Thoát");
        Console.Write("Chọn một tùy chọn (1-3): ");

        switch (Console.ReadLine())
        {
            case "1":
                AuthService.Login(tcpService);
                break;
            case "2":
                AuthService.Register(tcpService);
                break;
            case "3":
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Lựa chọn không hợp lệ.");
                ShowMenu(tcpService);
                break;
        }
    }

    public static void ShowMenu2(TcpService tcpService)
    {
        Console.Clear();
        Console.WriteLine("=== Menu 2 ===");
        Console.WriteLine("1. Chat với người dùng khác");
        Console.WriteLine("2. Đăng xuất");
        Console.Write("Chọn một tùy chọn (1-2): ");

        switch (Console.ReadLine())
        {
            case "1":
                tcpService.SendTcpMessage(new MessageNetwork<string>(CommandType.GetAllUsers, StatusCode.Success, "")
                    .ToJson());
                break;
            case "2":
                AuthService.Logout();
                ShowMenu(tcpService);
                break;
            default:
                Console.WriteLine("Lựa chọn không hợp lệ.");
                ShowMenu2(tcpService);
                break;
        }
    }

   
}