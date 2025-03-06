using DesServer.Services;
using MongoDB.Driver;
using Shared.AppSettings;
using Shared.Models;
using Shared.Services;

namespace DesServer.Database;

public class DbController : Singleton<DbController>
{
    private readonly DatabaseService _databaseService = new(Config.DatabaseConnectionString, Config.DatabaseString);

    public bool RegisterUser(string username, string password)
    {
        var users = _databaseService.GetCollection<User>("Users");
        var existingUser = users.Find(user => user.UserName == username).FirstOrDefault();
        if (existingUser != null) return false;
        
        var newUser = new User(userName: username, password: SecurityHelper.HashPassword(password));
        users.InsertOne(newUser);
        return true;
    }

    public bool Login(string username, string password)
    {
        var users = _databaseService.GetCollection<User>("Users");
        var user = users.Find(u => u.UserName == username).FirstOrDefault();
        if (user != null && user.Password == SecurityHelper.HashPassword(password)) return true;
        return false;
    }

    public bool UsernameValidation(string username)
    {
        var users = _databaseService.GetCollection<User>("Users");
        var user = users.Find(u => u.UserName == username).FirstOrDefault();
        if (user == null) return false;
        return true;
    }

    public List<User> GetAllUsers()
    {
        var users = _databaseService.GetCollection<User>("Users");
        return users.Find(u => u != null).ToList();
    }
}