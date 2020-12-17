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
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Runtime.Serialization;
using System.Management.Automation.Host;
using System.Xml;

namespace Neos.IdentityServer.MultiFactor.Administration
{  

    [Serializable]
    public enum FlatTemplateMode
    {
        Free = 0,                        // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment));
        Open = 1,                        // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment));
        Default = 2,                     // (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment));
        Mixed = 3,                       // (UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment));
        Managed = 4,                     // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword);
        Strict = 5,                      // (UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AdministrativeMode);
        Administrative = 6,              // (UserFeaturesOptions.AdministrativeMode);
        Custom = 7                       // Empty 
    }

    /// <summary>
    /// FlatSampleKind
    /// </summary>   
    public enum FlatSampleKind
    {
        QuizProvider = 1,
        CaesarEnryption = 2,
        InMemoryStorage = 3,
        TOTPProvider = 4,
        SMSProvider = 5
    }

    #region FlatConfig
    /// <summary>
    /// FlatConfig class implmentation
    /// </summary>
    public class FlatConfig
    {
        public bool IsDirty { get; set; }
        public string Issuer { get; set; }
        public string AdminContact { get; set; }
        public string DefaultCountryCode { get; set; }
        public PreferredMethod DefaultProviderMethod { get; set; }
        public UserFeaturesOptions UserFeatures { get; set; }
        public bool CustomUpdatePassword { get; set; }
        public bool KeepMySelectedOptionOn { get; set; }
        public bool ChangeNotificationsOn { get; set; }
        public bool UseOfUserLanguages { get; set; }
        public FlatAdvertising AdvertisingDays { get; set; }
        public ADFSUserInterfaceKind UiKind { get; set; }
        public bool UseUIPaginated { get; set; }
        public PrimaryAuthOptions PrimaryAuhenticationOptions { get; set; }
        public string ForcedLanguage { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            IsDirty = cfg.IsDirty;
            AdminContact = cfg.AdminContact;
            Issuer = cfg.Issuer;
            DefaultCountryCode = cfg.DefaultCountryCode;
            DefaultProviderMethod = cfg.DefaultProviderMethod;
            UserFeatures = cfg.UserFeatures;
            CustomUpdatePassword = cfg.CustomUpdatePassword;
            KeepMySelectedOptionOn = cfg.KeepMySelectedOptionOn;
            ChangeNotificationsOn = cfg.ChangeNotificationsOn;
            UseOfUserLanguages = cfg.UseOfUserLanguages;           
            AdvertisingDays = (FlatAdvertising)cfg.AdvertisingDays;
            UseUIPaginated = cfg.UseUIPaginated;
            UiKind = cfg.UiKind;
            PrimaryAuhenticationOptions = cfg.PrimaryAuhenticationOptions;
            ForcedLanguage = cfg.ForcedLanguage;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            cfg.IsDirty = IsDirty;
            cfg.AdminContact = AdminContact;
            cfg.Issuer = Issuer;
            cfg.DefaultCountryCode = DefaultCountryCode;
            cfg.DefaultProviderMethod = DefaultProviderMethod;
            cfg.UserFeatures = UserFeatures;
            cfg.CustomUpdatePassword = CustomUpdatePassword;
            cfg.KeepMySelectedOptionOn = KeepMySelectedOptionOn;
            cfg.ChangeNotificationsOn = ChangeNotificationsOn;
            cfg.UseOfUserLanguages = UseOfUserLanguages;
            cfg.AdvertisingDays = (ConfigAdvertising)AdvertisingDays;
            cfg.UseUIPaginated = UseUIPaginated;
            cfg.UiKind = cfg.UiKind;
            cfg.PrimaryAuhenticationOptions = PrimaryAuhenticationOptions;
            cfg.ForcedLanguage = ForcedLanguage;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }

        /// <summary>
        /// SetPolicyTemplate method implmentation
        /// </summary>
        public void SetPolicyTemplate(PSHost host, FlatTemplateMode mode)
        {
            ManagementService.Initialize(true);
            MFAConfig cfg = ManagementService.Config;
            UserTemplateMode md = (UserTemplateMode)mode;
            cfg.UserFeatures = cfg.UserFeatures.SetPolicyTemplate(md);
            if (md != UserTemplateMode.Administrative)
                cfg.KeepMySelectedOptionOn = true;
            else
                cfg.KeepMySelectedOptionOn = false;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }

        /// <summary>
        /// SetTheme method implementation
        /// </summary>
        internal void SetTheme(PSHost host, int _kind, string _theme, bool _dynparam)
        {
            RegistryVersion reg = new RegistryVersion();
            ManagementService.Initialize(true);
            MFAConfig cfg = ManagementService.Config;
            if (reg.IsWindows2019)
            {
                cfg.UiKind = (ADFSUserInterfaceKind)_kind;
                if ((ADFSUserInterfaceKind)_kind == ADFSUserInterfaceKind.Default)
                    cfg.UseUIPaginated = false;
                else
                    cfg.UseUIPaginated = _dynparam;
                ManagementService.ADFSManager.SetADFSTheme(host, _theme, cfg.UseUIPaginated, true);
                ManagementService.ADFSManager.WriteConfiguration(host);
            }
            else
            {
                cfg.UiKind = ADFSUserInterfaceKind.Default;
                cfg.UseUIPaginated = false;
                ManagementService.ADFSManager.SetADFSTheme(host, _theme, false, false);
                ManagementService.ADFSManager.WriteConfiguration(host);
            }
        }

