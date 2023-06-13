//******************************************************************************************************************************************************************************************//
// Copyright (c) 2023 redhook (adfsmfa@gmail.com)                                                                                                                                        //                        
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
//                                                                                                                                                                                          //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
// #define debugsid
using Microsoft.Win32;
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace Neos.IdentityServer.MultiFactor
{
    #region SIDs
    /// <summary>
    /// SIDs Class implementation
    /// </summary>
    internal static class SIDs
    {
        internal static uint STATUS_SUCCESS = 0;
        internal static uint STATUS_BUFFER_TOO_SMALL = 3221225507;
        internal static bool Loaded { get; private set; } = false;
        internal static bool ADFSSystemServiceAdministrationAllowed { get; set; }
        internal static bool ADFSLocalAdminServiceAdministrationAllowed { get; set; }
        internal static bool ADFSDelegateServiceAdministrationAllowed { get; set; }
        internal static bool ADFSDomainAdminServiceAdministrationAllowed { get; set; }
        internal static string ADFSAccountSID { get; private set; } = string.Empty;
        internal static string ADFSServiceSID { get; private set; } = string.Empty;
        internal static string ADFSAdminGroupSID { get; private set; } = string.Empty;
        internal static string ADFSAccountName { get; private set; } = string.Empty;
        internal static string ADFSServiceName { get; private set; } = string.Empty;
        internal static string ADFSAdminGroupName { get; private set; } = string.Empty;


        #region SIDs
        /// <summary>
        /// Initialize method implmentation
        /// </summary>
        internal static SIDsParametersRecord Initialize()
        {            
            SIDsParametersRecord rec = new SIDsParametersRecord();
            if (!Loaded)
            {
                try
                {
#if debugsid
                    Log.WriteEntry("SID Initialize Loading on Server", EventLogEntryType.Warning, 9200);
#endif
                    if (!File.Exists(SystemUtilities.SystemCacheFile))
                    {
#if debugsid
                        Log.WriteEntry("SID Initialize Loading without cache on Server", EventLogEntryType.Warning, 9201);
#endif
                        rec.Loaded = GetADFSAccounts(ref rec);
                        ADFSAccountSID = rec.ADFSAccountSID;
                        ADFSAccountName = rec.ADFSAccountName;
                        ADFSServiceSID = rec.ADFSServiceAccountSID;
                        ADFSServiceName = rec.ADFSServiceAccountName;
                        ADFSAdminGroupSID = rec.ADFSAdministrationGroupSID;
                        ADFSAdminGroupName = rec.ADFSAdministrationGroupName;
                        ADFSDelegateServiceAdministrationAllowed = rec.ADFSDelegateServiceAdministrationAllowed;
                        ADFSLocalAdminServiceAdministrationAllowed = rec.ADFSLocalAdminServiceAdministrationAllowed;
                        ADFSSystemServiceAdministrationAllowed = rec.ADFSSystemServiceAdministrationAllowed;
#if debugsid
                        Log.WriteEntry("SID Initialize SIDs are correctly loaded on Server", EventLogEntryType.Warning, 9202);
#endif
                        WriteToCache(rec);
#if debugsid
                        Log.WriteEntry("SID Initialize System cache file correctly saved on Server", EventLogEntryType.Warning, 9203);
#endif
                        Loaded = true;
                    }
                    else
                    {
#if debugsid
                        Log.WriteEntry("SID Initialize Loading with cache on Server", EventLogEntryType.Warning, 9204);
#endif
                        SIDsParametersRecord data = LoadFromCache();
                        if (data != null)
                        {
#if debugsid
                            Log.WriteEntry("SID Initialize SIDs cache file correctly Loaded on Server", EventLogEntryType.Warning, 9205);
#endif
                            rec.ADFSAccountSID = data.ADFSAccountSID;
                            rec.ADFSAccountName = data.ADFSAccountName;
                            rec.ADFSServiceAccountSID = data.ADFSServiceAccountSID;
                            rec.ADFSServiceAccountName = data.ADFSServiceAccountName;
                            rec.ADFSAdministrationGroupSID = data.ADFSAdministrationGroupSID;
                            rec.ADFSAdministrationGroupName = data.ADFSAdministrationGroupName;
                            rec.ADFSDelegateServiceAdministrationAllowed = data.ADFSDelegateServiceAdministrationAllowed;
                            rec.ADFSLocalAdminServiceAdministrationAllowed = data.ADFSLocalAdminServiceAdministrationAllowed;
                            rec.ADFSSystemServiceAdministrationAllowed = data.ADFSSystemServiceAdministrationAllowed;
                            rec.Loaded = true;

                            ADFSAccountSID = data.ADFSAccountSID;
                            ADFSAccountName = data.ADFSAccountName;
                            ADFSServiceSID = data.ADFSServiceAccountSID;
                            ADFSServiceName = data.ADFSServiceAccountName;
                            ADFSAdminGroupSID = data.ADFSAdministrationGroupSID;
                            ADFSAdminGroupName = data.ADFSAdministrationGroupName;
                            ADFSDelegateServiceAdministrationAllowed = data.ADFSDelegateServiceAdministrationAllowed;
                            ADFSLocalAdminServiceAdministrationAllowed = data.ADFSLocalAdminServiceAdministrationAllowed;
                            ADFSSystemServiceAdministrationAllowed = data.ADFSSystemServiceAdministrationAllowed;
#if debugsid
                            Log.WriteEntry("SID Initialize SIDs correctly retreived on Server", EventLogEntryType.Warning, 9206);
#endif
                            Loaded = true;
                        }
                        else
                        {
#if debugsid
                            Log.WriteEntry("SID Initialize SIDs Not loaded on Server", EventLogEntryType.Error, 9207);
#endif
                            Loaded = false;
                        }
                    }
                }
                catch (Exception ex)
                {
#if debugsid
                    Log.WriteEntry(string.Format("SID Initialize SIDs ERROR on Server {0}", ex.Message), EventLogEntryType.Error, 9208);
#endif
                    Loaded = false;
                }
            }
            else
            {
#if debugsid
                Log.WriteEntry("SID Initialize SIDs are loaded on Server", EventLogEntryType.Warning, 9209);
#endif
                try
                {
                    if (!File.Exists(SystemUtilities.SystemCacheFile))
                    {
#if debugsid
                        Log.WriteEntry("SID Initialize SIDs loaded creating the file cache on Server", EventLogEntryType.Warning, 9210);
#endif
                        rec.Loaded = GetADFSAccounts(ref rec);
                        ADFSAccountSID = rec.ADFSAccountSID;
                        ADFSAccountName = rec.ADFSAccountName;
                        ADFSServiceSID = rec.ADFSServiceAccountSID;
                        ADFSServiceName = rec.ADFSServiceAccountName;
                        ADFSAdminGroupSID = rec.ADFSAdministrationGroupSID;
                        ADFSAdminGroupName = rec.ADFSAdministrationGroupName;
                        ADFSDelegateServiceAdministrationAllowed = rec.ADFSDelegateServiceAdministrationAllowed;
                        ADFSLocalAdminServiceAdministrationAllowed = rec.ADFSLocalAdminServiceAdministrationAllowed;
                        ADFSSystemServiceAdministrationAllowed = rec.ADFSSystemServiceAdministrationAllowed;
                        WriteToCache(rec);
#if debugsid
                        Log.WriteEntry("SID Initialize System cache file correctly saved on Server", EventLogEntryType.Warning, 9211);
#endif
                        Loaded = true;
                    }
                    else
                    {
#if debugsid
                        Log.WriteEntry("SID Initialize SIDs loaded reading memory values on Server", EventLogEntryType.Warning, 9212);
#endif
                        rec.ADFSAccountSID = ADFSAccountSID;
                        rec.ADFSAccountName = ADFSAccountName;
                        rec.ADFSServiceAccountSID = ADFSServiceSID;
                        rec.ADFSServiceAccountName = ADFSServiceName;
                        rec.ADFSAdministrationGroupSID = ADFSAdminGroupSID;
                        rec.ADFSAdministrationGroupName = ADFSAdminGroupName;
                        rec.ADFSDelegateServiceAdministrationAllowed = ADFSDelegateServiceAdministrationAllowed;
                        rec.ADFSLocalAdminServiceAdministrationAllowed = ADFSLocalAdminServiceAdministrationAllowed;
                        rec.ADFSSystemServiceAdministrationAllowed = ADFSSystemServiceAdministrationAllowed;
                        rec.Loaded = true;
                    }
                }
                catch (Exception ex)
                {
#if debugsid
                    Log.WriteEntry(string.Format("SID Initialize SIDs ERROR on Server {0}", ex.Message), EventLogEntryType.Error, 9213);
#endif
                    Loaded = true;
                }
            }
#if debugsid
            Log.WriteEntry(string.Format("SID Initialize ADFSAccountSID : {0}", ADFSAccountSID), EventLogEntryType.SuccessAudit, 9301);
            Log.WriteEntry(string.Format("SID Initialize ADFSAccountName : {0}", ADFSAccountName), EventLogEntryType.SuccessAudit, 9302);
            Log.WriteEntry(string.Format("SID Initialize ADFSAccountSID : {0}", ADFSServiceSID), EventLogEntryType.SuccessAudit, 9303);
            Log.WriteEntry(string.Format("SID Initialize ADFSServiceName : {0}", ADFSServiceName), EventLogEntryType.SuccessAudit, 9304);
            Log.WriteEntry(string.Format("SID Initialize ADFSAdminGroupSID : {0}", ADFSAdminGroupSID), EventLogEntryType.SuccessAudit, 9305);
            Log.WriteEntry(string.Format("SID Initialize ADFSAdminGroupName : {0}", ADFSAdminGroupName), EventLogEntryType.SuccessAudit, 9306);
            Log.WriteEntry(string.Format("SID Initialize ADFSDelegateServiceAdministrationAllowed : {0}", ADFSDelegateServiceAdministrationAllowed), EventLogEntryType.SuccessAudit, 9307);
            Log.WriteEntry(string.Format("SID Initialize ADFSLocalAdminServiceAdministrationAllowed : {0}", ADFSLocalAdminServiceAdministrationAllowed), EventLogEntryType.SuccessAudit, 9308);
            Log.WriteEntry(string.Format("SID Initialize ADFSSystemServiceAdministrationAllowed : {0}", ADFSSystemServiceAdministrationAllowed), EventLogEntryType.SuccessAudit, 9309);
#endif
            return rec;
        }

        /// <summary>
        /// GetSIDs method implementation
        /// </summary>
        /// <returns></returns>
        internal static SIDsParametersRecord GetSIDs()
        {
            SIDsParametersRecord rec = new SIDsParametersRecord() { Loaded = false };
            if (!Loaded)
                return null;
            rec.ADFSAccountSID = ADFSAccountSID;
            rec.ADFSAccountName = ADFSAccountName;
            rec.ADFSServiceAccountSID = ADFSServiceSID;
            rec.ADFSServiceAccountName = ADFSServiceName;
            rec.ADFSAdministrationGroupSID = ADFSAdminGroupSID;
            rec.ADFSAdministrationGroupName = ADFSAdminGroupName;
            rec.ADFSDelegateServiceAdministrationAllowed = ADFSDelegateServiceAdministrationAllowed;
            rec.ADFSLocalAdminServiceAdministrationAllowed = ADFSLocalAdminServiceAdministrationAllowed;
            rec.ADFSSystemServiceAdministrationAllowed = ADFSSystemServiceAdministrationAllowed;
            rec.ADFSDomainAdminServiceAdministrationAllowed = ADFSDomainAdminServiceAdministrationAllowed;
            rec.Loaded = true;
#if debugsid
            Log.WriteEntry(string.Format("SID GetSIDs ADFSAccountSID : {0}", ADFSAccountSID), EventLogEntryType.SuccessAudit, 9301);
            Log.WriteEntry(string.Format("SID GetSIDs ADFSAccountName : {0}", ADFSAccountName), EventLogEntryType.SuccessAudit, 9302);
            Log.WriteEntry(string.Format("SID GetSIDs ADFSAccountSID : {0}", ADFSServiceSID), EventLogEntryType.SuccessAudit, 9303);
            Log.WriteEntry(string.Format("SID GetSIDs ADFSServiceName : {0}", ADFSServiceName), EventLogEntryType.SuccessAudit, 9304);
            Log.WriteEntry(string.Format("SID GetSIDs ADFSAdminGroupSID : {0}", ADFSAdminGroupSID), EventLogEntryType.SuccessAudit, 9305);
            Log.WriteEntry(string.Format("SID GetSIDs ADFSAdminGroupName : {0}", ADFSAdminGroupName), EventLogEntryType.SuccessAudit, 9306);
            Log.WriteEntry(string.Format("SID GetSIDs ADFSDelegateServiceAdministrationAllowed : {0}", ADFSDelegateServiceAdministrationAllowed), EventLogEntryType.SuccessAudit, 9307);
            Log.WriteEntry(string.Format("SID GetSIDs ADFSLocalAdminServiceAdministrationAllowed : {0}", ADFSLocalAdminServiceAdministrationAllowed), EventLogEntryType.SuccessAudit, 9308);
            Log.WriteEntry(string.Format("SID GetSIDs ADFSSystemServiceAdministrationAllowed : {0}", ADFSSystemServiceAdministrationAllowed), EventLogEntryType.SuccessAudit, 9309);
#endif
            return rec;
        }


        /// <summary>
        /// Assign method implmentation
        /// </summary>
        internal static void Assign(SIDsParametersRecord rec)
        {
            ADFSAccountSID = rec.ADFSAccountSID;
            ADFSAccountName = rec.ADFSAccountName;
            ADFSServiceSID = rec.ADFSServiceAccountSID;
            ADFSServiceName = rec.ADFSServiceAccountName;
            ADFSAdminGroupSID = rec.ADFSAdministrationGroupSID;
            ADFSAdminGroupName = rec.ADFSAdministrationGroupName;
            ADFSDelegateServiceAdministrationAllowed = rec.ADFSDelegateServiceAdministrationAllowed;
            ADFSLocalAdminServiceAdministrationAllowed = rec.ADFSLocalAdminServiceAdministrationAllowed;
            ADFSSystemServiceAdministrationAllowed = rec.ADFSSystemServiceAdministrationAllowed;
            ADFSDomainAdminServiceAdministrationAllowed = rec.ADFSDomainAdminServiceAdministrationAllowed;
            WriteToCache(rec);
#if debugsid
            Log.WriteEntry(string.Format("SID Assign ADFSAccountSID : {0}", ADFSAccountSID), EventLogEntryType.SuccessAudit, 9301);
            Log.WriteEntry(string.Format("SID Assign ADFSAccountName : {0}", ADFSAccountName), EventLogEntryType.SuccessAudit, 9302);
            Log.WriteEntry(string.Format("SID Assign ADFSAccountSID : {0}", ADFSServiceSID), EventLogEntryType.SuccessAudit, 9303);
            Log.WriteEntry(string.Format("SID Assign ADFSServiceName : {0}", ADFSServiceName), EventLogEntryType.SuccessAudit, 9304);
            Log.WriteEntry(string.Format("SID Assign ADFSAdminGroupSID : {0}", ADFSAdminGroupSID), EventLogEntryType.SuccessAudit, 9305);
            Log.WriteEntry(string.Format("SID Assign ADFSAdminGroupName : {0}", ADFSAdminGroupName), EventLogEntryType.SuccessAudit, 9306);
            Log.WriteEntry(string.Format("SID Assign ADFSDelegateServiceAdministrationAllowed : {0}", ADFSDelegateServiceAdministrationAllowed), EventLogEntryType.SuccessAudit, 9307);
            Log.WriteEntry(string.Format("SID Assign ADFSLocalAdminServiceAdministrationAllowed : {0}", ADFSLocalAdminServiceAdministrationAllowed), EventLogEntryType.SuccessAudit, 9308);
            Log.WriteEntry(string.Format("SID Assign ADFSSystemServiceAdministrationAllowed : {0}", ADFSSystemServiceAdministrationAllowed), EventLogEntryType.SuccessAudit, 9309);
#endif

            Loaded = rec.Loaded;
        }

        /// <summary>
        /// Clear method implementation
        /// </summary>
        internal static void Clear()
        {
            ADFSAccountSID = string.Empty;
            ADFSAccountName = string.Empty;
            ADFSServiceSID = string.Empty;
            ADFSServiceName = string.Empty;
            ADFSAdminGroupSID = string.Empty;
            ADFSAdminGroupName = string.Empty;
            ADFSDelegateServiceAdministrationAllowed = false;
            ADFSLocalAdminServiceAdministrationAllowed = false;
            ADFSSystemServiceAdministrationAllowed = false;
            ADFSDomainAdminServiceAdministrationAllowed = false;
            Loaded = false;
        }

        /// <summary>
        /// LoadFromCache method implementation
        /// </summary>
        private static SIDsParametersRecord LoadFromCache()
        {
            SIDsParametersRecord config = null;
            if (!File.Exists(SystemUtilities.SystemCacheFile))
                return null;
            DataContractSerializer serializer = new DataContractSerializer(typeof(SIDsParametersRecord));
            using (FileStream fs = new FileStream(SystemUtilities.SystemCacheFile, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fs.Length];
                int n = fs.Read(bytes, 0, (int)fs.Length);
                fs.Close();

                byte[] byt = null;
                using (AESSystemEncryption aes = new AESSystemEncryption())
                {
                    byt = aes.Decrypt(bytes);
                }
                using (MemoryStream ms = new MemoryStream(byt))
                {
                    using (StreamReader reader = new StreamReader(ms))
                    {
                        config = (SIDsParametersRecord)serializer.ReadObject(ms);
                    }
                }
            }
            return config;
        }

        /// <summary>
        /// WriteToCache method implementation
        /// </summary>
        private static void WriteToCache(SIDsParametersRecord config)
        {
            if (config.ADFSDelegateServiceAdministrationAllowed)
                StoreDelegatedGroupStatus(config.ADFSAdministrationGroupName);
            else
                StoreDelegatedGroupStatus(null);
            DataContractSerializer serializer = new DataContractSerializer(typeof(SIDsParametersRecord));
            MemoryStream stm = new MemoryStream();
            using (StreamReader reader = new StreamReader(stm))
            {
                serializer.WriteObject(stm, config);
                stm.Position = 0;
                byte[] byt = null;
                using (AESSystemEncryption aes = new AESSystemEncryption())
                {
                    byt = aes.Encrypt(stm.ToArray());
                }
                using (FileStream fs = new FileStream(SystemUtilities.SystemCacheFile, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Write(byt, 0, byt.Length);
                    fs.Close();
                }
                return;
            }
        }

        /// <summary>
        /// InternalUpdateSystemFilesACLs method implementation
        /// </summary>
        internal static void InternalUpdateSystemFilesACLs(string fullpath, bool fulltosystemonly = false)
        {
            if (!Loaded)
                Initialize();

            FileSecurity fSecurity = File.GetAccessControl(fullpath, AccessControlSections.Access);
            fSecurity.SetAccessRuleProtection(true, false);

            SecurityIdentifier localsys = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            fSecurity.PurgeAccessRules(localsys);
            fSecurity.AddAccessRule(new FileSystemAccessRule(localsys, FileSystemRights.FullControl, AccessControlType.Allow));

            SecurityIdentifier localacc = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            fSecurity.PurgeAccessRules(localacc);
            if (!fulltosystemonly)
                fSecurity.AddAccessRule(new FileSystemAccessRule(localacc, FileSystemRights.FullControl, AccessControlType.Allow));
            else
                fSecurity.AddAccessRule(new FileSystemAccessRule(localacc, FileSystemRights.Read, AccessControlType.Allow));
            if (ADFSDomainAdminServiceAdministrationAllowed)
            {
                SecurityIdentifier adfsacc = new SecurityIdentifier(WellKnownSidType.AccountDomainAdminsSid, WindowsIdentity.GetCurrent().User.AccountDomainSid);
                fSecurity.PurgeAccessRules(adfsacc);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsacc, FileSystemRights.FullControl, AccessControlType.Allow));
            }
            if (!string.IsNullOrEmpty(ADFSAccountSID))
            {
                SecurityIdentifier adfsacc = new SecurityIdentifier(ADFSAccountSID);
                fSecurity.PurgeAccessRules(adfsacc);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsacc, FileSystemRights.Read, AccessControlType.Allow));
            }
            if (!string.IsNullOrEmpty(ADFSServiceSID))
            {
                SecurityIdentifier adfsserv = new SecurityIdentifier(ADFSServiceSID);
                fSecurity.PurgeAccessRules(adfsserv);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsserv, FileSystemRights.Read, AccessControlType.Allow));
            }
            if (!string.IsNullOrEmpty(ADFSAdminGroupSID))
            {
                SecurityIdentifier adfsgroup = new SecurityIdentifier(ADFSAdminGroupSID);
                fSecurity.PurgeAccessRules(adfsgroup);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsgroup, FileSystemRights.Read, AccessControlType.Allow));
            }
            File.SetAccessControl(fullpath, fSecurity);
        }

        /// <summary>
        /// InternalUpdateACLs method implementation
        /// </summary>
        internal static void InternalUpdateFilesACLs(string fullpath)
        {
            if (!Loaded)
                Initialize();

            FileSecurity fSecurity = File.GetAccessControl(fullpath, AccessControlSections.Access);
            fSecurity.SetAccessRuleProtection(true, false);

            SecurityIdentifier localsys = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            fSecurity.PurgeAccessRules(localsys);
            fSecurity.AddAccessRule(new FileSystemAccessRule(localsys, FileSystemRights.FullControl, AccessControlType.Allow));

            SecurityIdentifier localacc = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            fSecurity.PurgeAccessRules(localacc);
            fSecurity.AddAccessRule(new FileSystemAccessRule(localacc, FileSystemRights.FullControl, AccessControlType.Allow));
            if (ADFSDomainAdminServiceAdministrationAllowed)
            {
                SecurityIdentifier adfsacc = new SecurityIdentifier(WellKnownSidType.AccountDomainAdminsSid, WindowsIdentity.GetCurrent().User.AccountDomainSid);
                fSecurity.PurgeAccessRules(adfsacc);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsacc, FileSystemRights.FullControl, AccessControlType.Allow));
            }
            if (!string.IsNullOrEmpty(ADFSAccountSID))
            {
                SecurityIdentifier adfsacc = new SecurityIdentifier(ADFSAccountSID);
                fSecurity.PurgeAccessRules(adfsacc);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsacc, FileSystemRights.FullControl, AccessControlType.Allow));
            }
            if (!string.IsNullOrEmpty(ADFSServiceSID))
            {
                SecurityIdentifier adfsserv = new SecurityIdentifier(ADFSServiceSID);
                fSecurity.PurgeAccessRules(adfsserv);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsserv, FileSystemRights.FullControl, AccessControlType.Allow));
            }
            if (!string.IsNullOrEmpty(ADFSAdminGroupSID))
            {
                SecurityIdentifier adfsgroup = new SecurityIdentifier(ADFSAdminGroupSID);
                fSecurity.PurgeAccessRules(adfsgroup);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsgroup, FileSystemRights.FullControl, AccessControlType.Allow));
            }
            File.SetAccessControl(fullpath, fSecurity);
        }

        /// <summary>
        /// InternalUpdateDirectoryACLs method implementation
        /// </summary>
        internal static void InternalUpdateDirectoryACLs(string fulldir)
        {
            if (!Loaded)
                Initialize();

            DirectorySecurity fSecurity = Directory.GetAccessControl(fulldir, AccessControlSections.Access);

            SecurityIdentifier localacc = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            fSecurity.PurgeAccessRules(localacc);
            fSecurity.AddAccessRule(new FileSystemAccessRule(localacc, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            fSecurity.AddAccessRule(new FileSystemAccessRule(localacc, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
            fSecurity.AddAccessRule(new FileSystemAccessRule(localacc, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));

            if (ADFSDomainAdminServiceAdministrationAllowed)
            {
                SecurityIdentifier domainacc = new SecurityIdentifier(WellKnownSidType.AccountDomainAdminsSid, WindowsIdentity.GetCurrent().User.AccountDomainSid);
                fSecurity.PurgeAccessRules(domainacc);
                fSecurity.AddAccessRule(new FileSystemAccessRule(domainacc, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
                fSecurity.AddAccessRule(new FileSystemAccessRule(domainacc, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                fSecurity.AddAccessRule(new FileSystemAccessRule(domainacc, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
            }
            if (!string.IsNullOrEmpty(ADFSAdminGroupSID))
            {
                SecurityIdentifier adfsgroup = new SecurityIdentifier(ADFSAdminGroupSID);
                fSecurity.PurgeAccessRules(adfsgroup);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsgroup, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsgroup, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsgroup, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
            }
            if (!string.IsNullOrEmpty(ADFSAccountSID))
            {
                SecurityIdentifier adfsaccount = new SecurityIdentifier(ADFSAccountSID);
                fSecurity.PurgeAccessRules(adfsaccount);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsaccount, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsaccount, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsaccount, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
            }
            Directory.SetAccessControl(fulldir, fSecurity);
        }

        /// <summary>
        /// InternalUpdateCertificatesACLs method implementation
        /// </summary>
        internal static bool InternalUpdateCertificatesACLs(KeyMgtOptions options = KeyMgtOptions.AllCerts)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.MaxAllowed);
            try
            {
                X509Certificate2Collection collection2 = (X509Certificate2Collection)store.Certificates;
                foreach (X509Certificate2 x509 in collection2)
                {
                    string fileName = string.Empty;
                    try
                    {
                        bool doit = options.Equals(KeyMgtOptions.AllCerts);
                        if (options.HasFlag(KeyMgtOptions.MFACerts))
                        {
                            if (x509.Subject.ToLower().StartsWith("cn=mfa rsa keys") || x509.Subject.ToLower().StartsWith("cn=mfa sql key"))
                                doit = true;
                        }
                        if (options.HasFlag(KeyMgtOptions.ADFSCerts))
                        {
                            if (x509.Subject.ToLower().StartsWith("cn=adfs"))
                                doit = true;
                        }
                        if (options.HasFlag(KeyMgtOptions.SSLCerts))
                        {
                            if (x509.Subject.ToLower().StartsWith("cn=*."))
                                doit = true;
                        }
                        if (doit)
                        {
                            RSA rsax = x509.GetRSAPrivateKey();
                            if (rsax is RSACng cng)
                            {
                                fileName = cng.Key.UniqueName;
                            }
                            else if (rsax is RSACryptoServiceProvider provider)
                            {
                                fileName = provider.CspKeyContainerInfo.UniqueKeyContainerName;
                            }
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                char sep = Path.DirectorySeparatorChar;
                                string rsamachinefullpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + sep + "Microsoft" + sep + "Crypto" + sep + "RSA" + sep + "MachineKeys" + sep + fileName;
                               // string rngmachinefullpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + sep + "Microsoft" + sep + "Crypto" + sep + "Keys" + sep + fileName;

                                if (File.Exists(rsamachinefullpath))
                                {
                                    InternalUpdateFilesACLs(rsamachinefullpath);
                                }
                               /* if (File.Exists(rngmachinefullpath))
                                {
                                    InternalUpdateFilesACLs(rngmachinefullpath);
                                } */
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteEntry("Error Updating certificates ACLs : \r" + ex.Message, EventLogEntryType.Error, 667);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                store.Close();
            }
            return true;
        }

        /// <summary>
        /// GetADFSAccounts method implmentation
        /// </summary>
        private static bool GetADFSAccounts(ref SIDsParametersRecord rec)
        {
            RegistryVersion reg = new RegistryVersion();
            ADFSAdminPolicies pol = new ADFSAdminPolicies();
            try
            {
                rec.ADFSAccountSID = GetADFSAccountSID();
                rec.ADFSAccountName = GetADFSAccountName();
                rec.ADFSServiceAccountSID = GetADFSServiceSID();
                rec.ADFSServiceAccountName = GetADFSServiceName();
                if (!reg.IsWindows2012R2)
                {
                    rec.ADFSAdministrationGroupName = GetADFSAdminsGroupName(ref pol);
                    rec.ADFSAdministrationGroupSID = GetADFSAdminsGroupSID(rec.ADFSAdministrationGroupName);
                    rec.ADFSDelegateServiceAdministrationAllowed = pol.DelegateServiceAdministrationAllowed;
                    rec.ADFSLocalAdminServiceAdministrationAllowed = pol.LocalAdminsServiceAdministrationAllowed;
                    rec.ADFSSystemServiceAdministrationAllowed = pol.SystemServiceAdministrationAllowed;                    
                }
                else
                {
                    rec.ADFSAdministrationGroupName = string.Empty;
                    rec.ADFSAdministrationGroupSID = string.Empty;
                    rec.ADFSDelegateServiceAdministrationAllowed = false;
                    rec.ADFSLocalAdminServiceAdministrationAllowed = true;
                    rec.ADFSSystemServiceAdministrationAllowed = true;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteEntry("Error loading Security informations : \r" + ex.Message, EventLogEntryType.Error, 666);
                return false;
            }
        }

        /// <summary>
        /// GetADFSServiceName method implmentation
        /// </summary>
        private static string GetADFSServiceName()
        {
            return "adfssrv";
        }

        /// <summary>
        /// GetADFSServiceSID method implmentation
        /// </summary>
        private static string GetADFSServiceSID()
        {
            NativeMethods.LSA_UNICODE_STRING lSA_UNICODE_STRING = default;
            lSA_UNICODE_STRING.SetTo("adfssrv");
            int cb = 0;
            try
            {
                uint num = NativeMethods.RtlCreateServiceSid(ref lSA_UNICODE_STRING, IntPtr.Zero, ref cb);
                if (num == STATUS_BUFFER_TOO_SMALL)
                {
                    IntPtr intPtr = Marshal.AllocHGlobal(cb);
                    try
                    {
                        if (NativeMethods.RtlCreateServiceSid(ref lSA_UNICODE_STRING, intPtr, ref cb) != STATUS_SUCCESS)
                           throw new Win32Exception(Marshal.GetLastWin32Error());
                        return new SecurityIdentifier(intPtr).Value;
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(intPtr);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry("Error loading ADFS Service SID information : \r" + ex.Message, EventLogEntryType.Error, 666);
                return string.Empty;
            }
            finally
            {
                lSA_UNICODE_STRING.Dispose();
            }
            return string.Empty;
        }

        /// <summary>
        /// GetADFSAccountSID() method implmentation
        /// </summary>
        private static string GetADFSAccountSID()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\adfssrv", false);
            try
            {
                if (key != null)
                {
                    string username = key.GetValue("ObjectName").ToString();
                    SecurityIdentifier sid;
                    NTAccount ntaccount;
                    try
                    {
                        ntaccount = new NTAccount(username);
                        sid = (SecurityIdentifier)ntaccount.Translate(typeof(SecurityIdentifier));
                        return sid.Value;
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                Log.WriteEntry("Error loading ADFS Account SID information : \r" + ex.Message, EventLogEntryType.Error, 666);
                return string.Empty;
            }
            finally
            {
                key.Close();
            }
        }

        /// <summary>
        /// GetADFSAccountName() method implmentation
        /// </summary>
        private static string GetADFSAccountName()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\adfssrv", false);
            try
            {
                if (key != null)
                {
                    string username = key.GetValue("ObjectName").ToString();
                    NTAccount ntaccount;
                    try
                    {
                        ntaccount = new NTAccount(username);
                        return ntaccount.Value;
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                Log.WriteEntry("Error loading ADFS Account Name : \r" + ex.Message, EventLogEntryType.Error, 666);
                return string.Empty;
            }
            finally
            {
                key.Close();
            }
        }

        /// <summary>
        /// GetADFSAdminsGroupSID() method implmentation
        /// </summary>
        private static string GetADFSAdminsGroupSID(string admingroupname)
        {
            try
            {
                if (!string.IsNullOrEmpty(admingroupname))
                {
                    PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                    GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, admingroupname);
                    if (group != null)
                    {
                        SecurityIdentifier sid = group.Sid;
                        ADFSAdminGroupSID = sid.Value;
                        return sid.Value;
                    }
                    else
                        return string.Empty;
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                Log.WriteEntry("Error loading ADFS Administration Group SID : \r" + ex.Message, EventLogEntryType.Error, 666);
                return string.Empty;
            }
        }

        /// <summary>
        /// GetADFSAdminsGroupName() method implmentation
        /// </summary>
        private static string GetADFSAdminsGroupName(ref ADFSAdminPolicies tuple)
        {
            try
            {
                return GetADFSServiceAdministrationProperties(ref tuple);
            }
            catch (Exception ex)
            {
                Log.WriteEntry("Error loading ADFS Administration Group Name : \r" + ex.Message, EventLogEntryType.Error, 666);
                return string.Empty;
            }
        }

        /// <summary>
        /// GetADFSServiceAdministrationProperties method implmentation
        /// </summary>
        private static string GetADFSServiceAdministrationProperties(ref ADFSAdminPolicies tuple)
        {
            Runspace SPRunSpace = null;
            PowerShell SPPowerShell = null;
            string grpname = string.Empty;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);

                SPPowerShell = PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command exportcmd = new Command("Get-AdfsProperties | Select-Object -Property DelegateServiceAdministration, AllowSystemServiceAdministration, AllowLocalAdminsServiceAdministration", true);
                pipeline.Commands.Add(exportcmd);
                Collection<PSObject> PSOutput = pipeline.Invoke();
                foreach (var result in PSOutput)
                {
                    object objgrpname = result.Properties["DelegateServiceAdministration"].Value;
                    if (objgrpname != null)
                        grpname = objgrpname.ToString();
                    else
                        grpname = string.Empty;
                    bool sysok = Convert.ToBoolean(result.Properties["AllowSystemServiceAdministration"].Value);
                    bool admok = Convert.ToBoolean(result.Properties["AllowLocalAdminsServiceAdministration"].Value);
                    tuple.DelegateServiceAdministrationAllowed = (!string.IsNullOrEmpty(grpname));
                    tuple.SystemServiceAdministrationAllowed = sysok;
                    tuple.LocalAdminsServiceAdministrationAllowed = admok;
                    return grpname.ToLower();
                }
            }
            catch (Exception)
            {
                grpname = string.Empty;
            }
            finally
            {
                SPRunSpace?.Close();
                SPPowerShell?.Dispose();
            }
            return grpname;
        }

        /// <summary>
        /// StoreDelegatedGroupStatus method implementation
        /// </summary>
        private static void StoreDelegatedGroupStatus(string delegatedgroup)
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("Software\\MFA", true);
            if (string.IsNullOrEmpty(delegatedgroup))
            {
                rk.DeleteValue("DelegatedAdminGroup", false);
                return;
            }
            object obj = rk.GetValue("DelegatedAdminGroup");
            if (obj == null)
                rk.SetValue("DelegatedAdminGroup", delegatedgroup, RegistryValueKind.String);
            else if (obj.ToString().ToLower().Equals(delegatedgroup.ToLower()))
                rk.SetValue("DelegatedAdminGroup", delegatedgroup, RegistryValueKind.String);
        }

        /// <summary>
        /// NativeMethod implementation
        /// The class exposes Windows APIs.
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        internal class NativeMethods
        {
            [DllImport("ntdll.dll", SetLastError = true)]
            internal static extern uint RtlCreateServiceSid(ref NativeMethods.LSA_UNICODE_STRING serviceName, IntPtr serviceSid, ref int serviceSidLength);

            internal struct LSA_UNICODE_STRING : IDisposable
            {
                public ushort Length;
                public ushort MaximumLength;
                public IntPtr Buffer;

                public void SetTo(string str)
                {
                    this.Buffer = Marshal.StringToHGlobalUni(str);
                    this.Length = (ushort)(str.Length * 2);
                    this.MaximumLength = Convert.ToUInt16(this.Length + 2);
                }

                public override string ToString()
                {
                    return Marshal.PtrToStringUni(this.Buffer);
                }

                public void Reset()
                {
                    if (this.Buffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(this.Buffer);
                    }
                    this.Buffer = IntPtr.Zero;
                    this.Length = 0;
                    this.MaximumLength = 0;
                }

                public void Dispose()
                {
                    this.Reset();
                }
            }
        }

        /// <summary>
        /// ADFSAdminPolicies class
        /// </summary>
        private class ADFSAdminPolicies
        {
            public bool DelegateServiceAdministrationAllowed = false;
            public bool SystemServiceAdministrationAllowed = false;
            public bool LocalAdminsServiceAdministrationAllowed = false;
        }
        #endregion
    }
    #endregion
}
