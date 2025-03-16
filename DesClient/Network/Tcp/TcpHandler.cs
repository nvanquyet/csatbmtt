using System.Text;
using Shared.Models;
using Shared.Networking.Interfaces;
using Shared.Utils;

namespace DesClient.Network.Tcp;

public class TcpHandler : INetworkHandler
{
    private static StringBuilder _messageBuilder = new StringBuilder();
    public void OnDataReceived(byte[] data, string sourceEndpoint)
    {
        var message = ByteUtils.GetStringFromBytes(data);
        Console.WriteLine($"TCP Received from {sourceEndpoint}: {message}");
        OnDataReceived(message, sourceEndpoint);
    }

    public void OnDataReceived(string message, string sourceEndpoint) => HandleMessage(message);


    private Task HandleMessage(string message)
    {
        var msg = MessageNetwork<dynamic>.FromJson(message);
        if (msg?.Code == StatusCode.Success)
        {
            switch (msg.Type)
            {
                case CommandType.Authentication:
                    if (msg.TryParseData(out User? user))
                    {
                        //AuthService.SaveUserInfo(user);
                        //MainMenu.ShowMenu2(this);
                    }
                    break;
                case CommandType.Registration:
                    Console.WriteLine("Register Success");
                    //MainMenu.ShowMenu(this);
                    break;
                case CommandType.GetAvailableClients:
                   // if (msg.TryParseData(out List<User>? allUsers))
                        // (allUsers != null)
                            //ChatMenu.ChatWith(allUsers, this);
                    break;
                case CommandType.ReceiveMessage:
                    // if (msg.TryParseData(out ChatMessage? cM))
                    // {
                    //     //ChatMenu.LoadMessage(cM);
                    // }
                    break;
                case CommandType.LoadMessage:
                    // if (msg.TryParseData(out ChatMessage[]? allMessages))
                    // {
                    //     ChatMenu.LoadAllMessage(allMessages);
                    // }
                    // else
                    // {
                    //     Console.WriteLine("Load All Messages Failed");
                    // }
                    break;
                default:
                    Console.WriteLine("Unknown Command");
                    break;
            }
        }
        else
        {
            //MainMenu.ShowMenu(this);
        }

        return Task.CompletedTask;
    }
    
    public void OnClientConnected<T>(T? client) where T : class { }
}