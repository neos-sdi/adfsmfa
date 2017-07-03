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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ManagementConsole;
using Neos.IdentityServer.MultiFactor.Administration;
using System.Windows.Forms;


namespace Neos.IdentityServer.Console
{
    /// <summary>
    /// RootScopeNode class
    /// </summary>
    public class RootScopeNode: ScopeNode
    {
        internal RootFormView rootFormView;

        /// <summary>
        /// Constructor
        /// </summary>
        public RootScopeNode()
        {
            this.DisplayName = "MFA";
            this.LanguageIndependentName = "MFA Authentication Console";

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.None;
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.None;

            base.OnExpand(status);
        }
    }

    /// <summary>
    /// ServiceScopeNode class
    /// </summary>
    public class ServiceScopeNode : ScopeNode
    {
        internal ServiceFormView serviceFormView;

        public ServiceScopeNode(): base(true)
        {
            this.DisplayName = "Etat des services";
            this.LanguageIndependentName = "MFA Service";

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            base.OnExpand(status);
        }

        /// <summary>
        /// OnRefresh method implmentattion
        /// </summary>
        protected override void OnRefresh(AsyncStatus status)
        {
            base.OnRefresh(status);
            if (this.serviceFormView != null)
                this.serviceFormView.Refresh();
        }

    }

    /// <summary>
    /// ServiceStatusScopeNode class
    /// </summary>
    public class ServiceGeneralScopeNode : ScopeNode
    {
        internal GeneralFormView generalFormView;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServiceGeneralScopeNode(): base(true)
        {
            this.DisplayName = "Paramétres Généraux";
            this.LanguageIndependentName = "MFA paramétres généraux";

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            base.OnExpand(status);
        }
    }

    public class ServiceSQLScopeNode : ScopeNode
    {
        internal ServiceSQLFormView SQLFormView;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServiceSQLScopeNode(): base(true)
        {
            this.DisplayName = "Configuration SQL";
            this.LanguageIndependentName = "MFA SQL Configuration";

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            base.OnExpand(status);
        }
    }

    public class ServiceADDSScopeNode : ScopeNode
    {
        internal ServiceADDSFormView ADDSFormView;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServiceADDSScopeNode(): base(true)
        {
            this.DisplayName = "Configuration Active Directory";
            this.LanguageIndependentName = "MFA Active Directory Configuration";

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            base.OnExpand(status);
        }
    }

    public class ServiceSMTPScopeNode : ScopeNode
    {
        internal ServiceSMTPFormView SMTPFormView;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServiceSMTPScopeNode(): base(true)
        {
            this.DisplayName = "Configuration SMTP";
            this.LanguageIndependentName = "MFA SMTP Configuration";

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            base.OnExpand(status);
        }
    }

    public class ServicePhoneScopeNode : ScopeNode
    {
        internal ServiceSMSFormView SMSFormView;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServicePhoneScopeNode(): base(true)
        {
            this.DisplayName = "Configuration SMS";
            this.LanguageIndependentName = "MFA SMS Configuration";

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            base.OnExpand(status);
        }
    }

    /// <summary>
    /// ServiceStatusScopeNode class
    /// </summary>
    public class ServiceSecurityScopeNode : ScopeNode
    {
        internal ServiceSecurityFormView SecurityFormView;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServiceSecurityScopeNode(): base(true)
        {
            this.DisplayName = "Gestion de la sécurité";
            this.LanguageIndependentName = "MFA paramétres de sécurité";

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.None;
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        /// <param name="status"></param>
        protected override void OnExpand(AsyncStatus status)
        {
            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            base.OnExpand(status);
        }
    }

    /// <summary>
    /// UsersScopeNode class
    /// </summary>
    public class UsersScopeNode : ScopeNode
    {
        internal UsersFormView usersFormView;

