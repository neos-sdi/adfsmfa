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
using System;
using System.Diagnostics;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Neos.IdentityServer.MultiFactor;
using Neos.IdentityServer.MultiFactor.Administration.Resources;
using System.Runtime.CompilerServices;
using Neos.IdentityServer.MultiFactor.Data;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net;
using static Neos.IdentityServer.MultiFactor.MailSlotServer;
using Neos.IdentityServer.MultiFactor.Common;
using System.IO;

namespace Neos.IdentityServer.MultiFactor.Administration
{
    #region ManagementService
    /// <summary>
    /// ManagementService class 
    /// </summary>
    internal static class ManagementService
    {
        private static string EventLogSource = "ADFS MFA Administration";
        private static string EventLogGroup = "Application";
        

        /// <summary>
        /// ManagementService static constructor
        /// </summary>
        static ManagementService()
        {
            if (!EventLog.SourceExists(EventLogSource))
                EventLog.CreateEventSource(ManagementService.EventLogSource, ManagementService.EventLogGroup);
        }

        #region Properties
        /// <summary>
        /// Filter Property
        /// </summary>
        internal static DataFilterObject Filter { get; set; } = new DataFilterObject();

        /// <summary>
        /// Paging Property
        /// </summary>
        internal static DataPagingObject Paging { get; } = new DataPagingObject();

        /// <summary>
        /// Order property
        /// </summary>
        internal static DataOrderObject Order { get; } = new DataOrderObject();

        /// <summary>
        /// ADFSManager property
        /// </summary>
        internal static ADFSServiceManager ADFSManager { get; private set; } = null;

        /// <summary>
        /// Config property
        /// </summary>
        internal static MFAConfig Config
        {
            get { return ADFSManager.Config; }
        }

        /// <summary>
        /// MailslotServer property implementation
        /// </summary>
        internal static MailSlotServer MailslotServer { get; private set; } = null;
        #endregion

