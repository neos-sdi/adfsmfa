using Neos.IdentityServer.MultiFactor.Resources;
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
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Neos.IdentityServer.MultiFactor
{
    using System.DirectoryServices.AccountManagement;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.ServiceModel.Channels;


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
        internal static Notification SetNotification(Registration registration, MFAConfig config, int otp)
        {
            if (config.UseActiveDirectory)
            {
                ADAdminService client = new ADAdminService(config.Hosts.ActiveDirectoryHost, config.DeliveryWindow);
                return client.SetNotification(registration, config, otp);
            }
            else
            {
                AdminService client = new AdminService(config.Hosts.SQLServerHost, config.DeliveryWindow);
                return client.SetNotification(registration, config, otp);
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

    /// <summary>
    /// Utilities static class
    /// </summary>
    public static class Utilities
    {
        private static IExternalOTPProvider _wrapper = null;
        /// <summary>
        /// GetRandomOTP  method implementation
        /// </summary>
        internal static int GetRandomOTP()
        {
            Random random = new Random();
            return random.Next(0, 1000000);
        }

        /// <summary>
        /// GetEmailOTP method implmentation
        /// </summary>
        public static int GetEmailOTP(Registration reg, SendMail mail)
        {
            int otpres = GetRandomOTP();
            SendOTPByEmail(reg.MailAddress, reg.UPN, otpres.ToString("D"), mail);
            return otpres;
        }

        /// <summary>
        /// GetPhoneOTP()
        /// </summary>
        /// <returns></returns>
        public static int GetPhoneOTP(Registration reg, ExternalOTPProvider sms)
        {
            if (reg.PreferredMethod == RegistrationPreferredMethod.Phone)
            {
                int result = -1;
                if (_wrapper == null)
                    _wrapper = LoadSMSwrapper(sms.FullQualifiedImplementation);
                if (_wrapper != null)
                {
                    result = _wrapper.GetUserCodeWithExternalSystem(reg.UPN, reg.PhoneNumber, reg.MailAddress, sms, html_strings.Culture);
                }
                return result;
            }
            else
                return -1;
        }

        /// <summary>
        /// LoadSMSwrapper method implmentation
        /// </summary>
        private static IExternalOTPProvider LoadSMSwrapper(string AssemblyFulldescription)
        {
            Assembly assembly = Assembly.Load(ParseAssembly(AssemblyFulldescription));

            Type _typetoload = assembly.GetType(ParseType(AssemblyFulldescription));
            IExternalOTPProvider _wrapper = null;
            if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalOTPProvider") != null)
            {
                object o = Activator.CreateInstance(_typetoload);
                if (o != null)
                    _wrapper = o as IExternalOTPProvider;
            }
            return _wrapper;
        }

        /// <summary>
        /// ParseType method implmentation
        /// </summary>
        private static string ParseAssembly(string AssemblyFulldescription)
        {
            int cnt = AssemblyFulldescription.IndexOf(',');
            return AssemblyFulldescription.Remove(0, cnt).TrimStart(new char[] { ',', ' ' });
        }

        /// <summary>
        /// ParseType method implmentation
        /// </summary>
        private static string ParseType(string AssemblyFulldescription)
        {
            string[] type = AssemblyFulldescription.Split(new char[] { ',' });
            return type[0];
        }

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
            return content.Replace("<![CDATA[", "").Replace("]]>", "");
        }
        #endregion
    }
}
