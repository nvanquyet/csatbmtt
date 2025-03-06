using System.Net;
using System.Net.Sockets;
using System.Text;
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
                var message = Message.FromJson(jsonMessage);
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
            if (message is { Code: StatusCode.Success, Data: not null })
            {
                if (message.Data.TryGetValue("Username", out var username) &&
                    message.Data.TryGetValue("Password", out var password))
                {
                    var result = ClientSessionService.Instance.LoginUser(
                        username.ToString(),
                        password.ToString()
                    );

                    var response = new Message
                    (
                        type: CommandType.Authentication,
                        code: result.Success ? StatusCode.Success : StatusCode.Failed,
                        content: result.Success ? "Success" : "Failed!",
                        data: result.Success
                            ? new Dictionary<string, object> { { "Success", result.Message } }
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
                    var result = ClientSessionService.Instance.RegisterUser(
                        username.ToString(),
                        password.ToString()
                    );

                    var response = new Message
                    (
                        type: CommandType.Authentication,
                        code: result.Success ? StatusCode.Success : StatusCode.Failed,
                        content: result.Success ? "Success" : "Failed",
                        data: result.Success
                            ? new Dictionary<string, object> { { "Success", result.Message } }
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