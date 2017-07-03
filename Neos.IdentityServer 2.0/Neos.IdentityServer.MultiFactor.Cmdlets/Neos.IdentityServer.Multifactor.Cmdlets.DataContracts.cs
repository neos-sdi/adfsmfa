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
    /// <summary>
    /// PSRegistrationList class
    /// <para type="synopsis">List of user entry in MFA System.</para>
    /// <para type="description">List a user entry registered with MFA.</para>
    /// </summary>
    /// <example>
    ///   <para>$users = Get-MFAUsers</para>
    /// </example>
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

    /// <summary>
    /// PSRegistration class
    /// <para type="synopsis">User entry in MFA System.</para>
    /// <para type="description">Represent a user entry registered with MFA.</para>
    /// <para type="description">You can access, update each user property.</para>
    /// </summary>
    /// <example>
    ///   <para>$u = Get-MFAUsers -Identity user@domain.com</para>
    ///   <para>$u.MailAddress = usermail@domain.com</para>
    ///   <para>$u.PhoneNumber = 0102030405</para>
    ///   <para>$u.PreferredMethod = Phone</para>
    ///   <para>Set-MFAUsers $u</para> 
    /// </example>
    public class PSRegistration
    {
        /// <summary>
        /// Properties
        /// </summary>
        public string ID { get; internal set; }

        /// <summary>
        /// <para type="description">User identity (upn).</para>
        /// </summary>
        public string UPN { get; set; }

        /// <summary>
        /// <para type="description">User mail address.</para>
        /// </summary>
        public string MailAddress { get; set; }

        /// <summary>
        /// <para type="description">User mobile phone number.</para>
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// <para type="description">User status.</para>
        /// </summary>
        public bool Enabled { get; set; }
        internal bool IsRegistered { get; set; }

        /// <summary>
        /// <para type="description">User registration or creation date.</para>
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// <para type="description">Preferred MFA method : Choose, Code, email, Phone, Face (for future use).</para>
        /// </summary>
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

    /// <summary>
    /// PSTemplateMode
    /// <para type="synopsis">Policy templates for users features.</para>
    /// <para type="description">Policy templates for users featuresregistered with MFA.</para>
    /// </summary>
    public enum PSTemplateMode
    {
        /// <summary>
        /// <para type="description">(UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword)</para>
        /// </summary>
        Free = 0,                        // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);

        /// <summary>
        /// <para type="description">(UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword)</para>
        /// </summary>
        Open = 1,                        // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);

        /// <summary>
        /// <para type="description">(UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword)</para>
        /// </summary>
        Default = 2,                     // (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);

        /// <summary>
        /// <para type="description">(UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword)</para>
        /// </summary>
        Managed = 3,                     // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword);

        /// <summary>
        /// <para type="description">(UserFeaturesOptions.AllowProvideInformations)</para>
        /// </summary>
        Strict = 4,                      // (UserFeaturesOptions.AllowProvideInformations);

        /// <summary>
        /// <para type="description">(UserFeaturesOptions.AdministrativeMode)</para>
        /// </summary>
        Administrative = 5               // (UserFeaturesOptions.AdministrativeMode);   
    }

    #region PSConfig
    /// <summary>
    /// PSConfig class
    /// <para type="synopsis">Main configuration properties in MFA System.</para>
    /// <para type="description">Represent Main configuration properties registered with MFA.</para>
    /// <para type="description">You can access, update each config property.</para>
    /// </summary>
    /// <example>
    ///   <para>$cfg = Get-MFAConfig</para>
    ///   <para>$cfg.UserActiveDirectory = $true</para>
    ///   <para>Set-MFAConfig $cfg</para> 
    /// </example>
    public class PSConfig
    {

        /// <summary>
        /// <para type="description">Delay between page refresh status when requesting a TOTP (milliseconds 3000 by default).</para>
        /// </summary>
        public int RefreshScan { get; set; }

        /// <summary>
        /// <para type="description">Max delay for the user to enter TOTP code (seconds 300 by default).</para>
        /// </summary>
        public int DeliveryWindow { get; set; }

        /// <summary>
        /// <para type="description">Number of prior TOTP codes allowed (default 2). Code change every 30 seconds.</para>
        /// </summary>
        public int TOTPShadows { get; set; }

        /// <summary>
        /// <para type="description">Globally allow MFA with sending email to users, less secure than TOTP Code.</para>
        /// <para type="description">Must specify properties of ConfigMail.</para>
        /// </summary>
        public bool MailEnabled { get; set; }

        /// <summary>
        /// <para type="description">Globally allow MFA with external Code Provider, sending SMS to users, less secure than TOTP Code.</para>
        /// <para type="description">Must define ExternalTOTPProvider (see SMS Azure sample).</para>
        /// </summary>
        public bool SMSEnabled { get; set; }

        /// <summary>
        /// <para type="description">Globally allow MFA with TOTP, users are using an applition to generate TOTP codes based on thier secret key (Default mode).</para>
        /// </summary>
        public bool AppsEnabled { get; set; }

        /// <summary>
        /// <para type="description">TOTP Hash mode for TOTP Key (Default SHA1).</para>
        /// </summary>
        public HashMode Algorithm { get; set; }

        /// <summary>
        /// <para type="description">Issuer description (eg "my company").</para>
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// <para type="description">If true, users metadata are stored in ADDS attributes see : Get-MFAConfigADDS.</para>
        /// <para type="description">If true, users metadata are stored in SQL Server database see : Get-MFAConfigSQL and New-MFADatabase.</para>/// 
        /// </summary>
        public bool UseActiveDirectory { get; set; }

        /// <summary>
        /// <para type="description">Use or not our implementation for changing user password,if not we are using /ADFS/Portal/updatepasswor.</para>
        /// </summary>
        public bool CustomUpdatePassword { get; set; }

        /// <summary>
        /// <para type="description">Default contry code, used for SMS calls .</para>
        /// </summary>
        public string DefaultCountryCode { get; set; }

        /// <summary>
        /// <para type="description">Administrators email, used in administrative emails sent to users.</para>
        /// </summary>
        public string AdminContact { get; set; }

        /// <summary>
        /// <para type="description">Policy attributes for users management and registration.</para>
        /// </summary>
        public UserFeaturesOptions UserFeatures { get; set; }

        /// <summary>
        /// <para type="description">Policy attributes for warnings to users.</para>
        /// </summary>
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
    /// <summary>
    /// PSConfigSQL class
    /// <para type="synopsis">SQL configuration properties in MFA System.</para>
    /// <para type="description">SQL configuration properties registered with MFA.</para>
    /// <para type="description">You can access, update connectionString property.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAConfigSQL</para>
    /// </example>
    public class PSConfigSQL
    {
        /// <summary>
        /// <para type="description">Get or Set the connection string used to access MFA SQL Database.</para>
        /// </summary>
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
    /// <summary>
    /// PSConfigADDS class
    /// <para type="synopsis">ADDS configuration properties in MFA System.</para>
    /// <para type="description">ADDS configuration properties registered with MFA.</para>
    /// <para type="description">You can access, update attributes properties.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAConfigADDS</para>
    /// </example>
    public class PSConfigADDS
    {
        /// <summary>
        /// <para type="description">Account name to access (read/write) Active Directory (if ADFS account has rights you leave it blank).</para>
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// <para type="description">Domain name to access (read/write) Active Directory (if ADFS account has rights you leave it blank).</para>
        /// </summary>
        public string DomainAddress { get; set; }

        /// <summary>
        /// <para type="description">ADDS atribute name user to store user secret key (default msDS-cloudExtensionAttribute10).</para>
        /// </summary>
        public string KeyAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS atribute name used to store user custom mail address (default msDS-cloudExtensionAttribute11).</para>
        /// </summary>
        public string MailAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS atribute name used to store user preferred method (Code, Phone, Mail) (default msDS-cloudExtensionAttribute13).</para>
        /// </summary>
        public string MethodAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS atribute name used to store last verification timestamp (default msDS-cloudExtensionAttribute16).</para>
        /// </summary>
        public string NotifCheckDateAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS atribute name used to store user registration timestamp (default msDS-cloudExtensionAttribute14).</para>
        /// </summary>
        public string NotifCreateDateAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS atribute name used to store TOTP code validity timestamp (default msDS-cloudExtensionAttribute15).</para>
        /// </summary>
        public string NotifValidityAttribute { get; set; }

        /// <summary>
        /// <para type="description">Password used for account to access (read/write) Active Directory (if ADFS account has rights you leave it blank).</para>
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// <para type="description">ADDS atribute name used to store user phone number (default msDS-cloudExtensionAttribute12).</para>
        /// </summary>
        public string PhoneAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS atribute name used to store TOTP code for email notification (default msDS-cloudExtensionAttribute17).</para>
        /// </summary>
        public string TOTPAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS atribute name used to store user status with MFA (default msDS-cloudExtensionAttribute18).</para>
        /// </summary>
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
    /// <summary>
    /// PSConfigMail class
    /// <para type="synopsis">SMTP configuration properties in MFA System.</para>
    /// <para type="description">SMTP/POP configuration properties registered with MFA.</para>
    /// <para type="description">You can access, update attributes properties.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAConfigMail</para>
    /// </example>
    public class PSConfigMail
    {
        /// <summary>
        /// <para type="description">Mail from property.</para>
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// <para type="description">Account Name used to access Mail platform.</para>
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// <para type="description">Password used with Username to access Mail platform.</para>
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// <para type="description">Mail platform Host eg : smtp.office365.com.</para>
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// <para type="description">Mail platform IP Port eg : 587.</para>
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// <para type="description">Mail platform SSL option.</para>
        /// </summary>
        public bool UseSSL { get; set; }

        /// <summary>
        /// <para type="description">Your company description, tis is used in default mails contents.</para>
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// <para type="description">Custom mail templates.</para>
        /// </summary>
        public PSConfigMailFileNames MailOTP { get; set; }

        /// <summary>
        /// <para type="description">Custom mail templates.</para>
        /// </summary>
        public PSConfigMailFileNames MailInscription { get; set; }

        /// <summary>
        /// <para type="description">Custom mail templates.</para>
        /// </summary>
        public PSConfigMailFileNames MailSecureKey { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
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

    /// <summary>
    /// PSConfigMailFileName class
    /// <para type="synopsis">Mail custom templates used in MFA System.</para>
    /// <para type="description">Mail custom templates registered with MFA.</para>
    /// </summary>
    public class PSConfigMailFileName
    {
        /// <summary>
        /// <para type="description">LCID (1033, 1034, 1036, 3082).</para>
        /// </summary>
        public int LCID { get; set; }

        /// <summary>
        /// <para type="description">File path to Html file user as custom template.</para>
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// <para type="description">Enabled status for custom template.</para>
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PSConfigMailFileName(int lcid, string filename, bool enabled = true)
        {
            this.LCID = lcid;
            this.FileName = filename;
            this.Enabled = enabled;
        }

        /// <summary>
        /// explicit operator 
        /// </summary>
        public static explicit operator PSConfigMailFileName(MMCConfigMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new PSConfigMailFileName(file.LCID, file.FileName, file.Enabled);
        }

        /// <summary>
        /// explicit operator 
        /// </summary>
        public static explicit operator MMCConfigMailFileName(PSConfigMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new MMCConfigMailFileName(file.LCID, file.FileName, file.Enabled);
        }
    }

    /// <summary>
    /// PSConfigMailFileName class
    /// <para type="synopsis">Mail custom templates collection used in MFA System.</para>
    /// <para type="description">Mail custom templates collection registered with MFA.</para>
    /// </summary>
    public class PSConfigMailFileNames
    {
        private List<PSConfigMailFileName> _list = new List<PSConfigMailFileName>();

        /// <summary>
        /// <para type="description">Templates property.</para>
        /// </summary>
        public List<PSConfigMailFileName> Templates 
        { 
            get 
            { 
                return _list; 
            } 
        }

        /// <summary>
        /// AddTemplate method implmentation
        /// </summary>
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

        /// <summary>
        /// SetTemplate method implmentation
        /// </summary>
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

        /// <summary>
        /// RemoveTemplate method implmentation
        /// </summary>
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
    /// <summary>
    /// PSKeysConfig class
    /// <para type="synopsis">Secret key Management used in MFA System.</para>
    /// <para type="description">Secret key Management registered with MFA.</para>
    /// </summary>
    public class PSKeysConfig
    {
        /// <summary>
        /// <para type="description">Used when RNG is selected, for choosing the size of the generated random number (128 to 512 bytes).</para>
        /// </summary>
        public KeyGeneratorMode KeyGenerator { get; set; }

        /// <summary>
        /// <para type="description">Used to trim the key at a fixed size, when you use RSA the key is very long, and QRCode is often too big for TOTP Application (1024 is a good size, even if RSA key is 2048 bytes long).</para>
        /// </summary>
        public KeySizeMode KeySize { get; set; }

        /// <summary>
        /// <para type="description">Type of generated Keys for users (RNG, RSA, CUSTOM RSA).</para>
        /// <para type="description">Changing the key format, invalidate all the users secret keys previously used.</para>
        /// <para type="description">RSA and RSA Custom are using Certificates. Custom RSA must Use Specific database to the keys and certs, one for each user, see New-MFASecretKeysDatabase cmdlet.</para>
        /// </summary>
        public RegistrationSecretKeyFormat KeyFormat { get; set; }

        /// <summary>
        /// <para type="description">Certificate Thumbprint when using KeyFormat==RSA. the certificate is deployed on all ADFS servers in Crypting Certificates store</para>
        /// </summary>
        public string CertificateThumbprint { get; set; }

        /// <summary>
        /// <para type="description">Certificate validity duration in Years (5 by default)</para>
        /// </summary>
        public int CertificateValidity { get; set; }

        /// <summary>
        /// <para type="description">External key Manager when using CUSTOM Keyformat.</para>
        /// <para type="description">You must specify an assembly reference and parameters.</para>
        /// </summary>
        public PSExternalKeyManager ExternalKeyManager { get; set; }

        /// <summary>
        /// PSKeysConfig constructor
        /// </summary>
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

    /// <summary>
    /// PSExternalKeyManager class
    /// <para type="synopsis">Secret key Management used in MFA System.</para>
    /// <para type="description">Secret key Management registered with MFA.</para>
    /// <para type="description">You must specify an assembly an parametes.</para>
    /// </summary>
    public class PSExternalKeyManager
    {
        /// <summary>
        /// <para type="description">Full qualified assembly ref that implements ISecretKeyManager, see sample implementation of Neos.IdentityServer.Multifactor.Keys.CustomKeyManager</para>
        /// </summary>
        public string FullQualifiedImplementation { get; set; }

        /// <summary>
        /// <para type="description">Specify your own parameters, values stored as CNAME, set it as string with Parameters.Data = "myparameters"</para>
        /// </summary>
        public XmlCDataSection Parameters { get; set; }

        /// <summary>
        /// explicit operator 
        /// </summary>
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

        /// <summary>
        /// explicit operator 
        /// </summary>
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
    /// <summary>
    /// PSExternalOTPProvider class
    /// <para type="synopsis">Specify External OTP Provider, you must implement IExternalOTPProvider interface.</para>
    /// <para type="description">Samples are provided for Azure and custom.</para>
    /// <para type="description">Typically this component is used when sending SMS, you can use your own SMS gateway.</para>
    /// </summary>
    public class PSExternalOTPProvider
    {
        /// <summary>
        /// <para type="description">your company name, can be used to format External message sent to user.</para>
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// <para type="description">Optional Salt value, if your gateway support this feature.</para>
        /// </summary>
        public string Sha1Salt { get; set; }

        /// <summary>
        /// <para type="description">Full qualified assembly ref that implements IExternalOTPProvider, see sample implementation of Neos.IdentityServer.Multifactor.SMS.SMSCall</para>
        /// </summary>
        public string FullQualifiedImplementation { get; set; }

        /// <summary>
        /// <para type="description">Specify your own parameters, values stored as CNAME, set it as string with Parameters.Data = "myparameters"</para>
        /// </summary>
        public XmlCDataSection Parameters { get; set; }

        /// <summary>
        /// <para type="description">Pass parameter to your implemented provider, indicating if the mode is Request/Response</para>
        /// </summary>
        public bool IsTwoWay { get; set; }

        /// <summary>
        /// <para type="description">TimeOut Before cancelling operation</para>
        /// </summary>
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