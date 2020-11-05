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
using Neos.IdentityServer.MultiFactor.Administration;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;

namespace Neos.IdentityServer.MultiFactor.NotificationHub
{
    public partial class MFANOTIFHUB : ServiceBase
    {
        private MailSlotServerManager _mailslotsmgr = new MailSlotServerManager();
        private ReplayServer<ReplayService> _svchost = new ReplayServer<ReplayService>();
        private WebThemesServer<WebThemeService> _svcthemeshost = new WebThemesServer<WebThemeService>();
        private CleanUpManager _cleanup = new CleanUpManager();

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
            try
            {
                _mailslotsmgr.Start();
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Mailslot Server Error on Start : {0}.", e.Message), EventLogEntryType.Error, 1000);
            }
        }

        /// <summary>
        /// OnStart event
        /// </summary>
        protected override void OnStart(string[] args)
        {
            Trace.WriteLine("MFANOTIFHUB:Start ADFS Service");
            LogForSlots.LogEnabled = false;
            try
            {                
                StartADFSService();
                StartReplayService();
                StartThemesService();
                StartKeyCleanup();
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Service Error on Start : {0}.", e.Message), EventLogEntryType.Error, 1000);
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
            try
            {
                StopKeyCleanup();
                StopThemesService();
                StopReplayService();
                StopADFSService();
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Service Error on Close : {0}.", e.Message), EventLogEntryType.Error, 1000);
            }
            LogForSlots.LogEnabled = false;
            this.EventLog.WriteEntry("Service was stopped successfully.", EventLogEntryType.Information, 1);
        }
        #endregion

        #region Replay Service
        /// <summary>
        /// StartReplayService method implementation
        /// </summary>
        private void StartReplayService()
        {
            try
            {
                _svchost.StartService(this);
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Error when starting ReplayService : {0}.", e.Message), EventLogEntryType.Error, 1001);
            }
        }

        /// <summary>
        /// StopReplayService method implementation
        /// </summary>
        private void StopReplayService()
        {
            try
            {
                _svchost.StopService();
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Error when stopping ReplayService : {0}.", e.Message), EventLogEntryType.Error, 1001);
            }
        }
        #endregion

        #region Themes Service
        /// <summary>
        /// StartThemesService method implementation
        /// </summary>
        private void StartThemesService()
        {
            try
            {
                _svcthemeshost.StartService(this);
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Error when starting ThemesService : {0}.", e.Message), EventLogEntryType.Error, 1002);
            }
        }

        /// <summary>
        /// StopThemesService method implementation
        /// </summary>
        private void StopThemesService()
        {
            try
            {
                _svcthemeshost.StopService();
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Error when stopping Themes Service : {0}.", e.Message), EventLogEntryType.Error, 1002);
            }
        }
        #endregion

        #region CleanUp
        /// <summary>
        /// StartKeyCleanup method implementation
        /// </summary>
        private void StartKeyCleanup()
        {
            try
            {
                _cleanup.Start(this);
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Error when starting CleanUp Service : {0}.", e.Message), EventLogEntryType.Error, 1003);
            }
        }

        /// <summary>
        /// StopKeyCleanup method implementation
        /// </summary>
        private void StopKeyCleanup()
        {
            try
            {
                _cleanup.Close();
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Error when stopping CleanUp Service : {0}.", e.Message), EventLogEntryType.Error, 1003);
            }
        }
        #endregion

        #region ADFS Service
        /// <summary>
        /// StartADFSService method implementation
        /// </summary>
        private void StartADFSService()
        {
            SvcController ADFSController = null;
            try
            {
                ADFSController = new SvcController("adfssrv");
                if ((ADFSController.Status != ServiceControllerStatus.Running) && (ADFSController.Status != ServiceControllerStatus.StartPending))
                {
                    ADFSController.Start();
                    ADFSController.GetStatus();
                    if (ADFSController.Status!=ServiceControllerStatus.Running)
                        ADFSController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));
                }
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
            SvcController ADFSController = null;
            try
            {
                ADFSController = new SvcController("adfssrv");
                if ((ADFSController.Status != ServiceControllerStatus.Stopped) && (ADFSController.Status != ServiceControllerStatus.StopPending))
                {
                    ADFSController.Stop();
                    ADFSController.GetStatus();
                    if (ADFSController.Status != ServiceControllerStatus.Stopped)
                        ADFSController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 1, 0));
                }
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
