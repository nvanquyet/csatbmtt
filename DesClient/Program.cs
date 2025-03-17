using DesClient.Menu;
using DesClient.Network;
using DesClient.Network.Tcp;
using DesClient.Network.Udp;
using DesClient.Services;
using Shared.Networking.Interfaces;

namespace DesClient
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            _ = new NetworkManager();
            while (true)
            {
                
            }
        }
    }
}