// using System.Net.Sockets;
// using System.Text;
// using DesClient.Menu;
// using Shared.AppSettings;
// using Shared.Models;
//
// namespace DesClient.Services;
//
// public class TcpService
// {
//     private TcpClient? _tcpClient;
//     private NetworkStream? _tcpStream;
//     
//     public TcpClient? GetTcpClient() => _tcpClient;
//     
//     public async Task ConnectAsync()
//     {
//         _tcpClient = new TcpClient();
//         await _tcpClient.ConnectAsync(Config.ServerIp, Config.ServerTcpPort);
//         _tcpStream = _tcpClient.GetStream();
//     }
//
//     public void SendTcpMessage(string message)
//     {
//         if (_tcpStream == null) return;
//         byte[] data = Encoding.UTF8.GetBytes(message);
//         _tcpStream.Write(data, 0, data.Length);
//     }
//
//     
//     public async Task ListenForTcpMessagesAsync()
//     {
//         byte[] buffer = new byte[1024];
//         StringBuilder messageBuilder = new StringBuilder();
//
//         while (true)
//         {
//             try
//             {
//                 int bytesRead = await _tcpStream?.ReadAsync(buffer, 0, buffer.Length)!;
//                 if (bytesRead > 0)
//                 {
//                     messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
//
//                     // Kiểm tra kiểu dữ liệu trong "data"
//                     string receivedMessage = messageBuilder.ToString();
//                 
//                     // Kiểm tra nếu dữ liệu là đối tượng hoặc mảng
//                     if ( (receivedMessage.Contains("\"data\":\"") || receivedMessage.Contains("\"data\":{")) && receivedMessage.Contains("}"))
//                     {
//                         // Nếu "data" là đối tượng, kiểm tra dấu đóng của đối tượng JSON
//                         if (receivedMessage.Contains("}"))
//                         {
//                             // Xử lý thông điệp đầy đủ
//                             _ = HandleMessage(receivedMessage);
//                             messageBuilder.Clear();  // Xóa dữ liệu đã xử lý
//                         }
//                     }
//                     else if (receivedMessage.Contains("\"data\":[{") && receivedMessage.Contains("}]}"))
//                     {
//                         // Nếu "data" là mảng, kiểm tra dấu đóng của mảng JSON
//                         if (receivedMessage.Contains("}]}"))
//                         {
//                             // Xử lý thông điệp đầy đủ
//                             _ = HandleMessage(receivedMessage);
//                             messageBuilder.Clear();  // Xóa dữ liệu đã xử lý
//                         }
//                     }
//                 }
//                 else
//                 {
//                     Console.WriteLine("No data received; connection may be closed.");
//                     break;
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error reading TCP message: " + ex.Message);
//                 break;
//             }
//         }
//     }
//     
//     private Task HandleMessage(string message)
//     {
//         var msg = MessageNetwork<dynamic>.FromJson(message);
//         if (msg?.Code == StatusCode.Success)
//         {
//             switch (msg.Type)
//             {
//                 case CommandType.Authentication:
//                     if (msg.TryParseData(out User? user))
//                     {
//                         AuthService.SaveUserInfo(user);
//                         MainMenu.ShowMenu2(this);
//                     }
//                     break;
//                 case CommandType.Registration:
//                     Console.WriteLine("Register Success");
//                     MainMenu.ShowMenu(this);
//                     break;
//                 case CommandType.GetAvailableClients:
//                     if (msg.TryParseData(out List<User>? allUsers))
//                         if (allUsers != null)
//                             ChatMenu.ChatWith(allUsers, this);
//                     break;
//                 case CommandType.ReceiveMessage:
//                     if (msg.TryParseData(out ChatMessage? cM))
//                     {
//                         ChatMenu.LoadMessage(cM);
//                     }
//                     break;
//                 case CommandType.LoadMessage:
//                     if (msg.TryParseData(out ChatMessage[]? allMessages))
//                     {
//                         ChatMenu.LoadAllMessage(allMessages);
//                     }
//                     else
//                     {
//                         Console.WriteLine("Load All Messages Failed");
//                     }
//                     break;
//                 default:
//                     Console.WriteLine("Unknown Command");
//                     break;
//             }
//         }
//         else
//         {
//             MainMenu.ShowMenu(this);
//         }
//
//         return Task.CompletedTask;
//     }
// }