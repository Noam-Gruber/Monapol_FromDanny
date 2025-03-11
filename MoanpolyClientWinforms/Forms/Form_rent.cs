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
        private readonly int _rentPrice;
        private readonly string _ownerName;

        public Form_rent(GameClient client, BoardSpace space, int rentPrice, string ownerName)
        {
            InitializeComponent();
            _client = client;
            _space = space;
            _rentPrice = rentPrice;
            _ownerName = ownerName;

            textBox_nameOfProperty.Text = _space.Name;
            textBox_rentMoney.Text = _space.RentPrice.ToString();
            textBox_ownedBy.Text = _ownerName;
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
