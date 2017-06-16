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
    /// ServiceFormView Class
    /// </summary>
    public class ServiceFormView : FormView
    {

        private ServiceScopeNode serviceScopeNode = null;
        private ServiceViewControl usersControl = null;

        /// <summary>
        /// StatusFormView constructor
        /// </summary>
        public ServiceFormView()
        {

        }


        /// <summary>
        /// Initialize method override
        /// </summary>
        protected override void OnInitialize(AsyncStatus status)
        {
            usersControl = (ServiceViewControl)this.Control;
            serviceScopeNode = (ServiceScopeNode)this.ScopeNode;
            serviceScopeNode.serviceFormView = this;

            ActionsPaneItems.Clear();
            SelectionData.ActionsPaneItems.Clear();
            SelectionData.ActionsPaneHelpItems.Clear();
            SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete | StandardVerbs.Properties);
            ModeActionsPaneItems.Clear();

            base.OnInitialize(status);
        }
    }
}
