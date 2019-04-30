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
using Microsoft.Win32;
using Neos.IdentityServer.MultiFactor.Administration.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace Neos.IdentityServer.MultiFactor.Administration
{
    #region enums
    /// <summary>
    /// ServiceOperationStatus enum
    /// </summary>
    public enum ServiceOperationStatus
    {
        OperationUnknown,
        OperationPending,
        OperationRunning,
        OperationStopped,
        OperationInError
    }

    /// <summary>
    /// ServiceOperationStatus enum
    /// </summary>
    public enum ConfigOperationStatus
    {
        ConfigUnknown,
        ConfigLoaded,
        ConfigIsDirty,
        ConfigSaved,
        ConfigStopped,
        ConfigInError
    }
    #endregion

    #region ADFSServiceManager
    /// <summary>
    /// ADFSServiceManager Class
    /// </summary>
    public class ADFSServiceManager
    {
        public delegate void ADFSServiceStatus(ADFSServiceManager mgr, ServiceOperationStatus status, string servername, Exception Ex = null);
        public delegate void ADFSConfigStatus(ADFSServiceManager mgr, ConfigOperationStatus status, Exception ex = null);
        public event ADFSServiceStatus ServiceStatusChanged;
        public event ADFSConfigStatus ConfigurationStatusChanged;

        private ServiceOperationStatus _status;
        private ConfigOperationStatus _configstatus;

        private MFAConfig _config = null;

        #region Constructors
        /// <summary>
        /// ADFSServiceManager constructor
        /// </summary>
        public ADFSServiceManager()
        {
            ServiceStatusChanged += DefaultServiceStatusChanged;
            ConfigurationStatusChanged += DefaultConfigurationStatusChanged;
            if (IsRunning())
                ServicesStatus = ServiceOperationStatus.OperationRunning;
            else
                ServicesStatus = ServiceOperationStatus.OperationStopped;
            this.ServiceStatusChanged(this, ServicesStatus, "");
            ConfigurationStatus = ConfigOperationStatus.ConfigUnknown;
            this.ConfigurationStatusChanged(this, ConfigurationStatus);
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(string mailslothost = "MGT", bool dontthrow = false)
        {
            try
            {
                ServiceController ADFSController = new ServiceController("mfanotifhub");
                try
                {
                    if (ADFSController.Status != ServiceControllerStatus.Running)
                        ADFSController.Start();
                    ADFSController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));
                }
                catch (Exception e)
                {
                    if (!dontthrow)
                        throw e;
                }
            }
            catch (Exception e)
            {
                if (!dontthrow)
                    throw e;
            }
        }

        public void OnServiceStatusChanged(ServiceOperationStatus status, string servername, Exception Ex = null)
        {
            this.ServiceStatusChanged(this, status, servername, Ex);
        }

        public void OnConfigurationStatusChanged(ConfigOperationStatus status, Exception Ex = null)
        {
            this.ConfigurationStatusChanged(this, status, Ex);
        }

        /// <summary>
        /// MailSlotMessageArrived method implmentation
        /// </summary>
        private void MailSlotMessageArrived(MailSlotServer maislotserver, MailSlotMessage message)
        {
            if (message.Operation == 0x10)
            {
                ServicesStatus = ServiceOperationStatus.OperationRunning;
                this.ServiceStatusChanged(this, ServicesStatus, message.Text);
            }
            else if (message.Operation == 0x11)
            {
                ServicesStatus = ServiceOperationStatus.OperationStopped;
                this.ServiceStatusChanged(this, ServicesStatus, message.Text);
            }
            else if (message.Operation == 0x12)
            {
                ServicesStatus = ServiceOperationStatus.OperationPending;
                this.ServiceStatusChanged(this, ServicesStatus, message.Text);
            }
            else if (message.Operation == 0x19)
            {
                ServicesStatus = ServiceOperationStatus.OperationInError;
                this.ServiceStatusChanged(this, ServicesStatus, message.Text);
            }
        }
        #endregion

        #region Utility Methods and events
        /// <summary>
        /// DefaultServiceStatusChanged method implementation
        /// </summary>
        private void DefaultServiceStatusChanged(ADFSServiceManager mgr, ServiceOperationStatus status, string servername, Exception Ex = null)
        {
            mgr.ServicesStatus = status;
        }

        /// <summary>
        /// DefaultConfigurationStatusChanged method implmentation 
        /// </summary>
        private void DefaultConfigurationStatusChanged(ADFSServiceManager mgr, ConfigOperationStatus status, Exception Ex = null)
        {
            mgr.ConfigurationStatus = status;
        }

        /// <summary>
        /// RefreshServiceStatus method implementation
        /// </summary>
        public void RefreshServiceStatus()
        {
            this.ServiceStatusChanged(this, ServicesStatus, "");
        }

        /// <summary>
        /// RefreshServiceStatus method implementation
        /// </summary>
        public void RefreshConfigurationStatus()
        {
            this.ConfigurationStatusChanged(this, ConfigurationStatus);
        }

        /// <summary>
        /// EnsureLocalService method implementation
        /// </summary>
        public void EnsureLocalService()
        {
            if (!IsADFSServer())
                throw new Exception(SErrors.ErrorADFSPlatformNotSupported); 
            if (!IsRunning())
                StartService();
        }

        /// <summary>
        /// EnsureConfiguration method implementation
        /// </summary>
        private void EnsureConfiguration(PSHost Host)
        {
            if (_config == null)
            {
                EnsureLocalService();
                _config = ReadConfiguration(Host);
                if (!IsFarmConfigured())
                    throw new Exception(SErrors.ErrorMFAFarmNotInitialized);
            }
        }

        /// <summary>
        /// EnsureLocalConfiguration method implementation
        /// </summary>
        public void EnsureLocalConfiguration(PSHost Host = null)
        {
            EnsureConfiguration(Host);
            if (_config == null)
                throw new Exception(SErrors.ErrorLoadingMFAConfiguration);
            return;
        }

        /// <summary>
        /// SetDirty method implmentayion
        /// </summary>
        public void SetDirty(bool value)
        {
            EnsureConfiguration(null);
            _config.IsDirty = value;
            if (value)
                this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigIsDirty);
        }
        #endregion

        #region Properties
        /// <summary>
        /// ADFSServers property
        /// </summary>
        public ADFSFarmHost ADFSFarm
        {
            get
            {
              //  EnsureConfiguration();
                if (_config != null)
                    return _config.Hosts.ADFSFarm;
                else
                    return null;
            }
        }

        /// <summary>
        /// Config property implementation
        /// </summary>
        public MFAConfig Config
        {
            get { return _config; }
            internal set { _config = value; }
        }

        /// <summary>
        /// ServicesStatus property
        /// </summary>
        public ServiceOperationStatus ServicesStatus
        {
            get { return _status; }
            set { _status = value; }
        }

        /// <summary>
        /// ConfigurationStatus property
        /// </summary>
        public ConfigOperationStatus ConfigurationStatus
        {
            get { return _configstatus; }
            set { _configstatus = value; }
        }
        #endregion

        #region ADFS Services
        /// <summary>
        /// IsFarmConfigured method implementation
        /// </summary>
        public bool IsFarmConfigured()
        {
            if (ADFSFarm==null)
                return false;
            else
                return ADFSFarm.IsInitialized;
        }

        /// <summary>
        /// IsRunning method iplementation
        /// </summary>
        public bool IsADFSServer(string servername = "local")
        {
            ServiceController ADFSController = null;
            try
            {
                if (servername.ToLowerInvariant().Equals("local"))
                    ADFSController = new ServiceController("adfssrv");
                else
                    ADFSController = new ServiceController("adfssrv", servername);
                ADFSController.Refresh();
                ServiceControllerStatus st = ADFSController.Status;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (ADFSController != null)
                    ADFSController.Close();
            }
        }

        /// <summary>
        /// IsRunning method iplementation
        /// </summary>
        public bool IsRunning(string servername = "local")
        {
            ServiceController ADFSController = null;
            try
            {
                if (servername.ToLowerInvariant().Equals("local"))
                    ADFSController = new ServiceController("adfssrv");
                else
                    ADFSController = new ServiceController("adfssrv", servername);
                ADFSController.Refresh();
                return ADFSController.Status == ServiceControllerStatus.Running;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (ADFSController != null)
                    ADFSController.Close();
            }
        }

        /// <summary>
        /// StartService method implementation
        /// </summary>
        public bool StartService(string servername = null)
        {
            if (servername != null)
                return internalStartService(servername);
            else
                return internalStartService();
        }

        /// <summary>
        /// StopService method implementation
        /// </summary>
        public bool StopService(string servername = null)
        {
            if (servername != null)
               return internalStopService(servername);
            else
                return internalStopService();
        }

        /// <summary>
        /// RestartFarm method implmentation
        /// </summary>
        public void RestartFarm(PSHost Host = null)
        {
            if (Host != null)
                Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : " + "Stopping ADFS Farm...");
            if (ADFSFarm != null)
            {
                foreach (ADFSServerHost srv in ADFSFarm.Servers)
                {
                    if (Host != null)
                        Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : " + "Stopping " + srv.FQDN.ToLower() + " ...");
                    StopService(srv.FQDN);
                    if (Host != null)
                        Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : " + "Starting " + srv.FQDN.ToLower() + " ...");
                    StartService(srv.FQDN);
                }
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : " + "ADFS Farm Started");
            }
            else
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : " + "ADFS Farm Not Starte !");

        }

        /// RestartFarm method implmentation
        /// </summary>
        public bool RestartServer(PSHost Host = null, string servername = "local")
        {
            bool result = false;
            if (servername.ToLower().Equals("local"))
            {
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : " + "Stopping  ...");
                StopService(servername);
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : " + "Starting  ...");
                StartService(servername);
                result = true;
            }
            else
            {
                foreach (ADFSServerHost srv in ADFSFarm.Servers)
                {
                    if (srv.FQDN.ToLower().Equals(servername.ToLower()))
                    {
                        if (Host != null)
                            Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : " + "Stopping " + srv.FQDN.ToLower() + " ...");
                        StopService(srv.FQDN);
                        if (Host != null)
                            Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : " + "Starting " + srv.FQDN.ToLower() + " ...");
                        StartService(srv.FQDN);
                        result = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// internalStartService method implementation
        /// </summary>
        private bool internalStartService(string servername = "local")
        {
            ServiceController ADFSController = null;
            try
            {
                this.ServiceStatusChanged(this, _status, servername);
                if (servername.ToLowerInvariant().Equals("local"))
                    ADFSController = new ServiceController("adfssrv");
                else
                    ADFSController = new ServiceController("adfssrv", servername);
                using (MailSlotClient mailslot = new MailSlotClient("MGT"))
                {
                    mailslot.Text = servername;
                    mailslot.SendNotification(0x12);
                }
                this.ServiceStatusChanged(this, ServiceOperationStatus.OperationPending, servername);
                if (ADFSController.Status != ServiceControllerStatus.Running)
                    ADFSController.Start();
                ADFSController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));
                this.ServiceStatusChanged(this, ServiceOperationStatus.OperationRunning, servername);
                using (MailSlotClient mailslot = new MailSlotClient("MGT"))
                {
                    mailslot.Text = servername;
                    mailslot.SendNotification(0x10);
                }
                return true;
            }
            catch (Exception)
            {
                this.ServiceStatusChanged(this, ServiceOperationStatus.OperationInError, servername);
                using (MailSlotClient mailslot = new MailSlotClient("MGT"))
                {
                    mailslot.Text = servername;
                    mailslot.SendNotification(0x19);
                }
                return false;
            }
            finally
            {
                ADFSController.Close();
            }
        }

        /// <summary>
        /// internalStopService method implementation
        /// </summary>
        private bool internalStopService(string servername = "local")
        {
            ServiceController ADFSController = null;
            try
            {
                this.ServiceStatusChanged(this, _status, servername);
                if (servername.ToLowerInvariant().Equals("local"))
                    ADFSController = new ServiceController("adfssrv");
                else
                    ADFSController = new ServiceController("adfssrv", servername);
                using (MailSlotClient mailslot = new MailSlotClient("MGT"))
                {
                    mailslot.Text = servername;
                    mailslot.SendNotification(0x12);
                }
                this.ServiceStatusChanged(this, ServiceOperationStatus.OperationPending, servername);
                if (ADFSController.Status != ServiceControllerStatus.Stopped)
                    ADFSController.Stop();
                ADFSController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 1, 0));
                this.ServiceStatusChanged(this, ServiceOperationStatus.OperationStopped, servername);
                using (MailSlotClient mailslot = new MailSlotClient("MGT"))
                {
                    mailslot.Text = servername;
                    mailslot.SendNotification(0x11);
                }
                return true;
            }
            catch (Exception)
            {
                this.ServiceStatusChanged(this, ServiceOperationStatus.OperationInError, servername);
                using (MailSlotClient mailslot = new MailSlotClient("MGT"))
                {
                    mailslot.Text = servername;
                    mailslot.SendNotification(0x19);
                }
                return false;
            }
            finally
            {
                ADFSController.Close();
            }
        }
        #endregion

        #region MFA Configuration Store
        /// <summary>
        /// ReadConfiguration method implementation
        /// </summary>
        public MFAConfig ReadConfiguration(PSHost Host = null)
        {
            this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigIsDirty);
            try
            {
                EnsureLocalService();
                _config = CFGUtilities.ReadConfiguration(Host);
                _config.IsDirty = false;
#if hardcheck
                if (this.IsMFAProviderEnabled(Host))
#else
                if (_config != null)
#endif
                    this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigLoaded);
                else
                    this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigStopped);
            }
            catch (CmdletInvocationException cm)
            {
                this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigInError, cm);
                throw new CmdletInvocationException(SErrors.ErrorMFAFarmNotInitialized, cm);
            }
            catch (Exception ex)
            {
                this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigInError, ex);
            }
            return _config;
        }

        /// <summary>
        /// WriteConfiguration method implmentation
        /// </summary>
        public void WriteConfiguration(PSHost Host = null)
        {
            EnsureConfiguration(Host);
            this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigIsDirty);
            try
            {
                EnsureLocalService();
                _config.IsDirty = false;
                CFGUtilities.WriteConfiguration(Host, _config);
                this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigSaved);
                using (MailSlotClient mailslot = new MailSlotClient())
                {
                    mailslot.SendNotification(0xAA);
                }
            }
            catch (Exception ex)
            {
                this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigInError, ex);
            }
        }

        /// <summary>
        /// internalRegisterConfiguration method implementation
        /// </summary>
        private void internalRegisterConfiguration(PSHost Host)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string pth = Path.GetTempPath() + Path.GetRandomFileName();
            try
            {
                FileStream stm = new FileStream(pth, FileMode.CreateNew, FileAccess.ReadWrite);
                XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                stm.Position = 0;
                using (StreamReader reader = new StreamReader(stm))
                {
                    xmlserializer.Serialize(stm, Config);
                }
                try
                {
                    RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                    SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                    SPPowerShell = PowerShell.Create();
                    SPPowerShell.Runspace = SPRunSpace;
                    SPRunSpace.Open();

                    Pipeline pipeline = SPRunSpace.CreatePipeline();
                    Command exportcmd = new Command("Register-AdfsAuthenticationProvider", false);
                    CommandParameter NParam = new CommandParameter("Name", "MultiFactorAuthenticationProvider");
                    exportcmd.Parameters.Add(NParam);
                    CommandParameter TParam = new CommandParameter("TypeName", "Neos.IdentityServer.MultiFactor.AuthenticationProvider, Neos.IdentityServer.MultiFactor, Version=2.4.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2");
                    exportcmd.Parameters.Add(TParam);
                    CommandParameter PParam = new CommandParameter("ConfigurationFilePath", pth);
                    exportcmd.Parameters.Add(PParam);
                    pipeline.Commands.Add(exportcmd);
                    Collection<PSObject> PSOutput = pipeline.Invoke();
                    if (Host != null)
                        Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : Registered");
                }
                finally
                {
                    if (SPRunSpace != null)
                        SPRunSpace.Close();
                }
            }
            finally
            {
                if (File.Exists(pth))
                    File.Delete(pth);
            }
            return;
        }

        /// <summary>
        /// internalUnRegisterConfiguration method implementation
        /// </summary>
        private void internalUnRegisterConfiguration(PSHost Host)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("UnRegister-AdfsAuthenticationProvider", false);
                CommandParameter NParam = new CommandParameter("Name", "MultiFactorAuthenticationProvider");
                exportcmd.Parameters.Add(NParam);
                CommandParameter CParam = new CommandParameter("Confirm", false);
                exportcmd.Parameters.Add(CParam);

                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : Removed");
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return ;
        }

        /// <summary>
        /// internalExportConfiguration method implmentation
        /// </summary>
        private void internalExportConfiguration(PSHost Host, string backupfile)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string pth = string.Empty;
            if (string.IsNullOrEmpty(backupfile))
                pth = Path.GetTempPath() + "adfsmfa_backup_" + DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm") + ".xml";
            else
                pth = backupfile;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("Export-AdfsAuthenticationProviderConfigurationData", false);
                CommandParameter NParam = new CommandParameter("Name", "MultiFactorAuthenticationProvider");
                exportcmd.Parameters.Add(NParam);
                CommandParameter PParam = new CommandParameter("FilePath", pth);
                exportcmd.Parameters.Add(PParam);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA Configuration saved to => "+pth);
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
        }

        /// <summary>
        /// internalImportConfiguration method implmentation
        /// </summary>
        private void internalImportConfiguration(PSHost Host, string importfile)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("Import-AdfsAuthenticationProviderConfigurationData", false);
                CommandParameter NParam = new CommandParameter("Name", "MultiFactorAuthenticationProvider");
                exportcmd.Parameters.Add(NParam);
                CommandParameter PParam = new CommandParameter("FilePath", importfile);
                exportcmd.Parameters.Add(PParam);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA Configuration saved to => " + importfile);
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
        }

        /// <summary>
        /// internalActivateConfiguration method implementation
        /// </summary>
        private void internalActivateConfiguration(PSHost Host)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            bool found = false;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-AdfsGlobalAuthenticationPolicy).AdditionalAuthenticationProvider", true);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                List<string> lst = new List<string>();
                try
                {
                    foreach (var result in PSOutput)
                    {
                        if (result.BaseObject.ToString().ToLower().Equals("multifactorauthenticationprovider"))
                        {
                            found = true;
                            break;
                        }
                        else
                        {
                            lst.Add(result.BaseObject.ToString());
                        }
                    }
                }
                catch (Exception)
                {
                    found = false;
                }
                if (!found)
                {
                    lst.Add("MultiFactorAuthenticationProvider");
                    Pipeline pipeline2 = SPRunSpace.CreatePipeline();
                    Command exportcmd2 = new Command("Set-AdfsGlobalAuthenticationPolicy", false);
                    pipeline2.Commands.Add(exportcmd2);
                    CommandParameter NParam = new CommandParameter("AdditionalAuthenticationProvider", lst);
                    exportcmd2.Parameters.Add(NParam);
                    Collection<PSObject> PSOutput2 = pipeline2.Invoke();
                    if (Host != null)
                        Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : Enabled");
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
        }

        /// <summary>
        /// internalDeActivateConfiguration method implementation
        /// </summary>
        private void internalDeActivateConfiguration(PSHost Host)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            bool found = false;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-AdfsGlobalAuthenticationPolicy).AdditionalAuthenticationProvider", true);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                List<string> lst = new List<string>();
                try
                {
                    foreach (var result in PSOutput)
                    {
                        if (result.BaseObject.ToString().ToLower().Equals("multifactorauthenticationprovider"))
                        {
                            found = true;
                            break;
                        }
                        else
                        {
                            lst.Add(result.BaseObject.ToString());
                        }
                    }
                }
                catch (Exception)
                {
                    found = false;
                }
                if (found)
                {
                    if (lst.Count == 0)
                        lst = null;
                    Pipeline pipeline2 = SPRunSpace.CreatePipeline();
                    Command exportcmd2 = new Command("Set-AdfsGlobalAuthenticationPolicy", false);
                    pipeline2.Commands.Add(exportcmd2);
                    CommandParameter NParam = new CommandParameter("AdditionalAuthenticationProvider", lst);
                    exportcmd2.Parameters.Add(NParam);
                    Collection<PSObject> PSOutput2 = pipeline2.Invoke();
                    if (Host != null)
                        Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : Disabled");
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
        }

        /// <summary>
        /// internalIsConfigurationActive method implementation
        /// </summary>
        private bool internalIsConfigurationActive(PSHost Host)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            bool found = false;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-AdfsGlobalAuthenticationPolicy).AdditionalAuthenticationProvider", true);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                try
                {
                    foreach (var result in PSOutput)
                    {
                        if (result.BaseObject.ToString().ToLower().Equals("multifactorauthenticationprovider"))
                        {
                            found = true;
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    found = false;
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return found;
        }

        /// <summary>
        /// SetADFSTheme method implementation
        /// </summary>
        internal void SetADFSTheme(PSHost pSHost, string themename, bool paginated, bool supports2019)
        {
            internalSetADFSTheme(pSHost, themename, paginated, supports2019);
        }

        /// <summary>
        /// internalSetADFSTheme method implementation
        /// </summary>
        private void internalSetADFSTheme(PSHost Host, string themename, bool paginated, bool supports2019)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                if (supports2019)
                {
                    Pipeline pipeline = SPRunSpace.CreatePipeline();
                    Command policycmd = new Command("Set-AdfsGlobalAuthenticationPolicy", false);
                    CommandParameter PParam = new CommandParameter("EnablePaginatedAuthenticationPages", paginated);
                    policycmd.Parameters.Add(PParam);
                    CommandParameter CParam = new CommandParameter("Force", true);
                    policycmd.Parameters.Add(CParam);
                    pipeline.Commands.Add(policycmd);

                    Collection<PSObject> PSOutput = pipeline.Invoke();
                    if (Host != null)
                        Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " ADFS MFA Pagination : Changed");
                }

                Pipeline pipeline2 = SPRunSpace.CreatePipeline();
                Command themecmd = new Command("Set-AdfsWebConfig", false);
                CommandParameter NParam = new CommandParameter("ActiveThemeName", themename);
                themecmd.Parameters.Add(NParam);
                pipeline2.Commands.Add(themecmd);

                Collection<PSObject> PSOutput2 = pipeline2.Invoke();
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " ADFS MFA Theme : Changed");
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return;
        }


        /// <summary>
        /// RegisterNewRSACertificate method implmentation
        /// </summary>
        public string RegisterNewRSACertificate(PSHost Host = null, int years = 5, bool restart = true)
        {
            if (_config.KeysConfig.KeyFormat == SecretKeyFormat.RSA)
            {
                _config.KeysConfig.CertificateThumbprint = internalRegisterNewRSACertificate(Host, years);
                _config.IsDirty = true;
                CFGUtilities.WriteConfiguration(Host, _config);
                if (restart)
                    RestartFarm(Host);
                return _config.KeysConfig.CertificateThumbprint;
            }
            else
            {
                if (Host != null)
                    Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System : Configuration is not RSA ! no action taken !");
                else
                    throw new Exception("MFA System : Configuration is not RSA ! no action taken !");
                return "";
            }
        }

        /// <summary>
        /// RegisterNewRSACertificate method implmentation
        /// </summary>
        public string RegisterNewSQLCertificate(PSHost Host = null, int years = 5, string keyname = "adfsmfa")
        {
            _config.Hosts.SQLServerHost.ThumbPrint = internalRegisterNewSQLCertificate(Host, years, keyname);
            _config.IsDirty = true;
            CFGUtilities.WriteConfiguration(Host, _config);
            return _config.Hosts.SQLServerHost.ThumbPrint;
        }

        /// <summary>
        /// CheckCertificate metnod implementation
        /// </summary>
        internal bool CheckCertificate(string thumbprint)
        {
            X509Certificate2 cert = Certs.GetCertificate(thumbprint, StoreLocation.LocalMachine);
            return (cert != null);
        }

        /// <summary>
        /// internalRegisterNewRSACertificate method implementation
        /// </summary>
        private string internalRegisterNewRSACertificate(PSHost Host, int years)
        {
            X509Certificate2 cert = Certs.CreateSelfSignedCertificate("MFA RSA Keys", years);
            if (cert != null)
            {
                string thumbprint = cert.Thumbprint;
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA Certificate \"" + thumbprint + "\" Created for using with RSA keys");
              /*  Runspace SPRunSpace = null;
                PowerShell SPPowerShell = null;
                try
                {
                    RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                    SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                    SPPowerShell = PowerShell.Create();
                    SPPowerShell.Runspace = SPRunSpace;
                    SPRunSpace.Open();

                    Pipeline pipeline = SPRunSpace.CreatePipeline();
                    Command exportcmd = new Command("Add-AdfsCertificate -CertificateType \"Token-Decrypting\" -Thumbprint \""+thumbprint+"\"", true);
                    pipeline.Commands.Add(exportcmd);

                    Collection<PSObject> PSOutput = pipeline.Invoke();
                    if (Host != null)
                        Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA Certificate \"" + thumbprint + "\" Added to ADFS Decrypting Certificates list");
                }
                catch (CmdletInvocationException) // if Rollover is enabled cannot add 
                {
                    if (Host != null)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " Error adding certificate \"" + thumbprint + "\" to ADFS Decrypting Certificates list, your must do it manually !");
                }
                catch (Exception)
                {
                    if (Host != null)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " Error adding certificate \"" + thumbprint + "\" to ADFS Decrypting Certificates list, your must do it manually !");
                }
                finally
                {
                    if (SPRunSpace != null)
                        SPRunSpace.Close();
                } */
                return thumbprint;
            }
            else
                return "";
        }

        /// <summary>
        /// internalRegisterNewSQLCertificate method implementation
        /// </summary>
        private string internalRegisterNewSQLCertificate(PSHost Host, int years, string keyname)
        {
            X509Certificate2 cert = Certs.CreateSelfSignedCertificateForSQLEncryption("MFA SQL Key : "+keyname, years);
            if (cert != null)
            {
                string thumbprint = cert.Thumbprint;
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA Certificate \"" + thumbprint + "\" Created for using with SQL keys");
              /*  Runspace SPRunSpace = null;
                PowerShell SPPowerShell = null;
                try
                {
                    RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                    SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                    SPPowerShell = PowerShell.Create();
                    SPPowerShell.Runspace = SPRunSpace;
                    SPRunSpace.Open();

                    Pipeline pipeline = SPRunSpace.CreatePipeline();
                    Command exportcmd = new Command("Add-AdfsCertificate -CertificateType \"Token-Decrypting\" -Thumbprint \"" + thumbprint + "\"", true);
                    pipeline.Commands.Add(exportcmd);

                    Collection<PSObject> PSOutput = pipeline.Invoke();
                    if (Host != null)
                        Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " SQL Certificate \"" + thumbprint + "\" Added to ADFS Decrypting Certificates list");
                }
                catch (CmdletInvocationException) // if Rollover is enabled cannot add 
                {
                    if (Host != null)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " Error adding certificate \"" + thumbprint + "\" to ADFS Decrypting Certificates list, your must do it manually !");
                }
                catch (Exception)
                {
                    if (Host != null)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " Error adding certificate \"" + thumbprint + "\" to ADFS Decrypting Certificates list, your must do it manually !");
                }
                finally
                {
                    if (SPRunSpace != null)
                        SPRunSpace.Close();
                } */
                return thumbprint;
            }
            else
                return "";
        }

        /// <summary>
        /// RegisterMFAProvider method implmentation
        /// </summary>
        public bool RegisterMFAProvider(PSHost host, bool activate = true, bool restartfarm = true, bool upgrade = false, string filepath = null, SecretKeyFormat format = SecretKeyFormat.RNG, int certduration = 5)
        {
            bool needregister = false;
            if (_config == null)
            {
                EnsureLocalService();
                try
                {
                    _config = ReadConfiguration(host);
                }
                catch (CmdletInvocationException)
                {
                    needregister = true;
                    _config = null;
                }
                if ((_config == null) && (!upgrade))
                    _config = new MFAConfig(true);
            }
            if ((_config != null) && (upgrade))
                _config.UpgradeDefaults();
            else if ((!upgrade) && (IsFarmConfigured()))
                return false;

            if ((_config != null) && (upgrade))
                internalExportConfiguration(host, filepath);
            else if ((_config == null) && (upgrade))
                throw new Exception("You Cannot use allowupgrade switch because MFA-System is not initialized ! You can also import a backup configuration with the cmdlet Import-MFASystemConfiguration");

            try
            {
                if (_config.KeysConfig.KeyFormat != format)
                {
                    _config.KeysConfig.KeyFormat = format;
                    _config.IsDirty = true;
                }
                if ((format == SecretKeyFormat.RSA) && (!Thumbprint.IsAllowed(Config.KeysConfig.CertificateThumbprint)))
                {
                    _config.KeysConfig.CertificateThumbprint = internalRegisterNewRSACertificate(host, certduration);
                    _config.IsDirty = true;
                }
                if (format == SecretKeyFormat.CUSTOM) 
                {
                    _config.KeysConfig.CertificateValidity = certduration;
                    _config.IsDirty = true;
                }
                if (_config.IsDirty)
                {
                    if (needregister)
                        internalRegisterConfiguration(host);
                    CFGUtilities.WriteConfiguration(host, _config);
                }
                else
                    needregister = false;
            }
            finally
            {
                _config.IsDirty = false;
            }
            if (upgrade)
            {
                internalDeActivateConfiguration(host);
                internalUnRegisterConfiguration(host);
                internalRegisterConfiguration(host);
                internalActivateConfiguration(host);
            }
            else
            {
                if (!needregister)
                    internalRegisterConfiguration(host);
                if (activate)
                    internalActivateConfiguration(host);
            }
            InitFarmConfiguration(host);
            WriteConfiguration(host);
            if (restartfarm)
                RestartFarm(host);
            return true;
        }

        /// <summary>
        /// UnRegisterMFAProvider method implmentation
        /// </summary>
        public void UnRegisterMFAProvider(PSHost Host, string filepath, bool restartfarm = true)
        {
            if (_config == null)
            {
                EnsureConfiguration(Host);
                _config = ReadConfiguration(Host);
            }
            if (!string.IsNullOrEmpty(filepath))
                internalExportConfiguration(Host, filepath);
            internalDeActivateConfiguration(Host);
            internalUnRegisterConfiguration(Host);
            if (restartfarm)
                RestartFarm(Host);
            _config = null;
        }

        /// <summary>
        /// ImportMFAProviderConfiguration method implementation
        /// </summary>
        public bool ImportMFAProviderConfiguration(PSHost Host, bool activate, bool restartfarm, string importfile)
        {
            if (_config == null)
            {
                EnsureLocalService();
                _config = ReadConfiguration(Host);
            }
            this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigIsDirty);
            try
            {
                internalImportConfiguration(Host, importfile);
                using (MailSlotClient mailslot = new MailSlotClient())
                {
                    mailslot.SendNotification(0xAA);
                }
                this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigSaved);
                _config = ReadConfiguration(Host);
                if (KeysManager.IsLoaded)
                {
                    if (activate)
                        internalActivateConfiguration(Host);
                    if (restartfarm)
                        RestartFarm(Host);
                }
                else
                    throw new NotSupportedException("Invalid key manager !");
            }
            catch (NotSupportedException ex)
            {
                this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigInError, ex);
                return false;
            }
            catch (Exception ex)
            {
                this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigInError, ex);
                throw ex;
            }
            return true;
        }

        /// <summary>
        /// ExportMFAProviderConfiguration method implementation
        /// </summary>
        public void ExportMFAProviderConfiguration(PSHost Host, string exportfilepath)
        {
            if (_config == null)
            {
                EnsureConfiguration(Host);
                _config = ReadConfiguration(Host);
            }
            internalExportConfiguration(Host, exportfilepath);
        }

        /// <summary>
        /// EnableMFAProvider method implmentation
        /// </summary>
        public void EnableMFAProvider(PSHost Host)
        {
            if (_config == null)
            {
                EnsureLocalService();
                _config = ReadConfiguration(Host);
                if (_config == null)
                    _config = new MFAConfig(true);
            }
            if (!_config.Hosts.ADFSFarm.IsInitialized)
                throw new Exception(SErrors.ErrorMFAFarmNotInitialized);
            internalActivateConfiguration(Host);
            using (MailSlotClient mailslot = new MailSlotClient())
            {
                mailslot.SendNotification(0xAA);
            }
        }

        /// <summary>
        /// DisableMFAProvider method implmentation
        /// </summary>
        public void DisableMFAProvider(PSHost Host)
        {
            if (_config == null)
            {
                EnsureConfiguration(Host);
                _config = ReadConfiguration(Host);
            }
            internalDeActivateConfiguration(Host);
            this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigStopped);
            using (MailSlotClient mailslot = new MailSlotClient())
            {
                mailslot.SendNotification(0xAA);
            }
        }

        /// <summary>
        /// IsMFAProviderEnabled method implmentation
        /// </summary>
        public bool IsMFAProviderEnabled(PSHost Host)
        {
            if (_config == null)
            {
                EnsureConfiguration(Host);
                _config = ReadConfiguration(Host);
            }
            return internalIsConfigurationActive(Host);
        }
        #endregion

        /// <summary>
        /// RegisterADFSComputer method implementation
        /// </summary>        
        public void RegisterADFSComputer(PSHost Host, string servername)
        {
            EnsureConfiguration(Host);
            try
            {
                using (MailSlotClient mailslot = new MailSlotClient("CP1", true))
                {
                    mailslot.Text = Dns.GetHostEntry(servername).HostName;
                    mailslot.SendNotification(0x10);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return;
        }

        /// <summary>
        /// RegisterADFSComputer method implementation
        /// </summary>        
        public ADFSServerHost RegisterADFSComputer(PSHost Host)
        {
            ADFSServerHost result = null;
            EnsureConfiguration(Host);
            try
            {
                result = InitServerNodeConfiguration(Host);
                SetDirty(true);
                WriteConfiguration(Host);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// UnRegisterADFSComputer method implementation
        /// </summary>
        public void UnRegisterADFSComputer(PSHost Host, string servername)
        {
            EnsureConfiguration(Host);
            try
            {
                string fqdn = Dns.GetHostEntry(servername).HostName;
                ADFSFarm.Servers.RemoveAll(c => c.FQDN.ToLower() == fqdn.ToLower());
                SetDirty(true);
                WriteConfiguration(Host);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return;
        }

        #region Farm Configuration
        /// <summary>
        /// InitFarmNodesConfiguration method implementation
        /// </summary>
        private ADFSServerHost InitFarmConfiguration(PSHost Host)
        {
            ADFSServerHost result = null;
            try
            {
                RegistryVersion reg = new RegistryVersion();
                InitFarmProperties(Host);
                if (reg.IsWindows2019)
                {
                    result = InitServerNodeConfiguration2019(Host, reg);
                }
                else if (reg.IsWindows2016)
                {
                    result = InitServerNodeConfiguration2016(Host, reg);
                }
                else if (reg.IsWindows2012R2)
                {
                    result = InitServerNodeConfiguration2012(Host, reg);
                }
                ADFSFarm.IsInitialized = true;
                SetDirty(true);
            }
            catch (Exception ex)
            {
                ADFSFarm.IsInitialized = false;
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// InitFarmProperties method implementation
        /// </summary>
        private void InitFarmProperties(PSHost Host)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline2 = SPRunSpace.CreatePipeline();
                Command exportcmd2 = new Command("(Get-AdfsFarmInformation).CurrentFarmBehavior", true);
                pipeline2.Commands.Add(exportcmd2);

                try
                {
                    Collection<PSObject> PSOutput2 = pipeline2.Invoke();
                    foreach (var result in PSOutput2)
                    {
                        ADFSFarm.CurrentFarmBehavior = Convert.ToInt32(result.BaseObject);
                        break;
                    }
                }
                catch (Exception)
                {
                    ADFSFarm.CurrentFarmBehavior = 1;
                }

                Pipeline pipeline3 = SPRunSpace.CreatePipeline();
                Command exportcmd3 = new Command("(Get-ADFSProperties).Identifier.OriginalString", true);
                pipeline3.Commands.Add(exportcmd3);

                Collection<PSObject> PSOutput3 = pipeline3.Invoke();
                foreach (var result in PSOutput3)
                {
                    ADFSFarm.FarmIdentifier = result.BaseObject.ToString();
                    break;
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return;
        }
        #endregion

        #region Servers Configuration
        /// <summary>
        /// InitFarmNodeConfiguration method implementation
        /// </summary>        
        private ADFSServerHost InitServerNodeConfiguration(PSHost Host)
        {
            ADFSServerHost result = null;
            if (!ADFSFarm.IsInitialized)
                throw new Exception(SErrors.ErrorMFAFarmNotInitialized);
            EnsureConfiguration(Host);
            try
            {
                RegistryVersion reg = new RegistryVersion();
                if (reg.IsWindows2019)
                    result = InitServerNodeConfiguration2019(Host, reg);
                else if (reg.IsWindows2016)
                    result = InitServerNodeConfiguration2016(Host, reg);
                else if (reg.IsWindows2012R2)
                    result = InitServerNodeConfiguration2012(Host, reg);
                SetDirty(true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// InitServerNodeConfiguration2012 method implementation
        /// </summary>
        private ADFSServerHost InitServerNodeConfiguration2012(PSHost Host, RegistryVersion reg)
        {

            string nodetype = string.Empty;
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);
                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-AdfsSyncProperties).Role", true);
                pipeline.Commands.Add(exportcmd);

                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var result in PSOutput)
                {
                    nodetype = result.BaseObject.ToString();
                    break;
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }

            ADFSServerHost props = new ADFSServerHost();
            props.FQDN = Dns.GetHostEntry("LocalHost").HostName;
            props.BehaviorLevel = 1;
            props.HeartbeatTmeStamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0, DateTimeKind.Local);
            props.NodeType = nodetype;
            props.CurrentVersion = reg.CurrentVersion;
            props.CurrentBuild = reg.CurrentBuild;
            props.InstallationType = reg.InstallationType;
            props.ProductName = reg.ProductName;
            props.CurrentMajorVersionNumber = reg.CurrentMajorVersionNumber;
            props.CurrentMinorVersionNumber = reg.CurrentMinorVersionNumber;
            int i = ADFSFarm.Servers.FindIndex(c => c.FQDN.ToLower() == props.FQDN.ToLower());
            if (i < 0)
                ADFSFarm.Servers.Add(props);
            else
                ADFSFarm.Servers[i] = props;
            return props;
        }

        /// <summary>
        /// InitServerNodeConfiguration2016 method implementation
        /// </summary>
        private ADFSServerHost InitServerNodeConfiguration2016(PSHost Host, RegistryVersion reg)
        {
            ADFSServerHost xprops = null;
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);
                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-AdfsFarmInformation).FarmNodes", true);

                pipeline.Commands.Add(exportcmd);

                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var result in PSOutput)
                {
                    string fqdn = result.Members["FQDN"].Value.ToString();
                    ADFSServerHost props = new ADFSServerHost();
                    props.FQDN = fqdn;
                    props.BehaviorLevel = Convert.ToInt32(result.Members["BehaviorLevel"].Value);
                    props.HeartbeatTmeStamp = Convert.ToDateTime(result.Members["HeartbeatTimeStamp"].Value);
                    props.NodeType = result.Members["NodeType"].Value.ToString();
                    props.CurrentVersion = reg.CurrentVersion;
                    props.CurrentBuild = reg.CurrentBuild;
                    props.InstallationType = reg.InstallationType;
                    props.ProductName = reg.ProductName;
                    props.CurrentMajorVersionNumber = reg.CurrentMajorVersionNumber;
                    props.CurrentMinorVersionNumber = reg.CurrentMinorVersionNumber;
                    int i = ADFSFarm.Servers.FindIndex(c => c.FQDN.ToLower() == props.FQDN.ToLower());
                    if (i<0)
                        ADFSFarm.Servers.Add(props);
                    else
                        ADFSFarm.Servers[i] = props;
                    xprops = props;
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return xprops;
        }

        /// <summary>
        /// InitServerNodeConfiguration2019 method implementation
        /// </summary>
        private ADFSServerHost InitServerNodeConfiguration2019(PSHost Host, RegistryVersion reg)
        {
            ADFSServerHost xprops = null;
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);
                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("(Get-AdfsFarmInformation).FarmNodes", true);

                pipeline.Commands.Add(exportcmd);

                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var result in PSOutput)
                {
                    string fqdn = result.Members["FQDN"].Value.ToString();
                    ADFSServerHost props = new ADFSServerHost();
                    props.FQDN = fqdn;
                    props.BehaviorLevel = Convert.ToInt32(result.Members["BehaviorLevel"].Value);
                    props.HeartbeatTmeStamp = Convert.ToDateTime(result.Members["HeartbeatTimeStamp"].Value);
                    props.NodeType = result.Members["NodeType"].Value.ToString();
                    props.CurrentVersion = reg.CurrentVersion;
                    props.CurrentBuild = reg.CurrentBuild;
                    props.InstallationType = reg.InstallationType;
                    props.ProductName = reg.ProductName;
                    props.CurrentMajorVersionNumber = reg.CurrentMajorVersionNumber;
                    props.CurrentMinorVersionNumber = reg.CurrentMinorVersionNumber;
                    int i = ADFSFarm.Servers.FindIndex(c => c.FQDN.ToLower() == props.FQDN.ToLower());
                    if (i < 0)
                        ADFSFarm.Servers.Add(props);
                    else
                        ADFSFarm.Servers[i] = props;
                    xprops = props;
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return xprops;
        }

        #endregion

        #region MFA Database
        /// <summary>
        /// CreateMFADatabase method implementation
        /// </summary>
        public string CreateMFADatabase(PSHost host, string _servername, string _databasename, string _username, string _password)
        {
            string sqlscript = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\MFA\mfa-db.sql");
            sqlscript = sqlscript.Replace("%DATABASENAME%", _databasename);
            SqlConnection cnx = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=master;Data Source=" + _servername);
            cnx.Open();
            try
            {
                SqlCommand cmd = new SqlCommand(string.Format("CREATE DATABASE {0}", _databasename), cnx);
                cmd.ExecuteNonQuery();
                SqlCommand cmdl = null;
                if (!string.IsNullOrEmpty(_password))
                    cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] WITH PASSWORD = '{1}', DEFAULT_DATABASE=[master] END", _username, _password), cnx);
                else
                    cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[master] END", _username), cnx);
                cmdl.ExecuteNonQuery();
            }
            finally
            {
                cnx.Close();
            }
            SqlConnection cnx2 = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=" + _databasename + ";Data Source=" + _servername);
            cnx2.Open();
            try
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(string.Format("CREATE USER [{0}] FOR LOGIN [{0}]", _username), cnx2);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }
                try
                {
                    SqlCommand cmd1 = new SqlCommand(string.Format("ALTER ROLE [db_owner] ADD MEMBER [{0}]", _username), cnx2);
                    cmd1.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }
                try
                {
                    SqlCommand cmd2 = new SqlCommand(string.Format("ALTER ROLE [db_securityadmin] ADD MEMBER [{0}]", _username), cnx2);
                    cmd2.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }

                SqlCommand cmdl = new SqlCommand(sqlscript, cnx2); // Create Tables and more
                cmdl.ExecuteNonQuery();
            }
            finally
            {
                cnx2.Close();
            }
            FlatConfigSQL  cf = new FlatConfigSQL();
            cf.Load(host);
            if (!string.IsNullOrEmpty(_password))
                cf.ConnectionString = "Persist Security Info=True;User ID="+ _username+";Password="+_password+";Initial Catalog=" + _databasename + ";Data Source=" + _servername;
            else
                cf.ConnectionString = "Persist Security Info=False;Integrated Security=SSPI;Initial Catalog=" + _databasename + ";Data Source=" + _servername;
            cf.IsAlwaysEncrypted = false;
            cf.ThumbPrint = string.Empty;
            cf.Update(host);
            return cf.ConnectionString;
        }

        /// <summary>
        /// CreateMFAEncryptedDatabase method implementation
        /// </summary>
        public string CreateMFAEncryptedDatabase(PSHost host, string _servername, string _databasename, string _username, string _password, string _keyname, string _thumbprint)
        {
            string _encrypted = GetSQLKeyEncryptedValue("LocalMachine/my/" + _thumbprint.ToUpper());
            string sqlscript = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\MFA\mfa-db-encrypted.sql");
            sqlscript = sqlscript.Replace("%DATABASENAME%", _databasename);
            sqlscript = sqlscript.Replace("%SQLKEY%", _keyname);
            SqlConnection cnx = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=master;Data Source=" + _servername);
            cnx.Open();
            try
            {
                SqlCommand cmd = new SqlCommand(string.Format("CREATE DATABASE {0}", _databasename), cnx);
                cmd.ExecuteNonQuery();
                SqlCommand cmdl = null;
                if (!string.IsNullOrEmpty(_password))
                    cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] WITH PASSWORD = '{1}', DEFAULT_DATABASE=[master] END", _username, _password), cnx);
                else
                    cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[master] END", _username), cnx);
                cmdl.ExecuteNonQuery();
            }
            finally
            {
                cnx.Close();
            }
            SqlConnection cnx2 = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=" + _databasename + ";Data Source=" + _servername);
            cnx2.Open();
            try
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(string.Format("CREATE USER [{0}] FOR LOGIN [{0}]", _username), cnx2);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }
                try
                {
                    SqlCommand cmd1 = new SqlCommand(string.Format("ALTER ROLE [db_owner] ADD MEMBER [{0}]", _username), cnx2);
                    cmd1.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }
                try
                {
                    SqlCommand cmd2 = new SqlCommand(string.Format("ALTER ROLE [db_securityadmin] ADD MEMBER [{0}]", _username), cnx2);
                    cmd2.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }
                try
                {
                    SqlCommand cmd3 = new SqlCommand(string.Format("GRANT ALTER ANY COLUMN ENCRYPTION KEY TO [{0}]", _username), cnx2);
                    cmd3.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }
                SqlCommand cmd4 = new SqlCommand(string.Format("CREATE COLUMN MASTER KEY [{0}] WITH (KEY_STORE_PROVIDER_NAME = 'MSSQL_CERTIFICATE_STORE', KEY_PATH = 'LocalMachine/My/{1}')", _keyname, _thumbprint.ToUpper()), cnx2);
                cmd4.ExecuteNonQuery();
                SqlCommand cmd5 = new SqlCommand(string.Format("CREATE COLUMN ENCRYPTION KEY [{0}] WITH VALUES (COLUMN_MASTER_KEY = [{0}], ALGORITHM = 'RSA_OAEP', ENCRYPTED_VALUE = {1})", _keyname, _encrypted), cnx2);
                cmd5.ExecuteNonQuery();
            }
            finally
            {
                cnx2.Close();
            }
            SqlConnection cnx3 = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=" + _databasename + ";Data Source=" + _servername);
            cnx3.Open();
            try
            {
                SqlCommand cmdl = new SqlCommand(sqlscript, cnx3); // create tables and more
                cmdl.ExecuteNonQuery();
            }
            finally
            {
                cnx3.Close();
            }

            FlatConfigSQL cf = new FlatConfigSQL();
            cf.Load(host);
            if (!string.IsNullOrEmpty(_password))
                cf.ConnectionString = "Persist Security Info=True;User ID=" + _username + ";Password=" + _password + ";Initial Catalog=" + _databasename + ";Data Source=" + _servername +";Column Encryption Setting=enabled";
            else
                cf.ConnectionString = "Persist Security Info=False;Integrated Security=SSPI;Initial Catalog=" + _databasename + ";Data Source=" + _servername + ";Column Encryption Setting=enabled";
            cf.IsAlwaysEncrypted = true;
            cf.ThumbPrint = _thumbprint;
            cf.Update(host);
            return cf.ConnectionString;
        }

        /// <summary>
        /// GetSQLKeyEncryptedValue method implementation
        /// </summary>
        private string GetSQLKeyEncryptedValue(string masterkeypath)
        {
            var randomBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            var provider = new SqlColumnEncryptionCertificateStoreProvider();
            var encryptedKey = provider.EncryptColumnEncryptionKey(masterkeypath, "RSA_OAEP", randomBytes);
            return "0x" + BitConverter.ToString(encryptedKey).Replace("-", "");
        }
        #endregion

        #region MFA SecretKey Database
        /// <summary>
        /// CreateMFADatabase method implementation
        /// </summary>
        public string CreateMFASecretKeysDatabase(PSHost host, string _servername, string _databasename, string _username, string _password)
        {
            string sqlscript = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\MFA\mfa-secretkey-db.sql");
            sqlscript = sqlscript.Replace("%DATABASENAME%", _databasename);
            SqlConnection cnx = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=master;Data Source=" + _servername);
            cnx.Open();
            try
            {
                SqlCommand cmd = new SqlCommand(string.Format("CREATE DATABASE {0}", _databasename), cnx);
                cmd.ExecuteNonQuery();
                SqlCommand cmdl = null;
                if (!string.IsNullOrEmpty(_password))
                    cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] WITH PASSWORD = '{1}', DEFAULT_DATABASE=[master] END", _username, _password), cnx);
                else
                    cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[master] END", _username), cnx);
                cmdl.ExecuteNonQuery();
            }
            finally
            {
                cnx.Close();
            }
            SqlConnection cnx2 = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=" + _databasename + ";Data Source=" + _servername);
            cnx2.Open();
            try
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(string.Format("CREATE USER [{0}] FOR LOGIN [{0}]", _username), cnx2);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }
                try
                {
                    SqlCommand cmd1 = new SqlCommand(string.Format("ALTER ROLE [db_owner] ADD MEMBER [{0}]", _username), cnx2);
                    cmd1.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }
                try
                {
                    SqlCommand cmd2 = new SqlCommand(string.Format("ALTER ROLE [db_securityadmin] ADD MEMBER [{0}]", _username), cnx2);
                    cmd2.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }
                SqlCommand cmdl = new SqlCommand(sqlscript, cnx2); // Create Tables and more
                cmdl.ExecuteNonQuery();
            }
            finally
            {
                cnx2.Close();
            }
            FlatExternalKeyManager cf = new FlatExternalKeyManager();
            cf.Load(host);
            if (!string.IsNullOrEmpty(_password))
                cf.ConnectionString = "Persist Security Info=True;User ID=" + _username + ";Password=" + _password + ";Initial Catalog=" + _databasename + ";Data Source=" + _servername;
            else
                cf.ConnectionString = "Persist Security Info=False;Integrated Security=SSPI;Initial Catalog=" + _databasename + ";Data Source=" + _servername;
            cf.IsAlwaysEncrypted = false;
            cf.ThumbPrint = string.Empty;
            cf.Update(host);
            return cf.Parameters;
        }

        /// <summary>
        /// CreateMFAEncryptedSecretKeysDatabase method implementation
        /// </summary>
        public string CreateMFAEncryptedSecretKeysDatabase(PSHost host, string _servername, string _databasename, string _username, string _password, string _keyname, string _thumbprint)
        {
            string _encrypted = GetSQLKeyEncryptedValue("LocalMachine/my/" + _thumbprint.ToUpper());
            string sqlscript = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\MFA\mfa-secretkey-db-encrypted.sql");
            sqlscript = sqlscript.Replace("%DATABASENAME%", _databasename);
            sqlscript = sqlscript.Replace("%SQLKEY%", _keyname);
            SqlConnection cnx = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=master;Data Source=" + _servername);
            cnx.Open();
            try
            {
                SqlCommand cmd = new SqlCommand(string.Format("CREATE DATABASE {0}", _databasename), cnx);
                cmd.ExecuteNonQuery();
                SqlCommand cmdl = null;
                if (!string.IsNullOrEmpty(_password))
                    cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] WITH PASSWORD = '{1}', DEFAULT_DATABASE=[master] END", _username, _password), cnx);
                else
                    cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[master] END", _username), cnx);
                cmdl.ExecuteNonQuery();
            }
            finally
            {
                cnx.Close();
            }
            SqlConnection cnx2 = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=" + _databasename + ";Data Source=" + _servername);
            cnx2.Open();
            try
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(string.Format("CREATE USER [{0}] FOR LOGIN [{0}]", _username), cnx2);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }
                try
                {
                    SqlCommand cmd1 = new SqlCommand(string.Format("ALTER ROLE [db_owner] ADD MEMBER [{0}]", _username), cnx2);
                    cmd1.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }
                try
                {
                    SqlCommand cmd2 = new SqlCommand(string.Format("ALTER ROLE [db_securityadmin] ADD MEMBER [{0}]", _username), cnx2);
                    cmd2.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }
                try
                {
                    SqlCommand cmd3 = new SqlCommand(string.Format("GRANT ALTER ANY COLUMN ENCRYPTION KEY TO [{0}]", _username), cnx2);
                    cmd3.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    // Nothing : the indicated user is definitely the interactive user so the dbo
                }
                SqlCommand cmd4 = new SqlCommand(string.Format("CREATE COLUMN MASTER KEY [{0}] WITH (KEY_STORE_PROVIDER_NAME = 'MSSQL_CERTIFICATE_STORE', KEY_PATH = 'LocalMachine/My/{1}')", _keyname, _thumbprint.ToUpper()), cnx2);
                cmd4.ExecuteNonQuery();
                SqlCommand cmd5 = new SqlCommand(string.Format("CREATE COLUMN ENCRYPTION KEY [{0}] WITH VALUES (COLUMN_MASTER_KEY = [{0}], ALGORITHM = 'RSA_OAEP', ENCRYPTED_VALUE = {1})", _keyname, _encrypted), cnx2);
                cmd5.ExecuteNonQuery();
            }
            finally
            {
                cnx2.Close();
            }

            SqlConnection cnx3 = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=" + _databasename + ";Data Source=" + _servername);
            cnx3.Open();
            try
            {
                SqlCommand cmdl = new SqlCommand(sqlscript, cnx3);
                cmdl.ExecuteNonQuery();
            }
            finally
            {
                cnx3.Close();
            }

            FlatExternalKeyManager cf = new FlatExternalKeyManager();
            cf.Load(host);
            if (!string.IsNullOrEmpty(_password))
                cf.ConnectionString = "Persist Security Info=True;User ID=" + _username + ";Password=" + _password + ";Initial Catalog=" + _databasename + ";Data Source=" + _servername + ";Column Encryption Setting=enabled";
            else
                cf.ConnectionString = "Persist Security Info=False;Integrated Security=SSPI;Initial Catalog=" + _databasename + ";Data Source=" + _servername + ";Column Encryption Setting=enabled";
            cf.IsAlwaysEncrypted = true;
            cf.ThumbPrint = _thumbprint;
            cf.Update(host);
            return cf.Parameters;
        }
        #endregion
    }
    #endregion
}