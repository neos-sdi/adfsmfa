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
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using Microsoft.Win32;
using Neos.IdentityServer.MultiFactor.Administration.Resources;
using Neos.IdentityServer.MultiFactor.Common;
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Security.Principal;

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
            try
            {
                EnsureService();
                return RuntimeRepository.CheckADDSConnection(Config, domainname, username, password);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// CheckADDSAttribute method implmentation
        /// </summary>
        internal static bool CheckADDSAttribute(string domainname, string username, string password, string attributename, int multivalued)
        {
            try
            {
                EnsureService();
                return RuntimeRepository.CheckADDSAttribute(Config, domainname, username, password, attributename, multivalued);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// CheckSQLConnection method implmentation
        /// </summary>
        internal static bool CheckSQLConnection(string connectionstring, string username, string password )
        {
            try
            { 
                EnsureService();
                return RuntimeRepository.CheckSQLConnection(Config, connectionstring, username, password);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// CheckKeysConnection method implmentation
        /// </summary>
        internal static bool CheckKeysConnection(string connectionstring, string username, string password)
        {
            try
            { 
                EnsureService();
                return RuntimeRepository.CheckKeysConnection(Config, connectionstring, username, password);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// VerifyPrimaryServer method implementation
        /// </summary>
        internal static void VerifyPrimaryServer(PSHost host = null)
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\MFA", false);
            bool isprimary = Convert.ToBoolean(rk.GetValue("IsPrimaryServer", 0, RegistryValueOptions.None));
            if (!isprimary)
            {
                if (host==null)
                    throw new InvalidOperationException("Must be executed from a primary server !");
                else
                    throw new InvalidOperationException("PS0033: This Cmdlet cannot be executed from a secondary server !");
            }
        }

        /// <summary>
        /// VerifyADFSServer2019 method implementation
        /// </summary>
        internal static void VerifyADFSServer2019(PSHost host = null)
        {
            RegistryVersion reg = new RegistryVersion();
            if (!reg.IsWindows2019)
            {
                if (host==null)
                    throw new InvalidOperationException("Must be executed on Windows 2019 server and up only !");
                else
                    throw new InvalidOperationException("PS0033: This Cmdlet must be executed on Windows 2019 server and up only !");
            }
        }

        /// <summary>
        /// VerifyADFSAdministrationRights method implementation
        /// </summary>
        internal static void VerifyADFSAdministrationRights(PSHost host = null)
        {
            ClientSIDsProxy.Initialize();
            if (!((ClientSIDsProxy.ADFSLocalAdminServiceAdministrationAllowed && ADFSManagementRights.IsAdministrator()) ||
                  (ClientSIDsProxy.ADFSSystemServiceAdministrationAllowed && ADFSManagementRights.IsSystem()) ||
                  (ClientSIDsProxy.ADFSDelegateServiceAdministrationAllowed && ADFSManagementRights.AllowedGroup(ClientSIDsProxy.ADFSAdminGroupName))))
            {
                if (host==null)
                    throw new InvalidOperationException("Must be executed with ADFS Administration rights granted for the current user !");
                else
                    throw new InvalidOperationException("PS0033: This Cmdlet must be executed with ADFS Administration rights granted for the current user !");
            }
        }

        /// <summary>
        /// VerifyMFAConfigurationRights method implementation
        /// </summary>
        internal static void VerifyMFAConfigurationRights(PSHost host = null)
        {
            if (!(ADFSManagementRights.IsAdministrator()) || ADFSManagementRights.IsSystem())
            {
                if (host==null)
                    throw new InvalidOperationException("Must be executed with System Administration rights granted for the current user !");
                else
                    throw new InvalidOperationException("PS0033: This Cmdlet must be executed with System Administration rights granted for the current user !");
            }
        }

        #endregion

        #region Notifications
        /// <summary>
        /// BroadcastNotification method implmentation
        /// </summary>
        internal static void BroadcastNotification(byte kind, string message, bool local = true, bool dispatch = true)
        {
            BroadcastNotification(ADFSManager.Config, (NotificationsKind)kind, message, local, dispatch);
        }

        /// <summary>
        /// BroadcastNotification method implmentation
        /// </summary>
        internal static void BroadcastNotification(MFAConfig config, NotificationsKind kind, string message, bool local = true, bool dispatch = true)
        {
            CFGUtilities.BroadcastNotification(config, kind, message, local, dispatch);
        }
        #endregion

        #region Computers Registration
        /// <summary>
        /// RegisterADFSComputer method implementation
        /// </summary>        
        public static bool RegisterADFSComputer(PSHost host, string servername, out List<ADFSServerHost> servers)
        {
            bool bRet = false;
            string fqdn = Dns.GetHostEntry(servername).HostName.ToLower();
            ADFSServerHost srvhost = WebAdminManagerClient.GetComputerInformations(fqdn);
            if (srvhost != null)
            {
                int i = ADFSManager.ADFSFarm.Servers.FindIndex(c => c.FQDN.ToLower().Equals(srvhost.FQDN.ToLower()));
                if (i < 0)
                    ADFSManager.ADFSFarm.Servers.Add(srvhost);
                else
                    ADFSManager.ADFSFarm.Servers[i] = srvhost;
                ADFSManager.SetDirty(true);
                ADFSManager.WriteConfiguration(host);
                bRet = true;
            }
            servers = ADFSManager.ADFSFarm.Servers;
            return bRet;
        }

        /// <summary>
        /// UnRegisterADFSComputer method implementation
        /// </summary>
        public static bool UnRegisterADFSComputer(PSHost Host, string servername, out List<ADFSServerHost> servers)
        {
            string fqdn = Dns.GetHostEntry(servername).HostName;
            ADFSManager.ADFSFarm.Servers.RemoveAll(c => c.FQDN.ToLower().Equals(fqdn.ToLower()));
            ADFSManager.SetDirty(true);
            ADFSManager.WriteConfiguration(Host);
            servers = ADFSManager.ADFSFarm.Servers;
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
            WebAdminManagerClient.AddFirewallRules(computers);
        }

        /// <summary>
        /// RemoveFirewallRules method implementation
        /// </summary>
        internal static void RemoveFirewallRules()
        {
            WebAdminManagerClient.RemoveFirewallRules();
        }       
        #endregion

        #region Common other methods
        /// <summary>
        /// UpdateCertificatesACL method implementation
        /// </summary>
        internal static bool UpdateCertificatesACL(KeyMgtOptions options = KeyMgtOptions.AllCerts)
        {
            return WebAdminManagerClient.UpdateCertificatesACL(options);
        }

        /// <summary>
        /// CleanOrphanedPrivateKeys method implmentation 
        /// </summary>
        internal static int CleanOrphanedPrivateKeys(byte option, int delay)
        {
            return WebAdminManagerClient.CleanOrphanedPrivateKeys(option, delay);
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
                    using (SystemEncryption MSIS = new SystemEncryption())
                    {
                        if (clearvalue)
                        {
                            config.Hosts.ActiveDirectoryHost.Password = string.Empty;
                            config.Hosts.ActiveDirectoryHost.Account = string.Empty;
                            config.Hosts.ActiveDirectoryHost.DomainAddress = string.Empty;
                            config.Hosts.SQLServerHost.SQLAccount = string.Empty;
                            config.Hosts.SQLServerHost.SQLPassword = string.Empty;
                            config.MailProvider.Password = string.Empty;
                            config.MailProvider.UserName = string.Empty;
                            config.MailProvider.Anonymous = true;
                            config.KeysConfig.XORSecret = XORUtilities.DefaultKey;
                        }
                        else
                        {
                            config.KeysConfig.XORSecret = MSIS.Encrypt(config.KeysConfig.XORSecret, "Pass Phrase Encryption");
                            config.Hosts.ActiveDirectoryHost.Password = MSIS.Encrypt(config.Hosts.ActiveDirectoryHost.Password, "ADDS Super Account Password");
                            config.Hosts.SQLServerHost.SQLPassword = MSIS.Encrypt(config.Hosts.SQLServerHost.SQLPassword, "SQL Super Account Password");
                            config.MailProvider.Password = MSIS.Encrypt(config.MailProvider.Password, "Mail Provider Account Password");

                            if (!string.IsNullOrEmpty(value))
                                host.UI.WriteWarningLine("Block Updates not allowed, values where only encrypted !");
                        }
                    }
                    break;
                case 0x01:
                    using (SystemEncryption MSIS = new SystemEncryption())
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
                                config.Hosts.ActiveDirectoryHost.Password = MSIS.Encrypt(config.Hosts.ActiveDirectoryHost.Password, "ADDS Super Account Password");
                                host.UI.WriteWarningLine("Empty value not allowed, value was only encrypted !");
                            }
                            else
                                config.Hosts.ActiveDirectoryHost.Password = MSIS.Encrypt(value, "ADDS Super Account Password");
                        }
                    }
                    break;
                case 0x02:
                    using (SystemEncryption MSIS = new SystemEncryption())
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
                                config.MailProvider.Password = MSIS.Encrypt(config.MailProvider.Password, "Mail Provider Account Password");
                                host.UI.WriteWarningLine("Empty value not allowed, value was only encrypted !");
                            }
                            else
                                config.MailProvider.Password = MSIS.Encrypt(value, "Mail Provider Account Password");
                        }
                    }
                    break;
                case 0x03:
                    using (SystemEncryption MSIS = new SystemEncryption())
                    {
                        if (clearvalue)
                        {
                            config.KeysConfig.XORSecret = XORUtilities.DefaultKey;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(value))
                            {
                                config.KeysConfig.XORSecret = MSIS.Encrypt(config.KeysConfig.XORSecret, "Pass Phrase Encryption");
                                host.UI.WriteWarningLine("Empty value not allowed, value was only encrypted !");
                            }
                            else
                                config.KeysConfig.XORSecret = MSIS.Encrypt(value, "Pass Phrase Encryption");
                        }
                    }
                    break;
                case 0x04:
                    using (SystemEncryption MSIS = new SystemEncryption())
                    {
                        if (clearvalue)
                        {
                            config.Hosts.SQLServerHost.SQLAccount = string.Empty;
                            config.Hosts.SQLServerHost.SQLPassword = string.Empty;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(value))
                            {
                                config.Hosts.SQLServerHost.SQLPassword = MSIS.Encrypt(config.Hosts.SQLServerHost.SQLPassword, "SQL Super Account Password");
                                host.UI.WriteWarningLine("Empty value not allowed, value was only encrypted !");
                            }
                            else
                                config.Hosts.SQLServerHost.SQLPassword = MSIS.Encrypt(value, "SQL Super Account Password");
                        }
                    }
                    break;

            }
            CFGUtilities.WriteConfigurationToDatabase(host, config, false);
            CFGUtilities.BroadcastNotification(config, NotificationsKind.ConfigurationCreated, Environment.MachineName, true, true);
        }

        /// <summary>
        /// ExportMFAMailTemplates method implementation 
        /// </summary>
        internal static void ExportMFAMailTemplates(PSHost host, int lcid)
        {
            MFAConfig config = CFGUtilities.ReadConfiguration(host);
            Dictionary<string, string> data = new Dictionary<string, string>();
            ResourcesLocale Resources = new ResourcesLocale(lcid);
            data.Add("MailOTPContent.html", Resources.GetString(ResourcesLocaleKind.Mail, "MailOTPContent"));
            data.Add("MailKeyContent.html", Resources.GetString(ResourcesLocaleKind.Mail, "MailKeyContent"));
            data.Add("MailAdminContent.html", Resources.GetString(ResourcesLocaleKind.Mail, "MailAdminContent"));
            data.Add("MailNotifications.html", Resources.GetString(ResourcesLocaleKind.Mail, "MailNotifications"));

            if (WebAdminManagerClient.ExportMailTemplates(config, lcid, data))
            { 
                CFGUtilities.WriteConfiguration(host, config);
                CFGUtilities.BroadcastNotification(config, NotificationsKind.ConfigurationReload, Environment.MachineName, true, true);
            }
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
            CFGUtilities.BroadcastNotification(config, NotificationsKind.ConfigurationReload, Environment.MachineName, true, true);
        }

        /// <summary>
        /// ResetWebThemesList method implementation
        /// </summary>
        internal static void ResetWebThemesList(PSHost host)
        {
            MFAConfig config = CFGUtilities.ReadConfiguration(host);
            WebThemeManagerClient.ResetThemesList(config);
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
        #endregion
    }

    /// <summary>
    /// ADFSManagementRights class
    /// </summary>
    public static class ADFSManagementRights
    {
        /// <summary>
        /// IsAdministrator method implementation
        /// </summary>
        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            try
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// IsSystem method implementation
        /// </summary>
        public static bool IsSystem()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                return identity.IsSystem;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// AllowedGroup method implementation
        /// </summary>
        public static bool AllowedGroup(string group)
        {
            if (string.IsNullOrEmpty(group))
                return false;
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(group);
            }
            catch
            {
                return false;
            }
        }
    }
    #endregion
}