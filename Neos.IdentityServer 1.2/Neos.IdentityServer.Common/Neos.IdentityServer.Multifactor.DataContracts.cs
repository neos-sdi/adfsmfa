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
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Neos.IdentityServer.MultiFactor
{
    [XmlRoot("MFAConfig")]
    public class MFAConfig
    {
        private KeyGeneratorMode _fgen = KeyGeneratorMode.ClientSecret512;
        private bool _isdirty = false;
        private string _country;

        /// <summary>
        /// Constructor
        /// </summary>
        public MFAConfig()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MFAConfig(bool initializedefaults):this()
        {
            if (initializedefaults)
            {
                _isdirty = false;
                DeliveryWindow = 300;
                TOTPShadows = 2;
                MailEnabled= true; 
                SMSEnabled = true; 
                AppsEnabled = true;
                Algorithm = HashMode.SHA1; 
                UseActiveDirectory = true; 
                CustomUpdatePassword = true;
                KeyGenerator = KeyGeneratorMode.ClientSecret512; 
                DefaultCountryCode = "fr";
                Hosts = new Hosts();
                Hosts.ActiveDirectoryHost = new ADDSHost();
                Hosts.SQLServerHost = new SQLServerHost();
                Hosts.SQLServerHost.ConnectionString="Password=yourpassword;Persist Security Info=True;User ID=yoursqlusername;Initial Catalog=yourdatabasename;Data Source=yoursqlserver\\yourinstance";
                Hosts.ADFSFarm = new ADFSFarmHost();
                Hosts.ADFSFarm.IsInitialized = false;
                SendMail = new SendMail();
                SendMail.From="sender.email@contoso.com"; 
                SendMail.UserName="user.name@contoso.com"; 
                SendMail.Password ="yourpass" ;
                SendMail.Host="smtp.office365.com";
                SendMail.Port = 587; 
                SendMail.UseSSL = true;
                SendMail.Company="Contoso";
            }
        }

        [XmlIgnore]
        public bool IsDirty
        {
            get { return _isdirty; }
            set { _isdirty = value; }
        }

        [XmlElement("Hosts")]
        public Hosts Hosts
        {
            get;
            set;
        }

        [XmlElement("SendMail")]
        public SendMail SendMail
        {
            get;
            set;
        }

        [XmlElement("ExternalOTPProvider")]
        public ExternalOTPProvider ExternalOTPProvider
        {
            get;
            set;
        }

        [XmlAttribute("DeliveryWindow")]
        public int DeliveryWindow
        {
            get;
            set;
        }

        [XmlAttribute("TOTPShadows")]
        public int TOTPShadows
        {
            get;
            set;
        }

        [XmlAttribute("MailEnabled")]
        public bool MailEnabled
        {
            get;
            set;
        }

        [XmlAttribute("SMSEnabled")]
        public bool SMSEnabled
        {
            get;
            set;
        }

        [XmlAttribute("AppsEnabled")]
        public bool AppsEnabled
        {
            get;
            set;
        }

        [XmlAttribute("Algorithm")]
        public HashMode Algorithm
        {
            get;
            set;
        }

        [XmlAttribute("Issuer")]
        public string Issuer
        {
            get;
            set;
        }

        [XmlAttribute("UseActiveDirectory")]
        public bool UseActiveDirectory
        {
            get;
            set;
        }

        [XmlAttribute("CustomUpdatePassword")]
        public bool CustomUpdatePassword
        {
            get;
            set;
        }

        [XmlAttribute("KeyGenerator")]
        public KeyGeneratorMode KeyGenerator
        {
            get { return _fgen; }
            set { _fgen = value; }
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
    }

    /// <summary>
    /// Hosts contract class
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

        [XmlElement("ADFSFarm")]
        public ADFSFarmHost ADFSFarm
        {
            get { return _host; }
            set { _host = value; }
        }
    }

    /// <summary>
    /// SQLServerHost  contract class
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
    /// ADDSHost  contract class
    /// </summary>
    public class ADDSHost
    {
        string _keyattr = "msDS-cloudExtensionAttribute10";
        string _mailattr = "msDS-cloudExtensionAttribute11";
        string _phoneattr = "msDS-cloudExtensionAttribute12";
        string _methodattr = "msDS-cloudExtensionAttribute13";
        string _notifCreateattr = "msDS-cloudExtensionAttribute14";
        string _notifValidityattr = "msDS-cloudExtensionAttribute15";
        string _notifcheckdateattribute = "msDS-cloudExtensionAttribute16";
        string _TOTPAttr = "msDS-cloudExtensionAttribute17";
        string _TOTPEnabled = "msDS-cloudExtensionAttribute18";

        [XmlAttribute("DomainAddress")]
        public string DomainAddress
        {
            get;
            set;
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

        [XmlAttribute("mailattribute")]
        public string mailAttribute
        {
            get { return _mailattr;}
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

        [XmlAttribute("methodattribute")]
        public string methodAttribute
        {
            get { return _methodattr; }
            set { _methodattr = value; }
        }

        [XmlAttribute("notifcreatedateattribute")]
        public string notifcreatedateAttribute
        {
            get { return _notifCreateattr; }
            set { _notifCreateattr = value; }
        }

        [XmlAttribute("notifvaliditydateattribute")]
        public string notifvalidityAttribute
        {
            get { return _notifValidityattr; }
            set { _notifValidityattr = value; }
        }

        [XmlAttribute("notifcheckdateattribute")]
        public string notifcheckdateattribute
        {
            get { return _notifcheckdateattribute; }
            set { _notifcheckdateattribute = value; }
        }

        [XmlAttribute("totpattribute")]
        public string totpAttribute
        {
            get { return _TOTPAttr; }
            set { _TOTPAttr = value; }
        }

        [XmlAttribute("totpEnabledAttribute")]
        public string totpEnabledAttribute
        {
            get { return _TOTPEnabled; }
            set { _TOTPEnabled = value; }
        }
    }

    #region ADFSServer Classes
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

        [XmlElement("ADFSNode")]
        public List<ADFSServerHost> Servers
        {
            get { return _lst; }
            set { _lst = value; }
        }
    }

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
    }
    #endregion

    /// <summary>
    /// SendMail contract
    /// </summary>
    public class SendMail
    {
        private string _comp = "your company description";

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

        [XmlElement("MailContent", IsNullable=true)]
        public string MailContent
        {
            get;
            set;
        }
    }

    /// <summary>
    /// ExternalOTPProvider contract
    /// </summary>
    public class ExternalOTPProvider
    {
        private string _comp = "your company description";
        private string _class;
        private string _cdata;
        private string _sha1 = "0x123456789";
        private bool   _istwoway = false;
        private int _timeout = 300;

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

        [XmlElement("Parameters")]
        public CData Parmeters 
        {
            get { return _cdata; }
            set { _cdata = value; }
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

    [XmlSchemaProvider("GetSchema")]
    public sealed class CData : IXmlSerializable
    {

        /// <summary>
        /// GetSchema method implementation
        /// </summary>
        public static XmlQualifiedName GetSchema(XmlSchemaSet xs)
        {           
            return XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String).QualifiedName;
        }

        /// <summary>
        /// implicit operation to string
        /// </summary>
        public static implicit operator string(CData value)
        {
            return value == null ? null : value.Value;
        }

        /// <summary>
        /// implict operator to CData
        /// </summary>
        public static implicit operator CData(string value)
        {
            return value == null ? null : new CData{Value = value};
        }

        /// <summary>
        /// GetSchema method implementation
        /// </summary>
        public XmlSchema GetSchema()
        {            
            return null;
        }

        /// <summary>
        /// WriteXml method implementation
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            if (string.IsNullOrEmpty(Value)) return;
            if ((Value.Contains("")) && !Value.Contains("]]>"))
            {
                writer.WriteCData(Value);
            }
            else
            {
                writer.WriteString(Value);
            }
        }

        /// <summary>
        /// ReadXml method implementation
        /// </summary>
        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                Value = "";
            }
            else
            {
                reader.Read();
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        Value = "";
                        break;
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        Value = reader.ReadContentAsString();
                        break;
                    default:
                        throw new InvalidOperationException("Invalid DataType must be text or CData : "+ reader.NodeType);
                }
            }
        }

        /// <summary>
        /// Value property implementation
        /// </summary>
        public string Value 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// ToString method override
        /// </summary>
        public override string ToString()
        {
            return Value;
        }
   }
}
