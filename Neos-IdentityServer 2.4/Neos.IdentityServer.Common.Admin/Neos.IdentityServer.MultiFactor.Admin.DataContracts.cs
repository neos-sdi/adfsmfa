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
        /// SetMFAMixed method implementation
        /// </summary>
        public static UserFeaturesOptions SetMFAMixed(this UserFeaturesOptions options)
        {
            options = options.Remove(UserFeaturesOptions.AllowUnRegistered);
            options = options.Remove(UserFeaturesOptions.BypassUnRegistered);
            options = options.Remove(UserFeaturesOptions.AllowDisabled);
            options = options.Remove(UserFeaturesOptions.BypassDisabled);
            options = options.Remove(UserFeaturesOptions.AdministrativeMode);
            options = options.Add(UserFeaturesOptions.AllowProvideInformations);   // Allow only provide informations
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
            options = options.Add(UserFeaturesOptions.AdministrativeMode); // Admin only
            return options;
        }

        /// <summary>
        /// SetSelfRegistration method implementation
        /// </summary>
        public static UserFeaturesOptions SetSelfRegistration(this UserFeaturesOptions options)
        {
            options = options.Remove(UserFeaturesOptions.BypassUnRegistered);
            options = options.Remove(UserFeaturesOptions.AllowProvideInformations);
            options = options.Remove(UserFeaturesOptions.AdministrativeMode);
            options = options.Add(UserFeaturesOptions.AllowUnRegistered);    // Allow User to register
            return options;
        }

        /// <summary>
        /// SetMixedRegistration method implementation
        /// </summary>
        public static UserFeaturesOptions SetMixedRegistration(this UserFeaturesOptions options)
        {
            options = options.Remove(UserFeaturesOptions.BypassUnRegistered);
            options = options.Remove(UserFeaturesOptions.AllowUnRegistered);
            options = options.Remove(UserFeaturesOptions.AdministrativeMode);
            options = options.Add(UserFeaturesOptions.AllowProvideInformations);    // Allow User to register
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
            if (options == (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment))
                return UserTemplateMode.Free;
            else if (options == (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment))
                return UserTemplateMode.Open;
            else if (options == (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment))
                return UserTemplateMode.Default;
            else if (options == (UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment))
                return UserTemplateMode.Mixed;
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
                    options = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment);
                    break;
                case UserTemplateMode.Open:
                    options = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment);
                    break;
                case UserTemplateMode.Default:
                    options = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment);
                    break;
                case UserTemplateMode.Mixed:
                    options = (UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment);
                    break;
                case UserTemplateMode.Managed:
                    options = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword);
                    break;
                case UserTemplateMode.Strict:
                    options = (UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AdministrativeMode);
                    break;
                case UserTemplateMode.Administrative:
                    options = (UserFeaturesOptions.AdministrativeMode);
                    break;
                default:
                    options = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered);
                    break;
            }
            return options;
        }
        #endregion
    }
   
    [Serializable]
    public class FlatConfig
    {
        public bool UseUIPaginated;
        public bool IsDirty { get; set; }
        public int DeliveryWindow { get; set; }
        public int MaxRetries { get; set; }
        public int PinLength { get; set; }
        public int DefaultPin { get; set; }
        public string Issuer { get; set; }
        public bool UseActiveDirectory { get; set; }
        public bool CustomUpdatePassword { get; set; }
        public bool KeepMySelectedOptionOn { get; set; }
        public bool ChangeNotificationsOn { get; set; }                        
        public string DefaultCountryCode { get; set; }
        public string AdminContact { get; set; }
        public UserFeaturesOptions UserFeatures { get; set; }
        public ConfigAdvertising AdvertisingDays { get; set; }
        public ADFSUserInterfaceKind UiKind { get; set; }

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
            MaxRetries = cfg.MaxRetries;
            DefaultPin = cfg.DefaultPin;
            PinLength = cfg.PinLength;
            Issuer = cfg.Issuer;
            UseActiveDirectory = cfg.UseActiveDirectory;
            CustomUpdatePassword = cfg.CustomUpdatePassword;
            DefaultCountryCode = cfg.DefaultCountryCode;
            KeepMySelectedOptionOn = cfg.KeepMySelectedOptionOn;
            ChangeNotificationsOn = cfg.ChangeNotificationsOn;
            AdminContact = cfg.AdminContact;
            UserFeatures = cfg.UserFeatures;
            AdvertisingDays = cfg.AdvertisingDays;
            UseUIPaginated = cfg.UseUIPaginated;
            UiKind = cfg.UiKind;
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
            cfg.MaxRetries = MaxRetries;
            cfg.DefaultPin = DefaultPin;
            cfg.PinLength = PinLength;
            cfg.Issuer = Issuer;
            cfg.UseActiveDirectory = UseActiveDirectory;
            cfg.CustomUpdatePassword = CustomUpdatePassword;
            cfg.KeepMySelectedOptionOn = KeepMySelectedOptionOn;
            cfg.ChangeNotificationsOn = ChangeNotificationsOn;
            cfg.DefaultCountryCode = DefaultCountryCode;
            cfg.AdminContact = AdminContact;
            cfg.UserFeatures = UserFeatures;
            cfg.AdvertisingDays = AdvertisingDays;
            cfg.UiKind = UiKind;
            cfg.UseUIPaginated = UseUIPaginated;
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
                    cfg.UserFeatures = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment);
                    break;
                case FlatTemplateMode.Open:
                    cfg.UserFeatures = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment);
                    break;
                case FlatTemplateMode.Default:
                    cfg.UserFeatures = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment);
                    break;
                case FlatTemplateMode.Managed:
                    cfg.UserFeatures = (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword);
                    break;
                case FlatTemplateMode.Strict:
                    cfg.UserFeatures = (UserFeaturesOptions.AdministrativeMode | UserFeaturesOptions.AllowProvideInformations);
                    break;
                case FlatTemplateMode.Administrative:
                    cfg.UserFeatures = (UserFeaturesOptions.AdministrativeMode);
                    break;
                default:
                    cfg.UserFeatures = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered);
                    break;
            }
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
    }

    [Serializable]
    public class FlatConfigSQL
    {
        public string KeyName { get; set; }
        public int CertificateValidity { get; set; }
        public bool IsDirty { get; set; }
        public string ConnectionString { get; set; }
        public int MaxRows { get; set; }
        public bool IsAlwaysEncrypted { get; set; }
        public string ThumbPrint { get; set; }
        public bool CertReuse { get; set; }

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
            MaxRows = sql.MaxRows;
            IsAlwaysEncrypted = sql.IsAlwaysEncrypted;
            ThumbPrint = sql.ThumbPrint;
            KeyName = sql.KeyName;
            CertReuse = sql.CertReuse;
            CertificateValidity = sql.CertificateValidity;
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
            sql.ConnectionString = ConnectionString;
            sql.MaxRows = MaxRows;
            sql.IsAlwaysEncrypted = IsAlwaysEncrypted;
            sql.ThumbPrint = ThumbPrint;
            sql.KeyName = KeyName;
            sql.CertReuse = CertReuse;
            sql.CertificateValidity = CertificateValidity;
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
        public string PinAttribute { get; set; }
        public string EnabledAttribute { get; set; }
        public int MaxRows { get; set; }

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
            PinAttribute = adds.pinattribute;
            EnabledAttribute = adds.totpEnabledAttribute;
            MaxRows = adds.MaxRows;
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
            if (!ManagementService.CheckADDSAttribute(DomainAddress, Account, Password, KeyAttribute, true))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", KeyAttribute));
            if (!ManagementService.CheckADDSAttribute(DomainAddress, Account, Password, MailAttribute, true))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", MailAttribute));
            if (!ManagementService.CheckADDSAttribute(DomainAddress, Account, Password, PhoneAttribute, true))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", PhoneAttribute));
            if (!ManagementService.CheckADDSAttribute(DomainAddress, Account, Password, MethodAttribute, false))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", MethodAttribute));
            if (!ManagementService.CheckADDSAttribute(DomainAddress, Account, Password, OverrideMethodAttribute, false))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", OverrideMethodAttribute));
            if (!ManagementService.CheckADDSAttribute(DomainAddress, Account, Password, PinAttribute, false))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", PinAttribute));
            if (!ManagementService.CheckADDSAttribute(DomainAddress, Account, Password, EnabledAttribute, false))
                throw new ArgumentException(string.Format("Attribute {0} not found in forest schema !", EnabledAttribute));
            adds.Account = Account;
            adds.Password = Password;
            adds.DomainAddress = DomainAddress;
            adds.keyAttribute = KeyAttribute;
            adds.mailAttribute = MailAttribute;
            adds.phoneAttribute = PhoneAttribute;
            adds.methodAttribute = MethodAttribute;
            adds.overridemethodAttribute = OverrideMethodAttribute;
            PinAttribute = adds.pinattribute;
            adds.totpEnabledAttribute = EnabledAttribute;
            adds.MaxRows = MaxRows;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }

    [Serializable]
    public class FlatKeysConfig
    {
        public bool IsDirty { get; set; }
        public KeyGeneratorMode KeyGenerator  { get; set; }
        public KeySizeMode KeySize { get; set; }
        public SecretKeyFormat KeysFormat  { get; set; }
        public SecretKeyVersion LibVersion { get; set; }
        public string CertificateThumbprint  { get; set; }
        public int CertificateValidity  { get; set; }
        public FlatExternalKeyManager ExternalKeyManager  { get; set; }
        public string XORSecret { get; set; }

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
            this.KeysFormat = keys.KeyFormat;
            this.KeyGenerator = keys.KeyGenerator;
            this.LibVersion = keys.KeyVersion;
            this.KeySize = keys.KeySize;
            this.XORSecret = keys.XORSecret;
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
            keys.KeyFormat = this.KeysFormat;
            keys.KeyGenerator = this.KeyGenerator;
            keys.KeyVersion = this.LibVersion;
            keys.KeySize = this.KeySize;
            keys.XORSecret = this.XORSecret;
            keys.ExternalKeyManager = (ExternalKeyManagerConfig)this.ExternalKeyManager;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }

    [Serializable]
    public class FlatExternalKeyManager
    {
        public string FullQualifiedImplementation { get; set; }
        public string ConnectionString { get; set; }
        public bool IsAlwaysEncrypted { get; set; }
        public string KeyName { get; set; }
        public string ThumbPrint { get; set; }
        public int CertificateValidity { get; set; }
        public bool CertReuse { get; set; }
        public string Parameters { get; set; }

        public static explicit operator ExternalKeyManagerConfig(FlatExternalKeyManager mgr)
        {
            if (mgr == null)
                return null;
            else
            {
                ExternalKeyManagerConfig ret = new ExternalKeyManagerConfig();
                ret.FullQualifiedImplementation = mgr.FullQualifiedImplementation;
                ret.ConnectionString = mgr.ConnectionString;
                ret.IsAlwaysEncrypted = mgr.IsAlwaysEncrypted;
                ret.ThumbPrint = mgr.ThumbPrint;
                ret.KeyName = mgr.KeyName;
                ret.Parameters.Data = mgr.Parameters;
                ret.CertificateValidity = mgr.CertificateValidity;
                ret.CertReuse = mgr.CertReuse;
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
                ret.ConnectionString = mgr.ConnectionString;
                ret.IsAlwaysEncrypted = mgr.IsAlwaysEncrypted;
                ret.ThumbPrint = mgr.ThumbPrint;
                ret.KeyName = mgr.KeyName;
                ret.Parameters = mgr.Parameters.Data;
                ret.CertificateValidity = mgr.CertificateValidity;
                ret.CertReuse = mgr.CertReuse;
                return ret;
            }
        }

        public void Load(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            KeysManagerConfig otp = cfg.KeysConfig;
            this.FullQualifiedImplementation = otp.ExternalKeyManager.FullQualifiedImplementation;
            this.ThumbPrint = otp.ExternalKeyManager.ThumbPrint;
            this.KeyName = otp.ExternalKeyManager.KeyName;
            this.CertificateValidity = otp.ExternalKeyManager.CertificateValidity;
            this.CertReuse = otp.ExternalKeyManager.CertReuse;
            this.ConnectionString = otp.ExternalKeyManager.ConnectionString;
            this.IsAlwaysEncrypted = otp.ExternalKeyManager.IsAlwaysEncrypted;
            this.Parameters = otp.ExternalKeyManager.Parameters.Data;
        }

        /// <summary>
        /// Update method implmentation
        /// </summary>
        public void Update(PSHost host)
        {
            ManagementService.Initialize(host, true);
            MFAConfig cfg = ManagementService.Config;
            KeysManagerConfig otp = cfg.KeysConfig;
            otp.ExternalKeyManager.FullQualifiedImplementation = this.FullQualifiedImplementation;
            otp.ExternalKeyManager.ThumbPrint = this.ThumbPrint;
            otp.ExternalKeyManager.KeyName = this.KeyName;
            otp.CertificateValidity = this.CertificateValidity;
            otp.ExternalKeyManager.CertReuse = this.CertReuse;
            otp.ExternalKeyManager.ConnectionString = this.ConnectionString;
            otp.ExternalKeyManager.IsAlwaysEncrypted = this.IsAlwaysEncrypted;
            otp.ExternalKeyManager.Parameters.Data = this.Parameters;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }

    [Serializable]
    public abstract class FlatBaseProvider
    {
        private string _cdata;
        public bool IsDirty { get; set; }
        public bool Enabled { get; set; }
        public bool EnrollWizard { get; set; }
        public bool PinRequired { get; set; }
        public ForceWizardMode ForceWizard { get; set; }
        public string FullQualifiedImplementation { get; set; }

        /// <summary>
        /// Parameters property
        /// </summary>
        public string Parameters
        {
            get
            {
                return _cdata;
            }
            set
            {
                _cdata = value;
            }
        }

        public abstract PreferredMethod Kind { get; }
        public abstract void Load(PSHost host);
        public abstract void Update(PSHost host);

        /// <summary>
        /// CkeckUpdate method implementation
        /// </summary>
        public virtual void CheckUpdates(PSHost host)
        {
            IExternalProvider prov = RuntimeAuthProvider.GetProviderInstance(Kind);
            if (prov != null)
            {
                if ((!prov.AllowDisable) && (!this.Enabled))
                    throw new Exception("This Provider cannot be Disabled !");
                if ((!prov.AllowEnrollment) && (this.EnrollWizard)) 
                    throw new Exception("This Provider do not support Wizards !");
            }
        }
    }

    [Serializable]
    public class FlatOTPProvider: FlatBaseProvider
    {
        public HashMode Algorithm { get; set; }
        public int TOTPShadows { get; set; }
        public OTPWizardOptions WizardOptions { get; set; }

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
            this.EnrollWizard = otp.EnrollWizard;
            this.ForceWizard = otp.ForceWizard;
            this.Algorithm = otp.Algorithm;
            this.TOTPShadows = otp.TOTPShadows;
            this.WizardOptions = otp.WizardOptions;
            this.PinRequired = otp.PinRequired;
            this.FullQualifiedImplementation = otp.FullQualifiedImplementation;
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
            otp.EnrollWizard = this.EnrollWizard;
            otp.ForceWizard = this.ForceWizard;
            otp.Algorithm = this.Algorithm;
            otp.TOTPShadows = this.TOTPShadows;
            otp.WizardOptions = this.WizardOptions;
            otp.PinRequired = this.PinRequired;
            otp.FullQualifiedImplementation = this.FullQualifiedImplementation;
            otp.Parameters.Data = this.Parameters;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }

    [Serializable]
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
            this.EnrollWizard = otp.EnrollWizard;
            this.ForceWizard = otp.ForceWizard;
            this.Company = otp.Company;
            this.FullQualifiedImplementation = otp.FullQualifiedImplementation;
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
            otp.EnrollWizard = this.EnrollWizard;
            otp.ForceWizard = this.ForceWizard;
            otp.Company = this.Company;
            otp.FullQualifiedImplementation = this.FullQualifiedImplementation;
            otp.IsTwoWay = this.IsTwoWay;
            otp.Sha1Salt = this.Sha1Salt;
            otp.Timeout = this.Timeout;
            otp.Parameters.Data = this.Parameters;
            otp.PinRequired = this.PinRequired;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }

    [Serializable]
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
            this.EnrollWizard = otp.EnrollWizard;
            this.ForceWizard = otp.ForceWizard;
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
            otp.ForceWizard = ForceWizardMode.Disabled;
            otp.PinRequired = false;
            otp.TenantId = this.TenantId;
            otp.ThumbPrint = this.ThumbPrint;
            otp.PinRequired = this.PinRequired;
            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }

    [Serializable]
    public class FlatConfigMail: FlatBaseProvider
    {
        public string From { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string Company { get; set; }
        public bool Anonymous { get; set; }
        public FlatConfigMailBlockedDomains BlockedDomains { get; set; }
        public List<FlatConfigMailFileName> MailOTPContent { get; set; }
        public List<FlatConfigMailFileName> MailAdminContent { get; set; }
        public List<FlatConfigMailFileName> MailKeyContent { get; set; }
        public List<FlatConfigMailFileName> MailNotifications { get; set; }

        public FlatConfigMail()
        {
            this.MailOTPContent = new List<FlatConfigMailFileName>();
            this.MailAdminContent = new List<FlatConfigMailFileName>();
            this.MailKeyContent = new List<FlatConfigMailFileName>();
            this.MailNotifications = new List<FlatConfigMailFileName>();
            this.BlockedDomains = new FlatConfigMailBlockedDomains();
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
            Password = mail.Password;
            Host = mail.Host;
            Port = mail.Port;
            UseSSL = mail.UseSSL;
            Company = mail.Company;
            PinRequired = mail.PinRequired;
            Anonymous = mail.Anonymous;
            FullQualifiedImplementation = mail.FullQualifiedImplementation;
            Parameters = mail.Parameters.Data;

            BlockedDomains.Clear();
            foreach (string itm in mail.BlockedDomains)
            {
                BlockedDomains.AddDomain(itm);
            }

            MailOTPContent.Clear();
            foreach (SendMailFileName itm in mail.MailOTPContent)
            {
                MailOTPContent.Add((FlatConfigMailFileName)itm);
            }
            MailAdminContent.Clear();
            foreach (SendMailFileName itm in mail.MailAdminContent)
            {
                MailAdminContent.Add((FlatConfigMailFileName)itm);
            }
            MailKeyContent.Clear();
            foreach (SendMailFileName itm in mail.MailKeyContent)
            {
                MailKeyContent.Add((FlatConfigMailFileName)itm);
            }
            MailNotifications.Clear();
            foreach (SendMailFileName itm in mail.MailNotifications)
            {
                MailNotifications.Add((FlatConfigMailFileName)itm);
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
            mail.Password = Password;
            mail.Host = Host;
            mail.Port = Port;
            mail.UseSSL = UseSSL;
            mail.Company = Company;
            mail.PinRequired = PinRequired;
            mail.Anonymous = Anonymous;
            mail.FullQualifiedImplementation = FullQualifiedImplementation;
            mail.Parameters.Data = Parameters;

            mail.BlockedDomains.Clear();
            foreach (string itm in BlockedDomains.Domains)
            {
                mail.BlockedDomains.Add(itm);
            }

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
            mail.MailNotifications.Clear();
            foreach (FlatConfigMailFileName itm in MailNotifications)
            {
                mail.MailNotifications.Add((SendMailFileName)itm);
            }

            ManagementService.ADFSManager.WriteConfiguration(host);
        }
    }

    [Serializable]
    public class FlatConfigMailBlockedDomains
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
}
