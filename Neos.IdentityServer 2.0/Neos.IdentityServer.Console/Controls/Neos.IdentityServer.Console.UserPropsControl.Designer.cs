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
            this.components = new System.ComponentModel.Container();
            this.Phone = new System.Windows.Forms.TextBox();
            this.PhonePrompt = new System.Windows.Forms.Label();
            this.UserName = new System.Windows.Forms.TextBox();
            this.UserNamePrompt = new System.Windows.Forms.Label();
            this.UserInfo = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CBMethod = new System.Windows.Forms.ComboBox();
            this.MethodSource = new System.Windows.Forms.BindingSource(this.components);
            this.cbEnabled = new System.Windows.Forms.CheckBox();
            this.Email = new System.Windows.Forms.TextBox();
            this.EmailPrompt = new System.Windows.Forms.Label();
            this.UserInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MethodSource)).BeginInit();
            this.SuspendLayout();
            // 
            // Phone
            // 
            this.Phone.Location = new System.Drawing.Point(106, 91);
            this.Phone.Name = "Phone";
            this.Phone.Size = new System.Drawing.Size(328, 20);
            this.Phone.TabIndex = 3;
            this.Phone.TextChanged += new System.EventHandler(this.Phone_TextChanged);
            // 
            // PhonePrompt
            // 
            this.PhonePrompt.AutoSize = true;
            this.PhonePrompt.Location = new System.Drawing.Point(32, 110);
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
            this.UserName.TabIndex = 1;
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
            this.UserInfo.Controls.Add(this.label1);
            this.UserInfo.Controls.Add(this.CBMethod);
            this.UserInfo.Controls.Add(this.cbEnabled);
            this.UserInfo.Controls.Add(this.Email);
            this.UserInfo.Controls.Add(this.Phone);
            this.UserInfo.Controls.Add(this.EmailPrompt);
            this.UserInfo.Controls.Add(this.UserNamePrompt);
            this.UserInfo.Controls.Add(this.UserName);
            this.UserInfo.Location = new System.Drawing.Point(16, 15);
            this.UserInfo.Name = "UserInfo";
            this.UserInfo.Size = new System.Drawing.Size(456, 227);
            this.UserInfo.TabIndex = 0;
            this.UserInfo.TabStop = false;
            this.UserInfo.Text = "Informations Utilisateur";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 135);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 39);
            this.label1.TabIndex = 6;
            this.label1.Text = "Méthode \r\nd\'identification\r\nséléctionnée : \r\n";
            // 
            // CBMethod
            // 
            this.CBMethod.DataSource = this.MethodSource;
            this.CBMethod.DisplayMember = "Label";
            this.CBMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBMethod.FormattingEnabled = true;
            this.CBMethod.Location = new System.Drawing.Point(106, 153);
            this.CBMethod.Name = "CBMethod";
            this.CBMethod.Size = new System.Drawing.Size(328, 21);
            this.CBMethod.TabIndex = 4;
            this.CBMethod.ValueMember = "ID";
            this.CBMethod.SelectionChangeCommitted += new System.EventHandler(this.CBMethod_SelectedIndexChanged);
            // 
            // MethodSource
            // 
            this.MethodSource.DataSource = typeof(Neos.IdentityServer.MultiFactor.Administration.UsersPreferredMethodList);
            // 
            // cbEnabled
            // 
            this.cbEnabled.AutoSize = true;
            this.cbEnabled.Checked = true;
            this.cbEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEnabled.Location = new System.Drawing.Point(106, 193);
            this.cbEnabled.Name = "cbEnabled";
            this.cbEnabled.Size = new System.Drawing.Size(157, 17);
            this.cbEnabled.TabIndex = 5;
            this.cbEnabled.Text = "Authentification MFA Active";
            this.cbEnabled.UseVisualStyleBackColor = true;
            this.cbEnabled.CheckedChanged += new System.EventHandler(this.cbEnabled_CheckedChanged);
            // 
            // Email
            // 
            this.Email.Location = new System.Drawing.Point(106, 57);
            this.Email.Name = "Email";
            this.Email.Size = new System.Drawing.Size(328, 20);
            this.Email.TabIndex = 2;
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
            // UserPropertiesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.PhonePrompt);
            this.Controls.Add(this.UserInfo);
            this.Name = "UserPropertiesControl";
            this.Size = new System.Drawing.Size(489, 626);
            this.Load += new System.EventHandler(this.UserPropertiesControl_Load);
            this.UserInfo.ResumeLayout(false);
            this.UserInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MethodSource)).EndInit();
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
        private System.Windows.Forms.ComboBox CBMethod;
        private System.Windows.Forms.BindingSource MethodSource;
        private System.Windows.Forms.Label label1;
    }
}
