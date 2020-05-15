//******************************************************************************************************************************************************************************************//
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
using System.Collections.Generic;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.ServiceProcess;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Threading;
using System.Windows.Forms;

namespace Neos.IdentityServer.Deployment
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult StartService(Session session)
        {
            try
            {
                session.Log("Service Starting [mfanotifhub]");
                Trace.TraceInformation("Service Starting [mfanotifhub]");
                internalStartService();
                session.Log("Service Started [mfanotifhub]");
                Trace.TraceInformation("Service Started [mfanotifhub]");
                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log("Service error [mfanotifhub] : "+e.Message);
                Trace.TraceInformation("Service error [mfanotifhub] : " + e.Message);
                return ActionResult.Success;
            }
        }

        [CustomAction]
        public static ActionResult StopService(Session session)
        {
            try
            {
                session.Log("Service Stopping [mfanotifhub]");
                internalStopService();
                session.Log("Service Stopped [mfanotifhub]");
                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log("Service error [mfanotifhub] : " + e.Message);
                return ActionResult.Success;
            }
        }

        [CustomAction]
        public static ActionResult InstallService(Session session)
        {
           // string path = session["ProgramFiles64Folder"];
            try
            {
                try
                {
                    RegisterEventLogs();
                }
                catch (Exception E)
                {
                    session.Log("Error registering EventLog entries : "+E.Message);
                }
                session.Log("Service Installing [mfanotifhub]");
                internalInstallService(session, @"C:\Program Files\MFA\Neos.IdentityServer.MultiFactor.NotificationHub.exe");
                session.Log("Service Installed [mfanotifhub]");
                session.Log("Snapin Installing [Neos.IdentityServer.console]");
                internalInstallSnapin(session, @"C:\Program Files\MFA\Neos.IdentityServer.Console.dll");
                session.Log("Snapin Installed [Neos.IdentityServer.console]");
                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log("Service error [mfanotifhub] : " + e.Message);
                return ActionResult.Success;
            }
        }

        [CustomAction]
        public static ActionResult UnInstallService(Session session)
        {
           // string path = session["ProgramFiles64Folder"];
            try
            {
                session.Log("Service UnInstalling [mfanotifhub]");
                internalUninstallService(session, @"C:\Program Files\MFA\Neos.IdentityServer.MultiFactor.NotificationHub.exe");
                session.Log("Service UnInstalled [mfanotifhub]");
                session.Log("Snapin UnInstalling [Neos.IdentityServer.console]");
                internalUninstallSnapin(session, @"C:\Program Files\MFA\Neos.IdentityServer.Console.dll");
                session.Log("Snapin UnInstalling [Neos.IdentityServer.console]");

                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log("Service error [mfanotifhub] : " + e.Message);
                return ActionResult.Success;
            }
            finally
            {
                try
                {
                    session.Log("Delete Cache config file");
                    File.Delete(@"C:\Program Files\MFA\Config\Config.db");
                }
                catch { }
            }
        }

        public static void internalInstallService(Session session, string exeFilename)
        {
            if (!File.Exists(exeFilename))
               return;
            try
            {
                System.Configuration.Install.AssemblyInstaller installer = new System.Configuration.Install.AssemblyInstaller();
                IDictionary mySavedState = new Hashtable();

                string dir = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);
                string file = dir + "\\adfsmfa_service.log";
                File.Delete(file);

                installer.UseNewContext = true;
                installer.Path = exeFilename;
                installer.CommandLine = new string[2] { string.Format("/logFile={0}", file), string.Format("/InstallStateDir={0}", dir) };
                mySavedState.Clear();
                installer.Install(mySavedState);
                installer.Commit(mySavedState);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void internalUninstallService(Session session, string exeFilename)
        {
            if (!File.Exists(exeFilename))
               return;
            try
            { 
            System.Configuration.Install.AssemblyInstaller installer = new System.Configuration.Install.AssemblyInstaller();
            IDictionary mySavedState = new Hashtable();

            string dir = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);
            string file = dir + "\\adfsmfa_service.log";
            installer.UseNewContext = true;
            installer.Path = exeFilename;
            installer.CommandLine = new string[2] { string.Format("/logFile={0}", file), string.Format("/InstallStateDir={0}", dir) };
            mySavedState.Clear();

            installer.Uninstall(mySavedState);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void internalInstallSnapin(Session session, string dllFilename)
        {
            if (!File.Exists(dllFilename))
                return;
            System.Configuration.Install.AssemblyInstaller installer = new System.Configuration.Install.AssemblyInstaller();
            IDictionary mySavedState = new Hashtable();

            string dir = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);
            string file = dir + "\\adfsmfa_snapin.log";
            File.Delete(file);

            installer.UseNewContext = true;
            installer.Path = dllFilename;
            installer.CommandLine = new string[2] { string.Format("/logFile={0}", file), string.Format("/InstallStateDir={0}", dir) };
            mySavedState.Clear();
            installer.Install(mySavedState);
            installer.Commit(mySavedState);
        }

        public static void internalUninstallSnapin(Session session, string dllFilename)
        {
            if (!File.Exists(dllFilename))
                return;
            System.Configuration.Install.AssemblyInstaller installer = new System.Configuration.Install.AssemblyInstaller();
            IDictionary mySavedState = new Hashtable();

            string dir = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);
            string file = dir + "\\adfsmfa_snapin.log";
            installer.UseNewContext = true;
            installer.Path = dllFilename;
            installer.CommandLine = new string[2] { string.Format("/logFile={0}", file), string.Format("/InstallStateDir={0}", dir) };
            mySavedState.Clear();

            installer.Uninstall(mySavedState);
        }

        /// <summary>
        /// internalStartService method implementation
        /// </summary>
        private static void internalStartService()
        {
            ServiceController ADFSController = null;
            try
            {
                ADFSController = new ServiceController("mfanotifhub");
                if ((ADFSController.Status != ServiceControllerStatus.Running) && (ADFSController.Status != ServiceControllerStatus.StartPending))
                {
                    ADFSController.Start(new string[] {"Install"});
                    ADFSController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));
                }
            }
            catch (Exception)
            {
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
        private static void internalStopService()
        {
            ServiceController ADFSController = null;
            try
            {
                ADFSController = new ServiceController("mfanotifhub");
                if ((ADFSController.Status != ServiceControllerStatus.Stopped) && (ADFSController.Status != ServiceControllerStatus.StopPending))
                {
                    ADFSController.Stop();
                    ADFSController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 1, 0));
                }
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                ADFSController.Close();
            }
        }

        private static string EventLogSource = "ADFS MFA DataServices";
        private static string AdminEventLogSource = "ADFS MFA Administration";
        private static string MFAEventLogSource = "ADFS MFA Service";
        private static string MMCEventLogSource = "ADFS MFA MMC";
        private static string NOTIFEventLogSource = "ADFS MFA Notification Hub";
        private static string EventLogGroup = "Application";

        /// <summary>
        /// RegisterEventLogs method implementation
        /// </summary>
        private static void RegisterEventLogs()
        {
            if (!EventLog.SourceExists(EventLogSource))
                EventLog.CreateEventSource(EventLogSource, EventLogGroup);
            if (!EventLog.SourceExists(AdminEventLogSource))
                EventLog.CreateEventSource(AdminEventLogSource, EventLogGroup);
            if (!EventLog.SourceExists(MFAEventLogSource))
                EventLog.CreateEventSource(MFAEventLogSource, EventLogGroup);
            if (!EventLog.SourceExists(MMCEventLogSource))
                EventLog.CreateEventSource(MMCEventLogSource, EventLogGroup);
            if (!EventLog.SourceExists(NOTIFEventLogSource))
                EventLog.CreateEventSource(NOTIFEventLogSource, EventLogGroup);
        }
    }
}
