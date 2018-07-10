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
using System.Text.RegularExpressions;

namespace Neos.IdentityServer.MultiFactor
{
    #region AuthenticationContext
    [Serializable]
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
            this.IsRegistered = reg.IsRegistered;
            this.Enabled = reg.Enabled;
            this.PreferredMethod = reg.PreferredMethod;
            this.OverrideMethod = reg.OverrideMethod;
            this.PinCode = reg.PIN;
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
            registration.Enabled = context.Enabled;
            registration.IsRegistered = context.IsRegistered;
            registration.PreferredMethod = context.PreferredMethod;
            registration.PIN = context.PinCode;
            registration.OverrideMethod = context.OverrideMethod;
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
            this.IsRegistered = reg.IsRegistered;
            this.Enabled = reg.Enabled;
            this.PreferredMethod = reg.PreferredMethod;
            this.OverrideMethod = reg.OverrideMethod;
            this.PinCode = reg.PIN;
            this.KeyStatus = SecretKeyStatus.Success;
        }

        /// <summary>
        /// AuthCtx property
        /// </summary>
        [XmlIgnore]
        public IAuthenticationContext AuthCtx
        {
            get { return _context; }
        }

        /// <summary>
        /// ID property implementation
        /// </summary>
        [XmlAttribute("ID")]
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

        /// <summary>
        /// UPN property implementation
        /// </summary>
        [XmlAttribute("UPN")]
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

        /// <summary>
        /// KeyChanged property implementation
        /// </summary>
        [XmlAttribute("KeyChanged")]
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

        /// <summary>
        /// KeyStatus property implementation
        /// </summary>
        [XmlAttribute("KeyStatus")]
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

        /// <summary>
        /// MailAddress property implementation
        /// </summary>
        [XmlAttribute("MailAddress")]
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

        /// <summary>
        /// PhoneNumber property implementation
        /// </summary>
        [XmlAttribute("PhoneNumber")]
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

        /// <summary>
        /// Enabled property implementation
        /// </summary>
        [XmlAttribute("Enabled")]
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

        /// <summary>
        /// IsRegistered property implementation
        /// </summary>
        [XmlAttribute("IsRegistered")]
        public bool IsRegistered
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxisregistered") && _context.Data["_authctxisregistered"] != null)
                    return (bool)_context.Data["_authctxisregistered"];
                else
                    return false;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxisregistered"))
                    _context.Data["_authctxisregistered"] = value;
                else
                    _context.Data.Add("_authctxisregistered", value);
            }
        }

        /// <summary>
        /// ShowOptions property implementation
        /// </summary>
        [XmlAttribute("ShowOptions")]
        public bool ShowOptions
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxhshowoptions") && _context.Data["_authctxhshowoptions"] != null)
                    return (bool)_context.Data["_authctxhshowoptions"];
                else
                    return false;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxhshowoptions"))
                    _context.Data["_authctxhshowoptions"] = value;
                else
                    _context.Data.Add("_authctxhshowoptions", value);
            }
        }

        /// <summary>
        /// UIMode property
        /// </summary>
        [XmlAttribute("UIMode")]
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
        [XmlAttribute("TargetUIMode")]
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
        [XmlAttribute("UIMessage")]
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

        /// <summary>
        /// Notification 
        /// </summary>
        [XmlAttribute("Notification")]
        public int Notification
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxnotif") && _context.Data["_authctxnotif"] != null)
                    return Convert.ToInt32(_context.Data["_authctxnotif"]);
                else
                    return (int)AuthenticationResponseKind.Error;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxnotif"))
                    _context.Data["_authctxnotif"] = (int)value;
                else
                    _context.Data.Add("_authctxnotif", (int)value);
            }
        }

        /// <summary>
        /// IsRemote property implementation
        /// </summary>
        [XmlAttribute("IsRemote")]
        public bool IsRemote
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxremote") && _context.Data["_authctxremote"] != null)
                    return (bool)_context.Data["_authctxremote"];
                else
                    return false;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxremote"))
                    _context.Data["_authctxremote"] = value;
                else
                    _context.Data.Add("_authctxremote", value);
            }
        }

        /// <summary>
        /// IsTwoWay property implementation
        /// </summary>
        [XmlAttribute("IsTwoWay")]
        public bool IsTwoWay
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxtwoway") && _context.Data["_authctxtwoway"] != null)
                    return (bool)_context.Data["_authctxtwoway"];
                else
                    return false;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxtwoway"))
                    _context.Data["_authctxtwoway"] = value;
                else
                    _context.Data.Add("_authctxtwoway", value);
            }
        }

        /// <summary>
        /// IsSendBack property implementation
        /// </summary>
        [XmlAttribute("IsSendBack")]
        public bool IsSendBack
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxsendback") && _context.Data["_authctxsendback"] != null)
                    return (bool)_context.Data["_authctxsendback"];
                else
                    return false;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxsendback"))
                    _context.Data["_authctxsendback"] = value;
                else
                    _context.Data.Add("_authctxsendback", value);
            }
        }

        /// <summary>
        /// TargetUIMode property
        /// </summary>
        [XmlAttribute("Lcid")]
        public int Lcid
        {
            get
            {
                return _context.Lcid;
            }
        }

        /// <summary>
        /// SessionId 
        /// </summary>
        [XmlAttribute("SessionId")]
        public string SessionId
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxsessionid") && _context.Data["_authctxsessionid"] != null)
                    return _context.Data["_authctxsessionid"].ToString();
                else
                    return string.Empty;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxsessionid"))
                    _context.Data["_authctxsessionid"] = value;
                else
                    _context.Data.Add("_authctxsessionid", value);
            }
        }

        /// <summary>
        /// SessionDate property implementation
        /// </summary>
        [XmlAttribute("SessionDate")]
        public DateTime SessionDate
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxsessiondate") && _context.Data["_authctxsessiondate"] != null)
                    return (DateTime)_context.Data["_authctxsessiondate"];
                else
                    return DateTime.MinValue;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxsessiondate"))
                    _context.Data["_authctxsessiondate"] = (DateTime)value;
                else
                    _context.Data.Add("_authctxsessiondate", (DateTime)value);
            }
        }

        /// <summary>
        /// ActivityId 
        /// </summary>
        [XmlAttribute("ActivityId")]
        public string ActivityId
        {
            get
            {
                return _context.ActivityId;
            }
        }

        /// <summary>
        /// PinRequired property implementation
        /// </summary>
        [XmlAttribute("PinRequired")]
        public bool PinRequired
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxpinrequired") && _context.Data["_authctxpinrequired"] != null)
                    return (bool)_context.Data["_authctxpinrequired"];
                else
                    return false;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxpinrequired"))
                    _context.Data["_authctxpinrequired"] = value;
                else
                    _context.Data.Add("_authctxpinrequired", value);
            }
        }

        /// <summary>
        /// PinCode property implementation
        /// </summary>
        [XmlAttribute("PinCode")]
        public int PinCode
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxpincode") && _context.Data["_authctxpincode"] != null)
                    return (int)_context.Data["_authctxpincode"];
                else
                    return 0;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxpincode"))
                    _context.Data["_authctxpincode"] = value;
                else
                    _context.Data.Add("_authctxpincode", value);
            }
        }

        /// <summary>
        /// PinEnabled property implementation
        /// </summary>
        [XmlAttribute("ExtraInfos")]
        public string ExtraInfos 
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxextrainfos") && _context.Data["_authctxextrainfos"] != null)
                    return _context.Data["_authctxextrainfos"].ToString();
                else
                    return string.Empty;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxextrainfos"))
                    _context.Data["_authctxextrainfos"] = value;
                else
                    _context.Data.Add("_authctxextrainfos", value);
            }
        }

        /// <summary>
        /// PreferredMethod property implementation
        /// </summary>
        [XmlAttribute("PreferredMethod")]
        public PreferredMethod PreferredMethod
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxmethod") && _context.Data["_authctxmethod"] != null)
                    return (PreferredMethod)_context.Data["_authctxmethod"];
                else
                    return (int)PreferredMethod.Choose;
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
        /// SelectedMethod property implementation
        /// </summary>
        [XmlAttribute("SelectedMethod")]
        public AuthenticationResponseKind SelectedMethod
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxselectedmethod") && _context.Data["_authctxselectedmethod"] != null)
                    return (AuthenticationResponseKind)_context.Data["_authctxselectedmethod"];
                else
                    return (int)AuthenticationResponseKind.Error;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxselectedmethod"))
                    _context.Data["_authctxselectedmethod"] = (int)value;
                else
                    _context.Data.Add("_authctxselectedmethod", (int)value);
            }
        }

        /// <summary>
        /// OverrideMethod property implementation
        /// </summary>
        [XmlAttribute("OverrideMethod")]
        public string OverrideMethod
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxoverridemethod") && _context.Data["_authctxoverridemethod"] != null)
                    return _context.Data["_authctxoverridemethod"].ToString();
                else
                    return string.Empty;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxoverridemethod"))
                    _context.Data["_authctxoverridemethod"] = value;
                else
                    _context.Data.Add("_authctxoverridemethod", value);
            }
        }

        /// <summary>
        /// PageId property implementation
        /// </summary>
        [XmlAttribute("PageID")]
        public int PageID 
        { 
            get
            {
                if (_context.Data.ContainsKey("_authctxpageid") && _context.Data["_authctxpageid"] != null)
                    return Convert.ToInt32(_context.Data["_authctxpageid"]);
                else
                    return 0;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxpageid"))
                    _context.Data["_authctxpageid"] = (int)value;
                else
                    _context.Data.Add("_authctxpageid", (int)value);
            }
        }

        [XmlAttribute("WizPageID")]
        public int WizPageID
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxwizpageid") && _context.Data["_authctxwizpageid"] != null)
                    return Convert.ToInt32(_context.Data["_authctxwizpageid"]);
                else
                    return 0;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxwizpageid"))
                    _context.Data["_authctxwizpageid"] = (int)value;
                else
                    _context.Data.Add("_authctxwizpageid", (int)value);
            }
        }

        /// <summary>
        /// AccountManagementUrl property implementation
        /// </summary>
        [XmlAttribute("AccountManagementUrl")]
        public string AccountManagementUrl
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxmgturl") && _context.Data["_authctxmgturl"] != null)
                    return _context.Data["_authctxmgturl"].ToString();
                else
                    return null;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxmgturl"))
                    _context.Data["_authctxmgturl"] = value;
                else
                    _context.Data.Add("_authctxmgturl", value);
            }
        }

        /// <summary>
        /// SessionData property implementation
        /// </summary>
        [XmlAttribute("SessionData")]
        public string SessionData
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxsessiondata") && _context.Data["_authctxsessiondata"] != null)
                    return _context.Data["_authctxsessiondata"].ToString();
                else
                    return null;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxsessiondata"))
                    _context.Data["_authctxsessiondata"] = value;
                else
                    _context.Data.Add("_authctxsessiondata", value);
            }
        }
    }
    #endregion

    #region MFA Data
    /// <summary>
    /// RegistrationList class implementation
    /// </summary>
    [Serializable]
    public class RegistrationList : List<Registration>
    {
        /// <summary>
        /// implicit conversion to byte array
        /// </summary>
        public static implicit operator byte[](RegistrationList registrations)
        {
            if (registrations == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, registrations);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// implicit conversion from ResultNode
        /// </summary>
        public static implicit operator RegistrationList(byte[] data)
        {
            if (data == null)
                return null;
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(data, 0, data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return (RegistrationList)binForm.Deserialize(memStream);
            }
        }
    }

    /// <summary>
    /// Registration Class implementation
    /// </summary>
    [Serializable]
    public class Registration
    {
        private string _id;
        private string _upn;
        private string _mail;
        private string _phone;
        private bool _enabled = false;
        private bool _isregistered = false;
        private int _pincode = 0;
        private PreferredMethod _method = PreferredMethod.Choose;
        private string _overridemethod = string.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        public Registration()
        {

        }

        /// <summary>
        /// IsApplied
        /// </summary>
        public bool IsApplied
        {
            get;
            set;
        }

        /// <summary>
        /// ID property implementation
        /// </summary>
        [XmlAttribute("ID")]
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

        /// <summary>
        /// UPN property implementation
        /// </summary>
        [XmlAttribute("UPN")]
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

        /// <summary>
        /// MailAddress property implementation
        /// </summary>
        [XmlAttribute("MailAddress")]
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

        /// <summary>
        /// PhoneNumber property implementation
        /// </summary>
        [XmlAttribute("PhoneNumber")]
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

        /// <summary>
        /// Enabled property implementation
        /// </summary>
        [XmlAttribute("Enabled")]
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

        /// <summary>
        /// ID property implementation
        /// </summary>
        [XmlAttribute("PIN")]
        public int PIN
        {
            get
            {
                return _pincode;
            }
            set
            {
                if (value >= 0)
                    _pincode = value;
                else
                    _pincode = 0;
            }
        }

        /// <summary>
        /// PreferredMethod property implementation
        /// </summary>
        [XmlAttribute("PreferredMethod")]
        public PreferredMethod PreferredMethod
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

        /// <summary>
        /// OverrideMethod property implementation
        /// </summary>
        [XmlAttribute("OverrideMethod")]
        public string OverrideMethod 
        {
            get
            {
                return _overridemethod;
            }
            set
            {
                _overridemethod = value;
            }
        }
        /// <summary>
        /// IsRegistered property implementation
        /// </summary>
        [XmlAttribute("IsRegistered")]
        public  bool IsRegistered
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

    /// <summary>
    /// AutenticationRequestKind enum
    /// </summary>
    public enum AuthenticationRequestKind
    {
        RequestCode = -1000,
        RequestEmailCode = -1001,
        RequestEmailInscription = -1002,
        RequestEmailForKey = -1003,
        
        RequestExternal = -1010,
        RequestAzure = -1011,
        RequestFIDO = -1012,
        RequestWindowsHello = -1013,
    }

    /// <summary>
    /// AuthenticationMethodKind enum
    /// </summary>
    public enum AuthenticationResponseKind
    {
        Error = 0,
        Bypass = -1,
        PhoneAppOTP = -2,
        EmailOTP = -3,
        SmsOTP = -4,
        PhoneAppConfirmation = -5,
        VoiceBiometric = -6,
        Kba = -7,
        FaceID = -8,
        WindowsHello = -9,
        FIDO = -10,
        Default = -50,

        EmailForInscription = -200,
        EmailForKey = -201,

        PhoneAppNotification = -1000,
        SmsOneWayOTP = -1001,
        SmsOneWayOTPplusPin = -1002,
        SmsTwoWayOTP = -1003,
        SmsTwoWayOTPplusPin = -1004,
        VoiceTwoWayMobile = -1005,
        VoiceTwoWayMobilePlusPin = -1006,
        VoiceTwoWayOffice = -1007,
        VoiceTwoWayOfficePlusPin = -1008,
        VoiceTwoWayAlternateMobile = -1009,
        VoiceTwoWayAlternateMobilePlusPin = -1010,
        BiometricsFinger = -2000,
        BiometricsFace = -2001,
        BiometricsPin = -2002,

        Sample1 = -9001,
        Sample2 = -9002,
        Sample3 = -9003,
        Sample1Async = -9011,
        Sample2Async = -9012,
        Sample3Async = -9013,

    }

    /// <summary>
    /// AvailableAuthenticationMethod class
    /// </summary>
    public class AvailableAuthenticationMethod
    {
        public AuthenticationResponseKind Method = AuthenticationResponseKind.Error;
        public bool IsDefault = false;
        public bool IsRemote = false;
        public bool IsTwoWay = false;
        public bool IsSendBack = false;
        public string ExtraInfos = string.Empty;
        public bool RequiredEmail = false;
        public bool RequiredPhone  = false;
        public bool RequiredCode = false;
        public bool RequiredPin = false;
    }
    #endregion

    #region Filters
    /// <summary>
    /// DataFilterObject class
    /// </summary>
    [Serializable]
    public class DataFilterObject
    {
        private DataFilterField filterfield = DataFilterField.UserName;
        private DataFilterOperator filteroperator = DataFilterOperator.Contains;
        private PreferredMethod filtermethod = PreferredMethod.None;
        private string filtervalue = string.Empty;
        private bool enabledonly = true;
        private bool filterisactive = true;


        /// <summary>
        /// implicit conversion to byte array
        /// </summary>
        public static explicit operator byte[](DataFilterObject filterobj)
        {
            if (filterobj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, filterobj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// implicit conversion from ResultNode
        /// </summary>
        public static explicit operator DataFilterObject(byte[] data)
        {
            if (data == null)
                return null;
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(data, 0, data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return (DataFilterObject)binForm.Deserialize(memStream);
            }
        }

        /// <summary>
        /// Clear method implementation
        /// </summary>
        public void Clear()
        {
            filterfield = DataFilterField.UserName;
            filteroperator = DataFilterOperator.Contains;
            filtermethod = PreferredMethod.None;
            filtervalue = string.Empty;
            enabledonly = false;
            filterisactive = false;
        }

        /// <summary>
        /// FilterField property implementation
        /// </summary>
        [XmlAttribute("FilterField")]
        public DataFilterField FilterField
        {
            get { return filterfield; }
            set { filterfield = value; }
        }

        /// <summary>
        /// FilterOperator property implementation
        /// </summary>
        [XmlAttribute("FilterOperator")]
        public DataFilterOperator FilterOperator
        {
            get { return filteroperator; }
            set { filteroperator = value; }
        }

        /// <summary>
        /// FilterMethod property implementation
        /// </summary>
        [XmlAttribute("FilterMethod")]
        public PreferredMethod FilterMethod
        {
            get { return filtermethod; }
            set
            {
                filtermethod = value;
                CheckForActiveFilter();
            }
        }

        /// <summary>
        /// FilterValue property implementation
        /// </summary>
        [XmlAttribute("FilterValue")]
        public string FilterValue
        {
            get { return filtervalue; }
            set
            {
                filtervalue = value;
                CheckForActiveFilter();
            }
        }

        /// <summary>
        /// EnabledOnly property implementation
        /// </summary>
        [XmlAttribute("EnabledOnly")]
        public bool EnabledOnly
        {
            get { return enabledonly; }
            set
            {
                enabledonly = value;
                CheckForActiveFilter();
            }
        }

        /// <summary>
        /// FilterisActive property implementation
        /// </summary>
        public bool FilterisActive
        {
            get { return filterisactive; }
        }

        /// <summary>
        /// CheckForActiveFilter property implementation
        /// </summary>
        private void CheckForActiveFilter()
        {
            filterisactive = false;
            if (string.Empty != filtervalue)
                filterisactive = true;
            if (filtermethod != PreferredMethod.None)
                filterisactive = true;
            if (enabledonly)
                filterisactive = true;
        }
    }

    /// <summary>
    /// DataOrderObject class
    /// </summary>
    [Serializable]
    public class DataOrderObject
    {
        private DataOrderField _order = DataOrderField.UserName;
        private SortDirection _sortorder = SortDirection.Ascending;

        /// <summary>
        /// Column property implmentation
        /// </summary>
        [XmlAttribute("Column")]
        public DataOrderField Column
        {
            get { return _order; }
            set { _order = value; }
        }

        /// <summary>
        /// Direction property implementation
        /// </summary>
        [XmlAttribute("Direction")]
        public SortDirection Direction
        {
            get { return _sortorder; }
            set { _sortorder = value; }
        }
    }

    /// <summary>
    /// DataPagingObject class
    /// </summary>
    [Serializable]
    public class DataPagingObject
    {
        private int _currentpage = 0;
        private int _pagesize = 50;
        private bool _isactive = false;
        private bool _isrecurse = false;

        public void Clear()
        {
            _currentpage = 0;
            _pagesize = 50;
            _isactive = false;
            _isrecurse = false;
        }

        public bool isActive
        {
            get { return _isactive; }
        }

        [XmlAttribute("CurrentPage")]
        public int CurrentPage
        {
            get { return _currentpage; }
            set
            {
                if (value < 0)
                    value = 0;
                _currentpage = value;
                _isactive = _currentpage > 0;
            }
        }

        [XmlAttribute("PageSize")]
        public int PageSize
        {
            get { return _pagesize; }
            set
            {
                if (value > 0)
                {
                    if (value >= int.MaxValue)
                        _pagesize = int.MaxValue;
                    else
                        _pagesize = value;
                }
            }
        }

        public bool IsRecurse
        {
            get { return _isrecurse; }
            set { _isrecurse = value; }
        }
    }

    /// <summary>
    /// DataFilterField enum
    /// </summary>
    [Serializable]
    public enum DataFilterField
    {
        UserName = 0,
        Email = 1,
        PhoneNumber = 2
    }

    /// <summary>
    /// DataOrderField enum
    /// </summary>
    [Serializable]
    public enum DataOrderField
    {
        None = 0,
        UserName = 1,
        Email = 2,
        Phone = 3,
        ID = 4
    }

    /// <summary>
    /// DataFilterOperators
    /// </summary>
    [Serializable]
    public enum DataFilterOperator
    {
        Equal = 0,
        StartWith = 1,
        Contains = 2,
        NotEqual = 3,
        EndsWith = 4,
        NotContains = 5
    }
    #endregion

    /// <summary>
    /// SecretKeyStatus
    /// </summary>
    [Serializable]
    public enum SecretKeyStatus
    {
        Success = 0,
        NoKey = 1,
        Unknown = 2
    }

    [Serializable]
    public enum SecretKeyFormat
    {
        RNG = 0,
        RSA = 1,
        CUSTOM = 2
    }

    [Serializable]
    public enum HashMode
    {
        SHA1 = 0,
        SHA256 = 1,
        SHA384 = 2,
        SHA512 = 3
    }

    [Serializable]
    public enum KeyGeneratorMode
    {
        Guid = 0,
        ClientSecret128 = 1,
        ClientSecret256 = 2,
        ClientSecret384 = 3,
        ClientSecret512 = 4,
        Custom = 5
    }

    [Serializable]
    public enum KeySizeMode
    {
        KeySizeDefault = 0,
        KeySize512 = 1,
        KeySize1024 = 2,
        KeySize2048 = 3
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
        SendAuthRequest = 9,
        SendAdministrativeRequest = 10,
        SendKeyRequest = 11,
        PreSet = 12,
        EnrollOTP = 13,
        EnrollBiometrics = 14,
        EnrollEmail = 15,
        EnrollPhone = 16,
        EnrollPin = 17,
        EnrollOTPAndSave = 23,
        EnrollBiometricsAndSave = 24,
        EnrollEmailAndSave = 25,
        EnrollPhoneAndSave = 26,
        EnrollPinAndSave = 27,
        None = 64,
        DefinitiveError = 128
    }

    /// <summary>
    /// PreferredMethod
    /// </summary>
    public enum PreferredMethod
    {
        Choose = 0,
        Code = 1,
        Email = 2,
        External = 3,
        Azure = 4,
        Biometrics = 5,
        None = 6
    }         

    /// <summary>
    /// Thumbprint Class
    /// </summary>
    public static class Thumbprint
    {
        public readonly static string Empty = "0000000000000000000000000000000000000000";
        public readonly static string Null  = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF";
        public readonly static string Demo  = "0123456789ABCDEF0123456789ABCDEF01234567";

        /// <summary>
        /// IsValid method implmentation
        /// </summary>
        public static bool IsValid(string thumbprint)
        {
            if (string.IsNullOrEmpty(thumbprint))
                return false;
            string pattern = @"\b([a-fA-F0-9]{40})\b";
            string input = thumbprint;
            RegexOptions options = RegexOptions.IgnorePatternWhitespace;

            Regex regex = new Regex(pattern, options);
            return regex.IsMatch(input);
        }

        /// <summary>
        /// IsAllowed method implmentation
        /// </summary>
        public static bool IsAllowed(string thumbprint)
        {
            bool result = IsValid(thumbprint);
            if (result)
            {
                if (thumbprint.ToUpper().Equals(Thumbprint.Null))
                    result = false;
                else if (thumbprint.ToUpper().Equals(Thumbprint.Empty))
                    result = false;
                else if (thumbprint.ToUpper().Equals(Thumbprint.Demo))
                    result = false;
            }
            return result;
        }

        /// <summary>
        /// IsNullOrEmpty method implmentation
        /// </summary>
        public static bool IsNullOrEmpty(string thumbprint)
        {
            bool result = IsValid(thumbprint);
            if (result)
            {
                if (thumbprint.ToUpper().Equals(Thumbprint.Null))
                    result = true;
                else if (thumbprint.ToUpper().Equals(Thumbprint.Empty))
                    result = true;
                else
                    result = false;
            }
            return result;
        }
    }
}
