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


namespace Neos.IdentityServer.Console.Controls
{

    public partial class ADFSServerControl : Panel
    {
        private Panel _panel;
        private Panel _txtpanel;
        private LinkLabel tblstartstop;
        private ADFSServerHost _host;

        /// <summary>
        /// ADFSServerControl Constructor
        /// </summary>
        public ADFSServerControl(ADFSServerHost server, bool isrunning = true)
        {
            _host = server;
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
            DoCreateControls();
        }

        /// <summary>
        /// ServersStatusChanged method implementation
        /// </summary>
        private void ServersStatusChanged(ADFSServiceManager mgr, ServiceOperationStatus status, string servername, Exception Ex = null)
        {
            if ((servername.ToLower() == _host.FQDN.ToLower()) || (servername.ToLower() == _host.MachineName.ToLower()))
            {
                UpdateLayoutConfigStatus(status);
            }
        }

        /// <summary>
        /// UpdateLayoutConfigStatus method implmentation
        /// </summary>
        private void UpdateLayoutConfigStatus(ServiceOperationStatus status)
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
        /// DoCreateControls method implementation
        /// </summary>
        /// <param name="isrunning"></param>
        private void DoCreateControls()
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
            Label lblFQDN = new Label();
            lblFQDN.Text = _host.FQDN;
            lblFQDN.Left = 10;
            lblFQDN.Top = 10;
            lblFQDN.Width = 200;
            _txtpanel.Controls.Add(lblFQDN);

            Label lblBehavior = new Label();
            lblBehavior.Text = "Behavior Level : "+_host.BehaviorLevel.ToString();
            lblBehavior.Left = 10;
            lblBehavior.Top = 32;
            lblBehavior.Width = 200;
            _txtpanel.Controls.Add(lblBehavior);

            Label lblNodetype = new Label();
            lblNodetype.Text = "Node Type : " + _host.NodeType;
            lblNodetype.Left = 10;
            lblNodetype.Top = 54;
            lblNodetype.Width = 200;
            _txtpanel.Controls.Add(lblNodetype);

            // Second col
            Label lblOsversion = new Label();
            if (_host.CurrentMajorVersionNumber!=0)
                lblOsversion.Text = _host.ProductName + " ("+_host.CurrentMajorVersionNumber.ToString()+"."+_host.CurrentMinorVersionNumber.ToString()+")";
            else
                lblOsversion.Text = _host.ProductName;
            lblOsversion.Left = 210;
            lblOsversion.Top = 10;
            lblOsversion.Width = 300;
            _txtpanel.Controls.Add(lblOsversion);

            // Second col
            Label lblcurrentversion = new Label();
            lblcurrentversion.Text = "Version : "+_host.CurrentVersion;
            lblcurrentversion.Left = 210;
            lblcurrentversion.Top = 32;
            lblcurrentversion.Width = 300;
            _txtpanel.Controls.Add(lblcurrentversion);

            Label lblBuild = new Label();
            lblBuild.Text = "Build : " + _host.CurrentBuild.ToString();
            lblBuild.Left = 210;
            lblBuild.Top = 54;
            lblBuild.Width = 300;
            _txtpanel.Controls.Add(lblBuild);

            LinkLabel tblRestart = new LinkLabel();
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

            LinkLabel tblrestartfarm = new LinkLabel();
            tblrestartfarm.Text = res.CRTLADFSRESTARTFARMSERVICES;
            tblrestartfarm.Left = 20;
            tblrestartfarm.Top = 105;
            tblrestartfarm.Width = 400;
            tblrestartfarm.LinkClicked += tblrestartfarmLinkClicked;
            tblrestartfarm.TabStop = true;
            this.Controls.Add(tblrestartfarm);
            UpdateLayoutConfigStatus(ManagementService.ADFSManager.ServicesStatus);
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

    public partial class MFAProvidersControl : Panel
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
        private CheckBox chkProviderEnrollStrict;
        private CheckBox chkProviderPin;

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
            get { return ManagementService.ADFSManager.Config; }
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
            this._panel.Refresh();
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
            this.Dock = DockStyle.Top;
            this.Height = 110;
            this.Width = 512;
            this.Margin = new Padding(30, 5, 30, 5);

            _panel.Width = 20;
            _panel.Height = 95;
            this.Controls.Add(_panel);

            _txtpanel.Left = 20;
            _txtpanel.Width = this.Width - 40;
            _txtpanel.Height = 95;
            _txtpanel.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(_txtpanel);

            // first col
            Label lblProviderDesc = new Label();

            lblProviderDesc.Text = _provider.Description;
            lblProviderDesc.Left = 10;
            lblProviderDesc.Top = 10;
            lblProviderDesc.Width = 500;
            lblProviderDesc.Font =  new System.Drawing.Font(lblProviderDesc.Font.Name, 16F, FontStyle.Bold);
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

            chkProviderEnrollStrict = new CheckBox();
            chkProviderEnrollStrict.Text = res.CTRLPROVWIZARDSTRICT;
            switch (_kind)
            {
                case PreferredMethod.Code:
                    chkProviderEnrollStrict.Checked = Config.OTPProvider.EnrollWizardStrict;
                    if (!chkProviderEnroll.Checked)
                        chkProviderEnrollStrict.Enabled = false;
                    else
                        chkProviderEnrollStrict.Enabled = true;
                    break;
                case PreferredMethod.Email:
                    chkProviderEnrollStrict.Checked = Config.MailProvider.EnrollWizardStrict;
                    if (!chkProviderEnroll.Checked)
                        chkProviderEnrollStrict.Enabled = false;
                    else
                        chkProviderEnrollStrict.Enabled = true;
                    break;
                case PreferredMethod.External:
                    chkProviderEnrollStrict.Checked = Config.ExternalProvider.EnrollWizardStrict;
                    if (!chkProviderEnroll.Checked)
                        chkProviderEnrollStrict.Enabled = false;
                    else
                        chkProviderEnrollStrict.Enabled = true;
                    break;
                case PreferredMethod.Azure:
                    chkProviderEnrollStrict.Checked = false;
                    chkProviderEnrollStrict.Enabled = false;
                    break;
            }
            chkProviderEnrollStrict.Enabled = chkProviderEnabled.Checked;
            chkProviderEnrollStrict.Left = 540;
            chkProviderEnrollStrict.Top = 50;
            chkProviderEnrollStrict.Width = 300;
            chkProviderEnrollStrict.CheckedChanged += chkProviderEnrollChanged;
            _txtpanel.Controls.Add(chkProviderEnrollStrict);

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
            chkProviderPin.Top = 70;
            chkProviderPin.Width = 300;
            chkProviderPin.CheckedChanged += chkProviderPinChanged;
            _txtpanel.Controls.Add(chkProviderPin);

            errors = new ErrorProvider(_view);
            _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
            errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
            this.ResumeLayout();
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
                        if (chkProviderEnroll.Checked)
                        {
                            Config.OTPProvider.EnrollWizardStrict = chkProviderEnrollStrict.Checked;
                            chkProviderEnrollStrict.Enabled = true;
                        }
                        else
                        {
                            Config.OTPProvider.EnrollWizardStrict = false;
                            chkProviderEnrollStrict.Enabled = false;
                        }
                        break;
                    case PreferredMethod.Email:
                        Config.MailProvider.EnrollWizard = chkProviderEnroll.Checked;
                        if (chkProviderEnroll.Checked)
                        {
                            Config.MailProvider.EnrollWizardStrict = chkProviderEnrollStrict.Checked;
                            chkProviderEnrollStrict.Enabled = true;
                        }
                        else
                        {
                            Config.MailProvider.EnrollWizardStrict = false;
                            chkProviderEnrollStrict.Enabled = false;
                        }
                        break;
                    case PreferredMethod.External:
                        Config.ExternalProvider.EnrollWizard = chkProviderEnroll.Checked;
                        if (chkProviderEnroll.Checked)
                        {
                            Config.ExternalProvider.EnrollWizardStrict = chkProviderEnrollStrict.Checked;
                            chkProviderEnrollStrict.Enabled = true;
                        }
                        else
                        {
                            Config.ExternalProvider.EnrollWizardStrict = false;
                            chkProviderEnrollStrict.Enabled = false;
                        }
                        break;
                    case PreferredMethod.Azure:
                        Config.AzureProvider.EnrollWizard = false;
                        Config.AzureProvider.EnrollWizardStrict = false;
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

    public partial class MFAProvidersValidationControl : Panel
    {
        private ProvidersViewControl _view;
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;

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
            get { return ManagementService.ADFSManager.Config; }
        }

        /// <summary>
        /// DoCreateControls method implementation
        /// </summary>
        /// <param name="isrunning"></param>
        private void DoCreateControls()
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


            LinkLabel tblSaveConfig = new LinkLabel();
            tblSaveConfig.Text = res.CTRLSAVE;
            tblSaveConfig.Left = 0;
            tblSaveConfig.Top = 0;
            tblSaveConfig.Width = 60;
            tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
            tblSaveConfig.TabStop = true;
            _txtpanel.Controls.Add(tblSaveConfig);

            LinkLabel tblCancelConfig = new LinkLabel();
            tblCancelConfig.Text = res.CTRLCANCEL;
            tblCancelConfig.Left = 70;
            tblCancelConfig.Top = 0;
            tblCancelConfig.Width = 60;
            tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
            tblCancelConfig.TabStop = true;
            _txtpanel.Controls.Add(tblCancelConfig);
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

