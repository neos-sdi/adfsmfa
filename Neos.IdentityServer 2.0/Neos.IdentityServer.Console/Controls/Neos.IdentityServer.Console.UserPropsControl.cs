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
using System.Windows.Forms;
using Microsoft.ManagementConsole;
using Microsoft.ManagementConsole.Advanced;
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Administration;



namespace Neos.IdentityServer.Console
{
    public partial class UserPropertiesControl : UserControl, IUserPropertiesDataObject
    {
        private UserPropertyPage userPropertyPage;
        private bool _syncdisabled = false;

        /// <summary>
        /// UserPropertiesControl Constructor
        /// </summary>
        public UserPropertiesControl(UserPropertyPage parentPropertyPage)
        {
            InitializeComponent();
            userPropertyPage = parentPropertyPage;
        }

        /// <summary>
        /// Load event implmentation
        /// </summary>
        private void UserPropertiesControl_Load(object sender, EventArgs e)
        {
            MethodSource.DataSource = new UsersPreferredMethodList(false);
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
        /// GetData method implmentation 
        /// </summary>
        public MMCRegistrationList GetUserControlData(MMCRegistrationList lst)
        {
            MMCRegistration obj = ((MMCRegistrationList)lst)[0];
            ((MMCRegistration)obj).UPN = this.UserName.Text;
            ((MMCRegistration)obj).MailAddress = this.Email.Text;
            ((MMCRegistration)obj).PhoneNumber = this.Phone.Text;
            ((MMCRegistration)obj).Enabled = this.cbEnabled.Checked;
            ((MMCRegistration)obj).PreferredMethod = (RegistrationPreferredMethod)((int)this.CBMethod.SelectedValue);
            return lst;
        }

        /// <summary>
        /// SetData method implmentation 
        /// </summary>
        public void SetUserControlData(MMCRegistrationList lst, bool disablesync)
        {
            SyncDisabled = disablesync;
            try
            {
                MMCRegistration obj = ((MMCRegistrationList)lst)[0];
                this.UserName.Text = ((MMCRegistration)obj).UPN;
                this.Email.Text = ((MMCRegistration)obj).MailAddress;
                this.Phone.Text = ((MMCRegistration)obj).PhoneNumber;
                this.cbEnabled.Checked = ((MMCRegistration)obj).Enabled;
                this.CBMethod.SelectedValue = (UsersPreferredMethod)(((MMCRegistration)obj).PreferredMethod);
            }
            finally
            {
                SyncDisabled = false;
            }
        }
        #endregion


        #region Controls events
        /// <summary>
        /// UserName_TextChanged event
        /// </summary>
        private void UserName_TextChanged(object sender, System.EventArgs e)
        {
            if (!SyncDisabled)
                userPropertyPage.SyncSharedUserData(this, true);
        }

        /// <summary>
        /// Email_TextChanged event
        /// </summary>
        private void Email_TextChanged(object sender, EventArgs e)
        {
            if (!SyncDisabled)
                userPropertyPage.SyncSharedUserData(this, true);
        }

        /// <summary>
        /// Phone_TextChanged event
        /// </summary>
        private void Phone_TextChanged(object sender, EventArgs e)
        {
            if (!SyncDisabled)
                userPropertyPage.SyncSharedUserData(this, true);
        }

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
        #endregion
    }
}
