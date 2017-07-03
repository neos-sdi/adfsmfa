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
            Initialize(isrunning);
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
            ManagementAdminService.ADFSManager.ServiceStatusChanged += ServersStatusChanged;
            ManagementAdminService.ADFSManager.RefreshServiceStatus();
        }

        /// <summary>
        /// ServersStatusChanged method implementation
        /// </summary>
        private void ServersStatusChanged(ADFSServiceManager mgr, ServiceOperationStatus status, string servername, Exception Ex = null)
        {
            if ((servername.ToLower() == _host.FQDN.ToLower()) || (servername.ToLower() == _host.MachineName.ToLower()))
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
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        /// <param name="isrunning"></param>
        private void Initialize(bool isrunning)
        {
            this.Dock = DockStyle.Top;
            this.Height = 110;
            this.Width = 512;
            this.Margin = new Padding(30, 5, 30, 5);

            _panel.Width = 20;
            _panel.Height = 75;
            if (isrunning)
                _panel.BackColor = Color.Green;
            else
                _panel.BackColor = Color.Red;
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
            tblRestart.Text = "Redémarrer les services de fédération";
            tblRestart.Left = 1;
            tblRestart.Top = 80;
            tblRestart.Width = 200;
            tblRestart.LinkClicked += tblRestart_LinkClicked;
            tblRestart.TabIndex = 0;
            tblRestart.TabStop = true;
            this.Controls.Add(tblRestart);

            tblstartstop = new LinkLabel();
            if (isrunning)
                tblstartstop.Text = "Arréter les services de fédération";
            else
                tblstartstop.Text = "Démarrer les services de fédération";
            tblstartstop.Tag = isrunning;
            tblstartstop.Left = 201;
            tblstartstop.Top = 80;
            tblstartstop.Width = 200;
            tblstartstop.LinkClicked += tblstartstop_LinkClicked;
            tblRestart.TabIndex = 1;
            tblRestart.TabStop = true;
            this.Controls.Add(tblstartstop);
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
            if (status==ServiceOperationStatus.OperationRunning)
            {
                tblstartstop.Text = "Arréter les services de fédération";
                tblstartstop.Tag = true;
            }
            else
            {
                tblstartstop.Text = "Démarrer les services de fédération";
                tblstartstop.Tag = false;
            }
        }

        private void tblRestart_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ManagementAdminService.ADFSManager.RestartServer(null, _host.FQDN);
        }

        private void tblstartstop_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            bool isrunning = Convert.ToBoolean((sender as LinkLabel).Tag);
            if (isrunning)
            {
                ManagementAdminService.ADFSManager.StopService(_host.FQDN);
                (sender as LinkLabel).Text = "Démarrer les services de fédération";
                (sender as LinkLabel).Tag = false;
            }
            else
            {
                ManagementAdminService.ADFSManager.StartService(_host.FQDN);
                (sender as LinkLabel).Text = "Arréter les services de fédération";
                (sender as LinkLabel).Tag = true;
            }
        }
    }

    public partial class ConfigurationControl : Panel
    {
        private Panel _panel;
        private Panel _txtpanel;
        private MFAConfig _cfg;
        private Label lblFarmActive; 

        /// <summary>
        /// ConfigurationControl Constructor
        /// </summary>
        public ConfigurationControl(MFAConfig cfg, bool isrunning = true, bool isactive = true)
        {
            _cfg = cfg;
            _panel = new Panel();
            _txtpanel = new Panel();
            Initialize(isrunning, isactive);
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
            ManagementAdminService.ADFSManager.ConfigurationStatusChanged += ConfigurationStatusChanged;
            ManagementAdminService.ADFSManager.RefreshConfigurationStatus();
        }

        /// <summary>
        /// ServersStatusChanged method implementation
        /// </summary>
        private void ConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null)
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
                    bool isactive = ManagementAdminService.ADFSManager.IsMFAProviderEnabled(null);
                    if (isactive)
                        _panel.BackColor = Color.Green;
                    else
                        _panel.BackColor = Color.Yellow;
                    lblFarmActive.Tag = isactive;
                    break;
                case ConfigOperationStatus.ConfigIsDirty:
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
        /// Initialize method implementation
        /// IsInitialized="true" CurrentFarmBehavior="3" FarmIdentifier="urn:sts:redhook:software"
        /// </summary>
        /// <param name="isrunning"></param>
        private void Initialize(bool isrunning, bool isactive)
        {
            this.Dock = DockStyle.Top;
            this.Height = 85;
            this.Width = 512;
            this.Margin = new Padding(30, 5, 30, 5);

            _panel.Width = 20;
            _panel.Height = 75;
            if (isrunning)
            {
                if (isactive)
                    _panel.BackColor = Color.Green;
                else 
                    _panel.BackColor = Color.Yellow;
            }
            else
                _panel.BackColor = Color.DarkRed;
            this.Controls.Add(_panel);

            _txtpanel.Left = 20;
            _txtpanel.Width = this.Width - 20;
            _txtpanel.Height = 75;
            _txtpanel.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(_txtpanel);

            // first col
            Label lblIsInitialized = new Label();
            lblIsInitialized.Text = "Initialized : " + _cfg.Hosts.ADFSFarm.IsInitialized.ToString();
            lblIsInitialized.Left = 10;
            lblIsInitialized.Top = 10;
            lblIsInitialized.Width = 200;
            _txtpanel.Controls.Add(lblIsInitialized);

            lblFarmActive = new Label();
            lblFarmActive.Text = "Active : " + isactive.ToString();
            lblFarmActive.Left = 10;
            lblFarmActive.Top = 32;
            lblFarmActive.Width = 200;
            lblFarmActive.Tag = isactive;
            _txtpanel.Controls.Add(lblFarmActive);

            Label lblIdentifier = new Label();
            lblIdentifier.Text = "Identifier : " + _cfg.Hosts.ADFSFarm.FarmIdentifier;
            lblIdentifier.Left = 10;
            lblIdentifier.Top = 54;
            lblIdentifier.Width = 200;
            _txtpanel.Controls.Add(lblIdentifier);

            // Second col
            Label lbladmincontact = new Label();
            lbladmincontact.Text = "Administrative contact : "+_cfg.AdminContact;
            lbladmincontact.Left = 210;
            lbladmincontact.Top = 10;
            lbladmincontact.Width = 300;
            _txtpanel.Controls.Add(lbladmincontact);

            Label lblstorageMode = new Label();
            if (_cfg.UseActiveDirectory)
                lblstorageMode.Text = "Mode : Active Directory";
            else
                lblstorageMode.Text = "Mode : Sql Server Database";
            lblstorageMode.Left = 210;
            lblstorageMode.Top = 32;
            lblstorageMode.Width = 300;
            _txtpanel.Controls.Add(lblstorageMode);

            Label lblFarmBehavior = new Label();
            lblFarmBehavior.Text = "Behavior : " + _cfg.Hosts.ADFSFarm.CurrentFarmBehavior.ToString();
            lblFarmBehavior.Left = 210;
            lblFarmBehavior.Top = 54;
            lblFarmBehavior.Width = 300;
            _txtpanel.Controls.Add(lblFarmBehavior);

            // third col
            Label lbloptions = new Label();
            lbloptions.Text += "Options : ";
            if (_cfg.AppsEnabled)
                lbloptions.Text += "TOPT ";
            if (_cfg.MailEnabled)
                lbloptions.Text += "EMAILS ";
            if (_cfg.SMSEnabled)
                lbloptions.Text += "SMS ";
            lbloptions.Left = 510;
            lbloptions.Top = 10;
            lbloptions.Width = 300;
            _txtpanel.Controls.Add(lbloptions);

            Label lblSecurity = new Label();
            switch(_cfg.KeysConfig.KeyFormat)
            { 
                case RegistrationSecretKeyFormat.RSA:
                    lblSecurity.Text += "Security : RSA   "+_cfg.KeysConfig.CertificateThumbprint;
                    break;
                case RegistrationSecretKeyFormat.CUSTOM:
                    lblSecurity.Text += "Security : RSA CUSTOM";
                    break;
                default:
                    lblSecurity.Text += "Security : RNG";
                    break;
            }
            lblSecurity.Left = 510;
            lblSecurity.Top = 32;
            lblSecurity.Width = 300;
            _txtpanel.Controls.Add(lblSecurity);

            if (_cfg.CustomUpdatePassword)
            {
                Label lblcutompwd = new Label();
                lblcutompwd.Text = "Use custom change password feature";
                lblcutompwd.Left = 510;
                lblcutompwd.Top = 54;
                lblcutompwd.Width = 300;
                _txtpanel.Controls.Add(lblcutompwd);
            }

/*
            LinkLabel tblRestart = new LinkLabel();
            tblRestart.Text = "Redémarrer les services de fédération";
            tblRestart.Left = 1;
            tblRestart.Top = 80;
            tblRestart.Width = 200;
            tblRestart.LinkClicked += tblRestart_LinkClicked;
            this.Controls.Add(tblRestart);

            tblstartstop = new LinkLabel();
            if (isrunning)
                tblstartstop.Text = "Arréter les services de fédération";
            else
                tblstartstop.Text = "Démarrer les services de fédération";
            tblstartstop.Tag = isrunning;
            tblstartstop.Left = 201;
            tblstartstop.Top = 80;
            tblstartstop.Width = 200;
            tblstartstop.LinkClicked += tblstartstop_LinkClicked;
            this.Controls.Add(tblstartstop);
 **/
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
            lblFarmActive.Text = "Active : " + Convert.ToBoolean(lblFarmActive.Tag).ToString();
        }

        private void tblRestart_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
        //    ManagementAdminService.ADFSManager.RestartServer(null, _cfg.FQDN);
        }

        private void tblstartstop_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
          /*  bool isrunning = Convert.ToBoolean((sender as LinkLabel).Tag);
            if (isrunning)
            {
                ManagementAdminService.ADFSManager.StopService(_cfg.FQDN);
                (sender as LinkLabel).Text = "Démarrer les services de fédération";
                (sender as LinkLabel).Tag = false;
            }
            else
            {
                ManagementAdminService.ADFSManager.StartService(_cfg.FQDN);
                (sender as LinkLabel).Text = "Arréter les services de fédération";
                (sender as LinkLabel).Tag = true;
            } */
        }
    }

    public partial class ADFSServersFooterControl : Panel
    {
        private LinkLabel tblrestartfarm;

        /// <summary>
        /// ADFSServerControl Constructor
        /// </summary>
        public ADFSServersFooterControl(bool isrunning = true)
        {
            Initialize(isrunning);
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        /// <param name="isrunning"></param>
        private void Initialize(bool isrunning)
        {
            this.Dock = DockStyle.Top;
            this.Height = 110;
            this.Width = 512;
            this.Margin = new Padding(30, 5, 30, 5);

            tblrestartfarm = new LinkLabel();
            tblrestartfarm.Text = "Redémarrer tous les services de fédération";
            tblrestartfarm.Left = 1;
            tblrestartfarm.Top = 1;
            tblrestartfarm.Width = 400;
            tblrestartfarm.LinkClicked += tblRestart_LinkClicked;
            tblrestartfarm.TabIndex = 0;
            tblrestartfarm.TabStop = true;
            this.Controls.Add(tblrestartfarm);

        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
        }

        private void tblRestart_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ManagementAdminService.ADFSManager.RestartFarm(null);
        }

    }

    public partial class ConfigurationFooterControl: Panel
    {
        private LinkLabel tblconfigure;

        /// <summary>
        /// ADFSServerControl Constructor
        /// </summary>
        public ConfigurationFooterControl(bool isrunning = true, bool isactive = true)
        {
            Initialize(isrunning, isactive);
            BackColor = System.Drawing.SystemColors.Window;
            AutoSize = false;
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        /// <param name="isrunning"></param>
        private void Initialize(bool isrunning, bool isactive)
        {
            this.Dock = DockStyle.Top;
            this.Height = 45;
            this.Width = 512;
            this.Margin = new Padding(30, 5, 30, 5);

            if (isrunning)
            {
                tblconfigure = new LinkLabel();
                if (isactive)
                    tblconfigure.Text = "Désactiver MFA";
                else
                    tblconfigure.Text = "Activer MFA";
                tblconfigure.Left = 1;
                tblconfigure.Top = 1;
                tblconfigure.Width = 400;
                tblconfigure.LinkClicked += tblRestart_LinkClicked;
                tblconfigure.TabIndex = 0;
                tblconfigure.TabStop = true;
                tblconfigure.Tag = isactive;
                this.Controls.Add(tblconfigure);
            }

        }

        /// <summary>
        /// OnResize method implmentation
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
        }

        private void tblRestart_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Convert.ToBoolean((sender as LinkLabel).Tag))
            {
                ManagementAdminService.ADFSManager.DisableMFAProvider(null);
                (sender as LinkLabel).Tag = false;
                (sender as LinkLabel).Text = "Activer MFA";
            }
            else
            {
                ManagementAdminService.ADFSManager.EnableMFAProvider(null);
                (sender as LinkLabel).Tag = true;
                (sender as LinkLabel).Text = "Désactiver MFA";
            }
        }
    }
}
