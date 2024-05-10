//******************************************************************************************************************************************************************************************//
// Copyright (c) 2024 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using Neos.IdentityServer.Console.Controls;
using Microsoft.ManagementConsole.Advanced;
using Neos.IdentityServer.Console.Forms;

namespace Neos.IdentityServer.Console
{
    public partial class ServiceSecurityRNGViewControl : UserControl, IFormViewControl, IMMCNotificationData
    {
        private Control oldParent;
        private ServiceSecurityRNGFormView _frm = null;
        private SecurityConfigurationRNGControl _ctrl = null;
        private bool _notifenabled = true;

        /// <summary>
        /// Constructor 
        /// </summary>
        public ServiceSecurityRNGViewControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize method 
        /// </summary>
        public void Initialize(FormView view)
        {
            FormView = (ServiceSecurityRNGFormView)view;
            OnInitialize();
        }

        /// <summary>
        /// OnInitialize method
        /// </summary>
        protected virtual void OnInitialize()
        {
            this.SuspendLayout();
            try
            {
                ControlInstance = new SecurityConfigurationRNGControl(this, this.SnapIn);
                this.tableLayoutPanel.Controls.Add(ControlInstance, 0, 1);
                RefreshProviderInformation();
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }

        #region Properties
        /// <summary>
        /// FormView property implementation
        /// </summary>
        protected ServiceSecurityRNGFormView FormView
        {
            get { return _frm; }
            private set { _frm = value; }
        }

        /// <summary>
        /// ControlInstance property implmentation
        /// </summary>
        protected SecurityConfigurationRNGControl ControlInstance
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
        protected ServiceSecurityRNGScopeNode ScopeNode
        {
            get { return this.FormView.ScopeNode as ServiceSecurityRNGScopeNode; }
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
                ManagementService.ADFSManager.ReadConfiguration(null);
                RefreshProviderInformation();
                ((IMMCRefreshData)ControlInstance).DoRefreshData();
            }
            finally
            {
                _notifenabled = true; 
                this.Cursor = Cursors.Default;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// CancelData method implementation
        /// </summary>
        internal void CancelData()
        {
            try
            {
                ComponentResourceManager resources = new ComponentResourceManager(typeof(ServiceTOTPViewControl));
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = resources.GetString("SECVALIDSAVE"),
                    Buttons = MessageBoxButtons.YesNo,
                    Icon = MessageBoxIcon.Question
                };
                if (this.SnapIn.Console.ShowDialog(messageBoxParameters) == DialogResult.Yes)
                   RefreshData();
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                {
                    Text = ex.Message,
                    Buttons = MessageBoxButtons.OK,
                    Icon = MessageBoxIcon.Error
                };
                this.SnapIn.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SaveData method implementation
        /// </summary>
        internal void SaveData()
        {
            if (this.ValidateChildren())
            {
                this.Cursor = Cursors.WaitCursor;
                _notifenabled = false;
                try
                {
                    bool result = true;
                    if ((ManagementService.Config.AdministrationPinEnabled) && (!ManagementService.PinValidated))
                    {
                        AdminPinWizard Wizard = new AdminPinWizard();
                        Wizard.AdminPin = ManagementService.ADFSManager.Config.AdministrationPin;
                        DialogResult dresult = this.SnapIn.Console.ShowDialog(Wizard);
                        if (dresult == DialogResult.Abort)
                        {
                            this.Cursor = Cursors.Default;
                            MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                            {
                                Text = Wizard.ErrorMessage,
                                Buttons = MessageBoxButtons.OK,
                                Icon = MessageBoxIcon.Error
                            };
                            this.SnapIn.Console.ShowDialog(messageBoxParameters);
                        }
                        result = (dresult == DialogResult.OK);
                        ManagementService.PinValidated = result;
                    }
                    if (result)
                        ManagementService.ADFSManager.WriteConfiguration(null);
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Default;
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters
                    {
                        Text = ex.Message,
                        Buttons = MessageBoxButtons.OK,
                        Icon = MessageBoxIcon.Error
                    };
                    this.SnapIn.Console.ShowDialog(messageBoxParameters);
                }
                finally
                {
                    _notifenabled = true;
                    this.Cursor = Cursors.Default;
                }
            }
        }

        /// <summary>
        /// IsNotifsEnabled method implementation
        /// </summary>
        public bool IsNotifsEnabled()
        {
            return _notifenabled;
        }

        /// <summary>
        /// RefreshProviderInformation method implementation
        /// </summary>
        public void RefreshProviderInformation()
        {
            this.ScopeNode.RefreshDescription();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(ServiceSecurityRNGViewControl));
            this.ProviderTitle.Text = string.Format(resources.GetString("ProviderTitle.Text"), this.ScopeNode.DisplayName);
        }
    }
}
