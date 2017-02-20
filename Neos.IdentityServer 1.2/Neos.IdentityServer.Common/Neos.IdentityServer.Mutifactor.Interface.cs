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
//******************************************************************************************************************************************************************************************//
using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;
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
    }

    [ServiceContract(Namespace = "http://adfsmfa.neos-sdi.com/identityserver/externalotpprovider")]
    public interface IExternalOTPProvider
    {
        [OperationContract]
        int GetUserCodeWithExternalSystem(string upn, string phonenumber, string email, ExternalOTPProvider externalsys, CultureInfo culture);
    }

    [DataContract, Serializable]
    public enum RegistrationPreferredMethod
    {
        [EnumMember(Value="0")]
        Choose = 0,

        [EnumMember(Value = "1")]
        ApplicationCode = 1,

        [EnumMember(Value = "2")]
        Email = 2,

        [EnumMember(Value = "3")]
        Phone = 3
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
    public class Registration
    {
        private string _id;
        private string _upn;
        private string _mail;
        private string _phone;
        private bool _enabled = true;
        private DateTime _creationdate;
        private RegistrationPreferredMethod _method = RegistrationPreferredMethod.Choose;
        private string _secretkey;

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
        public string SecretKey
        {
            get
            {
                return _secretkey;
            }
            set
            {
                _secretkey = value;
            }
        }

        [DataMember]
        public string DisplayKey
        {
            get
            {
                if (string.IsNullOrEmpty(_secretkey))
                    return string.Empty;
                else
                    return Base32.Encode(_secretkey);
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
        public AuthenticationContext(IAuthenticationContext ctx, Registration reg):this(ctx)
        {
            this.ID =  reg.ID;
            this.UPN = reg.UPN;
            this.MailAddress = reg.MailAddress;
            this.PhoneNumber = reg.PhoneNumber;
            this.CreationDate = reg.CreationDate;
            this.Enabled = reg.Enabled;
            this.SecretKey = reg.SecretKey;
            this.PreferredMethod = reg.PreferredMethod;
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
            registration.SecretKey = context.SecretKey;
            registration.PreferredMethod = context.PreferredMethod;
            return registration;
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
        public string SecretKey
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxkey") && _context.Data["_authctxkey"] != null)
                    return _context.Data["_authctxkey"].ToString();
                else
                    return string.Empty;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxkey"))
                    _context.Data["_authctxkey"] = value;
                else
                    _context.Data.Add("_authctxkey", value);
            }
        }

        [DataMember]
        public bool SecretKeyChanged
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
    }

    public enum ProviderPageMode
    {
        Locking = 0,
        Identification = 1,
        ChooseMethod = 2,
        ChangePassword = 3,
        Registration = 4,
        SelectOptions = 5,
        ShowQRCode = 6,
        Bypass = 7
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
