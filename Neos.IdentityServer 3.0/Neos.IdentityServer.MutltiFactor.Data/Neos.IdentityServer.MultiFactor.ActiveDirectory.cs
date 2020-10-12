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
using System;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Cryptography.X509Certificates;

namespace Neos.IdentityServer.MultiFactor.Data
{
    #region ADDS Service
    internal class ADDSDataRepositoryService : DataRepositoryService, IDataRepositoryADDSConnection, IWebAuthNDataRepositoryService
    {
        private readonly bool _mailismulti = false;
        private readonly bool _phoneismulti = false;

        public override event KeysDataManagerEvent OnKeyDataEvent;

        /// <summary>
        /// ADDSDataRepositoryService constructor
        /// </summary>
        public ADDSDataRepositoryService(BaseDataHost host, int deliverywindow = 3000): base(host, deliverywindow)
        {
            if (!(host is ADDSHost))
                throw new ArgumentException("Invalid Host ! : value but be an ADDSHost instance");

            ADHost.Bind();
            _mailismulti = ADDSUtils.IsMultivaluedAttribute(ADHost.DomainName, ADHost.Account, ADHost.Password, ADHost.MailAttribute);
            _phoneismulti = ADDSUtils.IsMultivaluedAttribute(ADHost.DomainName, ADHost.Account, ADHost.Password, ADHost.PhoneAttribute);
        }

        /// <summary>
        /// ADDSDataRepositoryService constructor
        /// </summary>
        public ADDSDataRepositoryService(BaseDataHost host, int deliverywindow, string domain, string account, string password) : base(host, deliverywindow)
        {
            if (!(host is ADDSHost))
                throw new ArgumentException("Invalid Host ! : value but be an ADDSHost instance");

            ADHost.Bind();
            _mailismulti = ADDSUtils.IsMultivaluedAttribute(domain, account, password, ADHost.MailAttribute);
            _phoneismulti = ADDSUtils.IsMultivaluedAttribute(domain, account, password, ADHost.PhoneAttribute);
        }

