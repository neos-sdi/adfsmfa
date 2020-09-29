namespace Neos.IdentityServer.Console
{
    partial class UsersListView
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UsersListView));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.GridView = new System.Windows.Forms.DataGridView();
            this.IMG = new System.Windows.Forms.DataGridViewImageColumn();
            this.iDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.uPNDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mailAddressDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.phoneNumberDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.preferredMethodDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.enabledDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.PIN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OverrideMethod = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.secretKeyDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.GridView)).BeginInit();
            this.SuspendLayout();
            // 
            // GridView
            // 
            this.GridView.AllowUserToAddRows = false;
            this.GridView.AllowUserToDeleteRows = false;
            this.GridView.AllowUserToOrderColumns = true;
            this.GridView.AllowUserToResizeRows = false;
            this.GridView.BackgroundColor = System.Drawing.SystemColors.Window;
            this.GridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.GridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.GridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.GridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.GridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.IMG,
            this.iDDataGridViewTextBoxColumn,
            this.uPNDataGridViewTextBoxColumn,
            this.mailAddressDataGridViewTextBoxColumn,
            this.phoneNumberDataGridViewTextBoxColumn,
            this.preferredMethodDataGridViewTextBoxColumn,
            this.enabledDataGridViewCheckBoxColumn,
            this.PIN,
            this.OverrideMethod,
            this.secretKeyDataGridViewTextBoxColumn});
            resources.ApplyResources(this.GridView, "GridView");
            this.GridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.GridView.GridColor = System.Drawing.SystemColors.Window;
            this.GridView.Name = "GridView";
            this.GridView.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.GridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.GridView.RowHeadersVisible = false;
            this.GridView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.GridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.GridView.ShowCellErrors = false;
            this.GridView.ShowCellToolTips = false;
            this.GridView.ShowEditingIcon = false;
            this.GridView.ShowRowErrors = false;
            this.GridView.StandardTab = true;
            this.GridView.VirtualMode = true;
            this.GridView.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.GridView_CellValueNeeded);
            this.GridView.CellValuePushed += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.GridView_CellValuePushed);
            this.GridView.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.GridView_ColumnHeaderMouseClick);
            this.GridView.SelectionChanged += new System.EventHandler(this.GridView_SelectionChanged);
            this.GridView.DoubleClick += new System.EventHandler(this.GridView_DoubleClick);
            this.GridView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.GridView_KeyPress);
            // 
            // IMG
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.NullValue = "System.Drawing.Image";
            this.IMG.DefaultCellStyle = dataGridViewCellStyle2;
            this.IMG.Frozen = true;
            resources.ApplyResources(this.IMG, "IMG");
            this.IMG.Name = "IMG";
            this.IMG.ReadOnly = true;
            // 
            // iDDataGridViewTextBoxColumn
            // 
            resources.ApplyResources(this.iDDataGridViewTextBoxColumn, "iDDataGridViewTextBoxColumn");
            this.iDDataGridViewTextBoxColumn.Name = "iDDataGridViewTextBoxColumn";
            this.iDDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // uPNDataGridViewTextBoxColumn
            // 
            resources.ApplyResources(this.uPNDataGridViewTextBoxColumn, "uPNDataGridViewTextBoxColumn");
            this.uPNDataGridViewTextBoxColumn.Name = "uPNDataGridViewTextBoxColumn";
            this.uPNDataGridViewTextBoxColumn.ReadOnly = true;
            this.uPNDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // mailAddressDataGridViewTextBoxColumn
            // 
            resources.ApplyResources(this.mailAddressDataGridViewTextBoxColumn, "mailAddressDataGridViewTextBoxColumn");
            this.mailAddressDataGridViewTextBoxColumn.Name = "mailAddressDataGridViewTextBoxColumn";
            this.mailAddressDataGridViewTextBoxColumn.ReadOnly = true;
            this.mailAddressDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // phoneNumberDataGridViewTextBoxColumn
            // 
            resources.ApplyResources(this.phoneNumberDataGridViewTextBoxColumn, "phoneNumberDataGridViewTextBoxColumn");
            this.phoneNumberDataGridViewTextBoxColumn.Name = "phoneNumberDataGridViewTextBoxColumn";
            this.phoneNumberDataGridViewTextBoxColumn.ReadOnly = true;
            this.phoneNumberDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // preferredMethodDataGridViewTextBoxColumn
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.preferredMethodDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle3;
            resources.ApplyResources(this.preferredMethodDataGridViewTextBoxColumn, "preferredMethodDataGridViewTextBoxColumn");
            this.preferredMethodDataGridViewTextBoxColumn.Name = "preferredMethodDataGridViewTextBoxColumn";
            this.preferredMethodDataGridViewTextBoxColumn.ReadOnly = true;
            this.preferredMethodDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // enabledDataGridViewCheckBoxColumn
            // 
            resources.ApplyResources(this.enabledDataGridViewCheckBoxColumn, "enabledDataGridViewCheckBoxColumn");
            this.enabledDataGridViewCheckBoxColumn.Name = "enabledDataGridViewCheckBoxColumn";
            this.enabledDataGridViewCheckBoxColumn.ReadOnly = true;
            // 
            // PIN
            // 
            resources.ApplyResources(this.PIN, "PIN");
            this.PIN.Name = "PIN";
            this.PIN.ReadOnly = true;
            // 
            // OverrideMethod
            // 
            resources.ApplyResources(this.OverrideMethod, "OverrideMethod");
            this.OverrideMethod.Name = "OverrideMethod";
            this.OverrideMethod.ReadOnly = true;
            // 
            // secretKeyDataGridViewTextBoxColumn
            // 
            resources.ApplyResources(this.secretKeyDataGridViewTextBoxColumn, "secretKeyDataGridViewTextBoxColumn");
            this.secretKeyDataGridViewTextBoxColumn.Name = "secretKeyDataGridViewTextBoxColumn";
            this.secretKeyDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // UsersListView
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.Controls.Add(this.GridView);
            this.Name = "UsersListView";
            ((System.ComponentModel.ISupportInitialize)(this.GridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView GridView;
        private System.Windows.Forms.DataGridViewImageColumn IMG;
        private System.Windows.Forms.DataGridViewTextBoxColumn iDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn uPNDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn mailAddressDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn phoneNumberDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn preferredMethodDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn enabledDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn PIN;
        private System.Windows.Forms.DataGridViewTextBoxColumn OverrideMethod;
        private System.Windows.Forms.DataGridViewTextBoxColumn secretKeyDataGridViewTextBoxColumn;
    }
}
