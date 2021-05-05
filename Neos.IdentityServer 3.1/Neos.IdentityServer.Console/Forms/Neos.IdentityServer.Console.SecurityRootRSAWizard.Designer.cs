namespace Neos.IdentityServer.Console
{
    partial class RSAWizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RSAWizard));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rdioDeleteECDHKeys = new System.Windows.Forms.RadioButton();
            this.rdioDeployECDHKeys = new System.Windows.Forms.RadioButton();
            this.rdioAddNewECDHKeys = new System.Windows.Forms.RadioButton();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.rdioDeleteECDHKeys);
            this.groupBox1.Controls.Add(this.rdioDeployECDHKeys);
            this.groupBox1.Controls.Add(this.rdioAddNewECDHKeys);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // rdioDeleteECDHKeys
            // 
            resources.ApplyResources(this.rdioDeleteECDHKeys, "rdioDeleteECDHKeys");
            this.rdioDeleteECDHKeys.Name = "rdioDeleteECDHKeys";
            this.rdioDeleteECDHKeys.UseVisualStyleBackColor = true;
            this.rdioDeleteECDHKeys.CheckedChanged += new System.EventHandler(this.ECDHKeys_CheckedChanged);
            // 
            // rdioDeployECDHKeys
            // 
            resources.ApplyResources(this.rdioDeployECDHKeys, "rdioDeployECDHKeys");
            this.rdioDeployECDHKeys.Name = "rdioDeployECDHKeys";
            this.rdioDeployECDHKeys.UseVisualStyleBackColor = true;
            this.rdioDeployECDHKeys.CheckedChanged += new System.EventHandler(this.ECDHKeys_CheckedChanged);
            // 
            // rdioAddNewECDHKeys
            // 
            resources.ApplyResources(this.rdioAddNewECDHKeys, "rdioAddNewECDHKeys");
            this.rdioAddNewECDHKeys.Checked = true;
            this.rdioAddNewECDHKeys.Name = "rdioAddNewECDHKeys";
            this.rdioAddNewECDHKeys.TabStop = true;
            this.rdioAddNewECDHKeys.UseVisualStyleBackColor = true;
            this.rdioAddNewECDHKeys.CheckedChanged += new System.EventHandler(this.ECDHKeys_CheckedChanged);
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // RSAWizard
            // 
            this.AcceptButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "RSAWizard";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Load += new System.EventHandler(this.RSAWizard_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rdioDeleteECDHKeys;
        private System.Windows.Forms.RadioButton rdioDeployECDHKeys;
        private System.Windows.Forms.RadioButton rdioAddNewECDHKeys;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
