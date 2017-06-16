namespace Neos.IdentityServer.Console
{
    partial class UserCommonPropertiesControl
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
            this.MethodSource = new System.Windows.Forms.BindingSource(this.components);
            this.UserInfo = new System.Windows.Forms.GroupBox();
            this.listUsers = new System.Windows.Forms.ListView();
            this.UserName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.CBMethod = new System.Windows.Forms.ComboBox();
            this.cbEnabled = new System.Windows.Forms.CheckBox();
            this.UserNamesPrompt = new System.Windows.Forms.Label();
            this.BTNReinit = new System.Windows.Forms.Button();
            this.BTNSendByMail = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.MethodSource)).BeginInit();
            this.UserInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // MethodSource
            // 
            this.MethodSource.DataSource = typeof(Neos.IdentityServer.MultiFactor.Administration.UsersPreferredMethodList);
            // 
            // UserInfo
            // 
            this.UserInfo.Controls.Add(this.listUsers);
            this.UserInfo.Controls.Add(this.label1);
            this.UserInfo.Controls.Add(this.CBMethod);
            this.UserInfo.Controls.Add(this.cbEnabled);
            this.UserInfo.Controls.Add(this.UserNamesPrompt);
            this.UserInfo.Location = new System.Drawing.Point(14, 18);
            this.UserInfo.Name = "UserInfo";
            this.UserInfo.Size = new System.Drawing.Size(456, 227);
            this.UserInfo.TabIndex = 1;
            this.UserInfo.TabStop = false;
            this.UserInfo.Text = "Informations Utilisateurs";
            // 
            // listUsers
            // 
            this.listUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.UserName});
            this.listUsers.FullRowSelect = true;
            this.listUsers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listUsers.Location = new System.Drawing.Point(107, 26);
            this.listUsers.MultiSelect = false;
            this.listUsers.Name = "listUsers";
            this.listUsers.Size = new System.Drawing.Size(327, 112);
            this.listUsers.TabIndex = 1;
            this.listUsers.UseCompatibleStateImageBehavior = false;
            this.listUsers.View = System.Windows.Forms.View.List;
            // 
            // UserName
            // 
            this.UserName.Text = "Nom";
            this.UserName.Width = 200;
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
            this.CBMethod.TabIndex = 2;
            this.CBMethod.ValueMember = "ID";
            this.CBMethod.SelectedIndexChanged += new System.EventHandler(this.CBMethod_SelectedIndexChanged);
            // 
            // cbEnabled
            // 
            this.cbEnabled.AutoSize = true;
            this.cbEnabled.Location = new System.Drawing.Point(106, 193);
            this.cbEnabled.Name = "cbEnabled";
            this.cbEnabled.Size = new System.Drawing.Size(157, 17);
            this.cbEnabled.TabIndex = 3;
            this.cbEnabled.Text = "Authentification MFA Active";
            this.cbEnabled.UseVisualStyleBackColor = true;
            this.cbEnabled.CheckedChanged += new System.EventHandler(this.cbEnabled_CheckedChanged);
            // 
            // UserNamesPrompt
            // 
            this.UserNamesPrompt.AutoSize = true;
            this.UserNamesPrompt.Location = new System.Drawing.Point(16, 26);
            this.UserNamesPrompt.Name = "UserNamesPrompt";
            this.UserNamesPrompt.Size = new System.Drawing.Size(89, 13);
            this.UserNamesPrompt.TabIndex = 3;
            this.UserNamesPrompt.Text = "Liste Utilisateurs :";
            // 
            // BTNReinit
            // 
            this.BTNReinit.Location = new System.Drawing.Point(201, 282);
            this.BTNReinit.Name = "BTNReinit";
            this.BTNReinit.Size = new System.Drawing.Size(247, 23);
            this.BTNReinit.TabIndex = 2;
            this.BTNReinit.Text = "Réinitialiser toutes les clés";
            this.BTNReinit.UseVisualStyleBackColor = true;
            this.BTNReinit.Click += new System.EventHandler(this.BTNReinit_Click);
            // 
            // BTNSendByMail
            // 
            this.BTNSendByMail.Location = new System.Drawing.Point(201, 329);
            this.BTNSendByMail.Name = "BTNSendByMail";
            this.BTNSendByMail.Size = new System.Drawing.Size(247, 23);
            this.BTNSendByMail.TabIndex = 3;
            this.BTNSendByMail.Text = "Envoyer les clés par mail";
            this.BTNSendByMail.UseVisualStyleBackColor = true;
            this.BTNSendByMail.Click += new System.EventHandler(this.BTNSendByMail_Click);
            // 
            // UserCommonPropertiesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.BTNSendByMail);
            this.Controls.Add(this.BTNReinit);
            this.Controls.Add(this.UserInfo);
            this.Name = "UserCommonPropertiesControl";
            this.Size = new System.Drawing.Size(489, 461);
            this.Load += new System.EventHandler(this.UserCommonPropertiesControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.MethodSource)).EndInit();
            this.UserInfo.ResumeLayout(false);
            this.UserInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.BindingSource MethodSource;
        private System.Windows.Forms.GroupBox UserInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox CBMethod;
        private System.Windows.Forms.CheckBox cbEnabled;
        private System.Windows.Forms.Label UserNamesPrompt;
        private System.Windows.Forms.ListView listUsers;
        private System.Windows.Forms.ColumnHeader UserName;
        private System.Windows.Forms.Button BTNReinit;
        private System.Windows.Forms.Button BTNSendByMail;
    }
}
