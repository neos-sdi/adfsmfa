//******************************************************************************************************************************************************************************************//
// Copyright (c) 2017 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
     internal class ADDSDataRepositoryService: DataRepositoryService
    {
        private ADDSHost _host;
        private int _deliverywindow = 300;

        public override event KeysDataManagerEvent OnKeyDataEvent;

        /// <summary>
        /// AdminService constructor
        /// </summary>
        public ADDSDataRepositoryService(ADDSHost host, int deliverywindow = 3000): base()
        {
            _host = host;
            _deliverywindow = deliverywindow;
        }

        /// <summary>
        /// CheckRepositoryAttribute method implementation
        /// </summary>
        public override bool CheckRepositoryAttribute(string attributename)
        {
            if (attributename.ToLower().Equals("connection"))
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
                {
                    Guid gd = rootdir.Guid;
                }
                return true;
            }

            DirectoryContext ctx = null;
            if (!string.IsNullOrEmpty(_host.DomainAddress))
            {
                if (!string.IsNullOrEmpty(_host.Account) || !string.IsNullOrEmpty(_host.Password))
                    ctx = new DirectoryContext(DirectoryContextType.Domain, _host.DomainName, _host.Account, _host.Password);
                else
                    ctx = new DirectoryContext(DirectoryContextType.Domain, _host.DomainName);
            }
            else
            {
                if (!string.IsNullOrEmpty(_host.Account) || !string.IsNullOrEmpty(_host.Password))
                    ctx = new DirectoryContext(DirectoryContextType.Domain, _host.Account, _host.Password);
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
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// GetUserRegistration method implementation
        /// </summary>
        public override Registration GetUserRegistration(string upn)
        {
            Registration reg = null;
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

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
                                    if (DirEntry.Properties[_host.mailAttribute].Value != null)
                                    {
                                        reg.MailAddress = DirEntry.Properties[_host.mailAttribute].Value.ToString();
                                        reg.IsRegistered = true;
                                    }
                                    if (DirEntry.Properties[_host.phoneAttribute].Value != null)
                                    {
                                        reg.PhoneNumber = DirEntry.Properties[_host.phoneAttribute].Value.ToString();
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
        public override Registration SetUserRegistration(Registration reg, bool resetkey = false)
        {
            if (!HasRegistration(reg.UPN))
                return AddUserRegistration(reg, resetkey);
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("userPrincipalName");

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(_host, sr))
                            {
                                if (!string.IsNullOrEmpty(reg.MailAddress))
                                    DirEntry.Properties[_host.mailAttribute].Value = reg.MailAddress;
                                else
                                    DirEntry.Properties[_host.mailAttribute].Clear();
                                if (!string.IsNullOrEmpty(reg.PhoneNumber))
                                    DirEntry.Properties[_host.phoneAttribute].Value = reg.PhoneNumber;
                                else
                                    DirEntry.Properties[_host.phoneAttribute].Clear();

                                if (reg.Enabled)
                                    DirEntry.Properties[_host.totpEnabledAttribute].Value = true;
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
        public override Registration AddUserRegistration(Registration reg, bool newkey = true)
        {
            if (HasRegistration(reg.UPN))
                return SetUserRegistration(reg, newkey);
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
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
                                if (!string.IsNullOrEmpty(reg.MailAddress))
                                    DirEntry.Properties[_host.mailAttribute].Value = reg.MailAddress;
                                else
                                    DirEntry.Properties[_host.mailAttribute].Clear();
                                if (!string.IsNullOrEmpty(reg.PhoneNumber))
                                    DirEntry.Properties[_host.phoneAttribute].Value = reg.PhoneNumber;
                                else
                                    DirEntry.Properties[_host.phoneAttribute].Clear();
                                if (reg.Enabled)
                                    DirEntry.Properties[_host.totpEnabledAttribute].Value = true;
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
                            if (newkey)
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

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
                                DirEntry.Properties[_host.mailAttribute].Clear();
                                DirEntry.Properties[_host.phoneAttribute].Clear();
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

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
                throw new Exception("The user " + reg.UPN + " cannot be updated ! \r User not found !");
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
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
                                            if (DirEntry.Properties[_host.mailAttribute].Value != null)
                                            {
                                                reg.MailAddress = DirEntry.Properties[_host.mailAttribute].Value.ToString();
                                                reg.IsRegistered = true;
                                            }
                                            if (DirEntry.Properties[_host.phoneAttribute].Value != null)
                                            {
                                                reg.PhoneNumber = DirEntry.Properties[_host.phoneAttribute].Value.ToString();
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
                                        if (DirEntry.Properties[_host.mailAttribute].Value != null)
                                        {
                                            reg.MailAddress = DirEntry.Properties[_host.mailAttribute].Value.ToString();
                                            reg.IsRegistered = true;
                                        }
                                        if (DirEntry.Properties[_host.phoneAttribute].Value != null)
                                        {
                                            reg.PhoneNumber = DirEntry.Properties[_host.phoneAttribute].Value.ToString();
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
            RegistrationList registrations = new RegistrationList();
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
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
                                        if (DirEntry.Properties[_host.mailAttribute].Value != null)
                                            IsRegistered = true;
                                        if (DirEntry.Properties[_host.phoneAttribute].Value != null)
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
                            return count;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new Exception(ex.Message);
            }
            return -1;
        }

        /// <summary>
        /// GetImportUserRegistrations
        /// </summary>
        public override RegistrationList GetImportUserRegistrations(string ldappath, bool enable = false)
        {
            ldappath = ldappath.Replace("ldap://", "LDAP://");
            if (!ldappath.StartsWith("LDAP://"))
                ldappath = "LDAP://" + ldappath;
            RegistrationList registrations = new RegistrationList();
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host, ldappath))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=*)";
                    qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
                    qryldap += ")";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("mail");
                        dsusr.PropertiesToLoad.Add("otherMailbox");
                        dsusr.PropertiesToLoad.Add("mobile");
                        dsusr.PropertiesToLoad.Add("otherMobile");
                        dsusr.PropertiesToLoad.Add("telephoneNumber");
                        dsusr.SizeLimit = _host.MaxRows;

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

                                            if (DirEntry.Properties["otherMailbox"].Value != null)
                                                reg.MailAddress = DirEntry.Properties["otherMailbox"].Value.ToString();
                                            else if (DirEntry.Properties["mail"].Value != null)
                                                reg.MailAddress = DirEntry.Properties["mail"].Value.ToString();

                                            if (DirEntry.Properties["mobile"].Value != null)
                                                reg.PhoneNumber = DirEntry.Properties["mobile"].Value.ToString();
                                            else if (DirEntry.Properties["otherMobile"].Value != null)
                                                reg.PhoneNumber = DirEntry.Properties["otherMobile"].Value.ToString();
                                            else if (DirEntry.Properties["telephoneNumber"].Value != null)
                                                reg.PhoneNumber = DirEntry.Properties["telephoneNumber"].Value.ToString();

                                            reg.OverrideMethod = string.Empty;
                                            reg.Enabled = enable;

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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
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
                                if (DirEntry.Properties[_host.mailAttribute].Value != null)
                                    return true;
                                if (DirEntry.Properties[_host.phoneAttribute].Value != null)
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
    }

     internal class ADDSKeysRepositoryService: KeysRepositoryService
     {
         ADDSHost _host;

        /// <summary>
         /// ADDSKeysRepositoryService constructor
        /// </summary>
        public ADDSKeysRepositoryService(MFAConfig cfg)
        {
            _host = cfg.Hosts.ActiveDirectoryHost;
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
                 using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
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
                                     if (DirEntry.Properties[_host.keyAttribute].Value != null)
                                         ret = DirEntry.Properties[_host.keyAttribute].Value.ToString();
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
                 using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
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
                                 DirEntry.Properties[_host.keyAttribute].Value = secretkey;
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
                 using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(_host))
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
         public override X509Certificate2 GetUserCertificate(string upn)
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
        /// GetDirectoryEntry() method implmentation
        /// </summary>
        internal static DirectoryEntry GetDirectoryEntry(ADDSHost host)
        {
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(host.DomainAddress))
                entry = new DirectoryEntry(host.DomainAddress);
            else
                entry = new DirectoryEntry();
            if (!string.IsNullOrEmpty(host.Account))
                entry.Username = host.Account;
            if (!string.IsNullOrEmpty(host.Password))
                entry.Password = host.Password;
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
            if (!string.IsNullOrEmpty(host.DomainAddress))
                entry = new DirectoryEntry(host.DomainAddress);
            else
                entry = new DirectoryEntry();
            entry.Path = path;
            if (!string.IsNullOrEmpty(host.Account))
                entry.Username = host.Account;
            if (!string.IsNullOrEmpty(host.Password))
                entry.Password = host.Password;
            return entry;
        }
        #endregion
/*
        #region Multi-Forest procedures
        /// <summary>
        /// GetDomains method implmentation
        /// </summary>
        /// <returns></returns>
        internal static ICollection<string> GetDomains() 
        {
            ICollection<string> domains = new List<string>();

            // Querying the current Forest for the domains within.
            foreach(Domain d in Forest.GetCurrentForest().Domains)
                domains.Add(d.Name);

            return domains;
        }


        internal static string GetDomainFullName(string friendlyName) 
        {
            DirectoryContext context = new DirectoryContext(DirectoryContextType.Domain, friendlyName);
            Domain domain = Domain.GetDomain(context);
            return domain.Name;
        }

        internal static IEnumerable<string> GetUserDomain(string userName)
        {
            foreach (string d in GetDomains())
            {
                // From the domains obtained from the Forest, we search the domain subtree for the given userName.
                using (DirectoryEntry domain = new DirectoryEntry(GetDomainFullName(d)))
                {
                    using (DirectorySearcher searcher = new DirectorySearcher())
                    {
                        searcher.SearchRoot = domain;
                        searcher.SearchScope = SearchScope.Subtree;
                        searcher.PropertiesToLoad.Add("sAMAccountName");
                        // The Filter is very important, so is its query string. The 'objectClass' parameter is mandatory.
                        // Once we specified the 'objectClass', we want to look for the user whose login
                        // login is userName.
                        searcher.Filter = string.Format("(&(objectClass=user)(sAMAccountName={0}))", userName);
                        try
                        {
                            SearchResultCollection results = searcher.FindAll();
                            // If the user cannot be found, then let's check next domain.
                            if (results == null || results.Count == 0)
                                continue;
                            // Here, we yield return for we want all of the domain which this userName is authenticated.
                            yield return domain.Path;
                        }
                        finally
                        {
                            searcher.Dispose();
                            domain.Dispose();
                        }
                    }
                }
            }
        }
        #endregion

        #region Loading of forests and domains lists
        /// <summary>
        /// LoadDomains method implementation
        /// </summary>
        private static void LoadDomains(ADDSHost host)
        {
            DateTime db = DateTime.Now;
            bool rootit = false;
            using (Identity impersonate = Identity.Impersonate(host.Account, host.Password))
            {
                DirectoryContext dctx = null;
                if (string.IsNullOrEmpty(host.Account) || string.IsNullOrEmpty(host.Password))
                    dctx = new DirectoryContext(DirectoryContextType.Forest);
                else
                    dctx = new DirectoryContext(DirectoryContextType.Forest, host.Account, host.Password);
                Forest f = Forest.GetForest(dctx);
                try
                {
                    foreach (Domain d in f.Domains)
                    {
                        if (d.Parent == null)
                        {
                            List<ActiveDirectoryDomainParam> lprm = GetDomainConfigurations(d.Name);
                            foreach (ActiveDirectoryDomainParam prm in lprm)
                            {
                                if (prm.Enabled)
                                {
                                    ActiveDirectoryRootDomain r = new ActiveDirectoryRootDomain(d, internalGetTopLevelNames(), prm, this.GlobalParams);
                                    this.RootDomains.Add(r);
                                    r.IsRoot = true;
                                    LoadChildDomainList(r, d);
                                }
                                else
                                    this.BadDomains.Add(new ActiveDirectoryBadDomain(d.Name, string.Format("This root domain {0} is administratively Disabled ", d), DateTime.Now.Subtract(db)));
                            }
                        }
                    }
                    try
                    {
                        TrustRelationshipInformationCollection t = f.GetAllTrustRelationships();
                        foreach (ForestTrustRelationshipInformation i in t)
                        {
                            List<ActiveDirectoryDomainParam> lprmx = GetDomainConfigurations(i.TargetName);
                            foreach (ActiveDirectoryDomainParam prmx in lprmx)
                            {
                                if (!prmx.Enabled)
                                {
                                    this.BadDomains.Add(new ActiveDirectoryBadDomain(i.TargetName, string.Format("This domain {0} is administratively Disabled ", i.TargetName), DateTime.Now.Subtract(db)));
                                    continue;
                                }
                                if (!rootit)
                                {
                                    rootit = true;
                                    manual.Reset();
                                }
                                try
                                {
                                    lock (lokobj)
                                    {
                                        countforest++;
                                    }
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(InternalLoadForest), new ActiveDirectoryForestLoadState(i.TargetName, relationshipsGetTopLevelNames(i)));
                                }
                                catch (Exception E)
                                {
                                    LogEvent.Log(E, string.Format(ResourcesValues.GetString("E1001"), i.TargetName), System.Diagnostics.EventLogEntryType.Error, 1001);
                                }
                            }
                        }
                    }
                    catch (Exception E)
                    {
                        LogEvent.Log(E, ResourcesValues.GetString("E1501"), System.Diagnostics.EventLogEntryType.Error, 1501);
                    }
                    if (rootit)
                        manual.WaitOne();
                    _isloaded = true;
                    TimeSpan _e = DateTime.Now.Subtract(db);
                    LogEvent.Trace(string.Format(ResourcesValues.GetString("E1000B"), this.ProviderName, _e.Minutes, _e.Seconds, _e.Milliseconds), System.Diagnostics.EventLogEntryType.Information, 1000);
                }
                catch (Exception E)
                {
                    _isloaded = false;
                    LogEvent.Log(E, ResourcesValues.GetString("E1500"), System.Diagnostics.EventLogEntryType.Error, 1500);
                }
                finally
                {
                    f.Dispose();
                }
                _elapsedtime = DateTime.Now.Subtract(db);
                _isloadedfromcache = false;
            }
        }

        /// <summary>
        /// InternalLoadForest method implementation
        /// </summary>
        private void InternalLoadForest(object objectstate)
        {
            ActiveDirectoryForestLoadState data = (ActiveDirectoryForestLoadState)objectstate;
            DirectoryContext dctx = null;
            if (string.IsNullOrEmpty(_aduser) || string.IsNullOrEmpty(_adpwd))
                dctx = new DirectoryContext(DirectoryContextType.Forest, data.ForestName);
            else
                dctx = new DirectoryContext(DirectoryContextType.Forest, data.ForestName, _aduser, _adpwd);
            try
            {
                DateTime db = DateTime.Now;
                Forest ff = null;
                try
                {
                    ff = Forest.GetForest(dctx);
                }
                catch (Exception E)
                {
                    this.BadDomains.Add(new ActiveDirectoryBadDomain(data.ForestName, E.Message, DateTime.Now.Subtract(db)));
                    throw E;
                }
                foreach (Domain d in ff.Domains)
                {
                    if (d.Parent == null)
                    {
                        List<ActiveDirectoryDomainParam> lprm = GetDomainConfigurations(d.Name);
                        foreach (ActiveDirectoryDomainParam prm in lprm)
                        {
                            if (prm.Enabled)
                            {
                                ActiveDirectoryRootDomain r = null;
                                try
                                {

                                    r = new ActiveDirectoryRootDomain(d, data.TopLevelNames, prm, this.GlobalParams);
                                    this.RootDomains.Add(r);
                                    r.IsRoot = true;
                                    LoadChildDomainList(r, d);
                                }
                                catch (Exception E)
                                {
                                    this.BadDomains.Add(new ActiveDirectoryBadDomain(d.Name, E.Message, DateTime.Now.Subtract(db)));
                                    throw E;
                                }
                            }
                            else
                                this.BadDomains.Add(new ActiveDirectoryBadDomain(d.Name, string.Format("This domain {0} is administratively Disabled ", d.Name), DateTime.Now.Subtract(db)));
                        }
                    }
                }
            }
            catch (Exception E)
            {
                LogEvent.Log(E, string.Format(ResourcesValues.GetString("E1200"), data.ForestName), System.Diagnostics.EventLogEntryType.Error, 1200);
            }
            finally
            {
                lock (lokobj)
                {
                    countforest--;
                    if (countforest == 0)
                    {
                        manual.Set();
                    }
                }
            }
        }

        /// <summary>
        /// LoadChildDomainList method implementation
        /// </summary>
        private void LoadChildDomainList(ActiveDirectoryDomain ad, Domain a)
        {
            if (!ad.IsReacheable)
                return;
            foreach (Domain d in a.Children)
            {
                List<ActiveDirectoryDomainParam> lprm = GetDomainConfigurations(d.Name);
                foreach (ActiveDirectoryDomainParam prm in lprm)
                {
                    if (prm.Enabled)
                    {
                        try
                        {
                            ActiveDirectoryDomain wr = new ActiveDirectoryDomain(d, prm, this.GlobalParams);
                            ad.Domains.Add(wr);
                            wr.Parent = ad;
                            if (wr.IsReacheable)
                                LoadChildDomainList(wr, d);
                        }
                        catch (Exception E)
                        {
                            this.BadDomains.Add(new ActiveDirectoryBadDomain(d.Name, string.Format("Error loading domain {0}", d.Name), DateTime.Now.Subtract(DateTime.Now)));
                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E1100"), d.Name), System.Diagnostics.EventLogEntryType.Error, 1100);
                            throw E;
                        }
                    }
                    else
                        this.BadDomains.Add(new ActiveDirectoryBadDomain(d.Name, string.Format("This domain {0} is administratively Disabled ", d.Name), DateTime.Now.Subtract(DateTime.Now)));
                }
            }
        }

        #endregion
 */
    }
}
