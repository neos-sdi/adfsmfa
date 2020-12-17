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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Web;
using System.DirectoryServices.ActiveDirectory;
using Neos.IdentityServer.MultiFactor.Data;

namespace Neos.IdentityServer.MultiFactor
{
    #region MFAConfig
    public enum ReplayLevel
    {
        Disabled = 0,
        Intermediate = 1,
        Full = 2
    }

    public enum UserTemplateMode
    {

        Free = 0,                   // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment)
        Open = 1,                   // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment)
        Default = 2,                // (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment)
        Mixed = 3,                  // (UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment)
        Managed = 4,                // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword)
        Strict = 5,                 // (UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AdministrativeMode)
        Administrative = 6,         // (UserFeaturesOptions.AdministrativeMode);
        Custom = 7                  // Custom
    }

    [Flags]
    public enum UserFeaturesOptions
    {
        NoSet = 0,
        BypassUnRegistered = 1,
        BypassDisabled = 2,
        AllowUnRegistered = 4,
        AllowDisabled = 8,
        AllowChangePassword = 16,
        AllowManageOptions = 32,
        AllowProvideInformations = 64,
        AllowEnrollment = 128,
        AdministrativeMode = 256,
    }

    public static class UserFeaturesOptionsExtensions
    {
        /// <summary>
        /// IsAdministrative method implementation
        /// </summary>
        public static bool IsAdministrative(this UserFeaturesOptions options)
        {
            return options.HasFlag(UserFeaturesOptions.AdministrativeMode) && !options.HasFlag(UserFeaturesOptions.AllowProvideInformations);
        }

        /// <summary>
        /// IsStrict method implementation
        /// </summary>
        public static bool IsStrict(this UserFeaturesOptions options)
        {
            return options.HasFlag(UserFeaturesOptions.AdministrativeMode) && options.HasFlag(UserFeaturesOptions.AllowProvideInformations);
        }

        /// <summary>
        /// IsManaged method implementation
        /// </summary>
        public static bool IsManaged(this UserFeaturesOptions options)
        {
            return !options.HasFlag(UserFeaturesOptions.AdministrativeMode) && (options.HasFlag(UserFeaturesOptions.AllowProvideInformations) && options.HasFlag(UserFeaturesOptions.AllowUnRegistered));
        }

        /// <summary>
        /// IsMixed method implementation
        /// </summary>
        public static bool IsMixed(this UserFeaturesOptions options)
        {
            return (!options.HasFlag(UserFeaturesOptions.BypassDisabled) && !options.HasFlag(UserFeaturesOptions.AllowDisabled) && !options.HasFlag(UserFeaturesOptions.AllowProvideInformations) && !options.HasFlag(UserFeaturesOptions.AdministrativeMode));
        }


        #region MFA Enabled
        /// <summary>
        /// IsMFARequired method implementation
        /// </summary>
        public static bool IsMFARequired(this UserFeaturesOptions options)
        {
            return (!options.HasFlag(UserFeaturesOptions.AllowDisabled) && !options.HasFlag(UserFeaturesOptions.BypassDisabled));
            // return (options.HasFlag(UserFeaturesOptions.AdministrativeMode));
        }

        /// <summary>
        /// IsMFAAllowed method implementation
        /// </summary>
        public static bool IsMFAAllowed(this UserFeaturesOptions options)
        {
            return options.HasFlag(UserFeaturesOptions.AllowDisabled);
        }

        /// <summary>
        /// IsMFAMixed method implementation
        /// </summary>
        public static bool IsMFAMixed(this UserFeaturesOptions options)
        {
            return (!options.HasFlag(UserFeaturesOptions.BypassDisabled) && !options.HasFlag(UserFeaturesOptions.AllowDisabled) && !options.HasFlag(UserFeaturesOptions.AllowProvideInformations) && !options.HasFlag(UserFeaturesOptions.AdministrativeMode));
        }

        /// <summary>
        /// IsMFANotRequired method implementation
        /// </summary>
        public static bool IsMFANotRequired(this UserFeaturesOptions options)
        {
            return options.HasFlag(UserFeaturesOptions.BypassDisabled);
        }
        #endregion

        #region MFA Registration
        /// <summary>
        /// InformationsRequired method implementation
        /// </summary>
        public static bool InformationsRequired(this UserFeaturesOptions options)
        {
            return options.HasFlag(UserFeaturesOptions.AllowProvideInformations);
        }

        /// <summary>
        /// IsRegistraitonRequired method implementation
        /// </summary>
        public static bool IsRegistrationRequired(this UserFeaturesOptions options)
        {
            return (!options.HasFlag(UserFeaturesOptions.AllowUnRegistered) && !options.HasFlag(UserFeaturesOptions.BypassUnRegistered) && (options.HasFlag(UserFeaturesOptions.AllowProvideInformations)));
        }

        /// <summary>
        /// RegistrationAllowed method implementation
        /// </summary>
        public static bool IsRegistrationAllowed(this UserFeaturesOptions options)
        {
            return (options.HasFlag(UserFeaturesOptions.AllowUnRegistered) && (!options.HasFlag(UserFeaturesOptions.AllowProvideInformations) && !options.HasFlag(UserFeaturesOptions.AdministrativeMode)));
        }

        /// <summary>
        /// IsRegistrationMixed method implementation
        /// </summary>
        public static bool IsRegistrationMixed(this UserFeaturesOptions options)
        {
            return (!options.HasFlag(UserFeaturesOptions.BypassUnRegistered) && !options.HasFlag(UserFeaturesOptions.AllowUnRegistered) && !options.HasFlag(UserFeaturesOptions.AllowProvideInformations) && !options.HasFlag(UserFeaturesOptions.AdministrativeMode));
        }

        /// <summary>
        /// IsRegistrationNotRequired method implementation
        /// </summary>
        public static bool IsRegistrationNotRequired(this UserFeaturesOptions options)
        {
            return ((options.HasFlag(UserFeaturesOptions.BypassUnRegistered) || options.HasFlag(UserFeaturesOptions.AdministrativeMode)) && !options.HasFlag(UserFeaturesOptions.AllowProvideInformations));
        }

        /// <summary>
        /// IsAdvertisable method implmentation
        /// </summary>
        public static bool IsAdvertisable(this UserFeaturesOptions options)
        {
            return !(options.IsRegistrationNotRequired()) || (options.IsRegistrationRequired());
        }
        #endregion

        #region MFA Options
        /// <summary>
        /// CanAccessOptions method
        /// </summary>
        public static bool CanAccessOptions(this UserFeaturesOptions options)
        {
            return options.HasFlag(UserFeaturesOptions.AllowChangePassword) || options.HasFlag(UserFeaturesOptions.AllowManageOptions) || options.HasFlag(UserFeaturesOptions.AllowEnrollment);
        }

        /// <summary>
        /// CanManagePassword method
        /// </summary>
        public static bool CanManagePassword(this UserFeaturesOptions options)
        {
            return options.HasFlag(UserFeaturesOptions.AllowChangePassword);
        }

        /// <summary>
        /// CanManageOptions method
        /// </summary>
        public static bool CanManageOptions(this UserFeaturesOptions options)
        {
            return options.HasFlag(UserFeaturesOptions.AllowManageOptions);
        }

