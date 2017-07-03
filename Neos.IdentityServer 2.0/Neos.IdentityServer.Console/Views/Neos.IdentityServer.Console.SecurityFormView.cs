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
    /// ServiceSecurityFormView Class
    /// </summary>
    public class ServiceSecurityFormView : FormView
    {
        private ServiceSecurityScopeNode securityScopeNode = null;
        private ServiceSecurityViewControl usersControl = null;

        /// <summary>
        /// ServiceSecurityFormView constructor
        /// </summary>
        public ServiceSecurityFormView()
        {

        }

        /// <summary>
        /// Initialize method override
        /// </summary>
        protected override void OnInitialize(AsyncStatus status)
        {
            usersControl = (ServiceSecurityViewControl)this.Control;
            securityScopeNode = (ServiceSecurityScopeNode)this.ScopeNode;
            securityScopeNode.SecurityFormView = this;

            ActionsPaneItems.Clear();
            SelectionData.ActionsPaneItems.Clear();
            SelectionData.ActionsPaneHelpItems.Clear();
            SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete | StandardVerbs.Properties);
            ModeActionsPaneItems.Clear();

            base.OnInitialize(status);
        }

    }
}
