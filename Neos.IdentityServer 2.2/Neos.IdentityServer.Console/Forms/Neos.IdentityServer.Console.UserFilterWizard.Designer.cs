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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UsersFilterWizard));
            this.BTNOk = new System.Windows.Forms.Button();
            this.BTNCancel = new System.Windows.Forms.Button();
            this.TXTValue = new System.Windows.Forms.TextBox();
            this.GBFilters = new System.Windows.Forms.GroupBox();
            this.cbNull = new System.Windows.Forms.CheckBox();
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
            this.GBFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingMethod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingFields)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingOperator)).BeginInit();
            this.SuspendLayout();
            // 
            // BTNOk
            // 
            resources.ApplyResources(this.BTNOk, "BTNOk");
            this.BTNOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BTNOk.Name = "BTNOk";
            this.BTNOk.UseVisualStyleBackColor = true;
            this.BTNOk.Click += new System.EventHandler(this.button1_Click);
            // 
            // BTNCancel
            // 
            resources.ApplyResources(this.BTNCancel, "BTNCancel");
            this.BTNCancel.CausesValidation = false;
            this.BTNCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BTNCancel.Name = "BTNCancel";
            this.BTNCancel.UseVisualStyleBackColor = true;
            // 
            // TXTValue
            // 
            resources.ApplyResources(this.TXTValue, "TXTValue");
            this.TXTValue.CausesValidation = false;
            this.TXTValue.Name = "TXTValue";
            // 
            // GBFilters
            // 
            resources.ApplyResources(this.GBFilters, "GBFilters");
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
            this.GBFilters.Name = "GBFilters";
            this.GBFilters.TabStop = false;
            // 
            // cbNull
            // 
            resources.ApplyResources(this.cbNull, "cbNull");
            this.cbNull.Name = "cbNull";
            this.cbNull.UseVisualStyleBackColor = true;
            this.cbNull.CheckedChanged += new System.EventHandler(this.cbNull_CheckedChanged);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // CBMethod
            // 
            resources.ApplyResources(this.CBMethod, "CBMethod");
            this.CBMethod.DataSource = this.bindingMethod;
            this.CBMethod.DisplayMember = "Label";
            this.CBMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBMethod.FormattingEnabled = true;
            this.CBMethod.Name = "CBMethod";
            this.CBMethod.ValueMember = "ID";
            // 
            // bindingMethod
            // 
            this.bindingMethod.DataSource = typeof(Neos.IdentityServer.Console.MMCPreferredMethodList);
            // 
            // CBEnabledOnly
            // 
            resources.ApplyResources(this.CBEnabledOnly, "CBEnabledOnly");
            this.CBEnabledOnly.Name = "CBEnabledOnly";
            this.CBEnabledOnly.UseVisualStyleBackColor = true;
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
            // CBFields
            // 
            resources.ApplyResources(this.CBFields, "CBFields");
            this.CBFields.CausesValidation = false;
            this.CBFields.DataSource = this.bindingFields;
            this.CBFields.DisplayMember = "Label";
            this.CBFields.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBFields.FormattingEnabled = true;
            this.CBFields.Name = "CBFields";
            this.CBFields.ValueMember = "ID";
            // 
            // bindingFields
            // 
            this.bindingFields.DataSource = typeof(Neos.IdentityServer.Console.MMCFilterFieldList);
            // 
            // CBOperators
            // 
            resources.ApplyResources(this.CBOperators, "CBOperators");
            this.CBOperators.CausesValidation = false;
            this.CBOperators.DataSource = this.bindingOperator;
            this.CBOperators.DisplayMember = "Label";
            this.CBOperators.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBOperators.FormattingEnabled = true;
            this.CBOperators.Name = "CBOperators";
            this.CBOperators.ValueMember = "ID";
            // 
            // bindingOperator
            // 
            this.bindingOperator.DataSource = typeof(Neos.IdentityServer.Console.MMCFilterOperatorList);
            // 
            // UsersFilterWizard
            // 
            this.AcceptButton = this.BTNOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BTNCancel;
            this.ControlBox = false;
            this.Controls.Add(this.BTNCancel);
            this.Controls.Add(this.BTNOk);
            this.Controls.Add(this.GBFilters);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "UsersFilterWizard";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
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