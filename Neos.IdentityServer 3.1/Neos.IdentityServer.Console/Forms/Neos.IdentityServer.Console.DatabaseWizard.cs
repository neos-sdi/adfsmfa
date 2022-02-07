//******************************************************************************************************************************************************************************************//
// Copyright (c) 2022 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neos.IdentityServer.Console
{
    public partial class DatabaseWizard : Form
    {

        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)]string lParam);

        ComponentResourceManager resources;
        public DatabaseWizard()
        {
            resources = new ComponentResourceManager(typeof(DatabaseWizard));
            InitializeComponent();
            SendMessage(txtAccount.Handle, EM_SETCUEBANNER, 0, "    ADFS account    ");
        }

        /// <summary>
        /// cbWindows_CheckedChanged method
        /// </summary>
        private void cbWindows_CheckedChanged(object sender, EventArgs e)
        {
            if (cbWindows.Checked)
                txtPwd.Enabled = false;
            else
                txtPwd.Enabled = true;
        }

        /// <summary>
        /// txtInstance_Validating method
        /// </summary>
        private void txtInstance_Validating(object sender, CancelEventArgs e)
        {
            if ((string.IsNullOrEmpty(txtInstance.Text)) || (string.IsNullOrWhiteSpace(txtInstance.Text)))
            {
                e.Cancel = true;
                errors.SetError(txtInstance, resources.GetString("DBWIZINSTANCEREQUIRED"));
            }
        }

        /// <summary>
        /// txtDBName_Validating method
        /// </summary>
        private void txtDBName_Validating(object sender, CancelEventArgs e)
        {
            if ((string.IsNullOrEmpty(txtDBName.Text)) || (string.IsNullOrWhiteSpace(txtDBName.Text)))
            {
                e.Cancel = true;
                errors.SetError(txtDBName, resources.GetString("DBWIZNAMEREQUIRED"));
            }
        }

        /// <summary>
        /// txtAccount_Validating method
        /// </summary>
        private void txtAccount_Validating(object sender, CancelEventArgs e)
        {
            if ((string.IsNullOrEmpty(txtAccount.Text)) || (string.IsNullOrWhiteSpace(txtAccount.Text)))
            {
                e.Cancel = true;
                errors.SetError(txtDBName, resources.GetString("DBWIZACCOUNTREQUIRED"));
            }
        }

        /// <summary>
        /// txtPwd_Validating method
        /// </summary>
        private void txtPwd_Validating(object sender, CancelEventArgs e)
        {
            if (!cbWindows.Checked)
            {
                if ((string.IsNullOrEmpty(txtPwd.Text)) || (string.IsNullOrWhiteSpace(txtPwd.Text)))
                {
                    e.Cancel = true;
                    errors.SetError(txtPwd, resources.GetString("DBWIZPASSREQUIRED"));
                }
            }
        }

        /// <summary>
        /// txtInstance_Validated method
        /// </summary>
        private void txtInstance_Validated(object sender, EventArgs e)
        {
            errors.SetError(txtInstance, "");
        }

        /// <summary>
        /// txtDBName_Validated method
        /// </summary>
        private void txtDBName_Validated(object sender, EventArgs e)
        {
            errors.SetError(txtDBName, "");
        }

        /// <summary>
        /// txtAccount_Validated method
        /// </summary>
        private void txtAccount_Validated(object sender, EventArgs e)
        {
            errors.SetError(txtAccount, "");
        }

        /// <summary>
        /// txtPwd_Validated method
        /// </summary>
        private void txtPwd_Validated(object sender, EventArgs e)
        {
            errors.SetError(txtPwd, "");
        }

        /// <summary>
        /// Click OK method
        /// </summary>
        private void OKBtn_Click(object sender, EventArgs e)
        {
            if (!this.ValidateChildren())
                this.DialogResult = DialogResult.None;
            else
                this.DialogResult = DialogResult.OK;
        }
    }
}
