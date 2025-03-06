using DesServer.AppSetting;
using DesServer.Services;

namespace DesServer.Database;

public class ADatabase
{
    protected static readonly DatabaseService DatabaseService = new(ServerConfig.DatabaseConnectionString, ServerConfig.DatabaseString);
}