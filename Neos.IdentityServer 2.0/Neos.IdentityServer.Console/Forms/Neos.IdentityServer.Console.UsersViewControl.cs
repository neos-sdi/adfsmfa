//******************************************************************************************************************************************************************************************//
// Copyright (c) 2017 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
// https://adfsmfa.codeplex.com                                                                                                                                                             //
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

namespace Neos.IdentityServer.Console
{
    public delegate void SelectionEventHandler(object sender, SelectionDataEventArgs e);

    public partial class UsersListView : UserControl, IFormViewControl
    {
        private UsersFormView _frm = null;
        private Control oldParent;
        private MMCRegistrationList _lst = null;

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
          //  RemoteAdminService.Paging.PageSize = 5000;
          //  RemoteAdminService.Paging.CurrentPage = 1;
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
            if (this.DataSelectionChanged != null)
                this.DataSelectionChanged(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.SelectionChanged));
           // IMG.ImageLayout = DataGridViewImageCellLayout.Normal;
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
        protected List<MMCRegistration> DataList
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
            try
            {
                _lst = ManagementAdminService.GetUsers();
                if (clearselection)
                   this.GridView.RowCount = 0;
                this.GridView.RowCount = ManagementAdminService.GetUsersCount(); 
                if (refreshgrid)
                    this.GridView.Refresh();
                if (clearselection)
                {
                   // this.GridView.ClearSelection(); 
                }

            }
            finally
            {
                this.UseWaitCursor = false;
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// SetUserData method implementation
        /// </summary>
        internal void SetUserData(MMCRegistrationList registrations)
        {
            ManagementAdminService.SetUser(registrations);
            UpdateRows(registrations);
        }

        /// <summary>
        /// AddUserData method implmentation
        /// </summary>
        internal void AddUserData(MMCRegistrationList registrations)
        {
            MMCRegistrationList results = ManagementAdminService.AddUser(registrations);
            AddRows(results);
        }

        /// <summary>
        /// DeleteUserData method implementation
        /// </summary>
        internal bool DeleteUserData(MMCRegistrationList registrations)
        {
            bool ret = ManagementAdminService.DeleteUser(registrations);
            DeleteRows(registrations);
            return ret;
        }

        /// <summary>
        /// EnableUserData method implementation
        /// </summary>
        internal void EnableUserData(MMCRegistrationList registrations)
        {
            MMCRegistrationList results = ManagementAdminService.EnableUser(registrations);
            EnableDisableRows(results);
        }

        /// <summary>
        /// DisableUserData method implementation
        /// </summary>
        internal void DisableUserData(MMCRegistrationList registrations)
        {
            MMCRegistrationList results = ManagementAdminService.DisableUser(registrations);
            EnableDisableRows(results);
        }

        /// <summary>
        /// GetSelectedUsers method implementation
        /// </summary>
        internal MMCRegistrationList GetSelectedUsers()
        {
            MMCRegistrationList result = new MMCRegistrationList();
            foreach (DataGridViewRow row in GridView.SelectedRows)
            {
                MMCRegistration reg = new MMCRegistration();
                reg.ID = GridView.Rows[row.Index].Cells[1].Value.ToString();
                if (reg.ID != Guid.Empty.ToString())
                {
                    reg.UPN = GridView.Rows[row.Index].Cells[2].Value.ToString();
                    reg.MailAddress = GridView.Rows[row.Index].Cells[3].Value.ToString();
                    reg.PhoneNumber = GridView.Rows[row.Index].Cells[4].Value.ToString();
                    reg.CreationDate = Convert.ToDateTime(GridView.Rows[row.Index].Cells[5].Value);
                    reg.PreferredMethod = (RegistrationPreferredMethod)Enum.Parse(typeof(RegistrationPreferredMethod), GridView.Rows[row.Index].Cells[6].Value.ToString());
                    reg.Enabled = (bool)bool.Parse(GridView.Rows[row.Index].Cells[7].Value.ToString());
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
            int idx = rowindex % ManagementAdminService.Paging.PageSize;
            int page = (rowindex / ManagementAdminService.Paging.PageSize) + 1;
            if (page != ManagementAdminService.Paging.CurrentPage)
            {
                this.UseWaitCursor = true;
                this.Cursor = Cursors.WaitCursor;
                try
                {
                    ManagementAdminService.Paging.CurrentPage = page;
                    _lst = ManagementAdminService.GetUsers();
                }
                finally
                {
                    this.UseWaitCursor = false;
                    this.Cursor = Cursors.Default;
                }
            }
            return idx;
        }
        #endregion

        #region Update Grid
        /// <summary>
        /// AddRows method implementation
        /// </summary>
        private void AddRows(MMCRegistrationList registrations)
        {
            RefreshData(true);
            if (this.DataSelectionChanged != null)
                this.DataSelectionChanged(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.SelectionChanged));
        }

        /// <summary>
        /// UpdateRows method implementation
        /// </summary>
        private void UpdateRows(MMCRegistrationList registrations)
        {
            RefreshData(true);
            if (this.DataSelectionChanged != null)
                this.DataSelectionChanged(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.SelectionChanged));
        }

        /// <summary>
        /// DeleteRows method implementation
        /// </summary>
        private void DeleteRows(MMCRegistrationList registrations)
        {
            RefreshData(true);
            if (this.DataSelectionChanged != null)
                this.DataSelectionChanged(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.SelectionChanged));
        }

