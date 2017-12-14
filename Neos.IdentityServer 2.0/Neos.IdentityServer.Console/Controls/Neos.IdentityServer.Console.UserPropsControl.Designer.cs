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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserPropertiesControl));
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
            resources.ApplyResources(this.Phone, "Phone");
            this.Phone.Name = "Phone";
            this.Phone.TextChanged += new System.EventHandler(this.Phone_TextChanged);
            // 
            // PhonePrompt
            // 
            resources.ApplyResources(this.PhonePrompt, "PhonePrompt");
            this.PhonePrompt.Name = "PhonePrompt";
            // 
            // UserName
            // 
            resources.ApplyResources(this.UserName, "UserName");
            this.UserName.Name = "UserName";
            this.UserName.TextChanged += new System.EventHandler(this.UserName_TextChanged);
            // 
            // UserNamePrompt
            // 
            resources.ApplyResources(this.UserNamePrompt, "UserNamePrompt");
            this.UserNamePrompt.Name = "UserNamePrompt";
            // 
            // UserInfo
            // 
            resources.ApplyResources(this.UserInfo, "UserInfo");
            this.UserInfo.Controls.Add(this.label1);
            this.UserInfo.Controls.Add(this.CBMethod);
            this.UserInfo.Controls.Add(this.cbEnabled);
            this.UserInfo.Controls.Add(this.Email);
            this.UserInfo.Controls.Add(this.Phone);
            this.UserInfo.Controls.Add(this.EmailPrompt);
            this.UserInfo.Controls.Add(this.UserNamePrompt);
            this.UserInfo.Controls.Add(this.UserName);
            this.UserInfo.Name = "UserInfo";
            this.UserInfo.TabStop = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // CBMethod
            // 
            resources.ApplyResources(this.CBMethod, "CBMethod");
            this.CBMethod.DataSource = this.MethodSource;
            this.CBMethod.DisplayMember = "Label";
            this.CBMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBMethod.FormattingEnabled = true;
            this.CBMethod.Name = "CBMethod";
            this.CBMethod.ValueMember = "ID";
            this.CBMethod.SelectionChangeCommitted += new System.EventHandler(this.CBMethod_SelectedIndexChanged);
            // 
            // MethodSource
            // 
            this.MethodSource.DataSource = typeof(Neos.IdentityServer.Console.MMCPreferredMethodList);
            // 
            // cbEnabled
            // 
            resources.ApplyResources(this.cbEnabled, "cbEnabled");
            this.cbEnabled.Checked = true;
            this.cbEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEnabled.Name = "cbEnabled";
            this.cbEnabled.UseVisualStyleBackColor = true;
            this.cbEnabled.CheckedChanged += new System.EventHandler(this.cbEnabled_CheckedChanged);
            // 
            // Email
            // 
            resources.ApplyResources(this.Email, "Email");
            this.Email.Name = "Email";
            this.Email.TextChanged += new System.EventHandler(this.Email_TextChanged);
            // 
            // EmailPrompt
            // 
            resources.ApplyResources(this.EmailPrompt, "EmailPrompt");
            this.EmailPrompt.Name = "EmailPrompt";
            // 
            // UserPropertiesControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.PhonePrompt);
            this.Controls.Add(this.UserInfo);
            this.Name = "UserPropertiesControl";
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
