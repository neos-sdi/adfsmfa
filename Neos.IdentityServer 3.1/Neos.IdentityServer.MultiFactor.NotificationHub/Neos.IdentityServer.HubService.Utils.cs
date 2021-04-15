#define test
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
        internal static bool Loaded { get; private set; } = false;
        internal static bool ADFSSystemServiceAdministrationAllowed { get; set; }
        internal static bool ADFSLocalAdminServiceAdministrationAllowed { get; set; }
        internal static bool ADFSDelegateServiceAdministrationAllowed { get; set; }
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
                    if (!File.Exists(SystemUtilities.SystemCacheFile))
                    {
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
                        Loaded = true;
                    }
                    else
                    {
                        SIDsParametersRecord data = LoadFromCache();
                        if (data != null)
                        {
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
                            Loaded = true;
                        }
                        else
                            Loaded = false;
                    }
                }
                catch (Exception)
                {
                    Loaded = false;
                }
            }
            else
            {
                try
                {
                    if (!File.Exists(SystemUtilities.SystemCacheFile))
                    {
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
                        Loaded = true;
                    }
                    else
                    {
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
                catch (Exception)
                {
                    Loaded = true;
                }
            }
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
            rec.Loaded = true;
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
            WriteToCache(rec);
            Loaded = rec.Loaded;
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

            bool mustsave = false;
            DirectorySecurity fSecurity = Directory.GetAccessControl(fulldir, AccessControlSections.Access);

            SecurityIdentifier localacc = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            fSecurity.PurgeAccessRules(localacc);
            fSecurity.AddAccessRule(new FileSystemAccessRule(localacc, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            fSecurity.AddAccessRule(new FileSystemAccessRule(localacc, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
            fSecurity.AddAccessRule(new FileSystemAccessRule(localacc, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));

            if (!string.IsNullOrEmpty(ADFSAdminGroupSID))
            {
                SecurityIdentifier adfsgroup = new SecurityIdentifier(ADFSAdminGroupSID);
                fSecurity.PurgeAccessRules(adfsgroup);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsgroup, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsgroup, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsgroup, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                mustsave = true;
            }
            if (!string.IsNullOrEmpty(ADFSAccountSID))
            {
                SecurityIdentifier adfsaccount = new SecurityIdentifier(ADFSAccountSID);
                fSecurity.PurgeAccessRules(adfsaccount);
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsaccount, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsaccount, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                fSecurity.AddAccessRule(new FileSystemAccessRule(adfsaccount, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                mustsave = true;
            }
            if (mustsave)
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
                            var rsakey = x509.GetRSAPrivateKey();
                            if (rsakey is RSACng)
                            {
                                fileName = ((RSACng)rsakey).Key.UniqueName;
                            }
                            else if (rsakey is RSACryptoServiceProvider)
                            {
                                fileName = ((RSACryptoServiceProvider)rsakey).CspKeyContainerInfo.UniqueKeyContainerName;
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
                    catch
                    {
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
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// GetADFSServiceSID method implmentation
        /// </summary>
        private static string GetADFSServiceSID()
        {
            try
            {
                IntPtr ptr = GetServiceSidPtr("adfssrv");
                return new SecurityIdentifier(ptr).Value;
            }
            catch (Exception)
            {
                return string.Empty;
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
            catch (Exception)
            {
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
            catch (Exception)
            {
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
            catch (Exception)
            {
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
                return GetADFSDelegateServiceAdministration(ref tuple);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// GetServiceSidPtr method implementation
        /// </summary>
        private static IntPtr GetServiceSidPtr(string service)
        {
            NativeMethods.LSA_UNICODE_STRING lSA_UNICODE_STRING = default(NativeMethods.LSA_UNICODE_STRING);
            lSA_UNICODE_STRING.SetTo(service);
            int cb = 0;
            IntPtr intPtr = IntPtr.Zero;
            IntPtr result;
            try
            {
                uint num = NativeMethods.RtlCreateServiceSid(ref lSA_UNICODE_STRING, IntPtr.Zero, ref cb);
                if (num == 3221225507u)
                {
                    intPtr = Marshal.AllocHGlobal(cb);
                    num = NativeMethods.RtlCreateServiceSid(ref lSA_UNICODE_STRING, intPtr, ref cb);
                }
                if (num != 0u)
                {
                    throw new Win32Exception(Convert.ToInt32(num));
                }
                result = intPtr;
            }
            finally
            {
                lSA_UNICODE_STRING.Dispose();
            }
            return result;
        }

        /// <summary>
        /// GetADFSDelegateServiceAdministration method implmentation
        /// </summary>
        private static string GetADFSDelegateServiceAdministration(ref ADFSAdminPolicies tuple)
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
                    grpname = result.Properties["DelegateServiceAdministration"].Value.ToString();
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
                if (SPRunSpace != null)
                    SPRunSpace.Close();
                if (SPPowerShell != null)
                    SPPowerShell.Dispose();
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
            [DllImport("ntdll.dll")]
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
