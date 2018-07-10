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
using System.Collections.Generic;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.IdentityServer.Web.Authentication.External;
using System.Web;

namespace Neos.IdentityServer.MultiFactor
{

    #region MFAConfig
    public enum UserTemplateMode
    {
        Free = 0,                        // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.BypassUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
        Open = 1,                        // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
        Default = 2,                     // (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);
        Managed = 3,                     // (UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword);
        Strict = 4,                      // (UserFeaturesOptions.AllowProvideInformations);
        Administrative = 5,              // (UserFeaturesOptions.AdministrativeMode);
        Custom = 6                       // Empty 
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

        #region MFA Enabled
        /// <summary>
        /// IsMFARequired method implementation
        /// </summary>
        public static bool IsMFARequired(this UserFeaturesOptions options)
        {
            return ((options == UserFeaturesOptions.NoSet) || options.HasFlag(UserFeaturesOptions.AdministrativeMode));
        }

        /// <summary>
        /// IsMFAAllowed method implementation
        /// </summary>
        public static bool IsMFAAllowed(this UserFeaturesOptions options)
        {
            return options.HasFlag(UserFeaturesOptions.AllowDisabled);
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
        /// IsRegistraitonRequired method implementation
        /// </summary>
        public static bool IsRegistrationRequired(this UserFeaturesOptions options)
        {
            return options.HasFlag(UserFeaturesOptions.AllowProvideInformations);
        }

        /// <summary>
        /// RegistrationAllowed method implementation
        /// </summary>
        public static bool IsRegistrationAllowed(this UserFeaturesOptions options)
        {
            return (options.HasFlag(UserFeaturesOptions.AllowUnRegistered) && !options.HasFlag(UserFeaturesOptions.AllowProvideInformations) && !options.HasFlag(UserFeaturesOptions.AdministrativeMode) && (options != UserFeaturesOptions.NoSet));
        }

        /// <summary>
        /// IsRegistrationNotRequired method implementation
        /// </summary>
        public static bool IsRegistrationNotRequired(this UserFeaturesOptions options)
        {
            return (((options == UserFeaturesOptions.NoSet) || options.HasFlag(UserFeaturesOptions.AdministrativeMode)) || options.HasFlag(UserFeaturesOptions.BypassUnRegistered)) && !options.HasFlag(UserFeaturesOptions.AllowProvideInformations);
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
        private bool _isdirty = false;
        private string _country = "fr";
        private int _deliveryWindow = 300;
        private int _pinlength = 4;
        private int _defaultpin = 0;

        private bool _useActiveDirectory = true;
        private bool _customUpdatePassword = true;
        private UserFeaturesOptions _userFeatures = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword); // Default Mode
        private ConfigAdvertising _advertising = new ConfigAdvertising(1, 31);
        private string _issuer;
       

        /// <summary>
        /// Constructor
        /// </summary>
        public MFAConfig()
        {
            this.Hosts = new Hosts();
            this.Hosts.ActiveDirectoryHost = new ADDSHost();
            this.Hosts.SQLServerHost = new SQLServerHost();
            this.Hosts.ADFSFarm = new ADFSFarmHost();
            this.KeysConfig = new KeysManagerConfig();
            this.KeysConfig.ExternalKeyManager = new ExternalKeyManagerConfig();
            this.OTPProvider = new OTPProvider();
            this.MailProvider = new MailProvider();
            this.ExternalProvider = new ExternalOTPProvider();
            this.AzureProvider = new AzureProvider();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MFAConfig(bool initializedefaults): this()
        {
            if (initializedefaults)
            {
                _isdirty = false;
                DeliveryWindow = 300;
                Issuer = "MFA";
                PinLength = 4;
                DefaultPin = 0;

                UseActiveDirectory = true;
                CustomUpdatePassword = true;
                DefaultCountryCode = "fr";
                AdminContact = "adminmfa@contoso.com";
                UserFeatures = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);

                KeysConfig.KeyGenerator = KeyGeneratorMode.ClientSecret512;
                KeysConfig.KeyFormat = SecretKeyFormat.RNG;
                KeysConfig.KeySize = KeySizeMode.KeySize1024;
                KeysConfig.CertificateThumbprint = Thumbprint.Empty;
                KeysConfig.ExternalKeyManager.FullQualifiedImplementation = "Neos.IdentityServer.Multifactor.Keys.CustomKeyManager, Neos.IdentityServer.Multifactor.Keys.Sample, Version=2.2.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
                KeysConfig.ExternalKeyManager.Parameters.Data = "Persist Security Info=False;Integrated Security=SSPI;Initial Catalog=yourdatabase;Data Source=yourserverinstance";

                OTPProvider.Enabled = true;
                OTPProvider.TOTPShadows = 2;
                OTPProvider.Algorithm = HashMode.SHA1;
                OTPProvider.EnrollWizard = true;
                OTPProvider.PinRequired = false;

                ExternalProvider.Enabled = false;
                ExternalProvider.EnrollWizard = true;
                ExternalProvider.PinRequired = false;
                ExternalProvider.Company = "Contoso";
                ExternalProvider.FullQualifiedImplementation = "Neos.IdentityServer.Multifactor.SMS.NeosSMSProvider, Neos.IdentityServer.Multifactor.SMS.Azure, Version=2.2.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
                ExternalProvider.IsTwoWay = false;
                ExternalProvider.Sha1Salt = "0x1230456789ABCDEF";
                ExternalProvider.Parameters.Data = "LICENSE_KEY = AZURELICKEY, GROUP_KEY = 01234567891011121314151617181920, CERT_THUMBPRINT = " + Thumbprint.Demo;

                AzureProvider.TenantId = "contoso.onmicrosoft.com";
                AzureProvider.ThumbPrint = Thumbprint.Demo;
                AzureProvider.Enabled = false;
                AzureProvider.EnrollWizard = false;
                AzureProvider.PinRequired = false;

                Hosts.SQLServerHost.ConnectionString = "Password=yourpassword;Persist Security Info=True;User ID=yoursqlusername;Initial Catalog=yourdatabasename;Data Source=yoursqlserver\\yourinstance";

                MailProvider.From = "sender.email@contoso.com";
                MailProvider.UserName = "user.name@contoso.com";
                MailProvider.Password = "yourpass";
                MailProvider.Host = "smtp.contoso.com";
                MailProvider.Port = 587;
                MailProvider.UseSSL = true;
                MailProvider.Company = "Contoso";
                MailProvider.Enabled = true;
                MailProvider.EnrollWizard = true;
                MailProvider.PinRequired = false;

            }
        }

        /// <summary>
        /// UpgradeDefaults method
        /// </summary>
        public void UpgradeDefaults()
        {
            _isdirty = false;
            if (DeliveryWindow <= 0)
                DeliveryWindow = 300;
            if (PinLength <= 0)
                PinLength = 4;
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
            UserFeatures = (UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword);

            if (string.IsNullOrEmpty(Hosts.SQLServerHost.ConnectionString))
                Hosts.SQLServerHost.ConnectionString = "Password=yourpassword;Persist Security Info=True;User ID=yoursqlusername;Initial Catalog=yourdatabasename;Data Source=yoursqlserver\\yourinstance";

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
                KeysConfig.KeyGenerator = KeyGeneratorMode.ClientSecret512;
                KeysConfig.KeyFormat = SecretKeyFormat.RNG;
                KeysConfig.KeySize = KeySizeMode.KeySize1024;
                if (!Thumbprint.IsValid(this.KeysConfig.CertificateThumbprint))
                    KeysConfig.CertificateThumbprint = Thumbprint.Empty;
                if (this.KeysConfig.ExternalKeyManager != null)
                {
                    if (string.IsNullOrEmpty(this.KeysConfig.ExternalKeyManager.FullQualifiedImplementation))
                        KeysConfig.ExternalKeyManager.FullQualifiedImplementation = "Neos.IdentityServer.Multifactor.Keys.CustomKeyManager, Neos.IdentityServer.Multifactor.Keys.Sample, Version=2.2.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
                    if (this.KeysConfig.ExternalKeyManager.Parameters.Length == 0)
                        KeysConfig.ExternalKeyManager.Parameters.Data = "Persist Security Info=False;Integrated Security=SSPI;Initial Catalog=yourdatabase;Data Source=yourserverinstance";
                }
            }
            if (this.ExternalProvider != null)
            {
                if (ExternalProvider.FullQualifiedImplementation.ToLower().StartsWith("neos.identityserver.multifactor.sms.smscall"))
                    ExternalProvider.FullQualifiedImplementation = "Neos.IdentityServer.Multifactor.SMS.NeosSMSProvider, Neos.IdentityServer.Multifactor.SMS.Azure, Version=2.2.0.0, Culture=neutral, PublicKeyToken=175aa5ee756d2aa2";
                if (string.IsNullOrEmpty(this.ExternalProvider.Company))
                    this.ExternalProvider.Company = "Contoso";
                if (string.IsNullOrEmpty(this.ExternalProvider.Sha1Salt))
                    this.ExternalProvider.Sha1Salt = "0x1230456789ABCDEF";
                if (this.ExternalProvider.Parameters.Length == 0)
                    ExternalProvider.Parameters.Data = "LICENSE_KEY = AZURELICKEY, GROUP_KEY = 01234567891011121314151617181920, CERT_THUMBPRINT = " + Thumbprint.Demo;
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
        public bool IsDirty
        {
            get { return _isdirty; }
            set { _isdirty = value; }
        }

        [XmlAttribute("DeliveryWindow")]
        public int DeliveryWindow
        {
            get { return _deliveryWindow; }
            set { _deliveryWindow = value; }
        }

        [XmlAttribute("PinLength")]
        public int PinLength
        {
            get { return _pinlength; }
            set { _pinlength = value; }
        }

        [XmlAttribute("DefaultPin")]
        public int DefaultPin
        {
            get { return _defaultpin; }
            set { _defaultpin = value; }
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

        [XmlAttribute("UseActiveDirectory")]
        public bool UseActiveDirectory
        {
            get { return _useActiveDirectory; }
            set { _useActiveDirectory = value; }
        }

        [XmlAttribute("CustomUpdatePassword")]
        public bool CustomUpdatePassword
        {
            get { return _customUpdatePassword; }
            set { _customUpdatePassword = value; }
        }

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
        public UserFeaturesOptions UserFeatures
        {
            get { return _userFeatures; }
            set { _userFeatures = value | UserFeaturesOptions.AllowEnrollment; }
        }

        [XmlElement("ActivationAdvertising")]
        public ConfigAdvertising AdvertisingDays
        {
            get { return _advertising; }
            set { _advertising = value; }
        }
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
    }
    #endregion

    #region KeysManagerConfig
    /// <summary>
    /// MFAKeysConfig class implementation
    /// </summary>
    public class KeysManagerConfig
    {
        private SecretKeyFormat _keyformat = SecretKeyFormat.RSA;
        private KeyGeneratorMode _fgen = KeyGeneratorMode.ClientSecret512;
        private KeySizeMode _ksize = KeySizeMode.KeySize1024;
        private string _thumbprint;
        private ExternalKeyManagerConfig _ckeymgr;
        private int _validity = 5;

        [XmlAttribute("KeyGenerator")]
        public KeyGeneratorMode KeyGenerator
        {
            get { return _fgen; }
            set { _fgen = value; }
        }

        [XmlAttribute("KeyFormat")]
        public SecretKeyFormat KeyFormat
        {
            get { return _keyformat; }
            set { _keyformat = value; }
        }

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
        public int CertificateValidity
        {
            get { return _validity; }
            set { _validity = value; }
        }

        [XmlAttribute("KeySize")]
        public KeySizeMode KeySize
        {
            get { return _ksize; }
            set { _ksize = value; }
        }

        [XmlElement("ExternalKeyManager")]
        public ExternalKeyManagerConfig ExternalKeyManager
        {
            get { return _ckeymgr; }
            set { _ckeymgr = value; }
        }
    }

    public class ExternalKeyManagerConfig
    {
        private string _class;
        private XmlCDataSection _cdata;

        [XmlAttribute("FullQualifiedImplementation")]
        public string FullQualifiedImplementation
        {
            get { return _class; }
            set { _class = value; }
        }

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

    #region Providers Parameters
    /// <summary>
    /// ExternalProvider contract
    /// </summary>
    public abstract class BaseProviderParams
    {
    }

    /// <summary>
    /// OTPProvider contract
    /// </summary>
    public class OTPProviderParams: BaseProviderParams
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OTPProviderParams() {}

        /// <summary>
        /// Constructor initialized
        /// </summary>
        public OTPProviderParams(OTPProvider prov): base()
        {
            this.TOTPShadows = prov.TOTPShadows;
            this.Algorithm = prov.Algorithm;
            this.Enabled = prov.Enabled;
            this.PinRequired = prov.PinRequired;
            this.EnrollWizard = prov.EnrollWizard;
        }

        public int TOTPShadows { get; set; }
        public HashMode Algorithm { get; set; }
        public bool Enabled { get; set; }
        public bool PinRequired { get; set; }
        public bool EnrollWizard { get; set; }
    }

    /// <summary>
    /// MailProviderParams contract
    /// </summary>
    public class MailProviderParams : BaseProviderParams
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MailProviderParams() {}

        /// <summary>
        /// Constructor initialized
        /// </summary>
        public MailProviderParams(MailProvider prov): base()
        {
            Data = prov;
            Enabled = prov.Enabled;
            PinRequired = prov.PinRequired;
            EnrollWizard = prov.EnrollWizard;
        }

        public MailProvider Data { get; set; }
        public bool Enabled { get; set; }
        public bool PinRequired { get; set; }
        public bool EnrollWizard { get; set; }
    }

    /// <summary>
    /// SMSProviderParams contract
    /// </summary>
    public class ExternalProviderParams : BaseProviderParams
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ExternalProviderParams() {}

        /// <summary>
        /// Constructor initialized
        /// </summary>
        public ExternalProviderParams(ExternalOTPProvider prov): base()
        {
            Data = prov;
            Enabled = prov.Enabled; 
            PinRequired = prov.PinRequired;
            EnrollWizard = prov.EnrollWizard;
        }

        public ExternalOTPProvider Data { get; set; }
        public bool Enabled { get; set; }
        public bool PinRequired { get; set; }
        public bool EnrollWizard { get; set; }
    }

