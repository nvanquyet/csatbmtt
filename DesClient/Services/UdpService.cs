// using System.Net.Sockets;
// using System.Text;
// using Shared.AppSettings;
//
// namespace DesClient.Services;
//
// public class UdpService
// {
//     private readonly UdpClient _udpClient = new();
//
//     public void SendUdpMessage(string message)
//     {
//         byte[] data = Encoding.UTF8.GetBytes(message);
//         _udpClient.Send(data, data.Length, Config.ServerIp, Config.ServerUdpPort);
//         Console.WriteLine("Sent UDP: " + message);
//     }
// }