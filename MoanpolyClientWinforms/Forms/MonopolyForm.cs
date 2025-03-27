using System;
using System.Drawing;
using MonopolyClient;
using MonopolyCommon;
using System.Windows.Forms;

namespace MoanpolyClientWinforms
{
    /// <summary>
    /// Represents the main form for the Monopoly game client.
    /// </summary>
    public partial class MonopolyForm : Form
    {
        private GameClient _client;
        private readonly int _serverPort = Params.GetPort();
        private readonly string _serverAddress = Params.GetServerAddress();

        /// <summary>
        /// Initializes a new instance of the <see cref="MonopolyForm"/> class.
        /// </summary>
        public MonopolyForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the btnConnect control.
        /// Connects to the game server.
        /// </summary>
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            _client = new GameClient();
            await _client.ConnectAsync(_serverAddress, _serverPort);

            _client.MessageReceived += (message) =>
            {
                Invoke(new Action(() => WriteToLogger(message)));
            };

            _client.MyTurnUpdated += (isMyTurn) =>
            {
                Invoke(new Action(() =>
                {
                    btnRollDice.Enabled = isMyTurn && !btnStartGame.Enabled;
                    btnEndGame.Enabled = !btnStartGame.Enabled;
                }));
            };

            _client.PlayersUpdated += () =>
            {
                Invoke(new Action(UpdatePlayerPositionsDisplay));
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
            btnJoinGame.Enabled = true;
            WriteToLogger("Connected to server.");
        }

        /// <summary>
        /// Handles the Click event of the btnJoinGame control.
        /// Joins a game with the specified player name and game ID.
        /// </summary>
        private async void btnJoinGame_Click(object sender, EventArgs e)
        {
            string playerName = txtPlayerName.Text;
            string gameId = txtGameId.Text;
            if (!string.IsNullOrWhiteSpace(playerName) && !string.IsNullOrWhiteSpace(gameId))
            {
                _client.SetGameId(gameId);
                await _client.JoinGameAsync(gameId, playerName);
                btnJoinGame.Enabled = false;
                txtPlayerName.Enabled = false;
                txtGameId.Enabled = false;
                btnStartGame.Enabled = true;
                WriteToLogger($"Joined the game '{gameId}' as {playerName}.");
            }
            else
            {
                WriteToLogger("Please enter both a player name and game ID.");
            }
        }

        /// <summary>
        /// Handles the Click event of the btnStartGame control.
        /// Starts the game.
        /// </summary>
        private async void btnStartGame_Click(object sender, EventArgs e)
        {
            await _client.StartGameAsync();
            btnStartGame.Enabled = false;
            btnEndGame.Enabled = true;
            btnRollDice.Enabled = true;
            WriteToLogger("The game is starting...");
        }

        /// <summary>
        /// Handles the Click event of the btnRollDice control.
        /// Rolls the dice.
        /// </summary>
        private async void btnRollDice_Click(object sender, EventArgs e)
        {
            await _client.RollDiceAsync();
        }

        /// <summary>
        /// Handles the Click event of the btnEndGame control.
        /// Ends the game.
        /// </summary>
        private async void btnEndGame_Click(object sender, EventArgs e)
        {
            await _client.EndGame();
            WriteToLogger("You ended the game.");
        }

        /// <summary>
        /// Handles the FormClosing event of the MonopolyForm control.
        /// Disconnects the client when the form is closing.
        /// </summary>
        private void MonopolyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _client?.Disconnect();
        }

        /// <summary>
        /// Updates the display of player positions, properties, and money.
        /// </summary>
        private void UpdatePlayerPositionsDisplay()
        {
            if (_client != null && _client.Players != null && _client.BoardSpaces != null)
            {
                rtbPlayerPositions.Clear();
                rtbPlayerProperties.Clear();
                rtbPlayerMoney.Clear();

                foreach (var player in _client.Players)
                {
                    rtbPlayerPositions.SelectionFont = new Font(rtbPlayerPositions.Font, FontStyle.Bold);
                    rtbPlayerPositions.AppendText($"{player.Name}: ");
                    rtbPlayerPositions.SelectionFont = new Font(rtbPlayerPositions.Font, FontStyle.Regular);
                    rtbPlayerPositions.AppendText($"{_client.GetPlayerPositionDisplay(player.Id)}\n");

                    rtbPlayerProperties.SelectionFont = new Font(rtbPlayerProperties.Font, FontStyle.Bold);
                    rtbPlayerProperties.AppendText($"{player.Name}: ");
                    rtbPlayerProperties.SelectionFont = new Font(rtbPlayerProperties.Font, FontStyle.Regular);
                    rtbPlayerProperties.AppendText($"{string.Join(", ", player.OwnedProperties)}\n");

                    rtbPlayerMoney.SelectionFont = new Font(rtbPlayerMoney.Font, FontStyle.Bold);
                    rtbPlayerMoney.AppendText($"{player.Name}: ");
                    rtbPlayerMoney.SelectionFont = new Font(rtbPlayerMoney.Font, FontStyle.Regular);
                    rtbPlayerMoney.AppendText($"{player.Money}\n");
                }
            }
            else
            {
                WriteToLogger("Client or Player list is not initialized");
            }
        }

        /// <summary>
        /// Plays a simple dice roll animation.
        /// </summary>
        /// <param name="value">The value of the dice roll.</param>
        private void PlayDiceAnimation(int value)
        {
            // simple simulation - could be replaced with image or real animation later
            MessageBox.Show($"You rolled a {value}!", "Dice Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Writes a message to the logger with a timestamp.
        /// </summary>
        /// <param name="message">The message to log.</param>
        private void WriteToLogger(string message)
        {
            string timeStampedMessage = $"{DateTime.Now:HH:mm:ss} - {message}";
            richTextBoxMessages.AppendText(timeStampedMessage + Environment.NewLine);
            richTextBoxMessages.SelectionStart = richTextBoxMessages.Text.Length;
            richTextBoxMessages.ScrollToCaret();
        }
    }
}
