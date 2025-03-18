using MongoDB.Driver;

namespace Server.Services;

public class DatabaseService(string connectionString, string dbName)
{
    private readonly IMongoDatabase _database = new MongoClient(connectionString).GetDatabase(dbName);

    public IMongoCollection<T> GetCollection<T>(string collectionName) => _database.GetCollection<T>(collectionName);
}