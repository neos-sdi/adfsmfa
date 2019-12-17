//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ManagementConsole;
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Administration;
using System.Diagnostics;
using Microsoft.ManagementConsole.Advanced;
using System.Windows.Forms;
using System.Threading;
using res = Neos.IdentityServer.Console.Resources.Neos_IdentityServer_Console_UsersFormView;

namespace Neos.IdentityServer.Console
{
    /// <summary>
    /// UsersFormView Class
    /// </summary>
    public class UsersFormView : FormView
    {
        private UsersListView usersControl = null;
        private UsersScopeNode usersScopeNode = null;
        private WritableSharedData _sharedUserData = null;

        /// <summary>
        /// UsersFormView Constructor
        /// </summary>
        public UsersFormView()
        {
            _sharedUserData = new WritableSharedData();
        }

        /// <summary>
        /// SharedUserData property
        /// </summary>
        public WritableSharedData SharedUserData
        {
            get { return _sharedUserData; }
        }

        /// <summary>
        /// Initialize method override
        /// </summary>
        protected override void OnInitialize(AsyncStatus status)
        {
            usersControl = (UsersListView)this.Control;
            usersScopeNode = (UsersScopeNode)this.ScopeNode;
            usersScopeNode.usersFormView = this;
            base.OnInitialize(status);
        }

