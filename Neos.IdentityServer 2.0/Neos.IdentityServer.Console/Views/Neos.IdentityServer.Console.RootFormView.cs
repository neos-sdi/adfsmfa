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
    public class RootFormView : FormView
    {
        private RootScopeNode rootScopeNode = null;
        private RootViewControl usersControl = null;

        /// <summary>
        /// RootFormView constructor
        /// </summary>
        public RootFormView()
        {

        }

        /// <summary>
        /// Initialize method override
        /// </summary>
        protected override void OnInitialize(AsyncStatus status)
        {
            usersControl = (RootViewControl)this.Control;
            rootScopeNode = (RootScopeNode)this.ScopeNode;
            rootScopeNode.rootFormView = this;

            ActionsPaneItems.Clear();
            SelectionData.ActionsPaneItems.Clear();
            SelectionData.ActionsPaneHelpItems.Clear();
            SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete | StandardVerbs.Properties);
            ModeActionsPaneItems.Clear(); 

            base.OnInitialize(status);
        }

    }
}
