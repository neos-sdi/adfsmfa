using Neos.IdentityServer.MultiFactor.Administration;
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
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.NotificationHub
{
    public partial class MFANOTIFHUB : ServiceBase
    {
       
        private List<string> _adfsservers = new List<string>();
        private MailSlotServerManager _mailslotsmgr = new MailSlotServerManager();
        private MailSlotServer _mailslotmfa = new MailSlotServer("NOT"); // And Broadcast
        private PipeServer _pipeserver = new PipeServer();
        private PipeServer _pipereplay = new PipeServer();

        #region Service override methods
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
            _pipeserver.OnMessage += PipeServerOnMessage;
            _pipeserver.OnDecrypt += PipeOnDecrypt;
            _pipeserver.OnEncrypt += PipeOnEncrypt;            
        }

        /// <summary>
        /// OnStart event
        /// </summary>
        protected override void OnStart(string[] args)
        {
            _mailslotsmgr.Start();
            _mailslotsmgr.MailSlotSystemMessageArrived += MailSlotSystemMessageArrived;

            _mailslotmfa.Start();
            _mailslotmfa.MailSlotMessageArrived += MFAMailSlotMessageArrived;

            Trace.WriteLine("MFANOTIFHUB:Start ADFS Service");

            StartADFSService();
            try
            {
                _pipeserver.Start();
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Pipe Server Error on Start : {0}.", e.Message), EventLogEntryType.Error, 1000);
            }
            LogForSlots.LogEnabled = true;
            this.EventLog.WriteEntry("Le service a démarré avec succès.", EventLogEntryType.Information, 0);
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
            _mailslotsmgr.MailSlotSystemMessageArrived -= MailSlotSystemMessageArrived;

            _mailslotmfa.Stop();
            _mailslotmfa.MailSlotMessageArrived -= MFAMailSlotMessageArrived;
            try
            {
                _pipeserver.Stop();
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Pipe Server Error on Close : {0}.", e.Message), EventLogEntryType.Error, 1000);
            }
            this.EventLog.WriteEntry("Le service s'est arrêté avec succès.", EventLogEntryType.Information, 1);
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
        #endregion

        #region System MailSlots Events
        /// <summary>
        /// MailSlotSystemMessageArrived implementation
        /// </summary>
        private void MailSlotSystemMessageArrived(MailSlotServerDispatcher mailslotserver)
        {
            try
            {
                StringBuilder bld = new StringBuilder();
                Process srvprocess = Process.GetProcessById(mailslotserver.ProcessID);
                bld.AppendLine(string.Format("Host Message Hub : {0} ", Environment.MachineName + "\\" + srvprocess.ProcessName));
                bld.AppendLine("-----------------------------------------------------------------------");
                bld.AppendLine(string.Format("Process ID : {0} ", mailslotserver.ProcessID));
                bld.AppendLine(string.Format("Machine Name: {0} ", Environment.MachineName));
                bld.AppendLine(string.Format("Application Name : {0} ", mailslotserver.ApplicationName));
                bld.AppendLine(string.Format("Multi Instances : {0} ", mailslotserver.MultiInstance.ToString()));
                bld.AppendLine(string.Format("Is Started : {0} ", mailslotserver.IsStarted.ToString()));
                bld.AppendLine(string.Format("Allow to Self : {0} ", mailslotserver.AllowToSelf.ToString()));
                bld.AppendLine(string.Format("Scan Duration : {0} ", mailslotserver.ScanDuration.ToString()));
                bld.AppendLine();
                foreach (MailSlotDispatcherMessage msg in mailslotserver.Instances)
                {
                    try
                    {
                        Process msgprocess = Process.GetProcessById(msg.TargetID);
                        bld.AppendLine(string.Format("Mailslot Process : {0} ", Environment.MachineName + "\\" + msgprocess.ProcessName));
                    }
                    catch (Exception E)
                    {
                        bld.AppendLine(string.Format("Mailslot Process : {0} ", Environment.MachineName + "\\Unknown Process"));
                        bld.AppendLine(E.Message);
                    }
                    bld.AppendLine("-----------------------------------------------------------------------");
                    bld.AppendLine(string.Format("Process ID : {0} ", msg.TargetID));
                    bld.AppendLine(string.Format("Machine Name: {0} ", Environment.MachineName));
                    bld.AppendLine(string.Format("Application Name : {0} ", msg.ApplicationName));
                    bld.AppendLine(string.Format("Multi Instances : {0} ", msg.MultiInstance.ToString()));
                    bld.AppendLine(string.Format("Allow to Self : {0} ", msg.AllowToSelf.ToString()));
                    bld.AppendLine();
                }
                this.EventLog.WriteEntry(bld.ToString(), EventLogEntryType.SuccessAudit, 10000);
            }
            catch (Exception ex)
            {
                this.EventLog.WriteEntry(ex.Message, EventLogEntryType.FailureAudit, 10000);
            }
        }

        /// <summary>
        /// MailSlotMessageArrived implementation
        /// </summary>
        private void MailSlotMessageArrived(MailSlotServer maislotserver, MailSlotMessage message)
        {
            if (message.ApplicationName == "CP1")
            {
                switch (message.Operation)
                {
                    case (byte)NotificationsKind.ServiceStatusRunning:
                        this.EventLog.WriteEntry(string.Format("Server Information REQUEST To {0}", message.Text), EventLogEntryType.SuccessAudit, 10000);
                        using (MailSlotClient mailslot = new MailSlotClient("CP1"))
                        {
                            mailslot.Text = Dns.GetHostEntry(message.Text).HostName.ToLower();
                            mailslot.SendNotification(NotificationsKind.ServiceStatusStopped);
                        }
                        break;
                    case (byte)NotificationsKind.ServiceStatusStopped:
                        string localname = Dns.GetHostEntry("localhost").HostName.ToLower();
                        if (localname.ToLower().Equals(message.Text.ToLower()))
                        {
                            this.EventLog.WriteEntry(string.Format("Server Information RECEIVED for {0}", message.Text), EventLogEntryType.SuccessAudit, 10000);
                            using (MailSlotClient mailslot = new MailSlotClient("CP1"))
                            {
                                FarmUtilities xreg = new FarmUtilities();
                                mailslot.Text = xreg.InitServerNodeConfiguration();
                                mailslot.SendNotification(NotificationsKind.ServiceStatusPending);
                            }
                        }
                        break;
                    case (byte)NotificationsKind.ServiceStatusPending :
                        FarmUtilities reg = new FarmUtilities();
                        ADFSServerHost host = reg.UnPackServerNodeConfiguration(message.Text);

                        ManagementService.EnsureService();
                        int i = ManagementService.ADFSManager.ADFSFarm.Servers.FindIndex(c => c.FQDN.ToLower() == host.FQDN.ToLower());
                        if (i < 0)
                            ManagementService.ADFSManager.ADFSFarm.Servers.Add(host);
                        else
                            ManagementService.ADFSManager.ADFSFarm.Servers[i] = host;

                        ManagementService.ADFSManager.SetDirty(true);
                        ManagementService.ADFSManager.WriteConfiguration(null);

                        this.EventLog.WriteEntry(string.Format("Server Information RESPONSE : {0}", message.Text), EventLogEntryType.SuccessAudit, 10000);
                        break;
                }
            }
        }
        #endregion

        #region Named Pipes Methods /events
        /// <summary>
        /// MFAMailSlotMessageArrived implementation
        /// </summary>
        private void MFAMailSlotMessageArrived(MailSlotServer maislotserver, MailSlotMessage message)
        {
            if (_adfsservers.Count == 0)
                BuildADFSServersList();

            if (message.ApplicationName == "BDC")
            {
                PipeClient pipe = new PipeClient(Utilities.XORKey, ADFSServers);
                if (message.Operation == (byte)NotificationsKind.ConfigurationReload)
                {
                    pipe.OnEncrypt += PipeOnEncrypt;
                    pipe.OnDecrypt += PipeOnDecrypt;
                    if (CFGUtilities.IsWIDConfiguration())
                    {
                        if (CFGUtilities.IsPrimaryComputer())
                        {
                            MFAConfig config = CFGUtilities.ReadConfiguration();
                            XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                            MemoryStream stm = new MemoryStream();
                            using (StreamReader reader = new StreamReader(stm))
                            {
                                xmlserializer.Serialize(stm, config);
                                stm.Position = 0;
                                byte[] byt = CFGUtilities.XOREncryptOrDecrypt(stm.ToArray(), Utilities.XORKey);
                                string msg = Convert.ToBase64String(byt, 0, byt.Length);
                                pipe.SendMessage(NotificationsKind.ConfigurationReload, "BDC", msg);
                            }
                        }
                    }
                    else
                        pipe.SendMessage((NotificationsKind)message.Operation, message.ApplicationName, message.Text);
                }
            }
        }

        /// <summary>
        /// PipeServerOnMessage method implementation
        /// </summary>
        private void PipeServerOnMessage(NotificationsKind notif, string username, string application, string value)
        {
            if (application.ToUpper().Equals("BDC"))
            {
                if (notif == NotificationsKind.ConfigurationReload)
                {
                    try
                    {
                        byte[] byt = Convert.FromBase64String(value); // do not decrypt 
                        using (FileStream fs = new FileStream(CFGUtilities.configcachedir, FileMode.Create, FileAccess.ReadWrite))
                        {
                            fs.Write(byt, 0, byt.Length);
                            fs.Close();
                        }
                    }
                    finally
                    {

                    }
                }
                using (MailSlotClient mailslot = new MailSlotClient())
                {
                    mailslot.Text = username;
                    mailslot.SendNotification(notif);
                }
            }
            else
            {
                using (MailSlotClient mailslot = new MailSlotClient(application))
                {
                    mailslot.Text = value;
                    mailslot.SendNotification(notif);
                }
            }
        }
        #endregion

        #region Encryption/Decryption
        /// <summary>
        /// PipeOnDecrypt method implementation
        /// </summary>
        private string PipeOnDecrypt(string cryptedvalue)
        {
            byte[] byt = CFGUtilities.XOREncryptOrDecrypt(System.Convert.FromBase64String(cryptedvalue), Utilities.XORKey);
            return System.Text.Encoding.UTF8.GetString(byt);
        }

        /// <summary>
        /// PipeOnEncrypt method implementation
        /// </summary>
        private string PipeOnEncrypt(string clearvalue)
        {
            byte[] byt = CFGUtilities.XOREncryptOrDecrypt(System.Text.Encoding.UTF8.GetBytes(clearvalue), Utilities.XORKey);
            return System.Convert.ToBase64String(byt);
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