    /// <summary>
    /// AzureProviderParams contract
    /// </summary>
    public class AzureProviderParams : BaseProviderParams
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AzureProviderParams() {}

        /// <summary>
        /// Constructor initialized
        /// </summary>
        public AzureProviderParams(AzureProvider prov, string adfsid, string company): base()
        {
            Data = prov;
            this.ADFSIdentifier = adfsid;
            this.CompanyName = company;
            this.Enabled = prov.Enabled;
            this.PinRequired = prov.PinRequired;
            this.EnrollWizard = false;
        }

        public AzureProvider Data { get; set; }
        public string ADFSIdentifier { get; set; }
        public string CompanyName { get; set; }
        public bool Enabled { get; set; }
        public bool PinRequired { get; set; }
        public bool EnrollWizard { get; set; }
    }
    #endregion

    #region ExternalOTPProvider
    /// <summary>
    /// ExternalOTPProvider contract
    /// </summary>
    public class ExternalOTPProvider
    {
        private string _comp = "your company description";
        private string _class;
        private XmlCDataSection _cdata;
        private string _sha1 = "0x123456789";
        private bool _istwoway = false;
        private int _timeout = 300;
        private bool _requiredpin = false;
        private bool _enabled = true;
        private bool _enrollwizard = true;

        [XmlAttribute("Enabled")]
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        [XmlAttribute("PinRequired")]
        public bool PinRequired
        {
            get { return _requiredpin; }
            set { _requiredpin = value; }
        }

