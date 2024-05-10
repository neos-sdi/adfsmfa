//******************************************************************************************************************************************************************************************//
// Copyright (c) 2024 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using Microsoft.ManagementConsole;
using Microsoft.ManagementConsole.Advanced;
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Administration;
using System;
using System.Drawing;
using System.Security.Principal;
using System.Windows.Forms;
using res = Neos.IdentityServer.Console.Resources.Neos_IdentityServer_Console_UsersFormView;

namespace Neos.IdentityServer.Console
{
    /// <summary>
    /// UsersFormView Class
    /// </summary>
    public class UsersFormView : FormView
    {
        private static Bitmap _deletemmc = new Bitmap(Resources.Neos_IdentityServer_Console_Snapin.delete);
        private static ToolStripMenuItem _activate = new ToolStripMenuItem(res.USERSFRMACTIVATE);
        private static ToolStripMenuItem _deactivate = new ToolStripMenuItem(res.USERSFRMDEACTIVATE);
        private static ToolStripMenuItem _properties = new ToolStripMenuItem(res.USERSFRMPROPERTIES);
        private static ToolStripMenuItem _delete = new ToolStripMenuItem(res.USERSFRMDELETE, _deletemmc);
        private static ToolStripMenuItem _passwords = new ToolStripMenuItem(res.USERSFRMPASSWORDS);


        private UsersListView usersControl = null;
        private UsersScopeNode usersScopeNode = null;
        private WritableSharedData _sharedUserData = null;

        /// <summary>
        /// UsersFormView Constructor
        /// </summary>
        public UsersFormView()
        {
            _sharedUserData = new WritableSharedData();
            _deletemmc.MakeTransparent();
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
            if (ManagementService.Config == null)
                MMCService.EnsureService(); 
        }

