using System;
using System.ComponentModel;
using System.Configuration;
using System.Text;
using System.Windows.Forms;
using Microsoft.ManagementConsole;

namespace Neos.IdentityServer.Console
{
    /// <summary>
    /// <summary>
    /// Provides the main entry point for the creation of a snap-in. 
    /// </summary>
    [SnapInSettings("{CFAA3895-4B02-4431-A168-A6416013C3DB}", DisplayName = "Initilization Wizard SnapIn", Description = "Sample - Shows Wizard during Add to Console", Vendor = "Neos-Sdi")]

    public class InitializationWizardSnapIn : SnapIn
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public InitializationWizardSnapIn()
        {
            this.RootNode = new ScopeNode();
            this.RootNode.DisplayName = "Unknown";

            this.IsModified = true;         // tells mmc to save custom data
        }


        /// <summary>
        /// Show the initialization wizard when the snap-in is added to console.
        /// </summary>
        /// <returns></returns>
        protected override bool OnShowInitializationWizard()
        {
            // Show a modal dialog to get the snap-in name.
            InitializationWizard initializationWizard = new InitializationWizard();
            bool result = (this.Console.ShowDialog(initializationWizard) == DialogResult.OK);

            if (result)
            {
                this.RootNode.DisplayName = initializationWizard.SelectedSnapInName;
            }

            return result;
        }
        /// <summary>
        /// Load any unsaved data.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="persistenceData"></param>
        protected override void OnLoadCustomData(AsyncStatus status, byte[] persistenceData)
        {
            // If the na,e is saved, then set the display name of the snap-in.
            if (string.IsNullOrEmpty(Encoding.Unicode.GetString(persistenceData)))
            {
                this.RootNode.DisplayName = "Unknown";
            }
            else
            {
                this.RootNode.DisplayName = Encoding.Unicode.GetString(persistenceData);
            }
        }
        /// <summary>
        /// If the snap-in has been modified, then save the data.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>

        protected override byte[] OnSaveCustomData(SyncStatus status)
        {
            return Encoding.Unicode.GetBytes(this.RootNode.DisplayName);
        }
    } //class
} // namespace