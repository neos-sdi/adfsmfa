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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceProcess;
using Neos.IdentityServer.MultiFactor.Administration;

namespace Neos.IdentityServer.MultiFactor.NotificationHub
{
    public partial class MFANOTIFHUB : ServiceBase
    {
        private List<string> _adfsservers = new List<string>();
        private MailSlotServerManager _mailslotsmgr = new MailSlotServerManager();
        private MailSlotServer _mailslotmfa = new MailSlotServer("NOT"); // And Broadcast
        private PipeServer _pipeserver = new PipeServer();
        private ReplayManager _replaymgr = new ReplayManager();

        #region Service override methods
        /// <summary>
        /// Constructor Override
        /// </summary>
        public MFANOTIFHUB()
        {
            Trace.TraceInformation("MFANOTIFHUB:InitializeComponent");
            InitializeComponent();
            Trace.TraceInformation("MFANOTIFHUB:EventLog Start");
            ((ISupportInitialize)this.EventLog).BeginInit();
            if (!EventLog.SourceExists("ADFS MFA Notification Hub"))
                EventLog.CreateEventSource("ADFS MFA Notification Hub", "Application");
            if (!EventLog.SourceExists("ADFS MFA Service"))
                EventLog.CreateEventSource("ADFS MFA Service", "Application");
            ((ISupportInitialize)this.EventLog).EndInit();
            Trace.TraceInformation("MFANOTIFHUB:EventLog End Init");
            this.EventLog.Source = "ADFS MFA Notification Hub";
            this.EventLog.Log = "Application";
            Trace.WriteLine("MFANOTIFHUB:Start MailSlot Manager " + _mailslotsmgr.ApplicationName);
            LogForSlots.LogEnabled = false;

            _pipeserver.Proofkey = XORUtilities.XORKey;
            _pipeserver.OnReloadConfiguration += DoOnReceiveConfiguration;
            _pipeserver.OnRequestServerConfiguration += DoOnReceiveServerConfiguration;
            _pipeserver.OnCheckForReplay += DoOnCheckForReplay;
            _pipeserver.OnCheckForRemoteReplay += DoOnCheckForRemoteReplay;
        }

        /// <summary>
        /// OnStart event
        /// </summary>
        protected override void OnStart(string[] args)
        {
            _mailslotsmgr.Start();

            _mailslotmfa.Start();
            _mailslotmfa.MailSlotMessageArrived += SyncMailSlotMessages;

            Trace.WriteLine("MFANOTIFHUB:Start ADFS Service");

            StartADFSService();
            try
            {
                _pipeserver.Start();
                _replaymgr.Start();
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Pipe Server Error on Start : {0}.", e.Message), EventLogEntryType.Error, 1000);
            }
            LogForSlots.LogEnabled = true;
            this.EventLog.WriteEntry("Service was started successfully.", EventLogEntryType.Information, 0);
        }

        /// <summary>
        /// OnStop event
        /// </summary>
        protected override void OnStop()
        {
            Trace.WriteLine("MFANOTIFHUB:Stop ADFS Service");
            LogForSlots.LogEnabled = false;

            StopADFSService();

            _mailslotsmgr.Stop();

            _mailslotmfa.Stop();
            _mailslotmfa.MailSlotMessageArrived -= SyncMailSlotMessages;
            try
            {
                _pipeserver.Stop();
                _replaymgr.Close();
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Pipe Server Error on Close : {0}.", e.Message), EventLogEntryType.Error, 1000);
            }
            this.EventLog.WriteEntry("Service was stopped successfully.", EventLogEntryType.Information, 1);
        }
        #endregion

        #region Properties
        /// <summary>
        /// ADFSServers List property
        /// </summary>
        private List<string> ADFSServers
        {
            get { return _adfsservers; }

        }
        #endregion

        #region Private Methods
        /// <summary>
        /// BuildADFSServersList method implementation
        /// </summary>
        private void BuildADFSServersList()
        {
            ADFSServers.Clear();
            ManagementService.EnsureService();
            foreach (ADFSServerHost host in ManagementService.ADFSManager.ADFSFarm.Servers)
            {
                _adfsservers.Add(host.MachineName);
            }
        }

