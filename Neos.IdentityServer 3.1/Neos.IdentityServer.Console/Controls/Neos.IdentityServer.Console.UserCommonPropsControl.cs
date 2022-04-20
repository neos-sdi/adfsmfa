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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Neos.IdentityServer.MultiFactor.Administration;
using Neos.IdentityServer.MultiFactor;
using Microsoft.ManagementConsole.Advanced;
using Neos.IdentityServer.Console.Resources;

namespace Neos.IdentityServer.Console
{
    public partial class UserCommonPropertiesControl : UserControl, IUserPropertiesDataObject
    {
        private UserPropertyPage userPropertyPage;
        private bool _syncdisabled = false;

        /// <summary>
        /// UserPropertiesControl Constructor
        /// </summary>
        public UserCommonPropertiesControl(UserPropertyPage parentPropertyPage)
        {
            InitializeComponent();
            userPropertyPage = parentPropertyPage;
        }

        /// <summary>
        /// Load event implmentation
        /// </summary>
        private void UserCommonPropertiesControl_Load(object sender, EventArgs e)
        {
            MethodSource.DataSource = new MMCPreferredMethodList(false);
        }


        #region IUserPropertiesDataObject
        /// <summary>
        /// SyncDisabled property implmentation
        /// </summary>
        public bool SyncDisabled
        {
            get { return _syncdisabled; }
            set { _syncdisabled = value; }
        }

        /// <summary>
        /// GetUserControlData method implmentation
        /// </summary>
        public MFAUserList GetUserControlData(MFAUserList lst)
        {
            foreach (MFAUser obj in lst)
            {
                ((MFAUser)obj).Enabled = this.cbEnabled.Checked;
                ((MFAUser)obj).PreferredMethod = (PreferredMethod)((int)this.CBMethod.SelectedValue);
            }
            return lst;
        }

        /// <summary>
        /// SetUserControlData method implementation
        /// </summary>
        public void SetUserControlData(MFAUserList lst, bool disablesync)
        {
            SyncDisabled = disablesync;
            try
            {
                bool isset = false;
                this.listUsers.Items.Clear();
                foreach (MFAUser obj in lst)
                {
                    this.listUsers.Items.Add(((MFAUser)obj).UPN);
                    if (!isset)
                    {
                        this.cbEnabled.Checked = ((MFAUser)obj).Enabled;
                        this.CBMethod.SelectedValue = (PreferredMethod)(((MFAUser)obj).PreferredMethod);
                        isset = true;
                    }
                }
            }
            finally
            {
                SyncDisabled = false;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// cbEnabled_CheckedChanged event
        /// </summary>
        private void cbEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (!SyncDisabled)
                userPropertyPage.SyncSharedUserData(this, true);
        }

        /// <summary>
        /// CBMethod_SelectedIndexChanged event
        /// </summary>
        private void CBMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!SyncDisabled)
                userPropertyPage.SyncSharedUserData(this, true);
        }

        /// <summary>
        /// BTNReinit_Click event
        /// </summary>
        private void BTNReinit_Click(object sender, EventArgs e)
        {
            MFAUserList lst = userPropertyPage.GetSharedUserData();
            foreach (MFAUser reg in lst)
            {
                MMCService.NewUserKey(reg.UPN);
            }
            if (!SyncDisabled)
                userPropertyPage.SyncSharedUserData(this, true);

        }

        /// <summary>
        /// BTNSendByMail_Click event
        /// </summary>
        private void BTNSendByMail_Click(object sender, EventArgs e)
        {
            Cursor crs = this.Cursor;
            int cnt = 0;
            try
            {
                this.Cursor = Cursors.WaitCursor;
                MFAUserList lst = userPropertyPage.GetSharedUserData();
                foreach (MFAUser reg in lst)
                {
                    string secret = MMCService.GetEncodedUserKey(reg.UPN);
                    MMCService.SendKeyByEmail(reg.MailAddress, reg.UPN, secret);
                    cnt++;
                }
            }
            catch (Exception ex)
            {
                this.Cursor = crs;
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                userPropertyPage.ParentSheet.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = crs;
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = string.Format(errors_strings.InfosSendingMails, cnt),
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Information
                };
                userPropertyPage.ParentSheet.ShowDialog(messageBoxParameters);
            }
        }
        #endregion
    }
}
