using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DesServer.Services
{
    public static class MessageService
    {
        #region Send

        // Send TCP
        public static void SendTcpMessage(TcpClient targetClient, string message)
        {
            try
            {
                // Get NetworkStream to Send Message
                NetworkStream stream = targetClient.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                stream.Flush();
                Console.WriteLine("Message sent via TCP.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending message via TCP: " + ex.Message);
            }
        }


        // Send UDP
        public static void SendUdpMessage(string targetIp, int targetPort, string message)
        {
            try
            {
                using UdpClient udpClient = new UdpClient();
                byte[] data = Encoding.UTF8.GetBytes(message);
                udpClient.Send(data, data.Length, targetIp, targetPort);
                Console.WriteLine("Message sent via UDP.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending message via UDP: " + ex.Message);
            }
        }

        #endregion

        #region Receive
        //
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
}