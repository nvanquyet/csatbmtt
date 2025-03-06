using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class TcpClientHandler(string ip, int port)
{
    private readonly string serverIp = ip;
    private readonly int serverPort = port;

    public async Task ConnectAndSend(string message)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(serverIp, serverPort);
                NetworkStream stream = client.GetStream();

                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
                Console.WriteLine($"[TCP] Đã gửi: {message}");

                // Nhận phản hồi từ server
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"[TCP] Server trả lời: {response}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TCP] Lỗi: {ex.Message}");
        }
    }
}
