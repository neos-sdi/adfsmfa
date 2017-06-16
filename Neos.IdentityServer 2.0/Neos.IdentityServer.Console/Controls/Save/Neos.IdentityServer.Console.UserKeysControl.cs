using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Neos.IdentityServer.MultiFactor.Admin;
using Neos.IdentityServer.MultiFactor;

namespace Neos.IdentityServer.Console
{
    public partial class UserPropertiesKeysControl : UserControl, IUserPropertiesDataObject
    {
        private UserPropertyPage userPropertyPage;
        private string _secretkey = string.Empty;
        private string _upn = string.Empty;

        public UserPropertiesKeysControl(UserPropertyPage parentPropertyPage)
        {
            InitializeComponent();
            userPropertyPage = parentPropertyPage;
        }

        private void UserPropertiesKeysControl_Load(object sender, EventArgs e)
        {
        }

        public object GetUserControlData(object lst)
        {
            if (lst is List<MMCRegistration>)
            {
                MMCRegistration obj = ((List<MMCRegistration>)lst)[0];
                ((MMCRegistration)obj).SecretKey = _secretkey;

                userPropertyPage.Dirty = false;
            }
            return lst;
        }

        public void SetUserControlData(object lst)
        {
            if (lst is List<MMCRegistration>)
            {
                MMCRegistration obj = ((List<MMCRegistration>)lst)[0];
                if (string.IsNullOrEmpty(((MMCRegistration)obj).SecretKey))
                    ((MMCRegistration)obj).SecretKey = RemoteAdminService.GetNewSecretKey();
                _secretkey = ((MMCRegistration)obj).SecretKey;
                _upn = ((MMCRegistration)obj).UPN;
                if (string.IsNullOrEmpty(((MMCRegistration)obj).MailAddress))
                    this.EmailPrompt.Text = "Adresse email : ";
                else
                    this.EmailPrompt.Text = string.Format("Adresse email : {0}", ((MMCRegistration)obj).MailAddress);
                this.DisplayKey.Text = ((MMCRegistration)obj).DisplayKey;
                if (!string.IsNullOrEmpty(_upn))
                    this.qrCodeGraphic.Text = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={0}", RemoteAdminService.Config.Issuer, _upn, this.DisplayKey.Text);
                else
                    this.qrCodeGraphic.Text = string.Empty;
                userPropertyPage.Dirty = false;
            }
        }

        public object AddUserControlData(object lst)
        {
            if (lst is List<MMCRegistration>)
            {
                MMCRegistration obj = ((List<MMCRegistration>)lst)[0];
                if (string.IsNullOrEmpty(((MMCRegistration)obj).SecretKey))
                    ((MMCRegistration)obj).SecretKey = RemoteAdminService.GetNewSecretKey();
                _secretkey = ((MMCRegistration)obj).SecretKey;
                _upn = ((MMCRegistration)obj).UPN;
                if (string.IsNullOrEmpty(((MMCRegistration)obj).MailAddress))
                    this.EmailPrompt.Text = "Adresse email : ";
                else
                    this.EmailPrompt.Text = string.Format("Adresse email : {0}", ((MMCRegistration)obj).MailAddress);
                this.DisplayKey.Text = ((MMCRegistration)obj).DisplayKey;
                if (!string.IsNullOrEmpty(_upn))
                    this.qrCodeGraphic.Text = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={0}", RemoteAdminService.Config.Issuer, _upn, this.DisplayKey.Text);
                else
                    this.qrCodeGraphic.Text = string.Empty;
                userPropertyPage.Dirty = false;
            }
            return lst;
        }


        public bool CanApplyChanges()
        {
            return true;
        }

        /// <summary>
        /// newkeyBtn_Click event
        /// </summary>
        private void newkeyBtn_Click(object sender, EventArgs e)
        {
            _secretkey = RemoteAdminService.GetNewSecretKey();
            this.DisplayKey.Text = Base32.Encode(_secretkey);
            this.qrCodeGraphic.Text = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={0}", RemoteAdminService.Config.Issuer, _upn, this.DisplayKey.Text); 
            userPropertyPage.Dirty = true;
        }

        /// <summary>
        /// clearkeyBtn_Click event
        /// </summary>
        private void clearkeyBtn_Click(object sender, EventArgs e)
        {
            _secretkey = string.Empty;
            this.DisplayKey.Text = string.Empty;
            this.qrCodeGraphic.Text = string.Empty;
            userPropertyPage.Dirty = true;
        }
    }
}
