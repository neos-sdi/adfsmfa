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
using Microsoft.ManagementConsole.Advanced;
using Neos.IdentityServer.MultiFactor.Administration;
using System.Windows.Forms;
using System.Resources;
using res = Neos.IdentityServer.Console.Resources.Neos_IdentityServer_Console_Nodes;
using System.Threading;
using System.Globalization;
using System.IO;
using Neos.IdentityServer.MultiFactor;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

namespace Neos.IdentityServer.Console
{
    /// <summary>
    /// RefreshableScopeNode class
    /// </summary>
    public abstract class RefreshableScopeNode: ScopeNode
    {
        public RefreshableScopeNode(): base(){}
        public RefreshableScopeNode(bool hideExpandIcon):base(hideExpandIcon){}
        protected abstract void RefreshDescription();
        protected abstract void RefreshActions();
        protected abstract void RefreshForms();

        /// <summary>
        /// RefreshUI method
        /// </summary>
        public virtual void RefreshUI()
        {
            RefreshDescription();
            RefreshActions();
            RefreshForms();
        }
    }

    /// <summary>
    /// RootScopeNode class
    /// </summary>
    public class RootScopeNode : RefreshableScopeNode
    {
        internal RootFormView rootFormView;
 
        /// <summary>
        /// Constructor
        /// </summary>
        public RootScopeNode()
        {
            this.DisplayName = res.ROOTSCOPENODEDESC;
            this.LanguageIndependentName = "MFA Authentication Console";

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            Microsoft.ManagementConsole.ActionGroup importgrp = new Microsoft.ManagementConsole.ActionGroup(res.ROOTCHANGELANGUAGE, res.ROOTCHANGELANGUAGEDESC, -1, "LCID_GROUP");
            importgrp.Items.Add(new Microsoft.ManagementConsole.Action(res.ROOTLANGUAGEFR, res.ROOTLANGUAGEFRDESC, -1, "LCID_FR"));
            importgrp.Items.Add(new Microsoft.ManagementConsole.Action(res.ROOTLANGUAGEUS, res.ROOTLANGUAGEUSDESC, -1, "LCID_US"));
            importgrp.Items.Add(new Microsoft.ManagementConsole.Action(res.ROOTLANGUAGEES, res.ROOTLANGUAGEESDESC, -1, "LCID_ES"));
            this.ActionsPaneItems.Add(importgrp);
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            base.OnExpand(status);
        }

