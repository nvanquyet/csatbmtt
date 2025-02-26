using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class UdpClientHandler
{
    private readonly string serverIp;
    private readonly int serverPort;

    public UdpClientHandler(string ip, int port)
    {
        serverIp = ip;
        serverPort = port;
    }

    public async Task SendAndReceive(string message)
    {
        try
        {
            using (UdpClient client = new UdpClient())
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

                await client.SendAsync(data, data.Length, serverEndpoint);
                Console.WriteLine($"[UDP] Đã gửi: {message}");

                // Nhận phản hồi từ server
                UdpReceiveResult receivedResult = await client.ReceiveAsync();
                string response = Encoding.UTF8.GetString(receivedResult.Buffer);
                Console.WriteLine($"[UDP] Server trả lời: {response}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UDP] Lỗi: {ex.Message}");
        }
    }
}
