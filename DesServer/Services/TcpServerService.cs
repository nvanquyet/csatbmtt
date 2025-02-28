using System.Net;
using System.Net.Sockets;
using System.Text;
using DesServer.Models;
using Newtonsoft.Json.Linq;
using Shared.Models;
using Shared.Network;
using ActionTypes = Shared.Models.ActionTypes;
using ProtocolType = DesServer.Models.ProtocolType;

namespace DesServer.Services
{
    public class TcpServerService(string ipAddress, int port)
    {
        private readonly TcpListener _tcpListener = new(IPAddress.Parse(ipAddress), port);
        private readonly ClientSessionService _clientSessionService = new();

        public void Start()
        {
            _tcpListener.Start();
            Console.WriteLine("TCP Server is running...");
            while (true)
            {
                var client = _tcpListener.AcceptTcpClient();
                HandleClient(client);
            }
        }

        private void HandleClient(TcpClient client)
        {
            var stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            var request = new ProtocolRequest(message, ProtocolType.Tcp);

            // Handle message (you can create a handler for TCP requests here)
            Console.WriteLine($"Received TCP Message: {request.Message}");

            HandleClientComm(client, request.Message);

            client.Close();
        }


        private void HandleClientComm(TcpClient client, string jsonMessage)
        {
            try
            {
                var message = Message.FromJson(jsonMessage);
                if (message == null || message.Type == MessageType.General)
                {
                    SendErrorMessage(client, "Invalid message format", StatusCode.Error);
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
                        SendErrorMessage(client, "Unsupported action type", StatusCode.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                SendErrorMessage(client, $"Server error: {ex.Message}", StatusCode.Error);
            }
        }

        private void HandleAuthentication(TcpClient client, Message message)
        {
            // Kiểm tra code và data
            if (message.Code == StatusCode.LoginAttempt && message.Data != null)
            {
                if (message.Data.TryGetValue("Username", out var username) &&
                    message.Data.TryGetValue("Password", out var password))
                {
                    var loginResult = _clientSessionService.LoginUser(
                        client,
                        username.ToString(),
                        password.ToString()
                    );

                    // Gửi kết quả đăng nhập theo chuẩn message
                    var response = new Message
                    {
                        Type = MessageType.Authentication,
                        Code = loginResult.Success ? StatusCode.Success : StatusCode.LoginFailed,
                        Content = loginResult.Success ? "Authenticated" : "Login FAILED!",
                        Data = loginResult.Success
                            ? new Dictionary<string, object> { { "SessionID", loginResult.SessionId } }
                            : null
                    };

                    MessageService.SendTcpMessage(client, response);
                }
                else
                {
                    SendErrorMessage(client, "Missing credentials", StatusCode.InvalidRequest);
                }
            }
        }

        private void HandleRegistration(TcpClient client, Message message)
        {
            // Logic tương tự cho đăng ký
            if (message.Data != null && /* validation logic */)
            {
                // ... xử lý đăng ký
            }
        }

        private void SendErrorMessage(TcpClient client, string error, StatusCode code)
        {
            var errorMessage = new Message
            {
                Type = MessageType.General,
                Code = code,
                Content = error,
                Data = new Dictionary<string, object> { { "Timestamp", DateTime.UtcNow } }
            };

            MessageService.SendTcpMessage(client, errorMessage);
        }
    }
}