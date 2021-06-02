//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using Neos.IdentityServer.MultiFactor.Data;
using Neos.IdentityServer.MultiFactor.Common;

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
        ConfigInError,
        UISync
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
        private bool _isprimaryserver = false;
        private bool _isprimaryserverread = false;

        #region Constructors
        /// <summary>
        /// ADFSServiceManager constructor
        /// </summary>
        public ADFSServiceManager()
        {
            ServiceStatusChanged += DefaultServiceStatusChanged;
            ConfigurationStatusChanged += DefaultConfigurationStatusChanged;
            if (ServiceIsRunning())
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
                try
                {
                    NTServiceManagerClient.IsRunning("mfanotifhub", "localhost");
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
            if (message.Operation == (byte)NotificationsKind.ServiceStatusRunning)
            {
                ServicesStatus = ServiceOperationStatus.OperationRunning;
                this.ServiceStatusChanged(this, ServicesStatus, message.Text);
            }
            else if (message.Operation == (byte)NotificationsKind.ServiceStatusStopped)
            {
                ServicesStatus = ServiceOperationStatus.OperationStopped;
                this.ServiceStatusChanged(this, ServicesStatus, message.Text);
            }
            else if (message.Operation == (byte)NotificationsKind.ServiceStatusPending)
            {
                ServicesStatus = ServiceOperationStatus.OperationPending;
                this.ServiceStatusChanged(this, ServicesStatus, message.Text);
            }
            else if (message.Operation == (byte)NotificationsKind.ServiceStatusInError)
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
            if (status != ConfigOperationStatus.UISync)
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
            if (!ServiceExists())
                throw new Exception(SErrors.ErrorADFSPlatformNotSupported);
            if (!ServiceIsRunning())
                StartService();
        }

        /// <summary>
        /// EnsureConfiguration method implementation
        /// </summary>
        private void EnsureConfiguration(PSHost Host)
        {
            if (Config == null)
            {
                EnsureLocalService();
                Config = ReadConfiguration(Host);
                if ((Config != null) && !IsFarmConfigured())
                    throw new Exception(SErrors.ErrorMFAFarmNotInitialized);
            }
        }

        /// <summary>
        /// EnsureLocalConfiguration method implementation
        /// </summary>
        public void EnsureLocalConfiguration(PSHost Host = null)
        {
            EnsureConfiguration(Host);
            if (Config == null)
                throw new Exception(string.Format(SErrors.ErrorLoadingMFAConfiguration, "Cannot read MFA configuration / acces denied"));
            return;
        }

        /// <summary>
        /// SetDirty method implmentayion
        /// </summary>
        public void SetDirty(bool value)
        {
            EnsureConfiguration(null);
            Config.IsDirty = value;
            if (value)
                this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigIsDirty);
        }

        /// <summary>
        /// ConsoleSync method implmentayion
        /// </summary>
        public void ConsoleSync()
        {
            this.ConfigurationStatusChanged(this, ConfigOperationStatus.UISync);
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
                if (Config != null)
                    return Config.Hosts.ADFSFarm;
                else
                    return null;
            }
        }

        /// <summary>
        /// Config property implementation
        /// </summary>
        public MFAConfig Config { get; internal set; } = null;

        /// <summary>
        /// ServicesStatus property
        /// </summary>
        public ServiceOperationStatus ServicesStatus { get; set; }

        /// <summary>
        /// ConfigurationStatus property
        /// </summary>
        public ConfigOperationStatus ConfigurationStatus { get; set; }
        #endregion

        #region ADFS Services
        /// <summary>
        /// IsFarmConfigured method implementation
        /// </summary>
        public bool IsFarmConfigured()
        {
            if (ADFSFarm == null)
                return false;
            else
                return ADFSFarm.IsInitialized;
        }

        /// <summary>
        /// ServiceExists method iplementation
        /// </summary>
        public bool ServiceExists(string servername = "localhost")
        {
            try
            {
                return NTServiceManagerClient.Exists("adfssrv", servername);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// ServiceIsRunning method iplementation
        /// </summary>
        public bool ServiceIsRunning(string servername = "localhost")
        {
            try
            {
                return NTServiceManagerClient.IsRunning("adfssrv", servername);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// StartService method implementation
        /// </summary>
        public bool StartService(string servername = "localhost")
        {
            try
            {
                ServiceStatusChanged(this, ServiceOperationStatus.OperationPending, servername);
                ManagementService.BroadcastNotification((byte)NotificationsKind.ServiceStatusPending, servername);
                NTServiceManagerClient.Start("adfssrv", servername);
                ServiceStatusChanged(this, ServiceOperationStatus.OperationRunning, servername);
                ManagementService.BroadcastNotification((byte)NotificationsKind.ServiceStatusRunning, servername);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// StopService method implementation
        /// </summary>
        public bool StopService(string servername = "localhost")
        {
            try
            {
                ServiceStatusChanged(this, ServiceOperationStatus.OperationPending, servername);
                ManagementService.BroadcastNotification((byte)NotificationsKind.ServiceStatusPending, servername);
                bool res = NTServiceManagerClient.Stop("adfssrv", servername);
                ServiceStatusChanged(this, ServiceOperationStatus.OperationStopped, servername);
                ManagementService.BroadcastNotification((byte)NotificationsKind.ServiceStatusStopped, servername);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
        public bool RestartServer(PSHost Host = null, string servername = "localhost")
        {
            bool result = false;
            if (servername.ToLower().Equals("localhost"))
            {
                string fqdn = Dns.GetHostEntry("localhost").HostName.ToLower();
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : " + "Stopping  ...");
                StopService(fqdn);
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : " + "Starting  ...");
                StartService(fqdn);
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
                Config = CFGUtilities.ReadConfiguration(Host);
                Config.IsDirty = false;
                if (Config != null)
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
            return Config;
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
                Config.IsDirty = false;
                CFGUtilities.WriteConfiguration(Host, Config);
                this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigSaved);
                ManagementService.BroadcastNotification(Config, NotificationsKind.ConfigurationReload, Environment.MachineName);
            }
            catch (Exception ex)
            {
                this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigInError, ex);
            }
        }

        /// <summary>
        /// internalRegisterConfiguration method implementation
        /// </summary>
        private void InternalRegisterConfiguration(PSHost Host)
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
                    SPRunSpace = RunspaceFactory.CreateRunspace();

                    SPPowerShell = PowerShell.Create();
                    SPPowerShell.Runspace = SPRunSpace;
                    SPRunSpace.Open();

                    Pipeline pipeline = SPRunSpace.CreatePipeline();
                    Command exportcmd = new Command("Register-AdfsAuthenticationProvider", false);
                    CommandParameter NParam = new CommandParameter("Name", "MultiFactorAuthenticationProvider");
                    exportcmd.Parameters.Add(NParam);
                    CommandParameter TParam = new CommandParameter("TypeName", "Neos.IdentityServer.MultiFactor.AuthenticationProvider, Neos.IdentityServer.MultiFactor, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey());
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
                    if (SPPowerShell != null)
                        SPPowerShell.Dispose();
                }
            }
            finally
            {
                File.Delete(pth);
            }
            return;
        }

        /// <summary>
        /// internalUnRegisterConfiguration method implementation
        /// </summary>
        private void InternalUnRegisterConfiguration(PSHost Host)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                SPRunSpace = RunspaceFactory.CreateRunspace();

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
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return;
        }

        /// <summary>
        /// internalExportConfiguration method implmentation
        /// </summary>
        private void InternalExportConfiguration(PSHost Host, string backupfile)
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
                SPRunSpace = RunspaceFactory.CreateRunspace();

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
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA Configuration saved to => " + pth);
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }

        /// <summary>
        /// internalImportConfiguration method implmentation
        /// </summary>
        private void InternalImportConfiguration(PSHost Host, string importfile)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                SPRunSpace = RunspaceFactory.CreateRunspace();

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
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }

        /// <summary>
        /// internalActivateConfiguration method implementation
        /// </summary>
        private void InternalActivateConfiguration(PSHost Host)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            bool found = false;
            try
            {
                SPRunSpace = RunspaceFactory.CreateRunspace();

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
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }

        /// <summary>
        /// internalDeActivateConfiguration method implementation
        /// </summary>
        private void InternalDeActivateConfiguration(PSHost Host)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            bool found = false;
            try
            {
                SPRunSpace = RunspaceFactory.CreateRunspace();

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
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }

        /// <summary>
        /// internalIsConfigurationActive method implementation
        /// </summary>
        private bool InternalIsConfigurationActive(PSHost Host)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            bool found = false;
            try
            {
                SPRunSpace = RunspaceFactory.CreateRunspace();

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
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return found;
        }

        /// <summary>
        /// IsPrimaryServer method implementation 
        /// </summary>
        internal bool IsPrimaryServer()
        {
            if (!_isprimaryserverread)
            {
                _isprimaryserverread = true;
                RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\MFA", false);
                _isprimaryserver = Convert.ToBoolean(rk.GetValue("IsPrimaryServer", 0, RegistryValueOptions.None));
                return _isprimaryserver;
            }
            else
                return _isprimaryserver;
        }

        /// <summary>
        /// SetADFSTheme method implementation
        /// </summary>
        internal void SetADFSTheme(PSHost pSHost, string themename, bool paginated, bool supports2019)
        {
            InternalSetADFSTheme(pSHost, themename, paginated, supports2019);
        }

        /// <summary>
        /// internalSetADFSTheme method implementation
        /// </summary>
        private void InternalSetADFSTheme(PSHost Host, string themename, bool paginated, bool supports2019)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                SPRunSpace = RunspaceFactory.CreateRunspace();

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                if (supports2019)
                {
                    Pipeline pipeline1 = SPRunSpace.CreatePipeline();

                    Command exportcmd = new Command("(Get-AdfsGlobalAuthenticationPolicy).AdditionalAuthenticationProvider", true);
                    pipeline1.Commands.Add(exportcmd);
                    Collection<PSObject> PSOutput1 = pipeline1.Invoke();
                    List<string> lst = new List<string>();
                    foreach (var result in PSOutput1)
                    {
                        lst.Add(result.BaseObject.ToString());
                    }

                    Pipeline pipeline2 = SPRunSpace.CreatePipeline();
                    Command policycmd = new Command("Set-AdfsGlobalAuthenticationPolicy", false);
                    CommandParameter PParam = new CommandParameter("EnablePaginatedAuthenticationPages", paginated);
                    policycmd.Parameters.Add(PParam);
                    CommandParameter CParam = new CommandParameter("Force", true);
                    policycmd.Parameters.Add(CParam);
                    CommandParameter AParam = new CommandParameter("AdditionalAuthenticationProvider", lst);
                    policycmd.Parameters.Add(AParam);
                    pipeline2.Commands.Add(policycmd);

                    Collection<PSObject> PSOutput2 = pipeline2.Invoke();
                    if (Host != null)
                        Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " ADFS MFA Pagination : Changed");
                }

                Pipeline pipeline3 = SPRunSpace.CreatePipeline();
                Command themecmd = new Command("Set-AdfsWebConfig", false);
                CommandParameter NParam = new CommandParameter("ActiveThemeName", themename);
                themecmd.Parameters.Add(NParam);
                pipeline3.Commands.Add(themecmd);

                Collection<PSObject> PSOutput3 = pipeline3.Invoke();
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " ADFS MFA Theme : Changed");
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return;
        }

        /// <summary>
        /// CheckCertificate metnod implementation
        /// </summary>
        internal bool CheckCertificate(string thumbprint, StoreLocation location = StoreLocation.LocalMachine)
        {
            return WebAdminManagerClient.CertificateExists(thumbprint, location);
        }

        /// <summary>
        /// RegisterMFASystemMasterKey method implmentation
        /// </summary>
        public bool RegisterMFASystemMasterKey(PSHost Host = null, bool deployonly = false, bool deleteonly = false)
        {
            using (SystemEncryption MSIS = new SystemEncryption())
            {
                Config.KeysConfig.XORSecret = MSIS.Decrypt(Config.KeysConfig.XORSecret, "Pass Phrase Encryption");
                Config.Hosts.ActiveDirectoryHost.Password = MSIS.Decrypt(Config.Hosts.ActiveDirectoryHost.Password, "ADDS Super Account Password");
                Config.Hosts.SQLServerHost.SQLPassword = MSIS.Decrypt(Config.Hosts.SQLServerHost.SQLPassword, "SQL Super Account Password");
                Config.MailProvider.Password = MSIS.Decrypt(Config.MailProvider.Password, "Mail Provider Account Password");
            };
            string xorsecret = Config.KeysConfig.XORSecret;
            string addspwd = Config.Hosts.ActiveDirectoryHost.Password;
            string sqlpwd = Config.Hosts.SQLServerHost.SQLPassword;
            string mailpwd = Config.MailProvider.Password;

            if (WebAdminManagerClient.NewMFASystemMasterKey(Config, deployonly, deleteonly))
            {
                Config = CFGUtilities.WriteConfiguration(Host, Config); // Save configuration with new key
                Config.KeysConfig.XORSecret = xorsecret;
                Config.Hosts.ActiveDirectoryHost.Password = addspwd;
                Config.Hosts.SQLServerHost.SQLPassword = sqlpwd;
                Config.MailProvider.Password = mailpwd;
                ManagementService.BroadcastNotification(Config, NotificationsKind.ConfigurationReload, Environment.MachineName);

                if (Host != null)
                {
                    if (deleteonly)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System Master Key  Deleted ! ");
                    else if (deployonly)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System Master Key  Deployed ! ");
                    else
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System Master Key  Generated ! ");
                }
                return true;
            }
            else
            {
                if (Host != null)
                {
                    if (deleteonly)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System Master Key  Key NOT Deleted ! ");
                    else if (deployonly)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System Master Key  NOT Deployed ! ");
                    else
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System Master Key  NOT Generated ! ");
                }
                return false;
            }
        }

        /// <summary>
        /// CheckMFASystemMasterKey
        /// </summary>
        public bool CheckMFASystemMasterKey()
        {
            return WebAdminManagerClient.ExistsMFASystemMasterKey();
        }

        /// <summary>
        /// RegisterMFASystemAESCngKey method implmentation
        /// </summary>
        public bool RegisterMFASystemAESCngKey(PSHost Host = null, bool deployonly = false, bool deleteonly = false)
        {
            if (WebAdminManagerClient.NewMFASystemAESCngKey(Config, deployonly, deleteonly))
            {
                if (Host != null)
                {
                    if (deleteonly)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System AESCng Key Deleted ! ");
                    else if (deployonly)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System AESCng Key Deployed ! ");
                    else
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System AESCng Key Generated ! ");
                }
                return true;
            }
            else
            {
                if (Host != null)
                {
                    if (deleteonly)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System AESCng Key NOT Deleted ! ");
                    else if (deployonly)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System AESCng Key NOT Deployed ! ");
                    else
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System AESCng Key NOT Generated ! ");
                }
                return false;
            }
        }

        /// <summary>
        /// CheckMFASystemAESCngKey
        /// </summary>
        public bool CheckMFASystemAESCngKey()
        {
            return WebAdminManagerClient.ExistsMFASystemAESCngKeys();
        }

        /// <summary>
        /// RegisterNewRSACertificate method implmentation
        /// </summary>
        public string RegisterNewRSACertificate(PSHost Host = null, int years = 5)
        {
            if (Config.KeysConfig.KeyFormat == SecretKeyFormat.RSA)
            {
                string thumbprint = WebAdminManagerClient.CreateRSACertificate(Config, "MFA RSA Keys", years);
                if (!string.IsNullOrEmpty(thumbprint))
                {
                    Config.KeysConfig.CertificateThumbprint = thumbprint;
                    Config.IsDirty = true;
                    CFGUtilities.WriteConfiguration(Host, Config);
                    ManagementService.BroadcastNotification(Config, NotificationsKind.ConfigurationReload, Environment.MachineName);
                    return Config.KeysConfig.CertificateThumbprint;
                }
                else
                {
                    if (Host != null)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA Certificate Not Generated ! ");
                    return Config.KeysConfig.CertificateThumbprint;
                }
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
        /// RegisterNewADFSCertificate method implmentation
        /// </summary>
        public bool RegisterNewADFSCertificate(PSHost Host, string subject, bool issigning, int years = 5)
        {
            if (!WebAdminManagerClient.CreateADFSCertificate(Config, subject, issigning, years))
            {
                if (Host != null)
                {
                    if (issigning)
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System : ADSF Signing certificate not created !");
                    else
                        Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA System : ADSF Decrypting certificate not created !");
                }
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// RegisterNewRSACertificate method implmentation
        /// </summary>
        public string RegisterNewSQLCertificate(PSHost Host = null, int years = 5, string keyname = "adfsmfa")
        {
            string thumbprint = WebAdminManagerClient.CreateRSACertificateForSQLEncryption(Config, "MFA SQL Key : " + keyname, years);
            if (!string.IsNullOrEmpty(thumbprint))
            {
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA Certificate \"" + thumbprint + "\" Created for using with SQL keys");
                Config.Hosts.SQLServerHost.ThumbPrint = thumbprint;
                Config.IsDirty = true;
                CFGUtilities.WriteConfiguration(Host, Config);
                ManagementService.BroadcastNotification(Config, NotificationsKind.ConfigurationReload, Environment.MachineName);
            }
            else
            {
                if (Host != null)
                    Host.UI.WriteWarningLine(DateTime.Now.ToLongTimeString() + " MFA Certificate Not Generated ! ");
            }
            return Config.Hosts.SQLServerHost.ThumbPrint;
        }

        /// <summary>
        /// RegisterMFASystem method implmentation
        /// </summary>
        public bool RegisterMFASystem(PSHost host)
        {
            if (Config != null)
                return false;

            EnsureLocalService();
            Config = new MFAConfig(true);
            if (Config != null)
            {
                InternalRegisterConfiguration(host);
                InternalActivateConfiguration(host);
                if (InitFarmConfiguration(host))
                {
                    WriteConfiguration(host);
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// UnRegisterMFASystem method implmentation
        /// </summary>
        public bool UnRegisterMFASystem(PSHost Host)
        {
            if (Config == null)
            {
                EnsureConfiguration(Host);
                Config = ReadConfiguration(Host);
            }
            if (Config == null)
                return false;
            ManagementService.BroadcastNotification(Config, NotificationsKind.ConfigurationDeleted, Environment.MachineName);
            InternalDeActivateConfiguration(Host);
            InternalUnRegisterConfiguration(Host);
            Config = null;
            return true;
        }

        /// <summary>
        /// ImportMFAProviderConfiguration method implementation
        /// </summary>
        public bool ImportMFAProviderConfiguration(PSHost Host, string importfile)
        {
            this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigIsDirty);
            try
            {
                InternalImportConfiguration(Host, importfile);
                Config = CFGUtilities.ReadConfigurationFromDatabase(Host);
                Config.IsDirty = false;
                CFGUtilities.WriteConfigurationToCache(Config);
                ManagementService.BroadcastNotification(Config, NotificationsKind.ConfigurationReload, Environment.MachineName);
                this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigSaved);
                if (!KeysManager.IsLoaded)
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
            InternalExportConfiguration(Host, exportfilepath);
        }

        /// <summary>
        /// EnableMFAProvider method implmentation
        /// </summary>
        public void EnableMFAProvider(PSHost Host)
        {
            if (Config == null)
            {
                EnsureLocalService();
                Config = ReadConfiguration(Host);
                if (Config == null)
                    Config = new MFAConfig(true);
            }
            if (!Config.Hosts.ADFSFarm.IsInitialized)
                throw new Exception(SErrors.ErrorMFAFarmNotInitialized);
            InternalActivateConfiguration(Host);
            ManagementService.BroadcastNotification(Config, NotificationsKind.ConfigurationReload, Environment.MachineName);
        }

        /// <summary>
        /// DisableMFAProvider method implmentation
        /// </summary>
        public void DisableMFAProvider(PSHost Host)
        {
            if (Config == null)
            {
                EnsureConfiguration(Host);
                Config = ReadConfiguration(Host);
            }
            InternalDeActivateConfiguration(Host);
            this.ConfigurationStatusChanged(this, ConfigOperationStatus.ConfigStopped);
            ManagementService.BroadcastNotification(Config, NotificationsKind.ConfigurationReload, Environment.MachineName);
        }

        /// <summary>
        /// IsMFAProviderEnabled method implmentation
        /// </summary>
        public bool IsMFAProviderEnabled(PSHost Host)
        {
            if (Config == null)
            {
                EnsureConfiguration(Host);
                Config = ReadConfiguration(Host);
            }
            return InternalIsConfigurationActive(Host);
        }
        #endregion

        #region Farm Configuration
        /// <summary>
        /// InitFarmConfiguration method implementation
        /// </summary>
        private bool InitFarmConfiguration(PSHost Host)
        {
            bool bRet = false;
            Dictionary<string, ADFSServerHost> result = null;
            try
            {
                RegistryVersion reg = new RegistryVersion();
                InitFarmProperties(Host);
                result = WebAdminManagerClient.GetAllComputerInformations();
                if (result != null)
                {
                    ADFSFarm.Servers.Clear();
                    foreach (KeyValuePair<string, ADFSServerHost> item in result)
                    {
                        ADFSFarm.Servers.Add(item.Value);
                    }
                    bRet = true;
                }
                else
                    bRet = false;
                ADFSFarm.IsInitialized = true;
                SetDirty(true);
            }
            catch (Exception ex)
            {
                ADFSFarm.IsInitialized = false;
                throw ex;
            }
            return bRet;
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
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return;
        }
        #endregion

        #region Servers Configuration
             /// <summary>
        /// GetFarmServers method implementation
        /// </summary>
        private static Dictionary<string, bool> GetFarmServers()
        {
            Dictionary<string, bool> svrlist = new Dictionary<string, bool>();
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
                    string type = result.Members["NodeType"].Value.ToString();
                    svrlist.Add(fqdn.ToLower(), type.ToLower().Equals("primarycomputer"));
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return svrlist;
        }
        #endregion

        #region MFA Database
        /// <summary>
        /// CreateMFADatabase method implementation
        /// </summary>
        public string CreateMFADatabase(PSHost host, string servername, string databasename, string username, string password)
        {
            char sep = Path.DirectorySeparatorChar;
            string sqlscript = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "SQLTools" + sep + "mfa-db.sql");
            sqlscript = sqlscript.Replace("%DATABASENAME%", databasename);
            SqlConnection cnx = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=master;Data Source=" + servername);
            cnx.Open();
            try
            {
                SqlCommand cmd = new SqlCommand(string.Format("CREATE DATABASE {0}", databasename), cnx);
                cmd.ExecuteNonQuery();

                try
                {
                    SqlCommand cmdl = null;
                    if (!string.IsNullOrEmpty(password))
                        cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] WITH PASSWORD = '{1}', DEFAULT_DATABASE=[master] END", username, password), cnx);
                    else
                        cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[master] END", username), cnx);
                    cmdl.ExecuteNonQuery();
                } catch (Exception) { }

                try
                {
                    if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAdminGroupName))
                    {
                        SqlCommand cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[master] END", ClientSIDsProxy.ADFSAdminGroupName), cnx);
                        cmdl.ExecuteNonQuery();
                    }
                } catch (Exception) { }

                try
                {
                    if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAccountName))
                    {
                        SqlCommand cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[master] END", ClientSIDsProxy.ADFSAccountName), cnx);
                        cmdl.ExecuteNonQuery();
                    }
                } catch (Exception) { }
            }
            finally
            {
                cnx.Close();
            }
            SqlConnection cnx2 = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=" + databasename + ";Data Source=" + servername);
            cnx2.Open();
            try
            {
                // USERNAME
                try { SqlCommand cmd = new SqlCommand(string.Format("CREATE USER [{0}] FOR LOGIN [{0}]", username), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_owner] ADD MEMBER [{0}]", username), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_securityadmin] ADD MEMBER [{0}]", username), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }

                // ADFS ADMINS
                try { SqlCommand cmd = new SqlCommand(string.Format("CREATE USER [{0}] FOR LOGIN [{0}]", ClientSIDsProxy.ADFSAdminGroupName), cnx2); cmd.ExecuteNonQuery();  } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_owner] ADD MEMBER [{0}]", ClientSIDsProxy.ADFSAdminGroupName), cnx2);  cmd.ExecuteNonQuery();  } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_securityadmin] ADD MEMBER [{0}]", ClientSIDsProxy.ADFSAdminGroupName), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }

                // ADFS ACCOUNT
                try { SqlCommand cmd = new SqlCommand(string.Format("CREATE USER [{0}] FOR LOGIN [{0}]", ClientSIDsProxy.ADFSAccountName), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_owner] ADD MEMBER [{0}]", ClientSIDsProxy.ADFSAccountName), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_securityadmin] ADD MEMBER [{0}]", ClientSIDsProxy.ADFSAccountName), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }

                SqlCommand cmdl = new SqlCommand(sqlscript, cnx2); // Create Tables and more
                cmdl.ExecuteNonQuery();
            }
            finally
            {
                cnx2.Close();
            }
            FlatSQLStore cf = new FlatSQLStore();
            cf.Load(host);
            if (!string.IsNullOrEmpty(password))
                cf.ConnectionString = "Persist Security Info=True;User ID=" + username + ";Password=" + password + ";Initial Catalog=" + databasename + ";Data Source=" + servername;
            else
                cf.ConnectionString = "Persist Security Info=False;Integrated Security=SSPI;Initial Catalog=" + databasename + ";Data Source=" + servername;
            cf.IsAlwaysEncrypted = false;
            cf.ThumbPrint = string.Empty;
            cf.Update(host);
            return cf.ConnectionString;
        }

        /// <summary>
        /// UpgradeMFADatabase method implementation
        /// </summary>
        public string UpgradeMFADatabase(PSHost host, string servername, string databasename)
        {
            char sep = Path.DirectorySeparatorChar;
            FlatSQLStore cf = new FlatSQLStore();
            cf.Load(host);
            bool encrypt = cf.IsAlwaysEncrypted;
            string sqlscript = string.Empty;
            if (encrypt)
            {
                string keyname = cf.KeyName;
                sqlscript = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "SQLTools" + sep + "mfa-db-Encrypted-upgrade.sql");
                sqlscript = sqlscript.Replace("%DATABASENAME%", databasename);
                sqlscript = sqlscript.Replace("%SQLKEY%", keyname);
            }
            else
            {
                sqlscript = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "SQLTools" + sep + "mfa-db-upgrade.sql");
                sqlscript = sqlscript.Replace("%DATABASENAME%", databasename);
            }

            SqlConnection cnx = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=master;Data Source=" + servername);
            cnx.Open();
            try
            {
                try
                {
                    if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAdminGroupName))
                    {
                        SqlCommand cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[master] END", ClientSIDsProxy.ADFSAdminGroupName), cnx);
                        cmdl.ExecuteNonQuery();
                    }
                }
                catch (Exception) { }

                try
                {
                    if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAccountName))
                    {
                        SqlCommand cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[master] END", ClientSIDsProxy.ADFSAccountName), cnx);
                        cmdl.ExecuteNonQuery();
                    }
                }
                catch (Exception) { }
            }
            finally
            {
                cnx.Close();
            }

            SqlConnection cnx2 = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=" + databasename + ";Data Source=" + servername);
            cnx2.Open();
            try
            {
                // ADFS ADMINS
                try { SqlCommand cmd = new SqlCommand(string.Format("CREATE USER [{0}] FOR LOGIN [{0}]", ClientSIDsProxy.ADFSAdminGroupName), cnx); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_owner] ADD MEMBER [{0}]", ClientSIDsProxy.ADFSAdminGroupName), cnx); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_securityadmin] ADD MEMBER [{0}]", ClientSIDsProxy.ADFSAdminGroupName), cnx); cmd.ExecuteNonQuery(); } catch (Exception) { }

                // ADFS ACCOUNT
                try { SqlCommand cmd = new SqlCommand(string.Format("CREATE USER [{0}] FOR LOGIN [{0}]", ClientSIDsProxy.ADFSAccountName), cnx); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_owner] ADD MEMBER [{0}]", ClientSIDsProxy.ADFSAccountName), cnx); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_securityadmin] ADD MEMBER [{0}]", ClientSIDsProxy.ADFSAccountName), cnx); cmd.ExecuteNonQuery(); } catch (Exception) { }

                SqlCommand cmdl = new SqlCommand(sqlscript, cnx); // Create Tables and more
                cmdl.ExecuteNonQuery();
            }
            finally
            {
                cnx2.Close();
            }
            return cf.ConnectionString;
        }

        /// <summary>
        /// CreateMFAEncryptedDatabase method implementation
        /// </summary>
        public string CreateMFAEncryptedDatabase(PSHost host, string servername, string databasename, string username, string password, string keyname, string thumbprint)
        {
            char sep = Path.DirectorySeparatorChar;
            string _encrypted = GetSQLKeyEncryptedValue("LocalMachine/my/" + thumbprint.ToUpper());
            string sqlscript = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) +sep + "MFA" + sep + "SQLTools" + sep + "mfa-db-encrypted.sql");
            sqlscript = sqlscript.Replace("%DATABASENAME%", databasename);
            sqlscript = sqlscript.Replace("%SQLKEY%", keyname);
            SqlConnection cnx = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=master;Data Source=" + servername);
            cnx.Open();
            try
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(string.Format("CREATE DATABASE {0}", databasename), cnx);
                    cmd.ExecuteNonQuery();
                    SqlCommand cmdl = null;
                    if (!string.IsNullOrEmpty(password))
                        cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] WITH PASSWORD = '{1}', DEFAULT_DATABASE=[master] END", username, password), cnx);
                    else
                        cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[master] END", username), cnx);
                    cmdl.ExecuteNonQuery();
                }
                catch (Exception) { }

                try
                {
                    if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAdminGroupName))
                    {
                        SqlCommand cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[master] END", ClientSIDsProxy.ADFSAdminGroupName), cnx);
                        cmdl.ExecuteNonQuery();
                    }
                }
                catch (Exception) { }

                try
                {
                    if (!string.IsNullOrEmpty(ClientSIDsProxy.ADFSAccountName))
                    {
                        SqlCommand cmdl = new SqlCommand(string.Format("IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name = '{0}') BEGIN CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[master] END", ClientSIDsProxy.ADFSAccountName), cnx);
                        cmdl.ExecuteNonQuery();
                    }
                }
                catch (Exception) { }
            }
            finally
            {
                cnx.Close();
            }
            SqlConnection cnx2 = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=" + databasename + ";Data Source=" + servername);
            cnx2.Open();
            try
            {
                // USERNAME
                try { SqlCommand cmd = new SqlCommand(string.Format("CREATE USER [{0}] FOR LOGIN [{0}]", username), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_owner] ADD MEMBER [{0}]", username), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_securityadmin] ADD MEMBER [{0}]", username), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }

                // ADFS ADMINS
                try { SqlCommand cmd = new SqlCommand(string.Format("CREATE USER [{0}] FOR LOGIN [{0}]", ClientSIDsProxy.ADFSAdminGroupName), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_owner] ADD MEMBER [{0}]", ClientSIDsProxy.ADFSAdminGroupName), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_securityadmin] ADD MEMBER [{0}]", ClientSIDsProxy.ADFSAdminGroupName), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }

                // ADFS ACCOUNT
                try { SqlCommand cmd = new SqlCommand(string.Format("CREATE USER [{0}] FOR LOGIN [{0}]", ClientSIDsProxy.ADFSAccountName), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_owner] ADD MEMBER [{0}]", ClientSIDsProxy.ADFSAccountName), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("ALTER ROLE [db_securityadmin] ADD MEMBER [{0}]", ClientSIDsProxy.ADFSAccountName), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }

                // ENCRYPTION KEY
                try { SqlCommand cmd = new SqlCommand(string.Format("GRANT ALTER ANY COLUMN ENCRYPTION KEY TO [{0}]", username), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }


                try { SqlCommand cmd = new SqlCommand(string.Format("CREATE COLUMN MASTER KEY [{0}] WITH (KEY_STORE_PROVIDER_NAME = 'MSSQL_CERTIFICATE_STORE', KEY_PATH = 'LocalMachine/My/{1}')", keyname, thumbprint.ToUpper()), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }
                try { SqlCommand cmd = new SqlCommand(string.Format("CREATE COLUMN ENCRYPTION KEY [{0}] WITH VALUES (COLUMN_MASTER_KEY = [{0}], ALGORITHM = 'RSA_OAEP', ENCRYPTED_VALUE = {1})", keyname, _encrypted), cnx2); cmd.ExecuteNonQuery(); } catch (Exception) { }
            }
            finally
            {
                cnx2.Close();
            }
            SqlConnection cnx3 = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=" + databasename + ";Data Source=" + servername);
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

            FlatSQLStore cf = new FlatSQLStore();
            cf.Load(host);
            if (!string.IsNullOrEmpty(password))
                cf.ConnectionString = "Persist Security Info=True;User ID=" + username + ";Password=" + password + ";Initial Catalog=" + databasename + ";Data Source=" + servername + ";Column Encryption Setting=enabled";
            else
                cf.ConnectionString = "Persist Security Info=False;Integrated Security=SSPI;Initial Catalog=" + databasename + ";Data Source=" + servername + ";Column Encryption Setting=enabled";
            cf.IsAlwaysEncrypted = true;
            cf.ThumbPrint = thumbprint;
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
    }
    #endregion
}