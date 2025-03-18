using System.Net.Sockets;
using Server.Controllers;
using Server.Database.Repositories;
using Server.Services;
using Shared.Models;
using Shared.Networking.Interfaces;
using Shared.Utils;

namespace Server.Networking.Protocols.Tcp;

public class TcpHandler : INetworkHandler
{
    private static readonly Dictionary<string, TcpClient?> Clients = new();

    public void OnDataReceived(byte[] data, string sourceEndpoint)
    {
        if (!Clients.TryGetValue(sourceEndpoint, out var client)) return;
        var message = ByteUtils.GetStringFromBytes(data);
        Logs.Logger.Log($"TCP Received from {sourceEndpoint}: {message}");
        HandleClientComm(client, message);
    }

    public void OnDataReceived(string message, string sourceEndpoint)
    {
        if (!Clients.TryGetValue(sourceEndpoint, out var client)) return;
        Logs.Logger.Log($"TCP Received from {sourceEndpoint}: {message}");
        HandleClientComm(client, message);
    }

    public void OnClientConnected<T>(string? userId, T? client) where T : class
    {
        if (client is not TcpClient c) return;
        var endpoint = c?.Client.RemoteEndPoint?.ToString();
        if (endpoint != null) Clients[endpoint] = c;
    }

    public void OnClientConnected<T>(T? client) where T : class
    {
        if(client is not TcpClient c) return;
        var endpoint = c?.Client.RemoteEndPoint?.ToString();
        Console.WriteLine($"Connected to {endpoint}");
        if (endpoint != null) Clients[endpoint] = c;
    }


    private void HandleClientComm(TcpClient? client, string jsonMessage)
    {
        try
        {
            var message = MessageNetwork<dynamic>.FromJson(jsonMessage);
            if (message == null) throw new InvalidOperationException("Invalid message format");
            switch (message.Type)
            {
                case CommandType.Login:
                    HandleLogin(client, message);
                    break;
                case CommandType.Registration:
                    HandleRegistration(client, message);
                    break;
                case CommandType.GetAvailableClients:
                    HandleGetAvailableClient(client, message);
                    break;
                case CommandType.GetClientRsaKey:
                    HandleGetClientRsaKey(client, message);
                    break;
                case CommandType.LoadMessage:
                    _ = HandleLoadMessage(client, message);
                    break;
                case CommandType.SendMessage:
                    _ = HandleSendMessage(client, message);
                    break;
                case CommandType.RegisterClientRsaKey:
                    _ = HandleRegisterClientKey(client, message);
                    break;
                // case CommandType.ChatRequest:
                //     _ = HandleChatRequest(client, message);
                //     break;
                // case CommandType.ChatResponse:
                //     _ = HandleChatResponse(client, message);
                //     break;
                case CommandType.None:
                case CommandType.ReceiveMessage:
                default:
                    throw new InvalidOperationException("Unsupported action type");
            }
        }
        catch (Exception ex)
        {
            MsgService.SendErrorMessage(client, $"Server error: {ex.Message}");
        }
    }

    // private static Task HandleChatResponse(TcpClient? client, MessageNetwork<dynamic> message)
    // {
    //     try
    //     {
    //         if (message is { Code: StatusCode.Success } && client != null)
    //         {
    //             if (message.TryParseData(out ChatResponseDto? dto) && dto != null)
    //             {
    //                 if (string.IsNullOrEmpty(dto.ToUser?.Id)) throw new InvalidOperationException("Invalid user id");
    //                 if (!Clients.TryGetValue(dto.ToUser.Id, out var toClient))
    //                     throw new InvalidOperationException("Invalid user id");
    //                 MsgService.SendTcpMessage(toClient, message.ToJson());
    //             }
    //             else throw new KeyNotFoundException("Target client not found.");
    //         }
    //         else throw new ArgumentException("Invalid message or client is null.");
    //     }
    //     catch (Exception ex)
    //     {
    //         MsgService.SendErrorMessage(client, $"An error occurred: {ex.Message}");
    //         Console.WriteLine($"Error: {ex.StackTrace}");
    //     }
    //
    //     return Task.CompletedTask;
    // }

    // private static Task HandleChatRequest(TcpClient? client, MessageNetwork<dynamic> message)
    // {
    //     try
    //     {
    //         if (message is { Code: StatusCode.Success } && client != null)
    //         {
    //             if (message.TryParseData(out ChatRequestDto? dto) && dto != null)
    //             {
    //                 if (string.IsNullOrEmpty(dto.ToUser?.Id)) throw new InvalidOperationException("Invalid user id");
    //                 //Get TcpClient from target
    //                 if (!Clients.TryGetValue(dto.ToUser.Id, out var toClient))
    //                     throw new InvalidOperationException("Invalid user id");
    //                 MsgService.SendTcpMessage(toClient, message.ToJson());
    //             }
    //             else throw new KeyNotFoundException("Target client not found.");
    //         }
    //         else throw new ArgumentException("Invalid message or client is null.");
    //     }
    //     catch (Exception ex)
    //     {
    //         MsgService.SendErrorMessage(client, $"An error occurred: {ex.Message}");
    //         Console.WriteLine($"Error: {ex.StackTrace}");
    //     }
    //
    //     return Task.CompletedTask;
    // }

