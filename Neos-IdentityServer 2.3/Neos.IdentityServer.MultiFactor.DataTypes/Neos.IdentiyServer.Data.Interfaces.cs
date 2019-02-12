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
    public enum KeysDataManagerEventKind 
    {
        Get,
        add,
        Remove
    }

    public enum DataRepositoryKind
    {
        ADDS,
        SQL
    }
        
    public interface IDataRepositoryADDSConnection
    {
        bool CheckConnection(string domainname, string username, string password);
        bool CheckAttribute(string domainname, string username, string password, string attributename, bool checkmultivalued);
    }

    public interface IDataRepositorySQLConnection
    {
        bool CheckConnection(string connectionstring);
    }

    public abstract class DataRepositoryService
    {
        public delegate void KeysDataManagerEvent(string user, KeysDataManagerEventKind kind);
        public abstract event KeysDataManagerEvent OnKeyDataEvent;
        public abstract Registration GetUserRegistration(string upn);
        public abstract Registration SetUserRegistration(Registration reg, bool resetkey = false, bool caninsert = true, bool disableoninsert = false);
        public abstract Registration AddUserRegistration(Registration reg, bool resetkey = false, bool canupdate = true, bool disableoninsert = false);
        public abstract bool DeleteUserRegistration(Registration reg, bool dropkey = true);
        public abstract Registration EnableUserRegistration(Registration reg);
        public abstract Registration DisableUserRegistration(Registration reg);
        public abstract RegistrationList GetUserRegistrations(DataFilterObject filter, DataOrderObject order, DataPagingObject paging);
        public abstract RegistrationList GetAllUserRegistrations(DataOrderObject order, bool enabledonly = false);
        public abstract int GetUserRegistrationsCount(DataFilterObject filter);
        public abstract bool HasRegistration(string upn);
        public abstract RegistrationList GetImportUserRegistrations(string domain, string username, string password, string ldappath, DateTime? created, DateTime? modified, string mailattribute, string phoneattribute, PreferredMethod method, bool disableall = false);        
    }

    public abstract class KeysRepositoryService
    {
        public abstract string GetUserKey(string upn);
        public abstract string NewUserKey(string upn, string secretkey, string cert = null);
        public abstract bool RemoveUserKey(string upn);
        public abstract X509Certificate2 GetUserCertificate(string upn);
        public abstract bool HasStoredKey(string upn);
        public abstract bool HasStoredCertificate(string upn);
    }
}
