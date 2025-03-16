using System.Net;
using System.Net.Sockets;
using System.Text;
using DesServer.AppSetting;
using DesServer.Controllers;
using DesServer.Database.Repositories;
using DesServer.Models;
using DesServer.Services;
using Shared.Models;
using Shared.Networking.Interfaces;
using Shared.Services;
using Shared.Utils;

namespace DesServer.Networking.Protocols.Tcp;

public class TcpHandler : INetworkHandler
{
    private readonly Dictionary<string, TcpClient?> _clients = new Dictionary<string, TcpClient?>();

    public void OnDataReceived(byte[] data, string sourceEndpoint)
    {
        if (_clients.TryGetValue(sourceEndpoint, out var client))
        {
            var message = ByteUtils.GetStringFromBytes(data);
            Logs.Logger.Log($"TCP Received from {sourceEndpoint}: {message}");
            HandleClientComm(client, message);
        }
    }

    public void OnDataReceived(string message, string sourceEndpoint)
    {
        if (_clients.TryGetValue(sourceEndpoint, out var client))
        {
            Logs.Logger.Log($"TCP Received from {sourceEndpoint}: {message}");
            HandleClientComm(client, message);
        }
    }

    public void OnClientConnected<T>(T? client) where T : class
    {
        if (client is not TcpClient c) return;
        var endpoint = c?.Client.RemoteEndPoint?.ToString();
        if (endpoint != null) _clients[endpoint] = c;
    }


    private void HandleClientComm(TcpClient? client, string jsonMessage)
    {
        try
        {
            var message = MessageNetwork<dynamic>.FromJson(jsonMessage);
            if (message == null)
            {
                MsgService.SendErrorMessage(client, "Invalid message format", StatusCode.Error,
                    ServerConfig.ShowConsoleLog);
                return;
            }

            switch (message.Type)
            {
                case CommandType.Login:
                    HandleLogin(client, message);
                    break;
                case CommandType.GetAvailableClients:
                    HandleGetAvailableClient(client, message);
                    break;
                case CommandType.GetClientRsaKey:
                    HandleGetClientRsaKey(client, message);
                    break;
                // case CommandType.Authentication:
                //     HandleAuthentication(client, message);
                //     break;
                // case CommandType.Registration:
                //     HandleRegistration(client, message);
                //     break;
                // case CommandType.GetAvailableClients:
                //     HandleGetAllUsers(client);
                //     break;
                // case CommandType.LoadMessage:
                //     _ = HandleLoadMessage(client, message);
                //     break;
                case CommandType.SendMessage:
                    _ = HandleSendMessage(client, message);
                    break;
                default:
                    MsgService.SendErrorMessage(client, "Unsupported action type", StatusCode.Error,
                        ServerConfig.ShowConsoleLog);
                    break;
            }
        }
        catch (Exception ex)
        {
            MsgService.SendErrorMessage(client, $"Server error: {ex.Message}", StatusCode.Error,
                ServerConfig.ShowConsoleLog);
        }
    }

    private void HandleGetClientRsaKey(TcpClient? client, MessageNetwork<dynamic> message)
    {
        try
        {
            if (message is { Code: StatusCode.Success } && client != null)
            {
                if (message.TryParseData(out ClientInfo? info) && info != null)
                {
                    var data = ClientKeyStore.Instance.GetClientById(info.Id)?.PublicKey;

                    if (data == null)
                    {
                        throw new InvalidOperationException("PublicKey not found for client.");
                    }

                    var response = new MessageNetwork<object>(
                        type: CommandType.GetClientRsaKey,
                        code: StatusCode.Success,
                        data: data
                    ).ToJson();

                    // _ = MsgService.SendTcpMessage(
                    //     ConnectionController.Instance.GetUserConnection(chatConversation.ReceiverId)?.TcpClient,
                    //     response, ServerConfig.ShowConsoleLog);
                }
                else
                {
                    throw new KeyNotFoundException("Target client not found.");
                }
            }
            else
            {
                throw new ArgumentException("Invalid message or client is null.");
            }
        }
        catch (Exception ex)
        {
            MsgService.SendErrorMessage(client, $"An error occurred: {ex.Message}", StatusCode.Error,
                ServerConfig.ShowConsoleLog);

            Console.WriteLine($"Error: {ex.StackTrace}");
        }
    }


