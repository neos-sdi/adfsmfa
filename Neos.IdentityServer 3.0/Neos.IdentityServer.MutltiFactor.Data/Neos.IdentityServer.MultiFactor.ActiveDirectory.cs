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
using System.Linq;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Neos.IdentityServer.MultiFactor.Data
{
    #region ADDS Service
    internal class ADDSDataRepositoryService : DataRepositoryService, IDataRepositoryADDSConnection, IWebAuthNDataRepositoryService
    {
        private ADDSHost _host;
        private readonly int _deliverywindow = 300;
        private readonly bool _mailismulti = false;
        private readonly bool _phoneismulti = false;

        public override event KeysDataManagerEvent OnKeyDataEvent;

        /// <summary>
        /// AdminService constructor
        /// </summary>
        public ADDSDataRepositoryService(ADDSHost host, string account, string password, int deliverywindow = 3000): base()
        {
            _host = host;
            _deliverywindow = deliverywindow;
            _host.Bind();
            _mailismulti = ADDSUtils.IsMultivaluedAttribute(_host.DomainName, account, password, _host.MailAttribute);
            _phoneismulti = ADDSUtils.IsMultivaluedAttribute(_host.DomainName, account, password, _host.PhoneAttribute);
        }

        public ADDSDataRepositoryService(ADDSHost host, string domain, string account, string password, int deliverywindow = 3000) : base()
        {
            _host = host;
            _deliverywindow = deliverywindow;
            _host.Bind();
            _mailismulti = ADDSUtils.IsMultivaluedAttribute(domain, account, password, _host.MailAttribute);
            _phoneismulti = ADDSUtils.IsMultivaluedAttribute(domain, account, password, _host.PhoneAttribute);
        }

        #region DataRepositoryService
        /// <summary>
        /// GetMFAUser method implementation
        /// </summary>
        public override MFAUser GetMFAUser(string upn)
        {
            MFAUser reg = null;
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, upn))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.MailAttribute);
                        dsusr.PropertiesToLoad.Add(_host.PhoneAttribute);
                        dsusr.PropertiesToLoad.Add(_host.MethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.OverrideMethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.PinAttribute);
                        dsusr.PropertiesToLoad.Add(_host.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            reg = new MFAUser();
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                    reg.UPN = DirEntry.Properties["userPrincipalName"].Value.ToString();
                                    if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.MailAttribute], _mailismulti) != null)
                                    {
                                        reg.MailAddress = ADDSUtils.GetMultiValued(DirEntry.Properties[_host.MailAttribute], _mailismulti);
                                        reg.IsRegistered = true;
                                    }
                                    if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.PhoneAttribute], _phoneismulti) != null)
                                    {
                                        reg.PhoneNumber = ADDSUtils.GetMultiValued(DirEntry.Properties[_host.PhoneAttribute], _phoneismulti);
                                        reg.IsRegistered = true;
                                    }

                                    if (DirEntry.Properties[_host.MethodAttribute].Value != null)
                                    {
                                        reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), DirEntry.Properties[_host.MethodAttribute].Value.ToString(), true);
                                        if (reg.PreferredMethod != PreferredMethod.Choose)
                                            reg.IsRegistered = true;
                                    }
                                    if (DirEntry.Properties[_host.OverrideMethodAttribute].Value != null)
                                    {
                                        try
                                        {
                                            reg.OverrideMethod = DirEntry.Properties[_host.OverrideMethodAttribute].Value.ToString();
                                        }
                                        catch
                                        {
                                            reg.OverrideMethod = string.Empty;
                                        }
                                    }
                                    else reg.OverrideMethod = string.Empty;

                                    if (DirEntry.Properties[_host.PinAttribute].Value != null)
                                    {
                                        try
                                        {
                                            reg.PIN = Convert.ToInt32(DirEntry.Properties[_host.PinAttribute].Value);
                                        }
                                        catch
                                        {
                                            reg.PIN = 0;
                                        }
                                    }
                                    else reg.PIN = 0;

                                    if (DirEntry.Properties[_host.TotpEnabledAttribute].Value != null)
                                    {
                                        reg.Enabled = bool.Parse(DirEntry.Properties[_host.TotpEnabledAttribute].Value.ToString());
                                        reg.IsRegistered = true;
                                    }
                                    if (reg.IsRegistered)
                                        return reg;
                                    else
                                        return null;
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

        /// <summary>
        /// SetMFAUser method implementation
        /// </summary>
        public override MFAUser SetMFAUser(MFAUser reg, bool resetkey = true, bool caninsert = true, bool disableoninsert = false)
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, reg.UPN))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.MailAttribute);
                        dsusr.PropertiesToLoad.Add(_host.PhoneAttribute);
                        dsusr.PropertiesToLoad.Add(_host.MethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.OverrideMethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.PinAttribute);
                        dsusr.PropertiesToLoad.Add(_host.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                ADDSUtils.SetMultiValued(DirEntry.Properties[_host.MailAttribute], _mailismulti, reg.MailAddress);
                                ADDSUtils.SetMultiValued(DirEntry.Properties[_host.PhoneAttribute], _phoneismulti, reg.PhoneNumber);

                                if (!disableoninsert) // disable change if not explicitely done
                                {
                                    if (reg.Enabled)
                                        DirEntry.Properties[_host.TotpEnabledAttribute].Value = true;
                                    else
                                        DirEntry.Properties[_host.TotpEnabledAttribute].Value = false;
                                }
                                else
                                    DirEntry.Properties[_host.TotpEnabledAttribute].Value = false;

                                DirEntry.Properties[_host.MethodAttribute].Value = ((int)reg.PreferredMethod).ToString();
                                DirEntry.Properties[_host.PinAttribute].Value = reg.PIN;
                                if (string.IsNullOrEmpty(reg.OverrideMethod))
                                    DirEntry.Properties[_host.OverrideMethodAttribute].Clear();
                                else
                                    DirEntry.Properties[_host.OverrideMethodAttribute].Value = reg.OverrideMethod.ToString();

                                DirEntry.CommitChanges();
                            };
                        }
                        if (resetkey)
                            this.OnKeyDataEvent(reg.UPN, KeysDataManagerEventKind.add);
                    }
                }
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, reg.UPN))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.MailAttribute);
                        dsusr.PropertiesToLoad.Add(_host.PhoneAttribute);
                        dsusr.PropertiesToLoad.Add(_host.MethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.OverrideMethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.PinAttribute);
                        dsusr.PropertiesToLoad.Add(_host.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                ADDSUtils.SetMultiValued(DirEntry.Properties[_host.MailAttribute], _mailismulti, reg.MailAddress);
                                ADDSUtils.SetMultiValued(DirEntry.Properties[_host.PhoneAttribute], _phoneismulti, reg.PhoneNumber);

                                if (!disableoninsert) // disable change if not explicitely done
                                {
                                    if (reg.Enabled)
                                        DirEntry.Properties[_host.TotpEnabledAttribute].Value = true;
                                    else
                                        DirEntry.Properties[_host.TotpEnabledAttribute].Value = false;
                                }
                                else
                                    DirEntry.Properties[_host.TotpEnabledAttribute].Value = false;

                                DirEntry.Properties[_host.MethodAttribute].Value = ((int)reg.PreferredMethod).ToString();
                                DirEntry.Properties[_host.PinAttribute].Value = reg.PIN;
                                if (string.IsNullOrEmpty(reg.OverrideMethod))
                                    DirEntry.Properties[_host.OverrideMethodAttribute].Clear();
                                else
                                    DirEntry.Properties[_host.OverrideMethodAttribute].Value = reg.OverrideMethod.ToString();

                                DirEntry.CommitChanges();
                            };
                            if (resetkey)
                                this.OnKeyDataEvent(reg.UPN, KeysDataManagerEventKind.add);
                        }
                    }
                }
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, reg.UPN))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(_host.MailAttribute);
                        dsusr.PropertiesToLoad.Add(_host.PhoneAttribute);
                        dsusr.PropertiesToLoad.Add(_host.MethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.OverrideMethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.PinAttribute);
                        dsusr.PropertiesToLoad.Add(_host.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                ADDSUtils.SetMultiValued(DirEntry.Properties[_host.MailAttribute], _mailismulti, string.Empty);
                                ADDSUtils.SetMultiValued(DirEntry.Properties[_host.PhoneAttribute], _phoneismulti, string.Empty);
                                DirEntry.Properties[_host.TotpEnabledAttribute].Clear();
                                DirEntry.Properties[_host.MethodAttribute].Clear();
                                DirEntry.Properties[_host.OverrideMethodAttribute].Clear();
                                DirEntry.Properties[_host.PinAttribute].Clear();
                                DirEntry.CommitChanges();
                            };
                            if (dropkey)
                                this.OnKeyDataEvent(reg.UPN, KeysDataManagerEventKind.Remove);
                        }
                    }
                }
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, reg.UPN))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                DirEntry.Properties[_host.TotpEnabledAttribute].Value = true;
                                DirEntry.CommitChanges();
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
            return GetMFAUser(reg.UPN);
        }

        /// <summary>
        /// DisableMFAUser method implementation
        /// </summary>
        public override MFAUser DisableMFAUser(MFAUser reg)
        {
            if (!IsMFAUserRegistered(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! User not found !");
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, reg.UPN))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                DirEntry.Properties[_host.TotpEnabledAttribute].Value = false;
                                DirEntry.CommitChanges();
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
            return GetMFAUser(reg.UPN);
        }

        /// <summary>
        /// GetMFAUsers method implementation
        /// </summary>
        public override MFAUserList GetMFAUsers(DataFilterObject filter, DataOrderObject order, DataPagingObject paging)
        {
            Dictionary<int, string> fliedlsvalues = new Dictionary<int, string> 
            {
                {0, " userprincipalname"},
                {1, _host.MailAttribute},
                {2, _host.PhoneAttribute}
            };

            Dictionary<int, string> operatorsvalues = new Dictionary<int, string> 
            { 
                {0, "({0}={1})"},
                {1, "({0}={1}*)"},
                {2, "({0}=*{1}*)"},
                {3, "(!({0}={1}))"},
                {4, "({0}=*{1})"},
                {5, "(!({0}=*{1}*))"}
            };

            Dictionary<int, string> nulloperatorsvalues = new Dictionary<int, string> 
            { 
                {0, "(!{0}=*)"},
                {1, "(!{0}=*)"},
                {2, "(!{0}=*)"},
                {3, "({0}=*)"},
                {4, "({0}=*)"},
                {5, "({0}=*)"}
            };

            Dictionary<int, string> methodvalues = new Dictionary<int, string> 
            { 
                {0, "(" + _host.MethodAttribute + "=0)"},
                {1, "(" + _host.MethodAttribute + "=1)"},
                {2, "(" + _host.MethodAttribute + "=2)"},
                {3, "(" + _host.MethodAttribute + "=3)"},
            };

            string qryldap = "(&(objectCategory=user)(objectClass=user)";
            bool hasparameter = string.Empty != filter.FilterValue;
            if (filter.FilterisActive)
            {
                if (hasparameter)
                {
                    string strfields = string.Empty;
                    string stroperator = string.Empty;
                    fliedlsvalues.TryGetValue((int)filter.FilterField, out strfields);
                    if (filter.FilterValue != null)
                    {
                        operatorsvalues.TryGetValue((int)filter.FilterOperator, out stroperator);
                        qryldap += string.Format(stroperator, strfields, filter.FilterValue);
                    }
                    else
                    {
                        nulloperatorsvalues.TryGetValue((int)filter.FilterOperator, out stroperator);
                        qryldap += string.Format(stroperator, strfields);
                    }
                }

                if (filter.FilterMethod != PreferredMethod.None)
                {
                    string strmethod = string.Empty;
                    methodvalues.TryGetValue((int)filter.FilterMethod, out strmethod);
                    qryldap += strmethod;
                }
                if (filter.EnabledOnly)
                {
                    qryldap += "(" + _host.TotpEnabledAttribute + "=" + true.ToString().ToUpper() + ")";
                }
                qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
            }
            else
            {
                qryldap += "(userprincipalname=*)";
                qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
            }
            qryldap += ")";


            MFAUserList registrations = new MFAUserList();
            try
            {
                foreach (ADDSHostForest f in _host.Forests)
                {
                    try
                    {
                        using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(f.ForestDNS, _host.Account, _host.Password))
                        {
                            using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                            {
                                dsusr.PropertiesToLoad.Clear();
                                dsusr.PropertiesToLoad.Add("objectGUID");
                                dsusr.PropertiesToLoad.Add("userPrincipalName");
                                dsusr.PropertiesToLoad.Add("whenCreated");
                                dsusr.PropertiesToLoad.Add(_host.MailAttribute);
                                dsusr.PropertiesToLoad.Add(_host.PhoneAttribute);
                                dsusr.PropertiesToLoad.Add(_host.MethodAttribute);
                                dsusr.PropertiesToLoad.Add(_host.OverrideMethodAttribute);
                                dsusr.PropertiesToLoad.Add(_host.PinAttribute);
                                dsusr.PropertiesToLoad.Add(_host.TotpEnabledAttribute);
                                dsusr.SizeLimit = _host.MaxRows;
                                dsusr.ReferralChasing = ReferralChasingOption.All;

                                switch (order.Column)
                                {
                                    case DataOrderField.UserName:
                                        dsusr.Sort.PropertyName = "userPrincipalName";
                                        break;
                                    case DataOrderField.Email:
                                        dsusr.Sort.PropertyName = _host.MailAttribute;
                                        break;
                                    case DataOrderField.Phone:
                                        dsusr.Sort.PropertyName = _host.PhoneAttribute;
                                        break;
                                    default:
                                        dsusr.Sort.PropertyName = "objectGUID";
                                        break;
                                }
                                dsusr.Sort.Direction = order.Direction;
                                SearchResultCollection src = dsusr.FindAll();

                                foreach (SearchResult sr in src)
                                {
                                    MFAUser reg = new MFAUser();
                                    using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                                    {
                                        if (DirEntry.Properties["objectGUID"].Value != null)
                                        {
                                            reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                            reg.UPN = DirEntry.Properties["userPrincipalName"].Value.ToString();
                                            if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.MailAttribute], _mailismulti) != null)
                                            {
                                                reg.MailAddress = ADDSUtils.GetMultiValued(DirEntry.Properties[_host.MailAttribute], _mailismulti);
                                                reg.IsRegistered = true;
                                            }
                                            if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.PhoneAttribute], _phoneismulti) != null)
                                            {
                                                reg.PhoneNumber = ADDSUtils.GetMultiValued(DirEntry.Properties[_host.PhoneAttribute], _phoneismulti);
                                                reg.IsRegistered = true;
                                            }
                                            if (DirEntry.Properties[_host.MethodAttribute].Value != null)
                                            {
                                                reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), DirEntry.Properties[_host.MethodAttribute].Value.ToString(), true);
                                                if (reg.PreferredMethod != PreferredMethod.Choose)
                                                    reg.IsRegistered = true;
                                            }
                                            if (DirEntry.Properties[_host.OverrideMethodAttribute].Value != null)
                                            {
                                                try
                                                {
                                                    reg.OverrideMethod = DirEntry.Properties[_host.OverrideMethodAttribute].Value.ToString();
                                                }
                                                catch
                                                {
                                                    reg.OverrideMethod = string.Empty;
                                                }
                                            }
                                            else reg.OverrideMethod = string.Empty;

                                            if (DirEntry.Properties[_host.PinAttribute].Value != null)
                                            {
                                                try
                                                {
                                                    reg.PIN = Convert.ToInt32(DirEntry.Properties[_host.PinAttribute].Value);
                                                }
                                                catch
                                                {
                                                    reg.PIN = 0;
                                                }
                                            }
                                            else reg.PIN = 0;

                                            if (DirEntry.Properties[_host.TotpEnabledAttribute].Value != null)
                                            {
                                                reg.Enabled = bool.Parse(DirEntry.Properties[_host.TotpEnabledAttribute].Value.ToString());
                                                reg.IsRegistered = true;
                                            }
                                            if (reg.IsRegistered)
                                                registrations.Add(reg);
                                        }
                                    };
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5001);
                        DataLog.WriteEntry("Forest ADDS : " + f.ForestDNS + " discarded !!!", System.Diagnostics.EventLogEntryType.Error, 5001);
                    }
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return SortAndPageResults(registrations, order, paging);
        }

        /// <summary>
        /// SortAndPageResults method implementation
        /// </summary>
        private MFAUserList SortAndPageResults(MFAUserList registrations, DataOrderObject order, DataPagingObject paging)
        {
            List<MFAUser> data = null;
            if (paging.IsActive)
                data = registrations.Skip<MFAUser>(((paging.CurrentPage - 1) * paging.PageSize)).Take<MFAUser>(paging.PageSize).ToList<MFAUser>();
            else
                data = registrations;
            switch (order.Column)
            {
                case DataOrderField.ID:
                    if (order.Direction==SortDirection.Ascending)
                        return new MFAUserList(data.OrderBy(c => c.ID).ToList<MFAUser>());
                    else
                        return new MFAUserList(data.OrderByDescending(c => c.ID).ToList<MFAUser>());
                case DataOrderField.UserName:
                    if (order.Direction == SortDirection.Ascending)
                        return new MFAUserList(data.OrderBy(c => c.UPN).ToList<MFAUser>());
                    else
                        return new MFAUserList(data.OrderByDescending(c => c.UPN).ToList<MFAUser>());
                case DataOrderField.Email:
                    if (order.Direction == SortDirection.Ascending)
                        return new MFAUserList(data.OrderBy(c => c.MailAddress).ToList<MFAUser>());
                    else
                        return new MFAUserList(data.OrderByDescending(c => c.MailAddress).ToList<MFAUser>());
                case DataOrderField.Phone:
                    if (order.Direction == SortDirection.Ascending)
                        return new MFAUserList(data.OrderBy(c => c.PhoneNumber).ToList<MFAUser>());
                    else
                        return new MFAUserList(data.OrderByDescending(c => c.PhoneNumber).ToList<MFAUser>());
                case DataOrderField.None:
                default:
                    return new MFAUserList(data);
            }
        }

        /// <summary>
        /// GetAllMFAUsers method implementation
        /// </summary>
        public override MFAUserList GetMFAUsersAll(DataOrderObject order, bool enabledonly = false)
        {
            MFAUserList registrations = new MFAUserList();
            try
            {
                foreach (ADDSHostForest f in _host.Forests)
                {
                    using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(f.ForestDNS, _host.Account, _host.Password))
                    {
                        string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=*)";
                        if (enabledonly)
                            qryldap += "(" + _host.TotpEnabledAttribute + "=" + true.ToString().ToUpper() + ")";
                        qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
                        qryldap += ")";

                        using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                        {
                            dsusr.PropertiesToLoad.Clear();
                            dsusr.PropertiesToLoad.Add("objectGUID");
                            dsusr.PropertiesToLoad.Add("userPrincipalName");
                            dsusr.PropertiesToLoad.Add(_host.MailAttribute);
                            dsusr.PropertiesToLoad.Add(_host.PhoneAttribute);
                            dsusr.PropertiesToLoad.Add(_host.MethodAttribute);
                            dsusr.PropertiesToLoad.Add(_host.OverrideMethodAttribute);
                            dsusr.PropertiesToLoad.Add(_host.PinAttribute);
                            dsusr.PropertiesToLoad.Add(_host.TotpEnabledAttribute);
                            dsusr.SizeLimit = _host.MaxRows;
                            dsusr.ReferralChasing = ReferralChasingOption.All;

                            switch (order.Column)
                            {
                                case DataOrderField.UserName:
                                    dsusr.Sort.PropertyName = "userPrincipalName";
                                    break;
                                case DataOrderField.Email:
                                    dsusr.Sort.PropertyName = _host.MailAttribute;
                                    break;
                                case DataOrderField.Phone:
                                    dsusr.Sort.PropertyName = _host.PhoneAttribute;
                                    break;
                                default:
                                    dsusr.Sort.PropertyName = "objectGUID";
                                    break;
                            }
                            dsusr.Sort.Direction = order.Direction;

                            SearchResultCollection src = dsusr.FindAll();
                            if (src != null)
                            {
                                foreach (SearchResult sr in src)
                                {
                                    MFAUser reg = new MFAUser();
                                    using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                                    {
                                        if (DirEntry.Properties["objectGUID"].Value != null)
                                        {
                                            reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                            reg.UPN = DirEntry.Properties["userPrincipalName"].Value.ToString();

                                            if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.MailAttribute], _mailismulti) != null)
                                            {
                                                reg.MailAddress = ADDSUtils.GetMultiValued(DirEntry.Properties[_host.MailAttribute], _mailismulti);
                                                reg.IsRegistered = true;
                                            }
                                            if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.PhoneAttribute], _phoneismulti) != null)
                                            {
                                                reg.PhoneNumber = ADDSUtils.GetMultiValued(DirEntry.Properties[_host.PhoneAttribute], _phoneismulti);
                                                reg.IsRegistered = true;
                                            }

                                            if (DirEntry.Properties[_host.MethodAttribute].Value != null)
                                            {
                                                reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), DirEntry.Properties[_host.MethodAttribute].Value.ToString(), true);
                                                if (reg.PreferredMethod != PreferredMethod.Choose)
                                                    reg.IsRegistered = true;
                                            }
                                            if (DirEntry.Properties[_host.OverrideMethodAttribute].Value != null)
                                            {
                                                try
                                                {
                                                    reg.OverrideMethod = DirEntry.Properties[_host.OverrideMethodAttribute].Value.ToString();
                                                }
                                                catch
                                                {
                                                    reg.OverrideMethod = string.Empty;
                                                }
                                            }
                                            else reg.OverrideMethod = string.Empty;

                                            if (DirEntry.Properties[_host.PinAttribute].Value != null)
                                            {
                                                try
                                                {
                                                    reg.PIN = Convert.ToInt32(DirEntry.Properties[_host.PinAttribute].Value);
                                                }
                                                catch
                                                {
                                                    reg.PIN = 0;
                                                }
                                            }
                                            else reg.PIN = 0;

                                            if (DirEntry.Properties[_host.TotpEnabledAttribute].Value != null)
                                            {
                                                reg.Enabled = bool.Parse(DirEntry.Properties[_host.TotpEnabledAttribute].Value.ToString());
                                                reg.IsRegistered = true;
                                            }
                                            if (reg.IsRegistered)
                                                registrations.Add(reg);
                                        }
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return SortAndPageResults(registrations, order, null); 
        }

        /// <summary>
        /// GetMFAUsersCount method implementation
        /// </summary>
        public override int GetMFAUsersCount(DataFilterObject filter)
        {
            Dictionary<int, string> fliedlsvalues = new Dictionary<int, string> 
            {
                {0, " userprincipalname"} ,
                {1, _host.MailAttribute},
                {2, _host.PhoneAttribute}
            };

            Dictionary<int, string> operatorsvalues = new Dictionary<int, string> 
            { 
                {0, "({0}={1})"},
                {1, "({0}={1}*)"},
                {2, "({0}=*{1}*)"},
                {3, "(!({0}={1}))"},
                {4, "({0}=*{1})"},
                {5, "(!({0}=*{1}*))"}
            };

            Dictionary<int, string> nulloperatorsvalues = new Dictionary<int, string> 
            { 
                {0, "(!{0}=*)"},
                {1, "(!{0}=*)"},
                {2, "(!{0}=*)"},
                {3, "({0}=*)"},
                {4, "({0}=*)"},
                {5, "({0}=*)"}
            };

            Dictionary<int, string> methodvalues = new Dictionary<int, string> 
            { 
                {0, "(" + _host.MethodAttribute + "=0)"},
                {1, "(" + _host.MethodAttribute + "=1)"},
                {2, "(" + _host.MethodAttribute + "=2)"},
                {3, "(" + _host.MethodAttribute + "=3)"},
                {4, "(" + _host.MethodAttribute + "=4)"}
            };

            string qryldap = "(&(objectCategory=user)(objectClass=user)";
            bool hasparameter = string.Empty != filter.FilterValue;
            if (filter.FilterisActive)
            {
                if (hasparameter)
                {
                    string strfields = string.Empty;
                    string stroperator = string.Empty;
                    fliedlsvalues.TryGetValue((int)filter.FilterField, out strfields);
                    if (filter.FilterValue != null)
                    {
                        operatorsvalues.TryGetValue((int)filter.FilterOperator, out stroperator);
                        qryldap += string.Format(stroperator, strfields, filter.FilterValue);
                    }
                    else
                    {
                        nulloperatorsvalues.TryGetValue((int)filter.FilterOperator, out stroperator);
                        qryldap += string.Format(stroperator, strfields);
                    }
                }

                if (filter.FilterMethod != PreferredMethod.None)
                {
                    string strmethod = string.Empty;
                    methodvalues.TryGetValue((int)filter.FilterMethod, out strmethod);
                    qryldap += strmethod;
                }
                if (filter.EnabledOnly)
                {
                    qryldap += "(" + _host.TotpEnabledAttribute + "=" + true.ToString().ToUpper() + ")";
                }
                qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
            }
            else
            {
                qryldap += "(userprincipalname=*)";
                qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
            }
            qryldap += ")";

            int count = 0;

            try
            {
                foreach (ADDSHostForest f in _host.Forests)
                {
                    try
                    {
                        using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(f.ForestDNS, _host.Account, _host.Password))
                        {
                            {
                                using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                                {
                                    dsusr.PropertiesToLoad.Clear();
                                    dsusr.PropertiesToLoad.Add("objectGUID");
                                    dsusr.SizeLimit = _host.MaxRows;
                                    dsusr.ReferralChasing = ReferralChasingOption.All;

                                    // filtrer IsRegistered
                                    SearchResultCollection src = dsusr.FindAll();
                                    if (src != null)
                                    {
                                        foreach (SearchResult sr in src)
                                        {
                                            MFAUser reg = new MFAUser();
                                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                                            {
                                                bool IsRegistered = false;
                                                if (DirEntry.Properties["objectGUID"].Value != null)
                                                {

                                                    if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.MailAttribute], _mailismulti) != null)
                                                        IsRegistered = true;
                                                    if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.PhoneAttribute], _phoneismulti) != null)
                                                        IsRegistered = true;

                                                    if (DirEntry.Properties[_host.MethodAttribute].Value != null)
                                                    {
                                                        PreferredMethod PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), DirEntry.Properties[_host.MethodAttribute].Value.ToString(), true);
                                                        if (PreferredMethod != PreferredMethod.Choose)
                                                            IsRegistered = true;
                                                    }
                                                    if (DirEntry.Properties[_host.OverrideMethodAttribute].Value != null)
                                                    {
                                                        try
                                                        {
                                                            reg.OverrideMethod = DirEntry.Properties[_host.OverrideMethodAttribute].Value.ToString();
                                                        }
                                                        catch
                                                        {
                                                            reg.OverrideMethod = string.Empty;
                                                        }
                                                    }
                                                    else reg.OverrideMethod = string.Empty;

                                                    if (DirEntry.Properties[_host.PinAttribute].Value != null)
                                                    {
                                                        try
                                                        {
                                                            reg.PIN = Convert.ToInt32(DirEntry.Properties[_host.PinAttribute].Value);
                                                        }
                                                        catch
                                                        {
                                                            reg.PIN = 0;
                                                        }
                                                    }
                                                    else reg.PIN = 0;

                                                    if (DirEntry.Properties[_host.TotpEnabledAttribute].Value != null)
                                                        IsRegistered = true;
                                                    if (IsRegistered)
                                                        count++;
                                                }
                                            };
                                        }

                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5001);
                        DataLog.WriteEntry("Forest ADDS : " + f.ForestDNS + " discarded !!!", System.Diagnostics.EventLogEntryType.Error, 5001);
                    }
                }
                return count;
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// ImportMFAUsers
        /// </summary>
        public override MFAUserList ImportMFAUsers(string domain, string username, string password, string ldappath, DateTime? created, DateTime? modified, string mailattribute, string phoneattribute, PreferredMethod meth, bool disableall = false)
        {
            if (!string.IsNullOrEmpty(ldappath))
            {
                ldappath = ldappath.Replace("ldap://", "LDAP://");
                if (!ldappath.StartsWith("LDAP://"))
                    ldappath = "LDAP://" + ldappath;
            }
            MFAUserList registrations = new MFAUserList();
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(domain, username, password, ldappath))
                {
                    string qryldap = string.Empty;
                    qryldap  = "(&";
                    qryldap     += "(objectCategory=user)(objectClass=user)(userprincipalname=*)";
                    if (created.HasValue)
                        qryldap += "(whenCreated>=" + created.Value.ToString("yyyyMMddHHmmss.0Z")+")";
                    if (modified.HasValue)
                        qryldap += "(whenChanged>=" + modified.Value.ToString("yyyyMMddHHmmss.0Z")+")";
                    qryldap += ")";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("userAccountControl");
                        dsusr.ReferralChasing = ReferralChasingOption.All;

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
                                using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                                {
                                    if (DirEntry.Properties["objectGUID"].Value != null)
                                    {
                                        reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                        if (DirEntry.Properties["userPrincipalName"].Value != null)
                                        {
                                            reg.UPN = DirEntry.Properties["userPrincipalName"].Value.ToString();

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
                                                reg.Enabled = ((v & 2)==0);
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

        /// <summary>
        /// IsMFAUserRegistered method implementation
        /// </summary>
        public override bool IsMFAUserRegistered(string upn)
        {
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, upn))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(_host.MailAttribute);
                        dsusr.PropertiesToLoad.Add(_host.PhoneAttribute);
                        dsusr.PropertiesToLoad.Add(_host.MethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.OverrideMethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.PinAttribute);
                        dsusr.PropertiesToLoad.Add(_host.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr == null)
                            return false;
                        using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                        {
                            if (DirEntry.Properties["objectGUID"].Value != null)
                            {
                                if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.MailAttribute], _mailismulti) != null)
                                    return true;
                                if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.PhoneAttribute], _phoneismulti) != null)
                                    return true;
                                if (DirEntry.Properties[_host.MethodAttribute].Value != null)
                                    return true;
                                if (DirEntry.Properties[_host.TotpEnabledAttribute].Value != null)
                                    return true;
                                if (DirEntry.Properties[_host.OverrideMethodAttribute].Value != null)
                                    return true;
                                if (DirEntry.Properties[_host.PinAttribute].Value != null)
                                    return true;
                            }
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region IWebAuthNDataRepositoryService
        /// <summary>
        /// GetUser method implementation
        /// </summary>
        public MFAWebAuthNUser GetUser(int challengesize, string username)
        {
            MFAWebAuthNUser result = null;
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, username))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + username + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                string upn = DirEntry.Properties["userPrincipalName"].Value.ToString();
                                result = new MFAWebAuthNUser()
                                {
                                    DisplayName = upn,
                                    Name = upn,
                                    Id = CheckSumEncoding.EncodeUserID(challengesize, username)
                                };
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
            return result;
        }

        /// <summary>
        /// GetUsersByCredentialId method implmentation
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
        /// GetCredentialsByUser method implementation
        /// </summary>
        public List<MFAUserCredential> GetCredentialsByUser(MFAWebAuthNUser user)
        {
            List<MFAUserCredential> _lst = new List<MFAUserCredential>();
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, user.Name))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + user.Name + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.PublicKeyCredentialAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            ResultPropertyValueCollection xcoll = sr.Properties[_host.PublicKeyCredentialAttribute];
                            foreach (string s in xcoll)
                            {
                                WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization(_host);
                                MFAUserCredential cred = ser.DeserializeCredentials(s, user.Name);
                                _lst.Add(cred);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return _lst;
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
        /// GetCredentialById method implementation
        /// </summary>
        public MFAUserCredential GetCredentialById(MFAWebAuthNUser user, byte[] credentialId)
        {
            string credsid = HexaEncoding.GetHexStringFromByteArray(credentialId);
            MFAUserCredential cred = GetCredentialByCredentialId(user, credsid);
            return cred;
        }

        /// <summary>
        /// GetCredentialByCredentialId method implementation
        /// </summary>
        public MFAUserCredential GetCredentialByCredentialId(MFAWebAuthNUser user, string  credentialid)
        {
            MFAUserCredential result = null;
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, user.Name))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + user.Name + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.PublicKeyCredentialAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            ResultPropertyValueCollection xcoll = sr.Properties[_host.PublicKeyCredentialAttribute];
                            foreach (string s in xcoll)
                            {
                                WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization(_host);
                                MFAUserCredential cred = ser.DeserializeCredentials(s, user.Name);
                                if (HexaEncoding.GetHexStringFromByteArray(cred.Descriptor.Id).Equals(credentialid))
                                {
                                    result = cred;
                                    break;
                                }
                            }
                        }
                    }
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

        /// <summary>
        /// AddUserCredential method implementation
        /// </summary>
        public bool AddUserCredential(MFAWebAuthNUser user, MFAUserCredential credential)
        {           
            credential.UserId = user.Id;
            WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization(_host);
            string value = ser.SerializeCredentials(credential, user.Name);
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, user.Name))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + user.Name + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.PublicKeyCredentialAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                DirEntry.Properties[_host.PublicKeyCredentialAttribute].Add(value);
                                DirEntry.CommitChanges();
                            };
                            return true;
                        }
                        else
                            return false;
                    }
                }
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
            credential.UserId = user.Id;
            WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization(_host);
            string newvalue = ser.SerializeCredentials(credential, user.Name);
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, user.Name))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + user.Name + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.PublicKeyCredentialAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                bool updated = false;
                                ResultPropertyValueCollection pcoll = sr.Properties[_host.PublicKeyCredentialAttribute];

                                for (int i = 0; i < pcoll.Count; i++)
                                {
                                    string s = pcoll[i].ToString();
                                    MFAUserCredential usr = ser.DeserializeCredentials(s, user.Name);
                                    if (usr.Descriptor.Id.SequenceEqual(credential.Descriptor.Id))
                                    {
                                        DirEntry.Properties[_host.PublicKeyCredentialAttribute][i] = newvalue;
                                        updated = true;
                                        break;
                                    }
                                }
                                if (updated)
                                    DirEntry.CommitChanges();
                            }
                            return true;
                        }
                        else
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// RemoveUserCredential method implementation
        /// </summary>
        public bool RemoveUserCredential(MFAWebAuthNUser user, string credentialid)
        {
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, user.Name))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + user.Name + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.PublicKeyCredentialAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            string idtodelete = string.Empty;
                            ResultPropertyValueCollection xcoll = sr.Properties[_host.PublicKeyCredentialAttribute];
                            foreach (string s in xcoll)
                            {
                                WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization(_host);
                                MFAUserCredential usr = ser.DeserializeCredentials(s, user.Name);
                                if (HexaEncoding.GetHexStringFromByteArray(usr.Descriptor.Id).Equals(credentialid))
                                {
                                    idtodelete = s;
                                    break;
                                }
                            }
                            if (idtodelete != string.Empty)
                            {
                                using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                                {
                                    PropertyValueCollection props = DirEntry.Properties[_host.PublicKeyCredentialAttribute];
                                    props.Remove(idtodelete);
                                    DirEntry.CommitChanges();
                                };
                            }
                            return true;
                        }
                        else
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region IDataRepositoryADDSConnection
        /// <summary>
        /// CheckConnection method implementation
        /// </summary>
        public bool CheckConnection(string domainname, string username, string password)
        {
            bool res = false;
            using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(domainname, username, password))
            {
                Guid gd = rootdir.Guid;
                res = true;
            }
            return res;
        }

        /// <summary>
        /// CheckAttribute method implmentation
        /// </summary>
        public bool CheckAttribute(string domainname, string username, string password, string attributename, int multivalued)
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
                       if (property!=null)
                       {
                            if (property.Name.Equals(attributename))
                            {
                                switch (multivalued)
                                {
                                    default:
                                        return property.IsSingleValued;
                                    case 1:
                                        return true;
                                    case 2:
                                        return !property.IsSingleValued;
                                }
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

    #region ADDS Keys Service
    internal class ADDSKeysRepositoryService: KeysRepositoryService
     {
        private ADDSHost _host;
        private readonly bool _keysismulti = false;
        /// <summary>
         /// ADDSKeysRepositoryService constructor
        /// </summary>
        public ADDSKeysRepositoryService(MFAConfig cfg)
        {
            _host = cfg.Hosts.ActiveDirectoryHost;
            _keysismulti = ADDSUtils.IsMultivaluedAttribute(_host.DomainName, _host.Account, _host.Password, _host.KeyAttribute);
        }

        #region Keys Management
        /// <summary>
        /// GetUserKey method implmentation
        /// </summary>
        public override string GetUserKey(string upn)
        {
             string ret = string.Empty;
             try
             {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, upn))
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(" + _host.KeyAttribute + "=*)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.KeyAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    ret = DirEntry.Properties[_host.KeyAttribute].Value.ToString();
                                }
                                else
                                    return string.Empty;
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
             return ret;
         }

         /// <summary>
         /// NewUserKey method implmentation
         /// </summary>
         public override string NewUserKey(string upn, string secretkey, X509Certificate2 cert = null)
         {
             string ret = string.Empty;
             try
             {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, upn))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.KeyAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                DirEntry.Properties[_host.KeyAttribute].Value = secretkey;
                                DirEntry.CommitChanges();
                                ret = secretkey;
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
             return ret;
         }

         /// <summary>
         /// RemoveUserKey method implmentation
         /// </summary>
         public override bool RemoveUserKey(string upn)
         {
             bool ret = false;
             try
             {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, upn))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.KeyAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    DirEntry.Properties[_host.KeyAttribute].Clear();
                                    DirEntry.Properties[_host.RSACertificateAttribute].Clear();
                                    PropertyValueCollection props = DirEntry.Properties[_host.PublicKeyCredentialAttribute];
                                    props.Clear();
                                    DirEntry.CommitChanges();
                                    ret = true;
                                }
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
             return ret;
         }

         /// <summary>
         /// GetUserCertificate implementation
         /// </summary>
         public override X509Certificate2 GetUserCertificate(string upn, bool generatepassword = false)
         {
             return null;
         }

         /// <summary>
         /// CreateCertificate implementation
         /// </summary>
         public override X509Certificate2 CreateCertificate(string upn, int validity, bool generatepassword = false)
         {
             return null;
         }


         /// <summary>
         /// HasStoredKey method implmentation 
         /// </summary>
         public override bool HasStoredKey(string upn)
         {
             return false;
         }

         /// <summary>
         /// HasStoredCertificate method implmentation
         /// </summary>
         public override bool HasStoredCertificate(string upn)
         {
             return false;

         }
         #endregion
    }
    #endregion

    #region ADDS Keys2 Service
    internal class ADDSKeys2RepositoryService : KeysRepositoryService
    {
        private ADDSHost _host;
        private readonly bool _keysisbinary = false;
        /// <summary>
        /// ADDSKeysRepositoryService constructor
        /// </summary>
        public ADDSKeys2RepositoryService(MFAConfig cfg)
        {
            _host = cfg.Hosts.ActiveDirectoryHost;
            _keysisbinary = ADDSUtils.IsBinaryAttribute(_host.DomainName, _host.Account, _host.Password, _host.RSACertificateAttribute);
        }

        #region Keys Management
        /// <summary>
        /// GetUserKey method implmentation
        /// </summary>
        public override string GetUserKey(string upn)
        {
            string ret = string.Empty;
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, upn))
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(" + _host.KeyAttribute + "=*)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.KeyAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    ret = DirEntry.Properties[_host.KeyAttribute].Value.ToString();
                                }
                                else
                                    return string.Empty;
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
            return ret;
        }

        /// <summary>
        /// NewUserKey method implmentation
        /// </summary>
        public override string NewUserKey(string upn, string secretkey, X509Certificate2 cert = null)
        {
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, upn))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.KeyAttribute);
                        dsusr.PropertiesToLoad.Add(_host.RSACertificateAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                DirEntry.Properties[_host.KeyAttribute].Value = secretkey;
                                byte[] data = cert.Export(X509ContentType.Pfx, CheckSumEncoding.CheckSumAsString(upn));
                                try
                                {
                                    if (_keysisbinary)
                                        DirEntry.Properties[_host.RSACertificateAttribute].Value = data;
                                    else
                                        DirEntry.Properties[_host.RSACertificateAttribute].Value = Convert.ToBase64String(data);
                                    DirEntry.CommitChanges();
                                }
                                finally
                                {
                                    cert.Reset();
                                }
                                return secretkey;
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

        /// <summary>
        /// RemoveUserKey method implmentation
        /// </summary>
        public override bool RemoveUserKey(string upn)
        {
            bool ret = false;
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, upn))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.KeyAttribute);
                        dsusr.PropertiesToLoad.Add(_host.RSACertificateAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    DirEntry.Properties[_host.KeyAttribute].Clear();
                                    DirEntry.Properties[_host.RSACertificateAttribute].Clear();
                                    PropertyValueCollection props = DirEntry.Properties[_host.PublicKeyCredentialAttribute];
                                    props.Clear();
                                    DirEntry.CommitChanges();
                                    ret = true;
                                }
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
            return ret;
        }

        /// <summary>
        /// GetUserCertificate implementation
        /// </summary>
        public override X509Certificate2 GetUserCertificate(string upn, bool generatepassword = false)
        {
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, upn))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.KeyAttribute);
                        dsusr.PropertiesToLoad.Add(_host.RSACertificateAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            string pass = string.Empty;
                            if (generatepassword)
                                pass = CheckSumEncoding.CheckSumAsString(upn);
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    if (_keysisbinary)
                                    {
                                        byte[] b = (Byte[])DirEntry.Properties[_host.RSACertificateAttribute].Value;
                                        return new X509Certificate2(b, pass, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet); // | X509KeyStorageFlags.PersistKeySet);
                                    }
                                    else
                                    {
                                        string b = DirEntry.Properties[_host.RSACertificateAttribute].Value.ToString();
                                        return new X509Certificate2(Convert.FromBase64String(b), pass, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet); // | X509KeyStorageFlags.PersistKeySet);
                                    }
                                }
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

        /// <summary>
        /// CreateCertificate implementation
        /// </summary>
        public override X509Certificate2 CreateCertificate(string upn, int validity, bool generatepassword = false)
        {
            string pass = string.Empty;
            string strcert = string.Empty;
            if (generatepassword)
                pass = CheckSumEncoding.CheckSumAsString(upn);
            return Certs.CreateRSAEncryptionCertificateForUser(upn.ToLower(), validity, pass);
        }


        /// <summary>
        /// HasStoredKey method implmentation 
        /// </summary>
        public override bool HasStoredKey(string upn)
        {
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, _host.Account, _host.Password, upn))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.RSACertificateAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    return (DirEntry.Properties[_host.RSACertificateAttribute].Value != null);
                                }
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
            return false;

        }

        /// <summary>
        /// HasStoredCertificate method implmentation
        /// </summary>
        public override bool HasStoredCertificate(string upn)
        {
            return true;

        }
        #endregion
    }
    #endregion
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
        internal static DirectoryEntry GetDirectoryEntryForUPN(ADDSHost host, string upn)
        {
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(host.DomainName))
                entry = new DirectoryEntry("LDAP://" + host.DomainName);
            else
            {
                string dom = host.GetForestForUPN(upn);
                entry = new DirectoryEntry("LDAP://" + dom);
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
        internal static DirectoryEntry GetDirectoryEntryForUPN(ADDSHost host, string account, string password, string upn)
        {
            DirectoryEntry entry = null;
            string dom = host.GetForestForUPN(upn);
            entry = new DirectoryEntry("LDAP://" + dom);
            if (!string.IsNullOrEmpty(account))
                entry.Username = account;
            if (!string.IsNullOrEmpty(password))
                entry.Password = password;
            return entry;
        }

        /// <summary>
        /// GetDirectoryEntry() method implmentation
        /// </summary>
        internal static DirectoryEntry GetDirectoryEntry(ADDSHost host, ADDSHostForest forest)
        {
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(host.DomainName))
                entry = new DirectoryEntry("LDAP://" + host.DomainName);
            else
                entry = new DirectoryEntry("LDAP://" + forest.ForestDNS);
            if (!string.IsNullOrEmpty(host.Account))
                entry.Username = host.Account;
            if (!string.IsNullOrEmpty(host.Password))
                entry.Password = host.Password;
            return entry;
        }

        /// <summary>
        /// GetDirectoryEntry() method implmentation
        /// </summary>
        internal static DirectoryEntry GetDirectoryEntry(ADDSHost host)
        {
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(host.DomainName))
                entry = new DirectoryEntry("LDAP://" + host.DomainName);
            else
                entry = new DirectoryEntry();
            if (!string.IsNullOrEmpty(host.Account))
                entry.Username = host.Account;
            if (!string.IsNullOrEmpty(host.Password))
                entry.Password = host.Password;
            return entry;
        }

        /// <summary>
        /// GetDirectoryEntry method implmentation
        /// </summary>
        internal static DirectoryEntry GetDirectoryEntry(string domainname, string username, string password)
        {
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(domainname))
                entry = new DirectoryEntry("LDAP://" + domainname);
            else
                entry = new DirectoryEntry();
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
            entry.Path = sr.Path;
            if (!string.IsNullOrEmpty(host.Account))
                entry.Username = host.Account;
            if (!string.IsNullOrEmpty(host.Password))
                entry.Password = host.Password;
            return entry;
        }

        /// <summary>
        /// GetDirectoryEntry() method implmentation
        /// </summary>
        internal static DirectoryEntry GetDirectoryEntry(ADDSHost host, string path)
        {
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(host.DomainName))
                entry = new DirectoryEntry("LDAP://" + host.DomainName);
            else
                entry = new DirectoryEntry();
            entry.Path = path;
            if (!string.IsNullOrEmpty(host.Account))
                entry.Username = host.Account;
            if (!string.IsNullOrEmpty(host.Password))
                entry.Password = host.Password;
            return entry;
        }

        /// <summary>
        /// GetDirectoryEntry() method implmentation
        /// </summary>
        internal static DirectoryEntry GetDirectoryEntry(string domain, string username, string password, string path)
        {
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(domain))
                entry = new DirectoryEntry("LDAP://" + domain);
            else
                entry = new DirectoryEntry();
            entry.Path = path;
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
}
