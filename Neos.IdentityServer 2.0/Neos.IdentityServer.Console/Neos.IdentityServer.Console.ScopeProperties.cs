using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.ManagementConsole.Advanced;


namespace Neos.IdentityServer.Console
{
        /// <summary>
        /// Gets the name and birthday.
        /// </summary>
        public partial class ScopePropertiesControl : UserControl
        {
            /// <summary>
            /// Defines the parent property page to expose data and state of property sheet.
            /// </summary>
            private ScopePropertyPage scopePropertyPage;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="parentPropertyPage">Container property page for the control</param>
            public ScopePropertiesControl(ScopePropertyPage parentPropertyPage)
            {
                // This call is required by the Windows form designer.
                InitializeComponent();

                // Assign a reference to the parent.
                scopePropertyPage = parentPropertyPage;
            }
            /// <summary>
            /// Populate control values from the SelectionObject (that is set in UserListView.SelectionOnChanged).
            /// </summary>
            public void RefreshData(SampleScopeNode scopeNode)
            {
                this.DisplayName.Text = scopeNode.DisplayName;
                scopePropertyPage.Dirty = false;
            }

            /// <summary>
            /// Update the node with the control values.
            /// </summary>
            /// <param name="scopeNode">Node being updated by property page</param>
            public void UpdateData(SampleScopeNode scopeNode)
            {
                scopeNode.DisplayName = this.DisplayName.Text;
                scopePropertyPage.Dirty = false;
            }

            /// <summary>
            /// Checks during UserProptertyPage.OnApply to ensure that changes can be applied.
            /// </summary>
            /// <returns>returns true if changes are valid</returns>
            public bool CanApplyChanges()
            {
                bool result = false;

                if (DisplayName.Text.Trim().Length == 0)
                {
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                    messageBoxParameters.Text = "Display Name cannot be blank";
                    scopePropertyPage.ParentSheet.ShowDialog(messageBoxParameters);

                    // MessageBox.Show("Display Name cannot be blank");
                }
                else
                {
                    result = true;
                }
                return result;
            }

            /// <summary>
            /// Notifies the PropertyPage that info has changed and that the PropertySheet can change the 
            /// buttons
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void DisplayName_TextChanged(object sender, System.EventArgs e)
            {
                scopePropertyPage.Dirty = true;
            }
        }//class
}
