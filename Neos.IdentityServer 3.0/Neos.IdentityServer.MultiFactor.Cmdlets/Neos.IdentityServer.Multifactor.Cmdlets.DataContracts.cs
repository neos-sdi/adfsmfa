using Neos.IdentityServer.MultiFactor.Cmdlets;
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
namespace MFA
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Xml;
    using Neos.IdentityServer.MultiFactor;
    using Neos.IdentityServer.MultiFactor.Administration;
    using System.Management.Automation;
    using System.Collections;
    using System.Management.Automation.Language;
    using System.Text;

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
        public static explicit operator PSRegistrationList(MFAUserList registrations)
        {
            if (registrations == null)
                return null;
            PSRegistrationList lst = new PSRegistrationList();
            foreach (MFAUser reg in registrations)
            {
                lst.Add((PSRegistration)reg);
            }
            return lst;
        }

        /// <summary>
        /// explicit conversion from PSRegistrationList
        /// </summary>
        public static explicit operator MFAUserList(PSRegistrationList registrations)
        {
            if (registrations == null)
                return null;
            MFAUserList lst = new MFAUserList();
            foreach (PSRegistration reg in registrations)
            {
                lst.Add((MFAUser)reg);
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
        /// <para type="description">User's PIN code. 0 if none.</para>
        /// </summary>
        public int PIN { get; set; }

        /// <summary>
        /// <para type="description">User status.</para>
        /// </summary>
        public bool Enabled { get; set; }


        internal bool IsRegistered { get; set; }

        /// <summary>
        /// <para type="description">Preferred MFA method : Choose, Code, email, Phone, Face (for future use).</para>
        /// </summary>
        public PSPreferredMethod PreferredMethod { get; set; }

        internal string OverrideMethod { get; set; }
        internal bool IsApplied { get; set; }

        /// <summary>
        /// implicit conversion to PSRegistration
        /// </summary>
        public static explicit operator PSRegistration(MFAUser registration)
        {
            if (registration == null)
                return null;
            else
            {
                PSRegistration psnode = new PSRegistration
                {
                    ID = registration.ID,
                    UPN = registration.UPN,
                    MailAddress = registration.MailAddress,
                    PhoneNumber = registration.PhoneNumber,
                    IsRegistered = registration.IsRegistered,
                    Enabled = registration.Enabled,
                    PreferredMethod = (PSPreferredMethod)registration.PreferredMethod,
                    OverrideMethod = registration.OverrideMethod,
                    PIN = registration.PIN,
                    IsApplied = registration.IsApplied
                };
                return psnode;
            }
        }

        /// <summary>
        /// implicit conversion from PSRegistration
        /// </summary>
        public static explicit operator MFAUser(PSRegistration psnode)
        {
            if (psnode == null)
                return null;
            else
            {
                MFAUser registration = new MFAUser
                {
                    ID = psnode.ID,
                    UPN = psnode.UPN,
                    MailAddress = psnode.MailAddress,
                    PhoneNumber = psnode.PhoneNumber,
                    IsRegistered = psnode.IsRegistered,
                    Enabled = psnode.Enabled,
                    PreferredMethod = (PreferredMethod)psnode.PreferredMethod,
                    OverrideMethod = psnode.OverrideMethod,
                    PIN = psnode.PIN,
                    IsApplied = psnode.IsApplied
                };
                return registration;
            }
        }
    }
    #endregion

    #region PSConfig
    /// <summary>
    /// PSGeneralConfiguration class
    /// <para type="synopsis">Main configuration properties in MFA System.</para>
    /// <para type="description">Represent Main configuration properties registered with MFA.</para>
    /// <para type="description">You can access, update each config property.</para>
    /// </summary>
    /// <example>
    ///   <para>$cfg = Get-MFAConfig</para>
    ///   <para>$cfg.UseOfUserLanguages = $true</para>
    ///   <para>Set-MFAConfig $cfg</para> 
    /// </example>
    public class PSConfig
    {
        /// <summary>
        /// <para type="description">Administrators email, used in administrative emails sent to users.</para>
        /// </summary>
        public string AdminContact { get; set; }

        /// <summary>
        /// <para type="description">Issuer description (eg "my company").</para>
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// <para type="description">Default contry code, used for SMS calls .</para>
        /// </summary>
        public string DefaultCountryCode { get; set; }

        /// <summary>
        /// <para type="description">Default provider when User method equals "Choose".</para>
        /// </summary>
        public PSPreferredMethod DefaultProviderMethod { get; set; }

        /// <summary>
        /// <para type="description">Policy attributes for users management and registration.</para>
        /// </summary>
        public PSUserFeaturesOptions UserFeatures { get; set; }

        /// <summary>
        /// <para type="description">Use or not our implementation for changing user password,if not we are using /ADFS/Portal/updatepasswor.</para>
        /// </summary>
        public bool CustomUpdatePassword { get; set; }

        /// <summary>
        /// <para type="description">Let user to change the default MFA provider for later use.</para>
        /// </summary>
        public bool KeepMySelectedOptionOn { get; set; }

        /// <summary>
        /// <para type="description">Send email notifications when a user update his configuration.</para>
        /// </summary>
        public bool ChangeNotificationsOn { get; set; }

        /// <summary>
        /// <para type="description">Use of User's browser laguages instead or standard localization features.</para>
        /// </summary>
        public bool UseOfUserLanguages { get; set; }

        /// <summary>
        /// <para type="description">Policy attributes for warnings to users.</para>
        /// </summary>
        public PSAdvertisingDays AdvertisingDays { get; set; }

        /// <summary>
        /// <para type="description">Kind of ADFS's User Interface version.</para>
        /// </summary>
        public PSUIKind UiKind { get; internal set; }

        /// <summary>
        /// <para type="description">Use ADFS 2019 paginated UI Styles.</para>
        /// </summary>
        public bool UseUIPaginated { get; internal set; }

        /// <summary>
        /// <para type="description">Options when using MFA as primary auth.</para>
        /// </summary>
        public PSPrimaryAuthOptions PrimaryAuhenticationOptions { get; set; } = PSPrimaryAuthOptions.None;

        /// <summary>
        /// <para type="description">Force adfsmfa to no Use localization but a specified language.</para>
        /// </summary>
        public string ForcedLanguage { get; set; }

        /// <summary>
        /// implicit conversion to PSConfig
        /// </summary>
        public static explicit operator PSConfig(FlatConfig config)
        {
            if (config == null)
                return null;
            else
            {
                PSConfig psconfig = new PSConfig
                {
                    AdminContact = config.AdminContact,
                    DefaultCountryCode = config.DefaultCountryCode,
                    Issuer = config.Issuer,
                    CustomUpdatePassword = config.CustomUpdatePassword,
                    KeepMySelectedOptionOn = config.KeepMySelectedOptionOn,
                    ChangeNotificationsOn = config.ChangeNotificationsOn,
                    UseOfUserLanguages = config.UseOfUserLanguages,
                    DefaultProviderMethod = (PSPreferredMethod)config.DefaultProviderMethod,
                    UserFeatures = (PSUserFeaturesOptions)config.UserFeatures,
                    AdvertisingDays = (PSAdvertisingDays)config.AdvertisingDays,
                    UseUIPaginated = config.UseUIPaginated,
                    UiKind = (PSUIKind)config.UiKind,
                    PrimaryAuhenticationOptions = (PSPrimaryAuthOptions)config.PrimaryAuhenticationOptions,
                    ForcedLanguage = config.ForcedLanguage
                };
                return psconfig;
            }
        }

        /// <summary>
        /// implicit conversion from PSConfig
        /// </summary>
        public static explicit operator FlatConfig(PSConfig psconfig)
        {
            if (psconfig == null)
                return null;
            else
            {
                FlatConfig config = new FlatConfig
                {
                    IsDirty = true,
                    AdminContact = psconfig.AdminContact,
                    CustomUpdatePassword = psconfig.CustomUpdatePassword,
                    DefaultCountryCode = psconfig.DefaultCountryCode,
                    Issuer = psconfig.Issuer,
                    KeepMySelectedOptionOn = psconfig.KeepMySelectedOptionOn,
                    ChangeNotificationsOn = psconfig.ChangeNotificationsOn,
                    UseOfUserLanguages = psconfig.UseOfUserLanguages,
                    DefaultProviderMethod = (PreferredMethod)psconfig.DefaultProviderMethod,
                    UserFeatures = (UserFeaturesOptions)psconfig.UserFeatures,
                    AdvertisingDays = (FlatAdvertising)psconfig.AdvertisingDays,
                    UseUIPaginated = psconfig.UseUIPaginated,
                    UiKind = (ADFSUserInterfaceKind)psconfig.UiKind,
                    PrimaryAuhenticationOptions = (PrimaryAuthOptions)psconfig.PrimaryAuhenticationOptions,
                    ForcedLanguage = psconfig.ForcedLanguage
                };
                return config;
            }
        }
    }
    #endregion

    #region PSAdvertisingDays
    /// <summary>
    /// PSAdvertisingDays class
    /// <para type="synopsis">Main configuration properties in MFA System.</para>
    /// <para type="description">Range of days during which users are invited to register.</para>
    /// </summary>
    /// <example>
    ///   <para>$cfg = Get-MFAConfig</para>
    ///   <para>$cfg.AdvertisingDays.FirstDay = 12</para>
    ///   <para>$cfg.AdvertisingDays.FirstDay = 25</para>
    ///   <para>Set-MFAConfig $cfg</para> 
    /// </example>
    public class PSAdvertisingDays
    {
        /// <summary>
        /// <para type="description">First Day for advertising.</para>
        /// </summary>
        public uint FirstDay { get; set; }

        /// <summary>
        /// <para type="description">Last Day for advertising.</para>
        /// </summary>
        public uint LastDay { get; set; }

        /// <summary>
        /// <para type="description">OnFire.</para>
        /// </summary>
        public bool OnFire { get; set; }

        /// <summary>
        /// <para type="description">explicit operator.</para>
        /// </summary>
        public static explicit operator PSAdvertisingDays(FlatAdvertising adv)
        {
            if (adv == null)
                return null;
            PSAdvertisingDays cfg = new PSAdvertisingDays
            {
                FirstDay = adv.FirstDay,
                LastDay = adv.LastDay,
                OnFire = adv.OnFire
            };
            return cfg;
        }

        /// <summary>
        /// <para type="description">explicit operator.</para>
        /// </summary>
        public static explicit operator FlatAdvertising(PSAdvertisingDays adv)
        {
            if (adv == null)
                return null;
            FlatAdvertising cfg = new FlatAdvertising
            {
                FirstDay = adv.FirstDay,
                LastDay = adv.LastDay,
                OnFire = adv.OnFire
            };
            return cfg;

        }

        /// <summary>
        /// <para type="description">explicit operator.</para>
        /// </summary>
        public static implicit operator PSAdvertisingDays(string advstr)
        {
            if (string.IsNullOrEmpty(advstr))
                return null;
            string rec = advstr.Replace("{", "").Replace("}", "");
            string[] data = rec.Split(';');
            List<KeyValuePair<string, UInt32>> pp = new List<KeyValuePair<string, uint>>();
            foreach (string xx in data)
            {
                string[] tp = xx.Split('=');
                if (tp.Length == 2)
                    pp.Add(new KeyValuePair<string, UInt32>(tp[0].ToString().Trim(), Convert.ToUInt32(tp[1])));
            }

            if ((pp.Count == 0))
                return null;
            PSAdvertisingDays days = new PSAdvertisingDays();
            foreach (KeyValuePair<string, UInt32> x in pp)
            {
                if (x.Key.ToLower().Equals("firstday"))
                    days.FirstDay = Convert.ToUInt32(x.Value);
                if (x.Key.ToLower().Equals("lastday"))
                    days.LastDay = Convert.ToUInt32(x.Value);
            }
            days.OnFire = true;
            return days;
        }

        /// <summary>
        /// <para type="description">explicit operator.</para>
        /// </summary>
        public static implicit operator PSAdvertisingDays(ScriptBlock advstr)
        {
            if (advstr==null)
                return null;
            string rec = advstr.ToString().Replace("{", "").Replace("}", "");
            string[] data = rec.Split(';');
            List<KeyValuePair<string, UInt32>> pp = new List<KeyValuePair<string, uint>>(); 
            foreach (string xx in data)
            {
                string[] tp = xx.Split('=');
                if (tp.Length==2)
                    pp.Add(new KeyValuePair<string, UInt32>(tp[0].ToString().Trim(), Convert.ToUInt32(tp[1])));                   
            }

            if ((pp.Count ==0))
                return null;
            PSAdvertisingDays days = new PSAdvertisingDays();
            foreach (KeyValuePair<string, UInt32> x in pp)
            {
                if (x.Key.ToLower().Equals("firstday"))
                    days.FirstDay = Convert.ToUInt32(x.Value);
                if (x.Key.ToLower().Equals("lastday"))
                    days.LastDay = Convert.ToUInt32(x.Value);
            }
            days.OnFire = true;
            return days;
        }

        /// <summary>
        /// <para type="description">ToString() override.</para>
        /// </summary>
        public override string ToString()
        {
            return "{FirstDay : " + this.FirstDay.ToString() + "; LastDay : " + this.LastDay.ToString() + "; OnFire : " + this.OnFire.ToString()+"}";
        }
    }
    #endregion

    #region PSBaseStore
    /// <summary>
    ///   <para type="synopsis">ADDS / SQL common class.</para>
    ///   <para type="description">ADDS / SQL base class.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAStore -Store ADDS</para>
    /// </example>
    public class PSBaseStore
    {
        /// <summary>
        /// <para type="description">If true, users metadata are stored in ADDS attributes or in SQL Database. see : Get-MFAStore -Store ADDS or Get-MFAStore -Store SQL.</para>
        /// </summary>
        public bool Active { get; set; }
    }
    #endregion

    #region PSADDSStore
    /// <summary>
    /// PSADDSStore class
    /// <para type="synopsis">ADDS configuration properties in MFA System.</para>
    /// <para type="description">ADDS configuration properties registered with MFA.You can access, update attributes properties.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAStore -Store ADDS</para>
    /// </example>
    public class PSADDSStore: PSBaseStore 
    {
        /// <summary>
        /// <para type="description">ADDS attribute name user to store user secret key (default msDS-cloudExtensionAttribute10).</para>
        /// </summary>
        public string KeyAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store user custom mail address (default msDS-cloudExtensionAttribute11).</para>
        /// </summary>
        public string MailAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store user phone number (default msDS-cloudExtensionAttribute12).</para>
        /// </summary>
        public string PhoneAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store user preferred method (Code, Phone, Mail) (default msDS-cloudExtensionAttribute13).</para>
        /// </summary>
        public string MethodAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store user preferred method (Code, Phone, Mail) (default msDS-cloudExtensionAttribute14).</para>
        /// </summary>
        public string OverrideMethodAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store user status with MFA (default msDS-cloudExtensionAttribute18).</para>
        /// </summary>
        public string EnabledAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store user pin code (default msDS-cloudExtensionAttribute15).</para>
        /// </summary>
        public string PinAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store multiple user Public Keys Credential (recommended msDS-KeyCredentialLink or othetMailbox).</para>
        /// </summary>
        public string PublicKeyCredentialAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store Client Certificate (default msDS-cloudExtensionAttribute16).</para>
        /// </summary>
        public string ClientCertificateAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS attribute name used to store RSA Certificate (default msDS-cloudExtensionAttribute17).</para>
        /// </summary>
        public string RSACertificateAttribute { get; set; }

        /// <summary>
        /// <para type="description">Get or Set the max rows limit used to access MFA Active Directory.</para>
        /// </summary>
        public int MaxRows { get; set; }

        /// <summary>
        /// <para type="description">Get or Set value indicating if ldap or ldaps must be used to access MFA Active Directory.</para>
        /// </summary>
        public bool UseSSL { get; set; }

        /// <summary>
        /// implicit conversion 
        /// </summary>
        public static explicit operator PSADDSStore(FlatADDSStore addsconfig)
        {
            if (addsconfig == null)
                return null;
            else
            {
                PSADDSStore psconfigadds = new PSADDSStore
                {
                    Active = addsconfig.Active,
                    KeyAttribute = addsconfig.KeyAttribute,
                    MailAttribute = addsconfig.MailAttribute,
                    PhoneAttribute = addsconfig.PhoneAttribute,
                    MethodAttribute = addsconfig.MethodAttribute,
                    OverrideMethodAttribute = addsconfig.OverrideMethodAttribute,
                    PinAttribute = addsconfig.PinAttribute,
                    EnabledAttribute = addsconfig.EnabledAttribute,
                    PublicKeyCredentialAttribute = addsconfig.PublicKeyCredentialAttribute,
                    ClientCertificateAttribute = addsconfig.ClientCertificateAttribute,
                    RSACertificateAttribute = addsconfig.RSACertificateAttribute,
                    MaxRows = addsconfig.MaxRows,
                    UseSSL = addsconfig.UseSSL
                };
                return psconfigadds;
            }
        }

        /// <summary>
        /// implicit conversion 
        /// </summary>
        public static explicit operator FlatADDSStore(PSADDSStore psconfig)
        {
            if (psconfig == null)
                return null;
            else
            {
                FlatADDSStore config = new FlatADDSStore
                {
                    IsDirty = true,
                    Active = psconfig.Active,
                    KeyAttribute = psconfig.KeyAttribute,
                    MailAttribute = psconfig.MailAttribute,
                    PhoneAttribute = psconfig.PhoneAttribute,
                    MethodAttribute = psconfig.MethodAttribute,
                    OverrideMethodAttribute = psconfig.OverrideMethodAttribute,
                    PinAttribute = psconfig.PinAttribute,
                    EnabledAttribute = psconfig.EnabledAttribute,
                    PublicKeyCredentialAttribute = psconfig.PublicKeyCredentialAttribute,
                    ClientCertificateAttribute = psconfig.ClientCertificateAttribute,
                    RSACertificateAttribute = psconfig.RSACertificateAttribute,
                    MaxRows = psconfig.MaxRows,
                    UseSSL = psconfig.UseSSL
            };
                return config;
            }
        }

        /// <summary>
        /// SetADDSAttributesTemplate method implementation
        /// </summary>
        public static void SetADDSAttributesTemplate(PSADDSTemplateKind kind)
        {
            ManagementService.SetADDSAttributesTemplate((ADDSTemplateKind)kind, true);
        }
    }
    #endregion

    #region PSSQLStore
    /// <summary>
    /// PSSQLStore class
    /// <para type="synopsis">SQL configuration properties in MFA System.</para>
    /// <para type="description">SQL configuration properties registered with MFA. You can access, update connectionString property.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAStore -Store SQL</para>
    /// </example>
    public class PSSQLStore : PSBaseStore
    {
        /// <summary>
        /// <para type="description">Get or Set the connection string used to access MFA SQL Database.</para>
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// <para type="description">Get or Set the max rows limit used to access MFA SQL Database.</para>
        /// </summary>
        public int MaxRows { get; set; }

        /// <summary>
        /// <para type="description">Get or Set the SQLServer 2016 and up Always Encrypted feature. default = false.</para>
        /// </summary>
        public bool IsAlwaysEncrypted { get; set; }

        /// <summary>
        /// <para type="description">Get or Set the validity of the generated certificate (in years, 5 per default).</para>
        /// </summary>
        public int CertificateValidity { get; set; }

        /// <summary>
        /// <para type="description">Get or Set the SQLServer 2016 and up Always Encrypted feature Thumprint.</para>
        /// </summary>
        public string ThumbPrint { get; set; }

        /// <summary>
        /// <para type="description">Get or Set the indicating if we are reusing an existing certificate.</para>
        /// </summary>
        public bool CertReuse { get; set; }

        /// <summary>
        /// <para type="description">Get or Set the SQLServer 2016 crypting key name.</para>
        /// </summary>
        public string KeyName { get; set; }


        /// <summary>
        /// implicit conversion to PSConfig
        /// </summary>
        public static explicit operator PSSQLStore(FlatSQLStore sqlconfig)
        {
            if (sqlconfig == null)
                return null;
            else
            {
                PSSQLStore psconfigsql = new PSSQLStore
                {
                    Active = sqlconfig.Active,
                    ConnectionString = sqlconfig.ConnectionString,
                    MaxRows = sqlconfig.MaxRows,
                    IsAlwaysEncrypted = sqlconfig.IsAlwaysEncrypted,
                    ThumbPrint = sqlconfig.ThumbPrint,
                    KeyName = sqlconfig.KeyName,
                    CertReuse = sqlconfig.CertReuse,
                    CertificateValidity = sqlconfig.CertificateValidity
                };
                return psconfigsql;
            }
        }

        /// <summary>
        /// implicit conversion from PSConfig
        /// </summary>
        public static explicit operator FlatSQLStore(PSSQLStore psconfig)
        {
            if (psconfig == null)
                return null;
            else
            {
                FlatSQLStore config = new FlatSQLStore
                {
                    IsDirty = true,
                    Active = psconfig.Active,
                    ConnectionString = psconfig.ConnectionString,
                    MaxRows = psconfig.MaxRows,
                    IsAlwaysEncrypted = psconfig.IsAlwaysEncrypted,
                    ThumbPrint = psconfig.ThumbPrint,
                    KeyName = psconfig.KeyName,
                    CertReuse = psconfig.CertReuse,
                    CertificateValidity = psconfig.CertificateValidity
                };
                return config;
            }
        }
    }
    #endregion

    #region PSCustomStore
    /// <summary>
    /// PSSQLStore class
    /// <para type="synopsis">SQL configuration properties in MFA System.</para>
    /// <para type="description">SQL configuration properties registered with MFA. You can access, update connectionString property.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAStore -Store SQL</para>
    /// </example>
    public class PSCustomStore : PSBaseStore
    {
        /// <summary>
        /// <para type="description">Get or Set the connection string used to access MFA SQL Database.</para>
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// <para type="description">Get or Set the max rows limit used to access MFA SQL Database.</para>
        /// </summary>
        public int MaxRows { get; set; }

               /// <summary>
        /// <para type="description">Full qualified implementation class for replacing default storage.</para>
        /// </summary>
        public string DataRepositoryFullyQualifiedImplementation { get; set; }

        /// <summary>
        /// <para type="description">Full qualified implementation class for replacing default storage for keys.</para>
        /// </summary>
        public string KeysRepositoryFullyQualifiedImplementation { get; set; }

        /// <summary>
        /// <para type="description">Full qualified implementation parameters for replacing default storage.</para>
        /// </summary>
        public string Parameters { get; set; }


        /// <summary>
        /// implicit conversion to PSConfig
        /// </summary>
        public static explicit operator PSCustomStore(FlatCustomStore sqlconfig)
        {
            if (sqlconfig == null)
                return null;
            else
            {
                PSCustomStore psconfigsql = new PSCustomStore
                {
                    Active = sqlconfig.Active,
                    ConnectionString = sqlconfig.ConnectionString,
                    MaxRows = sqlconfig.MaxRows,
                    DataRepositoryFullyQualifiedImplementation = sqlconfig.DataRepositoryFullyQualifiedImplementation,
                    KeysRepositoryFullyQualifiedImplementation = sqlconfig.KeysRepositoryFullyQualifiedImplementation,
                    Parameters = sqlconfig.Parameters
                };
                return psconfigsql;
            }
        }

        /// <summary>
        /// implicit conversion from PSConfig
        /// </summary>
        public static explicit operator FlatCustomStore(PSCustomStore psconfig)
        {
            if (psconfig == null)
                return null;
            else
            {
                FlatCustomStore config = new FlatCustomStore
                {
                    IsDirty = true,
                    Active = psconfig.Active,
                    ConnectionString = psconfig.ConnectionString,
                    MaxRows = psconfig.MaxRows,
                    DataRepositoryFullyQualifiedImplementation = psconfig.DataRepositoryFullyQualifiedImplementation,
                    KeysRepositoryFullyQualifiedImplementation = psconfig.KeysRepositoryFullyQualifiedImplementation,
                    Parameters = psconfig.Parameters
                };
                return config;
            }
        }
    }
    #endregion

    #region PSBaseSecurity
    /// <summary>
    /// PSBaseSecurity class implementation
    /// <para type="description">Base Security Class.</para>
    /// </summary>
    public abstract class PSBaseSecurity
    {
    }
    #endregion

    #region PSSecurity
    /// <summary>
    /// PSSecurity class
    /// <para type="synopsis">MFA Security Management.</para>
    /// <para type="description">MFA Security Management.</para>
    /// </summary>
    public class PSSecurity: PSBaseSecurity
    {
        /// <summary>
        /// <para type="description">Max delay for the user to enter TOTP code (seconds 300 by default).</para>
        /// </summary>
        public int DeliveryWindow { get; set; }

        /// <summary>
        /// <para type="description">Number of allowed retry allowed (default 3)..</para>
        /// </summary>
        public int MaxRetries { get; set; }

        /// <summary>
        /// <para type="description">TOTP Replay Level feature.</para>
        /// </summary>
        public PSReplayLevel ReplayLevel { get; set; }

        /// <summary>
        /// <para type="description">Used to Change the version of the encryption Library. V1 is the initial implementation but is less secure, you must migrate to V2</para>
        /// </summary>
        public PSSecretKeyVersion LibVersion { get; set; }

        /// <summary>
        /// <para type="description">String used for XOR operations in V2 LibVersion</para>
        /// </summary>
        public string XORSecret { get; set; }

        /// <summary>
        /// <para type="description">Required PIN length wehen using aditionnal control with personal PIN.</para>
        /// </summary>
        public int PinLength { get; set; }

        /// <summary>
        /// <para type="description">Default value for user's PIN.</para>
        /// </summary>
        public int DefaultPin { get; set; }

        /// <summary>
        /// <para type="description">Domain name to access (read/write) Active Directory (if ADFS account has rights you leave it blank).</para>
        /// </summary>
        public string DomainAddress { get; set; }

        /// <summary>
        /// <para type="description">Account name to access (read/write) Active Directory (if ADFS account has rights you leave it blank).</para>
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// <para type="description">Password used for account to access (read/write) Active Directory (if ADFS account has rights you leave it blank).</para>
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator PSSecurity(FlatSecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                PSSecurity target = new PSSecurity
                {
                    DeliveryWindow = mgr.DeliveryWindow,
                    MaxRetries = mgr.MaxRetries,
                    ReplayLevel = (PSReplayLevel)mgr.ReplayLevel,
                    LibVersion = (PSSecretKeyVersion)mgr.LibVersion,
                    XORSecret = mgr.XORSecret,
                    PinLength = mgr.PinLength,
                    DefaultPin = mgr.DefaultPin,
                    DomainAddress = mgr.DomainAddress,
                    Account = mgr.Account,
                    Password = mgr.Password,
                };
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator FlatSecurity(PSSecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                FlatSecurity target = new FlatSecurity
                {
                    IsDirty = true,
                    DeliveryWindow = mgr.DeliveryWindow,
                    MaxRetries = mgr.MaxRetries,
                    ReplayLevel = (ReplayLevel)mgr.ReplayLevel,
                    LibVersion = (SecretKeyVersion)mgr.LibVersion,
                    XORSecret = mgr.XORSecret,
                    PinLength = mgr.PinLength,
                    DefaultPin = mgr.DefaultPin,
                    DomainAddress = mgr.DomainAddress,
                    Account = mgr.Account,
                    Password = mgr.Password
                };
                return target;
            }
        }
    }
    #endregion

    #region PSRNGSecurity
    /// <summary>
    /// PSRNGSecurity class
    /// <para type="synopsis">MFA Security Management for RNG Keys.</para>
    /// <para type="description">MFA Security Management for RNG Keys.</para>
    /// </summary>
    public class PSRNGSecurity : PSBaseSecurity
    {
        /// <summary>
        /// <para type="description">Used when RNG is selected, for choosing the size of the generated random number (128 to 512 bytes).</para> 
        /// </summary>
        public PSKeyGeneratorMode KeyGenerator { get; set; }

        /// <summary>
        /// explicit operator from PSRNGSecurity
        /// </summary>
        public static explicit operator PSRNGSecurity(FlatRNGSecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                PSRNGSecurity target = new PSRNGSecurity
                {
                    KeyGenerator = (PSKeyGeneratorMode)mgr.KeyGenerator
                };
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator FlatRNGSecurity(PSRNGSecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                FlatRNGSecurity target = new FlatRNGSecurity
                {
                    IsDirty = true,
                    KeyGenerator = (KeyGeneratorMode)mgr.KeyGenerator
                };
                return target;
            }
        }
    }
    #endregion

    #region PSRNGSecurity
    /// <summary>
    /// PSCustomSecurity class
    /// <para type="synopsis">MFA Security Management for Custom Keys.</para>
    /// <para type="description">MFA Security Management for Custom Keys.</para>
    /// </summary>
    public class PSCustomSecurity : PSBaseSecurity
    {
        /// <summary>
        /// <para type="description">Fully Qualified Type for custom security implementation.</para> 
        /// </summary>
        public string CustomFullyQualifiedImplementation { get; set; }

        /// <summary>
        /// <para type="description">Custom parameters for custom security implementation.</para> 
        /// </summary>
        public string CustomParameters { get; set; }

        /// <summary>
        /// explicit operator from PSRNGSecurity
        /// </summary>
        public static explicit operator PSCustomSecurity(FlatCustomSecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                PSCustomSecurity target = new PSCustomSecurity
                {
                    CustomFullyQualifiedImplementation = mgr.CustomFullyQualifiedImplementation,
                    CustomParameters = mgr.CustomParameters
                };
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator FlatCustomSecurity(PSCustomSecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                FlatCustomSecurity target = new FlatCustomSecurity
                {
                    IsDirty = true,
                    CustomFullyQualifiedImplementation = mgr.CustomFullyQualifiedImplementation,
                    CustomParameters = mgr.CustomParameters
                };
                return target;
            }
        }
    }
    #endregion

    #region PSAESSecurity
    /// <summary>
    /// PSAESSecurity class
    /// <para type="synopsis">MFA Security Management for EAS Keys.</para>
    /// <para type="description">MFA Security Management for EAS Keys.</para>
    /// </summary>
    public class PSAESSecurity : PSBaseSecurity
    {
        /// <summary>
        /// <para type="description">Used when RNG is selected, for choosing the size of the generated random number (128 to 512 bytes).</para> 
        /// </summary>
        public PSAESKeyGeneratorMode AESKeyGenerator { get; set; }

        /// <summary>
        /// explicit operator from PSRNGSecurity
        /// </summary>
        public static explicit operator PSAESSecurity(FlatAESSecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                PSAESSecurity target = new PSAESSecurity
                {
                    AESKeyGenerator = (PSAESKeyGeneratorMode)mgr.AESKeyGenerator
                };
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator FlatAESSecurity(PSAESSecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                FlatAESSecurity target = new FlatAESSecurity
                {
                    IsDirty = true,
                    AESKeyGenerator = (AESKeyGeneratorMode)mgr.AESKeyGenerator
                };
                return target;
            }
        }
    }
    #endregion

    #region PSRSASecurity
    /// <summary>
    /// PSRSASecurity class
    /// <para type="synopsis">MFA Security Management.</para>
    /// <para type="description">MFA Security Management.</para>
    /// </summary>
    public class PSRSASecurity : PSBaseSecurity
    {
        /// <summary>
        /// <para type="description">Certificate validity duration in Years (5 by default)</para> 
        /// </summary>
        public int CertificateValidity { get; set; }

        /// <summary>
        /// <para type="description">Use a distinct certificate for each user when using KeyFormat==RSA. each certificate is deployed on ADDS or SQL Database</para> 
        /// </summary>
        public bool CertificatePerUser { get; set; }

        /// <summary>
        /// <para type="description">Certificate Thumbprint when using KeyFormat==RSA. the certificate is deployed on all ADFS servers in Crypting Certificates store</para> 
        /// </summary>
        public string CertificateThumbprint { get; set; }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator PSRSASecurity(FlatRSASecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                PSRSASecurity target = new PSRSASecurity
                {
                    CertificateValidity = mgr.CertificateValidity,
                    CertificatePerUser = mgr.CertificatePerUser,
                    CertificateThumbprint = mgr.CertificateThumbprint
                };
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator FlatRSASecurity(PSRSASecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                FlatRSASecurity target = new FlatRSASecurity
                {
                    IsDirty = true,
                    CertificateValidity = mgr.CertificateValidity,
                    CertificatePerUser = mgr.CertificatePerUser,
                    CertificateThumbprint = mgr.CertificateThumbprint
                };
                return target;
            }
        }
    }
    #endregion

    #region PSBiometricSecurity
    /// <summary>
    /// PSBiometricSecurity class
    /// <para type="synopsis">MFA Security Management.</para>
    /// <para type="description">MFA Security Management.</para>
    /// </summary>
    public class PSBiometricSecurity : PSBaseSecurity
    {       
        /// <summary>
        /// <para type="description">Authenticator Attachment property (empty, Platform, Crossplatform).</para>
        /// </summary>
        public PSAuthenticatorAttachmentKind AuthenticatorAttachment { get; set; }

        /// <summary>
        /// <para type="description">Attestation Conveyance Preference property (None, Direct, Indirect).</para>
        /// </summary>
        public PSAttestationConveyancePreferenceKind AttestationConveyancePreference { get; set; }

        /// <summary>
        /// <para type="description">User Verification Requirement property (Preferred, Required, Discouraged).</para>
        /// </summary>
        public PSUserVerificationRequirementKind UserVerificationRequirement { get; set; }

        /// <summary>
        /// <para type="description">Extensions property (boolean) supports extensions ?.</para>
        /// </summary>
        public bool Extensions { get; set; }

        /// <summary>
        /// <para type="description">User Verification Index property (boolean).</para>
        /// </summary>
        public bool UserVerificationIndex { get; set; }

        /// <summary>
        /// <para type="description">Location property (boolean).</para>
        /// </summary>
        public bool Location { get; set; }

        /// <summary>
        /// <para type="description">User Verification Method property (boolean).</para>
        /// </summary>
        public bool UserVerificationMethod { get; set; }

        /// <summary>
        /// <para type="description">Require Resident Key property (boolean).</para>
        /// </summary>
        public bool RequireResidentKey { get; set; }

        /// <summary>
        /// <para type="description">Use HMAC encryption.</para>
        /// </summary>
        public bool? HmacSecret { get; set; }

        /// <summary>
        /// <para type="description">Use credential protection.</para>
        /// </summary>
        public WebAuthNUserVerification? CredProtect { get; set; }

        /// <summary>
        /// <para type="description">Force credential protection.</para>
        /// </summary>
        public bool? EnforceCredProtect { get; set; }


        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator PSBiometricSecurity(FlatBiometricSecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                PSBiometricSecurity target = new PSBiometricSecurity
                {
                    AuthenticatorAttachment = (PSAuthenticatorAttachmentKind)mgr.AuthenticatorAttachment,
                    AttestationConveyancePreference = (PSAttestationConveyancePreferenceKind)mgr.AttestationConveyancePreference,
                    UserVerificationRequirement = (PSUserVerificationRequirementKind)mgr.UserVerificationRequirement,
                    Extensions = mgr.Extensions,
                    UserVerificationIndex = mgr.UserVerificationIndex,
                    Location = mgr.Location,
                    UserVerificationMethod = mgr.UserVerificationMethod,
                    RequireResidentKey = mgr.RequireResidentKey,
                    HmacSecret = mgr.HmacSecret,
                    CredProtect = mgr.CredProtect,
                    EnforceCredProtect = mgr.EnforceCredProtect
                };
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator FlatBiometricSecurity(PSBiometricSecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                FlatBiometricSecurity target = new FlatBiometricSecurity
                {
                    IsDirty = true,
                    AuthenticatorAttachment = (FlatAuthenticatorAttachmentKind)mgr.AuthenticatorAttachment,
                    AttestationConveyancePreference = (FlatAttestationConveyancePreferenceKind)mgr.AttestationConveyancePreference,
                    UserVerificationRequirement = (FlatUserVerificationRequirementKind)mgr.UserVerificationRequirement,
                    Extensions = mgr.Extensions,
                    UserVerificationIndex = mgr.UserVerificationIndex,
                    Location = mgr.Location,
                    UserVerificationMethod = mgr.UserVerificationMethod,
                    RequireResidentKey = mgr.RequireResidentKey,
                    HmacSecret = mgr.HmacSecret,
                    CredProtect = mgr.CredProtect,
                    EnforceCredProtect = mgr.EnforceCredProtect
                };
                return target;
            }
        }
    }
    #endregion

    #region PSWsManSecurity
    /// <summary>
    /// PSWsManSecurity class
    /// <para type="synopsis">MFA Security Management.</para>
    /// <para type="description">MFA Security Management.</para>
    /// </summary>
    public class PSWsManSecurity : PSBaseSecurity
    {
        /// <summary>
        /// <para type="description">WsMan port used (5985(http), 5986(https)).</para>
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// <para type="description">WsMan Application name used (default (wsman)).</para>
        /// </summary>        
        public string AppName { get; set; }

        /// <summary>
        /// <para type="description">WsMan Shell Uri.</para>
        /// </summary>        
        public string ShellUri { get; set; }

        /// <summary>
        /// <para type="description">WsMan Connection TimeOut (5000 ms).</para>
        /// </summary>        
        public int TimeOut { get; set; }

        /// <summary>
        /// explicit operator from WsMan
        /// </summary>
        public static explicit operator PSWsManSecurity(FlatWsManSecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                PSWsManSecurity target = new PSWsManSecurity
                {
                    Port = mgr.Port,
                    AppName = mgr.AppName,
                    ShellUri = mgr.ShellUri,
                    TimeOut = mgr.TimeOut
                };
                return target;
            }
        }

        /// <summary>
        /// explicit operator from WsMan
        /// </summary>
        public static explicit operator FlatWsManSecurity(PSWsManSecurity mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                FlatWsManSecurity target = new FlatWsManSecurity
                {
                    IsDirty = true,
                    Port = mgr.Port,
                    AppName = mgr.AppName,
                    ShellUri = mgr.ShellUri,
                    TimeOut = mgr.TimeOut
                };
                return target;
            }
        }
    }
    #endregion

    #region PSBaseProvider
    /// <summary>
    /// <para type="synopsis">configuration properties in MFA System.</para>
    /// <para type="description">base class for different configuration properties in MFA System.</para>
    /// </summary>
    public abstract class PSBaseProvider
    {
        /// <summary>
        /// <para type="description">Provider Enabled property.</para>
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// <para type="description">Provider Enrollment Wizard Enabled property.</para>
        /// </summary>
        public bool EnrollWizard { get; set; }

        /// <summary>
        /// <para type="description">Provider Force Wizard if user dosen't complete during signing.</para>
        /// </summary>
        public PSForceWizardMode ForceWizard { get; set; }

        /// <summary>
        /// <para type="description">Set if additionnal verification with PIN (locally administered) must be done.</para>
        /// </summary>
        public bool PinRequired { get; set; }

        /// <summary>
        /// <para type="description">Force users to use this provider (mandatory for registration or not).</para>
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// <para type="description">Full qualified implementation class for replacing default provider.</para>
        /// </summary>
        public string FullQualifiedImplementation { get; set; }

        /// <summary>
        /// <para type="description">Full qualified implementation parameters for replacing default provider.</para>
        /// </summary>
        public string Parameters { get; set; }
    }
    #endregion

    #region PSTOTPProvider
    /// <summary>
    /// PSTOTPProvider class
    /// <para type="synopsis">Parameters for TOTP Provider.</para>
    /// <para type="description">provided for TOTP MFA.</para>
    /// <para type="description">Typically this component is used with authenticator applications, Notification and more.</para>
    /// </summary>
    public class PSTOTPProvider: PSBaseProvider
    {
        /// <summary>
        /// <para type="description">TOTP Provider Shadow codes. 2 by default</para>
        /// </summary>
        public int TOTPShadows { get; set; }

        /// <summary>
        /// <para type="description">TOTP Provider Hash algorithm. SHA1 by default</para>
        /// </summary>
        public PSHashMode Algorithm { get; set; }

        /// <summary>
        /// <para type="description">TOTP Provider Code len between 4 and 8. 6 by default</para>
        /// </summary>
        public int Digits { get; set; }

        /// <summary>
        /// <para type="description">TOTP Provider Code renew duration in seconds. 30s1 by default</para>
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// <para type="description">Set TOP Wizard Application list enabled/ disabled.</para>
        /// </summary>
        public PSOTPWizardOptions WizardOptions { get; set; }

        /// <summary>
        /// <para type="description">Type of generated Keys for users (RNG, RSA, CUSTOM RSA).</para>
        /// <para type="description">Changing the key format, invalidate all the users secret keys previously used.</para>
        /// <para type="description">RSA and RSA Custom are using Certificates. Custom RSA must Use Specific database to the keys and certs, one for each user, see New-MFASecretKeysDatabase cmdlet.</para>
        /// </summary>
        public PSSecretKeyFormat KeysFormat { get; set; }

        /// <summary>
        /// <para type="description">Used to trim the key at a fixed size, when you use RSA the key is very long, and QRCode is often too big for TOTP Application (1024 is a good size, even if RSA key is 2048 bytes long).</para>
        /// </summary>
        public PSKeySizeMode KeySize { get; set; }

        /// <summary>
        /// explicit operator from PSTOTPProvider
        /// </summary>
        public static explicit operator PSTOTPProvider(FlatTOTPProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                PSTOTPProvider target = new PSTOTPProvider
                {
                    Enabled = otp.Enabled,
                    IsRequired = otp.IsRequired,
                    EnrollWizard = otp.EnrollWizard,
                    ForceWizard = (PSForceWizardMode)otp.ForceWizard,
                    TOTPShadows = otp.TOTPShadows,
                    Algorithm = (MFA.PSHashMode)otp.Algorithm,
                    Digits = otp.Digits,
                    Duration = otp.Duration,
                    PinRequired = otp.PinRequired,
                    WizardOptions = (PSOTPWizardOptions)otp.WizardOptions,
                    FullQualifiedImplementation = otp.FullyQualifiedImplementation,
                    KeySize = (PSKeySizeMode)otp.KeySize,
                    KeysFormat = (PSSecretKeyFormat)otp.KeysFormat,
                    Parameters = otp.Parameters
                };
                return target;
            }
        }

        /// <summary>
        /// explicit operator from FlatOTPProvider
        /// </summary>
        public static explicit operator FlatTOTPProvider(PSTOTPProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                FlatTOTPProvider target = new FlatTOTPProvider
                {
                    IsDirty = true,
                    Enabled = otp.Enabled,
                    IsRequired = otp.IsRequired,
                    EnrollWizard = otp.EnrollWizard,
                    ForceWizard = (ForceWizardMode)otp.ForceWizard,
                    TOTPShadows = otp.TOTPShadows,
                    Algorithm = (HashMode)otp.Algorithm,
                    Digits = otp.Digits,
                    Duration = otp.Duration,
                    PinRequired = otp.PinRequired,
                    WizardOptions = (OTPWizardOptions)otp.WizardOptions,
                    KeySize = (KeySizeMode)otp.KeySize,
                    KeysFormat = (SecretKeyFormat)otp.KeysFormat,
                    FullyQualifiedImplementation = otp.FullQualifiedImplementation,
                    Parameters = otp.Parameters
                };
                return target;
            }
        }
    }
    #endregion

    #region PSMailProvider
    /// <summary>
    /// PSMailProvider class
    /// <para type="synopsis">SMTP configuration properties in MFA System.</para>
    /// <para type="description">SMTP/POP configuration properties registered with MFA.</para>
    /// <para type="description">You can access, update attributes properties.</para>
    /// </summary>
    /// <example>
    ///   <para>Get-MFAProvider -ProviderType Email</para>
    /// </example>
    public class PSMailProvider: PSBaseProvider
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
        /// <para type="description">indicate if connetion is Anonymous.</para>
        /// </summary>
        public bool Anonymous { get; set; }

        /// <summary>
        /// <para type="description">if your want Delivery Failures, Delayed delivery or nothing (default).</para>
        /// </summary>
        public bool DeliveryNotifications { get; set; }

        /// <summary>
        /// <para type="description">List of domains that are only allowed.</para>
        /// </summary>
        public PSMailAllowedDomains AllowedDomains { get; internal set; }

        /// <summary>
        /// <para type="description">List of domains that are not allowed.</para>
        /// </summary>
        public PSMailBlockedDomains BlockedDomains { get; internal set; }

        /// <summary>
        /// <para type="description">Custom mail templates.</para>
        /// </summary>
        public PSMailFileNames MailOTP { get; internal set; }

        /// <summary>
        /// <para type="description">Custom mail templates.</para>
        /// </summary>
        public PSMailFileNames MailInscription { get; internal set; }

        /// <summary>
        /// <para type="description">Custom mail templates.</para>
        /// </summary>
        public PSMailFileNames MailSecureKey { get; internal set; }

        /// <summary>
        /// <para type="description">Custom mail templates.</para>
        /// </summary>
        public PSMailFileNames MailNotifications { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PSMailProvider()
        {
            this.BlockedDomains = new PSMailBlockedDomains();
            this.AllowedDomains = new PSMailAllowedDomains();
            this.MailOTP = new PSMailFileNames();
            this.MailInscription = new PSMailFileNames();
            this.MailSecureKey = new PSMailFileNames();
            this.MailNotifications = new PSMailFileNames();
        }

        /// <summary>
        /// explicit conversion to PSConfigMail
        /// </summary>
        public static explicit operator PSMailProvider(FlatMailProvider mails)
        {
            if (mails == null)
                return null;
            else
            {
                PSMailProvider psconfig = new PSMailProvider
                {
                    Enabled = mails.Enabled,
                    IsRequired = mails.IsRequired,
                    EnrollWizard = mails.EnrollWizard,
                    ForceWizard = (PSForceWizardMode)mails.ForceWizard,
                    From = mails.From,
                    UserName = mails.UserName,
                    Password = mails.Password,
                    Host = mails.Host,
                    Port = mails.Port,
                    UseSSL = mails.UseSSL,
                    Company = mails.Company,
                    PinRequired = mails.PinRequired,
                    Anonymous = mails.Anonymous,
                    DeliveryNotifications = mails.DeliveryNotifications,
                    FullQualifiedImplementation = mails.FullyQualifiedImplementation,
                    Parameters = mails.Parameters
                };

                psconfig.AllowedDomains.Domains.Clear();
                foreach (string itm in mails.AllowedDomains.Domains)
                {
                    psconfig.AllowedDomains.AddDomain(itm);
                }

                psconfig.BlockedDomains.Domains.Clear();
                foreach (string itm in mails.BlockedDomains.Domains)
                {
                    psconfig.BlockedDomains.AddDomain(itm);
                }

                psconfig.MailOTP.Templates.Clear();
                foreach (FlatMailFileName itm in mails.MailOTPContent)
                {
                    psconfig.MailOTP.Templates.Add((PSMailFileName)itm);
                }
                psconfig.MailInscription.Templates.Clear();
                foreach (FlatMailFileName itm in mails.MailAdminContent)
                {
                    psconfig.MailInscription.Templates.Add((PSMailFileName)itm);
                }
                psconfig.MailSecureKey.Templates.Clear();
                foreach (FlatMailFileName itm in mails.MailKeyContent)
                {
                    psconfig.MailSecureKey.Templates.Add((PSMailFileName)itm);
                }
                psconfig.MailNotifications.Templates.Clear();
                foreach (FlatMailFileName itm in mails.MailNotifications)
                {
                    psconfig.MailNotifications.Templates.Add((PSMailFileName)itm);
                }
                return psconfig;
            }
        }

        /// <summary>
        /// explicit conversion from PSConfigMail
        /// </summary>
        public static explicit operator FlatMailProvider(PSMailProvider mails)
        {
            if (mails == null)
                return null;
            else
            {
                FlatMailProvider psconfig = new FlatMailProvider
                {
                    IsDirty = true,
                    Enabled = mails.Enabled,
                    IsRequired = mails.IsRequired,
                    EnrollWizard = mails.EnrollWizard,
                    ForceWizard = (ForceWizardMode)mails.ForceWizard,
                    From = mails.From,
                    UserName = mails.UserName,
                    Password = mails.Password,
                    Host = mails.Host,
                    Port = mails.Port,
                    UseSSL = mails.UseSSL,
                    Company = mails.Company,
                    PinRequired = mails.PinRequired,
                    Anonymous = mails.Anonymous,
                    DeliveryNotifications = mails.DeliveryNotifications,
                    FullyQualifiedImplementation = mails.FullQualifiedImplementation,
                    Parameters = mails.Parameters
                };

                psconfig.AllowedDomains.Clear();
                foreach (string itm in mails.AllowedDomains.Domains)
                {
                    psconfig.AllowedDomains.AddDomain(itm);
                }

                psconfig.BlockedDomains.Clear();
                foreach (string itm in mails.BlockedDomains.Domains)
                {
                    psconfig.BlockedDomains.AddDomain(itm);
                }

                psconfig.MailOTPContent.Clear();
                foreach (PSMailFileName itm in mails.MailOTP.Templates)
                {
                    psconfig.MailOTPContent.Add((FlatMailFileName)itm);
                }
                psconfig.MailAdminContent.Clear();
                foreach (PSMailFileName itm in mails.MailInscription.Templates)
                {
                    psconfig.MailAdminContent.Add((FlatMailFileName)itm);
                }
                psconfig.MailKeyContent.Clear();
                foreach (PSMailFileName itm in mails.MailSecureKey.Templates)
                {
                    psconfig.MailKeyContent.Add((FlatMailFileName)itm);
                }
                psconfig.MailNotifications.Clear();
                foreach (PSMailFileName itm in mails.MailNotifications.Templates)
                {
                    psconfig.MailNotifications.Add((FlatMailFileName)itm);
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
    public class PSMailFileName
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
        public PSMailFileName(int lcid, string filename, bool enabled = true)
        {
            this.LCID = lcid;
            this.FileName = filename;
            this.Enabled = enabled;
        }

        /// <summary>
        /// explicit operator 
        /// </summary>
        public static explicit operator PSMailFileName(FlatMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new PSMailFileName(file.LCID, file.FileName, file.Enabled);
        }

        /// <summary>
        /// explicit operator 
        /// </summary>
        public static explicit operator FlatMailFileName(PSMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new FlatMailFileName(file.LCID, file.FileName, file.Enabled);
        }
    }

    /// <summary>
    /// PSMailFileName class
    /// <para type="synopsis">Mail custom templates collection used in MFA System.</para>
    /// <para type="description">Mail custom templates collection registered with MFA.</para>
    /// </summary>
    public class PSMailFileNames
    {
        private List<PSMailFileName> _list = new List<PSMailFileName>();

        /// <summary>
        /// <para type="description">Templates property.</para>
        /// </summary>
        public List<PSMailFileName> Templates
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
                PSMailFileName item = (from it in _list where it.LCID == lcid select it).FirstOrDefault();
                if (item!=null)
                    throw new Exception("Template already exists !");
                _list.Add(new PSMailFileName(lcid, filename, enabled));
            }
            catch (Exception ex )
            {
                throw new Exception("Error Adding Template !", ex);
            }
        }

        /// <summary>
        /// SetTemplate method implmentation
        /// </summary>
        public void SetTemplate(int lcid, string filename, bool enabled = true)
        {
            try
            {
                PSMailFileName item = (from it in _list where it.LCID == lcid select it).First();
                int i = _list.IndexOf(item);
                item.FileName = filename;
                item.Enabled = enabled;
                _list[i] = item;
            }
            catch (Exception ex)
            {
                throw new Exception("Template dosen't exists !", ex);
            }
        }

        /// <summary>
        /// RemoveTemplate method implmentation
        /// </summary>
        public void RemoveTemplate(int lcid)
        {
            try
            {
                PSMailFileName item = (from it in _list where it.LCID == lcid select it).First();
                int i = _list.IndexOf(item);
                _list.RemoveAt(i);
            }
            catch (Exception ex)
            {
                throw new Exception("Template dosen't exists !", ex);
            }
        }

        /// <summary>
        /// ToString method override
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _list.Count; i++)
            {
                PSMailFileName item = _list[i];
                sb.Append(string.Format("{0} {1} File : {2}", item.Enabled, item.LCID, item.FileName));
                if (i < _list.Count-1)
                    sb.AppendLine();
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// PSMailBlockedDomains class
    /// <para type="synopsis">Mail blocked domains collection used in MFA System.</para>
    /// <para type="description">Mail blocked domains collection registered with MFA.</para>
    /// </summary>
    public class PSMailBlockedDomains
    {
        private List<string> _list = new List<string>();

        /// <summary>
        /// <para type="description">BlockedDomains property.</para>
        /// </summary>
        public List<string> Domains
        {
            get
            {
                return _list;
            }
        }

        /// <summary>
        /// AddDomain method implmentation
        /// </summary>
        public void AddDomain(string domainname)
        {
            try
            {
                if (_list.Contains(domainname.ToLower()))
                    throw new Exception("Domain is already int blocked list !");
                _list.Add(domainname.ToLower());
            }
            catch (Exception ex)
            {
                throw new Exception("Error Adding blocked domain !", ex);
            }
        }

        /// <summary>
        /// RemoveDomain method implmentation
        /// </summary>
        public void RemoveDomain(string domainname)
        {
            try
            {
                if (!_list.Contains(domainname.ToLower()))
                    throw new Exception("Domain not found int blocked list !");
                int i = _list.IndexOf(domainname.ToLower());
                _list.RemoveAt(i);
            }
            catch (Exception ex)
            {
                throw new Exception("Error removing blocked domain !", ex);
            }
        }

        /// <summary>
        /// ToString method override
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _list.Count; i++)
            {
                string item = _list[i];
                sb.Append(item);
                if (i < _list.Count - 1)
                    sb.AppendLine();
            }
            return sb.ToString();
        }

    }

    /// <summary>
    /// PSMailAllowedDomains class
    /// <para type="synopsis">Mail allowed domains only collection used in MFA System.</para>
    /// <para type="description">Mail allowed domains only collection registered with MFA.</para>
    /// </summary>
    public class PSMailAllowedDomains
    {
        private List<string> _list = new List<string>();

        /// <summary>
        /// <para type="description">AllowedDomains property.</para>
        /// </summary>
        public List<string> Domains
        {
            get
            {
                return _list;
            }
        }

        /// <summary>
        /// AddDomain method implmentation
        /// </summary>
        public void AddDomain(string domainname)
        {
            try
            {
                if (_list.Contains(domainname.ToLower()))
                    throw new Exception("Domain is already int blocked list !");
                _list.Add(domainname.ToLower());
            }
            catch (Exception ex)
            {
                throw new Exception("Error Adding blocked domain !", ex);
            }
        }

        /// <summary>
        /// RemoveDomain method implmentation
        /// </summary>
        public void RemoveDomain(string domainname)
        {
            try
            {
                if (!_list.Contains(domainname.ToLower()))
                    throw new Exception("Domain not found int blocked list !");
                int i = _list.IndexOf(domainname.ToLower());
                _list.RemoveAt(i);
            }
            catch (Exception ex)
            {
                throw new Exception("Error removing blocked domain !", ex);
            }
        }

        /// <summary>
        /// ToString method override
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _list.Count; i++)
            {
                string item = _list[i];
                sb.Append(item);
                if (i < _list.Count - 1)
                    sb.AppendLine();
            }
            return sb.ToString();
        }
    }
    #endregion

    #region PSExternalProvider
    /// <summary>
    /// PSExternalProvider class
    /// <para type="synopsis">Specify External OTP Provider, you must implement IExternalOTPProvider interface.</para>
    /// <para type="description">Samples are provided for Azure and custom.</para>
    /// <para type="description">Typically this component is used when sending SMS, you can use your own SMS gateway.</para>
    /// </summary>
    public class PSExternalProvider: PSBaseProvider
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
        public static explicit operator PSExternalProvider(FlatExternalProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                PSExternalProvider target = new PSExternalProvider
                {
                    Enabled = otp.Enabled,
                    IsRequired = otp.IsRequired,
                    EnrollWizard = otp.EnrollWizard,
                    ForceWizard = (MFA.PSForceWizardMode)otp.ForceWizard,
                    Company = otp.Company,
                    FullQualifiedImplementation = otp.FullyQualifiedImplementation,
                    IsTwoWay = otp.IsTwoWay,
                    Sha1Salt = otp.Sha1Salt,
                    Timeout = otp.Timeout,
                    PinRequired = otp.PinRequired,
                    Parameters = otp.Parameters
                };
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator FlatExternalProvider(PSExternalProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                FlatExternalProvider target = new FlatExternalProvider
                {
                    IsDirty = true,
                    Enabled = otp.Enabled,
                    IsRequired = otp.IsRequired,
                    EnrollWizard = otp.EnrollWizard,
                    ForceWizard = (ForceWizardMode)otp.ForceWizard,
                    Company = otp.Company,
                    FullyQualifiedImplementation = otp.FullQualifiedImplementation,
                    IsTwoWay = otp.IsTwoWay,
                    Sha1Salt = otp.Sha1Salt,
                    Timeout = otp.Timeout,
                    PinRequired = otp.PinRequired,
                    Parameters = otp.Parameters
                };
                return target;
            }
        }
    }
    #endregion

    #region PSAzureProvider
    /// <summary>
    /// PSAzureProvider class
    /// <para type="synopsis">Parameters for Azure MFA Provider.</para>
    /// <para type="description">provided for Azure MFA.</para>
    /// <para type="description">Typically this component is used when sending SMS, Notification and more.</para>
    /// <para type="description">Note : everthing is managed by Microsoft MFA Remotely.</para>
    /// </summary>
    public class PSAzureProvider: PSBaseProvider
    {
        /// <summary>
        /// <para type="description">your Azure/o365 tenantId / tenant name.</para>
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// <para type="description">Thumbprint of the Azure cetificate (Done when configuring Azure MFA.</para>
        /// </summary>
        public string Thumbprint { get; set; }

        /// <summary>
        /// explicit operator from PSConfigAzureProvider
        /// </summary>
        public static explicit operator PSAzureProvider(FlatAzureProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                PSAzureProvider target = new PSAzureProvider
                {
                    TenantId = otp.TenantId,
                    Thumbprint = otp.ThumbPrint,
                    Enabled = otp.Enabled,
                    IsRequired = otp.IsRequired,
                    EnrollWizard = false,
                    ForceWizard = MFA.PSForceWizardMode.Disabled,
                    PinRequired = otp.PinRequired,
                    FullQualifiedImplementation = otp.FullyQualifiedImplementation,
                    Parameters = otp.Parameters
                };
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator FlatAzureProvider(PSAzureProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                FlatAzureProvider target = new FlatAzureProvider
                {
                    IsDirty = true,
                    TenantId = otp.TenantId,
                    ThumbPrint = otp.Thumbprint,
                    Enabled = otp.Enabled,
                    IsRequired = otp.IsRequired,
                    EnrollWizard = false,
                    ForceWizard = ForceWizardMode.Disabled,
                    PinRequired = otp.PinRequired,
                    FullyQualifiedImplementation = otp.FullQualifiedImplementation,
                    Parameters = otp.Parameters
                };
                return target;
            }
        }
    }
    #endregion

    #region PSBiometricProvider
    /// <summary>
    /// PSBiometricProvider class
    /// <para type="synopsis">Parameters for Biometric MFA Provider.</para>
    /// <para type="description">provided Biometric MFA.</para>
    /// <para type="description">Typically this component is used when using fingerprint or face recognition.</para>
    /// </summary>
    public class PSBiometricProvider : PSBaseProvider
    {
        /// <summary>
        /// <para type="description">When Biometrics is default method, authentication directly called a first time</para> 3
        /// </summary>
        public bool DirectLogin { get; set; }

        /// <summary>
        /// <para type="description">Timeout property (in milliseconds).</para> 
        /// </summary>
        public uint Timeout { get; set; }

        /// <summary>
        /// <para type="description">Timestamp Drift Tolerance property (in milliseconds).</para> 
        /// </summary>
        public int TimestampDriftTolerance { get; set; }

        /// <summary>
        /// <para type="description">Challenge Size property (16, 32, 48, 64 bytes) (128, 256, 384, 512 bits).</para>
        /// </summary>
        public int ChallengeSize { get; set; } 

        /// <summary>
        /// <para type="description">Server Domain property.</para> 
        /// </summary>
        public string ServerDomain { get; set; }

        /// <summary>
        /// <para type="description">Server Name property.</para> 
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// <para type="description">Server Icon property (url).</para> 
        /// </summary>
        public string ServerIcon { get; set; } 

        /// <summary>
        /// <para type="description">Server Uri property (url).</para>
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// <para type="description">Require Certificate chain validation.</para>
        /// </summary>
        public bool RequireValidAttestationRoot { get; set; }

        /// <summary>
        /// <para type="description">Show Sensitive informations for internal validation.</para>
        /// </summary>
        public bool ShowPII { get; set; }

        /// <summary>
        /// <para type="description">Pin requirements for unverified biometrics devices.</para>
        /// </summary>
        public WebAuthNPinRequirements PinRequirements { get; set; }

        /// <summary>
        /// explicit operator from PSConfigBiometricProvider
        /// </summary>
        public static explicit operator PSBiometricProvider(FlatBiometricProvider otp)
    {
            if (otp == null)
                return null;
            else
            {
                PSBiometricProvider target = new PSBiometricProvider
                {
                    Enabled = otp.Enabled,
                    IsRequired = otp.IsRequired,
                    EnrollWizard = otp.EnrollWizard,
                    ForceWizard = (PSForceWizardMode)otp.ForceWizard,
                    PinRequired = otp.PinRequired,
                    PinRequirements = otp.PinRequirements,
                    DirectLogin = otp.DirectLogin,
                    FullQualifiedImplementation = otp.FullyQualifiedImplementation,
                    Parameters = otp.Parameters,

                    Timeout = otp.Timeout,
                    TimestampDriftTolerance = otp.TimestampDriftTolerance,
                    ChallengeSize = otp.ChallengeSize,
                    ServerDomain = otp.ServerDomain,
                    ServerName = otp.ServerName,
                    ServerIcon = otp.ServerIcon,
                    Origin = otp.Origin,
                    RequireValidAttestationRoot = otp.RequireValidAttestationRoot,
                    ShowPII = otp.ShowPII
            };
            return target;
        }
    }

    /// <summary>
    /// explicit operator for FlatBiometricProvider
    /// </summary>
    public static explicit operator FlatBiometricProvider(PSBiometricProvider otp)
    {
        if (otp == null)
            return null;
        else
        {
            FlatBiometricProvider target = new FlatBiometricProvider
            {
                IsDirty = true,
                Enabled = otp.Enabled,
                IsRequired = otp.IsRequired,
                EnrollWizard = otp.EnrollWizard,
                ForceWizard = (ForceWizardMode)otp.ForceWizard,
                PinRequired = otp.PinRequired,
                DirectLogin = otp.DirectLogin,
                FullyQualifiedImplementation = otp.FullQualifiedImplementation,
                Parameters = otp.Parameters,

                Timeout = otp.Timeout,
                TimestampDriftTolerance = otp.TimestampDriftTolerance,
                ChallengeSize = otp.ChallengeSize,
                ServerDomain = otp.ServerDomain,
                ServerName = otp.ServerName,
                ServerIcon = otp.ServerIcon,
                Origin = otp.Origin,
                RequireValidAttestationRoot = otp.RequireValidAttestationRoot,
                ShowPII = otp.ShowPII
            };
            return target;
        }
    }
}
#endregion
}