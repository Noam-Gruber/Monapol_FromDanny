
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MonopolyClientUI
{
    public partial class MainWindow : Window
    {
        private GameClient _client;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = IpAddressTextBox.Text.Trim();
            if (!int.TryParse(PortTextBox.Text.Trim(), out int port))
            {
                MessageBox.Show("Invalid port number.");
                return;
            }

            _client = new GameClient();
            _client.MessageReceived += OnMessageReceived;

            try
            {
                await _client.ConnectAsync(ipAddress, port);
                AppendMessage("Connected to server.");
                
                JoinGameButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                AppendMessage($"Error connecting to server: {ex.Message}");
            }
        }

        private async void JoinGameButton_Click(object sender, RoutedEventArgs e)
        {
            string playerName = PlayerNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(playerName))
            {
                MessageBox.Show("Please enter your name.");
                return;
            }

            try
            {
                await _client.JoinGameAsync(playerName);
                AppendMessage("Joined game successfully.");

                RollDiceButton.IsEnabled = true;
              
                PlayerNameTextBox.IsEnabled = false;
            }
            catch (Exception ex)
            {
                AppendMessage($"Error joining game: {ex.Message}");
            }
        }

        private async void RollDiceButton_Click(object sender, RoutedEventArgs e)
        {
            var rollMessage = new GameMessage
            {
                Type = "RollDice",
                Data = JsonSerializer.SerializeToElement(new { })
            };
            
            await _client.SendMessageAsync(rollMessage);




            double currentLeft = Canvas.GetLeft(Player1Piece);
            double currentTop = Canvas.GetTop(Player1Piece);


            if (double.IsNaN(currentLeft)) currentLeft = 0;
            if (double.IsNaN(currentTop)) currentTop = 0;

            double newLeft = currentLeft + 20;
            double newTop = currentTop + 10;


            Canvas.SetLeft(Player1Piece, newLeft);
            Canvas.SetTop(Player1Piece, newTop);

            AppendMessage($"Moved Player1Piece to X: {newLeft}, Y: {newTop}");
        }



        private void AppendMessage(string message)
        {
            
            Dispatcher.Invoke(() =>
            {
                MessagesTextBox.AppendText(message + "\n");
                MessagesTextBox.ScrollToEnd();
            });
        }


      
        private void OnMessageReceived(string message)
        {
            AppendMessage("Server: " + message);
        }
    }
}
