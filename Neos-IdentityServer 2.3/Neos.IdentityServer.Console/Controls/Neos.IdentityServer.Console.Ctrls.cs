//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Administration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ManagementConsole;
using Microsoft.ManagementConsole.Advanced;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using res = Neos.IdentityServer.Console.Resources.Neos_IdentityServer_Console_Ctrls;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using Neos.IdentityServer.Console.Resources;
using System.Text.RegularExpressions;


namespace Neos.IdentityServer.Console.Controls
{

    public partial class ADFSServerControl : Panel, IMMCRefreshData
    {
        private Panel _panel;
        private Panel _txtpanel;
        private LinkLabel tblstartstop;
        private ADFSServerHost _host;
        private Label lblFQDN;
        private Label lblBehavior;
        private Label lblNodetype;
        private Label lblOsversion;
        private Label lblcurrentversion;
        private Label lblBuild;
        private LinkLabel tblRestart;
        private LinkLabel tblrestartfarm;
        private ServiceViewControl _view;

        /// <summary>
        /// ADFSServerControl Constructor
        /// </summary>
        public ADFSServerControl(ServiceViewControl view, ADFSServerHost server, bool isrunning = true)
        {
            _host = server;
            _view = view;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }


        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ServiceStatusChanged += ServersStatusChanged;
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        private void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();

        }

