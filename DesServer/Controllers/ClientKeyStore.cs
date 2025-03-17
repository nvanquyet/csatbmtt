using System.Net.Sockets;
using System.Text;
using Shared.Models;
using Shared.Utils;
using Shared.Utils.Patterns;

namespace DesServer.Controllers;

public interface IClientKeyStore
{
    bool RegisterClient(string id, byte[] publicKey);
    List<ClientInfo> GetAllClients();
}

public class ClientKeyStore : Singleton<ClientKeyStore>, IClientKeyStore
{
    private readonly Dictionary<string, ClientInfo> _clientsById = new();

    public bool RegisterClient(string id, byte[] publicKey)
    {
        if (_clientsById.TryGetValue(id, out var existingId))
        {
            //existingId.PublicKey = publicKey;
        }
        else
        {
            var newClient = new ClientInfo(id, publicKey);
            
            _clientsById[newClient.Id] = newClient;
        }
        return true;
    }

    public ClientInfo? GetClientById(string id) => _clientsById.GetValueOrDefault(id);

    public List<ClientInfo> GetAllClients() => _clientsById.Values.ToList();
    
    public List<ClientInfo> GetAllWithoutClient(string id) => _clientsById.Values.Where<ClientInfo>(x => x.Id != id).ToList<ClientInfo>();

}
