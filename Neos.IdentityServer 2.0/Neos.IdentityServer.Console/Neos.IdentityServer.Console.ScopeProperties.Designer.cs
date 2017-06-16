namespace Neos.IdentityServer.Console
{
    partial class ScopePropertiesControl
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
            this.DisplayNamePrompt = new System.Windows.Forms.Label();
            this.DisplayName = new System.Windows.Forms.TextBox();
            this.ScopeInfo = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // DisplayNamePrompt
            // 
            this.DisplayNamePrompt.AutoSize = true;
            this.DisplayNamePrompt.Location = new System.Drawing.Point(45, 48);
            this.DisplayNamePrompt.Name = "DisplayNamePrompt";
            this.DisplayNamePrompt.Size = new System.Drawing.Size(69, 13);
            this.DisplayNamePrompt.TabIndex = 0;
            this.DisplayNamePrompt.Text = "DisplayName";
            // 
            // DisplayName
            // 
            this.DisplayName.Location = new System.Drawing.Point(135, 45);
            this.DisplayName.Name = "DisplayName";
            this.DisplayName.Size = new System.Drawing.Size(252, 20);
            this.DisplayName.TabIndex = 1;
            // 
            // ScopeInfo
            // 
            this.ScopeInfo.Location = new System.Drawing.Point(19, 20);
            this.ScopeInfo.Name = "ScopeInfo";
            this.ScopeInfo.Size = new System.Drawing.Size(388, 69);
            this.ScopeInfo.TabIndex = 2;
            this.ScopeInfo.TabStop = false;
            this.ScopeInfo.Text = "Scope Info";
            // 
            // ScopePropertiesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DisplayName);
            this.Controls.Add(this.DisplayNamePrompt);
            this.Controls.Add(this.ScopeInfo);
            this.Name = "ScopePropertiesControl";
            this.Size = new System.Drawing.Size(430, 110);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DisplayNamePrompt;
        private System.Windows.Forms.TextBox DisplayName;
        private System.Windows.Forms.GroupBox ScopeInfo;
    }
}
