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
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using Neos.IdentityServer.MultiFactor.Common;
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Neos.IdentityServer.MultiFactor.Samples
{
    #region Custom Data Repository
    public class InMemoryDataRepositoryService : DataRepositoryService, IWebAuthNDataRepositoryService, IDisposable
    {
        internal static MemoryMap<List<MFAUser>> _mfausers = new MemoryMap<List<MFAUser>>("_mfa_ImMemUsers");
        internal static MemoryMap<List<MFAUserCredential>> _mfacredusers = new MemoryMap<List<MFAUserCredential>>("_mfa_ImMemCredUsers");

        public override event KeysDataManagerEvent OnKeyDataEvent;

        /// <summary>
        /// InMemoryDataRepositoryService constructor
        /// </summary>
        public InMemoryDataRepositoryService(BaseDataHost host, int deliverywindow) : base(host, deliverywindow)
        {
            if (!(host is CustomStoreHost))
                throw new ArgumentException("Invalid Host ! : value but be an CustomStoreHost instance");
            _mfausers.Open();           
            _mfacredusers.Open();
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        public void Dispose()
        {
            //  _mfausers.Close();
            // _mfawebusers.Close();
            // _mfacredusers.Close();
        }

        /// <summary>
        /// StoreHost property
        /// </summary>
        private CustomStoreHost StoreHost
        {
            get { return (CustomStoreHost)Host; }
        }

        #region DataRepositoryService
        /// <summary>
        /// GetMFAUser method implementation
        /// </summary>
        public override MFAUser GetMFAUser(string upn)
        {
            if (string.IsNullOrEmpty(upn))
                return null;
            List<MFAUser> _lst = _mfausers.GetData();
            return _lst.FirstOrDefault(s => s.UPN.ToLower().Equals(upn.ToLower()));
        }

        /// <summary>
        /// SetMFAUser method implementation
        /// </summary>
        public override MFAUser SetMFAUser(MFAUser reg, bool resetkey = false, bool caninsert = true, bool disableoninsert = false)
        {
            if (!IsMFAUserRegistered(reg.UPN))
            {
                if (caninsert)
                    return AddMFAUser(reg, resetkey, false);
                else
                    return GetMFAUser(reg.UPN);
            }
            try
            {
                MFAUser usr = GetMFAUser(reg.UPN);
                if (!disableoninsert) // disable change if not explicitely done
                {
                    if (reg.Enabled)
                        usr.Enabled = true;
                    else
                        usr.Enabled = false;
                }
                else
                    usr.Enabled = false;
                usr.IsRegistered = true;
                usr.MailAddress = reg.MailAddress;
                usr.PhoneNumber = reg.PhoneNumber;
                usr.PreferredMethod = reg.PreferredMethod;
                usr.PIN = reg.PIN;
                if (string.IsNullOrEmpty(reg.OverrideMethod))
                    usr.OverrideMethod = string.Empty;
                else
                    usr.OverrideMethod = reg.OverrideMethod;
                if (resetkey)
                    this.OnKeyDataEvent(reg.UPN, KeysDataManagerEventKind.add);
                List<MFAUser> _lst = _mfausers.GetData();
                _lst.Where(s => s.UPN.ToLower().Equals(reg.UPN.ToLower())).ToList().ForEach(s =>
                      {
                          s.Enabled = usr.Enabled;
                          s.MailAddress = usr.MailAddress;
                          s.PhoneNumber = usr.PhoneNumber;
                          s.PreferredMethod = usr.PreferredMethod;
                          s.OverrideMethod = usr.OverrideMethod;
                          s.PIN = usr.PIN;
                      });
                _mfausers.SetData(_lst);
            }                       
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return GetMFAUser(reg.UPN);
        }

        /// <summary>
        /// AddMFAUser method implementation
        /// </summary>
        public override MFAUser AddMFAUser(MFAUser reg, bool resetkey = false, bool canupdate = true, bool disableoninsert = false)
        {
            if (IsMFAUserRegistered(reg.UPN))
            {
                if (canupdate)
                    return SetMFAUser(reg, resetkey, false);
                else
                    return GetMFAUser(reg.UPN);
            }
            try
            {
                MFAUser usr = GetMFAUser(reg.UPN);
                if (disableoninsert) // disable change if not explicitely done
                    reg.Enabled = false;
                reg.IsRegistered = true;
                if (resetkey)
                    this.OnKeyDataEvent(reg.UPN, KeysDataManagerEventKind.add);
                List<MFAUser> _lst = _mfausers.GetData();
                _lst.Add(reg);
                _mfausers.SetData(_lst);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return GetMFAUser(reg.UPN);
        }

        /// <summary>
        /// DeleteMFAUser method implementation
        /// </summary>
        public override bool DeleteMFAUser(MFAUser reg, bool dropkey = true)
        {
            if (!IsMFAUserRegistered(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be deleted ! \r User not found !");
            try
            {
                if (dropkey)
                    this.OnKeyDataEvent(reg.UPN, KeysDataManagerEventKind.Remove);
                List<MFAUser> _lst = _mfausers.GetData();
                _lst.RemoveAll(s => s.UPN.ToLower().Equals(reg.UPN.ToLower()));
                _mfausers.SetData(_lst);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return true;
        }

        /// <summary>
        /// EnableMFAUser method implementation
        /// </summary>
        public override MFAUser EnableMFAUser(MFAUser reg)
        {
            if (!IsMFAUserRegistered(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! \r User not found !");
            try
            {
                reg.Enabled = true;
                reg.IsRegistered = true;
                List<MFAUser> _lst = _mfausers.GetData();
                _lst.Where(s => s.UPN.ToLower().Equals(reg.UPN.ToLower())).ToList().ForEach(s =>
                    { s.IsRegistered = reg.IsRegistered; s.Enabled = reg.Enabled; });
                _mfausers.SetData(_lst);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return reg;
        }

        /// <summary>
        /// DisableMFAUser method implementation
        /// </summary>
        public override MFAUser DisableMFAUser(MFAUser reg)
        {
            if (!IsMFAUserRegistered(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! \r User not found !");
            try
            {
                reg.Enabled = false;
                reg.IsRegistered = true;
                List<MFAUser> _lst = _mfausers.GetData();
                _lst.Where(s => s.UPN.ToLower().Equals(reg.UPN.ToLower())).ToList().ForEach(s =>
                    { s.IsRegistered = reg.IsRegistered; s.Enabled = reg.Enabled; });
                _mfausers.SetData(_lst);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return reg;
        }

        /// <summary>
        /// GetMFAUsers method implementation
        /// </summary>
        public override MFAUserList GetMFAUsers(DataFilterObject filter, DataOrderObject order, DataPagingObject paging)
        {
            return QueryBuilder(filter, order, paging);
        }

        /// <summary>
        /// GetMFAUsersAll method implementation
        /// </summary>
        public override MFAUserList GetMFAUsersAll(DataOrderObject order, bool enabledonly = false)
        {
            DataFilterObject filter = new DataFilterObject();
            filter.EnabledOnly = enabledonly;
            return QueryBuilder(filter, order, null);
        }

        /// <summary>
        /// GetMFAUsersCount method implementation
        /// </summary>
        public override int GetMFAUsersCount(DataFilterObject filter)
        {
            if (filter != null)
                return QueryBuilder(filter, null, null).Count();
            else
            {
                List<MFAUser> _lst = _mfausers.GetData();
                return _lst.Count();
            }
        }

        /// <summary>
        /// IsMFAUserRegistered method implementation
        /// </summary>
        public override bool IsMFAUserRegistered(string upn)
        {
            try
            {
                return (GetMFAUser(upn)!=null);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                return false;
            }
        }

        /// <summary>
        /// QueryBuilder method implementation
        /// </summary>
        private MFAUserList QueryBuilder(DataFilterObject filter, DataOrderObject order, DataPagingObject paging)
        {
            var conditions = LinqFilter.True<MFAUser>();
            if (filter != null)
            {
                if (filter.FilterisActive)
                {
                    switch (filter.FilterField)
                    {
                        case DataFilterField.UserName:
                            switch (filter.FilterOperator)
                            {
                                case DataFilterOperator.Equal:
                                    conditions = conditions.And(e => e.UPN.Equals(filter.FilterValue));
                                    break;
                                case DataFilterOperator.NotEqual:
                                    conditions = conditions.And(e => !e.UPN.Equals(filter.FilterValue));
                                    break;
                                case DataFilterOperator.Contains:
                                    conditions = conditions.And(e => e.UPN.Contains(filter.FilterValue));
                                    break;
                                case DataFilterOperator.NotContains:
                                    conditions = conditions.And(e => !e.UPN.Contains(filter.FilterValue));
                                    break;
                                case DataFilterOperator.StartWith:
                                    conditions = conditions.And(e => e.UPN.StartsWith(filter.FilterValue));
                                    break;
                                case DataFilterOperator.EndsWith:
                                    conditions = conditions.And(e => e.UPN.EndsWith(filter.FilterValue));
                                    break;
                            }
                            break;
                        case DataFilterField.Email:
                            switch (filter.FilterOperator)
                            {
                                case DataFilterOperator.Equal:
                                    conditions = conditions.And(e => e.MailAddress.Equals(filter.FilterValue));
                                    break;
                                case DataFilterOperator.NotEqual:
                                    conditions = conditions.And(e => !e.MailAddress.Equals(filter.FilterValue));
                                    break;
                                case DataFilterOperator.Contains:
                                    conditions = conditions.And(e => e.MailAddress.Contains(filter.FilterValue));
                                    break;
                                case DataFilterOperator.NotContains:
                                    conditions = conditions.And(e => !e.MailAddress.Contains(filter.FilterValue));
                                    break;
                                case DataFilterOperator.StartWith:
                                    conditions = conditions.And(e => e.MailAddress.StartsWith(filter.FilterValue));
                                    break;
                                case DataFilterOperator.EndsWith:
                                    conditions = conditions.And(e => e.MailAddress.EndsWith(filter.FilterValue));
                                    break;
                            }
                            break;
                        case DataFilterField.PhoneNumber:
                            switch (filter.FilterOperator)
                            {
                                case DataFilterOperator.Equal:
                                    conditions = conditions.And(e => e.PhoneNumber.Equals(filter.FilterValue));
                                    break;
                                case DataFilterOperator.NotEqual:
                                    conditions = conditions.And(e => !e.PhoneNumber.Equals(filter.FilterValue));
                                    break;
                                case DataFilterOperator.Contains:
                                    conditions = conditions.And(e => e.PhoneNumber.Contains(filter.FilterValue));
                                    break;
                                case DataFilterOperator.NotContains:
                                    conditions = conditions.And(e => !e.PhoneNumber.Contains(filter.FilterValue));
                                    break;
                                case DataFilterOperator.StartWith:
                                    conditions = conditions.And(e => e.PhoneNumber.StartsWith(filter.FilterValue));
                                    break;
                                case DataFilterOperator.EndsWith:
                                    conditions = conditions.And(e => e.PhoneNumber.EndsWith(filter.FilterValue));
                                    break;
                            }
                            break;
                    }
                    if (filter.EnabledOnly)
                        conditions = conditions.And(e => e.Enabled);
                    if (filter.FilterMethod != PreferredMethod.None)
                        conditions = conditions.And(e => !e.PreferredMethod.Equals(filter.FilterMethod));
                }
                List<MFAUser> _res = null;
                List<MFAUser> _lst = _mfausers.GetData();
                if (order!=null)
                {
                    switch (order.Column)
                    {
                        case DataOrderField.ID:
                            _res = _lst.AsQueryable().Where(conditions).OrderBy(e => e.ID).ToList();
                            break;
                        case DataOrderField.UserName:
                            _res = _lst.AsQueryable().Where(conditions).OrderBy(e => e.UPN).ToList();
                            break;
                        case DataOrderField.Email:
                            _res = _lst.AsQueryable().Where(conditions).OrderBy(e => e.MailAddress).ToList();
                            break;
                        case DataOrderField.Phone:
                            _res = _lst.AsQueryable().Where(conditions).OrderBy(e => e.PhoneNumber).ToList();
                            break;
                        default:
                            _res = _lst.AsQueryable().Where(conditions).ToList();
                            break;
                    }
                }
                else
                    _res = _lst.AsQueryable().Where(conditions).ToList();
                if (paging!=null)
                {
                    if (paging.IsActive)
                    {
                        int toprow = paging.PageSize * (paging.CurrentPage - 1);
                        _res =_res.Skip(toprow).Take(paging.PageSize).ToList();
                    }
                }
                return new MFAUserList(_res);
            }
            else
                return null;
        }
        #endregion

        #region IWebAuthNDataRepositoryService
        /// <summary>
        /// GetUser method implementation
        /// </summary>
        public MFAWebAuthNUser GetUser(int challengesize, string username)
        {
            MFAWebAuthNUser result = new MFAWebAuthNUser()
            {
                Id = CheckSumEncoding.EncodeUserID(challengesize, username),
                Name = username,
                DisplayName = username
            };
            return result;            
        }

        /// <summary>
        /// GetCredentialByCredentialId method implementation
        /// </summary>
        public MFAUserCredential GetCredentialByCredentialId(MFAWebAuthNUser user, string credentialId)
        {
            try
            {
                List<MFAUserCredential> _creds = _mfacredusers.GetData();
                return _creds.FirstOrDefault(s => s.UserId.SequenceEqual(user.Id) && (HexaEncoding.GetHexStringFromByteArray(s.Descriptor.Id)).Equals(credentialId));
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// GetCredentialById method implementation
        /// </summary>
        public MFAUserCredential GetCredentialById(MFAWebAuthNUser user, byte[] id)
        {
            string credsid = HexaEncoding.GetHexStringFromByteArray(id);
            return GetCredentialByCredentialId(user, credsid);
        }

        /// <summary>
        /// GetCredentialsByUser method implementation
        /// </summary>
        public List<MFAUserCredential> GetCredentialsByUser(MFAWebAuthNUser user)  
        {
            try
            { 
                List<MFAUserCredential> _creds = _mfacredusers.GetData();
                return _creds.Where(s => s.UserId.SequenceEqual(user.Id)).ToList();
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// GetCredentialsByUserHandle method implementation
        /// </summary>
        public List<MFAUserCredential> GetCredentialsByUserHandle(MFAWebAuthNUser user, byte[] userHandle)
        {
            List<MFAUserCredential> _lst = GetCredentialsByUser(user);
            return _lst.Where(c => c.UserHandle.SequenceEqual(userHandle)).ToList();
        }

        /// <summary>
        /// GetUsersByCredentialId method implementation
        /// </summary>
        public List<MFAWebAuthNUser> GetUsersByCredentialId(MFAWebAuthNUser user, byte[] credentialId)
        {
            List<MFAWebAuthNUser> _users = new List<MFAWebAuthNUser>();
            string credsid = HexaEncoding.GetHexStringFromByteArray(credentialId);
            MFAUserCredential cred = GetCredentialByCredentialId(user, credsid);
            if (cred != null)
                _users.Add(user);
            return _users;
        }

        /// <summary>
        /// AddCredential method implementation
        /// </summary>
        public bool AddUserCredential(MFAWebAuthNUser user, MFAUserCredential credential)
        {
            try
            { 
                credential.UserId = user.Id;
                List<MFAUserCredential> _lst = _mfacredusers.GetData();
                _lst.Add(credential);
                _mfacredusers.SetData(_lst);
                return true;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// SetUserCredential method implementation
        /// </summary>
        public bool SetUserCredential(MFAWebAuthNUser user, MFAUserCredential credential)
        {
            bool result = false;
            try
            {
                credential.UserId = user.Id;
                List<MFAUserCredential> _lst = _mfacredusers.GetData();
                _lst.Where(s => s.UserId.SequenceEqual(user.Id) && (s.Descriptor.Id.SequenceEqual(credential.Descriptor.Id))).ToList()
                .ForEach(s =>
                {
                    s.AaGuid = credential.AaGuid;
                    s.CredType = credential.CredType;
                    s.Descriptor = credential.Descriptor;
                    s.Descriptor.Id = credential.Descriptor.Id;
                    s.Descriptor.Transports = credential.Descriptor.Transports;
                    s.Descriptor.Type = credential.Descriptor.Type;
                    s.PublicKey = credential.PublicKey;
                    s.RegDate = credential.RegDate;
                    s.SignatureCounter = credential.SignatureCounter;
                    s.UserHandle = credential.UserHandle;
                    s.UserId = credential.UserId;
                    result = true;
                });
                _mfacredusers.SetData(_lst);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// RemoveUserCredential method implementation
        /// </summary>
        public bool RemoveUserCredential(MFAWebAuthNUser user, string credentialId)
        {
            try
            {
                List<MFAUserCredential> _lst = _mfacredusers.GetData();
                int res = _lst.RemoveAll(s => s.UserId.SequenceEqual(user.Id) && (HexaEncoding.GetHexStringFromByteArray(s.Descriptor.Id)).Equals(credentialId));
                _mfacredusers.SetData(_lst);
                return res > 0;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// UpdateCounter method implementation
        /// </summary>
        public void UpdateCounter(MFAWebAuthNUser user, byte[] credentialId, uint counter)
        {
            string credsid = HexaEncoding.GetHexStringFromByteArray(credentialId);
            MFAUserCredential cred = GetCredentialByCredentialId(user, credsid);

            if (cred != null)
            {
                cred.SignatureCounter = counter;
                SetUserCredential(user, cred);
            }
        }
        #endregion
    }
    #endregion

    #region Custom Keys Repository (Keys Only (RNG/ RSA)
    internal class InMemoryKeysRepositoryService : KeysRepositoryService
    {
        internal static MemoryMap<List<MFAUser>> _mfausers = new MemoryMap<List<MFAUser>>("_mfa_ImMemUsers");
        internal static MemoryMap<List<MFAUserKeys>> _mfakeysusers = new MemoryMap<List<MFAUserKeys>>("_mfa_ImMemKey1Users");

        /// <summary>
        /// InMemoryKeysRepositoryService constructor
        /// </summary>
        public InMemoryKeysRepositoryService(BaseDataHost host, int deliverywindow) : base(host, deliverywindow)
        {
            if (!(host is CustomStoreHost))
                throw new ArgumentException("Invalid Host ! : value but be an CustomStoreHost instance");
            _mfausers.Open();
            _mfakeysusers.Open();
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        public void Dispose()
        {
            // _mfausers.Close();
            // _mfakeysusers.Close();
        }

        /// <summary>
        /// StoreHost property
        /// </summary>
        private CustomStoreHost StoreHost
        {
            get { return (CustomStoreHost)Host; }
        }

        #region KeysRepositoryService
        /// <summary>
        /// CreateCertificate method implementation
        /// </summary>
        public override X509Certificate2 CreateCertificate(string upn, string password, int validity)
        {
            return null;
        }

        /// <summary>
        /// GetUserCertificate method implementation
        /// </summary>
        public override X509Certificate2 GetUserCertificate(string upn, string password)
        {
            return null;
        }

        /// <summary>
        /// HasStoredCertificate method implementation
        /// </summary>
        public override string GetUserKey(string upn)
        {
            string result = string.Empty;
            try
            {
                List<MFAUserKeys> _lst = _mfakeysusers.GetData();
                MFAUserKeys _itm = _lst.FirstOrDefault(s => s.UserName.ToLower().Equals(upn.ToLower()) && (!string.IsNullOrEmpty(s.UserKey)));
                if (_itm != null)
                    result = _itm.UserKey;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// NewUserKey method implementation
        /// </summary>
        public override string NewUserKey(string upn, string secretkey, X509Certificate2 cert = null)
        {
            if (!IsMFAUserRegistered(upn.ToLower()))
                return string.Empty;
            if (HasStoredKey(upn.ToLower()))
                DoUpdateUserKey(upn.ToLower(), secretkey);
            else
                DoInsertUserKey(upn.ToLower(), secretkey);
            return secretkey;
        }

        /// <summary>
        /// RemoveUserKey method implementation
        /// </summary>
        public override bool RemoveUserKey(string upn)
        {
            try
            {
                List<MFAUserKeys> _lst = _mfakeysusers.GetData();
                int res = _lst.RemoveAll(s => s.UserName.ToLower().Equals(upn.ToLower()));
                _mfakeysusers.SetData(_lst);
                return res > 0;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// DoInsertUserKey method implementation
        /// </summary>
        private void DoInsertUserKey(string upn, string secretkey)
        {
            List<MFAUserKeys> _lst = _mfakeysusers.GetData();
            MFAUserKeys _itm = new MFAUserKeys()
            {
                UserName = upn.ToLower(),
                UserKey = secretkey
            };
            _lst.Add(_itm);
            _mfakeysusers.SetData(_lst);
        }

        /// <summary>
        /// DoUpdateUserKey method implementation
        /// </summary>
        private void DoUpdateUserKey(string upn, string secretkey)
        {
            try
            {
                List<MFAUserKeys> _lst = _mfakeysusers.GetData();
                _lst.Where(s => s.UserName.ToLower().Equals(upn.ToLower())).ToList() 
                .ForEach(s =>
                {
                    s.UserKey = secretkey;
                });
                _mfakeysusers.SetData(_lst);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return;
        }

        /// <summary>
        /// GetUserKey method implementation
        /// </summary>
        public override bool HasStoredCertificate(string upn)
        {
            return false;
        }

        /// <summary>
        /// HasStoredKey method implementation
        /// </summary>
        public override bool HasStoredKey(string upn)
        {
            bool result = false;  
            try
            {
                List<MFAUserKeys> _lst = _mfakeysusers.GetData();
                result = (_lst.FirstOrDefault(s => s.UserName.ToLower().Equals(upn.ToLower()) && (!string.IsNullOrEmpty(s.UserKey))) != null);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// IsMFAUserRegistered method implementation
        /// </summary>
        private bool IsMFAUserRegistered(string upn)
        {
            try
            {
                if (string.IsNullOrEmpty(upn))
                    return false;
                List<MFAUser> _lst = _mfausers.GetData();
                return (_lst.FirstOrDefault(s => s.UPN.ToLower().Equals(upn.ToLower())) != null);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                return false;
            }
        }
        #endregion
    }
    #endregion

    #region Custom Keys2 Repository (Certificates RSA Per User)
    internal class InMemoryKeys2RepositoryService : KeysRepositoryService, IDisposable
    {
        internal static MemoryMap<List<MFAUser>> _mfausers = new MemoryMap<List<MFAUser>>("_mfa_ImMemUsers");
        internal static MemoryMap<List<MFAUserKeys>> _mfakeysusers = new MemoryMap<List<MFAUserKeys>>("_mfa_ImMemKeysUsers");

        /// <summary>
        /// CustomKeysRepositoryService constructor
        /// </summary>
        public InMemoryKeys2RepositoryService(BaseDataHost host, int deliverywindow) : base(host, deliverywindow)
        {
            if (!(host is CustomStoreHost))
                throw new ArgumentException("Invalid Host ! : value but be an CustomStoreHost instance");
            _mfausers.Open();
            _mfakeysusers.Open();
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        public void Dispose()
        {
            // _mfausers.Close();
            // _mfakeysusers.Close();
        }

        /// <summary>
        /// StoreHost property
        /// </summary>
        private CustomStoreHost StoreHost
        {
            get { return (CustomStoreHost)Host; }
        }

        #region KeysRepositoryService
        /// <summary>
        /// GetUserCertificate method implementation
        /// </summary>
        public override X509Certificate2 GetUserCertificate(string upn, string password)
        {
            X509Certificate2 result = null;
            try
            {
                List<MFAUserKeys> _lst = _mfakeysusers.GetData();
                MFAUserKeys _itm = _lst.FirstOrDefault(s => s.UserName.ToLower().Equals(upn.ToLower()) && (!string.IsNullOrEmpty(s.UserCertificate)));
                if (_itm != null)
                {
                    X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(_itm.UserCertificate), password, X509KeyStorageFlags.EphemeralKeySet);
                   // X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(_itm.UserCertificate), password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
                    result = cert;
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// CreateCertificate implementation
        /// </summary>
        public override X509Certificate2 CreateCertificate(string upn, string password, int validity)
        {
            string pass = string.Empty;
            if (!string.IsNullOrEmpty(password))
                pass = password;
            return Certs.CreateRSAEncryptionCertificateForUser(upn.ToLower(), validity, pass);
        }

        /// <summary>
        /// GetUserKey method implementation
        /// </summary>
        public override string GetUserKey(string upn)
        {
            string result = string.Empty;
            try
            {
                List<MFAUserKeys> _lst = _mfakeysusers.GetData();
                MFAUserKeys _itm = _lst.FirstOrDefault(s => s.UserName.ToLower().Equals(upn.ToLower()) && (!string.IsNullOrEmpty(s.UserKey)));
                if (_itm != null)
                    result = _itm.UserKey;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// NewUserKey method implementation
        /// </summary>
        public override string NewUserKey(string upn, string secretkey, X509Certificate2 cert = null)
        {
            if (!IsMFAUserRegistered(upn.ToLower()))
                return string.Empty;
            if (HasStoredKey(upn.ToLower()))
                DoUpdateUserKey(upn.ToLower(), secretkey);
            else
                DoInsertUserKey(upn.ToLower(), secretkey);
            if (cert != null)
            {
                if (HasStoredCertificate(upn.ToLower()))
                    DoUpdateUserCertificate(upn.ToLower(), cert);
                else
                    DoInsertUserCertificate(upn.ToLower(), cert);
            }
            return secretkey;
        }

        /// <summary>
        /// RemoveUserKey method implementation
        /// </summary>
        public override bool RemoveUserKey(string upn)
        {
            try
            {
                List<MFAUserKeys> _lst = _mfakeysusers.GetData();
                int res = _lst.RemoveAll(s => s.UserName.ToLower().Equals(upn.ToLower()));
                _mfakeysusers.SetData(_lst);
                return res > 0;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// DoInsertUserKey method implementation
        /// </summary>
        private void DoInsertUserKey(string upn, string secretkey)
        {
            List<MFAUserKeys> _lst = _mfakeysusers.GetData();
            MFAUserKeys _itm = new MFAUserKeys()
            {
                UserName = upn.ToLower(),
                UserKey = secretkey,
                UserCertificate = string.Empty
            };
            _lst.Add(_itm);
            _mfakeysusers.SetData(_lst);
        }

        /// <summary>
        /// DoUpdateUserKey method implementation
        /// </summary>
        private void DoUpdateUserKey(string upn, string secretkey)
        {
            try
            {
                List<MFAUserKeys> _lst = _mfakeysusers.GetData();
                _lst.Where(s => s.UserName.ToLower().Equals(upn.ToLower())).ToList()
                .ForEach(s =>
                {
                    s.UserKey = secretkey;
                });
                _mfakeysusers.SetData(_lst);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return;
        }

        /// <summary>
        /// DoInsertUserCertificate method implementation
        /// </summary>
        private void DoInsertUserCertificate(string upn, X509Certificate2 cert)
        {
            List<MFAUserKeys> _lst = _mfakeysusers.GetData();
            try
            {
                MFAUserKeys _itm = new MFAUserKeys()
                {
                    UserName = upn.ToLower(),
                    UserKey = string.Empty,
                    UserCertificate = Convert.ToBase64String(cert.Export(X509ContentType.Pfx, CheckSumEncoding.CheckSumAsString(upn)))
                };
                cert.Reset();
                _lst.Add(_itm);
                _mfakeysusers.SetData(_lst);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// DoUpdateUserCertificate method implementation
        /// </summary>
        private void DoUpdateUserCertificate(string upn, X509Certificate2 cert)
        {
            try
            {
                List<MFAUserKeys> _lst = _mfakeysusers.GetData();
                _lst.Where(s => s.UserName.ToLower().Equals(upn.ToLower())).ToList()
                .ForEach(s =>
                {                   
                    s.UserCertificate = Convert.ToBase64String(cert.Export(X509ContentType.Pfx, CheckSumEncoding.CheckSumAsString(upn)));
                    cert.Reset();
                });
                _mfakeysusers.SetData(_lst);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return;
        }

        /// <summary>
        /// HasStoredCertificate method implementation
        /// </summary>
        public override bool HasStoredCertificate(string upn)
        {
            bool result = false;
            try
            {
                List<MFAUserKeys> _lst = _mfakeysusers.GetData();
                result = (_lst.FirstOrDefault(s => s.UserName.ToLower().Equals(upn.ToLower()) && (!string.IsNullOrEmpty(s.UserCertificate))) != null);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// HasStoredKey method implementation
        /// </summary>
        public override bool HasStoredKey(string upn)
        {
            bool result = false;  
            try
            {
                List<MFAUserKeys> _lst = _mfakeysusers.GetData();
                result = (_lst.FirstOrDefault(s => s.UserName.ToLower().Equals(upn.ToLower()) && (!string.IsNullOrEmpty(s.UserKey))) != null);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return result;
        }


        /// <summary>
        /// IsMFAUserRegistered method implementation
        /// </summary>
        private bool IsMFAUserRegistered(string upn)
        {
            try
            {
                if (string.IsNullOrEmpty(upn))
                    return false;
                List<MFAUser> _lst = _mfausers.GetData();
                return (_lst.FirstOrDefault(s => s.UPN.ToLower().Equals(upn.ToLower())) != null);
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                return false;
            }
        }
        #endregion
    }
    #endregion

    /// <summary>
    /// MFAUserKeys class
    /// </summary>
    [Serializable]
    public class MFAUserKeys
    {
        public string UserName { get; set; }
        public string UserKey { get; set; }
        public string UserCertificate { get; set; }
    }

}
