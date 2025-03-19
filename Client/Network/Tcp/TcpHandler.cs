using Client.Form;
using Client.Menu;
using Client.Models;
using Client.Services;
using Shared.Models;
using Shared.Networking.Interfaces;
using Shared.Security.Interface;
using Shared.Services;
using Shared.Utils;

namespace Client.Network.Tcp;

public class TcpHandler : INetworkHandler
{
    public void OnDataReceived(byte[] data, string sourceEndpoint)
    {
        var message = ByteUtils.GetStringFromBytes(data);
        OnDataReceived(message, sourceEndpoint);
    }

    public void OnDataReceived(string message, string sourceEndpoint) => HandleMessage(message);
    public void OnClientConnected<T>(string id, T? client) where T : class { }


    private static Task HandleMessage(string message)
    {
        var msg = MessageNetwork<dynamic>.FromJson(message);
        //if (msg?.Code != StatusCode.Success) return Task.CompletedTask;
        switch (msg?.Type)
        {
            case CommandType.Login:
                if (msg.Code == StatusCode.Success && msg.TryParseData(out User? user) && user != null)
                {
                    if((bool)(FormController.GetForm(FormType.Login) as LoginForm)?.RememberMe) AuthService.SaveUserInfo(user);
                
                    //Todo: Register Rsa public key
                    if (user.Id != null)
                    {
                        SessionManager.SetUser(user);
                        var clientInfo = new ClientInfo(id: user.Id, EncryptionService.Instance
                            .GetAlgorithm(EncryptionType.Rsa)
                            .EncryptKey);
                        var response = new MessageNetwork<ClientInfo>(type: CommandType.RegisterClientRsaKey,
                            code: StatusCode.Success, data: clientInfo).ToJson();
                        //Send to server
                        NetworkManager.Instance.TcpService.Send(response, "");
                    }
                    //MainMenu.ShowMenu2(NetworkManager.Instance.TcpService);
                    //Show Home Form and Dialog Login Success
                    MessageBox.Show("Login Success.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    FormController.ShowDialog(FormType.Home);
                }
                else
                {
                    AuthService.Logout();
                    FormController.ShowDialog(FormType.Login);
                    MessageBox.Show("Login Failed Try Again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                break;
            case CommandType.Registration:
                if (msg.Code == StatusCode.Success)
                {
                    MessageBox.Show("Register Success Login to Continue", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    FormController.GetForm(FormType.Register)?.Close();
                }
                else
                {
                    MessageBox.Show("Register Failed Try Again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                break;
            case CommandType.RegisterClientRsaKey:
                Console.WriteLine("Register RSA public key Success");
                break;
            case CommandType.GetAvailableClients:
                if (msg.TryParseData(out List<UserDto>? allUsers) && allUsers != null)
                {
                    ChatMenu.ChatWith(allUsers, NetworkManager.Instance.TcpService);
                }
                break;
            // case CommandType.ChatRequest:
            //     if (msg.TryParseData(out ChatRequestDto? dto) && dto != null)
            //     {
            //         ChatMenu.ShowBoxConfirm(dto);
            //     }
            //     break;
            // case CommandType.ChatResponse:
            //     if (msg.TryParseData(out ChatResponseDto? r) && r != null)
            //     {
            //         Console.WriteLine($"User {r.FromUser?.UserName} {(r.Accepted ? "Accepted" : "Rejected")}");
            //         if (r.Accepted) ChatMenu.ShowChatMenu(r.FromUser, NetworkManager.Instance.TcpService, false);
            //     }
            //
            //     break;
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