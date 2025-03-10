using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoanpolyClientWinforms.Forms
{
    public partial class Form_Card: Form
    {
        private Label lblDescription;
        private Button btnOK;

        public Form_Card(string cardDescription)
        {
            lblDescription = new Label
            {
                Text = cardDescription,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            btnOK = new Button
            {
                Text = "OK",
                Dock = DockStyle.Bottom
            };

            btnOK.Click += (sender, e) => this.Close();

            this.Controls.Add(lblDescription);
            this.Controls.Add(btnOK);
        }

    }
}
