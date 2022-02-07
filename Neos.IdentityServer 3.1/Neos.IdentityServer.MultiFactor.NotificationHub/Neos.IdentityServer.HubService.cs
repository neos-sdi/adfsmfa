//******************************************************************************************************************************************************************************************//
// Copyright (c) 2022 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.ServiceProcess;

namespace Neos.IdentityServer.MultiFactor.NotificationHub
{
    public partial class MFANOTIFHUB : ServiceBase
    {
        private MailSlotServerManager _mailslotsmgr = new MailSlotServerManager();
        private ReplayServer<ReplayService> _svchost = new ReplayServer<ReplayService>();
        private WebThemesServer<WebThemeService> _svcthemeshost = new WebThemesServer<WebThemeService>();
        private WebAdminServer<WebAdminService> _svcadminhost = new WebAdminServer<WebAdminService>();
        private NTServiceServer<NTService> _svcnthost = new NTServiceServer<NTService>();
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
                StopADFSService();
                CleanupStartupFiles();
                StartNTService();
                StartReplayService();
                StartADFSService();
                StartThemesService();
                StartAdminService();
                StartKeyCleanup();
                InitSecurityFile();
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
                StopAdminService();
                StopThemesService();
                StopADFSService();
                StopReplayService();
                StopNTService();
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
                ReplayClient replaymanager = new ReplayClient();
                replaymanager.Initialize();
                try
                {
                    IReplay client = replaymanager.Open();
                    try
                    {
                        client.WarmUp();
                    }
                    catch (Exception e)
                    {
                        this.EventLog.WriteEntry(string.Format("Error on WarmUp ReplayService : {0}.", e.Message), EventLogEntryType.Error, 1001);
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

        #region Admin Service
        /// <summary>
        /// StartThemesService method implementation
        /// </summary>
        private void StartAdminService()
        {
            try
            {
                _svcadminhost.StartService(this);
                StorePrimaryServerStatus(InitLocalNodeType());
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Error when starting AdminService : {0}.", e.Message), EventLogEntryType.Error, 1002);
            }
        }

        /// <summary>
        /// StopThemesService method implementation
        /// </summary>
        private void StopAdminService()
        {
            try
            {
                _svcadminhost.StopService();
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Error when stopping AdminService : {0}.", e.Message), EventLogEntryType.Error, 1002);
            }
        }
        #endregion

        #region NTService
        /// <summary>
        /// StartThemesService method implementation
        /// </summary>
        private void StartNTService()
        {
            try
            {
                _svcnthost.StartService(this);
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Error when starting NTService : {0}.", e.Message), EventLogEntryType.Error, 1002);
            }
        }

        /// <summary>
        /// StopThemesService method implementation
        /// </summary>
        private void StopNTService()
        {
            try
            {
                _svcnthost.StopService();
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(string.Format("Error when stopping NTService : {0}.", e.Message), EventLogEntryType.Error, 1002);
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
            ServiceController ADFSController = null;
            try
            {
                ADFSController = new ServiceController("adfssrv");
                if ((ADFSController.Status != ServiceControllerStatus.Running) && (ADFSController.Status != ServiceControllerStatus.StartPending))
                {
                    ADFSController.Start();
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
            ServiceController ADFSController = null;
            try
            {
                ADFSController = new ServiceController("adfssrv");
                if ((ADFSController.Status != ServiceControllerStatus.Stopped) && (ADFSController.Status != ServiceControllerStatus.StopPending))
                {
                    ADFSController.Stop();
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

        #region private methods
        /// <summary>
        /// InitLocalNodeType method implementation
        /// </summary>
        private bool InitLocalNodeType()
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
                    if (!result.BaseObject.ToString().ToLower().Equals("primarycomputer"))
                       return false;
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
            }
            return true;
        }

        /// <summary>
        /// StorePrimaryServerStatus method implementation
        /// </summary>
        private void StorePrimaryServerStatus(bool isprimary)
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\MFA", true);
            if (isprimary)
                rk.SetValue("IsPrimaryServer", 1, RegistryValueKind.DWord);
            else
                rk.SetValue("IsPrimaryServer", 0, RegistryValueKind.DWord);
        }

        /// <summary>
        /// CleanupStartupFiles method implementation
        /// </summary>
        private void CleanupStartupFiles()
        {
            if (File.Exists(CFGUtilities.ConfigCacheFile))
                File.Delete(CFGUtilities.ConfigCacheFile);
            if (File.Exists(SystemUtilities.SystemCacheFile))
                File.Delete(SystemUtilities.SystemCacheFile);
        }

        /// <summary>
        /// InitSecurityFile method implementation
        /// </summary>
        protected void InitSecurityFile()
        {
            string fqdn = Dns.GetHostEntry("localhost").HostName;
            WebAdminClient manager = new WebAdminClient();
            manager.Initialize();
            try
            {
                MFAConfig config = CFGReaderUtilities.ReadConfiguration();
                IWebAdminServices client = manager.Open();
                try
                {
                    SIDs.Clear();
                    SIDs.Assign(client.GetSIDsInformations(GetServers(config)));
                    SIDs.InternalUpdateDirectoryACLs(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + Path.DirectorySeparatorChar + "MFA");
                }
                catch (Exception ex)
                {
                    Log.WriteEntry(string.Format("Error on WebAdminService Service GetSIDsInformations method : {0} / {1}.", fqdn, ex.Message), EventLogEntryType.Error, 2010);
                    SIDs.Assign(new SIDsParametersRecord() { Loaded = false });
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
            return;
        }

        /// <summary>
        /// GetServers method implementation
        /// </summary>
        private Dictionary<string, bool> GetServers(MFAConfig config)
        {
            var servernames = (from server in config.Hosts.ADFSFarm.Servers
                               select (server.FQDN.ToLower(), server.NodeType.ToLower().Equals("primarycomputer")));
            Dictionary<string, bool> servers = servernames.ToDictionary(s => s.Item1, s => s.Item2);
            return servers;
        }
        #endregion
    }
}
