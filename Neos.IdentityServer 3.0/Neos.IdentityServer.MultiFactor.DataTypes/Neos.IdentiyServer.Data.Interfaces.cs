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
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

namespace Neos.IdentityServer.MultiFactor
{   
    /// <summary>
    /// IDataRepositoryADDSConnection interface
    /// </summary>
    public interface IDataRepositoryADDSConnection
    {
        bool CheckConnection(string domainname, string username, string password);
        bool CheckAttribute(string domainname, string username, string password, string attributename, int multivalued);
    }

    /// <summary>
    /// IDataRepositorySQLConnection interface
    /// </summary>
    public interface IDataRepositorySQLConnection
    {
        bool CheckConnection(string connectionstring);
    }

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
      
        /// <summary>
        /// ImportMFAUsers method implementation
        /// </summary>
        public virtual MFAUserList ImportMFAUsers(string domain, string username, string password, string ldappath, DateTime? created, DateTime? modified, string mailattribute, string phoneattribute, PreferredMethod meth, bool usessl, bool disableall = false )
        {
            if (!string.IsNullOrEmpty(ldappath))
            {
                ldappath = ldappath.Replace("ldap://", "");
                ldappath = ldappath.Replace("ldaps://", "");
                ldappath = ldappath.Replace("LDAP://", "");
                ldappath = ldappath.Replace("LDAPS://", "");
            }
            MFAUserList registrations = new MFAUserList();
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(domain, username, password, ldappath, usessl))
                {
                    string qryldap = string.Empty;
                    qryldap = "(&";
                    qryldap += "(objectCategory=user)(objectClass=user)"+ ClaimsUtilities.BuildADDSUserFilter("*");
                    if (created.HasValue)
                        qryldap += "(whenCreated>=" + created.Value.ToString("yyyyMMddHHmmss.0Z") + ")";
                    if (modified.HasValue)
                        qryldap += "(whenChanged>=" + modified.Value.ToString("yyyyMMddHHmmss.0Z") + ")";
                    qryldap += ")";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add("userAccountControl");

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
                        dsusr.SizeLimit = 0; // _host.MaxRows; 

                        SearchResultCollection src = dsusr.FindAll();
                        if (src != null)
                        {
                            foreach (SearchResult sr in src)
                            {
                                MFAUser reg = new MFAUser();
                                using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(domain, username, password, sr, usessl))  
                                {
                                    if (DirEntry.Properties["objectGUID"].Value != null)
                                    {
                                        reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                        if (sr.Properties[ClaimsUtilities.GetADDSUserAttribute()][0] != null)
                                        {
                                            reg.UPN = sr.Properties[ClaimsUtilities.GetADDSUserAttribute()][0].ToString();

                                            if (!string.IsNullOrEmpty(mailattribute))
                                            {
                                                if (DirEntry.Properties[mailattribute].Value != null)
                                                    reg.MailAddress = DirEntry.Properties[mailattribute].Value.ToString();
                                            }
                                            else
                                            {
                                                if (DirEntry.Properties["otherMailbox"].Value != null)
                                                    reg.MailAddress = DirEntry.Properties["otherMailbox"].Value.ToString();
                                                else if (DirEntry.Properties["mail"].Value != null)
                                                    reg.MailAddress = DirEntry.Properties["mail"].Value.ToString();
                                            }

                                            if (!string.IsNullOrEmpty(phoneattribute))
                                            {
                                                if (DirEntry.Properties[phoneattribute].Value != null)
                                                    reg.PhoneNumber = DirEntry.Properties[phoneattribute].Value.ToString();
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
                                            reg.PreferredMethod = meth;
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
                                            registrations.Add(reg);
                                        }
                                    }
                                };
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
    }
    #endregion

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
        public abstract bool RemoveUserKey(string upn);
        public abstract X509Certificate2 GetUserCertificate(string upn, string password);

        /// <summary>
        /// CreateCertificate implementation
        /// </summary>
        public virtual X509Certificate2 CreateCertificate(string upn, string password, int validity)
        {
            string pass = string.Empty;
            if (!string.IsNullOrEmpty(password))
                pass = password;
            return Certs.CreateRSAEncryptionCertificateForUser(upn.ToLower(), validity, pass);
        }

        public abstract bool HasStoredKey(string upn);
        public abstract bool HasStoredCertificate(string upn);
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
        bool RequireValidAttestationRoot { get; set; }
        /* string MDSAccessKey { get; set; }
         string MDSCacheDirPath { get; set; } */
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
        bool UserVerificationIndex { get; set; }
        bool Location { get; set; }
        bool UserVerificationMethod { get; set; }
    }

    #endregion

    #region replay service
    [ServiceContract(Namespace ="http://adfsmfa.org", Name ="Replay")]
    public interface IReplay
    {
        [OperationContract]
        bool Check(List<string> computers, ReplayRecord record);

        [OperationContract]
        void Reset(List<string> computers);
    }

    public interface IDependency
    {
        EventLog GetEventLog();
    }
    #endregion

    #region WebThemes service
    [DataContract]
    public enum WebThemeAddressKind
    {
        [EnumMember]
        Illustration = 0,

        [EnumMember]
        CompanyLogo = 1,

        [EnumMember]
        StyleSheet = 2
    }

    /// <summary>
    /// WebThemesParametersRecord class implementation
    /// </summary>
    [DataContract]
    public class WebThemesParametersRecord
    {
        [DataMember]
        public int LCID { get; set; }

        [DataMember]
        public string Identifier { get; set; }
    }

    /// <summary>
    /// WebThemeRecord class implementation
    /// </summary>
    [DataContract]
    public class WebThemeRecord
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Identifier { get; set; }

        [DataMember]
        public string ObjectId { get; set; }

        [DataMember]
        public bool IsBuiltinTheme { get; set; } = false;

        [DataMember]
        public IDictionary<string, string> StyleSheetUri { get; set; }

        [DataMember]
        public IDictionary<string, string> LogoImageUri { get; set; }

        [DataMember]
        public IDictionary<string, string> IllustrationImageUri { get; set; }
    }

    [ServiceContract(Namespace = "http://adfsmfa.org", Name = "WebThemes")]
    public interface IWebThemeManager
    {
        [OperationContract]
        bool Initialize(Dictionary<string, bool> servers, WebThemesParametersRecord context, string request, out string identifier);

        [OperationContract]
        bool HasRelyingPartyTheme(WebThemesParametersRecord context);

        [OperationContract]
        string GetIllustrationAddress(WebThemesParametersRecord context);

        [OperationContract]
        string GetLogoAddress(WebThemesParametersRecord context);

        [OperationContract]
        string GetStyleSheetAddress(WebThemesParametersRecord context);

        [OperationContract]
        Dictionary<WebThemeAddressKind, string> GetAddresses(WebThemesParametersRecord context);

        [OperationContract]
        void DispachTheme(WebThemeRecord theme);

        [OperationContract]
        WebThemeRecord RequestTheme(WebThemesParametersRecord theme);

        [OperationContract]
        void ResetThemes();

        [OperationContract]
        void ResetThemesList(Dictionary<string, bool> servers);

    }
    #endregion
}
