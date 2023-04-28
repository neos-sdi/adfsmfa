//******************************************************************************************************************************************************************************************//
// Copyright (c) 2023 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
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
// #define debugsid
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Neos.IdentityServer.MultiFactor.Data
{
    #region ReplayMangerClient
    /// <summary>
    /// ReplayMangerClient class
    /// </summary>
    public static class ReplayManagerClient
    {
        /// <summary>
        /// Continue method iplementation
        /// </summary>
        public static bool Check(List<string> computers, ReplayRecord record)
        {
            ReplayClient manager = new ReplayClient();
            manager.Initialize(); 
            try
            {
                IReplay client = manager.Open();
                try
                {
                    return client.Check(computers, record);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// Pause method iplementation
        /// </summary>

        public static void Reset(List<string> computers)
        {
            ReplayClient manager = new ReplayClient();
            manager.Initialize();
            try
            {
                IReplay client = manager.Open();
                try
                {
                    client.Reset(computers);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }
    }
    #endregion

    #region WebThemeManagerClient
    public static class WebThemeManagerClient
    {
        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public static void Initialize(MFAConfig config, AuthenticationContext context, Uri request)
        {
            WebThemesClient manager = new WebThemesClient();
            manager.Initialize();
            try
            {
                IWebThemeManager client = manager.Open();
                try
                {
                    WebThemesParametersRecord message = new WebThemesParametersRecord()
                    {
                        Identifier = context.ThemeIdentifier,
                        LCID = context.Lcid
                    };
                    var servernames = (from server in config.Hosts.ADFSFarm.Servers
                                       select (server.FQDN.ToLower(), server.NodeType.ToLower().Equals("primarycomputer")));

                    Dictionary<string, bool> dic = servernames.ToDictionary(pair => pair.Item1, pair => pair.Item2);
                    string identifier = string.Empty;
                    if (client.Initialize(dic, message, request.OriginalString, out identifier))
                        context.ThemeIdentifier = identifier;
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// ResetThemesList method implmentation
        /// </summary>
        public static void ResetThemesList(MFAConfig config)
        {
            WebThemesClient manager = new WebThemesClient();
            manager.Initialize();
            try
            {
                IWebThemeManager client = manager.Open();
                try
                {
                    var servernames = (from server in config.Hosts.ADFSFarm.Servers
                                       select (server.FQDN.ToLower(), server.NodeType.ToLower().Equals("primarycomputer")));

                    Dictionary<string, bool> dic = servernames.ToDictionary(s => s.Item1, s => s.Item2);
                    client.ResetThemesList(dic);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// HasRelyingPartyTheme
        /// </summary>
        public static bool HasRelyingPartyTheme(AuthenticationContext context)
        {
            WebThemesClient manager = new WebThemesClient();
            manager.Initialize();
            try
            {
                IWebThemeManager client = manager.Open();
                try
                {
                    WebThemesParametersRecord message = new WebThemesParametersRecord()
                    {
                        Identifier = context.ThemeIdentifier,
                        LCID = context.Lcid
                    };
                    return client.HasRelyingPartyTheme(message);
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// GetIllustrationAddress method implementation
        /// </summary>
        public static string GetIllustrationAddress(AuthenticationContext context)
        {
            WebThemesClient manager = new WebThemesClient();
            manager.Initialize();
            try
            {                
                IWebThemeManager client = manager.Open();
                try
                {
                    WebThemesParametersRecord message = new WebThemesParametersRecord()
                    {
                        Identifier = context.ThemeIdentifier,
                        LCID = context.Lcid
                    };
                    return client.GetIllustrationAddress(message);
                }
                catch (Exception)
                {                    
                    return string.Empty; 
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// GetLogoAddress method implementation
        /// </summary>
        public static string GetLogoAddress(AuthenticationContext context)
        {
            WebThemesClient manager = new WebThemesClient();
            manager.Initialize();
            try
            {
                IWebThemeManager client = manager.Open();
                try
                {
                    WebThemesParametersRecord message = new WebThemesParametersRecord()
                    {
                        Identifier = context.ThemeIdentifier,
                        LCID = context.Lcid
                    };
                    return client.GetLogoAddress(message);
                }
                catch (Exception)
                {                   
                    return string.Empty;
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// GetStyleSheetAddress method implementation
        /// </summary>
        public static string GetStyleSheetAddress(AuthenticationContext context)
        {
            WebThemesClient manager = new WebThemesClient();
            manager.Initialize();
            try
            {
                IWebThemeManager client = manager.Open();
                try
                {
                    WebThemesParametersRecord message = new WebThemesParametersRecord()
                    {
                        Identifier = context.ThemeIdentifier,
                        LCID = context.Lcid
                    };
                    return client.GetStyleSheetAddress(message);
                }
                catch (Exception)
                {                    
                    return string.Empty;
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// GetAddresses method implmentation
        /// </summary>
        public static Dictionary<WebThemeAddressKind, string> GetAddresses(AuthenticationContext context)
        {
            WebThemesClient manager = new WebThemesClient();
            manager.Initialize();
            try
            {                
                IWebThemeManager client = manager.Open();
                try
                {
                    WebThemesParametersRecord message = new WebThemesParametersRecord()
                    {
                        Identifier = context.ThemeIdentifier,
                        LCID = context.Lcid
                    };
                    return client.GetAddresses(message);
                }
                catch (Exception)
                {                    
                    return null;
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }
    }
    #endregion

    #region WebAdminManagerClient
    public static class WebAdminManagerClient
    {
        /// <summary>
        /// GetSIDsInformations method implmentation
        /// </summary>
        public static SIDsParametersRecord GetSIDsInformations(MFAConfig config)
        {
            string fqdn = Dns.GetHostEntry("localhost").HostName;
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.GetSIDsInformations(GetServers(config));
                }
                catch (Exception ex)
                {
                    Log.WriteEntry(string.Format("Error on WebAdminService Service GetSIDsInformations method : {0} / {1}.", fqdn, ex.Message), EventLogEntryType.Error, 2010);
                    return new SIDsParametersRecord() { Loaded = false };
                }
                finally
                {
                    manager.Close(client);
                }
            }
            catch (Exception e)
            {
                Log.WriteEntry(string.Format("Error on WebAdminService Service GetSIDsInformations method : {0} / {1}.", fqdn, e.Message), EventLogEntryType.Error, 2010);
            }
            finally
            {
                manager.UnInitialize();
            }
            return null;
        }

        /// <summary>
        /// UpdateDirectoriesACL method implementation
        /// </summary>
        public static void UpdateDirectoriesACL()
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    client.UpdateDirectoriesACL(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + Path.DirectorySeparatorChar + "MFA");
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// UpdateCertificatesACL method implementation
        /// </summary>
        public static bool UpdateCertificatesACL(KeyMgtOptions options)
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.UpdateCertificatesACL(options);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// CleanOrphanedPrivateKeys method implementation
        /// </summary>
        public static int CleanOrphanedPrivateKeys(byte option, int delay)
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.CleanOrphanedPrivateKeys(option, delay);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// GetCertificate method implementation
        /// </summary>
        public static bool CertificateExists(string thumbprint, StoreLocation location)
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.CertificateExists(thumbprint, (byte)location);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// NewMFASystemMasterKey method implementation
        /// </summary>
        public static bool NewMFASystemMasterKey(MFAConfig config, bool deployonly = false, bool deleteonly = false)
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.NewMFASystemMasterKey(GetServers(config), deployonly, deleteonly);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// NewMFASystemMasterKey method implementation
        /// </summary>
        public static bool ExistsMFASystemMasterKey()
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.ExistsMFASystemMasterkey();
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// NewMFASystemAESCngKey method implementation
        /// </summary>
        public static bool NewMFASystemAESCngKey(MFAConfig config, bool deployonly = false, bool deleteonly = false)
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.NewMFASystemAESCngKey(GetServers(config), deployonly, deleteonly);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// ExistsMFASystemAESCngKeys method implementation
        /// </summary>
        public static bool ExistsMFASystemAESCngKeys()
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.ExistsMFASystemAESCngKeys();
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// CreateSelfSignedCertificate method implementation
        /// </summary>
        public static bool CreateSelfSignedCertificate(string subjectName, string dnsName, CertificatesKind kind, int years, string path, string pwd = "")
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.CreateSelfSignedCertificate(subjectName, dnsName, kind, years, path, pwd);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// CreateRSACertificate method implementation
        /// </summary>
        public static string CreateRSACertificate(MFAConfig config, string subject, int years)
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.CreateRSACertificate(GetServers(config), subject, years);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// CreateRSACertificateForSQLEncryption method implementation
        /// </summary>
        public static string CreateRSACertificateForSQLEncryption(MFAConfig config, string subject, int years)
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.CreateRSACertificateForSQLEncryption(GetServers(config), subject, years);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// CreateADFSCertificate method implementation
        /// </summary>
        public static bool CreateADFSCertificate(MFAConfig config, string subject, ADFSCertificatesKind kind, int years)
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.CreateADFSCertificate(GetServers(config), subject, kind, years);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }
      
        #region Firewall
        /// <summary>
        /// AddFirewallRules method implmentation
        /// </summary>
        public static void AddFirewallRules(string computers)
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    client.AddFirewallRules(computers);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// RemoveFirewallRules method implementation
        /// </summary>
        public static void RemoveFirewallRules()
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    client.RemoveFirewallRules();
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }
        #endregion

        #region Notifications
        /// <summary>
        /// BroadcastNotification method implementation
        /// </summary>
        public static void BroadcastNotification(MFAConfig config, NotificationsKind kind, string message, bool local = true, bool dispatch = true)
        {
            WebAdminClient manager = new WebAdminClient();
            try
            {
                manager.Initialize();     
                IWebAdminServices client = manager.Open();
                try
                {
                    client.BroadcastNotification(GetServers(config), CFGReaderUtilities.GetCryptedConfig(config), kind, message, local, dispatch);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// ExportMailTemplates method implementation 
        /// </summary>
        public static bool ExportMailTemplates(MFAConfig config, int lcid, Dictionary<string, string> data)
        {
            bool done = false;
            try
            {
                WebAdminClient manager = new WebAdminClient();
                try
                {
                    manager.Initialize();
                    IWebAdminServices client = manager.Open();
                    try
                    {
                        done = client.ExportMailTemplates(GetServers(config), CFGReaderUtilities.GetCryptedConfig(config), lcid, data);
                    }
                    finally
                    {
                        manager.Close(client);
                    }
                }
                finally
                {
                    manager.UnInitialize();
                }

                if (done)
                {
                    char sep = Path.DirectorySeparatorChar;
                    string htmlpath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "MailTemplates" + sep + lcid.ToString();

                    MailProvider mailprov = config.MailProvider;
                    if (!mailprov.MailOTPContent.Exists(c => c.LCID.Equals(lcid)))
                        mailprov.MailOTPContent.Add(new SendMailFileName(lcid, htmlpath + sep + "MailOTPContent.html"));
                    if (!mailprov.MailKeyContent.Exists(c => c.LCID.Equals(lcid)))
                        mailprov.MailKeyContent.Add(new SendMailFileName(lcid, htmlpath + sep + "MailKeyContent.html"));
                    if (!mailprov.MailAdminContent.Exists(c => c.LCID.Equals(lcid)))
                        mailprov.MailAdminContent.Add(new SendMailFileName(lcid, htmlpath + sep + "MailAdminContent.html"));
                    if (!mailprov.MailNotifications.Exists(c => c.LCID.Equals(lcid)))
                        mailprov.MailNotifications.Add(new SendMailFileName(lcid, htmlpath + sep + "MailNotifications.html"));
                }
                return done;
            }
            catch 
            {
                return false;
            }            
        }        
        #endregion

        #region Registry Versions
        /// <summary>
        /// GetCompterInformations method informations
        /// </summary>
        public static ADFSServerHost GetComputerInformations(string fqdn)
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.GetComputerInformations(fqdn);
                }                
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// GetAllComputerInformations method informations
        /// </summary>
        public static Dictionary<string, ADFSServerHost> GetAllComputerInformations()
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.GetAllComputersInformations();
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// GetServers method implementation
        /// </summary>
        private static Dictionary<string, bool> GetServers(MFAConfig config)
        {
            var servernames = (from server in config.Hosts.ADFSFarm.Servers
                               select (server.FQDN.ToLower(), server.NodeType.ToLower().Equals("primarycomputer")));
            Dictionary<string, bool> servers = servernames.ToDictionary(s => s.Item1, s => s.Item2);
            return servers;
        }
        #endregion
    }
    #endregion

    #region NTServiceManagerClient
    /// <summary>
    /// NTServiceManagerClient class
    /// </summary>
    public static class NTServiceManagerClient
    {       
        /// <summary>
        /// Continue method iplementation
        /// </summary>
        public static bool Continue(string name, string machinename)
        {
            NTServiceClient manager = new NTServiceClient();
            manager.Initialize();
            try
            {
                INTService client = manager.Open();
                try
                {
                    return client.Continue(name, machinename);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// Pause method iplementation
        /// </summary>
        public static bool Pause(string name, string machinename)
        {
            NTServiceClient manager = new NTServiceClient();
            manager.Initialize();
            try
            {
                INTService client = manager.Open();
                try
                {
                    return client.Pause(name, machinename);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// Start method iplementation
        /// </summary>
        public static bool Start(string name, string machinename)
        {
            NTServiceClient manager = new NTServiceClient();
            manager.Initialize();
            try
            { 
                INTService client = manager.Open();
                try
                {
                    return client.Start(name, machinename);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// Stop method iplementation
        /// </summary>
        public static bool Stop(string name, string machinename)
        {
            NTServiceClient manager = new NTServiceClient();
            manager.Initialize();
            try
            { 
                INTService client = manager.Open();
                try
                {
                    return client.Stop(name, machinename);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// IsRunning method iplementation
        /// </summary>
        public static bool IsRunning(string name, string machinename)
        {
            NTServiceClient manager = new NTServiceClient();
            manager.Initialize();
            try
            {
                INTService client = manager.Open();
                try
                {
                    return client.IsRunning(name, machinename);
                }
                finally
                {
                    manager.Close(client);
                }
            }             
            finally
            {
                manager.UnInitialize();
            }
        }

        /// <summary>
        /// Exists method iplementation
        /// </summary>
        public static bool Exists(string name, string machinename)
        {
            NTServiceClient manager = new NTServiceClient();
            manager.Initialize();
            try
            {
                INTService client = manager.Open();
                try
                {
                    return client.Exists(name, machinename);
                }
                finally
                {
                    manager.Close(client);
                }
            }
            finally
            {
                manager.UnInitialize();
            }
        }
    }
    #endregion

    #region ClientSIDsProxy
    /// <summary>
    /// 
    /// </summary>
    public static class ClientSIDsProxy
    {
        private static string s_aDFSAccountSID = string.Empty;
        private static string s_aDFSAccountName = string.Empty;
        private static string s_aDFSServiceSID = string.Empty;
        private static string s_aDFSServiceName = string.Empty;
        private static string s_aDFSAdminGroupSID = string.Empty;
        private static string s_aDFSAdminGroupName = string.Empty;
        private static bool s_aDFSDelegateServiceAdministrationAllowed;
        private static bool s_aDFSLocalAdminServiceAdministrationAllowed;
        private static bool s_aDFSSystemServiceAdministrationAllowed;

        /// <summary>
        /// Loaded property implmentation
        /// </summary>
        public static bool Loaded { get; set; } = false;

        /// <summary>
        /// ADFSDelegateServiceAdministrationAllowed property
        /// </summary>
        public static bool ADFSDelegateServiceAdministrationAllowed
        {
            get
            {
                EnsureLoaded();
                return s_aDFSDelegateServiceAdministrationAllowed;
            }
            private set
            {
                if (!value)
                {
                    s_aDFSAdminGroupSID = string.Empty;
                    s_aDFSAdminGroupName = string.Empty;
                }
                s_aDFSDelegateServiceAdministrationAllowed = value;
            }
        }

        /// <summary>
        /// ADFSLocalAdminServiceAdministrationAllowed property
        /// </summary>
        public static bool ADFSLocalAdminServiceAdministrationAllowed
        {
            get
            {
                EnsureLoaded();
                return s_aDFSLocalAdminServiceAdministrationAllowed;
            }
            private set => s_aDFSLocalAdminServiceAdministrationAllowed = value;
        }

        /// <summary>
        /// ADFSLocalAdminServiceAdministrationAllowed property
        /// </summary>
        public static bool ADFSSystemServiceAdministrationAllowed
        {
            get
            {
                EnsureLoaded();
                return s_aDFSSystemServiceAdministrationAllowed;
            }
            private set => s_aDFSSystemServiceAdministrationAllowed = value;
        }

        /// <summary>
        /// ADFSAccountSID property implmentation
        /// </summary>
        public static string ADFSAccountSID
        {
            get
            {
                EnsureLoaded();
                return s_aDFSAccountSID;
            }
            private set => s_aDFSAccountSID = value;
        }

        /// <summary>
        /// ADFSAccountName property implmentation
        /// </summary>
        public static string ADFSAccountName
        {
            get
            {
                EnsureLoaded();
                return s_aDFSAccountName;
            }
            private set => s_aDFSAccountName = value;
        }

        /// <summary>
        /// ADFSServiceSID property implmentation
        /// </summary>
        public static string ADFSServiceSID
        {
            get
            {
                EnsureLoaded();
                return s_aDFSServiceSID;
            }
            private set => s_aDFSServiceSID = value;
        }

        /// <summary>
        /// ADFSServiceName property implmentation
        /// </summary>
        public static string ADFSServiceName
        {
            get
            {
                EnsureLoaded();
                return s_aDFSServiceName;
            }
            private set => s_aDFSServiceName = value;
        }

        /// <summary>
        /// ADFSAdminGroupSID property implmentation
        /// </summary>
        public static string ADFSAdminGroupSID
        {
            get
            {
                EnsureLoaded();
                return s_aDFSAdminGroupSID;
            }
            private set => s_aDFSAdminGroupSID = value;
        }

        /// <summary>
        /// ADFSAdminGroupName property implmentation
        /// </summary>
        public static string ADFSAdminGroupName
        {
            get
            {
                EnsureLoaded();
                return s_aDFSAdminGroupName;
            }
            private set => s_aDFSAdminGroupName = value;
        }

        /// <summary>
        /// EnsureLoaded method implmentation
        /// </summary>
        public static void Initialize(MFAConfig cfg = null)
        {
            EnsureLoaded(cfg);
        }

        /// <summary>
        /// EnsureLoaded method implmentation
        /// </summary>
        private static void EnsureLoaded(MFAConfig cfg = null)
        {
            try
            {
#if debugsid
                Log.WriteEntry("Starting Retreive SIDs on Client Side", EventLogEntryType.Warning, 9001);
#endif
                if (!Loaded)
                {
#if debugsid
                    Log.WriteEntry("Starting Loading Configuration on Client Side", EventLogEntryType.Warning, 9002);
#endif
                    MFAConfig config = null;
                    try
                    {
#if debugsid
                        Log.WriteEntry("Loading Configuration on Client Side", EventLogEntryType.Warning, 9003);
#endif
                        config = CFGReaderUtilities.ReadConfiguration();
#if debugsid
                        Log.WriteEntry("Configuration correctly loaded on Client Side", EventLogEntryType.SuccessAudit, 9004);
#endif
                    }
                    catch (Exception e)
                    {
                        Log.WriteEntry(string.Format("Error retreiving security descriptors configuration : {0} ", e.Message), EventLogEntryType.Error, 2012);
                        throw e;
                    }
                    if (config != null)
                    {
#if debugsid
                        Log.WriteEntry("Starting Loading SIDs on Client Side", EventLogEntryType.Warning, 9005);
#endif
                        SIDsParametersRecord rec = null;
                        try
                        {
#if debugsid
                            Log.WriteEntry("Calling MFA Service from Client Side", EventLogEntryType.Warning, 9006);
#endif
                            rec = WebAdminManagerClient.GetSIDsInformations(config);
#if debugsid
                            Log.WriteEntry("SIDs returned from MFA Service on Client Side", EventLogEntryType.Warning, 9007);
#endif
                        }
                        catch (Exception e)
                        {
                            Log.WriteEntry(string.Format("Error retreiving security descriptors from MFA Service : {0} ", e.Message), EventLogEntryType.Error, 2012);
                            throw e;
                        }
#if debugsid
                        Log.WriteEntry(string.Format("Loading ADFS Account SID : {0}", rec.ADFSAccountSID), EventLogEntryType.SuccessAudit, 9100);
#endif
                        ADFSAccountSID = rec.ADFSAccountSID;
#if debugsid
                        Log.WriteEntry(string.Format("Loading ADFS Account Name : {0}", rec.ADFSAccountName), EventLogEntryType.SuccessAudit, 9101);
#endif
                        ADFSAccountName = rec.ADFSAccountName;
#if debugsid
                        Log.WriteEntry(string.Format("Loading ADFS Service Account SID : {0}", rec.ADFSServiceAccountSID), EventLogEntryType.SuccessAudit, 9102);
#endif
                        ADFSServiceSID = rec.ADFSServiceAccountSID;
#if debugsid
                        Log.WriteEntry(string.Format("Loading ADFS Service Account Name : {0}", rec.ADFSServiceAccountName), EventLogEntryType.SuccessAudit, 9103);
#endif
                        ADFSServiceName = rec.ADFSServiceAccountName;
#if debugsid
                        Log.WriteEntry(string.Format("Loading ADFS Administration Group SID : {0}", rec.ADFSAdministrationGroupSID), EventLogEntryType.SuccessAudit, 9104);
#endif
                        ADFSAdminGroupSID = rec.ADFSAdministrationGroupSID;
#if debugsid
                        Log.WriteEntry(string.Format("Loading ADFS Administration Group Name : {0}", rec.ADFSAdministrationGroupName), EventLogEntryType.SuccessAudit, 9105);
#endif
                        ADFSAdminGroupName = rec.ADFSAdministrationGroupName;
#if debugsid
                        Log.WriteEntry(string.Format("Loading ADFS ADFSDelegateServiceAdministrationAllowed Property : {0}", rec.ADFSDelegateServiceAdministrationAllowed), EventLogEntryType.SuccessAudit, 9106);
#endif
                        ADFSDelegateServiceAdministrationAllowed = rec.ADFSDelegateServiceAdministrationAllowed;
#if debugsid
                        Log.WriteEntry(string.Format("Loading ADFS ADFSLocalAdminServiceAdministrationAllowed Property : {0}", rec.ADFSLocalAdminServiceAdministrationAllowed), EventLogEntryType.SuccessAudit, 9107);
#endif
                        ADFSLocalAdminServiceAdministrationAllowed = rec.ADFSLocalAdminServiceAdministrationAllowed;
#if debugsid
                        Log.WriteEntry(string.Format("Loading ADFS ADFSSystemServiceAdministrationAllowed Property : {0}", rec.ADFSSystemServiceAdministrationAllowed), EventLogEntryType.SuccessAudit, 9108);
#endif
                        ADFSSystemServiceAdministrationAllowed = rec.ADFSSystemServiceAdministrationAllowed;
#if debugsid
                        Log.WriteEntry(string.Format("SIDs Loaded : {0}", rec.Loaded), EventLogEntryType.Warning, 9109);
#endif
                        Loaded = rec.Loaded;
                    }
                    else
                        Log.WriteEntry("Error retreiving security descriptors : Configuration is NULL", EventLogEntryType.Error, 2012);

                }
            }
            catch (Exception e)
            {
                Log.WriteEntry(string.Format("Error retreiving security descriptors : {0} ", e.Message), EventLogEntryType.Error, 2012);
                throw e;
            }
        }
    }
#endregion
}
