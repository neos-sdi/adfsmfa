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
using System.DirectoryServices;
using System.Text;

namespace Neos.IdentityServer.MultiFactor
{
    public class ADDSAdminService : IAdminService
    {
        ADDSHost _host;
        int _deliverywindow = 300;

        /// <summary>
        /// AdminService constructor
        /// </summary>
        public ADDSAdminService(ADDSHost host, int deliverywindow)
        {
            _host = host;
            _deliverywindow = deliverywindow;
        }

        #region Registration
        /// <summary>
        /// GetUserRegistration method implementation
        /// </summary>
        public Registration GetUserRegistration(string upn)
        {
            Registration reg = null; 
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
                    dsusr.PropertiesToLoad.Add(_host.mailAttribute);
                    dsusr.PropertiesToLoad.Add(_host.phoneAttribute);
                    dsusr.PropertiesToLoad.Add(_host.methodAttribute);
                    dsusr.PropertiesToLoad.Add(_host.totpEnabledAttribute);

                    SearchResult sr = dsusr.FindOne();
                    if (sr != null)
                    {
                        reg = new Registration();
                        using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                        {
                            if (DirEntry.Properties["objectGUID"].Value != null)
                            {
                                reg.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                                reg.UPN = DirEntry.Properties["userPrincipalName"].Value.ToString();
                                if (DirEntry.Properties["whenCreated"].Value != null)
                                    reg.CreationDate = Convert.ToDateTime(DirEntry.Properties["whenCreated"].Value);
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
                                    reg.PreferredMethod = (RegistrationPreferredMethod)Enum.Parse(typeof(RegistrationPreferredMethod), DirEntry.Properties[_host.methodAttribute].Value.ToString(), true);
                                    if (reg.PreferredMethod != RegistrationPreferredMethod.Choose)
                                        reg.IsRegistered = true;
                                }
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
            return null;
        }

        /// <summary>
        /// SetUserRegistration method implementation
        /// </summary>
        public void SetUserRegistration(Registration reg)
        {
            using (DirectoryEntry rootdir = GetDirectoryEntry())
            {
                string qryldap = "(&(objectCategory=user)(objectClass=user)(userprincipalname=" + reg.UPN + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

                using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                {
                    dsusr.PropertiesToLoad.Clear();
                    dsusr.PropertiesToLoad.Add("userPrincipalName");

                    SearchResult sr = dsusr.FindOne();
                    if (sr != null)
                    {
                        bool mustcommit = false;
                        using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                        {
                            if (!string.IsNullOrEmpty(reg.MailAddress))
                            {
                                DirEntry.Properties[_host.mailAttribute].Value = reg.MailAddress;
                                mustcommit = true;
                            }
                            else
                                DirEntry.Properties[_host.mailAttribute].Clear();
                            if (!string.IsNullOrEmpty(reg.PhoneNumber))
                            {
                                DirEntry.Properties[_host.phoneAttribute].Value = reg.PhoneNumber;
                                mustcommit = true;
                            }
                            else
                                DirEntry.Properties[_host.phoneAttribute].Clear();

                            if (reg.Enabled)
                                DirEntry.Properties[_host.totpEnabledAttribute].Value = true;
                            else
                                DirEntry.Properties[_host.totpEnabledAttribute].Value = false;
                            if (mustcommit)
                                DirEntry.Properties[_host.methodAttribute].Value = ((int)reg.PreferredMethod).ToString();
                            DirEntry.CommitChanges();
                        };
                    }
                }
            }
        }

        /// <summary>
        /// GetUserKey method implmentation
        /// </summary>
        public string GetUserKey(string upn)
        {
            string ret = string.Empty;
            using (DirectoryEntry rootdir = GetDirectoryEntry())
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
                        using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
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
            return ret;
        }

        /// <summary>
        /// NewUserKey method implmentation
        /// </summary>
        public string NewUserKey(string upn, string secretkey)
        {
            string ret = string.Empty;
            using (DirectoryEntry rootdir = GetDirectoryEntry())
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
                        using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                        {
                            DirEntry.Properties[_host.keyAttribute].Value = secretkey;
                            DirEntry.CommitChanges();
                            ret = secretkey;
                        };
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// RemoveUserKey method implmentation
        /// </summary>
        public bool RemoveUserKey(string upn)
        {
            bool ret = false;
            using (DirectoryEntry rootdir = GetDirectoryEntry())
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
                        using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
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
            return ret;
        }
        #endregion

        #region Notification
        /// <summary>
        /// SetNotification method implementation
        /// </summary>
        public Notification SetNotification(Registration reg, MFAConfig cfg, int otp)
        {
            Notification notif = new Notification();
            notif.ID = reg.ID;
            notif.RegistrationID = reg.ID;
            notif.OTP = otp;
            notif.CreationDate = DateTime.Now;
            notif.ValidityDate = notif.CreationDate.AddSeconds(_deliverywindow);
            notif.CheckDate = null;
            DoUpdateNotification(notif);
            return notif;
        }

        /// <summary>
        /// CheckNotification method implementation
        /// </summary>
        public Notification CheckNotification(string registrationid)
        {
            Notification notif = new Notification();
            notif.ID = registrationid;
            notif.RegistrationID = registrationid;
            notif.CheckDate = DateTime.Now;
            return DoCheckNotification(notif);
        }

        /// <summary>
        /// UpdateNotification method implementation
        /// </summary>
        private bool DoUpdateNotification(Notification notif)
        {
            bool isok = false;
            using (DirectoryEntry rootdir = GetDirectoryEntry())
            {
                string qryldap = "(&(objectCategory=user)(objectClass=user)(objectGUID=" + GetBinaryStringFromGuidString(notif.RegistrationID) + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

                using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                {
                    dsusr.PropertiesToLoad.Clear();
                    dsusr.PropertiesToLoad.Add("objectGUID");
                    dsusr.PropertiesToLoad.Add(_host.notifcreatedateAttribute);
                    dsusr.PropertiesToLoad.Add(_host.notifvalidityAttribute);
                    dsusr.PropertiesToLoad.Add(_host.totpAttribute);

                    SearchResult sr = dsusr.FindOne();
                    if (sr != null)
                    {
                        isok = true;
                        using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                        {
                            DirEntry.Properties[_host.notifcreatedateAttribute].Value = notif.CreationDate.ToString("u");
                            DirEntry.Properties[_host.notifvalidityAttribute].Value = notif.ValidityDate.ToString("u");
                            DirEntry.Properties[_host.totpAttribute].Value = notif.OTP.ToString();
                            DirEntry.CommitChanges();
                        };
                    }
                }
            }
            return isok;
        }

        /// <summary>
        /// DoCheckNotification method implementation
        /// </summary>
        private Notification DoCheckNotification(Notification notif)
        {
            Notification ret = null;
            using (DirectoryEntry rootdir = GetDirectoryEntry())
            {
                string qryldap = "(&(objectCategory=user)(objectClass=user)(objectGUID=" + GetBinaryStringFromGuidString(notif.RegistrationID) + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

                using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                {
                    dsusr.PropertiesToLoad.Clear();
                    dsusr.PropertiesToLoad.Add("objectGUID");
                    dsusr.PropertiesToLoad.Add(_host.notifcreatedateAttribute);
                    dsusr.PropertiesToLoad.Add(_host.notifvalidityAttribute);
                    dsusr.PropertiesToLoad.Add(_host.notifcheckdateattribute);
                    dsusr.PropertiesToLoad.Add(_host.totpAttribute);

                    SearchResult sr = dsusr.FindOne();
                    if (sr != null)
                    {
                        ret = new Notification();
                        using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                        {
                            ret.ID = new Guid((byte[])DirEntry.Properties["objectGUID"].Value).ToString();
                            ret.RegistrationID = ret.ID;
                            ret.CreationDate = Convert.ToDateTime(DirEntry.Properties[_host.notifcreatedateAttribute].Value);
                            ret.ValidityDate = Convert.ToDateTime(DirEntry.Properties[_host.notifvalidityAttribute].Value);
                            ret.CheckDate = notif.CheckDate;
                            ret.OTP = Convert.ToInt32(DirEntry.Properties[_host.totpAttribute].Value.ToString());

                            DirEntry.Properties[_host.notifcheckdateattribute].Value = notif.CheckDate.Value.ToString("u");
                            DirEntry.CommitChanges();
                        };
                    }
                }
            }
            return ret;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// GetBinaryStringFromGuidString
        /// </summary>
        private string GetBinaryStringFromGuidString(string guidstring)
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
        private DirectoryEntry GetDirectoryEntry()
        {
            DirectoryEntry entry = null;
            if (!string.IsNullOrEmpty(_host.DomainAddress))
                entry = new DirectoryEntry(_host.DomainAddress);
            else
                entry = new DirectoryEntry();
            if (!string.IsNullOrEmpty(_host.Account))
                entry.Username = _host.Account;
            if (!string.IsNullOrEmpty(_host.Password))
                entry.Password = _host.Password;
            return entry;
        }

        /// <summary>
        /// GetDirectoryEntry() method implmentation
        /// </summary>
        private DirectoryEntry GetDirectoryEntry(SearchResult sr)
        {
            DirectoryEntry entry = sr.GetDirectoryEntry();
            entry.Path = sr.Path;
            if (!string.IsNullOrEmpty(_host.Account))
                entry.Username = _host.Account;
            if (!string.IsNullOrEmpty(_host.Password))
                entry.Password = _host.Password;
            return entry;
        }
        #endregion
    }
}
