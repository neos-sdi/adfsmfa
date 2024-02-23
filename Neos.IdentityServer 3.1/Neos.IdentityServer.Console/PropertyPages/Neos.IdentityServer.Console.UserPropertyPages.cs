//******************************************************************************************************************************************************************************************//
// Copyright (c) 2023 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using res = Neos.IdentityServer.Console.Resources.Neos_IdentityServer_Console_UserPropertyPages;

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
        private readonly string seed = 0.ToString();

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
                this.Title = res.PPAGEGENERALTITLE;
            }
            else if (usercontrol.Equals(typeof(UserPropertiesKeysControl)))
            {
                this.Control = new UserPropertiesKeysControl(this);
                userPropertiesControl = this.Control as IUserPropertiesDataObject;
                this.Title = res.PPAGEKEYSTITLE;
            }
            else if (usercontrol.Equals(typeof(UserCommonPropertiesControl)))
            {
                this.Control = new UserCommonPropertiesControl(this);
                userPropertiesControl = this.Control as IUserPropertiesDataObject;
                this.Title = res.PPAGECOMMONTITILE;
            }
            else if (usercontrol.Equals(typeof(UserAttestationsControl)))
            {
                this.Control = new UserAttestationsControl(this);
                userPropertiesControl = this.Control as IUserPropertiesDataObject;
                this.Title = res.PPAGEWEBAUTHNKEYSTITLE;
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
                MFAUserList registrations = GetSharedUserData();
                foreach (MFAUser reg in registrations)
                {
                    if (CanApplyDataChanges(reg, isnew))
                    {
                        if (!reg.IsApplied)
                        {
                            if (isnew)
                                usersFormView.AddUserStoreData(registrations);
                            else
                                usersFormView.SetUserStoreData(registrations);
                            isnew = false;
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
            MFAUserList registrations = GetSharedUserData();
            control.GetUserControlData(registrations);
            foreach (MFAUser reg in registrations)
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
            MFAUserList registrations = GetSharedUserData();
            control.SetUserControlData(registrations, disablesync);
        }

        /// <summary>
        /// GetSharedUserData method implementation
        /// </summary>
        internal MFAUserList GetSharedUserData()
        {
            if (usersFormView == null)
                return null;
            WritableSharedDataItem shareddata = usersFormView.SharedUserData.GetItem("@adfsmfa_useredit" + seed);
            if (shareddata == null)
                return new MFAUserList();
            MFAUserList registrations = shareddata.GetData();
            if (registrations == null)
            {
                registrations = (MFAUserList)this.ParentSheet.SelectionObject;
                if (registrations == null)
                {
                    registrations = new MFAUserList();
                    MFAUser reg = new MFAUser
                    {
                        Enabled = true
                    };
                    registrations.Add(reg);
                }
                shareddata.SetData(registrations);
            }
            return registrations;
        }

        /// <summary>
        /// SetSharedUserData method implementation
        /// </summary>
        internal void SetSharedUserData(MFAUserList registrations)
        {
            if (usersFormView == null)
                return;
            WritableSharedDataItem shareddata = usersFormView.SharedUserData.GetItem("@adfsmfa_useredit" + seed);
            if (shareddata==null)
                return;
            if (registrations == null)
            {
                registrations = (MFAUserList)this.ParentSheet.SelectionObject;
                if (registrations == null)
                {
                    registrations = new MFAUserList();
                    MFAUser reg = new MFAUser
                    {
                        Enabled = true
                    };
                    registrations.Add(reg);
                }
            }
            shareddata.SetData(registrations);
        }

        /// <summary>
        /// CanApplyDataChanges method implementation
        /// </summary>
        private bool CanApplyDataChanges(MFAUser registration, bool isnew)
        {
            bool result = true;
            if (registration.IsApplied)
                return result;
            IExternalProvider prov1 = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Code);
            if (prov1 != null)
            {
                if ((prov1.Enabled) && (prov1.IsRequired))
                {
                    if (string.IsNullOrEmpty(registration.UPN))
                    {
                        MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                        {
                            Text = res.PPAGEVALIDUSER,
                            Buttons = MessageBoxButtons.OK,
                            Icon = MessageBoxIcon.Error
                        };
                        ParentSheet.ShowDialog(messageBoxParameters);
                        ParentSheet.SetActivePage(0);
                        result = false;
                    }
                    else if ((!isnew) && (string.IsNullOrEmpty(MMCService.GetEncodedUserKey(registration.UPN))))
                    {
                        MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                        {
                            Text = res.PPAGEVALIDKEY,
                            Buttons = MessageBoxButtons.OK,
                            Icon = MessageBoxIcon.Error
                        };
                        ParentSheet.ShowDialog(messageBoxParameters);
                        ParentSheet.SetActivePage(0);
                        result = false;
                    }
                }
            }
            IExternalProvider prov2 = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Email);
            if (prov2 != null)
            {
                if ((prov2.Enabled) && (prov2.IsRequired))
                {
                    if (string.IsNullOrEmpty(registration.MailAddress))
                    {
                        MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                        {
                            Text = res.PPAGEVALIDMAIL,
                            Buttons = MessageBoxButtons.YesNo,
                            Icon = MessageBoxIcon.Warning
                        };
                        if (ParentSheet.ShowDialog(messageBoxParameters) == DialogResult.Yes)
                            result = true;
                        else
                        {
                            result = false;
                            ParentSheet.SetActivePage(0);
                        }
                    }
                    else if (!MMCService.IsValidEmail(registration.MailAddress))
                    {
                        MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                        {
                            Text = res.PPAGEINVALIDMAIL,
                            Buttons = MessageBoxButtons.OK,
                            Icon = MessageBoxIcon.Error
                        };
                        ParentSheet.ShowDialog(messageBoxParameters);
                        ParentSheet.SetActivePage(0);
                        result = false;
                    }
                }
            }
            IExternalProvider prov3 = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.External);
            if (prov3 != null)
            {
                if ((prov3.Enabled) && (prov3.IsRequired))
                {
                    if (string.IsNullOrEmpty(registration.PhoneNumber))
                    {
                        MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                        {
                            Text = res.PPAGEVALIDPHONE,
                            Buttons = MessageBoxButtons.YesNo,
                            Icon = MessageBoxIcon.Warning
                        };
                        if (ParentSheet.ShowDialog(messageBoxParameters) == DialogResult.Yes)
                            result = true;
                        else
                        {
                            result = false;
                            ParentSheet.SetActivePage(0);
                        }

                    }
                    else if (!MMCService.IsValidPhone(registration.PhoneNumber))
                    {
                        MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                        {
                            Text = res.PPAGEINVALIDPHONE,
                            Buttons = MessageBoxButtons.OK,
                            Icon = MessageBoxIcon.Error
                        };
                        ParentSheet.ShowDialog(messageBoxParameters);
                        ParentSheet.SetActivePage(0);
                        result = false;
                    }
                }
            }
            return result;
        }
        #endregion
    } 
} 