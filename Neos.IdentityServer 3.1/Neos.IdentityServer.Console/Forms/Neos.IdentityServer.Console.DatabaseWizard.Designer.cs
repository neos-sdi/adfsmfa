namespace Neos.IdentityServer.Console
{
    partial class DatabaseWizard
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseWizard));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtPwd = new System.Windows.Forms.TextBox();
            this.txtAccount = new System.Windows.Forms.TextBox();
            this.txtDBName = new System.Windows.Forms.TextBox();
            this.txtInstance = new System.Windows.Forms.TextBox();
            this.cbWindows = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.OKBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.errors = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errors)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.txtPwd);
            this.groupBox1.Controls.Add(this.txtAccount);
            this.groupBox1.Controls.Add(this.txtDBName);
            this.groupBox1.Controls.Add(this.txtInstance);
            this.groupBox1.Controls.Add(this.cbWindows);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.errors.SetError(this.groupBox1, resources.GetString("groupBox1.Error"));
            this.errors.SetIconAlignment(this.groupBox1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("groupBox1.IconAlignment"))));
            this.errors.SetIconPadding(this.groupBox1, ((int)(resources.GetObject("groupBox1.IconPadding"))));
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // txtPwd
            // 
            resources.ApplyResources(this.txtPwd, "txtPwd");
            this.errors.SetError(this.txtPwd, resources.GetString("txtPwd.Error"));
            this.errors.SetIconAlignment(this.txtPwd, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtPwd.IconAlignment"))));
            this.errors.SetIconPadding(this.txtPwd, ((int)(resources.GetObject("txtPwd.IconPadding"))));
            this.txtPwd.Name = "txtPwd";
            this.txtPwd.Validating += new System.ComponentModel.CancelEventHandler(this.txtPwd_Validating);
            this.txtPwd.Validated += new System.EventHandler(this.txtPwd_Validated);
            // 
            // txtAccount
            // 
            resources.ApplyResources(this.txtAccount, "txtAccount");
            this.errors.SetError(this.txtAccount, resources.GetString("txtAccount.Error"));
            this.errors.SetIconAlignment(this.txtAccount, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtAccount.IconAlignment"))));
            this.errors.SetIconPadding(this.txtAccount, ((int)(resources.GetObject("txtAccount.IconPadding"))));
            this.txtAccount.Name = "txtAccount";
            this.txtAccount.Validating += new System.ComponentModel.CancelEventHandler(this.txtAccount_Validating);
            this.txtAccount.Validated += new System.EventHandler(this.txtAccount_Validated);
            // 
            // txtDBName
            // 
            resources.ApplyResources(this.txtDBName, "txtDBName");
            this.errors.SetError(this.txtDBName, resources.GetString("txtDBName.Error"));
            this.errors.SetIconAlignment(this.txtDBName, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtDBName.IconAlignment"))));
            this.errors.SetIconPadding(this.txtDBName, ((int)(resources.GetObject("txtDBName.IconPadding"))));
            this.txtDBName.Name = "txtDBName";
            this.txtDBName.Validating += new System.ComponentModel.CancelEventHandler(this.txtDBName_Validating);
            this.txtDBName.Validated += new System.EventHandler(this.txtDBName_Validated);
            // 
            // txtInstance
            // 
            resources.ApplyResources(this.txtInstance, "txtInstance");
            this.errors.SetError(this.txtInstance, resources.GetString("txtInstance.Error"));
            this.errors.SetIconAlignment(this.txtInstance, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("txtInstance.IconAlignment"))));
            this.errors.SetIconPadding(this.txtInstance, ((int)(resources.GetObject("txtInstance.IconPadding"))));
            this.txtInstance.Name = "txtInstance";
            this.txtInstance.Validating += new System.ComponentModel.CancelEventHandler(this.txtInstance_Validating);
            this.txtInstance.Validated += new System.EventHandler(this.txtInstance_Validated);
            // 
            // cbWindows
            // 
            resources.ApplyResources(this.cbWindows, "cbWindows");
            this.cbWindows.CausesValidation = false;
            this.cbWindows.Checked = true;
            this.cbWindows.CheckState = System.Windows.Forms.CheckState.Checked;
            this.errors.SetError(this.cbWindows, resources.GetString("cbWindows.Error"));
            this.errors.SetIconAlignment(this.cbWindows, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("cbWindows.IconAlignment"))));
            this.errors.SetIconPadding(this.cbWindows, ((int)(resources.GetObject("cbWindows.IconPadding"))));
            this.cbWindows.Name = "cbWindows";
            this.cbWindows.UseVisualStyleBackColor = true;
            this.cbWindows.CheckedChanged += new System.EventHandler(this.cbWindows_CheckedChanged);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.errors.SetError(this.label4, resources.GetString("label4.Error"));
            this.errors.SetIconAlignment(this.label4, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label4.IconAlignment"))));
            this.errors.SetIconPadding(this.label4, ((int)(resources.GetObject("label4.IconPadding"))));
            this.label4.Name = "label4";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.errors.SetError(this.label3, resources.GetString("label3.Error"));
            this.errors.SetIconAlignment(this.label3, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label3.IconAlignment"))));
            this.errors.SetIconPadding(this.label3, ((int)(resources.GetObject("label3.IconPadding"))));
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.errors.SetError(this.label2, resources.GetString("label2.Error"));
            this.errors.SetIconAlignment(this.label2, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label2.IconAlignment"))));
            this.errors.SetIconPadding(this.label2, ((int)(resources.GetObject("label2.IconPadding"))));
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.errors.SetError(this.label1, resources.GetString("label1.Error"));
            this.errors.SetIconAlignment(this.label1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label1.IconAlignment"))));
            this.errors.SetIconPadding(this.label1, ((int)(resources.GetObject("label1.IconPadding"))));
            this.label1.Name = "label1";
            // 
            // OKBtn
            // 
            resources.ApplyResources(this.OKBtn, "OKBtn");
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.errors.SetError(this.OKBtn, resources.GetString("OKBtn.Error"));
            this.errors.SetIconAlignment(this.OKBtn, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("OKBtn.IconAlignment"))));
            this.errors.SetIconPadding(this.OKBtn, ((int)(resources.GetObject("OKBtn.IconPadding"))));
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // CancelBtn
            // 
            resources.ApplyResources(this.CancelBtn, "CancelBtn");
            this.CancelBtn.CausesValidation = false;
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.errors.SetError(this.CancelBtn, resources.GetString("CancelBtn.Error"));
            this.errors.SetIconAlignment(this.CancelBtn, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("CancelBtn.IconAlignment"))));
            this.errors.SetIconPadding(this.CancelBtn, ((int)(resources.GetObject("CancelBtn.IconPadding"))));
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.UseVisualStyleBackColor = true;
            // 
            // errors
            // 
            this.errors.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errors.ContainerControl = this;
            resources.ApplyResources(this.errors, "errors");
            // 
            // DatabaseWizard
            // 
            this.AcceptButton = this.OKBtn;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.CancelButton = this.CancelBtn;
            this.ControlBox = false;
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "DatabaseWizard";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errors)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        internal System.Windows.Forms.CheckBox cbWindows;
        internal System.Windows.Forms.TextBox txtPwd;
        internal System.Windows.Forms.TextBox txtAccount;
        internal System.Windows.Forms.TextBox txtDBName;
        internal System.Windows.Forms.TextBox txtInstance;
        private System.Windows.Forms.ErrorProvider errors;
    }
}