        /// <summary>
        /// MFAMailSlotMessageArrived implementation
        /// </summary>
        private void SyncMailSlotMessages(MailSlotServer maislotserver, MailSlotMessage message)
        {
            if (message.Operation == (byte)NotificationsKind.ConfigurationReload) // Configuration Reload
                DoOnSendConfiguration();
            else if (message.Operation == (byte)NotificationsKind.ServiceServerInformation)
                DoOnRequestServerConfiguration(message.Text);
        }
        #endregion

        #region Add Server Configuration
        /// <summary>
        /// DoOnRequestServerConfiguration method implementation (Client)
        /// </summary>
        private void DoOnRequestServerConfiguration(string servername)
        {
            if (CFGUtilities.IsPrimaryComputer())
            {
                List<string> lst = new List<string>() { servername };
                PipeClient pipe = new PipeClient(XORUtilities.XORKey, lst);

                string req = Environment.MachineName;
                NamedPipeRegistryRecord reg = pipe.DoRequestServerConfiguration(req);
                UpgradeServersConfig(reg);
            }
            return;
        }

        /// <summary>
        /// UpgradeServersConfig method implementation (Client)
        /// </summary>
        private void UpgradeServersConfig(NamedPipeRegistryRecord reg)
        {
            NamedPipeRegistryRecord rec = FarmUtilities.InitServerNodeConfiguration(reg);

            MFAConfig cfg = CFGUtilities.ReadConfiguration(null);
            ADFSServerHost svr = null;
            if (cfg.Hosts.ADFSFarm.Servers.Exists(s => s.FQDN.ToLower().Equals(rec.FQDN.ToLower())))
            {
                svr = cfg.Hosts.ADFSFarm.Servers.Find(s => s.FQDN.ToLower().Equals(rec.FQDN.ToLower()));
                cfg.Hosts.ADFSFarm.Servers.Remove(svr);
            }
            svr = new ADFSServerHost();
            svr.FQDN = rec.FQDN;
            svr.CurrentVersion = rec.CurrentVersion;
            svr.CurrentBuild = rec.CurrentBuild;
            svr.CurrentMajorVersionNumber = rec.CurrentMajorVersionNumber;
            svr.CurrentMinorVersionNumber = rec.CurrentMinorVersionNumber;
            svr.InstallationType = rec.InstallationType;
            svr.ProductName = rec.ProductName;
            svr.NodeType = rec.NodeType;
            svr.BehaviorLevel = rec.BehaviorLevel;
            svr.HeartbeatTmeStamp = rec.HeartbeatTimestamp;
            cfg.Hosts.ADFSFarm.Servers.Add(svr);
            CFGUtilities.WriteConfiguration(null, cfg);

            using (MailSlotClient mailslot = new MailSlotClient())
            {
                mailslot.Text = Environment.MachineName;
                mailslot.SendNotification(NotificationsKind.ConfigurationReload);
            }
        }

        /// <summary>
        /// DoOnReceiveServerConfiguration method implementation (Server)
        /// </summary>
        private NamedPipeRegistryRecord DoOnReceiveServerConfiguration(string requestor)
        {
            RegistryVersion reg = new RegistryVersion();
            NamedPipeRegistryRecord rec = default(NamedPipeRegistryRecord);
            rec.FQDN = Dns.GetHostEntry("LocalHost").HostName;
            rec.CurrentVersion = reg.CurrentVersion;
            rec.CurrentBuild = reg.CurrentBuild;
            rec.CurrentMajorVersionNumber = reg.CurrentMajorVersionNumber;
            rec.CurrentMinorVersionNumber = reg.CurrentMinorVersionNumber;
            rec.InstallationType = reg.InstallationType;
            rec.ProductName = reg.ProductName;
            rec.IsWindows2012R2 = reg.IsWindows2012R2;
            rec.IsWindows2016 = reg.IsWindows2016;
            rec.IsWindows2019 = reg.IsWindows2019;
            rec.NodeType = FarmUtilities.InitServerNodeType();
            return rec;
        }

        #endregion

