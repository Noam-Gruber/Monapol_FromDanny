// GameClient.cs
using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonopolyClientUI
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

        public event Action<string> MessageReceived;

        public async Task ConnectAsync(string ipAddress, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ipAddress, port);
            _stream = _client.GetStream();
            StartListening();
        }

        private async void StartListening()
        {
            byte[] buffer = new byte[4096];
            try
            {
                while (true)
                {
                    int byteCount = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (byteCount == 0)
                    {
                        MessageReceived?.Invoke("Disconnected from server.");
                        break;
                    }
                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    MessageReceived?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                MessageReceived?.Invoke($"Error receiving data: {ex.Message}");
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

        public async Task RollDiceAsync()
        {
            var rollMessage = new GameMessage
            {
                Type = "RollDice",
                Data = JsonSerializer.SerializeToElement(new { })
            };
            await SendMessageAsync(rollMessage);
        }
    }
}
