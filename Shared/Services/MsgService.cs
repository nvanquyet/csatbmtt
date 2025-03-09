using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using Shared.Models;

namespace Shared.Services;

public static class MsgService
{
    // Send TCP
    public static void SendTcpMessage(TcpClient? target, string message, bool showLog = true)
    {
        try
        {
            if (target == null || !target.Connected)
            {
                if(showLog) Console.WriteLine("TCP client is not connected.");
                return;
            }

            NetworkStream stream = target.GetStream();

            if (!stream.CanWrite)
            {
                if(showLog) Console.WriteLine("NetworkStream is not writable.");
                return;
            }

            byte[] data = Encoding.UTF8.GetBytes(message);
            
            stream.Write(data, 0, data.Length);
            stream.Flush();
            if(showLog) Console.WriteLine($"Message sent to TCP: {message}");
        }
        catch (ObjectDisposedException ex)
        {
            if(showLog) Console.WriteLine("Stream or TcpClient was disposed: " + ex.Message);
        }
        catch (IOException ex)
        {
            if(showLog) Console.WriteLine("I/O Error: " + ex.Message);
        }
        catch (Exception ex)
        {
            if(showLog) Console.WriteLine("Error sending message to TCP: " + ex.Message);
        }
    }

    public static void SendErrorMessage(TcpClient? client, string error, StatusCode code, bool showLog = true)
    {
        var errorMessage = new MessageNetwork<ErrorData>
        (
            type: CommandType.None,
            code: code,
            data: new ErrorData($"Error: {error}")
        ).ToJson();

        SendTcpMessage(client, errorMessage, showLog);
    }

    // Send UDP
    public static async void SendUdpMessage(string targetIp, int targetPort, string message, bool showLog = true)
    {
        try
        {
            using UdpClient udpClient = new UdpClient();
            byte[] data = Encoding.UTF8.GetBytes(message);
            await udpClient.SendAsync(data, data.Length, targetIp, targetPort);
            if(showLog) Console.WriteLine($"Message sent via UDP. {data}");
        }
        catch (Exception ex)
        {
            if(showLog) Console.WriteLine("Error sending message via UDP: " + ex.Message);
        }
    }


}