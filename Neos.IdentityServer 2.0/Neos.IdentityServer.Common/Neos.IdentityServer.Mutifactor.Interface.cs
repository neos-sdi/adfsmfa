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
using System.Globalization;
using System.Runtime.Serialization;
using System.ServiceModel;
using Microsoft.IdentityServer.Web.Authentication.External;

namespace Neos.IdentityServer.MultiFactor
{
    [ServiceContract(Namespace="http://adfsmfa.neos-sdi.com/identityserver/adminservice")]
    public interface IAdminService
    {
        [OperationContract]
        Registration GetUserRegistration(string upn);

        [OperationContract]
        void SetUserRegistration(Registration reg);

        [OperationContract]
        Notification SetNotification(Registration reg, MFAConfig cfg, int otp);

        [OperationContract]
        Notification CheckNotification(string registrationid);

        [OperationContract]
        string GetUserKey(string upn);

        [OperationContract]
        string NewUserKey(string upn, string secretkey);

        [OperationContract]
        bool RemoveUserKey(string upn);
    }

    [ServiceContract(Namespace = "http://adfsmfa.neos-sdi.com/identityserver/externalotpprovider")]
    public interface IExternalOTPProvider
    {
        [OperationContract]
        int GetUserCodeWithExternalSystem(string upn, string phonenumber, string email, ExternalOTPProvider externalsys, CultureInfo culture);
    }

    [ServiceContract(Namespace = "http://adfsmfa.neos-sdi.com/identityserver/keymanager")]
    public interface ISecretKeyManager
    {
        [DataMember]
        string Prefix
        {
            get;
        }

        [OperationContract]
        void Initialize(MFAConfig config);

        [OperationContract]
        string NewKey(string upn);

        [OperationContract]
        string ReadKey(string upn);

        [OperationContract]
        string EncodedKey(string upn);

        [OperationContract]
        string ProbeKey(string upn);

        [OperationContract]
        bool RemoveKey(string upn); 

        [OperationContract]
        bool ValidateKey(string upn);

        [OperationContract]
        string StripKeyPrefix(string key);

        [OperationContract]
        string AddKeyPrefix(string key);

        [OperationContract]
        bool HasKeyPrefix(string key);
    }

    public static class NotificationStatus
    {
        public const int Error = 0;
        public const int Totp = -1;
        public const int RequestEmail = -2;
        public const int RequestSMS = -3;
        public const int RequestIncription = -4;
        public const int RequestEmailForKey = -5;
        public const int ResponseEmailForKeyRegistration = -80;
        public const int ResponseEmailForKeyInvitation = -81;
        public const int Bypass = -99;
    }

    /// <summary>
    /// SecretKeyStatus
    /// </summary>
    [DataContract, Serializable]
    public enum SecretKeyStatus
    {
        [EnumMember]
        Success = 0,
        [EnumMember]
        NoKey = 1,
        [EnumMember]
        Unknown = 2
    }

    [DataContract, Serializable]
    public enum RegistrationSecretKeyFormat
    {
        [EnumMember]
        CFG = 0,
        [EnumMember]
        RNG = 1,
        [EnumMember]
        RSA = 2,
        [EnumMember]
        CUSTOM = 3
    }

    [DataContract, Serializable]
    public enum RegistrationPreferredMethod
    {
        [EnumMember]
        Choose = 0,
        [EnumMember]
        Code = 1,
        [EnumMember]
        Email = 2,
        [EnumMember]
        Phone = 3,
        [EnumMember]
        Face = 4
    }

    [DataContract, Serializable]
    public enum HashMode
    {
        [EnumMember]
        SHA1 = 0,
        [EnumMember]
        SHA256 = 1,
        [EnumMember]
        SHA384 = 2,
        [EnumMember]
        SHA512 = 3
    }

    [DataContract, Serializable]
    public enum KeyGeneratorMode
    {
        [EnumMember]
        Guid = 0,
        [EnumMember]
        ClientSecret128 = 1,
        [EnumMember]
        ClientSecret256 = 2,
        [EnumMember]
        ClientSecret384 = 3,
        [EnumMember]
        ClientSecret512 = 4,
        [EnumMember]
        Custom = 5
    }

    [DataContract, Serializable]
    public enum KeySizeMode
    {
        [EnumMember]
        KeySizeDefault = 0,
        [EnumMember]
        KeySize512 = 1,
        [EnumMember]
        KeySize1024 = 2,
        [EnumMember]
        KeySize2048 = 3
    }

    [DataContract, Serializable]
    public class Registration
    {
        private string _id;
        private string _upn;
        private string _mail;
        private string _phone;
        private bool _enabled = false;
        private bool _isregistered = false;
        private DateTime _creationdate;
        private RegistrationPreferredMethod _method = RegistrationPreferredMethod.Choose;

