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
//#define NEA
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace Neos.IdentityServer.MultiFactor
{
    #region Keys Repository
    /// <summary>
    /// KeysRepositoryService abstract class
    /// </summary>
    public abstract class KeysRepositoryService
    {
        private BaseDataHost _host;
        private int _deliverywindow = 300;
        public KeysRepositoryService(BaseDataHost host, int deliverywindow)
        {
            _host = host;
            _deliverywindow = deliverywindow;
        }

        public BaseDataHost Host { get { return _host; } }
        public int DeliveryWindow { get { return _deliverywindow; } }

        public abstract string GetUserKey(string upn);
        public abstract string NewUserKey(string upn, string secretkey, X509Certificate2 cert = null);
        public abstract bool RemoveUserKey(string upn, bool fullclear);
        public abstract X509Certificate2 GetUserCertificate(string upn, string password);
        public abstract X509Certificate2 CreateCertificate(string upn, string password, int validity);

        public abstract bool HasStoredKey(string upn);
        public abstract bool HasStoredCertificate(string upn);
    }
    #endregion

    #region ADDS Repository Connection
    /// <summary>
    /// IDataRepositoryADDSConnection interface
    /// </summary>
    public interface IDataRepositoryADDSConnection
    {
        bool CheckConnection(string domainname, string username, string password);
        bool CheckAttribute(string domainname, string username, string password, string attributename, int multivalued);
    }
    #endregion

    #region SQL Repository Connection
    /// <summary>
    /// IDataRepositorySQLConnection interface
    /// </summary>
    public interface IDataRepositorySQLConnection
    {
       // bool CheckConnection(string connectionstring);
        bool CheckConnection(string connectionstring, string username, string password);
    }
    #endregion

    #region IWebAuthNDataRepositoryService
    /// <summary>
    /// IWebAuthNDataRepositoryService interface
    /// </summary>
    public interface IWebAuthNDataRepositoryService
    {
        MFAWebAuthNUser GetUser(int challengesize, string username);
        List<MFAUserCredential> GetCredentialsByUser(MFAWebAuthNUser user);       
        List<MFAUserCredential> GetCredentialsByUserHandle(MFAWebAuthNUser user, byte[] userHandle);
        MFAUserCredential GetCredentialById(MFAWebAuthNUser user, byte[] id);
        MFAUserCredential GetCredentialByCredentialId(MFAWebAuthNUser user, string credentialid);
        void UpdateCounter(MFAWebAuthNUser user, byte[] credentialId, uint counter);
        bool AddUserCredential(MFAWebAuthNUser user, MFAUserCredential credential);
        bool SetUserCredential(MFAWebAuthNUser user, MFAUserCredential credential);
        bool RemoveUserCredential(MFAWebAuthNUser user, string credentialId);
        List<MFAWebAuthNUser> GetUsersByCredentialId(MFAWebAuthNUser user, byte[] credentialId);
    }
    #endregion

    #region Data Repository
    /// <summary>
    /// DataRepositoryService abstract class
    /// </summary>
    public abstract class DataRepositoryService
    {
        private BaseDataHost _host;
        private int _deliverywindow = 300;
        public DataRepositoryService(BaseDataHost host, int deliverywindow)
        {
            _host = host;
            _deliverywindow = deliverywindow;
        }

        public BaseDataHost Host { get { return _host; } }
        public int DeliveryWindow { get { return _deliverywindow; } }

        public delegate void KeysDataManagerEvent(string user, KeysDataManagerEventKind kind);
        public abstract event KeysDataManagerEvent OnKeyDataEvent;
        public abstract MFAUser GetMFAUser(string upn);
        public abstract MFAUser SetMFAUser(MFAUser reg, bool resetkey = false, bool caninsert = true, bool disableoninsert = false);
        public abstract MFAUser AddMFAUser(MFAUser reg, bool resetkey = false, bool canupdate = true, bool disableoninsert = false);
        public abstract bool DeleteMFAUser(MFAUser reg, bool dropkey = true);
        public abstract MFAUser EnableMFAUser(MFAUser reg);
        public abstract MFAUser DisableMFAUser(MFAUser reg);
        public abstract MFAUserList GetMFAUsers(DataFilterObject filter, DataOrderObject order, DataPagingObject paging);
        public abstract MFAUserList GetMFAUsersAll(DataOrderObject order, bool enabledonly = false);
        public abstract int GetMFAUsersCount(DataFilterObject filter);
        public abstract bool IsMFAUserRegistered(string upn);

        #region Imports
        /// <summary>
        /// CleanMFAUsers method implementation
        /// </summary>
        public virtual List<string> CleanMFAUsers(UsersADDSRecord Parameters)
        {
            MFAUniqueDeletedUserList registrations = new MFAUniqueDeletedUserList();
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(Parameters.DomainName, Parameters.UserName, Parameters.Password))
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectClass=user)(isDeleted=TRUE))";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        AddPropertiesToLoadForDeleted(dsusr);
                        dsusr.SizeLimit = 10000; // Set maxrows
                        dsusr.PageSize = 5000;
                        dsusr.ExtendedDN = ExtendedDN.Standard;
                        dsusr.Tombstone = true;

                        SearchResultCollection src = dsusr.FindAll();
                        if (src != null)
                        {
                            foreach (SearchResult sr in src)
                            {
                                string upn = string.Empty;
                                string sam = string.Empty;
                                if (sr.Properties.Contains("userPrincipalName"))
                                    upn = sr.Properties["userPrincipalName"][0].ToString();
                                if (sr.Properties.Contains("sAMAccountName"))
                                    sam = sr.Properties["sAMAccountName"][0].ToString();
                                
                                if (!string.IsNullOrEmpty(upn) && !string.IsNullOrEmpty(sam))
                                {
                                    string identity = string.Empty;
                                    if (ADDSClaimsUtilities.GetADDSSearchAttribute().Equals("userPrincipalName"))
                                        identity = upn;
                                    else
                                        identity = sam;

                                    if (!CheckMFAUser(Parameters, identity))
                                    {
                                        registrations.AddOrUpdate(identity);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5100);
                throw new Exception(ex.Message);
            }
            return registrations;
        }

        /// <summary>
        /// ImportMFAUsers method implementation
        /// </summary>
        public virtual MFAUserList ImportMFAUsers(UsersADDSRecord Parameters, bool disableall = false )
        {
            if (!string.IsNullOrEmpty(Parameters.LDAPPath))
            {
                Parameters.LDAPPath = Parameters.LDAPPath.Replace("ldap://", "");
                Parameters.LDAPPath = Parameters.LDAPPath.Replace("ldaps://", "");
                Parameters.LDAPPath = Parameters.LDAPPath.Replace("LDAP://", "");
                Parameters.LDAPPath = Parameters.LDAPPath.Replace("LDAPS://", "");
            }
            MFAUniqueUserList registrations = new MFAUniqueUserList();
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(Parameters.DomainName, Parameters.UserName, Parameters.Password, Parameters.LDAPPath))
                {
                    string qryldap = string.Empty;
                    string subldap = string.Empty;
                    bool hasval1 = false;
                    bool hasval2 = false;
                    qryldap = "(| (&(objectCategory=group)(objectClass=group)) (&(objectCategory=user)(objectClass=user)";
                    if (!string.IsNullOrEmpty(Parameters.LDAPFilter))
                        qryldap += "(" + Parameters.LDAPFilter + ")";
                    if (Parameters.CreatedSince.HasValue)
                    {
                        subldap += "(whenCreated>=" + Parameters.CreatedSince.Value.ToString("yyyyMMddHHmmss.0Z") + ")";
                        hasval1 = true;
                    }
                    if (Parameters.ModifiedSince.HasValue)
                    {
                        subldap += "(whenChanged>=" + Parameters.ModifiedSince.Value.ToString("yyyyMMddHHmmss.0Z") + ")";
                        hasval2 = true;
                    }
                    if (hasval1 && hasval2)
                        qryldap += "(|" + subldap + ")";
                    else if (hasval1 || hasval2)
                        qryldap += subldap;
                    qryldap += "))";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        AddPropertiesToLoadForSearcher(dsusr, Parameters.MailAttribute, Parameters.PhoneAttribute);
                        dsusr.SizeLimit = 100000; // Set maxrows
                        dsusr.PageSize = 5000;

                        SearchResultCollection src = dsusr.FindAll();
                        if (src != null)
                        {
                            foreach (SearchResult sr in src)
                            {
                                using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(Parameters.DomainName, Parameters.UserName, Parameters.Password, sr))
                                {                                    
                                    int k = IsImportUser(DirEntry.Properties["objectClass"].Value);
                                    switch (k)
                                    {
                                        case 1:
                                            DoImportUser(DirEntry, registrations, Parameters, disableall);
                                            break;
                                        case 2:
                                            if (!Parameters.NoRecurse)
                                                DoImportGroup(DirEntry, registrations, Parameters, disableall);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry("Root : " + ex.Message, System.Diagnostics.EventLogEntryType.Error, 5100);
               // throw new Exception(ex.Message);
            }
            return registrations;
        }

        /// <summary>
        /// DoImportGroup method implementation
        /// </summary>
        private void DoImportGroup(DirectoryEntry DirEntry, MFAUniqueUserList users, UsersADDSRecord Parameters, bool disableall)
        {
            string distinguishedName = string.Empty;
            string sidstr = string.Empty;
            try
            {
                distinguishedName = DirEntry.Properties["distinguishedName"].Value.ToString();
                byte[] SD = (byte[])DirEntry.Properties["objectSID"].Value;
                string sid = new SecurityIdentifier(SD, 0).ToString();
                sidstr = sid.Substring(sid.LastIndexOf("-") + 1);
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(Parameters.DomainName, Parameters.UserName, Parameters.Password)) // Binding Root
                {
                    string qryldap = string.Empty;
                    string subldap = string.Empty;
                    bool hasval1 = false;
                    bool hasval2 = false;
                    qryldap = "(| (&(objectCategory=group)(objectClass=group)(memberof=" + distinguishedName + ")) (&(objectCategory=user)(objectClass=user)(|(memberof=" + distinguishedName + ")(primaryGroupID=" + sidstr + "))";
                    if (!string.IsNullOrEmpty(Parameters.LDAPFilter))
                        qryldap += "(" + Parameters.LDAPFilter + ")";
                    if (Parameters.CreatedSince.HasValue)
                    {
                        subldap += "(whenCreated>=" + Parameters.CreatedSince.Value.ToString("yyyyMMddHHmmss.0Z") + ")";
                        hasval1 = true;
                    }
                    if (Parameters.ModifiedSince.HasValue)
                    {
                        subldap += "(whenChanged>=" + Parameters.ModifiedSince.Value.ToString("yyyyMMddHHmmss.0Z") + ")";
                        hasval2 = true;
                    }
                    if (hasval1 && hasval2)
                        qryldap += "(|" + subldap + ")";
                    else if (hasval1 || hasval2)
                        qryldap += subldap;
                    qryldap += "))";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        AddPropertiesToLoadForSearcher(dsusr, Parameters.MailAttribute, Parameters.PhoneAttribute);
                        dsusr.SizeLimit = 100000; // Set maxrows
                        dsusr.PageSize = 5000;

                        SearchResultCollection src = dsusr.FindAll();
                        if (src != null)
                        {
                            foreach (SearchResult sr in src)
                            {
                                using (DirectoryEntry SubDirEntry = ADDSUtils.GetDirectoryEntry(Parameters.DomainName, Parameters.UserName, Parameters.Password, sr))
                                {
                                    int k = IsImportUser(SubDirEntry.Properties["objectClass"].Value);
                                    switch (k)
                                    {
                                        case 1:
                                            DoImportUser(SubDirEntry, users, Parameters, disableall);
                                            break;
                                        case 2:
                                            if (!Parameters.NoRecurse)
                                                DoImportGroup(SubDirEntry, users, Parameters, disableall);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry("DN : " + distinguishedName + "     SID : " + sidstr + "     Error : " + ex.Message, System.Diagnostics.EventLogEntryType.Error, 5100);
                // throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// DoImportUser method implementation
        /// </summary>
        private void DoImportUser(DirectoryEntry DirEntry, MFAUniqueUserList users, UsersADDSRecord Parameters, bool disableall = false)
        {
            if (DirEntry.Properties["objectGUID"].Value != null)
            {
                MFAUser reg = new MFAUser();
                try
                {
                    reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                    if (DirEntry.Properties[ADDSClaimsUtilities.GetADDSUserAttribute()] != null)
                    {
                        if (DirEntry.Properties[ADDSClaimsUtilities.GetADDSUserAttribute()].Count>0)
                        {
                            reg.UPN = DirEntry.Properties[ADDSClaimsUtilities.GetADDSUserAttribute()][0].ToString();
                            if (!string.IsNullOrEmpty(Parameters.MailAttribute))
                            {
                                if (DirEntry.Properties[Parameters.MailAttribute].Value != null)
                                    reg.MailAddress = DirEntry.Properties[Parameters.MailAttribute].Value.ToString();
                            }
                            else
                            {
                                if (DirEntry.Properties["otherMailbox"].Value != null)
                                    reg.MailAddress = DirEntry.Properties["otherMailbox"].Value.ToString();
                                else if (DirEntry.Properties["mail"].Value != null)
                                    reg.MailAddress = DirEntry.Properties["mail"].Value.ToString();
                            }

                            if (!string.IsNullOrEmpty(Parameters.PhoneAttribute))
                            {
                                if (DirEntry.Properties[Parameters.PhoneAttribute].Value != null)
                                    reg.PhoneNumber = DirEntry.Properties[Parameters.PhoneAttribute].Value.ToString();
                            }
                            else
                            {
                                if (DirEntry.Properties["mobile"].Value != null)
                                    reg.PhoneNumber = DirEntry.Properties["mobile"].Value.ToString();
                                else if (DirEntry.Properties["otherMobile"].Value != null)
                                    reg.PhoneNumber = DirEntry.Properties["otherMobile"].Value.ToString();
                                else if (DirEntry.Properties["telephoneNumber"].Value != null)
                                    reg.PhoneNumber = DirEntry.Properties["telephoneNumber"].Value.ToString();
                            }
                            reg.PreferredMethod = Parameters.Method;
                            reg.OverrideMethod = string.Empty;
                            if (disableall)
                                reg.Enabled = false;
                            else if (DirEntry.Properties["userAccountControl"] != null)
                            {
                                int v = Convert.ToInt32(DirEntry.Properties["userAccountControl"].Value);
                                reg.Enabled = ((v & 2) == 0);
                            }
                            else
                                reg.Enabled = true;
                            users.AddOrUpdate(reg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DataLog.WriteEntry("User ID : "+reg.ID + "    Error : "+ex.Message, System.Diagnostics.EventLogEntryType.Error, 20104);
                }
            }
        }

        /// <summary>
        /// CheckMFAUser method implmentation
        /// </summary>
        private bool CheckMFAUser(UsersADDSRecord Parameters, string identity)
        {
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(Parameters.DomainName, Parameters.UserName, Parameters.Password))
                {
                    string qryldap = "(&(objectCategory=person)(objectClass=user)(" + ADDSClaimsUtilities.GetADDSSearchAttribute()+"="+identity + "))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            return (sr.Properties["objectGUID"][0]!= null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
               // throw new Exception(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// AddPropertiesToLoadForSearcher method implementation
        /// </summary>
        private void AddPropertiesToLoadForSearcher(DirectorySearcher dsusr, string mailattribute, string phoneattribute)
        {
            dsusr.PropertiesToLoad.Clear();
            dsusr.PropertiesToLoad.Add("objectClass");
            dsusr.PropertiesToLoad.Add("objectGUID");
            dsusr.PropertiesToLoad.Add("objectSID");
            dsusr.PropertiesToLoad.Add("userPrincipalName");
            dsusr.PropertiesToLoad.Add("sAMAccountName");
            dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
            dsusr.PropertiesToLoad.Add("userAccountControl");
            dsusr.PropertiesToLoad.Add("distinguishedName");

            if (!string.IsNullOrEmpty(mailattribute))
                dsusr.PropertiesToLoad.Add(mailattribute);
            else
            {
                dsusr.PropertiesToLoad.Add("mail");
                dsusr.PropertiesToLoad.Add("otherMailbox");
            }
            if (!string.IsNullOrEmpty(phoneattribute))
                dsusr.PropertiesToLoad.Add(phoneattribute);
            else
            {
                dsusr.PropertiesToLoad.Add("mobile");
                dsusr.PropertiesToLoad.Add("otherMobile");
                dsusr.PropertiesToLoad.Add("telephoneNumber");
            }
        }

        /// <summary>
        /// AddPropertiesToLoadForSearcher method implementation
        /// </summary>
        private void AddPropertiesToLoadForDeleted(DirectorySearcher dsusr)
        {
            dsusr.PropertiesToLoad.Clear();
            dsusr.PropertiesToLoad.Add("objectClass");
            dsusr.PropertiesToLoad.Add("objectGUID");
            dsusr.PropertiesToLoad.Add("objectSID");
            dsusr.PropertiesToLoad.Add("userPrincipalName");
            dsusr.PropertiesToLoad.Add("sAMAccountName");
            dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
            dsusr.PropertiesToLoad.Add("userAccountControl");          
            dsusr.PropertiesToLoad.Add("distinguishedName");
        }

        /// <summary>
        /// IsImportUser method implementation
        /// </summary>
        private int IsImportUser(object value)
        {
            if (value is object[])
            {
                object[] values = value as object[];
                foreach (string s in values)
                {
                    if (s.ToLower().Equals("computer"))
                        return 0;
                }
                foreach (string s in values)
                {
                    if (s.ToLower().Equals("user"))
                        return 1;
                    if (s.ToLower().Equals("group"))
                        return 2;
                }
                return 0;
            }
            else
                return 0;
        }
        #endregion
    }

    public class UsersADDSRecord
    {
        /// <summary>
        /// DomainName property
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// UserName property
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password property
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// LDAPPath property
        /// </summary>
        public string LDAPPath { get; set; }

        /// <summary>
        /// LDAPFilter property
        /// </summary>
        public string LDAPFilter { get; set; }

        /// <summary>
        /// CreatedSince property
        /// </summary>
        public DateTime? CreatedSince { get; set; }

        /// <summary>
        /// ModifiedSince property
        /// </summary>
        public DateTime? ModifiedSince { get; set; }

        /// <summary>
        /// MailAttribute property
        /// </summary>
        public string MailAttribute { get; set; }

        /// <summary>
        /// PhoneAttribute property
        /// </summary>
        public string PhoneAttribute { get; set; }

        /// <summary>
        /// Method  property
        /// </summary>
        public PreferredMethod Method { get; set; }

        /// <summary>
        /// NoRecurse property
        /// </summary>
        public bool NoRecurse { get; set; }
    }
    #endregion

    #region WebAuthN
    /// <summary>
    /// IWebAuthNConfiguration interface
    /// </summary>
    public interface IWebAuthNConfiguration
    {
        uint Timeout { get; set; }
        int TimestampDriftTolerance { get; set; }
        int ChallengeSize { get; set; }
        string ServerDomain { get; set; }
        string ServerName { get; set; }
        string ServerIcon { get; set; }
        string Origin { get; set; }
        string ForbiddenBrowsers { get; set; }
        string InitiatedBrowsers { get; set; }
    }

    /// <summary>
    /// IWebAuthNOptions interface
    /// </summary>
    public interface IWebAuthNOptions
    {
        string AuthenticatorAttachment { get; set; }
        string AttestationConveyancePreference { get; set; }
        string UserVerificationRequirement { get; set; }
        bool Extensions { get; set; }
        bool UserVerificationMethod { get; set; }
        bool ConstrainedMetadataRepository { get; set; }
    }
    #endregion
}
