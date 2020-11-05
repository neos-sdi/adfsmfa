using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace MFA
{
    /// <summary>
    /// PSPreferredMethod
    /// <para type="synopsis">MFA Preferred method (Choose, Code, Email, External, Azure, Biometrics),  if sibling Provider is active.</para>
    /// <para type="description">MFA Preferred method (Choose, Code, Email, External, Azure, Biometrics).</para>
    /// </summary>    
    public enum PSPreferredMethod
    {
        /// <summary>
        /// <para type="description">Choose, let users to choose between differnet available active modes</para>
        /// </summary>
        Choose = 0,

        /// <summary>
        /// <para type="description">Code, default mode for users is TOTP computed number, available with authenticator Applications on thier mobile device. (MFA : 2 Facors for authentication, What i know (my login/passord) and wahat i hold (my device))</para>
        /// </summary>
        Code = 1,

        /// <summary>
        /// <para type="description">Email, default mode for users is email, an email is sent avec acces code.</para>
        /// </summary>
        Email = 2,

        /// <summary>
        /// <para type="description">External, default mode for users is SMS or any solution developped, this an be async. A Sanmple if provided for Azure MFA (old version from PhoneFactors). You can also watch to the "Quiz Provider Sample"</para>
        /// </summary>
        External = 3,

        /// <summary>
        /// <para type="description">Azure, default mode for users is Azure MFA, Like default Azure MFA Provider in ADFS 2016 and up.</para>
        /// </summary>
        Azure = 4,

        /// <summary>
        /// <para type="description">Biometrics, default mode for users is biometric authentication, based on WebAuthN specification (Not available, but coming soon).</para>
        /// </summary>
        Biometrics = 5,
    }

    /// <summary>
    /// PSStoreMode
    /// <para type="synopsis">MFA Store Kind (ADDS, SQL).</para>
    /// <para type="description">ADDS : ADDSL Store.</para>
    /// <para type="description">SQL : SQL Store.</para>
    /// </summary>    
    public enum PSStoreMode
    {
        /// <summary>
        /// <para type="description">Active Directory Store</para>
        /// </summary>
        ADDS = 0,

        /// <summary>
        /// <para type="description">SQL Server Store.</para>
        /// </summary>
        SQL = 1,

        /// <summary>
        /// <para type="description">Specific implementation for storage.</para>
        /// </summary>
        Custom = 2
    }

    /// <summary>
    /// PSSecurityMode
    /// <para type="synopsis">MFA Security Kind (ROOT, RNG, RSA, CUSTOM, WSMAN).</para>
    /// <para type="description">MFA Security Kind (ROOT, RNG, RSA, CUSTOM, WSMAN).</para>
    /// </summary>    
    public enum PSSecurityMode
    {
        /// <summary>
        /// <para type="description">Security parmaters for RNG Keys.</para>
        /// </summary>
        RNG = 0,

        /// <summary>
        /// <para type="description">Security parmaters for RSA Keys.</para>
        /// </summary>
        RSA = 1,

        /// <summary>
        /// <para type="description">Security parmaters for AES Keys.</para>
        /// </summary>
        AES = 2,

        /// <summary>
        /// <para type="description">Security parmaters for Custom implementations.</para>
        /// </summary>
        CUSTOM = 3,

        /// <summary>
        /// <para type="description">Security parmaters for Biometrics.</para>
        /// </summary>
        BIOMETRIC = 4,

        /// <summary>
        /// <para type="description">Security parmaters for WSMAN Configuration.</para>
        /// </summary>
        WSMAN = 5
    }

    /// <summary>
    /// PSReplayLevel
    /// <para type="synopsis">MFA TOTP Replay feature (Disabled, Intermediate, Full).</para>
    /// <para type="description">Disabled : Replay feature is disabled (default).</para>
    /// <para type="description">Intermediate : TOTP Replay is active, except fomr the same machine.</para>
    /// <para type="description">Full : TOTP Replay is fully active (more secure).</para>
    /// </summary>    
    public enum PSReplayLevel
    {
        /// <summary>
        /// <para type="description">Replay feature is disabled (default)</para>
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// <para type="description">TOTP Replay is active, except from the same machine.</para>
        /// </summary>
        Intermediate = 1,

        /// <summary>
        /// <para type="description">TOTP Replay is fully active (more secure).</para>
        /// </summary>
        Full = 2
    }

    /// <summary>
    /// PSPasswordSync
    /// <para type="synopsis">Kind of credential to update (SuperUser, Mail User, or System passphrase (for encryption).</para>
    /// <para type="description">SuperUser : Change the SuperUser password.</para>
    /// <para type="description">MailUser : Change the mail account password.</para>
    /// <para type="description">SystemPassPhrase : Change Pass Phrase used for encryption.</para>
    /// </summary>    
    public enum PSPasswordSync
    {
        /// <summary>
        /// <para type="description">Encrypt all credentials.</para>
        /// </summary>
        All = 0,

        /// <summary>
        /// <para type="description">Change the SuperUser password.</para>
        /// </summary>
        SuperUser = 1,

        /// <summary>
        /// <para type="description">Change the mail account password.</para>
        /// </summary>
        MailUser = 2,

        /// <summary>
        /// <para type="description">Change Pass Phrase used for encryption.</para>
        /// </summary>
        SystemPassPhrase = 3
    }


    /// <summary>
    /// PSADDSTemplateKind
    /// <para type="synopsis">Kind of the attributes list template (SchemaAll, Schema2016, SchemaMFA).</para>
    /// <para type="description">SchemaAll : Schema template compatible with all ADDS 2012 and up Schema versions.</para>
    /// <para type="description">Schema2016 : Schema template compatible with all ADDS 2016 and up Schema versions (87).</para>
    /// <para type="description">SchemaMFA : Custom Schema template for MFA, yoy must modify Shecma with supplied LDIF scripts.</para>
    /// </summary>    
    public enum PSADDSTemplateKind
    {
        /// <summary>
        /// <para type="description">Schema template compatible with all ADDS 2012 and up Schema versions</para>
        /// </summary>
        SchemaAll = 0,

        /// <summary>
        /// <para type="description">Schema template compatible with all ADDS 2016 and up Schema versions (87)</para>
        /// </summary>
        Schema2016 = 1,

        /// <summary>
        /// <para type="description">Custom Schema template for MFA, yoy must modify Shecma with supplied LDIF scripts</para>
        /// </summary>
        SchemaMFA = 2
    }

    /// <summary>
    /// PSSecretKeyFormat
    /// <para type="synopsis">MFA TOTP key generation mode (RNG, RSA and CUSTOM.</para>
    /// <para type="description">RND (Random,Number Generator) Strong generator for numbers, From 128 bytes to 512 bytes. can be stored in ADDS or SQL default MFA Database.</para>
    /// <para type="description">RSA A Shared Certificate 2048 bytes, including encrypted logon name. can be stored in ADDS or SQL default MFA Database.</para> 
    /// <para type="description">CUSTOM A Certificate 2048 bytes for each user, including encrypted logon name. can only be stored in SQL Keys MFA Database.</para> 
    /// </summary>    
    public enum PSSecretKeyFormat
    {
        /// <summary>
        /// <para type="description">RNG (Ramdom Number Generator).</para>
        /// </summary>
        RNG = 0,

        /// <summary>
        /// <para type="description">RSA encryption.</para>
        /// </summary>
        RSA = 1,

        /// <summary>
        /// <para type="description">AES encryption .</para>
        /// </summary>
        AES = 2,

        /// <summary>
        /// <para type="description">CUSTOM encryption.</para>
        /// </summary>
        CUSTOM = 3
    }

    /// <summary>
    /// PSSecretkeyVersion
    /// <para type="synopsis">MFA TOTP key Encryption Lib version (V1, V2).</para>
    /// <para type="description">V1 Old Encryption Library less secure. for compatibility</para>
    /// <para type="description">New Encryption Library more secure.</para> 
    /// </summary>    
    public enum PSSecretKeyVersion
    {
        /// <summary>
        /// <para type="description">Old Encryption Library less secure, you must select V2 as soon as possible.</para>
        /// </summary>
        V1 = 1,

        /// <summary>
        /// <para type="description">New Encryption Library more secure.</para>
        /// </summary>
        V2= 2
    }

    /// <summary>
    /// PSProviderType
    /// <para type="synopsis">MFA Providers Kinds.</para>
    /// <para type="description">MFA Providers Types (Code, Email, External, Azure, Biometrics).</para>
    /// </summary>    
    public enum PSProviderType
    {
        /// <summary>
        /// <para type="description">Kind for TOTP MFA Provider</para>
        /// </summary>
        Code = 1,

        /// <summary>
        /// <para type="description">Kind for Email MFA Provider</para>
        /// </summary>
        Email = 2,

        /// <summary>
        /// <para type="description">Kind for External / SMS MFA Provider</para>
        /// </summary>
        External = 3,

        /// <summary>
        /// <para type="description">Kind for Azure MFA Provider</para>
        /// </summary>
        Azure = 4,

        /// <summary>
        /// <para type="description">Kind for Biometric MFA Provider</para>
        /// </summary>
        Biometrics = 5
    }         

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
        Free = 0,                        

        /// <summary>
        /// <para type="description">(UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword)</para>
        /// </summary>
        Open = 1,                        

        /// <summary>
        /// <para type="description">(UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword)</para>
        /// </summary>
        Default = 2,

        /// <summary>
        /// <para type="description">(UserFeaturesOptions.ForceEnabled | UserFeaturesOptions.ForceRegistered | UserFeaturesOptions.AllowManageOptions | UserFeaturesOptions.AllowChangePassword)</para>
        /// </summary>
        Mixed = 3,                     

        /// <summary>
        /// <para type="description">(UserFeaturesOptions.BypassDisabled | UserFeaturesOptions.AllowUnRegistered | UserFeaturesOptions.AllowProvideInformations | UserFeaturesOptions.AllowChangePassword)</para>
        /// </summary>
        Managed = 4,                     

        /// <summary>
        /// <para type="description">(UserFeaturesOptions.AdministrativeMode | UserFeaturesOptions.AllowProvideInformations)</para>
        /// </summary>
        Strict = 5,                      

        /// <summary>
        /// <para type="description">(UserFeaturesOptions.AdministrativeMode)</para>
        /// </summary>
        Administrative = 6,               

        /// <summary>
        /// <para type="description">(UserFeaturesOptions.AllowDisabled | UserFeaturesOptions.AllowUnRegistered)</para>
        /// </summary>
        Custom = 7
    }

    /// <summary>
    /// PSForceWizardMode
    /// <para type="synopsis">Fallback mode when user have choosed "I'have no code".</para>
    /// <para type="description">If Enabled or Strict, force user to run Wizard after choose "I'have no code". Only available with PowerShell</para>
    /// </summary>
    public enum PSForceWizardMode
    {
        /// <summary>
        /// <para type="description">Fallback is disabled, this is the default</para>
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// <para type="description">Fallback is enabled, the user can cancel Wizard registration</para>
        /// </summary>
        Enabled = 1,

        /// <summary>
        /// <para type="description">Fallback is strict, the user cannot cancel Wizard registration</para>
        /// </summary>
        Strict = 2
    }

    /// <summary>
    /// PSUserFeaturesOptions
    /// <para type="synopsis">Configuration options for registering or accessing MFA. Must be combined with binary OR.</para>
    /// <para type="description">Configuration options for registering or accessing MFA. Must be combined with binary OR, in MMC you can use Templates</para>
    /// </summary>
    [Flags]
    public enum PSUserFeaturesOptions
    {
        /// <summary>
        /// <para type="description">NoSet, must not be used.</para>
        /// </summary>
        NoSet = 0,

        /// <summary>
        /// <para type="description">BypassUnRegistered, Allow user access if not registered in MFA.</para>
        /// </summary>
        BypassUnRegistered = 1,

        /// <summary>
        /// <para type="description">BypassDisabled, Allow user access if not enabled in MFA.</para>
        /// </summary>
        BypassDisabled = 2,

        /// <summary>
        /// <para type="description">AllowUnRegistered, Allow user access if not registered in MFA. But, the user is invited to register</para>
        /// </summary>
        AllowUnRegistered = 4,

        /// <summary>
        /// <para type="description">AllowDisabled, Allow user access if not enabled in MFA. But, the user is invited to enable his account</para>
        /// </summary>
        AllowDisabled = 8,

        /// <summary>
        /// <para type="description">AllowChangePassword, Change password link is provided for user in "Manage My options"</para>
        /// </summary>
        AllowChangePassword = 16,

        /// <summary>
        /// <para type="description">AllowManageOptions, Manage my options link is provided for user."</para>
        /// </summary>
        AllowManageOptions = 32,

        /// <summary>
        /// <para type="description">AllowProvideInformations, When not registered or enabled, user can post informations to administrators, and ask them to activate thier account."</para>
        /// </summary>
        AllowProvideInformations = 64,

        /// <summary>
        /// <para type="description">AllowEnrollment, Allow Wizards link for enabled Providers."</para>
        /// </summary>
        AllowEnrollment = 128,

        /// <summary>
        /// <para type="description">AdministrativeMode, Lock access to users if not registered o disabled."</para>
        /// </summary>
        AdministrativeMode = 256
    }


    /// <summary>
    /// PSWebAuthNPinRequirements
    /// <para type="synopsis">Configuration options for Pin requirement when Biometric login, the user is not verified. Must be combined with binary OR.</para>
    /// <para type="description">Configuration options for Pin requirement when Biometric login, the user is not verified. Must be combined with binary OR.</para>
    /// </summary>
    [Flags]
    public enum PSWebAuthNPinRequirements
    {
        /// <summary>
        /// <para type="description">Null, No requirement for pin.</para>
        /// </summary>
        Null = 0,

        /// <summary>
        /// <para type="description">None, Pin requirement for None Attestation type when user is not verified.</para>
        /// </summary>
        None = 1,

        /// <summary>
        /// <para type="description">AndroidKey, Pin requirement for AndroidKey Attestation type when user is not verified.</para>
        /// </summary>
        AndroidKey = 2,

        /// <summary>
        /// <para type="description">AndroidSafetyNet, Pin requirement for AndroidSafetyNet Attestation type when user is not verified.</para>
        /// </summary>
        AndroidSafetyNet = 4,

        /// <summary>
        /// <para type="description">Fido2U2f, Pin requirement for Fido2U2f Attestation type when user is not verified.</para>
        /// </summary>
        Fido2U2f = 8,

        /// <summary>
        /// <para type="description">Packed, Pin requirement for Packed Attestation type when user is not verified.</para>
        /// </summary>
        Packed = 16,

        /// <summary>
        /// <para type="description">TPM, Pin requirement for TPM Attestation type when user is not verified.</para>
        /// </summary>
        TPM = 32
    }


    /// <summary>
    /// PSUIKind
    /// <para type="synopsis">Kind of ADFS's User Interface version</para>
    /// <para type="description">Kind of ADFS's User Interface version (ADFS 2019 or Older with Custome Themes)</para>
    /// </summary>
    public enum PSUIKind
    {
        /// <summary>
        /// <para type="description">Default UI theme for ADFS 2012r2, 2016 and 2019</para>
        /// </summary>
        Default = 0,

        /// <summary>
        /// <para type="description">Default UI theme for ADFS 2019 only (Centered)</para>
        /// </summary>
        Default2019 = 1
    }

    /// <summary>
    /// PSPrimaryAuthOptions
    /// <para type="synopsis">Options when using MFA as Primary Authentication</para>
    /// <para type="description">Options when using MFA as Primary Authentication</para>
    /// </summary>
    [Flags]
    public enum PSPrimaryAuthOptions
    {
        /// <summary>
        /// <para type="description">None, Nothing allowed (no external providers, no registration process.</para>
        /// </summary>
        None = 0,

        /// <summary>
        /// <para type="description">Externals, Externals providers like Email or Phone are allowed as Primary authentication methods.</para>
        /// </summary>
        Externals = 1,

        /// <summary>
        /// <para type="description">Register, Registration process is aalowed as Primary authentication methos.</para>
        /// </summary>
        Register = 2
    }

    /// <summary>
    /// PSKeyGeneratorMode
    /// <para type="synopsis">For RNG Key generation, Key Size from 128 bits to 512 bits.</para>
    /// <para type="description">Configuration options for registering or accessing MFA. Must be combined with binary OR, in MMC you can use Templates</para>
    /// </summary>
    public enum PSKeyGeneratorMode
    {
        /// <summary>
        /// <para type="description">Guid, 128 bits length</para>
        /// </summary>
        Guid = 0,

        /// <summary>
        /// <para type="description">ClientSecret128, 128 bits length with RNG</para>
        /// </summary>
        ClientSecret128 = 1,

        /// <summary>
        /// <para type="description">ClientSecret256, 256 bits length with RNG</para>
        /// </summary>
        ClientSecret256 = 2,

        /// <summary>
        /// <para type="description">ClientSecret384, 384 bits length with RNG</para>
        /// </summary>
        ClientSecret384 = 3,

        /// <summary>
        /// <para type="description">ClientSecret512, 512 bits length with RNG</para>
        /// </summary>
        ClientSecret512 = 4,
    }

    /// <summary>
    /// PSAESKeyGeneratorMode
    /// <para type="synopsis">For AES Key encryption, Key Size from 512 bits to 1024 bits.</para>
    /// <para type="description">Configuration options for registering or accessing MFA. Must be combined with binary OR, in MMC you can use Templates</para>
    /// </summary>
    public enum PSAESKeyGeneratorMode
    {
        /// <summary>
        /// <para type="description">AESSecret512, 512 bits length with AES</para>
        /// </summary>
        AESSecret512 = 0,

        /// <summary>
        /// <para type="description">AESSecret1024, 1024 bits length with AES</para>
        /// </summary>
        AESSecret1024 = 1,
    }

    /// <summary>
    /// PSKeySizeMode
    /// <para type="synopsis">For RSA and Custom, encryption Key is 2018 bytes, but this generate a huge QRCode, to restrict the display Size.</para>
    /// <para type="description">Restricts QRCode size, by default 1024 bytes</para>
    /// </summary>
    public enum PSKeySizeMode
    {
        /// <summary>
        /// <para type="description">KeySizeDefault, 2048 bytes length for encryption and 1024 bytes for display</para>
        /// </summary>
        KeySizeDefault = 0,

        /// <summary>
        /// <para type="description">KeySize512, 2048 bytes length for encryption and 512 bytes for display</para>
        /// </summary>
        KeySize512 = 1,

        /// <summary>
        /// <para type="description">KeySize1024, 2048 bytes length for encryption and 1024 bytes for display</para>
        /// </summary>
        KeySize1024 = 2,

        /// <summary>
        /// <para type="description">KeySize2048, 2048 bytes length for encryption and 2048 bytes for display</para>
        /// </summary>
        KeySize2048 = 3,

        /// <summary>
        /// <para type="description">KeySize128, 2048 bytes length for encryption and 128 bytes for display</para>
        /// </summary>
        KeySize128 = 4,

        /// <summary>
        /// <para type="description">KeySize256, 2048 bytes length for encryption and 256 bytes for display</para>
        /// </summary>
        KeySize256 = 5,

        /// <summary>
        /// <para type="description">KeySize384, 2048 bytes length for encryption and 384 bytes for display</para>
        /// </summary>
        KeySize384 = 6
    }

    /// <summary>
    /// PSHashMode
    /// <para type="synopsis">Hash algo for TOTP Key generation, default is SHA1. (see rfc6238, rfc4226)</para>
    /// <para type="description">Microsoft, Google and others only supports SHA1, but rfc specification says that it can be 256, 384 and 512. Only Authy supports higher values.</para>
    /// </summary>   
    public enum PSHashMode
    {
        /// <summary>
        /// <para type="description">SHA1, default hashing for TOTP key generation</para>
        /// </summary>
        SHA1 = 0,

        /// <summary>
        /// <para type="description">SHA256, hashing for TOTP key generation. supported by Authy authenticator App.</para>
        /// </summary>
        SHA256 = 1,

        /// <summary>
        /// <para type="description">SHA384, hashing for TOTP key generation. supported by Authy authenticator App.</para>
        /// </summary>
        SHA384 = 2,

        /// <summary>
        /// <para type="description">SHA512, hashing for TOTP key generation. supported by Authy authenticator App.</para>
        /// </summary>
        SHA512 = 3
    }

    /// <summary>
    /// PSOTPWizardOptions
    /// <para type="synopsis">TOTP Wizard, allowed links</para>
    /// <para type="description">TOTP Wizard, allowed links</para>
    /// </summary>   
    [Flags]
    public enum PSOTPWizardOptions
    {

        /// <summary>
        /// <para type="description">All, all links are displayed.</para>
        /// </summary>
        All = 0x0,

        /// <summary>
        /// <para type="description">NoMicrosoftAuthenticator, disable links for Microsoft Authenticator.</para>
        /// </summary>
        NoMicrosoftAuthenticator = 0x1,

        /// <summary>
        /// <para type="description">NoGoogleAuthenticator, disable links for Google Authenticator.</para>
        /// </summary>
        NoGoogleAuthenticator = 0x2,

        /// <summary>
        /// <para type="description">NoAuthyAuthenticator, disable links for Authy Authenticator.</para>
        /// </summary>
        NoAuthyAuthenticator = 0x4,

        /// <summary>
        /// <para type="description">NoGooglSearch, disable links for searching on internet for Authenticator Apps.</para>
        /// </summary>
        NoGooglSearch = 0x8
    }


    /// <summary>
    /// PSSampleKind
    /// </summary>   
    public enum PSSampleKind
    {
        /// <summary>
        /// <para type="description">Quiz demo for external provider.</para>
        /// </summary>
        QuizProviderSample = 1,

        /// <summary>
        /// <para type="description">Caesar encryption demo for custom keys.</para>
        /// </summary>
        CaesarEnryptionSample = 2,

        /// <summary>
        /// <para type="description">InMemory custom storage demo for custom storage.</para>
        /// </summary>
        InMemoryStorageSample = 3,

        /// <summary>
        /// <para type="description">TOPT Provider sample for custom TOTP provider.</para>
        /// </summary>
        TOTPProviderSample = 4,

        /// <summary>
        /// <para type="description">SMS/External Provider sample for external provider.</para>
        /// </summary>
        SMSProviderSample = 5
    }

    /// <summary>
    /// PSDataOrderField
    /// <para type="synopsis">PowerShell Standard Order type</para>
    /// <para type="description">PowerShell Standard Order type for Get-MFAUsers</para>
    /// </summary>   
    public enum PSDataOrderField
    {
        /// <summary>
        /// <para type="description">None, No Order.</para>
        /// </summary>
        None = 0,

        /// <summary>
        /// <para type="description">UserName, Order by UserName.</para>
        /// </summary>
        UserName = 1,

        /// <summary>
        /// <para type="description">Email, Order by MailAddress.</para>
        /// </summary>
        Email = 2,

        /// <summary>
        /// <para type="description">Phnoe, Order by PhoneNumber.</para>
        /// </summary>
        Phone = 3,

        /// <summary>
        /// <para type="description">ID, Order by Internal ID.</para>
        /// </summary>
        ID = 4
    }

    /// <summary>
    /// PSDataOrderCryptedField
    /// <para type="synopsis">PowerShell Crypted Order type</para>
    /// <para type="description">PowerShell Crypted Order type for Get-MFAUsers</para>
    /// </summary>   
    public enum PSDataOrderCryptedField
    {
        /// <summary>
        /// <para type="description">None, No Order.</para>
        /// </summary>
        None = 0,

        /// <summary>
        /// <para type="description">UserName, Order by UserName.</para>
        /// </summary>
        UserName = 1,

        /// <summary>
        /// <para type="description">ID, Order by Internal ID.</para>
        /// </summary>
        ID = 4
    }

    /// <summary>
    /// PSDataFilterOperator
    /// <para type="synopsis">PowerShell Standard Filter type</para>
    /// <para type="description">PowerShell Standard Filter type for Get-MFAUsers</para>
    /// </summary>   
    public enum PSDataFilterOperator
    {
        /// <summary>
        /// <para type="description">Equal</para>
        /// </summary>
        Equal = 0,

        /// <summary>
        /// <para type="description">StartWith</para>
        /// </summary>
        StartWith = 1,

        /// <summary>
        /// <para type="description">Contains</para>
        /// </summary>
        Contains = 2,

        /// <summary>
        /// <para type="description">NotEqual</para>
        /// </summary>
        NotEqual = 3,

        /// <summary>
        /// <para type="description">EndsWith</para>
        /// </summary>
        EndsWith = 4,

        /// <summary>
        /// <para type="description">NotContains</para>
        /// </summary>
        NotContains = 5
    }

    /// <summary>
    /// PSDataFilterCryptedOperator
    /// <para type="synopsis">PowerShell Crypted Filter type</para>
    /// <para type="description">PowerShell Crypted Filter type for Get-MFAUsers</para>
    /// </summary>   
    public enum PSDataFilterCryptedOperator
    {
        /// <summary>
        /// <para type="description">Equal</para>
        /// </summary>
        Equal = 0,

        /// <summary>
        /// <para type="description">NotEqual</para>
        /// </summary>
        NotEqual = 3,
    }

    /// <summary>
    /// PSDataFilterField
    /// <para type="synopsis">PowerShell Filter Field type</para>
    /// <para type="description">PowerShell Filter Field type for Get-MFAUsers</para>
    /// </summary>   
    public enum PSDataFilterField
    {
        /// <summary>
        /// <para type="description">UserName</para>
        /// </summary>
        UserName = 0,

        /// <summary>
        /// <para type="description">MailAddress</para>
        /// </summary>
        Email = 1,

        /// <summary>
        /// <para type="description">PhoneNumber</para>
        /// </summary>
        PhoneNumber = 2
    }

    /// <summary>
    /// PSDataFilterCryptedField
    /// <para type="synopsis">PowerShell Crypted Filter Field type</para>
    /// <para type="description">PowerShell Crypted Filter Field type for Get-MFAUsers</para>
    /// </summary>   
    public enum PSDataFilterCryptedField
    {
        /// <summary>
        /// <para type="description">UserName</para>
        /// </summary>
        UserName = 0
    }

    /// <summary>
    /// PSAuthenticatorAttachmentKind
    /// <para type="synopsis">PowerShell Authenticator Attachement Kind</para>
    /// <para type="description">PowerShell PowerShell Authenticator Attachement Kind for Get-MFAProvider Biometrics</para>
    /// </summary>   
    public enum PSAuthenticatorAttachmentKind
    {
        /// <summary>
        /// <para type="description">Empty value</para>
        /// </summary>
        Empty = 0,

        /// <summary>
        /// <para type="description">Platform value</para>
        /// </summary>
        Platform = 1,

        /// <summary>
        /// <para type="description">Cross-Platform value</para>
        /// </summary>
        CrossPlatform = 2
    }

    /// <summary>
    /// PSAttestationConveyancePreferenceKind
    /// <para type="synopsis">PowerShell Attestation Conveyance Preference Kind</para>
    /// <para type="description">PowerShell PowerShell Attestation Conveyance Preference Kind for Get-MFAProvider Biometrics</para>
    /// </summary>   
    public enum PSAttestationConveyancePreferenceKind
    {
        /// <summary>
        /// <para type="description">None value</para>
        /// </summary>
        None = 0,

        /// <summary>
        /// <para type="description">Direct value (recommended)</para>
        /// </summary>
        Direct = 1,

        /// <summary>
        /// <para type="description">InDirect value (recommended)</para>
        /// </summary>
        Indirect = 2
    }

    /// <summary>
    /// PSUserVerificationRequirementKind
    /// <para type="synopsis">PowerShell User Verification Requirement Kind</para>
    /// <para type="description">PowerShell PowerShell User Verification Requirement Kind for Get-MFAProvider Biometrics</para>
    /// </summary>   
    public enum PSUserVerificationRequirementKind
    {
        /// <summary>
        /// <para type="description">Preferred value (recommended)</para>
        /// </summary>
        Preferred = 0,

        /// <summary>
        /// <para type="description">Required value (recommended)</para>
        /// </summary>
        Required = 1,

        /// <summary>
        /// <para type="description">Discouraged value (recommended)</para>
        /// </summary>
        Discouraged = 2
    }

    /// <summary>
    /// PSADFSCertificateKind
    /// <para type="synopsis">MFA ADFS certificate Kind (Signing, Decrypting).</para>
    /// /// <para type="description">MFA ADFS certificate Kind (Signing, Decrypting).</para>
    /// </summary>    
    public enum PSADFSCertificateKind
    {
        /// <summary>
        /// <para type="description">Signing Certificate.</para>
        /// </summary>
        Signing = 0,

        /// <summary>
        /// <para type="description">Decrypting certificate.</para>
        /// </summary>
        Decrypting = 1
    }

    /// <summary>
    /// PSKeyMgtOptions
    /// <para type="synopsis">Certificates Kind for ACLs.</para>
    /// /// <para type="description">Certificates Kind for ACLs and cleaning orphaned keys.</para>
    /// </summary>    
    [Flags]
    public enum PSKeyMgtOptions
    {
        /// <summary>
        /// <para type="description">All Certificates checked.</para>
        /// </summary>
        AllCerts = 0x0,

        /// <summary>
        /// <para type="description">MFA specific Certificates checked.</para>
        /// </summary>
        MFACerts = 0x1,

        /// <summary>
        /// <para type="description">ADFS Certificates checked (subject beginning by "ADFS".</para>
        /// </summary>
        ADFSCerts = 0x2,

        /// <summary>
        /// <para type="description">Wilcard SSL Certificates checked (subject beginning by *. .</para>
        /// </summary>
        SSLCerts = 0x4
    }

    /// <summary>
    /// PSKeyAutoCleanUp
    /// <para type="synopsis">Persistence options for Cleanning Orphaned private Keys.</para>
    /// /// <para type="description">options for automating Orphaned private Keys cleanup.</para>
    /// </summary>    
    public enum PSKeyAutoCleanUp
    {
        /// <summary>
        /// <para type="description">None, do not change anything.</para>
        /// </summary>
        None = 0x0,

        /// <summary>
        /// <para type="description">Enable persistent AutoCleanUp.</para>
        /// </summary>
        Enable = 0x1,

        /// <summary>
        /// <para type="description">Disable persistent AutoCleanUp.</para>
        /// </summary>
        Disable = 0x2,
    }

}
