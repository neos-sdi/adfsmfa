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
                string qryldap = "(&(objectCategory=user)(objectClass=user)(|";
                qryldap += "(userprincipalname=" + upn + ")";
                qryldap += ")(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
                qryldap += ")";
               
                using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                {
                    dsusr.PropertiesToLoad.Clear();
                    dsusr.PropertiesToLoad.Add("objectGUID");
                    dsusr.PropertiesToLoad.Add("userPrincipalName");
                    dsusr.PropertiesToLoad.Add("whenCreated");
                    dsusr.PropertiesToLoad.Add(_host.keyAttribute);
                    dsusr.PropertiesToLoad.Add(_host.mailAttribute);
                    dsusr.PropertiesToLoad.Add(_host.phoneAttribute);
                    dsusr.PropertiesToLoad.Add(_host.methodAttribute);

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
                                    reg.MailAddress = DirEntry.Properties[_host.mailAttribute].Value.ToString();
                                if (DirEntry.Properties[_host.phoneAttribute].Value != null)
                                    reg.PhoneNumber = DirEntry.Properties[_host.phoneAttribute].Value.ToString();
                                if (DirEntry.Properties[_host.methodAttribute].Value != null)
                                    reg.PreferredMethod = (RegistrationPreferredMethod)Enum.Parse(typeof(RegistrationPreferredMethod), DirEntry.Properties[_host.methodAttribute].Value.ToString(), true);
                                if (DirEntry.Properties[_host.keyAttribute].Value != null)
                                    reg.SecretKey = DirEntry.Properties[_host.keyAttribute].Value.ToString();
                            }
                            else
                                return null;
                            reg.Enabled = true;                            
                        };
                    }
                }
            }
            return reg;
        }

        /// <summary>
        /// SetUserRegistration method implementation
        /// </summary>
        public void SetUserRegistration(Registration reg)
        {
            using (DirectoryEntry rootdir = GetDirectoryEntry())
            {
                string qryldap = "(&(objectCategory=user)(objectClass=user)(|";
                qryldap += "(userprincipalname=" + reg.UPN + ")";
                qryldap += ")(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
                // qryldap += ")(userAccountControl:1.2.840.113556.1.4.803:=512)";
                qryldap += ")";

                using (DirectorySearcher dsusr = new DirectorySearcher(rootdir, qryldap))
                {
                    dsusr.PropertiesToLoad.Clear();
                    dsusr.PropertiesToLoad.Add("userPrincipalName");

                    SearchResult sr = dsusr.FindOne();
                    if (sr != null)
                    {
                        using (DirectoryEntry DirEntry = GetDirectoryEntry(sr))
                        {
                            DirEntry.Properties[_host.keyAttribute].Value = reg.SecretKey;
                            if (!string.IsNullOrEmpty(reg.MailAddress))
                                DirEntry.Properties[_host.mailAttribute].Value = reg.MailAddress;
                            else
                                DirEntry.Properties[_host.mailAttribute].Clear();
                            if (!string.IsNullOrEmpty(reg.PhoneNumber))
                                DirEntry.Properties[_host.phoneAttribute].Value = reg.PhoneNumber;
                            else
                                DirEntry.Properties[_host.phoneAttribute].Clear();
                            DirEntry.Properties[_host.methodAttribute].Value = ((int)reg.PreferredMethod).ToString();
                            DirEntry.CommitChanges();
                        };
                    }
                }
            }
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
                string qryldap = "(&(objectCategory=user)(objectClass=user)(|";
                qryldap += "(objectGUID=" + GetBinaryStringFromGuidString(notif.RegistrationID) + ")";
                qryldap += ")(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
                // qryldap += ")(userAccountControl:1.2.840.113556.1.4.803:=512)";
                qryldap += ")";

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
                string qryldap = "(&(objectCategory=user)(objectClass=user)(|";
                qryldap += "(objectGUID=" + GetBinaryStringFromGuidString(notif.RegistrationID) + ")";
                qryldap += ")(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
                // qryldap += ")(userAccountControl:1.2.840.113556.1.4.803:=512)";
                qryldap += ")";

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

    }
}
