using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ManagementConsole;
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Admin;
using System.Diagnostics;
using System.Collections;

namespace Neos.IdentityServer.Console
{
    public partial class UsersFormViewControl : UserControl, IFormViewControl
    {
        UsersFormView selectionFormView = null;

        private ListViewColumnSorter lvwColumnSorter;


        /// <summary>
        /// Constructor
        /// </summary>
        public UsersFormViewControl()
        {
            // Initialize the control.
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            //Set up the list.
            UserListView.View = System.Windows.Forms.View.Details;

            ColumnHeader userColumnHeader = new ColumnHeader();
            userColumnHeader.Text = "Nom";
            userColumnHeader.Width = 200;
            UserListView.Columns.Add(userColumnHeader);

            ColumnHeader mailColumnHeader = new ColumnHeader();
            mailColumnHeader.Text = "Email";
            mailColumnHeader.Width = 200;
            UserListView.Columns.Add(mailColumnHeader);

            ColumnHeader phoneColumnHeader = new ColumnHeader();
            phoneColumnHeader.Text = "Téléphone";
            phoneColumnHeader.Width = 100;
            UserListView.Columns.Add(phoneColumnHeader);

            ColumnHeader creationColumnHeader = new ColumnHeader();
            creationColumnHeader.Text = "Crée";
            creationColumnHeader.Width = 80;
            UserListView.Columns.Add(creationColumnHeader);

            ColumnHeader enabledColumnHeader = new ColumnHeader();
            enabledColumnHeader.Text = "Actif";
            enabledColumnHeader.Width = 40;
            UserListView.Columns.Add(enabledColumnHeader);

            lvwColumnSorter = new ListViewColumnSorter();
            this.UserListView.ListViewItemSorter = lvwColumnSorter;
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="parentSelectionFormView"></param>
        void IFormViewControl.Initialize(FormView parentSelectionFormView)
        {
            selectionFormView = (UsersFormView)parentSelectionFormView;
        }

        /// <summary>
        /// Populate the list with the sample data.
        /// </summary>
        /// <param name="users"></param>
        public void RefreshData(List<Registration> users)
        {
            UserListView.Items.Clear();
            foreach (Registration user in users)
            {
                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Text = user.UPN;
                listViewItem.SubItems.Add(user.MailAddress);
                listViewItem.SubItems.Add(user.PhoneNumber);
                listViewItem.SubItems.Add(user.CreationDate.ToShortDateString());
                listViewItem.SubItems.Add(user.Enabled.ToString());
                if (user.Enabled)
                    listViewItem.ImageIndex = 0;
                else
                    listViewItem.ImageIndex = 1;
                UserListView.Items.Add(listViewItem);
            }
            if (UserListView.Items.Count>0)
                UserListView.SelectedIndices.Add(0);
        }

        /// <summary>
        /// RefreshCurrent method implementation
        /// </summary>
        public void RefreshCurrent(Registration registration)
        {
            ListViewItem listViewItem = GetSelectedItem();
            listViewItem.Text = registration.UPN;
            listViewItem.SubItems[1].Text = registration.MailAddress;
            listViewItem.SubItems[2].Text = registration.PhoneNumber;
            listViewItem.SubItems[3].Text = registration.CreationDate.ToShortDateString();
            listViewItem.SubItems[4].Text = registration .Enabled.ToString();
            if (registration.Enabled)
                listViewItem.ImageIndex = 0;
            else
                listViewItem.ImageIndex = 1;
            selectionFormView.EnableDisableAction(registration.Enabled);
            selectionFormView.SelectionData.DisplayName = registration.UPN;
        }

        /// <summary>
        /// DeleteCurrent method
        /// </summary>
        public void DeleteCurrent(Registration registration)
        {
            ListViewItem listViewItem = GetSelectedItem();
            int idx = listViewItem.Index;
            UserListView.Items.Remove(listViewItem);
            if (UserListView.Items.Count > 0)
                if (idx < UserListView.Items.Count)
                    UserListView.SelectedIndices.Add(idx);
                else
                    UserListView.SelectedIndices.Add(0);
        }

        public void AddCurrent(Registration registration)
        {
            ListViewItem listViewItem = new ListViewItem();
            listViewItem.Text = registration.UPN;
            listViewItem.SubItems.Add(registration.MailAddress);
            listViewItem.SubItems.Add(registration.PhoneNumber);
            listViewItem.SubItems.Add(registration.CreationDate.ToShortDateString());
            listViewItem.SubItems.Add(registration.Enabled.ToString());
            if (registration.Enabled)
                listViewItem.ImageIndex = 0;
            else
                listViewItem.ImageIndex = 1;
            UserListView.Items.Add(listViewItem);
            UserListView.SelectedIndices.Clear();
            UserListView.SelectedIndices.Add(UserListView.Items.IndexOf(listViewItem));
        }

        /// <summary>
        /// Build a string of selected users.
        /// </summary>
        private Registration GetSelectedUser()
        {
            ListViewItem itm = GetSelectedItem();
            if (itm != null)
                return RemoteAdminService.GetUser(itm.Text);
            else
                return null;
        }

        /// <summary>
        /// GetSelectedItem method implmentation
        /// </summary>
        private ListViewItem GetSelectedItem()
        {
            if (UserListView.FocusedItem != null)
                return UserListView.FocusedItem;
            else if (UserListView.SelectedIndices.Count > 0)
                return UserListView.Items[UserListView.SelectedIndices[0]];
            else
                return null;
        }

        /// <summary>
        /// ShowPropertySheet method implementation
        /// </summary>
        public void ShowPropertySheet()
        {
            if (selectionFormView.SelectionData.SelectionObject != null)
            {
                Registration reg = (Registration)selectionFormView.SelectionData.SelectionObject;
                selectionFormView.SelectionData.ShowPropertySheet(string.Format("Propriétés : {0}", reg.UPN));   // triggers OnAddPropertyPages
            }
        }

        /// <summary>
        /// Update the context.
        /// </summary>
        private void UserListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (UserListView.SelectedItems.Count == 0)
            {
                selectionFormView.SelectionData.Clear();
            }
            else
            {
                Registration reg = GetSelectedUser();
                selectionFormView.SelectionData.Update(reg, false, null, null);
                selectionFormView.EnableDisableAction(reg.Enabled);
                selectionFormView.SelectionData.DisplayName = reg.UPN;
            }
        }

