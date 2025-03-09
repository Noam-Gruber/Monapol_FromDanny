using System;
using MonopolyClient;
using MonopolyCommon;
using MonopolyServer;
using System.Text.Json;
using System.Windows.Forms;

namespace MonapolClientUI.Forms
{
    public partial class Form_rent: Form
    {
        private readonly GameClient _client;
        private readonly BoardSpace _space;

        public Form_rent(GameClient client, BoardSpace space)
        {
            InitializeComponent();
            _client = client;
            _space = space;
        }

        private async void btnOK_Click(object sender, EventArgs e)
        {
            await _client.SendMessageAsync(new GameMessage
            {
                Type = "RentProperty",
                Data = JsonSerializer.SerializeToElement(new
                {
                    PropertyName = _space.Name
                })
            });
            this.Close();
        }
    }
}
