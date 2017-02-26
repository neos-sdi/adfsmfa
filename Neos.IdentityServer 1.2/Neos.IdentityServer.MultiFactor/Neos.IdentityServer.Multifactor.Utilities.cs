
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
using System.Net;
using System.Net.Mail;
using System.Text;
using Neos.IdentityServer.MultiFactor.Resources;

namespace Neos.IdentityServer.MultiFactor
{
    using System.DirectoryServices.AccountManagement;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.ServiceModel.Channels;
    using System.Diagnostics;
    using System.Threading;


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
                ADDSAdminService client = new ADDSAdminService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                return client.GetUserRegistration(upn);

            }
            else
            {
                SQLAdminService client = new SQLAdminService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                return client.GetUserRegistration(upn);
            }
        }

        /// <summary>
        /// SetUserRegistration method implementation
        /// </summary>
        internal static void SetUserRegistration(Registration registration, MFAConfig cfg)
        {
            KeyGenerator.EnsureSecretKey(registration, cfg);
            if (cfg.UseActiveDirectory)
            {
                ADDSAdminService client = new ADDSAdminService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                client.SetUserRegistration(registration);
            }
            else
            {
                SQLAdminService client = new SQLAdminService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
                client.SetUserRegistration(registration);
            }
        }

        /// <summary>
        /// SetNotification method implmentation
        /// </summary>
        internal static Notification SetNotification(Registration registration, MFAConfig config, int otp)
        {
            if (config.UseActiveDirectory)
            {
                ADDSAdminService client = new ADDSAdminService(config.Hosts.ActiveDirectoryHost, config.DeliveryWindow);
                return client.SetNotification(registration, config, otp);
            }
            else
            {
                SQLAdminService client = new SQLAdminService(config.Hosts.SQLServerHost, config.DeliveryWindow);
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
                ADDSAdminService client = new ADDSAdminService(cfg.Hosts.ActiveDirectoryHost, cfg.DeliveryWindow);
                return client.CheckNotification(registration.ID);
            }
            else
            {
                SQLAdminService client = new SQLAdminService(cfg.Hosts.SQLServerHost, cfg.DeliveryWindow);
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
            return random.Next(1, 1000000);
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
        public static int GetPhoneOTP(Registration reg, MFAConfig config)
        {
            if (config.ExternalOTPProvider == null)
                return NotificationStatus.Error;
            if (reg.PreferredMethod == RegistrationPreferredMethod.Phone)
            {
                if (_wrapper == null)
                   _wrapper = LoadSMSwrapper(config.ExternalOTPProvider.FullQualifiedImplementation);
                if (_wrapper != null)
                   return _wrapper.GetUserCodeWithExternalSystem(reg.UPN, reg.PhoneNumber, reg.MailAddress, config.ExternalOTPProvider, html_strings.Culture);
                else
                    return NotificationStatus.Error;
            }
            else
                return NotificationStatus.Error;
        }

        /// <summary>
        /// LoadSMSwrapper method implmentation
        /// </summary>
        private static IExternalOTPProvider LoadSMSwrapper(string AssemblyFulldescription)
        {
            Assembly assembly = Assembly.Load(ParseAssembly(AssemblyFulldescription));
            Type _typetoload = assembly.GetType(ParseType(AssemblyFulldescription));
            IExternalOTPProvider wrapper = null;
            if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalOTPProvider") != null)
            {
                object o = Activator.CreateInstance(_typetoload);
                if (o != null)
                    wrapper = o as IExternalOTPProvider;
            }
            return wrapper;
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
            try
            {
                if (string.IsNullOrEmpty(phone))
                    return "* ** ** ** **";
                else
                    return "* ** ** ** " + phone.Substring(phone.Length - 2, 2);
            }
            catch
            {
                return "* ** ** ** **";
            }
        }

        /// <summary>
        /// StripEmailAddress method
        /// </summary>
        internal static string StripEmailAddress(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return string.Empty;
                else
                    return email.Remove(2, email.IndexOf('@') - 2).Insert(2, "*********");
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// StripEmailDomain method
        /// </summary>
        internal static string StripEmailDomain(string email)
        {
            try
            {

                if (string.IsNullOrEmpty(email))
                    return string.Empty;
                else
                    return email.Substring(email.IndexOf("@", 0));
            }
            catch
            {
                return string.Empty;
            }
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
