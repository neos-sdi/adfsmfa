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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserCommonPropertiesControl));
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
            this.MethodSource.DataSource = typeof(Neos.IdentityServer.Console.MMCPreferredMethodList);
            // 
            // UserInfo
            // 
            this.UserInfo.Controls.Add(this.listUsers);
            this.UserInfo.Controls.Add(this.label1);
            this.UserInfo.Controls.Add(this.CBMethod);
            this.UserInfo.Controls.Add(this.cbEnabled);
            this.UserInfo.Controls.Add(this.UserNamesPrompt);
            resources.ApplyResources(this.UserInfo, "UserInfo");
            this.UserInfo.Name = "UserInfo";
            this.UserInfo.TabStop = false;
            // 
            // listUsers
            // 
            this.listUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.UserName});
            this.listUsers.FullRowSelect = true;
            this.listUsers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            resources.ApplyResources(this.listUsers, "listUsers");
            this.listUsers.MultiSelect = false;
            this.listUsers.Name = "listUsers";
            this.listUsers.UseCompatibleStateImageBehavior = false;
            this.listUsers.View = System.Windows.Forms.View.List;
            // 
            // UserName
            // 
            resources.ApplyResources(this.UserName, "UserName");
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // CBMethod
            // 
            this.CBMethod.DataSource = this.MethodSource;
            this.CBMethod.DisplayMember = "Label";
            this.CBMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBMethod.FormattingEnabled = true;
            resources.ApplyResources(this.CBMethod, "CBMethod");
            this.CBMethod.Name = "CBMethod";
            this.CBMethod.ValueMember = "ID";
            this.CBMethod.SelectedIndexChanged += new System.EventHandler(this.CBMethod_SelectedIndexChanged);
            // 
            // cbEnabled
            // 
            resources.ApplyResources(this.cbEnabled, "cbEnabled");
            this.cbEnabled.Name = "cbEnabled";
            this.cbEnabled.UseVisualStyleBackColor = true;
            this.cbEnabled.CheckedChanged += new System.EventHandler(this.cbEnabled_CheckedChanged);
            // 
            // UserNamesPrompt
            // 
            resources.ApplyResources(this.UserNamesPrompt, "UserNamesPrompt");
            this.UserNamesPrompt.Name = "UserNamesPrompt";
            // 
            // BTNReinit
            // 
            resources.ApplyResources(this.BTNReinit, "BTNReinit");
            this.BTNReinit.Name = "BTNReinit";
            this.BTNReinit.UseVisualStyleBackColor = true;
            this.BTNReinit.Click += new System.EventHandler(this.BTNReinit_Click);
            // 
            // BTNSendByMail
            // 
            resources.ApplyResources(this.BTNSendByMail, "BTNSendByMail");
            this.BTNSendByMail.Name = "BTNSendByMail";
            this.BTNSendByMail.UseVisualStyleBackColor = true;
            this.BTNSendByMail.Click += new System.EventHandler(this.BTNSendByMail_Click);
            // 
            // UserCommonPropertiesControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.BTNSendByMail);
            this.Controls.Add(this.BTNReinit);
            this.Controls.Add(this.UserInfo);
            this.Name = "UserCommonPropertiesControl";
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
