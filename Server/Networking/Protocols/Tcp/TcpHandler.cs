using System.Net.Sockets;
using System.Collections.Concurrent;
using Server.Database.Repositories;
using Server.Services;
using Shared.Models;
using Shared.Networking.Interfaces;
using Shared.Services;
using Shared.Utils;

namespace Server.Networking.Protocols.Tcp;

public class TcpHandler : INetworkHandler, IDisposable
{
    private static readonly ConcurrentDictionary<string, TcpClient?> Clients = new();
    private static readonly ConcurrentDictionary<string, string?> MapUserIdToIp = new();

    private static void MappingIdToIp(string? ip, string? userId)
    {
        if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(userId)) return;
        if (!MapUserIdToIp.TryAdd(ip, userId)) MapUserIdToIp[userId] = ip;
        foreach (var (key, value) in MapUserIdToIp)
        {
            Console.WriteLine($"Connect {key} to {value} ");
        }
    }

    private static bool UserIsOnline(string userId) => MapUserIdToIp.ContainsKey(userId);

    public void OnDataReceived(byte[] data, string sourceEndpoint)
    {
        if (!Clients.TryGetValue(sourceEndpoint, out var client)) return;
        var message = ByteUtils.GetStringFromBytes(data);
        _ = HandleClientComm(client, message);
    }

    public void OnDataReceived(string message, string sourceEndpoint)
    {
        if (!Clients.TryGetValue(sourceEndpoint, out var client)) return;
        _ = HandleClientComm(client, message);
    }

    public void OnClientDisconnect<T>(T? client) where T : class
    {
        if (client is not TcpClient c) return;
        var endPoint = c?.Client.RemoteEndPoint?.ToString();
        if (endPoint == null) return;
        if (Clients.TryRemove(endPoint, out var cl))
        {
            cl?.Dispose();
        }

        foreach (var (key, value) in MapUserIdToIp)
        {
            if(value == null) continue;
            if (value != endPoint) continue;
            MapUserIdToIp.TryRemove(endPoint, out _);
        }
    }

    public void OnClientConnected<T>(T? client) where T : class
    {
        if (client is not TcpClient c) return;
        var endpoint = c?.Client.RemoteEndPoint?.ToString();
        if (endpoint != null) Clients.TryAdd(endpoint, c);
    }

    private async Task HandleClientComm(TcpClient? client, string jsonMessage)
    {
        if (client == null) return;

        try
        {
            Console.WriteLine($"Message receive from {client.Client.RemoteEndPoint}: {jsonMessage}");
            var message = MessageNetwork<dynamic>.FromJson(jsonMessage);
            if (message == null) throw new InvalidOperationException("Invalid message format");

            switch (message.Type)
            {
                case CommandType.Login:
                    await HandleLogin(client, message);
                    break;
                case CommandType.Registration:
                    await HandleRegistration(client, message);
                    break;
                case CommandType.GetAvailableClients:
                    await HandleGetAvailableClient(client, message);
                    break;
                // case CommandType.GetClientRsaKey:
                //     await HandleGetClientRsaKey(client, message);
                //     break;
                // case CommandType.LoadMessage:
                //     await HandleLoadMessage(client, message);
                //     break;
                case CommandType.SendMessage:
                    await HandleSendMessage(client, message);
                    break;
                // case CommandType.RegisterClientRsaKey:
                //     await HandleRegisterClientKey(client, message);
                //     break;
                case CommandType.ClientDisconnect:
                    OnClientDisconnect(client);
                    break;
                case CommandType.HandshakeRequest:
                    await HandleHandshakeRequest(client, message);
                    break;
                case CommandType.HandshakeResponse:
                    await HandleHandshakeResponse(client, message);
                    break;
                case CommandType.GetUserShake:
                    await HandleHandGetUserShake(client, message);
                    break;
                case CommandType.CancelHandshake:
                    await HandleCancelHandShake(client, message);
                    break;
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

    public static Task HandleClientDisconnect(TcpClient? client, MessageNetwork<dynamic> message)
    {
        var endPoint = client?.Client.RemoteEndPoint?.ToString();
        if (endPoint != null && Clients.TryRemove(endPoint, out var c))
        {
            c?.Dispose();
        }

        //Remove

        return Task.CompletedTask;
    }

    #region HandShake

    private static async Task HandleHandGetUserShake(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (message is { Code: StatusCode.Success } && client != null)
        {
            if (message.TryParseData(out UserDto? user) && user != null)
            {
                if (user.Id == null) return;
                var data = await UserInteractionRepository.GetConversationRecord(user.Id);
                var response = new MessageNetwork<ConversationRecord?>(
                    type: CommandType.GetUserShake,
                    code: StatusCode.Success,
                    data: data
                ).ToJson();
                MsgService.SendTcpMessage(client, response);
            }
            else MsgService.SendErrorMessage(client, "Target not found");
        }
        else MsgService.SendErrorMessage(client, "Server Error");
    }

    private static Task HandleGetAvailableClient(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (message is { Code: StatusCode.Success } && client != null)
        {
            if (message.TryParseData(out UserDto? user) && user != null)
            {
                var data = UserRepository.GetAllUsers().Select(u =>
                        u is { Id: not null, UserName: not null } ? new UserDto(id: u.Id, userName: u.UserName) : null)
                    .ToList();
                //filter List without userId
                data = data.Where(us => us?.Id != user.Id).ToList();
                Console.WriteLine($"Found {data.Count} users.");
                var response = new MessageNetwork<List<UserDto?>>(
                    type: CommandType.GetAvailableClients,
                    code: StatusCode.Success,
                    data: data
                ).ToJson();
                MsgService.SendTcpMessage(client, response);
            }
            else MsgService.SendErrorMessage(client, "Target not found");
        }
        else MsgService.SendErrorMessage(client, "Server Error");

        return Task.CompletedTask;
    }

    private static Task HandleHandshakeResponse(TcpClient? client, MessageNetwork<dynamic> message)
    {
        try
        {
            if (message is { Code: StatusCode.Success } && client != null)
            {
                if (message.TryParseData(out HandshakeDto? dto) && dto != null)
                {
                    if (string.IsNullOrEmpty(dto.FromUser?.Id)) throw new InvalidOperationException("Invalid user id");
                    //Get TcpClient from target
                    if (!MapUserIdToIp.TryGetValue(dto.FromUser.Id, out var fromUserId) || fromUserId == null)
                    {
                        message.Code = StatusCode.Error;
                        dto.Description = "Target client not online.";
                        message.Data = dto;
                        MsgService.SendTcpMessage(client, message.ToJson());
                    }
                    else
                    {
                        if (!Clients.TryGetValue(fromUserId, out var toClient))
                        {
                            //Todo: Send error handshake cant get ip target
                            message.Code = StatusCode.Error;
                            dto.Description = "Can't connect to target client.";
                            message.Data = dto;
                            MsgService.SendTcpMessage(client, message.ToJson());
                        }
                        else
                        {
                            MsgService.SendTcpMessage(toClient, message.ToJson());
                            MsgService.SendTcpMessage(toClient, message.ToJson());
                        }
                    }
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

    private static Task HandleHandshakeRequest(TcpClient? client, MessageNetwork<dynamic> message)
    {
        try
        {
            if (message is { Code: StatusCode.Success } && client != null)
            {
                if (message.TryParseData(out HandshakeDto? dto) && dto != null)
                {
                    if (string.IsNullOrEmpty(dto.ToUser?.Id)) throw new InvalidOperationException("Invalid user id");
                    //Get TcpClient from target
                    if (!MapUserIdToIp.TryGetValue(dto.ToUser.Id, out var ip) || ip == null)
                    {
                        Console.WriteLine($"Ip: {ip}");
                        message.Code = StatusCode.Error;
                        dto.Description = "Target client not online.";
                        message.Data = dto;
                        MsgService.SendTcpMessage(client, message.ToJson());
                    }
                    else
                    {
                        Console.WriteLine($"Ip: {ip}");
                        if (!Clients.TryGetValue(ip, out var toClient))
                        {
                            //Todo: Send error handshake cant get ip target
                            message.Code = StatusCode.Error;
                            dto.Description = "Can't connect to target client.";
                            message.Data = dto;
                            MsgService.SendTcpMessage(client, message.ToJson());
                        }
                        else
                        {
                            MsgService.SendTcpMessage(toClient, message.ToJson());
                        }
                    }
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
    
    private static Task HandleCancelHandShake(TcpClient? client, MessageNetwork<dynamic> message)
    {
        try
        {
            if (message is { Code: StatusCode.Success } && client != null)
            {
                if (message.TryParseData(out HandshakeDto? dto) && dto != null)
                {
                    if (string.IsNullOrEmpty(dto.ToUser?.Id)) throw new InvalidOperationException("Invalid user id");
                    //Get TcpClient from target
                    if (!MapUserIdToIp.TryGetValue(dto.ToUser.Id, out var toUserId) || toUserId == null)
                    {
                        message.Code = StatusCode.Error;
                        dto.Description = "Target client not online.";
                        message.Data = dto;
                        MsgService.SendTcpMessage(client, message.ToJson());
                    }
                    else
                    {
                        if (!Clients.TryGetValue(toUserId, out var toClient))
                        {
                            //Todo: Send error handshake cant get ip target
                            message.Code = StatusCode.Error;
                            dto.Description = "Can't connect to target client.";
                            message.Data = dto;
                            MsgService.SendTcpMessage(client, message.ToJson());
                        }
                        else
                        {
                            MsgService.SendTcpMessage(toClient, message.ToJson());
                            MsgService.SendTcpMessage(client, message.ToJson());
                        }
                    }
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

    #endregion

    #region Message

    private static Task HandleSendMessage(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (message is { Code: StatusCode.Success } && client != null)
        {
            if (message.TryParseData(out MessageDto? messageDto) && messageDto is { ReceiverId: not null })
            {
                if (!MapUserIdToIp.TryGetValue(messageDto.ReceiverId, out var r) || r == null)
                    return Task.CompletedTask;
                if (!Clients.TryGetValue(r, out var toClient)) return Task.CompletedTask;
                message.Type = CommandType.ReceiveMessage;
                MsgService.SendTcpMessage(toClient, message.ToJson());
            }
            else MsgService.SendErrorMessage(client, "Target not found");
        }
        else MsgService.SendErrorMessage(client, "Server Error");

        return Task.CompletedTask;
    }

    // private static async Task HandleLoadMessage(TcpClient? client, MessageNetwork<dynamic> message)
    // {
    //     if (message is { Code: StatusCode.Success } && client != null)
    //     {
    //         if (message.TryParseData(out ChatHistoryRequest? history) &&
    //             history is { SenderId: not null, ReceiverId: not null })
    //         {
    //             var allChatMessage = await MessageRepository.LoadChatMessages(history.SenderId, history.ReceiverId);
    //
    //             var response = new MessageNetwork<object>(
    //                 type: CommandType.LoadMessage,
    //                 code: StatusCode.Success,
    //                 data: allChatMessage
    //             ).ToJson();
    //             MsgService.SendTcpMessage(client, response);
    //         }
    //         else MsgService.SendErrorMessage(client, "Target not found");
    //     }
    //     else MsgService.SendErrorMessage(client, "Server Error");
    // }
    

    #endregion

    #region Client Key

    // private static Task HandleRegisterClientKey(TcpClient? client, MessageNetwork<dynamic> message)
    // {
    //     try
    //     {
    //         if (message is { Code: StatusCode.Success } && client != null)
    //         {
    //             if (message.TryParseData(out ClientInfo? info) && info != null)
    //             {
    //                 if (ClientKeyStore.Instance.RegisterClient(info.Id, info.PublicKey))
    //                 {
    //                     var response = new MessageNetwork<string>(
    //                         type: CommandType.RegisterClientRsaKey,
    //                         code: StatusCode.Success,
    //                         data: "Register Public Key Success"
    //                     ).ToJson();
    //                     MsgService.SendTcpMessage(client, response);
    //                 }
    //                 else throw new InvalidOperationException("PublicKey not found for client.");
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
    //
    // private static Task HandleGetClientRsaKey(TcpClient? client, MessageNetwork<dynamic> message)
    // {
    //     try
    //     {
    //         if (message is { Code: StatusCode.Success } && client != null)
    //         {
    //             if (message.TryParseData(out ClientInfo? info) && info != null)
    //             {
    //                 var data = ClientKeyStore.Instance.GetClientById(info.Id)?.PublicKey;
    //
    //                 if (data == null) throw new InvalidOperationException("PublicKey not found for client.");
    //
    //                 var response = new MessageNetwork<object>(
    //                     type: CommandType.GetClientRsaKey,
    //                     code: StatusCode.Success,
    //                     data: data
    //                 ).ToJson();
    //                 MsgService.SendTcpMessage(client, response);
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

    #endregion

    #region Login and Registration

    private Task HandleLogin(TcpClient? client, MessageNetwork<dynamic> message)
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
                        var response = new MessageNetwork<object>(
                            type: CommandType.Login,
                            code: StatusCode.Success,
                            data: user
                        ).ToJson();
                        //Add to map
                        MappingIdToIp(client.GetStream().Socket.RemoteEndPoint?.ToString(), user.Id);
                        MsgService.SendTcpMessage(client, response);
                    }
                    else MsgService.SendErrorMessage(client, "Login failed");
                }
                else MsgService.SendErrorMessage(client, lR.Item2);
            }
            else MsgService.SendErrorMessage(client, "Target not found");
        }
        else MsgService.SendErrorMessage(client, "Server Error");

        return Task.CompletedTask;
    }

    private static Task HandleRegistration(TcpClient? client, MessageNetwork<dynamic> messageNetwork)
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

        return Task.CompletedTask;
    }

    #endregion

    public void Dispose()
    {
        foreach (var client in Clients.Values)
        {
            try
            {
                client?.Dispose();
            }
            catch (Exception ex)
            {
                MsgService.SendErrorMessage(client, $"Error disposing client {ex.Message}");
            }
        }

        Clients.Clear();
        MapUserIdToIp.Clear();
    }
}