        private Microsoft.ManagementConsole.Action ClearFilterUser;
        private Microsoft.ManagementConsole.Action FilterUser;
        private Microsoft.ManagementConsole.Action AddUser;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public UsersScopeNode():base(true)
        {
            this.DisplayName = "Gestion des utilisateurs";
            this.LanguageIndependentName = "MFA Users Management";

            AddUser = new Microsoft.ManagementConsole.Action("Ajouter un utilisateur", "Ajouter un utilisateur au système d'authentification MFA", -1, "AddUser");
            FilterUser = new Microsoft.ManagementConsole.Action("Filtrer les utilisateurs", "Filter la liste des utilisateur", -1, "FilterUser");
            ClearFilterUser = new Microsoft.ManagementConsole.Action("Effacer le Filtre utilisateurs", "Effacer les filtre de la liste des utilisateur", -1, "ClearFilterUser");
            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.None;
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implmentation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            this.ActionsPaneItems.Add(AddUser); 
            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            this.ActionsPaneItems.Add(FilterUser);

            ClearFilterUser.Enabled = ManagementAdminService.Filter.FilterisActive;
            this.ActionsPaneItems.Add(ClearFilterUser);

            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            Microsoft.ManagementConsole.ActionGroup importgrp = new Microsoft.ManagementConsole.ActionGroup("Importer", "Procédures d'import d'utilisateurs");
            importgrp.Items.Add(new Microsoft.ManagementConsole.Action("Import d'utilisateurs CSV", "Importer des utilisateur à partir d'un fichier CSV", -1, "ImportCSV"));
            importgrp.Items.Add(new Microsoft.ManagementConsole.Action("Import d'utilisateurs XML", "Importer des utilisateur à partir d'un fichier XML", -1, "ImportXML"));
            importgrp.Items.Add(new Microsoft.ManagementConsole.Action("Import d'utilisateurs Active Directory", "Importer des utilisateur à partir de ADDS", -1, "ImportADDS"));
            this.ActionsPaneItems.Add(importgrp);
            base.OnExpand(status);
        }

        /// <summary>
        /// OnAction method implmentation
        /// </summary>
        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "ImportCSV":
                    break;
                case "ImportXML":
                    break;
                case "ImportADDS":
                    break;
                case "AddUser":
                    this.ShowPropertySheet("Propriétés : Nouvel Utilisateur");
                    break;
                case "FilterUser":
                    DoFilterUsers();
                    break;
                case "ClearFilterUser":
                    DoClearFilterUsers();
                    break;
            }
        }

        /// <summary>
        /// DoFilterUsers method implementation
        /// </summary>
        private void DoFilterUsers()
        {
            UsersFilterWizard initializationWizard = new UsersFilterWizard();
            bool result = (this.SnapIn.Console.ShowDialog(initializationWizard) == DialogResult.OK);
            if (result)
            {
                ClearFilterUser.Enabled = ManagementAdminService.Filter.FilterisActive;
                if (this.usersFormView != null)
                    this.usersFormView.Refresh(true,true);
            }
        }

        /// <summary>
        /// DoClearFilterUsers method implmentation
        /// </summary>
        private void DoClearFilterUsers()
        {
            ManagementAdminService.Filter.Clear();
            ClearFilterUser.Enabled = ManagementAdminService.Filter.FilterisActive;
            if (this.usersFormView != null)
                this.usersFormView.Refresh(true,true);
        }

        /// <summary>
        /// OnRefresh method implmentattion
        /// </summary>
        protected override void OnRefresh(AsyncStatus status)
        {
            base.OnRefresh(status);
            if (this.usersFormView != null)
                this.usersFormView.Refresh();
        }

        /// <summary>
        /// Get the property pages to show.
        /// </summary>
        protected override void OnAddPropertyPages(PropertyPageCollection propertyPageCollection)
        {
            Random rand = new Random();
            int i = rand.Next();
            propertyPageCollection.Add(new UserPropertyPage(this.usersFormView, typeof(UserPropertiesControl), i, true));
            propertyPageCollection.Add(new UserPropertyPage(this.usersFormView, typeof(UserPropertiesKeysControl), i, true));
        }

    }
}
