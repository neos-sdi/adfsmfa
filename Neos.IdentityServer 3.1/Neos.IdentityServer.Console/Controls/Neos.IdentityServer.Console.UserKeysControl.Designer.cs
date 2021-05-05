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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserPropertiesKeysControl));
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
            resources.ApplyResources(this.groupBoxKey, "groupBoxKey");
            this.groupBoxKey.Name = "groupBoxKey";
            this.groupBoxKey.TabStop = false;
            // 
            // BTNSendByMail
            // 
            resources.ApplyResources(this.BTNSendByMail, "BTNSendByMail");
            this.BTNSendByMail.Name = "BTNSendByMail";
            this.BTNSendByMail.UseVisualStyleBackColor = true;
            this.BTNSendByMail.Click += new System.EventHandler(this.BTNSendByMail_Click);
            // 
            // clearkeyBtn
            // 
            resources.ApplyResources(this.clearkeyBtn, "clearkeyBtn");
            this.clearkeyBtn.Name = "clearkeyBtn";
            this.clearkeyBtn.UseVisualStyleBackColor = true;
            this.clearkeyBtn.Click += new System.EventHandler(this.clearkeyBtn_Click);
            // 
            // newkeyBtn
            // 
            resources.ApplyResources(this.newkeyBtn, "newkeyBtn");
            this.newkeyBtn.Name = "newkeyBtn";
            this.newkeyBtn.UseVisualStyleBackColor = true;
            this.newkeyBtn.Click += new System.EventHandler(this.newkeyBtn_Click);
            // 
            // DisplayKey
            // 
            resources.ApplyResources(this.DisplayKey, "DisplayKey");
            this.DisplayKey.Name = "DisplayKey";
            this.DisplayKey.ReadOnly = true;
            this.DisplayKey.TabStop = false;
            this.DisplayKey.TextChanged += new System.EventHandler(this.EmailPrompt_TextChanged);
            // 
            // EmailPrompt
            // 
            resources.ApplyResources(this.EmailPrompt, "EmailPrompt");
            this.EmailPrompt.Name = "EmailPrompt";
            this.EmailPrompt.TextChanged += new System.EventHandler(this.EmailPrompt_TextChanged);
            // 
            // qrCodeGraphic
            // 
            this.qrCodeGraphic.ErrorCorrectLevel = Neos.IdentityServer.MultiFactor.QrEncoding.ErrorCorrectionLevel.L;
            resources.ApplyResources(this.qrCodeGraphic, "qrCodeGraphic");
            this.qrCodeGraphic.Name = "qrCodeGraphic";
            this.qrCodeGraphic.QuietZoneModule = Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render.QuietZoneModules.Zero;
            this.qrCodeGraphic.TabStop = false;
            // 
            // UserPropertiesKeysControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.qrCodeGraphic);
            this.Controls.Add(this.EmailPrompt);
            this.Controls.Add(this.groupBoxKey);
            this.Name = "UserPropertiesKeysControl";
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
