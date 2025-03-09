using DesServer.AppSetting;
using MongoDB.Driver;
using Shared.Models;
using Shared.Services;

namespace DesServer.Database;

public abstract class UserDatabase : ADatabase
{
    private static readonly IMongoCollection<User> Users = DatabaseService.GetCollection<User>(ServerConfig.UserCollection);
    public static bool RegisterUser(string username, string password)
    {
        var existingUser = Users.Find(user => user.UserName == username).FirstOrDefault();
        if (existingUser != null) return false;
        
        var newUser = new User(userName: username, password: SecurityHelper.HashPassword(password));
        Users.InsertOne(newUser);
        return true;
    }

    public static User? Login(string username, string password)
    {
        var user = Users.Find(u => u.UserName == username).FirstOrDefault();
        if (user != null && user.Password == SecurityHelper.HashPassword(password)) return user;
        return null;
    }

    public static bool UsernameValidation(string username)
    {
        var user = Users.Find(u => u.UserName == username).FirstOrDefault();
        if (user == null) return false;
        return true;
    }

    public static List<User> GetAllUsers() =>  Users.Find(u => u != null).ToList();
    
    public static string? GetUserNameById(string? id) => Users.Find(u => u.Id == id).FirstOrDefault().UserName;
}