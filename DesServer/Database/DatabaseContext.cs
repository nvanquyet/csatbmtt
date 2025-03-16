using DesServer.AppSetting;
using DesServer.Services;

namespace DesServer.Database;

public abstract class DatabaseContext
{
    protected static DatabaseService DatabaseService { get; } = new(ServerConfig.DatabaseConnectionString, ServerConfig.DatabaseString);
}