        [XmlAttribute("EnrollWizard")]
        public bool EnrollWizard
        {
            get { return _enrollwizard; }
            set { _enrollwizard = value; }
        }

        [XmlAttribute("Company")]
        public string Company
        {
            get { return _comp; }
            set { _comp = value; }
        }

        [XmlAttribute("Sha1Salt")]
        public string Sha1Salt
        {
            get { return _sha1; }
            set { _sha1 = value; }
        }

        [XmlAttribute("FullQualifiedImplementation")]
        public string FullQualifiedImplementation
        {
            get { return _class; }
            set { _class = value; }
        }

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
        public bool IsTwoWay
        {
            get { return _istwoway; }
            set { _istwoway = value; }
        }

        [XmlAttribute("Timeout")]
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }
    }
    #endregion

    #region AzureProvider
    /// <summary>
    /// AzureProvider contract
    /// </summary>
    public class AzureProvider
    {
        private string _tenantid = "yourcompany.onnmicrosoft.com";
        private string _thumbprint = Thumbprint.Demo;
        private bool _requiredpin = false;
        private bool _enabled = false;
        private bool _enrollwizard = false;

        [XmlAttribute("Enabled")]
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        [XmlAttribute("PinRequired")]
        public bool PinRequired
        {
            get { return _requiredpin; }
            set { _requiredpin = value; }
        }

        [XmlAttribute("EnrollWizard")]
        public bool EnrollWizard
        {
            get { return _enrollwizard; }
            set { _enrollwizard = value; }
        }

        [XmlAttribute("TenantId")]
        public string TenantId
        {
            get { return _tenantid; }
            set { _tenantid = value; }
        }

        [XmlAttribute("ThumbPrint")]
        public string ThumbPrint
        {
            get { return _thumbprint; }
            set { _thumbprint = value; }
        }
    }
    #endregion

    #region MailProvider
    /// <summary>
    /// MailProvider class implementation
    /// </summary>
    public class MailProvider
    {
        private string _comp = "your company description";
        private bool _requiredpin = false;
        private bool _enabled = true;
        private bool _enrollwizard = true;

        [XmlAttribute("Enabled")]
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        [XmlAttribute("PinRequired")]
        public bool PinRequired
        {
            get { return _requiredpin; }
            set { _requiredpin = value; }
        }

        [XmlAttribute("EnrollWizard")]
        public bool EnrollWizard
        {
            get { return _enrollwizard; }
            set { _enrollwizard = value; }
        }
        [XmlAttribute("from")]
        public string From
        {
            get;
            set;
        }

        [XmlAttribute("username")]
        public string UserName
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

        [XmlAttribute("host")]
        public string Host
        {
            get;
            set;
        }

        [XmlAttribute("port")]
        public int Port
        {
            get;
            set;
        }

        [XmlAttribute("useSSL")]
        public bool UseSSL
        {
            get;
            set;
        }

        [XmlAttribute("Company")]
        public string Company
        {
            get { return _comp; }
            set { _comp = value; }
        }

        [XmlArray("MailOTP")]
        [XmlArrayItem("Template", Type = typeof(SendMailFileName))]
        public List<SendMailFileName> MailOTPContent
        {
            get;
            set;
        }

        [XmlArray("MailInscription")]
        [XmlArrayItem("Template", Type = typeof(SendMailFileName))]
        public List<SendMailFileName> MailAdminContent
        {
            get;
            set;
        }

        [XmlArray("MailSecureKey")]
        [XmlArrayItem("Template", Type = typeof(SendMailFileName))]
        public List<SendMailFileName> MailKeyContent
        {
            get;
            set;
        }
    }
    #endregion

    #region MailProvider
    /// <summary>
    /// OTPProvider class implementation
    /// </summary>
    public class OTPProvider
    {
        private bool _requiredpin = false;
        private bool _enabled = true;
        private bool _enrollwizard = true;
        private int _totpShadows = 2;
        private HashMode _algorithm = HashMode.SHA1;

        [XmlAttribute("Enabled")]
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        [XmlAttribute("PinRequired")]
        public bool PinRequired
        {
            get { return _requiredpin; }
            set { _requiredpin = value; }
        }

        [XmlAttribute("EnrollWizard")]
        public bool EnrollWizard
        {
            get { return _enrollwizard; }
            set { _enrollwizard = value; }
        }

        [XmlAttribute("TOTPShadows")]
        public int TOTPShadows
        {
            get { return _totpShadows; }
            set { _totpShadows = value; }
        }

        [XmlAttribute("Algorithm")]
        public HashMode Algorithm
        {
            get { return _algorithm; }
            set { _algorithm = value; }
        }
    }
    #endregion

    #region Hosts
    /// <summary>
    /// Hosts class implementation
    /// </summary>
    public class Hosts
    {
        private ADFSFarmHost _host;
        public Hosts()
        {
            _host = new ADFSFarmHost();
        }

        [XmlElement("SQLServer")]
        public SQLServerHost SQLServerHost
        {
            get;
            set;
        }

        [XmlElement("ActiveDirectory")]
        public ADDSHost ActiveDirectoryHost
        {
            get;
            set;
        }

        [XmlElement("ADFS")]
        public ADFSFarmHost ADFSFarm
        {
            get { return _host; }
            set { _host = value; }
        }
    }

    /// <summary>
    /// SendMailFileName class implementation
    /// </summary>
    public class SendMailFileName
    {
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
            this.LCID = lcid;
            this.FileName = filename;
            this.Enabled = enabled;
        }

        [XmlAttribute("LCID")]
        public int LCID
        {
            get;
            set;
        }

        [XmlAttribute("FileName")]
        public string FileName
        {
            get;
            set;
        }

        [XmlAttribute("Enabled")]
        public bool Enabled
        {
            get;
            set;
        }

    }

    /// <summary>
    /// SQLServerHost class implementation
    /// </summary>
    public class ADFSFarmHost
    {
        private int _level = 1;
        private bool _isinitialized = false;
        private List<ADFSServerHost> _lst = new List<ADFSServerHost>();

        /// <summary>
        /// IsInitialized property
        /// </summary>
        [XmlAttribute("IsInitialized")]
        public bool IsInitialized
        {
            get { return _isinitialized; }
            set { _isinitialized = value; }
        }

        /// <summary>
        /// BehaviorLevel property
        /// </summary>
        [XmlAttribute("CurrentFarmBehavior")]
        public int CurrentFarmBehavior
        {
            get { return _level; }
            set { _level = value; }
        }

        /// <summary>
        /// BehaviorLevel property
        /// </summary>
        [XmlAttribute("FarmIdentifier")]
        public string FarmIdentifier
        {
            get;
            set;
        }

        [XmlArray("Servers")]
        [XmlArrayItem("Server", Type = typeof(ADFSServerHost))]
        public List<ADFSServerHost> Servers
        {
            get { return _lst; }
            set { _lst = value; }
        }
    }

    /// <summary>
    /// SQLServerHost class implementation
    /// </summary>
    public class SQLServerHost
    {
        [XmlAttribute("ConnectionString")]
        public string ConnectionString 
        {
            get;
            set;
        }
    }

    /// <summary>
    /// ADDSHost class implementation
    /// </summary>
    public class ADDSHost
    {
        string _domainaddress = string.Empty;
        string _keyattr = "msDS-cloudExtensionAttribute10";
        string _mailattr = "msDS-cloudExtensionAttribute11";
        string _phoneattr = "msDS-cloudExtensionAttribute12";
        string _methodattr = "msDS-cloudExtensionAttribute13";
        string _overridemethodattr = "msDS-cloudExtensionAttribute14";
        string _pinattr = "msDS-cloudExtensionAttribute15";
       // string _notifcheckdateattribute = "msDS-cloudExtensionAttribute16";
       // string _TOTPAttr = "msDS-cloudExtensionAttribute17";
        string _TOTPEnabled = "msDS-cloudExtensionAttribute18";

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

        [XmlAttribute("mailattribute")]
        public string mailAttribute
        {
            get { return _mailattr; }
            set { _mailattr = value; }
        }

        [XmlAttribute("keyattribute")]
        public string keyAttribute
        {
            get { return _keyattr; }
            set { _keyattr = value; }
        }

        [XmlAttribute("phoneattribute")]
        public string phoneAttribute
        {
            get { return _phoneattr; }
            set { _phoneattr = value; }
        }

        [XmlAttribute("pinattribute")]
        public string pinattribute
        {
            get { return _pinattr; }
            set { _pinattr = value; }
        }

        [XmlAttribute("methodattribute")]
        public string methodAttribute
        {
            get { return _methodattr; }
            set { _methodattr = value; }
        }

        [XmlAttribute("overridemethodattribute")]
        public string overridemethodAttribute
        {
            get { return _overridemethodattr; }
            set { _overridemethodattr = value; }
        }

        [XmlAttribute("totpEnabledAttribute")]
        public string totpEnabledAttribute
        {
            get { return _TOTPEnabled; }
            set { _TOTPEnabled = value; }
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
    #endregion
}
