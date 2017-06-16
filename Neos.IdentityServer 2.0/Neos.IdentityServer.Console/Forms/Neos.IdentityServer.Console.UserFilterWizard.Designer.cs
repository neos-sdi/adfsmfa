namespace Neos.IdentityServer.Console
{
    partial class UsersFilterWizard
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
            this.BTNOk = new System.Windows.Forms.Button();
            this.BTNCancel = new System.Windows.Forms.Button();
            this.TXTValue = new System.Windows.Forms.TextBox();
            this.GBFilters = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.CBMethod = new System.Windows.Forms.ComboBox();
            this.bindingMethod = new System.Windows.Forms.BindingSource(this.components);
            this.CBEnabledOnly = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.CBFields = new System.Windows.Forms.ComboBox();
            this.bindingFields = new System.Windows.Forms.BindingSource(this.components);
            this.CBOperators = new System.Windows.Forms.ComboBox();
            this.bindingOperator = new System.Windows.Forms.BindingSource(this.components);
            this.cbNull = new System.Windows.Forms.CheckBox();
            this.GBFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingMethod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingFields)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingOperator)).BeginInit();
            this.SuspendLayout();
            // 
            // BTNOk
            // 
            this.BTNOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BTNOk.Location = new System.Drawing.Point(383, 205);
            this.BTNOk.Name = "BTNOk";
            this.BTNOk.Size = new System.Drawing.Size(75, 23);
            this.BTNOk.TabIndex = 7;
            this.BTNOk.Text = "OK";
            this.BTNOk.UseVisualStyleBackColor = true;
            this.BTNOk.Click += new System.EventHandler(this.button1_Click);
            // 
            // BTNCancel
            // 
            this.BTNCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BTNCancel.Location = new System.Drawing.Point(473, 205);
            this.BTNCancel.Name = "BTNCancel";
            this.BTNCancel.Size = new System.Drawing.Size(75, 23);
            this.BTNCancel.TabIndex = 8;
            this.BTNCancel.Text = "Cancel";
            this.BTNCancel.UseVisualStyleBackColor = true;
            // 
            // TXTValue
            // 
            this.TXTValue.CausesValidation = false;
            this.TXTValue.Location = new System.Drawing.Point(322, 55);
            this.TXTValue.Name = "TXTValue";
            this.TXTValue.Size = new System.Drawing.Size(194, 20);
            this.TXTValue.TabIndex = 3;
            // 
            // GBFilters
            // 
            this.GBFilters.Controls.Add(this.cbNull);
            this.GBFilters.Controls.Add(this.label4);
            this.GBFilters.Controls.Add(this.CBMethod);
            this.GBFilters.Controls.Add(this.CBEnabledOnly);
            this.GBFilters.Controls.Add(this.label3);
            this.GBFilters.Controls.Add(this.label2);
            this.GBFilters.Controls.Add(this.label1);
            this.GBFilters.Controls.Add(this.CBFields);
            this.GBFilters.Controls.Add(this.CBOperators);
            this.GBFilters.Controls.Add(this.TXTValue);
            this.GBFilters.Location = new System.Drawing.Point(13, 13);
            this.GBFilters.Name = "GBFilters";
            this.GBFilters.Size = new System.Drawing.Size(535, 179);
            this.GBFilters.TabIndex = 0;
            this.GBFilters.TabStop = false;
            this.GBFilters.Text = "Critères de filtrage";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 90);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(128, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Méthode d\'identification : ";
            // 
            // CBMethod
            // 
            this.CBMethod.DataSource = this.bindingMethod;
            this.CBMethod.DisplayMember = "Label";
            this.CBMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBMethod.FormattingEnabled = true;
            this.CBMethod.Location = new System.Drawing.Point(19, 109);
            this.CBMethod.Name = "CBMethod";
            this.CBMethod.Size = new System.Drawing.Size(297, 21);
            this.CBMethod.TabIndex = 5;
            this.CBMethod.ValueMember = "ID";
            // 
            // bindingMethod
            // 
            this.bindingMethod.DataSource = typeof(Neos.IdentityServer.MultiFactor.Administration.UsersPreferredMethodList);
            // 
            // CBEnabledOnly
            // 
            this.CBEnabledOnly.AutoSize = true;
            this.CBEnabledOnly.Location = new System.Drawing.Point(22, 142);
            this.CBEnabledOnly.Name = "CBEnabledOnly";
            this.CBEnabledOnly.Size = new System.Drawing.Size(163, 17);
            this.CBEnabledOnly.TabIndex = 6;
            this.CBEnabledOnly.Text = "Utilisateurs actifs uniquement";
            this.CBEnabledOnly.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(322, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Valeur :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(195, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Opérateur :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Propiétés :";
            // 
            // CBFields
            // 
            this.CBFields.CausesValidation = false;
            this.CBFields.DataSource = this.bindingFields;
            this.CBFields.DisplayMember = "Label";
            this.CBFields.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBFields.FormattingEnabled = true;
            this.CBFields.Location = new System.Drawing.Point(19, 54);
            this.CBFields.Name = "CBFields";
            this.CBFields.Size = new System.Drawing.Size(170, 21);
            this.CBFields.TabIndex = 1;
            this.CBFields.ValueMember = "ID";
            // 
            // bindingFields
            // 
            this.bindingFields.DataSource = typeof(Neos.IdentityServer.MultiFactor.Administration.UsersFilterFieldList);
            // 
            // CBOperators
            // 
            this.CBOperators.CausesValidation = false;
            this.CBOperators.DataSource = this.bindingOperator;
            this.CBOperators.DisplayMember = "Label";
            this.CBOperators.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBOperators.FormattingEnabled = true;
            this.CBOperators.Location = new System.Drawing.Point(195, 54);
            this.CBOperators.Name = "CBOperators";
            this.CBOperators.Size = new System.Drawing.Size(121, 21);
            this.CBOperators.TabIndex = 2;
            this.CBOperators.ValueMember = "ID";
            // 
            // bindingOperator
            // 
            this.bindingOperator.DataSource = typeof(Neos.IdentityServer.MultiFactor.Administration.UsersFilterOperatorList);
            // 
            // cbNull
            // 
            this.cbNull.AutoSize = true;
            this.cbNull.Location = new System.Drawing.Point(325, 85);
            this.cbNull.Name = "cbNull";
            this.cbNull.Size = new System.Drawing.Size(160, 17);
            this.cbNull.TabIndex = 4;
            this.cbNull.Text = "Rechercher une valeur nulle";
            this.cbNull.UseVisualStyleBackColor = true;
            this.cbNull.CheckedChanged += new System.EventHandler(this.cbNull_CheckedChanged);
            // 
            // UsersFilterWizard
            // 
            this.AcceptButton = this.BTNOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BTNCancel;
            this.ClientSize = new System.Drawing.Size(560, 241);
            this.ControlBox = false;
            this.Controls.Add(this.BTNCancel);
            this.Controls.Add(this.BTNOk);
            this.Controls.Add(this.GBFilters);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "UsersFilterWizard";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Filtrer les Utilisateurs";
            this.Load += new System.EventHandler(this.UsersFilterWizard_Load);
            this.GBFilters.ResumeLayout(false);
            this.GBFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingMethod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingFields)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingOperator)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BTNOk;
        private System.Windows.Forms.Button BTNCancel;
        private System.Windows.Forms.TextBox TXTValue;
        private System.Windows.Forms.GroupBox GBFilters;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox CBFields;
        private System.Windows.Forms.ComboBox CBOperators;
        private System.Windows.Forms.CheckBox CBEnabledOnly;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox CBMethod;
        private System.Windows.Forms.BindingSource bindingMethod;
        private System.Windows.Forms.BindingSource bindingOperator;
        private System.Windows.Forms.BindingSource bindingFields;
        private System.Windows.Forms.CheckBox cbNull;
    }
}