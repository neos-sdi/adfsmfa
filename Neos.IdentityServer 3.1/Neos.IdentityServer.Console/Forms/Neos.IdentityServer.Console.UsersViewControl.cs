//******************************************************************************************************************************************************************************************//
// Copyright (c) 2022 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
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
using Neos.IdentityServer.MultiFactor.Administration;
using System.Threading;
using Neos.IdentityServer.MultiFactor;
using System.DirectoryServices;
using Microsoft.ManagementConsole.Advanced;

namespace Neos.IdentityServer.Console
{
    public delegate void SelectionEventHandler(object sender, SelectionDataEventArgs e);

    public partial class UsersListView : UserControl, IFormViewControl
    {
        private UsersFormView _frm = null;
        private Control oldParent;
        private MFAUserList _lst = null;

        public event SelectionEventHandler DataSelectionChanged;
        public event SelectionEventHandler DataEditionActivated;

        /// <summary>
        /// Constructor
        /// </summary>
        public UsersListView()
        {
            InitializeComponent(); 
        }

        /// <summary>
        /// Initialize method 
        /// </summary>
        public void Initialize(FormView view)
        {
            FormView = (UsersFormView)view;
            FormView.PlugEvents(this);
            OnInitialize();

        }

        /// <summary>
        /// OnInitialize method
        /// </summary>
        protected virtual void OnInitialize() 
        {
            RefreshData();
            if (GridView.RowCount > 0)
            {
                GridView.Rows[0].Selected = true;
                GridView.Rows[0].Cells[2].Selected = true;
            }
            DataSelectionChanged?.Invoke(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.SelectionChanged));
        }

        #region Properties
        /// <summary>
        /// FormView property implementation
        /// </summary>
        protected UsersFormView FormView 
        { 
            get { return _frm;} 
            private set{ _frm = value;}  
        }

        /// <summary>
        /// SnapIn method implementation
        /// </summary>
        protected NamespaceSnapInBase SnapIn
        {
            get { return this.FormView.ScopeNode.SnapIn; }
        }

        /// <summary>
        /// ScopeNode method implementation
        /// </summary>
        protected UsersScopeNode ScopeNode
        {
            get { return this.FormView.ScopeNode as UsersScopeNode; }
        }

        /// <summary>
        /// DataList property
        /// </summary>
        protected List<MFAUser> DataList
        {
            get { return _lst; }
        }

        /// <summary>
        /// OnParentChanged method override
        /// </summary>
        protected override void OnParentChanged(EventArgs e)
        {
            if (Parent != null)
            {
                if (!DesignMode) 
                    Size = Parent.ClientSize;
                Parent.SizeChanged += Parent_SizeChanged;
            }
            if (oldParent != null)
            {
                oldParent.SizeChanged -= Parent_SizeChanged;
            }
            oldParent = Parent;
            base.OnParentChanged(e);
        }

        /// <summary>
        /// Parent_SizeChanged event
        /// </summary>
        private void Parent_SizeChanged(object sender, EventArgs e)
        {
            if (!DesignMode) 
               Size = Parent.ClientSize;
        }
        #endregion      

