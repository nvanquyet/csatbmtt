using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models;

public class ConversationRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("ownerId")]
    public string OwnerId { get; init; } = string.Empty;

    [BsonElement("interactions")]
    public List<InteractionDetail> Interactions { get; set; } = [];
}

public class InteractionDetail()
{
    [BsonElement("participantId")]
    public string ParticipantId { get; init; } = string.Empty;

    [BsonElement("lastInteractionTime")]
    public DateTime LastInteractionTime { get; init; }
}