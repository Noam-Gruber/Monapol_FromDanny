namespace MoanpolyClientWinforms
{
    partial class MonopolyForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MonopolyForm));
            this.btnRollDice = new System.Windows.Forms.Button();
            this.txtPlayerName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnJoinGame = new System.Windows.Forms.Button();
            this.btnStartGame = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.richTextBoxMessages = new System.Windows.Forms.RichTextBox();
            this.btnEndGame = new System.Windows.Forms.Button();
            this.rtbPlayerPositions = new System.Windows.Forms.RichTextBox();
            this.rtbPlayerProperties = new System.Windows.Forms.RichTextBox();
            this.rtbPlayerMoney = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRollDice
            // 
            this.btnRollDice.Enabled = false;
            this.btnRollDice.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.btnRollDice.Image = ((System.Drawing.Image)(resources.GetObject("btnRollDice.Image")));
            this.btnRollDice.Location = new System.Drawing.Point(920, 9);
            this.btnRollDice.Name = "btnRollDice";
            this.btnRollDice.Size = new System.Drawing.Size(229, 44);
            this.btnRollDice.TabIndex = 1;
            this.btnRollDice.Text = "Roll Dice";
            this.btnRollDice.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnRollDice.UseVisualStyleBackColor = true;
            this.btnRollDice.Click += new System.EventHandler(this.btnRollDice_Click);
            // 
            // txtPlayerName
            // 
            this.txtPlayerName.Location = new System.Drawing.Point(131, 21);
            this.txtPlayerName.Name = "txtPlayerName";
            this.txtPlayerName.Size = new System.Drawing.Size(111, 20);
            this.txtPlayerName.TabIndex = 2;
            this.txtPlayerName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label1.Location = new System.Drawing.Point(8, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Name of player:";
            // 
            // btnConnect
            // 
            this.btnConnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.btnConnect.Location = new System.Drawing.Point(3, 5);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(118, 49);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "Connect to Server";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnJoinGame
            // 
            this.btnJoinGame.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.btnJoinGame.Location = new System.Drawing.Point(255, 13);
            this.btnJoinGame.Name = "btnJoinGame";
            this.btnJoinGame.Size = new System.Drawing.Size(104, 35);
            this.btnJoinGame.TabIndex = 5;
            this.btnJoinGame.Text = "Join Game";
            this.btnJoinGame.UseVisualStyleBackColor = true;
            this.btnJoinGame.Click += new System.EventHandler(this.btnJoinGame_Click);
            // 
            // btnStartGame
            // 
            this.btnStartGame.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.btnStartGame.Location = new System.Drawing.Point(365, 11);
            this.btnStartGame.Name = "btnStartGame";
            this.btnStartGame.Size = new System.Drawing.Size(99, 37);
            this.btnStartGame.TabIndex = 6;
            this.btnStartGame.Text = "Start Game";
            this.btnStartGame.UseVisualStyleBackColor = true;
            this.btnStartGame.Click += new System.EventHandler(this.btnStartGame_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(1, 60);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(906, 528);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnJoinGame);
            this.groupBox1.Controls.Add(this.txtPlayerName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnStartGame);
            this.groupBox1.Location = new System.Drawing.Point(129, -2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(469, 55);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            // 
            // richTextBoxMessages
            // 
            this.richTextBoxMessages.Location = new System.Drawing.Point(1, 592);
            this.richTextBoxMessages.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBoxMessages.Name = "richTextBoxMessages";
            this.richTextBoxMessages.Size = new System.Drawing.Size(906, 86);
            this.richTextBoxMessages.TabIndex = 10;
            this.richTextBoxMessages.Text = "";
            // 
            // btnEndGame
            // 
            this.btnEndGame.Enabled = false;
            this.btnEndGame.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.btnEndGame.Image = ((System.Drawing.Image)(resources.GetObject("btnEndGame.Image")));
            this.btnEndGame.Location = new System.Drawing.Point(912, 592);
            this.btnEndGame.Name = "btnEndGame";
            this.btnEndGame.Size = new System.Drawing.Size(238, 88);
            this.btnEndGame.TabIndex = 17;
            this.btnEndGame.Text = "End Game";
            this.btnEndGame.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnEndGame.UseVisualStyleBackColor = true;
            this.btnEndGame.Click += new System.EventHandler(this.btnEndGame_Click);
            // 
            // rtbPlayerPositions
            // 
            this.rtbPlayerPositions.Location = new System.Drawing.Point(920, 60);
            this.rtbPlayerPositions.Name = "rtbPlayerPositions";
            this.rtbPlayerPositions.Size = new System.Drawing.Size(229, 143);
            this.rtbPlayerPositions.TabIndex = 19;
            this.rtbPlayerPositions.Text = "";
            // 
            // rtbPlayerProperties
            // 
            this.rtbPlayerProperties.Location = new System.Drawing.Point(920, 390);
            this.rtbPlayerProperties.Name = "rtbPlayerProperties";
            this.rtbPlayerProperties.Size = new System.Drawing.Size(229, 196);
            this.rtbPlayerProperties.TabIndex = 20;
            this.rtbPlayerProperties.Text = "";
            // 
            // rtbPlayerMoney
            // 
            this.rtbPlayerMoney.Location = new System.Drawing.Point(921, 222);
            this.rtbPlayerMoney.Name = "rtbPlayerMoney";
            this.rtbPlayerMoney.Size = new System.Drawing.Size(229, 143);
            this.rtbPlayerMoney.TabIndex = 21;
            this.rtbPlayerMoney.Text = "";
            // 
            // MonopolyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1161, 692);
            this.Controls.Add(this.rtbPlayerMoney);
            this.Controls.Add(this.rtbPlayerProperties);
            this.Controls.Add(this.rtbPlayerPositions);
            this.Controls.Add(this.btnEndGame);
            this.Controls.Add(this.richTextBoxMessages);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnRollDice);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MonopolyForm";
            this.Text = "Monopoly Game";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MonopolyForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnRollDice;
        private System.Windows.Forms.TextBox txtPlayerName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnJoinGame;
        private System.Windows.Forms.Button btnStartGame;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RichTextBox richTextBoxMessages;
        private System.Windows.Forms.Button btnEndGame;
        private System.Windows.Forms.RichTextBox rtbPlayerPositions;
        private System.Windows.Forms.RichTextBox rtbPlayerProperties;
        private System.Windows.Forms.RichTextBox rtbPlayerMoney;
    }
}

