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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Neos.IdentityServer.MultiFactor.Data
{
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

    /// <summary>
    /// ADDSForest class implementation
    /// </summary>
    public class ADDSForest
    {
        public bool IsRoot { get; set; }
        public string ForestDNS { get; set; }
        public List<string> TopLevelNames = new List<string>();
    }

    /// <summary>
    /// ADDSForestUtils class implementation
    /// </summary>
    public class ADDSForestUtils
    {
        private bool _isbinded = false;
        private List<ADDSForest> _forests = new List<ADDSForest>();

        /// <summary>
        /// ADDSForestUtils constructor
        /// </summary>
        public ADDSForestUtils()
        {
            _isbinded = false;
            Bind();
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

        public List<ADDSForest> Forests
        {
            get { return _forests; }
        }
    }

    #region ADDS Utils
    internal static class ADDSUtils
    {
        #region Private methods
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
        /// GetDirectoryEntryForUPN() method implmentation
        /// </summary>
        internal static DirectoryEntry GetDirectoryEntryForUser(ADDSHost host, string upn, bool usessl)
        {
            string root = "LDAP://";
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(host.DomainName))
            {
                if (usessl)
                    entry = new DirectoryEntry(root + host.DomainName+ ":636");
                else
                    entry = new DirectoryEntry(root + host.DomainName);
            }
            else
            {
                string dom = host.GetForestForUser(upn);
                if (usessl)
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
        internal static DirectoryEntry GetDirectoryEntryForUser(ADDSHost host, string account, string password, string upn, bool usessl)
        {
            string root = "LDAP://";
            DirectoryEntry entry = null;
            string dom = host.GetForestForUser(upn);

            if (usessl)
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
        internal static DirectoryEntry GetDirectoryEntry(string domainname, string username, string password, bool usessl)
        {
            string root = "LDAP://";
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(domainname))
            {
                if (usessl)
                    entry = new DirectoryEntry(root + domainname + ":636");
                else
                    entry = new DirectoryEntry(root + domainname);
            }
            else
            {
                Domain dom = Domain.GetComputerDomain();
                if (usessl)
                    entry = new DirectoryEntry(root + dom.Name + ":636");
                else
                    entry = new DirectoryEntry(root + dom.Name);
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
        internal static DirectoryEntry GetDirectoryEntry(ADDSHost host, SearchResult sr, bool usessl)
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
        internal static DirectoryEntry GetDirectoryEntry(string domainname, string account, string password, SearchResult sr, bool usessl)
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
        internal static DirectoryEntry GetDirectoryEntry(string domain, string username, string password, string path, bool usessl)
        {
            string root = "LDAP://";
            DirectoryEntry entry = null;
            if (string.IsNullOrEmpty(path))
            {
                  if (!string.IsNullOrEmpty(domain))
                  {
                      if (usessl)
                          entry = new DirectoryEntry(root + domain + ":636");
                      else
                          entry = new DirectoryEntry(root + domain);
                  }
                  else
                  {
                      Domain dom = Domain.GetComputerDomain();
                      if (usessl)
                          entry = new DirectoryEntry(root + dom.Name + ":636");
                      else
                          entry = new DirectoryEntry(root + dom.Name);
                  } 
            }
            else
            {
                entry = new DirectoryEntry();
                entry.Path = root + path;
            }
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
            if (ctx != null)
            {
                using (Domain dom = Domain.GetDomain(ctx))
                {
                    using (Forest forest = dom.Forest)
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
            }
            return false;
        }

        /// <summary>
        /// IsBinaryAttribute method implmentation
        /// </summary>
        internal static bool IsBinaryAttribute(string domainname, string username, string password, string attributename)
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
            if (ctx != null)
            {
                using (Domain dom = Domain.GetDomain(ctx))
                {
                    using (Forest forest = dom.Forest)
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
            }
            return false;
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
