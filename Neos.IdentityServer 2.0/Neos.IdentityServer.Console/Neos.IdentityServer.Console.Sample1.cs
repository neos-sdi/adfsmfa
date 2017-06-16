using Microsoft.ManagementConsole;
using System.ComponentModel;
using System;
using System.Security.Permissions;
using System.Collections;
// [assembly: SecurityPermission(SecurityAction.RequestMinimum, Unrestricted = true)]

namespace Neos.IdentityServer.Console
{
     /// <summary>
    /// The main entry point for the creation of the snap-in.
    /// </summary>
    [SnapInSettings("{CFAA3895-4B02-4431-A168-A6416013C3DD}", DisplayName = "Simple SnapIn Sample", Description = "Simple Hello World SnapIn", Vendor = "Neos-Sdi")]
    public class SimpleSnapIn : SnapIn
    {
        /// <summary>
        /// The constructor.
        /// </summary>
        public SimpleSnapIn()
        {
            // Update tree pane with a node in the tree
            this.RootNode = new ScopeNode();
            this.RootNode.DisplayName = "Hello World";
        }
    }
}
