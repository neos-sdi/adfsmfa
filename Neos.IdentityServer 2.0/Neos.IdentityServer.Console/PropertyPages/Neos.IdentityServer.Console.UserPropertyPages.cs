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
using System.ComponentModel;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using Microsoft.ManagementConsole;
using Microsoft.ManagementConsole.Advanced;
using Neos.IdentityServer.MultiFactor.Administration;
using Neos.IdentityServer.MultiFactor;
// using Neos.IdentityServer.Console.PropertyPages;

namespace Neos.IdentityServer.Console
{
     /// <summary>
    /// User property page.
    /// </summary>
    public class UserPropertyPage : PropertyPage
    {
        private IUserPropertiesDataObject userPropertiesControl = null;
        private UsersFormView usersFormView = null;
        private bool isnew = false;
        private string seed = 0.ToString();

        /// <summary>
        /// Constructor.
        /// </summary>
        public UserPropertyPage(UsersFormView frm, Type usercontrol, int seeding, bool isnewuser = false)
        {
            isnew = isnewuser;
            seed = seeding.ToString();
            usersFormView = frm;
            if (usercontrol.Equals(typeof(UserPropertiesControl)))
            {
                this.Control = new UserPropertiesControl(this);
                userPropertiesControl = this.Control as IUserPropertiesDataObject;
                this.Title = "Général";
            }
            else if (usercontrol.Equals(typeof(UserPropertiesKeysControl)))
            {
                this.Control = new UserPropertiesKeysControl(this);
                userPropertiesControl = this.Control as IUserPropertiesDataObject;
                this.Title = "Clé";
            }
            else if (usercontrol.Equals(typeof(UserCommonPropertiesControl)))
            {
                this.Control = new UserCommonPropertiesControl(this);
                userPropertiesControl = this.Control as IUserPropertiesDataObject;
                this.Title = "Propriétés Communes";
            }

        }
        /// <summary>
        /// Initialize the notification for the page.  
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (usersFormView != null)
            {
                WritableSharedDataItem shareddata = usersFormView.SharedUserData.GetItem("@adfsmfa_useredit" + seed);
                if (shareddata == null)
                {
                    shareddata = new WritableSharedDataItem("@adfsmfa_useredit" + seed, false);
                    usersFormView.SharedUserData.Add(shareddata);
                }
                LoadSharedUserData(this.userPropertiesControl, true);
            }
            Dirty = false;
        }

        /// <summary>
        /// OnApply method implmentation
        /// </summary>
        /// <returns></returns>
        protected override bool OnApply()
        {
            if (Dirty)
            {
                MMCRegistrationList registrations = GetSharedUserData();
                foreach (MMCRegistration reg in registrations)
                {
                    if (CanApplyDataChanges(reg))
                    {
                        if (!reg.IsApplied)
                        {
                            if (isnew)
                                usersFormView.AddUserStoreData(registrations);
                            else
                                usersFormView.SetUserStoreData(registrations);
                        }
                        reg.IsApplied = true;
                    }
                    else
                    {
                        reg.IsApplied = false;
                        return false;
                    }
                }
                SetSharedUserData(registrations);
                userPropertiesControl.SetUserControlData(registrations, true);
                return true;
            }
            return true;
        }

        /// <summary>
        /// OnSetActive method implmentation
        /// </summary>
        protected override void OnSetActive()
        {
            base.OnSetActive();
            LoadSharedUserData(userPropertiesControl, true);
        }

        /// <summary>
        /// OnOK method implmentation.
        /// </summary>
        /// <returns></returns>
        protected override bool OnOK()
        {
            return this.OnApply();
        }

        /// <summary>
        /// QueryCancel method implementation
        /// </summary>
        /// <returns></returns>
        protected override bool QueryCancel()
        {
            return true;
        }

        /// <summary>
        /// OnCancel method implementation
        /// </summary>
        protected override void OnCancel()
        {
           // userPropertiesControl.SetUserControlData((MMCRegistrationList)this.ParentSheet.SelectionObject, true);
        }

        /// <summary>
        /// OnDestroy method implementation
        /// </summary>
        protected override void OnDestroy()
        {
            if (usersFormView != null)
            {
                usersFormView.SharedUserData.Remove("@adfsmfa_useredit" + seed);
            }
        }

        #region Data management
        /// <summary>
        /// SyncSharedUserData method implementation
        /// </summary>
        internal virtual void SyncSharedUserData(IUserPropertiesDataObject control, bool isdirty)
        {
            if (usersFormView == null)
                return;
            MMCRegistrationList registrations = GetSharedUserData();
            userPropertiesControl.GetUserControlData(registrations);
            foreach (MMCRegistration reg in registrations)
            {
                reg.IsApplied = false;
            }
            SetSharedUserData(registrations);
            if (!Destroyed) 
                Dirty = isdirty;
        }

