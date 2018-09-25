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
    public partial class UserPropertiesKeysControl : UserControl, IUserPropertiesDataObject
    {
        private UserPropertyPage userPropertyPage;
        private string _secretkey = string.Empty;
        private string _upn = string.Empty;
        private bool _syncdisabled = false;
        private bool _emailnotset = false;
        private string _email;
        
        /// <summary>
        /// UserPropertiesKeysControl constructor
        /// </summary>
        public UserPropertiesKeysControl(UserPropertyPage parentPropertyPage)
        {
            InitializeComponent();
            userPropertyPage = parentPropertyPage;
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
        public RegistrationList GetUserControlData(RegistrationList lst)
        {
            Registration obj = lst[0];
            return lst;
        }

        /// <summary>
        /// SetUserControlData method implmentation
        /// </summary>
        public void SetUserControlData(RegistrationList lst, bool disablesync)
        {
            SyncDisabled = disablesync;
            try
            {
                Registration obj = ((RegistrationList)lst)[0];
                _secretkey = MMCService.GetEncodedUserKey(((Registration)obj).UPN);
               // _secretkey = KeysManager.EncodedKey(((Registration)obj).UPN);
                _upn = ((Registration)obj).UPN;
                _email = ((Registration)obj).MailAddress;

                if (string.IsNullOrEmpty(_email))
                {
                    this.EmailPrompt.Text = "Adresse email : ";
                    _emailnotset = true;
                }
                else
                {
                    this.EmailPrompt.Text = string.Format("Adresse email : {0}", _email);
                    _emailnotset = false;
                }
                if (!string.IsNullOrEmpty(_secretkey))
                {
                    this.DisplayKey.Text =_secretkey;
                    if (!string.IsNullOrEmpty(_upn))
                        this.qrCodeGraphic.Text = MMCService.GetQRCodeValue(_upn, this.DisplayKey.Text);
                    else
                        this.qrCodeGraphic.Text = string.Empty;
                }
                else
                    userPropertyPage.Dirty = true;
                UpdateControlsEnabled();
            }
            finally
            {
                SyncDisabled = false;
            }
        }
        #endregion

        /// <summary>
        /// newkeyBtn_Click event
        /// </summary>
        private void newkeyBtn_Click(object sender, EventArgs e)
        {
            MMCService.NewUserKey(_upn);
            _secretkey = MMCService.GetEncodedUserKey(_upn);
            this.DisplayKey.Text = _secretkey;
            this.qrCodeGraphic.Text = MMCService.GetQRCodeValue(_upn, this.DisplayKey.Text);
            if (!SyncDisabled)
                userPropertyPage.SyncSharedUserData(this, true);
        }

        /// <summary>
        /// clearkeyBtn_Click event
        /// </summary>
        private void clearkeyBtn_Click(object sender, EventArgs e)
        {
            _secretkey = string.Empty;
            this.DisplayKey.Text = string.Empty;
            this.qrCodeGraphic.Text = string.Empty;
            if (!SyncDisabled)
                userPropertyPage.SyncSharedUserData(this, true);
        }

        /// <summary>
        /// BTNSendByMail_Click event
        /// </summary>
        private void BTNSendByMail_Click(object sender, EventArgs e)
        {
            Cursor crs = this.Cursor;
            try
            {
                this.Cursor = Cursors.WaitCursor;
                 MMCService.SendKeyByEmail(_email, _upn, this.DisplayKey.Text);
            }
            catch (Exception ex)
            {
                this.Cursor = crs;
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                userPropertyPage.ParentSheet.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = crs;
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = string.Format(errors_strings.InfoSendingMailToUser, _email);
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Information;
                userPropertyPage.ParentSheet.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// EmailPrompt_TextChanged event
        /// </summary>
        private void EmailPrompt_TextChanged(object sender, EventArgs e)
        {
            if (!SyncDisabled)
                userPropertyPage.SyncSharedUserData(this, true);
            UpdateControlsEnabled();
        }

        /// <summary>
        /// UpdateControlsEnabled method implentation
        /// </summary>
        private void UpdateControlsEnabled()
        {
            if ((_emailnotset) || string.IsNullOrEmpty(_upn) || (string.IsNullOrEmpty(_secretkey)))
                BTNSendByMail.Enabled = false;
            else
                BTNSendByMail.Enabled = true;
        }
    }
}