        /// <summary>
        /// Constructor
        /// </summary>
        public Registration()
        {

        }

        [DataMember]
        public string ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        [DataMember]
        public string UPN
        {
            get
            {
                return _upn;
            }
            set
            {
                _upn = value;
            }
        }

        [DataMember]
        public string MailAddress
        {
            get
            {
                if (string.IsNullOrEmpty(_mail))
                    return string.Empty;
                else
                    return _mail;
            }
            set
            {
                _mail = value;
            }
        }

        [DataMember]
        public string PhoneNumber
        {
            get
            {
                if (string.IsNullOrEmpty(_phone))
                    return string.Empty;
                else
                    return _phone;
            }
            set
            {
                _phone = value;
            }
        }

        [DataMember]
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
            }
        }

        [DataMember]
        public DateTime CreationDate
        {
            get
            {
                return _creationdate;
            }
            set
            {
                _creationdate = value;
            }
        }

        [DataMember]
        public RegistrationPreferredMethod PreferredMethod
        {
            get
            {
                return _method;
            }
            set
            {
                _method = value;
            }
        }

        [DataMember]
        public bool IsRegistered
        {
            get
            {
                return _isregistered;
            }
            set
            {
                _isregistered = value;
            }
        }
    }

    [DataContract, Serializable]
    public class AuthenticationContext
    {
        private IAuthenticationContext _context = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public AuthenticationContext(IAuthenticationContext ctx)
        {
            _context = ctx;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AuthenticationContext(IAuthenticationContext ctx, Registration reg): this(ctx)
        {
            this.ID = reg.ID;
            this.UPN = reg.UPN;
            this.MailAddress = reg.MailAddress;
            this.PhoneNumber = reg.PhoneNumber;
            this.CreationDate = reg.CreationDate;
            this.IsRegistered = reg.IsRegistered;
            this.Enabled = reg.Enabled;
            this.PreferredMethod = reg.PreferredMethod;
            this.KeyStatus = SecretKeyStatus.Success;
        }

        /// <summary>
        /// implicit operator AuthenticationContext -> Registration
        /// </summary>
        public static explicit operator Registration(AuthenticationContext context)
        {
            Registration registration = new Registration();
            registration.ID = context.ID;
            registration.UPN = context.UPN;
            registration.MailAddress = context.MailAddress;
            registration.PhoneNumber = context.PhoneNumber;
            registration.CreationDate = context.CreationDate;
            registration.Enabled = context.Enabled;
            registration.IsRegistered = context.IsRegistered;
            registration.PreferredMethod = context.PreferredMethod;
            return registration;
        }

        /// <summary>
        /// Assign method
        /// </summary>
        public void Assign(Registration reg)
        {
            this.ID = reg.ID;
            this.UPN = reg.UPN;
            this.MailAddress = reg.MailAddress;
            this.PhoneNumber = reg.PhoneNumber;
            this.CreationDate = reg.CreationDate;
            this.IsRegistered = reg.IsRegistered;
            this.Enabled = reg.Enabled;
            this.PreferredMethod = reg.PreferredMethod;
            this.KeyStatus = SecretKeyStatus.Success;
        }

        [DataMember]
        public string ID
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxid") && _context.Data["_authctxid"] != null)
                    return _context.Data["_authctxid"].ToString();
                else
                    return string.Empty;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxid"))
                    _context.Data["_authctxid"] = value;
                else
                    _context.Data.Add("_authctxid", value);
            }
        }

        [DataMember]
        public string UPN
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxupn") && _context.Data["_authctxupn"] != null)
                    return _context.Data["_authctxupn"].ToString();
                else
                    return string.Empty;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxupn"))
                    _context.Data["_authctxupn"] = value;
                else
                    _context.Data.Add("_authctxupn", value);
            }
        }

        [DataMember]
        public bool KeyChanged
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxkeychanged") && _context.Data["_authctxkeychanged"] != null)
                    return (bool)_context.Data["_authctxkeychanged"];
                else
                    return false;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxkeychanged"))
                    _context.Data["_authctxkeychanged"] = value;
                else
                    _context.Data.Add("_authctxkeychanged", value);
            }
        }

        [DataMember]
        public SecretKeyStatus KeyStatus
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxkeystatus") && _context.Data["_authctxkeystatus"] != null)
                    return (SecretKeyStatus)_context.Data["_authctxkeystatus"];
                else
                    return SecretKeyStatus.NoKey;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxkeystatus"))
                    _context.Data["_authctxkeystatus"] = (int)value;
                else
                    _context.Data.Add("_authctxkeystatus", (int)value);
            }
        }

        [DataMember]
        public string MailAddress
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxmail") && _context.Data["_authctxmail"] != null)
                    return _context.Data["_authctxmail"].ToString();
                else
                    return string.Empty;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxmail"))
                    _context.Data["_authctxmail"] = value;
                else
                    _context.Data.Add("_authctxmail", value);
            }
        }

        [DataMember]
        public string PhoneNumber
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxphone") && _context.Data["_authctxphone"] != null)
                    return _context.Data["_authctxphone"].ToString();
                else
                    return string.Empty;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxphone"))
                    _context.Data["_authctxphone"] = value;
                else
                    _context.Data.Add("_authctxphone", value);
            }
        }

        [DataMember]
        public bool Enabled
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxenabled") && _context.Data["_authctxenabled"] != null)
                    return (bool)_context.Data["_authctxenabled"];
                else
                    return false;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxenabled"))
                    _context.Data["_authctxenabled"] = value;
                else
                    _context.Data.Add("_authctxenabled", value);
            }
        }

        [DataMember]
        public bool IsRegistered
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxhisregistered") && _context.Data["_authctxhisregistered"] != null)
                    return (bool)_context.Data["_authctxhisregistered"];
                else
                    return false;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxhisregistered"))
                    _context.Data["_authctxhisregistered"] = value;
                else
                    _context.Data.Add("_authctxhisregistered", value);
            }
        }

        [DataMember]
        public DateTime CreationDate
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxcreationdate") && _context.Data["_authctxcreationdate"] != null)
                    return (DateTime)_context.Data["_authctxcreationdate"];
                else
                    return DateTime.MinValue;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxcreationdate"))
                    _context.Data["_authctxcreationdate"] = (DateTime)value;
                else
                    _context.Data.Add("_authctxcreationdate", (DateTime)value);
            }
        }

        [DataMember]
        public RegistrationPreferredMethod PreferredMethod
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxmethod") && _context.Data["_authctxmethod"] != null)
                    return (RegistrationPreferredMethod)_context.Data["_authctxmethod"];
                else
                    return (int)RegistrationPreferredMethod.Choose;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxmethod"))
                    _context.Data["_authctxmethod"] = (int)value;
                else
                    _context.Data.Add("_authctxmethod", (int)value);
            }
        }

        /// <summary>
        /// UIMode property
        /// </summary>
        public ProviderPageMode UIMode
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxuimode") && _context.Data["_authctxuimode"] != null)
                    return (ProviderPageMode)_context.Data["_authctxuimode"];
                else
                    return ProviderPageMode.Locking;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxuimode"))
                    _context.Data["_authctxuimode"] = (int)value;
                else
                    _context.Data.Add("_authctxuimode", (int)value);
            }
        }

        /// <summary>
        /// TargetUIMode property
        /// </summary>
        public ProviderPageMode TargetUIMode
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxtargetuimode") && _context.Data["_authctxtargetuimode"] != null)
                    return (ProviderPageMode)_context.Data["_authctxtargetuimode"];
                else
                    return ProviderPageMode.Locking;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxtargetuimode"))
                    _context.Data["_authctxtargetuimode"] = (int)value;
                else
                    _context.Data.Add("_authctxtargetuimode", (int)value);
            }
        }

        /// <summary>
        /// UIMessage property
        /// </summary>
        public string UIMessage
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxuimessage") && _context.Data["_authctxuimessage"] != null)
                    return _context.Data["_authctxuimessage"].ToString();
                else
                    return string.Empty; 
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxuimessage"))
                    _context.Data["_authctxuimessage"] = value;
                else
                    _context.Data.Add("_authctxuimessage", value);
            }
        }

    }

    /// <summary>
    /// ProviderPageMode
    /// </summary>
    public enum ProviderPageMode
    {
        Locking = 0,
        Bypass = 1,
        Identification = 2,
        Registration = 3,
        Invitation = 4,
        SelectOptions = 5,
        ChooseMethod = 6,
        ChangePassword = 7,
        ShowQRCode = 8,
        CodeRequest = 9,
        InvitationRequest = 10,
        SendKeyRequest = 11,
        None = 64,
        DefinitiveError = 128
    }

    [DataContract, Serializable]
    public class Notification
    {
        [DataMember]
        public string ID
        {
            get;
            set;
        }

        [DataMember]
        public string RegistrationID
        {
            get;
            set;
        }

        [DataMember]
        public int OTP
        {
            get;
            set;
        }

        [DataMember]
        public DateTime CreationDate
        {
            get;
            set;
        }

        [DataMember]
        public DateTime ValidityDate
        {
            get;
            set;
        }

        [DataMember]
        public Nullable<DateTime> CheckDate
        {
            get;
            set;
        }
    }
}
