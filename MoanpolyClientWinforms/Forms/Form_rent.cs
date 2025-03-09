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

            textBox_nameOfProperty.Text = _space.Name;
            textBox_rentMoney.Text = _space.RentPrice.ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //await _client.SendMessageAsync(new GameMessage
            //{
            //    Type = "PayRent",
            //    Data = JsonSerializer.SerializeToElement(new
            //    {
            //        PropertyName = _space.Name,
            //        RentPrice = _space.RentPrice
            //    })
            //});
            _client.SendMessageAsync(new GameMessage
            {
                Type = "PayRent",
                Data = JsonSerializer.SerializeToElement(new
                {
                    PropertyName = _space.Name,
                    RentPrice = _space.RentPrice
                })
            });
            this.Close();
        }
    }
}
