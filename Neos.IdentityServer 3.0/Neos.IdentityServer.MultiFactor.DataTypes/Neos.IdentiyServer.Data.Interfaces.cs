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
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
        public abstract MFAUserList ImportMFAUsers(string domain, string username, string password, string ldappath, DateTime? created, DateTime? modified, string mailattribute, string phoneattribute, PreferredMethod method, bool disableall = false);        
    }
    #endregion

    #region Keys Repository
    /// <summary>
    /// KeysRepositoryService abstract class
    /// </summary>
    public abstract class KeysRepositoryService
    {
        public abstract string GetUserKey(string upn);
        public abstract string NewUserKey(string upn, string secretkey, X509Certificate2 cert = null);
        public abstract bool RemoveUserKey(string upn);
        public abstract X509Certificate2 GetUserCertificate(string upn, bool generatepassword = false);
        public abstract X509Certificate2 CreateCertificate(string upn, int validity, bool generatepassword = false);
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
}