        #region Initialization methods
        /// <summary>
        /// Initialize method 
        /// </summary>
        internal static void Initialize(bool loadconfig = false)
        {
            Initialize(null, loadconfig);
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        internal static void Initialize(PSHost host = null, bool loadconfig = false)
        {
            if (ADFSManager == null)
            {
                ADFSManager = new ADFSServiceManager();
                ADFSManager.Initialize();
            }
            if (MailslotServer == null)
            {
                MailslotServer = new MailSlotServer("MGT");
                MailslotServer.MailSlotMessageArrived += MailSlotMessageArrived;
                MailslotServer.AllowToSelf = true;
                MailslotServer.Start();
            }

            if (loadconfig)
            {
                try
                {
                    ADFSManager.EnsureLocalConfiguration(host);
                }
                catch (CmdletInvocationException cm)
                {
                    EventLog.WriteEntry(EventLogSource, SErrors.ErrorMFAUnAuthorized + "\r\r" + cm.Message, EventLogEntryType.Error, 30901);
                    throw cm;
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry(EventLogSource, string.Format(SErrors.ErrorLoadingMFAConfiguration, ex.Message), EventLogEntryType.Error, 30900);
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Reset method implementation
        /// </summary>
        /// <param name="host"></param>
        internal static void Reset(PSHost host = null)
        {
            MailslotServer.Stop();
            ADFSManager = null;
            MailslotServer.Start();
            Initialize(host, true);
        }

        /// <summary>
        /// EnsureService() method implmentation
        /// </summary>
        internal static void EnsureService()
        {
            Initialize(null, true);
        }

        #endregion

        #region Notification Events
        /// <summary>
        /// MailSlotMessageArrived method implmentation
        /// </summary>
        private static void MailSlotMessageArrived(MailSlotServer maislotserver, MailSlotMessage message)
        {
            if (message.Operation == (byte)NotificationsKind.ConfigurationReload)
            {
                ADFSManager.Config = null;
                ADFSManager.EnsureLocalConfiguration(null); // Force Reload Configuration
            }
            else if (message.Operation == (byte)NotificationsKind.ServiceStatusRunning)
            {
                ADFSManager.ServicesStatus = ServiceOperationStatus.OperationRunning;
                ADFSManager.OnServiceStatusChanged(ADFSManager.ServicesStatus, message.Text);
            }
            else if (message.Operation == (byte)NotificationsKind.ServiceStatusStopped)
            {
                ADFSManager.ServicesStatus = ServiceOperationStatus.OperationStopped;
                ADFSManager.OnServiceStatusChanged(ADFSManager.ServicesStatus, message.Text);
            }
            else if (message.Operation == (byte)NotificationsKind.ServiceStatusPending)
            {
                ADFSManager.ServicesStatus = ServiceOperationStatus.OperationPending;
                ADFSManager.OnServiceStatusChanged(ADFSManager.ServicesStatus, message.Text);
            }
            else if (message.Operation == (byte)NotificationsKind.ServiceStatusInError)
            {
                ADFSManager.ServicesStatus = ServiceOperationStatus.OperationInError;
                ADFSManager.OnServiceStatusChanged(ADFSManager.ServicesStatus, message.Text);
            }
        }
        #endregion

        #region Runtime Methods
        /// <summary>
        /// GetUserRegistration method implementation
        /// </summary>
        internal static MFAUser GetUserRegistration(string upn)
        {
            EnsureService();
            return RuntimeRepository.GetMFAUser(Config, upn);
        }

        /// <summary>
        /// SetUserRegistration method implementation
        /// </summary>
        internal static MFAUser SetUserRegistration(MFAUser reg, bool resetkey = false, bool caninsert = true, bool email = false)
        {
            EnsureService();
            return RuntimeRepository.SetMFAUser(Config, reg, resetkey, caninsert, email);
        }

        /// <summary>
        /// AddUserRegistration method implementation
        /// </summary>
        internal static MFAUser AddUserRegistration(MFAUser reg, bool resetkey = true, bool canupdate = true, bool email = false)
        {
            EnsureService();
            return RuntimeRepository.AddMFAUser(Config, reg, resetkey, canupdate, email);
        }

        /// <summary>
        /// DeleteUserRegistration method implementation
        /// </summary>
        internal static bool DeleteUserRegistration(MFAUser reg, bool dropkey = true)
        {
            EnsureService();
            return RuntimeRepository.DeleteMFAUser(Config, reg, dropkey);
        }

        /// <summary>
        /// EnableUserRegistration method implementation
        /// </summary>
        internal static MFAUser EnableUserRegistration(MFAUser reg)
        {
            EnsureService();
            return RuntimeRepository.EnableMFAUser(Config, reg);
        }

        /// <summary>
        /// DisableUserRegistration method implementation
        /// </summary>
        internal static MFAUser DisableUserRegistration(MFAUser reg)
        {
            EnsureService();
            return RuntimeRepository.DisableMFAUser(Config, reg);
        }

        /// <summary>
        /// GetUserRegistrations method implementation
        /// </summary>
        internal static MFAUserList GetUserRegistrations(DataFilterObject filter, DataOrderObject order, DataPagingObject paging)
        {
            EnsureService();
            return RuntimeRepository.GetMFAUsers(Config, filter, order, paging);
        }

        /// <summary>
        /// GetAllUserRegistrations method implementation
        /// </summary>
        internal static MFAUserList GetAllUserRegistrations(DataOrderObject order, bool enabledonly = false)
        {
            EnsureService();
            return RuntimeRepository.GetAllMFAUsers(Config, order, enabledonly);
        }

        /// <summary>
        /// GetUserRegistrationsCount method implementation
        /// </summary>
        internal static int GetUserRegistrationsCount(DataFilterObject filter)
        {
            EnsureService();
            return RuntimeRepository.GetMFAUsersCount(Config, filter);
        }

        /// <summary>
        /// GetEncodedUserKey method implmentation
        /// </summary>
        internal static string GetEncodedUserKey(string upn)
        {
            EnsureService();
            return RuntimeRepository.GetEncodedUserKey(Config, upn);
        }

        /// <summary>
        /// GetEncodedUserKey method implmentation
        /// </summary>
        internal static string NewUserKey(string upn)
        {
            EnsureService();
            return RuntimeRepository.NewUserKey(Config, upn);
        }

        /// <summary>
        /// SetADDSAttributesTemplate method implementation
        /// </summary>
        internal static bool SetADDSAttributesTemplate(ADDSTemplateKind kind, bool updatecfg = false)
        {
            try
            {
                Initialize(null, true);
                ADFSManager.EnsureLocalService();
                ADFSManager.Config.Hosts.ActiveDirectoryHost.ApplyAttributesTemplate(kind);
                if (updatecfg)
                {
                    ADFSManager.WriteConfiguration(null);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Verification methods
        /// <summary>
        /// CheckADDSConnection method implmentation
        /// </summary>
        internal static bool CheckADDSConnection(string domainname, string username, string password)
        {
            EnsureService();
            return RuntimeRepository.CheckADDSConnection(Config, domainname, username, password);
        }

        /// <summary>
        /// CheckADDSAttribute method implmentation
        /// </summary>
        internal static bool CheckADDSAttribute(string domainname, string username, string password, string attributename, int multivalued)
        {
            EnsureService();
            return RuntimeRepository.CheckADDSAttribute(Config, domainname, username, password, attributename, multivalued);
        }

        /// <summary>
        /// CheckADDSAttribute method implmentation
        /// </summary>
        internal static bool CheckSQLConnection(string connectionstring)
        {
            EnsureService();
            return RuntimeRepository.CheckSQLConnection(Config, connectionstring);
        }

        /// <summary>
        /// CheckADDSAttribute method implmentation
        /// </summary>
        internal static bool CheckKeysConnection(string connectionstring)
        {
            EnsureService();
            return RuntimeRepository.CheckKeysConnection(Config, connectionstring);
        }

        /// <summary>
        /// VerifyPrimaryServer method implementation
        /// </summary>
        internal static void VerifyPrimaryServer()
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
                Command exportcmd = new Command("(Get-AdfsSyncProperties).Role", true);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var result in PSOutput)
                {
                    if (!result.BaseObject.ToString().ToLower().Equals("primarycomputer"))
                        throw new InvalidOperationException("PS0033: This Cmdlet cannot be executed from a secondary server !");
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
        /// VerifyADFSServer2019 method implementation
        /// </summary>
        internal static void VerifyADFSServer2019()
        {
            RegistryVersion reg = new RegistryVersion();
            if (!reg.IsWindows2019)
               throw new InvalidOperationException("PS0033: This Cmdlet must be executed on Windows 2019 server and up only !");
        }

        /// <summary>
        /// VerifyADFSAdministrationRights method implementation
        /// </summary>
        internal static void VerifyADFSAdministrationRights()
        {
            if (!(ADFSManagementRights.IsAdministrator() || ADFSManagementRights.IsSystem() || ADFSManagementRights.AllowedGroup(Certs.ADFSAdminGroupSID)))
                throw new InvalidOperationException("PS0033: This Cmdlet must be executed with ADFS Administration rights granted for the current user !");
        }
        #endregion

        #region Notifications
        /// <summary>
        /// BroadcastNotification method implmentation
        /// </summary>
        internal static void BroadcastNotification(byte kind, string message, bool all = true)
        {
            BroadcastNotification(ADFSManager.Config, (NotificationsKind)kind, message, all);
        }

        /// <summary>
        /// BroadcastNotification method implmentation
        /// </summary>
        internal static void BroadcastNotification(MFAConfig config, byte kind, string message, bool all = true)
        {
            BroadcastNotification(config, (NotificationsKind)kind, message, all);
        }

        /// <summary>
        /// BroadcastNotification method implmentation
        /// </summary>
        internal static void BroadcastNotification(MFAConfig config, NotificationsKind kind, string message, bool all = true)
        {
            CFGUtilities.BroadcastNotification(config, kind, message, all);
        }

        /// <summary>
        /// PopNotification method implmentation
        /// </summary>
        internal static void PopNotification(byte kind, string message)
        {
            CFGUtilities.PopNotification((NotificationsKind)kind, message);
        }
        #endregion

        #region Computers Registration
        /// <summary>
        /// RegisterADFSComputer method implementation
        /// </summary>        
        public static bool RegisterADFSComputer(PSHost host, string servername)
        {
            try
            {
                string fqdn = Dns.GetHostEntry(servername).HostName.ToLower();
                bool islocal = (Dns.GetHostEntry("LocalHost").HostName.ToLower().Equals(fqdn));
                RegistryVersion reg = GetComputerInformations(host, fqdn, islocal);
                if (reg == null)
                    return false;
                ADFSServerHost srvhost = InitServerNodeConfiguration(host, reg, fqdn, islocal);
                if (srvhost != null)
                {
                    ADFSManager.SetDirty(true);
                    ADFSManager.WriteConfiguration(host);
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

        /// <summary>
        /// InitServerNodeConfiguration method implementation
        /// </summary>        
        private static ADFSServerHost InitServerNodeConfiguration(PSHost Host, RegistryVersion reg, string servername, bool islocal)
        {
            ADFSServerHost result = null;
            try
            {
                if (reg.IsWindows2019)
                    result = InitServerNodeConfiguration2019(Host, reg, servername, islocal);
                else if (reg.IsWindows2016)
                    result = InitServerNodeConfiguration2016(Host, reg, servername, islocal);
                else if (reg.IsWindows2012R2)
                    result = InitServerNodeConfiguration2012(Host, reg, servername, islocal);
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
        private static ADFSServerHost InitServerNodeConfiguration2012(PSHost Host, RegistryVersion reg, string servername, bool islocal)
        {
            string nodetype = string.Empty;
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                if (!islocal)
                {
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, servername, Config.WsMan.Port, "/" + Config.WsMan.AppName, Config.WsMan.ShellUri, null, Config.WsMan.TimeOut);
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

            ADFSServerHost props = new ADFSServerHost
            {
                FQDN = servername,  // Dns.GetHostEntry("LocalHost").HostName,
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
            int i = ADFSManager.ADFSFarm.Servers.FindIndex(c => c.FQDN.ToLower() == props.FQDN.ToLower());
            if (i < 0)
                ADFSManager.ADFSFarm.Servers.Add(props);
            else
                ADFSManager.ADFSFarm.Servers[i] = props;
            return props;
        }

        /// <summary>
        /// InitServerNodeConfiguration2016 method implementation
        /// </summary>
        private static ADFSServerHost InitServerNodeConfiguration2016(PSHost Host, RegistryVersion reg, string servername, bool islocal)
        {
            ADFSServerHost xprops = null;
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                SPRunSpace = RunspaceFactory.CreateRunspace();
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
                    if (servername.ToLower().Equals(fqdn.ToLower()))
                    {
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
                        int i = ADFSManager.ADFSFarm.Servers.FindIndex(c => c.FQDN.ToLower() == props.FQDN.ToLower());
                        if (i < 0)
                            ADFSManager.ADFSFarm.Servers.Add(props);
                        else
                            ADFSManager.ADFSFarm.Servers[i] = props;
                        xprops = props;
                        break;
                    }
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
        /// InitServerNodeConfiguration2019 method implementation
        /// </summary>
        private static ADFSServerHost InitServerNodeConfiguration2019(PSHost Host, RegistryVersion reg, string servername, bool islocal)
        {
            ADFSServerHost xprops = null;
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                SPRunSpace = RunspaceFactory.CreateRunspace();
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
                    if (servername.ToLower().Equals(fqdn.ToLower()))
                    {
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
                        int i = ADFSManager.ADFSFarm.Servers.FindIndex(c => c.FQDN.ToLower() == props.FQDN.ToLower());
                        if (i < 0)
                            ADFSManager.ADFSFarm.Servers.Add(props);
                        else
                            ADFSManager.ADFSFarm.Servers[i] = props;
                        xprops = props;
                        break;
                    }
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
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, servername, Config.WsMan.Port, "/" + Config.WsMan.AppName, Config.WsMan.ShellUri, null, Config.WsMan.TimeOut);
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

        /// <summary>
        /// UnRegisterADFSComputer method implementation
        /// </summary>
        public static bool UnRegisterADFSComputer(PSHost Host, string servername)
        {
            try
            {
                string fqdn = Dns.GetHostEntry(servername).HostName;
                ADFSManager.ADFSFarm.Servers.RemoveAll(c => c.FQDN.ToLower().Equals(fqdn.ToLower()));
                ADFSManager.SetDirty(true);
                ADFSManager.WriteConfiguration(Host);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }
        #endregion

        #region Services management
        /// <summary>
        /// RestartADFSService method implementation
        /// </summary>
        internal static bool RestartADFSService(PSHost hh, string servername)
        {
            return ADFSManager.RestartServer(hh, servername);
        }
        #endregion

        #region Firewall
        /// <summary>
        /// AddFirewallRules method implmentation
        /// </summary>
        internal static void AddFirewallRules(string computers)
        {
            foreach (ADFSServerHost srv in ADFSManager.ADFSFarm.Servers)
            {
                InternalAddFirewallRules(Config, srv.FQDN, computers);
            }
        }

        /// <summary>
        /// RemoveFirewallRules method implementation
        /// </summary>
        internal static void RemoveFirewallRules()
        {
            foreach (ADFSServerHost srv in ADFSManager.ADFSFarm.Servers)
            {
                InternalRemoveFirewallRules(Config, srv.FQDN);
            }
        }

        /// <summary>
        /// RemoveFirewallRules method implementation
        /// </summary>
        private static void InternalRemoveFirewallRules(MFAConfig cfg, string computername)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                bool islocal = (Dns.GetHostEntry("LocalHost").HostName.ToLower().Equals(computername.ToLower()));
                if (!islocal)
                {
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, computername, cfg.WsMan.Port, "/" + cfg.WsMan.AppName, cfg.WsMan.ShellUri, null, cfg.WsMan.TimeOut);
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
                // Obsolete rules
                Command rulein1 = new Command("Remove-NetFirewallRule -Name 'MFAIN1' ", true);
                Command rulein2 = new Command("Remove-NetFirewallRule -Name 'MFAIN2' ", true);
                Command ruleout1 = new Command("Remove-NetFirewallRule -Name 'MFAOUT1' ", true);
                Command ruleout2 = new Command("Remove-NetFirewallRule -Name 'MFAOUT2' ", true);
                // MFA Rules
                Command rulein = new Command("Remove-NetFirewallRule -Name 'MFAIN' ", true);
                Command ruleout = new Command("Remove-NetFirewallRule -Name 'MFAOUT' ", true);

                // final rules (to be remove UDP and work only with WinRM)
                Command ruletcpin = new Command("Remove-NetFirewallRule -Name 'MFATCPIN' ", true);
                Command ruletcpout = new Command("Remove-NetFirewallRule -Name 'MFATCPOUT' ", true);
                Command ruleudpin = new Command("Remove-NetFirewallRule -Name 'MFAUDPIN' ", true);
                Command ruleudpout = new Command("Remove-NetFirewallRule -Name 'MFAUDPOUT' ", true);

                pipeline.Commands.Add(rulein);
                pipeline.Commands.Add(ruleout);

                pipeline.Commands.Add(rulein1);
                pipeline.Commands.Add(rulein2);
                pipeline.Commands.Add(ruleout1);
                pipeline.Commands.Add(ruleout2);

                pipeline.Commands.Add(ruletcpin);
                pipeline.Commands.Add(ruleudpin);
                pipeline.Commands.Add(ruletcpout);
                pipeline.Commands.Add(ruletcpout);

                pipeline.Invoke();
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
        /// InternalAddFirewallRules method implmentation
        /// </summary>
        private static void InternalAddFirewallRules(MFAConfig cfg, string computername, string computers)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            try
            {
                bool islocal = (Dns.GetHostEntry("LocalHost").HostName.ToLower().Equals(computername.ToLower()));
                if (!islocal)
                {
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, computername, cfg.WsMan.Port, "/" + cfg.WsMan.AppName, cfg.WsMan.ShellUri, null, cfg.WsMan.TimeOut);
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
               // Command ruletcpin = new Command("New-NetFirewallRule -Name 'MFATCPIN'  -DisplayName 'MFA Inbound TCP Rule' -Group 'MFA' -Profile @('Domain') -Direction Inbound  -Action Allow -Protocol TCP -LocalPort @('135', '139', '445', '593', '5985', '5986', '5987') -RemoteAddress " + computers, true);
                Command ruletcpin = new Command("New-NetFirewallRule -Name 'MFATCPIN'  -DisplayName 'MFA Inbound TCP Rule' -Group 'MFA' -Profile @('Domain') -Direction Inbound  -Action Allow -Protocol TCP -LocalPort @('5985', '5986', '5987') -RemoteAddress " + computers, true);
                Command ruletcpout = new Command("New-NetFirewallRule -Name 'MFATCPOUT' -DisplayName 'MFA Outbound TCP Rule' -Group 'MFA' -Profile @('Domain') -Direction Outbound -Action Allow -Protocol TCP -LocalPort @('5985', '5986', '5987') -RemoteAddress " + computers, true);
              //  Command ruleudpin = new Command("New-NetFirewallRule  -Name 'MFAUDPIN'  -DisplayName 'MFA Inbound UDP Rule' -Group 'MFA' -Profile @('Domain') -Direction Inbound  -Action Allow -Protocol UDP -LocalPort @('135', '137', '138', '445') -RemoteAddress " + computers, true);
              //  Command ruleudpout = new Command("New-NetFirewallRule -Name 'MFAUDPOUT' -DisplayName 'MFA Outbound UDP Rule' -Group 'MFA' -Profile @('Domain') -Direction Outbound -Action Allow -Protocol UDP -LocalPort @('135', '137', '138', '445') -RemoteAddress " + computers, true);

                pipeline.Commands.Add(ruletcpin);
                pipeline.Commands.Add(ruletcpout);
              // pipeline.Commands.Add(ruleudpin);
              // pipeline.Commands.Add(ruleudpout);

                pipeline.Invoke();
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
        }
        #endregion

        #region ACL & Orphaned Keys
        /// <summary>
        /// UpdateCertificatesACL method implementation
        /// </summary>
        internal static bool UpdateCertificatesACL(Certs.KeyMgtOptions options = Certs.KeyMgtOptions.AllCerts)
        {
            EnsureService();
            try
            {
                return Certs.UpdateCertificatesACL(options);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// CleanOrphanedPrivateKeys method implmentation
        /// </summary>
        internal static int CleanOrphanedPrivateKeys(byte option, int delay)
        {
            EnsureService();
            try
            {
                Certs.CleanOrphanedPrivateKeysRegistry(option, delay);
                return Certs.CleanOrphanedPrivateKeys();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region ThreatDetection (2019)
        /// <summary>
        /// RegisterMFAThreatDetectionSystem method implementation
        /// </summary>
        internal static bool RegisterMFAThreatDetectionSystem(PSHost Host)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            char sep = Path.DirectorySeparatorChar;
            string db = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "Config" + sep + "threatconfig.db";
            try
            {
                SPRunSpace = RunspaceFactory.CreateRunspace();

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("Register-AdfsThreatDetectionModule", false);
                CommandParameter NParam = new CommandParameter("Name", "MFABlockPlugin");
                exportcmd.Parameters.Add(NParam);
                CommandParameter TParam = new CommandParameter("TypeName", "Neos.IdentityServer.MultiFactor.ThreatAnalyzer, Neos.IdentityServer.MultiFactor.ThreatDetection, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey());
                exportcmd.Parameters.Add(TParam);
                CommandParameter PParam = new CommandParameter("ConfigurationFilePath", db);
                exportcmd.Parameters.Add(PParam);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA Threat Detection : Registered");
                return true;
            }
            catch (Exception ex)
            {
                if (Host != null)
                    Host.UI.WriteErrorLine(DateTime.Now.ToLongTimeString() + " MFA Threat Detection Error : " +ex.Message);
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return false;
        }

        /// <summary>
        /// UnRegisterMFAThreatDetectionSystem method implementation
        /// </summary>
        internal static bool UnRegisterMFAThreatDetectionSystem(PSHost Host)
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
                Command exportcmd = new Command("UnRegister-AdfsThreatDetectionModule", false);
                CommandParameter NParam = new CommandParameter("Name", "MFABlockPlugin");
                exportcmd.Parameters.Add(NParam);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                if (Host != null)
                    Host.UI.WriteErrorLine(DateTime.Now.ToLongTimeString() + " MFA Threat Detection Error : " + ex.Message);
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return false;
        }

        /// <summary>
        /// UpdateMFAThreatDetectionData method implementation
        /// </summary>
        internal static bool UpdateMFAThreatDetectionData(PSHost Host)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            char sep = Path.DirectorySeparatorChar;
            string db = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "Config" + sep + "threatconfig.db";
            try
            {
                SPRunSpace = RunspaceFactory.CreateRunspace();

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("Import-AdfsThreatDetectionModuleConfiguration", false);
                CommandParameter NParam = new CommandParameter("Name", "MFABlockPlugin");
                exportcmd.Parameters.Add(NParam);
                CommandParameter PParam = new CommandParameter("ConfigurationFilePath", db);
                exportcmd.Parameters.Add(PParam);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                if (Host != null)
                    Host.UI.WriteVerboseLine(DateTime.Now.ToLongTimeString() + " MFA Threat Detection Data : Updated !");
                return true;
            }
            catch (Exception ex)
            {
                if (Host != null)
                    Host.UI.WriteErrorLine(DateTime.Now.ToLongTimeString() + " MFA Threat Detection Data Error : " + ex.Message);
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return false;
        }

        /// <summary>
        /// SetMFACredentials method implementation
        /// </summary>
        internal static void SetMFACredentials(PSHost host, byte kind, string value, bool clearvalue = false)
        {
            MFAConfig config = CFGUtilities.ReadConfigurationFromADFSStore(host);
            if (config == null)
                return;
            switch (kind)
            {
                case 0x00:
                    using (AESSystemEncryption MSIS = new AESSystemEncryption())
                    {
                        if (clearvalue)
                        {
                            config.Hosts.ActiveDirectoryHost.Password = string.Empty;
                            config.Hosts.ActiveDirectoryHost.Account = string.Empty;
                            config.Hosts.ActiveDirectoryHost.DomainAddress = string.Empty;
                            config.MailProvider.Password = string.Empty;
                            config.MailProvider.UserName = string.Empty;
                            config.MailProvider.Anonymous = true;
                            config.KeysConfig.XORSecret = XORUtilities.DefaultKey;
                        }
                        else
                        {
                            config.Hosts.ActiveDirectoryHost.Password = MSIS.Encrypt(config.Hosts.ActiveDirectoryHost.Password);
                            config.MailProvider.Password = MSIS.Encrypt(config.MailProvider.Password);
                            config.KeysConfig.XORSecret = MSIS.Encrypt(config.KeysConfig.XORSecret);
                            if (!string.IsNullOrEmpty(value))
                                host.UI.WriteWarningLine("Block Updates not allowed, values where only encrypted !");
                        }
                    }
                    break;
                case 0x01:
                    using (AESSystemEncryption MSIS = new AESSystemEncryption())
                    {
                        if (clearvalue)
                        {
                            config.Hosts.ActiveDirectoryHost.Password = string.Empty;
                            config.Hosts.ActiveDirectoryHost.Account = string.Empty;
                            config.Hosts.ActiveDirectoryHost.DomainAddress = string.Empty;
                        }
                        else
                        { 
                            if (string.IsNullOrEmpty(value))
                            {
                                config.Hosts.ActiveDirectoryHost.Password = MSIS.Encrypt(config.Hosts.ActiveDirectoryHost.Password);
                                host.UI.WriteWarningLine("Empty value not allowed, value was only encrypted !");
                            }
                            else
                                config.Hosts.ActiveDirectoryHost.Password = MSIS.Encrypt(value);
                        }
                    }
                    break;
                case 0x02:
                    using (AESSystemEncryption MSIS = new AESSystemEncryption())
                    {
                        if (clearvalue)
                        {
                            config.MailProvider.Password = string.Empty;
                            config.MailProvider.UserName = string.Empty;
                            config.MailProvider.Anonymous = true;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(value))
                            {
                                config.MailProvider.Password = MSIS.Encrypt(config.MailProvider.Password);
                                host.UI.WriteWarningLine("Empty value not allowed, value was only encrypted !");
                            }
                            else
                                config.MailProvider.Password = MSIS.Encrypt(value);
                        }
                    }
                    break;
                case 0x03:
                    using (AESSystemEncryption MSIS = new AESSystemEncryption())
                    {
                        if (clearvalue)
                        {
                            config.KeysConfig.XORSecret = XORUtilities.DefaultKey;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(value))
                            {
                                config.KeysConfig.XORSecret = MSIS.Encrypt(config.KeysConfig.XORSecret);
                                host.UI.WriteWarningLine("Empty value not allowed, value was only encrypted !");
                            }
                            else
                                config.KeysConfig.XORSecret = MSIS.Encrypt(value);
                        }
                    }
                    break;
            }
            CFGUtilities.WriteConfigurationToDatabase(host, config, false);
            CFGUtilities.BroadcastNotification(config, NotificationsKind.ConfigurationCreated, Environment.MachineName, true);
        }

        /// <summary>
        /// ExportMFAMailTemplates method implementation
        /// </summary>
        internal static void ExportMFAMailTemplates(PSHost host, int lcid)
        {
            MFAConfig config = CFGUtilities.ReadConfiguration(host);
            MailUtilities.ExportMailTemplates(config, lcid);
            CFGUtilities.WriteConfiguration(host, config);
            CFGUtilities.BroadcastNotification(config, NotificationsKind.ConfigurationReload, Environment.MachineName, true);
        }

        /// <summary>
        /// InstallMFASample method implementation
        /// </summary>
        internal static void InstallMFASample(PSHost host, FlatSampleKind kind, bool reset = false)
        {
            MFAConfig config = CFGUtilities.ReadConfiguration(host);
            switch (kind)
            {
                case FlatSampleKind.QuizProvider:
                    if (!reset)
                        config.ExternalProvider.FullQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.QuizProviderSample, Neos.IdentityServer.MultiFactor.Samples, Version = 3.0.0.0, Culture = neutral, " + Utilities.GetAssemblyPublicKey();
                    else
                        config.ExternalProvider.FullQualifiedImplementation = "";
                    break;
                case FlatSampleKind.CaesarEnryption:
                    if (!reset)
                    {
                        config.KeysConfig.CustomFullyQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.CaesarKeyManagerActivator, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey();
                        config.KeysConfig.KeyFormat = SecretKeyFormat.CUSTOM;
                    }
                    else
                    {
                        config.KeysConfig.CustomFullyQualifiedImplementation = "";
                        config.KeysConfig.KeyFormat = SecretKeyFormat.RNG;
                    }
                    break;
                case FlatSampleKind.InMemoryStorage:
                    if (!reset)
                    {
                        config.Hosts.CustomStoreHost.KeysRepositoryFullyQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.InMemoryKeys2RepositoryService, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey();
                        config.Hosts.CustomStoreHost.DataRepositoryFullyQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.InMemoryDataRepositoryService, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey();
                        config.StoreMode = DataRepositoryKind.Custom;
                    }
                    else
                    {
                        config.Hosts.CustomStoreHost.KeysRepositoryFullyQualifiedImplementation = "";
                        config.Hosts.CustomStoreHost.DataRepositoryFullyQualifiedImplementation = "";
                        config.StoreMode = DataRepositoryKind.ADDS;
                    }
                    break;
                case FlatSampleKind.SMSProvider:
                    if (!reset)
                        config.ExternalProvider.FullQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.NeosSMSProvider, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey();
                    else
                        config.ExternalProvider.FullQualifiedImplementation = "";
                    break;
                case FlatSampleKind.TOTPProvider:
                    if (!reset)
                        config.OTPProvider.FullQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.NeosOTPProvider430, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, " + Utilities.GetAssemblyPublicKey();
                    else
                        config.OTPProvider.FullQualifiedImplementation = "";
                    break;
            }
            CFGUtilities.WriteConfiguration(host, config);
            CFGUtilities.BroadcastNotification(config, NotificationsKind.ConfigurationReload, Environment.MachineName, true);
        }

        internal static void ResetWebThemesList(PSHost host)
        {
            MFAConfig config = CFGUtilities.ReadConfiguration(host);
            WebThemeManager.ResetThemesList(config);
        }
        #endregion
    }

    /// <summary>
    /// NotificationsEvents class
    /// </summary>
    public static class NotificationsEvents
    {
        /// <summary>
        /// PopNotification method implmentation
        /// </summary>
        public static void PopNotification(byte kind, string message)
        {
            switch (kind)
            {
                case (byte)NotificationsKind.ConfigurationReload:
                    PushNotification(NotificationsKind.ConfigurationReload, message);
                    break;
                case (byte)NotificationsKind.ConfigurationCreated:
                    PushNotification(NotificationsKind.ConfigurationCreated, message);
                    break;
                case (byte)NotificationsKind.ConfigurationDeleted:
                    PushNotification(NotificationsKind.ConfigurationDeleted, message);
                    break;
                case (byte)NotificationsKind.ServiceStatusInError:
                    PushNotification(NotificationsKind.ServiceStatusInError, message);
                    break;
                case (byte)NotificationsKind.ServiceStatusPending:
                    PushNotification(NotificationsKind.ServiceStatusPending, message);
                    break;
                case (byte)NotificationsKind.ServiceStatusRunning:
                    PushNotification(NotificationsKind.ServiceStatusRunning, message);
                    break;
                case (byte)NotificationsKind.ServiceStatusStopped:
                    PushNotification(NotificationsKind.ServiceStatusStopped, message);
                    break;
            }
        }

        /// <summary>
        /// PushNotification method implementation
        /// Push local notification
        /// </summary>
        private static void PushNotification(NotificationsKind kind, string message)
        {
            using (MailSlotClient mailslot = new MailSlotClient())
            {
                if (string.IsNullOrEmpty(message))
                    mailslot.Text = Environment.MachineName;
                else
                    mailslot.Text = message;
                mailslot.SendNotification(kind);
            }
        }
    }

    /// <summary>
    /// ADFSManagementRights class
    /// </summary>
    public static class ADFSManagementRights
    {
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool IsSystem()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            return identity.IsSystem;
        }

        public static bool AllowedGroup(string group)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(group);
        }
    }
    #endregion
}