using System.Net;
using System.Net.Sockets;
using System.Text;
using DesServer.Models;
using Newtonsoft.Json;
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
            // Deserialize JSON message
            dynamic message = JsonConvert.DeserializeObject(jsonMessage) ?? throw new InvalidOperationException();

            // Handle with type
            switch ((string)message.type)
            {
                case ActionTypes.Login:
                    _clientSessionService.LoginUser(client, message, message);
                    break;
                case ActionTypes.Register:
                    _clientSessionService.LoginUser(client, message, message);
                    break;
                default:
                    Console.WriteLine("Unknown message type: " + message.type);
                    break;
            }
        }
    }
}