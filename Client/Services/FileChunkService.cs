using System.Collections.Concurrent;
using Shared;
using Shared.Models;
using Shared.Utils.Patterns;

namespace Client.Services;

public class FileChunkService : Singleton<FileChunkService>
{
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<ChunkDto>> _chunks = new();
    private readonly List<Guid> _chunksStop = [];
    private readonly SemaphoreSlim _semaphore = new(5); 

    public void ProcessChunk(FileChunkMessageDto fileChunkMsg, Action<Guid, byte[]>? onFileReceived = null)
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
                    Logger.LogInfo($"File {fileId} canceled, stop processing.");
                    _chunksStop.Remove(fileId);
                    //Remove data chunks
                    _chunks.TryRemove(fileId, out _);
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

                    onFileReceived?.Invoke(fileId, fullData);
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
        Logger.LogError($"Cancel Process file: {fileId}");
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
                                    ?? throw new Exception("Can't parse TransferData from received data.");
        return transferData;
    }
}