        /// <summary>
        /// ServersStatusChanged method implementation
        /// </summary>
        private void ServersStatusChanged(ADFSServiceManager mgr, ServiceOperationStatus status, string servername, Exception Ex = null)
        {
            if ((servername.ToLower() == _host.FQDN.ToLower()) || (servername.ToLower() == _host.MachineName.ToLower()))
            {
                UpdateLayoutServerStatus(status);
                if (_view.IsNotifsEnabled())
                   DoRefreshData();
                else
                   _panel.Refresh();
            }
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implmentation
        /// </summary>
        private void UpdateLayoutServerStatus(ServiceOperationStatus status)
        {
            switch (status)
            {
                case ServiceOperationStatus.OperationInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ServiceOperationStatus.OperationPending:
                    _panel.BackColor = Color.Orange;
                    break;
                case ServiceOperationStatus.OperationRunning:
                    _panel.BackColor = Color.Green;
                    break;
                case ServiceOperationStatus.OperationStopped:
                    _panel.BackColor = Color.Red;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            UpdateLabels(status);
            this._panel.Refresh();
        }

                /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
        }


        /// <summary>
        /// DoCreateControls method implementation
        /// </summary>
        /// <param name="isrunning"></param>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 125;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 75;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 75;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                // first col
                lblFQDN = new Label();
                lblFQDN.Text = _host.FQDN;
                lblFQDN.Left = 10;
                lblFQDN.Top = 10;
                lblFQDN.Width = 200;
                _txtpanel.Controls.Add(lblFQDN);

                lblBehavior = new Label();
                lblBehavior.Text = "Behavior Level : "+_host.BehaviorLevel.ToString();
                lblBehavior.Left = 10;
                lblBehavior.Top = 32;
                lblBehavior.Width = 200;
                _txtpanel.Controls.Add(lblBehavior);

                lblNodetype = new Label();
                lblNodetype.Text = "Node Type : " + _host.NodeType;
                lblNodetype.Left = 10;
                lblNodetype.Top = 54;
                lblNodetype.Width = 200;
                _txtpanel.Controls.Add(lblNodetype);

                // Second col
                lblOsversion = new Label();
                if (_host.CurrentMajorVersionNumber!=0)
                    lblOsversion.Text = _host.ProductName + " ("+_host.CurrentMajorVersionNumber.ToString()+"."+_host.CurrentMinorVersionNumber.ToString()+")";
                else
                    lblOsversion.Text = _host.ProductName;
                lblOsversion.Left = 210;
                lblOsversion.Top = 10;
                lblOsversion.Width = 300;
                _txtpanel.Controls.Add(lblOsversion);

                // Second col
                lblcurrentversion = new Label();
                lblcurrentversion.Text = "Version : "+_host.CurrentVersion;
                lblcurrentversion.Left = 210;
                lblcurrentversion.Top = 32;
                lblcurrentversion.Width = 300;
                _txtpanel.Controls.Add(lblcurrentversion);

                lblBuild = new Label();
                lblBuild.Text = "Build : " + _host.CurrentBuild.ToString();
                lblBuild.Left = 210;
                lblBuild.Top = 54;
                lblBuild.Width = 300;
                _txtpanel.Controls.Add(lblBuild);

                tblRestart = new LinkLabel();
                tblRestart.Text = res.CRTLADFSRESTARTSERVICES;
                tblRestart.Left = 20;
                tblRestart.Top = 80;
                tblRestart.Width = 200;
                tblRestart.LinkClicked += tblRestartLinkClicked;
                tblRestart.TabIndex = 0;
                tblRestart.TabStop = true;
                this.Controls.Add(tblRestart);

                tblstartstop = new LinkLabel();
                if (ManagementService.ADFSManager.ServicesStatus == ServiceOperationStatus.OperationRunning)
                    tblstartstop.Text = res.CRTLADFSSTOPSERVICES;
                else
                    tblstartstop.Text = res.CRTLADFSSTARTSERVICES;
                tblstartstop.Left = 230;
                tblstartstop.Top = 80;
                tblstartstop.Width = 200;
                tblstartstop.LinkClicked += tblstartstopLinkClicked;
                tblRestart.TabIndex = 1;
                tblRestart.TabStop = true;
                this.Controls.Add(tblstartstop);

                tblrestartfarm = new LinkLabel();
                tblrestartfarm.Text = res.CRTLADFSRESTARTFARMSERVICES;
                tblrestartfarm.Left = 20;
                tblrestartfarm.Top = 105;
                tblrestartfarm.Width = 400;
                tblrestartfarm.LinkClicked += tblrestartfarmLinkClicked;
                tblrestartfarm.TabStop = true;
                this.Controls.Add(tblrestartfarm);
            }
            finally
            {
                UpdateLayoutServerStatus(ManagementService.ADFSManager.ServicesStatus);
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }

        }

        /// <summary>
        /// DoRefreshData method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {

                lblFQDN.Text = _host.FQDN;
                lblBehavior.Text = "Behavior Level : " + _host.BehaviorLevel.ToString();
                lblNodetype.Text = "Node Type : " + _host.NodeType;
                if (_host.CurrentMajorVersionNumber != 0)
                    lblOsversion.Text = _host.ProductName + " (" + _host.CurrentMajorVersionNumber.ToString() + "." + _host.CurrentMinorVersionNumber.ToString() + ")";
                else
                    lblOsversion.Text = _host.ProductName;
                lblcurrentversion.Text = "Version : " + _host.CurrentVersion;
                lblBuild.Text = "Build : " + _host.CurrentBuild.ToString();
                tblRestart.Text = res.CRTLADFSRESTARTSERVICES;
                if (ManagementService.ADFSManager.ServicesStatus == ServiceOperationStatus.OperationRunning)
                    tblstartstop.Text = res.CRTLADFSSTOPSERVICES;
                else
                    tblstartstop.Text = res.CRTLADFSSTARTSERVICES;
                tblrestartfarm.Text = res.CRTLADFSRESTARTFARMSERVICES;
                UpdateLayoutServerStatus(ManagementService.ADFSManager.ServicesStatus);
            }
            finally
            {
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// UpdateLabels method implmentation
        /// </summary>
        private void UpdateLabels(ServiceOperationStatus status)
        {
            if (ManagementService.ADFSManager.ServicesStatus == ServiceOperationStatus.OperationRunning)
            {
                tblstartstop.Text = res.CRTLADFSSTOPSERVICES;
                tblstartstop.Tag = true;
            }
            else
            {
                tblstartstop.Text = res.CRTLADFSSTARTSERVICES;
                tblstartstop.Tag = false;
            }
        }

        /// <summary>
        /// tblstartstopLinkClicked event implmentation
        /// </summary>
        private void tblstartstopLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (ManagementService.ADFSManager.ServicesStatus == ServiceOperationStatus.OperationRunning)
            {
                ManagementService.ADFSManager.StopService(_host.FQDN);
                (sender as LinkLabel).Text = res.CRTLADFSRESTARTSERVICES;
            }
            else
            {
                ManagementService.ADFSManager.StartService(_host.FQDN);
                (sender as LinkLabel).Text = res.CRTLADFSSTOPSERVICES;
            }
        }

        /// <summary>
        /// tblRestartLinkClicked event implementation
        /// </summary>
        private void tblRestartLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ManagementService.ADFSManager.RestartServer(null, _host.FQDN);
        }

        /// <summary>
        /// tblrestartfarmLinkClicked ebvent implmentation
        /// </summary>
        private void tblrestartfarmLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ManagementService.ADFSManager.RestartFarm(null);
        }   
    }

    public partial class MFAProvidersControl : Panel, IMMCRefreshData
    {
        private Panel _panel;
        private Panel _txtpanel;
        private NamespaceSnapInBase _snapin;
        private ProvidersViewControl _view;
        private PreferredMethod _kind;
        private IExternalProvider _provider;
        private ErrorProvider errors;
        private CheckBox chkProviderEnabled;
        private CheckBox chkProviderEnroll;
        private CheckBox chkProviderPin;
        private Label lblProviderDesc;

        /// <summary>
        /// ADFSServerControl Constructor
        /// </summary>
        public MFAProvidersControl(ProvidersViewControl view, NamespaceSnapInBase snap, IExternalProvider provider)
        {
            _view = view;
            _snapin = snap;
            _provider = provider;
            _kind = provider.Kind;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        private void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception Ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implmentation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    switch (_kind)
                    {
                        case PreferredMethod.Code:
                            if (!Config.OTPProvider.Enabled)
                                _panel.BackColor = Color.DarkGray;
                            break;
                        case PreferredMethod.Email:
                            if (!Config.MailProvider.Enabled)
                                _panel.BackColor = Color.DarkGray;
                            break;
                        case PreferredMethod.External:
                            if (!Config.ExternalProvider.Enabled)
                                _panel.BackColor = Color.DarkGray;
                            break;
                        case PreferredMethod.Azure:
                            if (!Config.AzureProvider.Enabled)
                                _panel.BackColor = Color.DarkGray;
                            break;
                    }
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.Red;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            this._panel.Refresh();
        }

        /// <summary>
        /// DoCreateControls method implementation
        /// </summary>
        /// <param name="isrunning"></param>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 75;
                this.Width = 760;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 75;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 100;
                _txtpanel.Height = 95;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                // first col
                lblProviderDesc = new Label();

                lblProviderDesc.Text = _provider.Description;
                lblProviderDesc.Left = 10;
                lblProviderDesc.Top = 10;
                lblProviderDesc.Width = 500;
                lblProviderDesc.Font = new System.Drawing.Font(lblProviderDesc.Font.Name, 16F, FontStyle.Bold);
                _txtpanel.Controls.Add(lblProviderDesc);

                // Second col
                chkProviderEnabled = new CheckBox();
                chkProviderEnabled.Text = res.CTRLPROVACTIVE;
                switch (_kind)
                {
                    case PreferredMethod.Code:
                        chkProviderEnabled.Checked = Config.OTPProvider.Enabled;
                        break;
                    case PreferredMethod.Email:
                        chkProviderEnabled.Checked = Config.MailProvider.Enabled;
                        break;
                    case PreferredMethod.External:
                        chkProviderEnabled.Checked = Config.ExternalProvider.Enabled;
                        break;
                    case PreferredMethod.Azure:
                        chkProviderEnabled.Checked = Config.AzureProvider.Enabled;
                        break;
                }
                chkProviderEnabled.Enabled = _provider.AllowDisable;
                chkProviderEnabled.Left = 510;
                chkProviderEnabled.Top = 10;
                chkProviderEnabled.Width = 300;
                chkProviderEnabled.CheckedChanged += chkProviderChanged;
                _txtpanel.Controls.Add(chkProviderEnabled);

                chkProviderEnroll = new CheckBox();
                chkProviderEnroll.Text = res.CTRLPROVWIZARD;
                switch (_kind)
                {
                    case PreferredMethod.Code:
                        chkProviderEnroll.Checked = Config.OTPProvider.EnrollWizard;
                        break;
                    case PreferredMethod.Email:
                        chkProviderEnroll.Checked = Config.MailProvider.EnrollWizard;
                        break;
                    case PreferredMethod.External:
                        chkProviderEnroll.Checked = Config.ExternalProvider.EnrollWizard;
                        break;
                    case PreferredMethod.Azure:
                        chkProviderEnroll.Checked = false;
                        chkProviderEnroll.Enabled = false;
                        break;
                }
                chkProviderEnroll.Enabled = chkProviderEnabled.Checked;
                chkProviderEnroll.Left = 510;
                chkProviderEnroll.Top = 30;
                chkProviderEnroll.Width = 300;
                chkProviderEnroll.CheckedChanged += chkProviderEnrollChanged;
                _txtpanel.Controls.Add(chkProviderEnroll);

                chkProviderPin = new CheckBox();
                chkProviderPin.Text = res.CTRLPROVPIN;
                switch (_kind)
                {
                    case PreferredMethod.Code:
                        chkProviderPin.Checked = Config.OTPProvider.PinRequired;
                        break;
                    case PreferredMethod.Email:
                        chkProviderPin.Checked = Config.MailProvider.PinRequired;
                        break;
                    case PreferredMethod.External:
                        chkProviderPin.Checked = Config.ExternalProvider.PinRequired;
                        break;
                    case PreferredMethod.Azure:
                        chkProviderPin.Checked = Config.AzureProvider.PinRequired;
                        break;
                }
                chkProviderPin.Enabled = chkProviderEnabled.Checked;
                chkProviderPin.Left = 510;
                chkProviderPin.Top = 50;
                chkProviderPin.Width = 300;
                chkProviderPin.CheckedChanged += chkProviderPinChanged;
                _txtpanel.Controls.Add(chkProviderPin);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implmentation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                lblProviderDesc.Text = _provider.Description;
                chkProviderEnabled.Text = res.CTRLPROVACTIVE;
                chkProviderEnroll.Text = res.CTRLPROVWIZARD;
                chkProviderPin.Text = res.CTRLPROVPIN;

                switch (_kind)
                {
                    case PreferredMethod.Code:
                        chkProviderEnabled.Checked = Config.OTPProvider.Enabled;
                        break;
                    case PreferredMethod.Email:
                        chkProviderEnabled.Checked = Config.MailProvider.Enabled;
                        break;
                    case PreferredMethod.External:
                        chkProviderEnabled.Checked = Config.ExternalProvider.Enabled;
                        break;
                    case PreferredMethod.Azure:
                        chkProviderEnabled.Checked = Config.AzureProvider.Enabled;
                        break;
                }
                chkProviderEnabled.Enabled = _provider.AllowDisable;

                switch (_kind)
                {
                    case PreferredMethod.Code:
                        chkProviderEnroll.Checked = Config.OTPProvider.EnrollWizard;
                        break;
                    case PreferredMethod.Email:
                        chkProviderEnroll.Checked = Config.MailProvider.EnrollWizard;
                        break;
                    case PreferredMethod.External:
                        chkProviderEnroll.Checked = Config.ExternalProvider.EnrollWizard;
                        break;
                    case PreferredMethod.Azure:
                        chkProviderEnroll.Checked = false;
                        chkProviderEnroll.Enabled = false;
                        break;
                }
                chkProviderEnroll.Enabled = chkProviderEnabled.Checked;

                switch (_kind)
                {
                    case PreferredMethod.Code:
                        chkProviderPin.Checked = Config.OTPProvider.PinRequired;
                        break;
                    case PreferredMethod.Email:
                        chkProviderPin.Checked = Config.MailProvider.PinRequired;
                        break;
                    case PreferredMethod.External:
                        chkProviderPin.Checked = Config.ExternalProvider.PinRequired;
                        break;
                    case PreferredMethod.Azure:
                        chkProviderPin.Checked = Config.AzureProvider.PinRequired;
                        break;
                }
                chkProviderPin.Enabled = chkProviderEnabled.Checked;
            }
            finally
            {
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// chkProviderPinChanged method implementation
        /// </summary>
        private void chkProviderPinChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                switch (_kind)
                {
                    case PreferredMethod.Code:
                        Config.OTPProvider.PinRequired = chkProviderPin.Checked;
                        break;
                    case PreferredMethod.Email:
                        Config.MailProvider.PinRequired = chkProviderPin.Checked;
                        break;
                    case PreferredMethod.External:
                        Config.ExternalProvider.PinRequired = chkProviderPin.Checked;
                        break;
                    case PreferredMethod.Azure:
                        Config.AzureProvider.PinRequired = chkProviderPin.Checked;
                        break;
                }
                ManagementService.ADFSManager.SetDirty(true);
            }
            catch (Exception ex)
            {
                errors.SetError(chkProviderPin, ex.Message);
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// chkProviderEnrollChanged method implementation
        /// </summary>
        private void chkProviderEnrollChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                switch (_kind)
                {
                    case PreferredMethod.Code:
                        Config.OTPProvider.EnrollWizard = chkProviderEnroll.Checked;
                        break;
                    case PreferredMethod.Email:
                        Config.MailProvider.EnrollWizard = chkProviderEnroll.Checked;
                        break;
                    case PreferredMethod.External:
                        Config.ExternalProvider.EnrollWizard = chkProviderEnroll.Checked;
                        break;
                    case PreferredMethod.Azure:
                        Config.AzureProvider.EnrollWizard = false;
                        break;
                }
                ManagementService.ADFSManager.SetDirty(true);
            }
            catch (Exception ex)
            {
                errors.SetError(chkProviderEnroll, ex.Message);
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// chkProviderChanged method implementation
        /// </summary>
        private void chkProviderChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                switch (_kind)
                {
                    case PreferredMethod.Code:
                        Config.OTPProvider.Enabled = chkProviderEnabled.Checked;
                        break;
                    case PreferredMethod.Email:
                        Config.MailProvider.Enabled = chkProviderEnabled.Checked;
                        break;
                    case PreferredMethod.External:
                        Config.ExternalProvider.Enabled = chkProviderEnabled.Checked;
                        break;
                    case PreferredMethod.Azure:
                        Config.AzureProvider.Enabled = chkProviderEnabled.Checked;
                        break;
                }
                ManagementService.ADFSManager.SetDirty(true);
            }
            catch (Exception ex)
            {
                errors.SetError(chkProviderEnabled, ex.Message);
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }

    public partial class MFAProvidersValidationControl : Panel, IMMCRefreshData
    {
        private ProvidersViewControl _view;
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;

        /// <summary>
        /// MFAProvidersValidationControl Constructor
        /// </summary>
        public MFAProvidersValidationControl(ProvidersViewControl view, NamespaceSnapInBase snap)
        {
            _panel = new Panel();
            _txtpanel = new Panel();
            _view = view;
            _snapin = snap;
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            DoCreateControls();
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// DoCreateControls method implementation
        /// </summary>
        /// <param name="isrunning"></param>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 40;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 30;

                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 20;
                _txtpanel.BackColor = System.Drawing.SystemColors.Window;
                this.Controls.Add(_txtpanel);


                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 0;
                tblSaveConfig.Top = 0;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                _txtpanel.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 90;
                tblCancelConfig.Top = 0;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                _txtpanel.Controls.Add(tblCancelConfig);

            }
            finally
            {
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshtta method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
            }
            finally
            {
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// SaveConfigLinkClicked method implementation
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }

        /// <summary>
        /// SaveConfigLinkClicked method implementation
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }
    }

    public partial class ConfigurationControl : Panel, IMMCRefreshData
    {
        private ServiceViewControl _view;
        private Panel _panel;
        private Panel _txtpanel;
        private Label lblFarmActive;
        private Label lblstorageMode;
        private Label lbladmincontact;
        private Label lblIdentifier;
        private Label lblIsInitialized;
        private Label lblFarmBehavior;
        private Label lbloptions;
        private Label lblSecurity;
        private Label lblcutompwd; 

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public ConfigurationControl(ServiceViewControl view)
        {
            _panel = new Panel();
            _txtpanel = new Panel();
            _view = view;
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            UpdateLabels(status);
            return;
        }

        /// <summary>
        /// DoCreateControls method implementation
        /// </summary>
        /// <param name="isrunning"></param>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 95;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 75;

                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 75;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                // first col
                lblIsInitialized = new Label();
                lblIsInitialized.Text = "Initialized : " + Config.Hosts.ADFSFarm.IsInitialized.ToString();
                lblIsInitialized.Left = 10;
                lblIsInitialized.Top = 10;
                lblIsInitialized.Width = 200;
                _txtpanel.Controls.Add(lblIsInitialized);

                lblFarmActive = new Label();
                lblFarmActive.Text = "Active : " + (ManagementService.ADFSManager.ConfigurationStatus != ConfigOperationStatus.ConfigStopped).ToString();
                lblFarmActive.Left = 10;
                lblFarmActive.Top = 32;
                lblFarmActive.Width = 200;
                _txtpanel.Controls.Add(lblFarmActive);

                lblIdentifier = new Label();
                lblIdentifier.Text = "Identifier : " + Config.Hosts.ADFSFarm.FarmIdentifier;
                lblIdentifier.Left = 10;
                lblIdentifier.Top = 54;
                lblIdentifier.Width = 200;
                _txtpanel.Controls.Add(lblIdentifier);

                // Second col
                lbladmincontact = new Label();
                lbladmincontact.Text = "Administrative contact : " + Config.AdminContact;
                lbladmincontact.Left = 230;
                lbladmincontact.Top = 10;
                lbladmincontact.Width = 300;
                _txtpanel.Controls.Add(lbladmincontact);

                lblstorageMode = new Label();
                if (Config.UseActiveDirectory)
                    lblstorageMode.Text = "Mode : Active Directory";
                else
                    lblstorageMode.Text = "Mode : Sql Server Database";
                lblstorageMode.Left = 230;
                lblstorageMode.Top = 32;
                lblstorageMode.Width = 300;
                _txtpanel.Controls.Add(lblstorageMode);

                lblFarmBehavior = new Label();
                lblFarmBehavior.Text = "Behavior : " + Config.Hosts.ADFSFarm.CurrentFarmBehavior.ToString();
                lblFarmBehavior.Left = 230;
                lblFarmBehavior.Top = 54;
                lblFarmBehavior.Width = 300;
                _txtpanel.Controls.Add(lblFarmBehavior);

                // third col
                lbloptions = new Label();
                lbloptions.Text = "Options : ";
                if (Config.OTPProvider.Enabled)
                    lbloptions.Text = "TOPT ";
                if (Config.MailProvider.Enabled)
                    lbloptions.Text = "EMAILS ";
                if (Config.ExternalProvider.Enabled)
                    lbloptions.Text = "SMS ";
                if (Config.AzureProvider.Enabled)
                    lbloptions.Text = "AZURE ";

                lbloptions.Left = 550;
                lbloptions.Top = 10;
                lbloptions.Width = 300;
                _txtpanel.Controls.Add(lbloptions);

                lblSecurity = new Label();
                switch (Config.KeysConfig.KeyFormat)
                {
                    case SecretKeyFormat.RSA:
                        lblSecurity.Text = "Security : RSA   " + Config.KeysConfig.CertificateThumbprint;
                        break;
                    case SecretKeyFormat.CUSTOM:
                        lblSecurity.Text = "Security : RSA CUSTOM";
                        break;
                    default:
                        lblSecurity.Text = "Security : RNG";
                        break;
                }
                lblSecurity.Left = 550;
                lblSecurity.Top = 32;
                lblSecurity.Width = 300;
                _txtpanel.Controls.Add(lblSecurity);

                lblcutompwd = new Label();
                lblcutompwd.Left = 550;
                lblcutompwd.Top = 54;
                lblcutompwd.Width = 300;
                _txtpanel.Controls.Add(lblcutompwd);

                if (Config.CustomUpdatePassword)
                    lblcutompwd.Text = "Use custom change password feature";
                else
                    lblcutompwd.Text = "";

                if (ManagementService.ADFSManager.IsRunning())
                {
                    LinkLabel tblconfigure = new LinkLabel();
                    if (ManagementService.ADFSManager.ConfigurationStatus != ConfigOperationStatus.ConfigStopped)
                        tblconfigure.Text = res.CTRLADFSDEACTIVATEMFA;
                    else
                        tblconfigure.Text = res.CTRLADFSACTIVATEMFA;
                    tblconfigure.Left = 20;
                    tblconfigure.Top = 80;
                    tblconfigure.Width = 400;
                    tblconfigure.LinkClicked += tblconfigureLinkClicked;
                    tblconfigure.TabIndex = 0;
                    tblconfigure.TabStop = true;
                    this.Controls.Add(tblconfigure);
                }
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData()
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                lblIsInitialized.Text = "Initialized : " + Config.Hosts.ADFSFarm.IsInitialized.ToString();
                lblFarmActive.Text = "Active : " + (ManagementService.ADFSManager.ConfigurationStatus != ConfigOperationStatus.ConfigStopped).ToString();
                lblIdentifier.Text = "Identifier : " + Config.Hosts.ADFSFarm.FarmIdentifier;
                lbladmincontact.Text = "Administrative contact : " + Config.AdminContact;
                if (Config.UseActiveDirectory)
                    lblstorageMode.Text = "Mode : Active Directory";
                else
                    lblstorageMode.Text = "Mode : Sql Server Database";

                lblFarmBehavior.Text = "Behavior : " + Config.Hosts.ADFSFarm.CurrentFarmBehavior.ToString();

                // third col
                lbloptions.Text = "Options : ";
                if (Config.OTPProvider.Enabled)
                    lbloptions.Text += "TOPT ";
                if (Config.MailProvider.Enabled)
                    lbloptions.Text += "EMAILS ";
                if (Config.ExternalProvider.Enabled)
                    lbloptions.Text += "SMS ";
                if (Config.AzureProvider.Enabled)
                    lbloptions.Text += "AZURE ";

                switch (Config.KeysConfig.KeyFormat)
                {
                    case SecretKeyFormat.RSA:
                        lblSecurity.Text = "Security : RSA   " + Config.KeysConfig.CertificateThumbprint;
                        break;
                    case SecretKeyFormat.CUSTOM:
                        lblSecurity.Text = "Security : RSA CUSTOM";
                        break;
                    default:
                        lblSecurity.Text = "Security : RNG";
                        break;
                }
                if (Config.CustomUpdatePassword)
                    lblcutompwd.Text = "Use custom change password feature";
                else
                    lblcutompwd.Text = "";
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// tblconfigureLinkClicked method implmentation
        /// </summary>
        private void tblconfigureLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (ManagementService.ADFSManager.ConfigurationStatus != ConfigOperationStatus.ConfigStopped)
            {
                ManagementService.ADFSManager.DisableMFAProvider(null);
                (sender as LinkLabel).Text = res.CTRLADFSACTIVATEMFA;
            }
            else
            {
                ManagementService.ADFSManager.EnableMFAProvider(null);
                (sender as LinkLabel).Text = res.CTRLADFSDEACTIVATEMFA;
            }
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// UpdateLabels method implmentation
        /// </summary>
        private void UpdateLabels(ConfigOperationStatus status)
        {
            lblFarmActive.Text = "Active : " + (ManagementService.ADFSManager.ConfigurationStatus != ConfigOperationStatus.ConfigStopped).ToString();
        }
    }

    public partial class GeneralConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;

        // Controls
        private TextBox txtIssuer;
        private TextBox txtAdminContact;
        private TextBox txtCountryCode;
        private TextBox txtDeliveryWindow;
        private TextBox txtMaxRetries;
        private ComboBox cbConfigTemplate;

        private RadioButton rdioMFARequired;
        private RadioButton rdioMFAAllowed;
        private RadioButton rdioMFANotRequired;
        private RadioButton rdioREGAdmin;
        private RadioButton rdioREGUser;
        private RadioButton rdioREGUnManaged;
        private CheckBox chkAllowManageOptions;
        private CheckBox chkAllowChangePassword;
        private CheckBox chkAllowEnrollment;
        private CheckBox chkAllowKMSOO;
        private NumericUpDown txtADVStart;
        private NumericUpDown txtADVEnd;

        private Panel _paneloptmfa;
        private Panel _panelstmfa;
        private Panel _panelregmfa;
        private Panel _paneladvmfa;
        private GeneralViewControl _view;

        private ErrorProvider errors;
        private Label lblIssuer;
        private Label lblAdminContact;
        private Label lblContryCode;
        private Label lblDeliveryWindow;
        private Label lblMaxRetries;
        private Label lblConfigTemplate;
        private Label rdioMFALabel;
        private Label rdioREGLabel;
        private Label optCFGLabel;
        private Label optADVLabel;
        private Label beginADVLabel;
        private Label endADVLabel;
        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;


        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public GeneralConfigurationControl(GeneralViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 545;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 425;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 425;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                // first col
                lblIssuer = new Label();
                lblIssuer.Text = res.CTRLGLCOMANYNAME + " : ";
                lblIssuer.Left = 10;
                lblIssuer.Top = 19;
                lblIssuer.Width = 200;
                _txtpanel.Controls.Add(lblIssuer);

                txtIssuer = new TextBox();
                txtIssuer.Text = Config.Issuer;
                txtIssuer.Left = 210;
                txtIssuer.Top = 15;
                txtIssuer.Width = 250;
                txtIssuer.Validating += IssuerValidating;
                txtIssuer.Validated += IssuerValidated;
                _txtpanel.Controls.Add(txtIssuer);

                lblAdminContact = new Label();
                lblAdminContact.Text = res.CTRLGLCONTACT + " : ";
                lblAdminContact.Left = 10;
                lblAdminContact.Top = 51;
                lblAdminContact.Width = 200;
                _txtpanel.Controls.Add(lblAdminContact);

                txtAdminContact = new TextBox();
                txtAdminContact.Text = Config.AdminContact;
                txtAdminContact.Left = 210;
                txtAdminContact.Top = 47;
                txtAdminContact.Width = 250;
                txtAdminContact.Validating += AdminContactValidating;
                txtAdminContact.Validated += AdminContactValidated;
                _txtpanel.Controls.Add(txtAdminContact);

                lblContryCode = new Label();
                lblContryCode.Text = res.CTRLGLCONTRYCODE + " : ";
                lblContryCode.Left = 10;
                lblContryCode.Top = 83;
                lblContryCode.Width = 130;
                _txtpanel.Controls.Add(lblContryCode);

                txtCountryCode = new TextBox();
                txtCountryCode.Text = Config.DefaultCountryCode;
                txtCountryCode.Left = 210;
                txtCountryCode.Top = 79;
                txtCountryCode.Width = 20;
                txtCountryCode.TextAlign = HorizontalAlignment.Center;
                txtCountryCode.MaxLength = 2;
                txtCountryCode.CharacterCasing = CharacterCasing.Lower;
                txtCountryCode.Validating += CountryCodeValidating;
                txtCountryCode.Validated += CountryCodeValidated;
                _txtpanel.Controls.Add(txtCountryCode);

                lblDeliveryWindow = new Label();
                lblDeliveryWindow.Text = res.CTRLGLDELVERY + " : ";
                lblDeliveryWindow.Left = 10;
                lblDeliveryWindow.Top = 115;
                lblDeliveryWindow.Width = 200;
                _txtpanel.Controls.Add(lblDeliveryWindow);

                txtDeliveryWindow = new TextBox();
                txtDeliveryWindow.Text = Config.DeliveryWindow.ToString();
                txtDeliveryWindow.Left = 210;
                txtDeliveryWindow.Top = 111;
                txtDeliveryWindow.Width = 60;
                txtDeliveryWindow.MaxLength = 4;
                txtDeliveryWindow.TextAlign = HorizontalAlignment.Center;
                txtDeliveryWindow.Validating += DeliveryWindowValidating;
                txtDeliveryWindow.Validated += DeliveryWindowValidated;
                _txtpanel.Controls.Add(txtDeliveryWindow);

                lblMaxRetries = new Label();
                lblMaxRetries.Text = res.CTRLDLGMAXRETRIES + " : ";
                lblMaxRetries.Left = 530;
                lblMaxRetries.Top = 115;
                lblMaxRetries.Width = 150;
                _txtpanel.Controls.Add(lblMaxRetries);

                txtMaxRetries = new TextBox();
                txtMaxRetries.Text = Config.MaxRetries.ToString();
                txtMaxRetries.Left = 690;
                txtMaxRetries.Top = 111;
                txtMaxRetries.Width = 40;
                txtMaxRetries.MaxLength = 2;
                txtMaxRetries.Validating += MaxRetriesValidating;
                txtMaxRetries.Validated += MaxRetriesValidated;
                txtMaxRetries.TextAlign = HorizontalAlignment.Center;
                _txtpanel.Controls.Add(txtMaxRetries);

                lblConfigTemplate = new Label();
                lblConfigTemplate.Text = res.CTRLGLPOLICY + " : ";
                lblConfigTemplate.Left = 10;
                lblConfigTemplate.Top = 168;
                lblConfigTemplate.Width = 180;
                _txtpanel.Controls.Add(lblConfigTemplate);

                MMCTemplateModeList lst = new MMCTemplateModeList();
                cbConfigTemplate = new ComboBox();
                cbConfigTemplate.DropDownStyle = ComboBoxStyle.DropDownList;
                cbConfigTemplate.Left = 210;
                cbConfigTemplate.Top = 164;
                cbConfigTemplate.Width = 250;
                _txtpanel.Controls.Add(cbConfigTemplate);

                cbConfigTemplate.DataSource = lst;
                cbConfigTemplate.ValueMember = "ID";
                cbConfigTemplate.DisplayMember = "Label";
                cbConfigTemplate.SelectedIndexChanged += SelectedPolicyTemplateChanged;

                _panelstmfa = new Panel();
                _panelstmfa.Left = 0;
                _panelstmfa.Top = 198;
                _panelstmfa.Height = 100;
                _panelstmfa.Width = 300;
                _txtpanel.Controls.Add(_panelstmfa);

                rdioMFALabel = new Label();
                rdioMFALabel.Text = res.CTRLGLMFASTATUS;
                rdioMFALabel.Left = 10;
                rdioMFALabel.Top = 10;
                rdioMFALabel.Width = 180;
                _panelstmfa.Controls.Add(rdioMFALabel);

                rdioMFARequired = new RadioButton();
                rdioMFARequired.Text = res.CTRLGLMFASTATUS1;
                rdioMFARequired.Left = 30;
                rdioMFARequired.Top = 29;
                rdioMFARequired.Width = 300;
                rdioMFARequired.CheckedChanged += MFARequiredCheckedChanged;
                _panelstmfa.Controls.Add(rdioMFARequired);

                rdioMFAAllowed = new RadioButton();
                rdioMFAAllowed.Text = res.CTRLGLMFASTATUS2;
                rdioMFAAllowed.Left = 30;
                rdioMFAAllowed.Top = 54;
                rdioMFAAllowed.Width = 300;
                rdioMFAAllowed.CheckedChanged += MFAAllowedCheckedChanged;
                _panelstmfa.Controls.Add(rdioMFAAllowed);

                rdioMFANotRequired = new RadioButton();
                rdioMFANotRequired.Text = res.CTRLGLMFASTATUS3;
                rdioMFANotRequired.Left = 30;
                rdioMFANotRequired.Top = 79;
                rdioMFANotRequired.Width = 300;
                rdioMFANotRequired.CheckedChanged += MFANotRequiredCheckedChanged;
                _panelstmfa.Controls.Add(rdioMFANotRequired);

                _panelregmfa = new Panel();
                _panelregmfa.Left = 0;
                _panelregmfa.Top = 300;
                _panelregmfa.Height = 115;
                _panelregmfa.Width = 300;
                _txtpanel.Controls.Add(_panelregmfa);

                rdioREGLabel = new Label();
                rdioREGLabel.Text = res.CTRLGLMFAREGISTER;
                rdioREGLabel.Left = 10;
                rdioREGLabel.Top = 10;
                rdioREGLabel.Width = 180;
                _panelregmfa.Controls.Add(rdioREGLabel);

                rdioREGAdmin = new RadioButton();
                rdioREGAdmin.Text = res.CTRLGLMFAREGISTER1;
                rdioREGAdmin.Left = 30;
                rdioREGAdmin.Top = 29;
                rdioREGAdmin.Width = 300;
                rdioREGAdmin.CheckedChanged += REGAdminCheckedChanged;
                _panelregmfa.Controls.Add(rdioREGAdmin);

                rdioREGUser = new RadioButton();
                rdioREGUser.Text = res.CTRLGLMFAREGISTER2;
                rdioREGUser.Left = 30;
                rdioREGUser.Top = 54;
                rdioREGUser.Width = 300;
                rdioREGUser.CheckedChanged += REGUserCheckedChanged;
                _panelregmfa.Controls.Add(rdioREGUser);

                rdioREGUnManaged = new RadioButton();
                rdioREGUnManaged.Text = res.CTRLGLMFAREGISTER3;
                rdioREGUnManaged.Left = 30;
                rdioREGUnManaged.Top = 79;
                rdioREGUnManaged.Width = 300;
                rdioREGUnManaged.CheckedChanged += REGUnManagedCheckedChanged;
                _panelregmfa.Controls.Add(rdioREGUnManaged);

                _paneloptmfa = new Panel();
                _paneloptmfa.Left = 530;
                _paneloptmfa.Top = 198;
                _paneloptmfa.Height = 125;
                _paneloptmfa.Width = 400;
                _txtpanel.Controls.Add(_paneloptmfa);

                optCFGLabel = new Label();
                optCFGLabel.Text = res.CTRLGLMANAGEOPTS;
                optCFGLabel.Left = 0;
                optCFGLabel.Top = 10;
                optCFGLabel.Width = 180;
                _paneloptmfa.Controls.Add(optCFGLabel);

                chkAllowManageOptions = new CheckBox();
                chkAllowManageOptions.Text = res.CTRLGLMANAGEOPTIONS;  
                chkAllowManageOptions.Left = 20;
                chkAllowManageOptions.Top = 29;
                chkAllowManageOptions.Width = 300;
                chkAllowManageOptions.CheckedChanged += AllowManageOptionsCheckedChanged;
                _paneloptmfa.Controls.Add(chkAllowManageOptions);

                chkAllowEnrollment = new CheckBox();
                chkAllowEnrollment.Text = res.CTRLGLENROLLWIZ; 
                chkAllowEnrollment.Left = 20;
                chkAllowEnrollment.Top = 54;
                chkAllowEnrollment.Width = 300;
                chkAllowEnrollment.CheckedChanged += AllowEnrollmentWizardCheckedChanged;
                _paneloptmfa.Controls.Add(chkAllowEnrollment);

                chkAllowChangePassword = new CheckBox();
                chkAllowChangePassword.Text = res.CTRLGLMANAGEPWD;
                chkAllowChangePassword.Left = 20;
                chkAllowChangePassword.Top = 79;
                chkAllowChangePassword.Width = 300;
                chkAllowChangePassword.CheckedChanged += AllowChangePasswordCheckedChanged;
                _paneloptmfa.Controls.Add(chkAllowChangePassword);

                chkAllowKMSOO = new CheckBox();
                chkAllowKMSOO.Text = res.CTRLGLMANAGEKMSOO;
                chkAllowKMSOO.Left = 20;
                chkAllowKMSOO.Top = 104;
                chkAllowKMSOO.Width = 400;
                chkAllowKMSOO.CheckedChanged += AllowKMSOOCheckedChanged;
                _paneloptmfa.Controls.Add(chkAllowKMSOO);

                _paneladvmfa = new Panel();
                _paneladvmfa.Left = 530;
                _paneladvmfa.Top = 330;
                _paneladvmfa.Height = 100;
                _paneladvmfa.Width = 300;
                _txtpanel.Controls.Add(_paneladvmfa);

                optADVLabel = new Label();
                optADVLabel.Text = res.CTRGLMANAGEREG;
                optADVLabel.Left = 0;
                optADVLabel.Top = 10;
                optADVLabel.Width = 160;
                _paneladvmfa.Controls.Add(optADVLabel);

                beginADVLabel = new Label();
                beginADVLabel.Text = res.CTRGLMANAGEREGSTART + " :";
                beginADVLabel.Left = 20;
                beginADVLabel.Top = 34;
                beginADVLabel.Width = 50;
                beginADVLabel.TextAlign = ContentAlignment.MiddleRight;
                _paneladvmfa.Controls.Add(beginADVLabel);

                endADVLabel = new Label();
                endADVLabel.Text = res.CTRGLMANAGEREGEND + " :";
                endADVLabel.Left = 150;
                endADVLabel.Top = 34;
                endADVLabel.Width = 50;
                endADVLabel.TextAlign = ContentAlignment.MiddleRight;
                _paneladvmfa.Controls.Add(endADVLabel);

                txtADVStart = new NumericUpDown();
                txtADVStart.Left = 70;
                txtADVStart.Top = 34;
                txtADVStart.Width = 50;
                txtADVStart.TextAlign = HorizontalAlignment.Center;
                txtADVStart.Value = Config.AdvertisingDays.FirstDay;
                txtADVStart.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
                txtADVStart.Maximum = new decimal(new int[] { 31, 0, 0, 0 });
                txtADVStart.ValueChanged += ADVStartValueChanged;
                _paneladvmfa.Controls.Add(txtADVStart);

                txtADVEnd = new NumericUpDown();
                txtADVEnd.Left = 200;
                txtADVEnd.Top = 34;
                txtADVEnd.Width = 50;
                txtADVEnd.TextAlign = HorizontalAlignment.Center;
                txtADVEnd.Value = Config.AdvertisingDays.LastDay;
                txtADVEnd.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
                txtADVEnd.Maximum = new decimal(new int[] { 31, 0, 0, 0 });
                txtADVEnd.ValueChanged += ADVEndValueChanged;
                _paneladvmfa.Controls.Add(txtADVEnd);


                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 440;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 440;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                AdjustPolicyTemplate();
                ValidateData();
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                txtIssuer.Text = Config.Issuer;
                txtAdminContact.Text = Config.AdminContact;
                txtCountryCode.Text = Config.DefaultCountryCode;
                txtDeliveryWindow.Text = Config.DeliveryWindow.ToString();
                txtMaxRetries.Text = Config.MaxRetries.ToString();
                txtADVStart.Value = Config.AdvertisingDays.FirstDay;
                txtADVEnd.Value = Config.AdvertisingDays.LastDay;

                lblIssuer.Text = res.CTRLGLCOMANYNAME + " : ";
                lblAdminContact.Text = res.CTRLGLCONTACT + " : ";
                lblContryCode.Text = res.CTRLGLCONTRYCODE + " : ";
                lblDeliveryWindow.Text = res.CTRLGLDELVERY + " : ";
                lblMaxRetries.Text = res.CTRLDLGMAXRETRIES + " : ";
                lblConfigTemplate.Text = res.CTRLGLPOLICY + " : ";
                rdioMFALabel.Text = res.CTRLGLMFASTATUS;
                rdioMFARequired.Text = res.CTRLGLMFASTATUS1;
                rdioMFAAllowed.Text = res.CTRLGLMFASTATUS2;
                rdioMFANotRequired.Text = res.CTRLGLMFASTATUS3;
                rdioREGLabel.Text = res.CTRLGLMFAREGISTER;
                rdioREGAdmin.Text = res.CTRLGLMFAREGISTER1;
                rdioREGUser.Text = res.CTRLGLMFAREGISTER2;
                rdioREGUnManaged.Text = res.CTRLGLMFAREGISTER3;
                optCFGLabel.Text = res.CTRLGLMANAGEOPTS;
                chkAllowManageOptions.Text = res.CTRLGLMANAGEOPTIONS;
                chkAllowEnrollment.Text = res.CTRLGLENROLLWIZ;
                chkAllowChangePassword.Text = res.CTRLGLMANAGEPWD;
                chkAllowKMSOO.Text = res.CTRLGLMANAGEKMSOO;
                optADVLabel.Text = res.CTRGLMANAGEREG;
                beginADVLabel.Text = res.CTRGLMANAGEREGSTART + " :";
                endADVLabel.Text = res.CTRGLMANAGEREGEND + " :";
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                AdjustPolicyTemplate();
                ValidateData();
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ValidateData method implementation
        /// </summary>
        private void ValidateData()
        {
            int refr = Convert.ToInt32(txtDeliveryWindow.Text);
            if (string.IsNullOrEmpty(txtDeliveryWindow.Text))
                errors.SetError(txtDeliveryWindow, res.CTRLNULLOREMPTYERROR);
            else if ((refr < 60) || (refr > 600))
                errors.SetError(txtDeliveryWindow, string.Format(res.CTRLINVALIDVALUE, "60", "600"));
            else
                errors.SetError(txtDeliveryWindow, "");

            int ref2 = Convert.ToInt32(txtMaxRetries.Text);
            if (string.IsNullOrEmpty(txtMaxRetries.Text))
                errors.SetError(txtMaxRetries, res.CTRLNULLOREMPTYERROR);
            else if ((ref2 < 1) || (ref2 > 12))
                errors.SetError(txtMaxRetries, string.Format(res.CTRLINVALIDVALUE, "1", "12"));
            else
                errors.SetError(txtMaxRetries, "");

            if (string.IsNullOrEmpty(txtCountryCode.Text))
                errors.SetError(txtCountryCode, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtCountryCode, "");

            if (string.IsNullOrEmpty(txtAdminContact.Text))
                errors.SetError(txtAdminContact, res.CTRLNULLOREMPTYERROR);
            else if (!MMCService.IsValidEmail(txtAdminContact.Text))
                errors.SetError(txtAdminContact, res.CTRLGLINVALIDEMAILCONTACT);
            else
                errors.SetError(txtAdminContact, "");

            if (string.IsNullOrEmpty(txtIssuer.Text))
                errors.SetError(txtIssuer, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtIssuer, "");
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
           if (_txtpanel != null)
               _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// SelectedTemplateChanged method implementation
        /// </summary>
        private void SelectedPolicyTemplateChanged(object sender, EventArgs e)
        {
            UserTemplateMode currenttmp = GetPolicyTemplate();
            SetPolicyTemplate(currenttmp);
        }

        /// <summary>
        /// SetPolicyTemplate method implmentation
        /// </summary>
        private void SetPolicyTemplate(UserTemplateMode template)
        {
            if (_view.AutoValidate != AutoValidate.Disable)
            {
                bool unlocked = false;
                int currentidx = cbConfigTemplate.Items.IndexOf(template);
                int newidx = cbConfigTemplate.SelectedIndex;
                if (currentidx == newidx)
                    return;
                if (newidx == cbConfigTemplate.Items.IndexOf(UserTemplateMode.Free))
                    Config.UserFeatures = Config.UserFeatures.SetPolicyTemplate(UserTemplateMode.Free);
                else if (newidx == cbConfigTemplate.Items.IndexOf(UserTemplateMode.Open))
                    Config.UserFeatures = Config.UserFeatures.SetPolicyTemplate(UserTemplateMode.Open);
                else if (newidx == cbConfigTemplate.Items.IndexOf(UserTemplateMode.Default))
                    Config.UserFeatures = Config.UserFeatures.SetPolicyTemplate(UserTemplateMode.Default);
                else if (newidx == cbConfigTemplate.Items.IndexOf(UserTemplateMode.Managed))
                    Config.UserFeatures = Config.UserFeatures.SetPolicyTemplate(UserTemplateMode.Managed);
                else if (newidx == cbConfigTemplate.Items.IndexOf(UserTemplateMode.Strict))
                    Config.UserFeatures = Config.UserFeatures.SetPolicyTemplate(UserTemplateMode.Strict);
                else if (newidx == cbConfigTemplate.Items.IndexOf(UserTemplateMode.Administrative))
                    Config.UserFeatures = Config.UserFeatures.SetPolicyTemplate(UserTemplateMode.Administrative);
                else
                {
                    unlocked = true;
                    Config.UserFeatures = Config.UserFeatures.SetPolicyTemplate(UserTemplateMode.Custom);
                }
                UpdateLayoutPolicyComponents(unlocked);
                ManagementService.ADFSManager.SetDirty(true);
            }
        }

        /// <summary>
        /// GetPolicyTemplate method implementation
        /// </summary>
        private UserTemplateMode GetPolicyTemplate()
        {
            return Config.UserFeatures.GetPolicyTemplate();
        }

        /// <summary>
        /// AdjustPolicyTemplate method implmentation
        /// </summary>
        private void AdjustPolicyTemplate()
        {
            bool unlocked = false;
            UserTemplateMode template = Config.UserFeatures.GetPolicyTemplate();
            cbConfigTemplate.SelectedIndex = cbConfigTemplate.Items.IndexOf(template);
            if (template == UserTemplateMode.Custom)
                unlocked = true;
            UpdateLayoutPolicyComponents(unlocked);
        }
        
        /// <summary>
        /// UpdateLayoutPolicyComponents method implmentation
        /// </summary>
        private void UpdateLayoutPolicyComponents(bool unlocked)
        {
            try
            {
                rdioMFARequired.Checked = Config.UserFeatures.IsMFARequired();
                rdioMFAAllowed.Checked = Config.UserFeatures.IsMFAAllowed();
                rdioMFANotRequired.Checked = Config.UserFeatures.IsMFANotRequired();

                rdioREGAdmin.Checked = Config.UserFeatures.IsRegistrationRequired();
                rdioREGUser.Checked = Config.UserFeatures.IsRegistrationAllowed();
                rdioREGUnManaged.Checked = Config.UserFeatures.IsRegistrationNotRequired();

                chkAllowManageOptions.Checked = Config.UserFeatures.CanManageOptions();
                chkAllowEnrollment.Checked = Config.UserFeatures.CanEnrollDevices();
                chkAllowChangePassword.Checked = Config.UserFeatures.CanManagePassword();
                chkAllowKMSOO.Checked = Config.KeepMySelectedOptionOn;

                if (!unlocked)
                {
                    rdioMFARequired.Enabled = rdioMFARequired.Checked;
                    rdioMFAAllowed.Enabled = rdioMFAAllowed.Checked;
                    rdioMFANotRequired.Enabled = rdioMFANotRequired.Checked;
                    rdioREGAdmin.Enabled = rdioREGAdmin.Checked;
                    rdioREGUser.Enabled = rdioREGUser.Checked;
                    rdioREGUnManaged.Enabled = rdioREGUnManaged.Checked;
                    chkAllowManageOptions.Enabled = false;
                    chkAllowEnrollment.Enabled = false;
                    chkAllowChangePassword.Enabled = false;
                    chkAllowKMSOO.Enabled = true;
                }
                else
                {
                    rdioMFARequired.Enabled = true;
                    rdioMFAAllowed.Enabled = true;
                    rdioMFANotRequired.Enabled = true;
                    rdioREGAdmin.Enabled = true;
                    rdioREGUser.Enabled = Config.UserFeatures.IsRegistrationAllowed();
                    rdioREGUnManaged.Enabled = true;
                    chkAllowManageOptions.Enabled = true;
                    chkAllowEnrollment.Enabled = true;
                    chkAllowChangePassword.Enabled = true;
                    chkAllowKMSOO.Enabled = true;
                }
                txtADVStart.Enabled = Config.UserFeatures.IsAdvertisable();
                txtADVEnd.Enabled = Config.UserFeatures.IsAdvertisable();
            }
            finally
            {

            }
        }

        #region Features mgmt
        /// <summary>
        /// REGUnManagedCheckedChanged event
        /// </summary>
        private void REGUnManagedCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (rdioREGUnManaged.Checked)
                    {
                        Config.UserFeatures = Config.UserFeatures.SetUnManagedRegistration();
                        txtADVStart.Enabled = Config.UserFeatures.IsAdvertisable();
                        txtADVEnd.Enabled = Config.UserFeatures.IsAdvertisable();
                        ManagementService.ADFSManager.SetDirty(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// REGUserCheckedChanged event
        /// </summary>
        private void REGUserCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (rdioREGUser.Checked)
                    {
                        Config.UserFeatures = Config.UserFeatures.SetSelfRegistration();
                        txtADVStart.Enabled = Config.UserFeatures.IsAdvertisable();
                        txtADVEnd.Enabled = Config.UserFeatures.IsAdvertisable();
                        ManagementService.ADFSManager.SetDirty(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// REGAdminCheckedChanged event
        /// </summary>
        private void REGAdminCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (rdioREGAdmin.Checked)
                    {
                        Config.UserFeatures = Config.UserFeatures.SetAdministrativeRegistration();
                        txtADVStart.Enabled = Config.UserFeatures.IsAdvertisable();
                        txtADVEnd.Enabled = Config.UserFeatures.IsAdvertisable();
                        ManagementService.ADFSManager.SetDirty(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// MFANotRequiredCheckedChanged event
        /// </summary>
        private void MFANotRequiredCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (rdioMFANotRequired.Checked)
                    {
                        Config.UserFeatures = Config.UserFeatures.SetMFANotRequired();
                        rdioREGUser.Enabled = true;
                        txtADVStart.Enabled = Config.UserFeatures.IsAdvertisable();
                        txtADVEnd.Enabled = Config.UserFeatures.IsAdvertisable();
                        ManagementService.ADFSManager.SetDirty(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// MFAAllowedCheckedChanged event
        /// </summary>
        private void MFAAllowedCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (rdioMFAAllowed.Checked)
                    {
                        Config.UserFeatures = Config.UserFeatures.SetMFAAllowed();
                        rdioREGUser.Enabled = true;
                        txtADVStart.Enabled = Config.UserFeatures.IsAdvertisable();
                        txtADVEnd.Enabled = Config.UserFeatures.IsAdvertisable();
                        ManagementService.ADFSManager.SetDirty(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// MFARequiredCheckedChanged event
        /// </summary>
        private void MFARequiredCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (rdioMFARequired.Checked)
                    {
                        Config.UserFeatures = Config.UserFeatures.SetMFARequired();
                        if (rdioREGUser.Checked)
                            rdioREGAdmin.Checked = true;
                        rdioREGUser.Enabled = false;
                        txtADVStart.Enabled = Config.UserFeatures.IsAdvertisable();
                        txtADVEnd.Enabled = Config.UserFeatures.IsAdvertisable();
                        ManagementService.ADFSManager.SetDirty(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// AllowChangePasswordCheckedChanged method implmentation
        /// </summary>
        private void AllowChangePasswordCheckedChanged(object sender, EventArgs e)
        {
            try 
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (chkAllowChangePassword.Checked)
                        Config.UserFeatures = Config.UserFeatures.Add(UserFeaturesOptions.AllowChangePassword);
                    else
                        Config.UserFeatures = Config.UserFeatures.Remove(UserFeaturesOptions.AllowChangePassword);
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// AllowManageOptionsCheckedChanged method implmentation
        /// </summary>
        private void AllowManageOptionsCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (chkAllowManageOptions.Checked)
                        Config.UserFeatures = Config.UserFeatures.Add(UserFeaturesOptions.AllowManageOptions);
                    else
                        Config.UserFeatures = Config.UserFeatures.Remove(UserFeaturesOptions.AllowManageOptions);
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// AllowManageOptionsCheckedChanged method implmentation
        /// </summary>
        private void AllowEnrollmentWizardCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    if (chkAllowEnrollment.Checked)
                        Config.UserFeatures = Config.UserFeatures.Add(UserFeaturesOptions.AllowEnrollment);
                    else
                        Config.UserFeatures = Config.UserFeatures.Remove(UserFeaturesOptions.AllowEnrollment);
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// AllowKMSOOCheckedChanged method implementation
        /// </summary>
        private void AllowKMSOOCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.KeepMySelectedOptionOn = chkAllowKMSOO.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        #endregion

        #region Advertising
        /// <summary>
        /// ADVEndValueChanged method implementation
        /// </summary>
        private void ADVEndValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.AdvertisingDays.LastDay = Convert.ToUInt32(txtADVEnd.Value);
                    errors.SetError(txtADVEnd, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(txtADVEnd, ex.Message);
            }
        }

        /// <summary>
        /// ADVStartValueChanged method implementation
        /// </summary>
        private void ADVStartValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.AdvertisingDays.FirstDay = Convert.ToUInt32(txtADVStart.Value);
                    errors.SetError(txtADVStart, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(txtADVStart, ex.Message);
            }
        }
        #endregion

        #region DeliveryWindow
        /// <summary>
        /// DeliveryWindowValidating method implementation
        /// </summary>
        private void DeliveryWindowValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtDeliveryWindow.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    int refr = Convert.ToInt32(txtDeliveryWindow.Text);
                    if (string.IsNullOrEmpty(txtDeliveryWindow.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if ((refr < 60) || (refr > 600))
                        throw new Exception(string.Format(res.CTRLINVALIDVALUE, "60", "600"));
                    Config.DeliveryWindow = refr;
                    errors.SetError(txtDeliveryWindow, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtDeliveryWindow, ex.Message);
            }
        }

        /// <summary>
        /// DeliveryWindowValidated method implementation
        /// </summary>
        private void DeliveryWindowValidated(object sender, EventArgs e)
        {
            try
            {
                Config.DeliveryWindow = Convert.ToInt32(txtDeliveryWindow.Text);
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtDeliveryWindow, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region MaxRetries
        /// <summary>
        /// MaxRetriesValidating method implementation
        /// </summary>
        private void MaxRetriesValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtMaxRetries.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    int refr = Convert.ToInt32(txtMaxRetries.Text);
                    if (string.IsNullOrEmpty(txtMaxRetries.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if ((refr < 1) || (refr > 12))
                        throw new Exception(string.Format(res.CTRLINVALIDVALUE, "1", "12"));
                    Config.MaxRetries = refr;
                    errors.SetError(txtMaxRetries, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtMaxRetries, ex.Message);
            }
        }

        /// <summary>
        /// MaxRetriesValidated method implementation
        /// </summary>
        private void MaxRetriesValidated(object sender, EventArgs e)
        {
            try
            {
                ManagementService.ADFSManager.SetDirty(true);
                Config.MaxRetries = Convert.ToInt32(txtMaxRetries.Text);
                errors.SetError(txtMaxRetries, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region CountryCode
        /// <summary>
        /// CountryCodeValidating event
        /// </summary>
        private void CountryCodeValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtCountryCode.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtCountryCode.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.DefaultCountryCode = txtCountryCode.Text.ToLower();
                    errors.SetError(txtCountryCode, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtCountryCode, ex.Message);
            }
        }

        /// <summary>
        /// CountryCodeValidated event
        /// </summary>
        private void CountryCodeValidated(object sender, EventArgs e)
        {
            try
            {
                Config.DefaultCountryCode = txtCountryCode.Text.ToLower();
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtCountryCode, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region AdminContact
        /// <summary>
        /// AdminContactTextChanged event
        /// </summary>
        private void AdminContactValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtAdminContact.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtAdminContact.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if (!MMCService.IsValidEmail(txtAdminContact.Text))
                        throw new Exception(res.CTRLGLINVALIDEMAILCONTACT);
                    Config.AdminContact = txtAdminContact.Text;
                    errors.SetError(txtAdminContact, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtAdminContact, ex.Message);
            }
        }

        /// <summary>
        /// AdminContactValidated event
        /// </summary>
        private void AdminContactValidated(object sender, EventArgs e)
        {
            try
            {
                Config.AdminContact = txtAdminContact.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtAdminContact, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Issuer
        /// <summary>
        /// IssuerValidating event
        /// </summary>
        private void IssuerValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtIssuer.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtIssuer.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.Issuer = txtIssuer.Text;
                    errors.SetError(txtIssuer, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtIssuer, ex.Message);
            }
        }

        /// <summary>
        /// IssuerValidated event
        /// </summary>
        private void IssuerValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Issuer = txtIssuer.Text; 
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtIssuer, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    public partial class ADDSConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private ADDSViewControl _view;
        private CheckBox chkUseADDS;
        private TextBox txtDomainName;
        private bool _UpdateControlsLayouts;
        private TextBox txtUserName;
        private TextBox txtPassword;
        private TextBox txtKeyAttribute;
        private TextBox txtMailAttribute;
        private TextBox txtPhoneAttribute;
        private TextBox txtMethodAttribute;
        private TextBox txtOverrideMethodAttribute;
        private TextBox txtPinAttribute;
        private TextBox txtTOTPAttribute;
        private TextBox txtEnabledAttribute;
        private TextBox txtCheckdateAttribute;
        private TextBox txtMaxRows;
        private Button btnConnect;
        private ErrorProvider errors;
        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;
        private Label lblMaxRows;
        private Label lblEnableAttribute;
        private Label lblValiditydateAttribute;
        private Label lblCreatedateAttribute;
        private Label lblMethodAttribute;
        private Label lblPhoneAttribute;
        private Label lblMailAttribute;
        private Label lblKeyAttribute;
        private Label lblAttributes;
        private Label lblPassword;
        private Label lblUserName;
        private Label lblDomainName;


        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public ADDSConfigurationControl(ADDSViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 585;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 471;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 471;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                chkUseADDS = new CheckBox();
                chkUseADDS.Text = res.CTRLADUSEADDS;
                chkUseADDS.Checked = Config.UseActiveDirectory;
                chkUseADDS.Left = 10;
                chkUseADDS.Top = 19;
                chkUseADDS.Width = 450;
                chkUseADDS.CheckedChanged += UseADDSCheckedChanged;
                _txtpanel.Controls.Add(chkUseADDS);

                lblDomainName = new Label();
                lblDomainName.Text = res.CTRLADDOMAIN + " : ";
                lblDomainName.Left = 50;
                lblDomainName.Top = 51;
                lblDomainName.Width = 150;
                _txtpanel.Controls.Add(lblDomainName);

                txtDomainName = new TextBox();
                txtDomainName.Text = Config.Hosts.ActiveDirectoryHost.DomainAddress;
                txtDomainName.Left = 210;
                txtDomainName.Top = 47;
                txtDomainName.Width = 230;
                txtDomainName.Enabled = Config.UseActiveDirectory;
                txtDomainName.Validating += DomainNameValidating;
                txtDomainName.Validated += DomainNameValidated;
                _txtpanel.Controls.Add(txtDomainName);

                lblUserName = new Label();
                lblUserName.Text = res.CTRLADACCOUNT + " : ";
                lblUserName.Left = 480;
                lblUserName.Top = 51;
                lblUserName.Width = 100;
                _txtpanel.Controls.Add(lblUserName);

                txtUserName = new TextBox();
                txtUserName.Text = Config.Hosts.ActiveDirectoryHost.Account;
                txtUserName.Left = 580;
                txtUserName.Top = 47;
                txtUserName.Width = 230;
                txtUserName.Enabled = Config.UseActiveDirectory;
                txtUserName.Validating += UserNameValidating;
                txtUserName.Validated += UserNameValidated;
                _txtpanel.Controls.Add(txtUserName);

                lblPassword = new Label();
                lblPassword.Text = res.CTRLADPASSWORD + " : ";
                lblPassword.Left = 480;
                lblPassword.Top = 82;
                lblPassword.Width = 85;
                _txtpanel.Controls.Add(lblPassword);

                txtPassword = new TextBox();
                txtPassword.Text = Config.Hosts.ActiveDirectoryHost.Account;
                txtPassword.Left = 580;
                txtPassword.Top = 78;
                txtPassword.Width = 230;
                txtPassword.PasswordChar = '*';
                txtPassword.Enabled = Config.UseActiveDirectory;
                txtPassword.Validating += PasswordValidating;
                txtPassword.Validated += PasswordValidated;
                _txtpanel.Controls.Add(txtPassword);

                btnConnect = new Button();
                btnConnect.Text = res.CTRLADTEST;
                btnConnect.Left = 580;
                btnConnect.Top = 109;
                btnConnect.Width = 230;
                btnConnect.Enabled = Config.UseActiveDirectory;
                btnConnect.Click += btnConnectClick;
                _txtpanel.Controls.Add(btnConnect);

                lblAttributes = new Label();
                lblAttributes.Text = res.CTRLADATTRIBUTES + " : ";
                lblAttributes.Left = 30;
                lblAttributes.Top = 119;
                lblAttributes.Width = 300;
                _txtpanel.Controls.Add(lblAttributes);

                lblKeyAttribute = new Label();
                lblKeyAttribute.Text = res.CTRLADATTKEY + " : ";
                lblKeyAttribute.Left = 50;
                lblKeyAttribute.Top = 150;
                lblKeyAttribute.Width = 150;
                _txtpanel.Controls.Add(lblKeyAttribute);

                txtKeyAttribute = new TextBox();
                txtKeyAttribute.Text = Config.Hosts.ActiveDirectoryHost.keyAttribute;
                txtKeyAttribute.Left = 210;
                txtKeyAttribute.Top = 146;
                txtKeyAttribute.Width = 600;
                txtKeyAttribute.Enabled = Config.UseActiveDirectory;
                txtKeyAttribute.Validating += KeyAttributeValidating;
                txtKeyAttribute.Validated += KeyAttributeValidated;
                _txtpanel.Controls.Add(txtKeyAttribute);

                lblMailAttribute = new Label();
                lblMailAttribute.Text = res.CTRLADATTMAIL + " : ";
                lblMailAttribute.Left = 50;
                lblMailAttribute.Top = 181;
                lblMailAttribute.Width = 150;
                _txtpanel.Controls.Add(lblMailAttribute);

                txtMailAttribute = new TextBox();
                txtMailAttribute.Text = Config.Hosts.ActiveDirectoryHost.mailAttribute;
                txtMailAttribute.Left = 210;
                txtMailAttribute.Top = 177;
                txtMailAttribute.Width = 600;
                txtMailAttribute.Enabled = Config.UseActiveDirectory;
                txtMailAttribute.Validating += MailAttributeValidating;
                txtMailAttribute.Validated += MailAttributeValidated;
                _txtpanel.Controls.Add(txtMailAttribute);

                lblPhoneAttribute = new Label();
                lblPhoneAttribute.Text = res.CTRLADATTPHONE + " : ";
                lblPhoneAttribute.Left = 50;
                lblPhoneAttribute.Top = 212;
                lblPhoneAttribute.Width = 150;
                _txtpanel.Controls.Add(lblPhoneAttribute);

                txtPhoneAttribute = new TextBox();
                txtPhoneAttribute.Text = Config.Hosts.ActiveDirectoryHost.phoneAttribute;
                txtPhoneAttribute.Left = 210;
                txtPhoneAttribute.Top = 208;
                txtPhoneAttribute.Width = 600;
                txtPhoneAttribute.Enabled = Config.UseActiveDirectory;
                txtPhoneAttribute.Validating += PhoneAttributeValidating;
                txtPhoneAttribute.Validated += PhoneAttributeValidated;
                _txtpanel.Controls.Add(txtPhoneAttribute);

                lblMethodAttribute = new Label();
                lblMethodAttribute.Text = res.CTRLADATTMETHOD + " : ";
                lblMethodAttribute.Left = 50;
                lblMethodAttribute.Top = 243;
                lblMethodAttribute.Width = 150;
                _txtpanel.Controls.Add(lblMethodAttribute);

                txtMethodAttribute = new TextBox();
                txtMethodAttribute.Text = Config.Hosts.ActiveDirectoryHost.methodAttribute;
                txtMethodAttribute.Left = 210;
                txtMethodAttribute.Top = 239;
                txtMethodAttribute.Width = 600;
                txtMethodAttribute.Enabled = Config.UseActiveDirectory;
                txtMethodAttribute.Validating += MethodAttributeValidating;
                txtMethodAttribute.Validated += MethodAttributeValidated;
                _txtpanel.Controls.Add(txtMethodAttribute);

                lblCreatedateAttribute = new Label();
                lblCreatedateAttribute.Text = res.CTRLADATTOVERRIDE + " : ";
                lblCreatedateAttribute.Left = 50;
                lblCreatedateAttribute.Top = 274;
                lblCreatedateAttribute.Width = 150;
                _txtpanel.Controls.Add(lblCreatedateAttribute);

                txtOverrideMethodAttribute = new TextBox();
                txtOverrideMethodAttribute.Text = Config.Hosts.ActiveDirectoryHost.overridemethodAttribute;
                txtOverrideMethodAttribute.Left = 210;
                txtOverrideMethodAttribute.Top = 270;
                txtOverrideMethodAttribute.Width = 600;
                txtOverrideMethodAttribute.Enabled = Config.UseActiveDirectory;
                txtOverrideMethodAttribute.Validating += OverrideMethodAttributeValidating;
                txtOverrideMethodAttribute.Validated += OverrideMethodAttributeValidated;
                _txtpanel.Controls.Add(txtOverrideMethodAttribute);

                lblValiditydateAttribute = new Label();
                lblValiditydateAttribute.Text = res.CTRLADATTPIN + " : ";
                lblValiditydateAttribute.Left = 50;
                lblValiditydateAttribute.Top = 305;
                lblValiditydateAttribute.Width = 150;
                _txtpanel.Controls.Add(lblValiditydateAttribute);

                txtPinAttribute = new TextBox();
                txtPinAttribute.Text = Config.Hosts.ActiveDirectoryHost.pinattribute;
                txtPinAttribute.Left = 210;
                txtPinAttribute.Top = 301;
                txtPinAttribute.Width = 600;
                txtPinAttribute.Enabled = Config.UseActiveDirectory;
                txtPinAttribute.Validating += PinAttributeValidating;
                txtPinAttribute.Validated += PinAttributeValidated;
                _txtpanel.Controls.Add(txtPinAttribute);

                /*  Label lblCheckdateAttribute = new Label();
                  lblCheckdateAttribute.Text = res.CTRLADATTVALIDATION+" : ";
                  lblCheckdateAttribute.Left = 50;
                  lblCheckdateAttribute.Top = 336;
                  lblCheckdateAttribute.Width = 150;
                  _txtpanel.Controls.Add(lblCheckdateAttribute); */

                txtCheckdateAttribute = new TextBox();
                txtCheckdateAttribute.Text = "Not Used";
                txtCheckdateAttribute.Left = 210;
                txtCheckdateAttribute.Top = 332;
                txtCheckdateAttribute.Width = 600;
                txtCheckdateAttribute.Enabled = Config.UseActiveDirectory;
                txtCheckdateAttribute.Validating += CheckDateAttributeValidating;
                txtCheckdateAttribute.Validated += CheckDateAttributeValidated;
                _txtpanel.Controls.Add(txtCheckdateAttribute);

                /*  Label lblTOTPAttribute = new Label();
                  lblTOTPAttribute.Text = res.CTRLADATTCODE+" : ";
                  lblTOTPAttribute.Left = 50;
                  lblTOTPAttribute.Top = 367;
                  lblTOTPAttribute.Width = 150;
                  _txtpanel.Controls.Add(lblTOTPAttribute); */

                txtTOTPAttribute = new TextBox();
                txtTOTPAttribute.Text = "Not Used";
                txtTOTPAttribute.Left = 210;
                txtTOTPAttribute.Top = 363;
                txtTOTPAttribute.Width = 600;
                txtTOTPAttribute.Enabled = Config.UseActiveDirectory;
                txtTOTPAttribute.Validating += TOTPAttributeValidating;
                txtTOTPAttribute.Validated += TOTPAttributeValidated;
                _txtpanel.Controls.Add(txtTOTPAttribute);

                lblEnableAttribute = new Label();
                lblEnableAttribute.Text = res.CTRLADATTSTATUS + " : ";
                lblEnableAttribute.Left = 50;
                lblEnableAttribute.Top = 398;
                lblEnableAttribute.Width = 150;
                _txtpanel.Controls.Add(lblEnableAttribute);

                txtEnabledAttribute = new TextBox();
                txtEnabledAttribute.Text = Config.Hosts.ActiveDirectoryHost.totpEnabledAttribute;
                txtEnabledAttribute.Left = 210;
                txtEnabledAttribute.Top = 394;
                txtEnabledAttribute.Width = 600;
                txtEnabledAttribute.Enabled = Config.UseActiveDirectory;
                txtEnabledAttribute.Validating += EnabledAttributeValidating;
                txtEnabledAttribute.Validated += EnabledAttributeValidated;
                _txtpanel.Controls.Add(txtEnabledAttribute);

                lblMaxRows = new Label();
                lblMaxRows.Text = res.CTRLSQLMAXROWS + " : ";
                lblMaxRows.Left = 50;
                lblMaxRows.Top = 429;
                lblMaxRows.Width = 150;
                _txtpanel.Controls.Add(lblMaxRows);

                txtMaxRows = new TextBox();
                txtMaxRows.Text = Config.Hosts.ActiveDirectoryHost.MaxRows.ToString();
                txtMaxRows.Left = 210;
                txtMaxRows.Top = 425;
                txtMaxRows.Width = 50;
                txtMaxRows.TextAlign = HorizontalAlignment.Right;
                txtMaxRows.Enabled = Config.UseActiveDirectory;
                txtMaxRows.Validating += MaxRowsValidating;
                txtMaxRows.Validated += MaxRowsValidated;
                _txtpanel.Controls.Add(txtMaxRows);


                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 481;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 481;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implmentation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                chkUseADDS.Checked = Config.UseActiveDirectory;

                txtDomainName.Text = Config.Hosts.ActiveDirectoryHost.DomainAddress;
                txtDomainName.Enabled = Config.UseActiveDirectory;

                txtUserName.Text = Config.Hosts.ActiveDirectoryHost.Account;
                txtUserName.Enabled = Config.UseActiveDirectory;

                txtPassword.Text = Config.Hosts.ActiveDirectoryHost.Account;
                txtPassword.Enabled = Config.UseActiveDirectory;

                btnConnect.Enabled = Config.UseActiveDirectory;

                txtKeyAttribute.Text = Config.Hosts.ActiveDirectoryHost.keyAttribute;
                txtKeyAttribute.Enabled = Config.UseActiveDirectory;

                txtMailAttribute.Text = Config.Hosts.ActiveDirectoryHost.mailAttribute;
                txtMailAttribute.Enabled = Config.UseActiveDirectory;

                txtPhoneAttribute.Text = Config.Hosts.ActiveDirectoryHost.phoneAttribute;
                txtPhoneAttribute.Enabled = Config.UseActiveDirectory;

                txtMethodAttribute.Text = Config.Hosts.ActiveDirectoryHost.methodAttribute;
                txtMethodAttribute.Enabled = Config.UseActiveDirectory;

                txtOverrideMethodAttribute.Text = Config.Hosts.ActiveDirectoryHost.overridemethodAttribute;
                txtOverrideMethodAttribute.Enabled = Config.UseActiveDirectory;

                txtPinAttribute.Text = Config.Hosts.ActiveDirectoryHost.pinattribute;
                txtPinAttribute.Enabled = Config.UseActiveDirectory;

                txtCheckdateAttribute.Text = "Not Used";
                txtCheckdateAttribute.Enabled = false;

                txtTOTPAttribute.Text = "Not Used";
                txtTOTPAttribute.Enabled = false;

                txtEnabledAttribute.Text = Config.Hosts.ActiveDirectoryHost.totpEnabledAttribute;
                txtEnabledAttribute.Enabled = Config.UseActiveDirectory;

                txtMaxRows.Text = Config.Hosts.ActiveDirectoryHost.MaxRows.ToString();
                txtMaxRows.Enabled = Config.UseActiveDirectory;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                lblMaxRows.Text = res.CTRLSQLMAXROWS + " : ";
                lblEnableAttribute.Text = res.CTRLADATTSTATUS + " : ";
                lblValiditydateAttribute.Text = res.CTRLADATTPIN + " : ";
                lblCreatedateAttribute.Text = res.CTRLADATTOVERRIDE + " : ";
                lblMethodAttribute.Text = res.CTRLADATTMETHOD + " : ";
                lblPhoneAttribute.Text = res.CTRLADATTPHONE + " : ";
                lblMailAttribute.Text = res.CTRLADATTMAIL + " : ";
                lblKeyAttribute.Text = res.CTRLADATTKEY + " : ";
                lblAttributes.Text = res.CTRLADATTRIBUTES + " : ";
                lblPassword.Text = res.CTRLADPASSWORD + " : ";
                lblUserName.Text = res.CTRLADACCOUNT + " : ";
                lblDomainName.Text = res.CTRLADDOMAIN + " : ";
                btnConnect.Text = res.CTRLADTEST;
                chkUseADDS.Text = res.CTRLADUSEADDS;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ValidateData method implementation
        /// </summary>
        private void ValidateData()
        {
            if (chkUseADDS.Checked)
            {
                if (!ManagementService.CheckADDSConnection(txtDomainName.Text, txtUserName.Text, txtPassword.Text))
                {
                    errors.SetError(txtDomainName, res.CTRLADATTDOMAIN);
                    errors.SetError(txtUserName, res.CTRLADATTACCOUNT);
                    errors.SetError(txtPassword, res.CTRLADATTPASSWORD);
                }
                else
                {
                    errors.SetError(txtDomainName, "");
                    errors.SetError(txtUserName, "");
                    errors.SetError(txtPassword, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtOverrideMethodAttribute.Text))
                        errors.SetError(txtOverrideMethodAttribute, res.CTRLADATOVERRIDEERROR);
                    else
                        errors.SetError(txtOverrideMethodAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtEnabledAttribute.Text))
                        errors.SetError(txtEnabledAttribute, res.CTRLADATENABLEDERROR);
                    else
                        errors.SetError(txtEnabledAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtKeyAttribute.Text))
                        errors.SetError(txtKeyAttribute, res.CTRLADATTKEYERROR);
                    else
                        errors.SetError(txtKeyAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtMailAttribute.Text))
                        errors.SetError(txtMailAttribute, res.CTRLADATTEMAILERROR);
                    else
                        errors.SetError(txtMailAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtMethodAttribute.Text))
                        errors.SetError(txtMethodAttribute, res.CTRLADATMETHODERROR);
                    else
                        errors.SetError(txtMethodAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtPhoneAttribute.Text))
                        errors.SetError(txtPhoneAttribute, res.CTRLADATTPHONEERROR);
                    else
                        errors.SetError(txtPhoneAttribute, "");

                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtPinAttribute.Text))
                        errors.SetError(txtPinAttribute, res.CTRLADATPINERROR);
                    else
                        errors.SetError(txtPinAttribute, "");

                    int maxrows = Convert.ToInt32(txtMaxRows.Text);
                    if ((maxrows < 1000) || (maxrows > 1000000))
                        errors.SetError(txtMaxRows, String.Format(res.CTRLSQLMAXROWSERROR, maxrows));
                    else
                        errors.SetError(txtMaxRows, "");
                }
            }
            else
            {
                errors.SetError(txtDomainName, "");
                errors.SetError(txtUserName, "");
                errors.SetError(txtPassword, "");
                errors.SetError(txtOverrideMethodAttribute, "");
                errors.SetError(txtEnabledAttribute, "");
                errors.SetError(txtKeyAttribute, "");
                errors.SetError(txtMailAttribute, "");
                errors.SetError(txtMethodAttribute, "");
                errors.SetError(txtPhoneAttribute, "");
                errors.SetError(txtPinAttribute, "");
                errors.SetError(txtMaxRows, "");
            }
        }

        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts(bool isenabled)
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
                txtDomainName.Enabled = isenabled;
                txtUserName.Enabled = isenabled;
                txtPassword.Enabled = isenabled;
                txtCheckdateAttribute.Enabled = isenabled;
                txtOverrideMethodAttribute.Enabled = isenabled;
                txtEnabledAttribute.Enabled = isenabled;
                txtKeyAttribute.Enabled = isenabled;
                txtMailAttribute.Enabled = isenabled;
                txtMethodAttribute.Enabled = isenabled;
                txtPhoneAttribute.Enabled = isenabled;
                txtTOTPAttribute.Enabled = isenabled;
                txtPinAttribute.Enabled = isenabled;
                txtMaxRows.Enabled = isenabled;
                btnConnect.Enabled = isenabled;
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// UseAADDSCheckedChanged method implementation
        /// </summary>
        private void UseADDSCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.UseActiveDirectory = chkUseADDS.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts(Config.UseActiveDirectory);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        #region DomainName
        /// <summary>
        /// DomainNameValidating method implementation
        /// </summary>
        private void DomainNameValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtDomainName.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSConnection(txtDomainName.Text, txtUserName.Text, txtPassword.Text))
                        throw new Exception(res.CTRLADATTDOMAIN);
                    Config.Hosts.ActiveDirectoryHost.DomainAddress = txtDomainName.Text;
                    errors.SetError(txtDomainName, "");
                }
            }
            catch (Exception)
            {
                e.Cancel = true;
                errors.SetError(txtDomainName, res.CTRLADATTDOMAIN);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// DomainNameValidated method implmentation
        /// </summary>
        private void DomainNameValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.DomainAddress = txtDomainName.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtDomainName, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Username
        /// <summary>
        /// UserNameValidating method implementation
        /// </summary>
        private void UserNameValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtUserName.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSConnection(txtDomainName.Text, txtUserName.Text, txtPassword.Text))
                       throw new Exception(res.CTRLADATTACCOUNT);
                    Config.Hosts.ActiveDirectoryHost.Account = txtUserName.Text;
                    errors.SetError(txtUserName, "");
                }
            }
            catch (Exception)
            {
                e.Cancel = true;
                errors.SetError(txtUserName, res.CTRLADATTACCOUNT);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// UserNameValidated method implementation
        /// </summary>
        private void UserNameValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.Account = txtUserName.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtUserName, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Password
        /// <summary>
        /// UserNameValidating method implementation
        /// </summary>
        private void PasswordValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtPassword.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSConnection(txtDomainName.Text, txtUserName.Text, txtPassword.Text))
                        throw new Exception(res.CTRLADATTPASSWORD);
                    Config.Hosts.ActiveDirectoryHost.Password = txtPassword.Text;
                    errors.SetError(txtPassword, "");
                }
            }
            catch (Exception)
            {
                e.Cancel = true;
                errors.SetError(txtPassword, res.CTRLADATTPASSWORD);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// PasswordValidated method implementation
        /// </summary>
        private void PasswordValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.Password = txtPassword.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtPassword, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region KeyAttribute
        /// <summary>
        /// KeyAttributeValidating method implementation
        /// </summary>
        private void KeyAttributeValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtKeyAttribute.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtKeyAttribute.Text))
                        throw new Exception(res.CTRLADATTKEYERROR);
                    Config.Hosts.ActiveDirectoryHost.keyAttribute = txtKeyAttribute.Text;
                    errors.SetError(txtKeyAttribute, "");
                }
            }
            catch (Exception)
            {
                e.Cancel = true;
                errors.SetError(txtKeyAttribute, res.CTRLADATTKEYERROR);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// KeyAttributeValidated method implmentation
        /// </summary>
        private void KeyAttributeValidated(object sender, EventArgs e)
        {

            try
            {
                Config.Hosts.ActiveDirectoryHost.keyAttribute = txtKeyAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtKeyAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region MailAttribute
        /// <summary>
        /// MailAttributeValidating method implementation
        /// </summary>
        private void MailAttributeValidating(object sender, CancelEventArgs e)
        {

            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtMailAttribute.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtMailAttribute.Text))
                       throw new Exception(res.CTRLADATTEMAILERROR);
                    Config.Hosts.ActiveDirectoryHost.mailAttribute = txtMailAttribute.Text;
                    errors.SetError(txtMailAttribute, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtMailAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// MailAttributeValidated method implmentation
        /// </summary>
        private void MailAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.mailAttribute = txtMailAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtMailAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region PhoneAttribute
        /// <summary>
        /// PhoneAttributeValidating method implementation
        /// </summary>
        private void PhoneAttributeValidating(object sender, CancelEventArgs e)
        {

            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtPhoneAttribute.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtPhoneAttribute.Text))
                        throw new Exception(res.CTRLADATTPHONEERROR);
                    Config.Hosts.ActiveDirectoryHost.phoneAttribute = txtPhoneAttribute.Text;
                    errors.SetError(txtPhoneAttribute, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtPhoneAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// PhoneAttributeValidated method implementation
        /// </summary>
        private void PhoneAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.phoneAttribute = txtPhoneAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtPhoneAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region MethodAttribute
        /// <summary>
        /// MethodAttributeValidating method implementation
        /// </summary>
        private void MethodAttributeValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtMethodAttribute.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtMethodAttribute.Text))
                        throw new Exception(res.CTRLADATMETHODERROR);
                    Config.Hosts.ActiveDirectoryHost.methodAttribute = txtMethodAttribute.Text;
                    errors.SetError(txtMethodAttribute, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtMethodAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// MethodAttributeValidated method implmentation
        /// </summary>
        private void MethodAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.methodAttribute = txtMethodAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtMethodAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region OverrideMethodAttribute
        /// <summary>
        /// OverrideMethodAttributeValidating method implementation
        /// </summary>
        private void OverrideMethodAttributeValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtOverrideMethodAttribute.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtOverrideMethodAttribute.Text))
                        throw new Exception(res.CTRLADATOVERRIDEERROR);
                    Config.Hosts.ActiveDirectoryHost.overridemethodAttribute = txtOverrideMethodAttribute.Text;
                    errors.SetError(txtOverrideMethodAttribute, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtOverrideMethodAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// OverrideMethodAttributeValidated method implmentation
        /// </summary>
        private void OverrideMethodAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.overridemethodAttribute = txtOverrideMethodAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtOverrideMethodAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region ValidityDateAttribute
        /// <summary>
        /// PinAttributeValidating method implementation
        /// </summary>
        private void PinAttributeValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                  if (txtPinAttribute.Modified)
                  {
                      ManagementService.ADFSManager.SetDirty(true);
                      if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtPinAttribute.Text))
                          throw new Exception(res.CTRLADATPINERROR);
                      Config.Hosts.ActiveDirectoryHost.pinattribute = txtPinAttribute.Text;
                      errors.SetError(txtPinAttribute, "");
                  }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtPinAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// PinAttributeValidated method implmentation
        /// </summary>
        private void PinAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                 Config.Hosts.ActiveDirectoryHost.pinattribute = txtPinAttribute.Text;
                 ManagementService.ADFSManager.SetDirty(true);
                 errors.SetError(txtPinAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        #endregion

        #region CheckDateAttribute
        /// <summary>
        /// CheckDateAttributeValidating method implementation
        /// </summary>
        private void CheckDateAttributeValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
               /* if (txtCheckdateAttribute.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.notifcheckdateattribute = txtCheckdateAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (!ManagementService.CheckRepositoryAttribute(txtCheckdateAttribute.Text, 1))
                {
                    e.Cancel = true;
                    errors.SetError(txtCheckdateAttribute, res.CTRLADATCHECKDATEERROR);
                } */
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtCheckdateAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// CheckDateAttributeValidated method implmentation
        /// </summary>
        private void CheckDateAttributeValidated(object sender, EventArgs e)
        {
            try
            {
               /* Config.Hosts.ActiveDirectoryHost.notifcheckdateattribute = txtCheckdateAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtCheckdateAttribute, ""); */
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region TOTPAttribute
        /// <summary>
        /// TOTPAttributeValidating method implementation
        /// </summary>
        private void TOTPAttributeValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
               /* if (txtTOTPAttribute.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.totpAttribute = txtTOTPAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (!ManagementService.CheckRepositoryAttribute(txtTOTPAttribute.Text, 1))
                {
                    e.Cancel = true;
                    errors.SetError(txtTOTPAttribute, res.CTRLADATTOTPERROR);
                } */
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtTOTPAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// TOTPAttributeValidated method implmentation
        /// </summary>
        private void TOTPAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                /*  Config.Hosts.ActiveDirectoryHost.totpAttribute = txtTOTPAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true); 
                    errors.SetError(txtTOTPAttribute, ""); */
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region EnabledAttribute
        /// <summary>
        /// EnabledAttributeValidating method implementation
        /// </summary>
        private void EnabledAttributeValidating(object sender, CancelEventArgs e)
        {

            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtEnabledAttribute.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckADDSAttribute(txtDomainName.Text, txtUserName.Text, txtPassword.Text, txtEnabledAttribute.Text))
                        throw new Exception(res.CTRLADATENABLEDERROR);
                    Config.Hosts.ActiveDirectoryHost.totpEnabledAttribute = txtEnabledAttribute.Text;
                    errors.SetError(txtEnabledAttribute, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtEnabledAttribute, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// EnabledAttributeValidated method implmentation
        /// </summary>
        private void EnabledAttributeValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.ActiveDirectoryHost.totpEnabledAttribute = txtEnabledAttribute.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtEnabledAttribute, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region MaxRows
        /// <summary>
        /// MaxRowsValidating method
        /// </summary>
        private void MaxRowsValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtMaxRows.Modified)
                {
                    int maxrows = Convert.ToInt32(txtMaxRows.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((maxrows < 1000) || (maxrows > 1000000))
                        throw new ArgumentException(String.Format(res.CTRLSQLMAXROWSERROR, maxrows), "MaxRows");
                    Config.Hosts.ActiveDirectoryHost.MaxRows = maxrows;
                    errors.SetError(txtMaxRows, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtMaxRows, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// MaxRowsValidated method
        /// </summary>
        private void MaxRowsValidated(object sender, EventArgs e)
        {
            try
            {
                int maxrows = Convert.ToInt32(txtMaxRows.Text);
                Config.Hosts.ActiveDirectoryHost.MaxRows = maxrows;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtMaxRows, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        /// <summary>
        /// btnConnectClick method implmentation
        /// </summary>
        private void btnConnectClick(object sender, EventArgs e)
        {
            try
            {
                if (!ManagementService.CheckADDSConnection(txtDomainName.Text, txtUserName.Text, txtPassword.Text))
                {
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                    messageBoxParameters.Text = res.CTRLADCONNECTIONERROR;
                    messageBoxParameters.Buttons = MessageBoxButtons.OK;
                    messageBoxParameters.Icon = MessageBoxIcon.Error;
                    this._snapin.Console.ShowDialog(messageBoxParameters);

                }
                else
                {
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                    messageBoxParameters.Text = res.CTRLADCONNECTIONOK;
                    messageBoxParameters.Buttons = MessageBoxButtons.OK;
                    messageBoxParameters.Icon = MessageBoxIcon.Information;
                    this._snapin.Console.ShowDialog(messageBoxParameters);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }


        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
               _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    public partial class SQLConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private SQLViewControl _view;
        private CheckBox chkUseSQL;
        private TextBox txtConnectionString;
        private TextBox txtMaxRows;
        private bool _UpdateControlsLayouts;
        private Button btnConnect;
        private CheckBox chkUseAlwaysEncryptSQL;
        private TextBox txtEncryptKeyName;
        private NumericUpDown txtCertificateDuration;
        private CheckBox chkReuseCertificate;
        private TextBox txtCertificateThumbPrint;
        private Button btnCreateDB;
        private Button btnCreateCryptedDB;
        private ErrorProvider errors;
        private LinkLabel tblCancelConfig;
        private LinkLabel tblSaveConfig;
        private Label lblCertificateThumbPrint;
        private Label lblCertificateDuration;
        private Label lblEncryptKeyName;
        private Label lblMaxRows;
        private Label lblConnectionString;


        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public SQLConfigurationControl(SQLViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 585;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 362;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 362;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                chkUseSQL = new CheckBox();
                chkUseSQL.Text = res.CTRLSQLUSING;
                chkUseSQL.Checked = !Config.UseActiveDirectory;
                chkUseSQL.Left = 10;
                chkUseSQL.Top = 19;
                chkUseSQL.Width = 450;
                chkUseSQL.CheckedChanged += UseSQLCheckedChanged;
                _txtpanel.Controls.Add(chkUseSQL);

                lblConnectionString = new Label();
                lblConnectionString.Text = res.CTRLSQLCONNECTSTR + " : ";
                lblConnectionString.Left = 50;
                lblConnectionString.Top = 51;
                lblConnectionString.Width = 150;
                _txtpanel.Controls.Add(lblConnectionString);

                txtConnectionString = new TextBox();
                txtConnectionString.Text = Config.Hosts.SQLServerHost.ConnectionString;
                txtConnectionString.Left = 210;
                txtConnectionString.Top = 47;
                txtConnectionString.Width = 700;
                txtConnectionString.Enabled = !Config.UseActiveDirectory;
                txtConnectionString.Validating += ConnectionStringValidating;
                txtConnectionString.Validated += ConnectionStringValidated;
                _txtpanel.Controls.Add(txtConnectionString);

                lblMaxRows = new Label();
                lblMaxRows.Text = res.CTRLSQLMAXROWS + " : ";
                lblMaxRows.Left = 50;
                lblMaxRows.Top = 82;
                lblMaxRows.Width = 150;
                _txtpanel.Controls.Add(lblMaxRows);

                txtMaxRows = new TextBox();
                txtMaxRows.Text = Config.Hosts.SQLServerHost.MaxRows.ToString();
                txtMaxRows.Left = 210;
                txtMaxRows.Top = 78;
                txtMaxRows.Width = 50;
                txtMaxRows.TextAlign = HorizontalAlignment.Right;
                txtMaxRows.Enabled = !Config.UseActiveDirectory;
                txtMaxRows.Validating += MaxRowsValidating;
                txtMaxRows.Validated += MaxRowsValidated;
                _txtpanel.Controls.Add(txtMaxRows);

                btnConnect = new Button();
                btnConnect.Text = res.CTRLSQLTEST;
                btnConnect.Left = 680;
                btnConnect.Top = 82;
                btnConnect.Width = 230;
                btnConnect.Enabled = !Config.UseActiveDirectory;
                btnConnect.Click += btnConnectClick;
                _txtpanel.Controls.Add(btnConnect);

                btnCreateDB = new Button();
                btnCreateDB.Text = res.CTRLSQLCREATEDB;
                btnCreateDB.Left = 680;
                btnCreateDB.Top = 113;
                btnCreateDB.Width = 230;
                btnCreateDB.Enabled = !Config.UseActiveDirectory;
                btnCreateDB.Click += btnCreateDBClick;
                _txtpanel.Controls.Add(btnCreateDB);

                chkUseAlwaysEncryptSQL = new CheckBox();
                chkUseAlwaysEncryptSQL.Text = res.CTRLSQLCRYPTUSING;
                chkUseAlwaysEncryptSQL.Checked = Config.Hosts.SQLServerHost.IsAlwaysEncrypted;
                chkUseAlwaysEncryptSQL.Enabled = !Config.UseActiveDirectory;
                chkUseAlwaysEncryptSQL.Left = 10;
                chkUseAlwaysEncryptSQL.Top = 144;
                chkUseAlwaysEncryptSQL.Width = 450;
                chkUseAlwaysEncryptSQL.CheckedChanged += UseSQLCryptCheckedChanged;
                _txtpanel.Controls.Add(chkUseAlwaysEncryptSQL);

                lblEncryptKeyName = new Label();
                lblEncryptKeyName.Text = res.CTRLSQLENCRYPTNAME + " : ";
                lblEncryptKeyName.Left = 50;
                lblEncryptKeyName.Top = 175;
                lblEncryptKeyName.Width = 150;
                _txtpanel.Controls.Add(lblEncryptKeyName);

                txtEncryptKeyName = new TextBox();
                txtEncryptKeyName.Text = Config.Hosts.SQLServerHost.KeyName;
                txtEncryptKeyName.Left = 210;
                txtEncryptKeyName.Top = 171;
                txtEncryptKeyName.Width = 100;
                txtEncryptKeyName.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);
                txtEncryptKeyName.Validating += EncryptKeyNameValidating;
                txtEncryptKeyName.Validated += EncryptKeyNameValidated;
                _txtpanel.Controls.Add(txtEncryptKeyName);

                lblCertificateDuration = new Label();
                lblCertificateDuration.Text = res.CTRLSECCERTIFDURATION + " : ";
                lblCertificateDuration.Left = 50;
                lblCertificateDuration.Top = 206;
                lblCertificateDuration.Width = 150;
                _txtpanel.Controls.Add(lblCertificateDuration);

                txtCertificateDuration = new NumericUpDown();
                txtCertificateDuration.Left = 210;
                txtCertificateDuration.Top = 202;
                txtCertificateDuration.Width = 50;
                txtCertificateDuration.TextAlign = HorizontalAlignment.Center;
                txtCertificateDuration.Value = Config.Hosts.SQLServerHost.CertificateValidity;
                txtCertificateDuration.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
                txtCertificateDuration.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
                txtCertificateDuration.ValueChanged += CertValidityValueChanged;
                txtCertificateDuration.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);
                _txtpanel.Controls.Add(txtCertificateDuration);

                chkReuseCertificate = new CheckBox();
                chkReuseCertificate.Text = res.CTRLSQLREUSECERT;
                chkReuseCertificate.Checked = Config.Hosts.SQLServerHost.CertReuse;
                chkReuseCertificate.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);
                chkReuseCertificate.Left = 50;
                chkReuseCertificate.Top = 233;
                chkReuseCertificate.Width = 450;
                chkReuseCertificate.CheckedChanged += UseSQLReuseCertCheckedChanged;
                _txtpanel.Controls.Add(chkReuseCertificate);

                lblCertificateThumbPrint = new Label();
                lblCertificateThumbPrint.Text = res.CTRLSQLTHUMBPRINT + " : ";
                lblCertificateThumbPrint.Left = 100;
                lblCertificateThumbPrint.Top = 264;
                lblCertificateThumbPrint.Width = 150;
                _txtpanel.Controls.Add(lblCertificateThumbPrint);

                txtCertificateThumbPrint = new TextBox();
                txtCertificateThumbPrint.Text = Config.Hosts.SQLServerHost.ThumbPrint;
                txtCertificateThumbPrint.Left = 260;
                txtCertificateThumbPrint.Top = 260;
                txtCertificateThumbPrint.Width = 300;
                txtCertificateThumbPrint.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted && Config.Hosts.SQLServerHost.CertReuse);
                txtCertificateThumbPrint.Validating += CertificateThumbPrintValidating;
                txtCertificateThumbPrint.Validated += CertificateThumbPrintValidated;
                _txtpanel.Controls.Add(txtCertificateThumbPrint);

                btnCreateCryptedDB = new Button();
                btnCreateCryptedDB.Text = res.CTRLSQLCREATECRYPTEDDB;
                btnCreateCryptedDB.Left = 680;
                btnCreateCryptedDB.Top = 322;
                btnCreateCryptedDB.Width = 230;
                btnCreateCryptedDB.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);
                btnCreateCryptedDB.Click += btnCreateCryptedDBClick;
                _txtpanel.Controls.Add(btnCreateCryptedDB);

                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 372;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 372;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts(!Config.UseActiveDirectory, Config.Hosts.SQLServerHost.IsAlwaysEncrypted, Config.Hosts.SQLServerHost.CertReuse);
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                chkUseSQL.Checked = !Config.UseActiveDirectory;

                txtConnectionString.Text = Config.Hosts.SQLServerHost.ConnectionString;
                txtConnectionString.Enabled = !Config.UseActiveDirectory;

                txtMaxRows.Text = Config.Hosts.SQLServerHost.MaxRows.ToString();
                txtMaxRows.Enabled = !Config.UseActiveDirectory;

                btnConnect.Enabled = !Config.UseActiveDirectory;

                btnCreateDB.Enabled = !Config.UseActiveDirectory;

                chkUseAlwaysEncryptSQL.Text = res.CTRLSQLCRYPTUSING;
                chkUseAlwaysEncryptSQL.Checked = Config.Hosts.SQLServerHost.IsAlwaysEncrypted;
                chkUseAlwaysEncryptSQL.Enabled = !Config.UseActiveDirectory;

                txtEncryptKeyName.Text = Config.Hosts.SQLServerHost.KeyName;
                txtEncryptKeyName.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);

                txtCertificateDuration.Value = Config.Hosts.SQLServerHost.CertificateValidity;
                txtCertificateDuration.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);

                chkReuseCertificate.Text = res.CTRLSQLREUSECERT;
                chkReuseCertificate.Checked = Config.Hosts.SQLServerHost.CertReuse;
                chkReuseCertificate.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);

                txtCertificateThumbPrint.Text = Config.Hosts.SQLServerHost.ThumbPrint;
                txtCertificateThumbPrint.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted && Config.Hosts.SQLServerHost.CertReuse);

                btnCreateCryptedDB.Enabled = (!Config.UseActiveDirectory && Config.Hosts.SQLServerHost.IsAlwaysEncrypted);
                btnCreateCryptedDB.Text = res.CTRLSQLCREATECRYPTEDDB;

                chkUseSQL.Text = res.CTRLSQLUSING;
                btnConnect.Text = res.CTRLSQLTEST;
                btnCreateDB.Text = res.CTRLSQLCREATEDB;
                lblMaxRows.Text = res.CTRLSQLMAXROWS + " : ";
                lblConnectionString.Text = res.CTRLSQLCONNECTSTR + " : ";
                lblEncryptKeyName.Text = res.CTRLSQLENCRYPTNAME + " : ";
                lblCertificateDuration.Text = res.CTRLSECCERTIFDURATION + " : ";
                lblCertificateThumbPrint.Text = res.CTRLSQLTHUMBPRINT + " : ";
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
            }
            finally
            {
                UpdateControlsLayouts(!Config.UseActiveDirectory, chkUseAlwaysEncryptSQL.Checked, chkReuseCertificate.Checked);
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ValidateData method implmentation
        /// </summary>
        private void ValidateData()
        {
            if (txtConnectionString.Enabled)
            {
                if (!ManagementService.CheckSQLConnection(txtConnectionString.Text))
                    errors.SetError(txtConnectionString, res.CTRLSQLCONNECTSTRERROR);
                else
                    errors.SetError(txtConnectionString, "");
            }
            else
                errors.SetError(txtConnectionString, "");

            if (txtMaxRows.Enabled)
            {
                int maxrows = Convert.ToInt32(txtMaxRows.Text);
                if ((maxrows < 1000) || (maxrows > 1000000))
                    errors.SetError(txtMaxRows, String.Format(res.CTRLSQLMAXROWSERROR, maxrows));
                else
                    errors.SetError(txtMaxRows, "");
            }
            else
                errors.SetError(txtMaxRows, "");
            if (chkUseAlwaysEncryptSQL.Checked)
            {
                if (txtEncryptKeyName.Enabled)
                {
                    if (string.IsNullOrEmpty(txtEncryptKeyName.Text))
                        errors.SetError(txtEncryptKeyName, res.CTRLNULLOREMPTYERROR);
                    else
                        errors.SetError(txtEncryptKeyName, "");
                }
                else
                    errors.SetError(txtEncryptKeyName, "");
                if (chkReuseCertificate.Checked)
                {
                    if (txtCertificateThumbPrint.Enabled)
                    {
                        if (!ManagementService.ADFSManager.CheckCertificate(txtCertificateThumbPrint.Text))
                            errors.SetError(txtCertificateThumbPrint, string.Format(res.CTRLSQLINVALIDCERTERROR, txtCertificateThumbPrint.Text));
                        else
                            errors.SetError(txtCertificateThumbPrint, "");
                    }
                    else
                        errors.SetError(txtCertificateThumbPrint, "");
                }
            }
            else
                errors.SetError(txtCertificateThumbPrint, "");
        }

        /// <summary>
        /// UpdateConnectionString method implmentation
        /// </summary>
        private void UpdateConnectionString(bool crypted)
        {
            if (_view.AutoValidate != AutoValidate.Disable)
            {
                string cs = txtConnectionString.Text;
                if (!crypted)
                {
                    cs = Regex.Replace(cs, ";column encryption setting=enabled", "", RegexOptions.IgnoreCase);
                    cs = Regex.Replace(cs, ";column encryption setting=disabled", "", RegexOptions.IgnoreCase);
                }
                else
                {
                    cs = Regex.Replace(cs, ";column encryption setting=enabled", "", RegexOptions.IgnoreCase);
                    cs = Regex.Replace(cs, ";column encryption setting=disabled", "", RegexOptions.IgnoreCase);
                    cs += ";Column Encryption Setting=enabled";
                }
                txtConnectionString.Text = cs;
            }
        }


        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts(bool isenabled, bool iscrypted, bool reusecert)
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
                txtConnectionString.Enabled = isenabled;
                txtMaxRows.Enabled = isenabled;
                btnConnect.Enabled = isenabled;
                btnCreateDB.Enabled = isenabled;
                txtCertificateDuration.Enabled = isenabled && iscrypted;
                txtEncryptKeyName.Enabled = isenabled && iscrypted;
                txtCertificateThumbPrint.Enabled = isenabled && iscrypted && reusecert;
                chkUseAlwaysEncryptSQL.Enabled = isenabled;
                chkReuseCertificate.Enabled = isenabled && iscrypted;
                btnCreateCryptedDB.Enabled = isenabled && iscrypted;
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// UseSQLCheckedChanged method implementation
        /// </summary>
        private void UseSQLCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.UseActiveDirectory = !chkUseSQL.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts(!Config.UseActiveDirectory, chkUseAlwaysEncryptSQL.Checked, chkReuseCertificate.Checked);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// ConnectionStringValidating method
        /// </summary>
        private void ConnectionStringValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtConnectionString.Modified) && (txtConnectionString.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckSQLConnection(txtConnectionString.Text))
                        throw new Exception(res.CTRLSQLCONNECTSTRERROR);
                    Config.Hosts.SQLServerHost.ConnectionString = txtConnectionString.Text;
                    errors.SetError(txtConnectionString, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtConnectionString, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// ConnectionStringValidated method
        /// </summary>
        private void ConnectionStringValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.SQLServerHost.ConnectionString = txtConnectionString.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtConnectionString, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// MaxRowsValidating method
        /// </summary>
        private void MaxRowsValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtMaxRows.Modified) && (txtMaxRows.Enabled))
                {
                    int maxrows = Convert.ToInt32(txtMaxRows.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((maxrows < 1000) || (maxrows > 1000000))
                        throw new Exception(String.Format(res.CTRLSQLMAXROWSERROR, maxrows));
                    Config.Hosts.SQLServerHost.MaxRows = maxrows;
                    errors.SetError(txtMaxRows, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtMaxRows, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// MaxRowsValidated method
        /// </summary>
        private void MaxRowsValidated(object sender, EventArgs e)
        {
            try
            {
                int maxrows = Convert.ToInt32(txtMaxRows.Text);
                Config.Hosts.SQLServerHost.MaxRows = maxrows;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtMaxRows, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// btnConnectClick method implmentation
        /// </summary>
        private void btnConnectClick(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (!ManagementService.CheckSQLConnection(txtConnectionString.Text))
                {
                    this.Cursor = Cursors.Default;
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                    messageBoxParameters.Text = res.CTRLSQLCONNECTERROR;
                    messageBoxParameters.Buttons = MessageBoxButtons.OK;
                    messageBoxParameters.Icon = MessageBoxIcon.Error;
                    this._snapin.Console.ShowDialog(messageBoxParameters);

                }
                else
                {
                    this.Cursor = Cursors.Default;
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                    messageBoxParameters.Text = res.CTRLSQLCONNECTOK;
                    messageBoxParameters.Buttons = MessageBoxButtons.OK;
                    messageBoxParameters.Icon = MessageBoxIcon.Information;
                    this._snapin.Console.ShowDialog(messageBoxParameters);
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// btnConnectClick method implmentation
        /// </summary>
        private void btnCreateDBClick(object sender, EventArgs e)
        {
            DatabaseWizard Wizard = new DatabaseWizard();
            try
            {
                bool result = (this._snapin.Console.ShowDialog(Wizard) == DialogResult.OK);
                Cursor crs = this.Cursor;
                try
                {
                    this.Cursor = Cursors.WaitCursor; 
                    if (result)
                    {
                        this.txtConnectionString.Text = ManagementService.ADFSManager.CreateMFADatabase(null, Wizard.txtInstance.Text, Wizard.txtDBName.Text, Wizard.txtAccount.Text, Wizard.txtPwd.Text);
                        MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                        messageBoxParameters.Text = string.Format(res.CTRLSQLNEWDBCREATED, Wizard.txtDBName.Text);
                        messageBoxParameters.Buttons = MessageBoxButtons.OK;
                        messageBoxParameters.Icon = MessageBoxIcon.Information;
                        this._snapin.Console.ShowDialog(messageBoxParameters);
                    }
                }
                finally
                {
                    this.Cursor = crs;
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default; 
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// btnCreateCryptedDBClick method implmentation
        /// </summary>
        private void btnCreateCryptedDBClick(object sender, EventArgs e)
        {
            DatabaseWizard Wizard = new DatabaseWizard();
            try
            {
                if (Config.Hosts.SQLServerHost.IsAlwaysEncrypted)
                {
                    bool result = (this._snapin.Console.ShowDialog(Wizard) == DialogResult.OK);
                    Cursor crs = this.Cursor;
                    try
                    {
                        this.Cursor = Cursors.WaitCursor; 
                        if (result)
                        {
                            bool isnew = false;
                            string thumb = string.Empty;
                            if ((Config.Hosts.SQLServerHost.CertReuse) && (Certs.GetCertificate(Config.Hosts.SQLServerHost.ThumbPrint.ToUpper(), StoreLocation.LocalMachine)) != null)
                                thumb = Config.Hosts.SQLServerHost.ThumbPrint.ToUpper();
                            else
                            { 
                                thumb = ManagementService.ADFSManager.RegisterNewSQLCertificate(null, Config.Hosts.SQLServerHost.CertificateValidity, Config.Hosts.SQLServerHost.KeyName);
                                isnew = true;
                            }
                            this.txtConnectionString.Text = ManagementService.ADFSManager.CreateMFAEncryptedDatabase(null, Wizard.txtInstance.Text, Wizard.txtDBName.Text, Wizard.txtAccount.Text, Wizard.txtPwd.Text, Config.Hosts.SQLServerHost.KeyName, thumb);
                            this.Cursor = Cursors.Default; 
                            MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                            if (isnew)
                                messageBoxParameters.Text = string.Format(res.CTRLSQLNEWDBCREATED2, Wizard.txtDBName.Text, thumb);
                            else
                                messageBoxParameters.Text = string.Format(res.CTRLSQLNEWDBCREATED, Wizard.txtDBName.Text);
                            messageBoxParameters.Buttons = MessageBoxButtons.OK;
                            messageBoxParameters.Icon = MessageBoxIcon.Information;
                            this._snapin.Console.ShowDialog(messageBoxParameters);
                        }
                    }
                    finally
                    {
                        this.Cursor = crs;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default; 
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// UseSQLCryptCheckedChanged method implementation
        /// </summary>
        private void UseSQLCryptCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.Hosts.SQLServerHost.IsAlwaysEncrypted = chkUseAlwaysEncryptSQL.Checked;
                    UpdateConnectionString(chkUseAlwaysEncryptSQL.Checked);
                    UpdateControlsLayouts(!Config.UseActiveDirectory, chkUseAlwaysEncryptSQL.Checked, chkReuseCertificate.Checked);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// UseSQLReuseCertCheckedChanged method implementaton
        /// </summary>
        private void UseSQLReuseCertCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    this.Config.Hosts.SQLServerHost.CertReuse = chkReuseCertificate.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts(!Config.UseActiveDirectory, Config.Hosts.SQLServerHost.IsAlwaysEncrypted, Config.Hosts.SQLServerHost.CertReuse);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// CertValidityValueChanged method implementation
        /// </summary>
        private void CertValidityValueChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                    ManagementService.ADFSManager.SetDirty(true);
            }
            catch (Exception ex)
            {
                errors.SetError(txtCertificateDuration, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// EncryptKeyNameValidating method implmentation
        /// </summary>
        private void EncryptKeyNameValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtEncryptKeyName.Modified) && (txtEncryptKeyName.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtEncryptKeyName.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.Hosts.SQLServerHost.KeyName = txtEncryptKeyName.Text;
                    errors.SetError(txtEncryptKeyName, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtEncryptKeyName, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// EncryptKeyNameValidated method implementation
        /// </summary>
        private void EncryptKeyNameValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.SQLServerHost.KeyName = txtEncryptKeyName.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtEncryptKeyName, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// CertificateThumbPrintValidating method implementation
        /// </summary>
        private void CertificateThumbPrintValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtCertificateThumbPrint.Modified) && (txtCertificateThumbPrint.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((chkUseAlwaysEncryptSQL.Checked) && (chkReuseCertificate.Checked))
                    {
                        if (!ManagementService.ADFSManager.CheckCertificate(txtCertificateThumbPrint.Text))
                            throw new Exception(string.Format(res.CTRLSQLINVALIDCERTERROR, txtCertificateThumbPrint.Text));
                    }
                    Config.Hosts.SQLServerHost.ThumbPrint = txtCertificateThumbPrint.Text.ToUpper();
                    errors.SetError(txtCertificateThumbPrint, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtCertificateThumbPrint, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// CertificateThumbPrintValidated method implementation
        /// </summary>
        private void CertificateThumbPrintValidated(object sender, EventArgs e)
        {
            try
            {
                Config.Hosts.SQLServerHost.ThumbPrint = txtCertificateThumbPrint.Text.ToUpper();
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtCertificateThumbPrint, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
               _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }   
    }

    public partial class SMTPConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private SMTPViewControl _view;
        private TextBox txtCompany;
        private ErrorProvider errors;
        private TextBox txtFrom;
        private TextBox txtHost;
        private TextBox txtPort;
        private CheckBox chkUseSSL;
        private TextBox txtAccount;
        private TextBox txtPassword;
        private CheckBox chkAnonymous;
        private Button btnConnect;
        private LinkLabel tblCancelConfig;
        private LinkLabel tblSaveConfig;
        private Label lblPassword;
        private Label lblAccount;
        private Label lblidentify;
        private Label lblPort;
        private Label lblHost;
        private Label lblServer;
        private Label lblFrom;
        private Label lblCompany;


        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public SMTPConfigurationControl(SMTPViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 585;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 351;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 351;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                lblCompany = new Label();
                lblCompany.Text = res.CTRLSMTPCOMPANY + " : ";
                lblCompany.Left = 10;
                lblCompany.Top = 19;
                lblCompany.Width = 200;
                _txtpanel.Controls.Add(lblCompany);

                txtCompany = new TextBox();
                txtCompany.Text = Config.MailProvider.Company;
                txtCompany.Left = 210;
                txtCompany.Top = 15;
                txtCompany.Width = 250;
                txtCompany.Validating += CompanyValidating;
                txtCompany.Validated += CompanyValidated;
                _txtpanel.Controls.Add(txtCompany);

                lblFrom = new Label();
                lblFrom.Text = res.CTRLSMTPFROM + " : ";
                lblFrom.Left = 10;
                lblFrom.Top = 51;
                lblFrom.Width = 200;
                _txtpanel.Controls.Add(lblFrom);

                txtFrom = new TextBox();
                txtFrom.Text = Config.MailProvider.From;
                txtFrom.Left = 210;
                txtFrom.Top = 47;
                txtFrom.Width = 250;
                txtFrom.Validating += FromValidating;
                txtFrom.Validated += FromValidated;
                _txtpanel.Controls.Add(txtFrom);

                lblServer = new Label();
                lblServer.Text = res.CTRLSMTPSERVER;
                lblServer.Left = 10;
                lblServer.Top = 95;
                lblServer.Width = 180;
                _txtpanel.Controls.Add(lblServer);

                lblHost = new Label();
                lblHost.Text = res.CTRLSMTPSERVERADDRESS + " : ";
                lblHost.Left = 30;
                lblHost.Top = 127;
                lblHost.Width = 180;
                _txtpanel.Controls.Add(lblHost);

                txtHost = new TextBox();
                txtHost.Text = Config.MailProvider.Host;
                txtHost.Left = 210;
                txtHost.Top = 123;
                txtHost.Width = 250;
                txtHost.Validating += HostValidating;
                txtHost.Validated += HostValidated;
                _txtpanel.Controls.Add(txtHost);

                lblPort = new Label();
                lblPort.Text = res.CTRLSMTPPORT + " : ";
                lblPort.Left = 480;
                lblPort.Top = 127;
                lblPort.Width = 40;
                _txtpanel.Controls.Add(lblPort);

                txtPort = new TextBox();
                txtPort.Text = Config.MailProvider.Port.ToString();
                txtPort.Left = 520;
                txtPort.Top = 123;
                txtPort.Width = 40;
                txtPort.TextAlign = HorizontalAlignment.Center;
                txtPort.Validating += PortValidating;
                txtPort.Validated += PortValidated;
                _txtpanel.Controls.Add(txtPort);

                chkUseSSL = new CheckBox();
                chkUseSSL.Text = "SSL";
                chkUseSSL.Checked = Config.MailProvider.UseSSL;
                chkUseSSL.Left = 590;
                chkUseSSL.Top = 123;
                chkUseSSL.Width = 100;
                chkUseSSL.CheckedChanged += SSLChecked;
                _txtpanel.Controls.Add(chkUseSSL);

                lblidentify = new Label();
                lblidentify.Text = res.CTRLSMTPIDENTIFICATION;
                lblidentify.Left = 10;
                lblidentify.Top = 170;
                lblidentify.Width = 180;
                _txtpanel.Controls.Add(lblidentify);

                lblAccount = new Label();
                lblAccount.Text = res.CTRLSMTPACCOUNT + " : ";
                lblAccount.Left = 30;
                lblAccount.Top = 202;
                lblAccount.Width = 180;
                _txtpanel.Controls.Add(lblAccount);

                txtAccount = new TextBox();
                txtAccount.Text = Config.MailProvider.UserName;
                txtAccount.Left = 210;
                txtAccount.Top = 200;
                txtAccount.Width = 250;
                txtAccount.Enabled = !Config.MailProvider.Anonymous;
                txtAccount.Validating += UserNameValidating;
                txtAccount.Validated += UserNameValidated;
                _txtpanel.Controls.Add(txtAccount);

                lblPassword = new Label();
                lblPassword.Text = res.CTRLSMTPPASSWORD + " : ";
                lblPassword.Left = 30;
                lblPassword.Top = 234;
                lblPassword.Width = 180;
                _txtpanel.Controls.Add(lblPassword);

                txtPassword = new TextBox();
                txtPassword.Text = Config.MailProvider.Password;
                txtPassword.Left = 210;
                txtPassword.Top = 232;
                txtPassword.Width = 250;
                txtPassword.Enabled = !Config.MailProvider.Anonymous;
                txtPassword.PasswordChar = '*';
                txtPassword.Validating += PwdValidating;
                txtPassword.Validated += PwdValidated;
                _txtpanel.Controls.Add(txtPassword);

                chkAnonymous = new CheckBox();
                chkAnonymous.Text = res.CTRLSMTPANONYMOUS;
                chkAnonymous.Checked = Config.MailProvider.Anonymous;
                chkAnonymous.Left = 480;
                chkAnonymous.Top = 232;
                chkAnonymous.Width = 150;
                chkAnonymous.CheckedChanged += AnonymousChecked;
                _txtpanel.Controls.Add(chkAnonymous);

                btnConnect = new Button();
                btnConnect.Text = res.CTRLSMTPTEST;
                btnConnect.Left = 480;
                btnConnect.Top = 270;
                btnConnect.Width = 150;
                btnConnect.Click += btnConnectClick;
                _txtpanel.Controls.Add(btnConnect);

                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 361;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 361;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                txtCompany.Text = Config.MailProvider.Company;

                txtFrom.Text = Config.MailProvider.From;

                txtHost.Text = Config.MailProvider.Host;

                txtPort.Text = Config.MailProvider.Port.ToString();

                chkUseSSL.CheckedChanged += SSLChecked;

                txtAccount.Text = Config.MailProvider.UserName;

                txtAccount.Enabled = !Config.MailProvider.Anonymous;

                txtPassword.Text = Config.MailProvider.Password;

                txtPassword.Enabled = !Config.MailProvider.Anonymous;

                chkAnonymous.Checked = Config.MailProvider.Anonymous;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                btnConnect.Text = res.CTRLSMTPTEST;
                chkAnonymous.Text = res.CTRLSMTPANONYMOUS;
                lblPassword.Text = res.CTRLSMTPPASSWORD + " : ";
                lblAccount.Text = res.CTRLSMTPACCOUNT + " : ";
                lblidentify.Text = res.CTRLSMTPIDENTIFICATION;
                lblPort.Text = res.CTRLSMTPPORT + " : ";
                lblHost.Text = res.CTRLSMTPSERVERADDRESS + " : ";
                lblServer.Text = res.CTRLSMTPSERVER;
                lblFrom.Text = res.CTRLSMTPFROM + " : ";
                lblCompany.Text = res.CTRLSMTPCOMPANY + " : ";
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ValidateData method implmentation
        /// </summary>
        private void ValidateData()
        {
            if (string.IsNullOrEmpty(txtCompany.Text))
                errors.SetError(txtCompany, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtCompany, "");

            if (string.IsNullOrEmpty(txtFrom.Text))
                errors.SetError(txtFrom, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtFrom, "");

            if (string.IsNullOrEmpty(txtHost.Text))
                errors.SetError(txtHost, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtHost, "");

            int port = Convert.ToInt32(txtPort.Text);
            if ((port <= UInt16.MinValue) || (port >= UInt16.MaxValue))
                errors.SetError(txtPort, res.CTRLSMTPPORTERROR);
            else
                errors.SetError(txtPort, "");

            if (string.IsNullOrEmpty(txtAccount.Text))
                errors.SetError(txtAccount, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtAccount, "");

            if (string.IsNullOrEmpty(txtPassword.Text))
                errors.SetError(txtPassword, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtPassword, "");
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        #region Company
        /// <summary>
        /// CompanyValidating method
        /// </summary>
        private void CompanyValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtCompany.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtCompany.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.MailProvider.Company = txtCompany.Text;
                    errors.SetError(txtCompany, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtCompany, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// CompanyValidated method
        /// </summary>
        private void CompanyValidated(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.Company = txtCompany.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtCompany, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region From
        /// <summary>
        /// FromValidating method implementation
        /// </summary>
        private void FromValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtFrom.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!MMCService.IsValidEmail(txtFrom.Text))
                        throw new Exception(res.CTRLSMTPMAILERROR);
                    Config.MailProvider.From = txtFrom.Text;
                    errors.SetError(txtFrom, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtFrom, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// FromValidated method implmentation
        /// </summary>
        private void FromValidated(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.From = txtFrom.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtFrom, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Host
        /// <summary>
        /// HostValidating method implementation
        /// </summary>
        private void HostValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtHost.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtHost.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.MailProvider.Host = txtHost.Text;
                    errors.SetError(txtHost, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtHost, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// HostValidated method implementation
        /// </summary>
        private void HostValidated(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.Host = txtHost.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtHost, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Port
        /// <summary>
        /// PortValidating method implementation
        /// </summary>
        private void PortValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtPort.Modified)
                {
                    int v = Convert.ToInt32(txtPort.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((v <= UInt16.MinValue) || (v >= UInt16.MaxValue))
                        throw new Exception(res.CTRLSMTPPORTERROR);
                    Config.MailProvider.Port = v;
                    errors.SetError(txtPort, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtPort, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// PortValidated method implementation
        /// </summary>
        private void PortValidated(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.Port = Convert.ToInt32(txtPort.Text);
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtPort, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region SSL
        /// <summary>
        /// SSLChecked method implementation
        /// </summary>
        private void SSLChecked(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.MailProvider.UseSSL = chkUseSSL.Checked;
                    errors.SetError(chkUseSSL, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkUseSSL, ex.Message);
            }
        }
        #endregion

        #region UserName
        /// <summary>
        /// UserNameValidating method implmentation
        /// </summary>
        private void UserNameValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtAccount.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtAccount.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.MailProvider.UserName = txtAccount.Text;
                    errors.SetError(txtAccount, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtAccount, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// UserNameValidated method implmentation
        /// </summary>
        private void UserNameValidated(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.UserName = txtAccount.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtAccount, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Password
        /// <summary>
        /// PwdValidating method implementation
        /// </summary>
        private void PwdValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtPassword.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtPassword.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.MailProvider.Password = txtPassword.Text;
                    errors.SetError(txtPassword, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtPassword, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// PwdValidated method implementation
        /// </summary>
        private void PwdValidated(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.Password = txtPassword.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtPassword, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Anonymous
        /// <summary>
        /// AnonymousChecked method implementation
        /// </summary>
        private void AnonymousChecked(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.MailProvider.Anonymous = chkAnonymous.Checked;
                    txtAccount.Enabled = !chkAnonymous.Checked;
                    txtPassword.Enabled = !chkAnonymous.Checked;
                    errors.SetError(chkAnonymous, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkAnonymous, ex.Message);
            }
        }
        #endregion

        /// <summary>
        /// btnConnectClick method
        /// </summary>
        private void btnConnectClick(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                MailProvider mail = Config.MailProvider;
                MailMessage Message = new MailMessage(mail.From, mail.From);
                Message.BodyEncoding = UTF8Encoding.UTF8;
                Message.Subject = "MFA SMTP¨Test";
                Message.IsBodyHtml = false;
                Message.Body = string.Format("Send mail test");
                Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

                SmtpClient client = new SmtpClient();
                client.Host = mail.Host;
                client.Port = mail.Port;
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = mail.UseSSL;
                if (!mail.Anonymous)
                    client.Credentials = new NetworkCredential(mail.UserName, mail.Password);
                client.Send(Message);

                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = res.CTRLSMTPMESSAGEOK+" "+mail.From;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Information;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    public partial class SMSConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private SMSViewControl _view;
        private bool _UpdateControlsLayouts;
        private ErrorProvider errors;
        private TextBox txtCompany;
        private CheckBox chkIsTwoWay;
        private TextBox txtTimeout;
        private TextBox txtSHA1;
        private TextBox txtDLL;
        private TextBox txtParams;
        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;
        private Label lblParams;
        private Label lblDLL;
        private Label lblSha1;
        private Label lblTimeout;
        private Label lblCompany;

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public SMSConfigurationControl(SMSViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 585;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 261;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 261;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                lblCompany = new Label();
                lblCompany.Text = res.CTRLSMSCOMPANY + " : ";
                lblCompany.Left = 10;
                lblCompany.Top = 19;
                lblCompany.Width = 170;
                _txtpanel.Controls.Add(lblCompany);

                txtCompany = new TextBox();
                txtCompany.Text = Config.ExternalProvider.Company;
                txtCompany.Left = 190;
                txtCompany.Top = 15;
                txtCompany.Width = 250;
                txtCompany.Validating += CompanyValidating;
                txtCompany.Validated += CompanyValidated;
                _txtpanel.Controls.Add(txtCompany);

                chkIsTwoWay = new CheckBox();
                chkIsTwoWay.Text = res.CTRLSMSASYNCCALL;
                chkIsTwoWay.Left = 10;
                chkIsTwoWay.Top = 47;
                chkIsTwoWay.Width = 160;
                chkIsTwoWay.Checked = Config.ExternalProvider.IsTwoWay;
                chkIsTwoWay.CheckedChanged += chkIsTwoWayChanged;
                _txtpanel.Controls.Add(chkIsTwoWay);

                lblTimeout = new Label();
                lblTimeout.Text = res.CTRLSMSTIMEOUT + " : ";
                lblTimeout.Left = 190;
                lblTimeout.Top = 51;
                lblTimeout.Width = 60;
                _txtpanel.Controls.Add(lblTimeout);

                txtTimeout = new TextBox();
                txtTimeout.Text = Config.ExternalProvider.Timeout.ToString();
                txtTimeout.Left = 260;
                txtTimeout.Top = 47;
                txtTimeout.Width = 50;
                txtTimeout.TextAlign = HorizontalAlignment.Center;
                txtTimeout.Validating += TimeOutValidating;
                txtTimeout.Validated += TimeOutValidated;
                _txtpanel.Controls.Add(txtTimeout);

                lblSha1 = new Label();
                lblSha1.Text = res.CTRLSMSSHA1 + " : ";
                lblSha1.Left = 370;
                lblSha1.Top = 51;
                lblSha1.Width = 110;
                _txtpanel.Controls.Add(lblSha1);

                txtSHA1 = new TextBox();
                txtSHA1.Text = Config.ExternalProvider.Sha1Salt;
                txtSHA1.Left = 490;
                txtSHA1.Top = 47;
                txtSHA1.Width = 120;
                txtSHA1.Validating += SHA1Validating;
                txtSHA1.Validated += SHA1Validated;
                _txtpanel.Controls.Add(txtSHA1);

                lblDLL = new Label();
                lblDLL.Text = res.CTRLSMSASSEMBLY + " : ";
                lblDLL.Left = 10;
                lblDLL.Top = 82;
                lblDLL.Width = 170;
                _txtpanel.Controls.Add(lblDLL);

                txtDLL = new TextBox();
                txtDLL.Text = Config.ExternalProvider.FullQualifiedImplementation;
                txtDLL.Left = 190;
                txtDLL.Top = 78;
                txtDLL.Width = 820;
                txtDLL.Validating += DLLValidating;
                txtDLL.Validated += DLLValidated;
                _txtpanel.Controls.Add(txtDLL);

                lblParams = new Label();
                lblParams.Text = res.CTRLSMSPARAMS + " : ";
                lblParams.Left = 10;
                lblParams.Top = 114;
                lblParams.Width = 170;
                _txtpanel.Controls.Add(lblParams);

                txtParams = new TextBox();
                txtParams.Text = Config.ExternalProvider.Parameters.Data;
                txtParams.Left = 190;
                txtParams.Top = 114;
                txtParams.Width = 820;
                txtParams.Height = 100;
                txtParams.Multiline = true;
                txtParams.Validating += ParamsValidating;
                txtParams.Validated += ParamsValidated;
                _txtpanel.Controls.Add(txtParams);

                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 271;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 271;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts(Config.ExternalProvider.Enabled);
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                txtCompany.Text = Config.ExternalProvider.Company;

                chkIsTwoWay.Checked = Config.ExternalProvider.IsTwoWay;

                txtTimeout.Text = Config.ExternalProvider.Timeout.ToString();

                txtSHA1.Text = Config.ExternalProvider.Sha1Salt;

                txtDLL.Text = Config.ExternalProvider.FullQualifiedImplementation;

                txtParams.Text = Config.ExternalProvider.Parameters.Data;

                lblCompany.Text = res.CTRLSMSCOMPANY + " : ";
                chkIsTwoWay.Text = res.CTRLSMSASYNCCALL;
                lblTimeout.Text = res.CTRLSMSTIMEOUT + " : ";
                lblSha1.Text = res.CTRLSMSSHA1 + " : ";
                lblDLL.Text = res.CTRLSMSASSEMBLY + " : ";
                lblParams.Text = res.CTRLSMSPARAMS + " : ";
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts(Config.ExternalProvider.Enabled);
                ValidateData();
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ValidateData method implmentation
        /// </summary>
        private void ValidateData()
        {
            if (string.IsNullOrEmpty(txtCompany.Text))
                errors.SetError(txtCompany, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtCompany, "");

            int timeout = Convert.ToInt32(txtTimeout.Text);
            if ((timeout <= 0) || (timeout > 1000))
                errors.SetError(txtTimeout, string.Format(res.CTRLINVALIDVALUE, "1", "1000"));
            else
                errors.SetError(txtTimeout, "");

            if (string.IsNullOrEmpty(txtSHA1.Text))
                errors.SetError(txtSHA1, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtSHA1, "");

            if (string.IsNullOrEmpty(txtDLL.Text))
                errors.SetError(txtDLL, res.CTRLNULLOREMPTYERROR);
            else if (!AssemblyParser.CheckSMSAssembly(txtDLL.Text))
                errors.SetError(txtDLL, res.CTRLSMSIVALIDEXTERROR);
            else
                errors.SetError(txtDLL, "");

            if (string.IsNullOrEmpty(txtParams.Text))
                errors.SetError(txtParams, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtParams, "");
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts(bool isenabled)
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
                txtCompany.Enabled = isenabled;
                txtTimeout.Enabled = isenabled;
                txtSHA1.Enabled = isenabled;
                txtDLL.Enabled = isenabled;
                txtParams.Enabled = isenabled;
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }

        /// <summary>
        /// chkIsTwoWayChanged method implementation
        /// </summary>
        private void chkIsTwoWayChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.ExternalProvider.IsTwoWay = chkIsTwoWay.Checked;
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkIsTwoWay, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #region Company
        /// <summary>
        /// Company method
        /// </summary>
        private void CompanyValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtCompany.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtCompany.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.ExternalProvider.Company = txtCompany.Text;
                    errors.SetError(txtCompany, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtCompany, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// CompanyValidated method
        /// </summary>
        private void CompanyValidated(object sender, EventArgs e)
        {
            try
            {
                Config.ExternalProvider.Company = txtCompany.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtCompany, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region TimeOut
        /// <summary>
        /// TimeOutValidating event
        /// </summary>
        private void TimeOutValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtTimeout.Modified)
                {
                    int v =  Convert.ToInt32(txtTimeout.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((v <= 0) || (v > 1000))
                        throw new Exception(string.Format(res.CTRLINVALIDVALUE, "1", "1000"));
                    Config.ExternalProvider.Timeout = v;
                    errors.SetError(txtTimeout, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtTimeout, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// TimeOutValidated method
        /// </summary>
        private void TimeOutValidated(object sender, EventArgs e)
        {
            try
            {
                Config.ExternalProvider.Timeout = Convert.ToInt32(txtTimeout.Text);
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtTimeout, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region SHA1Salt
        /// <summary>
        /// SHA1Validating event
        /// </summary>
        private void SHA1Validating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtSHA1.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtSHA1.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.ExternalProvider.Sha1Salt = txtSHA1.Text;
                    errors.SetError(txtSHA1, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtSHA1, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// SHA1Validated event
        /// </summary>
        private void SHA1Validated(object sender, EventArgs e)
        {
            try
            {
                Config.ExternalProvider.Sha1Salt = txtSHA1.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtSHA1, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region DLL
        /// <summary>
        /// DLLValidating event
        /// </summary>
        private void DLLValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtDLL.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtDLL.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if (!AssemblyParser.CheckSMSAssembly(txtDLL.Text))
                        throw new Exception(res.CTRLSMSIVALIDEXTERROR);
                    Config.ExternalProvider.FullQualifiedImplementation = txtDLL.Text;
                    errors.SetError(txtDLL, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtDLL, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// DLLValidated event
        /// </summary>
        private void DLLValidated(object sender, EventArgs e)
        {
            try
            {
                Config.ExternalProvider.FullQualifiedImplementation = txtDLL.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtDLL, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Params
        /// <summary>
        /// ParamsValidating event
        /// </summary>
        private void ParamsValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtParams.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtParams.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.ExternalProvider.Parameters.Data = txtParams.Text;
                    errors.SetError(txtParams, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtParams, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// ParamsValidated event
        /// </summary>
        private void ParamsValidated(object sender, EventArgs e)
        {
            try
            {
                Config.ExternalProvider.Parameters.Data = txtParams.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtParams, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    public partial class AzureConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private AzureViewControl _view;
        private bool _UpdateControlsLayouts;
        private ErrorProvider errors;
        private TextBox txtTenantId;
        private TextBox txtThumbprint;
        private LinkLabel tblCancelConfig;
        private LinkLabel tblSaveConfig;
        private Label lblThumbPrint;
        private Label lblTenantID;

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public AzureConfigurationControl(AzureViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 120;
                this.Width = 512;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 85;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 85;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                lblTenantID = new Label();
                lblTenantID.Text = res.CTRLAZURETENANTID + " : ";
                lblTenantID.Left = 10;
                lblTenantID.Top = 19;
                lblTenantID.Width = 160;
                _txtpanel.Controls.Add(lblTenantID);

                txtTenantId = new TextBox();
                txtTenantId.Text = Config.AzureProvider.TenantId;
                txtTenantId.Left = 180;
                txtTenantId.Top = 15;
                txtTenantId.Width = 300;
                txtTenantId.Validating += TenantidValidating;
                txtTenantId.Validated += TenantidValidated;
                _txtpanel.Controls.Add(txtTenantId);

                lblThumbPrint = new Label();
                lblThumbPrint.Text = res.CTRLAZURETHUMPRINT + " : ";
                lblThumbPrint.Left = 10;
                lblThumbPrint.Top = 51;
                lblThumbPrint.Width = 160;
                _txtpanel.Controls.Add(lblThumbPrint);

                txtThumbprint = new TextBox();
                txtThumbprint.Text = Config.AzureProvider.ThumbPrint;
                txtThumbprint.Left = 180;
                txtThumbprint.Top = 47;
                txtThumbprint.Width = 300;
                txtThumbprint.Validating += ThumbprintValidating;
                txtThumbprint.Validated += ThumbprintValidated;
                _txtpanel.Controls.Add(txtThumbprint);

                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 95;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 95;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implementation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                txtTenantId.Text = Config.AzureProvider.TenantId;
                txtThumbprint.Text = Config.AzureProvider.ThumbPrint;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                lblThumbPrint.Text = res.CTRLAZURETHUMPRINT + " : ";
                lblTenantID.Text = res.CTRLAZURETENANTID + " : ";

            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                ValidateData();
                _view.CausesValidation = true;
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// ValidateData
        /// </summary>
        private void ValidateData()
        {
            if (string.IsNullOrEmpty(txtTenantId.Text))
                errors.SetError(txtTenantId, res.CTRLNULLOREMPTYERROR);
            else
                errors.SetError(txtTenantId, "");

            if ((string.IsNullOrEmpty(txtThumbprint.Text)))
                errors.SetError(txtThumbprint, res.CTRLNULLOREMPTYERROR);
            else if (Certs.GetCertificate(txtThumbprint.Text, StoreLocation.LocalMachine) == null)
                errors.SetError(txtThumbprint, string.Format(res.CTRLSQLINVALIDCERTERROR, txtThumbprint.Text));
            else
                errors.SetError(txtThumbprint, "");
        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts(bool isenabled)
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
                txtTenantId.Enabled = isenabled;
                txtThumbprint.Enabled = isenabled;
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }

        #region Tenantid
        /// <summary>
        /// TenantidValidating method
        /// </summary>
        private void TenantidValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtTenantId.Modified)
                {
                   
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtTenantId.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.AzureProvider.TenantId = txtTenantId.Text;
                    errors.SetError(txtTenantId, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtTenantId, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// TenantidValidated method
        /// </summary>
        private void TenantidValidated(object sender, EventArgs e)
        {
            try
            {
                Config.AzureProvider.TenantId = txtTenantId.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtTenantId, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region Thumbprint
        /// <summary>
        /// ThumbprintValidating event
        /// </summary>
        private void ThumbprintValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtThumbprint.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((string.IsNullOrEmpty(txtThumbprint.Text)))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    Config.AzureProvider.ThumbPrint = txtThumbprint.Text;
                    errors.SetError(txtThumbprint, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtThumbprint, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// ThumbprintValidated method
        /// </summary>
        private void ThumbprintValidated(object sender, EventArgs e)
        {
            try
            {
                Config.AzureProvider.ThumbPrint = txtThumbprint.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtThumbprint, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    public partial class SecurityConfigurationControl : Panel, IMMCRefreshData
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private ServiceSecurityViewControl _view;
        private bool _UpdateControlsLayouts;
        private ErrorProvider errors;

        private TextBox txtTOTPShadows;
        private TextBox txtHashAlgo;
        private ComboBox cbFormat;
        private ComboBox cbKeySize;
        private Panel _panelRNG;
        private ComboBox cbKeyMode;
        private Panel _panelCERT;
        private NumericUpDown txtCERTDuration;
        private Panel _panelRSA;
        private TextBox txtRSAThumb;
        private Button btnRSACert;
        private Panel _panelCUSTOM;
        private TextBox txtDLLCUST;
        private TextBox txtParams;
        private Button btnCUSTOMDB;
        private Panel _panelWiz;
        private CheckBox chkAllowMicrosoft;
        private CheckBox chkAllowGoogle;
        private CheckBox chkAllowAuthy;
        private CheckBox chkAllowSearch;
        private CheckBox chkUseAlwaysEncryptSQL;
        private TextBox txtEncryptKeyName;
        private NumericUpDown txtCertificateDuration;
        private CheckBox chkReuseCertificate;
        private TextBox txtCertificateThumbPrint;
        private Button btnCreateCryptedDB;
        private TextBox txtConnectionString;
        private LinkLabel tblSaveConfig;
        private LinkLabel tblCancelConfig;
        private Label lblCertificateThumbPrint;
        private Label lblCertificateDuration;
        private Label lblEncryptKeyName;
        private Label lblParams;
        private Label lblConnectionString;
        private Label lblDLL;
        private Label lblRSAKey;
        private Label lblCERTDuration;
        private Label lblCERT;
        private Label lblRNGKey;
        private Label lblTOTPWizard;
        private Label lblMaxKeyLen;
        private Label lblSecMode;
        private Label lblHashAlgo;
        private Label lblTOTPShadows;

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public SecurityConfigurationControl(ServiceSecurityViewControl view, NamespaceSnapInBase snap)
        {
            _view = view;
            _snapin = snap;
            _panel = new Panel();
            _txtpanel = new Panel();
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// OnCreateControl method override
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ManagementService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            DoCreateControls();
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
            if (_view.IsNotifsEnabled())
            {
                if (status == ConfigOperationStatus.ConfigLoaded)
                    DoRefreshData();
            }
            else
                _panel.Refresh();
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implementation
        /// </summary>
        private void UpdateLayoutConfigStatus(ConfigOperationStatus status)
        {
            switch (status)
            {
                case ConfigOperationStatus.ConfigInError:
                    _panel.BackColor = Color.DarkRed;
                    break;
                case ConfigOperationStatus.ConfigSaved:
                    _panel.BackColor = Color.Orange;
                    break;
                case ConfigOperationStatus.ConfigLoaded:
                    _panel.BackColor = Color.Green;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
                    _panel.BackColor = Color.DarkOrange;
                    break;
                case ConfigOperationStatus.ConfigStopped:
                    _panel.BackColor = Color.DarkGray;
                    break;
                default:
                    _panel.BackColor = Color.Yellow;
                    break;
            }
            return;
        }

        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get 
            {
                ManagementService.ADFSManager.EnsureLocalConfiguration();
                return ManagementService.ADFSManager.Config; 
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                this.Dock = DockStyle.Top;
                this.Height = 783;
                this.Width = 1050;
                this.Margin = new Padding(30, 5, 30, 5);

                _panel.Width = 20;
                _panel.Height = 741;
                this.Controls.Add(_panel);

                _txtpanel.Left = 20;
                _txtpanel.Width = this.Width - 20;
                _txtpanel.Height = 741;
                _txtpanel.BackColor = System.Drawing.SystemColors.Control;
                this.Controls.Add(_txtpanel);

                lblTOTPShadows = new Label();
                lblTOTPShadows.Text = res.CTRLGLMAXCODES + " : ";
                lblTOTPShadows.Left = 10;
                lblTOTPShadows.Top = 19;
                lblTOTPShadows.Width = 170;
                _txtpanel.Controls.Add(lblTOTPShadows);

                txtTOTPShadows = new TextBox();
                txtTOTPShadows.Text = Config.OTPProvider.TOTPShadows.ToString();
                txtTOTPShadows.Left = 180;
                txtTOTPShadows.Top = 15;
                txtTOTPShadows.Width = 20;
                txtTOTPShadows.TextAlign = HorizontalAlignment.Center;
                txtTOTPShadows.MaxLength = 2;
                txtTOTPShadows.Validating += TOTPShadowsValidating;
                txtTOTPShadows.Validated += TOTPShadowsValidated;
                _txtpanel.Controls.Add(txtTOTPShadows);

                lblHashAlgo = new Label();
                lblHashAlgo.Text = res.CTRLGLHASH + " : ";
                lblHashAlgo.Left = 10;
                lblHashAlgo.Top = 51;
                lblHashAlgo.Width = 170;
                _txtpanel.Controls.Add(lblHashAlgo);

                txtHashAlgo = new TextBox();
                txtHashAlgo.Text = Config.OTPProvider.Algorithm.ToString();
                txtHashAlgo.Left = 180;
                txtHashAlgo.Top = 47;
                txtHashAlgo.Width = 60;
                txtHashAlgo.TextAlign = HorizontalAlignment.Center;
                txtHashAlgo.MaxLength = 6;
                txtHashAlgo.CharacterCasing = CharacterCasing.Upper;
                txtHashAlgo.Validating += HashAlgoValidating;
                txtHashAlgo.Validated += HashAlgoValidated;
                _txtpanel.Controls.Add(txtHashAlgo);

                lblSecMode = new Label();
                lblSecMode.Text = res.CTRLSECKEYMODE+" : ";
                lblSecMode.Left = 10;
                lblSecMode.Top = 100;
                lblSecMode.Width = 150;
                _txtpanel.Controls.Add(lblSecMode);

                MMCSecurityFormatList lst = new MMCSecurityFormatList();
                cbFormat = new ComboBox();
                cbFormat.DropDownStyle = ComboBoxStyle.DropDownList;
                cbFormat.Left = 180;
                cbFormat.Top = 96;
                cbFormat.Width = 200;
                _txtpanel.Controls.Add(cbFormat);

                cbFormat.DataSource = lst;
                cbFormat.ValueMember = "ID";
                cbFormat.DisplayMember = "Label";
                cbFormat.SelectedValue = Config.KeysConfig.KeyFormat;
                cbFormat.SelectedIndexChanged += SelectedFormatChanged;

                lblMaxKeyLen = new Label();
                lblMaxKeyLen.Text = res.CTRLSECKEYLENGTH+" : ";
                lblMaxKeyLen.Left = 10;
                lblMaxKeyLen.Top = 132;
                lblMaxKeyLen.Width = 150;
                _txtpanel.Controls.Add(lblMaxKeyLen);

                MMCSecurityKeySizeist lkeys = new MMCSecurityKeySizeist();
                cbKeySize = new ComboBox();
                cbKeySize.DropDownStyle = ComboBoxStyle.DropDownList;
                cbKeySize.Left = 180;
                cbKeySize.Top = 128;
                cbKeySize.Width = 200;
                _txtpanel.Controls.Add(cbKeySize);

                cbKeySize.DataSource = lkeys;
                cbKeySize.ValueMember = "ID";
                cbKeySize.DisplayMember = "Label";
                cbKeySize.SelectedValue = Config.KeysConfig.KeySize;
                cbKeySize.SelectedIndexChanged += SelectedKeySizeChanged;

                _panelWiz = new Panel();
                _panelWiz.Left = 500;
                _panelWiz.Top = 10;
                _panelWiz.Height = 200;
                _panelWiz.Width = 400;
                _txtpanel.Controls.Add(_panelWiz);

                lblTOTPWizard = new Label();
                lblTOTPWizard.Text = res.CTRLSECWIZARD + " : ";
                lblTOTPWizard.Left = 10;
                lblTOTPWizard.Top = 41;
                lblTOTPWizard.Width = 250;
                _panelWiz.Controls.Add(lblTOTPWizard);

                chkAllowMicrosoft = new CheckBox();
                chkAllowMicrosoft.Text = res.CTRLGLSHOWMICROSOFT;
                chkAllowMicrosoft.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoMicrosoftAuthenticator);
                chkAllowMicrosoft.Left = 20;
                chkAllowMicrosoft.Top = 65;
                chkAllowMicrosoft.Width = 300;
                chkAllowMicrosoft.CheckedChanged += AllowMicrosoftCheckedChanged;
                _panelWiz.Controls.Add(chkAllowMicrosoft);

                chkAllowGoogle = new CheckBox();
                chkAllowGoogle.Text = res.CTRLGLSHOWGOOGLE;
                chkAllowGoogle.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoGoogleAuthenticator);
                chkAllowGoogle.Left = 20;
                chkAllowGoogle.Top = 96;
                chkAllowGoogle.Width = 300;
                chkAllowGoogle.CheckedChanged += AllowGoogleCheckedChanged;
                _panelWiz.Controls.Add(chkAllowGoogle);

                chkAllowAuthy = new CheckBox();
                chkAllowAuthy.Text = res.CTRLGLSHOWAUTHY;
                chkAllowAuthy.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoAuthyAuthenticator);
                chkAllowAuthy.Left = 20;
                chkAllowAuthy.Top = 127;
                chkAllowAuthy.Width = 300;
                chkAllowAuthy.CheckedChanged += AllowAuthyCheckedChanged;
                _panelWiz.Controls.Add(chkAllowAuthy);

                chkAllowSearch = new CheckBox();
                chkAllowSearch.Text = res.CTRLGLALLOWGOOGLESEARCH;
                chkAllowSearch.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoGooglSearch);
                chkAllowSearch.Left = 20;
                chkAllowSearch.Top = 158;
                chkAllowSearch.Width = 300;
                chkAllowSearch.CheckedChanged += AllowSearchGoogleCheckedChanged;
                _panelWiz.Controls.Add(chkAllowSearch);

                _panelRNG = new Panel();
                _panelRNG.Left = 0;
                _panelRNG.Top = 171;
                _panelRNG.Height = 60;
                _panelRNG.Width = 400;
                _txtpanel.Controls.Add(_panelRNG);

                Label lblRNG = new Label();
                lblRNG.Text = "RNG (Random Number Generator)";
                lblRNG.Left = 10;
                lblRNG.Top = 0;
                lblRNG.Width = 250;
                _panelRNG.Controls.Add(lblRNG);

                lblRNGKey = new Label();
                lblRNGKey.Text = res.CTRLSECKEYGEN+" : ";
                lblRNGKey.Left = 30;
                lblRNGKey.Top = 27;
                lblRNGKey.Width = 140;
                _panelRNG.Controls.Add(lblRNGKey);

                MMCSecurityKeyGeneratorList lgens = new MMCSecurityKeyGeneratorList();
                cbKeyMode = new ComboBox();
                cbKeyMode.DropDownStyle = ComboBoxStyle.DropDownList;
                cbKeyMode.Left = 180;
                cbKeyMode.Top = 25;
                cbKeyMode.Width = 80;
                _panelRNG.Controls.Add(cbKeyMode);

                cbKeyMode.DataSource = lgens;
                cbKeyMode.ValueMember = "ID";
                cbKeyMode.DisplayMember = "Label";
                cbKeyMode.SelectedValue = Config.KeysConfig.KeyGenerator;
                cbKeyMode.SelectedIndexChanged += SelectedKeyGenChanged;

                _panelCERT = new Panel();
                _panelCERT.Left = 0;
                _panelCERT.Top = 236;
                _panelCERT.Height = 50;
                _panelCERT.Width = 400;
                _txtpanel.Controls.Add(_panelCERT);

                lblCERT = new Label();
                lblCERT.Text = res.CTRLSECCERTIFICATES;
                lblCERT.Left = 10;
                lblCERT.Top = 0;
                lblCERT.Width = 250;
                _panelCERT.Controls.Add(lblCERT);

                lblCERTDuration = new Label();
                lblCERTDuration.Text = res.CTRLSECCERTIFDURATION+" : ";
                lblCERTDuration.Left = 30;
                lblCERTDuration.Top = 27;
                lblCERTDuration.Width = 140;
                _panelCERT.Controls.Add(lblCERTDuration);

                txtCERTDuration = new NumericUpDown();
                txtCERTDuration.Left = 180;
                txtCERTDuration.Top = 24;
                txtCERTDuration.Width = 50;
                txtCERTDuration.TextAlign = HorizontalAlignment.Center;
                txtCERTDuration.Value = Config.KeysConfig.CertificateValidity;
                txtCERTDuration.Minimum = new decimal(new int[] { 1, 0, 0, 0});
                txtCERTDuration.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
                txtCERTDuration.ValueChanged += CertValidityChanged;
                _panelCERT.Controls.Add(txtCERTDuration);

                _panelRSA = new Panel();
                _panelRSA.Left = 0;
                _panelRSA.Top = 306;
                _panelRSA.Height = 50;
                _panelRSA.Width = 1050;
                _txtpanel.Controls.Add(_panelRSA);

                Label lblRSA = new Label();
                lblRSA.Text = "RSA (Rivest Shamir Adleman)";
                lblRSA.Left = 10;
                lblRSA.Top = 0;
                lblRSA.Width = 250;
                _panelRSA.Controls.Add(lblRSA);

                lblRSAKey = new Label();
                lblRSAKey.Text = res.CTRLSECTHUMPRINT+" : ";
                lblRSAKey.Left = 30;
                lblRSAKey.Top = 27;
                lblRSAKey.Width = 140;
                _panelRSA.Controls.Add(lblRSAKey);

                txtRSAThumb = new TextBox();
                if (!string.IsNullOrEmpty(Config.KeysConfig.CertificateThumbprint))
                    txtRSAThumb.Text = Config.KeysConfig.CertificateThumbprint.ToUpper();
                txtRSAThumb.Left = 180;
                txtRSAThumb.Top = 23;
                txtRSAThumb.Width = 300;
                txtRSAThumb.Validating += RSAThumbValidating;
                txtRSAThumb.Validated += RSAThumbValidated;
                _panelRSA.Controls.Add(txtRSAThumb);

                btnRSACert = new Button();
                btnRSACert.Text = res.CTRLSECNEWCERT;
                btnRSACert.Left = 680;
                btnRSACert.Top = 21;
                btnRSACert.Width = 250;
                btnRSACert.Click += btnRSACertClick;
                _panelRSA.Controls.Add(btnRSACert);

                _panelCUSTOM = new Panel();
                _panelCUSTOM.Left = 0;
                _panelCUSTOM.Top = 381;
                _panelCUSTOM.Height = 791;
                _panelCUSTOM.Width = 1050;
                _txtpanel.Controls.Add(_panelCUSTOM);

                Label lblRSACUST = new Label();
                lblRSACUST.Text = "RSA CUSTOM (One certificate per user)";
                lblRSACUST.Left = 10;
                lblRSACUST.Top = 0;
                lblRSACUST.Width = 250;
                _panelCUSTOM.Controls.Add(lblRSACUST);

                lblDLL = new Label();
                lblDLL.Text = res.CTRLSECASSEMBLY+" : ";
                lblDLL.Left = 30;
                lblDLL.Top = 27;
                lblDLL.Width = 150;
                _panelCUSTOM.Controls.Add(lblDLL);

                txtDLLCUST = new TextBox();
                txtDLLCUST.Text = Config.KeysConfig.ExternalKeyManager.FullQualifiedImplementation;
                txtDLLCUST.Left = 180;
                txtDLLCUST.Top = 23;
                txtDLLCUST.Width = 820;
                txtDLLCUST.Validating += DLLValidating;
                txtDLLCUST.Validated += DLLValidated;
                _panelCUSTOM.Controls.Add(txtDLLCUST);

                lblConnectionString = new Label();
                lblConnectionString.Text = res.CTRLSQLCONNECTSTR + " : ";
                lblConnectionString.Left = 30;
                lblConnectionString.Top = 58;
                lblConnectionString.Width = 150;
                _panelCUSTOM.Controls.Add(lblConnectionString);

                txtConnectionString = new TextBox();
                txtConnectionString.Text = Config.KeysConfig.ExternalKeyManager.ConnectionString;
                txtConnectionString.Left = 180;
                txtConnectionString.Top = 54;
                txtConnectionString.Width = 820;
                txtConnectionString.Enabled = !Config.UseActiveDirectory;
                txtConnectionString.Validating += ConnectionStringValidating;
                txtConnectionString.Validated += ConnectionStringValidated;
                _panelCUSTOM.Controls.Add(txtConnectionString);

                lblParams = new Label();
                lblParams.Text = res.CTRLSECPARAMS+" : ";
                lblParams.Left = 30;
                lblParams.Top = 85;
                lblParams.Width = 150;
                _panelCUSTOM.Controls.Add(lblParams);

                txtParams = new TextBox();
                txtParams.Text = Config.KeysConfig.ExternalKeyManager.Parameters.Data;
                txtParams.Left = 180;
                txtParams.Top = 85;
                txtParams.Width = 820;
                txtParams.Height = 60;
                txtParams.Multiline = true;
                txtParams.Validating += ParamsValidating;
                txtParams.Validated += ParamsValidated;
                _panelCUSTOM.Controls.Add(txtParams);

                btnCUSTOMDB = new Button();
                btnCUSTOMDB.Text = res.CTRLSECNEWDATABASE;
                btnCUSTOMDB.Left = 680;
                btnCUSTOMDB.Top = 160;
                btnCUSTOMDB.Width = 250;
                btnCUSTOMDB.Click += btnCUSTOMDBClick;
                _panelCUSTOM.Controls.Add(btnCUSTOMDB);

                chkUseAlwaysEncryptSQL = new CheckBox();
                chkUseAlwaysEncryptSQL.Text = res.CTRLSQLCRYPTUSING;
                chkUseAlwaysEncryptSQL.Checked = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;
                chkUseAlwaysEncryptSQL.Enabled = !Config.UseActiveDirectory;
                chkUseAlwaysEncryptSQL.Left = 10;
                chkUseAlwaysEncryptSQL.Top = 161;
                chkUseAlwaysEncryptSQL.Width = 450;
                chkUseAlwaysEncryptSQL.CheckedChanged += UseSQLCryptCheckedChanged;
                _panelCUSTOM.Controls.Add(chkUseAlwaysEncryptSQL);

                lblEncryptKeyName = new Label();
                lblEncryptKeyName.Text = res.CTRLSQLENCRYPTNAME + " : ";
                lblEncryptKeyName.Left = 50;
                lblEncryptKeyName.Top = 192;
                lblEncryptKeyName.Width = 150;
                _panelCUSTOM.Controls.Add(lblEncryptKeyName);

                txtEncryptKeyName = new TextBox();
                txtEncryptKeyName.Text = Config.KeysConfig.ExternalKeyManager.KeyName;
                txtEncryptKeyName.Left = 210;
                txtEncryptKeyName.Top = 188;
                txtEncryptKeyName.Width = 100;
                txtEncryptKeyName.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;
                txtEncryptKeyName.Validating += EncryptKeyNameValidating;
                txtEncryptKeyName.Validated += EncryptKeyNameValidated;
                _panelCUSTOM.Controls.Add(txtEncryptKeyName);

                lblCertificateDuration = new Label();
                lblCertificateDuration.Text = res.CTRLSECCERTIFDURATION + " : ";
                lblCertificateDuration.Left = 50;
                lblCertificateDuration.Top = 223;
                lblCertificateDuration.Width = 150;
                _panelCUSTOM.Controls.Add(lblCertificateDuration);

                txtCertificateDuration = new NumericUpDown();
                txtCertificateDuration.Left = 210;
                txtCertificateDuration.Top = 219;
                txtCertificateDuration.Width = 50;
                txtCertificateDuration.TextAlign = HorizontalAlignment.Center;
                txtCertificateDuration.Value = Config.KeysConfig.ExternalKeyManager.CertificateValidity;
                txtCertificateDuration.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
                txtCertificateDuration.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
                txtCertificateDuration.ValueChanged += CertCryptValidityChanged;
                txtCertificateDuration.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;
                _panelCUSTOM.Controls.Add(txtCertificateDuration);

                chkReuseCertificate = new CheckBox();
                chkReuseCertificate.Text = res.CTRLSQLREUSECERT;
                chkReuseCertificate.Checked = Config.KeysConfig.ExternalKeyManager.CertReuse;
                chkReuseCertificate.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;
                chkReuseCertificate.Left = 50;
                chkReuseCertificate.Top = 254;
                chkReuseCertificate.Width = 450;
                chkReuseCertificate.CheckedChanged += UseSQLReuseCertCheckedChanged;
                _panelCUSTOM.Controls.Add(chkReuseCertificate);

                lblCertificateThumbPrint = new Label();
                lblCertificateThumbPrint.Text = res.CTRLSQLTHUMBPRINT + " : ";
                lblCertificateThumbPrint.Left = 100;
                lblCertificateThumbPrint.Top = 285;
                lblCertificateThumbPrint.Width = 150;
                _panelCUSTOM.Controls.Add(lblCertificateThumbPrint);

                txtCertificateThumbPrint = new TextBox();
                if (!string.IsNullOrEmpty(Config.KeysConfig.ExternalKeyManager.ThumbPrint))
                    txtCertificateThumbPrint.Text = Config.KeysConfig.ExternalKeyManager.ThumbPrint.ToUpper();
                txtCertificateThumbPrint.Left = 260;
                txtCertificateThumbPrint.Top = 281;
                txtCertificateThumbPrint.Width = 300;
                txtCertificateThumbPrint.Enabled = (Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted && Config.KeysConfig.ExternalKeyManager.CertReuse);
                txtCertificateThumbPrint.Validating += CertificateThumbPrintValidating;
                txtCertificateThumbPrint.Validated += CertificateThumbPrintValidated;
                _panelCUSTOM.Controls.Add(txtCertificateThumbPrint);

                btnCreateCryptedDB = new Button();
                btnCreateCryptedDB.Text = res.CTRLSECNEWCRYPTEDDATABASE;
                btnCreateCryptedDB.Left = 680;
                btnCreateCryptedDB.Top = 312;
                btnCreateCryptedDB.Width = 250;
                btnCreateCryptedDB.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;
                btnCreateCryptedDB.Click += btnCUSTOMCRYPTEDDBClick;
                _panelCUSTOM.Controls.Add(btnCreateCryptedDB);
          
                tblSaveConfig = new LinkLabel();
                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblSaveConfig.Left = 20;
                tblSaveConfig.Top = 751;
                tblSaveConfig.Width = 80;
                tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
                tblSaveConfig.TabStop = true;
                this.Controls.Add(tblSaveConfig);

                tblCancelConfig = new LinkLabel();
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;
                tblCancelConfig.Left = 110;
                tblCancelConfig.Top = 751;
                tblCancelConfig.Width = 80;
                tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
                tblCancelConfig.TabStop = true;
                this.Controls.Add(tblCancelConfig);

                errors = new ErrorProvider(_view);
                errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// DoRefreshData method implmentation
        /// </summary>
        public void DoRefreshData()
        {
            this.SuspendLayout();
            _view.AutoValidate = AutoValidate.Disable;
            _view.CausesValidation = false;
            try
            {
                txtTOTPShadows.Text = Config.OTPProvider.TOTPShadows.ToString();
                txtHashAlgo.Text = Config.OTPProvider.Algorithm.ToString();

                chkAllowMicrosoft.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoMicrosoftAuthenticator);

                chkAllowGoogle.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoGoogleAuthenticator);

                chkAllowAuthy.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoAuthyAuthenticator);

                chkAllowSearch.Checked = !Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoGooglSearch);

                cbFormat.SelectedValue = Config.KeysConfig.KeyFormat;

                cbKeySize.SelectedValue = Config.KeysConfig.KeySize;

                cbKeyMode.SelectedValue = Config.KeysConfig.KeyGenerator;

                txtCERTDuration.Value = Config.KeysConfig.CertificateValidity;

                if (!string.IsNullOrEmpty(Config.KeysConfig.CertificateThumbprint))
                    txtRSAThumb.Text = Config.KeysConfig.CertificateThumbprint.ToUpper();
                else
                    txtRSAThumb.Text = string.Empty;

                txtDLLCUST.Text = Config.KeysConfig.ExternalKeyManager.FullQualifiedImplementation;
                txtConnectionString.Text = Config.KeysConfig.ExternalKeyManager.ConnectionString;
                txtParams.Text = Config.KeysConfig.ExternalKeyManager.Parameters.Data;

                chkUseAlwaysEncryptSQL.Checked = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;

                txtEncryptKeyName.Text = Config.KeysConfig.ExternalKeyManager.KeyName;
                txtEncryptKeyName.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;

                txtCertificateDuration.Value = Config.KeysConfig.ExternalKeyManager.CertificateValidity;
                txtCertificateDuration.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;

                chkReuseCertificate.Checked = Config.KeysConfig.ExternalKeyManager.CertReuse;
                chkReuseCertificate.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;

                if (!string.IsNullOrEmpty(Config.KeysConfig.ExternalKeyManager.ThumbPrint))
                    txtCertificateThumbPrint.Text = Config.KeysConfig.ExternalKeyManager.ThumbPrint.ToUpper();
                else
                    txtCertificateThumbPrint.Text = string.Empty;

                txtCertificateThumbPrint.Enabled = (Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted && Config.KeysConfig.ExternalKeyManager.CertReuse);

                btnCreateCryptedDB.Enabled = Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted;

                tblSaveConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPESAVE;
                tblCancelConfig.Text = Neos_IdentityServer_Console_Nodes.GENERALSCOPECANCEL;

                btnCreateCryptedDB.Text = res.CTRLSECNEWCRYPTEDDATABASE;
                lblCertificateThumbPrint.Text = res.CTRLSQLTHUMBPRINT + " : ";
                chkReuseCertificate.Text = res.CTRLSQLREUSECERT;
                lblCertificateDuration.Text = res.CTRLSECCERTIFDURATION + " : ";
                lblEncryptKeyName.Text = res.CTRLSQLENCRYPTNAME + " : ";
                chkUseAlwaysEncryptSQL.Text = res.CTRLSQLCRYPTUSING;
                btnCUSTOMDB.Text = res.CTRLSECNEWDATABASE;
                lblParams.Text = res.CTRLSECPARAMS + " : ";
                lblConnectionString.Text = res.CTRLSQLCONNECTSTR + " : ";
                lblDLL.Text = res.CTRLSECASSEMBLY + " : ";
                btnRSACert.Text = res.CTRLSECNEWCERT;
                lblRSAKey.Text = res.CTRLSECTHUMPRINT + " : ";
                lblCERTDuration.Text = res.CTRLSECCERTIFDURATION + " : ";
                lblCERT.Text = res.CTRLSECCERTIFICATES;
                lblRNGKey.Text = res.CTRLSECKEYGEN + " : ";
                chkAllowSearch.Text = res.CTRLGLALLOWGOOGLESEARCH;
                chkAllowAuthy.Text = res.CTRLGLSHOWAUTHY;
                chkAllowGoogle.Text = res.CTRLGLSHOWGOOGLE;
                chkAllowMicrosoft.Text = res.CTRLGLSHOWMICROSOFT;
                lblTOTPWizard.Text = res.CTRLSECWIZARD + " : ";
                lblMaxKeyLen.Text = res.CTRLSECKEYLENGTH + " : ";
                lblSecMode.Text = res.CTRLSECKEYMODE + " : ";
                lblHashAlgo.Text = res.CTRLGLHASH + " : ";
                lblTOTPShadows.Text = res.CTRLGLMAXCODES + " : ";

            }
            finally
            {
                UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
                UpdateControlsLayouts();
                ValidateData();
                _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
                _view.CausesValidation = true;
                this.ResumeLayout();
            }
        }

        /// <summary>
        /// IsValidData
        /// </summary>
        private void ValidateData()
        {
            try
            {
                errors.SetError(txtHashAlgo, "");
                HashMode hash = (HashMode)Enum.Parse(typeof(HashMode), txtHashAlgo.Text);
            }
            catch (Exception ex)
            {
                errors.SetError(txtHashAlgo, ex.Message);
            }
            try
            { 
                errors.SetError(txtTOTPShadows, "");
                int refr = Convert.ToInt32(txtTOTPShadows.Text);
                if (string.IsNullOrEmpty(txtTOTPShadows.Text))
                    throw new Exception(res.CTRLNULLOREMPTYERROR);
                else if ((refr < 1) || (refr > 10))
                    throw new Exception(string.Format(res.CTRLINVALIDVALUE, "1", "10"));
            }
            catch (Exception ex)
            {
                errors.SetError(txtTOTPShadows, ex.Message);
            }

            if (txtRSAThumb.Enabled)
            {
                if (string.IsNullOrEmpty(txtRSAThumb.Text))
                    errors.SetError(txtRSAThumb, res.CTRLNULLOREMPTYERROR);
                else if (!ManagementService.ADFSManager.CheckCertificate(txtRSAThumb.Text))
                    errors.SetError(txtRSAThumb, res.CTRLSECINVALIDCERT);
                else
                    errors.SetError(txtRSAThumb, "");
            }
            else
                errors.SetError(txtRSAThumb, "");

            if (string.IsNullOrEmpty(txtDLLCUST.Text))
                errors.SetError(txtDLLCUST, res.CTRLNULLOREMPTYERROR);
            else if (!AssemblyParser.CheckKeysAssembly(txtDLLCUST.Text))
                errors.SetError(txtDLLCUST, res.CTRLSECINVALIDEXTERROR);
            else
                errors.SetError(txtDLLCUST, "");

            if (txtConnectionString.Enabled)
            {
                if (!ManagementService.CheckKeysConnection(txtConnectionString.Text))
                    errors.SetError(txtConnectionString, res.CTRLSQLCONNECTSTRERROR);
                else
                    errors.SetError(txtConnectionString, "");
            }
            else
                errors.SetError(txtConnectionString, "");

            if (txtEncryptKeyName.Enabled)
            {
                if (string.IsNullOrEmpty(txtEncryptKeyName.Text))
                    errors.SetError(txtEncryptKeyName, res.CTRLNULLOREMPTYERROR);
                else
                    errors.SetError(txtEncryptKeyName, "");
            }
            else
                errors.SetError(txtEncryptKeyName, "");

            if ((chkUseAlwaysEncryptSQL.Checked) && (chkReuseCertificate.Checked))
            {
                if (txtCertificateThumbPrint.Enabled)
                {
                    if (!ManagementService.ADFSManager.CheckCertificate(txtCertificateThumbPrint.Text))
                        errors.SetError(txtCertificateThumbPrint, String.Format(res.CTRLSQLINVALIDCERTERROR, txtCertificateThumbPrint.Text));
                    else
                        errors.SetError(txtCertificateThumbPrint, "");
                }
                else
                    errors.SetError(txtCertificateThumbPrint, "");
            }
            else
                errors.SetError(txtCertificateThumbPrint, "");
        }

        /// <summary>
        /// UpdateControlsLayouts method implementation
        /// </summary>
        private void UpdateControlsLayouts()
        {
            if (_UpdateControlsLayouts)
                return;
            _UpdateControlsLayouts = true;
            try
            {
                switch ((SecretKeyFormat)cbFormat.SelectedValue)
                {
                    case SecretKeyFormat.RNG:
                        this._panelRSA.Enabled = false;
                        this._panelCUSTOM.Enabled = false;
                        this._panelRNG.Enabled = true;
                        this._panelCERT.Enabled = false;
                        break;
                    case SecretKeyFormat.RSA:
                        this._panelRSA.Enabled = true;
                        this._panelCUSTOM.Enabled = false;
                        this._panelRNG.Enabled = false;
                        this._panelCERT.Enabled = true;
                        break;
                    case SecretKeyFormat.CUSTOM:
                        this._panelRSA.Enabled = false;
                        this._panelCUSTOM.Enabled = true;
                        this._panelRNG.Enabled = false;
                        this._panelCERT.Enabled = false;
                        break;
                }
                if (_panelCUSTOM.Enabled)
                {
                    if (chkUseAlwaysEncryptSQL.Checked)
                    {
                        this.txtEncryptKeyName.Enabled = true;
                        this.txtCertificateDuration.Enabled = true;
                        this.chkReuseCertificate.Enabled = true;
                        if (this.chkReuseCertificate.Checked)
                            this.txtCertificateThumbPrint.Enabled = true;
                        else
                            this.txtCertificateThumbPrint.Enabled = false;
                        this.btnCreateCryptedDB.Enabled = true;
                    }
                    else
                    {
                        this.txtEncryptKeyName.Enabled = false;
                        this.txtCertificateDuration.Enabled = false;
                        this.chkReuseCertificate.Enabled = false;
                        this.txtCertificateThumbPrint.Enabled = false;
                        this.btnCreateCryptedDB.Enabled = false;
                    }
                }
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }


        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            if (_txtpanel != null)
                _txtpanel.Width = this.Width - 20;
        }

        #region HashAlgo
        /// <summary>
        /// HashAlgoValidating event
        /// </summary>
        private void HashAlgoValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtHashAlgo.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtHashAlgo.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    HashMode hash = (HashMode)Enum.Parse(typeof(HashMode), txtHashAlgo.Text);
                    Config.OTPProvider.Algorithm = hash;
                    errors.SetError(txtHashAlgo, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtHashAlgo, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// HashAlgoValidated method implmentation
        /// </summary>
        private void HashAlgoValidated(object sender, EventArgs e)
        {
            try
            {
                Config.OTPProvider.Algorithm = (HashMode)Enum.Parse(typeof(HashMode), txtHashAlgo.Text);
                ManagementService.ADFSManager.SetDirty(true);
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        #region TOTPShadows
        /// <summary>
        /// TOTPShadowsValidating event
        /// </summary>
        private void TOTPShadowsValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {

                if (txtTOTPShadows.Modified)
                {
                    int refr = Convert.ToInt32(txtTOTPShadows.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtTOTPShadows.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if ((refr < 1) || (refr > 10))
                        throw new Exception(string.Format(res.CTRLINVALIDVALUE, "1", "10"));
                    Config.OTPProvider.TOTPShadows = refr;
                    errors.SetError(txtTOTPShadows, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtTOTPShadows, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// TOTPShadowsValidated event
        /// </summary>
        private void TOTPShadowsValidated(object sender, EventArgs e)
        {
            try
            {
                Config.OTPProvider.TOTPShadows = Convert.ToInt32(txtTOTPShadows.Text);
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtTOTPShadows, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }
        #endregion

        /// <summary>
        /// SelectedFormatChanged method
        /// </summary>
        private void SelectedFormatChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.KeysConfig.KeyFormat = (SecretKeyFormat)cbFormat.SelectedValue;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SelectedKeySizeChanged method
        /// </summary>
        private void SelectedKeySizeChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.KeysConfig.KeySize = (KeySizeMode)cbKeySize.SelectedValue;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SelectedKeyGenChanged method
        /// </summary>
        private void SelectedKeyGenChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.KeysConfig.KeyGenerator = (KeyGeneratorMode)cbKeyMode.SelectedValue;
                    ManagementService.ADFSManager.SetDirty(true);
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// CertValidityChanged method 
        /// </summary>
        private void CertValidityChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    Config.KeysConfig.CertificateValidity = Convert.ToInt32(txtCERTDuration.Value);
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// RSAThumbValidating method implmentation
        /// </summary>
        private void RSAThumbValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtRSAThumb.Modified) && (txtRSAThumb.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtRSAThumb.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if (!ManagementService.ADFSManager.CheckCertificate(txtRSAThumb.Text))
                        throw new Exception(res.CTRLSECINVALIDCERT);
                    Config.KeysConfig.CertificateThumbprint = txtRSAThumb.Text.ToUpper();
                    errors.SetError(txtRSAThumb, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtRSAThumb, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// RSAThumbValidated method implmentation
        /// </summary>
        private void RSAThumbValidated(object sender, EventArgs e)
        {
            try
            {
                Config.KeysConfig.CertificateThumbprint = txtRSAThumb.Text.ToUpper(); ;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtRSAThumb, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// DLLValidating method implementation
        /// </summary>
        private void DLLValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtDLLCUST.Modified) && (txtDLLCUST.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (string.IsNullOrEmpty(txtDLLCUST.Text))
                        throw new Exception(res.CTRLNULLOREMPTYERROR);
                    if (!AssemblyParser.CheckKeysAssembly(txtDLLCUST.Text))
                        throw new Exception(res.CTRLSECINVALIDEXTERROR);
                    Config.KeysConfig.ExternalKeyManager.FullQualifiedImplementation = txtDLLCUST.Text;
                    errors.SetError(txtDLLCUST, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtDLLCUST, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// DLLValidated method implmentation
        /// </summary>
        private void DLLValidated(object sender, EventArgs e)
        {
            try
            {
                Config.KeysConfig.ExternalKeyManager.FullQualifiedImplementation = txtDLLCUST.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtDLLCUST, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// ConnectionStringValidating method implmentation
        /// </summary>
        private void ConnectionStringValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtConnectionString.Modified) && (txtConnectionString.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!ManagementService.CheckKeysConnection(txtConnectionString.Text))
                        throw new Exception(res.CTRLSQLCONNECTSTRERROR);
                    Config.KeysConfig.ExternalKeyManager.ConnectionString = txtConnectionString.Text;
                    errors.SetError(txtConnectionString, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtConnectionString, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// ConnectionStringValidated method implmentation
        /// </summary>
        private void ConnectionStringValidated(object sender, EventArgs e)
        {
            try
            {
                Config.KeysConfig.ExternalKeyManager.ConnectionString = txtConnectionString.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtConnectionString, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// ParamsValidating method implmentation
        /// </summary>
        private void ParamsValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtParams.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.KeysConfig.ExternalKeyManager.Parameters.Data = txtParams.Text;
                    errors.SetError(txtParams, "");
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtParams, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// ParamsValidated method implmentation
        /// </summary>
        private void ParamsValidated(object sender, EventArgs e)
        {
            try
            {
                Config.KeysConfig.ExternalKeyManager.Parameters.Data = txtParams.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtParams, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        #region Wizard Options
        /// <summary>
        /// AllowSearchGoogleCheckedChanged method implementation
        /// </summary>
        private void AllowSearchGoogleCheckedChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!chkAllowSearch.Checked)
                        Config.OTPProvider.WizardOptions |= OTPWizardOptions.NoGooglSearch;
                    else
                        Config.OTPProvider.WizardOptions &= ~OTPWizardOptions.NoGooglSearch;
                    errors.SetError(chkAllowSearch, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkAllowSearch, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// AllowAuthyCheckedChanged method implementation
        /// </summary>
        private void AllowAuthyCheckedChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!chkAllowAuthy.Checked)
                        Config.OTPProvider.WizardOptions |= OTPWizardOptions.NoAuthyAuthenticator;
                    else
                        Config.OTPProvider.WizardOptions &= ~OTPWizardOptions.NoAuthyAuthenticator;
                    errors.SetError(chkAllowAuthy, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkAllowAuthy, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// AllowGoogleCheckedChanged method implementation
        /// </summary>
        private void AllowGoogleCheckedChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!chkAllowGoogle.Checked)
                        Config.OTPProvider.WizardOptions |= OTPWizardOptions.NoGoogleAuthenticator;
                    else
                        Config.OTPProvider.WizardOptions &= ~OTPWizardOptions.NoGoogleAuthenticator;
                    errors.SetError(chkAllowGoogle, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkAllowGoogle, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// AllowMicrosoftCheckedChanged method implementation
        /// </summary>
        private void AllowMicrosoftCheckedChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if (!chkAllowMicrosoft.Checked)
                        Config.OTPProvider.WizardOptions |= OTPWizardOptions.NoMicrosoftAuthenticator;
                    else
                        Config.OTPProvider.WizardOptions &= ~OTPWizardOptions.NoMicrosoftAuthenticator;
                    errors.SetError(chkAllowMicrosoft, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkAllowMicrosoft, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        #endregion

        /// <summary>
        /// btnRSACertClick method implmentation
        /// </summary>
        private void btnRSACertClick(object sender, EventArgs e)
        {
            try
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = res.CTRLSECRSAGENERATE;
                messageBoxParameters.Buttons = MessageBoxButtons.OKCancel;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                bool result = (this._snapin.Console.ShowDialog(messageBoxParameters) == DialogResult.OK);
                Cursor curs = this.Cursor;
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (result)
                    {
                        this.txtRSAThumb.Text = ManagementService.ADFSManager.RegisterNewRSACertificate(null, Config.KeysConfig.CertificateValidity);

                        MessageBoxParameters messageBoxParameters2 = new MessageBoxParameters();
                        messageBoxParameters2.Text = string.Format(res.CTRLSECNEWCERTCREATED, this.txtRSAThumb.Text);
                        messageBoxParameters2.Buttons = MessageBoxButtons.OK;
                        messageBoxParameters2.Icon = MessageBoxIcon.Information;
                        this._snapin.Console.ShowDialog(messageBoxParameters2);

                    }
                }
                finally
                {
                    this.Cursor = curs;
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// btnCUSTOMDBClick 
        /// </summary>
        private void btnCUSTOMDBClick(object sender, EventArgs e)
        {
            DatabaseWizard Wizard = new DatabaseWizard();
            Wizard.Text = res.CTRLSECWIZTITLE;
            try
            {
                bool result = (this._snapin.Console.ShowDialog(Wizard) == DialogResult.OK);
                Cursor curs = this.Cursor;
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (result)
                    {
                        this.txtConnectionString.Text = ManagementService.ADFSManager.CreateMFASecretKeysDatabase(null, Wizard.txtInstance.Text, Wizard.txtDBName.Text, Wizard.txtAccount.Text, Wizard.txtPwd.Text);
                        MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                        messageBoxParameters.Text = string.Format(res.CTRLSQLNEWDBCREATED, Wizard.txtDBName.Text);
                        messageBoxParameters.Buttons = MessageBoxButtons.OK;
                        messageBoxParameters.Icon = MessageBoxIcon.Information;
                        this._snapin.Console.ShowDialog(messageBoxParameters);
                    }
                }
                finally
                {
                    this.Cursor = curs;
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// UseSQLCryptCheckedChanged method implementation
        /// </summary>
        private void UseSQLCryptCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted = chkUseAlwaysEncryptSQL.Checked;
                    UpdateConnectionString(chkUseAlwaysEncryptSQL.Checked);
                    UpdateControlsLayouts();
                    errors.SetError(chkUseAlwaysEncryptSQL, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkUseAlwaysEncryptSQL, ex.Message);
            }
        }

        /// <summary>
        /// UpdateConnectionString method implmentation
        /// </summary>
        private void UpdateConnectionString(bool crypted)
        {
            if (_view.AutoValidate != AutoValidate.Disable)
            {
                string cs = txtConnectionString.Text;
                if (!crypted)
                {
                    cs = Regex.Replace(cs, ";column encryption setting=enabled", "", RegexOptions.IgnoreCase);
                    cs = Regex.Replace(cs, ";column encryption setting=disabled", "", RegexOptions.IgnoreCase);
                }
                else
                {
                    cs = Regex.Replace(cs, ";column encryption setting=enabled", "", RegexOptions.IgnoreCase);
                    cs = Regex.Replace(cs, ";column encryption setting=disabled", "", RegexOptions.IgnoreCase);
                    cs += ";Column Encryption Setting=enabled";
                }
                txtConnectionString.Text = cs;
            }
        }

        /// <summary>
        /// UseSQLReuseCertCheckedChanged method implementaton
        /// </summary>
        private void UseSQLReuseCertCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.KeysConfig.ExternalKeyManager.CertReuse = chkReuseCertificate.Checked;
                    UpdateControlsLayouts();
                    errors.SetError(chkReuseCertificate, "");
                }
            }
            catch (Exception ex)
            {
                errors.SetError(chkReuseCertificate, ex.Message);
            }
        }

        /// <summary>
        /// CertValidityChanged method implementation
        /// </summary>
        private void CertCryptValidityChanged(object sender, EventArgs e)
        {
            try
            {
                if (_view.AutoValidate != AutoValidate.Disable)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.KeysConfig.ExternalKeyManager.CertificateValidity = Convert.ToInt32(txtCertificateDuration.Value);
                    errors.SetError(txtCertificateDuration, "");
                }

            }
            catch (Exception ex)
            {
                errors.SetError(txtCertificateDuration, ex.Message);
            }
        }

        /// <summary>
        /// EncryptKeyNameValidated method implementation
        /// </summary>
        private void EncryptKeyNameValidated(object sender, EventArgs e)
        {
            try
            {
                Config.KeysConfig.ExternalKeyManager.KeyName = txtEncryptKeyName.Text;
                ManagementService.ADFSManager.SetDirty(true);
                errors.SetError(txtEncryptKeyName, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// EncryptKeyNameValidating method implmentation
        /// </summary>
        private void EncryptKeyNameValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtEncryptKeyName.Modified) && (txtEncryptKeyName.Enabled))
                {
                    if (string.IsNullOrEmpty(txtEncryptKeyName.Text))
                        throw new ArgumentException(res.CTRLNULLOREMPTYERROR, "KeyName");
                    Config.KeysConfig.ExternalKeyManager.KeyName = txtEncryptKeyName.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                    errors.SetError(txtEncryptKeyName, "");
                }
                UpdateControlsLayouts();
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtEncryptKeyName, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// CertificateThumbPrintValidated method implementation
        /// </summary>
        private void CertificateThumbPrintValidated(object sender, EventArgs e)
        {
            try
            {
                ManagementService.ADFSManager.SetDirty(true);
                Config.KeysConfig.ExternalKeyManager.ThumbPrint = txtCertificateThumbPrint.Text.ToUpper();
                errors.SetError(txtCertificateThumbPrint, "");
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// CertificateThumbPrintValidating method implementation
        /// </summary>
        private void CertificateThumbPrintValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if ((txtCertificateThumbPrint.Modified) && (txtCertificateThumbPrint.Enabled))
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    if ((chkUseAlwaysEncryptSQL.Checked) && (chkReuseCertificate.Checked))
                    {
                        if (!ManagementService.ADFSManager.CheckCertificate(txtCertificateThumbPrint.Text))
                            throw new ArgumentException(String.Format(res.CTRLSQLINVALIDCERTERROR, txtCertificateThumbPrint.Text), "Certificate ThumbPrint");
                    }
                    Config.KeysConfig.ExternalKeyManager.ThumbPrint = txtCertificateThumbPrint.Text.ToUpper();
                    errors.SetError(txtCertificateThumbPrint, "");
                    UpdateControlsLayouts();
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtCertificateThumbPrint, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// btnCUSTOMCRYPTEDDBClick event
        /// </summary>
        private void btnCUSTOMCRYPTEDDBClick(object sender, EventArgs e)
        {
            DatabaseWizard Wizard = new DatabaseWizard();
            Wizard.Text = res.CTRLSECWIZTITLE;
            try
            {
                if (Config.KeysConfig.ExternalKeyManager.IsAlwaysEncrypted)
                {
                    bool result = (this._snapin.Console.ShowDialog(Wizard) == DialogResult.OK);
                    Cursor curs = this.Cursor;
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;
                        if (result)
                        {
                            bool isnew = false;
                            string thumb = string.Empty;
                            if ((Config.KeysConfig.ExternalKeyManager.CertReuse) && (Certs.GetCertificate(Config.KeysConfig.ExternalKeyManager.ThumbPrint.ToUpper(), StoreLocation.LocalMachine)) != null)
                                thumb = Config.Hosts.SQLServerHost.ThumbPrint.ToUpper();
                            else
                            {
                                thumb = ManagementService.ADFSManager.RegisterNewSQLCertificate(null, Config.KeysConfig.ExternalKeyManager.CertificateValidity, Config.KeysConfig.ExternalKeyManager.KeyName);
                                isnew = true;
                            }
                            this.txtConnectionString.Text = ManagementService.ADFSManager.CreateMFAEncryptedSecretKeysDatabase(null, Wizard.txtInstance.Text, Wizard.txtDBName.Text, Wizard.txtAccount.Text, Wizard.txtPwd.Text, Config.KeysConfig.ExternalKeyManager.KeyName, thumb);

                            MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                            if (isnew)
                                messageBoxParameters.Text = string.Format(res.CTRLSQLNEWDBCREATED2, Wizard.txtDBName.Text, thumb);
                            else
                                messageBoxParameters.Text = string.Format(res.CTRLSQLNEWDBCREATED, Wizard.txtDBName.Text);
                            messageBoxParameters.Buttons = MessageBoxButtons.OK;
                            messageBoxParameters.Icon = MessageBoxIcon.Information;
                            this._snapin.Console.ShowDialog(messageBoxParameters);
                        }
                    }
                    finally
                    {
                        this.Cursor = curs;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                messageBoxParameters.Text = ex.Message;
                messageBoxParameters.Buttons = MessageBoxButtons.OK;
                messageBoxParameters.Icon = MessageBoxIcon.Error;
                this._snapin.Console.ShowDialog(messageBoxParameters);
            }
        }

        /// <summary>
        /// SaveConfigLinkClicked event
        /// </summary>
        private void SaveConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
               _view.SaveData();
        }

        /// <summary>
        /// CancelConfigLinkClicked event
        /// </summary>
        private void CancelConfigLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_view != null)
                _view.CancelData();
        }
    }

    internal static class AssemblyParser
    {
        /// <summary>
        /// CheckSMSAssembly method implmentation
        /// </summary>
        internal static bool CheckSMSAssembly(string fqiassembly)
        {
            try
            {
                Assembly assembly = Assembly.Load(ParseAssembly(fqiassembly));
                Type _typetoload = assembly.GetType(ParseType(fqiassembly));
                if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalProvider") != null)
                    return (Activator.CreateInstance(_typetoload, true) != null); // Allow Calling internal Constructors
                else if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalOTPProvider") != null)
                    return (Activator.CreateInstance(_typetoload, true) != null); // Allow Calling internal Constructors
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// CheckKeysAssembly method implmentation
        /// </summary>
        internal static bool CheckKeysAssembly(string fqiassembly)
        {
            try
            {
                Assembly assembly = Assembly.Load(ParseAssembly(fqiassembly));
                Type _typetoload = assembly.GetType(ParseType(fqiassembly));

                if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("ISecretKeyManager") != null)
                    return (Activator.CreateInstance(_typetoload, true) != null); // Allow Calling internal Constructors
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// ParseType method implmentation
        /// </summary>
        private static string ParseAssembly(string AssemblyFulldescription)
        {
            int cnt = AssemblyFulldescription.IndexOf(',');
            return AssemblyFulldescription.Remove(0, cnt).TrimStart(new char[] { ',', ' ' });
        }

        /// <summary>
        /// ParseType method implmentation
        /// </summary>
        private static string ParseType(string AssemblyFulldescription)
        {
            string[] type = AssemblyFulldescription.Split(new char[] { ',' });
            return type[0];
        }
    }
}