    public partial class ConfigurationControl : Panel
    {
        private Panel _panel;
        private Panel _txtpanel;
        private Label lblFarmActive; 

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public ConfigurationControl()
        {
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
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get { return ManagementService.ADFSManager.Config; }
        }

        /// <summary>
        /// ConfigurationStatusChanged method implementation
        /// </summary>
        internal void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
        {
            UpdateLayoutConfigStatus(status);
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
            UpdateLabels(status);
            return;
        }

        /// <summary>
        /// DoCreateControls method implementation
        /// </summary>
        /// <param name="isrunning"></param>
        private void DoCreateControls()
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
            Label lblIsInitialized = new Label();
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

            Label lblIdentifier = new Label();
            lblIdentifier.Text = "Identifier : " + Config.Hosts.ADFSFarm.FarmIdentifier;
            lblIdentifier.Left = 10;
            lblIdentifier.Top = 54;
            lblIdentifier.Width = 200;
            _txtpanel.Controls.Add(lblIdentifier);

            // Second col
            Label lbladmincontact = new Label();
            lbladmincontact.Text = "Administrative contact : "+Config.AdminContact;
            lbladmincontact.Left = 230;
            lbladmincontact.Top = 10;
            lbladmincontact.Width = 300;
            _txtpanel.Controls.Add(lbladmincontact);

            Label lblstorageMode = new Label();
            if (Config.UseActiveDirectory)
                lblstorageMode.Text = "Mode : Active Directory";
            else
                lblstorageMode.Text = "Mode : Sql Server Database";
            lblstorageMode.Left = 230;
            lblstorageMode.Top = 32;
            lblstorageMode.Width = 300;
            _txtpanel.Controls.Add(lblstorageMode);

            Label lblFarmBehavior = new Label();
            lblFarmBehavior.Text = "Behavior : " + Config.Hosts.ADFSFarm.CurrentFarmBehavior.ToString();
            lblFarmBehavior.Left = 230;
            lblFarmBehavior.Top = 54;
            lblFarmBehavior.Width = 300;
            _txtpanel.Controls.Add(lblFarmBehavior);

            // third col
            Label lbloptions = new Label();
            lbloptions.Text += "Options : ";
            if (Config.OTPProvider.Enabled)
                lbloptions.Text += "TOPT ";
            if (Config.MailProvider.Enabled)
                lbloptions.Text += "EMAILS ";
            if (Config.ExternalProvider.Enabled)
                lbloptions.Text += "SMS ";
            lbloptions.Left = 550;
            lbloptions.Top = 10;
            lbloptions.Width = 300;
            _txtpanel.Controls.Add(lbloptions);

            Label lblSecurity = new Label();
            switch(Config.KeysConfig.KeyFormat)
            { 
                case SecretKeyFormat.RSA:
                    lblSecurity.Text += "Security : RSA   "+Config.KeysConfig.CertificateThumbprint;
                    break;
                case SecretKeyFormat.CUSTOM:
                    lblSecurity.Text += "Security : RSA CUSTOM";
                    break;
                default:
                    lblSecurity.Text += "Security : RNG";
                    break;
            }
            lblSecurity.Left = 550;
            lblSecurity.Top = 32;
            lblSecurity.Width = 300;
            _txtpanel.Controls.Add(lblSecurity);

            if (Config.CustomUpdatePassword)
            {
                Label lblcutompwd = new Label();
                lblcutompwd.Text = "Use custom change password feature";
                lblcutompwd.Left = 550;
                lblcutompwd.Top = 54;
                lblcutompwd.Width = 300;
                _txtpanel.Controls.Add(lblcutompwd);
            }

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
            UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
        }

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

    public partial class GeneralConfigurationControl : Panel
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;

        // Controls
        private TextBox txtIssuer;
        private TextBox txtAdminContact;
        private TextBox txtCountryCode;
        private TextBox txtDeliveryWindow;
        private ComboBox cbConfigTemplate;

        private RadioButton rdioMFARequired;
        private RadioButton rdioMFAAllowed;
        private RadioButton rdioMFANotRequired;
        private RadioButton rdioREGAdmin;
        private RadioButton rdioREGUser;
        private RadioButton rdioREGUnManaged;
        private CheckBox chkAllowManageOptions;
        private CheckBox chkAllowChangePassword;
        private CheckBox chkAllowKMSOO;
        private NumericUpDown txtADVStart;
        private NumericUpDown txtADVEnd;

        private Panel _paneloptmfa;
        private Panel _panelstmfa;
        private Panel _panelregmfa;
        private Panel _paneladvmfa;
        private GeneralViewControl _view;

        private bool _UpdateLayoutPolicy = false;
        private ErrorProvider errors;


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
                    if (IsValidData())
                        _panel.BackColor = Color.Green;
                    else
                        _panel.BackColor = Color.DarkRed;
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
            get { return ManagementService.ADFSManager.Config; }
        }

        /// <summary>
        /// UpdateAttibutesLayouts method implmentation
        /// </summary>
        private bool UpdateAttibutesLayouts()
        {
            bool ret = true;  
            try
            {
                ret = _view.ValidateChildren();
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// IsValidData
        /// </summary>
        private bool IsValidData()
        {
            return !(string.IsNullOrEmpty(txtIssuer.Text) ||
                    string.IsNullOrEmpty(txtAdminContact.Text) ||
                    string.IsNullOrEmpty(txtCountryCode.Text) ||
                    string.IsNullOrEmpty(txtDeliveryWindow.Text));
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
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
            Label lblIssuer = new Label();
            lblIssuer.Text = res.CTRLGLCOMANYNAME+" : ";
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

            Label lblAdminContact = new Label();
            lblAdminContact.Text = res.CTRLGLCONTACT+" : ";
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

            Label lblContryCode = new Label();
            lblContryCode.Text = res.CTRLGLCONTRYCODE+" : ";
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

            Label lblDeliveryWindow = new Label();
            lblDeliveryWindow.Text = res.CTRLGLDELVERY+" : ";
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
            txtDeliveryWindow.Validating += DeliveryWindowValidating;
            txtDeliveryWindow.Validated += DeliveryWindowValidated;
            _txtpanel.Controls.Add(txtDeliveryWindow);

            Label lblConfigTemplate = new Label();
            lblConfigTemplate.Text = res.CTRLGLPOLICY+" : ";
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

            Label rdioMFALabel = new Label();
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
            _panelregmfa.Height = 100;
            _panelregmfa.Width = 300;
            _txtpanel.Controls.Add(_panelregmfa);

            Label rdioREGLabel = new Label();
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
            _paneloptmfa.Height = 100;
            _paneloptmfa.Width = 400;
            _txtpanel.Controls.Add(_paneloptmfa);

            Label optCFGLabel = new Label();
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

            chkAllowChangePassword = new CheckBox();
            chkAllowChangePassword.Text = res.CTRLGLMANAGEPWD;
            chkAllowChangePassword.Left = 20;
            chkAllowChangePassword.Top = 54;
            chkAllowChangePassword.Width = 300;
            chkAllowChangePassword.CheckedChanged += AllowChangePasswordCheckedChanged;
            _paneloptmfa.Controls.Add(chkAllowChangePassword);

            chkAllowKMSOO = new CheckBox();
            chkAllowKMSOO.Text = res.CTRLGLMANAGEKMSOO;
            chkAllowKMSOO.Left = 20;
            chkAllowKMSOO.Top = 79;
            chkAllowKMSOO.Width = 400;
            chkAllowKMSOO.CheckedChanged += AllowKMSOOCheckedChanged;
            _paneloptmfa.Controls.Add(chkAllowKMSOO);


            _paneladvmfa = new Panel();
            _paneladvmfa.Left = 530;
            _paneladvmfa.Top = 300;
            _paneladvmfa.Height = 100;
            _paneladvmfa.Width = 300;
            _txtpanel.Controls.Add(_paneladvmfa);

            Label optADVLabel = new Label();
            optADVLabel.Text = res.CTRGLMANAGEREG;
            optADVLabel.Left = 0;
            optADVLabel.Top = 10;
            optADVLabel.Width = 180;
            _paneladvmfa.Controls.Add(optADVLabel);

            Label beginADVLabel = new Label();
            beginADVLabel.Text = res.CTRGLMANAGEREGSTART+" :";
            beginADVLabel.Left = 20;
            beginADVLabel.Top = 37;
            beginADVLabel.Width = 50;
            _paneladvmfa.Controls.Add(beginADVLabel);

            Label endADVLabel = new Label();
            endADVLabel.Text = res.CTRGLMANAGEREGEND+" :";
            endADVLabel.Left = 20;
            endADVLabel.Top = 65;
            endADVLabel.Width = 50;
            _paneladvmfa.Controls.Add(endADVLabel);

            txtADVStart = new NumericUpDown();
            txtADVStart.Left = 70;
            txtADVStart.Top = 34;
            txtADVStart.Width = 50;
            txtADVStart.TextAlign = HorizontalAlignment.Center;
            txtADVStart.Value = Config.AdvertisingDays.FirstDay;
            txtADVStart.Minimum = new decimal(new int[] { 1, 0, 0, 0});
            txtADVStart.Maximum = new decimal(new int[] { 31, 0, 0, 0 });
            txtADVStart.ValueChanged += ADVStartValueChanged;
            _paneladvmfa.Controls.Add(txtADVStart);

            txtADVEnd = new NumericUpDown();
            txtADVEnd.Left = 70;
            txtADVEnd.Top = 62;
            txtADVEnd.Width = 50;
            txtADVEnd.TextAlign = HorizontalAlignment.Center;
            txtADVEnd.Value = Config.AdvertisingDays.LastDay;
            txtADVEnd.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            txtADVEnd.Maximum = new decimal(new int[] { 31, 0, 0, 0 });
            txtADVEnd.ValueChanged += ADVEndValueChanged;
            _paneladvmfa.Controls.Add(txtADVEnd);


            LinkLabel tblSaveConfig = new LinkLabel();
            tblSaveConfig.Text = res.CTRLSAVE;
            tblSaveConfig.Left = 20;
            tblSaveConfig.Top = 440;
            tblSaveConfig.Width = 60;
            tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
            tblSaveConfig.TabStop = true;
            this.Controls.Add(tblSaveConfig);

            LinkLabel tblCancelConfig = new LinkLabel();
            tblCancelConfig.Text = res.CTRLCANCEL;
            tblCancelConfig.Left = 90;
            tblCancelConfig.Top = 440;
            tblCancelConfig.Width = 60;
            tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
            tblCancelConfig.TabStop = true;
            this.Controls.Add(tblCancelConfig);

            errors = new ErrorProvider(_view);
            _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
            errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
            UpdateAttibutesLayouts();
            AdjustPolicyTemplate();
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
              //  if (Config.UserFeatures==UserFeaturesOptions.NoSet)
                    Config.UserFeatures = Config.UserFeatures.SetPolicyTemplate(UserTemplateMode.Strict); // Force defaults
            }
            UpdateLayoutPolicyComponents(unlocked);
            ManagementService.ADFSManager.SetDirty(true);
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
            if (template==UserTemplateMode.Custom)
                unlocked = true;
            UpdateLayoutPolicyComponents(unlocked);
        }
        
        /// <summary>
        /// UpdateLayoutPolicyComponents method implmentation
        /// </summary>
        private void UpdateLayoutPolicyComponents(bool unlocked)
        {
            _UpdateLayoutPolicy = true;
            try
            {
                rdioMFARequired.Checked = Config.UserFeatures.IsMFARequired();
                rdioMFAAllowed.Checked = Config.UserFeatures.IsMFAAllowed();
                rdioMFANotRequired.Checked = Config.UserFeatures.IsMFANotRequired();

                rdioREGAdmin.Checked = Config.UserFeatures.IsRegistrationRequired();
                rdioREGUser.Checked = Config.UserFeatures.IsRegistrationAllowed();
                rdioREGUnManaged.Checked = Config.UserFeatures.IsRegistrationNotRequired();

                chkAllowManageOptions.Checked = Config.UserFeatures.CanManageOptions();
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
                    chkAllowChangePassword.Enabled = false;
                    chkAllowKMSOO.Enabled = false;
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
                    chkAllowChangePassword.Enabled = true;
                    chkAllowKMSOO.Enabled = true;
                }
                txtADVStart.Enabled = Config.UserFeatures.IsAdvertisable();
                txtADVEnd.Enabled = Config.UserFeatures.IsAdvertisable();

            }
            finally
            {
                _UpdateLayoutPolicy = false;
            }
        }

