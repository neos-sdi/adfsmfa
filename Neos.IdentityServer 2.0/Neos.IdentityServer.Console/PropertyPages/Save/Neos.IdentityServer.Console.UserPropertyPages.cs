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
using Neos.IdentityServer.MultiFactor.Admin;
using Neos.IdentityServer.MultiFactor;

namespace Neos.IdentityServer.Console
{
     /// <summary>
    /// User property page.
    /// </summary>
    public class UserPropertyPage : PropertyPage
    {
        private IUserPropertiesDataObject userPropertiesControl = null;
        private UsersMMCListView usersFormView = null;
        private bool isnew = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UserPropertyPage(UsersMMCListView frm, Type usercontrol, bool isnewuser = false)
        {
            
            isnew = isnewuser;
            usersFormView = frm;
            if (usercontrol.Equals(typeof(UserPropertiesControl)))
            {
                this.Control = new UserPropertiesControl(this);
                userPropertiesControl = this.Control as IUserPropertiesDataObject;
                this.Title = "Général";
            }
            else
            {
                this.Control = new UserPropertiesKeysControl(this);
                userPropertiesControl = this.Control as IUserPropertiesDataObject;
                this.Title = "Clé";
            }
        }
        /// <summary>
        /// Initialize the notification for the page.  
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            userPropertiesControl.SetUserControlData(this.ParentSheet.SelectionObject);
        }

        /// <summary>
        /// When the Apply button is clicked, this method makes the changes take effect.		
        /// </summary>
        /// <returns></returns>
        protected override bool OnApply()
        {
            if (this.Dirty)
            {
                if (userPropertiesControl.CanApplyChanges())
                {
                    if (isnew)
                    {
                        List<MMCRegistration> reg = new List<MMCRegistration>();
                        if (usersFormView.SelectionData.SharedData.GetItem("@adfsmfanewuser") == null)
                        {
                            reg.Add(new MMCRegistration());
                            userPropertiesControl.GetUserControlData(reg);
                            WritableSharedDataItem sh = new WritableSharedDataItem("@adfsmfanewuser", false);
                            sh.SetData(reg[0]);
                            usersFormView.SelectionData.SharedData.Add(sh);
                        }
                        else if (usersFormView != null)
                        {
                            WritableSharedDataItem sh = usersFormView.SelectionData.SharedData.GetItem("@adfsmfanewuser");
                            MMCRegistration res = sh.GetData();
                            reg.Add(res);
                            usersFormView.SelectionData.SharedData.Remove("@adfsmfanewuser");
                            userPropertiesControl.GetUserControlData(reg);
                            usersFormView.AddUserStoreData(reg);
                        }
                    }
                    else
                    {
                        userPropertiesControl.GetUserControlData(this.ParentSheet.SelectionObject);
                        if (usersFormView != null)
                            usersFormView.SetUserStoreData(this.ParentSheet.SelectionObject);
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }


        protected override void OnSetActive()
        {
            return;
        }


        protected override bool OnKillActive()
        {
            return true;
        }
                
        /// <summary>
        /// When the OK or Close button is clicked, this method makes the changes take effect.
        /// </summary>
        /// <returns></returns>
        protected override bool OnOK()
        {
            return this.OnApply();
        }

        /// <summary>
        /// Indicates that the property sheet needs to be canceled.
        /// </summary>
        /// <returns></returns>
        protected override bool QueryCancel()
        {
            return true;
        }

        /// <summary>
        /// Action to be taken before the property sheet is destroyed.
        /// </summary>
        protected override void OnCancel()
        {
            userPropertiesControl.SetUserControlData(this.ParentSheet.SelectionObject);
        }

        /// <summary>
        /// Opportunity to perform cleanup operations.
        /// </summary>
        protected override void OnDestroy()
        {
        }
    } // class


} 