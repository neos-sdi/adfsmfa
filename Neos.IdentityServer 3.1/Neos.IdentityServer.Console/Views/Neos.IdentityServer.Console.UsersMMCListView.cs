using Microsoft.ManagementConsole;
using Microsoft.ManagementConsole.Advanced;
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Admin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.DirectoryServices;

namespace Neos.IdentityServer.Console
{
    public class UsersMMCListView : MmcListView
    {
        private UsersScopeNode usersScopeNode = null;
        private WritableSharedData _sharedUserData = null;

        public WritableSharedData SharedUserData
        {
            get { return _sharedUserData; }
        }

        public UsersMMCListView()
        {
            _sharedUserData = new WritableSharedData();
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="status"></param>
        protected override void OnInitialize(AsyncStatus status)
        {
            base.OnInitialize(status);
            RemoteAdminService.Paging.PageSize = 100;
            RemoteAdminService.Paging.CurrentPage = 1;

            usersScopeNode = (UsersScopeNode)this.ScopeNode;
            usersScopeNode.usersFormView = this;
            this.Sorter = new UserMMCListComparer();

            ActionsPaneItems.Clear();
            SelectionData.ActionsPaneItems.Clear();
            SelectionData.ActionsPaneHelpItems.Clear(); 
            SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete | StandardVerbs.Properties);
            ModeActionsPaneItems.Clear();

         //   Mode = MmcListViewMode.Report;

            Columns[0].Title = "Nom";
            Columns[0].SetWidth(300);

            Columns.Add(new MmcListViewColumn("Email", 250, MmcListViewColumnFormat.Left, true));
            Columns.Add(new MmcListViewColumn("Téléphone", 100, MmcListViewColumnFormat.Left, true));
            Columns.Add(new MmcListViewColumn("Crée le", 80, MmcListViewColumnFormat.Center, true));
            Columns.Add(new MmcListViewColumn("Actif", 40, MmcListViewColumnFormat.Center, true));
            Columns.Add(new MmcListViewColumn("Méthode", 60, MmcListViewColumnFormat.Center, true));
            Columns.Add(new MmcListViewColumn("ID", 50, MmcListViewColumnFormat.Left, false));
            Columns.Add(new MmcListViewColumn("SecretKey", 200, MmcListViewColumnFormat.Left, false));

            ActionsPaneItems.Clear();
            SelectionData.ActionsPaneItems.Clear();
            SelectionData.ActionsPaneHelpItems.Clear();
            SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete | StandardVerbs.Properties);
            ModeActionsPaneItems.Clear();

            Refresh();
        }

        protected override void OnSyncAction(SyncAction action, SyncStatus status)
        {

        }

        protected override void OnSyncModeAction(SyncAction action, SyncStatus status)
        {

        }

        protected override void OnSyncSelectionAction(SyncAction action, SyncStatus status)
        {

        }

