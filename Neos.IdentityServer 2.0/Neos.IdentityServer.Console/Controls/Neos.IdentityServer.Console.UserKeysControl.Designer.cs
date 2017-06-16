namespace Neos.IdentityServer.Console
{
    partial class UserPropertiesKeysControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxKey = new System.Windows.Forms.GroupBox();
            this.BTNSendByMail = new System.Windows.Forms.Button();
            this.clearkeyBtn = new System.Windows.Forms.Button();
            this.newkeyBtn = new System.Windows.Forms.Button();
            this.DisplayKey = new System.Windows.Forms.TextBox();
            this.EmailPrompt = new System.Windows.Forms.Label();
            this.qrCodeGraphic = new Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Forms.QrCodeGraphicControl();
            this.groupBoxKey.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxKey
            // 
            this.groupBoxKey.Controls.Add(this.BTNSendByMail);
            this.groupBoxKey.Controls.Add(this.clearkeyBtn);
            this.groupBoxKey.Controls.Add(this.newkeyBtn);
            this.groupBoxKey.Controls.Add(this.DisplayKey);
            this.groupBoxKey.Location = new System.Drawing.Point(13, 433);
            this.groupBoxKey.Name = "groupBoxKey";
            this.groupBoxKey.Size = new System.Drawing.Size(456, 176);
            this.groupBoxKey.TabIndex = 7;
            this.groupBoxKey.TabStop = false;
            this.groupBoxKey.Text = "Secret";
            // 
            // BTNSendByMail
            // 
            this.BTNSendByMail.Location = new System.Drawing.Point(271, 142);
            this.BTNSendByMail.Name = "BTNSendByMail";
            this.BTNSendByMail.Size = new System.Drawing.Size(162, 23);
            this.BTNSendByMail.TabIndex = 4;
            this.BTNSendByMail.Text = "Envoyer la clé par mail";
            this.BTNSendByMail.UseVisualStyleBackColor = true;
            // 
            // clearkeyBtn
            // 
            this.clearkeyBtn.Location = new System.Drawing.Point(19, 142);
            this.clearkeyBtn.Name = "clearkeyBtn";
            this.clearkeyBtn.Size = new System.Drawing.Size(102, 23);
            this.clearkeyBtn.TabIndex = 2;
            this.clearkeyBtn.Text = "Effacer Key";
            this.clearkeyBtn.UseVisualStyleBackColor = true;
            this.clearkeyBtn.Click += new System.EventHandler(this.clearkeyBtn_Click);
            // 
            // newkeyBtn
            // 
            this.newkeyBtn.Location = new System.Drawing.Point(145, 142);
            this.newkeyBtn.Name = "newkeyBtn";
            this.newkeyBtn.Size = new System.Drawing.Size(102, 23);
            this.newkeyBtn.TabIndex = 3;
            this.newkeyBtn.Text = "Nouvelle Key";
            this.newkeyBtn.UseVisualStyleBackColor = true;
            this.newkeyBtn.Click += new System.EventHandler(this.newkeyBtn_Click);
            // 
            // DisplayKey
            // 
            this.DisplayKey.Location = new System.Drawing.Point(19, 28);
            this.DisplayKey.Multiline = true;
            this.DisplayKey.Name = "DisplayKey";
            this.DisplayKey.ReadOnly = true;
            this.DisplayKey.Size = new System.Drawing.Size(415, 98);
            this.DisplayKey.TabIndex = 1;
            this.DisplayKey.TabStop = false;
            this.DisplayKey.TextChanged += new System.EventHandler(this.EmailPrompt_TextChanged);
            // 
            // EmailPrompt
            // 
            this.EmailPrompt.AutoSize = true;
            this.EmailPrompt.Location = new System.Drawing.Point(19, 3);
            this.EmailPrompt.Name = "EmailPrompt";
            this.EmailPrompt.Size = new System.Drawing.Size(98, 13);
            this.EmailPrompt.TabIndex = 8;
            this.EmailPrompt.Text = "Adresse email :  {0}";
            this.EmailPrompt.TextChanged += new System.EventHandler(this.EmailPrompt_TextChanged);
            // 
            // qrCodeGraphic
            // 
            this.qrCodeGraphic.ErrorCorrectLevel = Neos.IdentityServer.MultiFactor.QrEncoding.ErrorCorrectionLevel.L;
            this.qrCodeGraphic.Location = new System.Drawing.Point(17, 26);
            this.qrCodeGraphic.Margin = new System.Windows.Forms.Padding(0);
            this.qrCodeGraphic.MaximumSize = new System.Drawing.Size(800, 800);
            this.qrCodeGraphic.MinimumSize = new System.Drawing.Size(400, 400);
            this.qrCodeGraphic.Name = "qrCodeGraphic";
            this.qrCodeGraphic.QuietZoneModule = Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render.QuietZoneModules.Zero;
            this.qrCodeGraphic.Size = new System.Drawing.Size(400, 400);
            this.qrCodeGraphic.TabIndex = 0;
            this.qrCodeGraphic.TabStop = false;
            // 
            // UserPropertiesKeysControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.qrCodeGraphic);
            this.Controls.Add(this.EmailPrompt);
            this.Controls.Add(this.groupBoxKey);
            this.Name = "UserPropertiesKeysControl";
            this.Size = new System.Drawing.Size(489, 626);
            this.groupBoxKey.ResumeLayout(false);
            this.groupBoxKey.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxKey;
        private System.Windows.Forms.Button clearkeyBtn;
        private System.Windows.Forms.Button newkeyBtn;
        private System.Windows.Forms.TextBox DisplayKey;
        private System.Windows.Forms.Label EmailPrompt;
        private MultiFactor.QrEncoding.Windows.Forms.QrCodeGraphicControl qrCodeGraphic;
        private System.Windows.Forms.Button BTNSendByMail;
    }
}
