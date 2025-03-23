using System.Net.Sockets;
using System.Collections.Concurrent;
using Server.Database.Repositories;
using Server.Services;
using Shared;
using Shared.Models;
using Shared.Networking.Interfaces;
using Shared.Services;
using Shared.Utils;

namespace Server.Networking.Protocols.Tcp;

public class TcpHandler : INetworkHandler, IDisposable
{
    private static readonly ConcurrentDictionary<string, TcpClient?> Clients = new();
    private static readonly ConcurrentDictionary<string, string?> MapUserIdToIp = new();    

    #region Helpers

    private static void MappingIdToIp(string? ip, string? userId)
    {
        if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(userId))
            return;

        if (!MapUserIdToIp.TryAdd(userId, ip))
            MapUserIdToIp[userId] = ip;
    }

    private static bool UserIsOnline(string? userId) => userId != null && MapUserIdToIp.ContainsKey(userId);

    private static bool ValidateMessage(TcpClient? client, MessageNetwork<dynamic> message, out string error)
    {
        if (client == null)
        {
            error = "Client is null.";
            return false;
        }
        if (message.Code != StatusCode.Success)
        {
            error = "Server Error";
            return false;
        }
        error = string.Empty;
        return true;
    }

    private static void SendError(TcpClient? client, string errorMessage)
    {
        if (client != null)
            MsgService.SendErrorMessage(client, errorMessage);
    }

    #endregion

    #region INetworkHandler Implementation

    public void OnDataReceived(byte[] data, string sourceEndpoint)
    {
        if (!Clients.TryGetValue(sourceEndpoint, out var client))
            return;
        var message = ByteUtils.GetStringFromBytes(data);
        _ = HandleClientComm(client, message);
    }

    public void OnDataReceived(string message, string sourceEndpoint)
    {
        if (!Clients.TryGetValue(sourceEndpoint, out var client))
            return;
        _ = HandleClientComm(client, message);
    }

    private Task HandleClientDisconnect(TcpClient? client)
    {
        OnClientDisconnect(client);
        return Task.CompletedTask;
    }

    public void OnClientDisconnect<T>(T? client) where T : class
    {
        if (client is not TcpClient tcpClient)
            return;

        var endPoint = tcpClient.Client.RemoteEndPoint?.ToString();
        if (endPoint == null)
            return;

        if (Clients.TryRemove(endPoint, out var removedClient))
            removedClient?.Dispose();

        // Loại bỏ mapping theo endpoint
        foreach (var (key, value) in MapUserIdToIp)
        {
            if (value == endPoint)
                MapUserIdToIp.TryRemove(key, out _);
        }
    }

    public void OnClientConnected<T>(T? client) where T : class
    {
        if (client is not TcpClient tcpClient)
            return;

        var endpoint = tcpClient.Client.RemoteEndPoint?.ToString();
        if (endpoint != null)
            Clients.TryAdd(endpoint, tcpClient);
    }

    public void BroadcastMessage(string message)
    {
        var targetClients = Clients.Values.Where(c => c != null).OfType<TcpClient>().ToList();
        foreach (var client in targetClients)
        {
            MsgService.SendTcpMessage(client, message);
        }
    }

    public void BroadcastMessageExcept<T>(T? excludedClient, string message) where T : class
    {
        var targetClients = Clients.Values
                                   .Where(c => c != excludedClient && c != null)
                                   .OfType<TcpClient>()
                                   .ToList();
        foreach (var client in targetClients)
        {
            MsgService.SendTcpMessage(client, message);
        }
    }

    #endregion

    #region Client Communication

    private async Task HandleClientComm(TcpClient? client, string jsonMessage)
    {
        if (client == null)
            return;

        try
        {
            Logger.LogInfo($"Message received from {client.Client.RemoteEndPoint}: {jsonMessage}");
            var message = MessageNetwork<dynamic>.FromJson(jsonMessage);
            if (message == null)
                throw new InvalidOperationException("Invalid message format");
            await (message.Type switch
            {
                CommandType.Login => HandleLogin(client, message),
                CommandType.Logout => HandleLogout(client, message),
                CommandType.Registration => HandleRegistration(client, message),
                CommandType.GetAvailableClients => HandleGetAvailableClient(client, message),
                CommandType.SendMessage => HandleSendMessage(client, message),
                CommandType.HandshakeRequest => HandleHandshakeRequest(client, message),
                CommandType.HandshakeResponse => HandleHandshakeResponse(client, message),
                CommandType.GetUserShaked => HandleHandGetUserShaked(client, message),
                CommandType.CancelHandshake => HandleCancelHandShake(client, message),
                CommandType.ClientDisconnect => HandleClientDisconnect(client),
                _ => throw new InvalidOperationException("Unsupported command type")
            });
        }
        catch (Exception ex)
        {
            SendError(client, $"Server error: {ex.Message}");
        }
    }

    #endregion

    #region Handshake Handlers

    private static async Task HandleHandGetUserShaked(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (!ValidateMessage(client, message, out var error))
        {
            SendError(client, error);
            return;
        }

        if (message.TryParseData(out UserDto? user) && user != null && !string.IsNullOrEmpty(user.Id))
        {
            var data = await UserInteractionRepository.GetConversationRecord(user.Id);
            var response = new MessageNetwork<ConversationRecord?>(
                type: CommandType.GetUserShaked,
                code: StatusCode.Success,
                data: data
            ).ToJson();
            MsgService.SendTcpMessage(client, response);
        }
        else
        {
            SendError(client, "Target not found");
        }
    }

    private static Task HandleGetAvailableClient(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (!ValidateMessage(client, message, out var error))
        {
            SendError(client, error);
            return Task.CompletedTask;
        }

        if (message.TryParseData(out UserDto? user) && user != null)
        {
            var data = UserRepository.GetAllUsers()
                .Where(u => u.Id != null && u.UserName != null)
                .Select(u => new UserDto(id: u.Id, userName: u.UserName))
                .Where(us => us.Id != user.Id)
                .ToList();

            // Cập nhật trạng thái online
            foreach (var d in data)
            {
                d.IsOnline = UserIsOnline(d.Id);
            }

            var response = new MessageNetwork<List<UserDto?>>(
                type: CommandType.GetAvailableClients,
                code: StatusCode.Success,
                data: data
            ).ToJson();
            MsgService.SendTcpMessage(client, response);
        }
        else
        {
            SendError(client, "Target not found");
        }

        return Task.CompletedTask;
    }

    private static Task HandleHandshakeResponse(TcpClient? client, MessageNetwork<dynamic> message)
    {
        try
        {
            if (!ValidateMessage(client, message, out var error))
                throw new ArgumentException(error);

            if (message.TryParseData(out HandshakeDto? dto) && dto != null)
            {
                if (string.IsNullOrEmpty(dto.FromUser?.Id))
                    throw new InvalidOperationException("Invalid user id");

                if (!MapUserIdToIp.TryGetValue(dto.FromUser.Id, out var fromUserIp) || string.IsNullOrEmpty(fromUserIp))
                {
                    message.Code = StatusCode.Error;
                    dto.Description = "Target client not online.";
                    message.Data = dto;
                    MsgService.SendTcpMessage(client, message.ToJson());
                }
                else if (!Clients.TryGetValue(fromUserIp, out var toClient))
                {
                    message.Code = StatusCode.Error;
                    dto.Description = "Can't connect to target client.";
                    message.Data = dto;
                    MsgService.SendTcpMessage(client, message.ToJson());
                }
                else
                {
                    dto.Description = "Successfully connected to target client.";
                    message.Data = dto;
                    MsgService.SendTcpMessage(client, message.ToJson());
                    MsgService.SendTcpMessage(toClient, message.ToJson());
                }
            }
            else throw new KeyNotFoundException("Target client not found.");
        }
        catch (Exception ex)
        {
            SendError(client, $"An error occurred: {ex.Message}");
            Logger.LogError($"Error: {ex.StackTrace}");
        }

        return Task.CompletedTask;
    }

    private static bool IsUserAvailableForHandshake(string userId) => true;

    private static Task HandleHandshakeRequest(TcpClient? client, MessageNetwork<dynamic> message)
    {
        try
        {
            if (!ValidateMessage(client, message, out var error))
                throw new ArgumentException(error);

            if (message.TryParseData(out HandshakeDto? dto) && dto != null)
            {
                if (string.IsNullOrEmpty(dto.ToUser?.Id))
                    throw new InvalidOperationException("Invalid user id");

                if (!dto.ToUser.IsOnline ||
                    !MapUserIdToIp.TryGetValue(dto.ToUser.Id, out var ip) || string.IsNullOrEmpty(ip))
                {
                    message.Code = StatusCode.Error;
                    dto.Description = "Target client not online.";
                    message.Data = dto;
                    MsgService.SendTcpMessage(client, message.ToJson());
                }
                else if (!Clients.TryGetValue(ip, out var toClient) || !IsUserAvailableForHandshake(dto.ToUser.Id))
                {
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
            else throw new KeyNotFoundException("Target client not found.");
        }
        catch (Exception ex)
        {
            SendError(client, $"An error occurred: {ex.Message}");
            Logger.LogError($"Error: {ex.StackTrace}");
        }

        return Task.CompletedTask;
    }

    private static Task HandleCancelHandShake(TcpClient? client, MessageNetwork<dynamic> message)
    {
        try
        {
            if (!ValidateMessage(client, message, out var error))
                throw new ArgumentException(error);

            if (message.TryParseData(out HandshakeDto? dto) && dto != null)
            {
                if (string.IsNullOrEmpty(dto.ToUser?.Id))
                    throw new InvalidOperationException("Invalid user id");

                if (!dto.ToUser.IsOnline ||
                    !MapUserIdToIp.TryGetValue(dto.ToUser.Id, out var toUserIp) || string.IsNullOrEmpty(toUserIp))
                {
                    message.Code = StatusCode.Error;
                    dto.Description = "Target client not online.";
                    message.Data = dto;
                    MsgService.SendTcpMessage(client, message.ToJson());
                }
                else if (!Clients.TryGetValue(toUserIp, out var toClient) || !IsUserAvailableForHandshake(dto.ToUser.Id))
                {
                    message.Code = StatusCode.Error;
                    dto.Description = "Can't connect to target client.";
                    message.Data = dto;
                    MsgService.SendTcpMessage(client, message.ToJson());
                }
                else
                {
                    // Gửi thông báo hủy handshake cho cả hai client
                    MsgService.SendTcpMessage(toClient, message.ToJson());
                    MsgService.SendTcpMessage(client, message.ToJson());
                }
            }
            else throw new KeyNotFoundException("Target client not found.");
        }
        catch (Exception ex)
        {
            SendError(client, $"An error occurred: {ex.Message}");
            Logger.LogError($"Error: {ex.StackTrace}");
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Message Handlers

    private static Task HandleSendMessage(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (!ValidateMessage(client, message, out var error))
        {
            SendError(client, error);
            return Task.CompletedTask;
        }

        if (message.TryParseData(out MessageDto? messageDto) && messageDto is { ReceiverId: not null })
        {
            if (!MapUserIdToIp.TryGetValue(messageDto.ReceiverId, out var receiverIp) || string.IsNullOrEmpty(receiverIp))
                return Task.CompletedTask;
            if (!Clients.TryGetValue(receiverIp, out var toClient))
                return Task.CompletedTask;

            message.Type = CommandType.ReceiveMessage;
            MsgService.SendTcpMessage(toClient, message.ToJson());
        }
        else
        {
            SendError(client, "Target not found");
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Login and Registration Handlers

    private Task HandleLogin(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (!ValidateMessage(client, message, out var error))
        {
            SendError(client, error);
            return Task.CompletedTask;
        }

        if (message.TryParseData(out User? u) && u != null)
        {
            var (success, resultMsg) = ClientSessionService.LoginUser(u.UserName, u.Password, out var user);
            if (success && user != null)
            {
                var response = new MessageNetwork<object>(
                    type: CommandType.Login,
                    code: StatusCode.Success,
                    data: user
                ).ToJson();

                // Cập nhật mapping giữa endpoint và user.Id
                MappingIdToIp(client?.GetStream().Socket.RemoteEndPoint?.ToString(), user.Id);

                // Gửi phản hồi cho client đăng nhập
                MsgService.SendTcpMessage(client, response);

                // Broadcast thông báo cho các client khác
                var broadcastMessage = new MessageNetwork<object>(
                    type: CommandType.LoginBroadcast,
                    code: StatusCode.Success,
                    data: new UserDto(user.Id, user.UserName, true)
                ).ToJson();
                BroadcastMessageExcept(client, broadcastMessage);
            }
            else
            {
                SendError(client, resultMsg);
            }
        }
        else
        {
            SendError(client, "Target not found");
        }

        return Task.CompletedTask;
    }

    private Task HandleLogout(TcpClient? client, MessageNetwork<dynamic> message)
    {
        if (!ValidateMessage(client, message, out var error))
        {
            SendError(client, error);
            return Task.CompletedTask;
        }

        if (message.TryParseData(out UserDto? u) && u != null)
        {
            u.IsOnline = false;
            var broadcastMessage = new MessageNetwork<object>(
                type: CommandType.LogoutBroadcast,
                code: StatusCode.Success,
                data: u
            ).ToJson();
            BroadcastMessageExcept(client, broadcastMessage);
        }
        else
        {
            SendError(client, "Target not found");
        }

        return Task.CompletedTask;
    }

    private static Task HandleRegistration(TcpClient? client, MessageNetwork<dynamic> messageNetwork)
    {
        if (messageNetwork is { Code: StatusCode.Success, Data: not null })
        {
            if (messageNetwork.TryParseData(out User? user) && user != null)
            {
                var (success, resultMsg) = ClientSessionService.RegisterUser(user.UserName, user.Password);
                if (success)
                {
                    var response = new MessageNetwork<object>(
                        type: CommandType.Registration,
                        code: StatusCode.Success,
                        data: user
                    ).ToJson();
                    MsgService.SendTcpMessage(client, response);
                }
                else
                {
                    SendError(client, $"Registration failed: {resultMsg}");
                }
            }
            else
            {
                SendError(client, "Missing credentials");
            }
        }
        else
        {
            SendError(client, "Invalid registration");
        }

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
                // Khi client là null, không cần gửi error message
                MsgService.SendErrorMessage(client, $"Error disposing client: {ex.Message}");
            }
        }

        Clients.Clear();
        MapUserIdToIp.Clear();
    }
}
