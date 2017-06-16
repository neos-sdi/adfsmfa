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

          /*  UsersListControl.DataSelectionChanged += OnDataSelectionChanged;
            UsersListControl.DataEditionActivated += OnDataEditionActivated;

            ActionsPaneItems.Clear();
            SelectionData.ActionsPaneItems.Clear();
            SelectionData.ActionsPaneHelpItems.Clear();
            SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete | StandardVerbs.Properties);
            ModeActionsPaneItems.Clear(); */

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
            SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete | StandardVerbs.Properties);
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
                            SelectionData.DisplayName = "Sélection multiple";
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
            this.SelectionData.ShowPropertySheet("Editing Users");
        }


        /// <summary>
        /// UpdateActionPanelItems method implmentation
        /// </summary>
        internal void UpdateActionPanelItems(MMCRegistrationList lst)
        {
            Nullable<bool> enb = null;
            SelectionData.ActionsPaneItems.Clear();
            foreach (MMCRegistration reg in lst)
            {
                if (!enb.HasValue)
                    enb = reg.Enabled;
                else if (enb.Value != reg.Enabled)
                    enb = null;
            }
            if (enb.HasValue)
            {
                if (!enb.Value)
                    SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action("Activer", "Activer MFA pour l'utilisateur sélectionné.", -1, "EnableUser"));
                else
                    SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action("Désactiver", "Désactiver MFA pour l'utilisateur sélectionné.", -1, "DisableUser"));
            }
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
            MMCRegistrationList reg = null;
            if (obj is MMCRegistrationList)
            {
                reg = (MMCRegistrationList)obj;
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
            MMCRegistrationList reg = null;
            if (obj is MMCRegistrationList)
            {
                reg = (MMCRegistrationList)obj;
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
            MMCRegistrationList reg = null;
            if (obj is MMCRegistrationList)
            {
                reg = (MMCRegistrationList)obj;
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
            MMCRegistrationList reg = null;
            if (obj is MMCRegistrationList)
            {
                reg = (MMCRegistrationList)obj;
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
        /// EnableDisableAction method implmentation
        /// </summary>
        internal void EnableDisableAction(bool value)
        {
            SelectionData.ActionsPaneItems.Clear();
            if (!value)
                SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action("Activer", "Activer MFA pour l'utilisateur sélectionné.", -1, "EnableUser"));
            else
                SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action("Désactiver", "Désactiver MFA pour l'utilisateur sélectionné.", -1, "DisableUser"));
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
                            MMCRegistrationList reg = (MMCRegistrationList)SelectionData.SelectionObject;
                            EnableUserStoreData(reg, true);
                            break;
                        }
                    case "DisableUser":
                        {
                            MMCRegistrationList reg = (MMCRegistrationList)SelectionData.SelectionObject;
                            EnableUserStoreData(reg, false);
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
            messageBoxParameters.Text = "Voulez vous vraiment supprimer cet élément ?"; 

            if (this.SnapIn.Console.ShowDialog(messageBoxParameters) == DialogResult.Yes) 
            {
                MMCRegistrationList reg = (MMCRegistrationList)SelectionData.SelectionObject;
                bool res = DeleteUserStoreData(reg);
                if (res)
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
            MMCRegistrationList registrations = (MMCRegistrationList)SelectionData.SelectionObject;
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