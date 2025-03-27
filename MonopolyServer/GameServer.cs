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
        private readonly ConcurrentDictionary<string, SslStream> _sslStreams = new();
        private readonly ConcurrentDictionary<string, GameSession> _games = new();

        private readonly X509Certificate2 _serverCertificate;
        private readonly string _certPath = Params.GetCertPath();
        private readonly string _password = Params.GetPassword();

        /// <summary>
        /// Initializes a new instance of the <see cref="GameServer"/> class.
        /// </summary>
        /// <param name="port">The port number on which the server listens for incoming connections.</param>
        public GameServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            string basePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", ".."));
            string certPath = Path.Combine(basePath, _certPath);
            _serverCertificate = new X509Certificate2(certPath, _password);
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
        /// Handles an incoming client connection asynchronously.
        /// </summary>
        /// <param name="client">The TCP client to handle.</param>
        private async Task HandleClientAsync(TcpClient client)
        {
            string clientId = Guid.NewGuid().ToString();
            _clients[clientId] = client;

            var stream = client.GetStream();
            var sslStream = new SslStream(stream, false);

            try
            {
                await sslStream.AuthenticateAsServerAsync(_serverCertificate, false, false);
                _sslStreams[clientId] = sslStream;

                while (true)
                {
                    byte[] lengthBytes = new byte[4];
                    int readLength = await sslStream.ReadAsync(lengthBytes, 0, 4);
                    if (readLength == 0) break;

                    int messageLength = BitConverter.ToInt32(lengthBytes, 0);
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
                Console.WriteLine($"Client error: {ex.Message}");
            }

            _clients.TryRemove(clientId, out _);
            _sslStreams.TryRemove(clientId, out _);
            Console.WriteLine($"Client disconnected: {clientId}");
        }

        /// <summary>
        /// Processes a message received from a client.
        /// </summary>
        /// <param name="clientId">The ID of the client that sent the message.</param>
        /// <param name="messageJson">The JSON string representing the message.</param>
        private void ProcessMessage(string clientId, string messageJson)
        {
            GameMessage msg;
            try
            {
                msg = JsonSerializer.Deserialize<GameMessage>(messageJson);
                if (msg == null) return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to deserialize message: {ex.Message}");
                return;
            }

            var session = _games.GetOrAdd(msg.GameId, _ => new GameSession());

            switch (msg.Type)
            {
                case "JoinGame":
                    HandleJoinGame(session, clientId, msg.Data);
                    break;
                case "StartGame":
                    HandleStartGame(session, clientId);
                    break;
                case "RollDice":
                    HandleRollDice(session, clientId);
                    break;
                case "BuyProperty":
                    HandleBuyProperty(session, clientId, msg.Data);
                    break;
                case "PayRent":
                    HandlePayRent(session, clientId, msg.Data);
                    break;
                case "EndGame":
                    HandleEndGame(session);
                    break;
            }
        }

        /// <summary>
        /// Handles a request to join a game.
        /// </summary>
        /// <param name="session">The game session.</param>
        /// <param name="clientId">The ID of the client requesting to join.</param>
        /// <param name="data">The data associated with the join request.</param>
        private void HandleJoinGame(GameSession session, string clientId, JsonElement data)
        {
            string playerName = data.GetProperty("Name").GetString();
            var player = new Player { Id = clientId, Name = playerName, Position = 0, CurrentProperty = session.Board.Spaces[0].Name };
            session.GameState.Players.Add(player);

            if (string.IsNullOrEmpty(session.GameState.GameId))
            {
                session.GameState.GameId = Guid.NewGuid().ToString();
                _games[session.GameState.GameId] = session;
            }

            SendToClient(clientId, new GameMessage
            {
                Type = "JoinGameSuccess",
                GameId = session.GameState.GameId,
                Data = JsonSerializer.SerializeToElement(player)
            });

            BroadcastGameState(session);
        }

        /// <summary>
        /// Handles a request to start a game.
        /// </summary>
        /// <param name="session">The game session.</param>
        /// <param name="clientId">The ID of the client requesting to start the game.</param>
        private void HandleStartGame(GameSession session, string clientId)
        {
            session.PlayersReady.Add(clientId);
            if (session.PlayersReady.Count == session.GameState.Players.Count)
            {
                session.IsGameStarted = true;
                session.GameState.CurrentPlayerIndex = 0;
                BroadcastGameState(session);
            }
        }

        /// <summary>
        /// Handles a request to roll the dice.
        /// </summary>
        /// <param name="session">The game session.</param>
        /// <param name="clientId">The ID of the client requesting to roll the dice.</param>
        private void HandleRollDice(GameSession session, string clientId)
        {
            if (!session.IsGameStarted) return;

            var currentPlayer = session.GameState.Players[session.GameState.CurrentPlayerIndex];
            if (currentPlayer.Id != clientId) return;

            Random rnd = new Random();
            int diceRoll = rnd.Next(1, 7) + rnd.Next(1, 7);

            SendToClient(clientId, new GameMessage
            {
                Type = "DiceRolled",
                Data = JsonSerializer.SerializeToElement(new { Value = diceRoll })
            });

            currentPlayer.Position = (currentPlayer.Position + diceRoll) % 40;
            currentPlayer.CurrentProperty = session.Board.Spaces[currentPlayer.Position].Name;
            session.Board.UpdatePlayerPosition(clientId, currentPlayer.Position);

            BroadcastLog(session, $"{currentPlayer.Name} rolled {diceRoll} and moved to {currentPlayer.CurrentProperty}");

            var space = session.Board.Spaces[currentPlayer.Position];
            if (space.IsChance)
            {
                var card = session.CardManager.DrawChanceCard();
                card.ApplyEffect(currentPlayer, session.GameState);
            }
            else if (space.IsCommunityChest)
            {
                var card = session.CardManager.DrawCommunityChestCard();
                card.ApplyEffect(currentPlayer, session.GameState);
            }
            else if (space.IsOwned && space.OwnedByPlayerId != clientId)
            {
                var owner = session.GameState.Players.First(p => p.Id == space.OwnedByPlayerId);
                SendToClient(clientId, new GameMessage
                {
                    Type = "ShowRentForm",
                    Data = JsonSerializer.SerializeToElement(new { Property = space, OwnerName = owner.Name })
                });
            }
            else if (!space.IsOwned && currentPlayer.Money >= space.PurchasePrice)
            {
                SendToClient(clientId, new GameMessage
                {
                    Type = "ShowBuyForm",
                    Data = JsonSerializer.SerializeToElement(space)
                });
                return;
            }

            session.GameState.CurrentPlayerIndex = (session.GameState.CurrentPlayerIndex + 1) % session.GameState.Players.Count;
            BroadcastGameState(session);
        }

        /// <summary>
        /// Handles a request to buy a property.
        /// </summary>
        /// <param name="session">The game session.</param>
        /// <param name="clientId">The ID of the client requesting to buy the property.</param>
        /// <param name="data">The data associated with the buy request.</param>
        private void HandleBuyProperty(GameSession session, string clientId, JsonElement data)
        {
            string propertyName = data.GetProperty("PropertyName").GetString();
            var space = session.Board.Spaces.FirstOrDefault(s => s.Name == propertyName);
            var player = session.GameState.Players.First(p => p.Id == clientId);

            if (space != null && !space.IsOwned && player.Money >= space.PurchasePrice)
            {
                player.Money -= space.PurchasePrice;
                space.OwnedByPlayerId = clientId;
                player.OwnedProperties.Add(space.Name);
                BroadcastLog(session, $"{player.Name} bought {space.Name} for ${space.PurchasePrice}");
            }

            session.GameState.CurrentPlayerIndex = (session.GameState.CurrentPlayerIndex + 1) % session.GameState.Players.Count;
            BroadcastGameState(session);
        }

        /// <summary>
        /// Handles a request to pay rent.
        /// </summary>
        /// <param name="session">The game session.</param>
        /// <param name="clientId">The ID of the client requesting to pay rent.</param>
        /// <param name="data">The data associated with the rent payment request.</param>
        private void HandlePayRent(GameSession session, string clientId, JsonElement data)
        {
            string propertyName = data.GetProperty("PropertyName").GetString();
            int rentPrice = data.GetProperty("RentPrice").GetInt32();

            var player = session.GameState.Players.First(p => p.Id == clientId);
            var space = session.Board.Spaces.First(s => s.Name == propertyName);
            var owner = session.GameState.Players.First(p => p.Id == space.OwnedByPlayerId);

            player.Money -= rentPrice;
            owner.Money += rentPrice;

            BroadcastLog(session, $"{player.Name} paid ${rentPrice} to {owner.Name} for {space.Name}");

            session.GameState.CurrentPlayerIndex = (session.GameState.CurrentPlayerIndex + 1) % session.GameState.Players.Count;
            BroadcastGameState(session);
        }

        /// <summary>
        /// Handles a request to end the game.
        /// </summary>
        /// <param name="session">The game session.</param>
        private void HandleEndGame(GameSession session)
        {
            session.IsGameStarted = false;
            var winner = session.GameState.Players.OrderByDescending(p => p.Money).First();
            BroadcastMessage(session, new GameMessage
            {
                Type = "GameEnded",
                Data = JsonSerializer.SerializeToElement(new { WinnerId = winner.Id, WinnerName = winner.Name, WinnerMoney = winner.Money })
            });
        }

        /// <summary>
        /// Sends a message to a specific client.
        /// </summary>
        /// <param name="clientId">The ID of the client to send the message to.</param>
        /// <param name="message">The message to send.</param>
        private void SendToClient(string clientId, GameMessage message)
        {
            if (_sslStreams.TryGetValue(clientId, out var sslStream))
            {
                string json = JsonSerializer.Serialize(message);
                byte[] data = Encoding.UTF8.GetBytes(json);
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
                sslStream.Write(lengthPrefix, 0, 4);
                sslStream.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Broadcasts the current game state to all clients in the session.
        /// </summary>
        /// <param name="session">The game session.</param>
        private void BroadcastGameState(GameSession session)
        {
            BroadcastMessage(session, new GameMessage
            {
                Type = "GameStateUpdate",
                Data = JsonSerializer.SerializeToElement(session.GameState)
            });
        }

        /// <summary>
        /// Broadcasts a log message to all clients in the session.
        /// </summary>
        /// <param name="session">The game session.</param>
        /// <param name="text">The log message text.</param>
        private void BroadcastLog(GameSession session, string text)
        {
            BroadcastMessage(session, new GameMessage
            {
                Type = "ServerLog",
                Data = JsonSerializer.SerializeToElement(new { Text = text })
            });
        }

        /// <summary>
        /// Broadcasts a message to all clients in the session.
        /// </summary>
        /// <param name="session">The game session.</param>
        /// <param name="message">The message to broadcast.</param>
        private void BroadcastMessage(GameSession session, GameMessage message)
        {
            string json = JsonSerializer.Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

            foreach (var player in session.GameState.Players)
            {
                if (_sslStreams.TryGetValue(player.Id, out var sslStream))
                {
                    sslStream.Write(lengthPrefix, 0, 4);
                    sslStream.Write(data, 0, data.Length);
                }
            }
        }

        /// <summary>
        /// Stops the game server.
        /// </summary>
        public void Stop()
        {
            Console.WriteLine("Stopping server...");
            _listener.Stop();
            foreach (var sslStream in _sslStreams.Values) sslStream.Close();
            foreach (var client in _clients.Values) client.Close();
            Console.WriteLine("Server stopped.");
        }

        /// <summary>
        /// Represents a game session.
        /// </summary>
        private class GameSession
        {
            public GameState GameState = new();
            public HashSet<string> PlayersReady = new();
            public bool IsGameStarted = false;
            public CardManager CardManager = new();
            public Board Board = new();
        }
    }
}