        /// <summary>
        /// ADHost property
        /// </summary>
        private ADDSHost ADHost
        {
            get { return (ADDSHost)Host; }
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, upn, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(upn) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.MailAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.PhoneAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.MethodAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.OverrideMethodAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.PinAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            reg = new MFAUser();
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                    reg.UPN = sr.Properties[ClaimsUtilities.GetADDSUserAttribute()][0].ToString();
                                    if (ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.MailAttribute], _mailismulti) != null)
                                    {
                                        reg.MailAddress = ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.MailAttribute], _mailismulti);
                                        reg.IsRegistered = true;
                                    }
                                    if (ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.PhoneAttribute], _phoneismulti) != null)
                                    {
                                        reg.PhoneNumber = ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.PhoneAttribute], _phoneismulti);
                                        reg.IsRegistered = true;
                                    }

                                    if (DirEntry.Properties[ADHost.MethodAttribute].Value != null)
                                    {
                                        reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), DirEntry.Properties[ADHost.MethodAttribute].Value.ToString(), true);
                                        if (reg.PreferredMethod != PreferredMethod.Choose)
                                            reg.IsRegistered = true;
                                    }
                                    if (DirEntry.Properties[ADHost.OverrideMethodAttribute].Value != null)
                                    {
                                        try
                                        {
                                            reg.OverrideMethod = DirEntry.Properties[ADHost.OverrideMethodAttribute].Value.ToString();
                                        }
                                        catch
                                        {
                                            reg.OverrideMethod = string.Empty;
                                        }
                                    }
                                    else reg.OverrideMethod = string.Empty;

                                    if (DirEntry.Properties[ADHost.PinAttribute].Value != null)
                                    {
                                        try
                                        {
                                            reg.PIN = Convert.ToInt32(DirEntry.Properties[ADHost.PinAttribute].Value);
                                        }
                                        catch
                                        {
                                            reg.PIN = 0;
                                        }
                                    }
                                    else reg.PIN = 0;

                                    if (DirEntry.Properties[ADHost.TotpEnabledAttribute].Value != null)
                                    {
                                        reg.Enabled = bool.Parse(DirEntry.Properties[ADHost.TotpEnabledAttribute].Value.ToString());
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, reg.UPN, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(reg.UPN) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.MailAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.PhoneAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.MethodAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.OverrideMethodAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.PinAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                ADDSUtils.SetMultiValued(DirEntry.Properties[ADHost.MailAttribute], _mailismulti, reg.MailAddress);
                                ADDSUtils.SetMultiValued(DirEntry.Properties[ADHost.PhoneAttribute], _phoneismulti, reg.PhoneNumber);

                                if (!disableoninsert) // disable change if not explicitely done
                                {
                                    if (reg.Enabled)
                                        DirEntry.Properties[ADHost.TotpEnabledAttribute].Value = true;
                                    else
                                        DirEntry.Properties[ADHost.TotpEnabledAttribute].Value = false;
                                }
                                else
                                    DirEntry.Properties[ADHost.TotpEnabledAttribute].Value = false;

                                DirEntry.Properties[ADHost.MethodAttribute].Value = ((int)reg.PreferredMethod).ToString();
                                DirEntry.Properties[ADHost.PinAttribute].Value = reg.PIN;
                                if (string.IsNullOrEmpty(reg.OverrideMethod))
                                    DirEntry.Properties[ADHost.OverrideMethodAttribute].Clear();
                                else
                                    DirEntry.Properties[ADHost.OverrideMethodAttribute].Value = reg.OverrideMethod.ToString();

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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, reg.UPN, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(reg.UPN) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.MailAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.PhoneAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.MethodAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.OverrideMethodAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.PinAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                ADDSUtils.SetMultiValued(DirEntry.Properties[ADHost.MailAttribute], _mailismulti, reg.MailAddress);
                                ADDSUtils.SetMultiValued(DirEntry.Properties[ADHost.PhoneAttribute], _phoneismulti, reg.PhoneNumber);

                                if (!disableoninsert) // disable change if not explicitely done
                                {
                                    if (reg.Enabled)
                                        DirEntry.Properties[ADHost.TotpEnabledAttribute].Value = true;
                                    else
                                        DirEntry.Properties[ADHost.TotpEnabledAttribute].Value = false;
                                }
                                else
                                    DirEntry.Properties[ADHost.TotpEnabledAttribute].Value = false;

                                DirEntry.Properties[ADHost.MethodAttribute].Value = ((int)reg.PreferredMethod).ToString();
                                DirEntry.Properties[ADHost.PinAttribute].Value = reg.PIN;
                                if (string.IsNullOrEmpty(reg.OverrideMethod))
                                    DirEntry.Properties[ADHost.OverrideMethodAttribute].Clear();
                                else
                                    DirEntry.Properties[ADHost.OverrideMethodAttribute].Value = reg.OverrideMethod.ToString();

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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, reg.UPN, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(reg.UPN) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(ADHost.MailAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.PhoneAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.MethodAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.OverrideMethodAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.PinAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                ADDSUtils.SetMultiValued(DirEntry.Properties[ADHost.MailAttribute], _mailismulti, string.Empty);
                                ADDSUtils.SetMultiValued(DirEntry.Properties[ADHost.PhoneAttribute], _phoneismulti, string.Empty);
                                DirEntry.Properties[ADHost.TotpEnabledAttribute].Clear();
                                DirEntry.Properties[ADHost.MethodAttribute].Clear();
                                DirEntry.Properties[ADHost.OverrideMethodAttribute].Clear();
                                DirEntry.Properties[ADHost.PinAttribute].Clear();
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, reg.UPN, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(reg.UPN) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                DirEntry.Properties[ADHost.TotpEnabledAttribute].Value = true;
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, reg.UPN, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(reg.UPN) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                DirEntry.Properties[ADHost.TotpEnabledAttribute].Value = false;
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
                {0, " " + ClaimsUtilities.GetADDSSearchAttribute() },
                {1, ADHost.MailAttribute},
                {2, ADHost.PhoneAttribute}
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
                {0, "(" + ADHost.MethodAttribute + "=0)"},
                {1, "(" + ADHost.MethodAttribute + "=1)"},
                {2, "(" + ADHost.MethodAttribute + "=2)"},
                {3, "(" + ADHost.MethodAttribute + "=3)"},
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
                    qryldap += "(" + ADHost.TotpEnabledAttribute + "=" + true.ToString().ToUpper() + ")";
                }
                qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
            }
            else
            {
                qryldap += ClaimsUtilities.BuildADDSUserFilter("*");
                qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
            }
            qryldap += ")";


            MFAUserList registrations = new MFAUserList();
            try
            {
                foreach (ADDSHostForest f in ADHost.Forests)
                {
                    try
                    {
                        using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(f.ForestDNS, ADHost.Account, ADHost.Password, ADHost.UseSSL))
                        {
                            using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                            {
                                dsusr.PropertiesToLoad.Clear();
                                dsusr.PropertiesToLoad.Add("objectGUID");
                                dsusr.PropertiesToLoad.Add("userPrincipalName");
                                dsusr.PropertiesToLoad.Add("sAMAccountName");
                                dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                                dsusr.PropertiesToLoad.Add("whenCreated");
                                dsusr.PropertiesToLoad.Add(ADHost.MailAttribute);
                                dsusr.PropertiesToLoad.Add(ADHost.PhoneAttribute);
                                dsusr.PropertiesToLoad.Add(ADHost.MethodAttribute);
                                dsusr.PropertiesToLoad.Add(ADHost.OverrideMethodAttribute);
                                dsusr.PropertiesToLoad.Add(ADHost.PinAttribute);
                                dsusr.PropertiesToLoad.Add(ADHost.TotpEnabledAttribute);
                                dsusr.SizeLimit = ADHost.MaxRows;
                                dsusr.ReferralChasing = ReferralChasingOption.All;

                                switch (order.Column)
                                {
                                    case DataOrderField.UserName:
                                        dsusr.Sort.PropertyName = ClaimsUtilities.GetADDSSearchAttribute();
                                        break;
                                    case DataOrderField.Email:
                                        dsusr.Sort.PropertyName = ADHost.MailAttribute;
                                        break;
                                    case DataOrderField.Phone:
                                        dsusr.Sort.PropertyName = ADHost.PhoneAttribute;
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
                                    using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                                    {
                                        if (DirEntry.Properties["objectGUID"].Value != null)
                                        {
                                            reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                            reg.UPN = sr.Properties[ClaimsUtilities.GetADDSUserAttribute()][0].ToString();
                                            if (ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.MailAttribute], _mailismulti) != null)
                                            {                                               
                                                reg.MailAddress = ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.MailAttribute], _mailismulti);
                                                reg.IsRegistered = true;
                                            }
                                            if (ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.PhoneAttribute], _phoneismulti) != null)                                            
                                            {                                                
                                                reg.PhoneNumber = ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.PhoneAttribute], _phoneismulti);
                                                reg.IsRegistered = true;
                                            }
                                            if (DirEntry.Properties[ADHost.MethodAttribute].Value != null)
                                            {
                                                reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), DirEntry.Properties[ADHost.MethodAttribute].Value.ToString(), true);
                                                if (reg.PreferredMethod != PreferredMethod.Choose)
                                                    reg.IsRegistered = true;
                                            }
                                            if (DirEntry.Properties[ADHost.OverrideMethodAttribute].Value != null)
                                            {
                                                try
                                                {
                                                    reg.OverrideMethod = DirEntry.Properties[ADHost.OverrideMethodAttribute].Value.ToString();                                                    
                                                }
                                                catch
                                                {
                                                    reg.OverrideMethod = string.Empty;
                                                }
                                            }
                                            else reg.OverrideMethod = string.Empty;

                                            if (DirEntry.Properties[ADHost.PinAttribute].Value != null)
                                            {
                                                try
                                                {
                                                    reg.PIN = Convert.ToInt32(DirEntry.Properties[ADHost.PinAttribute].Value);                                                    
                                                }
                                                catch
                                                {
                                                    reg.PIN = 0;
                                                }
                                            }
                                            else reg.PIN = 0;

                                            if (DirEntry.Properties[ADHost.TotpEnabledAttribute].Value != null)                                            
                                            {
                                                reg.Enabled = bool.Parse(DirEntry.Properties[ADHost.TotpEnabledAttribute].Value.ToString());                                                
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
                foreach (ADDSHostForest f in ADHost.Forests)
                {
                    using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(f.ForestDNS, ADHost.Account, ADHost.Password, ADHost.UseSSL))
                    {
                        string qryldap = "(&(objectCategory=user)(objectClass=user)"+ ClaimsUtilities.BuildADDSUserFilter("*");
                        if (enabledonly)
                            qryldap += "(" + ADHost.TotpEnabledAttribute + "=" + true.ToString().ToUpper() + ")";
                        qryldap += "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
                        qryldap += ")";

                        using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                        {
                            dsusr.PropertiesToLoad.Clear();
                            dsusr.PropertiesToLoad.Add("objectGUID");
                            dsusr.PropertiesToLoad.Add("userPrincipalName");
                            dsusr.PropertiesToLoad.Add("sAMAccountName");
                            dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                            dsusr.PropertiesToLoad.Add(ADHost.MailAttribute);
                            dsusr.PropertiesToLoad.Add(ADHost.PhoneAttribute);
                            dsusr.PropertiesToLoad.Add(ADHost.MethodAttribute);
                            dsusr.PropertiesToLoad.Add(ADHost.OverrideMethodAttribute);
                            dsusr.PropertiesToLoad.Add(ADHost.PinAttribute);
                            dsusr.PropertiesToLoad.Add(ADHost.TotpEnabledAttribute);
                            dsusr.SizeLimit = ADHost.MaxRows;
                            dsusr.ReferralChasing = ReferralChasingOption.All;

                            switch (order.Column)
                            {
                                case DataOrderField.UserName:
                                    dsusr.Sort.PropertyName = ClaimsUtilities.GetADDSSearchAttribute();
                                    break;
                                case DataOrderField.Email:
                                    dsusr.Sort.PropertyName = ADHost.MailAttribute;
                                    break;
                                case DataOrderField.Phone:
                                    dsusr.Sort.PropertyName = ADHost.PhoneAttribute;
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
                                    using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                                    {
                                        if (DirEntry.Properties["objectGUID"].Value != null)
                                        {
                                            reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                            reg.UPN = sr.Properties[ClaimsUtilities.GetADDSUserAttribute()][0].ToString();
                                            if (ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.MailAttribute], _mailismulti) != null)
                                            {
                                                reg.MailAddress = ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.MailAttribute], _mailismulti);
                                                reg.IsRegistered = true;
                                            }
                                            if (ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.PhoneAttribute], _phoneismulti) != null)
                                            {
                                                reg.PhoneNumber = ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.PhoneAttribute], _phoneismulti);
                                                reg.IsRegistered = true;
                                            }

                                            if (DirEntry.Properties[ADHost.MethodAttribute].Value != null)
                                            {
                                                reg.PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), DirEntry.Properties[ADHost.MethodAttribute].Value.ToString(), true);
                                                if (reg.PreferredMethod != PreferredMethod.Choose)
                                                    reg.IsRegistered = true;
                                            }
                                            if (DirEntry.Properties[ADHost.OverrideMethodAttribute].Value != null)
                                            {
                                                try
                                                {
                                                    reg.OverrideMethod = DirEntry.Properties[ADHost.OverrideMethodAttribute].Value.ToString();
                                                }
                                                catch
                                                {
                                                    reg.OverrideMethod = string.Empty;
                                                }
                                            }
                                            else reg.OverrideMethod = string.Empty;

                                            if (DirEntry.Properties[ADHost.PinAttribute].Value != null)
                                            {
                                                try
                                                {
                                                    reg.PIN = Convert.ToInt32(DirEntry.Properties[ADHost.PinAttribute].Value);
                                                }
                                                catch
                                                {
                                                    reg.PIN = 0;
                                                }
                                            }
                                            else reg.PIN = 0;

                                            if (DirEntry.Properties[ADHost.TotpEnabledAttribute].Value != null)
                                            {
                                                reg.Enabled = bool.Parse(DirEntry.Properties[ADHost.TotpEnabledAttribute].Value.ToString());
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
                {1, ADHost.MailAttribute},
                {2, ADHost.PhoneAttribute}
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
                {0, "(" + ADHost.MethodAttribute + "=0)"},
                {1, "(" + ADHost.MethodAttribute + "=1)"},
                {2, "(" + ADHost.MethodAttribute + "=2)"},
                {3, "(" + ADHost.MethodAttribute + "=3)"},
                {4, "(" + ADHost.MethodAttribute + "=4)"}
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
                    qryldap += "(" + ADHost.TotpEnabledAttribute + "=" + true.ToString().ToUpper() + ")";
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
                foreach (ADDSHostForest f in ADHost.Forests)
                {
                    try
                    {
                        using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(f.ForestDNS, ADHost.Account, ADHost.Password, ADHost.UseSSL))
                        {
                            {
                                using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                                {
                                    dsusr.PropertiesToLoad.Clear();
                                    dsusr.PropertiesToLoad.Add("objectGUID");
                                    dsusr.SizeLimit = ADHost.MaxRows;
                                    dsusr.ReferralChasing = ReferralChasingOption.All;

                                    // filtrer IsRegistered
                                    SearchResultCollection src = dsusr.FindAll();
                                    if (src != null)
                                    {
                                        foreach (SearchResult sr in src)
                                        {
                                            MFAUser reg = new MFAUser();
                                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                                            {
                                                bool IsRegistered = false;
                                                if (DirEntry.Properties["objectGUID"].Value != null)
                                                {

                                                    if (ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.MailAttribute], _mailismulti) != null)
                                                        IsRegistered = true;
                                                    if (ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.PhoneAttribute], _phoneismulti) != null)
                                                        IsRegistered = true;

                                                    if (DirEntry.Properties[ADHost.MethodAttribute].Value != null)
                                                    {
                                                        PreferredMethod PreferredMethod = (PreferredMethod)Enum.Parse(typeof(PreferredMethod), DirEntry.Properties[ADHost.MethodAttribute].Value.ToString(), true);
                                                        if (PreferredMethod != PreferredMethod.Choose)
                                                            IsRegistered = true;
                                                    }
                                                    if (DirEntry.Properties[ADHost.OverrideMethodAttribute].Value != null)
                                                    {
                                                        try
                                                        {
                                                            reg.OverrideMethod = DirEntry.Properties[ADHost.OverrideMethodAttribute].Value.ToString();
                                                        }
                                                        catch
                                                        {
                                                            reg.OverrideMethod = string.Empty;
                                                        }
                                                    }
                                                    else reg.OverrideMethod = string.Empty;

                                                    if (DirEntry.Properties[ADHost.PinAttribute].Value != null)
                                                    {
                                                        try
                                                        {
                                                            reg.PIN = Convert.ToInt32(DirEntry.Properties[ADHost.PinAttribute].Value);
                                                        }
                                                        catch
                                                        {
                                                            reg.PIN = 0;
                                                        }
                                                    }
                                                    else reg.PIN = 0;

                                                    if (DirEntry.Properties[ADHost.TotpEnabledAttribute].Value != null)
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
        /// IsMFAUserRegistered method implementation
        /// </summary>
        public override bool IsMFAUserRegistered(string upn)
        {
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, upn, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(upn) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add("whenCreated");
                        dsusr.PropertiesToLoad.Add(ADHost.MailAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.PhoneAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.MethodAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.OverrideMethodAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.PinAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.TotpEnabledAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr == null)
                            return false;
                        using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                        {
                            if (DirEntry.Properties["objectGUID"].Value != null)
                            {
                                if (ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.MailAttribute], _mailismulti) != null)
                                    return true;
                                if (ADDSUtils.GetMultiValued(DirEntry.Properties[ADHost.PhoneAttribute], _phoneismulti) != null)
                                    return true;
                                if (DirEntry.Properties[ADHost.MethodAttribute].Value != null)
                                    return true;
                                if (DirEntry.Properties[ADHost.TotpEnabledAttribute].Value != null)
                                    return true;
                                if (DirEntry.Properties[ADHost.OverrideMethodAttribute].Value != null)
                                    return true;
                                if (DirEntry.Properties[ADHost.PinAttribute].Value != null)
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, username, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(username) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                string upn = sr.Properties[ClaimsUtilities.GetADDSUserAttribute()][0].ToString();
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, user.Name, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(user.Name) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.PublicKeyCredentialAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            ResultPropertyValueCollection xcoll = sr.Properties[ADHost.PublicKeyCredentialAttribute];
                            foreach (string s in xcoll)
                            {
                                WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization(ADHost);
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, user.Name, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(user.Name) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.PublicKeyCredentialAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            ResultPropertyValueCollection xcoll = sr.Properties[ADHost.PublicKeyCredentialAttribute];
                            foreach (string s in xcoll)
                            {
                                WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization(ADHost);
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
            WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization(ADHost);
            string value = ser.SerializeCredentials(credential, user.Name);
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, user.Name, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(user.Name) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.PublicKeyCredentialAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                DirEntry.Properties[ADHost.PublicKeyCredentialAttribute].Add(value);
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
            WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization(ADHost);
            string newvalue = ser.SerializeCredentials(credential, user.Name);
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, user.Name, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(user.Name) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.PublicKeyCredentialAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                bool updated = false;
                                ResultPropertyValueCollection pcoll = sr.Properties[ADHost.PublicKeyCredentialAttribute];

                                for (int i = 0; i < pcoll.Count; i++)
                                {
                                    string s = pcoll[i].ToString();
                                    MFAUserCredential usr = ser.DeserializeCredentials(s, user.Name);
                                    if (usr.Descriptor.Id.SequenceEqual(credential.Descriptor.Id))
                                    {
                                        DirEntry.Properties[ADHost.PublicKeyCredentialAttribute][i] = newvalue;
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, user.Name, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(user.Name) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add(ADHost.PublicKeyCredentialAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            string idtodelete = string.Empty;
                            ResultPropertyValueCollection xcoll = sr.Properties[ADHost.PublicKeyCredentialAttribute];
                            WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization(ADHost);
                            foreach (string s in xcoll)
                            {
                                // WebAuthNPublicKeySerialization ser = new WebAuthNPublicKeySerialization(ADHost);
                                MFAUserCredential usr = ser.DeserializeCredentials(s, user.Name);
                                if (HexaEncoding.GetHexStringFromByteArray(usr.Descriptor.Id).Equals(credentialid))
                                {
                                    idtodelete = s;
                                    break;
                                }
                            }
                            if (idtodelete != string.Empty)
                            {
                                using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                                {
                                    PropertyValueCollection props = DirEntry.Properties[ADHost.PublicKeyCredentialAttribute];
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
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntry(domainname, username, password, ADHost.UseSSL))
                {
                    Guid gd = rootdir.Guid;
                    res = true;
                }
            }
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5100);
                return false;
            }
            return res;
        }

        /// <summary>
        /// CheckAttribute method implmentation
        /// </summary>
        public bool CheckAttribute(string domainname, string username, string password, string attributename, int multivalued)
        {
            try
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
            catch (Exception ex)
            {
                DataLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error, 5100);
                return false;
            }
        }
        #endregion
    }
    #endregion

    #region ADDS Keys Service
    internal class ADDSKeysRepositoryService: KeysRepositoryService
     {
        private readonly bool _keysismulti = false;

        /// <summary>
        /// ADDSKeysRepositoryService constructor
        /// </summary>
        public ADDSKeysRepositoryService(BaseDataHost host, int deliverywindow): base(host, deliverywindow)
        {
            if (!(host is ADDSHost))
                throw new ArgumentException("Invalid Host ! : value but be an ADDSHost instance");

            _keysismulti = ADDSUtils.IsMultivaluedAttribute(ADHost.DomainName, ADHost.Account, ADHost.Password, ADHost.KeyAttribute);
        }

        private ADDSHost ADHost
        {
            get { return (ADDSHost)Host; }
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, upn, ADHost.UseSSL))
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(upn) + "(" + ADHost.KeyAttribute + "=*)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.KeyAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    ret = DirEntry.Properties[ADHost.KeyAttribute].Value.ToString();
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, upn, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(upn) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.KeyAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        { 
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                DirEntry.Properties[ADHost.KeyAttribute].Value = secretkey;
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, upn, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(upn) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.KeyAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    DirEntry.Properties[ADHost.KeyAttribute].Clear();
                                    DirEntry.Properties[ADHost.RSACertificateAttribute].Clear();
                                    PropertyValueCollection props = DirEntry.Properties[ADHost.PublicKeyCredentialAttribute];
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
         public override X509Certificate2 GetUserCertificate(string upn, string password)
         {
             return null;
         }

         /// <summary>
         /// CreateCertificate implementation
         /// </summary>
         public override X509Certificate2 CreateCertificate(string upn, string password, int validity)
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
        private readonly bool _keysisbinary = false;
        /// <summary>
        /// ADDSKeysRepositoryService constructor
        /// </summary>
        public ADDSKeys2RepositoryService(BaseDataHost host, int deliverywindow): base(host, deliverywindow)
        {
            if (!(host is ADDSHost))
                throw new ArgumentException("Invalid Host ! : value but be an ADDSHost instance");

            _keysisbinary = ADDSUtils.IsBinaryAttribute(ADHost.DomainName, ADHost.Account, ADHost.Password, ADHost.RSACertificateAttribute);
        }

        private ADDSHost ADHost
        {
            get { return (ADDSHost)Host; }
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, upn, ADHost.UseSSL))
                {
                    string qryldap = string.Empty;
                    qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(upn) + "(" + ADHost.KeyAttribute + "=*)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.KeyAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    ret = DirEntry.Properties[ADHost.KeyAttribute].Value.ToString();
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, upn, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(upn) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.KeyAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.RSACertificateAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                DirEntry.Properties[ADHost.KeyAttribute].Value = secretkey;
                                byte[] data = cert.Export(X509ContentType.Pfx, CheckSumEncoding.CheckSumAsString(upn));
                                try
                                {
                                    if (_keysisbinary)
                                        DirEntry.Properties[ADHost.RSACertificateAttribute].Value = data;
                                    else
                                        DirEntry.Properties[ADHost.RSACertificateAttribute].Value = Convert.ToBase64String(data);
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
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, upn, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(upn) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.KeyAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.RSACertificateAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    DirEntry.Properties[ADHost.KeyAttribute].Clear();
                                    DirEntry.Properties[ADHost.RSACertificateAttribute].Clear();
                                    PropertyValueCollection props = DirEntry.Properties[ADHost.PublicKeyCredentialAttribute];
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
        public override X509Certificate2 GetUserCertificate(string upn, string password)
        {
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, upn, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(upn) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.KeyAttribute);
                        dsusr.PropertiesToLoad.Add(ADHost.RSACertificateAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            string pass = string.Empty;
                            if (!string.IsNullOrEmpty(password))
                                pass = password;
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    if (_keysisbinary)
                                    {
                                        byte[] b = (Byte[])DirEntry.Properties[ADHost.RSACertificateAttribute].Value;
                                        return new X509Certificate2(b, pass, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet); // | X509KeyStorageFlags.PersistKeySet);
                                    }
                                    else
                                    {
                                        string b = DirEntry.Properties[ADHost.RSACertificateAttribute].Value.ToString();
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
        /// HasStoredKey method implmentation 
        /// </summary>
        public override bool HasStoredKey(string upn)
        {
            try
            {
                using (DirectoryEntry rootdir = ADDSUtils.GetDirectoryEntryForUser(ADHost, ADHost.Account, ADHost.Password, upn, ADHost.UseSSL))
                {
                    string qryldap = "(&(objectCategory=user)(objectClass=user)" + ClaimsUtilities.BuildADDSUserFilter(upn) + "(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                    using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                    {
                        dsusr.PropertiesToLoad.Clear();
                        dsusr.PropertiesToLoad.Add("objectGUID");
                        dsusr.PropertiesToLoad.Add("userPrincipalName");
                        dsusr.PropertiesToLoad.Add("sAMAccountName");
                        dsusr.PropertiesToLoad.Add("msDS-PrincipalName");
                        dsusr.PropertiesToLoad.Add(ADHost.RSACertificateAttribute);
                        dsusr.ReferralChasing = ReferralChasingOption.All;

                        SearchResult sr = dsusr.FindOne();
                        if (sr != null)
                        {
                            using (DirectoryEntry DirEntry = ADDSUtils.GetDirectoryEntry(ADHost, sr, ADHost.UseSSL))
                            {
                                if (DirEntry.Properties["objectGUID"].Value != null)
                                {
                                    return (DirEntry.Properties[ADHost.RSACertificateAttribute].Value != null);
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
}