        /// <summary>
        /// PlugEvents method implementation
        /// </summary>
        internal void PlugEvents(UsersListView lst)
        {
            usersControl = lst;
            ActionsPaneItems.Clear();
            SelectionData.ActionsPaneItems.Clear();
            SelectionData.ActionsPaneHelpItems.Clear();
            SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete);
            SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMPROPERTIES, res.USERSFRMPROPERTIESDESC, -1, "PropertyUser"));
            if ((ManagementService.Config.AllowPasswordsReset) && (ADFSManagementRights.IsDomainAdministrator(true) || ADFSManagementRights.AllowedGroup(WindowsBuiltInRole.AccountOperator)))
            {
                SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
                SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMPASSWORDS, res.USERSFRMPASSWORDS, -1, "PasswordUser"));
            }
            UsersListControl.contextMenuStripGrid.Items.Clear();
            UsersListControl.contextMenuStripGrid.Items.Add(res.USERSFRMPROPERTIES);
            if ((ManagementService.Config.AllowPasswordsReset) && (ADFSManagementRights.IsDomainAdministrator(true) || ADFSManagementRights.AllowedGroup(WindowsBuiltInRole.AccountOperator)))
            {
                UsersListControl.contextMenuStripGrid.Items.Add(new ToolStripSeparator());
                UsersListControl.contextMenuStripGrid.Items.Add(_passwords);
            }
            ModeActionsPaneItems.Clear();
            UsersListControl.DataSelectionChanged += OnDataSelectionChanged;
            UsersListControl.DataEditionActivated += OnDataEditionActivated;
            _activate.Text = res.USERSFRMACTIVATE;
            _activate.ToolTipText = res.USERSFRMACTIVATEDESC;
            _activate.Click += _activate_Click;
            _deactivate.Text = res.USERSFRMDEACTIVATE;
            _deactivate.ToolTipText = res.USERSFRMDEACTIVATEDESC;
            _deactivate.Click += _deactivate_Click;
            _properties.Text = res.USERSFRMPROPERTIES;
            _properties.ToolTipText = res.USERSFRMPROPERTIESDESC;
            _properties.Click += _properties_Click;
            _delete.Text = res.USERSFRMDELETE;
            _delete.ToolTipText = res.USERSFRMDELETE;
            _delete.Click += _delete_Click;
            _passwords.Text = res.USERSFRMPASSWORDS;
            _passwords.ToolTipText = res.USERSFRMPASSWORDSDESC;
            _passwords.Click += _passwords_Click;
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
            UsersListControl.contextMenuStripGrid.Items.Clear();
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
                {
                    SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMACTIVATE, res.USERSFRMACTIVATEDESC, -1, "EnableUser"));
                    UsersListControl.contextMenuStripGrid.Items.Add(_activate);
                }
                else
                {
                    SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMDEACTIVATE, res.USERSFRMDEACTIVATEDESC, -1, "DisableUser"));
                    UsersListControl.contextMenuStripGrid.Items.Add(_deactivate);
                }
            }
            SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMPROPERTIES, res.USERSFRMPROPERTIESDESC, -1, "PropertyUser"));           
            UsersListControl.contextMenuStripGrid.Items.Add(_properties);
            if ((ManagementService.Config.AllowPasswordsReset) && (ADFSManagementRights.IsDomainAdministrator(true) || ADFSManagementRights.AllowedGroup(WindowsBuiltInRole.AccountOperator)))
            {
                SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
                SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMPASSWORDS, res.USERSFRMPASSWORDSDESC, -1, "PasswordUser"));
                UsersListControl.contextMenuStripGrid.Items.Add(new ToolStripSeparator());
                UsersListControl.contextMenuStripGrid.Items.Add(_passwords);
            }
            UsersListControl.contextMenuStripGrid.Items.Add(new ToolStripSeparator());
            UsersListControl.contextMenuStripGrid.Items.Add(_delete);
        }

        /// <summary>
        /// EnableDisableAction method implmentation
        /// </summary>
        internal void EnableDisableAction(bool value)
        {
            SelectionData.ActionsPaneItems.Clear();
            UsersListControl.contextMenuStripGrid.Items.Clear();
            if (!value)
            {
                SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMACTIVATE, res.USERSFRMACTIVATEDESC, -1, "EnableUser"));
                UsersListControl.contextMenuStripGrid.Items.Add(_activate);
            }
            else
            {
                SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMDEACTIVATE, res.USERSFRMDEACTIVATEDESC, -1, "DisableUser"));
                UsersListControl.contextMenuStripGrid.Items.Add(_deactivate);
            }
            SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMPROPERTIES, res.USERSFRMPROPERTIESDESC, -1, "PropertyUser"));
            UsersListControl.contextMenuStripGrid.Items.Add(_properties);
            if ((ManagementService.Config.AllowPasswordsReset) && (ADFSManagementRights.IsDomainAdministrator(true) || ADFSManagementRights.AllowedGroup(WindowsBuiltInRole.AccountOperator)))
            {
                SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
                SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action(res.USERSFRMPASSWORDS, res.USERSFRMPASSWORDSDESC, -1, "PasswordUser"));
                UsersListControl.contextMenuStripGrid.Items.Add(new ToolStripSeparator());
                UsersListControl.contextMenuStripGrid.Items.Add(_passwords);
            }
            UsersListControl.contextMenuStripGrid.Items.Add(new ToolStripSeparator());
            UsersListControl.contextMenuStripGrid.Items.Add(_delete);
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
                            else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "PasswordUser")
                            {
                                ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.USERSFRMPASSWORDS;
                                ((Microsoft.ManagementConsole.Action)itm).Description = res.USERSFRMPASSWORDSDESC;
                            }
                        }
                    }
                    _activate.Text = res.USERSFRMACTIVATE;
                    _activate.ToolTipText = res.USERSFRMACTIVATEDESC;
                    _deactivate.Text = res.USERSFRMDEACTIVATE;
                    _deactivate.ToolTipText = res.USERSFRMDEACTIVATEDESC;
                    _properties.Text = res.USERSFRMPROPERTIES;
                    _properties.ToolTipText = res.USERSFRMPROPERTIESDESC;
                    _delete.Text = res.USERSFRMDELETE;
                    _delete.ToolTipText = res.USERSFRMDELETE;
                    _passwords.Text = res.USERSFRMPASSWORDS;
                    _passwords.ToolTipText = res.USERSFRMPASSWORDSDESC;
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
        /// ResetUserPasswordData method implementation
        /// </summary>
        internal bool ResetUserPasswordData(object obj)
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
                        ret = UsersListControl.ResetUserPasswordData(reg);
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
                    case "PasswordUser":
                        {
                            MFAUserList reg = (MFAUserList)SelectionData.SelectionObject;
                            ResetUserPasswordData(reg);
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
            MessageBoxParameters messageBoxParameters = new MessageBoxParameters
            {
                Caption = "Multi-Factor Authentication",
                Buttons = MessageBoxButtons.YesNo,
                DefaultButton = MessageBoxDefaultButton.Button1,
                Icon = MessageBoxIcon.Question,
                Text = res.USERSFRMCONFIRMDELETE
            };

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

        #region ContextMenu Click events
        /// <summary>
        /// _delete_Click event
        /// </summary>
        private void _delete_Click(object sender, EventArgs e)
        {
            MessageBoxParameters messageBoxParameters = new MessageBoxParameters
            {
                Caption = "Multi-Factor Authentication",
                Buttons = MessageBoxButtons.YesNo,
                DefaultButton = MessageBoxDefaultButton.Button1,
                Icon = MessageBoxIcon.Question,
                Text = res.USERSFRMCONFIRMDELETE
            };

            if (this.SnapIn.Console.ShowDialog(messageBoxParameters) == DialogResult.Yes)
            {
                MFAUserList reg = (MFAUserList)SelectionData.SelectionObject;
                bool xres = DeleteUserStoreData(reg);

            }
        }

        /// <summary>
        /// _passwords_Click
        /// </summary>
        private void _passwords_Click(object sender, EventArgs e)
        {
            MessageBoxParameters messageBoxParameters = new MessageBoxParameters
            {
                Caption = "Multi-Factor Authentication",
                Buttons = MessageBoxButtons.YesNo,
                DefaultButton = MessageBoxDefaultButton.Button1,
                Icon = MessageBoxIcon.Question,
                Text = res.USERSFRMRESETPWD
            };

            if (this.SnapIn.Console.ShowDialog(messageBoxParameters) == DialogResult.Yes)
            {
                MFAUserList reg = (MFAUserList)SelectionData.SelectionObject;
                bool xres = ResetUserPasswordData(reg);
            }
        }

        /// <summary>
        /// _properties_Click event
        /// </summary>
        private void _properties_Click(object sender, EventArgs e)
        {
            this.SelectionData.ShowPropertySheet(res.USERSFRMPROPERTIES + " : " + SelectionData.DisplayName);
        }

        /// <summary>
        /// _deactivate_Click event
        /// </summary>
        private void _deactivate_Click(object sender, EventArgs e)
        {
            MFAUserList reg = (MFAUserList)SelectionData.SelectionObject;
            EnableUserStoreData(reg, false);
        }

        /// <summary>
        /// _activate_Click event
        /// </summary>
        private void _activate_Click(object sender, EventArgs e)
        {
            MFAUserList reg = (MFAUserList)SelectionData.SelectionObject;
            EnableUserStoreData(reg, true);
        }

        #endregion
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
                propertyPageCollection.Add(new UserPropertyPage(this, typeof(UserAttestationsControl), i));
            }
        }
    }
}