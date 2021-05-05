namespace Neos.IdentityServer.Console
{
    partial class StorageViewControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StorageViewControl));
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panelGlobal = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel.SuspendLayout();
            this.panelGlobal.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
            this.tableLayoutPanel.Controls.Add(this.panelGlobal, 0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // panelGlobal
            // 
            this.panelGlobal.Controls.Add(this.label3);
            this.panelGlobal.Controls.Add(this.label2);
            this.panelGlobal.Controls.Add(this.label1);
            resources.ApplyResources(this.panelGlobal, "panelGlobal");
            this.panelGlobal.Name = "panelGlobal";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // StorageViewControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "StorageViewControl";
            this.tableLayoutPanel.ResumeLayout(false);
            this.panelGlobal.ResumeLayout(false);
            this.panelGlobal.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Panel panelGlobal;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}
