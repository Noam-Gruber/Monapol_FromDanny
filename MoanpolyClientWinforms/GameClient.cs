using System;
using System.Linq;
using System.Text;
using MonopolyServer;
using MonopolyCommon;
using System.Text.Json;
using System.Net.Sockets;
using System.Net.Security;
using System.Windows.Forms;
using MonapolClientUI.Forms;
using System.Threading.Tasks;
using MoanpolyClientWinforms;
using System.Collections.Generic;

namespace MonopolyClient
{
    /// <summary>
    /// Represents the game client for the Monopoly game.
    /// </summary>
    public class GameClient
    {
        private TcpClient _client;
        private string _myPlayerId;
        private HashSet<string> _buyFormShownForProperties = new();
        private SslStream _sslStream;

        /// <summary>
        /// Gets the player ID of the current player.
        /// </summary>
        public string MyPlayerId => _myPlayerId;

        /// <summary>
        /// Gets the list of players in the game.
        /// </summary>
        public List<Player> Players { get; private set; } = new();

        /// <summary>
        /// Gets the list of board spaces in the game.
        /// </summary>
        public List<BoardSpace> BoardSpaces { get; private set; }

        /// <summary>
        /// Occurs when a message is received from the server.
        /// </summary>
        public event Action<string> MessageReceived;

        /// <summary>
        /// Occurs when the turn status of the current player is updated.
        /// </summary>
        public event Action<bool> MyTurnUpdated;

        /// <summary>
        /// Occurs when the game ends.
        /// </summary>
        public event Action<string> GameEnded;

        /// <summary>
        /// Occurs when the list of players is updated.
        /// </summary>
        public event Action PlayersUpdated;

