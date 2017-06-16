namespace Neos.IdentityServer.Console
{
    partial class UserPropertiesControl
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
            this.Phone = new System.Windows.Forms.TextBox();
            this.PhonePrompt = new System.Windows.Forms.Label();
            this.UserName = new System.Windows.Forms.TextBox();
            this.UserNamePrompt = new System.Windows.Forms.Label();
            this.UserInfo = new System.Windows.Forms.GroupBox();
            this.cbEnabled = new System.Windows.Forms.CheckBox();
            this.Email = new System.Windows.Forms.TextBox();
            this.EmailPrompt = new System.Windows.Forms.Label();
            this.groupBoxKey = new System.Windows.Forms.GroupBox();
            this.newkeyBtn = new System.Windows.Forms.Button();
            this.DisplayKey = new System.Windows.Forms.TextBox();
            this.clearkeyBtn = new System.Windows.Forms.Button();
            this.UserInfo.SuspendLayout();
            this.groupBoxKey.SuspendLayout();
            this.SuspendLayout();
            // 
            // Phone
            // 
            this.Phone.Location = new System.Drawing.Point(106, 91);
            this.Phone.Name = "Phone";
            this.Phone.Size = new System.Drawing.Size(328, 20);
            this.Phone.TabIndex = 2;
            this.Phone.TextChanged += new System.EventHandler(this.Phone_TextChanged);
            // 
            // PhonePrompt
            // 
            this.PhonePrompt.AutoSize = true;
            this.PhonePrompt.Location = new System.Drawing.Point(32, 114);
            this.PhonePrompt.Name = "PhonePrompt";
            this.PhonePrompt.Size = new System.Drawing.Size(79, 13);
            this.PhonePrompt.TabIndex = 1;
            this.PhonePrompt.Text = "N° Téléphone :";
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(106, 23);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(328, 20);
            this.UserName.TabIndex = 0;
            this.UserName.TextChanged += new System.EventHandler(this.UserName_TextChanged);
            // 
            // UserNamePrompt
            // 
            this.UserNamePrompt.AutoSize = true;
            this.UserNamePrompt.Location = new System.Drawing.Point(16, 26);
            this.UserNamePrompt.Name = "UserNamePrompt";
            this.UserNamePrompt.Size = new System.Drawing.Size(84, 13);
            this.UserNamePrompt.TabIndex = 3;
            this.UserNamePrompt.Text = "Nom Utilisateur :";
            // 
            // UserInfo
            // 
            this.UserInfo.Controls.Add(this.cbEnabled);
            this.UserInfo.Controls.Add(this.Email);
            this.UserInfo.Controls.Add(this.Phone);
            this.UserInfo.Controls.Add(this.EmailPrompt);
            this.UserInfo.Controls.Add(this.UserNamePrompt);
            this.UserInfo.Controls.Add(this.UserName);
            this.UserInfo.Location = new System.Drawing.Point(16, 15);
            this.UserInfo.Name = "UserInfo";
            this.UserInfo.Size = new System.Drawing.Size(456, 164);
            this.UserInfo.TabIndex = 4;
            this.UserInfo.TabStop = false;
            this.UserInfo.Text = "Informations Utilisateur";
            // 
            // cbEnabled
            // 
            this.cbEnabled.AutoSize = true;
            this.cbEnabled.Location = new System.Drawing.Point(106, 131);
            this.cbEnabled.Name = "cbEnabled";
            this.cbEnabled.Size = new System.Drawing.Size(157, 17);
            this.cbEnabled.TabIndex = 3;
            this.cbEnabled.Text = "Authentification MFA Active";
            this.cbEnabled.UseVisualStyleBackColor = true;
            this.cbEnabled.CheckedChanged += new System.EventHandler(this.cbEnabled_CheckedChanged);
            // 
            // Email
            // 
            this.Email.Location = new System.Drawing.Point(106, 57);
            this.Email.Name = "Email";
            this.Email.Size = new System.Drawing.Size(328, 20);
            this.Email.TabIndex = 1;
            this.Email.TextChanged += new System.EventHandler(this.Email_TextChanged);
            // 
            // EmailPrompt
            // 
            this.EmailPrompt.AutoSize = true;
            this.EmailPrompt.Location = new System.Drawing.Point(16, 60);
            this.EmailPrompt.Name = "EmailPrompt";
            this.EmailPrompt.Size = new System.Drawing.Size(81, 13);
            this.EmailPrompt.TabIndex = 4;
            this.EmailPrompt.Text = "Adresse email : ";
            // 
            // groupBoxKey
            // 
            this.groupBoxKey.Controls.Add(this.clearkeyBtn);
            this.groupBoxKey.Controls.Add(this.newkeyBtn);
            this.groupBoxKey.Controls.Add(this.DisplayKey);
            this.groupBoxKey.Location = new System.Drawing.Point(16, 193);
            this.groupBoxKey.Name = "groupBoxKey";
            this.groupBoxKey.Size = new System.Drawing.Size(456, 174);
            this.groupBoxKey.TabIndex = 6;
            this.groupBoxKey.TabStop = false;
            this.groupBoxKey.Text = "Secret";
            // 
            // newkeyBtn
            // 
            this.newkeyBtn.Location = new System.Drawing.Point(332, 137);
            this.newkeyBtn.Name = "newkeyBtn";
            this.newkeyBtn.Size = new System.Drawing.Size(102, 23);
            this.newkeyBtn.TabIndex = 6;
            this.newkeyBtn.Text = "Nouvelle Key";
            this.newkeyBtn.UseVisualStyleBackColor = true;
            this.newkeyBtn.Click += new System.EventHandler(this.newkeyBtn_Click);
            // 
            // DisplayKey
            // 
            this.DisplayKey.Location = new System.Drawing.Point(19, 27);
            this.DisplayKey.Multiline = true;
            this.DisplayKey.Name = "DisplayKey";
            this.DisplayKey.ReadOnly = true;
            this.DisplayKey.Size = new System.Drawing.Size(415, 98);
            this.DisplayKey.TabIndex = 6;
            this.DisplayKey.TabStop = false;
            // 
            // clearkeyBtn
            // 
            this.clearkeyBtn.Location = new System.Drawing.Point(19, 136);
            this.clearkeyBtn.Name = "clearkeyBtn";
            this.clearkeyBtn.Size = new System.Drawing.Size(102, 23);
            this.clearkeyBtn.TabIndex = 5;
            this.clearkeyBtn.Text = "Effacer Key";
            this.clearkeyBtn.UseVisualStyleBackColor = true;
            this.clearkeyBtn.Click += new System.EventHandler(this.clearkeyBtn_Click);
            // 
            // UserPropertiesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxKey);
            this.Controls.Add(this.PhonePrompt);
            this.Controls.Add(this.UserInfo);
            this.Name = "UserPropertiesControl";
            this.Size = new System.Drawing.Size(489, 385);
            this.UserInfo.ResumeLayout(false);
            this.UserInfo.PerformLayout();
            this.groupBoxKey.ResumeLayout(false);
            this.groupBoxKey.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Phone;
        private System.Windows.Forms.Label PhonePrompt;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.Label UserNamePrompt;
        private System.Windows.Forms.GroupBox UserInfo;
        private System.Windows.Forms.TextBox Email;
        private System.Windows.Forms.Label EmailPrompt;
        private System.Windows.Forms.CheckBox cbEnabled;
        private System.Windows.Forms.GroupBox groupBoxKey;
        private System.Windows.Forms.TextBox DisplayKey;
        private System.Windows.Forms.Button newkeyBtn;
        private System.Windows.Forms.Button clearkeyBtn;
    }
}
