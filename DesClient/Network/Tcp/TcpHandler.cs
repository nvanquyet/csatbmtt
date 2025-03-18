using DesClient.Menu;
using DesClient.Services;
using Shared.Models;
using Shared.Networking.Interfaces;
using Shared.Security.Interface;
using Shared.Services;
using Shared.Utils;

namespace DesClient.Network.Tcp;

public class TcpHandler : INetworkHandler
{
    public void OnDataReceived(byte[] data, string sourceEndpoint)
    {
        var message = ByteUtils.GetStringFromBytes(data);
        OnDataReceived(message, sourceEndpoint);
    }

    public void OnDataReceived(string message, string sourceEndpoint) => HandleMessage(message);


    private static Task HandleMessage(string message)
    {
        var msg = MessageNetwork<dynamic>.FromJson(message);
        if (msg?.Code != StatusCode.Success) return Task.CompletedTask;
        switch (msg.Type)
        {
            case CommandType.Login:
                if (msg.TryParseData(out User? user) && user != null)
                {
                    AuthService.SaveUserInfo(user);

                    //Todo: Register Rsa public key
                    if (user.Id != null)
                    {
                        var clientInfo = new ClientInfo(id: user.Id, EncryptionService.Instance
                            .GetAlgorithm(EncryptionType.Rsa)
                            .EncryptKey);
                        var response = new MessageNetwork<ClientInfo>(type: CommandType.RegisterClientRsaKey,
                            code: StatusCode.Success, data: clientInfo).ToJson();
                        //Send to server
                        NetworkManager.Instance.TcpService.Send(response, "");
                    }

                    MainMenu.ShowMenu2(NetworkManager.Instance.TcpService);
                }

                break;
            case CommandType.Registration:
                Console.WriteLine("Register Success");
                break;
            case CommandType.RegisterClientRsaKey:
                Console.WriteLine("Register RSA public key Success");
                break;
            case CommandType.GetAvailableClients:
                if (msg.TryParseData(out List<UserDto>? allUsers) && allUsers != null)
                {
                    ChatMenu.ChatWith(allUsers);
                }

                break;
            case CommandType.ChatRequest:
                if (msg.TryParseData(out ChatRequestDto? dto) && dto != null)
                {
                    ChatMenu.ShowBoxConfirm(dto);
                }
                break;
            case CommandType.ChatResponse:
                if (msg.TryParseData(out ChatResponseDto? r) && r != null)
                {
                    Console.WriteLine($"User {r.FromUser?.UserName} {(r.Accepted ? "Accepted" : "Rejected")}");
                    if (r.Accepted) ChatMenu.ShowChatMenu(r.FromUser, NetworkManager.Instance.TcpService, false);
                }

                break;
            case CommandType.ReceiveMessage:
                if (msg.TryParseData(out ChatMessage? cM))
                {
                    ChatMenu.LoadMessage(cM);
                }

                break;
            case CommandType.LoadMessage:
                if (msg.TryParseData(out ChatMessage[]? allMessages))
                {
                    ChatMenu.LoadAllMessage(allMessages);
                }
                else
                {
                    Console.WriteLine("Load All Messages Failed");
                }

                break;
            case CommandType.None:
            case CommandType.SendMessage:
            case CommandType.GetClientRsaKey:
            default:
                Console.WriteLine("Unknown Command");
                break;
        }

        return Task.CompletedTask;
    }

    public void OnClientConnected<T>(T? client) where T : class
    {
    }
}