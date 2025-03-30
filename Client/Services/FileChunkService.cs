using System.Collections.Concurrent;
using Shared;
using Shared.Models;
using Shared.Utils.Patterns;

namespace Client.Services;

public class FileChunkService : Singleton<FileChunkService>
{
    private ConcurrentDictionary<Guid, ConcurrentBag<ChunkDto>> _chunks = new ConcurrentDictionary<Guid, ConcurrentBag<ChunkDto>>();
    private List<Guid> _chunksStop = new();
    private SemaphoreSlim _semaphore = new SemaphoreSlim(5); // Giới hạn 5 file xử lý cùng lúc

    public void ProcessChunk(FileChunkMessageDto fileChunkMsg, Action<Guid, byte[]>? OnFileReceived = null)
    {
        Task.Run(async () =>
        {
            await _semaphore.WaitAsync();
            try
            {
                if (fileChunkMsg.Chunk == null) return;
                var fileId = fileChunkMsg.Chunk.MessageId;
                var totalChunks = fileChunkMsg.Chunk.TotalChunks;
                
                if (_chunksStop.Contains(fileId)) 
                {
                    Logger.LogInfo($"File {fileId} đã bị hủy, dừng xử lý.");
                    return;
                }

                var chunkList = _chunks.GetOrAdd(fileId, _ => new ConcurrentBag<ChunkDto>());
                

                if (chunkList.Count >= totalChunks)
                {
                    if (!_chunks.TryRemove(fileId, out _)) return;

                    var orderedChunks = chunkList.OrderBy(c => c.ChunkIndex).ToList();
                    int totalSize = orderedChunks.Sum(c => c.Payload.Length);
                    byte[] fullData = new byte[totalSize];
                    int offset = 0;

                    foreach (var chunk in orderedChunks)
                    {
                        Buffer.BlockCopy(chunk.Payload, 0, fullData, offset, chunk.Payload.Length);
                        offset += chunk.Payload.Length;
                    }

                    OnFileReceived?.Invoke(fileId, fullData);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error processing file chunk: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        });
    }



    public void CancelProcessChunk(FileChunkMessageDto fileChunkMsg, Action? onFileCanceled = null)
    {
        if (fileChunkMsg.Chunk == null) return;
        var fileId = fileChunkMsg.Chunk.MessageId;
        _chunksStop.Add(fileId);
        Logger.LogError($"Đã hủy nhận file: {fileId}");
        onFileCanceled?.Invoke();
    }
    
    public static byte[] SerializeTransferData(TransferData transferData)
    {
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(transferData);
        return System.Text.Encoding.UTF8.GetBytes(json);
    }
    
    public static TransferData ParseTransferData(byte[] fullData)
    {
        string json = System.Text.Encoding.UTF8.GetString(fullData);
        TransferData transferData = Newtonsoft.Json.JsonConvert.DeserializeObject<TransferData>(json)
                                    ?? throw new Exception("Không thể parse TransferData từ dữ liệu nhận được.");
        return transferData;
    }
}