        /// <summary>
        /// CanEnrollDevices method
        /// </summary>
        public static bool CanEnrollDevices(this UserFeaturesOptions options)
        {
            return options.HasFlag(UserFeaturesOptions.AllowEnrollment);
        }
        #endregion

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
                case UserTemplateMode.Custom:
                    options = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered);
                    break;
            }
            return options;
        }

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
        /// Clear method implmentation
        /// </summary>
        public static UserFeaturesOptions Clear(this UserFeaturesOptions options)
        {
            options = UserFeaturesOptions.NoSet;
            return options;
        }
        #endregion

        #region MMC Enable Options
        /// <summary>
        /// SetMFARequired method implementation
        /// </summary>
        public static UserFeaturesOptions MMCSetMFARequired(this UserFeaturesOptions options)
        {
            options = options.Remove(UserFeaturesOptions.BypassDisabled);
            options = options.Remove(UserFeaturesOptions.AllowDisabled);
            // options = options.Add(UserFeaturesOptions.AdministrativeMode); // Admin only
            return options;
        }

        /// <summary>
        /// MMCSetMFAAllowed method implementation
        /// </summary>
        public static UserFeaturesOptions MMCSetMFAAllowed(this UserFeaturesOptions options)
        {
            options = options.Remove(UserFeaturesOptions.BypassDisabled);
            // options = options.Remove(UserFeaturesOptions.AdministrativeMode);
            options = options.Add(UserFeaturesOptions.AllowDisabled); // Allow Disable Only
            return options;
        }


        /// <summary>
        /// MMCSetMFANotRequired method implementation
        /// </summary>
        public static UserFeaturesOptions MMCSetMFANotRequired(this UserFeaturesOptions options)
        {
            // options = options.Remove(UserFeaturesOptions.AdministrativeMode);
            options = options.Remove(UserFeaturesOptions.AllowDisabled);
            options = options.Add(UserFeaturesOptions.BypassDisabled); // Allow Bypass Only  
            return options;
        }
        #endregion

        #region MMC Registration options
        /// <summary>
        /// MMCSetMandatoryRegistration method implementation
        /// </summary>
        public static UserFeaturesOptions MMCSetMandatoryRegistration(this UserFeaturesOptions options)
        {

            options = options.Remove(UserFeaturesOptions.BypassUnRegistered);
            options = options.Remove(UserFeaturesOptions.AllowUnRegistered);
            options = options.Remove(UserFeaturesOptions.AllowProvideInformations);
            options = options.Add(UserFeaturesOptions.AdministrativeMode);
            return options;
        }


        /// <summary>
        /// SetAdministrativeRegistration method implementation
        /// </summary>
        public static UserFeaturesOptions MMCSetAdministrativeRegistration(this UserFeaturesOptions options)
        {
            options = options.Remove(UserFeaturesOptions.BypassUnRegistered);
            options = options.Remove(UserFeaturesOptions.AllowUnRegistered);
            options = options.Add(UserFeaturesOptions.AdministrativeMode);
            options = options.Add(UserFeaturesOptions.AllowProvideInformations);   // Allow only provide informations
            return options;
        }

        /// <summary>
        /// MMCSetSelfRegistration method implementation
        /// </summary>
        public static UserFeaturesOptions MMCSetSelfRegistration(this UserFeaturesOptions options)
        {
            options = options.Remove(UserFeaturesOptions.AllowProvideInformations);
            options = options.Remove(UserFeaturesOptions.AdministrativeMode);
            options = options.Remove(UserFeaturesOptions.BypassUnRegistered);
            options = options.Add(UserFeaturesOptions.AllowUnRegistered);    // Allow User to register
            return options;
        }

        /// <summary>
        /// MMCSetUnManagedRegistration method implementation
        /// </summary>
        public static UserFeaturesOptions MMCSetUnManagedRegistration(this UserFeaturesOptions options)
        {
            options = options.Add(UserFeaturesOptions.BypassUnRegistered);
            options = options.Remove(UserFeaturesOptions.AdministrativeMode);
            options = options.Remove(UserFeaturesOptions.AllowProvideInformations);
            options = options.Remove(UserFeaturesOptions.AllowUnRegistered);
            return options;
        }
        #endregion
    }

    public class ConfigAdvertising
    {
        private uint _firstDay = 1;
        private uint _lastDay = 31;

        /// <summary>
        /// MFAConfigAdvertising constructor
        /// </summary>
        public ConfigAdvertising()
        {
        }

        /// <summary>
        /// MFAConfigAdvertising constructor
        /// </summary>
        public ConfigAdvertising(uint firstday, uint lastday)
        {
            this.FirstDay = firstday;
            this.LastDay = lastday;
        }

        /// <summary>
        /// FirstDay property
        /// </summary>
        public uint FirstDay
        {
            get { return _firstDay; }
            set
            { _firstDay = value; }
        }

        /// <summary>
        /// LastDay property
        /// </summary>
        public uint LastDay
        {
            get { return _lastDay; }
            set { _lastDay = value; }
        }

        /// <summary>
        /// Inverted property
        /// </summary>
        public bool OnFire
        {
            get
            {

                bool res = true;
                int DD = DateTime.Now.ToUniversalTime().Day;
                if (FirstDay <= LastDay)
                {
                    res = ((DD >= FirstDay) && (DD <= LastDay));
                }
                else
                {
                    uint FD = LastDay;
                    uint LD = FirstDay;
                    res = !((DD >= FD) && (DD <= LD));
                }
                return res;
            }
        }

        /// <summary>
        /// CheckDataValue method
        /// </summary>
        private bool CheckDataValue(uint value)
        {
            if (value < 1)
                return false;
            else if (value > 31)
                return false;
            return true;
        }
    }

    [XmlRoot("MFAConfig")]
    public class MFAConfig
    {
        private string _country = "fr";
        private int _maxretries = 3;
        private string _issuer;
        private int _pinlength = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        public MFAConfig()
        {
            this.Hosts = new Hosts();
            this.KeysConfig = new KeysManagerConfig();
            this.OTPProvider = new OTPProvider();
            this.MailProvider = new MailProvider();
            this.ExternalProvider = new ExternalOTPProvider();
            this.AzureProvider = new AzureProvider();
            this.WebAuthNProvider = new WebAuthNProvider();
            this.WsMan = new ADFSWSManager();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MFAConfig(bool initializedefaults) : this()
        {
            if (initializedefaults)
            {
                IsDirty = false;
                DeliveryWindow = 300;
                Issuer = "MFA";
                PinLength = 4;
                DefaultPin = 1234;
                MaxRetries = 3;
                UiKind = ADFSUserInterfaceKind.Default;
                DefaultProviderMethod = PreferredMethod.Choose;
                ReplayLevel = ReplayLevel.Disabled;
                UseUIPaginated = false;

                StoreMode = DataRepositoryKind.ADDS;
                CustomUpdatePassword = true;
                KeepMySelectedOptionOn = true;
                ChangeNotificationsOn = true;
                IsPrimaryAuhentication = false;
                PrimaryAuhenticationOptions = PrimaryAuthOptions.None;
                DefaultCountryCode = "fr";
                AdminContact = "adminmfa@contoso.com";
                UserFeatures = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment);

                KeysConfig.KeyGenerator = KeyGeneratorMode.ClientSecret512;
                KeysConfig.KeyFormat = SecretKeyFormat.RNG;
                KeysConfig.KeySize = KeySizeMode.KeySize1024;
                KeysConfig.AESKeyGenerator = AESKeyGeneratorMode.AESSecret1024;
                KeysConfig.CertificateThumbprint = Thumbprint.Empty;
                KeysConfig.CertificatePerUser = false;

                OTPProvider.Enabled = true;
                OTPProvider.IsRequired = true;
                OTPProvider.TOTPShadows = 2;
                OTPProvider.Algorithm = HashMode.SHA1;
                OTPProvider.EnrollWizard = true;
                OTPProvider.PinRequired = false;
                OTPProvider.Parameters.Data = string.Empty;
                OTPProvider.FullQualifiedImplementation = string.Empty;

                MailProvider.From = "sender.email@contoso.com";
                MailProvider.UserName = "user.name@contoso.com";
                MailProvider.Password = "yourpass";
                MailProvider.Host = "smtp.contoso.com";
                MailProvider.Port = 587;
                MailProvider.UseSSL = true;
                MailProvider.Company = "Contoso";
                MailProvider.Enabled = true;
                MailProvider.IsRequired = true;
                MailProvider.EnrollWizard = true;
                MailProvider.PinRequired = false;
                MailProvider.FullQualifiedImplementation = string.Empty;
                MailProvider.Parameters.Data = string.Empty;

                ExternalProvider.Enabled = false;
                ExternalProvider.IsRequired = false;
                ExternalProvider.EnrollWizard = false;
                ExternalProvider.PinRequired = false;
                ExternalProvider.Company = "Contoso";
                ExternalProvider.FullQualifiedImplementation = string.Empty;
                ExternalProvider.IsTwoWay = false;
                ExternalProvider.Sha1Salt = "0x1230456789ABCDEF";
                ExternalProvider.Parameters.Data = string.Empty;
                ExternalProvider.FullQualifiedImplementation = string.Empty;
                if (ExternalProvider.FullQualifiedImplementation.ToLower().StartsWith("neos.identityserver.multifactor.sms.smscall"))
                    ExternalProvider.FullQualifiedImplementation = "Neos.IdentityServer.Multifactor.Samples.SMSCall, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
                if (ExternalProvider.FullQualifiedImplementation.ToLower().StartsWith("neos.identityserver.multifactor.samples.smscall"))
                    ExternalProvider.FullQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.SMSCall, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
                if (ExternalProvider.FullQualifiedImplementation.ToLower().StartsWith("neos.identityserver.multifactor.sms.neossmsprovider"))
                    ExternalProvider.FullQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.NeosSMSProvider, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
                if (ExternalProvider.FullQualifiedImplementation.ToLower().StartsWith("neos.identityserver.multifactor.samples.neossmsprovider"))
                    ExternalProvider.FullQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.NeosSMSProvider, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";

                AzureProvider.TenantId = "contoso.onmicrosoft.com";
                AzureProvider.ThumbPrint = Thumbprint.Demo;
                AzureProvider.Enabled = false;
                AzureProvider.IsRequired = false;
                AzureProvider.EnrollWizard = false;
                AzureProvider.PinRequired = false;

                WebAuthNProvider.Enabled = true;
                WebAuthNProvider.IsRequired = false;
                WebAuthNProvider.EnrollWizard = true;
                WebAuthNProvider.PinRequired = false;
                WebAuthNProvider.DirectLogin = true;

                Hosts.SQLServerHost.ConnectionString = "Password=yourpassword;Persist Security Info=True;User ID=yoursqlusername;Initial Catalog=yourdatabasename;Data Source=yoursqlserver\\yourinstance";
                Hosts.SQLServerHost.IsAlwaysEncrypted = false;
                Hosts.SQLServerHost.ThumbPrint = Thumbprint.Demo;
                Hosts.SQLServerHost.MaxRows = 10000;

                Hosts.CustomStoreHost.DataRepositoryFullyQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.InMemoryDataRepositoryService, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
                Hosts.CustomStoreHost.KeysRepositoryFullyQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.InMemoryKeys2RepositoryService, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
            }
        }

        /// <summary>
        /// UpgradeDefaults method
        /// </summary>
        public void UpgradeDefaults()
        {
            IsDirty = false;
            if (DeliveryWindow <= 0)
                DeliveryWindow = 300;

            if (PinLength < 4)
                PinLength = 4;
            else if (PinLength > 9)
                PinLength = 9;

            if (MaxRetries <= 0)
                MaxRetries = 1;
            if (string.IsNullOrEmpty(Issuer))
                Issuer = "MFA";
            if (OTPProvider.TOTPShadows <= 0)
                OTPProvider.TOTPShadows = 2;
            if (OTPProvider.Algorithm != HashMode.SHA1)
                OTPProvider.Algorithm = HashMode.SHA1;

            if (string.IsNullOrEmpty(DefaultCountryCode))
                DefaultCountryCode = "fr";
            if (string.IsNullOrEmpty(AdminContact))
                AdminContact = "adminmfa@contoso.com";
            UiKind = ADFSUserInterfaceKind.Default;
            DefaultProviderMethod = PreferredMethod.Choose;
            ReplayLevel = ReplayLevel.Disabled;
            UseUIPaginated = false;
            IsPrimaryAuhentication = false;
            PrimaryAuhenticationOptions = PrimaryAuthOptions.None;
            UserFeatures = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);

            if (string.IsNullOrEmpty(Hosts.SQLServerHost.ConnectionString))
                Hosts.SQLServerHost.ConnectionString = "Password=yourpassword;Persist Security Info=True;User ID=yoursqlusername;Initial Catalog=yourdatabasename;Data Source=yoursqlserver\\yourinstance";
            if (string.IsNullOrEmpty(Hosts.SQLServerHost.ThumbPrint))
                Hosts.SQLServerHost.ThumbPrint = Thumbprint.Demo;
            Hosts.SQLServerHost.MaxRows = 10000;
            Hosts.SQLServerHost.IsAlwaysEncrypted = false;

            if (string.IsNullOrEmpty(Hosts.CustomStoreHost.DataRepositoryFullyQualifiedImplementation))
                Hosts.CustomStoreHost.DataRepositoryFullyQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.InMemoryDataRepositoryService, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
            if (string.IsNullOrEmpty(Hosts.CustomStoreHost.KeysRepositoryFullyQualifiedImplementation))
                Hosts.CustomStoreHost.KeysRepositoryFullyQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.InMemoryKeys2RepositoryService, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";

            if (string.IsNullOrEmpty(MailProvider.From))
                MailProvider.From = "sender.email@contoso.com";
            if (string.IsNullOrEmpty(MailProvider.UserName))
                MailProvider.UserName = "user.name@contoso.com";
            if (string.IsNullOrEmpty(MailProvider.Password))
                MailProvider.Password = "yourpass";
            if (string.IsNullOrEmpty(MailProvider.Host))
                MailProvider.Host = "smtp.contoso.com";
            if (string.IsNullOrEmpty(MailProvider.Company))
                MailProvider.Company = "Contoso";
            if (MailProvider.Port == 0)
            {
                MailProvider.Port = 587;
                MailProvider.UseSSL = true;
            }

            if (this.KeysConfig != null)
            {
                KeysConfig.KeyFormat = SecretKeyFormat.RNG;
                KeysConfig.KeySize = KeySizeMode.KeySize1024;
                KeysConfig.KeyGenerator = KeyGeneratorMode.ClientSecret512;
                KeysConfig.AESKeyGenerator = AESKeyGeneratorMode.AESSecret1024;
                KeysConfig.CertificatePerUser = false;
                if (!Thumbprint.IsValid(this.KeysConfig.CertificateThumbprint))
                    KeysConfig.CertificateThumbprint = Thumbprint.Empty;
            }
            if (this.ExternalProvider != null)
            {
                if (ExternalProvider.FullQualifiedImplementation.ToLower().StartsWith("neos.identityserver.multifactor.sms.smscall"))
                    ExternalProvider.FullQualifiedImplementation = "Neos.IdentityServer.Multifactor.Samples.SMSCall, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
                if (ExternalProvider.FullQualifiedImplementation.ToLower().StartsWith("neos.identityserver.multifactor.samples.smscall"))
                    ExternalProvider.FullQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.SMSCall, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
                if (ExternalProvider.FullQualifiedImplementation.ToLower().StartsWith("neos.identityserver.multifactor.sms.neossmsprovider"))
                    ExternalProvider.FullQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.NeosSMSProvider, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
                if (ExternalProvider.FullQualifiedImplementation.ToLower().StartsWith("neos.identityserver.multifactor.samples.neossmsprovider"))
                    ExternalProvider.FullQualifiedImplementation = "Neos.IdentityServer.MultiFactor.Samples.NeosSMSProvider, Neos.IdentityServer.MultiFactor.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
                if (string.IsNullOrEmpty(this.ExternalProvider.Company))
                    this.ExternalProvider.Company = "Contoso";
                if (string.IsNullOrEmpty(this.ExternalProvider.Sha1Salt))
                    this.ExternalProvider.Sha1Salt = "0x1230456789ABCDEF";
                if (string.IsNullOrEmpty(this.ExternalProvider.Parameters.Data))
                    ExternalProvider.Parameters.Data = "LICENSE_KEY = LICENCE, GROUP_KEY = 01234567891011121314151617181920, CERT_THUMBPRINT = " + Thumbprint.Demo;
            }
            if (this.AzureProvider != null)
            {
                if (string.IsNullOrEmpty(this.AzureProvider.TenantId))
                    this.AzureProvider.TenantId = "contoso.onmicrosoft.com";
                if (string.IsNullOrEmpty(this.AzureProvider.ThumbPrint))
                    this.AzureProvider.ThumbPrint = Thumbprint.Demo;
            }


        }

        [XmlIgnore]
        public bool IsDirty { get; set; } = false;

        [XmlAttribute("DeliveryWindow")]
        public int DeliveryWindow { get; set; } = 300;

        [XmlAttribute("PinLength")]
        public int PinLength
        {
            get { return _pinlength; }
            set
            {
                if ((value >= 4) && (value <= 9))
                    _pinlength = value;
                else
                    throw new ArgumentException("Invalid PIN len (must be between 4 and 9 numbers lenght");
            }
        }

        [XmlAttribute("DefaultPin")]
        public int DefaultPin { get; set; } = 1234;

        [XmlAttribute("KMSOO")]
        public bool KeepMySelectedOptionOn { get; set; } = true;

        [XmlAttribute("ChangeNotificationsOn")]
        public bool ChangeNotificationsOn { get; set; } = true;

        [XmlAttribute("DefaultProviderMethod")]
        public PreferredMethod DefaultProviderMethod { get; set; } = PreferredMethod.Choose;

        [XmlAttribute("UseOfUserLanguages")]
        public bool UseOfUserLanguages { get; set; } = true;

        [XmlAttribute("MaxRetries")]
        public int MaxRetries
        {
            get { return _maxretries; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();
                _maxretries = value;
            }
        }

        [XmlAttribute("Issuer")]
        public string Issuer
        {
            get
            {
                if (string.IsNullOrEmpty(_issuer))
                    return "MFA";
                else
                    return _issuer;
            }
            set { _issuer = value; }
        }

        [XmlIgnore]
        public string QRIssuer
        {
            get
            {
                return HttpUtility.UrlEncode(this.Issuer);
            }
        }

        [XmlAttribute("StoreMode")]
        public DataRepositoryKind StoreMode { get; set; } = DataRepositoryKind.ADDS;

        [XmlAttribute("CustomUpdatePassword")]
        public bool CustomUpdatePassword { get; set; } = true;

        [XmlAttribute("DefaultCountryCode")]
        public string DefaultCountryCode
        {
            get
            {
                if (string.IsNullOrEmpty(_country))
                {
                    CultureInfo culture = CultureInfo.InstalledUICulture;
                    _country = culture.TwoLetterISOLanguageName;
                }
                return _country;
            }
            set { _country = value; }
        }

        [XmlAttribute("AdminContact")]
        public string AdminContact
        {
            get;
            set;
        }

        [XmlAttribute("UserFeatures")]
        public UserFeaturesOptions UserFeatures { get; set; } = UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword | UserFeaturesOptions.AllowEnrollment;

        [XmlElement("ActivationAdvertising")]
        public ConfigAdvertising AdvertisingDays { get; set; } = new ConfigAdvertising(1, 31);

        [XmlElement("UiKind")]
        public ADFSUserInterfaceKind UiKind { get; set; } = ADFSUserInterfaceKind.Default;

        [XmlElement("UseUIPaginated")]
        public bool UseUIPaginated { get; set; } = false;

        [XmlElement("IsPrimaryAuhentication")]
        public bool IsPrimaryAuhentication { get; set; } = false;

        [XmlElement("PrimaryAuhenticationOptions")]
        public PrimaryAuthOptions PrimaryAuhenticationOptions { get; set; } = PrimaryAuthOptions.None;

        [XmlElement("LastUpdated")]
        public DateTime LastUpdated { get; set; }

        [XmlElement("ReplayLevel")]
        public ReplayLevel ReplayLevel { get; set; } = ReplayLevel.Disabled;

        [XmlElement("Hosts")]
        public Hosts Hosts
        {
            get;
            set;
        }

        [XmlElement("KeysConfig")]
        public KeysManagerConfig KeysConfig
        {
            get;
            set;
        }

        [XmlElement("OTPProvider")]
        public OTPProvider OTPProvider
        {
            get;
            set;
        }

        [XmlElement("SendMail")]
        public MailProvider MailProvider
        {
            get;
            set;
        }

        [XmlElement("ExternalOTPProvider")]
        public ExternalOTPProvider ExternalProvider
        {
            get;
            set;
        }

        [XmlElement("AzureProvider")]
        public AzureProvider AzureProvider
        {
            get;
            set;
        }

        [XmlElement("WebAuthNProvider")]
        public WebAuthNProvider WebAuthNProvider
        {
            get;
            set;
        }

        [XmlElement("WsMan")]
        public ADFSWSManager WsMan
        {
            get;
            set;
        }

        [XmlAttribute("ForcedLanguage")]
        public string ForcedLanguage { get; set; }
    }
    #endregion

    #region KeysManagerConfig
    /// <summary>
    /// MFAKeysConfig class implementation
    /// </summary>
    public class KeysManagerConfig
    {
        private string _thumbprint;
        private XmlCDataSection _cdata;

        [XmlAttribute("KeyGenerator")]
        public KeyGeneratorMode KeyGenerator { get; set; } = KeyGeneratorMode.ClientSecret512;

        [XmlAttribute("AESKeyGenerator")]
        public AESKeyGeneratorMode AESKeyGenerator { get; set; } = AESKeyGeneratorMode.AESSecret1024;

        [XmlAttribute("KeyFormat")]
        public SecretKeyFormat KeyFormat { get; set; } = SecretKeyFormat.RSA;

        [XmlAttribute("KeyVersion")]
        public SecretKeyVersion KeyVersion { get; set; } = SecretKeyVersion.V2;

        [XmlAttribute("XORSecret")]
        public string XORSecret
        {
            get { return XORUtilities.XORKey; }
            set
            {
                XORUtilities.XORKey = value;
            }
        }

        [XmlAttribute("CertificatePerUser")]
        public bool CertificatePerUser { get; set; } = false;

        [XmlAttribute("CertificateThumbprint")]
        public string CertificateThumbprint
        {
            get { return _thumbprint; }
            set
            {
                if (Thumbprint.IsValid(value))
                    _thumbprint = value;
                else
                    _thumbprint = Thumbprint.Null;
            }
        }

        [XmlAttribute("CertificateValidity")]
        public int CertificateValidity { get; set; } = 5;

        [XmlAttribute("KeySize")]
        public KeySizeMode KeySize { get; set; } = KeySizeMode.KeySize1024;

        [XmlAttribute("CustomFullyQualifiedImplementation")]
        public string CustomFullyQualifiedImplementation { get; set; }

        [XmlElement("CustomParameters", typeof(XmlCDataSection))]
        public XmlCDataSection CustomParameters
        {
            get
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                return _cdata;
            }
            set
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                _cdata.Data = value.Data;
            }
        }
    }
    #endregion

    #region Providers Parameters
    /// <summary>
    /// ExternalProvider contract
    /// </summary>
    public abstract class BaseProviderParams
    {
        public abstract bool Enabled { get; set; }
        public abstract bool PinRequired { get; set; }
        public abstract bool EnrollWizard { get; set; }
        public abstract bool IsRequired { get; set; }
        public abstract ForceWizardMode ForceWizard { get; set; }
    }

    /// <summary>
    /// OTPProvider contract
    /// </summary>
    public class OTPProviderParams : BaseProviderParams
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OTPProviderParams() { }

        /// <summary>
        /// Constructor initialized
        /// </summary>
        public OTPProviderParams(OTPProvider prov) : base()
        {
            this.Data = prov;
            this.TOTPShadows = prov.TOTPShadows;
            this.Algorithm = prov.Algorithm;
            this.Digits = prov.TOTPDigits;
            this.Duration = prov.TOTPDuration;
            this.Enabled = prov.Enabled;
            this.IsRequired = prov.IsRequired;
            this.PinRequired = prov.PinRequired;
            this.EnrollWizard = prov.EnrollWizard;
            this.ForceWizard = prov.ForceWizard;
        }

        public OTPProvider Data { get; set; }
        public int TOTPShadows { get; set; }
        public HashMode Algorithm { get; set; }
        public int Digits { get; set; }
        public int Duration { get; set; }
        public MFAConfig Config { get; set; }
        public override bool Enabled { get; set; }
        public override bool IsRequired { get; set; }
        public override bool PinRequired { get; set; }
        public override bool EnrollWizard { get; set; }
        public override ForceWizardMode ForceWizard { get; set; }
    }

    /// <summary>
    /// MailProviderParams contract
    /// </summary>
    public class MailProviderParams : BaseProviderParams
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MailProviderParams() { }

        /// <summary>
        /// Constructor initialized
        /// </summary>
        public MailProviderParams(MailProvider prov) : base()
        {
            Data = prov;
            Enabled = prov.Enabled;
            IsRequired = prov.IsRequired;
            PinRequired = prov.PinRequired;
            EnrollWizard = prov.EnrollWizard;
            ForceWizard = prov.ForceWizard;
            SendDeliveryNotifications = prov.DeliveryNotifications;
        }

        public MailProvider Data { get; set; }
        public override bool Enabled { get; set; }
        public override bool IsRequired { get; set; }
        public override bool PinRequired { get; set; }
        public override bool EnrollWizard { get; set; }
        public override ForceWizardMode ForceWizard { get; set; }
        public bool SendDeliveryNotifications { get; set; }
    }

    /// <summary>
    /// SMSProviderParams contract
    /// </summary>
    public class ExternalProviderParams : BaseProviderParams
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ExternalProviderParams() { }

        /// <summary>
        /// Constructor initialized
        /// </summary>
        public ExternalProviderParams(ExternalOTPProvider prov) : base()
        {
            Data = prov;
            Enabled = prov.Enabled;
            IsRequired = prov.IsRequired;
            PinRequired = prov.PinRequired;
            EnrollWizard = prov.EnrollWizard;
            ForceWizard = prov.ForceWizard;
        }

        public ExternalOTPProvider Data { get; set; }
        public override bool Enabled { get; set; }
        public override bool IsRequired { get; set; }
        public override bool PinRequired { get; set; }
        public override bool EnrollWizard { get; set; }
        public override ForceWizardMode ForceWizard { get; set; }
    }

    /// <summary>
    /// AzureProviderParams contract
    /// </summary>
    public class AzureProviderParams : BaseProviderParams
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AzureProviderParams() { }

        /// <summary>
        /// Constructor initialized
        /// </summary>
        public AzureProviderParams(AzureProvider prov, string adfsid, string company) : base()
        {
            Data = prov;
            this.ADFSIdentifier = adfsid;
            this.CompanyName = company;
            this.Enabled = prov.Enabled;
            this.IsRequired = prov.IsRequired;
            this.PinRequired = prov.PinRequired;
            this.EnrollWizard = prov.EnrollWizard;
            this.ForceWizard = ForceWizardMode.Disabled;
        }

        public AzureProvider Data { get; set; }
        public string ADFSIdentifier { get; set; }
        public string CompanyName { get; set; }
        public override bool Enabled { get; set; }
        public override bool IsRequired { get; set; }
        public override bool PinRequired { get; set; }
        public override bool EnrollWizard { get; set; }
        public override ForceWizardMode ForceWizard { get; set; }
    }

    /// <summary>
    /// WebAuthNProviderParams contract
    /// </summary>
    public class WebAuthNProviderParams : BaseProviderParams
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WebAuthNProviderParams() { }

        /// <summary>
        /// Constructor initialized
        /// </summary>
        public WebAuthNProviderParams(MFAConfig Cfg, WebAuthNProvider prov) : base()
        {
            this.Config = Cfg;
            this.Data = prov;
            this.Enabled = prov.Enabled;
            this.IsRequired = prov.IsRequired;
            this.PinRequired = prov.PinRequired;
            this.PinRequirements = prov.PinRequirements;
            this.EnrollWizard = prov.EnrollWizard;
            this.ForceWizard = prov.ForceWizard;
            this.Configuration = prov.Configuration;
            this.Options = prov.Options;
            this.DirectLogin = prov.DirectLogin;
        }

        public MFAConfig Config { get; set; }
        public WebAuthNProvider Data { get; set; }
        public override bool Enabled { get; set; }
        public override bool IsRequired { get; set; }
        public override bool PinRequired { get; set; }
        public virtual WebAuthNPinRequirements PinRequirements { get; set; }
        public override bool EnrollWizard { get; set; }
        public bool DirectLogin { get; set; }
        public override ForceWizardMode ForceWizard { get; set; }
        public WebAuthNProviderConfig Configuration { get; set; } = new WebAuthNProviderConfig();
        public WebAuthNProviderOptions Options { get; set; } = new WebAuthNProviderOptions();
    }
    #endregion

    #region Keys Manager Parameters
    /// <summary>
    /// ExternalProvider contract
    /// </summary>
    public abstract class BaseKeysManagerParams
    {
        public BaseKeysManagerParams(string xorsecret)
        {
            XORSecret = xorsecret;
        }

        /// <summary>
        /// XORSecret propertiy
        /// </summary>
        public string XORSecret { get; private set; }

        /// <summary>
        /// KeySizeMode property
        /// </summary>
        public KeySizeMode KeySizeMode { get; internal set; }

        /// <summary>
        /// KeysFormat abstract property
        /// </summary>
        public abstract SecretKeyFormat KeysFormat { get; }

        /// <summary>
        /// PatchFromSecurityConfig method implementation
        /// </summary>
        internal abstract void PatchFromSecurityConfig(KeysManagerConfig config);
    }

    /// <summary>
    /// RNGKeysManagerParams contract
    /// </summary>
    internal class RNGKeysManagerParams : BaseKeysManagerParams
    {
        public RNGKeysManagerParams(string xorsecret) : base(xorsecret)
        {
        }

        /// <summary>
        /// KeysFormat Override property
        /// </summary>
        public override SecretKeyFormat KeysFormat
        {
            get { return SecretKeyFormat.RNG; }
        }

        /// <summary>
        /// KeyGenerator property
        /// </summary>
        public KeyGeneratorMode KeyGenerator { get; private set; }

        /// <summary>
        /// PatchFromSecurityConfig method implementation
        /// </summary>
        internal override void PatchFromSecurityConfig(KeysManagerConfig config)
        {
            KeyGenerator = config.KeyGenerator;
            KeySizeMode = config.KeySize;
        }
    }

    /// <summary>
    /// AESKeysManagerParams contract
    /// </summary>
    internal class AESKeysManagerParams : BaseKeysManagerParams
    {
        public AESKeysManagerParams(string xorsecret) : base(xorsecret)
        {
        }

        /// <summary>
        /// KeysFormat Override property
        /// </summary>
        public override SecretKeyFormat KeysFormat
        {
            get { return SecretKeyFormat.AES; }
        }

        /// <summary>
        /// AESKeyGenerator property
        /// </summary>
        public AESKeyGeneratorMode AESKeyGenerator { get; private set; }

        /// <summary>
        /// PatchFromSecurityConfig method implementation
        /// </summary>
        internal override void PatchFromSecurityConfig(KeysManagerConfig config)
        {
            AESKeyGenerator = config.AESKeyGenerator;
        }
    }

    /// <summary>
    /// RSAKeysManagerParams contract
    /// </summary>
    internal class RSAKeysManagerParams : BaseKeysManagerParams
    {
        public RSAKeysManagerParams(string xorsecret) : base(xorsecret)
        {
        }

        /// <summary>
        /// KeysFormat Override property
        /// </summary>
        public override SecretKeyFormat KeysFormat
        {
            get { return SecretKeyFormat.RSA; }
        }


        /// <summary>
        /// CertificateThumbprint property
        /// </summary>
        public string CertificateThumbprint { get; private set; }

        /// <summary>
        /// PatchFromSecurityConfig method implementation
        /// </summary>
        internal override void PatchFromSecurityConfig(KeysManagerConfig config)
        {
            CertificateThumbprint = config.CertificateThumbprint;
        }
    }

    /// <summary>
    /// RSA2KeysManagerParams contract
    /// </summary>
    internal class RSA2KeysManagerParams : BaseKeysManagerParams
    {
        public RSA2KeysManagerParams(string xorsecret) : base(xorsecret)
        {
        }

        /// <summary>
        /// KeysFormat Override property
        /// </summary>
        public override SecretKeyFormat KeysFormat
        {
            get { return SecretKeyFormat.RSA; }
        }

        /// <summary>
        /// CertificateThumbprint property
        /// </summary>
        public string CertificateThumbprint { get; private set; }

        /// <summary>
        /// CertificateValidity property
        /// </summary>
        public int CertificateValidity { get; private set; }

        /// <summary>
        /// PatchFromSecurityConfig method implementation
        /// </summary>
        internal override void PatchFromSecurityConfig(KeysManagerConfig config)
        {
            CertificateThumbprint = config.CertificateThumbprint;
            CertificateValidity = config.CertificateValidity;
        }
    }
    /// <summary>
    /// CustomKeysManagerParams contract
    /// </summary>
    public class CustomKeysManagerParams : BaseKeysManagerParams
    {
        public CustomKeysManagerParams(string xorsecret) : base(xorsecret)
        {
        }

        /// <summary>
        /// KeysFormat Override propertiy
        /// </summary>
        public override SecretKeyFormat KeysFormat
        {
            get { return SecretKeyFormat.CUSTOM; }
        }

        /// <summary>
        /// CustomParameters poroperty
        /// </summary>
        public string CustomParameters { get; set; }

        /// <summary>
        /// PatchFromSecurityConfig method implementation
        /// </summary>
        internal override void PatchFromSecurityConfig(KeysManagerConfig config)
        {
            CustomParameters = config.CustomParameters.Data;
        }
    }

    #endregion

    #region ExternalOTPProvider
    /// <summary>
    /// ExternalOTPProvider contract
    /// </summary>
    public class ExternalOTPProvider
    {
        private XmlCDataSection _cdata;

        [XmlAttribute("Enabled")]
        public bool Enabled { get; set; } = true;

        [XmlAttribute("PinRequired")]
        public bool PinRequired { get; set; } = false;

        [XmlAttribute("IsRequired")]
        public bool IsRequired { get; set; } = false;

        [XmlAttribute("EnrollWizard")]
        public bool EnrollWizard { get; set; } = true;

        [XmlAttribute("ForceWizard")]
        public ForceWizardMode ForceWizard { get; set; } = ForceWizardMode.Disabled;

        [XmlAttribute("Company")]
        public string Company { get; set; } = "your company description";

        [XmlAttribute("Sha1Salt")]
        public string Sha1Salt { get; set; } = "0x123456789";

        [XmlAttribute("FullQualifiedImplementation")]
        public string FullQualifiedImplementation { get; set; }

        [XmlElement("Parameters", typeof(XmlCDataSection))]
        public XmlCDataSection Parameters
        {
            get
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                return _cdata;
            }
            set
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                _cdata.Data = value.Data;
            }
        }

        [XmlAttribute("IsTwoWay")]
        public bool IsTwoWay { get; set; } = false;

        [XmlAttribute("Timeout")]
        public int Timeout { get; set; } = 300;
    }
    #endregion

    #region AzureProvider
    /// <summary>
    /// AzureProvider contract
    /// </summary>
    public class AzureProvider
    {
        private XmlCDataSection _cdata;

        [XmlAttribute("Enabled")]
        public bool Enabled { get; set; } = false;

        [XmlAttribute("PinRequired")]
        public bool PinRequired { get; set; } = false;

        [XmlAttribute("IsRequired")]
        public bool IsRequired { get; set; } = false;

        [XmlAttribute("EnrollWizard")]
        public bool EnrollWizard { get; set; } = false;

        [XmlAttribute("ForceWizard")]
        public ForceWizardMode ForceWizard { get; set; } = ForceWizardMode.Disabled;

        [XmlAttribute("TenantId")]
        public string TenantId { get; set; } = "yourcompany.onnmicrosoft.com";

        [XmlAttribute("ThumbPrint")]
        public string ThumbPrint { get; set; } = Thumbprint.Demo;

        [XmlAttribute("FullQualifiedImplementation")]
        public string FullQualifiedImplementation { get; set; }

        [XmlElement("Parameters", typeof(XmlCDataSection))]
        public XmlCDataSection Parameters
        {
            get
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                return _cdata;
            }
            set
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                _cdata.Data = value.Data;
            }
        }
    }
    #endregion

    #region MailProvider
    /// <summary>
    /// MailProvider class implementation
    /// </summary>
    public class MailProvider
    {
        private XmlCDataSection _cdata;
        private readonly List<string> _blocked = new List<string>();

        [XmlAttribute("Enabled")]
        public bool Enabled { get; set; } = true;

        [XmlAttribute("PinRequired")]
        public bool PinRequired { get; set; } = true;

        [XmlAttribute("IsRequired")]
        public bool IsRequired { get; set; } = true;

        [XmlAttribute("EnrollWizard")]
        public bool EnrollWizard { get; set; } = true;

        [XmlAttribute("ForceWizard")]
        public ForceWizardMode ForceWizard { get; set; } = ForceWizardMode.Disabled;

        [XmlAttribute("from")]
        public string From { get; set; }

        [XmlAttribute("username")]
        public string UserName { get; set; }

        [XmlAttribute("password")]
        public string Password { get; set; }

        [XmlAttribute("anonymous")]
        public bool Anonymous { get; set; } = false;

        [XmlAttribute("host")]
        public string Host { get; set; }

        [XmlAttribute("port")]
        public int Port { get; set; }

        [XmlAttribute("useSSL")]
        public bool UseSSL { get; set; } = false;

        [XmlAttribute("Company")]
        public string Company { get; set; } = "your company description";

        [XmlAttribute("deliverynotifications")]
        public bool DeliveryNotifications { get; set; } = false;

        [XmlArray("BlockedDomains")]
        [XmlArrayItem("Domain", Type = typeof(string))]
        public List<string> BlockedDomains { get; set; }

        [XmlArray("AllowedDomains")]
        [XmlArrayItem("Domain", Type = typeof(string))]
        public List<string> AllowedDomains { get; set; }

        [XmlArray("MailOTP")]
        [XmlArrayItem("Template", Type = typeof(SendMailFileName))]
        public List<SendMailFileName> MailOTPContent { get; set; }

        [XmlArray("MailInscription")]
        [XmlArrayItem("Template", Type = typeof(SendMailFileName))]
        public List<SendMailFileName> MailAdminContent { get; set; }

        [XmlArray("MailSecureKey")]
        [XmlArrayItem("Template", Type = typeof(SendMailFileName))]
        public List<SendMailFileName> MailKeyContent { get; set; }

        [XmlArray("MailNotifications")]
        [XmlArrayItem("Template", Type = typeof(SendMailFileName))]
        public List<SendMailFileName> MailNotifications { get; set; }

        [XmlAttribute("FullQualifiedImplementation")]
        public string FullQualifiedImplementation { get; set; }

        [XmlElement("Parameters", typeof(XmlCDataSection))]
        public XmlCDataSection Parameters
        {
            get
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                return _cdata;
            }
            set
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                _cdata.Data = value.Data;
            }
        }
    }
    #endregion

    #region OTPProvider
    /// <summary>
    /// OTPProvider class implementation
    /// </summary>
    public class OTPProvider
    {
        private XmlCDataSection _cdata;
        private int _digits = 6;
        private int _duration = 30;

        [XmlAttribute("Enabled")]
        public bool Enabled { get; set; } = true;

        [XmlAttribute("PinRequired")]
        public bool PinRequired { get; set; } = false;

        [XmlAttribute("IsRequired")]
        public bool IsRequired { get; set; } = true;

        [XmlAttribute("EnrollWizard")]
        public bool EnrollWizard { get; set; } = true;

        [XmlAttribute("ForceWizard")]
        public ForceWizardMode ForceWizard { get; set; } = ForceWizardMode.Disabled;

        [XmlAttribute("TOTPShadows")]
        public int TOTPShadows { get; set; } = 2;

        [XmlAttribute("Algorithm")]
        public HashMode Algorithm { get; set; } = HashMode.SHA1;

        [XmlAttribute("TOTPDigits")]
        public int TOTPDigits
        {
            get { return _digits; }
            set
            {
                if ((_digits < 4) || (_digits > 8))
                    _digits = 6;
                else
                    _digits = value;
            }
        }

        [XmlAttribute("TOTPDuration")]
        public int TOTPDuration
        {
            get { return _duration; }
            set
            {
                int xvalue = ((value / 30) * 30);
                if ((xvalue < 30) || (xvalue > 180))
                    _duration = 30;
                else
                    _duration = xvalue;
            }
        }

        [XmlAttribute("WizardOptions")]
        public OTPWizardOptions WizardOptions { get; set; } = OTPWizardOptions.All;

        [XmlAttribute("FullQualifiedImplementation")]
        public string FullQualifiedImplementation { get; set; }

        [XmlElement("Parameters", typeof(XmlCDataSection))]
        public XmlCDataSection Parameters
        {
            get
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                return _cdata;
            }
            set
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                _cdata.Data = value.Data;
            }
        }
    }
    #endregion

    #region WebAuthNProvider
    /// <summary>
    /// WebAuthNProvider class implementation
    /// </summary>
    public class WebAuthNProvider
    {
        private XmlCDataSection _cdata;
        private WebAuthNProviderConfig _cfg;
        private WebAuthNProviderOptions _opt;

        [XmlAttribute("Enabled")]
        public bool Enabled { get; set; } = true;

        [XmlAttribute("PinRequired")]
        public bool PinRequired { get; set; } = false;

        [XmlAttribute("PinRequirements")]
        public WebAuthNPinRequirements PinRequirements { get; set; } = WebAuthNPinRequirements.Null;

        [XmlAttribute("IsRequired")]
        public bool IsRequired { get; set; } = false;

        [XmlAttribute("EnrollWizard")]
        public bool EnrollWizard { get; set; } = true;

        [XmlAttribute("ForceWizard")]
        public ForceWizardMode ForceWizard { get; set; } = ForceWizardMode.Disabled;

        [XmlAttribute("DirectLogin")]
        public bool DirectLogin { get; set; } = true;

        [XmlElement("Configuration", typeof(WebAuthNProviderConfig))]
        public WebAuthNProviderConfig Configuration
        {
            get
            {
                if (_cfg == null)
                    _cfg = new WebAuthNProviderConfig();
                return _cfg;
            }
            set
            {
                if (_cfg == null)
                    _cfg = new WebAuthNProviderConfig();
                _cfg = value;
            }
        }

        [XmlElement("Options", typeof(WebAuthNProviderOptions))]
        public WebAuthNProviderOptions Options
        {
            get
            {
                if (_opt == null)
                    _opt = new WebAuthNProviderOptions();
                return _opt;
            }
            set
            {
                if (_opt == null)
                    _opt = new WebAuthNProviderOptions();
                _opt = value;
            }
        }

        [XmlAttribute("FullQualifiedImplementation")]
        public string FullQualifiedImplementation { get; set; }

        [XmlElement("Parameters", typeof(XmlCDataSection))]
        public XmlCDataSection Parameters
        {
            get
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                return _cdata;
            }
            set
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                _cdata.Data = value.Data;
            }
        }
    }

    /// <summary>
    /// WebAuthNProviderConfig class implementation
    /// </summary>
    public class WebAuthNProviderConfig : IWebAuthNConfiguration
    {
        private uint _timeout = 60000;
        private int _tolerance = 300000;
        private int _challengesize = 16;
        private string _serverdomain = "contoso.com";
        private string _servername = "yourservername";
        private string _origin = "https://sts.contoso.com";

        [XmlAttribute("Timeout")]
        public uint Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                if ((value < 60000) || (value > 600000))
                    throw new InvalidDataException("WebAuthN Configuration : Invalid TimeOut value ! ");
                _timeout = value;
            }
        }

        [XmlAttribute("TimestampDriftTolerance")]
        public int TimestampDriftTolerance
        {
            get
            {
                return _tolerance;
            }
            set
            {
                if ((value < 0) || (value > 300000))
                    throw new InvalidDataException("WebAuthN Configuration : Invalid Timestamp Drift Tolerance value ! ");
                _tolerance = value;
            }
        }

        [XmlAttribute("ChallengeSize")]
        public int ChallengeSize
        {
            get
            {
                return _challengesize;
            }
            set
            {
                if ((value != 16) && (value != 32) && (value != 48) && (value != 64))
                    throw new InvalidDataException("WebAuthN Configuration : Invalid Challenge Size value ! ");
                _challengesize = value;
            }
        }

        [XmlAttribute("ServerDomain")]
        public string ServerDomain
        {
            get
            {
                return _serverdomain;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new InvalidDataException("WebAuthN Configuration : Invalid Server Domain value ! ");
                _serverdomain = value;
            }
        }

        [XmlAttribute("ServerName")]
        public string ServerName
        {
            get
            {
                return _servername;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new InvalidDataException("WebAuthN Configuration : Invalid Server Name value ! ");
                _servername = value;
            }
        }

        [XmlAttribute("ServerIcon")]
        public string ServerIcon { get; set; }

        [XmlAttribute("Origin")]
        public string Origin
        {
            get
            {
                return _origin;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new InvalidDataException("WebAuthN Configuration : Invalid Server Origin value ! ");
                _origin = value;
            }
        }

        [XmlAttribute("RequireValidAttestationRoot")]
        public bool RequireValidAttestationRoot { get; set; }

        [XmlAttribute("ShowPII")]
        public bool ShowPII { get; set; } = false;
    }

    /// <summary>
    /// WebAuthNProviderOptions class implementation
    /// </summary>
    public class WebAuthNProviderOptions : IWebAuthNOptions
    {
        [XmlAttribute("AuthenticatorAttachment")]
        public string AuthenticatorAttachment { get; set; } = "platform";

        [XmlAttribute("AttestationConveyancePreference")]
        public string AttestationConveyancePreference { get; set; } = "direct";

        [XmlAttribute("UserVerificationRequirement")]
        public string UserVerificationRequirement { get; set; } = "preferred";

        [XmlAttribute("Extensions")]
        public bool Extensions { get; set; } = true;

        [XmlAttribute("UserVerificationIndex")]
        public bool UserVerificationIndex { get; set; } = true;

        [XmlAttribute("Location")]
        public bool Location { get; set; } = false;

        [XmlAttribute("UserVerificationMethod")]
        public bool UserVerificationMethod { get; set; } = true;

        [XmlAttribute("RequireResidentKey")]
        public bool RequireResidentKey { get; set; } = false;


        [XmlIgnore]
        public bool? HmacSecret { get; set; } = null;

        [XmlAttribute("HmacSecret")]
        public string HmacSecretAsText
        {
            get { return (HmacSecret.HasValue) ? HmacSecret.ToString() : null; }
            set { HmacSecret = !string.IsNullOrEmpty(value) ? bool.Parse(value) : default(bool?); }
        }


        [XmlIgnore]
        public WebAuthNUserVerification? CredProtect { get; set; } = null;

        [XmlAttribute("CredProtect")]
        public string CredProtectAsText
        {
            get { return (CredProtect.HasValue) ? CredProtect.ToString() : null; }
            set
            {
                WebAuthNUserVerification parsed;
                if (Enum.TryParse<WebAuthNUserVerification>(value, out parsed))
                    CredProtect = parsed;
                else
                    CredProtect = null;
               // var res = Enum.TryParse<WebAuthNUserVerification>(value, out var CredProtect) ? CredProtect : (WebAuthNUserVerification?)null;
            }
        }

        [XmlIgnore]
        public bool? EnforceCredProtect { get; set; } = null;

        [XmlAttribute("EnforceCredProtect")]
        public string EnforceCredProtectAsText
        {
            get { return (EnforceCredProtect.HasValue) ? EnforceCredProtect.ToString() : null; }
            set { EnforceCredProtect = !string.IsNullOrEmpty(value) ? bool.Parse(value) : default(bool?); }
        }
    }

    #endregion

    #region Hosts
    /// <summary>
    /// Hosts class implementation
    /// </summary>
    public class Hosts
    {
        public Hosts()
        {
            ActiveDirectoryHost = new ADDSHost();
            SQLServerHost = new SQLServerHost();
            CustomStoreHost = new CustomStoreHost();
            ADFSFarm = new ADFSFarmHost();
        }

        [XmlElement("SQLServer")]
        public SQLServerHost SQLServerHost { get; set; }

        [XmlElement("ActiveDirectory")]
        public ADDSHost ActiveDirectoryHost { get; set; }

        [XmlElement("CustomStore")]
        public CustomStoreHost CustomStoreHost { get; set; }

        [XmlElement("ADFS")]
        public ADFSFarmHost ADFSFarm { get; set; }
    }

    /// <summary>
    /// SendMailFileName class implementation
    /// </summary>
    public class SendMailFileName
    {
        private CultureInfo info;
        /// <summary>
        /// constructor 
        /// </summary>
        public SendMailFileName()
        {

        }

        /// <summary>
        /// constructor 
        /// </summary>
        public SendMailFileName(int lcid, string filename, bool enabled = true)
        {
            this.info = new CultureInfo(lcid);
            this.FileName = filename;
            this.Enabled = enabled;
        }

        [XmlAttribute("LCID")]
        public int LCID
        {
            get { return this.info.LCID; }
            set { this.info = new CultureInfo(value); }
        }

        [XmlAttribute("FileName")]
        public string FileName { get; set; }

        [XmlAttribute("Enabled")]
        public bool Enabled { get; set; }

        public int ParentLCID
        {
            get
            {
                if (info == null)
                    return 0;
                if (info.Parent != null)
                    return info.Parent.LCID;
                else
                    return 0;
            }
        }

    }

    /// <summary>
    /// ADFSFarmHost class implementation
    /// </summary>
    public class ADFSFarmHost
    {
        /// <summary>
        /// IsInitialized property
        /// </summary>
        [XmlAttribute("IsInitialized")]
        public bool IsInitialized { get; set; } = false;

        /// <summary>
        /// BehaviorLevel property
        /// </summary>
        [XmlAttribute("CurrentFarmBehavior")]
        public int CurrentFarmBehavior { get; set; } = 1;

        /// <summary>
        /// BehaviorLevel property
        /// </summary>
        [XmlAttribute("FarmIdentifier")]
        public string FarmIdentifier { get; set; }

        [XmlArray("Servers")]
        [XmlArrayItem("Server", Type = typeof(ADFSServerHost))]
        public List<ADFSServerHost> Servers { get; set; } = new List<ADFSServerHost>();
    }

    /// <summary>
    /// SQLServerHost class implementation
    /// </summary>
    public class SQLServerHost: BaseDataHost
    {
        [XmlAttribute("ConnectionString")]
        public string ConnectionString {get; set; }

        [XmlAttribute("KeyName")]
        public string KeyName { get; set; } = "adfsmfa";

        [XmlAttribute("CertReuse")]
        public bool CertReuse { get; set; }

        [XmlAttribute("ThumbPrint")]
        public string ThumbPrint { get; set; } = Thumbprint.Demo;

        [XmlAttribute("CertificateValidity")]
        public int CertificateValidity { get; set; } = 5;

        [XmlAttribute("IsAlwaysEncrypted")]
        public bool IsAlwaysEncrypted { get; set; } = false;

        [XmlAttribute("MaxRows")]
        public int MaxRows { get; set; } = 10000;
    }

    /// <summary>
    /// ADDSHostForest class implementation
    /// </summary>
    public class ADDSHostForest
    {
        public bool IsRoot { get; set; }
        public string ForestDNS { get; set; }
        public List<string> TopLevelNames = new List<string>();
    }

    /// <summary>
    /// BaseDataHost class implementation
    /// </summary>
    public abstract class BaseDataHost
    {

    }

    /// <summary>
    /// ADDSHost class implementation
    /// </summary>
    public class ADDSHost: BaseDataHost
    {
        private string _domainaddress = string.Empty;
        private bool _isbinded = false;

        /// <summary>
        /// ADDSHost constructor
        /// </summary>
        public ADDSHost()
        {
            _isbinded = false;
        }

        /// <summary>
        /// Bind method implementation
        /// </summary>
        public void Bind()
        {
            if (_isbinded)
                return;
            _isbinded = true;

            Forests.Clear();
            Forest currentforest = Forest.GetCurrentForest();
            ADDSHostForest root = new ADDSHostForest
            {
                IsRoot = true,
                ForestDNS = currentforest.Name
            };
            Forests.Add(root);
            foreach (ForestTrustRelationshipInformation trusts in currentforest.GetAllTrustRelationships())
            {
                ADDSHostForest sub = new ADDSHostForest
                {
                    IsRoot = false,
                    ForestDNS = trusts.TargetName
                };
                foreach (TopLevelName t in trusts.TopLevelNames)
                {
                    if (t.Status == TopLevelNameStatus.Enabled)
                        sub.TopLevelNames.Add(t.Name);
                }
                Forests.Add(sub);
            }
        }

        /// <summary>
        /// GetForestForUser method implementation
        /// </summary>
        public string GetForestForUser(string account)
        {
            string result = string.Empty;
            switch (ClaimsUtilities.IdentityClaimTag)
            {
                case MFASecurityClaimTag.Upn:
                    string foresttofind = account.Substring(account.IndexOf('@') + 1);
                    foreach (ADDSHostForest f in Forests)
                    {
                        if (f.IsRoot)
                            result = f.ForestDNS;
                        else
                        {
                            foreach (string s in f.TopLevelNames)
                            {
                                if (s.ToLower().Equals(foresttofind.ToLower()))
                                {
                                    result = s;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case MFASecurityClaimTag.WindowsAccountName:                   
                    result = account.Substring(0, account.IndexOf('\\') );
                    break;
            }
            return result;
        }

        [XmlIgnore]
        public List<ADDSHostForest> Forests { get; } = new List<ADDSHostForest>();

        #region ADDS Connection attributes
        [XmlAttribute("DomainAddress")]
        public string DomainAddress
        {
            get
            {
                return _domainaddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _domainaddress = value.Replace(@"ldap://", @"LDAP://").Replace(@"ldaps://", @"LDAPS://");
                }
                else
                    _domainaddress = value;
            }
        }

        public string DomainName
        {
            get
            {
                return _domainaddress.Replace("LDAP://", "").Replace("LDAPS://","").Replace(":389","").Replace(":686","");
            }
        }

        [XmlAttribute("Account")]
        public string Account
        {
            get;
            set;
        }

        [XmlAttribute("password")]
        public string Password
        {
            get;
            set;
        }
        #endregion

        [XmlAttribute("keyAttribute")]
        public string KeyAttribute { get; set; } = "msDS-cloudExtensionAttribute10";

        [XmlAttribute("mailAttribute")]
        public string MailAttribute { get; set; } = "msDS-cloudExtensionAttribute11";

        [XmlAttribute("phoneAttribute")]
        public string PhoneAttribute { get; set; } = "msDS-cloudExtensionAttribute12";

        [XmlAttribute("pinAttribute")]
        public string PinAttribute { get; set; } = "msDS-cloudExtensionAttribute15";

        [XmlAttribute("methodAttribute")]
        public string MethodAttribute { get; set; } = "msDS-cloudExtensionAttribute13";

        [XmlAttribute("overridemethodAttribute")]
        public string OverrideMethodAttribute { get; set; } = "msDS-cloudExtensionAttribute14";

        [XmlAttribute("totpEnabledAttribute")]
        public string TotpEnabledAttribute { get; set; } = "msDS-cloudExtensionAttribute18";

        [XmlAttribute("publickeyAttribute")]
        public string PublicKeyCredentialAttribute { get; set; } = "otherMailBox";

        [XmlAttribute("ClientCertificateAttribute")]
        public string ClientCertificateAttribute { get; set; } = "msDS-cloudExtensionAttribute16";

        [XmlAttribute("RSACertificateAttribute")]
        public string RSACertificateAttribute { get;  set; } = "msDS-cloudExtensionAttribute17";

        [XmlAttribute("MaxRows")]
        public int MaxRows { get; set; } = 10000;

        [XmlAttribute("UseSSL")]
        public bool UseSSL { get; set; } = false;

        /// <summary>
        /// ApplyAttributesTemplate method implementation
        /// </summary>
        public void ApplyAttributesTemplate(ADDSTemplateKind kind)
        {
            switch (kind)
            {
                case ADDSTemplateKind.AllSchemaVersions:
                    KeyAttribute = "msDS-cloudExtensionAttribute10";
                    MailAttribute = "msDS-cloudExtensionAttribute11";
                    PhoneAttribute = "msDS-cloudExtensionAttribute12";
                    PinAttribute = "msDS-cloudExtensionAttribute15";
                    MethodAttribute = "msDS-cloudExtensionAttribute13";
                    OverrideMethodAttribute = "msDS-cloudExtensionAttribute14";
                    TotpEnabledAttribute = "msDS-cloudExtensionAttribute18";
                    ClientCertificateAttribute = "msDS-cloudExtensionAttribute16";
                    RSACertificateAttribute = "msDS-cloudExtensionAttribute17";
                    PublicKeyCredentialAttribute = "otherMailBox";

                    break;
                case ADDSTemplateKind.Windows2016Schemaversion:
                    KeyAttribute = "msDS-cloudExtensionAttribute10";
                    MailAttribute  = "msDS-cloudExtensionAttribute11";
                    PhoneAttribute  = "msDS-cloudExtensionAttribute12";
                    PinAttribute  = "msDS-cloudExtensionAttribute15";
                    MethodAttribute  = "msDS-cloudExtensionAttribute13";
                    OverrideMethodAttribute = "msDS-cloudExtensionAttribute14";
                    TotpEnabledAttribute  = "msDS-cloudExtensionAttribute18";
                    ClientCertificateAttribute = "msDS-cloudExtensionAttribute16";
                    RSACertificateAttribute = "msDS-cloudExtensionAttribute17";
                    PublicKeyCredentialAttribute = "msDS-KeyCredentialLink"; 
                    break;
                case ADDSTemplateKind.MFASchemaVersion:
                    KeyAttribute = "MFA-TOTPKey";
                    MailAttribute = "MFA-TOTPEmail";
                    PhoneAttribute = "MFA-TOTPExternal";
                    PinAttribute = "MFA-PinCode";
                    MethodAttribute = "MFA-SelectedMethod";
                    OverrideMethodAttribute = "MFA-SpecificMethod";
                    TotpEnabledAttribute = "MFA-EnabledStatus";
                    PublicKeyCredentialAttribute = "MFA-WebAuthNCredential";
                    ClientCertificateAttribute = "MFA-ClientCertificate";
                    RSACertificateAttribute = "MFA-RSACertificate";
                    break;
            }
        }
    }

    /// <summary>
    /// CustomStoreHost class implementation
    /// </summary>
    public class CustomStoreHost: BaseDataHost
    {
        private XmlCDataSection _cdata;

        [XmlAttribute("ConnectionString")]
        public string ConnectionString { get; set; }

        [XmlAttribute("MaxRows")]
        public int MaxRows { get; set; } = 10000;

        [XmlAttribute("DataRepositoryFullyQualifiedImplementation")]
        public string DataRepositoryFullyQualifiedImplementation { get; set; } = "Neos.IdentityServer.MultiFactor.Data.InMemoryDataRepositoryService, Neos.IdentityServer.MultiFactor.Repository.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";

        [XmlAttribute("KeysRepositoryFullyQualifiedImplementation")]
        public string KeysRepositoryFullyQualifiedImplementation { get; set; } = "Neos.IdentityServer.MultiFactor.Data.InMemoryKeys2RepositoryService, Neos.IdentityServer.MultiFactor.Repository.Samples, Version=3.0.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";

        [XmlElement("Parameters", typeof(XmlCDataSection))]
        public XmlCDataSection Parameters
        {
            get
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                return _cdata;
            }
            set
            {
                if (_cdata == null)
                {
                    XmlDocument doc = new XmlDocument();
                    _cdata = doc.CreateCDataSection(null);
                }
                _cdata.Data = value.Data;
            }

        }
    }

    /// <summary>
    /// ADFSServerHost class implementation
    /// </summary>
    public class ADFSServerHost
    {
        /// <summary>
        /// FQDN property
        /// </summary>
        [XmlAttribute("FQDN")]
        public string FQDN
        {
            get;
            set;
        }

        /// <summary>
        /// BehaviorLevel property
        /// </summary>
        [XmlAttribute("BehaviorLevel")]
        public int BehaviorLevel
        {
            get;
            set;
        }

        /// <summary>
        /// HeartbeatTmeStamp property
        /// </summary>
        [XmlAttribute("HeartbeatTmeStamp")]
        public DateTime HeartbeatTmeStamp
        {
            get;
            set;
        }

        /// <summary>
        /// NodeType property
        /// </summary>
        [XmlAttribute("NodeType")]
        public string NodeType
        {
            get;
            set;
        }

        /// <summary>
        /// CurrentVersion property
        /// </summary>
        [XmlAttribute("CurrentVersion")]
        public string CurrentVersion
        {
            get;
            set;
        }

        /// <summary>
        /// CurrentVersion property
        /// </summary>
        [XmlAttribute("ProductName")]
        public string ProductName
        {
            get;
            set;
        }

        /// <summary>
        /// InstallationType property
        /// </summary>
        [XmlAttribute("InstallationType")]
        public string InstallationType
        {
            get;
            set;
        }

        /// <summary>
        /// CurrentBuild property
        /// </summary>
        [XmlAttribute("CurrentBuild")]
        public int CurrentBuild
        {
            get;
            set;
        }

        /// <summary>
        /// CurrentMajorVersionNumber property
        /// </summary>
        [XmlAttribute("CurrentMajorVersionNumber")]
        public int CurrentMajorVersionNumber
        {
            get;
            set;
        }

        /// <summary>
        /// CurrentMinorVersionNumber property
        /// </summary>
        [XmlAttribute("CurrentMinorVersionNumber")]
        public int CurrentMinorVersionNumber
        {
            get;
            set;
        }

        /// <summary>
        /// MachineName property
        /// </summary>
        [XmlIgnore]
        public string MachineName
        {
            get
            {
                string[] svr = FQDN.Split('.');
                if (svr.Length >= 1)
                    return svr[0];
                else
                    return string.Empty;
            }
        }
    }

    /// <summary>
    /// ADFSWSManager class implementation
    /// </summary>
    public class ADFSWSManager
    {
        /// <summary>
        /// Port property
        /// </summary>
        [XmlAttribute("Port")]
        public int Port { get; set; } = 5985;

        /// <summary>
        /// AppName property
        /// </summary>
        [XmlAttribute("AppName")]
        public string AppName { get; set; } = "wsman";

        /// <summary>
        /// AppName property
        /// </summary>
        [XmlAttribute("ShellUri")]
        public string ShellUri { get; set; } = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";

        /// <summary>
        /// Port property
        /// </summary>
        [XmlAttribute("TimeOut")]
        public int TimeOut { get; set; } = 5000;
    }
    #endregion
}
