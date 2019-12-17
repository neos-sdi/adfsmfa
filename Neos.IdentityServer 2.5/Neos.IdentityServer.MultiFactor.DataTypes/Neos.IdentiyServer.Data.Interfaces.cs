//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.Data
{
    /// <summary>
    /// KeysDataManagerEventKind enum
    /// </summary>
    public enum KeysDataManagerEventKind 
    {
        Get,
        add,
        Remove
    }

    /// <summary>
    /// DataRepositoryKind enum
    /// </summary>
    public enum DataRepositoryKind
    {
        ADDS,
        SQL
    }

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
        MFAUserCredential GetCredentialByUserWithAAGuid(MFAWebAuthNUser user, Guid aaguid);
        void UpdateCounter(MFAWebAuthNUser user, byte[] credentialId, uint counter);
        bool AddUserCredential(MFAWebAuthNUser user, MFAUserCredential credential);
        bool RemoveUserCredential(MFAWebAuthNUser user, string aaguid);
        List<MFAWebAuthNUser> GetUsersByCredentialId(MFAWebAuthNUser user, byte[] credentialId);
    }

    /// <summary>
    /// MFAUserCredential class
    /// </summary>
    public class MFAUserCredential
    {
        public byte[] UserId { get; set; }
        public MFAPublicKeyCredentialDescriptor Descriptor { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] UserHandle { get; set; }
        public uint SignatureCounter { get; set; }
        public string CredType { get; set; }
        public DateTime RegDate { get; set; }
        public Guid AaGuid { get; set; }
    }

    /// <summary>
    /// MFAWebAuthNUser class
    /// </summary>
    public class MFAWebAuthNUser
    {
        public string Name { get; set; }
        public byte[] Id { get; set; }
        public string DisplayName { get; set; }
    }

    /// <summary>
    /// MFAPublicKeyCredentialDescriptor class implementation
    /// </summary>
    public class MFAPublicKeyCredentialDescriptor
    {
        /// <summary>
        /// Constructors
        /// </summary>
        public MFAPublicKeyCredentialDescriptor() { }
        public MFAPublicKeyCredentialDescriptor(byte[] credentialId) { Id = credentialId; }

        /// <summary>
        /// Type property implementation
        /// </summary>
        public MFAPublicKeyCredentialType? Type { get; set; } = MFAPublicKeyCredentialType.PublicKey;

        /// <summary>
        /// ID Property implementation
        /// </summary>
        public byte[] Id { get; set; }

        /// <summary>
        /// Transports property implmentation
        /// </summary>
        public MFAAuthenticatorTransport[] Transports { get; set; }
    };

    /// <summary>
    /// MFAPublicKeyCredentialType enum
    /// </summary>
    public enum MFAPublicKeyCredentialType
    {
        PublicKey
    }

    /// <summary>
    /// MFAAuthenticatorTransport enum
    /// </summary>
    public enum MFAAuthenticatorTransport
    {
        Usb,
        Nfc,
        Ble,
        Internal,
        Lightning
    }

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

    public abstract class KeysRepositoryService
    {
        public abstract string GetUserKey(string upn);
        public abstract string NewUserKey(string upn, string secretkey, string cert = null);
        public abstract bool RemoveUserKey(string upn);
        public abstract X509Certificate2 GetUserCertificate(string upn, bool generatepassword = false);
        public abstract X509Certificate2 CreateCertificate(string upn, int validity, out string strcert, bool generatepassword = false);
        public abstract bool HasStoredKey(string upn);
        public abstract bool HasStoredCertificate(string upn);
    }

    #region WebAuthN
    public interface IWebAuthNConfiguration
    {
        uint Timeout { get; set; }
        int TimestampDriftTolerance { get; set; }
        int ChallengeSize { get; set; }
        string ServerDomain { get; set; }
        string ServerName { get; set; }
        string ServerIcon { get; set; }
        string Origin { get; set; }
       /* string MDSAccessKey { get; set; }
        string MDSCacheDirPath { get; set; } */
    }

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

}
