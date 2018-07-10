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
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Runtime.Serialization;
using System.Management.Automation.Host;
using System.Xml;

namespace Neos.IdentityServer.MultiFactor.Administration
{
    [Serializable]
    public enum FlatSecretKeyFormat
    {
        RNG = 0,
        RSA = 1,
        CUSTOM = 2
    }

    [Serializable]
    public enum FlatTemplateMode
    {
        Free = 0,                        // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
        Open = 1,                        // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
        Default = 2,                     // (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
        Managed = 3,                     // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword);
        Strict = 4,                      // (UserFeaturesOptions.AllowProvideInformations);
        Administrative = 5,              // (UserFeaturesOptions.AdministrativeMode);
        Custom = 6                       // Empty 
    }

    public static class FlatUserFeaturesOptionsExtensions
    {
        /// <summary>
        /// Add method implementation
        /// </summary>
        public static UserFeaturesOptions Add(this UserFeaturesOptions options, UserFeaturesOptions toadd)
        {
            options |= toadd;
            return options;
        }

        /// <summary>
        /// Remove method implmentation
        /// </summary>
        public static UserFeaturesOptions Remove(this UserFeaturesOptions options, UserFeaturesOptions toremove)
        {
            options &= ~toremove;
            return options;
        }

        /// <summary>
        /// SetMFARequired method implementation
        /// </summary>
        public static UserFeaturesOptions SetMFARequired(this UserFeaturesOptions options)
        {
            options = options.Remove(UserFeaturesOptions.BypassDisabled);
            options = options.Remove(UserFeaturesOptions.AllowDisabled);
            options = options.Add(UserFeaturesOptions.AdministrativeMode); // Admin only
            return options;
        }

        /// <summary>
        /// SetMFAAllowed method implementation
        /// </summary>
        public static UserFeaturesOptions SetMFAAllowed(this UserFeaturesOptions options)
        {
            options = options.Remove(UserFeaturesOptions.BypassDisabled);
            options = options.Remove(UserFeaturesOptions.AdministrativeMode);
            options = options.Add(UserFeaturesOptions.AllowDisabled); // Allow Disable Only
            return options;
        }

        /// <summary>
        /// SetMFANotRequired method implementation
        /// </summary>
        public static UserFeaturesOptions SetMFANotRequired(this UserFeaturesOptions options)
        {
            options = options.Remove(UserFeaturesOptions.AdministrativeMode);
            options = options.Remove(UserFeaturesOptions.AllowDisabled);
            options = options.Add(UserFeaturesOptions.BypassDisabled); // Allow Bypass Only  
            return options;
        }

        /// <summary>
        /// SetAdministrativeRegistration method implementation
        /// </summary>
        public static UserFeaturesOptions SetAdministrativeRegistration(this UserFeaturesOptions options)
        {
            options = options.Remove(UserFeaturesOptions.BypassUnRegistered);
            options = options.Remove(UserFeaturesOptions.AllowUnRegistered);
            options = options.Add(UserFeaturesOptions.AllowProvideInformations);   // Allow only provide informations
            return options;
        }

        /// <summary>
        /// SetSelfRegistration method implementation
        /// </summary>
        public static UserFeaturesOptions SetSelfRegistration(this UserFeaturesOptions options)
        {
            options = options.Remove(UserFeaturesOptions.BypassUnRegistered);
            options = options.Remove(UserFeaturesOptions.AllowProvideInformations);
            options = options.Add(UserFeaturesOptions.AllowUnRegistered);    // Allow User to register
            return options;
        }

        /// <summary>
        /// SetUnManagedRegistration method implementation
        /// </summary>
        public static UserFeaturesOptions SetUnManagedRegistration(this UserFeaturesOptions options)
        {
            options = options.Remove(UserFeaturesOptions.AllowProvideInformations);
            options = options.Remove(UserFeaturesOptions.AllowUnRegistered);
            options = options.Add(UserFeaturesOptions.BypassUnRegistered);   // Allow Unregistered
            return options;
        }

