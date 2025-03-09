using System.Net;
using System.Text;
using MonopolyCommon;
using System.Text.Json;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace MonopolyServer
{
    public class GameServer
    {
        private readonly TcpListener _listener;
        private readonly ConcurrentDictionary<string, TcpClient> _clients = new();
        private readonly GameState _gameState = new();
        private readonly Board _board = new Board();

        private bool _isGameStarted = false;
        private HashSet<string> _playersReady = new HashSet<string>(); // שחקנים שמוכנים להתחיל

        public GameServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        private void ProcessMessage(string clientId, string messageJson)
        {
            var msg = JsonSerializer.Deserialize<GameMessage>(messageJson);
            if (msg == null) return;

            switch (msg.Type)
            {
                case "JoinGame":
                    HandleJoinGame(clientId, msg.Data);
                    break;
                case "StartGame":
                    HandleStartGame(clientId);
                    break;
                case "RollDice":
                    HandleRollDice(clientId);
                    break;
                default:
                    Console.WriteLine($"Unknown message type: {msg.Type}");
                    break;
            }
        }

        private void HandleStartGame(string clientId)
        {
            if (_isGameStarted)
            {
                Console.WriteLine("The game has already started.");
                return;
            }

            _playersReady.Add(clientId);

            if (_playersReady.Count == _gameState.Players.Count)
            {
                _isGameStarted = true;
                _gameState.CurrentPlayerIndex = 0;
                Console.WriteLine("Game started!");
                BroadcastGameState();
            }
            else
            {
                Console.WriteLine($"{_playersReady.Count}/{_gameState.Players.Count} players are ready to start the game.");
            }
        }

        private void HandleRollDice(string clientId)
        {
            if (!_isGameStarted)
            {
                Console.WriteLine("The game hasn't started yet.");
                return;
            }

            var currentPlayer = _gameState.Players[_gameState.CurrentPlayerIndex];
            if (currentPlayer.Id != clientId)
            {
                Console.WriteLine("It's not your turn.");
                return;
            }

            Random rnd = new Random();
            int diceRoll = rnd.Next(1, 7) + rnd.Next(1, 7);
            currentPlayer.Position = (currentPlayer.Position + diceRoll) % 40;

            // עדכון מיקום השחקן בלוח
            _board.UpdatePlayerPosition(clientId, currentPlayer.Position);
            Console.WriteLine($"{currentPlayer.Name} rolled {diceRoll} and moved to {currentPlayer.Position}");

            _gameState.CurrentPlayerIndex = (_gameState.CurrentPlayerIndex + 1) % _gameState.Players.Count;
            BroadcastGameState();
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

        private async Task HandleJoinGame(string clientId, JsonElement data)
        {
            string playerName = data.GetProperty("Name").GetString();
            var player = new Player { Id = clientId, Name = playerName, Position = 0 };
            _gameState.Players.Add(player);

            // שליחת הודעה לשחקן שהצטרף עם ה-Id שלו
            var joinSuccessMsg = new GameMessage
            {
                Type = "JoinGameSuccess",
                Data = JsonSerializer.SerializeToElement(new { PlayerId = clientId })
            };

            string json = JsonSerializer.Serialize(joinSuccessMsg);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            await _clients[clientId].GetStream().WriteAsync(bytes, 0, bytes.Length);

            await BroadcastGameState();
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            string clientId = Guid.NewGuid().ToString();
            _clients.TryAdd(clientId, client);
            Console.WriteLine($"Client connected: {clientId}");

            using var stream = client.GetStream();
            byte[] buffer = new byte[4096];

            try
            {
                while (true)
                {
                    int byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (byteCount == 0) break;

                    string messageJson = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    ProcessMessage(clientId, messageJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with client {clientId}: {ex.Message}");
            }

            _clients.TryRemove(clientId, out _);
            _playersReady.Remove(clientId); // השחקן התנתק
            Console.WriteLine($"Client disconnected: {clientId}");
        }

        private async Task BroadcastGameState()
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
                        await stream.WriteAsync(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending to client {kvp.Key}: {ex.Message}");
                    }
                }
            }
        }

        // פונקציה בשרת ששולחת עדכון ללקוח על המיקום החדש של השחקן
        private void BroadcastPlayerPosition(string playerId)
        {
            string positionInfo = _board.GetPlayerPositionDisplay(playerId);
            var message = new GameMessage
            {
                Type = "PlayerPositionUpdate",
                Data = JsonSerializer.SerializeToElement(new { PlayerId = playerId, Position = positionInfo })
            };

            string json = JsonSerializer.Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json);

            // שליחה לכל הלקוחות
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

        private void BroadcastPlayerList()
        {
            var gameMessage = new GameMessage
            {
                Type = "PlayerListUpdate",
                Data = JsonSerializer.SerializeToElement(_gameState.Players)
            };

            string json = JsonSerializer.Serialize(gameMessage);
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
