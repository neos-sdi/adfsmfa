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
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.IdentityServer.Web.Authentication.External;
using System.Text.RegularExpressions;
using System.ServiceModel;

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
        public AuthenticationContext(IAuthenticationContext ctx, MFAUser reg): this(ctx)
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
        public static explicit operator MFAUser(AuthenticationContext context)
        {
            MFAUser registration = new MFAUser
            {
                ID = context.ID,
                UPN = context.UPN,
                MailAddress = context.MailAddress,
                PhoneNumber = context.PhoneNumber,
                Enabled = context.Enabled,
                IsRegistered = context.IsRegistered,
                PreferredMethod = context.PreferredMethod,
                PIN = context.PinCode,
                OverrideMethod = context.OverrideMethod
            };
            return registration;
        }

        /// <summary>
        /// Assign method
        /// </summary>
        public void Assign(MFAUser reg)
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
        /// NotificationSent property implementation
        /// </summary>
        [XmlAttribute("NotificationSent")]
        public bool NotificationSent
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxnotifsent") && _context.Data["_authctxnotifsent"] != null)
                    return (bool)_context.Data["_authctxnotifsent"];
                else
                    return false;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxnotifsent"))
                    _context.Data["_authctxnotifsent"] = value;
                else
                    _context.Data.Add("_authctxnotifsent", value);
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
        /// UserLcid 
        /// </summary>
        [XmlAttribute("Lcid")]
        public int Lcid
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxuserlcid") && _context.Data["_authctxuserlcid"] != null)
                    return Convert.ToInt32(_context.Data["_authctxuserlcid"]);
                else
                    return _context.Lcid;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxuserlcid"))
                    _context.Data["_authctxuserlcid"] = value;
                else
                    _context.Data.Add("_authctxuserlcid", value);
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
        /// LogonDate property implementation
        /// </summary>
        [XmlAttribute("LogonDate")]
        public DateTime LogonDate
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxlogondate") && _context.Data["_authctxlogondate"] != null)
                    return (DateTime)_context.Data["_authctxlogondate"];
                else
                    return DateTime.MinValue;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxlogondate"))
                    _context.Data["_authctxlogondate"] = (DateTime)value;
                else
                    _context.Data.Add("_authctxlogondate", (DateTime)value);
            }
        }

        /// <summary>
        /// IPAddress 
        /// </summary>
        [XmlAttribute("IPAddress")]
        public string IPAddress
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxipaddress") && _context.Data["_authctxipaddress"] != null)
                    return _context.Data["_authctxipaddress"].ToString();
                else
                    return "127.0.0.1";
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxipaddress"))
                    _context.Data["_authctxipaddress"] = value;
                else
                    _context.Data.Add("_authctxipaddress", value);
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
        /// PinDone property implementation
        /// </summary>
        [XmlAttribute("PinDone")]
        public bool PinDone
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxpindone") && _context.Data["_authctxpindone"] != null)
                    return (bool)_context.Data["_authctxpindone"];
                else
                    return false;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxpindone"))
                    _context.Data["_authctxpindone"] = value;
                else
                    _context.Data.Add("_authctxpindone", value);
            }
        }


        /// <summary>
        /// ExtraInfos property implementation
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
        /// FirstChoiceMethod property implementation
        /// </summary>
        [XmlAttribute("FirstChoiceMethod")]
        public PreferredMethod FirstChoiceMethod
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxfirstmethod") && _context.Data["_authctxfirstmethod"] != null)
                    return (PreferredMethod)_context.Data["_authctxfirstmethod"];
                else
                    return (int)PreferredMethod.Choose;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxfirstmethod"))
                    _context.Data["_authctxfirstmethod"] = (int)value;
                else
                    _context.Data.Add("_authctxfirstmethod", (int)value);
            }
        }

        /// <summary>
        /// CurrentRetries property implementation
        /// </summary>
        [XmlAttribute("CurrentRetries")]
        public int CurrentRetries
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxcurrentretries") && _context.Data["_authctxcurrentretries"] != null)
                    return (int)_context.Data["_authctxcurrentretries"];
                else
                    return 0;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxcurrentretries"))
                    _context.Data["_authctxcurrentretries"] = (int)value;
                else
                    _context.Data.Add("_authctxcurrentretries", (int)value);
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
                    return AuthenticationResponseKind.Error;
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
        /// WizPageID property implementation
        /// </summary>
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
        /// EnrollPageID property implementation
        /// </summary>
        [XmlAttribute("EnrollPageID")]
        public PreferredMethod EnrollPageID
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxenrollpageid") && _context.Data["_authctxenrollpageid"] != null)
                    return (PreferredMethod)_context.Data["_authctxenrollpageid"];
                else
                    return PreferredMethod.Choose;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxenrollpageid"))
                    _context.Data["_authctxenrollpageid"] = (int)value;
                else
                    _context.Data.Add("_authctxenrollpageid", (int)value);
            }
        }

        /// <summary>
        /// EnrollPageStatus property implementation
        /// </summary>
        [XmlAttribute("EnrollPageStatus")]
        public EnrollPageStatus EnrollPageStatus
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxenrollpagest") && _context.Data["_authctxenrollpagest"] != null)
                    return (EnrollPageStatus)_context.Data["_authctxenrollpagest"];
                else
                    return EnrollPageStatus.Start;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxenrollpagest"))
                    _context.Data["_authctxenrollpagest"] = (int)value;
                else
                    _context.Data.Add("_authctxenrollpagest", (int)value);
            }
        }

        /// <summary>
        /// WizContext property implementation
        /// </summary>
        [XmlAttribute("WizContext")]
        public WizardContextMode WizContext
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxwizcontext") && _context.Data["_authctxwizcontext"] != null)
                    return (WizardContextMode)_context.Data["_authctxwizcontext"];
                else
                    return WizardContextMode.ManageOptions;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxwizcontext"))
                    _context.Data["_authctxwizcontext"] = (int)value;
                else
                    _context.Data.Add("_authctxwizcontext", (int)value);
            }
        }

        /// <summary>
        /// DirectLogin property implementation
        /// </summary>
        [XmlAttribute("DirectLogin")]
        public bool DirectLogin
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxdirectlogin") && _context.Data["_authctxdirectlogin"] != null)
                    return (bool)_context.Data["_authctxdirectlogin"];
                else
                    return true;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxdirectlogin"))
                    _context.Data["_authctxdirectlogin"] = value;
                else
                    _context.Data.Add("_authctxdirectlogin", value);
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

        /// <summary>
        /// CredentialOptions property implementation
        /// </summary>
        [XmlAttribute("CredentialOptions")]
        public string CredentialOptions
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxcredoptions") && _context.Data["_authctxcredoptions"] != null)
                    return _context.Data["_authctxcredoptions"].ToString();
                else
                    return string.Empty;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxcredoptions"))
                    _context.Data["_authctxcredoptions"] = value;
                else
                    _context.Data.Add("_authctxcredoptions", value);
            }
        }

        /// <summary>
        /// CredentialOptions property implementation
        /// </summary>
        [XmlAttribute("AssertionOptions")]
        public string AssertionOptions
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxassertionoptions") && _context.Data["_authctxassertionoptions"] != null)
                    return _context.Data["_authctxassertionoptions"].ToString();
                else
                    return string.Empty;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxassertionoptions"))
                    _context.Data["_authctxassertionoptions"] = value;
                else
                    _context.Data.Add("_authctxassertionoptions", value);
            }
        }

        /// <summary>
        /// PinRequirements property implementation
        /// </summary>
        [XmlAttribute("PinRequirements")]
        public bool PinRequirements
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxpinrequirements") && _context.Data["_authctxpinrequirements"] != null)
                    return (bool)_context.Data["_authctxpinrequirements"];
                else
                    return false;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxpinrequirements"))
                    _context.Data["_authctxpinrequirements"] = value;
                else
                    _context.Data.Add("_authctxpinrequirements", value);
            }
        }

        /// <summary>
        /// ThemeIdentifier property implementation
        /// </summary>
        [XmlAttribute("ThemeIdentifier")]
        public string ThemeIdentifier
        {
            get
            {
                if (_context.Data.ContainsKey("_authctxthemeidentifier") && _context.Data["_authctxthemeidentifier"] != null)
                    return _context.Data["_authctxthemeidentifier"].ToString();
                else
                    return string.Empty;
            }
            set
            {
                if (_context.Data.ContainsKey("_authctxthemeidentifier"))
                    _context.Data["_authctxthemeidentifier"] = value;
                else
                    _context.Data.Add("_authctxthemeidentifier", value);
            }
        }
    }
    #endregion

    #region MFA Data
    /// <summary>
    /// MFAUserList class implementation
    /// </summary>
    [Serializable]
    public class MFAUserList : List<MFAUser>
    {
        public MFAUserList() { }

        public MFAUserList(List<MFAUser> data):base(data) { }

        /// <summary>
        /// implicit conversion to byte array
        /// </summary>
        public static implicit operator byte[](MFAUserList registrations)
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
        public static implicit operator MFAUserList(byte[] data)
        {
            if (data == null)
                return null;
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(data, 0, data.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return (MFAUserList)binForm.Deserialize(memStream);
            }
        }
    }

    /// <summary>
    /// MFAUser Class implementation
    /// </summary>
    [Serializable]
    public class MFAUser
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
        public MFAUser()
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
        Biometrics = -2000,

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
        public bool RequiredBiometrics = false;
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

        public void Clear()
        {
            _currentpage = 0;
            _pagesize = 50;
            IsActive = false;
        }

        public bool IsActive { get; private set; } = false;

        [XmlAttribute("CurrentPage")]
        public int CurrentPage
        {
            get { return _currentpage; }
            set
            {
                if (value < 0)
                    value = 0;
                _currentpage = value;
                IsActive = _currentpage > 0;
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

    /// <summary>
    /// SecretKeyFormat
    /// </summary>
    [Serializable]
    public enum SecretKeyFormat
    {
        RNG = 0,
        RSA = 1,
        AES = 2,
        CUSTOM = 3
    }

    /// <summary>
    /// SecretKeyVersion
    /// </summary>
    [Serializable]
    public enum SecretKeyVersion
    {
        V1 = 1,
        V2 = 2,
        V3 = 3
    }

    /// <summary>
    /// PrimaryAuthOptions
    /// </summary>
    [Serializable, Flags]
    public enum PrimaryAuthOptions
    {
        None = 0,
        Externals = 1,
        Register = 2
    }

    /// <summary>
    /// HashMode
    /// </summary>
    [Serializable]
    public enum HashMode
    {
        SHA1 = 0,
        SHA256 = 1,
        SHA384 = 2,
        SHA512 = 3
    }

    /// <summary>
    /// KeyGeneratorMode
    /// </summary>
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

    /// <summary>
    /// AESKeyGeneratorMode
    /// </summary>
    [Serializable]
    public enum AESKeyGeneratorMode
    {
        AESSecret512 = 0,
        AESSecret1024 = 1,
    }

    /// <summary>
    /// NotificationsKind
    /// </summary>
    [Serializable]
    public enum NotificationsKind: byte
    {
        None = 0x00,
        ConfigurationReload  = 0xAA,
        ConfigurationCreated = 0xAB,
        ConfigurationDeleted = 0xAC,
        ServiceStatusRunning = 0x10,
        ServiceStatusStopped = 0x11,
        ServiceStatusPending = 0x12,
        ServiceStatusInError = 0x19,
    }

    /// <summary>
    /// OTPWizardOptions
    /// </summary>
    [Serializable, Flags]
    public enum OTPWizardOptions
    {
        All = 0x0,
        NoMicrosoftAuthenticator = 0x1,
        NoGoogleAuthenticator = 0x2,
        NoAuthyAuthenticator = 0x4,
        NoGooglSearch = 0x8
    }

    /// <summary>
    /// KeySizeMode
    /// </summary>
    [Serializable]
    public enum KeySizeMode
    {
        KeySizeDefault = 0,
        KeySize512 = 1,
        KeySize1024 = 2,
        KeySize2048 = 3,
        KeySize128 = 4,
        KeySize256 = 5,
        KeySize384 = 6
    }

    /// <summary>
    /// ForceWizardMode
    /// </summary>
    [Serializable]
    public enum ForceWizardMode
    {
        Disabled = 0,
        Enabled = 1,
        Strict = 2
    }

    /// <summary>
    /// WizardContextMode
    /// </summary>
    [Serializable]
    public enum WizardContextMode
    {
        ManageOptions = 0,
        Registration = 1,
        Invitation = 2,
        DirectWizards = 3,
        ForceWizard = 4
    }

    /// <summary>
    /// ProviderPageMode
    /// </summary>
    [Serializable]
    public enum ProviderPageMode
    {
        Locking = 0,
        Bypass = 1,
        Identification = 2,
        Registration = 3,
        Invitation = 4,
        Activation = 5,
        SelectOptions = 6,
        ChooseMethod = 7,
        ChangePassword = 8,
        ShowQRCode = 9,
        SendAuthRequest = 10,
        SendAdministrativeRequest = 11,
        SendKeyRequest = 12,
        PreSet = 13,
        EnrollOTP = 14,
        EnrollBiometrics = 15,
        EnrollEmail = 16,
        EnrollPhone = 17,
        EnrollPin = 18,
        ManageOptions = 63,
        None = 64,
        DefinitiveError = 128
    }

    /// <summary>
    /// PreferredMethod
    /// </summary>
    [Serializable]
    public enum PreferredMethod
    {
        Choose = 0,
        Code = 1,
        Email = 2,
        External = 3,
        Azure = 4,
        Biometrics = 5,
        Pin = 6,
        None = 7
    }

    /// <summary>
    /// EnrollPageStatus
    /// </summary>
    [Serializable]
    public enum EnrollPageStatus
    {
        Start = 0,
        Run = 1,
        Stop = 2,
        NewStep = 3
    }

    /// <summary>
    /// ADFSUserInterfaceKind
    /// </summary>
    [Serializable]
    public enum ADFSUserInterfaceKind
    {
        Default = 0,
        Default2019 = 1
    }

    /// <summary>
    /// ADDSTemplateKind
    /// </summary>
    [Serializable]
    public enum ADDSTemplateKind
    {
        AllSchemaVersions = 0,
        Windows2016Schemaversion = 1,
        MFASchemaVersion = 2
    }

    /// <summary>
    /// KeysDataManagerEventKind enum
    /// </summary>
    [Serializable]
    public enum KeysDataManagerEventKind
    {
        Get,
        add,
        Remove
    }

    /// <summary>
    /// DataRepositoryKind enum
    /// </summary>
    [Serializable]
    public enum DataRepositoryKind
    {
        ADDS = 0,
        SQL = 1,
        Custom = 2
    }

    /// <summary>
    /// WebAuthNPinRequirements enum
    /// </summary>
    [Flags, Serializable]
    public enum WebAuthNPinRequirements
    {
        Null = 0,
        None = 1,
        AndroidKey = 2,
        AndroidSafetyNet = 4,
        Fido2U2f = 8,
        Packed = 16,
        TPM = 32,
        Apple = 64
    }

    /// <summary>
    /// WebAuthNUserVerification enum
    /// </summary>
    [Serializable]
    public enum WebAuthNUserVerification
    {
        Optional = 0x1,
        OptionalWithCredentialIDList = 0x2,
        Required = 0x3
    }

    /// <summary>
    /// MFAUserCredential class
    /// </summary>
    [Serializable]
    public class MFAUserCredential
    {
        public byte[] UserId { get; set; }
        public MFAPublicKeyCredentialDescriptor Descriptor { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] UserHandle { get; set; }
        public uint SignatureCounter { get; set; }
        public string CredType { get; set; }
        public DateTime RegDate { get; set; }
        public Guid AaGuid { get; set; }
    }

    /// <summary>
    /// MFAWebAuthNUser class
    /// </summary>
    [Serializable]
    public class MFAWebAuthNUser
    {
        public string Name { get; set; }
        public byte[] Id { get; set; }
        public string DisplayName { get; set; }
    }

    /// <summary>
    /// MFAPublicKeyCredentialDescriptor class implementation
    /// </summary>
    [Serializable]
    public class MFAPublicKeyCredentialDescriptor
    {
        /// <summary>
        /// Constructors
        /// </summary>
        public MFAPublicKeyCredentialDescriptor() { }
        public MFAPublicKeyCredentialDescriptor(byte[] credentialId) { Id = credentialId; }

        /// <summary>
        /// Type property implementation
        /// </summary>
        public MFAPublicKeyCredentialType? Type { get; set; } = MFAPublicKeyCredentialType.PublicKey;

        /// <summary>
        /// ID Property implementation
        /// </summary>
        public byte[] Id { get; set; }

        /// <summary>
        /// Transports property implmentation
        /// </summary>
        public MFAAuthenticatorTransport[] Transports { get; set; }
    };

    /// <summary>
    /// MFAPublicKeyCredentialType enum
    /// </summary>
    [Serializable]
    public enum MFAPublicKeyCredentialType
    {
        PublicKey
    }

    /// <summary>
    /// MFAAuthenticatorTransport enum
    /// </summary>
    [Serializable]
    public enum MFAAuthenticatorTransport
    {
        Usb,
        Nfc,
        Ble,
        Internal,
        Lightning
    }

    /// <summary>
    /// MFASecurityClaimTag enum
    /// </summary>
    [Serializable]
    public enum MFASecurityClaimTag
    {
        Upn = 0,
        WindowsAccountName = 1
    }

    /// <summary>
    /// ReplayRecord class implementation
    /// </summary>
    [DataContract]
    public class ReplayRecord
    {
        [DataMember]
        public bool MustDispatch;

        [DataMember]
        public int Code;

        [DataMember]
        public ReplayLevel ReplayLevel;

        [DataMember]
        public string UserIPAdress;

        [DataMember]
        public string UserName;

        [DataMember]
        public DateTime UserLogon;

        [DataMember]
        public int DeliveryWindow;
    }

    /// <summary>
    /// XORUtilities class
    /// </summary>
    public static class XORUtilities
    {
        static readonly string _defaultxor = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        static string _xorkey = _defaultxor;

        /// <summary>
        /// XORKey property
        /// </summary>
        public static string XORKey
        {
            get
            {
                if (string.IsNullOrEmpty(_xorkey))
                    return _defaultxor;
                else
                    return _xorkey;
            }
            set { _xorkey = value; }
        }

        /// <summary>
        /// DefaultKey property
        /// </summary>
        public static string DefaultKey
        {
            get { return _defaultxor; }
        }

        /// <summary>
        /// XOREncryptOrDecrypt method
        /// </summary>
        public static byte[] XOREncryptOrDecrypt(byte[] value, string secret)
        {
            if (string.IsNullOrEmpty(secret))
                secret = XORKey;
            byte[] xor = new byte[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                xor[i] = (byte)(value[i] ^ secret[i % secret.Length]);
            }
            return xor;
        }
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

    public class PlaceHolders
    {
        public string TagName { get; set; }
        public string FiledName { get; set; }
    }
}

