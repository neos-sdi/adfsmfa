//******************************************************************************************************************************************************************************************//
// Copyright (c) 2015 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Neos.IdentityServer.MultiFactor
{
    [XmlRoot("MFAConfig")]
    public class MFAConfig
    {
        KeyGeneratorMode _fgen = KeyGeneratorMode.ClientSecret512;

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
    }

    /// <summary>
    /// Hosts contract class
    /// </summary>
    public class Hosts
    {
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
        string _TOTPpattr = "msDS-cloudExtensionAttribute17";

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
            get { return _TOTPpattr; }
            set { _TOTPpattr = value; }
        }
    }

    /// <summary>
    /// SendMail contract
    /// </summary>
    public class SendMail
    {
        private string _html = string.Empty;
      /*  private string _html = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">" +
                      "<html>" +
                        "<head>" +
                            "<title></title>" +
                            "<base target=\"_blank\">" +
                        "</head>" +
                        "<body>" +
                            "<table dir=\"ltr\">" +
                                "<tr>" +
                                    "<td id=\"i1\" style=\"padding:0;font-family:'Segoe UI Semibold', 'Segoe UI Bold', 'Segoe UI', 'Helvetica Neue Medium', Arial, sans-serif;font-size:17px;color:#707070;\">Compte {0}</td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td id=\"i2\" style=\"padding:0;font-family:'Segoe UI Light', 'Segoe UI', 'Helvetica Neue Medium', Arial, sans-serif;font-size:41px;color:#2672ec;\">Code de sécurité</td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td id=\"i3\" style=\"padding:0;padding-top:25px;font-family:'Segoe UI', Tahoma, Verdana, Arial, sans-serif;font-size:14px;color:#2a2a2a;\">Utilisez le code de sécurité suivant pour le compte {0} : " +
                                        "<a dir=\"ltr\" id=\"iAccount\" class=\"link\" style=\"color:#2672ec;text-decoration:none;\" >{1}." +
                                    "</td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td id=\"i4\" style=\"padding:0;padding-top:25px;font-family:'Segoe UI', Tahoma, Verdana, Arial, sans-serif;font-size:14px;color:#2a2a2a;\">Code de sécurité : " +
                                        "<span style=\"font-family:'Segoe UI Bold', 'Segoe UI Semibold', 'Segoe UI', 'Helvetica Neue Medium', Arial, sans-serif;font-size:18px;font-weight:bold;color:#2a2a2a;\">{2}</span>" +
                                    "</td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td id=\"i6\" style=\"padding:0;padding-top:25px;font-family:'Segoe UI', Tahoma, Verdana, Arial, sans-serif;font-size:14px;color:#2a2a2a;\">Si vous n’avez demandé aucun code, vous devez ignorer cet e-mail. Un autre utilisateur a peut-être indiqué votre adresse e-mail par erreur.</td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td id=\"i7\" style=\"padding:0;padding-top:25px;font-family:'Segoe UI', Tahoma, Verdana, Arial, sans-serif;font-size:14px;color:#2a2a2a;\">Merci,</td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td id=\"i8\" style=\"padding:0;font-family:'Segoe UI', Tahoma, Verdana, Arial, sans-serif;font-size:14px;color:#2a2a2a;\">L’équipe des comptes {0}</td>" +
                                "</tr>" +
                            "</table>" +
                            "<br />" +
                    "</body>" +
                "</html>";  */                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   

        private string _comp = "redhook software";

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
}
