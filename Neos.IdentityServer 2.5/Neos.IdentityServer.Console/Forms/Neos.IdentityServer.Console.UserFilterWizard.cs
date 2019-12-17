using Neos.IdentityServer.MultiFactor;
//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
// https://adfsmfa.codeplex.com                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using Neos.IdentityServer.MultiFactor.Administration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neos.IdentityServer.Console
{
    public partial class UsersFilterWizard : Form
    {
        /// <summary>
        /// UsersFilterWizard constructor
        /// </summary>
        public UsersFilterWizard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// UsersFilterWizard_Load method
        /// </summary>
        private void UsersFilterWizard_Load(object sender, EventArgs e)
        {
            bindingMethod.DataSource = new MMCPreferredMethodList(true);
            bindingOperator.DataSource = new MMCFilterOperatorList();
            bindingFields.DataSource = new MMCFilterFieldList();
            bindingRestrictedOperator.DataSource = new MMCFilterOperatorRestrictedList();

            if (!MMCService.Filter.FilterisActive)
            {
                CBFields.SelectedValue = DataFilterField.UserName;
                CBOperators.SelectedValue = DataFilterOperator.Contains;
                CBMethod.SelectedValue = PreferredMethod.None;
                CBEnabledOnly.Checked = false;
                cbNull.Checked = false;
            }
            else
            {
                CBFields.SelectedValue = MMCService.Filter.FilterField;
                CBOperators.SelectedValue = MMCService.Filter.FilterOperator;
                CBMethod.SelectedValue = MMCService.Filter.FilterMethod;
                CBEnabledOnly.Checked = MMCService.Filter.EnabledOnly;
                cbNull.Checked = (MMCService.Filter.FilterValue == null);
                if (string.IsNullOrEmpty(MMCService.Filter.FilterValue))
                    TXTValue.Text = string.Empty;
                else
                    TXTValue.Text = MMCService.Filter.FilterValue;
            }
        }

        /// <summary>
        /// button1_Click method
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            MMCService.Filter.FilterField = (DataFilterField)CBFields.SelectedValue;
            MMCService.Filter.FilterOperator = (DataFilterOperator)CBOperators.SelectedValue;
            MMCService.Filter.FilterMethod = (PreferredMethod)CBMethod.SelectedValue;
            MMCService.Filter.EnabledOnly = CBEnabledOnly.Checked;
            if (string.IsNullOrEmpty(TXTValue.Text))
            {
                if (cbNull.Checked)
                    MMCService.Filter.FilterValue = null;
                else
                    MMCService.Filter.FilterValue = string.Empty;
            }
            else
            {
                MMCService.Filter.FilterValue = TXTValue.Text;
            }
        }

        /// <summary>
        /// cbNull_CheckedChanged method
        /// </summary>
        private void cbNull_CheckedChanged(object sender, EventArgs e)
        {
            if (cbNull.Checked)
                TXTValue.Text = string.Empty;
        }

        /// <summary>
        /// CBFields_SelectedIndexChanged method 
        /// </summary>
        private void CBFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MMCService.IsSQLEncrypted)
            {
                switch (((MMCFilterFieldItem)this.CBFields.SelectedItem).ID)
                {
                    case DataFilterField.UserName:
                        if (this.CBOperators.DataSource != this.bindingOperator)
                        {
                            this.CBOperators.DataSource = this.bindingOperator;
                            this.CBOperators.DisplayMember = "Label";
                            this.CBOperators.ValueMember = "ID";
                        }
                        break;
                    case DataFilterField.Email:
                        if (this.CBOperators.DataSource != this.bindingRestrictedOperator)
                        {
                            this.CBOperators.DataSource = this.bindingRestrictedOperator;
                            this.CBOperators.DisplayMember = "Label";
                            this.CBOperators.ValueMember = "ID";
                        }
                        break;
                    case DataFilterField.PhoneNumber:
                        if (this.CBOperators.DataSource != this.bindingRestrictedOperator)
                        {
                            this.CBOperators.DataSource = this.bindingRestrictedOperator;
                            this.CBOperators.DisplayMember = "Label";
                            this.CBOperators.ValueMember = "ID";
                        }
                        break;
                    default:
                        if (this.CBOperators.DataSource != this.bindingOperator)
                        {
                            this.CBOperators.DataSource = this.bindingOperator;
                            this.CBOperators.DisplayMember = "Label";
                            this.CBOperators.ValueMember = "ID";
                        }
                        break;
                }
            }
        }
    }
}
