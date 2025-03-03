using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonopolyClient
{
    public class GameMessage
    {
        public string Type { get; set; }
        public JsonElement Data { get; set; }
    }

    public class GameClient
    {
        private TcpClient _client;
        private NetworkStream _stream;

        public async Task ConnectAsync(string ipAddress, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ipAddress, port);
            _stream = _client.GetStream();
            Console.WriteLine("Connected to server.");

            _ = ListenForMessagesAsync();
        }

        private async Task ListenForMessagesAsync()
        {
            byte[] buffer = new byte[4096];
            try
            {
                while (true)
                {
                    int byteCount = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (byteCount == 0)
                    {
                        Console.WriteLine("Disconnected from server.");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    Console.WriteLine("Received: " + message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error receiving data: " + ex.Message);
            }
        }

        public async Task SendMessageAsync(GameMessage message)
        {
            string json = JsonSerializer.Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json);
            await _stream.WriteAsync(data, 0, data.Length);
        }

        public async Task JoinGameAsync(string playerName)
        {
            var joinMessage = new GameMessage
            {
                Type = "JoinGame",
                Data = JsonSerializer.SerializeToElement(new { Name = playerName })
            };
            await SendMessageAsync(joinMessage);
        }
    }
}
