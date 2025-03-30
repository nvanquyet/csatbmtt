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
        void EnqueueMessage(T? client, string message, Action<byte[]>? progressCallback = null);
    }
    private abstract class MessageDispatcherBase<T> : IMessageDispatcher<T> where T : class
    {
        private static readonly ConcurrentDictionary<T, ClientQueue> ClientQueues = new();

        public void EnqueueMessage(T? client, string message, Action<byte[]>? progressCallback = null)
        {
            EnqueueMessage(client, ByteUtils.GetBytesFromString(message), progressCallback);
        }

        private void EnqueueMessage(T? client, byte[] data,Action<byte[]>? progressCallback = null)
        {
            if (client is null or TcpClient { Connected: false })
            {
                Logger.LogInfo("client không kết nối.");
                return;
            }

            var queue = ClientQueues.GetOrAdd(client, _ => new ClientQueue());
            queue.Queue.Enqueue(data);

            if (!queue.IsProcessing)
            {
                _ = ProcessQueue(client, queue, progressCallback);
            }
        }

        private async Task ProcessQueue(T client, ClientQueue queue, Action<byte[]>? progressCallback = null)
        {
            queue.IsProcessing = true;
            try
            {
                while (queue.Queue.TryDequeue(out byte[]? data))
                {
                    await queue.Semaphore.WaitAsync();
                    try
                    {
                        await SendMessage(client, data);
                        progressCallback?.Invoke(data);
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

    public static void SendTcpMessage(TcpClient? tcpClient, string msg, Action<byte[]>? progressCallback = null)
        => TcpDispatch.EnqueueMessage(tcpClient, msg, progressCallback);
    
    public static void SendUdpMessage(UdpClient? udpClient, string msg, Action<byte[]>? progressCallback = null)
        => UdpDispatch.EnqueueMessage(udpClient, msg, progressCallback);

    public static void SendErrorMessage(TcpClient? client, string error, Action<byte[]>? progressCallback = null)
    {
        var errorMessage = new MessageNetwork<ErrorMessage>
        (
            type: CommandType.None,
            code: StatusCode.Error,
            data: new ErrorMessage($"Error: {error}")
        ).ToJson();

        SendTcpMessage(client, errorMessage, progressCallback);
    }
}