    private static Task HandleRegisterClientKey(TcpClient? client, MessageNetwork<dynamic> message)
    {
        try
        {
            if (message is { Code: StatusCode.Success } && client != null)
            {
                if (message.TryParseData(out ClientInfo? info) && info != null)
                {
                    if (ClientKeyStore.Instance.RegisterClient(info.Id, info.PublicKey))
                    {
                        var response = new MessageNetwork<string>(
                            type: CommandType.RegisterClientRsaKey,
                            code: StatusCode.Success,
                            data: "Register Public Key Success"
                        ).ToJson();
                        MsgService.SendTcpMessage(client, response);
                    }
                    else throw new InvalidOperationException("PublicKey not found for client.");
                }
                else throw new KeyNotFoundException("Target client not found.");
            }
            else throw new ArgumentException("Invalid message or client is null.");
        }
        catch (Exception ex)
        {
            MsgService.SendErrorMessage(client, $"An error occurred: {ex.Message}");
            Console.WriteLine($"Error: {ex.StackTrace}");
        }

        return Task.CompletedTask;
    }

    private static void HandleGetClientRsaKey(TcpClient? client, MessageNetwork<dynamic> message)
    {
        try
        {
            if (message is { Code: StatusCode.Success } && client != null)
            {
                if (message.TryParseData(out ClientInfo? info) && info != null)
                {
                    var data = ClientKeyStore.Instance.GetClientById(info.Id)?.PublicKey;

                    if (data == null) throw new InvalidOperationException("PublicKey not found for client.");

                    var response = new MessageNetwork<object>(
                        type: CommandType.GetClientRsaKey,
                        code: StatusCode.Success,
                        data: data
                    ).ToJson();
                    MsgService.SendTcpMessage(client, response);
                }
                else throw new KeyNotFoundException("Target client not found.");
            }
            else throw new ArgumentException("Invalid message or client is null.");
        }
        catch (Exception ex)
        {
            MsgService.SendErrorMessage(client, $"An error occurred: {ex.Message}");
            Console.WriteLine($"Error: {ex.StackTrace}");
        }
    }

    private static void HandleGetAvailableClient(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (message is { Code: StatusCode.Success } && client != null)
        {
            if (message.TryParseData(out string? userId) && userId != null)
            {
                var data = UserRepository.GetAllUsers().Select(u =>
                        u is { Id: not null, UserName: not null } ? new UserDto(id: u.Id, userName: u.UserName) : null)
                    .ToList();
                //filter List without userId
                data = data.Where(us => us?.Id != userId).ToList();
                Console.WriteLine($"Found {data.Count} users.");
                var response = new MessageNetwork<List<UserDto?>>(
                    type: CommandType.GetAvailableClients,
                    code: StatusCode.Success,
                    data: data
                ).ToJson();
                MsgService.SendTcpMessage(client, response);
                // _ = MsgService.SendTcpMessage(
                //     ConnectionController.Instance.GetUserConnection(chatConversation.ReceiverId)?.TcpClient,
                //     response, ServerConfig.ShowConsoleLog);
            }
            else
            {
                MsgService.SendErrorMessage(client, "Target not found");
            }
        }
        else
        {
            MsgService.SendErrorMessage(client, "Server Error");
        }
    }

    private void HandleLogin(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (message is { Code: StatusCode.Success } && client != null)
        {
            if (message.TryParseData(out User? u) && u != null)
            {
                var lR = ClientSessionService.Instance.LoginUser(u.UserName, u.Password, out var user);
                if (lR.Item1)
                {
                    if (user != null)
                    {
                        OnClientConnected(user.Id, client);

                        var response = new MessageNetwork<object>(
                            type: CommandType.Login,
                            code: StatusCode.Success,
                            data: user
                        ).ToJson();
                        MsgService.SendTcpMessage(client, response);
                    }
                    else MsgService.SendErrorMessage(client, "Login failed");
                }
                else MsgService.SendErrorMessage(client, lR.Item2);
            }
            else MsgService.SendErrorMessage(client, "Target not found");
        }
        else MsgService.SendErrorMessage(client, "Server Error");
    }

    private static void HandleRegistration(TcpClient? client, MessageNetwork<dynamic> messageNetwork)
    {
        if (messageNetwork is { Code: StatusCode.Success, Data: not null })
        {
            if (messageNetwork.TryParseData(out User? user) && user != null)
            {
                var result = ClientSessionService.Instance.RegisterUser(
                    user.UserName,
                    user.Password
                );
                if (result.Item1)
                {
                    var response = new MessageNetwork<object>(
                        type: CommandType.Registration,
                        code: StatusCode.Success,
                        data: user
                    ).ToJson();
                    MsgService.SendTcpMessage(client, response);
                }
                else MsgService.SendErrorMessage(client, $"Registration failed {result.Item2}");
            }
            else MsgService.SendErrorMessage(client, "Missing credentials");
        }
        else MsgService.SendErrorMessage(client, "Invalid registration");
    }

    private static async Task HandleSendMessage(TcpClient? client, MessageNetwork<dynamic> message)
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
                //MsgService.SendTcpMessage(
                // ConnectionController.Instance.GetUserConnection(chatConversation.ReceiverId)?.TcpClient);
            }
            else MsgService.SendErrorMessage(client, "Target not found");
        }
        else MsgService.SendErrorMessage(client, "Server Error");
    }

    private static async Task HandleLoadMessage(TcpClient? client, MessageNetwork<dynamic> message)
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

                MsgService.SendTcpMessage(client, response);
            }
            else MsgService.SendErrorMessage(client, "Target not found");
        }
        else MsgService.SendErrorMessage(client, "Server Error");
    }
}