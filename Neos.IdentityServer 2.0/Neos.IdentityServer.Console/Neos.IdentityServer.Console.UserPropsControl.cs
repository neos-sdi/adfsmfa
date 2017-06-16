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
using Neos.IdentityServer.MultiFactor.Admin;


namespace Neos.IdentityServer.Console
{
    public partial class UserPropertiesControl : UserControl, ISnapinDataObject
    {
        private UserPropertyPage userPropertyPage;
        private string _secretkey = string.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        public UserPropertiesControl(UserPropertyPage parentPropertyPage)
        {
            InitializeComponent();
            userPropertyPage = parentPropertyPage;
        }

        /// <summary>
        /// GetData method implmentation 
        /// </summary>
        public object GetData(object obj)
        {
            if (obj is Registration)
            {
                ((Registration)obj).UPN = this.UserName.Text;
                ((Registration)obj).MailAddress = this.Email.Text;
                ((Registration)obj).PhoneNumber = this.Phone.Text;
                ((Registration)obj).Enabled = this.cbEnabled.Checked;
                ((Registration)obj).SecretKey = _secretkey;
                userPropertyPage.Dirty = false;
            }
            return obj;
        }

        /// <summary>
        /// SetData method implmentation 
        /// </summary>
        public void SetData(object obj)
        {
            if (obj is Registration)
            {
                this.UserName.Text = ((Registration)obj).UPN;
                this.Email.Text = ((Registration)obj).MailAddress;
                this.Phone.Text = ((Registration)obj).PhoneNumber;
                this.cbEnabled.Checked = ((Registration)obj).Enabled;
                _secretkey = ((Registration)obj).SecretKey;
                this.DisplayKey.Text = ((Registration)obj).DisplayKey;

                userPropertyPage.Dirty = false;
            }
        }

        /// <summary>
        /// AddData method implmentation 
        /// </summary>
        public object AddData(object obj)
        {
            if (obj is Registration)
            {
                this.UserName.Text = ((Registration)obj).UPN;
                this.Email.Text = ((Registration)obj).MailAddress;
                this.Phone.Text = ((Registration)obj).PhoneNumber;
                this.cbEnabled.Checked = ((Registration)obj).Enabled;
                if (string.IsNullOrEmpty(((Registration)obj).SecretKey))
                    ((Registration)obj).SecretKey = RemoteAdminService.GetNewSecretKey();
                _secretkey = ((Registration)obj).SecretKey;
                this.DisplayKey.Text = ((Registration)obj).DisplayKey;

                userPropertyPage.Dirty = false;
            }
            return obj;
        }

        /// <summary>
        /// CanApplyChanges method implementation
        /// </summary>
        public bool CanApplyChanges()
        {
            bool result = false;

            if (UserName.Text.Trim().Length == 0)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = "le nom de l'utilsateur ne peux être vide !";
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                userPropertyPage.ParentSheet.ShowDialog(messageBoxParameters);
                UserName.Focus();
            }
            else if (Email.Text.Trim().Length == 0)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = "Une adresse de messagerie secondaire est requis pour recevoir les codes par e-mails !\rSouhaitez-vous continuer ?";
                messageBoxParameters.Buttons = MessageBoxButtons.YesNo;
                messageBoxParameters.Icon = MessageBoxIcon.Warning;
                if (userPropertyPage.ParentSheet.ShowDialog(messageBoxParameters) == DialogResult.Yes)
                    result = true;
                else
                    Email.Focus();
            }
            else if (!IsValidEmail(Email.Text))
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = "Adresse de messagerie secondaire invalide !";
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                userPropertyPage.ParentSheet.ShowDialog(messageBoxParameters);
                Email.Focus();
            }
            else if (Phone.Text.Trim().Length == 0)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = "Le N° de téléphone est requis pour recevoir les codes par SMS !\r\rSouhaitez-vous continuer ?";
                messageBoxParameters.Buttons = MessageBoxButtons.YesNo;
                messageBoxParameters.Icon = MessageBoxIcon.Warning;
                if (userPropertyPage.ParentSheet.ShowDialog(messageBoxParameters) == DialogResult.Yes)
                    result = true;
                else
                    Phone.Focus();
            }
            else
            {
                result = true;
            }
            return result;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #region Controls events
        /// <summary>
        /// UserName_TextChanged event
        /// </summary>
        private void UserName_TextChanged(object sender, System.EventArgs e)
        {
            userPropertyPage.Dirty = true;
        }

        /// <summary>
        /// Email_TextChanged event
        /// </summary>
        private void Email_TextChanged(object sender, EventArgs e)
        {
            userPropertyPage.Dirty = true;
        }

        /// <summary>
        /// Phone_TextChanged event
        /// </summary>
        private void Phone_TextChanged(object sender, EventArgs e)
        {
            userPropertyPage.Dirty = true;
        }

        /// <summary>
        /// cbEnabled_CheckedChanged event
        /// </summary>
        private void cbEnabled_CheckedChanged(object sender, EventArgs e)
        {
            userPropertyPage.Dirty = true;
        }

        /// <summary>
        /// newkeyBtn_Click event
        /// </summary>
        private void newkeyBtn_Click(object sender, EventArgs e)
        {
            _secretkey = RemoteAdminService.GetNewSecretKey();
            this.DisplayKey.Text = Base32.Encode(_secretkey);
            userPropertyPage.Dirty = true;          
        }

        /// <summary>
        /// clearkeyBtn_Click event
        /// </summary>
        private void clearkeyBtn_Click(object sender, EventArgs e)
        {
            _secretkey = string.Empty;
            this.DisplayKey.Text = string.Empty;
            userPropertyPage.Dirty = true;
        }
        #endregion
    }
}
