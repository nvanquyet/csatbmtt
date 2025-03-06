using MongoDB.Driver;

namespace DesServer.Services;

public class DatabaseService
{
    private readonly IMongoDatabase _database;
    
    public DatabaseService(string connectionString, string dbName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(dbName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        var collection = _database.GetCollection<T>(collectionName);
        return collection;
    }
}