//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using Neos.IdentityServer.MultiFactor.Data;

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
                SvcController ADFSController = new SvcController("mfanotifhub");
                try
                {
                    if ((ADFSController.Status != ServiceControllerStatus.Running) && (ADFSController.Status != ServiceControllerStatus.StartPending))
                    {
                        ADFSController.Start();
                        ADFSController.GetStatus();
                        if (ADFSController.Status != ServiceControllerStatus.Running)
                            ADFSController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));
                    }
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
            if (Config == null)
            {
                EnsureLocalService();
                Config = ReadConfiguration(Host);
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
            if (Config == null)
                throw new Exception(SErrors.ErrorLoadingMFAConfiguration);
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
        /// IsRunning method iplementation
        /// </summary>
        public bool IsADFSServer(string servername = "local")
        {
            SvcController ADFSController = null;
            try
            {
                if (servername.ToLowerInvariant().Equals("local"))
                    ADFSController = new SvcController("adfssrv");
                else
                    ADFSController = new SvcController("adfssrv", servername);
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
            SvcController ADFSController = null;
            try
            {
                if (servername.ToLowerInvariant().Equals("local"))
                    ADFSController = new SvcController("adfssrv");
                else
                    ADFSController = new SvcController("adfssrv", servername);
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
                return InternalStartService(servername);
            else
                return InternalStartService();
        }

        /// <summary>
        /// StopService method implementation
        /// </summary>
        public bool StopService(string servername = null)
        {
            if (servername != null)
                return InternalStopService(servername);
            else
                return InternalStopService();
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
                StopService();
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA System : " + "Starting  ...");
                StartService();
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
        /// InternalStartService method implementation
        /// </summary>
        private bool InternalStartService(string servername = "local")
        {
            SvcController ADFSController = null;
            try
            {
                this.ServiceStatusChanged(this, ServicesStatus, servername);
                if (servername.ToLowerInvariant().Equals("local"))
                {
                    ADFSController = new SvcController("adfssrv");
                    servername = Environment.MachineName;
                }
                else
                    ADFSController = new SvcController("adfssrv", servername);
                this.ServiceStatusChanged(this, ServiceOperationStatus.OperationPending, servername);
                ManagementService.BroadcastNotification((byte)NotificationsKind.ServiceStatusPending, servername);
                ADFSController.GetStatus();
                if ((ADFSController.Status != ServiceControllerStatus.Running) && (ADFSController.Status != ServiceControllerStatus.StartPending))
                {
                    ADFSController.Start();
                    ADFSController.GetStatus();
                    if (ADFSController.Status != ServiceControllerStatus.Running)
                        ADFSController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));
                }
                this.ServiceStatusChanged(this, ServiceOperationStatus.OperationRunning, servername);
                ManagementService.BroadcastNotification((byte)NotificationsKind.ServiceStatusRunning, servername);
                return true;
            }
            catch (Exception)
            {
                this.ServiceStatusChanged(this, ServiceOperationStatus.OperationInError, servername);
                ManagementService.BroadcastNotification((byte)NotificationsKind.ServiceStatusInError, servername);
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
        private bool InternalStopService(string servername = "local")
        {
            SvcController ADFSController = null;
            try
            {
                this.ServiceStatusChanged(this, ServicesStatus, servername);
                if (servername.ToLowerInvariant().Equals("local"))
                {
                    ADFSController = new SvcController("adfssrv");
                    servername = Environment.MachineName;
                }
                else
                    ADFSController = new SvcController("adfssrv", servername);
                this.ServiceStatusChanged(this, ServiceOperationStatus.OperationPending, servername);
                ManagementService.BroadcastNotification((byte)NotificationsKind.ServiceStatusPending, servername);
                ADFSController.GetStatus();
                if ((ADFSController.Status != ServiceControllerStatus.Stopped) && (ADFSController.Status != ServiceControllerStatus.StopPending))
                {
                    ADFSController.Stop();
                    ADFSController.GetStatus();
                    if (ADFSController.Status != ServiceControllerStatus.Stopped)
                        ADFSController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 1, 0));
                }
                this.ServiceStatusChanged(this, ServiceOperationStatus.OperationStopped, servername);
                ManagementService.BroadcastNotification((byte)NotificationsKind.ServiceStatusRunning, servername);
                return true;
            }
            catch (Exception)
            {
                this.ServiceStatusChanged(this, ServiceOperationStatus.OperationInError, servername);
                ManagementService.BroadcastNotification((byte)NotificationsKind.ServiceStatusInError, servername);
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
                Config = CFGUtilities.ReadConfiguration(Host);
                Config.IsDirty = false;
#if hardcheck
                if (this.IsMFAProviderEnabled(Host))
#else
                if (Config != null)
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
                Runspace SPRunSpace = null;
                PowerShell SPPowerShell = null;
                try
                {
                    _isprimaryserverread = true;
                    SPRunSpace = RunspaceFactory.CreateRunspace();

                    SPPowerShell = PowerShell.Create();
                    SPPowerShell.Runspace = SPRunSpace;
                    SPRunSpace.Open();

                    Pipeline pipeline = SPRunSpace.CreatePipeline();
                    Command exportcmd = new Command("(Get-AdfsSyncProperties).Role", true);
                    pipeline.Commands.Add(exportcmd);
                    Collection<PSObject> PSOutput = pipeline.Invoke();
                    foreach (var result in PSOutput)
                    {
                        if (!result.BaseObject.ToString().ToLower().Equals("primarycomputer"))
                        {
                            _isprimaryserver = false;
                            return false;
                        }
                    }
                    _isprimaryserver = true;
                    return true;
                }
                finally
                {
                    if (SPRunSpace != null)
                        SPRunSpace.Close();
                    if (SPPowerShell != null)
                        SPPowerShell.Dispose();
                }
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
        /// RegisterNewRSACertificate method implmentation
        /// </summary>
        public string RegisterNewRSACertificate(PSHost Host = null, int years = 5, bool restart = true)
        {
            if (Config.KeysConfig.KeyFormat == SecretKeyFormat.RSA)
            {
                Config.KeysConfig.CertificateThumbprint = InternalRegisterNewRSACertificate(Host, years);
                Config.IsDirty = true;
                CFGUtilities.WriteConfiguration(Host, Config);
                if (restart)
                    RestartFarm(Host);
                return Config.KeysConfig.CertificateThumbprint;
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
            if (!InternalRegisterNewADFSCertificate(Host, subject, issigning, years))
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
            Config.Hosts.SQLServerHost.ThumbPrint = InternalRegisterNewSQLCertificate(Host, years, keyname);
            Config.IsDirty = true;
            CFGUtilities.WriteConfiguration(Host, Config);
            return Config.Hosts.SQLServerHost.ThumbPrint;
        }

        /// <summary>
        /// CheckCertificate metnod implementation
        /// </summary>
        internal bool CheckCertificate(string thumbprint, StoreLocation location = StoreLocation.LocalMachine)
        {
            X509Certificate2 cert = Certs.GetCertificate(thumbprint, location);
            if (cert != null)
                cert.Reset();
            return (cert != null);
        }

        /// <summary>
        /// internalRegisterNewRSACertificate method implementation
        /// </summary>
        private string InternalRegisterNewRSACertificate(PSHost Host, int years)
        {
            X509Certificate2 cert = Certs.CreateRSACertificate("MFA RSA Keys", years);
            if (cert != null)
            {
                string thumbprint = cert.Thumbprint;
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA Certificate \"" + thumbprint + "\" Created for using with RSA keys");
                cert.Reset();
                return thumbprint;
            }
            else
                return "";
        }

        /// <summary>
        /// InternalRegisterNewADFSCertificate method implementation
        /// </summary>
        private bool InternalRegisterNewADFSCertificate(PSHost Host, string subject, bool issigning, int years)
        {
            return Certs.CreateADFSCertificate(subject, issigning, years);
        }

        /// <summary>
        /// internalRegisterNewSQLCertificate method implementation
        /// </summary>
        private string InternalRegisterNewSQLCertificate(PSHost Host, int years, string keyname)
        {
            X509Certificate2 cert = Certs.CreateRSACertificateForSQLEncryption("MFA SQL Key : " + keyname, years);
            if (cert != null)
            {
                string thumbprint = cert.Thumbprint;
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA Certificate \"" + thumbprint + "\" Created for using with SQL keys");
                cert.Reset();
                return thumbprint;
            }
            else
                return "";
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
                InitFarmConfiguration(host);
                WriteConfiguration(host);
                return true;
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
            /* if (Config == null)
             {
                 EnsureLocalService();
                 Config = ReadConfiguration(Host);
             } */
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
            /* if (Config == null)
             {
                 EnsureConfiguration(Host);
                 Config = ReadConfiguration(Host);
             } */
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
                    result = InitServersConfiguration2019(Host);
                }
                else if (reg.IsWindows2016)
                {
                    result = InitServersConfiguration2016(Host);
                }
                else if (reg.IsWindows2012R2)
                {
                    result = InitServersConfiguration2012(Host);
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
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return;
        }
        #endregion

        #region Servers Configuration
        /// <summary>
        /// InitServersConfiguration2012 method implementation
        /// </summary>
        internal ADFSServerHost InitServersConfiguration2012(PSHost Host)
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
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            string fqdn = Dns.GetHostEntry("LocalHost").HostName.ToLower();
            RegistryVersion reg = GetComputerInformations(Host, fqdn, true);
            ADFSServerHost props = new ADFSServerHost
            {
                FQDN = fqdn,
                BehaviorLevel = 1,
                HeartbeatTmeStamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0, DateTimeKind.Local),
                NodeType = nodetype,
                CurrentVersion = reg.CurrentVersion,
                CurrentBuild = reg.CurrentBuild,
                InstallationType = reg.InstallationType,
                ProductName = reg.ProductName,
                CurrentMajorVersionNumber = reg.CurrentMajorVersionNumber,
                CurrentMinorVersionNumber = reg.CurrentMinorVersionNumber
            };
            int i = ADFSFarm.Servers.FindIndex(c => c.FQDN.ToLower() == props.FQDN.ToLower());
            if (i < 0)
                ADFSFarm.Servers.Add(props);
            else
                ADFSFarm.Servers[i] = props;
            return props;
        }

        /// <summary>
        /// InitServersConfiguration2016 method implementation
        /// </summary>
        internal ADFSServerHost InitServersConfiguration2016(PSHost Host)
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
                    bool islocal = (Dns.GetHostEntry("LocalHost").HostName.ToLower().Equals(fqdn.ToLower()));
                    RegistryVersion reg = GetComputerInformations(Host, fqdn, islocal);
                    ADFSServerHost props = new ADFSServerHost
                    {
                        FQDN = fqdn,
                        BehaviorLevel = Convert.ToInt32(result.Members["BehaviorLevel"].Value),
                        HeartbeatTmeStamp = Convert.ToDateTime(result.Members["HeartbeatTimeStamp"].Value),
                        NodeType = result.Members["NodeType"].Value.ToString(),
                        CurrentVersion = reg.CurrentVersion,
                        CurrentBuild = reg.CurrentBuild,
                        InstallationType = reg.InstallationType,
                        ProductName = reg.ProductName,
                        CurrentMajorVersionNumber = reg.CurrentMajorVersionNumber,
                        CurrentMinorVersionNumber = reg.CurrentMinorVersionNumber
                    };
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
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return xprops;
        }

        /// <summary>
        /// InitServersConfiguration2019 method implementation
        /// </summary>
        internal ADFSServerHost InitServersConfiguration2019(PSHost Host)
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
                    bool islocal = (Dns.GetHostEntry("LocalHost").HostName.ToLower().Equals(fqdn.ToLower()));
                    RegistryVersion reg = GetComputerInformations(Host, fqdn, islocal);
                    ADFSServerHost props = new ADFSServerHost
                    {
                        FQDN = fqdn,
                        BehaviorLevel = Convert.ToInt32(result.Members["BehaviorLevel"].Value),
                        HeartbeatTmeStamp = Convert.ToDateTime(result.Members["HeartbeatTimeStamp"].Value),
                        NodeType = result.Members["NodeType"].Value.ToString(),
                        CurrentVersion = reg.CurrentVersion,
                        CurrentBuild = reg.CurrentBuild,
                        InstallationType = reg.InstallationType,
                        ProductName = reg.ProductName,
                        CurrentMajorVersionNumber = reg.CurrentMajorVersionNumber,
                        CurrentMinorVersionNumber = reg.CurrentMinorVersionNumber
                    };
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
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return xprops;
        }

        /// <summary>
        /// GetComputerInformations
        /// </summary>
        private static RegistryVersion GetComputerInformations(PSHost Host, string servername, bool islocal)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            RegistryVersion reg = null;
            try
            {
                string regpath = @"HKLM:\Software\Microsoft\Windows NT\CurrentVersion";

                if (!islocal)
                {
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, servername, 5985, "/wsman", "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", null, 3000);
                    SPRunSpace = RunspaceFactory.CreateRunspace(connectionInfo);
                    connectionInfo.AuthenticationMechanism = AuthenticationMechanism.NegotiateWithImplicitCredential;
                }
                else
                {
                    SPRunSpace = RunspaceFactory.CreateRunspace();
                }

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("Get-ItemProperty \"" + regpath + "\"", true);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                reg = new RegistryVersion(false);

                foreach (var result in PSOutput)
                {
                    reg.CurrentVersion = Convert.ToString(result.Properties["CurrentVersion"].Value);
                    reg.ProductName = Convert.ToString(result.Properties["ProductName"].Value);
                    reg.InstallationType = Convert.ToString(result.Properties["InstallationType"].Value);
                    reg.CurrentBuild = Convert.ToInt32(result.Properties["CurrentBuild"].Value);
                    if (result.Properties["CurrentMajorVersionNumber"] != null)
                        reg.CurrentMajorVersionNumber = Convert.ToInt32(result.Properties["CurrentMajorVersionNumber"].Value);
                    else
                        reg.CurrentMajorVersionNumber = 0;
                    if (result.Properties["CurrentMinorVersionNumber"] != null)
                        reg.CurrentMinorVersionNumber = Convert.ToInt32(result.Properties["CurrentMinorVersionNumber"].Value);
                    else
                        reg.CurrentMinorVersionNumber = 0;
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
            return reg;
        }

        #endregion

        #region MFA Database
        /// <summary>
        /// CreateMFADatabase method implementation
        /// </summary>
        public string CreateMFADatabase(PSHost host, string _servername, string _databasename, string _username, string _password)
        {
            char sep = Path.DirectorySeparatorChar;
            string sqlscript = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "SQLTools" + sep + "mfa-db.sql");
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
            FlatSQLStore cf = new FlatSQLStore();
            cf.Load(host);
            if (!string.IsNullOrEmpty(_password))
                cf.ConnectionString = "Persist Security Info=True;User ID=" + _username + ";Password=" + _password + ";Initial Catalog=" + _databasename + ";Data Source=" + _servername;
            else
                cf.ConnectionString = "Persist Security Info=False;Integrated Security=SSPI;Initial Catalog=" + _databasename + ";Data Source=" + _servername;
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

            SqlConnection cnx2 = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=" + databasename + ";Data Source=" + servername);
            cnx2.Open();
            try
            {
                SqlCommand cmdl = new SqlCommand(sqlscript, cnx2); // Create Tables and more
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
        public string CreateMFAEncryptedDatabase(PSHost host, string _servername, string _databasename, string _username, string _password, string _keyname, string _thumbprint)
        {
            char sep = Path.DirectorySeparatorChar;
            string _encrypted = GetSQLKeyEncryptedValue("LocalMachine/my/" + _thumbprint.ToUpper());
            string sqlscript = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) +sep + "MFA" + sep + "SQLTools" + sep + "mfa-db-encrypted.sql");
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

            FlatSQLStore cf = new FlatSQLStore();
            cf.Load(host);
            if (!string.IsNullOrEmpty(_password))
                cf.ConnectionString = "Persist Security Info=True;User ID=" + _username + ";Password=" + _password + ";Initial Catalog=" + _databasename + ";Data Source=" + _servername + ";Column Encryption Setting=enabled";
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
    }
    #endregion
}