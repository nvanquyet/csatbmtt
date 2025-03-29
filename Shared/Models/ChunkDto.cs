namespace Shared.Models;
public class ChunkDto
{
    public Guid MessageId { get; set; }
    public int ChunkIndex { get; set; }
    public int TotalChunks { get; set; }
    public byte[] Payload { get; set; } = [];
}

public class FileChunkMessageDto
{
    public string? ReceiverId { get; set; }
    public ChunkDto? Chunk { get; set; }
}

