using System.Net;
using System.Net.Sockets;
using System.Text;
using DesServer.Controllers;
using DesServer.Database;
using DesServer.Models;
using Shared.Models;
using Shared.Services;

namespace DesServer.Services
{
    public class TcpServerService(string ipAddress, int port)
    {
        private readonly TcpListener _tcpListener = new(IPAddress.Any, port);


        public void Start()
        {
            try
            {
                _tcpListener.Start();
                Console.WriteLine("TCP Server is running...");
                while (true)
                {
                    var client = _tcpListener.AcceptTcpClient();
                    Task.Run(() => HandleClient(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting TCP listener: {ex.Message}");
            }
        }

        private void HandleClient(TcpClient? client)
        {
            var stream = client?.GetStream();
            byte[] buffer = new byte[1024];
            if (stream != null)
            {
                while (client?.Connected == true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0) continue;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.WriteLine($"Received TCP Message: {message}");
                    HandleClientComm(client, message);
                }
            }
        }

        private void HandleClientComm(TcpClient? client, string jsonMessage)
        {
            try
            {
                var message = MessageNetwork<dynamic>.FromJson(jsonMessage);
                if (message == null)
                {
                    MsgService.SendErrorMessage(client, "Invalid message format", StatusCode.Error);
                    return;
                }

                switch (message.Type)
                {
                    case CommandType.Authentication:
                        HandleAuthentication(client, message);
                        break;

                    case CommandType.Registration:
                        HandleRegistration(client, message);
                        break;
                    case CommandType.GetAllUsers:
                        HandleGetAllUsers(client);
                        break;
                    case CommandType.LoadMessage:
                        HandleLoadMessage(client, message);
                        break;
                    case CommandType.SendMessage:
                        HandleSendMessage(client, message);
                        break;
                    default:
                        MsgService.SendErrorMessage(client, "Unsupported action type", StatusCode.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                MsgService.SendErrorMessage(client, $"Server error: {ex.Message}", StatusCode.Error);
            }
        }

        private void HandleSendMessage(TcpClient? client, MessageNetwork<dynamic> message)
        {
            if (message is { Code: StatusCode.Success } && client != null)
            {
                if (message.TryParseData(out ChatMessage? chatMessage) && chatMessage is
                    {
                        ReceiverId: not null
                    })
                {
                    var targetClient = ConnectionController.Instance.GetUserConnection(chatMessage.ReceiverId);

                    chatMessage.SenderName = UserDatabase.GetUserNameById(chatMessage.SenderId);
                    chatMessage.ReceiverName = UserDatabase.GetUserNameById(chatMessage.ReceiverId);

                    //Save to database
                    MessageDatabase.SaveMessage(chatMessage);

                    var response = new MessageNetwork<object>(
                        type: CommandType.ReceiveMessage,
                        code: StatusCode.Success,
                        data: chatMessage
                    ).ToJson();

                    //MsgService.SendTcpMessage(client, response);
                    MsgService.SendTcpMessage(targetClient?.TcpClient, response);
                }
                else
                {
                    MsgService.SendErrorMessage(client, "Target not found", StatusCode.InvalidRequest);
                }
            }
            else
            {
                MsgService.SendErrorMessage(client, "Server Error", StatusCode.InvalidRequest);
            }
        }

        private void HandleLoadMessage(TcpClient? client, MessageNetwork<dynamic> message)
        {
            if (message is { Code: StatusCode.Success } && client != null)
            {
                if (message.TryParseData(out ChatHistoryRequest? history) && history != null)
                {
                    var allChatMessage = MessageDatabase.LoadMessages(history.SenderId, history.ReceiverId);

                    var response = new MessageNetwork<object>(
                        type: CommandType.ReceiveMessage,
                        code: StatusCode.Success,
                        data: allChatMessage
                    ).ToJson();

                    MsgService.SendTcpMessage(client, response);
                }
                else
                {
                    MsgService.SendErrorMessage(client, "Target not found", StatusCode.InvalidRequest);
                }
            }
            else
            {
                MsgService.SendErrorMessage(client, "Server Error", StatusCode.InvalidRequest);
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

                    MsgService.SendTcpMessage(client, response);
                }
                else
                {
                    MsgService.SendErrorMessage(client, "Missing credentials", StatusCode.InvalidRequest);
                }
            }
            else
            {
                MsgService.SendErrorMessage(client, "Invalid authentication", StatusCode.InvalidRequest);
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
                    MsgService.SendTcpMessage(client, response);
                }
                else
                {
                    MsgService.SendErrorMessage(client, "Missing credentials", StatusCode.InvalidRequest);
                }
            }
            else
            {
                MsgService.SendErrorMessage(client, "Invalid registration", StatusCode.InvalidRequest);
            }
        }

        private void HandleGetAllUsers(TcpClient? client)
        {
            if (client == null) return;
            var allUsers = UserDatabase.GetAllUsers();
            var msg = new MessageNetwork<List<User>>(type: CommandType.GetAllUsers, code: StatusCode.Success,
                data: allUsers);
            MsgService.SendTcpMessage(client, msg.ToJson());
        }
    }
}