        /// <summary>
        /// PlugEvents method implementation
        /// </summary>
        internal void PlugEvents(UsersListView lst)
        {
            lst.DataSelectionChanged += OnDataSelectionChanged;
            lst.DataEditionActivated += OnDataEditionActivated;
            ActionsPaneItems.Clear();
            SelectionData.ActionsPaneItems.Clear();
            SelectionData.ActionsPaneHelpItems.Clear();
            SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete);
            SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMPROPERTIES, res.USERSFRMPROPERTIESDESC, -1, "PropertyUser"));
            ModeActionsPaneItems.Clear();
        }

        /// <summary>
        /// UsersListControl property implementation
        /// </summary>
        public UsersListView UsersListControl
        {
            get { return usersControl; }
        }

        /// <summary>
        /// OnDataSelectionChanged method implementation
        /// </summary>
        private void OnDataSelectionChanged(object sender, SelectionDataEventArgs e)
        {
            if (e.Action != MMCListAction.SelectionChanged)
                return;
            this.SelectionData.BeginUpdates();
            try
            {
                if (e.Selection.Count == 0)
                {
                    this.SelectionData.Clear();
                }
                else
                {
                    if (e.Selection.Count > 0)
                    {
                        SelectionData.Update(e.Selection, e.Selection.Count > 1, null, null);
                        if (e.Selection.Count == 1)
                        {
                            UpdateActionPanelItems(e.Selection);
                            SelectionData.DisplayName = e.Selection[0].UPN;
                        }
                        else
                        {
                            UpdateActionPanelItems(e.Selection);
                            SelectionData.DisplayName = res.USERSFRMSLECTMULTIPLE;
                        }
                    }
                }
            }
            finally
            {
                this.SelectionData.EndUpdates();
            }
        }

        /// <summary>
        /// OnDataEditionActivated method implmentation
        /// </summary>
        private void OnDataEditionActivated(object sender, SelectionDataEventArgs e)
        {
            this.SelectionData.ShowPropertySheet(res.USERSFRMPROPERTIES+" : "+SelectionData.DisplayName);
        }


        /// <summary>
        /// UpdateActionPanelItems method implmentation
        /// </summary>
        internal void UpdateActionPanelItems(MFAUserList lst)
        {
            Nullable<bool> enb = null;
            SelectionData.ActionsPaneItems.Clear();
            foreach (MFAUser reg in lst)
            {
                if (!enb.HasValue)
                    enb = reg.Enabled;
                else if (enb.Value != reg.Enabled)
                    enb = null;
            }
            if (enb.HasValue)
            {
                if (!enb.Value)
                    SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMACTIVATE, res.USERSFRMACTIVATEDESC, -1, "EnableUser"));
                else
                    SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMDEACTIVATE, res.USERSFRMDEACTIVATEDESC, -1, "DisableUser"));
                SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            }
            SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMPROPERTIES, res.USERSFRMPROPERTIESDESC, -1, "PropertyUser"));
        }

        /// <summary>
        /// EnableDisableAction method implmentation
        /// </summary>
        internal void EnableDisableAction(bool value)
        {
            SelectionData.ActionsPaneItems.Clear();
            if (!value)
                SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMACTIVATE, res.USERSFRMACTIVATEDESC, -1, "EnableUser"));
            else
                SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMDEACTIVATE, res.USERSFRMDEACTIVATEDESC, -1, "DisableUser"));
            SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMPROPERTIES, res.USERSFRMPROPERTIESDESC, -1, "PropertyUser"));
        }


        /// <summary>
        /// Refresh method implmentation
        /// </summary>
        public void Refresh(bool refreshgrid = false, bool clearselection = false)
        {
            if (UsersListControl != null)
            {
                this.SelectionData.BeginUpdates();
                try
                {
                    UsersListControl.RefreshData(refreshgrid, clearselection);
                    foreach (ActionsPaneItem itm in this.SelectionData.ActionsPaneItems)
                    {
                        if (itm is Microsoft.ManagementConsole.Action)
                        {
                            if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "EnableUser")
                            {
                                ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.USERSFRMACTIVATE;
                                ((Microsoft.ManagementConsole.Action)itm).Description = res.USERSFRMACTIVATEDESC;
                            }
                            else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "DisableUser")
                            {
                                ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.USERSFRMDEACTIVATE;
                                ((Microsoft.ManagementConsole.Action)itm).Description = res.USERSFRMDEACTIVATEDESC;
                            }
                            else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "PropertyUser")
                            {
                                ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.USERSFRMPROPERTIES;
                                ((Microsoft.ManagementConsole.Action)itm).Description = res.USERSFRMPROPERTIESDESC;
                            }
                        }
                    }
                }
                finally
                {
                    this.SelectionData.EndUpdates();
                }
            }
        }

        /// <summary>
        /// SetUserStoreData method implmentation
        /// </summary>
        internal void SetUserStoreData(object obj)
        {
            MFAUserList reg = null;
            if (obj is MFAUserList)
            {
                reg = (MFAUserList)obj;
                if (UsersListControl != null)
                {
                    this.SelectionData.BeginUpdates();
                    try
                    {
                        UsersListControl.SetUserData(reg);
                    }
                    finally
                    {
                        this.SelectionData.EndUpdates();
                    }
                }
            }
        }

        /// <summary>
        /// AddUserStoreData method implementation
        /// </summary>
        internal void AddUserStoreData(object obj)
        {
            MFAUserList reg = null;
            if (obj is MFAUserList)
            {
                reg = (MFAUserList)obj;
                if (UsersListControl != null)
                {
                    this.SelectionData.BeginUpdates();
                    try
                    {
                        UsersListControl.AddUserData(reg);
                    }
                    finally
                    {
                        this.SelectionData.EndUpdates();
                    }
                }
            }
        }

        /// <summary>
        /// DeleteUserStoreData method implementation
        /// </summary>
        internal bool DeleteUserStoreData(object obj)
        {
            bool ret = false;
            MFAUserList reg = null;
            if (obj is MFAUserList)
            {
                reg = (MFAUserList)obj;
                if (UsersListControl != null)
                {
                    this.SelectionData.BeginUpdates();
                    try
                    {
                        ret = UsersListControl.DeleteUserData(reg);
                    }
                    finally
                    {
                        this.SelectionData.EndUpdates();
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// EnableUserStoreData method implementation
        /// </summary>
        private void EnableUserStoreData(object obj, bool enabled)
        {
            MFAUserList reg = null;
            if (obj is MFAUserList)
            {
                reg = (MFAUserList)obj;
                if (UsersListControl != null)
                {
                    this.SelectionData.BeginUpdates();
                    try
                    {
                        if (enabled)
                            UsersListControl.EnableUserData(reg);
                        else
                            UsersListControl.DisableUserData(reg);
                    }
                    finally
                    {
                        this.SelectionData.EndUpdates();
                    }
                }
            }
        }

        /// <summary>
        /// Handle the selected action.
        /// </summary>
        protected override void OnSelectionAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            this.SelectionData.BeginUpdates();
            try
            {
                switch ((string)action.Tag)
                {
                    case "EnableUser":
                        {
                            MFAUserList reg = (MFAUserList)SelectionData.SelectionObject;
                            EnableUserStoreData(reg, true);
                            break;
                        }
                    case "DisableUser":
                        {
                            MFAUserList reg = (MFAUserList)SelectionData.SelectionObject;
                            EnableUserStoreData(reg, false);
                            break;
                        }
                    case "PropertyUser":
                        {
                            this.SelectionData.ShowPropertySheet(res.USERSFRMPROPERTIES + " : " + SelectionData.DisplayName);
                            break;
                        }
                }
            }
            finally
            {
                this.SelectionData.EndUpdates();
            }
        }

        /// <summary>
        /// OnDelete method implmentation 
        /// </summary>
        protected override void OnDelete(SyncStatus status)
        {

            MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
            messageBoxParameters.Caption = "Multi-Factor Authentication"; 
            messageBoxParameters.Buttons = MessageBoxButtons.YesNo;
            messageBoxParameters.DefaultButton = MessageBoxDefaultButton.Button1;
            messageBoxParameters.Icon = MessageBoxIcon.Question;
            messageBoxParameters.Text = res.USERSFRMCONFIRMDELETE; 

            if (this.SnapIn.Console.ShowDialog(messageBoxParameters) == DialogResult.Yes) 
            {
                MFAUserList reg = (MFAUserList)SelectionData.SelectionObject;
                bool xres = DeleteUserStoreData(reg);
                if (xres)
                {
                    status.Complete("ok", true);
                }
                else
                {
                    status.CanCancel = true;
                    status.Complete("error", false);
                }
            }
            else
            {
                status.CanCancel = true;
                base.OnDelete(status);
            }
        }

        /// <summary>
        /// OnAddPropertyPages method implementation
        /// </summary>
        protected override void OnAddPropertyPages(PropertyPageCollection propertyPageCollection)
        {
            Random rand = new Random();
            int i = rand.Next();
            MFAUserList registrations = (MFAUserList)SelectionData.SelectionObject;
            if (registrations.Count > 1)
                propertyPageCollection.Add(new UserPropertyPage(this, typeof(UserCommonPropertiesControl), i));
            else
            {
                propertyPageCollection.Add(new UserPropertyPage(this, typeof(UserPropertiesControl), i));
                propertyPageCollection.Add(new UserPropertyPage(this, typeof(UserPropertiesKeysControl), i));
            }
        }
    }
}