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
#define softemail
using Microsoft.IdentityServer.Web.Authentication.External;
using Neos.IdentityServer.MultiFactor.QrEncoding;
using Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render;
using Neos.IdentityServer.MultiFactor.Resources;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// AuthenticationProvider class
    /// </summary>
    public class AuthenticationProvider : IAuthenticationAdapter
    {
        private MFAConfig _config;
        private string EventLogSource = "ADFS Multi-Factor Ex"; 
        private string EventLogGroup = "Application";

        /// <summary>
        /// Constructor override
        /// </summary>
        public AuthenticationProvider()
        {
            if (!EventLog.SourceExists(this.EventLogSource))
                EventLog.CreateEventSource(this.EventLogSource, this.EventLogGroup);
        }

        #region properties
        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }
        #endregion

        #region IAuthenticationAdapter implementation
        /// <summary>
        /// BeginAuthentication method implmentation
		/// </summary>
        public IAdapterPresentation BeginAuthentication(System.Security.Claims.Claim identityClaim, System.Net.HttpListenerRequest request, IAuthenticationContext context)
        {
            try
            {
                AuthenticationContext usercontext = new AuthenticationContext(context);
                SendNotification(usercontext);
                return new AdapterPresentation(this, context);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(EventLogSource, string.Format(errors_strings.ErrorAuthenticating, ex.Message), EventLogEntryType.Error, 802);
                throw new ExternalAuthenticationException();
            }
        }
		/// <summary>
        /// IsAvailableForUser method implementation
		/// </summary>
        public bool IsAvailableForUser(System.Security.Claims.Claim identityClaim, IAuthenticationContext context)
        {
            try
            {
                string upn = identityClaim.Value;
                Registration reg = RemoteAdminService.GetUserRegistration(upn, Config);
                if (reg != null)
                {
                    AuthenticationContext usercontext = new AuthenticationContext(context, reg);
                    if (usercontext.Enabled)
                    {
                        if (string.IsNullOrEmpty(usercontext.SecretKey)) 
                        {
                            usercontext.SecretKey = KeyGenerator.GetNewSecretKey(Config, usercontext.UPN);
                            usercontext.SecretKeyChanged = true;
                            usercontext.UIMode = ProviderPageMode.Registration;
                        }
                        else if (usercontext.PreferredMethod == RegistrationPreferredMethod.Choose)
                            usercontext.UIMode = ProviderPageMode.ChooseMethod;
                        else
                            usercontext.UIMode = ProviderPageMode.Identification;
                        return true;
                    }
                    else
                    {
                        usercontext.UIMode = ProviderPageMode.Bypass;
                        return true; // Can be refused in version 2.0, administrators can require MFA or deny access
                    }
                }
                else
                {
                    // New User - Auto Registration / can be disabled un version 2.0 and in this case bypass
                    AuthenticationContext usercontext = new AuthenticationContext(context);
                    usercontext.UPN = upn;
                    usercontext.Enabled = true;
                    usercontext.SecretKey = KeyGenerator.GetNewSecretKey(Config, usercontext.UPN);
                    usercontext.SecretKeyChanged = true;
                    usercontext.PreferredMethod = RegistrationPreferredMethod.Choose;
                    usercontext.UIMode = ProviderPageMode.Registration;
                    return true;
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(EventLogSource, string.Format(errors_strings.ErrorLoadingUserRegistration, ex.Message), EventLogEntryType.Error, 801);
                throw new ExternalAuthenticationException(ex.Message, context);
            }
        }

		/// <summary>
        /// Metadata property implementation
		/// </summary>
        public IAuthenticationAdapterMetadata Metadata
        {
            get { return new AuthenticationAdapterMetadata(); }
        }

		/// <summary>
        /// OnAuthenticationPipelineLoad method implmentation
		/// </summary>
        public void OnAuthenticationPipelineLoad(IAuthenticationMethodConfigData configData)
        {
             if (configData.Data != null)
             {
                 try
                 {
                     Stream stm = configData.Data;
                     XmlSerializer xmlserializer = new XmlSerializer(typeof(MFAConfig));
                     using (StreamReader reader = new StreamReader(stm))
                     {
                         _config = (MFAConfig)xmlserializer.Deserialize(stm);
                         if ((!_config.AppsEnabled) && (!_config.MailEnabled) && (!_config.SMSEnabled))
                             _config.MailEnabled = true;  // always let an active option eg : email in this case
                     }
                 }
                 catch (Exception EX)
                 {
                     EventLog.WriteEntry(EventLogSource, string.Format(errors_strings.ErrorLoadingConfigurationFile, EX.Message), EventLogEntryType.Error, 900);
                     throw new ExternalAuthenticationException();
                 }
             }
             else
             {
                 EventLog.WriteEntry(EventLogSource, errors_strings.ErrorLoadingConfigurationFileNotFound, EventLogEntryType.Error, 900);
                 throw new ExternalAuthenticationException();
             }
        }

		/// <summary>
        /// OnAuthenticationPipelineUnload method implementation
		/// </summary>
        public void OnAuthenticationPipelineUnload()
        {
            _config = null;
        }

		/// <summary>
        /// OnError method implementation
		/// </summary>
        public IAdapterPresentation OnError(System.Net.HttpListenerRequest request, ExternalAuthenticationException ex)
        {
            return new AdapterPresentation(this, null, ex.Message, true);  /// ICI
        }

		/// <summary>
        /// TryEndAuthentication method implementation
		/// </summary>
        public IAdapterPresentation TryEndAuthentication(IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            claims = null;
            IAdapterPresentation result = null;
            AdapterPresentation.SetCultureInfo(context.Lcid);
            AuthenticationContext usercontext = new AuthenticationContext(context);

            ProviderPageMode ui = usercontext.UIMode;        
            switch (ui)
            {
                case ProviderPageMode.Identification:
                    usercontext.SecretKeyChanged = false;
                    try
                    {
                        string pin = proofData.Properties["pin"].ToString();
                        object opt = null;
                        bool suitetooptions = proofData.Properties.TryGetValue("options", out opt);
                        int xlnk = Convert.ToInt32(proofData.Properties["lnk"].ToString());
                        if (xlnk==0)
                        {
                            Notification notif = RemoteAdminService.CheckNotification((Registration)usercontext, Config);
                            if (notif != null)
                            {
                                if (notif.CheckDate.Value.ToUniversalTime() > notif.ValidityDate.ToUniversalTime())  // Always check with Universal Time
                                {
                                    usercontext.UIMode = ProviderPageMode.Locking;
                                    return new AdapterPresentation(this, context, errors_strings.ErrorValidationTimeWindowElapsed, true);
                                }
                                if (CheckPin(pin, notif, usercontext))
                                {
                                    if (!suitetooptions)
                                    {
                                        System.Security.Claims.Claim claim0 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");
                                        System.Security.Claims.Claim claim1 = null;
                                        switch (usercontext.PreferredMethod)
                                        {
                                            case RegistrationPreferredMethod.Email:
                                                claim1 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpmode", "mail");
                                                break;
                                            case RegistrationPreferredMethod.Phone:
                                                claim1 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpmode", "phone");
                                                break;
                                            case RegistrationPreferredMethod.ApplicationCode:
                                                claim1 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpmode", "appcode");
                                                break;
                                        }
                                        TimeSpan duration = notif.CheckDate.Value.Subtract(notif.CreationDate);  // We Want only the difference no need UTC
                                        System.Security.Claims.Claim claim2 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpduration", duration.ToString());
                                        claims = new System.Security.Claims.Claim[] { claim0, claim1, claim2 };
                                    }
                                    if (suitetooptions)
                                    {
                                        usercontext.UIMode = ProviderPageMode.SelectOptions;
                                        result = new AdapterPresentation(this, context);
                                    }
                                }
                                else
                                {
                                    usercontext.UIMode = ProviderPageMode.Locking;
                                    result = new AdapterPresentation(this, context, errors_strings.ErrorInvalidIdentificationRestart, true);
                                }
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.Locking;
                                result = new AdapterPresentation(this, context, errors_strings.ErrorInvalidIdentificationRestart, true);
                            }
                        }
                        else
                        {
                            usercontext.UIMode = ProviderPageMode.ChooseMethod;
                            result = new AdapterPresentation(this, context);
                        }
                    }
                    catch (Exception ex)
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        result = new AdapterPresentation(this, context, ex.Message, true);
                    }
                    break;
                case ProviderPageMode.Registration:
                    usercontext.SecretKeyChanged = false;
                    try
                    {
                        string email = null;
                        string secretkey = null;
                        string phone = null;
                        int xlnk2 = 0;

                        if (Config.MailEnabled)
                        {
                            email = proofData.Properties["email"].ToString();
                            ValidateEmail(email);
                            usercontext.MailAddress = email;
                        }
                        if ((Config.AppsEnabled) && (!string.IsNullOrEmpty(usercontext.SecretKey)))
                        {
                            secretkey = proofData.Properties["secretkey"].ToString();
                            xlnk2 = Convert.ToInt32(proofData.Properties["lnk"].ToString());
                        }
                        if (Config.SMSEnabled)
                        {
                            phone = proofData.Properties["phone"].ToString();
                            usercontext.PhoneNumber = phone;
                        }
                        int select = Convert.ToInt32(proofData.Properties["selectopt"].ToString());

                        usercontext.PreferredMethod = (RegistrationPreferredMethod)select;
                        usercontext.UIMode = ProviderPageMode.SelectOptions;

                        if (xlnk2 == 0)
                        {
                            string btnclicka = proofData.Properties["btnclicked"].ToString();
                            if (btnclicka == "1")
                            {
                                RemoteAdminService.SetUserRegistration((Registration)usercontext, Config);
                                usercontext.UIMode = ProviderPageMode.SelectOptions;
                                result = new AdapterPresentation(this, context, infos_strings.InfosConfigurationModified);
                            }
                            else
                                result = new AdapterPresentation(this, context);
                        }
                        else if (xlnk2 == 1)
                        {
                            usercontext.UIMode = ProviderPageMode.ShowQRCode;
                            result = new AdapterPresentation(this, context);
                        }
                        else if (xlnk2 == 2)
                        {
                            usercontext.SecretKey = KeyGenerator.GetNewSecretKey(Config, usercontext.UPN);
                            RemoteAdminService.SetUserRegistration((Registration)usercontext, Config);
                            usercontext.UIMode = ProviderPageMode.Registration;
                            usercontext.SecretKeyChanged = true;
                            result = new AdapterPresentation(this, context, infos_strings.InfosConfigurationModified);
                        }
                    }
                    catch (Exception ex)
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        result = new AdapterPresentation(this, context, ex.Message, true);
                    }
                    break;

                case ProviderPageMode.ChangePassword:
                    usercontext.SecretKeyChanged = false;
                    if (_config.CustomUpdatePassword)
                    {
                        try
                        {
                            string oldpass = proofData.Properties["oldpwd"].ToString();
                            string newpass = proofData.Properties["newpwd"].ToString();
                            string cnfpass = proofData.Properties["cnfpwd"].ToString();
                            if (newpass.Equals(cnfpass))
                            {
                                try
                                {
                                    usercontext.UIMode = ProviderPageMode.SelectOptions;
                                    string btnclick = proofData.Properties["btnclicked"].ToString();
                                    if (btnclick == "1")
                                    {
                                        RemoteAdminService.ChangePassword(usercontext.UPN, oldpass, newpass);
                                        result = new AdapterPresentation(this, context, infos_strings.InfosPasswordModified);
                                    }
                                    else
                                        result = new AdapterPresentation(this, context);
                                }
                                catch (Exception ex)
                                {
                                    usercontext.UIMode = ProviderPageMode.Locking;
                                    result = new AdapterPresentation(this, context, ex.Message, true);
                                }
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.SelectOptions;
                                result = new AdapterPresentation(this, context, errors_strings.ErrorInvalidPassword, true);
                            }
                        }
                        catch (Exception ex)
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, ex.Message, true);
                        }
                    }
                    else
                    {
                        try
                        {
                        }
                        catch (Exception ex)
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, ex.Message, true);
                        }
                    }
                    break;
                case ProviderPageMode.Bypass:
                    usercontext.SecretKeyChanged = false;
                    System.Security.Claims.Claim claimx1 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");
                    System.Security.Claims.Claim claimx2 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpmode", "bypass");
                    System.Security.Claims.Claim claimx3 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpduration", "0");
                    claims = new System.Security.Claims.Claim[] { claimx1, claimx2, claimx3 };
                    break;
                case ProviderPageMode.SelectOptions:
                    usercontext.SecretKeyChanged = false;
                    try
                    {
                        int lnk = Convert.ToInt32(proofData.Properties["lnk"].ToString());
                        int xlnk4 = Convert.ToInt32(proofData.Properties["btnclicked"].ToString());
                        if (xlnk4 != 0)
                        {
                            usercontext.UIMode = ProviderPageMode.Bypass;
                            result = new AdapterPresentation(this, context);
                        }
                        else
                        {
                            switch (lnk)
                            {
                                case 1:
                                    usercontext.UIMode = ProviderPageMode.Registration;
                                    break;
                                case 2:
                                    usercontext.UIMode = ProviderPageMode.ChangePassword;
                                    break;
                            }
                            result = new AdapterPresentation(this, context);
                        }
                    }
                    catch (Exception ex)
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        result = new AdapterPresentation(this, context, ex.Message, true);
                    }
                    break;
                case ProviderPageMode.ChooseMethod:
                    usercontext.SecretKeyChanged = false;
                    try
                    {
                        usercontext.PreferredMethod = RegistrationPreferredMethod.ApplicationCode;
                        int optsel = Convert.ToInt32(proofData.Properties["opt"].ToString());
                        object rem = null;
                        bool dorem = proofData.Properties.TryGetValue("remember", out rem);
                        switch (optsel)
                        {
                            case 0:
                                if (Config.AppsEnabled)
                                {
                                    usercontext.PreferredMethod = RegistrationPreferredMethod.ApplicationCode;
                                    usercontext.UIMode = ProviderPageMode.Identification;
                                    SendNotification(usercontext);
                                    result = new AdapterPresentation(this, context);
                                    if (dorem)
                                        RemoteAdminService.SetUserRegistration((Registration)usercontext, Config);
                                }
                                else
                                {
                                    usercontext.PreferredMethod = RegistrationPreferredMethod.Choose;
                                    usercontext.UIMode = ProviderPageMode.Locking;
                                    result = new AdapterPresentation(this, context, errors_strings.ErrorInvalidIdentificationRestart, true);
                                }
                                break;
                            case 1:
                                if (Config.SMSEnabled)
                                {
                                    usercontext.PreferredMethod = RegistrationPreferredMethod.Phone;
                                    usercontext.UIMode = ProviderPageMode.Identification;
                                    SendNotification(usercontext);
                                    result = new AdapterPresentation(this, context);
                                    if (dorem)
                                        RemoteAdminService.SetUserRegistration((Registration)usercontext, Config);
                                }
                                else
                                {
                                    usercontext.PreferredMethod = RegistrationPreferredMethod.Choose;
                                    usercontext.UIMode = ProviderPageMode.Locking;
                                    result = new AdapterPresentation(this, context, errors_strings.ErrorInvalidIdentificationRestart, true);
                                }
                                break;
                            case 2:
                                if (Config.MailEnabled)
                                {
                                    usercontext.PreferredMethod = RegistrationPreferredMethod.Email;
                                    string aname = proofData.Properties["stmail"].ToString();
#if softemail
                                    // the email is not registered, but registration has been made, so we force to register the email. this is a little security hole, but at this point the user is authenticated. then we log an alert in the EventLog
                                    if (string.IsNullOrEmpty(usercontext.MailAddress))
                                    {
                                        usercontext.MailAddress = aname;
                                        dorem = true;
                                        EventLog.WriteEntry(EventLogSource, string.Format(errors_strings.ErrorRegistrationEmptyEmail, usercontext.UPN, usercontext.MailAddress), EventLogEntryType.Error, 9999);
                                        usercontext.UIMode = ProviderPageMode.Identification;
                                        SendNotification(usercontext);
                                        result = new AdapterPresentation(this, context);
                                        if (dorem)
                                            RemoteAdminService.SetUserRegistration((Registration)usercontext, Config);
                                    }
                                    else
#endif
                                    {
                                        string idom = Utilities.StripEmailDomain(usercontext.MailAddress);
                                        if ((aname.ToLower() + idom.ToLower()).Equals(usercontext.MailAddress.ToLower()))
                                        {
                                            usercontext.UIMode = ProviderPageMode.Identification;
                                            SendNotification(usercontext);
                                            result = new AdapterPresentation(this, context);
                                            if (dorem)
                                                RemoteAdminService.SetUserRegistration((Registration)usercontext, Config);
                                        }
                                        else
                                        {
                                            usercontext.PreferredMethod = RegistrationPreferredMethod.Choose;
                                            usercontext.UIMode = ProviderPageMode.Locking;
                                            result = new AdapterPresentation(this, context, errors_strings.ErrorInvalidIdentificationRestart, true);
                                        }
                                    }
                                }
                                else
                                {
                                    usercontext.PreferredMethod = RegistrationPreferredMethod.Choose;
                                    usercontext.UIMode = ProviderPageMode.Locking;
                                    result = new AdapterPresentation(this, context, errors_strings.ErrorInvalidIdentificationRestart, true);
                                }
                                break;
                            case 3:
                                usercontext.PreferredMethod = RegistrationPreferredMethod.Choose;
                                usercontext.UIMode = ProviderPageMode.Locking;
                                result = new AdapterPresentation(this, context, errors_strings.ErrorInvalidIdentificationRestart, true);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        result = new AdapterPresentation(this, context, ex.Message, true);
                    }
                    break;
                case ProviderPageMode.ShowQRCode:
                    usercontext.SecretKeyChanged = false;
                    usercontext.UIMode = ProviderPageMode.Registration;
                    result = new AdapterPresentation(this, context);
                    break;
            }
            return result;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// SendNotification method implementation
        /// </summary>
        private void SendNotification(AuthenticationContext usercontext)
        {
            if (usercontext.UIMode == ProviderPageMode.Identification)
            {
                Task tsk = new Task(() => internalSendNotification(usercontext));
                tsk.Start();
            }
        }

        /// <summary>
        /// internalSendNotification method implementation
        /// </summary>
        private void internalSendNotification(AuthenticationContext usercontext)
        {
            try
            {
                int otpres = 0;
                switch (usercontext.PreferredMethod)
                {
                    case RegistrationPreferredMethod.Email:
                        otpres = Utilities.GetEmailOTP((Registration)usercontext, Config.SendMail);
                        break;
                    case RegistrationPreferredMethod.ApplicationCode:
                        otpres = -1;
                        break;
                    case RegistrationPreferredMethod.Phone:
                        otpres = Utilities.GetPhoneOTP((Registration)usercontext, Config.ExternalOTPProvider);
                        break;
                }
                RemoteAdminService.SetNotification((Registration)usercontext, Config, otpres);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(EventLogSource, string.Format(errors_strings.ErrorSendingToastInformation, ex.Message+"\r\n"+ex.StackTrace), EventLogEntryType.Error, 800);
                // do not throw the exception, no more information sent 
            }
        }

        /// <summary>
        /// CheckPin method inplementation
        /// </summary>
        private bool CheckPin(string pin, Notification notif, AuthenticationContext usercontext)
        {
            if (notif.OTP == 0)
                return false;
            if (notif.OTP > 0)
            {
                string generatedpin = notif.OTP.ToString("D");  // eg : transmitted by email or by External System (SMS)
                return  (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin));
            }
            else  // Using a TOTP Application (Microsoft Authnetication, Google Authentication, etc...)
            {
                if (Config.TOTPShadows <= 0)
                {
                    DateTime tcall = DateTime.UtcNow;
                    OneTimePasswordGenerator gen = new OneTimePasswordGenerator(usercontext.SecretKey, usercontext.UPN, tcall, Config.Algorithm);  // eg : TOTP code
                    gen.ComputeOneTimePassword(tcall);
                    string generatedpin = gen.OneTimePassword.ToString("D");
                    return (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin));
                }
                else
                {   // Current TOTP
                    DateTime call = DateTime.UtcNow;
                    OneTimePasswordGenerator currentgen = new OneTimePasswordGenerator(usercontext.SecretKey, usercontext.UPN, call, Config.Algorithm);  // eg : TOTP code
                    currentgen.ComputeOneTimePassword(call);
                    string currentpin = currentgen.OneTimePassword.ToString("D");
                    if (Convert.ToInt32(pin) == Convert.ToInt32(currentpin))
                    {
                        return true;
                    }
                    // TOTP with Shadow (current - x latest)
                    for (int i = 1; i <= Config.TOTPShadows; i++ )
                    {
                        DateTime tcall = call.AddSeconds(-(i * OneTimePasswordGenerator.TOTPDuration));
                        OneTimePasswordGenerator gen = new OneTimePasswordGenerator(usercontext.SecretKey, usercontext.UPN, tcall, Config.Algorithm);  // eg : TOTP code
                        gen.ComputeOneTimePassword(tcall);
                        string generatedpin = gen.OneTimePassword.ToString("D");
                        if (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin))
                        {
                            return true;
                        }
                    }
                    // TOTP with Shadow (current + x latest) - not possible. but can be usefull if time sync is not adequate
                    for (int i = 1; i <= Config.TOTPShadows; i++)
                    {
                        DateTime tcall = call.AddSeconds(i * OneTimePasswordGenerator.TOTPDuration);
                        OneTimePasswordGenerator gen = new OneTimePasswordGenerator(usercontext.SecretKey, usercontext.UPN, tcall, Config.Algorithm);  // eg : TOTP code
                        gen.ComputeOneTimePassword(tcall);
                        string generatedpin = gen.OneTimePassword.ToString("D");
                        if (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// GetPin method implmentation (Obsolete)
        /// </summary>
  /*      private string GetPin(Notification notif, Registration userreg)
        {
            if (notif.OTP > 0)  // transmitted by email or by sms
                return notif.OTP.ToString("D"); 
            else // computed 
            {
                OneTimePasswordGenerator gen = new OneTimePasswordGenerator(userreg.SecretKey, userreg.UPN, DateTime.UtcNow, Config.Algorithm);  // eg : TOTP code
                gen.ComputeOneTimePassword(DateTime.UtcNow);
                return gen.OneTimePassword.ToString("D");
            }
        } */

        /// <summary>
        /// ValidateEmail method implementation
        /// </summary>
        private void ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                    throw new Exception(errors_strings.ErrorEmailException);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(EventLogSource, string.Format(errors_strings.ErrorInvalidEmail, ex.Message), EventLogEntryType.Error, 800);
                throw ex;
            }
        }

        /// <summary>
        /// ConvertToBase64 method implmentation
        /// </summary>
        private string ConvertToBase64(Stream stream)
        {
            Byte[] inArray = new Byte[(int)stream.Length];
            stream.Read(inArray, 0, (int)stream.Length);
            return Convert.ToBase64String(inArray);
        }

        /// <summary>
        /// GetQRCodeString method implmentation
        /// </summary>
        public string GetQRCodeString(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            string displaykey = Base32.Encode(usercontext.SecretKey);
            string Content = string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={0}&algorithm={3}", this.Config.Issuer, usercontext.UPN, displaykey, this.Config.Algorithm);
            var encoder = new QrEncoder(ErrorCorrectionLevel.L);
            QrCode qr;
            if (!encoder.TryEncode(Content, out qr))
                return string.Empty;
            BitMatrix matrix = qr.Matrix;
            using (MemoryStream ms = new MemoryStream())
            {
                var render = new GraphicsRenderer(new FixedModuleSize(3, QuietZoneModules.Zero));
                render.WriteToStream(matrix, ImageFormat.Png, ms);
                ms.Position = 0;
                result = ConvertToBase64(ms);
            }
            return result;
        }
        #endregion
    }
}
