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
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor.Resources;

namespace Neos.IdentityServer.MultiFactor
{
    using System.DirectoryServices.AccountManagement;
    using System.Resources;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml.Linq;
    using System.Security.Cryptography;

    /// <summary>
    /// Utilities class implementation
    /// </summary>
    internal static class Utilities
    {
		/// <summary>
		/// SendMail method implementation
		/// </summary>
        private static void SendMail(MailMessage Message, SendMail mail)
        {
            SmtpClient client = new SmtpClient();
            client.Host = mail.Host;
            client.Port = mail.Port;
            client.UseDefaultCredentials = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = mail.UseSSL;
            client.Credentials = new NetworkCredential(mail.UserName, mail.Password);
            client.Send(Message);
        }

		/// <summary>
        /// SendOTPByEmail method implementation
		/// </summary>
        internal static void SendOTPByEmail(string to, string upn, string code, SendMail mail)
        {
            string htmlres = mail.MailContent;
            if (string.IsNullOrEmpty(htmlres))
                htmlres = mail_strings.MailContent;
            string html = StripEmailContent(htmlres);
            string name = upn.Remove(2, upn.IndexOf('@') - 2).Insert(2, "*********");
            MailMessage Message = new MailMessage(mail.From, to);
            Message.BodyEncoding = UTF8Encoding.UTF8;
            Message.IsBodyHtml = true;
            Message.Body = string.Format(html, mail.Company, name, code);
            Message.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;
            Message.Subject = title_strings.MailTitle;  
            SendMail(Message, mail);
        }

        #region private methods
        /// <summary>
        /// StripPhoneNumer method
        /// </summary>
        internal static string StripPhoneNumer(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return "* ** ** ** **";
            else
                return "* ** ** ** " + phone.Substring(phone.Length - 2, 2);
        }

        /// <summary>
        /// StripEmailAddress method
        /// </summary>
        internal static string StripEmailAddress(string email)
        {
            if (string.IsNullOrEmpty(email))
                return string.Empty;
            else
                return email.Remove(2, email.IndexOf('@') - 2).Insert(2, "*********"); 
        }

        /// <summary>
        /// StripEmailDomain method
        /// </summary>
        internal static string StripEmailDomain(string email)
        {
            if (string.IsNullOrEmpty(email))
                return string.Empty;
            else
                return email.Substring(email.IndexOf("@", 0));
        }

        /// <summary>
        /// StripEmailContent method 
        /// </summary>
        internal static string StripEmailContent(string content)
        {
            return content.Replace("<![CDATA[", "").Replace("]]>","");
        }
        #endregion
    }

	/// <summary>
    /// RemoteAdminService class implementation
	/// </summary>
    internal static class RemoteAdminService
    {
        /// <summary>
        /// GetUserRegistration method implementation
        /// </summary>
        internal static Registration GetUserRegistration(string upn, MFAConfig cfg)
        {
            if (cfg.UseActiveDirectory)
            {
                ADAdminService client = new ADAdminService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                return client.GetUserRegistration(upn);

            }
            else
            {
                AdminService client = new AdminService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                return client.GetUserRegistration(upn);
            }
        }

        /// <summary>
        /// SetUserRegistration method implementation
        /// </summary>
        internal static void SetUserRegistration(Registration registration, MFAConfig cfg)
        {
            EnsureSecretKey(registration, cfg);
            if (cfg.UseActiveDirectory)
            {
                ADAdminService client = new ADAdminService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                client.SetUserRegistration(registration);
            }
            else
            {
                AdminService client = new AdminService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                client.SetUserRegistration(registration);
            }
        }

        /// <summary>
        /// EnsureSecretKey method iplementation
        /// </summary>
        internal static void EnsureSecretKey(Registration registration, MFAConfig cfg)
        {
            if (string.IsNullOrEmpty(registration.SecretKey))
            {
                registration.SecretKey = GetNewSecretKey(cfg);
            }
        }

        /// <summary>
        /// GetNewSecretKey method implmentation
        /// </summary>
        internal static string GetNewSecretKey(MFAConfig cfg)
        {
            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            byte[] buffer = null;
            switch (cfg.KeyGenerator)
            {
                case KeyGeneratorMode.ClientSecret128:
                    buffer = new byte[16];
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    return Convert.ToBase64String(buffer);
                case KeyGeneratorMode.ClientSecret256:
                    buffer = new byte[32];
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    return Convert.ToBase64String(buffer);
                case KeyGeneratorMode.ClientSecret384:
                    buffer = new byte[48];
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    return Convert.ToBase64String(buffer);
                case KeyGeneratorMode.ClientSecret512:
                    buffer = new byte[64];
                    cryptoRandomDataGenerator.GetBytes(buffer);
                    return Convert.ToBase64String(buffer);
                default:
                    return Guid.NewGuid().ToString("D");
            }
        }
        /// <summary>
        /// SetNotification method implmentation
        /// </summary>
        internal static Notification SetNotification(Registration registration, MFAConfig cfg)
        {
            if (cfg.UseActiveDirectory)
            {
                ADAdminService client = new ADAdminService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                return client.SetNotification(registration.ID, registration.PreferredMethod);
            }
            else
            {
                AdminService client = new AdminService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                return client.SetNotification(registration.ID, registration.PreferredMethod);
            }
        }

        /// <summary>
        /// CheckNotification method implementation
        /// </summary>
        internal static Notification CheckNotification(Registration registration, MFAConfig cfg)
        {
            if (cfg.UseActiveDirectory)
            {
                ADAdminService client = new ADAdminService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                return client.CheckNotification(registration.ID);
            }
            else
            {
                AdminService client = new AdminService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                return client.CheckNotification(registration.ID);
            }
        }

        /// <summary>
        /// ChangePassword method implmentation
        /// </summary>
        internal static void ChangePassword(string username, string oldpassword, string newpassword)
        {
            using (var ctx = new PrincipalContext(ContextType.Domain))
            {
                using (var user = UserPrincipal.FindByIdentity(ctx, IdentityType.UserPrincipalName, username))
                {
                    user.ChangePassword(oldpassword, newpassword);
                }
            }
        }
    }    
}
