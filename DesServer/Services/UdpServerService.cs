using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DesServer.Services
{
    public class UdpServerService(string ipAddress, int port)
    {
        private readonly UdpClient _udpListener = new(new IPEndPoint(IPAddress.Any, port));

        public void Start()
        {
            Console.WriteLine("UDP Server is running...");
            while (true)
            {
                var result = _udpListener.ReceiveAsync().Result;
                var message = Encoding.UTF8.GetString(result.Buffer);

                // Handle message (you can create a handler for UDP requests here)
                Console.WriteLine($"Received UDP Message: {message}");
            }
        }
    }
}