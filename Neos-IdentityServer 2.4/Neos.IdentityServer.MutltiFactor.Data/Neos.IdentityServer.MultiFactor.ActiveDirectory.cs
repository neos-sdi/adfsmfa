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
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Security.Principal;


namespace Neos.IdentityServer.MultiFactor.Data
{
    #region ADDS Service
    internal class ADDSDataRepositoryService : DataRepositoryService, IDataRepositoryADDSConnection
    {
        private ADDSHost _host;
        private int _deliverywindow = 300;
        private bool _mailismulti = false;
        private bool _phoneismulti = false;

        public override event KeysDataManagerEvent OnKeyDataEvent;

        /// <summary>
        /// AdminService constructor
        /// </summary>
        public ADDSDataRepositoryService(ADDSHost host, int deliverywindow = 3000): base()
        {
            _host = host;
            _deliverywindow = deliverywindow;
            _host.Bind();
            _mailismulti = ADDSUtils.IsMultivaluedAttribute(_host.DomainName, _host.Account, _host.Password, _host.mailAttribute);
            _phoneismulti = ADDSUtils.IsMultivaluedAttribute(_host.DomainName, _host.Account, _host.Password, _host.phoneAttribute);
        }

        /// <summary>
        /// GetUserRegistration method implementation
        /// </summary>
        public override Registration GetUserRegistration(string upn)
        {
            Registration reg = null;
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, upn))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";                  
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_host.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_host.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.overridemethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.pinattribute);
                        dsusr.PropertiesToLoad.Add(_host.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            reg = new Registration();
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                    reg.UPN = DirEntry.Properties["userPrincipalName"].Value.ToString();
                                    if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.mailAttribute], _mailismulti) != null)
                                    {
                                        reg.MailAddress = ADDSUtils.GetMultiValued(DirEntry.Properties[_host.mailAttribute], _mailismulti);
                                        reg.IsRegistered = true;
                                    }
                                    if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.phoneAttribute], _phoneismulti) != null)
                                    {
                                        reg.PhoneNumber = ADDSUtils.GetMultiValued(DirEntry.Properties[_host.phoneAttribute], _phoneismulti);
                                        reg.IsRegistered = true;
                                    }

                                    if (DirEntry.Properties[_host.methodAttribute].Value != null)
                                    {
                                        reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), DirEntry.Properties[_host.methodAttribute].Value.ToString(), true);
                                        if (reg.PreferredMethod != PreferredMethod.Choose)
                                            reg.IsRegistered = true;
                                    }
                                    if (DirEntry.Properties[_host.overridemethodAttribute].Value != null)
                                    {
                                        try
                                        {
                                            reg.OverrideMethod = DirEntry.Properties[_host.overridemethodAttribute].Value.ToString();
                                        }
                                        catch
                                        {
                                            reg.OverrideMethod = string.Empty;
                                        }
                                    }
                                    else reg.OverrideMethod = string.Empty;

                                    if (DirEntry.Properties[_host.pinattribute].Value != null)
                                    {
                                        try
                                        {
                                            reg.PIN = Convert.ToInt32(DirEntry.Properties[_host.pinattribute].Value);
                                        }
                                        catch
                                        {
                                            reg.PIN = 0;
                                        }
                                    }
                                    else reg.PIN = 0;

                                    if (DirEntry.Properties[_host.totpEnabledAttribute].Value != null)
                                    {
                                        reg.Enabled = bool.Parse(DirEntry.Properties[_host.totpEnabledAttribute].Value.ToString());
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
        /// SetUserRegistration method implementation
        /// </summary>
        public override Registration SetUserRegistration(Registration reg, bool resetkey = true, bool caninsert = true, bool disableoninsert = false)
        {
            if (!HasRegistration(reg.UPN))
            {
                if (caninsert)
                    return AddUserRegistration(reg, resetkey, false);
                else
                    return GetUserRegistration(reg.UPN);
            }
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, reg.UPN))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.ReferralChasing = ReferralChasingOption.All;
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_host.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_host.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.overridemethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.pinattribute);
                        dsusr.PropertiesToLoad.Add(_host.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                ADDSUtils.SetMultiValued(DirEntry.Properties[_host.mailAttribute], _mailismulti, reg.MailAddress);
                                ADDSUtils.SetMultiValued(DirEntry.Properties[_host.phoneAttribute], _phoneismulti, reg.PhoneNumber);

                                if (!disableoninsert) // disable change if not explicitely done
                                {
                                    if (reg.Enabled)
                                        DirEntry.Properties[_host.totpEnabledAttribute].Value = true;
                                    else
                                        DirEntry.Properties[_host.totpEnabledAttribute].Value = false;
                                }
                                else
                                    DirEntry.Properties[_host.totpEnabledAttribute].Value = false;

                                DirEntry.Properties[_host.methodAttribute].Value = ((int)reg.PreferredMethod).ToString();
                                DirEntry.Properties[_host.pinattribute].Value = reg.PIN;
                                if (string.IsNullOrEmpty(reg.OverrideMethod))
                                    DirEntry.Properties[_host.overridemethodAttribute].Clear();
                                else
                                    DirEntry.Properties[_host.overridemethodAttribute].Value = reg.OverrideMethod.ToString();

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
            return GetUserRegistration(reg.UPN);
        }

        /// <summary>
        /// AddUserRegistration method implementation
        /// </summary>
        public override Registration AddUserRegistration(Registration reg, bool resetkey = false, bool canupdate = true, bool disableoninsert = false)
        {
            if (HasRegistration(reg.UPN))
            {
                if (canupdate)
                    return SetUserRegistration(reg, resetkey, false);
                else
                    return GetUserRegistration(reg.UPN);
            }
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, reg.UPN))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.ReferralChasing = ReferralChasingOption.All;
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_host.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_host.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.overridemethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.pinattribute);
                        dsusr.PropertiesToLoad.Add(_host.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                ADDSUtils.SetMultiValued(DirEntry.Properties[_host.mailAttribute], _mailismulti, reg.MailAddress);
                                ADDSUtils.SetMultiValued(DirEntry.Properties[_host.phoneAttribute], _phoneismulti, reg.PhoneNumber);

                                if (!disableoninsert) // disable change if not explicitely done
                                {
                                    if (reg.Enabled)
                                        DirEntry.Properties[_host.totpEnabledAttribute].Value = true;
                                    else
                                        DirEntry.Properties[_host.totpEnabledAttribute].Value = false;
                                }
                                else
                                    DirEntry.Properties[_host.totpEnabledAttribute].Value = false;

                                DirEntry.Properties[_host.methodAttribute].Value = ((int)reg.PreferredMethod).ToString();
                                DirEntry.Properties[_host.pinattribute].Value = reg.PIN;
                                if (string.IsNullOrEmpty(reg.OverrideMethod))
                                    DirEntry.Properties[_host.overridemethodAttribute].Clear();
                                else
                                    DirEntry.Properties[_host.overridemethodAttribute].Value = reg.OverrideMethod.ToString();

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
            return GetUserRegistration(reg.UPN);
        }

        /// <summary>
        /// DeleteUserRegistration method implementation
        /// </summary>
        public override bool DeleteUserRegistration(Registration reg, bool dropkey = true)
        {
            if (!HasRegistration(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be deleted ! \r User not found !");
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, reg.UPN))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(_host.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_host.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_host.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.overridemethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.pinattribute);
                        dsusr.PropertiesToLoad.Add(_host.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                ADDSUtils.SetMultiValued(DirEntry.Properties[_host.mailAttribute], _mailismulti, string.Empty);
                                ADDSUtils.SetMultiValued(DirEntry.Properties[_host.phoneAttribute], _phoneismulti, string.Empty);
                                DirEntry.Properties[_host.totpEnabledAttribute].Clear();
                                DirEntry.Properties[_host.methodAttribute].Clear();
                                DirEntry.Properties[_host.overridemethodAttribute].Clear();
                                DirEntry.Properties[_host.pinattribute].Clear();
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
        /// EnableUserRegistration method implementation
        /// </summary>
        public override Registration EnableUserRegistration(Registration reg)
        {
            if (!HasRegistration(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! \r User not found !");
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, reg.UPN))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                DirEntry.Properties[_host.totpEnabledAttribute].Value = true;
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
            return GetUserRegistration(reg.UPN);
        }

        /// <summary>
        /// DisableUserRegistration method implementation
        /// </summary>
        public override Registration DisableUserRegistration(Registration reg)
        {
            if (!HasRegistration(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! User not found !");
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, reg.UPN))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                DirEntry.Properties[_host.totpEnabledAttribute].Value = false;
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
            return GetUserRegistration(reg.UPN);
        }

        /// <summary>
        /// GetUserRegistrations method implementation
        /// </summary>
        public override RegistrationList GetUserRegistrations(DataFilterObject filter, DataOrderObject order, DataPagingObject paging)
        {
            Dictionary<int, string> fliedlsvalues = new Dictionary<int, string> 
            {
                {0, " userprincipalname"},
                {1, _host.mailAttribute},
                {2, _host.phoneAttribute}
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
                {0, "(" + _host.methodAttribute + "=0)"},
                {1, "(" + _host.methodAttribute + "=1)"},
                {2, "(" + _host.methodAttribute + "=2)"},
                {3, "(" + _host.methodAttribute + "=3)"},
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
                    qryldap += "(" + _host.totpEnabledAttribute + "=" + true.ToString() + ")";
                }
                qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
            }
            else
            {
                qryldap += "(userprincipalname=*)";
                qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
            }
            qryldap += ")";


            RegistrationList registrations = new RegistrationList();
            try
            {
                foreach (ADDSHostForest f in _host.Forests)
                {
                    try
                    {
                        using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host, f))
                        {
                            using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                            {
                                dsusr.PropertiesToLoad.Clear();
                                dsusr.PropertiesToLoad.Add("objectGUID");
                                dsusr.PropertiesToLoad.Add("userPrincipalName");
                                dsusr.PropertiesToLoad.Add("whenCreated");
                                dsusr.PropertiesToLoad.Add(_host.mailAttribute);
                                dsusr.PropertiesToLoad.Add(_host.phoneAttribute);
                                dsusr.PropertiesToLoad.Add(_host.methodAttribute);
                                dsusr.PropertiesToLoad.Add(_host.overridemethodAttribute);
                                dsusr.PropertiesToLoad.Add(_host.pinattribute);
                                dsusr.PropertiesToLoad.Add(_host.totpEnabledAttribute);
                                dsusr.SizeLimit = _host.MaxRows;

                                switch (order.Column)
                                {
                                    case DataOrderField.UserName:
                                        dsusr.Sort.PropertyName = "userPrincipalName";
                                        break;
                                    case DataOrderField.Email:
                                        dsusr.Sort.PropertyName = _host.mailAttribute;
                                        break;
                                    case DataOrderField.Phone:
                                        dsusr.Sort.PropertyName = _host.phoneAttribute;
                                        break;
                                    default:
                                        dsusr.Sort.PropertyName = "objectGUID";
                                        break;
                                }
                                dsusr.Sort.Direction = order.Direction;

                                DirectoryVirtualListView lstv = null;
                                DirectoryVirtualListViewContext ctx = new DirectoryVirtualListViewContext();
                                int virtualListCount = int.MaxValue;
                                if (paging.isActive)
                                {
                                    int pg = paging.PageSize;
                                    int of = (((paging.CurrentPage - 1) * paging.PageSize) + 1);
                                    lstv = new DirectoryVirtualListView(0, pg - 1, of, ctx);
                                    dsusr.VirtualListView = lstv;
                                    virtualListCount = pg;

                                }
                                SearchResultCollection src = dsusr.FindAll();
                                if (src != null)
                                {
                                    if ((!paging.IsRecurse) && (src.Count == 1) && (paging.isActive))
                                    {
                                        DataPagingObject xpaging = new DataPagingObject();
                                        xpaging.PageSize = paging.PageSize;
                                        xpaging.CurrentPage = paging.CurrentPage - 1;
                                        xpaging.IsRecurse = true;
                                        if (xpaging.CurrentPage > 0)
                                        {
                                            RegistrationList verif = GetUserRegistrations(filter, order, xpaging);
                                            foreach (Registration reg in verif)
                                            {
                                                using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, src[0]))
                                                {
                                                    string theID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                                    if (reg.ID == theID)
                                                    {
                                                        return registrations; // empty
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    int i = 0;
                                    foreach (SearchResult sr in src)
                                    {
                                        if (i < virtualListCount)
                                        {
                                            i++;
                                            Registration reg = new Registration();
                                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                                            {
                                                if (DirEntry.Properties["objectGUID"].Value != null)
                                                {
                                                    reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                                    reg.UPN = DirEntry.Properties["userPrincipalName"].Value.ToString();
                                                    if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.mailAttribute], _mailismulti) != null)
                                                    {
                                                        reg.MailAddress = ADDSUtils.GetMultiValued(DirEntry.Properties[_host.mailAttribute], _mailismulti);
                                                        reg.IsRegistered = true;
                                                    }
                                                    if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.phoneAttribute], _phoneismulti) != null)
                                                    {
                                                        reg.PhoneNumber = ADDSUtils.GetMultiValued(DirEntry.Properties[_host.phoneAttribute], _phoneismulti);
                                                        reg.IsRegistered = true;
                                                    }
                                                    if (DirEntry.Properties[_host.methodAttribute].Value != null)
                                                    {
                                                        reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), DirEntry.Properties[_host.methodAttribute].Value.ToString(), true);
                                                        if (reg.PreferredMethod != PreferredMethod.Choose)
                                                            reg.IsRegistered = true;
                                                    }
                                                    if (DirEntry.Properties[_host.overridemethodAttribute].Value != null)
                                                    {
                                                        try
                                                        {
                                                            reg.OverrideMethod = DirEntry.Properties[_host.overridemethodAttribute].Value.ToString();
                                                        }
                                                        catch
                                                        {
                                                            reg.OverrideMethod = string.Empty;
                                                        }
                                                    }
                                                    else reg.OverrideMethod = string.Empty;

                                                    if (DirEntry.Properties[_host.pinattribute].Value != null)
                                                    {
                                                        try
                                                        {
                                                            reg.PIN = Convert.ToInt32(DirEntry.Properties[_host.pinattribute].Value);
                                                        }
                                                        catch
                                                        {
                                                            reg.PIN = 0;
                                                        }
                                                    }
                                                    else reg.PIN = 0;

                                                    if (DirEntry.Properties[_host.totpEnabledAttribute].Value != null)
                                                    {
                                                        reg.Enabled = bool.Parse(DirEntry.Properties[_host.totpEnabledAttribute].Value.ToString());
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
            return registrations;
        }

        /// <summary>
        /// GetAllUserRegistrations method implementation
        /// </summary>
        public override RegistrationList GetAllUserRegistrations(DataOrderObject order, bool enabledonly = false)
        {
            RegistrationList registrations = new RegistrationList();
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=*)";
                    if (enabledonly)
                        qryldap += "(" + _host.totpEnabledAttribute + "=" + true.ToString() + ")";
                    qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
                    qryldap += ")";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add(_host.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_host.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_host.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.overridemethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.pinattribute);
                        dsusr.PropertiesToLoad.Add(_host.totpEnabledAttribute);
                        dsusr.SizeLimit = _host.MaxRows;

                        switch (order.Column)
                        {
                            case DataOrderField.UserName:
                                dsusr.Sort.PropertyName = "userPrincipalName";
                                break;
                            case DataOrderField.Email:
                                dsusr.Sort.PropertyName = _host.mailAttribute;
                                break;
                            case DataOrderField.Phone:
                                dsusr.Sort.PropertyName = _host.phoneAttribute;
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
                                Registration reg = new Registration();
                                using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                                {
                                    if (DirEntry.Properties["objectGUID"].Value != null)
                                    {
                                        reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                        reg.UPN = DirEntry.Properties["userPrincipalName"].Value.ToString();

                                        if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.mailAttribute], _mailismulti) != null)
                                        {
                                            reg.MailAddress = ADDSUtils.GetMultiValued(DirEntry.Properties[_host.mailAttribute], _mailismulti);
                                            reg.IsRegistered = true;
                                        }
                                        if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.phoneAttribute], _phoneismulti) != null)
                                        {
                                            reg.PhoneNumber = ADDSUtils.GetMultiValued(DirEntry.Properties[_host.phoneAttribute], _phoneismulti);
                                            reg.IsRegistered = true;
                                        }

                                        if (DirEntry.Properties[_host.methodAttribute].Value != null)
                                        {
                                            reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), DirEntry.Properties[_host.methodAttribute].Value.ToString(), true);
                                            if (reg.PreferredMethod != PreferredMethod.Choose)
                                                reg.IsRegistered = true;
                                        }
                                        if (DirEntry.Properties[_host.overridemethodAttribute].Value != null)
                                        {
                                            try
                                            {
                                                reg.OverrideMethod = DirEntry.Properties[_host.overridemethodAttribute].Value.ToString();
                                            }
                                            catch
                                            {
                                                reg.OverrideMethod = string.Empty;
                                            }
                                        }
                                        else reg.OverrideMethod = string.Empty;

                                        if (DirEntry.Properties[_host.pinattribute].Value != null)
                                        {
                                            try
                                            {
                                                reg.PIN = Convert.ToInt32(DirEntry.Properties[_host.pinattribute].Value);
                                            }
                                            catch
                                            {
                                                reg.PIN = 0;
                                            }
                                        }
                                        else reg.PIN = 0;

                                        if (DirEntry.Properties[_host.totpEnabledAttribute].Value != null)
                                        {
                                            reg.Enabled = bool.Parse(DirEntry.Properties[_host.totpEnabledAttribute].Value.ToString());
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
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return registrations;
        }

        /// <summary>
        /// GetUserRegistrationsCount method implementation
        /// </summary>
        public override int GetUserRegistrationsCount(DataFilterObject filter)
        {
            Dictionary<int, string> fliedlsvalues = new Dictionary<int, string> 
            {
                {0, " userprincipalname"} ,
                {1, _host.mailAttribute},
                {2, _host.phoneAttribute}
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
                {0, "(" + _host.methodAttribute + "=0)"},
                {1, "(" + _host.methodAttribute + "=1)"},
                {2, "(" + _host.methodAttribute + "=2)"},
                {3, "(" + _host.methodAttribute + "=3)"},
                {4, "(" + _host.methodAttribute + "=4)"}
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
                    qryldap += "(" + _host.totpEnabledAttribute + "=" + true.ToString() + ")";
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
                        using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host, f))
                        {
                            using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                            {
                                dsusr.PropertiesToLoad.Clear();
                                dsusr.PropertiesToLoad.Add("objectGUID");
                                dsusr.SizeLimit = _host.MaxRows;
                                // filtrer IsRegistered
                                SearchResultCollection src = dsusr.FindAll();
                                if (src != null)
                                {
                                    foreach (SearchResult sr in src)
                                    {
                                        Registration reg = new Registration();
                                        using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                                        {
                                            bool IsRegistered = false;
                                            if (DirEntry.Properties["objectGUID"].Value != null)
                                            {

                                                if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.mailAttribute], _mailismulti) != null)
                                                    IsRegistered = true;
                                                if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.phoneAttribute], _phoneismulti) != null)
                                                    IsRegistered = true;

                                                if (DirEntry.Properties[_host.methodAttribute].Value != null)
                                                {
                                                    PreferredMethod PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), DirEntry.Properties[_host.methodAttribute].Value.ToString(), true);
                                                    if (PreferredMethod != PreferredMethod.Choose)
                                                        IsRegistered = true;
                                                }
                                                if (DirEntry.Properties[_host.overridemethodAttribute].Value != null)
                                                {
                                                    try
                                                    {
                                                        reg.OverrideMethod = DirEntry.Properties[_host.overridemethodAttribute].Value.ToString();
                                                    }
                                                    catch
                                                    {
                                                        reg.OverrideMethod = string.Empty;
                                                    }
                                                }
                                                else reg.OverrideMethod = string.Empty;

                                                if (DirEntry.Properties[_host.pinattribute].Value != null)
                                                {
                                                    try
                                                    {
                                                        reg.PIN = Convert.ToInt32(DirEntry.Properties[_host.pinattribute].Value);
                                                    }
                                                    catch
                                                    {
                                                        reg.PIN = 0;
                                                    }
                                                }
                                                else reg.PIN = 0;

                                                if (DirEntry.Properties[_host.totpEnabledAttribute].Value != null)
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
                    catch (Exception ex)
                    {
                        DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5001);
                        DataLog.WriteEntry("Forest ADDS : "+f.ForestDNS+" discarded !!!" , System.Diagnostics.EventLogEntryType.Error, 5001);
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
        /// GetImportUserRegistrations
        /// </summary>
        public override RegistrationList GetImportUserRegistrations(string domain, string username, string password, string ldappath, DateTime? created, DateTime? modified, string mailattribute, string phoneattribute, PreferredMethod meth, bool disableall = false)
        {
            if (!string.IsNullOrEmpty(ldappath))
            {
                ldappath = ldappath.Replace("ldap://", "LDAP://");
                if (!ldappath.StartsWith("LDAP://"))
                    ldappath = "LDAP://" + ldappath;
            }
            RegistrationList registrations = new RegistrationList();
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(domain, username, password, ldappath))
                {
                    string qryldap = string.Empty;
                    qryldap  = "(&";
                    qryldap     += "(objectCategory=user)(objectClass=user)(userprincipalname=*)";
                   // qryldap     += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
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
                                Registration reg = new Registration();
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
        /// HasRegistration method implementation
        /// </summary>
        public override bool HasRegistration(string upn)
        {
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, upn))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(_host.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_host.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_host.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.overridemethodAttribute);
                        dsusr.PropertiesToLoad.Add(_host.pinattribute);
                        dsusr.PropertiesToLoad.Add(_host.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr == null)
                            return false;
                        using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                        {
                            if (DirEntry.Properties["objectGUID"].Value != null)
                            {
                                if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.mailAttribute], _mailismulti) != null)
                                    return true;
                                if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.phoneAttribute], _phoneismulti) != null)
                                    return true;
                                if (DirEntry.Properties[_host.methodAttribute].Value != null)
                                    return true;
                                if (DirEntry.Properties[_host.totpEnabledAttribute].Value != null)
                                    return true;
                                if (DirEntry.Properties[_host.overridemethodAttribute].Value != null)
                                    return true;
                                if (DirEntry.Properties[_host.pinattribute].Value != null)
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
        public bool CheckAttribute(string domainname, string username, string password, string attributename, bool checkmultivalued)
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
                        var userClass = forest.Schema.FindClass("user");
                        foreach (ActiveDirectorySchemaProperty property in userClass.GetAllProperties())
                        {
                            if (property.Name.Equals(attributename))
                            {
                                if ((!checkmultivalued) && (property.IsSingleValued))
                                    return true;
                                if (checkmultivalued) 
                                    return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
     }
    #endregion

    #region ADDS Keys Service
    internal class ADDSKeysRepositoryService: KeysRepositoryService
     {
        private ADDSHost _host;
        private bool _keysismulti = false;
        /// <summary>
         /// ADDSKeysRepositoryService constructor
        /// </summary>
        public ADDSKeysRepositoryService(MFAConfig cfg)
        {
            _host = cfg.Hosts.ActiveDirectoryHost;
            _keysismulti = ADDSUtils.IsMultivaluedAttribute(_host.DomainName, _host.Account, _host.Password, _host.keyAttribute);
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
                 using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, upn))
                 {
                     string qryldap = string.Empty;
                     qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(" + _host.keyAttribute + "=*)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                     using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                     {
                         dsusr.PropertiesToLoad.Clear();
                         dsusr.PropertiesToLoad.Add("objectGUID");
                         dsusr.PropertiesToLoad.Add("userPrincipalName");
                         dsusr.PropertiesToLoad.Add(_host.keyAttribute);

                         SearchResult sr = dsusr.FindOne();
                         if (sr != null)
                         {
                             using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                             {
                                 if (DirEntry.Properties["objectGUID"].Value != null)
                                 {
                                     if (ADDSUtils.GetMultiValued(DirEntry.Properties[_host.keyAttribute], _keysismulti) != null)
                                         ret = ADDSUtils.GetMultiValued(DirEntry.Properties[_host.keyAttribute], _keysismulti);
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
         public override string NewUserKey(string upn, string secretkey, string cert = null)
         {
             string ret = string.Empty;
             try
             {
                 using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, upn))
                 {
                     string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                     using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                     {
                         dsusr.PropertiesToLoad.Clear();
                         dsusr.PropertiesToLoad.Add("userPrincipalName");
                         dsusr.PropertiesToLoad.Add(_host.keyAttribute);

                         SearchResult sr = dsusr.FindOne();
                         if (sr != null)
                         {
                             using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                             {
                                 ADDSUtils.SetMultiValued(DirEntry.Properties[_host.keyAttribute], _keysismulti, secretkey);
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
                 using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUPN(_host, upn))
                 {
                     string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                     using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                     {
                         dsusr.PropertiesToLoad.Clear();
                         dsusr.PropertiesToLoad.Add("objectGUID");
                         dsusr.PropertiesToLoad.Add("userPrincipalName");
                         dsusr.PropertiesToLoad.Add(_host.keyAttribute);

                         SearchResult sr = dsusr.FindOne();
                         if (sr != null)
                         {
                             using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                             {
                                 if (DirEntry.Properties["objectGUID"].Value != null)
                                 {
                                     DirEntry.Properties[_host.keyAttribute].Clear();
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
         public override X509Certificate2 CreateCertificate(string upn, int validity, out string strcert, bool generatepassword = false)
         {
             strcert = null;
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
                        var userClass = forest.Schema.FindClass("user");
                        foreach (ActiveDirectorySchemaProperty property in userClass.GetAllProperties())
                        {
                            if (property.Name.Equals(attributename))
                                return !property.IsSingleValued;
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