        /// <summary>
        /// LoadSharedUserData method implementation
        /// </summary>
        internal virtual void LoadSharedUserData(IUserPropertiesDataObject control, bool disablesync = false)
        {
            if (usersFormView == null)
                return;
            MMCRegistrationList registrations = GetSharedUserData();
            userPropertiesControl.SetUserControlData(registrations, disablesync);
        }

        /// <summary>
        /// GetSharedUserData method implementation
        /// </summary>
        internal MMCRegistrationList GetSharedUserData()
        {
            if (usersFormView == null)
                return null;
            WritableSharedDataItem shareddata = usersFormView.SharedUserData.GetItem("@adfsmfa_useredit" + seed);
            if (shareddata == null)
                return new MMCRegistrationList();
            MMCRegistrationList registrations = shareddata.GetData();
            if (registrations == null)
            {
                registrations = (MMCRegistrationList)this.ParentSheet.SelectionObject;
                if (registrations == null)
                {
                    registrations = new MMCRegistrationList();
                    MMCRegistration reg = new MMCRegistration();
                    reg.Enabled = true;
                    registrations.Add(reg);
                }
                shareddata.SetData(registrations);
            }
            return registrations;
        }

        /// <summary>
        /// SetSharedUserData method implementation
        /// </summary>
        internal void SetSharedUserData(MMCRegistrationList registrations)
        {
            if (usersFormView == null)
                return;
            WritableSharedDataItem shareddata = usersFormView.SharedUserData.GetItem("@adfsmfa_useredit" + seed);
            if (shareddata==null)
                return;
            if (registrations == null)
            {
                registrations = (MMCRegistrationList)this.ParentSheet.SelectionObject;
                if (registrations == null)
                {
                    registrations = new MMCRegistrationList();
                    MMCRegistration reg = new MMCRegistration();
                    reg.Enabled = true;
                    registrations.Add(reg);
                }
            }
            shareddata.SetData(registrations);
        }

        /// <summary>
        /// CanApplyDataChanges method implementation
        /// </summary>
        private bool CanApplyDataChanges(MMCRegistration registration)
        {
            bool result = false;
            if (registration.IsApplied)
                return true;
            if (string.IsNullOrEmpty(registration.UPN))
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = "le nom de l'utilsateur ne peux être vide !";
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                ParentSheet.ShowDialog(messageBoxParameters);
                ParentSheet.SetActivePage(0);
            }
            else if (string.IsNullOrEmpty(KeysManager.ReadKey(registration.UPN)))
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = "Une clé numérique valide est requise pour générer des codes TOTP permettant de valider votre identité !";
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                ParentSheet.ShowDialog(messageBoxParameters);
                ParentSheet.SetActivePage(1);
            }
            else if (string.IsNullOrEmpty(registration.MailAddress))
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = "Une adresse de messagerie secondaire est requis pour recevoir les codes par e-mails !\rSouhaitez-vous continuer ?";
                messageBoxParameters.Buttons = MessageBoxButtons.YesNo;
                messageBoxParameters.Icon = MessageBoxIcon.Warning;
                if (ParentSheet.ShowDialog(messageBoxParameters) == DialogResult.Yes)
                    result = true;
                else
                    ParentSheet.SetActivePage(0);
            }
            else if (!ManagementAdminService.IsValidEmail(registration.MailAddress))
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = "Adresse de messagerie secondaire invalide !";
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                ParentSheet.ShowDialog(messageBoxParameters);
                ParentSheet.SetActivePage(0);
            }
            else if (string.IsNullOrEmpty(registration.PhoneNumber))
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = "Un N° de téléphone est requis pour recevoir les codes par SMS !\r\rSouhaitez-vous continuer ?";
                messageBoxParameters.Buttons = MessageBoxButtons.YesNo;
                messageBoxParameters.Icon = MessageBoxIcon.Warning;
                if (ParentSheet.ShowDialog(messageBoxParameters) == DialogResult.Yes)
                    result = true;
                else
                    ParentSheet.SetActivePage(0);
            }
            else if (!ManagementAdminService.IsValidPhone(registration.PhoneNumber))
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = "Un N° de téléphone valide est requis pour recevoir les codes par SMS !";
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                ParentSheet.ShowDialog(messageBoxParameters);
                ParentSheet.SetActivePage(0);
            }
            else
            {
                result = true;
            }
            return result;
        }

        #endregion
    } 
} 