    private void HandleGetAvailableClient(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (message is { Code: StatusCode.Success } && client != null)
        {
            if (message.TryParseData(out ClientInfo? info) && info != null)
            {
                var data = ClientKeyStore.Instance.GetAllWithoutClient(info.Id);
                var response = new MessageNetwork<object>(
                    type: CommandType.GetAvailableClients,
                    code: StatusCode.Success,
                    data: data
                ).ToJson();

                //MsgService.SendTcpMessage(client, response);
                // _ = MsgService.SendTcpMessage(
                //     ConnectionController.Instance.GetUserConnection(chatConversation.ReceiverId)?.TcpClient,
                //     response, ServerConfig.ShowConsoleLog);
            }
            else
            {
                MsgService.SendErrorMessage(client, "Target not found", StatusCode.InvalidRequest,
                    ServerConfig.ShowConsoleLog);
            }
        }
        else
        {
            MsgService.SendErrorMessage(client, "Server Error", StatusCode.InvalidRequest,
                ServerConfig.ShowConsoleLog);
        }
    }

    private void HandleLogin(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (message is { Code: StatusCode.Success } && client != null)
        {
            if (message.TryParseData(out ClientInfo? info) && info != null)
            {
                ClientKeyStore.Instance.RegisterClient(info.Id, info.PublicKey);
                var data = ClientKeyStore.Instance.GetAllWithoutClient(info.Id);
                var response = new MessageNetwork<object>(
                    type: CommandType.Login,
                    code: StatusCode.Success,
                    data: data
                ).ToJson();

                //MsgService.SendTcpMessage(client, response);
                // _ = MsgService.SendTcpMessage(
                //     ConnectionController.Instance.GetUserConnection(chatConversation.ReceiverId)?.TcpClient,
                //     response, ServerConfig.ShowConsoleLog);
            }
            else
            {
                MsgService.SendErrorMessage(client, "Target not found", StatusCode.InvalidRequest,
                    ServerConfig.ShowConsoleLog);
            }
        }
        else
        {
            MsgService.SendErrorMessage(client, "Server Error", StatusCode.InvalidRequest,
                ServerConfig.ShowConsoleLog);
        }
    }

    private async Task HandleSendMessage(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (message is { Code: StatusCode.Success } && client != null)
        {
            if (message.TryParseData(out ChatConversation? chatConversation) && chatConversation != null)
            {
                //Save to database
                await MessageRepository.SaveChatMessage(chatConversation.SenderId, chatConversation.ReceiverId,
                    chatConversation.Messages[0]);

                var response = new MessageNetwork<object>(
                    type: CommandType.ReceiveMessage,
                    code: StatusCode.Success,
                    data: chatConversation
                ).ToJson();

                //MsgService.SendTcpMessage(client, response);
                _ = MsgService.SendTcpMessage(
                    ConnectionController.Instance.GetUserConnection(chatConversation.ReceiverId)?.TcpClient,
                    response, ServerConfig.ShowConsoleLog);
            }
            else
            {
                MsgService.SendErrorMessage(client, "Target not found", StatusCode.InvalidRequest,
                    ServerConfig.ShowConsoleLog);
            }
        }
        else
        {
            MsgService.SendErrorMessage(client, "Server Error", StatusCode.InvalidRequest,
                ServerConfig.ShowConsoleLog);
        }
    }

