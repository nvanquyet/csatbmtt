using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Shared.Models;
using Shared.Utils;

namespace Shared.Services;

public static class MsgService
{
    private class ClientQueue
    {
        public ConcurrentQueue<byte[]> Queue { get; } = new ConcurrentQueue<byte[]>();
        public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);
        public bool IsProcessing { get; set; } = false;
    }
    private interface IMessageDispatcher<in T> where T : class
    {
        /// <summary>
        /// Enqueue the message to be sent to the given client.
        /// </summary>
        /// <param name="client">The client (destination) that will receive the message.</param>
        /// <param name="message">The content of the message to be sent.</param>
        void EnqueueMessage(T? client, string message);
    }
    private abstract class MessageDispatcherBase<T> : IMessageDispatcher<T> where T : class
    {
        // Shared static state across implementations.
        private static readonly ConcurrentDictionary<T, ClientQueue> ClientQueues = new();

        public void EnqueueMessage(T? client, string message)
        {
            EnqueueMessage(client, ByteUtils.GetBytesFromString(message));
        }

        private void EnqueueMessage(T? client, byte[] data)
        {
            if (client is null or TcpClient { Connected: false })
            {
                Logger.LogInfo("client không kết nối.");
                return;
            }

            // Lấy hoặc tạo mới hàng đợi cho client đó
            var queue = ClientQueues.GetOrAdd(client, _ => new ClientQueue());
            queue.Queue.Enqueue(data);

            // Nếu chưa có tiến trình xử lý hàng đợi cho client này, bắt đầu xử lý
            if (!queue.IsProcessing)
            {
                _ = ProcessQueue(client, queue);
            }
            
            // Nếu chưa có tiến trình xử lý cho endpoint này, bắt đầu xử lý hàng đợi
            if (!queue.IsProcessing)
            {
                _ = ProcessQueue(client, queue);
            }
        }

        private async Task ProcessQueue(T client, ClientQueue queue)
        {
            queue.IsProcessing = true;
            try
            {
                // Tiếp tục xử lý cho đến khi hàng đợi rỗng
                while (queue.Queue.TryDequeue(out byte[]? data))
                {
                    // Sử dụng semaphore để đảm bảo không có hai tác vụ gửi cùng lúc cho cùng một client
                    await queue.Semaphore.WaitAsync();
                    try
                    {
                        // Gọi hàm gửi tin nhắn bất đồng bộ
                        await SendMessage(client, data);
                    }
                    finally
                    {
                        queue.Semaphore.Release();
                    }
                }
            }
            finally
            {
                queue.IsProcessing = false;
            }
        }

        protected abstract Task SendMessage(T client, byte[] data);
    }

    private class TcpDispatcher : MessageDispatcherBase<TcpClient>
    {
        protected override async Task SendMessage(TcpClient client, byte[] data)
        {
            try
            {
                if (!client.Connected)
                {
                    Logger.LogInfo("TCP client is not connected.");
                    return;
                }
    
                var stream = client.GetStream();
    
                if (!stream.CanWrite)
                {
                    Logger.LogInfo("NetworkStream is not writable.");
                    return;
                }
            
                await stream.WriteAsync(data);
                stream.Flush();
                Logger.LogInfo($"Message sent to {client.Client.RemoteEndPoint}: {ByteUtils.GetStringFromBytes(data)}");
            }
            catch (ObjectDisposedException ex)
            {
                Logger.LogInfo("Stream or TcpClient was disposed: " + ex.Message);
            }
            catch (IOException ex)
            {
                Logger.LogInfo("I/O Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogInfo("Error sending message to TCP: " + ex.Message);
            }
        }
    }

    private class UdpDispatcher : MessageDispatcherBase<UdpClient>
    {
        protected override async Task SendMessage(UdpClient client, byte[] data)
        {
            try
            {
                // Ví dụ: ta cần địa chỉ IP/Port, 
                // nhưng UdpClient mặc định không lưu trữ Endpoint đích 
                // -> Tuỳ vào logic của bạn, có thể cài đặt ở chỗ khác.
                var endpoint = new IPEndPoint(IPAddress.Loopback, 9999);

                await client.SendAsync(data, data.Length, endpoint);

                Logger.LogInfo($"[UDP] Sent to {endpoint}: {data}");
            }
            catch (Exception ex)
            {
                Logger.LogInfo("Error sending UDP message: " + ex.Message);
            }
        }
    }

    
    private static readonly IMessageDispatcher<TcpClient> TcpDispatch = new TcpDispatcher();
    private static readonly IMessageDispatcher<UdpClient> UdpDispatch = new UdpDispatcher();

    public static void SendTcpMessage(TcpClient? tcpClient, string msg) => TcpDispatch.EnqueueMessage(tcpClient, msg);
    
    public static void SendUdpMessage(UdpClient? udpClient, string msg) => UdpDispatch.EnqueueMessage(udpClient, msg);

    public static void SendErrorMessage(TcpClient? client, string error)
    {
        var errorMessage = new MessageNetwork<ErrorMessage>
        (
            type: CommandType.None,
            code:  StatusCode.Error,
            data: new ErrorMessage($"Error: {error}")
        ).ToJson();

        SendTcpMessage(client, errorMessage);
    }
}