        #region Features mgmt
        /// <summary>
        /// REGUnManagedCheckedChanged event
        /// </summary>
        private void REGUnManagedCheckedChanged(object sender, EventArgs e)
        {
            if (_UpdateLayoutPolicy)
                return;
            try
            {
                if (rdioREGUnManaged.Checked)
                {
                    Config.UserFeatures = Config.UserFeatures.SetUnManagedRegistration();
                    txtADVStart.Enabled = Config.UserFeatures.IsAdvertisable();
                    txtADVEnd.Enabled = Config.UserFeatures.IsAdvertisable();
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
        /// REGUserCheckedChanged event
        /// </summary>
        private void REGUserCheckedChanged(object sender, EventArgs e)
        {
            if (_UpdateLayoutPolicy)
                return;
            try
            {
                if (rdioREGUser.Checked)
                {
                    Config.UserFeatures = Config.UserFeatures.SetSelfRegistration();
                    txtADVStart.Enabled = Config.UserFeatures.IsAdvertisable();
                    txtADVEnd.Enabled = Config.UserFeatures.IsAdvertisable();
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
        /// REGAdminCheckedChanged event
        /// </summary>
        private void REGAdminCheckedChanged(object sender, EventArgs e)
        {
            if (_UpdateLayoutPolicy)
                return;
            try
            {
                if (rdioREGAdmin.Checked)
                {
                    Config.UserFeatures = Config.UserFeatures.SetAdministrativeRegistration();
                    txtADVStart.Enabled = Config.UserFeatures.IsAdvertisable();
                    txtADVEnd.Enabled = Config.UserFeatures.IsAdvertisable();
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
        /// MFANotRequiredCheckedChanged event
        /// </summary>
        private void MFANotRequiredCheckedChanged(object sender, EventArgs e)
        {
            if (_UpdateLayoutPolicy)
                return;
            try
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
            if (_UpdateLayoutPolicy)
                return;
            try
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
            if (_UpdateLayoutPolicy)
                return;
            try
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
            if (_UpdateLayoutPolicy) 
                return;
            try {
                if (chkAllowChangePassword.Checked)
                    Config.UserFeatures = Config.UserFeatures.Add(UserFeaturesOptions.AllowChangePassword);
                else
                    Config.UserFeatures = Config.UserFeatures.Remove(UserFeaturesOptions.AllowChangePassword);
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

        /// <summary>
        /// AllowManageOptionsCheckedChanged method implmentation
        /// </summary>
        private void AllowManageOptionsCheckedChanged(object sender, EventArgs e)
        {
            if (_UpdateLayoutPolicy)
                return;
            try
            {
                if (chkAllowManageOptions.Checked)
                    Config.UserFeatures = Config.UserFeatures.Add(UserFeaturesOptions.AllowManageOptions);
                else
                    Config.UserFeatures = Config.UserFeatures.Remove(UserFeaturesOptions.AllowManageOptions);
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

        /// <summary>
        /// AllowKMSOOCheckedChanged method implementation
        /// </summary>
        private void AllowKMSOOCheckedChanged(object sender, EventArgs e)
        {
            if (_UpdateLayoutPolicy)
                return;
            try
            {
                Config.KeepMySelectedOptionOn = chkAllowKMSOO.Checked;
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

        #region Advertising
        /// <summary>
        /// ADVEndValueChanged method implementation
        /// </summary>
        private void ADVEndValueChanged(object sender, EventArgs e)
        {
            try
            { 
                Config.AdvertisingDays.LastDay = Convert.ToUInt32(txtADVEnd.Value);
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

        /// <summary>
        /// ADVStartValueChanged method implementation
        /// </summary>
        private void ADVStartValueChanged(object sender, EventArgs e)
        {
            try
            {
                Config.AdvertisingDays.FirstDay = Convert.ToUInt32(txtADVStart.Value);
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

        #region DeliveryWindow
        /// <summary>
        /// DeliveryWindowValidating method implementation
        /// </summary>
        private void DeliveryWindowValidating(object sender, CancelEventArgs e)
        {
            try
            {
                int refr = Convert.ToInt32(txtDeliveryWindow.Text);
                if (txtDeliveryWindow.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.DeliveryWindow = refr;
                }
                if (string.IsNullOrEmpty(txtDeliveryWindow.Text))
                {
                    e.Cancel = true;
                    errors.SetError(txtDeliveryWindow, res.CTRLNULLOREMPTYERROR);
                }
                if ((refr < 60) || (refr > 600))
                {
                    e.Cancel = true;
                    errors.SetError(txtDeliveryWindow, string.Format(res.CTRLINVALIDVALUE, "60", "600"));
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
                if (txtDeliveryWindow.Modified)
                {
                    Config.DeliveryWindow = Convert.ToInt32(txtDeliveryWindow.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.DefaultCountryCode = txtCountryCode.Text.ToLower();
                }
                if (string.IsNullOrEmpty(txtCountryCode.Text))
                {
                    e.Cancel = true;
                    errors.SetError(txtCountryCode, res.CTRLNULLOREMPTYERROR);
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
                if (txtCountryCode.Modified)
                {
                    Config.DefaultCountryCode = txtCountryCode.Text.ToLower();
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.AdminContact = txtAdminContact.Text;
                }
                if (string.IsNullOrEmpty(txtAdminContact.Text))
                {
                    e.Cancel = true;
                    errors.SetError(txtAdminContact, res.CTRLNULLOREMPTYERROR);
                }
                if (!MMCService.IsValidEmail(txtAdminContact.Text))
                {
                    e.Cancel = true;
                    errors.SetError(txtAdminContact, res.CTRLGLINVALIDEMAILCONTACT);
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
                if (txtAdminContact.Modified)
                {
                    Config.AdminContact = txtAdminContact.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.Issuer = txtIssuer.Text;
                }
                if (string.IsNullOrEmpty(txtIssuer.Text))
                {
                    e.Cancel = true;
                    errors.SetError(txtIssuer, res.CTRLNULLOREMPTYERROR);
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
                if (txtIssuer.Modified)
                {
                    Config.Issuer = txtIssuer.Text; 
                    ManagementService.ADFSManager.SetDirty(true);
                }
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

    public partial class ADDSConfigurationControl : Panel
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
            if (Config != null)
            {
                if (Config.UseActiveDirectory != chkUseADDS.Checked)
                    chkUseADDS.Checked = Config.UseActiveDirectory;
            }
            UpdateLayoutConfigStatus(status);
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
                    if (!IsValidData())
                        _panel.BackColor = Color.DarkRed;
                    else
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
            get { return ManagementService.ADFSManager.Config; }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
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

            Label lblDomainName = new Label();
            lblDomainName.Text = res.CTRLADDOMAIN+" : ";
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

            Label lblUserName = new Label();
            lblUserName.Text = res.CTRLADACCOUNT+" : ";
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


            Label lblPassword = new Label();
            lblPassword.Text = res.CTRLADPASSWORD+" : ";
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

            Label lblAttributes = new Label();
            lblAttributes.Text = res.CTRLADATTRIBUTES+" : ";
            lblAttributes.Left = 30;
            lblAttributes.Top = 119;
            lblAttributes.Width = 300;
            _txtpanel.Controls.Add(lblAttributes);

            Label lblKeyAttribute = new Label();
            lblKeyAttribute.Text = res.CTRLADATTKEY+" : ";
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

            Label lblMailAttribute = new Label();
            lblMailAttribute.Text = res.CTRLADATTMAIL+" : ";
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

            Label lblPhoneAttribute = new Label();
            lblPhoneAttribute.Text = res.CTRLADATTPHONE+" : ";
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

            Label lblMethodAttribute = new Label();
            lblMethodAttribute.Text = res.CTRLADATTMETHOD+" : ";
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

            Label lblCreatedateAttribute = new Label();
            lblCreatedateAttribute.Text = res.CTRLADATTOVERRIDE+" : ";
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

            Label lblValiditydateAttribute = new Label();
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

            Label lblEnableAttribute = new Label();
            lblEnableAttribute.Text = res.CTRLADATTSTATUS+" : ";
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


            Label lblMaxRows = new Label();
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


            LinkLabel tblSaveConfig = new LinkLabel();
            tblSaveConfig.Text = res.CTRLSAVE;
            tblSaveConfig.Left = 20;
            tblSaveConfig.Top = 481;
            tblSaveConfig.Width = 60;
            tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
            tblSaveConfig.TabStop = true;
            this.Controls.Add(tblSaveConfig);

            LinkLabel tblCancelConfig = new LinkLabel();
            tblCancelConfig.Text = res.CTRLCANCEL;
            tblCancelConfig.Left = 90;
            tblCancelConfig.Top = 481;
            tblCancelConfig.Width = 60;
            tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
            tblCancelConfig.TabStop = true;
            this.Controls.Add(tblCancelConfig);

            errors = new ErrorProvider(_view);
            _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
            errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
            this.ResumeLayout();
            UpdateConnectionAttributesLayouts();
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
        /// UpdateAttributesLayouts() method implmentation
        /// </summary>
        private bool UpdateConnectionAttributesLayouts()
        {
            bool ret = true;
            try
            {
                if (!ManagementService.CheckRepositoryAttribute("connection", 1))
                {
                    ret = false;
                    errors.SetError(txtDomainName, res.CTRLADATTDOMAIN);
                    errors.SetError(txtUserName, res.CTRLADATTACCOUNT);
                    errors.SetError(txtPassword, res.CTRLADATTPASSWORD);
                }
                else
                {
                    errors.SetError(txtDomainName, "");
                    errors.SetError(txtUserName, "");
                    errors.SetError(txtPassword, "");
                }
                if (!ManagementService.CheckRepositoryAttribute(txtKeyAttribute.Text, 1))
                {
                    ret = false;
                    errors.SetError(txtKeyAttribute, res.CTRLADATTKEYERROR);
                }
                else errors.SetError(txtKeyAttribute, "");

                if (!ManagementService.CheckRepositoryAttribute(txtMailAttribute.Text, 1))
                {
                    ret = false;
                    errors.SetError(txtMailAttribute, res.CTRLADATTEMAILERROR);
                }
                else errors.SetError(txtMailAttribute, "");

                if (!ManagementService.CheckRepositoryAttribute(txtPhoneAttribute.Text, 1))
                {
                    ret = false;
                    errors.SetError(txtPhoneAttribute, res.CTRLADATTPHONEERROR);
                }
                else errors.SetError(txtPhoneAttribute, "");

                if (!ManagementService.CheckRepositoryAttribute(txtMethodAttribute.Text, 1))
                {
                    ret = false;
                    errors.SetError(txtMethodAttribute, res.CTRLADATMETHODERROR);
                }
                else errors.SetError(txtMethodAttribute, "");

                if (!ManagementService.CheckRepositoryAttribute(txtOverrideMethodAttribute.Text, 1))
                {
                    ret = false;
                    errors.SetError(txtOverrideMethodAttribute, res.CTRLADATOVERRIDEERROR);
                }
                else errors.SetError(txtOverrideMethodAttribute, "");

               /* if (!ManagementService.CheckRepositoryAttribute(txtValiditydateAttribute.Text, 1))
                {
                    ret = false;
                    errors.SetError(txtValiditydateAttribute, res.CTRLADATVALIDITYDATEERROR);
                }
                else errors.SetError(txtValiditydateAttribute, "");

                if (!ManagementService.CheckRepositoryAttribute(txtCheckdateAttribute.Text, 1))
                {
                    ret = false;
                    errors.SetError(txtCheckdateAttribute, res.CTRLADATCHECKDATEERROR);
                }
                else errors.SetError(txtCheckdateAttribute, "");

                if (!ManagementService.CheckRepositoryAttribute(txtTOTPAttribute.Text, 1))
                {
                    ret = false;
                    errors.SetError(txtTOTPAttribute, res.CTRLADATTOTPERROR);
                }
                else errors.SetError(txtTOTPAttribute, ""); */

                if (!ManagementService.CheckRepositoryAttribute(txtEnabledAttribute.Text, 1))
                {
                    ret = false;
                    errors.SetError(txtEnabledAttribute, res.CTRLADATENABLEDERROR);
                }
                else errors.SetError(txtEnabledAttribute, "");

            }
            catch (Exception Ex)
            {
                errors.SetError(txtDomainName, Ex.Message);
                errors.SetError(txtUserName, Ex.Message);
                errors.SetError(txtPassword, Ex.Message);
                errors.SetError(txtKeyAttribute, Ex.Message);
                errors.SetError(txtMailAttribute, Ex.Message);
                errors.SetError(txtPhoneAttribute, Ex.Message);
                errors.SetError(txtMethodAttribute, Ex.Message);
                errors.SetError(txtOverrideMethodAttribute, Ex.Message);
                errors.SetError(txtPinAttribute, Ex.Message);
                errors.SetError(txtCheckdateAttribute, Ex.Message);
                errors.SetError(txtTOTPAttribute, Ex.Message);
                errors.SetError(txtEnabledAttribute, Ex.Message);
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// IsValidData
        /// </summary>
        private bool IsValidData()
        {
            return ( // ManagementService.CheckRepositoryAttribute(txtCheckdateAttribute.Text, 1) &&
                    ManagementService.CheckRepositoryAttribute(txtOverrideMethodAttribute.Text, 1) &&
                    ManagementService.CheckRepositoryAttribute(txtEnabledAttribute.Text, 1) &&
                    ManagementService.CheckRepositoryAttribute(txtKeyAttribute.Text, 1) &&
                    ManagementService.CheckRepositoryAttribute(txtMailAttribute.Text, 1) &&
                    ManagementService.CheckRepositoryAttribute(txtMethodAttribute.Text, 1) &&
                    ManagementService.CheckRepositoryAttribute(txtPhoneAttribute.Text, 1) &&
                  //  ManagementService.CheckRepositoryAttribute(txtTOTPAttribute.Text, 1) &&
                  //  ManagementService.CheckRepositoryAttribute(txtValiditydateAttribute.Text, 1) &&
                    ManagementService.CheckRepositoryAttribute("connection", 1));
        }

        /// <summary>
        /// UseAADDSCheckedChanged method implementation
        /// </summary>
        private void UseADDSCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                Config.UseActiveDirectory = chkUseADDS.Checked;
                ManagementService.ADFSManager.SetDirty(true);
                UpdateControlsLayouts(Config.UseActiveDirectory);
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
                    Config.Hosts.ActiveDirectoryHost.DomainAddress = txtDomainName.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                e.Cancel = UpdateConnectionAttributesLayouts(); 
            }
            catch (Exception ex)
            {
                e.Cancel = true;
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
        /// DomainNameValidated method implmentation
        /// </summary>
        private void DomainNameValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtDomainName.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.DomainAddress = txtDomainName.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.Hosts.ActiveDirectoryHost.Account = txtUserName.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                e.Cancel = UpdateConnectionAttributesLayouts(); 
            }
            catch (Exception ex)
            {
                e.Cancel = true;
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
        /// UserNameValidated method implementation
        /// </summary>
        private void UserNameValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtUserName.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.Account = txtUserName.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.Hosts.ActiveDirectoryHost.Password = txtPassword.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                e.Cancel = UpdateConnectionAttributesLayouts(); 
            }
            catch (Exception ex)
            {
                e.Cancel = true;
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
        /// PasswordValidated method implementation
        /// </summary>
        private void PasswordValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtPassword.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.Password = txtPassword.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.Hosts.ActiveDirectoryHost.keyAttribute = txtKeyAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (!ManagementService.CheckRepositoryAttribute(txtKeyAttribute.Text, 1))
                {
                    e.Cancel = true;
                    errors.SetError(txtKeyAttribute, res.CTRLADATTKEYERROR);
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtKeyAttribute, ex.Message);
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
                if (txtKeyAttribute.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.keyAttribute = txtKeyAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.Hosts.ActiveDirectoryHost.mailAttribute = txtMailAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (!ManagementService.CheckRepositoryAttribute(txtMailAttribute.Text, 1))
                {
                    e.Cancel = true;
                    errors.SetError(txtMailAttribute, res.CTRLADATTEMAILERROR);
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
                if (txtMailAttribute.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.mailAttribute = txtMailAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.Hosts.ActiveDirectoryHost.phoneAttribute = txtPhoneAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (!ManagementService.CheckRepositoryAttribute(txtPhoneAttribute.Text, 1))
                {
                    e.Cancel = true;
                    errors.SetError(txtPhoneAttribute, res.CTRLADATTPHONEERROR);
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
                if (txtPhoneAttribute.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.phoneAttribute = txtPhoneAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.Hosts.ActiveDirectoryHost.methodAttribute = txtMethodAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (!ManagementService.CheckRepositoryAttribute(txtMethodAttribute.Text, 1))
                {
                    e.Cancel = true;
                    errors.SetError(txtMethodAttribute, res.CTRLADATMETHODERROR);
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
                if (txtMethodAttribute.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.methodAttribute = txtMethodAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.Hosts.ActiveDirectoryHost.overridemethodAttribute = txtOverrideMethodAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (!ManagementService.CheckRepositoryAttribute(txtOverrideMethodAttribute.Text, 1))
                {
                    e.Cancel = true;
                    errors.SetError(txtOverrideMethodAttribute, res.CTRLADATOVERRIDEERROR);
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
                if (txtOverrideMethodAttribute.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.overridemethodAttribute = txtOverrideMethodAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                      Config.Hosts.ActiveDirectoryHost.pinattribute = txtPinAttribute.Text;
                      ManagementService.ADFSManager.SetDirty(true);
                  }
                  if (!ManagementService.CheckRepositoryAttribute(txtPinAttribute.Text, 1))
                  {
                      e.Cancel = true;
                      errors.SetError(txtPinAttribute, res.CTRLADATPINERROR);
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
                 if (txtPinAttribute.Modified)
                 {
                     Config.Hosts.ActiveDirectoryHost.pinattribute = txtPinAttribute.Text;
                     ManagementService.ADFSManager.SetDirty(true);
                 } 
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
               /* if (txtCheckdateAttribute.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.notifcheckdateattribute = txtCheckdateAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                } */
                errors.SetError(txtCheckdateAttribute, "");
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
               /* if (txtTOTPAttribute.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.totpAttribute = txtTOTPAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                } */
                errors.SetError(txtTOTPAttribute, "");
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
                    Config.Hosts.ActiveDirectoryHost.totpEnabledAttribute = txtEnabledAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (!ManagementService.CheckRepositoryAttribute(txtEnabledAttribute.Text, 1))
                {
                    e.Cancel = true;
                    errors.SetError(txtEnabledAttribute, res.CTRLADATENABLEDERROR);
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
                if (txtEnabledAttribute.Modified)
                {
                    Config.Hosts.ActiveDirectoryHost.totpEnabledAttribute = txtEnabledAttribute.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    if ((maxrows < 1000) || (maxrows > 1000000))
                        throw new ArgumentException(String.Format(res.CTRLSQLMAXROWSERROR, maxrows), "MaxRows");
                    Config.Hosts.ActiveDirectoryHost.MaxRows = maxrows;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                e.Cancel = false;
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
                if (txtMaxRows.Modified)
                {
                    int maxrows = Convert.ToInt32(txtMaxRows.Text);
                    if ((maxrows < 1000) || (maxrows > 1000000))
                        throw new ArgumentException(String.Format(res.CTRLSQLMAXROWSERROR, maxrows), "MaxRows");
                    Config.Hosts.ActiveDirectoryHost.MaxRows = maxrows;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
            try
            {
                if (!ManagementService.CheckRepositoryAttribute("connection", 1))
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

    public partial class SQLConfigurationControl : Panel
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
        private Button btnCreateDB;
        private ErrorProvider errors;

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
            if (Config != null)
            {
                if (Config.UseActiveDirectory == chkUseSQL.Checked)
                    chkUseSQL.Checked = !Config.UseActiveDirectory;
            }
            UpdateLayoutConfigStatus(status);
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
                    if (!IsValidData())
                        _panel.BackColor = Color.DarkRed;
                    else
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
            get { return ManagementService.ADFSManager.Config; }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            this.Dock = DockStyle.Top;
            this.Height = 585;
            this.Width = 512;
            this.Margin = new Padding(30, 5, 30, 5);

            _panel.Width = 20;
            _panel.Height = 151;
            this.Controls.Add(_panel);

            _txtpanel.Left = 20;
            _txtpanel.Width = this.Width - 20;
            _txtpanel.Height = 151;
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

            Label lblConnectionString = new Label();
            lblConnectionString.Text = res.CTRLSQLCONNECTSTR+" : ";
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

            Label lblMaxRows = new Label();
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


            LinkLabel tblSaveConfig = new LinkLabel();
            tblSaveConfig.Text = res.CTRLSAVE;
            tblSaveConfig.Left = 20;
            tblSaveConfig.Top = 161;
            tblSaveConfig.Width = 60;
            tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
            tblSaveConfig.TabStop = true;
            this.Controls.Add(tblSaveConfig);

            LinkLabel tblCancelConfig = new LinkLabel();
            tblCancelConfig.Text = res.CTRLCANCEL;
            tblCancelConfig.Left = 90;
            tblCancelConfig.Top = 161;
            tblCancelConfig.Width = 60;
            tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
            tblCancelConfig.TabStop = true;
            this.Controls.Add(tblCancelConfig);

            errors = new ErrorProvider(_view);
            _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
            errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
            this.ResumeLayout();
            UpdateAttributesLayouts();
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
                txtConnectionString.Enabled = isenabled;
                txtMaxRows.Enabled = isenabled;
                btnConnect.Enabled = isenabled;
                btnCreateDB.Enabled = isenabled;
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }

        /// <summary>
        /// UpdateAttributesLayouts method implementation
        /// </summary>
        private bool UpdateAttributesLayouts()
        {
            bool ret = true;
            try
            {
                if (!ManagementService.CheckRepositoryAttribute("connectionstring", 2))
                {
                    ret = false;
                    errors.SetError(txtConnectionString, res.CTRLSQLCONNECTSTRERROR);
                }
                else
                    errors.SetError(txtConnectionString, "");
            }
            catch (Exception Ex)
            {
                errors.SetError(txtConnectionString, Ex.Message);
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// IsValidData
        /// </summary>
        private bool IsValidData()
        {
            try
            {
                return ManagementService.CheckRepositoryAttribute("connectionstring", 2);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// UseSQLCheckedChanged method implementation
        /// </summary>
        private void UseSQLCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                Config.UseActiveDirectory = !chkUseSQL.Checked;
                ManagementService.ADFSManager.SetDirty(true);
                UpdateControlsLayouts(!Config.UseActiveDirectory);
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
                if (txtConnectionString.Modified)
                {
                    Config.Hosts.SQLServerHost.ConnectionString = txtConnectionString.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                e.Cancel = !UpdateAttributesLayouts();
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
                if (txtConnectionString.Modified)
                {
                    Config.Hosts.SQLServerHost.ConnectionString = txtConnectionString.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                if (txtMaxRows.Modified)
                {
                    int maxrows = Convert.ToInt32(txtMaxRows.Text);
                    if ((maxrows<1000) || (maxrows>1000000))
                        throw new ArgumentException(String.Format(res.CTRLSQLMAXROWSERROR, maxrows), "MaxRows");
                    Config.Hosts.SQLServerHost.MaxRows = maxrows;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                e.Cancel = !UpdateAttributesLayouts();
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
                if (txtMaxRows.Modified)
                {
                    int maxrows = Convert.ToInt32(txtMaxRows.Text);
                    if ((maxrows < 1000) || (maxrows > 1000000))
                        throw new ArgumentException(String.Format(res.CTRLSQLMAXROWSERROR, maxrows), "MaxRows");
                    Config.Hosts.SQLServerHost.MaxRows = maxrows;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
            try
            {
                if (!ManagementService.CheckRepositoryAttribute("connectionstring", 2))
                {
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                    messageBoxParameters.Text = res.CTRLSQLCONNECTERROR;
                    messageBoxParameters.Buttons = MessageBoxButtons.OK;
                    messageBoxParameters.Icon = MessageBoxIcon.Error;
                    this._snapin.Console.ShowDialog(messageBoxParameters);

                }
                else
                {
                    MessageBoxParameters messageBoxParameters = new MessageBoxParameters();
                    messageBoxParameters.Text = res.CTRLSQLCONNECTOK;
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
        /// btnConnectClick method implmentation
        /// </summary>
        private void btnCreateDBClick(object sender, EventArgs e)
        {
            DatabaseWizard Wizard = new DatabaseWizard();
            try
            {
                bool result = (this._snapin.Console.ShowDialog(Wizard) == DialogResult.OK);
                if (result)
                {
                    this.txtConnectionString.Text = ManagementService.ADFSManager.CreateMFADatabase(null, Wizard.txtInstance.Text, Wizard.txtDBName.Text, Wizard.txtAccount.Text, Wizard.txtPwd.Text);
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

    public partial class SMTPConfigurationControl : Panel
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
                    if (!IsValidData())
                        _panel.BackColor = Color.DarkRed;
                    else
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
            get { return ManagementService.ADFSManager.Config; }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
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

            Label lblCompany = new Label();
            lblCompany.Text = res.CTRLSMTPCOMPANY+" : ";
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

            Label lblFrom = new Label();
            lblFrom.Text = res.CTRLSMTPFROM+" : ";
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

            Label lblServer = new Label();
            lblServer.Text = res.CTRLSMTPSERVER;
            lblServer.Left = 10;
            lblServer.Top = 95;
            lblServer.Width = 180;
            _txtpanel.Controls.Add(lblServer);

            Label lblHost = new Label();
            lblHost.Text = res.CTRLSMTPSERVERADDRESS+" : ";
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


            Label lblPort = new Label();
            lblPort.Text = res.CTRLSMTPPORT+" : ";
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
            chkUseSSL.Validating += SSLValidating;
            chkUseSSL.Validated += SSLValidated;
            chkUseSSL.CheckedChanged += SSLChecked;
            _txtpanel.Controls.Add(chkUseSSL);


            Label lblidentify = new Label();
            lblidentify.Text = res.CTRLSMTPIDENTIFICATION;
            lblidentify.Left = 10;
            lblidentify.Top = 170;
            lblidentify.Width = 180;
            _txtpanel.Controls.Add(lblidentify);

            Label lblAccount = new Label();
            lblAccount.Text = res.CTRLSMTPACCOUNT+" : ";
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

            Label lblPassword = new Label();
            lblPassword.Text = res.CTRLSMTPPASSWORD+" : ";
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
            chkAnonymous.Validating += AnonymousValidating;
            chkAnonymous.Validated += AnonymousValidated;
            chkAnonymous.CheckedChanged += AnonymousChecked;
            _txtpanel.Controls.Add(chkAnonymous);



            btnConnect = new Button();
            btnConnect.Text = res.CTRLSMTPTEST;
            btnConnect.Left = 480;
            btnConnect.Top = 270;
            btnConnect.Width = 150;
            btnConnect.Click += btnConnectClick;
            _txtpanel.Controls.Add(btnConnect);


            LinkLabel tblSaveConfig = new LinkLabel();
            tblSaveConfig.Text = res.CTRLSAVE;
            tblSaveConfig.Left = 20;
            tblSaveConfig.Top = 361;
            tblSaveConfig.Width = 60;
            tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
            tblSaveConfig.TabStop = true;
            this.Controls.Add(tblSaveConfig);

            LinkLabel tblCancelConfig = new LinkLabel();
            tblCancelConfig.Text = res.CTRLCANCEL;
            tblCancelConfig.Left = 90;
            tblCancelConfig.Top = 361;
            tblCancelConfig.Width = 60;
            tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
            tblCancelConfig.TabStop = true;
            this.Controls.Add(tblCancelConfig);

            errors = new ErrorProvider(_view);
            _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
            errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
            this.ResumeLayout();
            UpdateAttributesLayouts();
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
        /// UpdateAttributesLayouts method implementation
        /// </summary>
        private bool UpdateAttributesLayouts()
        {
            if (_view!=null)
                return _view.ValidateChildren();
            else
                return true;
        }

        /// <summary>
        /// IsValidData
        /// </summary>
        private bool IsValidData()
        {
            try
            {
                if (_view != null)
                    return _view.ValidateChildren();
                else
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
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
                    Config.MailProvider.Company = txtCompany.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (string.IsNullOrEmpty(Config.MailProvider.Host))
                {
                    e.Cancel = true;
                    errors.SetError(txtFrom, res.CTRLNULLOREMPTYERROR);
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
                if (txtCompany.Modified)
                {
                    Config.MailProvider.Company = txtCompany.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.MailProvider.From = txtFrom.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (!MMCService.IsValidEmail(txtFrom.Text))
                {
                    e.Cancel = true;
                    errors.SetError(txtFrom, res.CTRLSMTPMAILERROR);
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
                if (txtFrom.Modified)
                {
                    Config.MailProvider.From = txtFrom.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.MailProvider.Host = txtHost.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (string.IsNullOrEmpty(Config.MailProvider.Host))
                {
                    e.Cancel = true;
                    errors.SetError(txtHost, res.CTRLNULLOREMPTYERROR);
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
                if (txtHost.Modified)
                {
                    Config.MailProvider.Host = txtHost.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.MailProvider.Port = Convert.ToInt32(txtPort.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if ((Config.MailProvider.Port<=UInt16.MinValue) || (Config.MailProvider.Port>=UInt16.MaxValue))
                {
                    e.Cancel = true;
                    errors.SetError(txtPort, res.CTRLSMTPPORTERROR);
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
                if (txtPort.Modified)
                {
                    Config.MailProvider.Port = Convert.ToInt32(txtPort.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                }
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

        #region Port
        /// <summary>
        /// SSLValidating method implementation
        /// </summary>
        private void SSLValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (Config.MailProvider.UseSSL != chkUseSSL.Checked)
                {
                    Config.MailProvider.UseSSL = chkUseSSL.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(chkUseSSL, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// SSLValidated method implementation
        /// </summary>
        private void SSLValidated(object sender, EventArgs e)
        {
            try
            {
                if (Config.MailProvider.UseSSL != chkUseSSL.Checked)
                {
                    Config.MailProvider.UseSSL = chkUseSSL.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                errors.SetError(chkUseSSL, "");
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
        /// SSLChecked method implementation
        /// </summary>
        private void SSLChecked(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.UseSSL = chkUseSSL.Checked;
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
                    Config.MailProvider.UserName = txtAccount.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (string.IsNullOrEmpty(Config.MailProvider.UserName))
                {
                    e.Cancel = true;
                    errors.SetError(txtAccount, res.CTRLNULLOREMPTYERROR);
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
                if (txtAccount.Modified)
                {
                    Config.MailProvider.UserName = txtAccount.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.MailProvider.Password = txtPassword.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (string.IsNullOrEmpty(Config.MailProvider.Password))
                {
                    e.Cancel = true;
                    errors.SetError(txtPassword, res.CTRLNULLOREMPTYERROR);
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
                if (txtPassword.Modified)
                {
                    Config.MailProvider.Password = txtPassword.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
        /// AnonymousValidating method implementation
        /// </summary>
        private void AnonymousValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (Config.MailProvider.Anonymous != chkAnonymous.Checked)
                {
                    Config.MailProvider.Anonymous = chkAnonymous.Checked;
                    txtAccount.Enabled = !chkAnonymous.Checked;
                    txtPassword.Enabled = !chkAnonymous.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(chkAnonymous, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// AnonymousValidated method implementation
        /// </summary>
        private void AnonymousValidated(object sender, EventArgs e)
        {
            try
            {
                if (Config.MailProvider.Anonymous != chkAnonymous.Checked)
                {
                    Config.MailProvider.Anonymous = chkAnonymous.Checked;
                    txtAccount.Enabled = !chkAnonymous.Checked;
                    txtPassword.Enabled = !chkAnonymous.Checked;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                errors.SetError(chkAnonymous, "");
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
        /// AnonymousChecked method implementation
        /// </summary>
        private void AnonymousChecked(object sender, EventArgs e)
        {
            try
            {
                Config.MailProvider.Anonymous = chkAnonymous.Checked;
                txtAccount.Enabled = !chkAnonymous.Checked;
                txtPassword.Enabled = !chkAnonymous.Checked;
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

    public partial class SMSConfigurationControl : Panel
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
           // UpdateControlsLayouts(Config.ExternalProvider.Enabled);
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
                    if (!IsValidData())
                        _panel.BackColor = Color.DarkRed;
                    else
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
            get { return ManagementService.ADFSManager.Config; }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
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

            Label lblCompany = new Label();
            lblCompany.Text = res.CTRLSMSCOMPANY+" : ";
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

            Label lblTimeout = new Label();
            lblTimeout.Text = res.CTRLSMSTIMEOUT+" : ";
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

            Label lblSha1 = new Label();
            lblSha1.Text = res.CTRLSMSSHA1+" : ";
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

            Label lblDLL = new Label();
            lblDLL.Text = res.CTRLSMSASSEMBLY+" : ";
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

            Label lblParams = new Label();
            lblParams.Text = res.CTRLSMSPARAMS+" : ";
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

            LinkLabel tblSaveConfig = new LinkLabel();
            tblSaveConfig.Text = res.CTRLSAVE;
            tblSaveConfig.Left = 20;
            tblSaveConfig.Top = 271;
            tblSaveConfig.Width = 60;
            tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
            tblSaveConfig.TabStop = true;
            this.Controls.Add(tblSaveConfig);

            LinkLabel tblCancelConfig = new LinkLabel();
            tblCancelConfig.Text = res.CTRLCANCEL;
            tblCancelConfig.Left = 90;
            tblCancelConfig.Top = 271;
            tblCancelConfig.Width = 60;
            tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
            tblCancelConfig.TabStop = true;
            this.Controls.Add(tblCancelConfig);

            errors = new ErrorProvider(_view);
            _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
            errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
           // UpdateControlsLayouts(Config.ExternalProvider.Enabled);
            this.ResumeLayout();
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
        /// IsValidData
        /// </summary>
        private bool IsValidData()
        {
            try
            {
                return _view.ValidateChildren();
            }
            catch (Exception)
            {
                return false;
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
                Config.ExternalProvider.IsTwoWay = chkIsTwoWay.Checked;
                ManagementService.ADFSManager.SetDirty(true);
            }
            catch (Exception ex)
            {
                errors.SetError(txtCompany, ex.Message);
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
                    Config.ExternalProvider.Company = txtCompany.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (string.IsNullOrEmpty(Config.ExternalProvider.Company))
                {
                    errors.SetError(txtCompany, res.CTRLNULLOREMPTYERROR);
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtCompany, ex.Message);
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
        /// CompanyValidated method
        /// </summary>
        private void CompanyValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtCompany.Modified)
                {
                    Config.ExternalProvider.Company = txtCompany.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.ExternalProvider.Timeout = Convert.ToInt32(txtTimeout.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if ((Config.ExternalProvider.Timeout <= 0) || (Config.ExternalProvider.Timeout > 1000))
                {
                    errors.SetError(txtTimeout, string.Format(res.CTRLINVALIDVALUE , "1", "1000"));
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtTimeout, ex.Message);
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
        /// TimeOutValidated method
        /// </summary>
        private void TimeOutValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtTimeout.Modified)
                {
                    Config.ExternalProvider.Timeout = Convert.ToInt32(txtTimeout.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.ExternalProvider.Sha1Salt = txtSHA1.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (string.IsNullOrEmpty(txtSHA1.Text))
                {
                    errors.SetError(txtSHA1, res.CTRLNULLOREMPTYERROR);
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtSHA1, ex.Message);
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
        /// SHA1Validated event
        /// </summary>
        private void SHA1Validated(object sender, EventArgs e)
        {
            try
            {
                if (txtSHA1.Modified)
                {
                    Config.ExternalProvider.Sha1Salt = txtSHA1.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.ExternalProvider.FullQualifiedImplementation = txtDLL.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (string.IsNullOrEmpty(txtDLL.Text))
                {
                    errors.SetError(txtDLL, res.CTRLNULLOREMPTYERROR);
                    e.Cancel = true;
                }
                if (!AssemblyParser.CheckSMSAssembly(Config.ExternalProvider.FullQualifiedImplementation))
                {
                    errors.SetError(txtDLL, res.CTRLSMSIVALIDEXTERROR);
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtDLL, ex.Message);
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
        /// DLLValidated event
        /// </summary>
        private void DLLValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtDLL.Modified)
                {
                    Config.ExternalProvider.FullQualifiedImplementation = txtDLL.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.ExternalProvider.Parameters.Data = txtParams.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (string.IsNullOrEmpty(txtParams.Text))
                {
                    errors.SetError(txtParams, res.CTRLNULLOREMPTYERROR);
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtParams, ex.Message);
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
        /// ParamsValidated event
        /// </summary>
        private void ParamsValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtParams.Modified)
                {
                    Config.ExternalProvider.Parameters.Data = txtParams.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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

    public partial class AzureConfigurationControl : Panel
    {
        private NamespaceSnapInBase _snapin;
        private Panel _panel;
        private Panel _txtpanel;
        private AzureViewControl _view;
        private bool _UpdateControlsLayouts;
        private ErrorProvider errors;
        private TextBox txtTenantId;
        private TextBox txtThumbprint;

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
           // UpdateControlsLayouts(Config.AzureProvider.Enabled);
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
                    if (!IsValidData())
                        _panel.BackColor = Color.DarkRed;
                    else
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
            get { return ManagementService.ADFSManager.Config; }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
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

            Label lblTenantID = new Label();
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

            Label lblThumbPrint = new Label();
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

            LinkLabel tblSaveConfig = new LinkLabel();
            tblSaveConfig.Text = res.CTRLSAVE;
            tblSaveConfig.Left = 20;
            tblSaveConfig.Top = 95;
            tblSaveConfig.Width = 60;
            tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
            tblSaveConfig.TabStop = true;
            this.Controls.Add(tblSaveConfig);

            LinkLabel tblCancelConfig = new LinkLabel();
            tblCancelConfig.Text = res.CTRLCANCEL;
            tblCancelConfig.Left = 90;
            tblCancelConfig.Top = 95;
            tblCancelConfig.Width = 60;
            tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
            tblCancelConfig.TabStop = true;
            this.Controls.Add(tblCancelConfig);

            errors = new ErrorProvider(_view);
            _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
            errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
          //  UpdateControlsLayouts(Config.AzureProvider.Enabled);
            this.ResumeLayout();
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

        /// <summary>
        /// IsValidData
        /// </summary>
        private bool IsValidData()
        {
            try
            {
                return _view.ValidateChildren();
            }
            catch (Exception)
            {
                return false;
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
                    Config.AzureProvider.TenantId = txtTenantId.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (string.IsNullOrEmpty(Config.AzureProvider.TenantId))
                {
                    errors.SetError(txtTenantId, res.CTRLNULLOREMPTYERROR);
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtTenantId, ex.Message);
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
        /// TenantidValidated method
        /// </summary>
        private void TenantidValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtTenantId.Modified)
                {
                    Config.AzureProvider.TenantId = txtTenantId.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                    Config.AzureProvider.ThumbPrint = txtThumbprint.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if ((string.IsNullOrEmpty(Config.AzureProvider.ThumbPrint)))
                {
                    errors.SetError(txtThumbprint, res.CTRLNULLOREMPTYERROR);
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtThumbprint, ex.Message);
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
        /// ThumbprintValidated method
        /// </summary>
        private void ThumbprintValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtThumbprint.Modified)
                {
                    Config.AzureProvider.ThumbPrint = txtThumbprint.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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

    public partial class SecurityConfigurationControl : Panel
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
                    if (!IsValidData())
                        _panel.BackColor = Color.DarkRed;
                    else
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
            get { return ManagementService.ADFSManager.Config; }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void DoCreateControls()
        {
            this.SuspendLayout();
            this.Dock = DockStyle.Top;
            this.Height = 612;
            this.Width = 1050;
            this.Margin = new Padding(30, 5, 30, 5);

            _panel.Width = 20;
            _panel.Height = 550;
            this.Controls.Add(_panel);

            _txtpanel.Left = 20;
            _txtpanel.Width = this.Width - 20;
            _txtpanel.Height = 550;
            _txtpanel.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(_txtpanel);

            Label lblTOTPShadows = new Label();
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

            Label lblHashAlgo = new Label();
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

            Label lblSecMode = new Label();
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

            Label lblMaxKeyLen = new Label();
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

            Label lblTOTPWizard = new Label();
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

            Label lblRNGKey = new Label();
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

            Label lblCERT = new Label();
            lblCERT.Text = res.CTRLSECCERTIFICATES;
            lblCERT.Left = 10;
            lblCERT.Top = 0;
            lblCERT.Width = 250;
            _panelCERT.Controls.Add(lblCERT);

            Label lblCERTDuration = new Label();
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
            _panelRSA.Width = 700;
            _txtpanel.Controls.Add(_panelRSA);

            Label lblRSA = new Label();
            lblRSA.Text = "RSA (Rivest Shamir Adleman)";
            lblRSA.Left = 10;
            lblRSA.Top = 0;
            lblRSA.Width = 250;
            _panelRSA.Controls.Add(lblRSA);

            Label lblRSAKey = new Label();
            lblRSAKey.Text = res.CTRLSECTHUMPRINT+" : ";
            lblRSAKey.Left = 30;
            lblRSAKey.Top = 27;
            lblRSAKey.Width = 140;
            _panelRSA.Controls.Add(lblRSAKey);

            txtRSAThumb = new TextBox();
            txtRSAThumb.Text = Config.KeysConfig.CertificateThumbprint;
            txtRSAThumb.Left = 180;
            txtRSAThumb.Top = 23;
            txtRSAThumb.Width = 300;
            txtRSAThumb.Validating += RSAThumbValidating;
            txtRSAThumb.Validated += RSAThumbValidated;
            _panelRSA.Controls.Add(txtRSAThumb);

            btnRSACert = new Button();
            btnRSACert.Text = res.CTRLSECNEWCERT;
            btnRSACert.Left = 510;
            btnRSACert.Top = 21;
            btnRSACert.Width = 150;
            btnRSACert.Click += btnRSACertClick;
            _panelRSA.Controls.Add(btnRSACert);


            _panelCUSTOM = new Panel();
            _panelCUSTOM.Left = 0;
            _panelCUSTOM.Top = 381;
            _panelCUSTOM.Height = 160;
            _panelCUSTOM.Width = 1050;
            _txtpanel.Controls.Add(_panelCUSTOM);

            Label lblRSACUST = new Label();
            lblRSACUST.Text = "RSA CUSTOM (One certificate per user)";
            lblRSACUST.Left = 10;
            lblRSACUST.Top = 0;
            lblRSACUST.Width = 250;
            _panelCUSTOM.Controls.Add(lblRSACUST);

            Label lblDLL = new Label();
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

            Label lblParams = new Label();
            lblParams.Text = res.CTRLSECPARAMS+" : ";
            lblParams.Left = 30;
            lblParams.Top = 55;
            lblParams.Width = 150;
            _panelCUSTOM.Controls.Add(lblParams);

            txtParams = new TextBox();
            txtParams.Text = Config.KeysConfig.ExternalKeyManager.Parameters.Data;
            txtParams.Left = 180;
            txtParams.Top = 53;
            txtParams.Width = 820;
            txtParams.Height = 70;
            txtParams.Multiline = true;
            txtParams.Validating += ParamsValidating;
            txtParams.Validated += ParamsValidated;
            _panelCUSTOM.Controls.Add(txtParams);

            btnCUSTOMDB = new Button();
            btnCUSTOMDB.Text = res.CTRLSECNEWDATABASE;
            btnCUSTOMDB.Left = 180;
            btnCUSTOMDB.Top = 130;
            btnCUSTOMDB.Width = 250;
            btnCUSTOMDB.Click += btnCUSTOMDBClick;
            _panelCUSTOM.Controls.Add(btnCUSTOMDB);

            LinkLabel tblSaveConfig = new LinkLabel();
            tblSaveConfig.Text = res.CTRLSAVE;
            tblSaveConfig.Left = 20;
            tblSaveConfig.Top = 561;
            tblSaveConfig.Width = 60;
            tblSaveConfig.LinkClicked += SaveConfigLinkClicked;
            tblSaveConfig.TabStop = true;
            this.Controls.Add(tblSaveConfig);

            LinkLabel tblCancelConfig = new LinkLabel();
            tblCancelConfig.Text = res.CTRLCANCEL;
            tblCancelConfig.Left = 90;
            tblCancelConfig.Top = 561;
            tblCancelConfig.Width = 60;
            tblCancelConfig.LinkClicked += CancelConfigLinkClicked;
            tblCancelConfig.TabStop = true;
            this.Controls.Add(tblCancelConfig);

            errors = new ErrorProvider(_view);
            _view.AutoValidate = AutoValidate.EnableAllowFocusChange;
            errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            UpdateLayoutConfigStatus(ManagementService.ADFSManager.ConfigurationStatus);
            UpdateControlsLayouts();
            this.ResumeLayout();
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
                        this._panelCERT.Enabled = true;
                        break;
                }
            }
            finally
            {
                _UpdateControlsLayouts = false;
            }
        }

        /// <summary>
        /// IsValidData
        /// </summary>
        private bool IsValidData()
        {
            try
            {
                if (_view != null)
                    return _view.ValidateChildren();
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


        #region HashAlgo
        /// <summary>
        /// HashAlgoValidating event
        /// </summary>
        private void HashAlgoValidating(object sender, CancelEventArgs e)
        {
            try
            {
                HashMode hash = (HashMode)Enum.Parse(typeof(HashMode), txtHashAlgo.Text);
                if (txtHashAlgo.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.OTPProvider.Algorithm = hash;
                }
                if (string.IsNullOrEmpty(txtHashAlgo.Text))
                {
                    e.Cancel = true;
                    errors.SetError(txtHashAlgo, res.CTRLNULLOREMPTYERROR);
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtHashAlgo, ex.Message);
            }
        }

        /// <summary>
        /// HashAlgoValidated method implmentation
        /// </summary>
        private void HashAlgoValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtHashAlgo.Modified)
                {
                    Config.OTPProvider.Algorithm = (HashMode)Enum.Parse(typeof(HashMode), txtHashAlgo.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                }
                errors.SetError(txtHashAlgo, "");
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
            try
            {
                int refr = Convert.ToInt32(txtTOTPShadows.Text);
                if (txtTOTPShadows.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.OTPProvider.TOTPShadows = refr;
                }
                if (string.IsNullOrEmpty(txtTOTPShadows.Text))
                {
                    e.Cancel = true;
                    errors.SetError(txtTOTPShadows, res.CTRLNULLOREMPTYERROR);
                }
                if ((refr < 1) || (refr > 10))
                {
                    e.Cancel = true;
                    errors.SetError(txtTOTPShadows, string.Format(res.CTRLINVALIDVALUE, "1", "10"));
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtTOTPShadows, ex.Message);
            }
        }

        /// <summary>
        /// TOTPShadowsValidated event
        /// </summary>
        private void TOTPShadowsValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtTOTPShadows.Modified)
                {
                    Config.OTPProvider.TOTPShadows = Convert.ToInt32(txtTOTPShadows.Text);
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                Config.KeysConfig.KeyFormat = (SecretKeyFormat)cbFormat.SelectedValue;
                ManagementService.ADFSManager.SetDirty(true);
                UpdateControlsLayouts();
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
                Config.KeysConfig.KeySize = (KeySizeMode)cbKeySize.SelectedValue;
                ManagementService.ADFSManager.SetDirty(true);
                UpdateControlsLayouts();
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
                Config.KeysConfig.KeyGenerator = (KeyGeneratorMode)cbKeyMode.SelectedValue;
                ManagementService.ADFSManager.SetDirty(true);
                UpdateControlsLayouts();
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
                Config.KeysConfig.CertificateValidity = Convert.ToInt32(txtCERTDuration.Value);
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

        /// <summary>
        /// RSAThumbValidating method implmentation
        /// </summary>
        private void RSAThumbValidating(object sender, CancelEventArgs e)
        {
            try
            {
                if (txtRSAThumb.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.KeysConfig.CertificateThumbprint = txtRSAThumb.Text;
                }
                if (string.IsNullOrEmpty(txtRSAThumb.Text))
                {
                    e.Cancel = true;
                    errors.SetError(txtRSAThumb, res.CTRLNULLOREMPTYERROR);
                }
                if (!ManagementService.ADFSManager.CheckCertificate(txtRSAThumb.Text))
                {
                    e.Cancel = true;
                    errors.SetError(txtRSAThumb, res.CTRLSECINVALIDCERT);
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtRSAThumb, ex.Message);
            }
        }

        /// <summary>
        /// RSAThumbValidated method implmentation
        /// </summary>
        private void RSAThumbValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtRSAThumb.Modified)
                {
                    Config.KeysConfig.CertificateThumbprint = txtRSAThumb.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
            try
            {
                if (txtDLLCUST.Modified)
                {
                    ManagementService.ADFSManager.SetDirty(true);
                    Config.KeysConfig.ExternalKeyManager.FullQualifiedImplementation = txtDLLCUST.Text;
                }
                if (string.IsNullOrEmpty(txtDLLCUST.Text))
                {
                    e.Cancel = true;
                    errors.SetError(txtDLLCUST, res.CTRLNULLOREMPTYERROR);
                }
                if (!AssemblyParser.CheckKeysAssembly(Config.KeysConfig.ExternalKeyManager.FullQualifiedImplementation))
                {
                    errors.SetError(txtDLLCUST, res.CTRLSECINVALIDEXTERROR);
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtDLLCUST, ex.Message);
            }
        }

        /// <summary>
        /// DLLValidated method implmentation
        /// </summary>
        private void DLLValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtDLLCUST.Modified)
                {
                    Config.KeysConfig.ExternalKeyManager.FullQualifiedImplementation = txtDLLCUST.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
        /// ParamsValidating method implmentation
        /// </summary>
        private void ParamsValidating(object sender, CancelEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (txtParams.Modified)
                {
                    Config.KeysConfig.ExternalKeyManager.Parameters.Data = txtParams.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
                if (string.IsNullOrEmpty(txtParams.Text))
                {
                    errors.SetError(txtParams, res.CTRLNULLOREMPTYERROR);
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                errors.SetError(txtParams, ex.Message);
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
        /// ParamsValidated method implmentation
        /// </summary>
        private void ParamsValidated(object sender, EventArgs e)
        {
            try
            {
                if (txtParams.Modified)
                {
                    Config.KeysConfig.ExternalKeyManager.Parameters.Data = txtParams.Text;
                    ManagementService.ADFSManager.SetDirty(true);
                }
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
                if (!chkAllowSearch.Checked)
                {
                    Config.OTPProvider.WizardOptions |= OTPWizardOptions.NoGooglSearch;
                }
                else
                {
                    Config.OTPProvider.WizardOptions &= ~ OTPWizardOptions.NoGooglSearch;
                }
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
                if (!chkAllowAuthy.Checked)
                {
                    Config.OTPProvider.WizardOptions |= OTPWizardOptions.NoAuthyAuthenticator;
                }
                else
                {
                    Config.OTPProvider.WizardOptions &= ~OTPWizardOptions.NoAuthyAuthenticator;
                }
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
                if (!chkAllowGoogle.Checked)
                {
                    Config.OTPProvider.WizardOptions |= OTPWizardOptions.NoGoogleAuthenticator;
                }
                else
                {
                    Config.OTPProvider.WizardOptions &= ~OTPWizardOptions.NoGoogleAuthenticator;
                }
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
                if (!chkAllowMicrosoft.Checked)
                {
                    Config.OTPProvider.WizardOptions |= OTPWizardOptions.NoMicrosoftAuthenticator;
                }
                else
                {
                    Config.OTPProvider.WizardOptions &= ~OTPWizardOptions.NoMicrosoftAuthenticator;
                }
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
                if (result)
                {
                    this.txtRSAThumb.Text = ManagementService.ADFSManager.RegisterNewRSACertificate(null, Config.KeysConfig.CertificateValidity);
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
                if (result)
                {
                    this.txtParams.Text = ManagementService.ADFSManager.CreateMFASecretKeysDatabase(null, Wizard.txtInstance.Text, Wizard.txtDBName.Text, Wizard.txtAccount.Text, Wizard.txtPwd.Text);
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
