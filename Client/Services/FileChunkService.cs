using System.Collections.Concurrent;
using Shared.Models;
using Shared.Utils.Patterns;

namespace Client.Services;

public class FileChunkService : Singleton<FileChunkService>
{
    private readonly ConcurrentDictionary<Guid, List<ChunkDto>> _chunks = new ConcurrentDictionary<Guid, List<ChunkDto>>();

    /// <summary>
    /// Process incoming file chunk.
    /// Được gọi từ client khi nhận được FileChunkMessageDto.
    /// Xử lý trên một Task riêng để không block luồng chính.
    /// </summary>
    /// <param name="fileChunkMsg">FileChunkMessageDto chứa ReceiverId và ChunkDto</param>
    public void ProcessChunk(FileChunkMessageDto fileChunkMsg, Action<Guid, byte[]>? OnFileReceived = null)
    {
        Task.Run(() =>
        {
            try
            {
                if (fileChunkMsg.Chunk == null) return;
                var fileId = fileChunkMsg.Chunk.MessageId;
                var totalChunks = fileChunkMsg.Chunk.TotalChunks;

                // Lấy hoặc tạo mới danh sách chunk cho file này
                var chunkList = _chunks.GetOrAdd(fileId, _ => new List<ChunkDto>());

                lock (chunkList)
                {
                    // Thêm chunk mới vào danh sách
                    chunkList.Add(fileChunkMsg.Chunk);

                    // Nếu đã nhận đủ các chunk
                    if (chunkList.Count >= totalChunks)
                    {
                        // Sắp xếp các chunk theo ChunkIndex
                        var orderedChunks = chunkList.OrderBy(c => c.ChunkIndex).ToList();
                        // Tính tổng kích thước payload
                        int totalSize = orderedChunks.Sum(c => c.Payload.Length);
                        byte[] fullData = new byte[totalSize];
                        int offset = 0;
                        foreach (var chunk in orderedChunks)
                        {
                            Buffer.BlockCopy(chunk.Payload, 0, fullData, offset, chunk.Payload.Length);
                            offset += chunk.Payload.Length;
                        }

                        // File được nhận đầy đủ, xoá entry khỏi dictionary
                        _chunks.TryRemove(fileId, out _);

                        // Gọi event hoặc callback để thông báo file đã hoàn thành
                        OnFileReceived?.Invoke(fileId, fullData);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file chunk: {ex.Message}");
            }
        });
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