        #region Data
        /// <summary>
        /// RefreshData method implmentation
        /// </summary>
        public void RefreshData(bool refreshgrid = false, bool clearselection = false)
        {
            this.UseWaitCursor = true;
            this.Cursor = Cursors.WaitCursor;
            ComponentResourceManager resources = new ComponentResourceManager(typeof(UsersListView));
            this.uPNDataGridViewTextBoxColumn.HeaderText = resources.GetString("uPNDataGridViewTextBoxColumn.HeaderText");
            this.mailAddressDataGridViewTextBoxColumn.HeaderText = resources.GetString("mailAddressDataGridViewTextBoxColumn.HeaderText");
            this.phoneNumberDataGridViewTextBoxColumn.HeaderText = resources.GetString("phoneNumberDataGridViewTextBoxColumn.HeaderText");
            this.preferredMethodDataGridViewTextBoxColumn.HeaderText = resources.GetString("preferredMethodDataGridViewTextBoxColumn.HeaderText");
            this.enabledDataGridViewCheckBoxColumn.HeaderText = resources.GetString("enabledDataGridViewCheckBoxColumn.HeaderText");
            try
            {
                _lst = MMCService.GetUsers();
                if (clearselection)
                   GridView.RowCount = 0;
               // GridView.RowCount = _lst.Count;
                GridView.RowCount = MMCService.GetUsersCount(); 
                if (refreshgrid)
                    GridView.Refresh();
                if (clearselection)
                   GridView.ClearSelection(); 
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                SnapIn.Console.ShowDialog(messageBoxParameters);
            }
            finally
            {
                UseWaitCursor = false;
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// SetUserData method implementation
        /// </summary>
        internal void SetUserData(MFAUserList registrations)
        {
            try
            {
                MMCService.SetUser(registrations);
                UpdateRows(registrations);
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                SnapIn.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// AddUserData method implmentation
        /// </summary>
        internal void AddUserData(MFAUserList registrations)
        {
            try
            { 
                MFAUserList results = MMCService.AddUser(registrations);
                AddRows(results);
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                SnapIn.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// DeleteUserData method implementation
        /// </summary>
        internal bool DeleteUserData(MFAUserList registrations)
        {
            try
            {
                bool ret = MMCService.DeleteUser(registrations);
                DeleteRows(registrations);
                return ret;
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                SnapIn.Console.ShowDialog(messageBoxParameters);
                return false;
            }
        }

        /// <summary>
        /// EnableUserData method implementation
        /// </summary>
        internal void EnableUserData(MFAUserList registrations)
        {
            try
            {
                MFAUserList results = MMCService.EnableUser(registrations);
                EnableDisableRows(results);
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                SnapIn.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// DisableUserData method implementation
        /// </summary>
        internal void DisableUserData(MFAUserList registrations)
        {
            try
            {
                MFAUserList results = MMCService.DisableUser(registrations);
                EnableDisableRows(results);
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                SnapIn.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// GetSelectedUsers method implementation
        /// </summary>
        internal MFAUserList GetSelectedUsers()
        {
            MFAUserList result = new MFAUserList();
            foreach (DataGridViewRow row in GridView.SelectedRows)
            {
                MFAUser reg = new MFAUser();
                reg.ID = GridView.Rows[row.Index].Cells[1].Value.ToString();
                if (reg.ID != Guid.Empty.ToString())
                {
                    reg.UPN = GridView.Rows[row.Index].Cells[2].Value.ToString();
                    reg.MailAddress = GridView.Rows[row.Index].Cells[3].Value.ToString();
                    reg.PhoneNumber = GridView.Rows[row.Index].Cells[4].Value.ToString();
                    reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), GridView.Rows[row.Index].Cells[5].Value.ToString());
                    reg.Enabled = (bool)bool.Parse(GridView.Rows[row.Index].Cells[6].Value.ToString());
                    reg.PIN = Convert.ToInt32(GridView.Rows[row.Index].Cells[7].Value.ToString());
                    reg.OverrideMethod = GridView.Rows[row.Index].Cells[8].Value.ToString();
                    reg.IsRegistered = true;
                    reg.IsApplied = true;
                    result.Add(reg);
                }
            }
            return result;
        }

        /// <summary>
        /// EnsurePageForRowIndex method implmentation
        /// </summary>
        private int EnsurePageForRowIndex(int rowindex)
        {
            int idx = rowindex % MMCService.Paging.PageSize;
            int page = (rowindex / MMCService.Paging.PageSize) + 1;
            if (page != MMCService.Paging.CurrentPage)
            {
                UseWaitCursor = true;
                Cursor = Cursors.WaitCursor;
                try
                {
                    MMCService.Paging.CurrentPage = page;
                    _lst = MMCService.GetUsers();
                }
                catch (Exception ex)
                {
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                    {
                        Text = ex.Message,
                        Buttons = MessageBoxButtons.OK,
                        Icon = MessageBoxIcon.Error
                    };
                    SnapIn.Console.ShowDialog(messageBoxParameters);
                }
                finally
                {
                    UseWaitCursor = false;
                    Cursor = Cursors.Default;
                }
            }
            return idx;
        }
        #endregion

        #region Update Grid
        /// <summary>
        /// AddRows method implementation
        /// </summary>
        private void AddRows(MFAUserList registrations)
        {
            RefreshData(true);
            DataSelectionChanged?.Invoke(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.SelectionChanged));
        }

        /// <summary>
        /// UpdateRows method implementation
        /// </summary>
        private void UpdateRows(MFAUserList registrations)
        {
            RefreshData(true);
            DataSelectionChanged?.Invoke(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.SelectionChanged));
        }

        /// <summary>
        /// DeleteRows method implementation
        /// </summary>
        private void DeleteRows(MFAUserList registrations)
        {
            RefreshData(true, true);
            DataSelectionChanged?.Invoke(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.SelectionChanged));
        }

        /// <summary>
        /// EnableDisableRows method implementation
        /// </summary>
        private void EnableDisableRows(MFAUserList registrations)
        {
            RefreshData(true);
            DataSelectionChanged?.Invoke(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.SelectionChanged));
        }
        #endregion

        #region Notification Events
        /// <summary>
        /// GridView_SelectionChanged event implementation
        /// </summary>
        private void GridView_SelectionChanged(object sender, EventArgs e)
        {
            DataSelectionChanged?.Invoke(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.SelectionChanged));
        }

        /// <summary>
        /// GridView_DoubleClick event implmentation
        /// </summary>
        private void GridView_DoubleClick(object sender, EventArgs e)
        {
            DataEditionActivated?.Invoke(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.EditionActivated));
        }

        /// <summary>
        /// GridView_KeyPress event implementation
        /// </summary>
        private void GridView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar==(char)Keys.Return)
            {
                e.Handled = true;
                DataEditionActivated?.Invoke(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.EditionActivated));
            }
        }

        /// <summary>
        /// GridView_CellValueNeeded event implementation
        /// </summary>
        private void GridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            int idx = EnsurePageForRowIndex(e.RowIndex);
            bool isfound = (idx < _lst.Count);
            switch (e.ColumnIndex)
            {
                case 0:
                    if (isfound)
                    {
                        if (_lst[idx].Enabled)
                            e.Value = Resources.Neos_IdentityServer_Console_Snapin.small_user_enabled;
                        else
                            e.Value = Resources.Neos_IdentityServer_Console_Snapin.small_user_disabled;
                    }
                    else
                        e.Value = Resources.Neos_IdentityServer_Console_Snapin.small_user_disabled;
                    break;
                case 1:
                    if (isfound)
                        e.Value = _lst[idx].ID;
                    else
                        e.Value = Guid.Empty.ToString();
                    break;
                case 2:
                    if (isfound)
                        e.Value = _lst[idx].UPN;
                    else
                        e.Value = string.Empty;
                    break;
                case 3:
                    if (isfound)
                        e.Value = _lst[idx].MailAddress;
                    else
                        e.Value = string.Empty;
                    break;
                case 4:
                    if (isfound)
                        e.Value = _lst[idx].PhoneNumber;
                    else
                        e.Value = string.Empty;
                    break;
                case 5:
                    if (isfound)
                        e.Value = _lst[idx].PreferredMethod;
                    else
                        e.Value = PreferredMethod.Choose;
                    break;
                case 6:
                    if (isfound)
                        e.Value = _lst[idx].Enabled;
                    else
                        e.Value = false;
                    break;
                case 7:
                    if (isfound)
                        e.Value = _lst[idx].PIN;
                    else
                        e.Value = string.Empty;
                    break;
                case 8:
                    if (isfound)
                        e.Value = _lst[idx].OverrideMethod;
                    else
                        e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// CellValuePushed event implementation
        /// </summary>
        private void GridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            int idx = EnsurePageForRowIndex(e.RowIndex);
            switch (e.ColumnIndex)
            {
                case 1:
                    _lst[idx].ID = e.Value.ToString();
                    break;
                case 2:
                    _lst[idx].UPN = e.Value.ToString();
                    break;
                case 3:
                    _lst[idx].MailAddress = e.Value.ToString();
                    break;
                case 4:
                    _lst[idx].PhoneNumber = e.Value.ToString();
                    break;
                case 5:
                    _lst[idx].PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), e.Value.ToString());
                    break;
                case 6:
                    _lst[idx].Enabled = bool.Parse(e.Value.ToString());
                    break;
                case 7:
                    if (!string.IsNullOrEmpty(e.Value.ToString()))
                        _lst[idx].PIN = Convert.ToInt32(e.Value);
                    break;
                case 8:
                    _lst[idx].OverrideMethod = e.Value.ToString();
                    break;
            }
        }

        /// <summary>
        /// ColumnHeaderMouseClick event implmen,tation
        /// </summary>
        private void GridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataOrderField ff = MMCService.Order.Column;
            SortDirection ss = MMCService.Order.Direction;
            switch (e.ColumnIndex)
            {
                case 1: MMCService.Order.Column = DataOrderField.ID;
                    if (ff == DataOrderField.ID)
                    {
                        if (MMCService.Order.Direction == SortDirection.Ascending)
                            MMCService.Order.Direction = SortDirection.Descending;
                        else
                            MMCService.Order.Direction = SortDirection.Ascending;
                    }
                    else
                        MMCService.Order.Direction = SortDirection.Ascending;
                   break;
                case 2: MMCService.Order.Column = DataOrderField.UserName;
                   if (ff == DataOrderField.UserName)
                   {
                       if (MMCService.Order.Direction == SortDirection.Ascending)
                           MMCService.Order.Direction = SortDirection.Descending;
                       else
                           MMCService.Order.Direction = SortDirection.Ascending;
                   }
                   else
                       MMCService.Order.Direction = SortDirection.Ascending;
                   break;
                case 3: MMCService.Order.Column = DataOrderField.Email;
                   if (ff == DataOrderField.Email)
                   {
                       if (MMCService.Order.Direction == SortDirection.Ascending)
                           MMCService.Order.Direction = SortDirection.Descending;
                       else
                           MMCService.Order.Direction = SortDirection.Ascending;
                   }
                   else
                       MMCService.Order.Direction = SortDirection.Ascending;
                   break;
                case 4: MMCService.Order.Column = DataOrderField.Phone;
                   if (ff == DataOrderField.Phone)
                   {
                       if (MMCService.Order.Direction == SortDirection.Ascending)
                           MMCService.Order.Direction = SortDirection.Descending;
                       else
                           MMCService.Order.Direction = SortDirection.Ascending;
                   }
                   else
                       MMCService.Order.Direction = SortDirection.Ascending;
                   break;
            }
            RefreshData(true);
           // RefreshData(true, true);
        }
        #endregion
    }

    public class SelectionDataEventArgs : EventArgs
    {
        public SelectionDataEventArgs(MFAUserList list, MMCListAction action)
        {
            Selection = list;
            Action = action;
        }

        /// <summary>
        /// Selection property
        /// </summary>
        public MFAUserList Selection { get; } = null;

        /// <summary>
        /// Action  property
        /// </summary>
        public MMCListAction Action { get; }
    }

    public enum MMCListAction
    {
        SelectionChanged = 0,
        EditionActivated = 1,
    }

}
