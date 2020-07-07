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
using System.Linq;
using System.Reflection;
using Microsoft.ManagementConsole;

namespace Neos.IdentityServer.Deployment
{
    public partial class CustomActions
    {
        /// <summary>
        /// StartService method implementation
        /// </summary>
        public static bool StartService(Session session, string exeFileName)
        {
            try
            {
                if (!File.Exists(exeFileName))
                    return false;
                session.Log("Service Starting [mfanotifhub]");
                Trace.TraceInformation("Service Starting [mfanotifhub]");
                internalStartService();
                session.Log("Service Started [mfanotifhub]");
                Trace.TraceInformation("Service Started [mfanotifhub]");
                return true;
            }
            catch (Exception e)
            {
                session.Log("Service error [mfanotifhub] : "+e.Message);
                Trace.TraceInformation("Service error [mfanotifhub] : " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// StopService method implementation
        /// </summary>
        public static bool StopService(Session session, string exeFileName)
        {
            bool ret = false;
            try
            {
                if (!File.Exists(exeFileName))
                   return false;
                session.Log("Service Stopping [mfanotifhub]");
                ret = internalStopService();
                session.Log("Service Stopped [mfanotifhub]");
                return ret;
            }
            catch (Exception e)
            {
                session.Log("Service error [mfanotifhub] : " + e.Message);
                return false;
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

        public static void ResetProgressBar(Session session, int totalStatements, string actionName, string actionDesc)
        {
            using (Record actionrecord = new Record(2))
            {
                actionrecord.SetString(1, actionName);
                actionrecord.SetString(2, actionDesc);
               // actionrecord.SetString(3, "[0]");
                session.Message(InstallMessage.ActionStart, actionrecord);
            }
            Application.DoEvents();
            using (Record record = new Record(4))
            {
                record.SetInteger(1, 0);
                record.SetInteger(2, totalStatements-1);
                record.SetInteger(3, 0);
                record.SetInteger(4, 0);
                session.Message(InstallMessage.Progress, record);
            }
            Application.DoEvents();
            using (Record record2 = new Record(3))
            {
                record2.SetInteger(1, 1);
                record2.SetInteger(2, 1);
                record2.SetInteger(3, 1);
                session.Message(InstallMessage.Progress, record2);
            }
            Application.DoEvents();
        }

        public static void ProgressBarMessage(Session session, string message)
        {
            using (Record actiondata = new Record(0))
            {
                actiondata.SetString(0, message);
                session.Message(InstallMessage.ActionData, actiondata);
                Application.DoEvents();
                Thread.Sleep(500);
            }
        }

        [CustomAction]
        public static ActionResult InstallService(Session session)
        {
            const int iNumberItems = 5;
            string path = string.Empty;

            try
            {
                ResetProgressBar(session, iNumberItems, "xInstallService", "Installing MFA Services...");
                try
                {
                    RegisterEventLogs();
                    path = GetInstallPath(session);
                    ProgressBarMessage(session, "installtion paths and Evenlog sources");
                }
                catch (Exception e)
                {
                    session.Log(e.Message);
                }
                if (!IsServiceInstalled(session, path + @"Neos.IdentityServer.MultiFactor.NotificationHub.exe"))
                {
                    ProgressBarMessage(session, "Installing MFA Service");
                    internalInstallService(session, path + @"Neos.IdentityServer.MultiFactor.NotificationHub.exe");
                }

                if (!IsSnapinInstalled(session, path + @"Neos.IdentityServer.Console.dll"))
                {
                    ProgressBarMessage(session, "Installing MFA Admin Console");
                    internalInstallSnapin(session, path + @"Neos.IdentityServer.Console.dll");
                }
                if (IsServiceInstalled(session, path + @"Neos.IdentityServer.MultiFactor.NotificationHub.exe"))
                {
                    ProgressBarMessage(session, "Starting MFA Service");
                    StartService(session, path + @"Neos.IdentityServer.MultiFactor.NotificationHub.exe");
                    ProgressBarMessage(session, "Removing Backup Files");
                }
                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log(e.Message);
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult UnInstallService(Session session)
        {
            const int iNumberItems = 5;
            string path = string.Empty;

            try
            {
                ResetProgressBar(session, iNumberItems, "xUnInstallService", "UnInstalling MFA Services...");
                try
                {
                    path = GetInstallPath(session);
                    ProgressBarMessage(session, "installtion paths");
                }
                catch (Exception e)
                {
                    session.Log(e.Message);
                }
                if (IsServiceInstalled(session, path + @"Neos.IdentityServer.MultiFactor.NotificationHub.exe"))
                {
                    ProgressBarMessage(session, "Stopping MFA Service");
                    if (StopService(session, path + @"Neos.IdentityServer.MultiFactor.NotificationHub.exe"))
                    {
                        ProgressBarMessage(session, "UnInstalling MFA Service");
                        internalUninstallService(session, path + @"Neos.IdentityServer.MultiFactor.NotificationHub.exe");
                    }
                }
                if (IsSnapinInstalled(session, path + @"Neos.IdentityServer.Console.dll"))
                {
                    ProgressBarMessage(session, "UnInstalling MFA Admin Console");
                    internalUninstallSnapin(session, path + @"Neos.IdentityServer.Console.dll");
                }
                return ActionResult.Success;
            }
            catch (Exception e)
            {
                session.Log(e.Message);
                return ActionResult.Failure;
            }
            finally
            {
                try
                {
                    ProgressBarMessage(session, "Removing Backup Files");
                    File.Delete(path + @"Config\Config.db");
                }
                catch (Exception e)
                {
                    session.Log(e.Message);
                }
            }
        }

        /// <summary>
        /// internalInstallService method implementation
        /// </summary>
        private static void internalInstallService(Session session, string exeFilename)
        {
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
        private static bool internalStartService()
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
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                ADFSController.Close();
            }
        }

        /// <summary>
        /// internalStopService method implementation
        /// </summary>
        private static bool internalStopService()
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
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                ADFSController.Close();
            }
        }

        /// <summary>
        /// IsServiceInstalled method implementation
        /// </summary>
        private static bool IsServiceInstalled(Session session, string exeFilename)
        {
            if (!File.Exists(exeFilename))
                return false;
            else
                return ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals("mfanotifhub"));
        }

        /// <summary>
        /// IsSnapinInstalled method implementation
        /// </summary>
        private static bool IsSnapinInstalled(Session session, string dllFilename)
        {
            string xx = @"Neos.IdentityServer.Console.ADFSSnapIn, Neos.IdentityServer.Console, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
            if (!File.Exists(dllFilename))
                return false;
            else
            {
                RegistryKey rkey64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                try
                {
                    RegistryKey key = rkey64.OpenSubKey(@"Software\Microsoft\MMC\Snapins\FX:{9627F1F3-A6D2-4cf8-90A2-10F85A7A4EE7}", RegistryKeyPermissionCheck.ReadSubTree);
                    try
                    {
                        object o = key.GetValue("Type");
                        if (o != null)
                        {
                            Assembly assembly = Assembly.LoadFile(dllFilename);
                            foreach (Type type in assembly.GetTypes())
                            {
                                if (type.IsDefined(typeof(SnapInSettingsAttribute)))
                                {
                                    SnapInSettingsAttribute attrib = (SnapInSettingsAttribute)type.GetCustomAttribute(typeof(SnapInSettingsAttribute), false);
                                    if (attrib != null)
                                    {
                                        return (type.AssemblyQualifiedName.ToLower().Equals(xx.ToLower()));
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    finally
                    {
                        key.Close();
                    }
                }
                catch 
                {
                    return false;
                }
                finally
                {
                    rkey64.Close();
                }
            }
            return false;
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
