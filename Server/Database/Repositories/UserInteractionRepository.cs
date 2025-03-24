using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Models;
using Server.AppSetting;

namespace Server.Database.Repositories
{
    public abstract class UserInteractionRepository : DatabaseContext
    {
        // Lấy collection chứa ConversationRecord
        private static readonly IMongoCollection<ConversationRecord> InteractionCollection =
            DatabaseService.GetCollection<ConversationRecord>(ServerConfig.UserInteractionCollection);

        
        public static async Task UpsertInteraction(string firstUser, string secondUser)
        {
            await UpsertInteraction(firstUser, secondUser, DateTime.Now);
            await UpsertInteraction(secondUser, firstUser, DateTime.Now);
        }
        
        /// <summary>
        /// Cập nhật hoặc thêm mới InteractionDetail cho một owner.
        /// Nếu record của owner chưa tồn tại thì tạo mới record với interaction.
        /// Nếu đã tồn tại, kiểm tra xem đã có interaction với partner hay chưa:
        /// - Nếu có thì cập nhật LastInteractionTime.
        /// - Nếu chưa có thì thêm mới InteractionDetail.
        /// </summary>
        /// <param name="ownerId">ID của user sở hữu bản ghi</param>
        /// <param name="partnerId">ID của user đã chat cùng</param>
        /// <param name="interactionTime">Thời gian tương tác</param>
        private static async Task UpsertInteraction(string ownerId, string partnerId, DateTime interactionTime)
        {
            if (string.IsNullOrWhiteSpace(ownerId) || string.IsNullOrWhiteSpace(partnerId))
                return;

            // Lọc record theo ownerId (userA)
            var filter = Builders<ConversationRecord>.Filter.Eq(r => r.OwnerId, ownerId);
            var conversation = await InteractionCollection.Find(filter).FirstOrDefaultAsync();

            if (conversation == null)
            {
                // Nếu userA chưa có record nào, tạo mới record với InteractionDetail chứa userB
                var newRecord = new ConversationRecord
                {
                    OwnerId = ownerId,
                    Interactions =
                    [
                        new InteractionDetail
                        {
                            ParticipantId = partnerId,
                            LastInteractionTime = interactionTime
                        }
                    ]
                };

                await InteractionCollection.InsertOneAsync(newRecord);
            }
            else
            {
                // Nếu record của userA đã tồn tại, kiểm tra xem đã có interaction với userB chưa
                var existingInteraction = conversation.Interactions.FirstOrDefault(i => i.ParticipantId == partnerId);

                if (existingInteraction != null)
                {
                    // Nếu đã có, cập nhật LastInteractionTime của interaction đó
                    var update = Builders<ConversationRecord>.Update
                        .Set("interactions.$[elem].lastInteractionTime", interactionTime);

                    var arrayFilter = new List<ArrayFilterDefinition>
                    {
                        new BsonDocumentArrayFilterDefinition<InteractionDetail>(
                            new BsonDocument("elem.participantId", partnerId))
                    };

                    var updateOptions = new UpdateOptions { ArrayFilters = arrayFilter };

                    await InteractionCollection.UpdateOneAsync(filter, update, updateOptions);
                }
                else
                {
                    // Nếu chưa có interaction với userB, thêm mới InteractionDetail vào mảng
                    var newInteraction = new InteractionDetail
                    {
                        ParticipantId = partnerId,
                        LastInteractionTime = interactionTime
                    };

                    var update = Builders<ConversationRecord>.Update.Push(r => r.Interactions, newInteraction);
                    await InteractionCollection.UpdateOneAsync(filter, update);
                }
            }
        }

        /// <summary>
        /// Lấy danh sách conversation record của một user (theo ownerId)
        /// </summary>
        /// <param name="ownerId">ID của user</param>
        /// <returns>ConversationRecord sắp xếp giảm dần theo thời gian tương tác của từng partner</returns>
        public static async Task<ConversationRecord?> GetConversationRecord(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
                return null;

            var filter = Builders<ConversationRecord>.Filter.Eq(r => r.OwnerId, ownerId);
            var record = await InteractionCollection.Find(filter).FirstOrDefaultAsync();

            // Nếu record tồn tại, sắp xếp danh sách Interactions theo LastInteractionTime giảm dần
            if (record != null)
            {
                record.Interactions = record.Interactions
                    .OrderByDescending(i => i.LastInteractionTime)
                    .ToList();
            }

            return record;
        }
    }
}