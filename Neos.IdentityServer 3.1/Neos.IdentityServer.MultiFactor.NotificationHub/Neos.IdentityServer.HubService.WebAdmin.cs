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
using Neos.IdentityServer.MultiFactor.Common;
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor
{
    #region WebAdminManager
    /// <summary>
    /// WebThemeManager class implementation
    /// </summary>
    public class WebAdminManager
    {
        private EventLog _log;

        /// <summary>
        /// Constructor implmentation
        /// </summary>
        public WebAdminManager(IDependency dep)
        {
            _log = dep.GetEventLog();
        }

        #region SIDs and Key Mgmt
        /// <summary>
        /// GetSIDsInformations method implmentation
        /// </summary>
        public SIDsParametersRecord GetSIDsInformations(Dictionary<string, bool> servers)
        {
            try
            {
                string fqdn = Dns.GetHostEntry("localhost").HostName;
                bool isprimary = (from server in servers where (server.Key.ToLower() == fqdn.ToLower()) && (server.Value == true) select server.Value).ToList().FirstOrDefault();
                if (isprimary)
                    return GetLocalSIDsInformations(servers);
                else
                    return GetRemoteSIDsInformations(servers);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service GetSIDsInformations method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// GetLocalSIDsInformations method implmentation
        /// </summary>
        private SIDsParametersRecord GetLocalSIDsInformations(Dictionary<string, bool> servers)
        {
            SIDsParametersRecord retvalue = null;
            try
            {
                retvalue = SIDs.Initialize();
                if (retvalue != null)
                {
                    string fqdn = Dns.GetHostEntry("localhost").HostName;
                    foreach (var srv in servers)
                    {
                        if (srv.Key.ToLower().Equals(fqdn.ToLower()))
                           continue;

                        WebAdminClient manager = new WebAdminClient();
                        manager.Initialize(srv.Key.ToLower());
                        try
                        {
                            IWebAdminServices client = manager.Open();
                            try
                            {
                                client.PushSIDsInformations(retvalue);
                            }
                            finally
                            {
                                manager.Close(client);
                            }
                        }
                        catch (Exception e)
                        {
                            _log.WriteEntry(string.Format("Error on WebAdminService Service GetLocalSIDsInformations method : {0} / {1}.", srv.Key.ToLower(), e.Message), EventLogEntryType.Error, 2010);
                        }
                        finally
                        {
                            manager.UnInitialize();
                        }
                    } 
                } 
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service GetLocalSIDsInformations method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
            return retvalue;
        }

        /// <summary>
        /// GetRemoteSIDsInformations method implmentation
        /// </summary>
        private SIDsParametersRecord GetRemoteSIDsInformations(Dictionary<string, bool> servers)
        {
            SIDsParametersRecord retvalue = null;
            try
            {
                retvalue = SIDs.GetSIDs();
                if (retvalue == null)
                {
                    string fqdn = Dns.GetHostEntry("localhost").HostName;
                    foreach (var srv in servers)
                    {
                        if (srv.Key.ToLower().Equals(fqdn.ToLower()) || (!srv.Value))
                            continue;
                        WebAdminClient manager = new WebAdminClient();
                        manager.Initialize(srv.Key);
                        try
                        {
                            IWebAdminServices client = manager.Open();
                            try
                            {
                                retvalue = client.RequestSIDsInformations();
                                SIDs.Assign(retvalue);
                                break; // Break on first primary server;
                            }
                            finally
                            {
                                manager.Close(client);
                            }
                        }
                        catch (Exception e)
                        {
                            _log.WriteEntry(string.Format("Error on WebAdminService Service GetRemoteSIDsInformations method : {0} / {1}.", srv, e.Message), EventLogEntryType.Error, 2010);
                        }
                        finally
                        {
                            manager.UnInitialize();
                        }
                    }
                } 
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service GetLocalSIDsInformations method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
            return retvalue;
        }

        /// <summary>
        /// RequestSIDsInformations method implementation
        /// </summary>
        public SIDsParametersRecord RequestSIDsInformations()
        {
            SIDsParametersRecord retvalue = null;
            try
            {
                retvalue = SIDs.Initialize();
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service RequestSIDsInformations method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
            return retvalue;
        }

        /// <summary>
        /// PushSIDsInformations method implementation
        /// </summary>
        public void PushSIDsInformations(SIDsParametersRecord rec)
        {
            SIDs.Assign(rec); 
        }

        /// <summary>
        /// ApplyDirectoriesACL method implmentation
        /// </summary>
        public void UpdateDirectoriesACL(string path)
        {
            try
            {
                SIDs.InternalUpdateDirectoryACLs(path);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service ApplyDirectoriesACL ACL method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// ApplyCertificatesACL method implementation
        /// </summary>
        public bool UpdateCertificatesACL(KeyMgtOptions options)
        {
            try
            {
                return SIDs.internalUpdateCertificatesACLs(options);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service ApplyCertificatesACL ACL method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// CleanOrphanedPrivateKeys method implmentation
        /// </summary>
        internal int CleanOrphanedPrivateKeys(byte option, int delay)
        {
            try
            {
                Certs.CleanOrphanedPrivateKeysRegistry(option, delay);
                if (option == 0x00)
                    return Certs.CleanOrphanedPrivateKeys();
                else
                    return 0;
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service CleanOrphanedPrivateKeys method : {0}.", ex.Message), EventLogEntryType.Error, 2010);
                throw ex;
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
                MFAFirewall.AddFirewallRules(computers);
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service AddFirewallRules method : {0}.", ex.Message), EventLogEntryType.Error, 2010);
                throw ex;
            }
        }

        /// <summary>
        /// RemoveFirewallRules method implementation
        /// </summary>
        public void RemoveFirewallRules()
        {
            try
            {
                MFAFirewall.RemoveFirewallRules();
            }
            catch (Exception ex)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service RemoveFirewallRules method : {0}.", ex.Message), EventLogEntryType.Error, 2010);
                throw ex;
            }
        }
        #endregion

        #region Mail Templates
        /// <summary>
        /// ExportMailTemplates method implementation
        /// </summary>
        internal bool ExportMailTemplates(Dictionary<string, bool> servers, byte[] config, int lcid, Dictionary<string, string> templates, bool dispatch = true)
        {
            char sep = Path.DirectorySeparatorChar;
            string htmlpath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "MailTemplates" + sep + lcid.ToString();
            try
            {
                if (!Directory.Exists(htmlpath))
                    Directory.CreateDirectory(htmlpath);

                foreach (var item in templates)
                {
                    if (!File.Exists(htmlpath + sep + item.Key))
                        File.WriteAllText(htmlpath + sep + item.Key, item.Value, Encoding.UTF8);
                }

                if (dispatch)
                {
                    string fqdn = Dns.GetHostEntry("localhost").HostName;
                    List<string> servernames = (from server in servers
                                                where (server.Key.ToLower() != fqdn.ToLower())
                                                select server.Key.ToLower()).ToList<string>();

                    foreach (string srv in servernames)
                    {
                        WebAdminClient manager = new WebAdminClient();
                        manager.Initialize(srv);
                        try
                        {
                            IWebAdminServices client = manager.Open();
                            try
                            {
                                client.ExportMailTemplates(servers, config, lcid, templates, false);
                            }
                            catch (Exception e)
                            {
                                _log.WriteEntry(string.Format("Error on WebAdminService Service ExportMailTemplates method : {0} - {1} / {2}.", srv, lcid, e.Message), EventLogEntryType.Error, 2010);
                            }
                            finally
                            {
                                manager.Close(client);
                            }
                        }
                        catch (Exception e)
                        {
                            _log.WriteEntry(string.Format("Error on WebAdminService Service ExportMailTemplates method : {0} / {1}.", srv, e.Message), EventLogEntryType.Error, 2010);
                        }
                        finally
                        {
                            manager.UnInitialize();
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service ExportMailTemplates method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                return false;
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
                if (local)
                {
                    switch (kind)
                    {
                        case NotificationsKind.ConfigurationReload:
                            if (!File.Exists(CFGUtilities.ConfigCacheFile))
                                mustwrite = true;
                            if (!File.Exists(SystemUtilities.SystemCacheFile))
                                mustwrite = true;
                            if (mustwrite)
                                WriteConfigurationToCache(config);
                            PushNotification(NotificationsKind.ConfigurationReload, message, null); 
                            break;
                        case NotificationsKind.ConfigurationCreated:
                            WriteConfigurationToCache(config);
                            PushNotification(NotificationsKind.ConfigurationCreated, message, null);
                            break;
                        case NotificationsKind.ConfigurationDeleted:
                            DeleteConfigurationFromCache();
                            PushNotification(NotificationsKind.ConfigurationDeleted, message, null);
                            break;
                        case NotificationsKind.ServiceStatusInError:
                            PushNotification(NotificationsKind.ServiceStatusInError, message, "MGT");
                            break;
                        case NotificationsKind.ServiceStatusPending:
                            PushNotification(NotificationsKind.ServiceStatusPending, message, "MGT");
                            break;
                        case NotificationsKind.ServiceStatusRunning:
                            PushNotification(NotificationsKind.ServiceStatusRunning, message, "MGT");
                            break;
                        case NotificationsKind.ServiceStatusStopped:
                            PushNotification(NotificationsKind.ServiceStatusStopped, message, "MGT");
                            break;
                    }
                }
                if (dispatch)
                {
                    string fqdn = Dns.GetHostEntry("localhost").HostName;
                    List<string> servernames = (from server in servers
                                                where (server.Key.ToLower() != fqdn.ToLower())
                                                select server.Key.ToLower()).ToList<string>();

                    foreach (string srv in servernames)
                    {
                        WebAdminClient manager = new WebAdminClient();
                        manager.Initialize(srv);
                        try
                        {                            
                            IWebAdminServices client = manager.Open();
                            try
                            {
                                client.BroadcastNotification(servers, config, kind, message, true, false, true);
                            }
                            finally
                            {
                                manager.Close(client);
                            }
                        }
                        catch (Exception e)
                        {
                            _log.WriteEntry(string.Format("Error on WebAdminService Service BroadcastNotification method : {0} / {1}.", srv, e.Message), EventLogEntryType.Error, 2010);
                        }
                        finally
                        {
                            manager.UnInitialize();
                        }                       
                    }
                }                
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service BroadcastNotification method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// WriteConfigurationToCache method implementation
        /// </summary>
        internal void WriteConfigurationToCache(byte[] config)
        {
            try
            {
                // Write Crypted Config only
                using (FileStream fs = new FileStream(CFGUtilities.ConfigCacheFile, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Write(config, 0, config.Length);
                    fs.Close();
                }
                if (SIDs.Loaded)
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(SIDsParametersRecord));
                    MemoryStream stm = new MemoryStream();
                    byte[] bytes = null;
                    using (StreamReader reader = new StreamReader(stm))
                    {
                        serializer.WriteObject(stm, SIDs.GetSIDs());
                        stm.Position = 0;
                        using (AESSystemEncryption aes = new AESSystemEncryption())
                        {
                            bytes = aes.Encrypt(stm.ToArray());
                        }
                    }
                    using (FileStream fs = new FileStream(SystemUtilities.SystemCacheFile, FileMode.Create, FileAccess.ReadWrite))
                    {
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Close();
                    }
                }
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service WriteConfigurationToCache method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// DeleteConfigurationFromCache method implementation
        /// </summary>
        internal void DeleteConfigurationFromCache()
        {
            try
            {
                if (File.Exists(CFGUtilities.ConfigCacheFile))
                    File.Delete(CFGUtilities.ConfigCacheFile);
                if (File.Exists(SystemUtilities.SystemCacheFile))
                    File.Delete(SystemUtilities.SystemCacheFile);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service DeleteConfigurationFromCache method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }

        }

        /// <summary>
        /// PushNotification method implementation
        /// </summary>
        internal void PushNotification(NotificationsKind kind, string message, string appname)
        {
            using (MailSlotClient mailslot = new MailSlotClient(appname))
            {
                if (string.IsNullOrEmpty(message))
                    mailslot.Text = Environment.MachineName;
                else
                    mailslot.Text = message;
                mailslot.SendNotification(kind);
            }
        }
        #endregion

        #region Computers Information
        /// <summary>
        /// GetComputerInformations method implementation
        /// </summary>
        internal ADFSServerHost GetComputerInformations(string servername)
        {
            
            string nodetype = GetLocalNodeType();
            if (nodetype.ToLower().Equals("primarycomputer"))
            {
                string fqdn = Dns.GetHostEntry("localhost").HostName;
                string requested = Dns.GetHostEntry(servername).HostName;
                bool local = fqdn.ToLower().Equals(requested.ToLower());
                try
                {
                    RegistryVersion localreg = new RegistryVersion();
                    List<ADFSNodeInformation> nodes = GetNodesInformations(localreg);
                    foreach (ADFSNodeInformation node in nodes)
                    {
                        if (node.FQDN.ToLower().Equals(fqdn.ToLower()) && local)  // Local
                        {
                            return new ADFSServerHost()
                            {
                                FQDN = node.FQDN.ToLower(),
                                BehaviorLevel = node.BehaviorLevel,
                                HeartbeatTmeStamp = node.HeartbeatTmeStamp,
                                NodeType = node.NodeType,
                                CurrentVersion = localreg.CurrentVersion,
                                CurrentBuild = localreg.CurrentBuild,
                                InstallationType = localreg.InstallationType,
                                ProductName = localreg.ProductName,
                                CurrentMajorVersionNumber = localreg.CurrentMajorVersionNumber,
                                CurrentMinorVersionNumber = localreg.CurrentMinorVersionNumber
                            };
                        }
                        else if (node.FQDN.ToLower().Equals(requested.ToLower())) // Found 2016 or 2019
                        {
                            RegistryVersion remotereg = GetRemoteRegistryInformations(requested);
                            if (remotereg != null)
                            {
                                return new ADFSServerHost()
                                {
                                    FQDN = node.FQDN.ToLower(),
                                    BehaviorLevel = node.BehaviorLevel,
                                    HeartbeatTmeStamp = node.HeartbeatTmeStamp,
                                    NodeType = node.NodeType,
                                    CurrentVersion = remotereg.CurrentVersion,
                                    CurrentBuild = remotereg.CurrentBuild,
                                    InstallationType = remotereg.InstallationType,
                                    ProductName = remotereg.ProductName,
                                    CurrentMajorVersionNumber = remotereg.CurrentMajorVersionNumber,
                                    CurrentMinorVersionNumber = remotereg.CurrentMinorVersionNumber
                                };
                            }
                        }
                    }
                    if (localreg.IsWindows2012R2)
                    {
                        RegistryVersion remotereg2 = GetRemoteRegistryInformations(requested);
                        if (remotereg2 != null)
                        {
                            return new ADFSServerHost()
                            {
                                FQDN = Dns.GetHostEntry(servername).HostName.ToLower(),
                                NodeType = "SecondaryComputer",
                                BehaviorLevel = 1,
                                HeartbeatTmeStamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0, DateTimeKind.Local),
                                CurrentVersion = remotereg2.CurrentVersion,
                                CurrentBuild = remotereg2.CurrentBuild,
                                InstallationType = remotereg2.InstallationType,
                                ProductName = remotereg2.ProductName,
                                CurrentMajorVersionNumber = remotereg2.CurrentMajorVersionNumber,
                                CurrentMinorVersionNumber = remotereg2.CurrentMinorVersionNumber
                            };
                        }
                    }
                    return null;
                }
                catch (Exception e)
                {
                    _log.WriteEntry(string.Format("Error on WebAdminService Service GetComputerInformations method : {0} / {1}.", servername, e.Message), EventLogEntryType.Error, 2010);
                    throw e;
                }
            }
            else
                return null;
        }

        /// <summary>
        /// GetAllComputerInformations method implementation
        /// </summary>
        internal Dictionary<string, ADFSServerHost> GetAllComputersInformations()
        {
            Dictionary<string, ADFSServerHost> dict = null;
            string nodetype = GetLocalNodeType();
            if (nodetype.ToLower().Equals("primarycomputer"))
            {
                try
                {
                    string fqdn = Dns.GetHostEntry("localhost").HostName;
                    dict = new Dictionary<string, ADFSServerHost>();
                    RegistryVersion localreg = new RegistryVersion();
                    List<ADFSNodeInformation> nodes = GetNodesInformations(localreg);
                    foreach (ADFSNodeInformation node in nodes)
                    {
                        if (node.FQDN.ToLower().Equals(fqdn.ToLower()))
                        { 
                            dict.Add(node.FQDN.ToLower(), new ADFSServerHost()
                            {
                                FQDN = node.FQDN.ToLower(),
                                BehaviorLevel = node.BehaviorLevel,
                                HeartbeatTmeStamp = node.HeartbeatTmeStamp,
                                NodeType = node.NodeType,
                                CurrentVersion = localreg.CurrentVersion,
                                CurrentBuild = localreg.CurrentBuild,
                                InstallationType = localreg.InstallationType,
                                ProductName = localreg.ProductName,
                                CurrentMajorVersionNumber = localreg.CurrentMajorVersionNumber,
                                CurrentMinorVersionNumber = localreg.CurrentMinorVersionNumber
                            });
                        }
                        else
                        {
                            RegistryVersion remotereg = GetRemoteRegistryInformations(node.FQDN.ToLower());
                            if (remotereg != null)
                            {
                                dict.Add(node.FQDN.ToLower(), new ADFSServerHost()
                                {
                                    FQDN = node.FQDN.ToLower(),
                                    BehaviorLevel = node.BehaviorLevel,
                                    HeartbeatTmeStamp = node.HeartbeatTmeStamp,
                                    NodeType = node.NodeType,
                                    CurrentVersion = remotereg.CurrentVersion,
                                    CurrentBuild = remotereg.CurrentBuild,
                                    InstallationType = remotereg.InstallationType,
                                    ProductName = remotereg.ProductName,
                                    CurrentMajorVersionNumber = remotereg.CurrentMajorVersionNumber,
                                    CurrentMinorVersionNumber = remotereg.CurrentMinorVersionNumber
                                });
                            }
                        }
                    }
                    if (dict.Count == 0)
                        return null;
                }
                catch (Exception e)
                {
                    _log.WriteEntry(string.Format("Error on WebAdminService Service GetAllComputersInformations method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                    throw e;
                }
            }
            return dict; 
        }

        /// <summary>
        /// GetRemoteRegistryInformations method implementations
        /// </summary>
        private RegistryVersion GetRemoteRegistryInformations(string fqdn)
        {
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize(fqdn);
            try
            {
                IWebAdminServices client = manager.Open();
                try
                {
                    return client.GetRegistryInformations();
                }
                finally
                {
                    manager.Close(client);
                }
            }
            catch (Exception e)
            {
                Log.WriteEntry(string.Format("Error on WebAdminService Service GetRemoteRegistryInformations method : {0} / {1}.", fqdn, e.Message), EventLogEntryType.Error, 2010);
            }
            finally
            {
                manager.UnInitialize();
            }
            return null;
        }
        #endregion

        #region ADFS Node Information
        /// <summary>
        /// GetLocalNodeInformations method implementation
        /// </summary>
        private List<ADFSNodeInformation> GetNodesInformations(RegistryVersion reg)
        {
            List<ADFSNodeInformation> _list = new List<ADFSNodeInformation>();
            if (reg.IsWindows2012R2)
            {
                _list.Add(new ADFSNodeInformation()
                {
                    FQDN = Dns.GetHostEntry("localhost").HostName,
                    NodeType = "PrimaryComputer",
                    BehaviorLevel = 1,
                    HeartbeatTmeStamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0, DateTimeKind.Local)
                });
            }
            else
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
                    Command exportcmd = new Command("(Get-AdfsFarmInformation).FarmNodes", true);

                    pipeline.Commands.Add(exportcmd);

                    Collection<PSObject> PSOutput = pipeline.Invoke();
                    foreach (var result in PSOutput)
                    {
                        _list.Add(new ADFSNodeInformation
                        {
                            FQDN = result.Members["FQDN"].Value.ToString(),
                            NodeType = result.Members["NodeType"].Value.ToString(),
                            BehaviorLevel = Convert.ToInt32(result.Members["BehaviorLevel"].Value),
                            HeartbeatTmeStamp = Convert.ToDateTime(result.Members["HeartbeatTimeStamp"].Value),
                        });
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
            return _list;
        }

        /// <summary>
        /// GetLocalNodeType method implementation
        /// </summary>
        private string GetLocalNodeType()
        {
            string nodetype = string.Empty;
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
            return nodetype;
        }
        #endregion

        #region Certs / ACLs       
        /// <summary>
        /// CertificateExists method implementation
        /// </summary>
        internal bool CertificateExists(string thumbprint, byte location)
        {
            try
            {
                return Certs.CertificateExists(thumbprint, (StoreLocation)location);
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service CertificateExists method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
        }

        /// <summary>
        /// CreateRSACertificate method implementation
        /// </summary>
        internal string CreateRSACertificate(Dictionary<string, bool> servers, string subject, int years)
        {
            SIDs.Initialize();
            string thumbprint = null;
            try
            {
                string strcert = string.Empty;
                X509Certificate2 cert = null;
                try
                {
                    cert = Certs.CreateRSACertificate(subject, years, out strcert);
                    if (cert == null)
                        return null;
                    else
                        thumbprint = cert.Thumbprint;
                }
                finally
                {
                    cert.Reset();
                }

                SIDs.internalUpdateCertificatesACLs(KeyMgtOptions.MFACerts);

                string fqdn = Dns.GetHostEntry("localhost").HostName;
                List<string> servernames = (from server in servers
                                            where (server.Key.ToLower() != fqdn.ToLower())
                                            select server.Key.ToLower()).ToList<string>();

                foreach (string srv in servernames)
                {
                    WebAdminClient manager = new WebAdminClient();
                    manager.Initialize(srv);
                    try
                    {
                        IWebAdminServices client = manager.Open();
                        try
                        {
                            client.PushCertificate(strcert);
                        }
                        finally
                        {
                            manager.Close(client);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.WriteEntry(string.Format("Error on WebAdminService Service CreateRSACertificate method : {0} / {1}.", srv, e.Message), EventLogEntryType.Error, 2010);
                    }
                    finally
                    {
                        manager.UnInitialize();
                    }
                }
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service CreateRSACertificate method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
            return thumbprint;
        }

        /// <summary>
        /// CreateRSACertificateForSQLEncryption method implementation
        /// </summary>
        internal string CreateRSACertificateForSQLEncryption(Dictionary<string, bool> servers, string subject, int years)
        {
            SIDs.Initialize();

            string thumbprint = null;
            try
            { 
                string strcert = string.Empty;
                X509Certificate2 cert = null;
                try
                {
                    cert = Certs.CreateRSACertificateForSQLEncryption(subject, years, out strcert);
                    if (cert == null)
                        return null;
                    else
                        thumbprint = cert.Thumbprint;
                }
                finally
                {
                    cert.Reset();
                }

                SIDs.internalUpdateCertificatesACLs(KeyMgtOptions.MFACerts);

                string fqdn = Dns.GetHostEntry("localhost").HostName;
                List<string> servernames = (from server in servers
                                            where (server.Key.ToLower() != fqdn.ToLower())
                                            select server.Key.ToLower()).ToList<string>();
                foreach (string srv in servernames)
                {
                    WebAdminClient manager = new WebAdminClient();
                    manager.Initialize(srv);
                    try
                    {
                        IWebAdminServices client = manager.Open();
                        try
                        {
                            client.PushCertificate(strcert);
                        }
                        finally
                        {
                            manager.Close(client);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.WriteEntry(string.Format("Error on WebAdminService Service CreateRSACertificateForSQLEncryption method : {0} / {1}.", srv, e.Message), EventLogEntryType.Error, 2010);
                    }
                    finally
                    {
                        manager.UnInitialize();
                    }
                }
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service CreateRSACertificateForSQLEncryption method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
            return thumbprint;
        }

        /// <summary>
        /// CreateADFSCertificate method implementation
        /// </summary>
        internal string CreateADFSCertificate(Dictionary<string, bool> servers, string subject, bool issigning, int years)
        {
            SIDs.Initialize();

            string thumbprint = null;
            try
            { 
                string strcert = string.Empty;
                X509Certificate2 cert = null;
                try
                {
                    cert = Certs.CreateADFSCertificate(subject, issigning, years, out strcert);
                    if (cert == null)
                        return null;
                    else
                        thumbprint = cert.Thumbprint;
                }
                finally
                {
                    cert.Reset();
                }

                SIDs.internalUpdateCertificatesACLs(KeyMgtOptions.ADFSCerts);

                string fqdn = Dns.GetHostEntry("localhost").HostName;
                List<string> servernames = (from server in servers
                                            where (server.Key.ToLower() != fqdn.ToLower())
                                            select server.Key.ToLower()).ToList<string>();
                foreach (string srv in servernames)
                {
                    WebAdminClient manager = new WebAdminClient();
                    manager.Initialize(srv);
                    try
                    {
                        IWebAdminServices client = manager.Open();
                        try
                        {
                            client.PushCertificate(strcert);
                        }
                        finally
                        {
                            manager.Close(client);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.WriteEntry(string.Format("Error on WebAdminService Service CreateADFSCertificate method : {0} / {1}.", srv, e.Message), EventLogEntryType.Error, 2010);
                    }
                    finally
                    {
                        manager.UnInitialize();
                    }
                }
            }
            catch (Exception e)
            {
                _log.WriteEntry(string.Format("Error on WebAdminService Service CreateADFSCertificate method : {0}.", e.Message), EventLogEntryType.Error, 2010);
                throw e;
            }
            return thumbprint;
        }

        /// <summary>
        /// PushCertificate method implmentation
        /// </summary>
        internal void PushCertificate(string cert)
        {
            X509Certificate2 x509 = new X509Certificate2(Convert.FromBase64String(cert), "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            try
            {
                if (x509 == null)
                    return;
                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.MaxAllowed);
                store.Add(x509);
                store.Close();
            }
            finally
            {
                Certs.CleanSelfSignedCertificate(x509, StoreLocation.LocalMachine);
                x509.Reset();
                SIDs.internalUpdateCertificatesACLs(KeyMgtOptions.AllCerts);
            }
        }
        #endregion   
    }
    #endregion
}
