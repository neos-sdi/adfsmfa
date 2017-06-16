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
    public partial class UserPropertiesControl : UserControl, IUserPropertiesDataObject
    {
        private UserPropertyPage userPropertyPage;

        /// <summary>
        /// Constructor
        /// </summary>
        public UserPropertiesControl(UserPropertyPage parentPropertyPage)
        {
            InitializeComponent();
            userPropertyPage = parentPropertyPage;
        }

        private void UserPropertiesControl_Load(object sender, EventArgs e)
        {
            MethodSource.DataSource = new UsersPreferredMethodList(false);
        }


        /// <summary>
        /// GetData method implmentation 
        /// </summary>
        public object GetUserControlData(object lst)
        {
            if (lst is List<MMCRegistration>)
            {
                MMCRegistration obj = ((List<MMCRegistration>)lst)[0];
                ((MMCRegistration)obj).UPN = this.UserName.Text;
                ((MMCRegistration)obj).MailAddress = this.Email.Text;
                ((MMCRegistration)obj).PhoneNumber = this.Phone.Text;
                ((MMCRegistration)obj).Enabled = this.cbEnabled.Checked;
                ((MMCRegistration)obj).PreferredMethod = (RegistrationPreferredMethod)((int)this.CBMethod.SelectedValue);
                userPropertyPage.Dirty = false;
            }
            return lst;
        }

        /// <summary>
        /// SetData method implmentation 
        /// </summary>
        public void SetUserControlData(object lst)
        {
            if (lst is List<MMCRegistration>)
            {
                MMCRegistration obj = ((List<MMCRegistration>)lst)[0];
                this.UserName.Text = ((MMCRegistration)obj).UPN;
                this.Email.Text = ((MMCRegistration)obj).MailAddress;
                this.Phone.Text = ((MMCRegistration)obj).PhoneNumber;
                this.cbEnabled.Checked = ((MMCRegistration)obj).Enabled;
                this.CBMethod.SelectedValue = (UsersPreferredMethod)(((MMCRegistration)obj).PreferredMethod);
                userPropertyPage.Dirty = false;
            }
        }

        /// <summary>
        /// AddData method implmentation 
        /// </summary>
        public object AddUserControlData(object lst)
        {
            if (lst is List<MMCRegistration>)
            {
                MMCRegistration obj = ((List<MMCRegistration>)lst)[0];
                this.UserName.Text = ((MMCRegistration)obj).UPN;
                this.Email.Text = ((MMCRegistration)obj).MailAddress;
                this.Phone.Text = ((MMCRegistration)obj).PhoneNumber;
                this.cbEnabled.Checked = ((MMCRegistration)obj).Enabled;
                this.CBMethod.SelectedValue = (UsersPreferredMethod)(((MMCRegistration)obj).PreferredMethod);
                userPropertyPage.Dirty = false;
            }
            return lst;
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

        /// <summary>
        /// IsValidEmail method implementation
        /// </summary>
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
            if (userPropertyPage != null)
                userPropertyPage.Dirty = true;
        }

        /// <summary>
        /// Email_TextChanged event
        /// </summary>
        private void Email_TextChanged(object sender, EventArgs e)
        {
            if (userPropertyPage != null)
                userPropertyPage.Dirty = true;
        }

        /// <summary>
        /// Phone_TextChanged event
        /// </summary>
        private void Phone_TextChanged(object sender, EventArgs e)
        {
            if (userPropertyPage != null)
                userPropertyPage.Dirty = true;
        }

        /// <summary>
        /// cbEnabled_CheckedChanged event
        /// </summary>
        private void cbEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (userPropertyPage != null)
                userPropertyPage.Dirty = true;
        }

        /// <summary>
        /// CBMethod_SelectedIndexChanged event
        /// </summary>
        private void CBMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (userPropertyPage!=null)
                userPropertyPage.Dirty = true;
        }
        #endregion
    }
}