        /// <summary>
        /// MouseClick event
        /// </summary>
        private void UserListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Check selected items for a right-click.
                bool rightClickedOnSelection = false;

                ListViewItem rightClickedItem = UserListView.GetItemAt(e.X, e.Y);
                if (rightClickedItem == null || rightClickedItem.Selected == false)
                {
                    rightClickedOnSelection = false;
                }
                else
                {
                    rightClickedOnSelection = true;
                }
                selectionFormView.ShowContextMenu(PointToScreen(e.Location), rightClickedOnSelection);
            }
        }

        /// <summary>
        /// MouseDoubleClick event
        /// </summary>
        private void UserListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ShowPropertySheet();
            }
        }

        /// <summary>
        /// ColumnClick event
        /// </summary>
        private void UserListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                    lvwColumnSorter.Order = SortOrder.Descending;
                else
                    lvwColumnSorter.Order = SortOrder.Ascending;
            }
            else
            {
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }
            this.UserListView.Sort();
        }
    } // class

    #region ListView Sorting
    /// <summary>
    /// ListViewColumnSorter Class
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        private int ColumnToSort;
        private SortOrder OrderOfSort;
        private CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// Constructor
        /// </summary>
        public ListViewColumnSorter()
        {
            ColumnToSort = 0;
            OrderOfSort = SortOrder.None;
            ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// Compare method implementation
        /// </summary>
        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listviewX, listviewY;


            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
            if (OrderOfSort == SortOrder.Ascending)
            {
                return compareResult;
            }
            else if (OrderOfSort == SortOrder.Descending)
            {
                return (-compareResult);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// SortColumn property implementation
        /// </summary>
        public int SortColumn
        {
            get { return ColumnToSort; }
            set { ColumnToSort = value; }
        }

        /// <summary>
        /// Order property implmentation
        /// </summary>
        public SortOrder Order
        {
            get { return OrderOfSort; }
            set { OrderOfSort = value; }
        }
    }
    #endregion
}
