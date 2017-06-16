using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ManagementConsole;
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Admin;
using System.Diagnostics;
using Microsoft.ManagementConsole.Advanced;
using System.Windows.Forms;
using System.Threading;

namespace Neos.IdentityServer.Console
{
    /// <summary>
    /// The form view to display the Winforms controls.
    /// </summary>
    public class UsersFormView : FormView
    {
        private UsersFormViewControl usersControl = null;
        private UsersScopeNode usersScopeNode = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UsersFormView()
        {
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="status"></param>
        protected override void OnInitialize(AsyncStatus status)
        {
            base.OnInitialize(status);
            usersControl = (UsersFormViewControl)this.Control;
            usersScopeNode = (UsersScopeNode)this.ScopeNode;
            usersScopeNode.usersFormView = this;

            ActionsPaneItems.Clear();
            SelectionData.ActionsPaneItems.Clear();
            SelectionData.ActionsPaneHelpItems.Clear();
            SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete | StandardVerbs.Properties);
            ModeActionsPaneItems.Clear();

            Refresh();
        }

        /// <summary>
        /// Refresh method implmentation
        /// </summary>
        public void Refresh(bool all = false)
        {
            if (all)
                usersControl.RefreshData(RemoteAdminService.GetAllUsers());            
            else
                usersControl.RefreshData(RemoteAdminService.GetUsers());            
        }

        /// <summary>
        /// RefreshCurrent method implmentation
        /// </summary>
        public void RefreshCurrent(object obj)
        {
            if (obj is Registration)
                usersControl.RefreshCurrent((Registration)obj);
        }

        /// <summary>
        /// DeleteCurrent method implmentation
        /// </summary>
        public void DeleteCurrent(object obj)
        {
            if (obj is Registration)
                usersControl.DeleteCurrent((Registration)obj);
        }

        /// <summary>
        /// AddCurrent method implmentation
        /// </summary>
        public void AddCurrent(object obj)
        {
            if (obj is Registration)
                usersControl.AddCurrent((Registration)obj);
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
           // status.
            switch ((string)action.Tag)
            {
                case "EnableUser":
                    {
                        Registration reg = (Registration)SelectionData.SelectionObject;
                        reg = RemoteAdminService.EnableUser(reg.UPN);
                        SelectionData.Update(reg, false, null, null);
                        RefreshCurrent(reg);
                        break;
                    }
                case "DisableUser":
                    {
                        Registration reg = (Registration)SelectionData.SelectionObject;
                        reg = RemoteAdminService.DisableUser(reg.UPN);
                        SelectionData.Update(reg, false, null, null);
                        RefreshCurrent(reg);
                        break;
                    }

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
                Registration reg = (Registration)SelectionData.SelectionObject;
                bool res = RemoteAdminService.DeleteUser(reg);
                if (res)
                {
                    DeleteCurrent(reg);
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
        /// Get the property pages to show.
        /// </summary>
        protected override void OnAddPropertyPages(PropertyPageCollection propertyPageCollection)
        {
            propertyPageCollection.Add(new UserPropertyPage(this));
        }


        /// <summary>
        /// Assign values from repository
        /// </summary>
        /// <param name="obj"></param>
        public static object GetData(object obj)
        {
            if (obj is Registration)
            {
                obj = RemoteAdminService.GetUser(((Registration)obj).UPN);
            }
            return obj;
        }

        /// <summary>
        /// Assign values after updates
        /// </summary>
        public static void SetData(object obj)
        {
            if (obj is Registration)
            {
                RemoteAdminService.SetUser((Registration)obj);
            }
        }

        /// <summary>
        /// Assign values after updates
        /// </summary>
        public static object AddData(object obj)
        {
            if (obj is Registration)
            {
                RemoteAdminService.AddUser((Registration)obj);
            }
            return obj;
        }
    }
}