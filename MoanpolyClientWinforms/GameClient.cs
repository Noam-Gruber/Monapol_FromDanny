using System;
using System.Linq;
using System.Text;
using MonopolyCommon;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MonopolyClient
{
    public class GameClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private string _myPlayerId;

        public List<Player> Players { get; private set; } 

        public event Action<string> MessageReceived;
        public event Action<bool> MyTurnUpdated;

        public GameClient()
        {
            Players = new List<Player>();
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

                    HandleMessage(message);
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

            if (gameMessage.Type == "GameStateUpdate")
            {
                var gameState = JsonSerializer.Deserialize<GameState>(gameMessage.Data.ToString());
                Players = gameState.Players;

                bool isMyTurn = Players[gameState.CurrentPlayerIndex].Id == _myPlayerId;
                MyTurnUpdated?.Invoke(isMyTurn);
            }
            else if (gameMessage.Type == "JoinGameSuccess") // הוספת סוג הודעה חדש
            {
                _myPlayerId = gameMessage.Data.GetProperty("PlayerId").GetString();
                MessageReceived?.Invoke($"You joined the game successfully. Your ID is {_myPlayerId}.");
            }
        }

        public string GetPlayerPositionDisplay(string playerId)
        {
            var player = Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                return $"Position: {player.Position}";
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

        public void Disconnect()
        {
            _stream.Close();
            _client.Close();
        }
    }
}
