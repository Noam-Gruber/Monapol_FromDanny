using System;
using System.Linq;
using System.Text;
using MonopolyCommon;
using MonopolyServer;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace MonopolyClient
{
    public class GameClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private string _myPlayerId;

        public string MyPlayerId => _myPlayerId;
        public Board Board { get; private set; }
        public List<Player> Players { get; private set; }
        public List<BoardSpace> BoardSpaces { get; private set; }

        public event Action<string> MessageReceived;
        public event Action<bool> MyTurnUpdated;
        public event Action PlayersUpdated;
        public event Action<string> GameEnded;

        public GameClient()
        {
            Players = new List<Player>();
        }

        private async void StartListening()
        {
            try
            {
                while (true)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        byte[] buffer = new byte[8192]; // הגדלת ה-buffer
                        int bytesRead;

                        // קריאה רציפה עד שהזרם מסתיים
                        while ((bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            memoryStream.Write(buffer, 0, bytesRead);

                            // אם התקבל פחות מה-buffer המלא, סביר להניח שסיימנו
                            if (bytesRead < buffer.Length)
                                break;
                        }

                        string message = Encoding.UTF8.GetString(memoryStream.ToArray());
                        Console.WriteLine($"DEBUG: Received message: {message}"); // הדפסת ההודעה המלאה
                        HandleMessage(message);
                    }
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
                case "PlayerListUpdate":
                    Players = JsonSerializer.Deserialize<List<Player>>(gameMessage.Data.ToString());
                    PlayersUpdated?.Invoke();
                    break;

                case "GameStateUpdate":
                    var gameState = JsonSerializer.Deserialize<GameState>(gameMessage.Data.ToString());
                    Players = gameState.Players;
                    BoardSpaces = gameState.Board.Spaces;
                    bool isMyTurn = Players[gameState.CurrentPlayerIndex].Id == _myPlayerId;
                    MyTurnUpdated?.Invoke(isMyTurn);
                    PlayersUpdated?.Invoke();

                    // כאן להוסיף את השורות הבאות:
                    foreach (var space in BoardSpaces)
                    {
                        if (space.IsOwned)
                            Console.WriteLine($"DEBUG: Space {space.Name} owned by {space.OwnedByPlayerId}");
                    }

                    break;

                case "JoinGameSuccess":
                    var player = JsonSerializer.Deserialize<Player>(gameMessage.Data.ToString());
                    _myPlayerId = player.Id;
                    MessageReceived?.Invoke($"You joined successfully. Your ID is {_myPlayerId}.");
                    break;

                case "GameEnded":
                    string winnerName = gameMessage.Data.GetProperty("WinnerName").GetString();
                    int winnerMoney = gameMessage.Data.GetProperty("WinnerMoney").GetInt32();
                    string endGameMessage = $"Game Ended! Winner: {winnerName}, Money: ${winnerMoney}";
                    GameEnded?.Invoke(endGameMessage);
                    break;
            }
        }

        public string GetPlayerPositionDisplay(string playerId)
        {
            var player = Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                return $"Position: {player.Position} ({player.CurrentProperty})";
            }
            return "Player not found";
        }

        public async Task ConnectAsync(string ipAddress, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ipAddress, port);
            _stream = _client.GetStream();
            StartListening();
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

            if (Players == null)
            {
                Players = new List<Player>();
            }
        }

        public async Task StartGameAsync()
        {
            var startMessage = new GameMessage
            {
                Type = "StartGame",
                Data = JsonSerializer.SerializeToElement(new { })
            };
            await SendMessageAsync(startMessage);
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

        public async Task EndGame()
        {
            await SendMessageAsync(new GameMessage { Type = "EndGame", Data = JsonSerializer.SerializeToElement(new { }) });
        }

        public void Disconnect()
        {
            _stream.Close();
            _client.Close();
        }
    }
}
