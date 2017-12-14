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
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.Administration
{
    public class ADDSAdminService : IAdministrationService
    {
        MFAConfig _config = null;
        string _connectionstring;
        int _deliverywindow = 300;

        /// <summary>
        /// AdminService constructor
        /// </summary>
        public ADDSAdminService(MFAConfig config)
        {
            _config = config;
            _connectionstring = _config.Hosts.SQLServerHost.ConnectionString;
            _deliverywindow = _config.DeliveryWindow;
        }

        /// <summary>
        /// GetDirectoryEntry() method implmentation
        /// </summary>
        private DirectoryEntry GetDirectoryEntry()
        {
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(_config.Hosts.ActiveDirectoryHost.DomainAddress))
                entry = new DirectoryEntry(_config.Hosts.ActiveDirectoryHost.DomainAddress);
            else
                entry = new DirectoryEntry();
            if (!string.IsNullOrEmpty(_config.Hosts.ActiveDirectoryHost.Account))
                entry.Username = _config.Hosts.ActiveDirectoryHost.Account;
            if (!string.IsNullOrEmpty(_config.Hosts.ActiveDirectoryHost.Password))
                entry.Password = _config.Hosts.ActiveDirectoryHost.Password;
            return entry;
        }

        /// <summary>
        /// GetDirectoryEntry() method implmentation
        /// </summary>
        private DirectoryEntry GetDirectoryEntry(SearchResult sr)
        {
            DirectoryEntry entry = sr.GetDirectoryEntry();
            entry.Path = sr.Path;
            if (!string.IsNullOrEmpty(_config.Hosts.ActiveDirectoryHost.Account))
                entry.Username = _config.Hosts.ActiveDirectoryHost.Account;
            if (!string.IsNullOrEmpty(_config.Hosts.ActiveDirectoryHost.Password))
                entry.Password = _config.Hosts.ActiveDirectoryHost.Password;
            return entry;
        }

        /// <summary>
        /// GetUser method implementation
        /// </summary>
        public MMCRegistration GetUserRegistration(string upn)
        {
            MMCRegistration reg = null;
            try
            {
                using (DirectoryEntry rootdir = GetDirectoryEntry())
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            reg = new MMCRegistration();
                            using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                    reg.UPN = DirEntry.Properties["userPrincipalName"].Value.ToString();
                                    if (DirEntry.Properties["whenCreated"].Value != null)
                                        reg.CreationDate = Convert.ToDateTime(DirEntry.Properties["whenCreated"].Value);
                                    if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.mailAttribute].Value != null)
                                    {
                                        reg.MailAddress = DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.mailAttribute].Value.ToString();
                                        reg.IsRegistered = true;
                                    }
                                    if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.phoneAttribute].Value != null)
                                    {
                                        reg.PhoneNumber = DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.phoneAttribute].Value.ToString();
                                        reg.IsRegistered = true;
                                    }
                                    if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.methodAttribute].Value != null)
                                    {
                                        reg.PreferredMethod = (RegistrationPreferredMethod)Enum.Parse(typeof(RegistrationPreferredMethod), DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.methodAttribute].Value.ToString(), true);
                                        reg.IsRegistered = true;
                                    }
                                    if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value != null)
                                    {
                                        reg.Enabled = bool.Parse(DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value.ToString());
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
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new FaultException(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// SetUserRegistration method implementation
        /// </summary>
        public void SetUserRegistration(MMCRegistration reg)
        {
            if (!HasRegistration(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! \r User not found !");

            try
            {
                using (DirectoryEntry rootdir = GetDirectoryEntry())
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.keyAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                            {
                                DirEntry.Properties["userPrincipalName"].Value = reg.UPN;
                                if (!string.IsNullOrEmpty(reg.MailAddress))
                                    DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.mailAttribute].Value = reg.MailAddress;
                                else
                                    DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.mailAttribute].Clear();
                                if (!string.IsNullOrEmpty(reg.PhoneNumber))
                                    DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.phoneAttribute].Value = reg.PhoneNumber;
                                else
                                    DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.phoneAttribute].Clear();
                                if (reg.Enabled)
                                    DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value = true;
                                else
                                    DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value = false;
                                DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.methodAttribute].Value = ((int)reg.PreferredMethod).ToString();
                                DirEntry.CommitChanges();
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// AddUserRegistration method implementation
        /// </summary>
        public MMCRegistration AddUserRegistration(MMCRegistration reg)
        {
            if (HasRegistration(reg.UPN))
            {
                SetUserRegistration(reg);
                return GetUserRegistration(reg.UPN);
            }

            try
            {
                using (DirectoryEntry rootdir = GetDirectoryEntry())
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                            {
                                DirEntry.Properties["userPrincipalName"].Value = reg.UPN; // ICI Why
                                if (!string.IsNullOrEmpty(reg.MailAddress))
                                    DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.mailAttribute].Value = reg.MailAddress;
                                else
                                    DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.mailAttribute].Clear();
                                if (!string.IsNullOrEmpty(reg.PhoneNumber))
                                    DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.phoneAttribute].Value = reg.PhoneNumber;
                                else
                                    DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.phoneAttribute].Clear();
                                if (reg.Enabled)
                                    DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value = true;
                                else
                                    DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value = false;
                                DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.methodAttribute].Value = ((int)reg.PreferredMethod).ToString();
                                DirEntry.CommitChanges();
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new FaultException(ex.Message);
            }
            return GetUserRegistration(reg.UPN);
        }

        /// <summary>
        /// DeleteUserRegistration method implementation
        /// </summary>
        public bool DeleteUserRegistration(MMCRegistration reg)
        {
            if (!HasRegistration(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be deleted ! \r User not found !");

            try
            {
                using (DirectoryEntry rootdir = GetDirectoryEntry())
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                            {
                                DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.mailAttribute].Clear();
                                DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.phoneAttribute].Clear();
                                DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Clear();
                                DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.methodAttribute].Clear();
                                DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.notifcreatedateAttribute].Clear();
                                DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.notifvalidityAttribute].Clear();
                                DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.notifcheckdateattribute].Clear();
                                DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpAttribute].Clear();
                                DirEntry.CommitChanges();
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new FaultException(ex.Message);
            }
            return true;
        }

        /// <summary>
        /// EnableUserRegistration method implementation
        /// </summary>
        public MMCRegistration EnableUserRegistration(MMCRegistration reg)
        {
            if (!HasRegistration(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! \r User not found !");

            try
            {
                using (DirectoryEntry rootdir = GetDirectoryEntry())
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                            {
                                DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value = true;
                                DirEntry.CommitChanges();
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new FaultException(ex.Message);
            }
            return GetUserRegistration(reg.UPN);
        }

        /// <summary>
        /// DisableUserRegistration method implementation
        /// </summary>
        public MMCRegistration DisableUserRegistration(MMCRegistration reg)
        {
            if (!HasRegistration(reg.UPN))
                throw new Exception("The user " + reg.UPN + " cannot be updated ! \r User not found !");
            try
            {
                using (DirectoryEntry rootdir = GetDirectoryEntry())
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                            {
                                DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value = false;
                                DirEntry.CommitChanges();
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new FaultException(ex.Message);
            }
            return GetUserRegistration(reg.UPN);
        }

        /// <summary>
        /// GetUserRegistrations method implementation
        /// </summary>
        public MMCRegistrationList GetUserRegistrations(UsersFilterObject filter, UsersOrderObject order, UsersPagingObject paging, int maxrows = 20000)
        {
            Dictionary<int, string> fliedlsvalues = new Dictionary<int, string> 
            {
                {0, " userprincipalname"} ,
                {1, _config.Hosts.ActiveDirectoryHost.mailAttribute},
                {2, _config.Hosts.ActiveDirectoryHost.phoneAttribute}
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
                {0, "(" + _config.Hosts.ActiveDirectoryHost.methodAttribute + "=0)"},
                {1, "(" + _config.Hosts.ActiveDirectoryHost.methodAttribute + "=1)"},
                {2, "(" + _config.Hosts.ActiveDirectoryHost.methodAttribute + "=2)"},
                {3, "(" + _config.Hosts.ActiveDirectoryHost.methodAttribute + "=3)"},
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

                if (filter.FilterMethod != UsersPreferredMethod.None)
                {
                    string strmethod = string.Empty;
                    methodvalues.TryGetValue((int)filter.FilterMethod, out strmethod);
                    qryldap += strmethod;
                }
                if (filter.EnabledOnly)
                {
                    qryldap += "(" + _config.Hosts.ActiveDirectoryHost.totpEnabledAttribute + "=" + true.ToString() + ")";
                }
                qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
            }
            else
            {
                qryldap += "(userprincipalname=*)";
                qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
            }
            qryldap += ")";


            MMCRegistrationList registrations = new MMCRegistrationList();
            try
            {
                using (DirectoryEntry rootdir = GetDirectoryEntry())
                {
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute);
                        dsusr.SizeLimit = maxrows;

                        switch (order.Column)
                        {
                            case UsersOrderField.UserName:
                                dsusr.Sort.PropertyName = "userPrincipalName";
                                break;
                            case UsersOrderField.Email:
                                dsusr.Sort.PropertyName = _config.Hosts.ActiveDirectoryHost.mailAttribute;
                                break;
                            case UsersOrderField.Phone:
                                dsusr.Sort.PropertyName = _config.Hosts.ActiveDirectoryHost.phoneAttribute;
                                break;
                            case UsersOrderField.CreationDate:
                                dsusr.Sort.PropertyName = "whenCreated";
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
                                UsersPagingObject xpaging = new UsersPagingObject();
                                xpaging.PageSize = paging.PageSize;
                                xpaging.CurrentPage = paging.CurrentPage - 1;
                                xpaging.IsRecurse = true;
                                if (xpaging.CurrentPage > 0)
                                {
                                    MMCRegistrationList verif = GetUserRegistrations(filter, order, xpaging, maxrows);
                                    foreach (MMCRegistration reg in verif)
                                    {
                                        using (DirectoryEntry DirEntry = GetDirectoryEntry(src[0]))
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
                                    MMCRegistration reg = new MMCRegistration();
                                    using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                                    {
                                        if (DirEntry.Properties["objectGUID"].Value != null)
                                        {
                                            reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                            reg.UPN = DirEntry.Properties["userPrincipalName"].Value.ToString();
                                            if (DirEntry.Properties["whenCreated"].Value != null)
                                                reg.CreationDate = Convert.ToDateTime(DirEntry.Properties["whenCreated"].Value);
                                            if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.mailAttribute].Value != null)
                                            {
                                                reg.MailAddress = DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.mailAttribute].Value.ToString();
                                                reg.IsRegistered = true;
                                            }
                                            if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.phoneAttribute].Value != null)
                                            {
                                                reg.PhoneNumber = DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.phoneAttribute].Value.ToString();
                                                reg.IsRegistered = true;
                                            }
                                            if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.methodAttribute].Value != null)
                                            {
                                                reg.PreferredMethod = (RegistrationPreferredMethod)Enum.Parse(typeof(RegistrationPreferredMethod), DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.methodAttribute].Value.ToString(), true);
                                                if (reg.PreferredMethod != RegistrationPreferredMethod.Choose)
                                                    reg.IsRegistered = true;
                                            }
                                            if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value != null)
                                            {
                                                reg.Enabled = bool.Parse(DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value.ToString());
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
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new FaultException(ex.Message);
            }
            return registrations;
        }

        /// <summary>
        /// GetAllUserRegistrations method implementation
        /// </summary>
        public MMCRegistrationList GetAllUserRegistrations(UsersOrderObject order, int maxrows = 20000, bool enabledonly = false)
        {
            MMCRegistrationList registrations = new MMCRegistrationList();

            try
            {
                using (DirectoryEntry rootdir = GetDirectoryEntry())
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=*)";
                    if (enabledonly)
                        qryldap += "(" + _config.Hosts.ActiveDirectoryHost.totpEnabledAttribute + "=" + true.ToString() + ")";
                    qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
                    qryldap += ")";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute);
                        dsusr.SizeLimit = maxrows;

                        switch (order.Column)
                        {
                            case UsersOrderField.UserName:
                                dsusr.Sort.PropertyName = "userPrincipalName";
                                break;
                            case UsersOrderField.Email:
                                dsusr.Sort.PropertyName = _config.Hosts.ActiveDirectoryHost.mailAttribute;
                                break;
                            case UsersOrderField.Phone:
                                dsusr.Sort.PropertyName = _config.Hosts.ActiveDirectoryHost.phoneAttribute;
                                break;
                            case UsersOrderField.CreationDate:
                                dsusr.Sort.PropertyName = "whenCreated";
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
                                MMCRegistration reg = new MMCRegistration();
                                using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                                {
                                    if (DirEntry.Properties["objectGUID"].Value != null)
                                    {
                                        reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                        reg.UPN = DirEntry.Properties["userPrincipalName"].Value.ToString();
                                        if (DirEntry.Properties["whenCreated"].Value != null)
                                            reg.CreationDate = Convert.ToDateTime(DirEntry.Properties["whenCreated"].Value);
                                        if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.mailAttribute].Value != null)
                                        {
                                            reg.MailAddress = DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.mailAttribute].Value.ToString();
                                            reg.IsRegistered = true;
                                        }
                                        if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.phoneAttribute].Value != null)
                                        {
                                            reg.PhoneNumber = DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.phoneAttribute].Value.ToString();
                                            reg.IsRegistered = true;
                                        }
                                        if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.methodAttribute].Value != null)
                                        {
                                            reg.PreferredMethod = (RegistrationPreferredMethod)Enum.Parse(typeof(RegistrationPreferredMethod), DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.methodAttribute].Value.ToString(), true);
                                            if (reg.PreferredMethod != RegistrationPreferredMethod.Choose)
                                                reg.IsRegistered = true;
                                        }
                                        if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value != null)
                                        {
                                            reg.Enabled = bool.Parse(DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value.ToString());
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
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new FaultException(ex.Message);
            }
            return registrations;
        }

        /// <summary>
        /// GetUserRegistrationsCount method implmentation
        /// </summary>
        public int GetUserRegistrationsCount(UsersFilterObject filter)
        {
            Dictionary<int, string> fliedlsvalues = new Dictionary<int, string> 
            {
                {0, " userprincipalname"} ,
                {1, _config.Hosts.ActiveDirectoryHost.mailAttribute},
                {2, _config.Hosts.ActiveDirectoryHost.phoneAttribute}
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
                {0, "(" + _config.Hosts.ActiveDirectoryHost.methodAttribute + "=0)"},
                {1, "(" + _config.Hosts.ActiveDirectoryHost.methodAttribute + "=1)"},
                {2, "(" + _config.Hosts.ActiveDirectoryHost.methodAttribute + "=2)"},
                {3, "(" + _config.Hosts.ActiveDirectoryHost.methodAttribute + "=3)"},
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

                if (filter.FilterMethod != UsersPreferredMethod.None)
                {
                    string strmethod = string.Empty;
                    methodvalues.TryGetValue((int)filter.FilterMethod, out strmethod);
                    qryldap += strmethod;
                }
                if (filter.EnabledOnly)
                {
                    qryldap += "(" + _config.Hosts.ActiveDirectoryHost.totpEnabledAttribute + "=" + true.ToString() + ")";
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
            MMCRegistrationList registrations = new MMCRegistrationList();
            try
            {
                using (DirectoryEntry rootdir = GetDirectoryEntry())
                {
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        // filtrer IsRegistered
                        SearchResultCollection src = dsusr.FindAll();
                        if (src != null)
                        {
                            foreach (SearchResult sr in src)
                            {
                                MMCRegistration reg = new MMCRegistration();
                                using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                                {
                                    bool IsRegistered = false;
                                    if (DirEntry.Properties["objectGUID"].Value != null)
                                    {
                                        if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.mailAttribute].Value != null)
                                            IsRegistered = true;
                                        if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.phoneAttribute].Value != null)
                                            IsRegistered = true;
                                        if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.methodAttribute].Value != null)
                                        {
                                            RegistrationPreferredMethod PreferredMethod = (RegistrationPreferredMethod)Enum.Parse(typeof(RegistrationPreferredMethod), DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.methodAttribute].Value.ToString(), true);
                                            if (PreferredMethod != RegistrationPreferredMethod.Choose)
                                                IsRegistered = true;
                                        }
                                        if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value != null)
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
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new FaultException(ex.Message);
            }
            return -1;
        }

        /// <summary>
        /// UserExists method implementation
        /// </summary>
        private bool HasRegistration(string upn)
        {
            try
            {
                using (DirectoryEntry rootdir = GetDirectoryEntry())
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + upn + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.mailAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.phoneAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.methodAttribute);
                        dsusr.PropertiesToLoad.Add(_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute);

                        SearchResult sr = dsusr.FindOne();
                        if (sr == null)
                            return false;
                        using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                        {
                            if (DirEntry.Properties["objectGUID"].Value != null)
                            {
                                if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.mailAttribute].Value != null)
                                    return true;
                                if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.phoneAttribute].Value != null)
                                    return true;
                                if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.methodAttribute].Value != null)
                                    return true;
                                if (DirEntry.Properties[_config.Hosts.ActiveDirectoryHost.totpEnabledAttribute].Value != null)
                                    return true;
                            }
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5000);
                throw new FaultException(ex.Message);
            }
        }
    }
}
