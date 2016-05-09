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
using Neos.IdentityServer.MultiFactor.QrEncoding;
using Neos.IdentityServer.MultiFactor.QrEncoding.Windows.Render;
using Neos.IdentityServer.MultiFactor.Resources;
using Microsoft.IdentityServer.Web.Authentication.External;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Serialization;

namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// ProviderPageMode enum
    /// </summary>
    public enum ProviderPageMode
    {
        locking,
        Identification,
        ChooseMethod,
        ChangePassword,
        Registration,
        SelectOptions,
        ShowQRCode,
        Bypass
    }

    /// <summary>
    /// AuthenticationProvider class
    /// </summary>
    public class AuthenticationProvider : IAuthenticationAdapter
    {
        private MFAConfig _config;
        private Registration _registration;
        private ProviderPageMode _uimode = ProviderPageMode.Identification;

        private string EventLogSource = "ADFS Multi-Factor Ex"; 
        private string EventLogGroup = "Application";
        private string _qrstring;

        /// <summary>
        /// Constructor override
        /// </summary>
        public AuthenticationProvider()
        {
            if (!EventLog.SourceExists(this.EventLogSource))
                EventLog.CreateEventSource(this.EventLogSource, this.EventLogGroup);
        }

        #region IAuthenticationAdapter implementation
        /// <summary>
        /// BeginAuthentication method implmentation
		/// </summary>
        public IAdapterPresentation BeginAuthentication(System.Security.Claims.Claim identityClaim, System.Net.HttpListenerRequest request, IAuthenticationContext context)
        {
            try
            { 
                return new AdapterPresentation(this);
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
                UserRegistration = RemoteAdminService.GetUserRegistration(upn, Config);
                if (UserRegistration != null)
                {
                    if (UserRegistration.Enabled)
                    {
                        if (string.IsNullOrEmpty(UserRegistration.DisplayKey))
                        {
                            UserRegistration.SecretKey = RemoteAdminService.GetNewSecretKey(Config);
                            UIM = ProviderPageMode.Registration;
                        }
                        else if (UserRegistration.PreferredMethod == RegistrationPreferredMethod.Choose)
                            UIM = ProviderPageMode.ChooseMethod;
                        else
                            UIM = ProviderPageMode.Identification;
                    }
                    else
                    {
                        UIM = ProviderPageMode.Bypass;
                        return true;
                    }
                    return UserRegistration.Enabled;
                }
                else
                {
                    UserRegistration = new Registration();
                    UserRegistration.UPN = upn;
                    UserRegistration.Enabled = true;
                    UserRegistration.PreferredMethod = RegistrationPreferredMethod.Choose;
                    UIM = ProviderPageMode.Registration;
                    return true;
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(EventLogSource, string.Format(errors_strings.ErrorLoadingUserRegistration, ex.Message), EventLogEntryType.Error, 801);
                throw new ExternalAuthenticationException();
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
            return new AdapterPresentation(this, ex.Message, true);
        }

		/// <summary>
        /// TryEndAuthentication method implementation
		/// </summary>
        public IAdapterPresentation TryEndAuthentication(IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            claims = null;
            IAdapterPresentation result = null;
            AdapterPresentation.SetCultureInfo(context.Lcid);

            ProviderPageMode ui = (ProviderPageMode)Enum.Parse(typeof(ProviderPageMode), proofData.Properties["uimode"].ToString());        
            switch (ui)
            {
                case ProviderPageMode.Identification:
                    SecretKeyAsChanged = false;
                    try
                    {
                        string pin = proofData.Properties["pin"].ToString();
                        object opt = null;
                        bool suitetooptions = proofData.Properties.TryGetValue("options", out opt);
                        int xlnk = Convert.ToInt32(proofData.Properties["lnk"].ToString());
                        if (xlnk==0)
                        {
                            Notification notif = RemoteAdminService.CheckNotification(UserRegistration, Config);
                            if (notif != null)
                            {
                                if (notif.CheckDate.Value > notif.ValidityDate)
                                {
                                    UIM = ProviderPageMode.locking;
                                    return new AdapterPresentation(this, errors_strings.ErrorValidationTimeWindowElapsed, true);
                                }
                                if (CheckPin(pin, notif, UserRegistration))
                              //  string originalPin = GetPin(notif, UserRegistration);
                              //  if (Convert.ToInt32(pin) == Convert.ToInt32(originalPin))
                                {
                                    if (!suitetooptions)
                                    {
                                        System.Security.Claims.Claim claim0 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");
                                        System.Security.Claims.Claim claim1 = null;
                                        switch (UserRegistration.PreferredMethod)
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
                                        TimeSpan duration = notif.CheckDate.Value.Subtract(notif.CreationDate);
                                        System.Security.Claims.Claim claim2 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpduration", duration.ToString());
                                        claims = new System.Security.Claims.Claim[] { claim0, claim1, claim2 };
                                    }
                                    if (suitetooptions)
                                    {
                                        UIM = ProviderPageMode.SelectOptions;
                                        result = new AdapterPresentation(this);
                                    }
                                }
                                else
                                {
                                    UIM = ProviderPageMode.locking;
                                    result = new AdapterPresentation(this, errors_strings.ErrorInvalidIdentificationRestart, true);
                                }
                            }
                            else
                            {
                                UIM = ProviderPageMode.locking;
                                result = new AdapterPresentation(this, errors_strings.ErrorInvalidIdentificationRestart, true);

                            }
                        }
                        else
                        {
                            UIM = ProviderPageMode.ChooseMethod;
                            result = new AdapterPresentation(this);
                        }
                    }
                    catch (Exception ex)
                    {
                        UIM = ProviderPageMode.locking;
                        result = new AdapterPresentation(this, ex.Message, true);
                    }

                    break;
                case ProviderPageMode.Registration:
                    try
                    {
                        string email = proofData.Properties["email"].ToString();
                        string secretkey = null;
                        string phone = null;
                        int xlnk2 = 0;
                        if ((Config.AppsEnabled) && (!string.IsNullOrEmpty(UserRegistration.DisplayKey)))
                        {
                            secretkey = proofData.Properties["secretkey"].ToString();
                            xlnk2 = Convert.ToInt32(proofData.Properties["lnk"].ToString());
                        }
                        if (Config.SMSEnabled)
                            phone = proofData.Properties["phone"].ToString();
                        int  select = Convert.ToInt32(proofData.Properties["selectopt"].ToString());
                        ValidateEmail(email);
                        UserRegistration.MailAddress = email;
                        // UserRegistration.SecretKey = secretkey; // Read-Only
                        if (Config.SMSEnabled)
                            UserRegistration.PhoneNumber = phone;
                        UserRegistration.PreferredMethod = (RegistrationPreferredMethod)select;
                        UIM = ProviderPageMode.SelectOptions;

                        if (xlnk2 == 0)
                        {
                            string btnclicka = proofData.Properties["btnclicked"].ToString();
                            if (btnclicka == "1")
                            {
                                RemoteAdminService.SetUserRegistration(UserRegistration, Config);
                                UIM = ProviderPageMode.SelectOptions;
                                result = new AdapterPresentation(this, infos_strings.InfosConfigurationModified);
                            }
                            else
                                result = new AdapterPresentation(this);
                        }
                        else if (xlnk2 == 1)
                        {
                            UIM = ProviderPageMode.ShowQRCode;
                            QRString = secretkey;
                            QRSource = 1;
                            result = new AdapterPresentation(this);
                        }
                        else if (xlnk2 == 2)
                        {
                            UserRegistration.SecretKey = RemoteAdminService.GetNewSecretKey(Config);
                            RemoteAdminService.SetUserRegistration(UserRegistration, Config);
                            UIM = ProviderPageMode.Registration;
                            SecretKeyAsChanged = true;
                            result = new AdapterPresentation(this, infos_strings.InfosConfigurationModified);
                        }
                    }
                    catch (Exception ex)
                    {
                        UIM = ProviderPageMode.locking;
                        result = new AdapterPresentation(this, ex.Message, true);
                    }
                    break;

                case ProviderPageMode.ChangePassword:
                    SecretKeyAsChanged = false;
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
                                    UIM = ProviderPageMode.SelectOptions;
                                    string btnclick = proofData.Properties["btnclicked"].ToString();
                                    if (btnclick == "1")
                                    {
                                        RemoteAdminService.ChangePassword(UserRegistration.UPN, oldpass, newpass);
                                        result = new AdapterPresentation(this, infos_strings.InfosPasswordModified);
                                    }
                                    else
                                        result = new AdapterPresentation(this);
                                }
                                catch (Exception ex)
                                {
                                    UIM = ProviderPageMode.locking;
                                    result = new AdapterPresentation(this, ex.Message, true);
                                }
                            }
                            else
                            {
                                UIM = ProviderPageMode.SelectOptions;
                                result = new AdapterPresentation(this, errors_strings.ErrorInvalidPassword, true);
                            }
                        }
                        catch (Exception ex)
                        {
                            UIM = ProviderPageMode.locking;
                            result = new AdapterPresentation(this, ex.Message, true);
                        }
                    }
                    else
                    {
                        try
                        {
                        }
                        catch (Exception ex)
                        {
                            UIM = ProviderPageMode.locking;
                            result = new AdapterPresentation(this, ex.Message, true);
                        }
                    }
                    break;
                case ProviderPageMode.Bypass:
                    SecretKeyAsChanged = false;
                    System.Security.Claims.Claim claimx1 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");
                    System.Security.Claims.Claim claimx2 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpmode", "bypass");
                    System.Security.Claims.Claim claimx3 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpduration", "0");
                    claims = new System.Security.Claims.Claim[] { claimx1, claimx2, claimx3 };
                    break;
                case ProviderPageMode.SelectOptions:
                    SecretKeyAsChanged = false;
                    try
                    {
                        int lnk = Convert.ToInt32(proofData.Properties["lnk"].ToString());
                        int xlnk4 = Convert.ToInt32(proofData.Properties["btnclicked"].ToString());
                        if (xlnk4 != 0)
                        {
                            UIM = ProviderPageMode.Bypass;
                            result = new AdapterPresentation(this);
                        }
                        else
                        {
                            switch (lnk)
                            {
                                case 1:
                                    UIM = ProviderPageMode.Registration;
                                    break;
                                case 2:
                                    UIM = ProviderPageMode.ChangePassword;
                                    break;
                            }
                            result = new AdapterPresentation(this);
                        }
                    }
                    catch (Exception ex)
                    {
                        UIM = ProviderPageMode.locking;
                        result = new AdapterPresentation(this, ex.Message, true);
                    }
                    break;
                case ProviderPageMode.ChooseMethod:
                    SecretKeyAsChanged = false;
                    try
                    {
                        UserRegistration.PreferredMethod = RegistrationPreferredMethod.ApplicationCode;
                        int optsel = Convert.ToInt32(proofData.Properties["opt"].ToString());
                        object rem = null;
                        bool dorem = proofData.Properties.TryGetValue("remember", out rem);
                        switch (optsel)
                        {
                            case 0:
                                UserRegistration.PreferredMethod = RegistrationPreferredMethod.ApplicationCode;
                                UIM = ProviderPageMode.Identification;
                                result = new AdapterPresentation(this);
                                if (dorem)
                                    RemoteAdminService.SetUserRegistration(UserRegistration, Config);
                                break;
                            case 1:
                                UserRegistration.PreferredMethod = RegistrationPreferredMethod.Phone;
                                UIM = ProviderPageMode.Identification;
                                result = new AdapterPresentation(this);
                                if (dorem)
                                    RemoteAdminService.SetUserRegistration(UserRegistration, Config);
                                break;
                            case 2:
                                UserRegistration.PreferredMethod = RegistrationPreferredMethod.Email;
                                string aname = proofData.Properties["stmail"].ToString();
                                string idom = Utilities.StripEmailDomain(UserRegistration.MailAddress);
                                if ((aname.ToLower() + idom.ToLower()).Equals(UserRegistration.MailAddress.ToLower()))
                                {
                                    UIM = ProviderPageMode.Identification;
                                    result = new AdapterPresentation(this);
                                    if (dorem)
                                        RemoteAdminService.SetUserRegistration(UserRegistration, Config);
                                }
                                else
                                {
                                    UserRegistration.PreferredMethod = RegistrationPreferredMethod.Choose;
                                    UIM = ProviderPageMode.locking;
                                    result = new AdapterPresentation(this, errors_strings.ErrorInvalidIdentificationRestart, true);
                                }
                                break;
                            case 3:
                                UserRegistration.PreferredMethod = RegistrationPreferredMethod.Choose;
                                UIM = ProviderPageMode.locking;
                                result = new AdapterPresentation(this, errors_strings.ErrorInvalidIdentificationRestart, true);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        UIM = ProviderPageMode.locking;
                        result = new AdapterPresentation(this, ex.Message, true);
                    }
                    break;
                case ProviderPageMode.ShowQRCode:
                    SecretKeyAsChanged = false;
                    if (QRSource==1)
                        UIM = ProviderPageMode.Registration;
                    else
                        UIM = ProviderPageMode.ChooseMethod;
                    result = new AdapterPresentation(this);
                    break;
            }
            return result;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// SendNotification method implementation
        /// </summary>
        private void SendNotification()
        {
            try
            {
                Notification notif = RemoteAdminService.SetNotification(_registration, _config);
                switch (_registration.PreferredMethod)
                {
                    case RegistrationPreferredMethod.Email:
                        Utilities.SendOTPByEmail(_registration.MailAddress, _registration.UPN, notif.OTP.ToString("D"), _config.SendMail);
                        break;
                    case RegistrationPreferredMethod.ApplicationCode:
                        break;
                    case RegistrationPreferredMethod.Phone:
                        break;
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(EventLogSource, string.Format(errors_strings.ErrorSendingToastInformation, ex.Message), EventLogEntryType.Error, 800);
                throw ex;
            }
        }

        /// <summary>
        /// CheckPin method inplementation
        /// </summary>
        private bool CheckPin(string pin, Notification notif, Registration userreg)
        {
            if (notif.OTP > 0)
            {
                string generatedpin = notif.OTP.ToString("D");  // eg : transmitted by email
                return  (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin));
            }
            else
            {
                if (Config.TOTPShadows <= 0)
                {
                    DateTime tcall = DateTime.UtcNow;
                    OneTimePasswordGenerator gen = new OneTimePasswordGenerator(userreg.SecretKey, userreg.UPN, tcall, Config.Algorithm);  // eg : TOTP code
                    gen.ComputeOneTimePassword(tcall);
                    string generatedpin = gen.OneTimePassword.ToString("D");
                    return (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin));
                }
                else
                {   // Current TOTP
                    DateTime call = DateTime.UtcNow;
                    OneTimePasswordGenerator currentgen = new OneTimePasswordGenerator(userreg.SecretKey, userreg.UPN, call, Config.Algorithm);  // eg : TOTP code
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
                        OneTimePasswordGenerator gen = new OneTimePasswordGenerator(userreg.SecretKey, userreg.UPN, tcall, Config.Algorithm);  // eg : TOTP code
                        gen.ComputeOneTimePassword(tcall);
                        string generatedpin = gen.OneTimePassword.ToString("D");
                        if (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin))
                        {
                            return true;
                        }
                    }
                    // TOTP with Shadow (current + x latest)
                    for (int i = 1; i <= Config.TOTPShadows; i++)
                    {
                        DateTime tcall = call.AddSeconds(i * OneTimePasswordGenerator.TOTPDuration);
                        OneTimePasswordGenerator gen = new OneTimePasswordGenerator(userreg.SecretKey, userreg.UPN, tcall, Config.Algorithm);  // eg : TOTP code
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
        private string GetPin(Notification notif, Registration userreg)
        {
            if (notif.OTP > 0)
                return notif.OTP.ToString("D");  // eg : transmitted by email
            else
            {
                OneTimePasswordGenerator gen = new OneTimePasswordGenerator(userreg.SecretKey, userreg.UPN, DateTime.UtcNow, Config.Algorithm);  // eg : TOTP code
                gen.ComputeOneTimePassword(DateTime.UtcNow);
                return gen.OneTimePassword.ToString("D");
            }
        }

        /// <summary>
        /// ValidateEmail method implementation
        /// </summary>
        private void ValidateEmail(string email)
        {
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
        public string GetQRCodeString()
        {
            string result = string.Empty;
            string Content = string.Format("otpauth://totp/{0}:{1}?secret={2}&Issuer={0}&Algorithm={3}", this.Config.Issuer, this.UserRegistration.UPN, this.QRString, this.Config.Algorithm);
            var encoder = new QrEncoder(ErrorCorrectionLevel.H);
            QrCode qr;
            if (!encoder.TryEncode(Content, out qr))
                return string.Empty;
            BitMatrix matrix = qr.Matrix;
            using (MemoryStream ms = new MemoryStream())
            {
                var render = new GraphicsRenderer(new FixedModuleSize(4, QuietZoneModules.Four));
                render.WriteToStream(matrix, ImageFormat.Png, ms);
                ms.Position = 0;
                result = ConvertToBase64(ms);
            }
            return result;
        }
        #endregion

        #region properties
        /// <summary>
        /// Config property
        /// </summary>
        public MFAConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }


        /// <summary>
        /// UserRegistration property
        /// </summary>
        public Registration UserRegistration
        {
            get { return _registration; }
            set { _registration = value; }
        }

        /// <summary>
        /// UIM property
        /// </summary>
        public ProviderPageMode UIM
        {
            get { return _uimode; }
            set 
            { 
                _uimode = value;
                if (_uimode == ProviderPageMode.Identification)
                    SendNotification();
            }
        }

        /// <summary>
        /// QRString property implementation
        /// </summary>
        internal string QRString
        {
            get { return _qrstring; }
            set { _qrstring = value;}
        }

        /// <summary>
        /// QRSource property implementation
        /// </summary>
        public int QRSource 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// SecretKeyAsChanged property implementation
        /// </summary>
        public bool SecretKeyAsChanged
        {
            get;
            set;
        }
        #endregion
    }
}
