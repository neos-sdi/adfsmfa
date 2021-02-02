//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 @redhook62 (adfsmfa@gmail.com)                                                                                                                                        //                        
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
using Neos.IdentityServer.MultiFactor.Data;
using Neos.IdentityServer.MultiFactor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// ReplayService class
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ReplayService : IReplay
    {
        private static ReplayManager _manager = null;
        private static readonly object _lock = new object();
        private EventLog _log;

        /// <summary>
        /// Constructor implmentation
        /// </summary>
        public ReplayService(IDependency dep)
        {
            _log = dep.GetEventLog();
            if (_manager==null)
                _manager = new ReplayManager(dep); 
        }

        /// <summary>
        /// Check method implementation
        /// </summary>
        public bool Check(List<string> servernames, ReplayRecord record)
        {
            bool tempresult = false;
            try
            {
                lock (_lock)
                {
                    tempresult = _manager.AddToReplay(record);
                    if (tempresult)
                    {
                        if ((record.MustDispatch) && (servernames != null))
                        {
                            foreach (string fqdn in servernames)
                            {
                                ReplayClient replaymanager = new ReplayClient();
                                replaymanager.Initialize(fqdn);
                                try
                                {
                                    IReplay client = replaymanager.Open();
                                    try
                                    {
                                        record.MustDispatch = false;
                                        tempresult = client.Check(servernames, record);
                                    }
                                    catch (Exception e)
                                    {
                                        _log.WriteEntry(string.Format("Error on Check Remote Service method : {0} => {1}.", fqdn, e.Message), EventLogEntryType.Error, 1011);
                                    }
                                    finally
                                    {
                                        replaymanager.Close(client);
                                    }
                                }
                                finally
                                {
                                    replaymanager.UnInitialize();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on Check Service method : {0}.", e.Message), EventLogEntryType.Error, 1010);
            }
            return tempresult;
        }

        /// <summary>
        /// Reset method implementation
        /// </summary>
        public void Reset(List<string> servernames)
        {
            try
            {
                lock (_lock)
                {
                    _manager.Reset();
                }
                if (servernames != null)
                {
                    foreach (string fqdn in servernames)
                    {
                        ReplayClient replaymanager = new ReplayClient();
                        replaymanager.Initialize(fqdn);
                        try
                        {
                            IReplay client = replaymanager.Open();
                            try
                            {
                                client.Reset(null);
                            }
                            catch (Exception e)
                            {
                                _log.WriteEntry(string.Format("Error on Check Remote Service method : {0} => {1}.", fqdn, e.Message), EventLogEntryType.Error, 1011);
                            }
                            finally
                            {
                                replaymanager.Close(client);
                            }
                        }
                        finally
                        {
                            replaymanager.UnInitialize();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on Reset Service method : {0}.", e.Message), EventLogEntryType.Error, 1011);
            }
        }
    }

    /// <summary>
    /// WebThemeService class
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WebThemeService : IWebThemeManager
    {
        private static WebThemeManager _manager = null;
        private EventLog _log;

        /// <summary>
        /// Constructor implmentation
        /// </summary>
        public WebThemeService(IDependency dep)
        {
            _log = dep.GetEventLog();
            _manager = new WebThemeManager(dep);
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public bool Initialize(Dictionary<string, bool> servers, WebThemesParametersRecord context, string request, out string identifier)
        {
            try
            {
                return _manager.Initialize(servers, context, new Uri(request), out identifier);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service Initialize method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// HasRelyingPartyTheme method implmentation
        /// </summary>
        public bool HasRelyingPartyTheme(WebThemesParametersRecord context)
        {
            try
            {
                return _manager.HasRelyingPartyTheme(context);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service HasRelyingPartyTheme method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// GetAddresses method implementation
        /// </summary>
        public Dictionary<WebThemeAddressKind, string> GetAddresses(WebThemesParametersRecord context)
        {
            try
            {
                return _manager.GetAddresses(context);   
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service GetAddresses method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// GetIllustrationAddress method implementation
        /// </summary>
        public string GetIllustrationAddress(WebThemesParametersRecord context)
        {
            try
            {
                return _manager.GetIllustrationAddress(context);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service GetIllustrationAddress method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// GetLogoAddress method implementation
        /// </summary>
        public string GetLogoAddress(WebThemesParametersRecord context)
        {
            try
            {
                return _manager.GetLogoAddress(context);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service GetLogoAddress method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// GetStyleSheetAddress method implementation
        /// </summary>
        public string GetStyleSheetAddress(WebThemesParametersRecord context)
        {
            try
            {
                return _manager.GetStyleSheetAddress(context);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service GetStyleSheetAddress method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// DispachTheme method implmentation
        /// </summary>
        public void DispachTheme(WebThemeRecord theme)
        {
            try
            {
                _manager.DispachTheme(theme);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service DispachTheme method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// RequestTheme method implmentation
        /// </summary>
        public WebThemeRecord RequestTheme(WebThemesParametersRecord theme)
        {
            try
            {
                return _manager.RequestTheme(theme);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service RequestTheme method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// ResetThemesList method implmentation
        /// </summary>
        public void ResetThemes()
        {
            try
            {
                _manager.ResetThemes();
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service RestThemesList method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// ResetThemesList method implmentation
        /// </summary>
        public void ResetThemesList(Dictionary<string, bool> servers)
        {
            try
            {
                _manager.ResetThemesList(servers);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebTheme Service RestThemesList method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }
    }

    /// <summary>
    /// WebThemeService class
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WebAdminService : IWebAdminServices
    {
        private static WebAdminManager _manager = null;
        private EventLog _log;

        /// <summary>
        /// Constructor implmentation
        /// </summary>
        public WebAdminService(IDependency dep)
        {
            _log = dep.GetEventLog();
            _manager = new WebAdminManager(dep);
        }

        #region ACLs


        /// <summary>
        /// GetSIDsInformations method implementation
        /// </summary>
        public SIDsParametersRecord GetSIDsInformations(Dictionary<string, bool> servers)
        {
            try
            {
                return _manager.GetSIDsInformations(servers);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service GetSIDsInformations method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                SIDsParametersRecord rec = new SIDsParametersRecord();
                rec.Loaded = false;
                return rec;
            }
        }

        /// <summary>
        /// RequestSIDsInformations method implementation
        /// </summary>
        public SIDsParametersRecord RequestSIDsInformations()
        {
            try
            {
                return _manager.RequestSIDsInformations();
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service GetSIDsInformations method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                SIDsParametersRecord rec = new SIDsParametersRecord();
                rec.Loaded = false;
                return rec;
            }
        }
        /// <summary>
        /// PushSIDsInformations method implmentation
        /// </summary>
        public void PushSIDsInformations(SIDsParametersRecord rec)
        {
            try
            {
                _manager.PushSIDsInformations(rec);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service GetSIDsInformations method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// UpdateDirectoriesACL method implementation
        /// </summary>
        public void UpdateDirectoriesACL(string path)
        {
            try
            {
                _manager.UpdateDirectoriesACL(path);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service UpdateDirectoriesACL ACL method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                return;
            }
        }

        /// <summary>
        /// UpdateCertificatesACL method implementation
        /// </summary>
        public bool UpdateCertificatesACL(KeyMgtOptions options)
        {
            try
            {
                return _manager.UpdateCertificatesACL(options);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service UpdateCertificatesACL ACL method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                return false;
            }
        }

        /// <summary>
        /// CleanOrphanedPrivateKeys method implementation
        /// </summary>
        public int CleanOrphanedPrivateKeys(byte option, int delay)
        {
            try
            {
                return _manager.CleanOrphanedPrivateKeys(option, delay);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service CleanOrphanedPrivateKeys method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                return 0;
            }
        }
        #endregion

        #region Certificates
        /// <summary>
        /// CertificateExists method implementation
        /// </summary>
        public bool CertificateExists(string thumbprint, byte location)
        {
            try
            {
                return _manager.CertificateExists(thumbprint, location);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service CertificateExists method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                return false;
            }
        }

        /// <summary>
        /// CreateRSACertificate method implementation
        /// </summary>
        public string CreateRSACertificate(Dictionary<string, bool> servers, string subject, int years)
        {
            try
            {
                return _manager.CreateRSACertificate(servers, subject, years);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service CreateRSACertificate method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                return null;
            }
        }

        /// <summary>
        /// CreateRSACertificateForSQLEncryption method implementation
        /// </summary>
        public string CreateRSACertificateForSQLEncryption(Dictionary<string, bool> servers, string subject, int years)
        {
            try
            {
                return _manager.CreateRSACertificateForSQLEncryption(servers, subject, years);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service CreateRSACertificateForSQLEncryption method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                return null;
            }
        }

        /// <summary>
        /// CreateADFSCertificate method implementation
        /// </summary>
        public bool CreateADFSCertificate(Dictionary<string, bool> servers, string subject, bool issigning, int years)
        {
            try
            {
                return (_manager.CreateADFSCertificate(servers, subject, issigning, years)!=null);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service CreateADFSCertificate method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                return false;
            }
        }

        /// <summary>
        /// PushCertificate method implementation
        /// </summary>
        public void PushCertificate(string cert)
        {
            try
            {
                _manager.PushCertificate(cert);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service PushCertificate method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                return;
            }
        }
        #endregion

        #region Firewall
        /// <summary>
        /// AddFirewallRules method implmentation
        /// </summary>
        public void AddFirewallRules(string computers)
        {
            try
            {
                _manager.AddFirewallRules(computers);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service AddFirewallRules method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// RemoveFirewallRules method implementation
        /// </summary>
        public void RemoveFirewallRules()
        {
            try
            {
                _manager.RemoveFirewallRules();
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service RemoveFirewallRules method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }
        #endregion

        #region Mail Templates
        /// <summary>
        /// ExportMailTemplates method implementation
        /// </summary>
        public bool ExportMailTemplates(Dictionary<string, bool> servers, byte[] config, int lcid, Dictionary<string, string> templates, bool dispatch = true)
        {
            try
            {
                return _manager.ExportMailTemplates(servers, config, lcid, templates, dispatch);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service ExportMailTemplates method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }
        #endregion

        #region Notifications
        /// <summary>
        /// BroadcastNotification method implementation
        /// </summary>
        public void BroadcastNotification(Dictionary<string, bool> servers, byte[] config, NotificationsKind kind, string message, bool local = true, bool dispatch = true, bool mustwrite = false)
        {
            try
            {
                _manager.BroadcastNotification(servers, config, kind, message, local, dispatch, mustwrite);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service BroadcastNotification method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }
        #endregion

        #region Computer Informations
        /// <summary>
        /// GetComputerInformations method implementation
        /// </summary>
        public ADFSServerHost GetComputerInformations(string servername, bool dispatch = true)
        {
            try
            {
                return _manager.GetComputerInformations(servername, dispatch);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service GetComputerInformations method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// GetComputerInformations method implementation
        /// </summary>
        public Dictionary<string, ADFSServerHost> GetAllComputerInformations(Dictionary<string, bool> servers)
        {
            try
            {
                return _manager.GetAllComputerInformations(servers);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service GetAllComputerInformations method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }            
        }
        #endregion

        #region Nodes
        /// <summary>
        /// GetNodeInformations method implementation
        /// </summary>
        public ADFSNodeInformation GetNodeInformations(RegistryVersion reg, string servername = "localhost", bool dispatch = true)
        {
            try
            {
                return _manager.GetNodeInformations(reg, servername, dispatch);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service GetNodeInformations method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// GetNodeType method implementation
        /// </summary>
        public string GetNodeType(string servername = "localhost", bool dispatch = true)
        {
            try
            {
                return _manager.GetNodeType(servername, dispatch);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service GetNodeType method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }
        #endregion
    }

    /// <summary>
    /// NTService class
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class NTService : INTService
    {
        private static NTServiceManager _manager = null;
        private EventLog _log;

        /// <summary>
        /// Constructor implmentation
        /// </summary>
        public NTService(IDependency dep)
        {
            _log = dep.GetEventLog();
            _manager = new NTServiceManager(dep);
        }

        /// <summary>
        /// Continue method implementation
        /// </summary>
        public bool Continue(string name, string machinename)
        {
            try
            { 
                if (IsLocalServer(machinename))
                    return _manager.Continue(name);
                else
                    return TransfertToRemoteServer(name, machinename, 3);
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("error on NTService Continue method : {0}.", ex.Message), EventLogEntryType.Error, 1012);
                throw ex;
            }
        }

        /// <summary>
        /// Pause method implementation
        /// </summary>
        public bool Pause(string name, string machinename)
        {
            try
            {
                if (IsLocalServer(machinename))
                    return _manager.Pause(name);
                else
                    return TransfertToRemoteServer(name, machinename, 4);
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("error on NTService Pause method : {0}.", ex.Message), EventLogEntryType.Error, 1012);
                throw ex;
            }
        }

        /// <summary>
        /// Start method implementation
        /// </summary>
        public bool Start(string name, string machinename)
        {
            try
            {
                if (IsLocalServer(machinename))
                    return _manager.Start(name);
                else
                    return TransfertToRemoteServer(name, machinename, 1);
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("error on NTService Start method : {0}.", ex.Message), EventLogEntryType.Error, 1012);
                throw ex;
            }
        }

        /// <summary>
        /// Stop method implementation
        /// </summary>
        public bool Stop(string name, string machinename)
        {
            try
            {
                if (IsLocalServer(machinename))
                    return _manager.Stop(name);
                else
                    return TransfertToRemoteServer(name, machinename, 2);
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("error on NTService Stop method : {0}.", ex.Message), EventLogEntryType.Error, 1012);
                throw ex;
            }
        }

        /// <summary>
        /// IsRunning method implementation
        /// </summary>
        public bool IsRunning(string name, string machinename)
        {
            try
            {
                if (IsLocalServer(machinename))
                    return _manager.IsRunning(name);
                else
                    return TransfertToRemoteServer(name, machinename, 5);
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("error on NTService IsRunning method : {0}.", ex.Message), EventLogEntryType.Error, 1012);
                throw ex;
            }
        }

        /// <summary>
        /// Exists method implementation
        /// </summary>
        public bool Exists(string name, string machinename)
        {
            try
            {
                if (IsLocalServer(machinename))
                    return _manager.Exists(name);
                else
                    return TransfertToRemoteServer(name, machinename, 6);
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("error on NTService Exists method : {0}.", ex.Message), EventLogEntryType.Error, 1012);
                throw ex;
            }
        }

        /// <summary>
        /// TransfertToRemoteServer method implementation
        /// </summary>
        private bool TransfertToRemoteServer(string name, string machinename, int kind)
        {
            NTServiceClient manager = new NTServiceClient();
            manager.Initialize(machinename);
            INTService client = manager.Open();
            try
            {
                string targetname = "localhost";
                switch (kind)
                {
                    case 1:
                        return client.Start(name, targetname);
                    case 2:
                        return client.Stop(name, targetname);
                    case 3:
                        return client.Continue(name, targetname);
                    case 4:
                        return client.Pause(name, targetname);
                    case 5:
                        return client.IsRunning(name, targetname);
                    case 6:
                        return client.Exists(name, targetname);
                    default:
                        return client.Start(name, targetname);
                }
            }
            catch (Exception ex)
            {
                manager.UnInitialize();
                throw ex;
            }
            finally
            {
                manager.Close(client);
            }
        }

        /// <summary>
        /// IsLocalServer method implementation
        /// </summary>
        private bool IsLocalServer(string machinename)
        {
            if (machinename.ToLower().Equals("localhost"))
                return true;
            return (Dns.GetHostEntry("LocalHost").HostName.ToLower().Equals(machinename.ToLower()));
        }
    }
}
