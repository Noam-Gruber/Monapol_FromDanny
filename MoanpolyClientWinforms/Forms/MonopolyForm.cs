using System;
using System.Linq;
using MonopolyClient;
using System.Windows.Forms;
using MonapolClientUI.Forms;

namespace MoanpolyClientWinforms
{
    public partial class MonopolyForm: Form
    {
        private GameClient _client;
        private Form_buy buyForm = null;
        private bool _buyFormOpenedThisTurn = false;

        public MonopolyForm()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            _client = new GameClient();
            await _client.ConnectAsync("127.0.0.1", 5000);

            _client.MessageReceived += (message) =>
            {
                Invoke(new Action(() =>
                {
                    WriteToLogger(message);
                }));
            };

            _client.MyTurnUpdated += (isMyTurn) =>
            {
                Invoke(new Action(() =>
                {
                    btnRollDice.Enabled = isMyTurn && !btnStartGame.Enabled;
                    btnEndGame.Enabled = !btnStartGame.Enabled;

                    if (isMyTurn && !_buyFormOpenedThisTurn)
                    {
                        var player = _client.Players.FirstOrDefault(p => p.Id == _client.MyPlayerId);
                        var currentSpace = _client.BoardSpaces[player.Position];

                        if (!currentSpace.IsOwned && !currentSpace.IsSpecial && player.Money >= currentSpace.PurchasePrice)
                        {
                            _buyFormOpenedThisTurn = true; // הגדרת החלון כנפתח עבור התור הנוכחי
                            using (Form_buy buyForm = new Form_buy(_client, currentSpace))
                            {
                                buyForm.ShowDialog();
                            }
                        }
                    }

                    WriteToLogger(isMyTurn ? "It's your turn!" : "Waiting for other players...");
                }));
            };


            // להוסיף את הקטע הבא:
            _client.PlayersUpdated += () =>
            {
                Invoke(new Action(() =>
                {
                    UpdatePlayerPositionsDisplay();
                }));
            };

            _client.GameEnded += (endGameMessage) =>
            {
                Invoke(new Action(() =>
                {
                    WriteToLogger(endGameMessage);
                    btnRollDice.Enabled = false;
                    btnEndGame.Enabled = false;
                }));
            };


            btnConnect.Enabled = false;
            WriteToLogger("Connected to server.");
        }



        private async void btnJoinGame_Click(object sender, EventArgs e)
        {
            string playerName = txtPlayerName.Text;
            if (!string.IsNullOrWhiteSpace(playerName))
            {
                await _client.JoinGameAsync(playerName);
                WriteToLogger($"Joined the game as {playerName}.");
                btnJoinGame.Enabled = false;
                txtPlayerName.Enabled = false;
                // הצג את המיקום ההתחלתי
                UpdatePlayerPositionsDisplay();
            }
            else
            {
                WriteToLogger("Please enter a player name.");
            }
        }

        private async void btnRollDice_Click(object sender, EventArgs e)
        {
            _buyFormOpenedThisTurn = false;
            await _client.RollDiceAsync();  // שלח בקשה לשרת לביצוע גלגול קוביות
            WriteToLogger("Roll Dice");
        }

        private async void btnStartGame_Click(object sender, EventArgs e)
        {
            await _client.StartGameAsync();
            btnStartGame.Enabled = false;
            btnEndGame.Enabled = true;
            btnRollDice.Enabled = true;
            WriteToLogger("The game is starting...");
        }

        private void MonopolyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _client?.Disconnect();
        }

        private void UpdatePlayerPositionsDisplay()
        {
            if (_client != null && _client.Players != null)
            {
                lstPlayerPositions.Items.Clear();
                foreach (var player in _client.Players)
                {
                    string position = _client.GetPlayerPositionDisplay(player.Id);  // קריאה לפונקציה
                    lstPlayerPositions.Items.Add($"{player.Name}: {position}");  // עדכון ה-UI עם המיקום
                }
                btnConnect.Text = "Connected";
                btnConnect.Enabled = false;
            }
            else
            {
                WriteToLogger("Client or Player list is not initialized");
            }
        }

        private void WriteToLogger(string message)
        {
            string timeStampedMessage = $"{DateTime.Now:HH:mm:ss} - {message}";
            richTextBoxMessages.AppendText(timeStampedMessage + Environment.NewLine);
            richTextBoxMessages.SelectionStart = richTextBoxMessages.Text.Length;
            richTextBoxMessages.ScrollToCaret();
        }

        private async void btnEndGame_Click(object sender, EventArgs e)
        {
            await _client.EndGame();
            WriteToLogger("You ended the game.");
        }
    }
}
