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
        public PreferredMethod PreferredMethod { get; set; }

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
                psnode.PreferredMethod = registration.PreferredMethod;
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
                registration.PreferredMethod = psnode.PreferredMethod;
                registration.OverrideMethod = psnode.OverrideMethod;
                registration.PIN = psnode.PIN;
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
    ///   <para>$cfg.UseActiveDirectory = $true</para>
    ///   <para>Set-MFAConfig $cfg</para> 
    /// </example>
    public class PSConfig
    {
        /// <summary>
        /// <para type="description">Max delay for the user to enter TOTP code (seconds 300 by default).</para>
        /// </summary>
        public int DeliveryWindow { get; set; }

        /*
        /// <summary>
        /// <para type="description">Number of prior TOTP codes allowed (default 2). Code change every 30 seconds.</para>
        /// </summary>
        public int TOTPShadows { get; set; }
        */

        /// <summary>
        /// <para type="description">Required PIN length wehen using aditionnal control with personal PIN.</para>
        /// </summary>
        public int PinLength { get; set; }

        /*
        /// <summary>
        /// <para type="description">Globally allow MFA with sending email to users, less secure than TOTP Code.</para>
        /// <para type="description">Must specify properties of ConfigMail.</para>
        /// </summary>
        public bool MailEnabled { get; set; }

        /// <summary>
        /// <para type="description">Globally allow MFA with external Code Provider, sending SMS to users, less secure than TOTP Code.</para>
        /// <para type="description">Must define ExternalProvider (see SMS Azure and samples).</para>
        /// </summary>
        public bool SMSEnabled { get; set; }

        /// <summary>
        /// <para type="description">Globally allow MFA with TOTP, users are using an applition to generate TOTP codes based on thier secret key (Default mode).</para>
        /// </summary>
        public bool AppsEnabled { get; set; }

        /// <summary>
        /// <para type="description">Globally allow MFA with Microsoft Azure, All verifications are made by Microsoft (very less secure).</para>
        /// </summary>
        public bool AzureEnabled { get; set; }

        /// <summary>
        /// <para type="description">Globally allow MFA Biometrics / FIDO.</para>
        /// </summary>
        public bool BiometricsEnabled { get; set; }
        */

        /// <summary>
        /// <para type="description">Default value for user's PIN.</para>
        /// </summary>
        public int DefaultPin { get; set; }

        /*
        /// <summary>
        /// <para type="description">TOTP Hash mode for TOTP Key (Default SHA1).</para>
        /// </summary>
        public HashMode Algorithm { get; set; }
        */

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
                psconfig.Issuer = config.Issuer;
                psconfig.DefaultPin = config.DefaultPin;
                psconfig.PinLength = config.PinLength;
                psconfig.UseActiveDirectory = config.UseActiveDirectory;
                psconfig.CustomUpdatePassword = config.CustomUpdatePassword;
                psconfig.KeepMySelectedOptionOn = config.KeepMySelectedOptionOn;
                psconfig.UserFeatures = config.UserFeatures;
                psconfig.AdvertisingDays = config.AdvertisingDays;
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
                config.IsDirty = true;
                config.Issuer = psconfig.Issuer;
                config.DefaultPin = psconfig.DefaultPin;
                config.PinLength = psconfig.PinLength;
                config.UseActiveDirectory = psconfig.UseActiveDirectory;
                config.KeepMySelectedOptionOn = psconfig.KeepMySelectedOptionOn;
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
        public static explicit operator PSConfigSQL(FlatConfigSQL sqlconfig)
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
        public static explicit operator FlatConfigSQL(PSConfigSQL psconfig)
        {
            if (psconfig == null)
                return null;
            else
            {
                FlatConfigSQL config = new FlatConfigSQL();
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
                return config;
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
        public SecretKeyFormat KeyFormat { get; set; }

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
        public static explicit operator PSKeysConfig(FlatKeysConfig mgr)
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
        public static explicit operator FlatKeysConfig(PSKeysConfig mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                FlatKeysConfig target = new FlatKeysConfig();
                target.IsDirty = true;
                target.CertificateThumbprint = mgr.CertificateThumbprint;
                target.CertificateValidity = mgr.CertificateValidity;
                target.KeyFormat = mgr.KeyFormat;
                target.KeyGenerator = mgr.KeyGenerator;
                target.KeySize = mgr.KeySize;
                target.ExternalKeyManager = (FlatExternalKeyManager)mgr.ExternalKeyManager;
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
        public static explicit operator PSExternalKeyManager(FlatExternalKeyManager mgr)
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
        public static explicit operator FlatExternalKeyManager(PSExternalKeyManager mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                FlatExternalKeyManager ret = new FlatExternalKeyManager();
                ret.FullQualifiedImplementation = mgr.FullQualifiedImplementation;
                ret.Parameters = mgr.Parameters;
                return ret;
            }
        }
    }
    #endregion

    #region PSConfigTOTPProvider
    /// <summary>
    /// PSConfigTOTPProvider class
    /// <para type="synopsis">Parameters for TOTP Provider.</para>
    /// <para type="description">provided for TOTP MFA.</para>
    /// <para type="description">Typically this component is used with authenticator applications, Notification and more.</para>
    /// </summary>
    public class PSConfigTOTPProvider
    {
        /// <summary>
        /// <para type="description">TOTP Provider Enabled property.</para>
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// <para type="description">TOTP Provider Enrollment Wizard Enabled property.</para>
        /// </summary>
        public bool EnrollWizard { get; set; }

        /// <summary>
        /// <para type="description">TOTP Provider Enrollment Wizard Enabled in manage my options property.</para>
        /// </summary>
        public bool EnrollWizardStrict { get; set; }

        /// <summary>
        /// <para type="description">TOTP Provider Shadow codes. 2 by default</para>
        /// </summary>
        public int TOTPShadows { get; set; }

        /// <summary>
        /// <para type="description">TOTP Provider Hash algorithm. SHA1 by default</para>
        /// </summary>
        public HashMode Algorithm { get; set; }

        /// <summary>
        /// <para type="description">Set if additionnal verification with PIN (locally administered) must be done.</para>
        /// </summary>
        public bool PinRequired { get; set; }

        /// <summary>
        /// <para type="description">Set TOP Wizard Application list enabled/ disabled.</para>
        /// </summary>
        public OTPWizardOptions WizardOptions { get; set; }

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
                target.EnrollWizardStrict = otp.EnrollWizardStrict;
                target.TOTPShadows = otp.TOTPShadows;
                target.Algorithm = otp.Algorithm; 
                target.PinRequired = otp.PinRequired;
                target.WizardOptions = otp.WizardOptions;
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
                target.EnrollWizardStrict = otp.EnrollWizardStrict;
                target.TOTPShadows = otp.TOTPShadows;
                target.Algorithm = otp.Algorithm;
                target.PinRequired = otp.PinRequired;
                target.WizardOptions = otp.WizardOptions;
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
    public class PSConfigMail
    {
        /// <summary>
        /// <para type="description">Mail Provider Enabled property.</para>
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// <para type="description">Mail Provider Enrollment Wizard Enabled property.</para>
        /// </summary>
        public bool EnrollWizard { get; set; }

        /// <summary>
        /// <para type="description">Mail Provider Enrollment Wizard Enabled in manage my options property.</para>
        /// </summary>
        public bool EnrollWizardStrict { get; set; }

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
        /// <para type="description">indicate if IN validation is required with Mail.</para>
        /// </summary>
        public bool PinRequired { get; set; }

        /// <summary>
        /// <para type="description">indicate if connetion is Anonymous.</para>
        /// </summary>
        public bool Anonymous { get; set; }

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
        public static explicit operator PSConfigMail(FlatConfigMail mails)
        {
            if (mails == null)
                return null;
            else
            {
                PSConfigMail psconfig = new PSConfigMail();
                psconfig.Enabled = mails.Enabled;
                psconfig.EnrollWizard =  mails.EnrollWizard;
                psconfig.EnrollWizardStrict = mails.EnrollWizardStrict;
                psconfig.From = mails.From;
                psconfig.UserName = mails.UserName;
                psconfig.Password = mails.Password;
                psconfig.Host = mails.Host;
                psconfig.Port = mails.Port;
                psconfig.UseSSL = mails.UseSSL;
                psconfig.Company = mails.Company;
                psconfig.PinRequired = mails.PinRequired;
                psconfig.Anonymous = mails.Anonymous;
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
                return psconfig;
            }
        }

        /// <summary>
        /// explicit conversion from PSConfigMail
        /// </summary>
        public static explicit operator FlatConfigMail(PSConfigMail mails)
        {
            if (mails == null)
                return null;
            else
            {
                FlatConfigMail psconfig = new FlatConfigMail();
                psconfig.IsDirty = true;
                psconfig.Enabled = mails.Enabled;
                psconfig.EnrollWizard = mails.EnrollWizard;
                psconfig.EnrollWizardStrict = mails.EnrollWizardStrict;
                psconfig.From = mails.From;
                psconfig.UserName = mails.UserName;
                psconfig.Password = mails.Password;
                psconfig.Host = mails.Host;
                psconfig.Port = mails.Port;
                psconfig.UseSSL = mails.UseSSL;
                psconfig.Company = mails.Company;
                psconfig.PinRequired = mails.PinRequired;
                psconfig.Anonymous = mails.Anonymous;
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

    #region PSConfigExternalProvider
    /// <summary>
    /// PSConfigExternalProvider class
    /// <para type="synopsis">Specify External OTP Provider, you must implement IExternalOTPProvider interface.</para>
    /// <para type="description">Samples are provided for Azure and custom.</para>
    /// <para type="description">Typically this component is used when sending SMS, you can use your own SMS gateway.</para>
    /// </summary>
    public class PSConfigExternalProvider
    {
        /// <summary>
        /// <para type="description">External Provider Enabled property.</para>
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// <para type="description">External Provider Enrollment Wizard Enabled property.</para>
        /// </summary>
        public bool EnrollWizard { get; set; }

        /// <summary>
        /// <para type="description">External Provider Enrollment Wizard Enabled in manage my options property.</para>
        /// </summary>
        public bool EnrollWizardStrict { get; set; }

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
        /// <para type="description">indicate if IN validation is required with Mail.</para>
        /// </summary>
        public bool PinRequired { get; set; }

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
                target.EnrollWizardStrict = otp.EnrollWizardStrict;
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
                target.EnrollWizardStrict = otp.EnrollWizardStrict;
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
    public class PSConfigAzureProvider
    {
        /// <summary>
        /// <para type="description">Azure Provider Enabled property.</para>
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// <para type="description">Azure Provider Enrollment Wizard Enabled property.</para>
        /// </summary>
        public bool EnrollWizard { get; set; }

        /// <summary>
        /// <para type="description">Azure Provider Enrollment Wizard Enabled in manage my options property.</para>
        /// </summary>
        public bool EnrollWizardStrict { get; set; }

        /// <summary>
        /// <para type="description">your Azure/o365 tenantId / tenant name.</para>
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// <para type="description">Thumbprint of the Azure cetificate (Done when configuring Azure MFA.</para>
        /// </summary>
        public string Thumbprint { get; set; }

        /// <summary>
        /// <para type="description">Set if additionnal verification with PIN (locally administered) must be done.</para>
        /// </summary>
        public bool PinRequired { get; set; }

        /// <summary>
        /// explicit operator from MMCKeysConfig
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
                target.EnrollWizardStrict = false;
                target.PinRequired = otp.PinRequired;
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
                target.EnrollWizardStrict = false;
                target.PinRequired = otp.PinRequired;
                return target;
            }
        }
    }
    #endregion
}