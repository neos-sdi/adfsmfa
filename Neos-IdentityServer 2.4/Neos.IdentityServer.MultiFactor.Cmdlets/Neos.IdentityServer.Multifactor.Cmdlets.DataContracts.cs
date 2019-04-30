using Neos.IdentityServer.MultiFactor.Cmdlets;
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
namespace MFA
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Xml;
    using Neos.IdentityServer.MultiFactor;
    using Neos.IdentityServer.MultiFactor.Administration;

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
        public static explicit operator PSRegistrationList(RegistrationList registrations)
        {
            if (registrations == null)
                return null;
            PSRegistrationList lst = new PSRegistrationList();
            foreach (Registration reg in registrations)
            {
                lst.Add((PSRegistration)reg);
            }
            return lst;
        }

        /// <summary>
        /// explicit conversion from PSRegistrationList
        /// </summary>
        public static explicit operator RegistrationList(PSRegistrationList registrations)
        {
            if (registrations == null)
                return null;
            RegistrationList lst = new RegistrationList();
            foreach (PSRegistration reg in registrations)
            {
                lst.Add((Registration)reg);
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
        public static explicit operator PSRegistration(Registration registration)
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
                psnode.IsRegistered = registration.IsRegistered;
                psnode.Enabled = registration.Enabled;
                psnode.PreferredMethod = (PSPreferredMethod)registration.PreferredMethod;
                psnode.OverrideMethod = registration.OverrideMethod;
                psnode.PIN = registration.PIN;
                psnode.IsApplied = registration.IsApplied;
                return psnode;
            }
        }

        /// <summary>
        /// implicit conversion from PSRegistration
        /// </summary>
        public static explicit operator Registration(PSRegistration psnode)
        {
            if (psnode == null)
                return null;
            else
            {
                Registration registration = new Registration();
                registration.ID = psnode.ID;
                registration.UPN = psnode.UPN;
                registration.MailAddress = psnode.MailAddress;
                registration.PhoneNumber = psnode.PhoneNumber;
                registration.IsRegistered = psnode.IsRegistered;
                registration.Enabled = psnode.Enabled;
                registration.PreferredMethod = (PreferredMethod)psnode.PreferredMethod;
                registration.OverrideMethod = psnode.OverrideMethod;
                registration.PIN = psnode.PIN;
                registration.IsApplied = psnode.IsApplied;
                return registration;
            }
        }
    }
    #endregion

    #region PSConfig
    /// <summary>
    /// PSConfig class
    /// <para type="synopsis">Main configuration properties in MFA System.</para>
    /// <para type="description">Represent Main configuration properties registered with MFA.</para>
    /// <para type="description">You can access, update each config property.</para>
    /// </summary>
    /// <example>
    ///   <para>$cfg = Get-MFAConfig</para>
    ///   <para>$cfg.UseActiveDirectory = $true</para>
    ///   <para>Set-MFAConfig $cfg</para> 
    /// </example>
    public class PSConfig
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
        /// <para type="description">Required PIN length wehen using aditionnal control with personal PIN.</para>
        /// </summary>
        public int PinLength { get; set; }

        /// <summary>
        /// <para type="description">Default value for user's PIN.</para>
        /// </summary>
        public int DefaultPin { get; set; }

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
        /// <para type="description">Let user to change the default MFA provider for later use.</para>
        /// </summary>
        public bool KeepMySelectedOptionOn { get; set; }

        /// <summary>
        /// <para type="description">Send email notifications when a user update his configuration.</para>
        /// </summary>
        public bool ChangeNotificationsOn { get; set; }

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
        public PSUserFeaturesOptions UserFeatures { get; set; }

        /// <summary>
        /// <para type="description">Kind of ADFS's User Interface version.</para>
        /// </summary>
        public PSUIKind UiKind { get; set; }

        /// <summary>
        /// <para type="description">Use ADFS 2019 paginated UI Styles.</para>
        /// </summary>
        public bool UseUIPaginated { get; set; }

        /// <summary>
        /// <para type="description">Policy attributes for warnings to users.</para>
        /// </summary>
        public ConfigAdvertising AdvertisingDays { get; set; }

        /// <summary>
        /// implicit conversion to PSConfig
        /// </summary>
        public static explicit operator PSConfig(FlatConfig config)
        {
            if (config == null)
                return null;
            else
            {
                PSConfig psconfig = new PSConfig();
                psconfig.AdminContact = config.AdminContact;
                psconfig.DefaultCountryCode = config.DefaultCountryCode;
                psconfig.DeliveryWindow = config.DeliveryWindow;
                psconfig.MaxRetries = config.MaxRetries;
                psconfig.Issuer = config.Issuer;
                psconfig.DefaultPin = config.DefaultPin;
                psconfig.PinLength = config.PinLength;
                psconfig.UseActiveDirectory = config.UseActiveDirectory;
                psconfig.CustomUpdatePassword = config.CustomUpdatePassword;
                psconfig.KeepMySelectedOptionOn = config.KeepMySelectedOptionOn;
                psconfig.ChangeNotificationsOn = config.ChangeNotificationsOn;
                psconfig.UserFeatures = (PSUserFeaturesOptions)config.UserFeatures;
                psconfig.AdvertisingDays = config.AdvertisingDays;
                psconfig.UseUIPaginated = config.UseUIPaginated;
                psconfig.UiKind = (PSUIKind)config.UiKind;
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
                FlatConfig config = new FlatConfig();
                config.AdminContact = psconfig.AdminContact;
                config.CustomUpdatePassword = psconfig.CustomUpdatePassword;
                config.DefaultCountryCode = psconfig.DefaultCountryCode;
                config.DeliveryWindow = psconfig.DeliveryWindow;
                config.MaxRetries = psconfig.MaxRetries;
                config.IsDirty = true;
                config.Issuer = psconfig.Issuer;
                config.DefaultPin = psconfig.DefaultPin;
                config.PinLength = psconfig.PinLength;
                config.UseActiveDirectory = psconfig.UseActiveDirectory;
                config.KeepMySelectedOptionOn = psconfig.KeepMySelectedOptionOn;
                config.ChangeNotificationsOn = psconfig.ChangeNotificationsOn;
                config.UserFeatures = (UserFeaturesOptions)psconfig.UserFeatures;
                config.AdvertisingDays = psconfig.AdvertisingDays;
                config.UseUIPaginated = psconfig.UseUIPaginated;
                config.UiKind = (ADFSUserInterfaceKind)psconfig.UiKind;
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
        public static explicit operator PSConfigSQL(FlatConfigSQL sqlconfig)
        {
            if (sqlconfig == null)
                return null;
            else
            {
                PSConfigSQL psconfigsql = new PSConfigSQL();
                psconfigsql.ConnectionString = sqlconfig.ConnectionString;
                psconfigsql.MaxRows = sqlconfig.MaxRows;
                psconfigsql.IsAlwaysEncrypted = sqlconfig.IsAlwaysEncrypted;
                psconfigsql.ThumbPrint = sqlconfig.ThumbPrint;
                psconfigsql.KeyName = sqlconfig.KeyName;
                psconfigsql.CertReuse = sqlconfig.CertReuse;
                psconfigsql.CertificateValidity = sqlconfig.CertificateValidity;
                return psconfigsql;
            }
        }

        /// <summary>
        /// implicit conversion from PSConfig
        /// </summary>
        public static explicit operator FlatConfigSQL(PSConfigSQL psconfig)
        {
            if (psconfig == null)
                return null;
            else
            {
                FlatConfigSQL config = new FlatConfigSQL();
                config.IsDirty = true;
                config.ConnectionString = psconfig.ConnectionString;
                config.MaxRows = psconfig.MaxRows;
                config.IsAlwaysEncrypted = psconfig.IsAlwaysEncrypted;
                config.ThumbPrint = psconfig.ThumbPrint;
                config.KeyName = psconfig.KeyName;
                config.CertReuse = psconfig.CertReuse;
                config.CertificateValidity = psconfig.CertificateValidity;
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
        /// <para type="description">Password used for account to access (read/write) Active Directory (if ADFS account has rights you leave it blank).</para>
        /// </summary>
        public string Password { get; set; }

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
        /// <para type="description">ADDS atribute name used to store user phone number (default msDS-cloudExtensionAttribute12).</para>
        /// </summary>
        public string PhoneAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS atribute name used to store user preferred method (Code, Phone, Mail) (default msDS-cloudExtensionAttribute13).</para>
        /// </summary>
        public string MethodAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS atribute name used to store user preferred method (Code, Phone, Mail) (default msDS-cloudExtensionAttribute13).</para>
        /// </summary>
        internal string OverrideMethodAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS atribute name used to store user status with MFA (default msDS-cloudExtensionAttribute18).</para>
        /// </summary>
        public string EnabledAttribute { get; set; }

        /// <summary>
        /// <para type="description">ADDS atribute name used to store user pin code (default msDS-cloudExtensionAttribute15).</para>
        /// </summary>
        public string PinAttribute { get; set; }

        /// <summary>
        /// <para type="description">Get or Set the max rows limit used to access MFA Active Directory.</para>
        /// </summary>
        public int MaxRows { get; set; }

        /// <summary>
        /// implicit conversion to PSConfig
        /// </summary>
        public static explicit operator PSConfigADDS(FlatConfigADDS addsconfig)
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
                psconfigadds.PhoneAttribute = addsconfig.PhoneAttribute;
                psconfigadds.MethodAttribute = addsconfig.MethodAttribute;
                psconfigadds.OverrideMethodAttribute = addsconfig.OverrideMethodAttribute;
                psconfigadds.PinAttribute = addsconfig.PinAttribute;
                psconfigadds.EnabledAttribute = addsconfig.EnabledAttribute;
                psconfigadds.MaxRows = addsconfig.MaxRows;
                return psconfigadds;
            }
        }

        /// <summary>
        /// implicit conversion from PSConfig
        /// </summary>
        public static explicit operator FlatConfigADDS(PSConfigADDS psconfig)
        {
            if (psconfig == null)
                return null;
            else
            {
                FlatConfigADDS config = new FlatConfigADDS();
                config.IsDirty = true;
                config.Account = psconfig.Account;
                config.Password = psconfig.Password;
                config.DomainAddress = psconfig.DomainAddress;
                config.KeyAttribute = psconfig.KeyAttribute;
                config.MailAttribute = psconfig.MailAttribute;
                config.PhoneAttribute = psconfig.PhoneAttribute;
                config.MethodAttribute = psconfig.MethodAttribute;
                config.OverrideMethodAttribute = psconfig.OverrideMethodAttribute;
                config.PinAttribute = psconfig.PinAttribute;
                config.EnabledAttribute = psconfig.EnabledAttribute;
                config.MaxRows = psconfig.MaxRows;
                return config;
            }
        }
    }
    #endregion

    #region PSConfigKeys
    /// <summary>
    /// PSKeysConfig class
    /// <para type="synopsis">Secret key Management used in MFA System.</para>
    /// <para type="description">Secret key Management registered with MFA.</para>
    /// </summary>
    public class PSConfigKeys
    {
        /// <summary>
        /// <para type="description">Used when RNG is selected, for choosing the size of the generated random number (128 to 512 bytes).</para>
        /// </summary>
        public PSKeyGeneratorMode KeyGenerator { get; set; }

        /// <summary>
        /// <para type="description">Used to trim the key at a fixed size, when you use RSA the key is very long, and QRCode is often too big for TOTP Application (1024 is a good size, even if RSA key is 2048 bytes long).</para>
        /// </summary>
        public PSKeySizeMode KeySize { get; set; }

        /// <summary>
        /// <para type="description">Type of generated Keys for users (RNG, RSA, CUSTOM RSA).</para>
        /// <para type="description">Changing the key format, invalidate all the users secret keys previously used.</para>
        /// <para type="description">RSA and RSA Custom are using Certificates. Custom RSA must Use Specific database to the keys and certs, one for each user, see New-MFASecretKeysDatabase cmdlet.</para>
        /// </summary>
        public PSSecretKeyFormat KeysFormat { get; set; }

        /// <summary>
        /// <para type="description">Used to Change the version of the encryption Library. V1 is the initial implementation but is less secure, you must migrate to V2</para>
        /// </summary>
        public PSSecretKeyVersion LibVersion { get; set; }

        /// <summary>
        /// <para type="description">String used for XOR operations in V2 LibVersion</para>
        /// </summary>
        public string XORSecret { get; set; } 

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
        public PSConfigKeys()
        {
            this.ExternalKeyManager = new PSExternalKeyManager();
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator PSConfigKeys(FlatKeysConfig mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                PSConfigKeys target = new PSConfigKeys();
                target.CertificateThumbprint = mgr.CertificateThumbprint;
                target.CertificateValidity = mgr.CertificateValidity;
                target.KeysFormat = (PSSecretKeyFormat)mgr.KeysFormat;
                target.KeyGenerator = (PSKeyGeneratorMode)mgr.KeyGenerator;
                target.LibVersion = (PSSecretKeyVersion)mgr.LibVersion;
                target.KeySize = (PSKeySizeMode)mgr.KeySize;
                target.ExternalKeyManager = (PSExternalKeyManager)mgr.ExternalKeyManager;
                target.XORSecret = mgr.XORSecret;
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator FlatKeysConfig(PSConfigKeys mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                FlatKeysConfig target = new FlatKeysConfig();
                target.IsDirty = true;
                target.CertificateThumbprint = mgr.CertificateThumbprint;
                target.CertificateValidity = mgr.CertificateValidity;
                target.KeysFormat = (SecretKeyFormat)mgr.KeysFormat;
                target.KeyGenerator = (KeyGeneratorMode)mgr.KeyGenerator;
                target.LibVersion = (SecretKeyVersion)mgr.LibVersion;
                target.KeySize = (KeySizeMode)mgr.KeySize;
                target.ExternalKeyManager = (FlatExternalKeyManager)mgr.ExternalKeyManager;
                target.XORSecret = mgr.XORSecret;
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
        /// <para type="description">Connection string to SQL Database, see sample implementation of Neos.IdentityServer.Multifactor.Keys.CustomKeyManager</para>
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// <para type="description">Encrypted, information for KeyManager about encrypting his data</para>
        /// </summary>
        public bool IsAlwaysEncrypted { get; set; }

        /// <summary>
        /// <para type="description">ThumbPrint of encryption certificate, see sample implementation of Neos.IdentityServer.Multifactor.Keys.CustomKeyManager</para>
        /// </summary>
        public string ThumbPrint { get; set; }

        /// <summary>
        /// <para type="description">Name of the SQL Server encryption key (default adfsmfa)</para>
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// <para type="description">ThumbPrint of encryption certificate, see sample implementation of Neos.IdentityServer.Multifactor.Keys.CustomKeyManager</para>
        /// </summary>
        public int CertificateValidity { get; set; }

        /// <summary>
        /// <para type="description">if you want to reuse the same certificate ThumbPrint of encryption certificate. </para>
        /// </summary>
        public bool CertReuse { get; set; }

        /// <summary>
        /// <para type="description">Specify your own parameters, values stored as CData, set it as string with Parameters = "myparameters"</para>
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// explicit operator 
        /// </summary>
        public static explicit operator PSExternalKeyManager(FlatExternalKeyManager mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                PSExternalKeyManager ret = new PSExternalKeyManager();
                ret.FullQualifiedImplementation = mgr.FullQualifiedImplementation;
                ret.ConnectionString = mgr.ConnectionString;
                ret.IsAlwaysEncrypted = mgr.IsAlwaysEncrypted;
                ret.KeyName = mgr.KeyName;
                ret.CertificateValidity = mgr.CertificateValidity;
                ret.CertReuse = mgr.CertReuse;
                ret.ThumbPrint = mgr.ThumbPrint;
                ret.Parameters = mgr.Parameters;

                return ret;
            }
        }

        /// <summary>
        /// explicit operator 
        /// </summary>
        public static explicit operator FlatExternalKeyManager(PSExternalKeyManager mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                FlatExternalKeyManager ret = new FlatExternalKeyManager();
                ret.FullQualifiedImplementation = mgr.FullQualifiedImplementation;
                ret.ConnectionString = mgr.ConnectionString;
                ret.IsAlwaysEncrypted = mgr.IsAlwaysEncrypted;
                ret.KeyName = mgr.KeyName;
                ret.CertificateValidity = mgr.CertificateValidity;
                ret.CertReuse = mgr.CertReuse;
                ret.ThumbPrint = mgr.ThumbPrint;
                ret.Parameters = mgr.Parameters;
                return ret;
            }
        }
    }
    #endregion

    #region PSConfigBaseProvider
    /// <summary>
    /// PSConfigBaseProvider class
    /// <para type="synopsis">configuration properties in MFA System.</para>
    /// </summary>
    public abstract class PSConfigBaseProvider
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
        /// <para type="description">Full qualified implementation class for replacing default provider.</para>
        /// </summary>
        public string FullQualifiedImplementation { get; set; }

        /// <summary>
        /// <para type="description">Full qualified implementation parameters for replacing default provider.</para>
        /// </summary>
        public string Parameters { get; set; }
    }
    #endregion

    #region PSConfigTOTPProvider
    /// <summary>
    /// PSConfigTOTPProvider class
    /// <para type="synopsis">Parameters for TOTP Provider.</para>
    /// <para type="description">provided for TOTP MFA.</para>
    /// <para type="description">Typically this component is used with authenticator applications, Notification and more.</para>
    /// </summary>
    public class PSConfigTOTPProvider: PSConfigBaseProvider
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
        /// <para type="description">Set TOP Wizard Application list enabled/ disabled.</para>
        /// </summary>
        public PSOTPWizardOptions WizardOptions { get; set; }

        /// <summary>
        /// explicit operator from PSConfigTOTPProvider
        /// </summary>
        public static explicit operator PSConfigTOTPProvider(FlatOTPProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                PSConfigTOTPProvider target = new PSConfigTOTPProvider();
                target.Enabled = otp.Enabled;
                target.EnrollWizard = otp.EnrollWizard;
                target.ForceWizard = (PSForceWizardMode)otp.ForceWizard;
                target.TOTPShadows = otp.TOTPShadows;
                target.Algorithm = (MFA.PSHashMode)otp.Algorithm; 
                target.PinRequired = otp.PinRequired;
                target.WizardOptions = (PSOTPWizardOptions)otp.WizardOptions;
                target.FullQualifiedImplementation = otp.FullQualifiedImplementation;
                target.Parameters = otp.Parameters;
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator FlatOTPProvider(PSConfigTOTPProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                FlatOTPProvider target = new FlatOTPProvider();
                target.IsDirty = true;
                target.Enabled = otp.Enabled;
                target.EnrollWizard = otp.EnrollWizard;
                target.ForceWizard = (ForceWizardMode)otp.ForceWizard;
                target.TOTPShadows = otp.TOTPShadows;
                target.Algorithm = (HashMode)otp.Algorithm;
                target.PinRequired = otp.PinRequired;
                target.WizardOptions = (OTPWizardOptions)otp.WizardOptions;
                target.FullQualifiedImplementation = otp.FullQualifiedImplementation;
                target.Parameters = otp.Parameters;
                return target;
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
    public class PSConfigMailProvider: PSConfigBaseProvider
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
        /// <para type="description">List of domains that are not allowed.</para>
        /// </summary>
        public PSConfigMailBlockedDomains BlockedDomains { get; set; }

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
        /// <para type="description">Custom mail templates.</para>
        /// </summary>
        public PSConfigMailFileNames MailNotifications { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public PSConfigMailProvider()
        {
            this.BlockedDomains = new PSConfigMailBlockedDomains();
            this.MailOTP = new PSConfigMailFileNames();
            this.MailInscription = new PSConfigMailFileNames();
            this.MailSecureKey = new PSConfigMailFileNames();
            this.MailNotifications = new PSConfigMailFileNames();
        }

        /// <summary>
        /// explicit conversion to PSConfigMail
        /// </summary>
        public static explicit operator PSConfigMailProvider(FlatConfigMail mails)
        {
            if (mails == null)
                return null;
            else
            {
                PSConfigMailProvider psconfig = new PSConfigMailProvider();
                psconfig.Enabled = mails.Enabled;
                psconfig.EnrollWizard =  mails.EnrollWizard;
                psconfig.ForceWizard = (PSForceWizardMode)mails.ForceWizard;
                psconfig.From = mails.From;
                psconfig.UserName = mails.UserName;
                psconfig.Password = mails.Password;
                psconfig.Host = mails.Host;
                psconfig.Port = mails.Port;
                psconfig.UseSSL = mails.UseSSL;
                psconfig.Company = mails.Company;
                psconfig.PinRequired = mails.PinRequired;
                psconfig.Anonymous = mails.Anonymous;
                psconfig.FullQualifiedImplementation = mails.FullQualifiedImplementation;
                psconfig.Parameters = mails.Parameters;

                psconfig.BlockedDomains.Domains.Clear();
                foreach (string itm in mails.BlockedDomains.Domains)
                {
                    psconfig.BlockedDomains.AddDomain(itm);
                }

                psconfig.MailOTP.Templates.Clear();
                foreach (FlatConfigMailFileName itm in mails.MailOTPContent)
                {
                    psconfig.MailOTP.Templates.Add((PSConfigMailFileName)itm);
                }
                psconfig.MailInscription.Templates.Clear();
                foreach (FlatConfigMailFileName itm in mails.MailAdminContent)
                {
                    psconfig.MailInscription.Templates.Add((PSConfigMailFileName)itm);
                }
                psconfig.MailSecureKey.Templates.Clear();
                foreach (FlatConfigMailFileName itm in mails.MailKeyContent)
                {
                    psconfig.MailSecureKey.Templates.Add((PSConfigMailFileName)itm);
                }
                psconfig.MailNotifications.Templates.Clear();
                foreach (FlatConfigMailFileName itm in mails.MailNotifications)
                {
                    psconfig.MailNotifications.Templates.Add((PSConfigMailFileName)itm);
                }
                return psconfig;
            }
        }

        /// <summary>
        /// explicit conversion from PSConfigMail
        /// </summary>
        public static explicit operator FlatConfigMail(PSConfigMailProvider mails)
        {
            if (mails == null)
                return null;
            else
            {
                FlatConfigMail psconfig = new FlatConfigMail();
                psconfig.IsDirty = true;
                psconfig.Enabled = mails.Enabled;
                psconfig.EnrollWizard = mails.EnrollWizard;
                psconfig.ForceWizard = (ForceWizardMode)mails.ForceWizard;
                psconfig.From = mails.From;
                psconfig.UserName = mails.UserName;
                psconfig.Password = mails.Password;
                psconfig.Host = mails.Host;
                psconfig.Port = mails.Port;
                psconfig.UseSSL = mails.UseSSL;
                psconfig.Company = mails.Company;
                psconfig.PinRequired = mails.PinRequired;
                psconfig.Anonymous = mails.Anonymous;
                psconfig.FullQualifiedImplementation = mails.FullQualifiedImplementation;
                psconfig.Parameters = mails.Parameters;

                psconfig.BlockedDomains.Clear();
                foreach (string itm in mails.BlockedDomains.Domains)
                {
                    psconfig.BlockedDomains.AddDomain(itm);
                }

                psconfig.MailOTPContent.Clear();
                foreach (PSConfigMailFileName itm in mails.MailOTP.Templates)
                {
                    psconfig.MailOTPContent.Add((FlatConfigMailFileName)itm);
                }
                psconfig.MailAdminContent.Clear();
                foreach (PSConfigMailFileName itm in mails.MailInscription.Templates)
                {
                    psconfig.MailAdminContent.Add((FlatConfigMailFileName)itm);
                }
                psconfig.MailKeyContent.Clear();
                foreach (PSConfigMailFileName itm in mails.MailSecureKey.Templates)
                {
                    psconfig.MailKeyContent.Add((FlatConfigMailFileName)itm);
                }
                psconfig.MailNotifications.Clear();
                foreach (PSConfigMailFileName itm in mails.MailNotifications.Templates)
                {
                    psconfig.MailNotifications.Add((FlatConfigMailFileName)itm);
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
        public static explicit operator PSConfigMailFileName(FlatConfigMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new PSConfigMailFileName(file.LCID, file.FileName, file.Enabled);
        }

        /// <summary>
        /// explicit operator 
        /// </summary>
        public static explicit operator FlatConfigMailFileName(PSConfigMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new FlatConfigMailFileName(file.LCID, file.FileName, file.Enabled);
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
                PSConfigMailFileName item = (from it in _list where it.LCID == lcid select it).FirstOrDefault();
                if (item!=null)
                    throw new Exception("Template already exists !");
                _list.Add(new PSConfigMailFileName(lcid, filename, enabled));
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
                PSConfigMailFileName item = (from it in _list where it.LCID == lcid select it).First();
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

    /// <summary>
    /// PSConfigMailBlockedDomains class
    /// <para type="synopsis">Mail blocked domains collection used in MFA System.</para>
    /// <para type="description">Mail blocked domains collection registered with MFA.</para>
    /// </summary>
    public class PSConfigMailBlockedDomains
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
    }

    #endregion

    #region PSConfigExternalProvider
    /// <summary>
    /// PSConfigExternalProvider class
    /// <para type="synopsis">Specify External OTP Provider, you must implement IExternalOTPProvider interface.</para>
    /// <para type="description">Samples are provided for Azure and custom.</para>
    /// <para type="description">Typically this component is used when sending SMS, you can use your own SMS gateway.</para>
    /// </summary>
    public class PSConfigExternalProvider: PSConfigBaseProvider
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
        public static explicit operator PSConfigExternalProvider(FlatExternalProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                PSConfigExternalProvider target = new PSConfigExternalProvider();
                target.Enabled = otp.Enabled;
                target.EnrollWizard = otp.EnrollWizard;
                target.ForceWizard = (MFA.PSForceWizardMode)otp.ForceWizard;
                target.Company = otp.Company;
                target.FullQualifiedImplementation = otp.FullQualifiedImplementation;
                target.IsTwoWay = otp.IsTwoWay;
                target.Sha1Salt = otp.Sha1Salt;
                target.Timeout = otp.Timeout;
                target.PinRequired = otp.PinRequired;
                target.Parameters = otp.Parameters;
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator FlatExternalProvider(PSConfigExternalProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                FlatExternalProvider target = new FlatExternalProvider();
                target.IsDirty = true;
                target.Enabled = otp.Enabled;
                target.EnrollWizard = otp.EnrollWizard;
                target.ForceWizard = (ForceWizardMode)otp.ForceWizard;
                target.Company = otp.Company;
                target.FullQualifiedImplementation = otp.FullQualifiedImplementation;
                target.IsTwoWay = otp.IsTwoWay;
                target.Sha1Salt = otp.Sha1Salt;
                target.Timeout = otp.Timeout;
                target.PinRequired = otp.PinRequired;
                target.Parameters = otp.Parameters;
                return target;
            }
        }
    }
    #endregion

    #region PSConfigAzureProvider
    /// <summary>
    /// PSConfigAzureProvider class
    /// <para type="synopsis">Parameters for Azure MFA Provider.</para>
    /// <para type="description">provided for Azure MFA.</para>
    /// <para type="description">Typically this component is used when sending SMS, Notification and more.</para>
    /// <para type="description">Note : everthing is managed by Microsoft MFA Remotely.</para>
    /// </summary>
    public class PSConfigAzureProvider: PSConfigBaseProvider
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
        public static explicit operator PSConfigAzureProvider(FlatAzureProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                PSConfigAzureProvider target = new PSConfigAzureProvider();
                target.TenantId = otp.TenantId;
                target.Thumbprint = otp.ThumbPrint;
                target.Enabled = otp.Enabled;
                target.EnrollWizard = false;
                target.ForceWizard = MFA.PSForceWizardMode.Disabled;
                target.PinRequired = otp.PinRequired;
                target.FullQualifiedImplementation = otp.FullQualifiedImplementation;
                target.Parameters = otp.Parameters;
                return target;
            }
        }

        /// <summary>
        /// explicit operator from MMCKeysConfig
        /// </summary>
        public static explicit operator FlatAzureProvider(PSConfigAzureProvider otp)
        {
            if (otp == null)
                return null;
            else
            {
                FlatAzureProvider target = new FlatAzureProvider();
                target.IsDirty = true;
                target.TenantId = otp.TenantId;
                target.ThumbPrint = otp.Thumbprint;
                target.Enabled = otp.Enabled;
                target.EnrollWizard = false;
                target.ForceWizard = ForceWizardMode.Disabled;
                target.PinRequired = otp.PinRequired;
                target.FullQualifiedImplementation = otp.FullQualifiedImplementation;
                target.Parameters = otp.Parameters;
                return target;
            }
        }
    }
    #endregion
}