        /// <summary>
        /// Defines actions for selection.
        /// </summary>
        /// <param name="status"></param>
        protected override void OnSelectionChanged(SyncStatus status)
        {
            this.SelectionData.BeginUpdates();
            try
            {
                if (this.SelectedNodes.Count == 0)
                {
                    this.SelectionData.Clear();
                }
                else
                {
                    var r = this.ResultNodes.IndexOf((ResultNode)this.SelectedNodes[0]);
                    MMCRegistrationList regs = GetSelectedUsers();
                    if (regs.Count > 0)
                    {
                        SelectionData.Update(regs, this.SelectedNodes.Count > 1, null, null);
                        if (regs.Count == 1)
                        {
                            UpdateActionPanelItems(regs);
                            SelectionData.DisplayName = regs[0].UPN;
                        }
                        else
                        {
                            UpdateActionPanelItems(regs);
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
        /// OnSortCompleted method implmentation
        /// </summary>
        protected override void OnSortCompleted(int columnIndex, bool descending)
        {
            this.SelectionData.BeginUpdates();
            try
            {
                this.ResultNodes.Clear();
                switch (columnIndex)
                {
                    case 0:
                        RemoteAdminService.Order.Column = UsersOrderField.UserName;
                        break;
                    case 1:
                        RemoteAdminService.Order.Column = UsersOrderField.Email;
                        break;
                    case 2:
                        RemoteAdminService.Order.Column = UsersOrderField.Phone;
                        break;
                    case 3:
                        RemoteAdminService.Order.Column = UsersOrderField.CreationDate;
                        break;
                    default:
                        RemoteAdminService.Order.Column = UsersOrderField.ID;
                        break;
                }
                if (descending)
                    RemoteAdminService.Order.Direction = SortDirection.Descending;
                else
                    RemoteAdminService.Order.Direction = SortDirection.Ascending;
                Refresh();
            }
            finally
            {
                this.SelectionData.EndUpdates();
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
                            MMCRegistrationList reg = (MMCRegistrationList)SelectionData.SelectionObject;
                            reg = RemoteAdminService.EnableUser(reg);
                            UpdateNodes(reg); 
                            break;
                        }
                    case "DisableUser":
                        {
                            MMCRegistrationList reg = (MMCRegistrationList)SelectionData.SelectionObject;
                            reg = RemoteAdminService.DisableUser(reg);
                            UpdateNodes(reg);
                            break;
                        }
                }
            }
            finally
            {
                this.SelectionData.EndUpdates();
            }
        }

        internal void NextPage()
        {
            this.SelectionData.BeginUpdates();
            try
            {
                RemoteAdminService.Paging.CurrentPage++;
                this.ResultNodes.Clear();
                Refresh();
            }
            finally
            {
                this.SelectionData.EndUpdates();
            }

        }

        internal void PriorPage()
        {
            this.SelectionData.BeginUpdates();
            try
            {
                if (RemoteAdminService.Paging.CurrentPage > 1)
                {
                    RemoteAdminService.Paging.CurrentPage--;
                    this.ResultNodes.Clear();
                    Refresh();
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
        /// <param name="status"></param>
        protected override void OnDelete(SyncStatus status)
        {
            MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
            messageBoxParameters.Caption = "Multi-Factor Authentication";
            messageBoxParameters.Buttons = MessageBoxButtons.YesNo;
            messageBoxParameters.DefaultButton = MessageBoxDefaultButton.Button1;
            messageBoxParameters.Icon = MessageBoxIcon.Question;
            messageBoxParameters.Text = "Voulez vous vraiment supprimer cet élément ?";

            this.SelectionData.BeginUpdates();
            try
            {
                if (this.SnapIn.Console.ShowDialog(messageBoxParameters) == DialogResult.Yes)
                {
                    MMCRegistrationList reg = (MMCRegistrationList)SelectionData.SelectionObject;
                    DeleteNodes(reg);
                }
                else
                {
                    status.CanCancel = true;
                }
                base.OnDelete(status);
            }
            finally
            {
                this.SelectionData.EndUpdates();
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
            if (registrations.Count>1)
                propertyPageCollection.Add(new UserPropertyPage(this, typeof(UserCommonPropertiesControl), i));
            else
            {
                propertyPageCollection.Add(new UserPropertyPage(this, typeof(UserPropertiesControl), i));
                propertyPageCollection.Add(new UserPropertyPage(this, typeof(UserPropertiesKeysControl), i));
            }
        }


        /// <summary>
        /// Refresh method implmentation
        /// </summary>
        public void Refresh()
        {
            RefreshData(RemoteAdminService.GetUsers());
        }

        /// <summary>
        /// UpdateActionPanelItems method implmentation
        /// </summary>
        internal void UpdateActionPanelItems(MMCRegistrationList lst)
        {
            Nullable<bool> enb = null;
            SelectionData.ActionsPaneItems.Clear();
            foreach(MMCRegistration reg in lst)
            {
                if (!enb.HasValue)
                    enb = reg.Enabled;
                else if (enb.Value != reg.Enabled)
                    enb = null;
            }
           /* if (lst.Count==1)
                SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete | StandardVerbs.Properties);
            else if (lst.Count > 1)
                SelectionData.EnabledStandardVerbs = StandardVerbs.Delete; */
            if (enb.HasValue)
            {
                if (!enb.Value)
                    SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action("Activer", "Activer MFA pour l'utilisateur sélectionné.", -1, "EnableUser"));
                else
                    SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action("Désactiver", "Désactiver MFA pour l'utilisateur sélectionné.", -1, "DisableUser"));
            }
        }

        /// <summary>
        /// Populate the list with the sample data.
        /// </summary>
        /// <param name="users"></param>
        public void RefreshData(MMCRegistrationList users)
        {
            this.SelectionData.BeginUpdates();
            try
            {
                this.ResultNodes.Clear();
                foreach (MMCRegistration user in users)
                {
                    this.ResultNodes.Add(user);
                }
            }
            finally
            {
                this.SelectionData.EndUpdates();
            }
        }

        /// <summary>
        /// Build a string of selected users.
        /// </summary>
        private MMCRegistrationList GetSelectedUsers()
        {
            MMCRegistrationList lst = new MMCRegistrationList();
            foreach (ResultNode resultNode in this.SelectedNodes)
            {
                lst.Add(resultNode);
            }
            return lst;
        }


        /// <summary>
        /// UpdateNodes method implementation
        /// </summary>
        private void AddNodes(MMCRegistrationList reg)
        {
            this.SelectionData.BeginUpdates();
            try
            {
                foreach (ResultNode resultNode in this.SelectedNodes)
                {
                    resultNode.SendSelectionRequest(false);
                }
                foreach (MMCRegistration registration in reg)
                {
                    ResultNode resultNode = registration;
                    ResultNodes.Add(resultNode);
                    resultNode.SendSelectionRequest(true);
                }
                UpdateActionPanelItems(reg);
            }
            finally
            {
                this.SelectionData.EndUpdates();
            }
        }

        /// <summary>
        /// UpdateNodes method implementation
        /// </summary>
        private void UpdateNodes(MMCRegistrationList reg)
        {
            this.SelectionData.BeginUpdates();
            try
            {
                foreach (ResultNode resultNode in this.ResultNodes)
                {
                    foreach(MMCRegistration registration in reg)
                    {
                        if (((MMCRegistration)resultNode).ID == registration.ID)
                        {
                            UpdateResultNode(registration, resultNode);
                            resultNode.SendSelectionRequest(true);
                        }
                    }
                    
                }
                UpdateActionPanelItems(reg);
            }
            finally
            {
                this.SelectionData.EndUpdates();
            }
        }

        /// <summary>
        /// UpdateNodes method implementation
        /// </summary>
        private void DeleteNodes(MMCRegistrationList registrations)
        {
            this.SelectionData.BeginUpdates();
            try
            {
                List<ResultNode> nds = new List<ResultNode>();
                RemoteAdminService.DeleteUser(registrations);
                foreach(MMCRegistration reg in registrations)
                {
                    foreach (ResultNode res in this.ResultNodes)
                    {
                        MMCRegistration xres = res; 
                        if (reg.ID == xres.ID)
                            nds.Add(res);
                    }
                }
                foreach (ResultNode res in nds)
                {
                    ResultNodes.Remove(res);
                }
            }
            finally
            {
                this.SelectionData.EndUpdates();
            }
        }


        /// <summary>
        /// SetData method implmentation
        /// </summary>
        internal void SetUserStoreData(object obj)
        {
            MMCRegistrationList reg = null;
            if (obj is MMCRegistrationList)
            {
                reg = (MMCRegistrationList)obj;
                RemoteAdminService.SetUser(reg);
                UpdateNodes(reg);
            }
        }

        internal void AddUserStoreData(object obj)
        {
            MMCRegistrationList reg = null;
            if (obj is MMCRegistrationList)
            {
                reg = (MMCRegistrationList)obj;
                reg = RemoteAdminService.AddUser(reg);
                AddNodes(reg);
            }
        }

        /// <summary>
        /// UpdateResultNode method 
        /// </summary>
        internal void UpdateResultNode(MMCRegistration registration, ResultNode resultnode)
        {
            if (registration == null)
                return;
            else
            {
                resultnode.SubItemDisplayNames.Clear();
                resultnode.DisplayName = registration.UPN;
                resultnode.SubItemDisplayNames.Add(registration.MailAddress);
                resultnode.SubItemDisplayNames.Add(registration.PhoneNumber);
                resultnode.SubItemDisplayNames.Add(registration.CreationDate.ToString());
                resultnode.SubItemDisplayNames.Add(registration.Enabled.ToString());
                resultnode.SubItemDisplayNames.Add(((int)registration.PreferredMethod).ToString());
                resultnode.SubItemDisplayNames.Add(registration.ID);
                resultnode.SubItemDisplayNames.Add(registration.SecretKey);
                if (registration.Enabled)
                    resultnode.ImageIndex = 1;
                else
                    resultnode.ImageIndex = 2;
            }
        }

        /// <summary>
        /// UpdateRegistration method
        /// </summary>
        internal void UpdateRegistration(ResultNode resultnode, MMCRegistration registration)
        {
            if (resultnode == null)
                return;
            else
            {
                registration.UPN = resultnode.DisplayName;
                registration.MailAddress = resultnode.SubItemDisplayNames[0];
                registration.PhoneNumber = resultnode.SubItemDisplayNames[1];
                registration.CreationDate = Convert.ToDateTime(resultnode.SubItemDisplayNames[2]);
                registration.Enabled = bool.Parse(resultnode.SubItemDisplayNames[3]);
                registration.PreferredMethod = (RegistrationPreferredMethod)Convert.ToInt32(resultnode.SubItemDisplayNames[4]);
                registration.ID = resultnode.SubItemDisplayNames[5];
                registration.SecretKey = resultnode.SubItemDisplayNames[6];
            }
        }
    }

    internal class UserMMCListComparer : IResultNodeComparer
    {
        int _culomnindex = 0;

        public void SetColumnIndex(int index)
        {
            _culomnindex = index;
        }

        public int Compare(object x, object y)
        {
            return 0;
        }
    }

}
