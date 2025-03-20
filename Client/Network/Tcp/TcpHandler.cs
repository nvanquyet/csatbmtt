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
                    if (user.Id != null)
                    {
                        SessionManager.SetUser(user);
                        //Show Home Form and Dialog Login Success
                        MessageBox.Show("Login Success.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        FormController.ShowDialog(FormType.Home);
                    }
                    else
                    {
                        AuthService.Logout();
                        FormController.ShowDialog(FormType.Login);
                    }
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
            case CommandType.GetAvailableClients:
                if (msg.TryParseData(out List<UserDto>? allUsers) && allUsers != null)
                {
                    //ChatMenu.ChatWith(allUsers, NetworkManager.Instance.TcpService);
                    FormController.GetForm<HomeForm>(FormType.Home)?.SetAllUsers(allUsers);
                }
                break;
            case CommandType.HandshakeRequest:
                if (msg.TryParseData(out HandshakeDto? dtoRequest) && dtoRequest != null)
                {
                    if (msg.Code == StatusCode.Success)
                    {
                        FormController.GetForm<HomeForm>(FormType.Home)?.ShowHandshakeConfirm(dtoRequest);
                    }
                    else
                    {
                        FormController.GetForm<HomeForm>(FormType.Home)?.ShowHandshakeError(dtoRequest.Description);
                    }
                }
                break;
            case CommandType.HandshakeResponse:
                if (msg.TryParseData(out HandshakeDto? dtoResponse) && dtoResponse != null)
                {
                    if (msg.Code == StatusCode.Success)
                    {
                        if (dtoResponse.Accepted)
                        {
                            FormController.GetForm<HomeForm>(FormType.Home)?.HandShakeSuccess(dtoResponse.Description);
                            FormController.ShowDialog(FormType.Chat);
                            FormController.GetForm<ChatForm>(FormType.Chat)?.SetUserTarget(dtoResponse.FromUser?.Id == SessionManager.GetUserId() ? dtoResponse.ToUser : dtoResponse.FromUser);
                        }
                        else
                        {
                            FormController.GetForm<HomeForm>(FormType.Home)?.ShowHandshakeError(
                                dtoResponse.FromUser?.Id == SessionManager.GetUserId()
                                    ? $"{dtoResponse.ToUser?.UserName} rejected handshake request."
                                    : $"You rejected handshake request.");
                        }
                        
                    } else
                    {
                        FormController.GetForm<HomeForm>(FormType.Home)?.ShowHandshakeError(dtoResponse.Description);
                    }
                }
                break;
            case CommandType.GetUserShake:
                if (msg.TryParseData(out ConversationRecord? c) && c != null)
                {
                    FormController.GetForm<HomeForm>(FormType.Home)?.LoadHandshake(c);
                }
                break;
            case CommandType.CancelHandshake:
                if (msg.TryParseData(out HandshakeDto? cancel) && cancel != null)
                {
                    if (msg.Code == StatusCode.Success)
                    {
                        MessageBox.Show("Handshake Cancelled", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        FormController.ShowDialog(FormType.Home);
                    }
                    else
                    {
                        MessageBox.Show("Handshake Cancelled Try Again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                break;
            case CommandType.ReceiveMessage:
                if (msg.TryParseData(out MessageDto? md) && md?.Data != null)
                {
                    FormController.GetForm<ChatForm>(FormType.Chat)?.AddMessage(md.Data, false);
                }
            
                break;
            // case CommandType.LoadMessage:
            //     if (msg.TryParseData(out ChatMessage[]? allMessages))
            //     {
            //         ChatMenu.LoadAllMessage(allMessages);
            //     }
            //     else
            //     {
            //         Console.WriteLine("Load All Messages Failed");
            //     }
            //
            //     break;
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