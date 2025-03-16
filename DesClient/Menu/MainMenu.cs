using System.Net.Sockets;
using System.Security.Cryptography;
using DesClient.Network.Tcp;
using DesClient.Services;
using MongoDB.Bson;
using Shared.Models;
using Shared.Utils;

namespace DesClient.Menu;

public static class MainMenu
{
    public static void ShowMenu(TcpProtocol tcpClient)
    {
        Console.Clear();
        Console.WriteLine("=== Menu 1 ===");
        Console.WriteLine("1. Nhập số điện thoại");
        Console.WriteLine("2. Thoát");
        Console.Write("Chọn một tùy chọn (1 -2): ");

        switch (Console.ReadLine())
        {
            case "1":
                Console.Write("Nhập số điện thoại: ");
                string? phoneNumber = Console.ReadLine();

                if (string.IsNullOrEmpty(phoneNumber))
                {
                    Console.WriteLine("Số điện thoại không hợp lệ. Vui lòng thử lại.");
                    ShowMenu(tcpClient); 
                    return;
                }
                
                //Todo: Register client info to server
                
                break;
            case "2":
                Environment.Exit(0); 
                break;
            default:
                Console.WriteLine("Lựa chọn không hợp lệ.");
                ShowMenu(tcpClient);
                break;
        }
    }

    // public static void ShowMenu2(TcpProtocol tcpService)
    // {
    //     Console.Clear();
    //     Console.WriteLine("=== Menu 2 ===");
    //     Console.WriteLine("1. Chat với người dùng khác");
    //     Console.WriteLine("2. Đăng xuất");
    //     Console.Write("Chọn một tùy chọn (1-2): ");
    //
    //     switch (Console.ReadLine())
    //     {
    //         case "1":
    //             tcpService.SendTcpMessage(
    //                 new MessageNetwork<string>(CommandType.GetAvailableClients, StatusCode.Success, "")
    //                     .ToJson());
    //             break;
    //         case "2":
    //             AuthService.Logout();
    //             ShowMenu(tcpService);
    //             break;
    //         default:
    //             Console.WriteLine("Lựa chọn không hợp lệ.");
    //             ShowMenu2(tcpService);
    //             break;
    //     }
    // }
}