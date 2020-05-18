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
using Microsoft.Win32;

namespace Neos.IdentityServer.Deployment
{
    public class CustomActions
    {
      //  [CustomAction]
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
                return ActionResult.Failure;
            }
        }

      //  [CustomAction]
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
                return ActionResult.Failure;
            }
        }

        /// <summary>
        /// GetInstallPath method implementation
        /// Doing that, because with Wix Custom actions runs in 32 bits, and hangs in 64 Bits...
        /// </summary>
        private static string GetInstallPath(Session session)
        {         
            string baseDirectory = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
            string programFiles = "Program Files";
            string programFilesX86 = "Program Files (x86)";
            if (Environment.Is64BitOperatingSystem)
                return Path.Combine(baseDirectory, programFiles)+@"\MFA\";
            else
                return Path.Combine(baseDirectory, programFilesX86) + @"\MFA\";
        }

        [CustomAction]
        public static ActionResult InstallService(Session session)
        {

            string path = string.Empty;
            try
            {
                try
                {
                    RegisterEventLogs();
                    path = GetInstallPath(session);
                }
                catch (Exception e)
                {
                    session.Log(e.Message);
                }
                session.Log("Service Installing [mfanotifhub]");
                internalInstallService(session, path + @"Neos.IdentityServer.MultiFactor.NotificationHub.exe");
                session.Log("Service Installed [mfanotifhub]");
                session.Log("Snapin Installing [Neos.IdentityServer.console]");
                internalInstallSnapin(session, path + @"Neos.IdentityServer.Console.dll");
                session.Log("Snapin Installed [Neos.IdentityServer.console]");
                StartService(session);
                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log("Service error [mfanotifhub] : " + e.Message);
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult UnInstallService(Session session)
        {
            string path = string.Empty;
            try
            {
                try
                {
                    path = GetInstallPath(session);
                }
                catch (Exception e)
                {
                    session.Log(e.Message);
                }
                StopService(session);
                session.Log("Service UnInstalling [mfanotifhub]");
                internalUninstallService(session, path + @"Neos.IdentityServer.MultiFactor.NotificationHub.exe");
                session.Log("Service UnInstalled [mfanotifhub]");
                session.Log("Snapin UnInstalling [Neos.IdentityServer.console]");
                internalUninstallSnapin(session, path + @"Neos.IdentityServer.Console.dll");
                session.Log("Snapin UnInstalled [Neos.IdentityServer.console]");

                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log("Service error [mfanotifhub] : " + e.Message);
                return ActionResult.Failure;
            }
            finally
            {
                try
                {
                    session.Log("Delete Cache config file");
                    File.Delete(path + @"Config\Config.db");
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// internalInstallService method implementation
        /// </summary>
        private static void internalInstallService(Session session, string exeFilename)
        {
            if (!File.Exists(exeFilename))
                throw new FileLoadException("Invalid FileName : " + exeFilename);
            try
            {
                IDictionary installstate = new Hashtable();

                string dir = Path.GetDirectoryName(exeFilename);
                string file = dir + "\\" + Path.GetFileNameWithoutExtension(exeFilename) + ".installLog";
                File.Delete(file);

                System.Configuration.Install.AssemblyInstaller installer = new System.Configuration.Install.AssemblyInstaller();
                installer.UseNewContext = true;
                installer.Path = exeFilename;
                installer.CommandLine = new string[2] { string.Format("/logFile={0}", file), string.Format("/InstallStateDir={0}", dir) };

                installer.Install(installstate);
                installer.Commit(installstate); 

               // System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { exeFilename });

                string state = Path.GetFileNameWithoutExtension(exeFilename) + ".installState";
                File.Delete(dir + "\\" + state);
            }
            catch (Exception e)
            {
                session.Log(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// internalUninstallService method implementation
        /// </summary>
        private static void internalUninstallService(Session session, string exeFilename)
        {
            if (!File.Exists(exeFilename))
               throw new FileLoadException("Invalid FileName : " + exeFilename);
            try
            {
                string dir = Path.GetDirectoryName(exeFilename);
                string file = dir + "\\" + Path.GetFileNameWithoutExtension(exeFilename) + ".installLog";
                File.Delete(file);

                System.Configuration.Install.AssemblyInstaller installer = new System.Configuration.Install.AssemblyInstaller();
                installer.UseNewContext = true;
                installer.Path = exeFilename;
                installer.CommandLine = new string[2] { string.Format("/logFile={0}", file), string.Format("/InstallStateDir={0}", dir) };
                installer.Uninstall(new Hashtable()); 

               // System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { "/u", exeFilename });

                string state = Path.GetFileNameWithoutExtension(exeFilename) + ".installState";
                File.Delete(dir + "\\" + state);
            }
            catch (Exception e)
            {
                session.Log(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// internalInstallSnapin method implementation
        /// </summary>
        private static void internalInstallSnapin(Session session, string dllFilename)
        {
            if (!File.Exists(dllFilename))
                throw new FileLoadException("Invalid FileName : " + dllFilename);
            try
            {
                IDictionary installstate = new Hashtable();

                string dir = Path.GetDirectoryName(dllFilename);
                string file = dir + "\\" + Path.GetFileNameWithoutExtension(dllFilename) + ".installLog";
                File.Delete(file);

                System.Configuration.Install.AssemblyInstaller installer = new System.Configuration.Install.AssemblyInstaller();
                installer.UseNewContext = true;
                installer.Path = dllFilename;
                installer.CommandLine = new string[2] { string.Format("/logFile={0}", file), string.Format("/InstallStateDir={0}", dir) };
                installer.Install(installstate);
                installer.Commit(installstate); 

              //  System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { dllFilename });

                string state = Path.GetFileNameWithoutExtension(dllFilename) + ".installState";
                File.Delete(dir + "\\" + state);
            }
            catch (Exception e)
            {
                session.Log(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// internalUninstallSnapin method implementation
        /// </summary>
        private static void internalUninstallSnapin(Session session, string dllFilename)
        {
            if (!File.Exists(dllFilename))
                throw new FileLoadException("Invalid FileName : " + dllFilename);
            try
            {
                string dir = Path.GetDirectoryName(dllFilename);
                string file = dir + "\\" + Path.GetFileNameWithoutExtension(dllFilename) + ".installLog";
                File.Delete(file);

                System.Configuration.Install.AssemblyInstaller installer = new System.Configuration.Install.AssemblyInstaller();
                installer.UseNewContext = true;
                installer.Path = dllFilename;
                installer.CommandLine = new string[2] { string.Format("/logFile={0}", file), string.Format("/InstallStateDir={0}", dir) };
                installer.Uninstall(new Hashtable());

               // System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { "/u", dllFilename });

                string state = Path.GetFileNameWithoutExtension(dllFilename) + ".installState";
                File.Delete(dir +"\\" + state);
            }
            catch (Exception e)
            {
                session.Log(e.Message);
                throw e;
            }
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