        /// <summary>
        /// Starts listening for messages from the server.
        /// </summary>
        private async void StartListening()
        {
            try
            {
                while (true)
                {
                    byte[] lengthBuffer = new byte[4];
                    int readLen = await _sslStream.ReadAsync(lengthBuffer, 0, 4);
                    if (readLen == 0) break;

                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                    if (messageLength <= 0) continue;

                    byte[] data = new byte[messageLength];
                    int totalRead = 0;
                    while (totalRead < messageLength)
                    {
                        int read = await _sslStream.ReadAsync(data, totalRead, messageLength - totalRead);
                        if (read == 0) break;
                        totalRead += read;
                    }

                    string json = Encoding.UTF8.GetString(data);
                    Console.WriteLine($"DEBUG: Received message: {json}");
                    HandleMessage(json);
                }
            }
            catch (Exception ex)
            {
                MessageReceived?.Invoke($"Error receiving data: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles a message received from the server.
        /// </summary>
        /// <param name="messageJson">The JSON string of the message.</param>
        private void HandleMessage(string messageJson)
        {
            var gameMessage = JsonSerializer.Deserialize<GameMessage>(messageJson);
            if (gameMessage == null) return;

            switch (gameMessage.Type)
            {
                case "JoinGameSuccess":
                    var player = JsonSerializer.Deserialize<Player>(gameMessage.Data.ToString());
                    _myPlayerId = player.Id;
                    MessageReceived?.Invoke($"You joined successfully. Your ID is {_myPlayerId}.");
                    break;

                case "GameStateUpdate":
                    var gameState = JsonSerializer.Deserialize<GameState>(gameMessage.Data.ToString());
                    Players = gameState.Players;
                    BoardSpaces = gameState.Board.Spaces;
                    bool isMyTurn = Players[gameState.CurrentPlayerIndex].Id == _myPlayerId;
                    MyTurnUpdated?.Invoke(isMyTurn);
                    PlayersUpdated?.Invoke();

                    // 🧽 Clear the set of forms that have already been shown when the turn passes
                    if (!isMyTurn)
                        _buyFormShownForProperties.Clear();

                    break;

                case "GameEnded":
                    string winnerName = gameMessage.Data.GetProperty("WinnerName").GetString();
                    int winnerMoney = gameMessage.Data.GetProperty("WinnerMoney").GetInt32();
                    GameEnded?.Invoke($"Game Ended! Winner: {winnerName}, Money: ${winnerMoney}");
                    break;

                case "ShowBuyForm":
                    var propertyToBuy = JsonSerializer.Deserialize<BoardSpace>(gameMessage.Data.ToString());
                    if (propertyToBuy == null) return;

                    string propKey = propertyToBuy.Name;
                    if (_buyFormShownForProperties.Contains(propKey)) return;
                    _buyFormShownForProperties.Add(propKey);

                    if (Application.OpenForms["MonopolyForm"] is MonopolyForm mainForm)
                    {
                        mainForm.Invoke(new Action(() =>
                        {
                            using var form = new Form_buy(this, propertyToBuy);
                            form.ShowDialog();
                        }));
                    }
                    break;

                case "ShowRentForm":
                    var rentData = JsonSerializer.Deserialize<JsonElement>(gameMessage.Data.ToString());
                    var space = JsonSerializer.Deserialize<BoardSpace>(rentData.GetProperty("Property").ToString());
                    string ownerName = rentData.GetProperty("OwnerName").GetString();

                    if (space != null && Application.OpenForms["MonopolyForm"] is Form formMain)
                    {
                        formMain.Invoke(new Action(() =>
                        {
                            using var rentForm = new Form_rent(this, space, space.RentPrice, ownerName);
                            rentForm.ShowDialog();
                        }));
                    }
                    break;

                case "ServerLog":
                    string text = gameMessage.Data.GetProperty("Text").GetString();
                    MessageReceived?.Invoke(text);
                    break;
            }
        }

        /// <summary>
        /// Connects to the game server asynchronously.
        /// </summary>
        /// <param name="ip">The IP address of the server.</param>
        /// <param name="port">The port number of the server.</param>
        /// <returns>A task that represents the asynchronous connect operation.</returns>
        public async Task ConnectAsync(string ip, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port);

            var stream = _client.GetStream();
            _sslStream = new SslStream(stream, false, (sender, cert, chain, errors) => true); // ❗ Accepts any certificate (for testing only)

            await _sslStream.AuthenticateAsClientAsync("localhost");
            StartListening();
        }

        /// <summary>
        /// Sends a message to the server asynchronously.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public async Task SendMessageAsync(GameMessage message)
        {
            string json = JsonSerializer.Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] prefix = BitConverter.GetBytes(data.Length);

            await _sslStream.WriteAsync(prefix, 0, prefix.Length);
            await _sslStream.WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// Sends a request to join the game asynchronously.
        /// </summary>
        /// <param name="name">The name of the player.</param>
        /// <returns>A task that represents the asynchronous join operation.</returns>
        public async Task JoinGameAsync(string name)
        {
            await SendMessageAsync(new GameMessage { Type = "JoinGame", Data = JsonSerializer.SerializeToElement(new { Name = name }) });
        }

        /// <summary>
        /// Sends a request to start the game asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous start operation.</returns>
        public async Task StartGameAsync()
        {
            await SendMessageAsync(new GameMessage { Type = "StartGame", Data = JsonSerializer.SerializeToElement(new { }) });
        }

        /// <summary>
        /// Sends a request to roll the dice asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous roll operation.</returns>
        public async Task RollDiceAsync()
        {
            await SendMessageAsync(new GameMessage { Type = "RollDice", Data = JsonSerializer.SerializeToElement(new { }) });
        }

        /// <summary>
        /// Sends a request to end the game asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous end operation.</returns>
        public async Task EndGame()
        {
            await SendMessageAsync(new GameMessage { Type = "EndGame", Data = JsonSerializer.SerializeToElement(new { }) });
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        public void Disconnect()
        {
            _sslStream?.Close();
            _client?.Close();
        }

        /// <summary>
        /// Gets the display string for the position of a player.
        /// </summary>
        /// <param name="playerId">The ID of the player.</param>
        /// <returns>The display string for the player's position.</returns>
        public string GetPlayerPositionDisplay(string playerId)
        {
            Player player = Players.FirstOrDefault(p => p.Id == playerId);
            return player != null ? $"Position: {player.Position} ({player.CurrentProperty})" : "Player not found";
        }
    }
}