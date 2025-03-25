using System;
using MonopolyServer;
using MonopolyClient;
using MonopolyCommon;
using System.Text.Json;
using System.Windows.Forms;

namespace MonapolClientUI.Forms
{
    public partial class Form_buy : Form
    {
        private readonly GameClient _client;
        private readonly BoardSpace _space;
        private bool hasSent = false;

        public Form_buy(GameClient client, BoardSpace space)
        {
            InitializeComponent();
            _client = client;
            _space = space;

            textBox_nameOfProperty.Text = _space.Name;
            textBox_buyProperty.Text = _space.PurchasePrice.ToString();
            textBox_rentProperty.Text = _space.RentPrice.ToString();
        }

        private async void btnOK_Click(object sender, EventArgs e)
        {
            if (hasSent) return;
            hasSent = true;

            await _client.SendMessageAsync(new GameMessage
            {
                Type = "BuyProperty",
                Data = JsonSerializer.SerializeToElement(new
                {
                    PropertyName = _space.Name
                })
            });
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
