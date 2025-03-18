using Server.AppSetting;
using Server.Services;

namespace Server.Database;

public abstract class DatabaseContext
{
    protected static DatabaseService DatabaseService { get; } = new(ServerConfig.DatabaseConnectionString, ServerConfig.DatabaseString);
}