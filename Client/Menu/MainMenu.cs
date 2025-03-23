// using Client.Services;
// using Client.Models;
// using Client.Services;
// using Shared.Models;
// using Shared.Networking.Interfaces;
//
// namespace Client.Menu;
//
// public static class MainMenu
// {
//     public static void ShowMenu(INetworkProtocol protocol)
//     {
//         while (true)
//         {
//             Console.WriteLine("=== Menu 1 ===");
//             Console.WriteLine("1. Login");
//             Console.WriteLine("2. Register");
//             Console.WriteLine("3. Exit");
//             Console.Write("Select Option (1 -2): ");
//
//             switch (Console.ReadLine())
//             {
//                 case "1":
//                     AuthService.Login(protocol);
//                     break;
//                 case "2":
//                     AuthService.Register(protocol);
//                     break;
//                 case "3":
//                     Environment.Exit(0);
//                     break;
//                 default:
//                     Console.WriteLine("Select a valid option.");
//                     continue;
//             }
//
//             break;
//         }
//     }
//
//     public static void ShowMenu2(INetworkProtocol protocol)
//     {
//         while (true)
//         {
//             Console.WriteLine("=== Menu 2 ===");
//             Console.WriteLine("1. Find all users");
//             Console.WriteLine("2. Log out");
//             Console.Write("Select Option (1-2): ");
//
//             switch (Console.ReadLine())
//             {
//                 case "1":
//                     var response =
//                         new MessageNetwork<string?>(type: CommandType.GetAvailableClients, StatusCode.Success,
//                                 data: SessionManager.GetUserId())
//                             .ToJson();
//                     protocol.Send(response, "");
//                     break;
//                 case "2":
//                     Logout(protocol);
//                     ShowMenu(protocol);
//                     break;
//                 default:
//                     Console.WriteLine("Input a valid option.");
//                     continue;
//             }
//
//             break;
//         }
//     }
//
//     private static void Logout(INetworkProtocol protocol)
//     {
//         AuthService.Logout();
//         ShowMenu(protocol);
//     } 
//         
// }