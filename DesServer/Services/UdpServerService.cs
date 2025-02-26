using System.Net;
using System.Net.Sockets;
using System.Text;
using DesServer.Models;
using ProtocolType = DesServer.Models.ProtocolType;

namespace DesServer.Services
{
    public class UdpServerService(string ipAddress, int port)
    {
        private readonly UdpClient _udpListener = new(new IPEndPoint(IPAddress.Parse(ipAddress), port));

        public void Start()
        {
            Console.WriteLine("UDP Server is running...");
            while (true)
            {
                var result = _udpListener.ReceiveAsync().Result;
                var message = Encoding.UTF8.GetString(result.Buffer);

                var request = new ProtocolRequest(message: message, protocol: ProtocolType.Udp);

                // Handle message (you can create a handler for UDP requests here)
                Console.WriteLine($"Received UDP Message: {request.Message}");
            }
        }
    }
}