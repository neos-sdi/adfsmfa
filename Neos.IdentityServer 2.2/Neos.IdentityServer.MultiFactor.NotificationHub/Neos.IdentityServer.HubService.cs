//******************************************************************************************************************************************************************************************//
// Copyright (c) 2017 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor
{
    public partial class MFANOTIFHUB : ServiceBase
    {
        private MailSlotServerManager _mailslotsmgr = new MailSlotServerManager();

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
        }

        /// <summary>
        /// MailSlotSystemMessageArrived implementation
        /// </summary>
        private void MailSlotSystemMessageArrived(MailSlotServerDispatcher mailslotserver)
        {
            try
            {
                StringBuilder bld = new StringBuilder();
                Process srvprocess = Process.GetProcessById(mailslotserver.ProcessID);
                bld.AppendLine(string.Format("Host Message Hub : {0} ", mailslotserver.MachineName + "\\" + srvprocess.ProcessName));
                bld.AppendLine("-----------------------------------------------------------------------");
                bld.AppendLine(string.Format("Process ID : {0} ", mailslotserver.ProcessID));
                bld.AppendLine(string.Format("Machine Name: {0} ", mailslotserver.MachineName));
                bld.AppendLine(string.Format("Application Name : {0} ", mailslotserver.ApplicationName));
                bld.AppendLine(string.Format("Multi Instances : {0} ", mailslotserver.MultiInstance.ToString()));
                bld.AppendLine(string.Format("Is Started : {0} ", mailslotserver.IsStarted.ToString()));
                bld.AppendLine(string.Format("Local only : {0} ", mailslotserver.LocalOnly.ToString()));
                bld.AppendLine(string.Format("Allow to Self : {0} ", mailslotserver.AllowToSelf.ToString()));
                bld.AppendLine(string.Format("Scan Duration : {0} ", mailslotserver.ScanDuration.ToString()));
                bld.AppendLine();
                foreach (MailSlotDispatcherMessage msg in mailslotserver.Instances)
                {
                    try
                    {
                        Process msgprocess = Process.GetProcessById(msg.TargetID);
                        bld.AppendLine(string.Format("Mailslot Process : {0} ", msg.MachineName + "\\" + msgprocess.ProcessName));
                    }
                    catch (Exception E)
                    {
                        bld.AppendLine(string.Format("Mailslot Process : {0} ", msg.MachineName + "\\Unknown Process"));
                        bld.AppendLine(E.Message);
                    }
                    bld.AppendLine("-----------------------------------------------------------------------");
                    bld.AppendLine(string.Format("Process ID : {0} ", msg.TargetID));
                    bld.AppendLine(string.Format("Machine Name: {0} ", msg.MachineName));
                    bld.AppendLine(string.Format("Application Name : {0} ", msg.ApplicationName));
                    bld.AppendLine(string.Format("Multi Instances : {0} ", msg.MultiInstance.ToString()));
                    bld.AppendLine(string.Format("Local only : {0} ", msg.Local.ToString()));
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
        /// OnStart event
        /// </summary>
        protected override void OnStart(string[] args)
        {
            Trace.WriteLine("MFANOTIFHUB:Start MailSlot Manager " + _mailslotsmgr.ApplicationName);
            _mailslotsmgr.Start();
            _mailslotsmgr.MailSlotSystemMessageArrived += MailSlotSystemMessageArrived;
            Trace.WriteLine("MFANOTIFHUB:Start ADFS Service");
            StartADFSService();
            this.EventLog.WriteEntry("Le service a démarré avec succès.",  EventLogEntryType.Information, 0); 
        }

        /// <summary>
        /// OnStop event
        /// </summary>
        protected override void OnStop()
        {
            Trace.WriteLine("MFANOTIFHUB:Stop ADFS Service");
            StopADFSService();
            _mailslotsmgr.MailSlotSystemMessageArrived -= MailSlotSystemMessageArrived;
            Trace.WriteLine("MFANOTIFHUB:Stop MailSlot Manager " + _mailslotsmgr.ApplicationName);
            _mailslotsmgr.Stop();
            this.EventLog.WriteEntry("Le service s'est arrêté avec succès.",  EventLogEntryType.Information, 1);
        }

        /// <summary>
        /// internalStartService method implementation
        /// </summary>
        private void StartADFSService()
        {
            ServiceController ADFSController = null;
            try
            {
                ADFSController = new ServiceController("adfssrv");
                using (MailSlotClient mailslot = new MailSlotClient("MGT"))
                {
                    mailslot.Text = Environment.MachineName;
                    mailslot.SendNotification(0x12);
                }
                if (ADFSController.Status != ServiceControllerStatus.Running)
                    ADFSController.Start();
                ADFSController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));
                using (MailSlotClient mailslot = new MailSlotClient("MGT"))
                {
                    mailslot.Text = Environment.MachineName;
                    mailslot.SendNotification(0x10);
                }
            }
            catch (Exception e)
            {
                using (MailSlotClient mailslot = new MailSlotClient("MGT"))
                {
                    mailslot.Text = Environment.MachineName;
                    mailslot.SendNotification(0x19);
                }
                this.EventLog.WriteEntry("Error Starting ADFS Service \r"+e.Message, EventLogEntryType.Error, 2);
                return;
            }
            finally
            {
                ADFSController.Close();
            }
        }

        /// <summary>
        /// internalStopService method implementation
        /// </summary>
        private void StopADFSService()
        {
            ServiceController ADFSController = null;
            try
            {
                ADFSController = new ServiceController("adfssrv");
                using (MailSlotClient mailslot = new MailSlotClient("MGT"))
                {
                    mailslot.Text = Environment.MachineName;
                    mailslot.SendNotification(0x12);
                }
                if (ADFSController.Status != ServiceControllerStatus.Stopped)
                    ADFSController.Stop();
                ADFSController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 1, 0));
                using (MailSlotClient mailslot = new MailSlotClient("MGT"))
                {
                    mailslot.Text = Environment.MachineName;
                    mailslot.SendNotification(0x11);
                }
            }
            catch (Exception e)
            {
                using (MailSlotClient mailslot = new MailSlotClient("MGT"))
                {
                    mailslot.Text = Environment.MachineName;
                    mailslot.SendNotification(0x19);
                }
                this.EventLog.WriteEntry("Error Stopping ADFS Service \r" + e.Message, EventLogEntryType.Error, 3);
                return;
            }
            finally
            {
                ADFSController.Close();
            }
        }
    }
}
