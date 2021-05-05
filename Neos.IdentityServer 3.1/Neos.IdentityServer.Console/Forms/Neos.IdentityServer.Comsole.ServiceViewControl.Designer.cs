namespace Neos.IdentityServer.Console
{
    partial class ServiceViewControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServiceViewControl));
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panelConfig = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panelServers = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.tableLayoutPanel.SuspendLayout();
            this.panelConfig.SuspendLayout();
            this.panelServers.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
            this.tableLayoutPanel.Controls.Add(this.panelConfig, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.panelServers, 0, 2);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // panelConfig
            // 
            this.panelConfig.BackColor = System.Drawing.SystemColors.Window;
            this.panelConfig.Controls.Add(this.label3);
            this.panelConfig.Controls.Add(this.label2);
            this.panelConfig.Controls.Add(this.label1);
            resources.ApplyResources(this.panelConfig, "panelConfig");
            this.panelConfig.Name = "panelConfig";
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
            // panelServers
            // 
            this.panelServers.BackColor = System.Drawing.SystemColors.Window;
            this.panelServers.Controls.Add(this.label4);
            resources.ApplyResources(this.panelServers, "panelServers");
            this.panelServers.Name = "panelServers";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // ServiceViewControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "ServiceViewControl";
            this.tableLayoutPanel.ResumeLayout(false);
            this.panelConfig.ResumeLayout(false);
            this.panelConfig.PerformLayout();
            this.panelServers.ResumeLayout(false);
            this.panelServers.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Panel panelConfig;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelServers;
        private System.Windows.Forms.Label label4;

    }
}