        /// <summary>
        /// SetLibraryVersion method implementation
        /// </summary>
        internal void SetLibraryVersion(PSHost host, int version)
        {
            ManagementService.Initialize(true);
            MFAConfig cfg = ManagementService.Config;
            cfg.KeysConfig.KeyVersion = (SecretKeyVersion)version;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }

        /// <summary>
        /// SetPrimaryAuthenticationStatus method implementation
        /// </summary>
        internal void SetPrimaryAuthenticationStatus(PSHost host, bool enabled, PrimaryAuthOptions options)
        {
            ManagementService.Initialize(true);
            MFAConfig cfg = ManagementService.Config;
            cfg.IsPrimaryAuhentication = enabled;
            cfg.PrimaryAuhenticationOptions = options;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion

    #region FlatSQLStore
    /// <summary>
    /// FlatSQLStore class implementation
    /// </summary>
    public class FlatSQLStore
    {
        public bool IsDirty { get; set; }
        public bool Active { get; set; }
        public string ConnectionString { get; set; }
        public bool IsAlwaysEncrypted { get; set; }
        public int CertificateValidity { get; set; }
        public string ThumbPrint { get; set; }
        public string KeyName { get; set; }
        public bool CertReuse { get; set; }
        public int MaxRows { get; set; }
        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            SQLServerHost sql = cfg.Hosts.SQLServerHost;
            IsDirty = cfg.IsDirty;
            Active = (cfg.StoreMode == DataRepositoryKind.SQL);
            ConnectionString = sql.ConnectionString;
            IsAlwaysEncrypted = sql.IsAlwaysEncrypted;
            CertificateValidity = sql.CertificateValidity;
            ThumbPrint = sql.ThumbPrint;
            CertReuse = sql.CertReuse;
            KeyName = sql.KeyName;
            MaxRows = sql.MaxRows;
           
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
            if (!ManagementService.CheckSQLConnection(ConnectionString))
                throw new ArgumentException(string.Format("Invalid ConnectionString {0} !", ConnectionString));

            if (Active)
                cfg.StoreMode = DataRepositoryKind.SQL;

            sql.ConnectionString = ConnectionString;
            sql.IsAlwaysEncrypted = IsAlwaysEncrypted;
            sql.CertificateValidity = CertificateValidity;
            sql.ThumbPrint = ThumbPrint;
            sql.CertReuse = CertReuse;
            sql.KeyName = KeyName;     
            sql.MaxRows = MaxRows;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion

    #region FlatADDSStore
    /// <summary>
    /// FlatADDSStore class implementation
    /// </summary>
    public class FlatADDSStore
    {
        public bool IsDirty { get; set; }
        public bool Active { get; set; }
        public string KeyAttribute { get; set; }
        public string MailAttribute { get; set; }
        public string PhoneAttribute { get; set; }
        public string MethodAttribute { get; set; }
        public string OverrideMethodAttribute { get; set; }
        public string PinAttribute { get; set; }
        public string EnabledAttribute { get; set; }
        public string PublicKeyCredentialAttribute { get; set; }
        public string ClientCertificateAttribute { get; set; }
        public string RSACertificateAttribute { get; set; }
        public int MaxRows { get; set; }
        public bool UseSSL { get; set; }
        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            ADDSHost adds = cfg.Hosts.ActiveDirectoryHost;
            IsDirty = cfg.IsDirty;
            Active = (cfg.StoreMode== DataRepositoryKind.ADDS);
            KeyAttribute = adds.KeyAttribute;
            MailAttribute = adds.MailAttribute;
            PhoneAttribute = adds.PhoneAttribute;
            MethodAttribute = adds.MethodAttribute;
            OverrideMethodAttribute = adds.OverrideMethodAttribute;
            PinAttribute = adds.PinAttribute;
            EnabledAttribute = adds.TotpEnabledAttribute;
            PublicKeyCredentialAttribute = adds.PublicKeyCredentialAttribute;
            ClientCertificateAttribute = adds.ClientCertificateAttribute;
            RSACertificateAttribute = adds.RSACertificateAttribute;
            MaxRows = adds.MaxRows;
            UseSSL = adds.UseSSL;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            ADDSHost adds = cfg.Hosts.ActiveDirectoryHost;
            if (Active)
                cfg.StoreMode = DataRepositoryKind.ADDS;
            cfg.IsDirty = IsDirty;
            if (!ManagementService.CheckADDSAttribute(adds.DomainAddress, adds.Account, adds.Password, KeyAttribute, 1))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", KeyAttribute));
            if (!ManagementService.CheckADDSAttribute(adds.DomainAddress, adds.Account, adds.Password, MailAttribute, 1))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", MailAttribute));
            if (!ManagementService.CheckADDSAttribute(adds.DomainAddress, adds.Account, adds.Password, PhoneAttribute, 1))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", PhoneAttribute));
            if (!ManagementService.CheckADDSAttribute(adds.DomainAddress, adds.Account, adds.Password, MethodAttribute, 0))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", MethodAttribute));
            if (!ManagementService.CheckADDSAttribute(adds.DomainAddress, adds.Account, adds.Password, OverrideMethodAttribute, 0))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", OverrideMethodAttribute));
            if (!ManagementService.CheckADDSAttribute(adds.DomainAddress, adds.Account, adds.Password, PinAttribute, 0))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", PinAttribute));
            if (!ManagementService.CheckADDSAttribute(adds.DomainAddress, adds.Account, adds.Password, EnabledAttribute, 0))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", EnabledAttribute));
            if (!ManagementService.CheckADDSAttribute(adds.DomainAddress, adds.Account, adds.Password, PublicKeyCredentialAttribute, 2))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", PublicKeyCredentialAttribute));
            if (!ManagementService.CheckADDSAttribute(adds.DomainAddress, adds.Account, adds.Password, ClientCertificateAttribute, 0))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", ClientCertificateAttribute));
            if (!ManagementService.CheckADDSAttribute(adds.DomainAddress, adds.Account, adds.Password, RSACertificateAttribute, 0))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", RSACertificateAttribute));

            adds.KeyAttribute = KeyAttribute;
            adds.MailAttribute = MailAttribute;
            adds.PhoneAttribute = PhoneAttribute;
            adds.MethodAttribute = MethodAttribute;
            adds.OverrideMethodAttribute = OverrideMethodAttribute;
            adds.PinAttribute = PinAttribute;
            adds.TotpEnabledAttribute = EnabledAttribute;
            adds.PublicKeyCredentialAttribute = PublicKeyCredentialAttribute;
            ClientCertificateAttribute = adds.ClientCertificateAttribute;
            RSACertificateAttribute = adds.RSACertificateAttribute;
            adds.MaxRows = MaxRows;
            adds.UseSSL = UseSSL;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion

    #region FlatCustomStore
    /// <summary>
    /// FlatCustomStore class implementation
    /// </summary>
    public class FlatCustomStore
    {
        public bool IsDirty { get; set; }
        public bool Active { get; set; }
        public string ConnectionString { get; set; }
        public int MaxRows { get; set; }
        public string DataRepositoryFullyQualifiedImplementation { get; set; }
        public string KeysRepositoryFullyQualifiedImplementation { get; set; }
        public string Parameters { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            CustomStoreHost sql = cfg.Hosts.CustomStoreHost;
            IsDirty = cfg.IsDirty;
            Active = (cfg.StoreMode== DataRepositoryKind.Custom);
            ConnectionString = sql.ConnectionString;
            MaxRows = sql.MaxRows;
            DataRepositoryFullyQualifiedImplementation = sql.DataRepositoryFullyQualifiedImplementation;
            KeysRepositoryFullyQualifiedImplementation = sql.KeysRepositoryFullyQualifiedImplementation;
            Parameters = sql.Parameters.Data;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            CustomStoreHost sql = cfg.Hosts.CustomStoreHost;
            if (Active)
                cfg.StoreMode = DataRepositoryKind.Custom;
            cfg.IsDirty = IsDirty;
            
            sql.ConnectionString = ConnectionString;
            sql.MaxRows = MaxRows;
            sql.DataRepositoryFullyQualifiedImplementation = DataRepositoryFullyQualifiedImplementation;
            sql.KeysRepositoryFullyQualifiedImplementation = KeysRepositoryFullyQualifiedImplementation;
            sql.Parameters.Data = Parameters;

            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion

    #region FlatSecurity
    /// <summary>
    /// FlatSecurity class implementation
    /// </summary>
    public class FlatSecurity
    {
        public bool IsDirty { get; set; }
        public int DeliveryWindow { get; set; }
        public int MaxRetries { get; set; }
        public ReplayLevel ReplayLevel { get; set; }
        public SecretKeyVersion LibVersion { get; set; }
        public string XORSecret { get; set; }
        public int PinLength { get; set; }
        public int DefaultPin { get; set; }
        public string DomainAddress { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            KeysManagerConfig keys = cfg.KeysConfig;
            ADDSHost adds = cfg.Hosts.ActiveDirectoryHost;
            IsDirty = cfg.IsDirty;
            DeliveryWindow = cfg.DeliveryWindow;
            MaxRetries = cfg.MaxRetries;
            ReplayLevel = cfg.ReplayLevel;
            LibVersion = keys.KeyVersion;
            PinLength = cfg.PinLength;
            DefaultPin = cfg.DefaultPin;
            DomainAddress = adds.DomainAddress;
            Account = adds.Account;
            using (AESSystemEncryption MSIS = new AESSystemEncryption())
            {
                Password = MSIS.Encrypt(adds.Password);
                XORSecret = MSIS.Encrypt(keys.XORSecret);
            };
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            KeysManagerConfig keys = cfg.KeysConfig;
            ADDSHost adds = cfg.Hosts.ActiveDirectoryHost;
            cfg.IsDirty = true;
            cfg.DeliveryWindow = this.DeliveryWindow;
            cfg.MaxRetries = this.MaxRetries;
            cfg.ReplayLevel = this.ReplayLevel;
            keys.KeyVersion = this.LibVersion;
            cfg.PinLength = this.PinLength;
            cfg.DefaultPin = this.DefaultPin;
            adds.DomainAddress = this.DomainAddress;
            adds.Account = this.Account;
            using (AESSystemEncryption MSIS = new AESSystemEncryption())
            {
                adds.Password = MSIS.Decrypt(Password);
                keys.XORSecret = MSIS.Decrypt(XORSecret);
            };
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion

    #region FlatRNGSecurity
    /// <summary>
    /// FlatRNGSecurity class implementation
    /// </summary>
    public class FlatRNGSecurity
    {
        public bool IsDirty { get; set; }
        public KeyGeneratorMode KeyGenerator { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            KeysManagerConfig keys = cfg.KeysConfig;
            IsDirty = cfg.IsDirty;
            KeyGenerator = keys.KeyGenerator;
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
            keys.KeyGenerator = KeyGenerator;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion

    #region FlatRNGSecurity
    /// <summary>
    /// FlatCustomSecurity class implementation
    /// </summary>
    public class FlatCustomSecurity
    {
        public bool IsDirty { get; set; }
        public string CustomFullyQualifiedImplementation { get; set; }

        public string CustomParameters { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            KeysManagerConfig keys = cfg.KeysConfig;
            IsDirty = cfg.IsDirty;
            CustomFullyQualifiedImplementation = keys.CustomFullyQualifiedImplementation;
            CustomParameters = keys.CustomParameters.Data;
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
            keys.CustomFullyQualifiedImplementation = CustomFullyQualifiedImplementation;
            keys.CustomParameters.Data = CustomParameters;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion

    #region FlatAESSecurity
    /// <summary>
    /// FlatAESSecurity class implementation
    /// </summary>
    public class FlatAESSecurity
    {
        public bool IsDirty { get; set; }
        public AESKeyGeneratorMode AESKeyGenerator { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            KeysManagerConfig keys = cfg.KeysConfig;
            IsDirty = cfg.IsDirty;
            AESKeyGenerator = keys.AESKeyGenerator;
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
            keys.AESKeyGenerator = AESKeyGenerator;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion

    #region FlatRSASecurity
    /// <summary>
    /// FlatRngSecurity class implementation
    /// </summary>
    public class FlatRSASecurity
    {
        public bool IsDirty { get; set; }
        public int CertificateValidity { get; set; }
        public bool CertificatePerUser { get; set; }
        public string CertificateThumbprint { get; set; }


        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            KeysManagerConfig keys = cfg.KeysConfig;
            IsDirty = cfg.IsDirty;
            CertificateValidity = keys.CertificateValidity;
            CertificatePerUser = keys.CertificatePerUser;
            CertificateThumbprint = keys.CertificateThumbprint;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            KeysManagerConfig keys = cfg.KeysConfig;
            ADDSHost adds = cfg.Hosts.ActiveDirectoryHost;
            cfg.IsDirty = true;
            keys.CertificateValidity = CertificateValidity;
            keys.CertificatePerUser = CertificatePerUser;
            keys.CertificateThumbprint = CertificateThumbprint;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion

    #region FlatBiometricSecurity
    /// <summary>
    /// FlatBiometricSecurity class implementation
    /// </summary>
    public class FlatBiometricSecurity 
    {
        public bool IsDirty { get; set; }
        public FlatAuthenticatorAttachmentKind AuthenticatorAttachment { get; set; }
        public FlatAttestationConveyancePreferenceKind AttestationConveyancePreference { get; set; }
        public FlatUserVerificationRequirementKind UserVerificationRequirement { get; set; }
        public bool Extensions { get; set; }
        public bool UserVerificationIndex { get; set; }
        public bool Location { get; set; }
        public bool UserVerificationMethod { get; set; }
        public bool RequireResidentKey { get; set; }
        public bool? HmacSecret { get; set; }
        public WebAuthNUserVerification? CredProtect { get; set; }
        public bool? EnforceCredProtect { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            WebAuthNProvider otp = cfg.WebAuthNProvider;
            this.IsDirty = cfg.IsDirty;
            this.AuthenticatorAttachment = otp.Options.AuthenticatorAttachment.FromAuthenticatorAttachmentValue();
            this.AttestationConveyancePreference = otp.Options.AttestationConveyancePreference.FromAttestationConveyancePreferenceValue();
            this.UserVerificationRequirement = otp.Options.UserVerificationRequirement.FromUserVerificationRequirementValue();
            this.Extensions = otp.Options.Extensions;
            this.UserVerificationIndex = otp.Options.UserVerificationIndex;
            this.Location = otp.Options.Location;
            this.UserVerificationMethod = otp.Options.UserVerificationMethod;
            this.RequireResidentKey = otp.Options.RequireResidentKey;
            this.HmacSecret = otp.Options.HmacSecret;
            this.CredProtect = otp.Options.CredProtect;
            this.EnforceCredProtect = otp.Options.EnforceCredProtect;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            WebAuthNProvider otp = cfg.WebAuthNProvider;
            cfg.IsDirty = true;
            otp.Options.AuthenticatorAttachment = this.AuthenticatorAttachment.ToAuthenticatorAttachmentValue();
            otp.Options.AttestationConveyancePreference = this.AttestationConveyancePreference.ToAttestationConveyancePreferenceValue();
            otp.Options.UserVerificationRequirement = this.UserVerificationRequirement.ToUserVerificationRequirementValue();
            otp.Options.Extensions = this.Extensions;
            otp.Options.UserVerificationIndex = this.UserVerificationIndex;
            otp.Options.Location = this.Location;
            otp.Options.UserVerificationMethod = this.UserVerificationMethod;
            otp.Options.RequireResidentKey = this.RequireResidentKey;
            otp.Options.HmacSecret = this.HmacSecret;
            otp.Options.CredProtect = this.CredProtect;
            otp.Options.EnforceCredProtect = this.EnforceCredProtect;

            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion 

    #region FlatWsManSecurity
    /// <summary>
    /// FlatWsManSecurity class implementation
    /// </summary>
    public class FlatWsManSecurity
    {
        public bool IsDirty { get; set; }
        public int Port { get; set; }
        public string AppName { get; set; }
        public string ShellUri { get; set; }
        public int TimeOut { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            ADFSWSManager otp = cfg.WsMan;
            this.IsDirty = cfg.IsDirty;
            this.Port = otp.Port;
            this.AppName = otp.AppName;
            this.ShellUri = otp.ShellUri;
            this.TimeOut = otp.TimeOut;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            ADFSWSManager otp = cfg.WsMan;
            cfg.IsDirty = true;
            otp.Port = this.Port;
            otp.AppName = this.AppName;
            otp.ShellUri = this.ShellUri;
            otp.TimeOut = this.TimeOut;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion 

    #region FlatBaseProvider
    /// <summary>
    /// FlatBaseProvider class implementation
    /// </summary>
    public abstract class FlatBaseProvider
    {
        public bool IsDirty { get; set; }
        public bool Enabled { get; set; }
        public bool IsRequired { get; set; }
        public bool EnrollWizard { get; set; }
        public bool PinRequired { get; set; }
        public ForceWizardMode ForceWizard { get; set; }
        public string FullyQualifiedImplementation { get; set; }
        public string Parameters { get; set; }

        public abstract PreferredMethod Kind { get; }
        public abstract void Load(PSHost host);
        public abstract void Update(PSHost host);

        /// <summary>
        /// CkeckUpdate method implementation
        /// </summary>
        public virtual void CheckUpdates(PSHost host)
        {
            IExternalProvider prov = RuntimeAuthProvider.GetProviderInstance(Kind);
            if (prov == null)
                return;
            if (prov.Name.Equals("Neos.Provider.Plug.External"))
                return;
            if (prov != null)
            {
                if ((!prov.AllowDisable) && (!this.Enabled))
                    throw new Exception("This Provider cannot be Disabled !");
                if ((!prov.AllowEnrollment) && (this.EnrollWizard)) 
                    throw new Exception("This Provider do not support Wizards !");
            }
        }
    }
    #endregion

    #region FlatTOTPProvider
    /// <summary>
    /// FlatOTPProvider class implementation
    /// </summary>
    public class FlatTOTPProvider: FlatBaseProvider
    {
        public HashMode Algorithm { get; set; }
        public int TOTPShadows { get; set; }
        public int Digits { get; set; }
        public int Duration { get; set; }
        public OTPWizardOptions WizardOptions { get; set; }
        public KeySizeMode KeySize { get; set; }
        public SecretKeyFormat KeysFormat { get; set; }

        /// <summary>
        /// Kind  Property
        /// </summary>
        public override PreferredMethod Kind 
        { 
            get 
            { 
                return PreferredMethod.Code; 
            } 
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public override void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            OTPProvider otp = cfg.OTPProvider;
            this.IsDirty = cfg.IsDirty;            
            this.Enabled = otp.Enabled;
            this.IsRequired = otp.IsRequired;
            this.EnrollWizard = otp.EnrollWizard;
            this.ForceWizard = otp.ForceWizard;
            this.Algorithm = otp.Algorithm;
            this.Digits = otp.TOTPDigits;
            this.Duration = otp.TOTPDuration;
            this.TOTPShadows = otp.TOTPShadows;
            this.WizardOptions = otp.WizardOptions;
            this.KeySize = cfg.KeysConfig.KeySize;
            this.KeysFormat = cfg.KeysConfig.KeyFormat;
            this.PinRequired = otp.PinRequired;
            this.FullyQualifiedImplementation = otp.FullQualifiedImplementation;
            this.Parameters = otp.Parameters.Data;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public override void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            OTPProvider otp = cfg.OTPProvider;
            cfg.IsDirty = true;
            CheckUpdates(host);
            otp.Enabled = this.Enabled;
            otp.IsRequired = this.IsRequired;
            otp.EnrollWizard = this.EnrollWizard;
            otp.ForceWizard = this.ForceWizard;
            otp.Algorithm = this.Algorithm;
            otp.TOTPShadows = this.TOTPShadows;
            otp.TOTPDigits = this.Digits;
            otp.TOTPDuration = this.Duration;
            otp.WizardOptions = this.WizardOptions;
            cfg.KeysConfig.KeySize = this.KeySize;
            cfg.KeysConfig.KeyFormat = this.KeysFormat;
            otp.PinRequired = this.PinRequired;
            otp.FullQualifiedImplementation = this.FullyQualifiedImplementation;
            otp.Parameters.Data = this.Parameters;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion

    #region FlatMailProvider
    /// <summary>
    /// FlatMailProvider class implementation
    /// </summary>
    public class FlatMailProvider : FlatBaseProvider
    {
        public string From { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string Company { get; set; }
        public bool Anonymous { get; set; }
        public bool DeliveryNotifications { get; set; }
        public FlatMailAllowedDomains AllowedDomains { get; set; }
        public FlatMailBlockedDomains BlockedDomains { get; set; }
        public List<FlatMailFileName> MailOTPContent { get; set; }
        public List<FlatMailFileName> MailAdminContent { get; set; }
        public List<FlatMailFileName> MailKeyContent { get; set; }
        public List<FlatMailFileName> MailNotifications { get; set; }

        public FlatMailProvider()
        {
            this.MailOTPContent = new List<FlatMailFileName>();
            this.MailAdminContent = new List<FlatMailFileName>();
            this.MailKeyContent = new List<FlatMailFileName>();
            this.MailNotifications = new List<FlatMailFileName>();
            this.BlockedDomains = new FlatMailBlockedDomains();
            this.AllowedDomains = new FlatMailAllowedDomains();
        }

        /// <summary>
        /// Kind  Property
        /// </summary>
        public override PreferredMethod Kind
        {
            get
            {
                return PreferredMethod.Email;
            }
        }


        /// <summary>
        /// Update method implmentation
        /// </summary>
        public override void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            MailProvider mail = cfg.MailProvider;
            IsDirty = cfg.IsDirty;
            Enabled = mail.Enabled;
            EnrollWizard = mail.EnrollWizard;
            ForceWizard = mail.ForceWizard;
            From = mail.From;
            UserName = mail.UserName;
            using (AESSystemEncryption MSIS = new AESSystemEncryption())
            {
                Password = MSIS.Encrypt(mail.Password);
            };
            Host = mail.Host;
            Port = mail.Port;
            UseSSL = mail.UseSSL;
            Company = mail.Company;
            PinRequired = mail.PinRequired;
            Anonymous = mail.Anonymous;
            DeliveryNotifications = mail.DeliveryNotifications;
            FullyQualifiedImplementation = mail.FullQualifiedImplementation;
            Parameters = mail.Parameters.Data;

            AllowedDomains.Clear();
            foreach (string itm in mail.AllowedDomains)
            {
                AllowedDomains.AddDomain(itm);
            }

            BlockedDomains.Clear();
            foreach (string itm in mail.BlockedDomains)
            {
                BlockedDomains.AddDomain(itm);
            }

            MailOTPContent.Clear();
            foreach (SendMailFileName itm in mail.MailOTPContent)
            {
                MailOTPContent.Add((FlatMailFileName)itm);
            }
            MailAdminContent.Clear();
            foreach (SendMailFileName itm in mail.MailAdminContent)
            {
                MailAdminContent.Add((FlatMailFileName)itm);
            }
            MailKeyContent.Clear();
            foreach (SendMailFileName itm in mail.MailKeyContent)
            {
                MailKeyContent.Add((FlatMailFileName)itm);
            }
            MailNotifications.Clear();
            foreach (SendMailFileName itm in mail.MailNotifications)
            {
                MailNotifications.Add((FlatMailFileName)itm);
            }
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public override void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            MailProvider mail = cfg.MailProvider;
            cfg.IsDirty = true;
            CheckUpdates(host);
            mail.Enabled = Enabled;
            mail.EnrollWizard = EnrollWizard;
            mail.ForceWizard = ForceWizard;
            mail.From = From;
            mail.UserName = UserName;
            using (AESSystemEncryption MSIS = new AESSystemEncryption())
            {
                mail.Password = MSIS.Decrypt(Password);
            };
            mail.Host = Host;
            mail.Port = Port;
            mail.UseSSL = UseSSL;
            mail.Company = Company;
            mail.PinRequired = PinRequired;
            mail.Anonymous = Anonymous;
            mail.DeliveryNotifications = DeliveryNotifications;
            mail.FullQualifiedImplementation = FullyQualifiedImplementation;
            mail.Parameters.Data = Parameters;

            mail.AllowedDomains.Clear();
            foreach (string itm in AllowedDomains.Domains)
            {
                mail.AllowedDomains.Add(itm);
            }

            mail.BlockedDomains.Clear();
            foreach (string itm in BlockedDomains.Domains)
            {
                mail.BlockedDomains.Add(itm);
            }

            mail.MailOTPContent.Clear();
            foreach (FlatMailFileName itm in MailOTPContent)
            {
                mail.MailOTPContent.Add((SendMailFileName)itm);
            }
            mail.MailAdminContent.Clear();
            foreach (FlatMailFileName itm in MailAdminContent)
            {
                mail.MailAdminContent.Add((SendMailFileName)itm);
            }
            mail.MailKeyContent.Clear();
            foreach (FlatMailFileName itm in MailKeyContent)
            {
                mail.MailKeyContent.Add((SendMailFileName)itm);
            }
            mail.MailNotifications.Clear();
            foreach (FlatMailFileName itm in MailNotifications)
            {
                mail.MailNotifications.Add((SendMailFileName)itm);
            }

            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion

    #region FlatExternalProvider
    /// <summary>
    /// FlatExternalProvider class implementation
    /// </summary>
    public class FlatExternalProvider: FlatBaseProvider
    {
        public string Company { get; set; }
        public string Sha1Salt { get; set; }
        public bool IsTwoWay  { get; set; }
        public int Timeout  { get; set; }

        /// <summary>
        /// Kind  Property
        /// </summary>
        public override PreferredMethod Kind
        {
            get
            {
                return PreferredMethod.External;
            }
        }


        /// <summary>
        /// Update method implmentation
        /// </summary>
        public override void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            ExternalOTPProvider otp = cfg.ExternalProvider;
            this.IsDirty = cfg.IsDirty;
            this.Enabled = otp.Enabled;
            this.IsRequired = otp.IsRequired;
            this.EnrollWizard = otp.EnrollWizard;
            this.ForceWizard = otp.ForceWizard;
            this.Company = otp.Company;
            this.FullyQualifiedImplementation = otp.FullQualifiedImplementation;
            this.IsTwoWay = otp.IsTwoWay;
            this.Sha1Salt = otp.Sha1Salt;
            this.Timeout = otp.Timeout;
            this.PinRequired = otp.PinRequired;
            this.Parameters = otp.Parameters.Data;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public override void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            ExternalOTPProvider otp = cfg.ExternalProvider;
            cfg.IsDirty = true;
            CheckUpdates(host);
            otp.Enabled = this.Enabled;
            otp.IsRequired = this.IsRequired;
            otp.EnrollWizard = this.EnrollWizard;
            otp.ForceWizard = this.ForceWizard;
            otp.Company = this.Company;
            otp.FullQualifiedImplementation = this.FullyQualifiedImplementation;
            otp.IsTwoWay = this.IsTwoWay;
            otp.Sha1Salt = this.Sha1Salt;
            otp.Timeout = this.Timeout;
            otp.Parameters.Data = this.Parameters;
            otp.PinRequired = this.PinRequired;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion

    #region FlatAzureProvider
    /// <summary>
    /// FlatAzureProvider class implementation
    /// </summary>
    public class FlatAzureProvider: FlatBaseProvider
    {
        public string TenantId { get; set; }
        public string ThumbPrint { get; set; }

        /// <summary>
        /// Kind  Property
        /// </summary>
        public override PreferredMethod Kind
        {
            get
            {
                return PreferredMethod.Azure;
            }
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public override void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            AzureProvider otp = cfg.AzureProvider;
            this.IsDirty = cfg.IsDirty;
            this.Enabled = otp.Enabled;
            IsRequired = otp.IsRequired;
            this.EnrollWizard = false;
            this.ForceWizard = ForceWizardMode.Disabled;
            this.TenantId = otp.TenantId;
            this.ThumbPrint = otp.ThumbPrint;
            this.PinRequired = otp.PinRequired;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public override void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            AzureProvider otp = cfg.AzureProvider;
            cfg.IsDirty = true;
            CheckUpdates(host);
            otp.Enabled = Enabled;
            otp.EnrollWizard = false;
            otp.IsRequired = IsRequired;
            otp.ForceWizard = ForceWizardMode.Disabled;
            otp.TenantId = this.TenantId;
            otp.ThumbPrint = this.ThumbPrint;
            otp.PinRequired = this.PinRequired;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion

    #region FlatBiometricProvider
    /// <summary>
    /// FlatBiometricProvider class implementation
    /// </summary>
    public class FlatBiometricProvider : FlatBaseProvider
    {
        public uint Timeout { get; set; }
        public int TimestampDriftTolerance { get; set; }
        public int ChallengeSize { get; set; }
        public string ServerDomain { get; set; }
        public string ServerName { get; set; }
        public string ServerIcon { get; set; }
        public string Origin { get; set; }
        public bool DirectLogin { get; set; }
        public bool RequireValidAttestationRoot { get; set; }
        public bool ShowPII { get; set; }

        /// <summary>
        /// Kind  Property
        /// </summary>
        public override PreferredMethod Kind
        {
            get
            {
                return PreferredMethod.Biometrics;
            }
        }

        public WebAuthNPinRequirements PinRequirements { get; set; }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public override void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            WebAuthNProvider otp = cfg.WebAuthNProvider;
            this.IsDirty = cfg.IsDirty;
            this.Enabled = otp.Enabled;
            this.IsRequired = otp.IsRequired;
            this.EnrollWizard = otp.EnrollWizard;
            this.ForceWizard = otp.ForceWizard;
            this.DirectLogin = otp.DirectLogin;
            this.PinRequired = otp.PinRequired;
            this.PinRequirements = otp.PinRequirements;
            this.FullyQualifiedImplementation = otp.FullQualifiedImplementation;
            this.Parameters = otp.Parameters.Data;

            this.Timeout = otp.Configuration.Timeout;
            this.TimestampDriftTolerance = otp.Configuration.TimestampDriftTolerance;
            this.ChallengeSize = otp.Configuration.ChallengeSize;
            this.ServerDomain = otp.Configuration.ServerDomain;
            this.ServerName = otp.Configuration.ServerName;
            this.ServerIcon = otp.Configuration.ServerIcon;
            this.Origin = otp.Configuration.Origin;
            this.RequireValidAttestationRoot = otp.Configuration.RequireValidAttestationRoot;
            this.ShowPII = otp.Configuration.ShowPII;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public override void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            WebAuthNProvider otp = cfg.WebAuthNProvider;
            cfg.IsDirty = true;
            CheckUpdates(host);
            otp.Enabled = this.Enabled;
            otp.EnrollWizard = this.EnrollWizard;
            otp.ForceWizard = this.ForceWizard;
            otp.IsRequired = this.IsRequired;
            otp.PinRequirements = this.PinRequirements;
            otp.DirectLogin = this.DirectLogin;
            otp.PinRequired = this.PinRequired;
            otp.FullQualifiedImplementation = this.FullyQualifiedImplementation;
            otp.Parameters.Data = this.Parameters;

            otp.Configuration.Timeout = this.Timeout;
            otp.Configuration.TimestampDriftTolerance = this.TimestampDriftTolerance;
            otp.Configuration.ChallengeSize = this.ChallengeSize;
            otp.Configuration.ServerDomain = this.ServerDomain;
            otp.Configuration.ServerName = this.ServerName;
            otp.Configuration.ServerIcon = this.ServerIcon;
            otp.Configuration.Origin = this.Origin;
            otp.Configuration.RequireValidAttestationRoot = this.RequireValidAttestationRoot;
            otp.Configuration.ShowPII = this.ShowPII;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }
    #endregion 

    #region Biometrics Provider Classes
    /// <summary>
    /// FlatAuthenticatorAttachmentKind
    /// </summary>   
    public enum FlatAuthenticatorAttachmentKind
    {
        Empty = 0,
        Platform = 1,
        CrossPlatform = 2
    }

    /// <summary>
    /// FlatAttestationConveyancePreferenceKind
    /// </summary>   
    public enum FlatAttestationConveyancePreferenceKind
    {
        None = 0,
        Direct = 1,
        Indirect = 2
    }

    /// <summary>
    /// FlatUserVerificationRequirementKind
    /// </summary>      
    public enum FlatUserVerificationRequirementKind
    {
        Preferred = 0,
        Required = 1,
        Discouraged = 2
    }
    #endregion

    #region Extension methods for convertion
    /// <summary>
    /// FlatWebAuthNExtensions
    /// </summary>   
    public static class FlatWebAuthNExtensions
    {
        /// <summary>
        /// ToAuthenticatorAttachmentValue method implementation
        /// </summary>
        public static string ToAuthenticatorAttachmentValue(this FlatAuthenticatorAttachmentKind type)
        {
            switch (type)
            {
                case FlatAuthenticatorAttachmentKind.Empty:
                    return string.Empty;
                case FlatAuthenticatorAttachmentKind.Platform:
                    return "platform";
                case FlatAuthenticatorAttachmentKind.CrossPlatform:
                    return "cross-platform";
                default:
                    return "platform";
            }
        }

        /// <summary>
        /// ToAttestationConveyancePreferenceValue method implementation
        /// </summary>
        public static string ToAttestationConveyancePreferenceValue(this FlatAttestationConveyancePreferenceKind type)
        {
            switch (type)
            {
                case FlatAttestationConveyancePreferenceKind.None:
                    return "none";
                case FlatAttestationConveyancePreferenceKind.Direct:
                    return "direct";
                case FlatAttestationConveyancePreferenceKind.Indirect:
                    return "indirect";
                default:
                    return "direct";
            }
        }

        /// <summary>
        /// ToUserVerificationRequirementValue method implementation
        /// </summary>
        public static string ToUserVerificationRequirementValue(this FlatUserVerificationRequirementKind type)
        {
            switch (type)
            {
                case FlatUserVerificationRequirementKind.Preferred:
                    return "preferred";
                case FlatUserVerificationRequirementKind.Required:
                    return "required";
                case FlatUserVerificationRequirementKind.Discouraged:
                    return "discouraged";
                default:
                    return "preferred";
            }
        }


        /// <summary>
        /// FromAuthenticatorAttachmentValue method implementation
        /// </summary>
        public static FlatAuthenticatorAttachmentKind FromAuthenticatorAttachmentValue(this string type)
        {
            switch (type.ToLower())
            {
                case "":
                    return FlatAuthenticatorAttachmentKind.Empty;
                case "platform":
                    return FlatAuthenticatorAttachmentKind.Platform;
                case "cross-platform":
                    return FlatAuthenticatorAttachmentKind.CrossPlatform;
                default:
                    return FlatAuthenticatorAttachmentKind.Platform;
            }
        }

        /// <summary>
        /// FromAttestationConveyancePreferenceValue method implementation
        /// </summary>
        public static FlatAttestationConveyancePreferenceKind FromAttestationConveyancePreferenceValue(this string type)
        {
            switch (type.ToLower())
            {
                case "none":
                    return FlatAttestationConveyancePreferenceKind.None;
                case "direct":
                    return FlatAttestationConveyancePreferenceKind.Direct;
                case "indirect":
                    return FlatAttestationConveyancePreferenceKind.Indirect;
                default:
                    return FlatAttestationConveyancePreferenceKind.Direct;
            }
        }

        /// <summary>
        /// FromAttestationConveyancePreferenceValue method implementation
        /// </summary>
        public static FlatUserVerificationRequirementKind FromUserVerificationRequirementValue(this string type)
        {
            switch (type.ToLower())
            {
                case "preferred":
                    return FlatUserVerificationRequirementKind.Preferred;
                case "required":
                    return FlatUserVerificationRequirementKind.Required;
                case "discouraged":
                    return FlatUserVerificationRequirementKind.Discouraged;
                default:
                    return FlatUserVerificationRequirementKind.Preferred;
            }
        }
    }
    #endregion

    #region Mail Provider FileNames and Domains
    /// <summary>
    /// FlatMailBlockedDomains class implementation
    /// </summary>      
    public class FlatMailBlockedDomains
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
        /// Clear method implmentation
        /// </summary>
        public void Clear()
        {
            _list.Clear();
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

    /// <summary>
    /// FlatMailAllowedDomains class implementation
    /// </summary>      
    public class FlatMailAllowedDomains
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
        /// Clear method implmentation
        /// </summary>
        public void Clear()
        {
            _list.Clear();
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

    /// <summary>
    /// FlatMailFileName class implementation
    /// </summary>      
    public class FlatMailFileName
    {
        public int LCID { get; set; }
        public string FileName { get; set; }
        public bool Enabled { get; set; }

        public FlatMailFileName(int lcid, string filename, bool enabled = true)
        {
            this.LCID = lcid;
            this.FileName = filename;
            this.Enabled = enabled;
        }

        public static explicit operator SendMailFileName(FlatMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new SendMailFileName(file.LCID, file.FileName, file.Enabled);
        }

        public static explicit operator FlatMailFileName(SendMailFileName file)
        {
            if (file == null)
                return null;
            else
                return new FlatMailFileName(file.LCID, file.FileName, file.Enabled);
        }
    }
    #endregion

    #region Config Advertising Days
    /// <summary>
    /// FlatAdvertising class implementation
    /// </summary>
    public class FlatAdvertising
    {
        public uint FirstDay { get; set; }
        public uint LastDay { get; set; }
        public bool OnFire { get; set; }

        public static explicit operator ConfigAdvertising(FlatAdvertising adv)
        {
            ConfigAdvertising cfg = new ConfigAdvertising
            {
                FirstDay = adv.FirstDay,
                LastDay = adv.LastDay
            };
            return cfg;
        }

        public static explicit operator FlatAdvertising(ConfigAdvertising adv)
        {
            FlatAdvertising cfg = new FlatAdvertising
            {
                FirstDay = adv.FirstDay,
                LastDay = adv.LastDay,
                OnFire = adv.OnFire
            };
            return cfg;
        }
    }
    #endregion
}
