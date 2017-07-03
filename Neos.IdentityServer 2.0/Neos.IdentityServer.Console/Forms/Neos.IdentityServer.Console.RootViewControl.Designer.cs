namespace Neos.IdentityServer.Console
{
    partial class RootViewControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RootViewControl));
            this.GlobalViewlabel = new System.Windows.Forms.Label();
            this.GlobalViewLabelText = new System.Windows.Forms.Label();
            this.pictureNeosSdi = new System.Windows.Forms.PictureBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureNeosSdi)).BeginInit();
            this.SuspendLayout();
            // 
            // GlobalViewlabel
            // 
            this.GlobalViewlabel.AutoSize = true;
            this.GlobalViewlabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GlobalViewlabel.Location = new System.Drawing.Point(21, 16);
            this.GlobalViewlabel.Name = "GlobalViewlabel";
            this.GlobalViewlabel.Size = new System.Drawing.Size(182, 26);
            this.GlobalViewlabel.TabIndex = 0;
            this.GlobalViewlabel.Text = "Vue d\'ensemble";
            // 
            // GlobalViewLabelText
            // 
            this.GlobalViewLabelText.AutoSize = true;
            this.GlobalViewLabelText.Location = new System.Drawing.Point(26, 57);
            this.GlobalViewLabelText.Name = "GlobalViewLabelText";
            this.GlobalViewLabelText.Size = new System.Drawing.Size(647, 13);
            this.GlobalViewLabelText.TabIndex = 1;
            this.GlobalViewLabelText.Text = "L\'extension MFA pour ADFS 2012R2 ou ADFS 2016 permettent d\'utiliser l\'authentifia" +
    "ction multi-facteur (2FA) sans côuts supplémentaires";
            // 
            // pictureNeosSdi
            // 
            this.pictureNeosSdi.Image = ((System.Drawing.Image)(resources.GetObject("pictureNeosSdi.Image")));
            this.pictureNeosSdi.Location = new System.Drawing.Point(29, 87);
            this.pictureNeosSdi.Name = "pictureNeosSdi";
            this.pictureNeosSdi.Size = new System.Drawing.Size(250, 66);
            this.pictureNeosSdi.TabIndex = 2;
            this.pictureNeosSdi.TabStop = false;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel1.Location = new System.Drawing.Point(297, 87);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(250, 26);
            this.linkLabel1.TabIndex = 5;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://www.neos-sdi.com";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel2.Location = new System.Drawing.Point(25, 171);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(216, 20);
            this.linkLabel2.TabIndex = 6;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "https://adfsmfa.codeplex.com";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // linkLabel3
            // 
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel3.Location = new System.Drawing.Point(25, 201);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(325, 20);
            this.linkLabel3.TabIndex = 7;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "https://github.com/neos-sdi/adfsmfa/releases";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Enabled = false;
            this.richTextBox1.Location = new System.Drawing.Point(29, 240);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(625, 262);
            this.richTextBox1.TabIndex = 8;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // RootViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.linkLabel3);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.pictureNeosSdi);
            this.Controls.Add(this.GlobalViewLabelText);
            this.Controls.Add(this.GlobalViewlabel);
            this.Name = "RootViewControl";
            this.Size = new System.Drawing.Size(676, 514);
            ((System.ComponentModel.ISupportInitialize)(this.pictureNeosSdi)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label GlobalViewlabel;
        private System.Windows.Forms.Label GlobalViewLabelText;
        private System.Windows.Forms.PictureBox pictureNeosSdi;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}
