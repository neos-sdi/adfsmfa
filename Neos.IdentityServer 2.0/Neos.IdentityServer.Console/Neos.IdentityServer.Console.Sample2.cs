using System;
using System.ComponentModel;
using System.Security.Permissions;
using Microsoft.ManagementConsole;
using System.Windows.Forms;
using System.Text;


namespace Neos.IdentityServer.Console
{

    /// <summary>
    /// Provides the main entry point for the creation of a snap-in.
    /// </summary>
    [SnapInSettings("{CFAA3895-4B02-4431-A168-A6416013C3DC}", DisplayName = "Selection (MmcListView) Sample", Description = "Shows MmcListView with multi-selection.", Vendor="Neos-Sdi")]
    public class SelectionListviewSnapIn : SnapIn
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public SelectionListviewSnapIn()
        {
            // Create the root node.
            this.RootNode = new ScopeNode();
            this.RootNode.DisplayName = "Selection (MmcListView) Sample";

            // Create a message view for the root node.
            MmcListViewDescription lvd = new MmcListViewDescription();
            lvd.DisplayName = "Users (MmcListView)";
            lvd.ViewType = typeof(SelectionListView);
            lvd.Options = MmcListViewOptions.ExcludeScopeNodes;

            // Attach the view to the root node.
            this.RootNode.ViewDescriptions.Add(lvd);
            this.RootNode.ViewDescriptions.DefaultIndex = 0;
            this.RootNode.Children.Add(new ScopeNode());
            this.RootNode.Children.Add(new ScopeNode());
            this.RootNode.Children.Add(new ScopeNode());
            this.RootNode.Children.Add(new ScopeNode());
        }
    }

    /// <summary>
    /// This class provides list of icons and names in the results pane.
    /// </summary>
    public class SelectionListView : MmcListView
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public SelectionListView()
        {
        }

        /// <summary>
        /// Defines the structure of the list view.
        /// </summary>
        /// <param name="status"></param>
        protected override void OnInitialize(AsyncStatus status)
        {
            // do default handling
            base.OnInitialize(status);

            // Create a set of columns for use in the list view
            // Define the default column title
            this.Columns[0].Title = "User";
            this.Columns[0].SetWidth(300);

            // Add detail column
            this.Columns.Add(new MmcListViewColumn("Birthday", 200));

            // Set to show all columns
            this.Mode = MmcListViewMode.Report;  // default (set for clarity)

            // Set to show refresh as an option
            this.SelectionData.EnabledStandardVerbs = StandardVerbs.Refresh;

            // Load the list with values
            Refresh();
        }

        /// <summary>
        /// Defines actions for selection.
        /// </summary>
        /// <param name="status"></param>
        protected override void OnSelectionChanged(SyncStatus status)
        {
            if (this.SelectedNodes.Count == 0)
            {
                this.SelectionData.Clear();
            }
            else
            {
                this.SelectionData.Update(GetSelectedUsers(), this.SelectedNodes.Count > 1, null, null);
                this.SelectionData.ActionsPaneItems.Clear();
                this.SelectionData.ActionsPaneItems.Add(new Microsoft.ManagementConsole.Action("Show Selected", "Shows list of selected Users.", -1, "ShowSelected"));
            }
        }

        /// <summary>
        /// Handles menu actions.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="status"></param>
        protected override void OnSelectionAction(Microsoft.ManagementConsole.Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "ShowSelected":
                    {
                        ShowSelected();
                        break;
                    }
            }
        }

        /// <summary>
        /// Placeholder.
        /// </summary>
        /// <param name="status"></param>
        protected override void OnRefresh(AsyncStatus status)
        {
            Refresh();
           // MessageBox.Show("The method or operation is not implemented.");
        }

        /// <summary>
        /// Shows selected items.
        /// </summary>
        private void ShowSelected()
        {
            MessageBox.Show("Selected Users: \n" + GetSelectedUsers());
        }

        /// <summary>
        /// Builds a string of selected users.
        /// </summary>
        /// <returns></returns>
        private string GetSelectedUsers()
        {
            StringBuilder selectedUsers = new StringBuilder();

            foreach (ResultNode resultNode in this.SelectedNodes)
            {

                selectedUsers.Append(resultNode.DisplayName + "\n");
            }

            return selectedUsers.ToString();
        }

        /// <summary>
        /// Loads the list view with data.
        /// </summary>
        public void Refresh()
        {
            // Clear existing information.
            this.ResultNodes.Clear();

            // Use fictitious data to populate the lists.
            string[][] users = { new string[] {"Karen", "February 14th"},
                                new string[] {"Sue", "May 5th"},
                                new string[] {"Tina", "April 15th"},
                                new string[] {"Lisa", "March 27th"},
                                new string[] {"Tom", "December 25th"},
                                new string[] {"John", "January 1st"},
                                new string[] {"Harry", "October 31st"},
                                new string[] {"Bob", "July 4th"}
                            };

            // Populate the list.
            foreach (string[] user in users)
            {
                ResultNode node = new ResultNode();
                node.DisplayName = user[0];
                node.SubItemDisplayNames.Add(user[1]);

                this.ResultNodes.Add(node);
            }
        }
    } 
}
