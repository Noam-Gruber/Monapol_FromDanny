using System;
using System.Threading.Tasks;

namespace MonopolyServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int port = 5000; 
            GameServer server = new GameServer(port);
            await server.StartAsync();
        }
    }
}
