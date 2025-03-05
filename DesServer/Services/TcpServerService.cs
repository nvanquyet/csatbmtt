using System.Net;
using System.Net.Sockets;
using System.Text;
using DesServer.Models;
using Newtonsoft.Json.Linq;
using Shared.Models;
using Shared.Services;
using ProtocolType = DesServer.Models.ProtocolType;

namespace DesServer.Services
{
    public class TcpServerService(string ipAddress, int port)
    {
        private readonly TcpListener _tcpListener = new(IPAddress.Any, port);
        private readonly ClientSessionService _clientSessionService = new();

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
                while (client?.Connected == true) // Tiếp tục nhận dữ liệu khi client vẫn còn kết nối
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    // Nếu không có dữ liệu (client đóng kết nối), thoát khỏi vòng lặp
                    if (bytesRead == 0) continue;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var request = new ProtocolRequest(message, ProtocolType.Tcp);

                    // Xử lý tin nhắn
                    Console.WriteLine($"Received TCP Message: {request.Message}");
                    HandleClientComm(client, request.Message); // Gọi xử lý thông điệp

                }
            }
        }

        private void HandleClientComm(TcpClient? client, string jsonMessage)
        {
            try
            {
                var message = Message.FromJson(jsonMessage);
                if (message == null || message.Type == MessageType.General)
                {
                    MsgService.SendErrorMessage(client, "Invalid message format", StatusCode.Error);
                    return;
                }

                switch (message.Type)
                {
                    case MessageType.Authentication:
                        HandleAuthentication(client, message);
                        break;

                    case MessageType.Registration:
                        HandleRegistration(client, message);
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

        private void HandleAuthentication(TcpClient? client, Message message)
        {
            // Kiểm tra code và data
            if (message is { Code: StatusCode.Success, Data: not null })
            {
                if (message.Data.TryGetValue("Username", out var username) &&
                    message.Data.TryGetValue("Password", out var password))
                {
                    var result = _clientSessionService.LoginUser(
                        username.ToString(),
                        password.ToString()
                    );

                    var response = new Message
                    (
                        type: MessageType.Authentication,
                        code: result.Success ? StatusCode.Success : StatusCode.Failed,
                        content: result.Success ? "Authenticated" : "Login FAILED!",
                        data: result.Success
                            ? new Dictionary<string, object> { { "SessionID", result.Message } }
                            : new Dictionary<string, object> { { "Failed", result.Message } }
                    ).ToJson();

                    MsgService.SendTcpMessage(client, response);
                }
                else
                {
                    MsgService.SendErrorMessage(client, "Missing credentials", StatusCode.InvalidRequest);
                }
            }

        }

        private void HandleRegistration(TcpClient? client, Message message)
        {
            if (message is { Code: StatusCode.Success, Data: not null })
            {
                if (message.Data.TryGetValue("Username", out var username) &&
                    message.Data.TryGetValue("Password", out var password))
                {
                    var result = _clientSessionService.RegisterUser(
                        username.ToString(),
                        password.ToString()
                    );

                    var response = new Message
                    (
                        type: MessageType.Authentication,
                        code: result.Success ? StatusCode.Success : StatusCode.Failed,
                        content: result.Success ? "Authenticated" : "Register FAILED!",
                        data: result.Success
                            ? new Dictionary<string, object> { { "SessionID", result.Message } }
                            : new Dictionary<string, object> { { "Failed", result.Message } }
                    ).ToJson();

                    MsgService.SendTcpMessage(client, response);
                }
                else
                {
                    MsgService.SendErrorMessage(client, "Missing credentials", StatusCode.InvalidRequest);
                }
            }
        }
    }
}