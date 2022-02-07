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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Neos.IdentityServer.MultiFactor.Data
{
    public class Utilities
    {
        /// <summary>
        /// GetAssemblyPublicKey method implmentation
        /// </summary>
        public static string GetAssemblyPublicKey()
        {
            string assemblyname = Assembly.GetExecutingAssembly().FullName;
            string[] str = assemblyname.Split(',');
            return str[str.Length - 1];
        }
    }

    /// <summary>
    /// Log class
    /// </summary>
    public class DataLog
    {
        private const string EventLogSource = "ADFS MFA DataServices";
        private const string EventLogGroup = "Application";

        /// <summary>
        /// Log constructor
        /// </summary>
        static DataLog()
        {
            if (!EventLog.SourceExists(DataLog.EventLogSource))
                EventLog.CreateEventSource(DataLog.EventLogSource, DataLog.EventLogGroup);
        }

        /// <summary>
        /// WriteEntry method implementation
        /// </summary>
        public static void WriteEntry(string message, EventLogEntryType type, int eventID)
        {
            EventLog.WriteEntry(EventLogSource, message, type, eventID);
        }
    }

    /*
    /// <summary>
    /// ADDSForest class implementation
    /// </summary>
    public class ADDSForest
    {
        public bool IsRoot { get; set; }
        public string ForestDNS { get; set; }
        public List<string> TopLevelNames = new List<string>();
    }

    /*
    /// <summary>
    /// ADDSForestUtils class implementation
    /// </summary>
    public class ADDSForestUtils
    {
        private bool _isbinded = false;
        private readonly List<ADDSForest> _forests = new List<ADDSForest>();

        /// <summary>
        /// ADDSForestUtils constructor
        /// </summary>
        public ADDSForestUtils()
        {
            _isbinded = false;
            Bind();
        }

        public List<ADDSForest> Forests
        {
            get { return _forests; }
        }

        /// <summary>
        /// Bind method implementation
        /// </summary>
        public void Bind()
        {
            if (_isbinded)
                return;
            _isbinded = true;

            Forests.Clear();
            Forest currentforest = Forest.GetCurrentForest();

            ADDSForest root = new ADDSForest();
            root.IsRoot = true;
            root.ForestDNS = currentforest.Name;
            Forests.Add(root);
            foreach (ForestTrustRelationshipInformation trusts in currentforest.GetAllTrustRelationships())
            {
                ADDSForest sub = new ADDSForest();
                sub.IsRoot = false;
                sub.ForestDNS = trusts.TargetName;
                foreach (TopLevelName t in trusts.TopLevelNames)
                {
                    if (t.Status == TopLevelNameStatus.Enabled)
                        sub.TopLevelNames.Add(t.Name);
                }
                Forests.Add(sub);
            }
        }

        /// <summary>
        /// GetForestDNSForUPN method implementation
        /// </summary>
        public string GetForestDNSForUPN(string upn)
        {
            string result = string.Empty;
            string domtofind = upn.Substring(upn.IndexOf('@') + 1);
            foreach (ADDSForest f in Forests)
            {
                if (f.IsRoot) // Fallback/default Forest
                    result = f.ForestDNS;
                else
                {
                    foreach (string s in f.TopLevelNames)
                    {
                        if (s.ToLower().Equals(domtofind.ToLower()))
                        {
                            result = f.ForestDNS;
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
    */

    /// <summary>
    /// ADDSHostForest class implementation
    /// </summary>
    public class ADDSHostForest
    {
        public bool IsRoot { get; set; }
        public string ForestDNS { get; set; }
        public List<string> TopLevelNames = new List<string>();
    }

    #region ADDS Utils
    public static class ADDSUtils
    {
        private static bool _isbinded = false;
        private static bool _usessl = false;

        /// <summary>
        /// ADDSHost constructor
        /// </summary>
        static ADDSUtils()
        {
            _isbinded = false;
        }

        internal static List<ADDSHostForest> Forests { get; } = new List<ADDSHostForest>();

        /// <summary>
        /// LoadForests method implementation
        /// </summary>
        public static void LoadForests(string domainname, string account, string password, bool usessl, bool reset = false)
        {
            if (reset)
                ResetForests();
            if (_isbinded)
                return;
            try
            {
                _usessl = usessl;
                using (Domain domain = ADDSUtils.GetRootDomain(domainname, account, password))
                {
                    using (Forest forest = ADDSUtils.GetForest(domain.Name, account, password))
                    {
                        Forests.Clear();
                        ADDSHostForest root = new ADDSHostForest
                        {
                            IsRoot = true,
                            ForestDNS = forest.Name
                        };
                        Forests.Add(root);
                        foreach (ForestTrustRelationshipInformation trusts in forest.GetAllTrustRelationships())
                        {
                            ADDSHostForest sub = new ADDSHostForest
                            {
                                IsRoot = false,
                                ForestDNS = trusts.TargetName
                            };
                            foreach (TopLevelName t in trusts.TopLevelNames)
                            {
                                if (t.Status == TopLevelNameStatus.Enabled)
                                    sub.TopLevelNames.Add(t.Name);
                            }
                            Forests.Add(sub);
                        }
                    }
                }
                _isbinded = true;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5100);
                _isbinded = false;
            }
        }

        /// <summary>
        /// ResetForests method implementation
        /// </summary>
        public static void ResetForests()
        {
            _usessl = false;
            _isbinded = false;
            Forests.Clear();
        }

        /// <summary>
        /// GetForestForUser method implementation
        /// </summary>
        internal static string GetForestForUser(ADDSHost host, string username)
        {
            string result = string.Empty;
            switch (ClaimsUtilities.IdentityClaimTag)
            {
                case MFASecurityClaimTag.Upn:
                    string foresttofind = username.Substring(username.IndexOf('@') + 1);
                    result = foresttofind;
                    foreach (ADDSHostForest f in Forests)
                    {
                        if (f.IsRoot) // By default Any root domain, subdomain, toplevelname on default forest
                        {
                            result = f.ForestDNS;
                        }
                        else // trusted forests
                        {
                            if (f.ForestDNS.ToLower().Equals(foresttofind.ToLower())) // root domain
                            {
                                result = f.ForestDNS;
                                break;
                            }
                            if (foresttofind.ToLower().EndsWith("." + f.ForestDNS.ToLower()))  // subdomain
                            {
                                result = f.ForestDNS;
                                break;
                            }
                            foreach (string s in f.TopLevelNames) // toplevelnames
                            {
                                if (s.ToLower().Equals(foresttofind.ToLower()))
                                {
                                    result = f.ForestDNS;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case MFASecurityClaimTag.WindowsAccountName:
                    string ntlmdomain = username.Substring(0, username.IndexOf('\\'));
                    DirectoryContext ctx = null;
                    if (string.IsNullOrEmpty(host.Account) && string.IsNullOrEmpty(host.Password))
                        ctx = new DirectoryContext(DirectoryContextType.Domain, ntlmdomain);
                    else
                        ctx = new DirectoryContext(DirectoryContextType.Domain, ntlmdomain, host.Account, host.Password);
                    result = Domain.GetDomain(ctx).Forest.RootDomain.Name;
                    break;
            }
            return result;
        }

        /// <summary>
        /// GetDomainForUser method implmentation
        /// </summary>
        internal static string GetSAMAccountForUser(ADDSHost host, string username)
        {
            string result = string.Empty;
            switch (ClaimsUtilities.IdentityClaimTag)
            {
                case MFASecurityClaimTag.Upn:
                    string foresttofind = username.Substring(username.IndexOf('@') + 1);
                    if (!string.IsNullOrEmpty(foresttofind))
                        return GetNetBiosName(host, username);
                    else
                        return null;
                case MFASecurityClaimTag.WindowsAccountName:
                    string ntlmdomain = username.Substring(0, username.IndexOf('\\'));
                    if (!string.IsNullOrEmpty(ntlmdomain))
                        return username;
                    else
                        return null;
            }
            return result;
        }

        /// <summary>
        /// GetNetBiosName method 
        /// </summary>
        private static string GetNetBiosName(ADDSHost host, string username)
        {
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(host, host.Account, host.Password, username))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userPrincipalName=" + username + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(host, sr))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    return sr.Properties["msDS-PrincipalName"][0].ToString();
                                }
                                else
                                    return null;
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return null;
        }

        #region methods
        /// <summary>
        /// GetBinaryStringFromGuidString
        /// </summary>
        internal static string GetBinaryStringFromGuidString(string guidstring)
        {
            Guid guid = new Guid(guidstring);
            byte[] bytes = guid.ToByteArray();
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(string.Format(@"\{0}", b.ToString("X")));
            }
            return sb.ToString();
        }

        /// <summary>
        /// GetRootDomain method implmentation
        /// </summary>
        internal static Domain GetRootDomain(string domainname, string username, string password)
        {
            DirectoryContext ctx = null;
            if (!string.IsNullOrEmpty(domainname))
            {
                if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
                    ctx = new DirectoryContext(DirectoryContextType.Domain, domainname, username, password);
                else
                    ctx = new DirectoryContext(DirectoryContextType.Domain, domainname);
            }
            else
            {
                if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
                    ctx = new DirectoryContext(DirectoryContextType.Domain, username, password);
                else
                    ctx = new DirectoryContext(DirectoryContextType.Domain);
            }
            return Domain.GetDomain(ctx).Forest.RootDomain;
        }

        /// <summary>
        /// GetForest method implmentation
        /// </summary>
        internal static Forest GetForest(string domainname, string username, string password)
        {
            DirectoryContext ctx = null;
            if (!string.IsNullOrEmpty(domainname))
            {
                if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
                    ctx = new DirectoryContext(DirectoryContextType.Forest, domainname, username, password);
                else
                    ctx = new DirectoryContext(DirectoryContextType.Forest, domainname);
            }
            else
            {
                if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
                    ctx = new DirectoryContext(DirectoryContextType.Forest, username, password);
                else
                    ctx = new DirectoryContext(DirectoryContextType.Forest);
            }
            return Forest.GetForest(ctx);
        }

        /// <summary>
        /// GetDirectoryEntryForUPN() method implmentation
        /// </summary>
        internal static DirectoryEntry GetDirectoryEntryForUser(ADDSHost host, string upn)
        {
            string root = "LDAP://";
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(host.DomainName))
            {
                if (_usessl)
                    entry = new DirectoryEntry(root + host.DomainName+ ":636");
                else
                    entry = new DirectoryEntry(root + host.DomainName);
            }
            else
            {
                string dom = ADDSUtils.GetForestForUser(host, upn);
                if (_usessl)
                    entry = new DirectoryEntry(root + dom + ":636");
                else
                    entry = new DirectoryEntry(root + dom);
            }
            if (!string.IsNullOrEmpty(host.Account))
                entry.Username = host.Account;
            if (!string.IsNullOrEmpty(host.Password))
                entry.Password = host.Password;
            return entry;
        }

        /// <summary>
        /// GetDirectoryEntryForUPN() method implmentation
        /// </summary>
        internal static DirectoryEntry GetDirectoryEntryForUser(ADDSHost host, string account, string password, string upn)
        {
            string root = "LDAP://";
            DirectoryEntry entry = null;
            string dom = ADDSUtils.GetForestForUser(host, upn);

            if (_usessl)
                entry = new DirectoryEntry(root + dom + ":636");
            else
                entry = new DirectoryEntry(root + dom);

            if (!string.IsNullOrEmpty(account))
                entry.Username = account;
            if (!string.IsNullOrEmpty(password))
                entry.Password = password;
            return entry;
        }

        /// <summary>
        /// GetDirectoryEntry method implmentation
        /// </summary>
        internal static DirectoryEntry GetDirectoryEntry(string domainname, string username, string password)
        {
            string root = "LDAP://";
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(domainname))
            {
                if (_usessl)
                    entry = new DirectoryEntry(root + domainname + ":636");
                else
                    entry = new DirectoryEntry(root + domainname);
            }
            else
            {
                using (Domain dom = Domain.GetComputerDomain())
                {
                    if (_usessl)
                        entry = new DirectoryEntry(root + dom.Name + ":636");
                    else
                        entry = new DirectoryEntry(root + dom.Name);
                }
            }
            if (!string.IsNullOrEmpty(username))
                entry.Username = username;
            if (!string.IsNullOrEmpty(password))
                entry.Password = password;
            return entry;
        }

        /// <summary>
        /// GetDirectoryEntry() method implmentation
        /// </summary>
        internal static DirectoryEntry GetDirectoryEntry(ADDSHost host, SearchResult sr)
        {
            DirectoryEntry entry = sr.GetDirectoryEntry();

            entry.Path = sr.Path; // Take SearchResult path
            if (!string.IsNullOrEmpty(host.Account))
                entry.Username = host.Account;
            if (!string.IsNullOrEmpty(host.Password))
                entry.Password = host.Password;
            return entry;
        }

        /// <summary>
        /// GetDirectoryEntry() method implmentation
        /// </summary>
        internal static DirectoryEntry GetDirectoryEntry(string domainname, string account, string password, SearchResult sr)
        {
            DirectoryEntry entry = sr.GetDirectoryEntry();
            entry.Path = sr.Path; // Take SearchResult path
            if (!string.IsNullOrEmpty(account))
                entry.Username = account;
            if (!string.IsNullOrEmpty(password))
                entry.Password = password;
            return entry;
        }

        /// <summary>
        /// GetDirectoryEntry() method implmentation
        /// </summary>
        internal static DirectoryEntry GetDirectoryEntry(string domain, string username, string password, string path)
        {
            string root = "LDAP://";
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(domain))
            {
                if (_usessl)
                    entry = new DirectoryEntry(root + domain + ":636");
                else
                    entry = new DirectoryEntry(root + domain);
            }
            else
            {
                using (Domain dom = Domain.GetComputerDomain())
                {
                    if (_usessl)
                        entry = new DirectoryEntry(root + dom.Name + ":636");
                    else
                        entry = new DirectoryEntry(root + dom.Name);
                }
            } 
            if (!string.IsNullOrEmpty(path))
               entry.Path += "/"+path;
            if (!string.IsNullOrEmpty(username))
                entry.Username = username;
            if (!string.IsNullOrEmpty(password))
                entry.Password = password;
            return entry;
        }
        #endregion

        #region MultiValued Properties
        /// <summary>
        /// GetMultiValued method implementation
        /// </summary>
        internal static string GetMultiValued(PropertyValueCollection props, bool ismultivalued)
        {
            if (props == null)
                return null;
            if (!ismultivalued)
            {
                if (props.Value != null)
                    return props.Value as string;
                else
                    return null;
            }
            else
            {
                switch (props.Count)
                {
                    case 0:
                        return null;
                    case 1:
                        if (props.Value == null)
                            return null;
                        if (props.Value is string)
                        {
                            string so = props.Value as string;
                            if (HasSetMultiValued(so))
                                return SetMultiValuedHeader(so, false);
                            else
                                return so; // Allow return of original value, because only one value. after update an new value wil be tagged
                        }
                        break;
                    default:
                        foreach (object o2 in props)
                        {
                            if (o2 == null)
                                continue;
                            if (o2 is string)
                            {
                                string so2 = o2 as string;
                                if (HasSetMultiValued(so2))
                                    return SetMultiValuedHeader(so2, false);
                            }
                        }
                        break;
                }
            }
            return null;
        }

        /// <summary>
        /// SetMultiValued method implementation
        /// </summary>
        internal static void SetMultiValued(PropertyValueCollection props, bool ismultivalued, string value)
        {
            if (props == null)
                return;
            if (!ismultivalued)
            {
                if (string.IsNullOrEmpty(value))
                    props.Clear();
                else
                    props.Value = value;
            }
            else
            {
                switch (props.Count)
                {
                    case 0:
                        if (!string.IsNullOrEmpty(value))
                            props.Add(SetMultiValuedHeader(value));
                        break;
                    case 1:
                        if (props.Value == null)
                            return;
                        if (props.Value is string)
                        {
                            string so = props.Value as string;
                            if (string.IsNullOrEmpty(value))
                            {
                                props.Remove(so);  // Clean value anyway
                                return;
                            }
                            if (HasSetMultiValued(so))
                                props.Value = SetMultiValuedHeader(value);
                            else
                                props.Add(SetMultiValuedHeader(value));
                        }
                        break;
                    default:
                        int j = props.Count;
                        for (int i = 0; i < j; i++)
                        {
                            if (props[i] != null)
                            {
                                if (props[i] is string)
                                {
                                    string so2 = props[i] as string;
                                    if (HasSetMultiValued(so2))
                                    {
                                        if (string.IsNullOrEmpty(value))
                                            props.Remove(so2);
                                        else
                                            props[i] = SetMultiValuedHeader(value);
                                        return;
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(value))
                            props.Add(SetMultiValuedHeader(value)); // Add tagged value if not null
                        break;
                }
            }
        }

        /// <summary>
        /// HasSetMultiValued method implementation
        /// </summary>
        private static bool HasSetMultiValued(string value)
        {
            if (value == null)
                return false;
            return (value.ToLower().StartsWith("mfa:"));
        }

        /// <summary>
        /// SetMultiValuedHeader method implementation
        /// </summary>
        private static string SetMultiValuedHeader(string value, bool addheader = true)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            if (addheader)
            {
                if (HasSetMultiValued(value))
                    return value;
                else
                    return "mfa:" + value;
            }
            else
            {
                if (!HasSetMultiValued(value))
                    return value;
                else
                    return value.Replace("mfa:", "");
            }
        }

        /// <summary>
        /// IsMultivaluedAttribute method implmentation
        /// </summary>
        internal static bool IsMultivaluedAttribute(string domainname, string username, string password, string attributename)
        {
            try
            {
                using (Domain domain = ADDSUtils.GetRootDomain(domainname, username, password))
                {
                    using (Forest forest = ADDSUtils.GetForest(domain.Name, username, password))
                    {
                        ActiveDirectorySchemaProperty property = forest.Schema.FindProperty(attributename);
                        if (property != null)
                        {
                            if (property.Name.Equals(attributename))
                            {
                                return !property.IsSingleValued;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5100);
                return false;
            }
        }

        /// <summary>
        /// IsBinaryAttribute method implmentation
        /// </summary>
        internal static bool IsBinaryAttribute(string domainname, string username, string password, string attributename)
        {
            try
            {
                using (Domain domain = ADDSUtils.GetRootDomain(domainname, username, password))
                {
                    using (Forest forest = ADDSUtils.GetForest(domain.Name, username, password))
                    {
                        ActiveDirectorySchemaProperty property = forest.Schema.FindProperty(attributename);
                        if (property != null)
                        {
                            if (property.Syntax.Equals(ActiveDirectorySyntax.OctetString))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5100);
                return false;
            }
        }
        #endregion
    }
    #endregion

    #region ClaimUtils
    /// <summary>
    /// Utilities class
    /// </summary>
    public static class ClaimsUtilities
    {
        private static string _identityclaim;
        private static MFASecurityClaimTag _identitylaimtag = MFASecurityClaimTag.Upn;

        /// <summary>
        /// 
        /// </summary>
        public static MFASecurityClaimTag IdentityClaimTag { get => _identitylaimtag; }

        /// <summary>
        /// 
        /// </summary>
        public static void SetIdentityClaim(Claim value)
        {
            if ((string.IsNullOrEmpty(_identityclaim) || (!value.Type.Equals(_identityclaim))))
            {
                _identityclaim = value.Type.ToLower();
                if (_identityclaim.Equals(ClaimTypes.Upn.ToLower()))
                    _identitylaimtag = MFASecurityClaimTag.Upn;
                else if (_identityclaim.Equals(ClaimTypes.WindowsAccountName.ToLower()))
                    _identitylaimtag = MFASecurityClaimTag.WindowsAccountName;
                else
                    _identitylaimtag = MFASecurityClaimTag.Upn;
            }
        }

        /// <summary>
        /// LoadIdentityClaim method implementation
        /// </summary>
        public static void LoadIdentityClaim()
        {
            try
            {
                RegistryKey ek = Registry.LocalMachine.OpenSubKey("Software\\MFA", false);
                if (ek == null)
                    _identitylaimtag = MFASecurityClaimTag.Upn;
                int keyvalue = Convert.ToInt32(ek.GetValue("IdentityClaim", 0, RegistryValueOptions.None));
                switch ((MFASecurityClaimTag)keyvalue)
                {
                    case MFASecurityClaimTag.Upn:
                        _identitylaimtag = MFASecurityClaimTag.Upn;
                        break;
                    case MFASecurityClaimTag.WindowsAccountName:
                        _identitylaimtag = MFASecurityClaimTag.WindowsAccountName;
                        break;
                    default:
                        _identitylaimtag = MFASecurityClaimTag.Upn;
                        break;
                }
            }
            catch (Exception)
            {
                _identitylaimtag = MFASecurityClaimTag.Upn;
            }
        }
    

        /// <summary>
        /// BuildADDSUserFilter method implmentation
        /// </summary>
        public static string BuildADDSUserFilter(string data)
        {
            switch (IdentityClaimTag)
            {
                case MFASecurityClaimTag.Upn:
                    return "(userPrincipalName=" + data + ")";
                case MFASecurityClaimTag.WindowsAccountName:
                    string name = data.Substring(data.IndexOf('\\') + 1);
                    return "(sAMAccountName=" + name + ")";
                default:
                    return "(userPrincipalName=" + data + ")";
            }
        }

        /// <summary>
        /// GetADDSUserAttribute method implmentation
        /// </summary>
        public static string GetADDSUserAttribute()
        {
            switch (IdentityClaimTag)
            {
                case MFASecurityClaimTag.Upn:
                    return "userPrincipalName";
                case MFASecurityClaimTag.WindowsAccountName:
                    return "msDS-PrincipalName";
                default:
                    return "userPrincipalName";
            }
        }

        /// <summary>
        /// GetADDSSearchAttribute method implmentation
        /// </summary>
        public static string GetADDSSearchAttribute()
        {
            switch (IdentityClaimTag)
            {
                case MFASecurityClaimTag.Upn:
                    return "userPrincipalName";
                case MFASecurityClaimTag.WindowsAccountName:
                    return "sAMAccountName";
                default:
                    return "userPrincipalName";
            }
        }

    }
    #endregion
}
