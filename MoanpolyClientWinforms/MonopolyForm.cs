using System;
using MonopolyClient;
using System.Windows.Forms;

namespace MoanpolyClientWinforms
{
    public partial class MonopolyForm: Form
    {
        private GameClient _client;

        public MonopolyForm()
        {
            InitializeComponent();
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

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            _client = new GameClient();  // אתחול כ- GameClient
            await _client.ConnectAsync("127.0.0.1", 5000);  // חיבור לשרת
            _client.MessageReceived += (message) =>
            {
                Invoke(new Action(() =>
                {
                    WriteToLogger($"{DateTime.Now:HH:mm:ss} - {message}");
                    // אחרי קבלת הודעה, עדכן את המיקומים ב-UI
                    UpdatePlayerPositionsDisplay();  // עדכון המיקומים ב-UI
                }));
            };

            // האזן לאירוע התור שלך
            _client.MyTurnUpdated += (isMyTurn) =>
            {
                Invoke(new Action(() =>
                {
                    btnRollDice.Enabled = isMyTurn;
                    WriteToLogger(isMyTurn ? "It's your turn!" : "Waiting for other players...");
                }));
            };

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
            await _client.RollDiceAsync();  // שלח בקשה לשרת לביצוע גלגול קוביות
            WriteToLogger("Roll Dice");
            UpdatePlayerPositionsDisplay();  // עדכון המיקומים ב-UI
        }

        private void MonopolyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _client?.Disconnect();
        }

        private async void btnStartGame_Click(object sender, EventArgs e)
        {
            await _client.StartGameAsync();
            WriteToLogger("The game is starting :-)");
            btnStartGame.Enabled = false;
        }

        private void WriteToLogger(string message)
        {
            //string timeStampedMessage = $"{DateTime.Now:HH:mm:ss} - {message}";
            //richTextBoxMessages.AppendText(timeStampedMessage + Environment.NewLine);
            //richTextBoxMessages.ScrollToCaret();

            string timeStampedMessage = $"{DateTime.Now:HH:mm:ss} - {message}";
            richTextBoxMessages.AppendText(timeStampedMessage + Environment.NewLine);
            richTextBoxMessages.SelectionStart = richTextBoxMessages.Text.Length;
            richTextBoxMessages.ScrollToCaret();
        }
    }
}
