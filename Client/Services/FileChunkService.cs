﻿using System.Collections.Concurrent;
using System.Text;
using Newtonsoft.Json;
using Shared;
using Shared.Models;
using Shared.Utils;
using Shared.Utils.Patterns;

namespace Client.Services;

public class FileChunkService : Singleton<FileChunkService>
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<int, ChunkDto>> _chunks = new();
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
                var chunkIndex = fileChunkMsg.Chunk.ChunkIndex;

                if (_chunksStop.Contains(fileId))
                {
                    Logger.LogInfo($"File {fileId} canceled, stop processing.");
                    _chunksStop.Remove(fileId);
                    _chunks.TryRemove(fileId, out _);
                    return;
                }

                // Sử dụng ConcurrentDictionary<int, ChunkDto> để tránh lỗi thứ tự
                var chunkList = _chunks.GetOrAdd(fileId, _ => new ConcurrentDictionary<int, ChunkDto>());

                // Kiểm tra nếu chunk đã tồn tại thì bỏ qua (tránh trùng lặp)
                if (!chunkList.ContainsKey(chunkIndex))
                {
                    chunkList[chunkIndex] = fileChunkMsg.Chunk;
                    Logger.LogInfo($"Received chunk {chunkIndex}/{totalChunks} for file {fileId}");
                }

                // Kiểm tra đã nhận đủ chunk chưa
                if (chunkList.Count >= totalChunks)
                {
                    if (!_chunks.TryRemove(fileId, out var finalChunks)) return;

                    // Sắp xếp chunk theo index
                    var orderedChunks = finalChunks.OrderBy(c => c.Key).Select(c => c.Value).ToList();

                    // Tính tổng kích thước
                    int totalSize = orderedChunks.Sum(c => c.Payload.Length);
                    byte[] fullData = new byte[totalSize];
                    int offset = 0;

                    

                    // Ghép file theo thứ tự
                    foreach (var chunk in orderedChunks)
                    {
                        Logger.LogInfo($"Chunk index: {chunk.ChunkIndex}, Size: {chunk.Payload.Length}");
                        Buffer.BlockCopy(chunk.Payload, 0, fullData, offset, chunk.Payload.Length);
                        offset += chunk.Payload.Length;
                    }
                    Logger.LogInfo($"Total chunks received: {chunkList.Count}, expected: {totalChunks}");
                    Logger.LogInfo($"Total data size: {totalSize}, expected: {fileChunkMsg.Chunk.Payload.Length * totalChunks}");
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

    public static byte[] SerializeTransferData(TransferData transferData) => JsonUtils.Serialize(transferData);

    public static TransferData? ParseTransferData(byte[] fullData) => JsonUtils.Deserialize<TransferData>(fullData);

}