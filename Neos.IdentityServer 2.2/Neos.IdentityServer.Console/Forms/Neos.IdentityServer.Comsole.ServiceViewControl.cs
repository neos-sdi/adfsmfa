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
using Neos.IdentityServer.Console.Controls;
using System.Threading;
using Neos.IdentityServer.MultiFactor;
using System.DirectoryServices;
using Neos.IdentityServer.MultiFactor.Administration;

namespace Neos.IdentityServer.Console
{
    public partial class ServiceViewControl : UserControl, IFormViewControl, IMMCNotificationData
    {
        private Control oldParent;
        private ServiceFormView _frm = null;
        private ConfigurationControl _ctrl = null;
        private List<ADFSServerControl> _lst = new List<ADFSServerControl>();
        private bool _notifenabled;

        /// <summary>
        /// Constructor
        /// </summary>
        public ServiceViewControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize method 
        /// </summary>
        public void Initialize(FormView view)
        {
            FormView = (ServiceFormView)view;
            OnInitialize();
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
        /// ControlInstance property implmentation
        /// </summary>
        protected ConfigurationControl ControlInstance
        {
            get { return _ctrl; }
            private set { _ctrl = value; }
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
        #endregion

        /// <summary>
        /// OnInitialize method
        /// </summary>
        protected virtual void OnInitialize()
        {
            this.Cursor = Cursors.WaitCursor;
            this.SuspendLayout();
            try
            {
                ControlInstance = new ConfigurationControl(this);
                this.tableLayoutPanel.Controls.Add(ControlInstance, 0, 1);

                bool isconfigured = ManagementService.ADFSManager.IsFarmConfigured();
                _lst.Clear();
                if (isconfigured)
                {
                    int i = 3;
                    foreach (ADFSServerHost srv in ManagementService.ADFSManager.ADFSFarm.Servers)
                    {
                        bool isok = ManagementService.ADFSManager.IsRunning(srv.FQDN);
                        ADFSServerControl crt = new ADFSServerControl(this, srv, isok);
                        _lst.Add(crt);
                        this.tableLayoutPanel.Controls.Add(crt, 0, i);
                        i++;
                    }
                }
            }
            finally
            {
                this.ResumeLayout(true);
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// RefreshData method implementation
        /// </summary>
        internal void RefreshData()
        {
            this.SuspendLayout();
            this.Cursor = Cursors.WaitCursor;
            _notifenabled = false;
            try
            {
                ComponentResourceManager resources = new ComponentResourceManager(typeof(ServiceViewControl));
                this.label1.Text = resources.GetString("label1.Text");
                this.label2.Text = resources.GetString("label2.Text");
                this.label3.Text = resources.GetString("label3.Text");

                ManagementService.ADFSManager.ReadConfiguration(null);
                ((IMMCRefreshData)ControlInstance).DoRefreshData();
                bool isconfigured = ManagementService.ADFSManager.IsFarmConfigured();
                if (isconfigured)
                {
                    foreach (ADFSServerControl srv in _lst)
                    {
                        ((IMMCRefreshData)srv).DoRefreshData();
                    }
                }
            }
            finally
            {
                _notifenabled = true;
                this.Cursor = Cursors.Default;
                this.ResumeLayout();
            }
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

        /// <summary>
        /// IsNotifsEnabled method implementation
        /// </summary>
        public bool IsNotifsEnabled()
        {
            return _notifenabled;
        }
    }
}
