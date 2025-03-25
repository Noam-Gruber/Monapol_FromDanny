using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using MonopolyCommon;
using System.Text.Json;
using System.Net.Sockets;
using System.Net.Security;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;

namespace MonopolyServer
{
    /// <summary>
    /// Represents the game server for the Monopoly game.
    /// </summary>
    public class GameServer
    {
        private readonly TcpListener _listener;
        private readonly ConcurrentDictionary<string, TcpClient> _clients = new();
        private readonly GameState _gameState = new();
        private readonly Board _board = new Board();
        private const string _certPath = "cert.pfx";
        private const string _password = "aviel";

        private bool _isGameStarted = false;
        private HashSet<string> _playersReady = new HashSet<string>();
        private CardManager _cardManager = new CardManager();
        private X509Certificate2 _serverCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameServer"/> class.
        /// </summary>
        /// <param name="port">The port number to listen on.</param>
        public GameServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            string basePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\.."));
            string certPath = Path.Combine(basePath, _certPath);

            _serverCertificate = new X509Certificate2(certPath, _password);
        }

        /// <summary>
        /// Processes a message received from a client.
        /// </summary>
        /// <param name="clientId">The ID of the client that sent the message.</param>
        /// <param name="messageJson">The message in JSON format.</param>
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

        /// <summary>
        /// Handles the start game request from a client.
        /// </summary>
        /// <param name="clientId">The ID of the client that requested to start the game.</param>
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

        /// <summary>
        /// Handles the roll dice request from a client.
        /// </summary>
        /// <param name="clientId">The ID of the client that requested to roll the dice.</param>
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
            await BroadcastLogMessageAsync($"{currentPlayer.Name} rolled {diceRoll} and moved to {currentPlayer.Position}- {currentPlayer.CurrentProperty}");

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

        /// <summary>
        /// Handles the buy property request from a client.
        /// </summary>
        /// <param name="clientId">The ID of the client that requested to buy a property.</param>
        /// <param name="data">The data containing the property information.</param>
        private async void HandleBuyProperty(string clientId, JsonElement data)
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

                await BroadcastLogMessageAsync($"{player.Name} bought {space.Name} for ${space.PurchasePrice}");
            }
            else
            {
                Console.WriteLine($"{player.Name} can't buy {propertyName}");
            }

            _gameState.CurrentPlayerIndex = (_gameState.CurrentPlayerIndex + 1) % _gameState.Players.Count;
            _ = BroadcastGameState();
        }

        /// <summary>
        /// Handles the pay rent request from a client.
        /// </summary>
        /// <param name="clientId">The ID of the client that requested to pay rent.</param>
        /// <param name="data">The data containing the rent information.</param>
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

        /// <summary>
        /// Handles the end game request from a client.
        /// </summary>
        /// <param name="clientId">The ID of the client that requested to end the game.</param>
        private void HandleEndGame(string clientId)
        {
            if (!_isGameStarted) return;

            _isGameStarted = false;
            var winner = _gameState.Players.OrderByDescending(p => p.Money).FirstOrDefault();
            BroadcastEndGame(winner);
        }

        /// <summary>
        /// Starts the game server asynchronously.
        /// </summary>
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

        /// <summary>
        /// Handles the join game request from a client.
        /// </summary>
        /// <param name="clientId">The ID of the client that requested to join the game.</param>
        /// <param name="data">The data containing the player information.</param>
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

        /// <summary>
        /// Handles a client connection asynchronously.
        /// </summary>
        /// <param name="client">The client to handle.</param>
        private async Task HandleClientAsync(TcpClient client)
        {
            string clientId = Guid.NewGuid().ToString();
            _clients.TryAdd(clientId, client);
            Console.WriteLine($"Client connected: {clientId}");

            var stream = client.GetStream();
            var sslStream = new SslStream(stream, false);
            try
            {
                await sslStream.AuthenticateAsServerAsync(_serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: false);
                _sslStreams.TryAdd(clientId, sslStream);

                while (true)
                {
                    byte[] lengthBytes = new byte[4];
                    int readLength = await sslStream.ReadAsync(lengthBytes, 0, 4);
                    if (readLength == 0) break;

                    int messageLength = BitConverter.ToInt32(lengthBytes, 0);
                    if (messageLength <= 0) continue;

                    byte[] messageBuffer = new byte[messageLength];
                    int totalRead = 0;
                    while (totalRead < messageLength)
                    {
                        int read = await sslStream.ReadAsync(messageBuffer, totalRead, messageLength - totalRead);
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

        private readonly ConcurrentDictionary<string, SslStream> _sslStreams = new();

        /// <summary>
        /// Sends a message to a specific client asynchronously.
        /// </summary>
        /// <param name="clientId">The ID of the client to send the message to.</param>
        /// <param name="message">The message to send.</param>
        private async Task SendMessageAsync(string clientId, GameMessage message)
        {
            string json = JsonSerializer.Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

            if (_clients.TryGetValue(clientId, out var client) && _sslStreams.TryGetValue(clientId, out var sslStream) && client.Connected)
            {
                await sslStream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                await sslStream.WriteAsync(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Broadcasts the current game state to all clients asynchronously.
        /// </summary>
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

        /// <summary>
        /// Broadcasts the end game message to all clients asynchronously.
        /// </summary>
        /// <param name="winner">The player who won the game.</param>
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

        /// <summary>
        /// Broadcasts a message to all clients asynchronously.
        /// </summary>
        /// <param name="message">The message to broadcast.</param>
        private async Task BroadcastMessageAsync(GameMessage message)
        {
            string json = JsonSerializer.Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

            foreach (var kvp in _clients)
            {
                if (_sslStreams.TryGetValue(kvp.Key, out var sslStream))
                {
                    try
                    {
                        await sslStream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                        await sslStream.WriteAsync(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending to client {kvp.Key}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Broadcasts a log message to all clients asynchronously.
        /// </summary>
        /// <param name="text">The log message text.</param>
        private async Task BroadcastLogMessageAsync(string text)
        {
            var logMessage = new GameMessage
            {
                Type = "ServerLog",
                Data = JsonSerializer.SerializeToElement(new { Text = text })
            };
            Console.WriteLine(text);
            await BroadcastMessageAsync(logMessage);
        }

        /// <summary>
        /// Stops the game server.
        /// </summary>
        public void Stop()
        {
            Console.WriteLine("Stopping server...");
            _listener.Stop();
            foreach (var sslStreams in _sslStreams.Values) sslStreams.Close();
            foreach (var client in _clients.Values) client.Close();
            Console.WriteLine("Server stopped.");
        }
    }
}
