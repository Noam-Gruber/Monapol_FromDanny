using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonopolyClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GameClient client = new GameClient();
           
            await client.ConnectAsync("127.0.0.1", 5000);

            Console.WriteLine("Enter your name:");
            string name = Console.ReadLine();
            await client.JoinGameAsync(name);

        
            while (true)
            {
                Console.WriteLine("Press 'r' to roll the dice or 'q' to quit.");
                var key = Console.ReadKey();
                Console.WriteLine();
                if (key.KeyChar == 'r')
                {
                    var rollMsg = new GameMessage
                    {
                        Type = "RollDice",
                        Data = JsonSerializer.SerializeToElement(new { })
                    };
                    await client.SendMessageAsync(rollMsg);
                }
                else if (key.KeyChar == 'q')
                {
                    break;
                }
            }
        }
    }
}
