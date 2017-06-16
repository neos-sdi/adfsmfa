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
    /// StatusFormView Class
    /// </summary>
    public class StatusFormView: FormView
    {
        private ServiceStatusScopeNode statusScopeNode = null;
        private StatusViewControl usersControl = null;

        /// <summary>
        /// StatusFormView constructor
        /// </summary>
        public StatusFormView()
        {

        }

        /// <summary>
        /// Initialize method override
        /// </summary>
        protected override void OnInitialize(AsyncStatus status)
        {
            usersControl = (StatusViewControl)this.Control;
            statusScopeNode = (ServiceStatusScopeNode)this.ScopeNode;
            statusScopeNode.statusFormView = this;

            ActionsPaneItems.Clear();
            SelectionData.ActionsPaneItems.Clear();
            SelectionData.ActionsPaneHelpItems.Clear();
            SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete | StandardVerbs.Properties);
            ModeActionsPaneItems.Clear(); 

            base.OnInitialize(status);
        }
    }
}
