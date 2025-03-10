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
                case "BuyProperty":
                    HandleBuyProperty(clientId, msg.Data);
                    break;
                case "PayRent":
                    HandlePayRent(clientId, msg.Data);
                    break;
                case "EndGame":
                    HandleEndGame(clientId);
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

            if (_playersReady.Count == _gameState.Players.Count || _gameState.Players.Count == 1)
            {
                _isGameStarted = true;
                _gameState.CurrentPlayerIndex = 0;
                Console.WriteLine("Game started immediately!");
                BroadcastGameState();
            }
            else
            {
                Console.WriteLine($"{_playersReady.Count}/{_gameState.Players.Count} players are ready.");
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
            currentPlayer.CurrentProperty = _board.Spaces[currentPlayer.Position].Name;

            var space = _board.Spaces[currentPlayer.Position];
            // בדיקה אם צריך לשלם שכירות
            if (space.IsOwned && space.OwnedByPlayerId != clientId)
            {
                var owner = _gameState.Players.First(p => p.Id == space.OwnedByPlayerId);
                currentPlayer.Money -= space.RentPrice;
                owner.Money += space.RentPrice;

                Console.WriteLine($"{currentPlayer.Name} paid ${space.RentPrice} to {owner.Name} for landing on {space.Name}.");
                BroadcastGameState();
            }

            // עדכון מיקום השחקן בלוח
            _board.UpdatePlayerPosition(clientId, currentPlayer.Position);
            Console.WriteLine($"{currentPlayer.Name} rolled {diceRoll} and moved to {currentPlayer.Position}");

            _gameState.CurrentPlayerIndex = (_gameState.CurrentPlayerIndex + 1) % _gameState.Players.Count;
            BroadcastGameState();
        }

        private void HandleBuyProperty(string clientId, JsonElement data)
        {
            string propertyName = data.GetProperty("PropertyName").GetString();
            var space = _board.Spaces.FirstOrDefault(s => s.Name == propertyName);
            var player = _gameState.Players.First(p => p.Id == clientId);

            if (space != null && !space.IsOwned && player.Money >= space.PurchasePrice)
            {
                player.Money -= space.PurchasePrice;
                space.OwnedByPlayerId = clientId;
                player.OwnedProperties.Add(space.Name);

                // ודא שהנכס באמת מתווסף לרשימת הנכסים של השחקן
                if (!player.OwnedProperties.Contains(space.Name))
                    player.OwnedProperties.Add(space.Name);

                foreach (var prop in player.OwnedProperties)
                {
                    Console.WriteLine($"DEBUG: {player.Name} owns {prop}");
                }

                Console.WriteLine($"{player.Name} bought {space.Name} for ${space.PurchasePrice}");

                BroadcastGameState();
            }
            else
            {
                Console.WriteLine($"{player.Name} can't buy {propertyName}");
            }
        }

        private void HandlePayRent(string clientId, JsonElement data)
        {
            string propertyName = data.GetProperty("PropertyName").GetString();
            int rentPrice = data.GetProperty("RentPrice").GetInt32();

            var player = _gameState.Players.First(p => p.Id == clientId);
            var space = _board.Spaces.First(s => s.Name == propertyName);

            if (space != null && space.IsOwned && space.OwnedByPlayerId != clientId)
            {
                player.Money -= rentPrice;

                var owner = _gameState.Players.First(p => p.Id == space.OwnedByPlayerId);
                owner.Money += rentPrice;

                Console.WriteLine($"{player.Name} paid rent ${rentPrice} to {owner.Name} for {space.Name}");

                BroadcastGameState();
            }
        }

        private void HandleEndGame(string clientId)
        {
            if (!_isGameStarted)
            {
                Console.WriteLine("Game hasn't started yet.");
                return;
            }

            _isGameStarted = false;

            // חישוב המנצח (לדוגמה, השחקן עם הכי הרבה כסף)
            var winner = _gameState.Players.OrderByDescending(p => p.Money).FirstOrDefault();

            // שליחת הודעת סיום המשחק לכל הלקוחות
            BroadcastEndGame(winner);
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
            var player = new Player { Id = clientId, Name = playerName, Position = 0 , CurrentProperty = _board.Spaces[0].Name };
            _gameState.Players.Add(player);

            // שליחת הודעה לשחקן שהצטרף עם ה-Id שלו ושמו
            var joinSuccessMsg = new GameMessage
            {
                Type = "JoinGameSuccess",
                Data = JsonSerializer.SerializeToElement(player) // לשלוח את כל האובייקט של השחקן
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

        private async void BroadcastEndGame(Player winner)
        {
            var endGameMessage = new GameMessage
            {
                Type = "GameEnded",
                Data = JsonSerializer.SerializeToElement(new
                {
                    WinnerId = winner.Id,
                    WinnerName = winner.Name,
                    WinnerMoney = winner.Money
                })
            };

            string json = JsonSerializer.Serialize(endGameMessage);
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

            Console.WriteLine($"Game ended! Winner is {winner.Name}");
        }

        public void Stop()
        {
            Console.WriteLine("Stopping server...");

            _listener.Stop(); // עוצר את ה-listener מלקבל לקוחות חדשים
            foreach (var client in _clients.Values)
            {
                client.Close(); // סגירת כל החיבורים הפעילים
            }

            Console.WriteLine("Server stopped.");
        }

    }
}
