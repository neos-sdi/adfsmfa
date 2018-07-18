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
using Neos.IdentityServer.Console.Controls;
using System.Threading;
using Neos.IdentityServer.MultiFactor;
using System.DirectoryServices;

namespace Neos.IdentityServer.Console
{
    public partial class ServiceViewControl : UserControl, IFormViewControl
    {
        private Control oldParent;
        private ServiceFormView _frm = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public ServiceViewControl()
        {
            InitializeComponent();
        }

        private void mgr_ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {

        }

        private void mgr_BroadcastStatusChanged(ADFSServiceManager mgr, ServiceOperationStatus status, string servername, Exception Ex = null)
        {

        }

        /// <summary>
        /// Initialize method 
        /// </summary>
        public void Initialize(FormView view)
        {
            FormView = (ServiceFormView)view;
            OnInitialize();
        }

        /// <summary>
        /// OnInitialize method
        /// </summary>
        protected virtual void OnInitialize()
        {
            ManagementAdminService.ADFSManager.ConfigurationStatusChanged += mgr_ConfigurationStatusChanged;
            ManagementAdminService.ADFSManager.RefreshConfigurationStatus();

            int i = 1;
            foreach (ADFSServerHost srv in ManagementAdminService.ADFSManager.ADFSFarm.Servers)
            {
                bool isok = ManagementAdminService.ADFSManager.IsRunning(srv.FQDN);
                this.tableLayoutPanel.Controls.Add(new ADFSServerControl(srv, isok), 0, i);
                i++;
            }
        }

        internal void RefreshData()
        {
            for (int j = this.tableLayoutPanel.Controls.Count-1; j >= 0; j--)
            {
                Control ctrl = this.tableLayoutPanel.Controls[j];
                if (ctrl is ADFSServerControl)
                    this.tableLayoutPanel.Controls.RemoveAt(j);
            }
            ManagementAdminService.ADFSManager.RefreshServiceStatus();
            int i = 1;
            foreach (ADFSServerHost srv in ManagementAdminService.ADFSManager.ADFSFarm.Servers)
            {
                bool isok = ManagementAdminService.ADFSManager.IsRunning(srv.FQDN);
                this.tableLayoutPanel.Controls.Add(new ADFSServerControl(srv, isok), 0, i);
                i++;
            }
        }

        #region Properties
        /// <summary>
        /// FormView property implementation
        /// </summary>
        protected ServiceFormView FormView
        {
            get { return _frm; }
            private set { _frm = value; }
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
        protected ServiceScopeNode ScopeNode
        {
            get { return this.FormView.ScopeNode as ServiceScopeNode; }
        }

        /// <summary>
        /// OnParentChanged method override
        /// </summary>
        protected override void OnParentChanged(EventArgs e)
        {
            if (Parent != null)
            {
                if (!DesignMode)
                {
                    Size = Parent.ClientSize;
                    tableLayoutPanel.Size = Size; 
                }
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
    }
}
