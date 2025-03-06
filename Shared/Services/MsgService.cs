using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using Shared.Models;

namespace Shared.Services;

public static class MsgService
{
    #region Send
    // Send TCP
    public static void SendTcpMessage(TcpClient? target, string message)
    {
        try
        {
            if (target == null || !target.Connected)
            {
                Console.WriteLine("TCP client is not connected.");
                return;
            }

            NetworkStream stream = target.GetStream();

            if (!stream.CanWrite)
            {
                Console.WriteLine("NetworkStream is not writable.");
                return;
            }

            byte[] data = Encoding.UTF8.GetBytes(message);
            
            stream.Write(data, 0, data.Length);
            stream.Flush();
            Console.WriteLine($"Message sent to TCP: {message}");
        }
        catch (ObjectDisposedException ex)
        {
            Console.WriteLine("Stream or TcpClient was disposed: " + ex.Message);
        }
        catch (IOException ex)
        {
            Console.WriteLine("I/O Error: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending message to TCP: " + ex.Message);
        }
    }

    public static void SendErrorMessage(TcpClient? client, string error, StatusCode code)
    {
        var errorMessage = new Message
        (
            type: CommandType.None,
            code: code,
            content: error,
            data: new Dictionary<string, object> { { "Timestamp", DateTime.UtcNow } }
        ).ToJson();

        SendTcpMessage(client, errorMessage);
    }

    // Send UDP
    public static async void SendUdpMessage(string targetIp, int targetPort, string message)
    {
        try
        {
            using UdpClient udpClient = new UdpClient();
            byte[] data = Encoding.UTF8.GetBytes(message);
            await udpClient.SendAsync(data, data.Length, targetIp, targetPort);
            Console.WriteLine($"Message sent via UDP. {data}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending message via UDP: " + ex.Message);
        }
    }

    #endregion

    #region Receive

    // // Receive TCP
    // public static void ReceiveTcpMessage(TcpClient client)
    // {
    //     try
    //     {
    //         NetworkStream stream = client.GetStream();
    //         byte[] buffer = new byte[1024];
    //         int bytesRead;
    //
    //         while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
    //         {
    //             string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    //             Console.WriteLine("Received message via TCP: " + message);
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine("Error receiving message via TCP: " + ex.Message);
    //     }
    // }
    //
    // //Receive UDP
    // public static void ReceiveUdpMessage(int listenPort)
    // {
    //     try
    //     {
    //         using UdpClient udpClient = new UdpClient(listenPort);
    //         IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, listenPort);
    //         while (true)
    //         {
    //             byte[] data = udpClient.Receive(ref endPoint);
    //             string message = Encoding.UTF8.GetString(data);
    //             Console.WriteLine("Received message via UDP: " + message);
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine("Error receiving message via UDP: " + ex.Message);
    //     }
    // }
    //

    #endregion
}