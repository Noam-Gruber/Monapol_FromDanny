using System;
using System.Linq;
using System.Text;
using MonopolyServer;
using MonopolyCommon;
using System.Text.Json;
using System.Net.Sockets;
using System.Windows.Forms;
using MonapolClientUI.Forms;
using System.Threading.Tasks;
using MoanpolyClientWinforms;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace MonopolyClient
{
    public class GameClient
    {
        private TcpClient _client;
        private string _myPlayerId;
        private HashSet<string> _buyFormShownForProperties = new();
        private SslStream _sslStream;

        public string MyPlayerId => _myPlayerId;
        public List<Player> Players { get; private set; } = new();
        public List<BoardSpace> BoardSpaces { get; private set; }

        public event Action<string> MessageReceived;
        public event Action<bool> MyTurnUpdated;
        public event Action<string> GameEnded;
        public event Action PlayersUpdated;

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

                    // 🧽 ננקה את סט הטפסים שכבר הוצגו ברגע שהתור עבר
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

        public async Task ConnectAsync(string ip, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port);

            var stream = _client.GetStream();
            _sslStream = new SslStream(stream, false, (sender, cert, chain, errors) => true); // ❗ מקבל כל תעודה (לבדיקה בלבד)

            await _sslStream.AuthenticateAsClientAsync("localhost");
            StartListening();
        }


        public async Task SendMessageAsync(GameMessage message)
        {
            string json = JsonSerializer.Serialize(message);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] prefix = BitConverter.GetBytes(data.Length);

            await _sslStream.WriteAsync(prefix, 0, prefix.Length);
            await _sslStream.WriteAsync(data, 0, data.Length);
        }

        public async Task JoinGameAsync(string name)
        {
            await SendMessageAsync(new GameMessage { Type = "JoinGame", Data = JsonSerializer.SerializeToElement(new { Name = name }) });
        }

        public async Task StartGameAsync()
        {
            await SendMessageAsync(new GameMessage { Type = "StartGame", Data = JsonSerializer.SerializeToElement(new { }) });
        }

        public async Task RollDiceAsync()
        {
            await SendMessageAsync(new GameMessage { Type = "RollDice", Data = JsonSerializer.SerializeToElement(new { }) });
        }

        public async Task EndGame()
        {
            await SendMessageAsync(new GameMessage { Type = "EndGame", Data = JsonSerializer.SerializeToElement(new { }) });
        }

        public void Disconnect()
        {
            _sslStream?.Close();
            _client?.Close();
        }

        public string GetPlayerPositionDisplay(string playerId)
        {
            Player player = Players.FirstOrDefault(p => p.Id == playerId);
            return player != null ? $"Position: {player.Position} ({player.CurrentProperty})" : "Player not found";
        }
    }
}