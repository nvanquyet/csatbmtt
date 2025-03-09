using DesServer.AppSetting;
using DesServer.Services;

namespace DesServer.Database;

public abstract class ADatabase
{
    protected static DatabaseService DatabaseService { get; } = new(ServerConfig.DatabaseConnectionString, ServerConfig.DatabaseString);
}