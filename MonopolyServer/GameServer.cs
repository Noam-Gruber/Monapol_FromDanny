using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using MonopolyCommon;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MonopolyServer
{
    public class GameServer
    {
        private readonly TcpListener _listener;
        private readonly ConcurrentDictionary<string, TcpClient> _clients = new();
        private readonly GameState _gameState = new();
        private readonly Board _board = new Board();

        private bool _isGameStarted = false;
        private HashSet<string> _playersReady = new HashSet<string>();
        private CardManager _cardManager = new CardManager();

        public GameServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        private async void ProcessMessage(string clientId, string messageJson)
        {
            var msg = JsonSerializer.Deserialize<GameMessage>(messageJson);
            if (msg == null) return;

            switch (msg.Type)
            {
                case "JoinGame":
                    await HandleJoinGame(clientId, msg.Data);
                    break;
                case "StartGame":
                    HandleStartGame(clientId);
                    break;
                case "RollDice":
                    await HandleRollDice(clientId);
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
                _ = BroadcastGameState();
            }
            else
            {
                Console.WriteLine($"{_playersReady.Count}/{_gameState.Players.Count} players are ready.");
            }
        }

        private async Task HandleRollDice(string clientId)
        {
            if (!_isGameStarted) return;

            var currentPlayer = _gameState.Players[_gameState.CurrentPlayerIndex];
            if (currentPlayer.Id != clientId) return;

            Random rnd = new Random();
            int diceRoll = rnd.Next(1, 7) + rnd.Next(1, 7);
            currentPlayer.Position = (currentPlayer.Position + diceRoll) % 40;
            currentPlayer.CurrentProperty = _board.Spaces[currentPlayer.Position].Name;

            var space = _board.Spaces[currentPlayer.Position];

            _board.UpdatePlayerPosition(clientId, currentPlayer.Position);
            string log = $"{currentPlayer.Name} rolled {diceRoll} and moved to {currentPlayer.Position}- {currentPlayer.CurrentProperty}";
            Console.WriteLine(log);
            await BroadcastLogMessageAsync(log);

            if (space.IsChance)
            {
                var card = _cardManager.DrawChanceCard();
                card.ApplyEffect(currentPlayer, _gameState);
                Console.WriteLine($"Chance Card: {card.Description}");
            }
            else if (space.IsCommunityChest)
            {
                var card = _cardManager.DrawCommunityChestCard();
                card.ApplyEffect(currentPlayer, _gameState);
                Console.WriteLine($"Community Chest Card: {card.Description}");
            }
            else if (space.IsOwned && space.OwnedByPlayerId != clientId)
            {
                var owner = _gameState.Players.First(p => p.Id == space.OwnedByPlayerId);
                var rentMessage = new GameMessage
                {
                    Type = "ShowRentForm",
                    Data = JsonSerializer.SerializeToElement(new { Property = space, OwnerName = owner.Name })
                };
                Console.WriteLine("Sending ShowRentForm to " + clientId);
                await SendMessageAsync(clientId, rentMessage);
            }
            else if (!space.IsOwned && currentPlayer.Money >= space.PurchasePrice)
            {
                var buyMessage = new GameMessage
                {
                    Type = "ShowBuyForm",
                    Data = JsonSerializer.SerializeToElement(space)
                };
                Console.WriteLine("Sending ShowBuyForm to " + clientId);
                await SendMessageAsync(clientId, buyMessage);
                return; // חשוב: לא להתקדם לתור הבא עד שהשחקן יבחר
            }

            // תור עובר רק אם אין צורך בהצגת טופס
            _gameState.CurrentPlayerIndex = (_gameState.CurrentPlayerIndex + 1) % _gameState.Players.Count;
            Console.WriteLine($"Next turn: {_gameState.Players[_gameState.CurrentPlayerIndex].Name}");

            await BroadcastGameState();
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
                if (!player.OwnedProperties.Contains(space.Name))
                    player.OwnedProperties.Add(space.Name);

                Console.WriteLine($"{player.Name} bought {space.Name} for ${space.PurchasePrice}");
            }
            else
            {
                Console.WriteLine($"{player.Name} can't buy {propertyName}");
            }

            _gameState.CurrentPlayerIndex = (_gameState.CurrentPlayerIndex + 1) % _gameState.Players.Count;
            _ = BroadcastGameState();
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
            }

            _gameState.CurrentPlayerIndex = (_gameState.CurrentPlayerIndex + 1) % _gameState.Players.Count;
            _ = BroadcastGameState();
        }

        private void HandleEndGame(string clientId)
        {
            if (!_isGameStarted) return;

            _isGameStarted = false;
            var winner = _gameState.Players.OrderByDescending(p => p.Money).FirstOrDefault();
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
            var player = new Player { Id = clientId, Name = playerName, Position = 0, CurrentProperty = _board.Spaces[0].Name };
            _gameState.Players.Add(player);

            var joinSuccessMsg = new GameMessage
            {
                Type = "JoinGameSuccess",
                Data = JsonSerializer.SerializeToElement(player)
            };
            Console.WriteLine("Sending JoinGameSuccess to " + clientId);
            await SendMessageAsync(clientId, joinSuccessMsg);
            await BroadcastGameState();
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            string clientId = Guid.NewGuid().ToString();
            _clients.TryAdd(clientId, client);
            Console.WriteLine($"Client connected: {clientId}");

            using var stream = client.GetStream();
            var reader = new BinaryReader(stream, Encoding.UTF8);
            try
            {
                while (true)
                {
                    byte[] lengthBytes = new byte[4];
                    int readLength = await stream.ReadAsync(lengthBytes, 0, 4);
                    if (readLength == 0) break;

                    int messageLength = BitConverter.ToInt32(lengthBytes, 0);
                    if (messageLength <= 0) continue;

                    byte[] messageBuffer = new byte[messageLength];
                    int totalRead = 0;
                    while (totalRead < messageLength)
                    {
                        int read = await stream.ReadAsync(messageBuffer, totalRead, messageLength - totalRead);
                        if (read == 0) break;
                        totalRead += read;
                    }

                    string messageJson = Encoding.UTF8.GetString(messageBuffer);
                    ProcessMessage(clientId, messageJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with client {clientId}: {ex.Message}");
            }

            _clients.TryRemove(clientId, out _);
            _playersReady.Remove(clientId);
            Console.WriteLine($"Client disconnected: {clientId}");
        }

        private async Task SendMessageAsync(string clientId, GameMessage message)
        {
            string json = JsonSerializer.Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

            if (_clients.TryGetValue(clientId, out var client) && client.Connected)
            {
                var stream = client.GetStream();
                await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                await stream.WriteAsync(data, 0, data.Length);
            }
        }

        private async Task BroadcastGameState()
        {
            var gameStateMsg = new GameMessage
            {
                Type = "GameStateUpdate",
                Data = JsonSerializer.SerializeToElement(_gameState)
            };
            await BroadcastMessageAsync(gameStateMsg);
            Console.WriteLine($"Sent updated game state. Current turn: {_gameState.Players[_gameState.CurrentPlayerIndex].Name}");
        }

        private async void BroadcastEndGame(Player winner)
        {
            var endGameMessage = new GameMessage
            {
                Type = "GameEnded",
                Data = JsonSerializer.SerializeToElement(new { WinnerId = winner.Id, WinnerName = winner.Name, WinnerMoney = winner.Money })
            };
            await BroadcastMessageAsync(endGameMessage);
            Console.WriteLine($"Game ended! Winner is {winner.Name}");
        }

        private async Task BroadcastMessageAsync(GameMessage message)
        {
            string json = JsonSerializer.Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

            foreach (var kvp in _clients)
            {
                var client = kvp.Value;
                if (client.Connected)
                {
                    try
                    {
                        var stream = client.GetStream();
                        await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                        await stream.WriteAsync(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending to client {kvp.Key}: {ex.Message}");
                    }
                }
            }
        }

        private async Task SendLogMessageAsync(string clientId, string logText)
        {
            var logMessage = new GameMessage
            {
                Type = "ServerLog",
                Data = JsonSerializer.SerializeToElement(new { Text = logText })
            };
            await SendMessageAsync(clientId, logMessage);
        }

        private async Task BroadcastLogMessageAsync(string text)
        {
            var logMessage = new GameMessage
            {
                Type = "ServerLog",
                Data = JsonSerializer.SerializeToElement(new { Text = text })
            };
            await BroadcastMessageAsync(logMessage);
        }

        public void Stop()
        {
            Console.WriteLine("Stopping server...");
            _listener.Stop();
            foreach (var client in _clients.Values) client.Close();
            Console.WriteLine("Server stopped.");
        }
    }
}