    private async Task HandleLoadMessage(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (message is { Code: StatusCode.Success } && client != null)
        {
            if (message.TryParseData(out ChatHistoryRequest? history) &&
                history is { SenderId: not null, ReceiverId: not null })
            {
                var allChatMessage = await MessageRepository.LoadChatMessages(history.SenderId, history.ReceiverId);

                var response = new MessageNetwork<object>(
                    type: CommandType.LoadMessage,
                    code: StatusCode.Success,
                    data: allChatMessage
                ).ToJson();

                _ = MsgService.SendTcpMessage(client, response, ServerConfig.ShowConsoleLog);
            }
            else
            {
                MsgService.SendErrorMessage(client, "Target not found", StatusCode.InvalidRequest,
                    ServerConfig.ShowConsoleLog);
            }
        }
        else
        {
            MsgService.SendErrorMessage(client, "Server Error", StatusCode.InvalidRequest,
                ServerConfig.ShowConsoleLog);
        }
    }

    private void HandleAuthentication(TcpClient? client, MessageNetwork<dynamic> messageNetwork)
    {
        if (messageNetwork is { Code: StatusCode.Success })
        {
            if (messageNetwork.TryParseData(out AuthData? authData) && authData != null)
            {
                var result = ClientSessionService.Instance.LoginUser(
                    authData.Username,
                    authData.Password,
                    out User? user
                );
                var response = new MessageNetwork<object?>(
                    type: CommandType.Authentication,
                    code: result.Item1 ? StatusCode.Success : StatusCode.Failed,
                    data: result.Item1 ? (object?)user : (object)result.Item2
                ).ToJson();

                if (result.Item1)
                {
                    var socket = client?.Client;
                    if (socket != null && user?.Id != null && client != null)
                    {
                        var remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
                        if (remoteEndPoint != null)
                        {
                            string ipAddress = remoteEndPoint.Address.ToString();
                            int port = remoteEndPoint.Port;
                            ConnectionController.Instance.AddClient(userId: user.Id,
                                client: new UserConnection(userId: user.Id, tcpClient: client, ipAddress: ipAddress,
                                    port: port, lastConnection: DateTime.Now));
                        }
                    }
                }

                _ = MsgService.SendTcpMessage(client, response, ServerConfig.ShowConsoleLog);
            }
            else
            {
                MsgService.SendErrorMessage(client, "Missing credentials", StatusCode.InvalidRequest,
                    ServerConfig.ShowConsoleLog);
            }
        }
        else
        {
            MsgService.SendErrorMessage(client, "Invalid authentication", StatusCode.InvalidRequest,
                ServerConfig.ShowConsoleLog);
        }
    }

    private void HandleRegistration(TcpClient? client, MessageNetwork<dynamic> messageNetwork)
    {
        if (messageNetwork is { Code: StatusCode.Success, Data: not null })
        {
            if (messageNetwork.TryParseData(out AuthData? authData) && authData != null)
            {
                var result = ClientSessionService.Instance.RegisterUser(
                    authData.Username,
                    authData.Password
                );
                var response = new MessageNetwork<object>(
                    type: CommandType.Registration,
                    code: result.Item1 ? StatusCode.Success : StatusCode.Failed,
                    data: result.Item1 ? (object)authData : (object)result.Item2
                ).ToJson();
                _ = MsgService.SendTcpMessage(client, response, ServerConfig.ShowConsoleLog);
            }
            else
            {
                MsgService.SendErrorMessage(client, "Missing credentials", StatusCode.InvalidRequest,
                    ServerConfig.ShowConsoleLog);
            }
        }
        else
        {
            MsgService.SendErrorMessage(client, "Invalid registration", StatusCode.InvalidRequest,
                ServerConfig.ShowConsoleLog);
        }
    }

    private void HandleGetAllUsers(TcpClient? client)
    {
        if (client == null) return;
        var allUsers = UserRepository.GetAllUsers();
        var msg = new MessageNetwork<List<User>>(type: CommandType.GetAvailableClients, code: StatusCode.Success,
            data: allUsers);
        _ = MsgService.SendTcpMessage(client, msg.ToJson(), ServerConfig.ShowConsoleLog);
    }
}