        /// <summary>
        /// OnAction method implmentation
        /// </summary>
        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "LCID_FR":
                    CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(1036); 
                    break;
                case "LCID_US":
                    CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(1033); 
                    break;
                case "LCID_ES":
                    CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(3082); 
                    break;
            }
            ((ADFSSnapIn)this.SnapIn).RefreshUI();
        }

        /// <summary>
        /// RefreshDescription method
        /// </summary>
        protected override void RefreshDescription()
        {
            this.DisplayName = res.ROOTSCOPENODEDESC;
        }

        /// <summary>
        /// RefreshActions method
        /// </summary>
        protected override void RefreshActions()
        {
            foreach(ActionsPaneItem itm in  this.ActionsPaneItems)
            {
                if (itm is Microsoft.ManagementConsole.ActionGroup)
                {
                    if ((string)((Microsoft.ManagementConsole.ActionGroup)itm).Tag == "LCID_GROUP")
                    {
                        ((Microsoft.ManagementConsole.ActionGroup)itm).DisplayName = res.ROOTCHANGELANGUAGE;
                        ((Microsoft.ManagementConsole.ActionGroup)itm).Description = res.ROOTCHANGELANGUAGEDESC;
                    }
                    foreach (ActionsPaneItem itms in ((Microsoft.ManagementConsole.ActionGroup)itm).Items)
                    {
                        if (itms is Microsoft.ManagementConsole.Action)
                        {
                            if ((string)((Microsoft.ManagementConsole.Action)itms).Tag == "LCID_FR")
                            {
                                ((Microsoft.ManagementConsole.Action)itms).DisplayName = res.ROOTLANGUAGEFR;
                                ((Microsoft.ManagementConsole.Action)itms).Description = res.ROOTLANGUAGEFRDESC;
                            }
                            else if ((string)((Microsoft.ManagementConsole.Action)itms).Tag == "LCID_US")
                            {
                                ((Microsoft.ManagementConsole.Action)itms).DisplayName = res.ROOTLANGUAGEUS;
                                ((Microsoft.ManagementConsole.Action)itms).Description = res.ROOTLANGUAGEUSDESC;
                            }
                            else if ((string)((Microsoft.ManagementConsole.Action)itms).Tag == "LCID_ES")
                            {
                                ((Microsoft.ManagementConsole.Action)itms).DisplayName = res.ROOTLANGUAGEES;
                                ((Microsoft.ManagementConsole.Action)itms).Description = res.ROOTLANGUAGEESDESC;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// RefreshForms method
        /// </summary>
        protected override void RefreshForms()
        {
            if (rootFormView != null)
                rootFormView.Refresh();
        }
    }

    /// <summary>
    /// ServiceScopeNode class
    /// </summary>
    public class ServiceScopeNode : RefreshableScopeNode
    {
        internal ServiceFormView serviceFormView;

        public ServiceScopeNode(): base(true)
        {
            this.DisplayName = res.SERVICESCOPENODEDESC;
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

        /// <summary>
        /// RefreshDescription method
        /// </summary>
        protected override void RefreshDescription()
        {
            this.DisplayName = res.SERVICESCOPENODEDESC;
        }

        /// <summary>
        /// RefreshActions method
        /// </summary>
        protected override void RefreshActions()
        {

        }

        /// <summary>
        /// RefreshForms method
        /// </summary>
        protected override void RefreshForms()
        {
            if (serviceFormView != null)
                serviceFormView.Refresh();
        }
    }

    /// <summary>
    /// ServiceStatusScopeNode class
    /// </summary>
    public class ServiceGeneralScopeNode : RefreshableScopeNode
    {
        internal GeneralFormView generalFormView;
        private Microsoft.ManagementConsole.Action SaveConfig;
        private Microsoft.ManagementConsole.Action CancelConfig;


        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServiceGeneralScopeNode(): base(true)
        {
            this.DisplayName = res.GENERALSCOPENODEDESC;
            this.LanguageIndependentName = "Generic MFA parmeters";

            SaveConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPESAVE, res.GENERALSCOPESAVEDESC, -1, "SaveConfig");
            CancelConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPECANCEL, res.GENERALSCOPECANCELDESC, -1, "CancelConfig");

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            this.ActionsPaneItems.Add(SaveConfig);
            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            this.ActionsPaneItems.Add(CancelConfig);
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implmentation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            base.OnExpand(status);
        }

        /// <summary>
        /// OnRefresh method implmentattion
        /// </summary>
        protected override void OnRefresh(AsyncStatus status)
        {
            base.OnRefresh(status);
            if (this.generalFormView != null)
                this.generalFormView.Refresh();
        }

        /// <summary>
        /// OnAction method implmentation
        /// </summary>
        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "SaveConfig":
                    if (this.generalFormView != null)
                        this.generalFormView.DoSave();
                    break;
                case "CancelConfig":
                    if (this.generalFormView != null)
                        this.generalFormView.DoCancel();
                    break;
            }
        }

        /// <summary>
        /// RefreshDescription method
        /// </summary>
        protected override void RefreshDescription()
        {
            this.DisplayName = res.GENERALSCOPENODEDESC;
        }

        /// <summary>
        /// RefreshActions method
        /// </summary>
        protected override void RefreshActions()
        {
            foreach (ActionsPaneItem itm in this.ActionsPaneItems)
            {
                if (itm is Microsoft.ManagementConsole.Action)
                {
                    if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "SaveConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPESAVE; 
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPESAVEDESC;
                    }
                    else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "CancelConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPECANCEL;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPECANCELDESC;
                    }
                }
            }
        }

        /// <summary>
        /// RefreshForms method
        /// </summary>
        protected override void RefreshForms()
        {
            if (generalFormView != null)
                generalFormView.Refresh();
        }
    }

    /// <summary>
    /// ServiceADDSScopeNode class
    /// </summary>
    public class ServiceADDSScopeNode: RefreshableScopeNode
    {
        internal ServiceADDSFormView ADDSFormView;
        private Microsoft.ManagementConsole.Action SaveConfig;
        private Microsoft.ManagementConsole.Action CancelConfig;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServiceADDSScopeNode()
            : base(true)
        {
            this.DisplayName = res.ADDSSCOPENODEDESC;
            this.LanguageIndependentName = "MFA Active Directory Configuration";

            SaveConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPESAVE, res.GENERALSCOPESAVEDESC, -1, "SaveConfig");
            CancelConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPECANCEL, res.GENERALSCOPECANCELDESC, -1, "CancelConfig");

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            this.ActionsPaneItems.Add(SaveConfig);
            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            this.ActionsPaneItems.Add(CancelConfig);
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            base.OnExpand(status);
        }

        /// <summary>
        /// OnRefresh method implmentattion
        /// </summary>
        protected override void OnRefresh(AsyncStatus status)
        {
            base.OnRefresh(status);
            if (this.ADDSFormView != null)
                this.ADDSFormView.Refresh();
        }

        /// <summary>
        /// OnAction method implmentation
        /// </summary>
        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "SaveConfig":
                    if (this.ADDSFormView != null)
                        this.ADDSFormView.DoSave();
                    break;
                case "CancelConfig":
                    if (this.ADDSFormView != null)
                        this.ADDSFormView.DoCancel();
                    break;
            }
        }

        /// <summary>
        /// RefreshDescription method
        /// </summary>
        protected override void RefreshDescription()
        {
            this.DisplayName = res.ADDSSCOPENODEDESC;
        }

        /// <summary>
        /// RefreshActions method
        /// </summary>
        protected override void RefreshActions()
        {
            foreach (ActionsPaneItem itm in this.ActionsPaneItems)
            {
                if (itm is Microsoft.ManagementConsole.Action)
                {
                    if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "SaveConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPESAVE;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPESAVEDESC;
                    }
                    else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "CancelConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPECANCEL;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPECANCELDESC;
                    }
                }
            }
        }

        /// <summary>
        /// RefreshForms method
        /// </summary>
        protected override void RefreshForms()
        {
            if (ADDSFormView != null)
                ADDSFormView.Refresh();
        }
    }

    /// <summary>
    /// ServiceSQLScopeNode class
    /// </summary>
    public class ServiceSQLScopeNode : RefreshableScopeNode
    {
        internal ServiceSQLFormView SQLFormView;
        private Microsoft.ManagementConsole.Action SaveConfig;
        private Microsoft.ManagementConsole.Action CancelConfig;


        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServiceSQLScopeNode(): base(true)
        {
           // this.DisplayName = "Configuration SQL";
            this.DisplayName = res.SQLSCOPENODEDESC;
            this.LanguageIndependentName = "MFA SQL Configuration";

            SaveConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPESAVE, res.GENERALSCOPESAVEDESC, -1, "SaveConfig");
            CancelConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPECANCEL, res.GENERALSCOPECANCELDESC, -1, "CancelConfig");

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            this.ActionsPaneItems.Add(SaveConfig);
            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            this.ActionsPaneItems.Add(CancelConfig);
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            base.OnExpand(status);
        }

        /// <summary>
        /// OnRefresh method implmentattion
        /// </summary>
        protected override void OnRefresh(AsyncStatus status)
        {
            base.OnRefresh(status);
            if (this.SQLFormView != null)
                this.SQLFormView.Refresh();
        }

        /// <summary>
        /// OnAction method implmentation
        /// </summary>
        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "SaveConfig":
                    if (this.SQLFormView != null)
                        this.SQLFormView.DoSave();
                    break;
                case "CancelConfig":
                    if (this.SQLFormView != null)
                        this.SQLFormView.DoCancel();
                    break;
            }
        }

        /// <summary>
        /// RefreshDescription method
        /// </summary>
        protected override void RefreshDescription()
        {
            this.DisplayName = res.SQLSCOPENODEDESC;
        }

        /// <summary>
        /// RefreshActions method
        /// </summary>
        protected override void RefreshActions()
        {
            foreach (ActionsPaneItem itm in this.ActionsPaneItems)
            {
                if (itm is Microsoft.ManagementConsole.Action)
                {
                    if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "SaveConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPESAVE;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPESAVEDESC;
                    }
                    else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "CancelConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPECANCEL;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPECANCELDESC;
                    }
                }
            }
        }

        /// <summary>
        /// RefreshForms method
        /// </summary>
        protected override void RefreshForms()
        {
            if (SQLFormView != null)
                SQLFormView.Refresh();
        }
    }

    /// <summary>
    /// ServiceProvidersScopeNode class
    /// </summary>
    public class ServiceProvidersScopeNode : RefreshableScopeNode
    {
        internal ServiceProvidersFormView ProvidersFormView;
        private Microsoft.ManagementConsole.Action SaveConfig;
        private Microsoft.ManagementConsole.Action CancelConfig;


        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServiceProvidersScopeNode()
            : base(true)
        {
            this.DisplayName = res.MFASCOPENODEDESC;
            this.LanguageIndependentName = "MFA Providers Configuration";

            SaveConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPESAVE, res.GENERALSCOPESAVEDESC, -1, "SaveConfig");
            CancelConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPECANCEL, res.GENERALSCOPECANCELDESC, -1, "CancelConfig");

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            this.ActionsPaneItems.Add(SaveConfig);
            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            this.ActionsPaneItems.Add(CancelConfig);
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            base.OnExpand(status);
        }

        /// <summary>
        /// OnRefresh method implmentattion
        /// </summary>
        protected override void OnRefresh(AsyncStatus status)
        {
            base.OnRefresh(status);
            if (this.ProvidersFormView != null)
                this.ProvidersFormView.Refresh();
        }

        /// <summary>
        /// OnAction method implmentation
        /// </summary>
        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "SaveConfig":
                    if (this.ProvidersFormView != null)
                        this.ProvidersFormView.DoSave();
                    break;
                case "CancelConfig":
                    if (this.ProvidersFormView != null)
                        this.ProvidersFormView.DoCancel();
                    break;
            }
        }

        /// <summary>
        /// RefreshDescription method
        /// </summary>
        protected override void RefreshDescription()
        {
            this.DisplayName = res.MFASCOPENODEDESC;
        }

        /// <summary>
        /// RefreshActions method
        /// </summary>
        protected override void RefreshActions()
        {
            foreach (ActionsPaneItem itm in this.ActionsPaneItems)
            {
                if (itm is Microsoft.ManagementConsole.Action)
                {
                    if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "SaveConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPESAVE;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPESAVEDESC;
                    }
                    else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "CancelConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPECANCEL;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPECANCELDESC;
                    }
                }
            }
        }

        /// <summary>
        /// RefreshForms method
        /// </summary>
        protected override void RefreshForms()
        {
            if (ProvidersFormView != null)
                ProvidersFormView.Refresh();
        }
    }

    /// <summary>
    /// ServiceTOTPScopeNode class
    /// </summary>
    public class ServiceTOTPScopeNode : RefreshableScopeNode
    {
        internal ServiceTOTPFormView TOTPFormView;
        private Microsoft.ManagementConsole.Action SaveConfig;
        private Microsoft.ManagementConsole.Action CancelConfig;


        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServiceTOTPScopeNode()
            : base(true)
        {
            //  this.DisplayName = "Configuration TOTP";
            this.DisplayName = res.TOTPSCOPENODEDESC;
            this.LanguageIndependentName = "MFA SMTP Configuration";

            SaveConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPESAVE, res.GENERALSCOPESAVEDESC, -1, "SaveConfig");
            CancelConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPECANCEL, res.GENERALSCOPECANCELDESC, -1, "CancelConfig");

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            this.ActionsPaneItems.Add(SaveConfig);
            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            this.ActionsPaneItems.Add(CancelConfig);
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            base.OnExpand(status);
        }

        /// <summary>
        /// OnRefresh method implmentattion
        /// </summary>
        protected override void OnRefresh(AsyncStatus status)
        {
            base.OnRefresh(status);
            if (this.TOTPFormView != null)
                this.TOTPFormView.Refresh();
        }

        /// <summary>
        /// OnAction method implmentation
        /// </summary>
        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "SaveConfig":
                    if (this.TOTPFormView != null)
                        this.TOTPFormView.DoSave();
                    break;
                case "CancelConfig":
                    if (this.TOTPFormView != null)
                        this.TOTPFormView.DoCancel();
                    break;
            }
        }

        /// <summary>
        /// RefreshDescription method
        /// </summary>
        protected override void RefreshDescription()
        {
            this.DisplayName = res.SMTPSCOPENODEDESC;
        }

        /// <summary>
        /// RefreshActions method
        /// </summary>
        protected override void RefreshActions()
        {
            foreach (ActionsPaneItem itm in this.ActionsPaneItems)
            {
                if (itm is Microsoft.ManagementConsole.Action)
                {
                    if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "SaveConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPESAVE;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPESAVEDESC;
                    }
                    else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "CancelConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPECANCEL;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPECANCELDESC;
                    }
                }
            }
        }

        /// <summary>
        /// RefreshForms method
        /// </summary>
        protected override void RefreshForms()
        {
            if (TOTPFormView != null)
                TOTPFormView.Refresh();
        }
    }

    /// <summary>
    /// ServiceSMTPScopeNode class
    /// </summary>
    public class ServiceSMTPScopeNode: RefreshableScopeNode
    {
        internal ServiceSMTPFormView SMTPFormView;
        private Microsoft.ManagementConsole.Action SaveConfig;
        private Microsoft.ManagementConsole.Action CancelConfig;


        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServiceSMTPScopeNode(): base(true)
        {
          //  this.DisplayName = "Configuration SMTP";
            this.DisplayName = res.SMTPSCOPENODEDESC;
            this.LanguageIndependentName = "MFA SMTP Configuration";

            SaveConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPESAVE, res.GENERALSCOPESAVEDESC, -1, "SaveConfig");
            CancelConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPECANCEL, res.GENERALSCOPECANCELDESC, -1, "CancelConfig");

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            this.ActionsPaneItems.Add(SaveConfig);
            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            this.ActionsPaneItems.Add(CancelConfig);
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            base.OnExpand(status);
        }

        /// <summary>
        /// OnRefresh method implmentattion
        /// </summary>
        protected override void OnRefresh(AsyncStatus status)
        {
            base.OnRefresh(status);
            if (this.SMTPFormView != null)
                this.SMTPFormView.Refresh();
        }

        /// <summary>
        /// OnAction method implmentation
        /// </summary>
        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "SaveConfig":
                    if (this.SMTPFormView != null)
                        this.SMTPFormView.DoSave();
                    break;
                case "CancelConfig":
                    if (this.SMTPFormView != null)
                        this.SMTPFormView.DoCancel();
                    break;
            }
        }

        /// <summary>
        /// RefreshDescription method
        /// </summary>
        protected override void RefreshDescription()
        {
            this.DisplayName = res.SMTPSCOPENODEDESC;
        }

        /// <summary>
        /// RefreshActions method
        /// </summary>
        protected override void RefreshActions()
        {
            foreach (ActionsPaneItem itm in this.ActionsPaneItems)
            {
                if (itm is Microsoft.ManagementConsole.Action)
                {
                    if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "SaveConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPESAVE;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPESAVEDESC;
                    }
                    else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "CancelConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPECANCEL;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPECANCELDESC;
                    }
                }
            }
        }

        /// <summary>
        /// RefreshForms method
        /// </summary>
        protected override void RefreshForms()
        {
            if (SMTPFormView != null)
                SMTPFormView.Refresh();
        }
    }

    /// <summary>
    /// ServicePhoneScopeNode class
    /// </summary>
    public class ServicePhoneScopeNode: RefreshableScopeNode
    {
        internal ServiceSMSFormView SMSFormView;
        private Microsoft.ManagementConsole.Action SaveConfig;
        private Microsoft.ManagementConsole.Action CancelConfig;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServicePhoneScopeNode(): base(true)
        {
           // this.DisplayName = "Configuration SMS";
            this.DisplayName = res.SMSSCOPENODEDESC;
            this.LanguageIndependentName = "MFA SMS Configuration";

            SaveConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPESAVE, res.GENERALSCOPESAVEDESC, -1, "SaveConfig");
            CancelConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPECANCEL, res.GENERALSCOPECANCELDESC, -1, "CancelConfig");

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            this.ActionsPaneItems.Add(SaveConfig);
            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            this.ActionsPaneItems.Add(CancelConfig);
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            base.OnExpand(status);
        }

        /// <summary>
        /// OnRefresh method implmentattion
        /// </summary>
        protected override void OnRefresh(AsyncStatus status)
        {
            base.OnRefresh(status);
            if (this.SMSFormView != null)
                this.SMSFormView.Refresh();
        }

        /// <summary>
        /// OnAction method implmentation
        /// </summary>
        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "SaveConfig":
                    if (this.SMSFormView != null)
                        this.SMSFormView.DoSave();
                    break;
                case "CancelConfig":
                    if (this.SMSFormView != null)
                        this.SMSFormView.DoCancel();
                    break;
            }
        }

        /// <summary>
        /// RefreshDescription method
        /// </summary>
        protected override void RefreshDescription()
        {
            this.DisplayName = res.SMSSCOPENODEDESC;
        }

        /// <summary>
        /// RefreshActions method
        /// </summary>
        protected override void RefreshActions()
        {
            foreach (ActionsPaneItem itm in this.ActionsPaneItems)
            {
                if (itm is Microsoft.ManagementConsole.Action)
                {
                    if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "SaveConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPESAVE;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPESAVEDESC;
                    }
                    else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "CancelConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPECANCEL;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPECANCELDESC;
                    }
                }
            }
        }

        /// <summary>
        /// RefreshForms method
        /// </summary>
        protected override void RefreshForms()
        {
            if (this.SMSFormView != null)
                this.SMSFormView.Refresh();
        }
    }

    /// <summary>
    /// ServiceAzureScopeNode class
    /// </summary>
    public class ServiceAzureScopeNode : RefreshableScopeNode
    {
        internal ServiceAzureFormView AzureFormView;
        private Microsoft.ManagementConsole.Action SaveConfig;
        private Microsoft.ManagementConsole.Action CancelConfig;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServiceAzureScopeNode()
            : base(true)
        {
            // this.DisplayName = "Configuration Azure";
            this.DisplayName = res.AZURESCOPENODEDESC;
            this.LanguageIndependentName = "MFA Azure Configuration";

            SaveConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPESAVE, res.GENERALSCOPESAVEDESC, -1, "SaveConfig");
            CancelConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPECANCEL, res.GENERALSCOPECANCELDESC, -1, "CancelConfig");

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            this.ActionsPaneItems.Add(SaveConfig);
            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            this.ActionsPaneItems.Add(CancelConfig);
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
            base.OnExpand(status);
        }

        /// <summary>
        /// OnRefresh method implmentattion
        /// </summary>
        protected override void OnRefresh(AsyncStatus status)
        {
            base.OnRefresh(status);
            if (this.AzureFormView != null)
                this.AzureFormView.Refresh();
        }

        /// <summary>
        /// OnAction method implmentation
        /// </summary>
        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "SaveConfig":
                    if (this.AzureFormView != null)
                        this.AzureFormView.DoSave();
                    break;
                case "CancelConfig":
                    if (this.AzureFormView != null)
                        this.AzureFormView.DoCancel();
                    break;
            }
        }

        /// <summary>
        /// RefreshDescription method
        /// </summary>
        protected override void RefreshDescription()
        {
            this.DisplayName = res.SMSSCOPENODEDESC;
        }

        /// <summary>
        /// RefreshActions method
        /// </summary>
        protected override void RefreshActions()
        {
            foreach (ActionsPaneItem itm in this.ActionsPaneItems)
            {
                if (itm is Microsoft.ManagementConsole.Action)
                {
                    if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "SaveConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPESAVE;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPESAVEDESC;
                    }
                    else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "CancelConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPECANCEL;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPECANCELDESC;
                    }
                }
            }
        }

        /// <summary>
        /// RefreshForms method
        /// </summary>
        protected override void RefreshForms()
        {
            if (this.AzureFormView != null)
                this.AzureFormView.Refresh();
        }
    }

    /// <summary>
    /// ServiceStatusScopeNode class
    /// </summary>
    public class ServiceSecurityScopeNode: RefreshableScopeNode
    {
        internal ServiceSecurityFormView SecurityFormView;
        private Microsoft.ManagementConsole.Action SaveConfig;
        private Microsoft.ManagementConsole.Action CancelConfig;


        /// <summary>
        /// Constructor implementation
        /// </summary>
        public ServiceSecurityScopeNode(): base(true)
        {
           // this.DisplayName = "Gestion de la sécurité";
            this.DisplayName = res.SECURITYSCOPENODEDESC;
            this.LanguageIndependentName = "MFA paramétres de sécurité";

            SaveConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPESAVE, res.GENERALSCOPESAVEDESC, -1, "SaveConfig");
            CancelConfig = new Microsoft.ManagementConsole.Action(res.GENERALSCOPECANCEL, res.GENERALSCOPECANCELDESC, -1, "CancelConfig");

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            this.ActionsPaneItems.Add(SaveConfig);
            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            this.ActionsPaneItems.Add(CancelConfig);
            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implementation
        /// </summary>
        /// <param name="status"></param>
        protected override void OnExpand(AsyncStatus status)
        {
            base.OnExpand(status);
        }

        /// <summary>
        /// OnRefresh method implmentattion
        /// </summary>
        protected override void OnRefresh(AsyncStatus status)
        {
            base.OnRefresh(status);
            if (this.SecurityFormView != null)
                this.SecurityFormView.Refresh();
        }

        /// <summary>
        /// OnAction method implmentation
        /// </summary>
        protected override void OnAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "SaveConfig":
                    if (this.SecurityFormView != null)
                        this.SecurityFormView.DoSave();
                    break;
                case "CancelConfig":
                    if (this.SecurityFormView != null)
                        this.SecurityFormView.DoCancel();
                    break;
            }
        }

        /// <summary>
        /// RefreshDescription method
        /// </summary>
        protected override void RefreshDescription()
        {
            this.DisplayName = res.SECURITYSCOPENODEDESC;
        }

        /// <summary>
        /// RefreshActions method
        /// </summary>
        protected override void RefreshActions()
        {
            foreach (ActionsPaneItem itm in this.ActionsPaneItems)
            {
                if (itm is Microsoft.ManagementConsole.Action)
                {
                    if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "SaveConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPESAVE;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPESAVEDESC;
                    }
                    else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "CancelConfig")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.GENERALSCOPECANCEL;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.GENERALSCOPECANCELDESC;
                    }
                }
            }
        }

        /// <summary>
        /// RefreshForms method
        /// </summary>
        protected override void RefreshForms()
        {
            if (this.SecurityFormView != null)
                this.SecurityFormView.Refresh();
        }
    }

    /// <summary>
    /// UsersScopeNode class
    /// </summary>
    public class UsersScopeNode: RefreshableScopeNode
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
           // this.DisplayName = "Gestion des utilisateurs";
            this.DisplayName = res.USERSSCOPENODEDESC;
            this.LanguageIndependentName = "MFA Users Management";

            this.ActionsPaneHelpItems.Clear();
            this.ActionsPaneItems.Clear();
            this.EnabledStandardVerbs = StandardVerbs.Refresh;

            AddUser = new Microsoft.ManagementConsole.Action(res.USERSCOPEADDUSER, res.USERSCOPEADDUSERDESC, -1, "AddUser");
            FilterUser = new Microsoft.ManagementConsole.Action(res.USERSCOPEFILTERUSERS, res.USERSCOPEFILTERUSERSDESC, -1, "FilterUser");
            ClearFilterUser = new Microsoft.ManagementConsole.Action(res.USERSCOPEFILTERCLEAR, res.USERSCOPEFILTERCLEARDESC, -1, "ClearFilterUser");

            this.ActionsPaneItems.Add(AddUser);
            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            this.ActionsPaneItems.Add(FilterUser);
            ClearFilterUser.Enabled = MMCService.Filter.FilterisActive;
            this.ActionsPaneItems.Add(ClearFilterUser);

            this.ActionsPaneItems.Add(new Microsoft.ManagementConsole.ActionSeparator());
            Microsoft.ManagementConsole.ActionGroup importgrp = new Microsoft.ManagementConsole.ActionGroup(res.USERSCOPEIMPORT, res.USERSCOPEIMPORTDESC, -1, "Import");
            importgrp.Items.Add(new Microsoft.ManagementConsole.Action(res.USERSCOPEIMPORTCSV, res.USERSCOPEIMPORTCSVDESC, -1, "ImportCSV"));
            importgrp.Items.Add(new Microsoft.ManagementConsole.Action(res.USERSCOPEIMPORTXML, res.USERSCOPEIMPORTXMLDESC, -1, "ImportXML"));
            importgrp.Items.Add(new Microsoft.ManagementConsole.Action(res.USERSCOPEIMPORTADDS, res.USERSCOPEIMPORTADDSDESC, -1, "ImportADDS"));
            this.ActionsPaneItems.Add(importgrp);

            this.HelpTopic = string.Empty;
        }

        /// <summary>
        /// OnExpand method implmentation
        /// </summary>
        protected override void OnExpand(AsyncStatus status)
        {
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
                    DoImportCSV();
                    break;
                case "ImportXML":
                    DoImportXML();
                    break;
                case "ImportADDS":
                    DoImportADDS();
                    break;
                case "AddUser":
                    this.ShowPropertySheet(res.USERSPROPERTYADD);
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
        /// RefreshDescription method
        /// </summary>
        protected override void RefreshDescription()
        {
            this.DisplayName = res.USERSSCOPENODEDESC;
        }

        /// <summary>
        /// RefreshActions method
        /// </summary>
        protected override void RefreshActions()
        {
            foreach (ActionsPaneItem itm in this.ActionsPaneItems)
            {
                if (itm is Microsoft.ManagementConsole.ActionGroup)
                {
                    if ((string)((Microsoft.ManagementConsole.ActionGroup)itm).Tag == "Import")
                    {
                        ((Microsoft.ManagementConsole.ActionGroup)itm).DisplayName = res.USERSCOPEIMPORT;
                        ((Microsoft.ManagementConsole.ActionGroup)itm).Description = res.USERSCOPEIMPORTDESC;
                    }
                    foreach (ActionsPaneItem itms in ((Microsoft.ManagementConsole.ActionGroup)itm).Items)
                    {
                        if (itms is Microsoft.ManagementConsole.Action)
                        {
                            if ((string)((Microsoft.ManagementConsole.Action)itms).Tag == "ImportCSV")
                            {
                                ((Microsoft.ManagementConsole.Action)itms).DisplayName = res.USERSCOPEIMPORTCSV;
                                ((Microsoft.ManagementConsole.Action)itms).Description = res.USERSCOPEIMPORTCSVDESC;
                            }
                            else if ((string)((Microsoft.ManagementConsole.Action)itms).Tag == "ImportXML")
                            {
                                ((Microsoft.ManagementConsole.Action)itms).DisplayName = res.USERSCOPEIMPORTXML;
                                ((Microsoft.ManagementConsole.Action)itms).Description = res.USERSCOPEIMPORTXMLDESC;
                            }
                            else if ((string)((Microsoft.ManagementConsole.Action)itms).Tag == "ImportADDS")
                            {
                                ((Microsoft.ManagementConsole.Action)itms).DisplayName = res.USERSCOPEIMPORTADDS;
                                ((Microsoft.ManagementConsole.Action)itms).Description = res.USERSCOPEIMPORTADDSDESC;
                            }
                        }
                    }
                }
                else if (itm is Microsoft.ManagementConsole.Action)
                {

                    if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "AddUser")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.USERSCOPEADDUSER;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.USERSCOPEADDUSERDESC;
                    }
                    else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "FilterUser")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.USERSCOPEFILTERUSERS;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.USERSCOPEFILTERUSERSDESC;
                    }
                    else if ((string)((Microsoft.ManagementConsole.Action)itm).Tag == "ClearFilterUser")
                    {
                        ((Microsoft.ManagementConsole.Action)itm).DisplayName = res.USERSCOPEFILTERCLEAR;
                        ((Microsoft.ManagementConsole.Action)itm).Description = res.USERSCOPEFILTERCLEARDESC;
                    }
                }
            }
        }

        /// <summary>
        /// RefreshForms method
        /// </summary>
        protected override void RefreshForms()
        {
            if (this.usersFormView != null)
                this.usersFormView.Refresh();
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

        /// <summary>
        /// DoFilterUsers method implementation
        /// </summary>
        private void DoFilterUsers()
        {
            UsersFilterWizard Wizard = new UsersFilterWizard();
            bool result = (this.SnapIn.Console.ShowDialog(Wizard) == DialogResult.OK);
            if (result)
            {
                ClearFilterUser.Enabled = MMCService.Filter.FilterisActive;
                if (this.usersFormView != null)
                    this.usersFormView.Refresh(true,true);
            }
        }

        /// <summary>
        /// DoClearFilterUsers method implmentation
        /// </summary>
        private void DoClearFilterUsers()
        {
            MMCService.Filter.Clear();
            ClearFilterUser.Enabled = MMCService.Filter.FilterisActive;
            if (this.usersFormView != null)
                this.usersFormView.Refresh(true,true);
        }

        #region Imports

        /// <summary>
        /// InitializeTrace method implementation
        /// </summary>
        private TraceListener InitializeTrace(string filename)
        {
            DefaultTraceListener listen = new DefaultTraceListener();
            listen.Name = "MFATrace";
            Trace.Listeners.Add(listen);
            listen.TraceOutputOptions = TraceOptions.DateTime;
            listen.LogFileName = filename + ".log";
            return listen;
        }

        /// <summary>
        /// FinalizeTrace method implmentation
        /// </summary>
        private void FinalizeTrace(TraceListener listen)
        {
            Trace.Flush();
            Trace.Close();
            listen.Close();
            Trace.Listeners.Remove("MFATrace");
        }

        /// <summary>
        /// DoImportCSV method
        /// </summary>
        private void DoImportCSV()
        {
            OpenFileDialog Wizard = new OpenFileDialog();
            try
            {
                int all = 1;
                int cnt = 0;
                int err = 0;
                Wizard.DefaultExt = "csv";
                Wizard.Filter = "CSV Files (.csv)|*.csv|All Files (*.*)|*.*";
                Wizard.Title = res.USERSSCOPEIMPORTCSVTITLE;
                bool result = (this.usersFormView.SnapIn.Console.ShowDialog(Wizard) == DialogResult.OK);
                if (result)
                {
                    string filename = Wizard.FileName;
                    var contents = File.ReadAllText(filename).Split('\n');
                    var csv = from line in contents
                              select line.Split(';').ToArray();
                    var header = csv.First(r => r.Length > 1 && r.Last().Trim().Length > 0);
                    int?[] ids = GetCSVHeadersId(header);

                    var listen = InitializeTrace(filename);
                    try
                    {
                        Trace.WriteLine("");
                        Trace.WriteLine(string.Format("Importing file : {0}", filename));
                        Trace.Indent();
                        foreach (var row in csv.Skip(1).TakeWhile(r => r.Length > 1 && r.Last().Trim().Length > 0))
                        {
                            Trace.TraceInformation("Importing record N° {0}", all.ToString());
                            this.usersFormView.Control.Cursor = Cursors.WaitCursor;
                            try
                            {
                                Registration reg = new Registration();
                                if ((ids[0].HasValue) && (!string.IsNullOrEmpty(row[ids[0].Value])))
                                    reg.UPN = row[ids[0].Value];
                                else
                                    throw new InvalidDataException("upn must be provided !");

                                if ((ids[1].HasValue) && (!string.IsNullOrEmpty(row[ids[1].Value])))
                                    reg.MailAddress = row[ids[1].Value];
                                else if (ManagementService.ADFSManager.Config.MailProvider.Enabled)
                                    throw new InvalidDataException("email must be provided !");

                                if ((ids[2].HasValue) && (!string.IsNullOrEmpty(row[ids[2].Value])))
                                    reg.PhoneNumber = row[ids[2].Value];
                                else if (ManagementService.ADFSManager.Config.ExternalProvider.Enabled)
                                    throw new InvalidDataException("mobile must be provided !");

                                if ((ids[3].HasValue) && (!string.IsNullOrEmpty(row[ids[3].Value])))
                                    reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), row[ids[3].Value]);
                                else
                                    reg.PreferredMethod = PreferredMethod.Choose;

                                if ((ids[4].HasValue) && (!string.IsNullOrEmpty(row[ids[4].Value])))
                                    reg.Enabled = bool.Parse(row[ids[4].Value]);
                                else
                                    reg.Enabled = false;

                                ManagementService.AddUserRegistration(reg);
                                Trace.TraceInformation("Record N° {0} imported for user : {1} !", all.ToString(), reg.UPN);
                                cnt++;
                            }
                            catch (Exception ex)
                            {
                                err++;
                                Trace.TraceError("Error importing Record N° {0} \r\r {1}", all.ToString(), ex.Message);
                            }
                            finally
                            {
                                all++;
                                this.usersFormView.Control.Cursor = Cursors.Default;
                            }
                        }
                    }
                    finally
                    {
                        Trace.Unindent();
                        FinalizeTrace(listen);
                    }
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                    messageBoxParameters.Caption = res.USERSSCOPEIMPORTCSVRESTITLE;
                    messageBoxParameters.Text = string.Format(res.USERSSCOPEIMPORTCSVCOUNT, cnt.ToString(), err.ToString());
                    messageBoxParameters.Buttons = MessageBoxButtons.OK;
                    messageBoxParameters.Icon = MessageBoxIcon.Information;
                    this.usersFormView.SnapIn.Console.ShowDialog(messageBoxParameters);
                    if (this.usersFormView != null)
                        this.usersFormView.Refresh(true, true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this.usersFormView.SnapIn.Console.ShowDialog(messageBoxParameters);
            }

        }

        /// <summary>
        /// GetCSVHeadersId implmentation
        /// </summary>
        private int?[] GetCSVHeadersId(string[] header)
        {
            int?[] tmp = new int?[5] {null, null, null, null, null};
            int pos = 0;
            
            foreach(string s in header)
            {
                string hs = s.Replace("\r", "").Replace("\n", "");
                if (hs.ToLower().Equals("upn"))
                    tmp[0] = pos;
                else if (hs.ToLower().Equals("email"))
                    tmp[1] = pos;
                else if (hs.ToLower().Equals("mobile"))
                    tmp[2] = pos;
                else if (hs.ToLower().Equals("method"))
                    tmp[3] = pos;
                else if (hs.ToLower().Equals("enabled"))
                    tmp[4] = pos;
                pos++;
            }
            return tmp;
        }

        /// <summary>
        /// DoImportXML method
        /// </summary>
        private void DoImportXML()
        {
            OpenFileDialog Wizard = new OpenFileDialog();
            try
            {
                int all = 1;
                int cnt = 0;
                int err = 0;
                Wizard.DefaultExt = "xml";
                Wizard.Filter = "XML Files (.xml)|*.xml|All Files (*.*)|*.*";
                Wizard.Title = res.USERSSCOPEIMPORTXMLTITLE;
                bool result = (this.usersFormView.SnapIn.Console.ShowDialog(Wizard) == DialogResult.OK);
                if (result)
                {
                    string filename = Wizard.FileName;
                    var xml = XDocument.Load(filename);
                    var listen = InitializeTrace(filename);
                    try
                    {
                        Trace.WriteLine("");
                        Trace.WriteLine(string.Format("Importing file : {0}", filename));
                        Trace.Indent();

                        foreach (var row in xml.Root.Descendants("User"))
                        {
                            Trace.TraceInformation("Importing record N° {0}", all.ToString());
                            this.usersFormView.Control.Cursor = Cursors.WaitCursor;
                            try
                            {
                                Registration reg = new Registration();
                                if (row.Attribute("upn") != null)
                                    reg.UPN = row.Attribute("upn").Value;
                                else
                                    throw new InvalidDataException("upn must be provided !");

                                if (row.Attribute("email") != null)
                                    reg.MailAddress = row.Attribute("email").Value;
                                else if (ManagementService.ADFSManager.Config.MailProvider.Enabled)
                                    throw new InvalidDataException("email must be provided !");

                                if (row.Attribute("mobile") != null)
                                    reg.PhoneNumber = row.Attribute("mobile").Value;
                                else if (ManagementService.ADFSManager.Config.ExternalProvider.Enabled)
                                    throw new InvalidDataException("mobile must be provided !");

                                if (row.Attribute("method") != null)
                                    reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), row.Attribute("method").Value);
                                else
                                    reg.PreferredMethod = PreferredMethod.Choose;

                                if (row.Attribute("enabled") != null)
                                    reg.Enabled = bool.Parse(row.Attribute("enabled").Value);
                                else
                                    reg.Enabled = false;

                                ManagementService.AddUserRegistration(reg);
                                Trace.TraceInformation("Record N° {0} imported for user : {1} !", all.ToString(), reg.UPN);
                                cnt++;
                            }
                            catch (Exception ex)
                            {
                                err++;
                                Trace.TraceError("Error importing Record N° {0} \r\r {1}", all.ToString(), ex.Message);
                            }
                            finally
                            {
                                all++;
                                this.usersFormView.Control.Cursor = Cursors.Default;
                            }
                        }
                    }
                    finally
                    {
                        Trace.Unindent();
                        FinalizeTrace(listen);
                    }
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                    messageBoxParameters.Caption = res.USERSSCOPEIMPORTXMLRESTITLE;
                    messageBoxParameters.Text = string.Format(res.USERSSCOPEIMPORTXMLCOUNT, cnt.ToString(), err.ToString());
                    messageBoxParameters.Buttons = MessageBoxButtons.OK;
                    messageBoxParameters.Icon = MessageBoxIcon.Information;
                    this.usersFormView.SnapIn.Console.ShowDialog(messageBoxParameters);
                    if (this.usersFormView != null)
                        this.usersFormView.Refresh(true, true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this.usersFormView.SnapIn.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// DoImprtADDS method
        /// </summary>
        private void DoImportADDS()
        {
            LDAPWizard Wizard = new LDAPWizard();
            try
            {
                bool result = (this.SnapIn.Console.ShowDialog(Wizard) == DialogResult.OK);
                if (result)
                {
                    ManagementService.ImportADDSUsers(Wizard.LDAPQuery.Text, Wizard.checkBoxEnabled.Checked);
                    if (this.usersFormView != null)
                        this.usersFormView.Refresh(true, true);
                                    }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this.SnapIn.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

    }
}
