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
    public class GeneralFormView: FormView
    {
        private ServiceGeneralScopeNode statusScopeNode = null;
        private GeneralViewControl usersControl = null;

        /// <summary>
        /// StatusFormView constructor
        /// </summary>
        public GeneralFormView()
        {

        }

        /// <summary>
        /// Initialize method override
        /// </summary>
        protected override void OnInitialize(AsyncStatus status)
        {
            usersControl = (GeneralViewControl)this.Control;
            statusScopeNode = (ServiceGeneralScopeNode)this.ScopeNode;
            statusScopeNode.generalFormView = this;

            ActionsPaneItems.Clear();
            SelectionData.ActionsPaneItems.Clear();
            SelectionData.ActionsPaneHelpItems.Clear();
            SelectionData.EnabledStandardVerbs = (StandardVerbs.Delete | StandardVerbs.Properties);
            ModeActionsPaneItems.Clear(); 

            base.OnInitialize(status);
        }
    }
}
