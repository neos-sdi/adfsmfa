﻿//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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

        /// <summary>
        /// EnsureService() method implmentation
        /// </summary>
        internal static void EnsureService()
        {
          //  if (_manager == null)
                Initialize(null, true);
        }

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
                    if (!result.BaseObject.ToString().ToLower().Equals("primarycomputer"))
                        throw new InvalidOperationException("PS0033: This Cmdlet cannot be executed from a secondary server !");
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
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
        /// RemoveFirewallRules method implementation
        /// </summary>
        internal static void RemoveFirewallRules()
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
                Command rulein1 = new Command("Remove-NetFirewallRule -Name 'MFAIN1' ", true);
                Command rulein2 = new Command("Remove-NetFirewallRule -Name 'MFAIN2' ", true);
                Command ruleout1 = new Command("Remove-NetFirewallRule -Name 'MFAOUT1' ", true);
                Command ruleout2 = new Command("Remove-NetFirewallRule -Name 'MFAOUT2' ", true);
                pipeline.Commands.Add(rulein1);
                pipeline.Commands.Add(rulein2);
                pipeline.Commands.Add(ruleout1);
                pipeline.Commands.Add(ruleout2);
                pipeline.Invoke();
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
        }

        /// <summary>
        /// AddFirewallRules method implmentation
        /// </summary>
        /// <param name="lst"></param>
        internal static void AddFirewallRules(string computers)
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
                Command rulein1  = new Command("New-NetFirewallRule -Name 'MFAIN1' -DisplayName 'MFA IN Notification Service UDP' -Group 'MFA' -Profile @('Domain') -Direction Inbound -Action Allow -Protocol UDP -LocalPort @('137', '138') -RemoteAddress " + computers, true);
                Command rulein2 = new Command("New-NetFirewallRule -Name 'MFAIN2'  -DisplayName 'MFA IN Notification Service TCP' -Group 'MFA' -Profile @('Domain') -Direction Inbound -Action Allow -Protocol TCP -LocalPort @('139', '445') -RemoteAddress " + computers, true);
                Command ruleout1 = new Command("New-NetFirewallRule -Name 'MFAOUT1'  -DisplayName 'MFA OUT Notification Service UDP' -Group 'MFA' -Profile @('Domain') -Direction Outbound -Action Allow -Protocol UDP -LocalPort @('137', '138') -RemoteAddress " + computers, true);
                Command ruleout2 = new Command("New-NetFirewallRule -Name 'MFAOUT2'  -DisplayName 'MFA OUT Notification Service TCP' -Group 'MFA' -Profile @('Domain') -Direction Outbound -Action Allow -Protocol TCP -LocalPort @('139', '445') -RemoteAddress " + computers, true);
                pipeline.Commands.Add(rulein1);
                pipeline.Commands.Add(rulein2);
                pipeline.Commands.Add(ruleout1);
                pipeline.Commands.Add(ruleout2);
                pipeline.Invoke();
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
        }

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
        internal static int CleanOrphanedPrivateKeys()
        {
            EnsureService();
            try
            {
                return Certs.CleanOrphanedPrivateKeys();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /*
        Register-AdfsThreatDetectionModule -Name "MFABlockPlugin" -TypeName "Neos.IdentityServer.Multifactor.ThreatAnalyzer, Neos.IdentityServer.Multifactor.ThreatDetection, Version=2.5.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2" -ConfigurationFilePath "C:\temp\mfa\authconfigdb.csv”
        UnRegister-AdfsThreatDetectionModule -Name "MFABlockPlugin"

        Import-AdfsThreatDetectionModuleConfiguration
        */

        /// <summary>
        /// RegisterMFAThreatDetectionSystem method implementation
        /// </summary>
        internal static bool RegisterMFAThreatDetectionSystem(PSHost Host)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string db = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\MFA\Config\threatconfig.db";
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("Register-AdfsThreatDetectionModule", false);
                CommandParameter NParam = new CommandParameter("Name", "MFABlockPlugin");
                exportcmd.Parameters.Add(NParam);
                CommandParameter TParam = new CommandParameter("TypeName", "Neos.IdentityServer.MultiFactor.ThreatAnalyzer, Neos.IdentityServer.MultiFactor.ThreatDetection, Version=2.5.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2");
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
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

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
            string db = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\MFA\Config\threatconfig.db";
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

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
            }
            return false;
        }
    }

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