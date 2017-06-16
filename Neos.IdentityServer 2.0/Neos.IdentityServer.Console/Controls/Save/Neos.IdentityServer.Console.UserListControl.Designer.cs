namespace Neos.IdentityServer.Console
{
    partial class UsersFormViewControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UsersFormViewControl));
            this.UsersimageList = new System.Windows.Forms.ImageList(this.components);
            this.UserListView = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // UsersimageList
            // 
            this.UsersimageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("UsersimageList.ImageStream")));
            this.UsersimageList.TransparentColor = System.Drawing.Color.Transparent;
            this.UsersimageList.Images.SetKeyName(0, "dsuiext_4126.ico");
            this.UsersimageList.Images.SetKeyName(1, "dsuiext_4129.ico");
            // 
            // UserListView
            // 
            this.UserListView.BackColor = System.Drawing.SystemColors.Window;
            this.UserListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.UserListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UserListView.FullRowSelect = true;
            this.UserListView.HideSelection = false;
            this.UserListView.Location = new System.Drawing.Point(0, 0);
            this.UserListView.Margin = new System.Windows.Forms.Padding(15);
            this.UserListView.MultiSelect = false;
            this.UserListView.Name = "UserListView";
            this.UserListView.Size = new System.Drawing.Size(897, 479);
            this.UserListView.SmallImageList = this.UsersimageList;
            this.UserListView.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.UserListView.TabIndex = 3;
            this.UserListView.UseCompatibleStateImageBehavior = false;
            this.UserListView.View = System.Windows.Forms.View.Details;
            this.UserListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.UserListView_ColumnClick);
            this.UserListView.SelectedIndexChanged += new System.EventHandler(this.UserListView_SelectedIndexChanged);
            this.UserListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.UserListView_MouseClick);
            this.UserListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.UserListView_MouseDoubleClick);
            // 
            // UsersFormViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.UserListView);
            this.Name = "UsersFormViewControl";
            this.Size = new System.Drawing.Size(897, 479);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList UsersimageList;
        private System.Windows.Forms.ListView UserListView;


    }
}