        #region MFA Policies
        /// <summary>
        /// GetPolicyTemplate method implementation
        /// </summary>
        public static UserTemplateMode GetPolicyTemplate(this UserFeaturesOptions options)
        {
            if (options == (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword))
                return UserTemplateMode.Free;
            else if (options == (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword))
                return UserTemplateMode.Open;
            else if (options == (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword))
                return UserTemplateMode.Default;
            else if (options == (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword))
                return UserTemplateMode.Managed;
            else if (options == (UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AdministrativeMode))
                return UserTemplateMode.Strict;
            else if (options == (UserFeaturesOptions.AdministrativeMode))
                return UserTemplateMode.Administrative;
            else
                return UserTemplateMode.Custom;
        }

        /// <summary>
        /// SetPolicyTemplate method implementation
        /// </summary>
        public static UserFeaturesOptions SetPolicyTemplate(this UserFeaturesOptions options, UserTemplateMode template)
        {
            switch (template)
            {
                case UserTemplateMode.Free:
                    options = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
                    break;
                case UserTemplateMode.Open:
                    options = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
                    break;
                case UserTemplateMode.Default:
                    options = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
                    break;
                case UserTemplateMode.Managed:
                    options = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword);
                    break;
                case UserTemplateMode.Strict:
                    options = (UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AdministrativeMode);
                    break;
                case UserTemplateMode.Administrative:
                    options = UserFeaturesOptions.AdministrativeMode;
                    break;
                default:
                    options = UserFeaturesOptions.NoSet;
                    break;
            }
            return options;
        }
        #endregion
    }
   
    [Serializable]
    public class FlatConfig
    {
        public bool IsDirty { get; set; }
        public int DeliveryWindow { get; set; }
        public int TOTPShadows { get; set; }
        public bool MailEnabled { get; set; }
        public bool SMSEnabled { get; set; }
        public bool AppsEnabled { get; set; }
        public bool AzureEnabled { get; set; }
        public bool BiometricsEnabled { get; set; }
        public int PinLength { get; set; }
        public int DefaultPin { get; set; }
        public HashMode Algorithm { get; set; }
        public string Issuer { get; set; }
        public bool UseActiveDirectory { get; set; }
        public bool CustomUpdatePassword { get; set; }
        public string DefaultCountryCode { get; set; }
        public string AdminContact { get; set; }
        public UserFeaturesOptions UserFeatures { get; set; }
        public ConfigAdvertising AdvertisingDays { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            AdminContact = cfg.AdminContact;
            IsDirty = cfg.IsDirty;
            DeliveryWindow = cfg.DeliveryWindow;
            TOTPShadows = cfg.OTPProvider.TOTPShadows;
            MailEnabled = cfg.MailProvider.Enabled;
            SMSEnabled = cfg.ExternalProvider.Enabled;
            AppsEnabled = cfg.OTPProvider.Enabled;
            AzureEnabled = cfg.AzureProvider.Enabled;
            BiometricsEnabled = false;
            DefaultPin = cfg.DefaultPin;
            PinLength = cfg.PinLength;
            Algorithm = cfg.OTPProvider.Algorithm;
            Issuer = cfg.Issuer;
            UseActiveDirectory = cfg.UseActiveDirectory;
            CustomUpdatePassword = cfg.CustomUpdatePassword;
            DefaultCountryCode = cfg.DefaultCountryCode;
            AdminContact = cfg.AdminContact;
            UserFeatures = cfg.UserFeatures;
            AdvertisingDays = cfg.AdvertisingDays;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            cfg.AdminContact = AdminContact;
            cfg.IsDirty = IsDirty;
            cfg.DeliveryWindow = DeliveryWindow;
            cfg.OTPProvider.TOTPShadows = TOTPShadows;
            cfg.MailProvider.Enabled = MailEnabled;
            cfg.ExternalProvider.Enabled = SMSEnabled;
            cfg.OTPProvider.Enabled = AppsEnabled;
            cfg.AzureProvider.Enabled = AzureEnabled;
           // cfg.BiometricsEnabled = false;
            cfg.DefaultPin = DefaultPin;
            cfg.PinLength = PinLength;
            cfg.OTPProvider.Algorithm = Algorithm;
            cfg.Issuer = Issuer;
            cfg.UseActiveDirectory = UseActiveDirectory;
            cfg.CustomUpdatePassword = CustomUpdatePassword;
            cfg.DefaultCountryCode = DefaultCountryCode;
            cfg.AdminContact = AdminContact;
            cfg.UserFeatures = UserFeatures;
            cfg.AdvertisingDays = AdvertisingDays;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }

        /// <summary>
        /// SetTemplate method implmentation
        /// </summary>
        public void SetTemplate(PSHost host, FlatTemplateMode mode)
        {
            ManagementService.Initialize(true);
            MFAConfig cfg = ManagementService.Config;
            switch (mode)
            {
                case FlatTemplateMode.Free:
                    cfg.UserFeatures = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
                    break;
                case FlatTemplateMode.Open:
                    cfg.UserFeatures = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
                    break;
                case FlatTemplateMode.Default:
                    cfg.UserFeatures = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
                    break;
                case FlatTemplateMode.Managed:
                    cfg.UserFeatures = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword);
                    break;
                case FlatTemplateMode.Strict:
                    cfg.UserFeatures = (UserFeaturesOptions.AllowProvideInformations);
                    break;
                case FlatTemplateMode.Administrative:
                    cfg.UserFeatures = (UserFeaturesOptions.AdministrativeMode);
                    break;
            }
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }

    [Serializable]
    public class FlatConfigSQL
    {
        public bool IsDirty { get; set; }
        public string ConnectionString { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            SQLServerHost sql = cfg.Hosts.SQLServerHost;
            IsDirty = cfg.IsDirty;
            ConnectionString = sql.ConnectionString;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            SQLServerHost sql = cfg.Hosts.SQLServerHost;
            cfg.IsDirty = IsDirty;
            if (!ManagementService.CheckRepositoryAttribute(ConnectionString, 2))
                throw new ArgumentException(string.Format("Invalid ConnectionString {0} !", ConnectionString));
            sql.ConnectionString = ConnectionString;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }

    [Serializable]
    public class FlatConfigADDS
    {
        public bool IsDirty { get; set; }
        public string DomainAddress { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string KeyAttribute { get; set; }
        public string MailAttribute { get; set; }
        public string PhoneAttribute { get; set; }
        public string MethodAttribute { get; set; }
        public string OverrideMethodAttribute { get; set; }
        public string EnabledAttribute { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            ADDSHost adds = cfg.Hosts.ActiveDirectoryHost;
            IsDirty = cfg.IsDirty;
            Account = adds.Account;
            Password = adds.Password;
            DomainAddress = adds.DomainAddress;
            KeyAttribute = adds.keyAttribute;
            MailAttribute = adds.mailAttribute;
            PhoneAttribute = adds.phoneAttribute;
            MethodAttribute = adds.methodAttribute;
            OverrideMethodAttribute = adds.overridemethodAttribute;
            EnabledAttribute = adds.totpEnabledAttribute;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            ADDSHost adds = cfg.Hosts.ActiveDirectoryHost;
            cfg.IsDirty = IsDirty;
            adds.Account = Account;
            adds.Password = adds.Password;
            adds.DomainAddress = adds.DomainAddress;
            if (!ManagementService.CheckRepositoryAttribute(KeyAttribute, 1))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", KeyAttribute));
            adds.keyAttribute = KeyAttribute;
            if (!ManagementService.CheckRepositoryAttribute(MailAttribute, 1))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", MailAttribute));
            adds.mailAttribute = MailAttribute;
            if (!ManagementService.CheckRepositoryAttribute(PhoneAttribute, 1))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", PhoneAttribute));
            adds.phoneAttribute = PhoneAttribute;
            if (!ManagementService.CheckRepositoryAttribute(MethodAttribute,1 ))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", MethodAttribute));
            adds.methodAttribute = MethodAttribute;
            if (!ManagementService.CheckRepositoryAttribute(OverrideMethodAttribute, 1))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", OverrideMethodAttribute));
            adds.overridemethodAttribute = OverrideMethodAttribute;
            if (!ManagementService.CheckRepositoryAttribute(EnabledAttribute, 1))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", EnabledAttribute));
            adds.totpEnabledAttribute = EnabledAttribute;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }

    [Serializable]
    public class FlatConfigMail
    {
        public bool IsDirty { get; set; }
        public string From { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public bool PinRequired { get; set; }
        public string Company { get; set; }
        public List<FlatConfigMailFileName> MailOTPContent { get; set; }
        public List<FlatConfigMailFileName> MailAdminContent { get; set; }
        public List<FlatConfigMailFileName> MailKeyContent { get; set; }

        public FlatConfigMail()
        {
            this.MailOTPContent = new List<FlatConfigMailFileName>();
            this.MailAdminContent = new List<FlatConfigMailFileName>();
            this.MailKeyContent = new List<FlatConfigMailFileName>();
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            MailProvider mail = cfg.MailProvider;
            IsDirty = cfg.IsDirty;
            From  = mail.From;
            UserName = mail.UserName;
            Password = mail.Password;
            Host = mail.Host;
            Port = mail.Port; 
            UseSSL = mail.UseSSL;
            Company = mail.Company;
            PinRequired = mail.PinRequired;
            MailOTPContent.Clear();
            foreach(SendMailFileName itm in mail.MailOTPContent)
            {
                MailOTPContent.Add((FlatConfigMailFileName)itm);
            }
            MailAdminContent.Clear();
            foreach(SendMailFileName itm in mail.MailAdminContent)
            {
                MailAdminContent.Add((FlatConfigMailFileName)itm);
            }
            MailKeyContent.Clear();
            foreach(SendMailFileName itm in mail.MailKeyContent)
            {
                MailKeyContent.Add((FlatConfigMailFileName)itm);
            }
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            MailProvider mail = cfg.MailProvider;
            cfg.IsDirty = IsDirty;
            mail.From = From;
            mail.UserName = UserName;
            mail.Password = Password;
            mail.Host = Host;
            mail.Port = Port;
            mail.UseSSL = UseSSL;
            mail.Company = Company;
            mail.PinRequired = PinRequired;
            mail.MailOTPContent.Clear();
            foreach (FlatConfigMailFileName itm in MailOTPContent)
            {
                mail.MailOTPContent.Add((SendMailFileName)itm);
            }
            mail.MailAdminContent.Clear();
            foreach (FlatConfigMailFileName itm in MailAdminContent)
            {
                mail.MailAdminContent.Add((SendMailFileName)itm);
            }
            mail.MailKeyContent.Clear();
            foreach (FlatConfigMailFileName itm in MailKeyContent)
            {
                mail.MailKeyContent.Add((SendMailFileName)itm);
            }
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }

    [Serializable]
    public class FlatConfigMailFileName
    {
        public int LCID { get; set; }
        public string FileName { get; set; }
        public bool Enabled { get; set; }

        public FlatConfigMailFileName(int lcid, string filename, bool enabled = true)
        {
            this.LCID = lcid;
            this.FileName = filename;
            this.Enabled = enabled;
        }

        public static explicit operator SendMailFileName(FlatConfigMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new SendMailFileName(file.LCID, file.FileName, file.Enabled);
        }

        public static explicit operator FlatConfigMailFileName(SendMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new FlatConfigMailFileName(file.LCID, file.FileName, file.Enabled);
        }
    }

    [Serializable]
    public class FlatKeysConfig
    {
        public bool IsDirty { get; set; }
        public KeyGeneratorMode KeyGenerator  { get; set; }
        public KeySizeMode KeySize { get; set; }
        public SecretKeyFormat KeyFormat  { get; set; }
        public string CertificateThumbprint  { get; set; }
        public int CertificateValidity  { get; set; }
        public FlatExternalKeyManager ExternalKeyManager  { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            KeysManagerConfig keys = cfg.KeysConfig;
            IsDirty = cfg.IsDirty;
            this.CertificateThumbprint = keys.CertificateThumbprint;
            this.CertificateValidity = keys.CertificateValidity;
            this.KeyFormat = keys.KeyFormat;
            this.KeyGenerator = keys.KeyGenerator;
            this.KeySize = keys.KeySize;
            this.ExternalKeyManager = (FlatExternalKeyManager)keys.ExternalKeyManager;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            KeysManagerConfig keys = cfg.KeysConfig;
            cfg.IsDirty = true;
            keys.CertificateThumbprint = this.CertificateThumbprint;
            keys.CertificateValidity = this.CertificateValidity;
            keys.KeyFormat = this.KeyFormat;
            keys.KeyGenerator = this.KeyGenerator;
            keys.KeySize = this.KeySize;
            keys.ExternalKeyManager = (ExternalKeyManagerConfig)this.ExternalKeyManager;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }

    [Serializable]
    public class FlatExternalKeyManager
    {
        public string FullQualifiedImplementation { get; set; }
        public XmlCDataSection Parameters { get; set; }

        public static explicit operator ExternalKeyManagerConfig(FlatExternalKeyManager mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                ExternalKeyManagerConfig ret = new ExternalKeyManagerConfig();
                ret.FullQualifiedImplementation = mgr.FullQualifiedImplementation;
                ret.Parameters = mgr.Parameters;
                return ret;
            }
        }

        public static explicit operator FlatExternalKeyManager(ExternalKeyManagerConfig mgr)
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

        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
           // MFAConfig cfg = ManagementService.ADFSManager.ReadConfiguration(host);
            MFAConfig cfg = ManagementService.Config;
            KeysManagerConfig otp = cfg.KeysConfig;
            this.FullQualifiedImplementation = otp.ExternalKeyManager.FullQualifiedImplementation;
            this.Parameters = otp.ExternalKeyManager.Parameters;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
           // MFAConfig cfg = ManagementService.ADFSManager.Config;
            MFAConfig cfg = ManagementService.Config;
            KeysManagerConfig otp = cfg.KeysConfig;
            otp.ExternalKeyManager.FullQualifiedImplementation = this.FullQualifiedImplementation;
            otp.ExternalKeyManager.Parameters = this.Parameters;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }

    }

    [Serializable]
    public class FlatExternalProvider
    {
        public bool IsDirty { get; set; }
        public string Company { get; set; }
        public string Sha1Salt { get; set; }
        public string FullQualifiedImplementation  { get; set; }
        public bool PinRequired { get; set; }
        public XmlCDataSection Parameters  { get; set; }
        public bool IsTwoWay  { get; set; }
        public int Timeout  { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
           // MFAConfig cfg = ManagementService.ADFSManager.ReadConfiguration(host);
            MFAConfig cfg = ManagementService.Config;
            ExternalOTPProvider otp = cfg.ExternalProvider;
            this.IsDirty = cfg.IsDirty;
            this.Company = otp.Company;
            this.FullQualifiedImplementation = otp.FullQualifiedImplementation;
            this.IsTwoWay = otp.IsTwoWay;
            this.Sha1Salt = otp.Sha1Salt;
            this.Timeout = otp.Timeout;
            this.PinRequired = otp.PinRequired;
            this.Parameters = otp.Parameters;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
           // MFAConfig cfg = ManagementService.ADFSManager.Config;
            MFAConfig cfg = ManagementService.Config;
            ExternalOTPProvider otp = cfg.ExternalProvider;
            cfg.IsDirty = true;
            otp.Company = this.Company;
            otp.FullQualifiedImplementation = this.FullQualifiedImplementation;
            otp.IsTwoWay = this.IsTwoWay;
            otp.Sha1Salt = this.Sha1Salt;
            otp.Timeout = this.Timeout;
            otp.Parameters = this.Parameters;
            otp.PinRequired = this.PinRequired;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }

    [Serializable]
    public class FlatAzureProvider
    {
        public bool IsDirty { get; set; }
        public string TenantId { get; set; }
        public string ThumbPrint { get; set; }
        public bool PinRequired { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            AzureProvider otp = cfg.AzureProvider;
            this.IsDirty = cfg.IsDirty;
            this.TenantId = otp.TenantId;
            this.ThumbPrint = otp.ThumbPrint;
            this.PinRequired = otp.PinRequired;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            AzureProvider otp = cfg.AzureProvider;
            cfg.IsDirty = true;
            otp.TenantId = this.TenantId;
            otp.ThumbPrint = this.ThumbPrint;
            otp.PinRequired = this.PinRequired;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
}
