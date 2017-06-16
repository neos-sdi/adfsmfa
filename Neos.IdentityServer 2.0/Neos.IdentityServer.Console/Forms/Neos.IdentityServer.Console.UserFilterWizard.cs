//******************************************************************************************************************************************************************************************//
// Copyright (c) 2017 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neos.IdentityServer.Console
{
    public partial class UsersFilterWizard : Form
    {
        public UsersFilterWizard()
        {
            InitializeComponent();
        }

        private void UsersFilterWizard_Load(object sender, EventArgs e)
        {
            bindingMethod.DataSource = new UsersPreferredMethodList(true);
            bindingOperator.DataSource = new UsersFilterOperatorList();
            bindingFields.DataSource = new UsersFilterFieldList();

            if (!ManagementAdminService.Filter.FilterisActive)
            {
                CBFields.SelectedValue = UsersFilterField.UserName;
                CBOperators.SelectedValue = UsersFilterOperator.Contains;
                CBMethod.SelectedValue = UsersPreferredMethod.None;
                CBEnabledOnly.Checked = false;
                cbNull.Checked = false;
            }
            else
            {
                CBFields.SelectedValue = ManagementAdminService.Filter.FilterField;
                CBOperators.SelectedValue = ManagementAdminService.Filter.FilterOperator;
                CBMethod.SelectedValue = ManagementAdminService.Filter.FilterMethod;
                CBEnabledOnly.Checked = ManagementAdminService.Filter.EnabledOnly;
                cbNull.Checked = (ManagementAdminService.Filter.FilterValue == null);
                if (string.IsNullOrEmpty(ManagementAdminService.Filter.FilterValue))
                    TXTValue.Text = string.Empty;
                else
                    TXTValue.Text = ManagementAdminService.Filter.FilterValue;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            ManagementAdminService.Filter.FilterField = (UsersFilterField)CBFields.SelectedValue;
            ManagementAdminService.Filter.FilterOperator = (UsersFilterOperator)CBOperators.SelectedValue;
            ManagementAdminService.Filter.FilterMethod = (UsersPreferredMethod)CBMethod.SelectedValue;
            ManagementAdminService.Filter.EnabledOnly = CBEnabledOnly.Checked;
            if (string.IsNullOrEmpty(TXTValue.Text))
            {
                if (cbNull.Checked)
                    ManagementAdminService.Filter.FilterValue = null;
                else
                    ManagementAdminService.Filter.FilterValue = string.Empty;
            }
            else
            {
                ManagementAdminService.Filter.FilterValue = TXTValue.Text;
            }
        }

        private void cbNull_CheckedChanged(object sender, EventArgs e)
        {
            if (cbNull.Checked)
                TXTValue.Text = string.Empty;
        }

    }
}