        /// <summary>
        /// EnableDisableRows method implementation
        /// </summary>
        private void EnableDisableRows(MMCRegistrationList registrations)
        {
            RefreshData(true);
            if (this.DataSelectionChanged != null)
                this.DataSelectionChanged(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.SelectionChanged));
        }
        #endregion

        #region Notification Events
        /// <summary>
        /// GridView_SelectionChanged event implementation
        /// </summary>
        private void GridView_SelectionChanged(object sender, EventArgs e)
        {
            if (this.DataSelectionChanged != null)
                this.DataSelectionChanged(this, new SelectionDataEventArgs(GetSelectedUsers(),MMCListAction.SelectionChanged));
        }

        /// <summary>
        /// GridView_DoubleClick event implmentation
        /// </summary>
        private void GridView_DoubleClick(object sender, EventArgs e)
        {
            if (this.DataEditionActivated != null)
                this.DataEditionActivated(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.EditionActivated));
        }

        /// <summary>
        /// GridView_KeyPress event implementation
        /// </summary>
        private void GridView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar==(char)Keys.Return)
            {
                e.Handled = true;
                if (this.DataEditionActivated != null)
                    this.DataEditionActivated(this, new SelectionDataEventArgs(GetSelectedUsers(), MMCListAction.EditionActivated));

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
                            e.Value = Neos.IdentityServer.Console.Neos_IdentityServer_Console_Snapin.small_user_enabled;
                        else
                            e.Value = Neos.IdentityServer.Console.Neos_IdentityServer_Console_Snapin.small_user_disabled;
                    }
                    else
                        e.Value = Neos.IdentityServer.Console.Neos_IdentityServer_Console_Snapin.small_user_disabled;
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
                        e.Value = _lst[idx].CreationDate;
                    else
                        e.Value = DateTime.MinValue;
                    break;
                case 6:
                    if (isfound)
                        e.Value = _lst[idx].PreferredMethod;
                    else
                        e.Value = RegistrationPreferredMethod.Choose;
                    break;
                case 7:
                    if (isfound)
                        e.Value = _lst[idx].Enabled;
                    else
                        e.Value = false;
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
                    _lst[idx].CreationDate = Convert.ToDateTime(e.Value.ToString());
                    break;
                case 6:
                    _lst[idx].PreferredMethod = (RegistrationPreferredMethod)Enum.Parse(typeof(RegistrationPreferredMethod), e.Value.ToString());
                    break;
                case 7:
                    _lst[idx].Enabled = bool.Parse(e.Value.ToString());
                    break;
            }
        }

        /// <summary>
        /// ColumnHeaderMouseClick event implmen,tation
        /// </summary>
        private void GridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            UsersOrderField ff = ManagementAdminService.Order.Column;
            SortDirection ss = ManagementAdminService.Order.Direction;
            switch (e.ColumnIndex)
            {
                case 1: ManagementAdminService.Order.Column = UsersOrderField.ID;
                    if (ff == UsersOrderField.ID)
                    {
                        if (ManagementAdminService.Order.Direction == SortDirection.Ascending)
                            ManagementAdminService.Order.Direction = SortDirection.Descending;
                        else
                            ManagementAdminService.Order.Direction = SortDirection.Ascending;
                    }
                    else
                        ManagementAdminService.Order.Direction = SortDirection.Ascending;
                   break;
                case 2: ManagementAdminService.Order.Column = UsersOrderField.UserName;
                   if (ff == UsersOrderField.UserName)
                   {
                       if (ManagementAdminService.Order.Direction == SortDirection.Ascending)
                           ManagementAdminService.Order.Direction = SortDirection.Descending;
                       else
                           ManagementAdminService.Order.Direction = SortDirection.Ascending;
                   }
                   else
                       ManagementAdminService.Order.Direction = SortDirection.Ascending;
                   break;
                case 3: ManagementAdminService.Order.Column = UsersOrderField.Email;
                   if (ff == UsersOrderField.Email)
                   {
                       if (ManagementAdminService.Order.Direction == SortDirection.Ascending)
                           ManagementAdminService.Order.Direction = SortDirection.Descending;
                       else
                           ManagementAdminService.Order.Direction = SortDirection.Ascending;
                   }
                   else
                       ManagementAdminService.Order.Direction = SortDirection.Ascending;
                   break;
                case 4: ManagementAdminService.Order.Column = UsersOrderField.Phone;
                   if (ff == UsersOrderField.Phone)
                   {
                       if (ManagementAdminService.Order.Direction == SortDirection.Ascending)
                           ManagementAdminService.Order.Direction = SortDirection.Descending;
                       else
                           ManagementAdminService.Order.Direction = SortDirection.Ascending;
                   }
                   else
                       ManagementAdminService.Order.Direction = SortDirection.Ascending;
                   break;
                case 5: ManagementAdminService.Order.Column = UsersOrderField.CreationDate;
                   if (ff == UsersOrderField.CreationDate)
                   {
                       if (ManagementAdminService.Order.Direction == SortDirection.Ascending)
                           ManagementAdminService.Order.Direction = SortDirection.Descending;
                       else
                           ManagementAdminService.Order.Direction = SortDirection.Ascending;
                   }
                   else
                       ManagementAdminService.Order.Direction = SortDirection.Ascending;
                   break;
            }
            RefreshData(true);
           // RefreshData(true, true);
        }
        #endregion
    }

    public class SelectionDataEventArgs : EventArgs
    {
        MMCRegistrationList _list = null;
        MMCListAction _action;

        public SelectionDataEventArgs(MMCRegistrationList list, MMCListAction action)
        {
            _list = list;
            _action = action;
        }

        /// <summary>
        /// Selection property
        /// </summary>
        public MMCRegistrationList Selection
        {
            get { return _list; }
        }

        /// <summary>
        /// Action  property
        /// </summary>
        public MMCListAction Action
        {
            get { return _action; }
        } 
    }

    public enum MMCListAction
    {
        SelectionChanged = 0,
        EditionActivated = 1,
    }

}
