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
namespace Neos.IdentityServer.MultiFactor.Administration
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Xml;

    #region PSRegistration class
    public class PSRegistrationList : List<PSRegistration>
    {
        /// <summary>
        /// explicit conversion to PSRegistrationList
        /// </summary>
        public static explicit operator PSRegistrationList(MMCRegistrationList registrations)
        {
            if (registrations == null)
                return null;
            PSRegistrationList lst = new PSRegistrationList();
            foreach (MMCRegistration reg in registrations)
            {
                lst.Add((PSRegistration)reg);
            }
            return lst;
        }

        /// <summary>
        /// explicit conversion from PSRegistrationList
        /// </summary>
        public static explicit operator MMCRegistrationList(PSRegistrationList registrations)
        {
            if (registrations == null)
                return null;
            MMCRegistrationList lst = new MMCRegistrationList();
            foreach (PSRegistration reg in registrations)
            {
                lst.Add((MMCRegistration)reg);
            }
            return lst;
        }
    }

    public class PSRegistration
    {
        /// <summary>
        /// Properties
        /// </summary>
        public string ID { get; internal set; }
        public string UPN { get; set; }
        public string MailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public bool Enabled { get; set; }
        internal bool IsRegistered { get; set; }
        public DateTime CreationDate { get; set; }
        public RegistrationPreferredMethod PreferredMethod { get; set; }
        internal bool IsApplied { get; set; }

        /// <summary>
        /// implicit conversion to PSRegistration
        /// </summary>
        public static explicit operator PSRegistration(MMCRegistration registration)
        {
            if (registration == null)
                return null;
            else
            {
                PSRegistration psnode = new PSRegistration();
                psnode.ID = registration.ID;
                psnode.UPN = registration.UPN;
                psnode.MailAddress = registration.MailAddress;
                psnode.PhoneNumber = registration.PhoneNumber;
                psnode.CreationDate = registration.CreationDate;
                psnode.IsRegistered = registration.IsRegistered;
                psnode.Enabled = registration.Enabled;
                psnode.PreferredMethod = registration.PreferredMethod;
                psnode.IsApplied = registration.IsApplied;
                return psnode;
            }
        }

        /// <summary>
        /// implicit conversion from PSRegistration
        /// </summary>
        public static explicit operator MMCRegistration(PSRegistration psnode)
        {
            if (psnode == null)
                return null;
            else
            {
                MMCRegistration registration = new MMCRegistration();
                registration.ID = psnode.ID;
                registration.UPN = psnode.UPN;
                registration.MailAddress = psnode.MailAddress;
                registration.PhoneNumber = psnode.PhoneNumber;
                registration.CreationDate = psnode.CreationDate;
                registration.IsRegistered = psnode.IsRegistered;
                registration.Enabled = psnode.Enabled;
                registration.PreferredMethod = psnode.PreferredMethod;
                registration.IsApplied = psnode.IsApplied;
                return registration;
            }
        }
    }
    #endregion

    public enum PSTemplateMode
    {
        Free = 0,                        // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
        Open = 1,                        // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
        Default = 2,                     // (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
        Managed = 3,                     // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword);
        Strict = 4,                      // (UserFeaturesOptions.AllowProvideInformations);
        Administrative = 5               // (UserFeaturesOptions.AdministrativeMode);   
    }

    #region PSConfig
    public class PSConfig
    {
        
        public int RefreshScan { get; set; }
        public int DeliveryWindow { get; set; }
        public int TOTPShadows { get; set; }
        public bool MailEnabled { get; set; }
        public bool SMSEnabled { get; set; }
        public bool AppsEnabled { get; set; }
        public HashMode Algorithm { get; set; }
        public string Issuer { get; set; }
        public bool UseActiveDirectory { get; set; }
        public bool CustomUpdatePassword { get; set; }
        public string DefaultCountryCode { get; set; }
        public string AdminContact { get; set; }
        public UserFeaturesOptions UserFeatures { get; set; }
        public MFAConfigAdvertising AdvertisingDays { get; set; }

        /// <summary>
        /// implicit conversion to PSConfig
        /// </summary>
        public static explicit operator PSConfig(MMCConfig config)
        {
            
            if (config == null)
                return null;
            else
            {
                PSConfig psconfig = new PSConfig();
                psconfig.AdminContact = config.AdminContact;
                psconfig.Algorithm = config.Algorithm;
                psconfig.AppsEnabled = config.AppsEnabled;
                psconfig.CustomUpdatePassword = config.CustomUpdatePassword;
                psconfig.DefaultCountryCode = config.DefaultCountryCode;
                psconfig.DeliveryWindow = config.DeliveryWindow;
                psconfig.Issuer = config.Issuer;
                psconfig.MailEnabled = config.MailEnabled;
                psconfig.RefreshScan = config.RefreshScan;
                psconfig.SMSEnabled = config.SMSEnabled;
                psconfig.TOTPShadows = config.TOTPShadows;
                psconfig.UseActiveDirectory = config.UseActiveDirectory;
                psconfig.UserFeatures = config.UserFeatures;
                psconfig.AdvertisingDays = config.AdvertisingDays;
                return psconfig;
            }
        }

        /// <summary>
        /// implicit conversion from PSConfig
        /// </summary>
        public static explicit operator MMCConfig(PSConfig psconfig)
        {
            if (psconfig == null)
                return null;
            else
            {
                MMCConfig config = new MMCConfig();
                config.AdminContact = psconfig.AdminContact;
                config.Algorithm = psconfig.Algorithm;
                config.AppsEnabled = psconfig.AppsEnabled;
                config.CustomUpdatePassword = psconfig.CustomUpdatePassword;
                config.DefaultCountryCode = psconfig.DefaultCountryCode;
                config.DeliveryWindow = psconfig.DeliveryWindow;
                config.IsDirty = true;
                config.Issuer = psconfig.Issuer;
                config.MailEnabled = psconfig.MailEnabled;
                config.RefreshScan = psconfig.RefreshScan;
                config.SMSEnabled = psconfig.SMSEnabled;
                config.TOTPShadows = psconfig.TOTPShadows;
                config.UseActiveDirectory = psconfig.UseActiveDirectory;
                config.UserFeatures = psconfig.UserFeatures;
                config.AdvertisingDays = psconfig.AdvertisingDays;
                return config;
            }
        }
    }
    #endregion

    #region PSConfigSQL
    public class PSConfigSQL
    {
        public string ConnectionString { get; set; }

        /// <summary>
        /// implicit conversion to PSConfig
        /// </summary>
        public static explicit operator PSConfigSQL(MMCConfigSQL sqlconfig)
        {
            if (sqlconfig == null)
                return null;
            else
            {
                PSConfigSQL psconfigsql = new PSConfigSQL();
                psconfigsql.ConnectionString = sqlconfig.ConnectionString;
                return psconfigsql;
            }
        }

        /// <summary>
        /// implicit conversion from PSConfig
        /// </summary>
        public static explicit operator MMCConfigSQL(PSConfigSQL psconfig)
        {
            if (psconfig == null)
                return null;
            else
            {
                MMCConfigSQL config = new MMCConfigSQL();
                config.IsDirty = true;
                config.ConnectionString = psconfig.ConnectionString;
                return config;
            }
        }
    }
    #endregion

    #region PSConfigADDS
    public class PSConfigADDS
    {
        public string Account { get; set; }
        public string DomainAddress { get; set; }
        public string KeyAttribute { get; set; }
        public string MailAttribute { get; set; }
        public string MethodAttribute { get; set; }
        public string NotifCheckDateAttribute { get; set; }
        public string NotifCreateDateAttribute { get; set; }
        public string NotifValidityAttribute { get; set; }
        public string Password { get; set; }
        public string PhoneAttribute { get; set; }
        public string TOTPAttribute { get; set; }
        public string TOTPEnabledAttribute { get; set; }

        /// <summary>
        /// implicit conversion to PSConfig
        /// </summary>
        public static explicit operator PSConfigADDS(MMCConfigADDS addsconfig)
        {
            if (addsconfig == null)
                return null;
            else
            {
                PSConfigADDS psconfigadds = new PSConfigADDS();
                psconfigadds.Account = addsconfig.Account;
                psconfigadds.Password = addsconfig.Password;
                psconfigadds.DomainAddress = addsconfig.DomainAddress;
                psconfigadds.KeyAttribute = addsconfig.KeyAttribute;
                psconfigadds.MailAttribute = addsconfig.MailAttribute;
                psconfigadds.MethodAttribute = addsconfig.MethodAttribute;
                psconfigadds.NotifCheckDateAttribute = addsconfig.NotifCheckDateAttribute;
                psconfigadds.NotifCreateDateAttribute = addsconfig.NotifCreateDateAttribute;
                psconfigadds.NotifValidityAttribute = addsconfig.NotifValidityAttribute;
                psconfigadds.PhoneAttribute = addsconfig.PhoneAttribute;
                psconfigadds.TOTPAttribute = addsconfig.TOTPAttribute;
                psconfigadds.TOTPEnabledAttribute = addsconfig.TOTPEnabledAttribute;
                return psconfigadds;
            }
        }

        /// <summary>
        /// implicit conversion from PSConfig
        /// </summary>
        public static explicit operator MMCConfigADDS(PSConfigADDS psconfig)
        {
            if (psconfig == null)
                return null;
            else
            {
                MMCConfigADDS config = new MMCConfigADDS();
                config.IsDirty = true;
                config.Account = psconfig.Account;
                config.Password = psconfig.Password;
                config.DomainAddress = psconfig.DomainAddress;
                config.KeyAttribute = psconfig.KeyAttribute;
                config.MailAttribute = psconfig.MailAttribute;
                config.MethodAttribute = psconfig.MethodAttribute;
                config.NotifCheckDateAttribute = psconfig.NotifCheckDateAttribute;
                config.NotifCreateDateAttribute = psconfig.NotifCreateDateAttribute;
                config.NotifValidityAttribute = psconfig.NotifValidityAttribute;
                config.PhoneAttribute = psconfig.PhoneAttribute;
                config.TOTPAttribute = psconfig.TOTPAttribute;
                config.TOTPEnabledAttribute = psconfig.TOTPEnabledAttribute;
                return config;
            }
        }
    }
    #endregion

    #region PSConfigMail
    public class PSConfigMail
    {
        public string From { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string Company { get; set; }
        public PSConfigMailFileNames MailOTP { get; set; }
        public PSConfigMailFileNames MailInscription { get; set; }
        public PSConfigMailFileNames MailSecureKey { get; set; }

        public PSConfigMail()
        {
            this.MailOTP = new PSConfigMailFileNames();
            this.MailInscription = new PSConfigMailFileNames();
            this.MailSecureKey = new PSConfigMailFileNames();
        }

        /// <summary>
        /// explicit conversion to PSConfigMail
        /// </summary>
        public static explicit operator PSConfigMail(MMCConfigMail mails)
        {
            if (mails == null)
                return null;
            else
            {
                PSConfigMail psconfig = new PSConfigMail();
                psconfig.From = mails.From;
                psconfig.UserName = mails.UserName;
                psconfig.Password = mails.Password;
                psconfig.Host = mails.Host;
                psconfig.Port = mails.Port;
                psconfig.UseSSL = mails.UseSSL;
                psconfig.Company = mails.Company;
                psconfig.MailOTP.Templates.Clear();
                foreach (MMCConfigMailFileName itm in mails.MailOTPContent)
                {
                    psconfig.MailOTP.Templates.Add((PSConfigMailFileName)itm);
                }
                psconfig.MailInscription.Templates.Clear();
                foreach (MMCConfigMailFileName itm in mails.MailAdminContent)
                {
                    psconfig.MailInscription.Templates.Add((PSConfigMailFileName)itm);
                }
                psconfig.MailSecureKey.Templates.Clear();
                foreach (MMCConfigMailFileName itm in mails.MailKeyContent)
                {
                    psconfig.MailSecureKey.Templates.Add((PSConfigMailFileName)itm);
                }
                return psconfig;
            }
        }

        /// <summary>
        /// explicit conversion from PSConfigMail
        /// </summary>
        public static explicit operator MMCConfigMail(PSConfigMail mails)
        {
            if (mails == null)
                return null;
            else
            {
                MMCConfigMail psconfig = new MMCConfigMail();
                psconfig.IsDirty = true;
                psconfig.From = mails.From;
                psconfig.UserName = mails.UserName;
                psconfig.Password = mails.Password;
                psconfig.Host = mails.Host;
                psconfig.Port = mails.Port;
                psconfig.UseSSL = mails.UseSSL;
                psconfig.Company = mails.Company;
                psconfig.MailOTPContent.Clear();
                foreach (PSConfigMailFileName itm in mails.MailOTP.Templates)
                {
                    psconfig.MailOTPContent.Add((MMCConfigMailFileName)itm);
                }
                psconfig.MailAdminContent.Clear();
                foreach (PSConfigMailFileName itm in mails.MailInscription.Templates)
                {
                    psconfig.MailAdminContent.Add((MMCConfigMailFileName)itm);
                }
                psconfig.MailKeyContent.Clear();
                foreach (PSConfigMailFileName itm in mails.MailSecureKey.Templates)
                {
                    psconfig.MailKeyContent.Add((MMCConfigMailFileName)itm);
                }
                return psconfig;
            }
        }
    }

    public class PSConfigMailFileName
    {

        public int LCID { get; set; }
        public string FileName { get; set; }
        public bool Enabled { get; set; }

        public PSConfigMailFileName(int lcid, string filename, bool enabled = true)
        {
            this.LCID = lcid;
            this.FileName = filename;
            this.Enabled = enabled;
        }

        public static explicit operator PSConfigMailFileName(MMCConfigMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new PSConfigMailFileName(file.LCID, file.FileName, file.Enabled);
        }

        public static explicit operator MMCConfigMailFileName(PSConfigMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new MMCConfigMailFileName(file.LCID, file.FileName, file.Enabled);
        }
    }

    public class PSConfigMailFileNames
    {
        private List<PSConfigMailFileName> _list = new List<PSConfigMailFileName>();

        public List<PSConfigMailFileName> Templates 
        { 
            get 
            { 
                return _list; 
            } 
        }

        public void AddTemplate(int lcid, string filename, bool enabled = true)
        {
            try
            {
                var item = (from i in _list where i.LCID == lcid select i).First();
                _list.Add(new PSConfigMailFileName(lcid, filename, enabled));
            }
            catch (Exception ex)
            {
                throw new Exception("Template always exists !", ex);
            }
        }

        public void SetTemplate(int lcid, string filename, bool enabled = true)
        {
            try
            {
                PSConfigMailFileName item = (from it in _list where it.LCID == lcid select it).First();
                int i = _list.IndexOf(item);
                item.FileName = filename;
                item.Enabled = enabled;
                _list[i] = item;
            }
            catch (Exception)
            {
                _list.Add(new PSConfigMailFileName(lcid, filename, enabled));
            }
        }

        public void RemoveTemplate(int lcid)
        {
            try
            {
                PSConfigMailFileName item = (from it in _list where it.LCID == lcid select it).First();
                int i = _list.IndexOf(item);
                _list.RemoveAt(i);
            }
            catch (Exception ex)
            {
                throw new Exception("Template dosen't exists !", ex);
            }
        }
    }
    #endregion

    #region PSKeysConfig
    public class PSKeysConfig
    {
        public KeyGeneratorMode KeyGenerator { get; set; }
        public KeySizeMode KeySize { get; set; }
        public RegistrationSecretKeyFormat KeyFormat { get; set; }
        public string CertificateThumbprint { get; set; }
        public int CertificateValidity { get; set; }
        public PSExternalKeyManager ExternalKeyManager { get; set; }

        public PSKeysConfig()
        {
            this.ExternalKeyManager = new PSExternalKeyManager();
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator PSKeysConfig(MMCKeysConfig mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                PSKeysConfig target = new PSKeysConfig();
                target.CertificateThumbprint = mgr.CertificateThumbprint;
                target.CertificateValidity = mgr.CertificateValidity;
                target.KeyFormat = mgr.KeyFormat;
                target.KeyGenerator = mgr.KeyGenerator;
                target.KeySize = mgr.KeySize;
                target.ExternalKeyManager = (PSExternalKeyManager)mgr.ExternalKeyManager;
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator MMCKeysConfig(PSKeysConfig mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                MMCKeysConfig target = new MMCKeysConfig();
                target.IsDirty = true;
                target.CertificateThumbprint = mgr.CertificateThumbprint;
                target.CertificateValidity = mgr.CertificateValidity;
                target.KeyFormat = mgr.KeyFormat;
                target.KeyGenerator = mgr.KeyGenerator;
                target.KeySize = mgr.KeySize;
                target.ExternalKeyManager = (MMCExternalKeyManager)mgr.ExternalKeyManager;
                return target;
            }
        }
    }

    public class PSExternalKeyManager
    {
        public string FullQualifiedImplementation { get; set; }
        public XmlCDataSection Parameters { get; set; }

        public static explicit operator PSExternalKeyManager(MMCExternalKeyManager mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                PSExternalKeyManager ret = new PSExternalKeyManager();
                ret.FullQualifiedImplementation = mgr.FullQualifiedImplementation;
                ret.Parameters = mgr.Parameters;
                return ret;
            }
        }

        public static explicit operator MMCExternalKeyManager(PSExternalKeyManager mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                MMCExternalKeyManager ret = new MMCExternalKeyManager();
                ret.FullQualifiedImplementation = mgr.FullQualifiedImplementation;
                ret.Parameters = mgr.Parameters;
                return ret;
            }
        }
    }
    #endregion

    #region PSExternalOTPProvider
    public class PSExternalOTPProvider
    {
        public string Company { get; set; }
        public string Sha1Salt { get; set; }
        public string FullQualifiedImplementation { get; set; }
        public XmlCDataSection Parameters { get; set; }
        public bool IsTwoWay { get; set; }
        public int Timeout { get; set; }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator PSExternalOTPProvider(MMCExternalOTPProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                PSExternalOTPProvider target = new PSExternalOTPProvider();
                target.Company = otp.Company;
                target.FullQualifiedImplementation = otp.FullQualifiedImplementation;
                target.IsTwoWay = otp.IsTwoWay;
                target.Sha1Salt = otp.Sha1Salt;
                target.Timeout = otp.Timeout;
                target.Parameters = otp.Parameters;
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator MMCExternalOTPProvider(PSExternalOTPProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                MMCExternalOTPProvider target = new MMCExternalOTPProvider();
                target.IsDirty = true;
                target.Company = otp.Company;
                target.FullQualifiedImplementation = otp.FullQualifiedImplementation;
                target.IsTwoWay = otp.IsTwoWay;
                target.Sha1Salt = otp.Sha1Salt;
                target.Timeout = otp.Timeout;
                target.Parameters = otp.Parameters;
                return target;
            }
        }
    }
    #endregion
}