        #region Configuration Cache management
        /// <summary>
        /// DoOnSendConfiguration method implementation (Client)
        /// </summary>
        private void DoOnSendConfiguration()
        {
            if (CFGUtilities.IsPrimaryComputer())
            {
                if (_adfsservers.Count == 0)
                    BuildADFSServersList();

                PipeClient pipe = new PipeClient(XORUtilities.XORKey, ADFSServers, true);

                byte[] byt = ReadConfigurationForCache();
                string msg = Convert.ToBase64String(byt, 0, byt.Length);
                string req = Environment.MachineName;
                if (!pipe.DoReloadConfiguration(req, msg))
                {
                    this.EventLog.WriteEntry("Some Servers configuration where not updated ! Wait for WID synchro !", EventLogEntryType.Warning, 10000);
                }
            }
        }

        /// <summary>
        /// DoOnReceiveConfiguration method implementation (Server)
        /// </summary>
        private bool DoOnReceiveConfiguration(string requestor, string value)
        {
            try
            {
                WriteConfigurationToCache(value);
                using (MailSlotClient mailslot = new MailSlotClient())
                {
                    mailslot.Text = requestor;
                    mailslot.SendNotification(NotificationsKind.ConfigurationReload);
                }
            }
            catch (Exception ex)
            {
                this.EventLog.WriteEntry("Error when writing Confoguration cache ! "+ex.Message, EventLogEntryType.Error, 10000);
                return false;
            }
            return true;
        }

        /// <summary>
        /// ReadConfigurationForCache method implementation
        /// </summary>
        private byte[] ReadConfigurationForCache()
        {
            MFAConfig config = CFGUtilities.ReadConfiguration();
            XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
            MemoryStream stm = new MemoryStream();
            using (StreamReader reader = new StreamReader(stm))
            {
                xmlserializer.Serialize(stm, config);
                stm.Position = 0;
                return XORUtilities.XOREncryptOrDecrypt(stm.ToArray(), XORUtilities.XORKey);
            }
        }

        /// <summary>
        /// WriteConfigurationToCache method implementation
        /// </summary>
        private void WriteConfigurationToCache(string value)
        {
            byte[] byt = Convert.FromBase64String(value); // do not decrypt 
            using (FileStream fs = new FileStream(CFGUtilities.configcachedir, FileMode.Create, FileAccess.ReadWrite))
            {
                fs.Write(byt, 0, byt.Length);
                fs.Close();
            }
        }
        #endregion

        #region Replay management
        /// <summary>
        /// DoOnCheckForReplay method implementation (Server)
        /// </summary>
        private bool DoOnCheckForReplay(NamedPipeNotificationReplayRecord record)
        {
            return _replaymgr.AddToReplay(record);
        }

        /// <summary>
        /// DoOnCheckForRemoteReplay method implementation (Server)
        /// </summary>
        private bool DoOnCheckForRemoteReplay(NamedPipeNotificationReplayRecord record)
        {
            if (_adfsservers.Count == 0)
                BuildADFSServersList();

            PipeClient pipe = new PipeClient(XORUtilities.XORKey, this.ADFSServers, true);
            return pipe.DoCheckForRemoteReplay(record);
        }

        #endregion

        #region ADFS Service
        /// <summary>
        /// StartADFSService method implementation
        /// </summary>
        private void StartADFSService()
        {
            ServiceController ADFSController = null;
            try
            {
                ADFSController = new ServiceController("adfssrv");
                if (ADFSController.Status != ServiceControllerStatus.Running)
                    ADFSController.Start();
                ADFSController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry("Error Starting ADFS Service \r"+e.Message, EventLogEntryType.Error, 2);
                return;
            }
            finally
            {
                ADFSController.Close();
            }
        }

        /// <summary>
        /// StopADFSService method implementation
        /// </summary>
        private void StopADFSService()
        {
            ServiceController ADFSController = null;
            try
            {
                ADFSController = new ServiceController("adfssrv");
                if (ADFSController.Status != ServiceControllerStatus.Stopped)
                    ADFSController.Stop();
                ADFSController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 1, 0));
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry("Error Stopping ADFS Service \r" + e.Message, EventLogEntryType.Error, 3);
                return;
            }
            finally
            {
                ADFSController.Close();
            }
        }
        #endregion
    }
}
