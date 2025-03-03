using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MonopolyGame
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public int Money { get; set; }
    }

    public class GameState
    {
        public List<Player> Players { get; set; } = new List<Player>();
        public int CurrentPlayerIndex { get; set; }
    }

    public class GameMessage
    {
        public string Type { get; set; }
        public JsonElement Data { get; set; }
    }

    public class GameServer
    {
        private readonly TcpListener _listener;
        private readonly ConcurrentDictionary<string, TcpClient> _clients = new();
        private GameState _gameState = new GameState();

        public GameServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("Server started...");

            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            string clientId = Guid.NewGuid().ToString();
            _clients.TryAdd(clientId, client);
            Console.WriteLine($"Client connected: {clientId}");

            using (var networkStream = client.GetStream())
            {
                byte[] buffer = new byte[4096];
                try
                {
                    while (true)
                    {
                        int byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                        if (byteCount == 0)
                            break;

                        string messageJson = Encoding.UTF8.GetString(buffer, 0, byteCount);
                        Console.WriteLine($"Received from {clientId}: {messageJson}");
                        ProcessMessage(clientId, messageJson);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error with client {clientId}: {ex.Message}");
                }
            }

            _clients.TryRemove(clientId, out _);
            Console.WriteLine($"Client disconnected: {clientId}");
        }

        private void ProcessMessage(string clientId, string messageJson)
        {
            var msg = JsonSerializer.Deserialize<GameMessage>(messageJson);
            if (msg == null)
                return;

            switch (msg.Type)
            {
                case "JoinGame":
                    HandleJoinGame(clientId, msg.Data);
                    break;
                case "RollDice":
                    HandleRollDice(clientId, msg.Data);
                    break;
                default:
                    Console.WriteLine($"Unknown message type: {msg.Type}");
                    break;
            }
        }

        private void HandleJoinGame(string clientId, JsonElement data)
        {
            string playerName = data.GetProperty("Name").GetString();
            var player = new Player { Id = clientId, Name = playerName, Position = 0, Money = 1500 };
            _gameState.Players.Add(player);
            Console.WriteLine($"{playerName} joined the game.");
            BroadcastGameState();
        }

        private void HandleRollDice(string clientId, JsonElement data)
        {
            Random rnd = new Random();
            int diceRoll = rnd.Next(1, 7) + rnd.Next(1, 7);
            var player = _gameState.Players.Find(p => p.Id == clientId);
            if (player != null)
            {
                player.Position = (player.Position + diceRoll) % 40;
                Console.WriteLine($"{player.Name} rolled {diceRoll} and moved to {player.Position}");
            }
            BroadcastGameState();
        }

        private void BroadcastGameState()
        {
            var gameStateMsg = new GameMessage
            {
                Type = "GameStateUpdate",
                Data = JsonSerializer.SerializeToElement(_gameState)
            };

            string json = JsonSerializer.Serialize(gameStateMsg);
            byte[] data = Encoding.UTF8.GetBytes(json);

            foreach (var kvp in _clients)
            {
                var client = kvp.Value;
                if (client.Connected)
                {
                    try
                    {
                        var stream = client.GetStream();
                        stream.WriteAsync(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending to client {kvp.Key}: {ex.Message}");
                    }
                }
            }
        }
    }
}
