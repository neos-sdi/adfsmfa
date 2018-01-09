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
#define softemail
using Microsoft.IdentityServer.Web.Authentication.External;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// AuthenticationProvider class
    /// </summary>
    public class AuthenticationProvider : IAuthenticationAdapter
    {
        private static MFAConfig _config;
        private static object __notificationobject = 0;
        private static RegistryVersion _registry = new RegistryVersion();

        /// <summary>
        /// Constructor override
        /// </summary>
        public AuthenticationProvider()
        {
            Trace.AutoFlush = true;
            Trace.TraceInformation("AuthenticationProvider:Contructor");
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
            DateTime st = DateTime.Now;
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);

            IAdapterPresentation result = null;
            try
            {
                AuthenticationContext usercontext = new AuthenticationContext(context);
                switch (usercontext.UIMode)
                {
                    case ProviderPageMode.Identification:
                        switch (usercontext.PreferredMethod)
                        {
                            case PreferredMethod.Code:
                                usercontext.UIMode = ProviderPageMode.Identification;
                                RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.Totp);
                                break;
                            case PreferredMethod.Email:
                                usercontext.UIMode = ProviderPageMode.SendCodeRequest;
                                RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.RequestEmail);
                                break;
                            case PreferredMethod.Phone:
                                usercontext.UIMode = ProviderPageMode.SendCodeRequest;
                                RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.RequestExternal);
                                break;
                        }
                        result = new AdapterPresentation(this, context);
                        break;
                    case ProviderPageMode.Locking:  // Only for locking mode
                        if (usercontext.TargetUIMode == ProviderPageMode.DefinitiveError)
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNoAccess"), ProviderPageMode.DefinitiveError);
                        else if (Config.UserFeatures.IsMFARequired())
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNoAccess"), ProviderPageMode.DefinitiveError);
                        else if (Config.UserFeatures.IsMFAAllowed())  
                        {
                            if (Config.UserFeatures.IsAdvertisable() && (Config.AdvertisingDays.OnFire))
                            {
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.Bypass;
                                result = new AdapterPresentation(this, context);
                            }
                        }
                        else if (Config.UserFeatures.IsRegistrationAllowed()) 
                        {
                            if (Config.UserFeatures.IsAdvertisable() && (Config.AdvertisingDays.OnFire))
                            {
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.Bypass;
                                result = new AdapterPresentation(this, context);
                            }
                        }
                        else
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNotEnabled"), ProviderPageMode.DefinitiveError);
                        break;
                    default:
                        if ((HookOptionParameter(request)) && (Config.UserFeatures.CanManageOptions() || Config.UserFeatures.CanManagePassword()))
                            usercontext.UIMode = ProviderPageMode.SelectOptions;
                        result = new AdapterPresentation(this, context);
                        break;
                }
                Trace.TraceInformation(String.Format("AuthenticationProvider:BeginAuthentication Duration : {0}", (DateTime.Now - st).ToString()));
                return result;
            }
            catch (Exception ex)
            {
                Trace.TraceError(String.Format("AuthenticationProvider:BeginAuthentication Duration : {0}", (DateTime.Now - st).ToString()));
                Log. WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAuthenticating"), ex.Message), EventLogEntryType.Error, 802);
                throw new ExternalAuthenticationException(ex.Message, context);
            }
        }
		/// <summary>
        /// IsAvailableForUser method implementation
		/// </summary>
        public bool IsAvailableForUser(System.Security.Claims.Claim identityClaim, IAuthenticationContext context)
        {
            DateTime st = DateTime.Now;
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            try
            {
                string upn = identityClaim.Value;
                Registration reg = RuntimeRepository.GetUserRegistration(Config, upn);
                if (reg != null) // User Is Registered
                {
                    AuthenticationContext usercontext = new AuthenticationContext(context, reg);
                    if (usercontext.Enabled)
                    {
                        if (usercontext.KeyStatus == SecretKeyStatus.NoKey)
                        {
                            if (Config.UserFeatures.CanManageOptions())
                            {
                                KeysManager.NewKey(usercontext.UPN);
                                usercontext.KeyStatus = SecretKeyStatus.Success;
                                usercontext.KeyChanged = true;
                                usercontext.UIMode = ProviderPageMode.Registration;
                            }
                        }
                        else if (usercontext.PreferredMethod == PreferredMethod.Choose)
                            usercontext.UIMode = ProviderPageMode.ChooseMethod;
                        else
                            usercontext.UIMode = ProviderPageMode.Identification;
                        Trace.TraceInformation(String.Format("AuthenticationProvider:IsAvailableForUser (Standard) Duration : {0}", (DateTime.Now - st).ToString()));
                        return true;
                    }
                    else // Not enabled
                    {
                        if (Config.UserFeatures.IsMFARequired())
                            usercontext.UIMode = ProviderPageMode.Locking;
                        else if (Config.UserFeatures.IsMFANotRequired())
                        {
                            if (usercontext.KeyStatus == SecretKeyStatus.NoKey)
                            {
                                KeysManager.NewKey(usercontext.UPN);
                                usercontext.KeyStatus = SecretKeyStatus.Success;
                                usercontext.KeyChanged = true;
                                usercontext.PreferredMethod = PreferredMethod.Choose;
                            }
                            usercontext.UIMode = ProviderPageMode.Bypass;
                        }
                        else if (Config.UserFeatures.IsMFAAllowed())
                        {
                            if (usercontext.KeyStatus == SecretKeyStatus.NoKey)
                            {
                                KeysManager.NewKey(usercontext.UPN);
                                usercontext.KeyStatus = SecretKeyStatus.Success;
                                usercontext.KeyChanged = true;
                                usercontext.PreferredMethod = PreferredMethod.Choose;
                            }
                            usercontext.UIMode = ProviderPageMode.Locking;
                            usercontext.TargetUIMode = ProviderPageMode.None;
                        }
                        else
                        {
                            usercontext.TargetUIMode = ProviderPageMode.DefinitiveError;
                            usercontext.UIMode = ProviderPageMode.Locking;
                        }
                        Trace.TraceInformation(String.Format("AuthenticationProvider:IsAvailableForUser (Not Enabled) Duration : {0}", (DateTime.Now - st).ToString()));
                        return true;
                    }
                }
                else //Not registered
                {
                    AuthenticationContext usercontext = new AuthenticationContext(context);
                    usercontext.UPN = upn;

                    if (Config.UserFeatures.IsAdministrative())
                        usercontext.UIMode = ProviderPageMode.Locking;
                    else if (Config.UserFeatures.IsRegistrationNotRequired())
                    {
                        KeysManager.NewKey(usercontext.UPN);
                        usercontext.KeyStatus = SecretKeyStatus.Success;
                        usercontext.KeyChanged = true;
                        usercontext.PreferredMethod = PreferredMethod.Choose;
                        usercontext.Enabled = false;
                        usercontext.UIMode = ProviderPageMode.Bypass;
                    }
                    else if (Config.UserFeatures.IsRegistrationRequired())
                    {
                        KeysManager.NewKey(usercontext.UPN);
                        usercontext.KeyStatus = SecretKeyStatus.Success;
                        usercontext.KeyChanged = true;
                        usercontext.PreferredMethod = PreferredMethod.Choose;
                        usercontext.Enabled = false;
                        usercontext.UIMode = ProviderPageMode.Locking;
                    }
                    else if (Config.UserFeatures.IsRegistrationAllowed())
                    {
                        KeysManager.NewKey(usercontext.UPN);
                        usercontext.KeyStatus = SecretKeyStatus.Success;
                        usercontext.KeyChanged = true;
                        usercontext.PreferredMethod = PreferredMethod.Choose;
                        usercontext.Enabled = true;
                        usercontext.UIMode = ProviderPageMode.Locking;
                        usercontext.TargetUIMode = ProviderPageMode.None;
                    }
                    else
                    {
                        usercontext.TargetUIMode = ProviderPageMode.DefinitiveError;
                        usercontext.UIMode = ProviderPageMode.Locking;
                    }
                    Trace.TraceInformation(String.Format("AuthenticationProvider:IsAvailableForUser (Not Registered) Duration : {0}", (DateTime.Now - st).ToString()));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(String.Format("AuthenticationProvider:IsAvailableForUser Error : {0}", ex.Message));
                Log.WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorLoadingUserRegistration"), ex.Message), EventLogEntryType.Error, 801);
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
             DateTime st = DateTime.Now;
             ResourcesLocale Resources = new ResourcesLocale(CultureInfo.CurrentUICulture.LCID);
             if (configData.Data != null)
             {
                 if (_config == null)
                 {
                     try
                     {
                         Stream stm = configData.Data;
                         XmlConfigSerializer xmlserializer = new XmlConfigSerializer(typeof(MFAConfig));
                         using (StreamReader reader = new StreamReader(stm))
                         {
                             _config = (MFAConfig)xmlserializer.Deserialize(stm);
                             if ((!_config.AppsEnabled) && (!_config.MailEnabled) && (!_config.SMSEnabled))
                                 _config.MailEnabled = true;   // always let an active option eg : email in this case
                             KeysManager.Initialize(_config);  // Always Bind KeysManager Otherwise this is made in CFGUtilities.ReadConfiguration
                         }

                         RuntimeRepository.MailslotServer.MailSlotMessageArrived += this.OnMessageArrived;
                         RuntimeRepository.MailslotServer.AllowedMachines.Clear();
                         foreach(ADFSServerHost svr in Config.Hosts.ADFSFarm.Servers)
                         {
                             RuntimeRepository.MailslotServer.AllowedMachines.Add(svr.MachineName);
                         }
                         RuntimeRepository.MailslotServer.Start();
                         Trace.TraceInformation(String.Format("AuthenticationProvider:OnAuthenticationPipelineLoad Duration : {0}", (DateTime.Now - st).ToString()));
                     }
                     catch (Exception ex)
                     {
                         Trace.TraceError(String.Format("AuthenticationProvider:OnAuthenticationPipelineLoad Error : {0}", ex.Message));
                         Log.WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorLoadingConfigurationFile"), ex.Message), EventLogEntryType.Error, 900);
                         throw new ExternalAuthenticationException();
                     }
                 }
             }
             else
             {
                 Trace.TraceError(String.Format("AuthenticationProvider:OnAuthenticationPipelineLoad Error : 900"));
                 Log. WriteEntry(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorLoadingConfigurationFileNotFound"), EventLogEntryType.Error, 900);
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
            return null; // Do not present an adapter let default adfs error handling
        }

		/// <summary>
        /// TryEndAuthentication method implementation
		/// </summary>
        public IAdapterPresentation TryEndAuthentication(IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            claims = null;
            IAdapterPresentation result = null;
            AuthenticationContext usercontext = new AuthenticationContext(context);

            ProviderPageMode ui = usercontext.UIMode;        
            switch (ui)
            {
                case ProviderPageMode.Identification:
                    result = TryIdentification(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.Registration: // User self registration and enable
                    result = TryRegistration(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.Invitation: // admministrative user registration and let disabled
                    result = TryInvitation(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.SelectOptions:
                    result = TrySelectOptions(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.ChooseMethod:
                    result = TryChooseMethod(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.ChangePassword:
                    result = TryChangePassword(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.Bypass:
                    result = TryBypass(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.Locking:
                    result = TryLocking(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.ShowQRCode:
                    result = TryShowQRCode(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.SendCodeRequest:
                    result = TrySendCodeRequest(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.SendAdministrativeRequest:
                    result = TrySendAdministrativeRequest(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.SendKeyRequest:
                    result = TrySendKeyRequest(usercontext, context, proofData, request, out claims);
                    break;
            }
            return result;
        }

        /// <summary>
        /// TryIdentification method implementation
        /// </summary>
        private IAdapterPresentation TryIdentification(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Identification
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            claims = null;
            IAdapterPresentation result = null;
            usercontext.KeyChanged = false;
            try
            {
                string pin = proofData.Properties["pin"].ToString();
                object opt = null;
                bool options = proofData.Properties.TryGetValue("options", out opt);
                int lnk = Convert.ToInt32(proofData.Properties["lnk"].ToString());
                if (lnk == 0)
                {
                    Notification notif = RuntimeRepository.CheckNotification(Config, (Registration)usercontext);
                    if (notif != null)
                    {
                        if (notif.CheckDate.Value.ToUniversalTime() > notif.ValidityDate.ToUniversalTime())  // Always check with Universal Time
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorValidationTimeWindowElapsed"), ProviderPageMode.DefinitiveError);
                        }
                        try
                        {
                            if (CheckPin(pin, notif, usercontext))
                            {
                                if (!options)
                                    claims = new Claim[] { GetAuthMethodClaim(usercontext.Notification) };
                                if (options)
                                {
                                    usercontext.UIMode = ProviderPageMode.SelectOptions;
                                    return new AdapterPresentation(this, context);
                                }
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.Locking;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                            }
                        }
                        catch (CryptographicException cex)
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            Log.WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAuthenticating"), cex.Message), EventLogEntryType.Error, 10000);
                            return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                        }
                        catch (Exception)
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                        }
                    }
                    else
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                    }
                }
                else
                {
                    usercontext.UIMode = ProviderPageMode.ChooseMethod;
                    return new AdapterPresentation(this, context);
                }
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;
            #endregion

        }

        /// <summary>
        /// GetAuthMethodClaim method implementation
        /// </summary>
        private Claim GetAuthMethodClaim(NotificationStatus notificationStatus)
        {
            if (_registry.IsWindows2016)
                        {
                switch (notificationStatus)
                {
                    case NotificationStatus.Totp:
                        return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");
                    case NotificationStatus.ResponseEmail:
                        return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/email");
                    case NotificationStatus.ResponseSMS:
                        return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/sms");
                    case NotificationStatus.ResponseSMSOTP:
                        return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/smsotp");
                    case NotificationStatus.ResponseSMSReply:
                        return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/smsreply");
                    case NotificationStatus.ResponseVoiceBiometric:
                        return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/voicebiometrics");
                    case NotificationStatus.ResponsePhoneApplication:
                        return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/phoneapplication");
                    case NotificationStatus.ResponsePhoneConfirmation:
                        return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/phoneconfirmation");
                    case NotificationStatus.ResponseKba:
                        return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/kba");
                    case NotificationStatus.ResponseFaceID:
                        return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/facebiometrics");
                    case NotificationStatus.ResponseWindowsHello:
                        return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/windowshello");
                    case NotificationStatus.ResponseFIDO:
                        return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/fido");
                    default:
                        return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/none");
                }
            }
            else
                return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");
        }

        /// <summary>
        /// TryRegistration method implementation
        /// </summary>
        private IAdapterPresentation TryRegistration(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Registration
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            claims = null;
            IAdapterPresentation result = null;
            usercontext.KeyChanged = false;
            try
            {
                string email = null;
                string phone = null;
                int lnk = 0;
                if (proofData.Properties.ContainsKey("lnk"))
                {
                    lnk = Convert.ToInt32(proofData.Properties["lnk"].ToString());
                    if (lnk == 1)
                    {
                        if (Config.MailEnabled)
                        {
                            email = proofData.Properties["email"].ToString();
                            try
                            {
                                ValidateEmail(email, Resources, false);
                            }
                            catch (Exception ex)
                            {
                                return new AdapterPresentation(this, context, ex.Message, ProviderPageMode.DefinitiveError);
                            }
                            usercontext.MailAddress = email;
                        }
                        if (Config.SMSEnabled)
                        {
                            phone = proofData.Properties["phone"].ToString();
                            try
                            {
                                ValidatePhoneNumber(phone, Resources, false);
                            }
                            catch (Exception ex)
                            {
                                return new AdapterPresentation(this, context, ex.Message, ProviderPageMode.DefinitiveError);
                            }
                            usercontext.PhoneNumber = phone;
                        }

                        object opt = null;
                        bool options = proofData.Properties.TryGetValue("disablemfa", out opt);
                        usercontext.Enabled = !options;

                        Registration reg = RuntimeRepository.SetUserRegistration(Config, (Registration)usercontext);
                        usercontext.Assign(reg);
                        usercontext.KeyChanged = false;

                        usercontext.UIMode = ProviderPageMode.ShowQRCode;
                        result = new AdapterPresentation(this, context);
                    }
                    else if (lnk == 2)
                    {
                        RuntimeRepository.SetUserRegistration(Config, (Registration)usercontext, true);
                        usercontext.KeyStatus = SecretKeyStatus.Success;
                        usercontext.KeyChanged = true;
                        usercontext.UIMode = ProviderPageMode.Registration;
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Informations, "InfosConfigurationModified"));
                    }
                    else if (lnk==3) // Send Email
                    {
                        if (Config.MailEnabled)
                        {
                            email = proofData.Properties["email"].ToString();
                            try
                            {
                                ValidateEmail(email, Resources, true);
                            }
                            catch (Exception ex)
                            {
                                return new AdapterPresentation(this, context, ex.Message, ProviderPageMode.DefinitiveError);
                            }
                            usercontext.MailAddress = email;
                        }
                        if (Config.SMSEnabled)
                        {
                            phone = proofData.Properties["phone"].ToString();
                            try
                            {
                                ValidatePhoneNumber(phone, Resources, true);
                            }
                            catch (Exception ex)
                            {
                                return new AdapterPresentation(this, context, ex.Message, ProviderPageMode.DefinitiveError);
                            }
                            usercontext.PhoneNumber = phone;
                        }

                        object opt = null;
                        bool options = proofData.Properties.TryGetValue("disablemfa", out opt);
                        usercontext.Enabled = !options;

                        Registration reg = RuntimeRepository.SetUserRegistration(Config, (Registration)usercontext);
                        usercontext.Assign(reg);
                        usercontext.KeyChanged = false;

                        usercontext.UIMode = ProviderPageMode.SendKeyRequest;
                        result = new AdapterPresentation(this, context, ProviderPageMode.Registration);
                    }
                }
                if (lnk == 0)
                {
                    int select = Convert.ToInt32(proofData.Properties["selectopt"].ToString());
                    usercontext.PreferredMethod = (PreferredMethod)select;
                    usercontext.UIMode = ProviderPageMode.SelectOptions;
                    int btnclicked = Convert.ToInt32(proofData.Properties["btnclicked"].ToString());
                    if (btnclicked == 1)  // OK
                    {
                        if (Config.MailEnabled)
                        {
                            email = proofData.Properties["email"].ToString();
                            try
                            {
                                ValidateEmail(email, Resources, true);
                            }
                            catch (Exception ex)
                            {
                                return new AdapterPresentation(this, context, ex.Message, ProviderPageMode.DefinitiveError);
                            }
                            usercontext.MailAddress = email;
                        }
                        if (Config.SMSEnabled)
                        {
                            phone = proofData.Properties["phone"].ToString();
                            try
                            {
                                ValidatePhoneNumber(phone, Resources, true);
                            }
                            catch (Exception ex)
                            {
                                return new AdapterPresentation(this, context, ex.Message, ProviderPageMode.DefinitiveError);
                            }
                            usercontext.PhoneNumber = phone;
                        }

                        object opt = null;
                        bool options = proofData.Properties.TryGetValue("disablemfa", out opt);
                        usercontext.Enabled = !options;

                        RuntimeRepository.SetUserRegistration(Config, (Registration)usercontext);
                        if (usercontext.TargetUIMode == ProviderPageMode.Bypass)
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass);
                        }
                        else
                        {
                            usercontext.UIMode = ProviderPageMode.SelectOptions;
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Informations, "InfosConfigurationModified"));
                        }
                    }
                    else if (btnclicked == 2) //Cancel
                    {
                        
                        if (usercontext.TargetUIMode == ProviderPageMode.Bypass)
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass);
                        }
                        else 
                        {
                            usercontext.UIMode = ProviderPageMode.SelectOptions;
                            result = new AdapterPresentation(this, context);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;
            #endregion
        }

        /// <summary>
        /// TryInvitation method implementation
        /// </summary>
        private IAdapterPresentation TryInvitation(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Invitation
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            claims = null;
            IAdapterPresentation result = null;

           // usercontext.KeyChanged = true;
            try
            {
                string email = null;
                string phone = null;

                int lnk = 0;
                if (proofData.Properties.ContainsKey("lnk"))
                {
                    lnk = Convert.ToInt32(proofData.Properties["lnk"].ToString());
                    if (lnk == 1)  // Show QR Code
                    {
                        if (Config.MailEnabled)
                        {
                            email = proofData.Properties["email"].ToString();
                            try
                            {
                                ValidateEmail(email, Resources, false);
                            }
                            catch (Exception ex)
                            {
                                return new AdapterPresentation(this, context, ex.Message, ProviderPageMode.DefinitiveError);
                            }
                            usercontext.MailAddress = email;
                        }
                        if (Config.SMSEnabled)
                        {
                            phone = proofData.Properties["phone"].ToString();
                            try
                            {
                                ValidatePhoneNumber(phone, Resources, false);
                            }
                            catch (Exception ex)
                            {
                                return new AdapterPresentation(this, context, ex.Message, ProviderPageMode.DefinitiveError);
                            }
                            usercontext.PhoneNumber = phone;
                        }

                        usercontext.Enabled = false;
                        Registration reg = RuntimeRepository.SetUserRegistration(Config, (Registration)usercontext);
                        usercontext.Assign(reg);
                        usercontext.KeyChanged = false;

                        usercontext.UIMode = ProviderPageMode.ShowQRCode;
                        result = new AdapterPresentation(this, context);
                    }
                    else if (lnk == 3)  // Send email
                    {
                        if (Config.MailEnabled)
                        {
                            email = proofData.Properties["email"].ToString();
                            try
                            {
                                ValidateEmail(email, Resources, true);
                            }
                            catch (Exception ex)
                            {
                                return new AdapterPresentation(this, context, ex.Message, ProviderPageMode.DefinitiveError);
                            }
                            usercontext.MailAddress = email;
                        }
                        if (Config.SMSEnabled)
                        {
                            phone = proofData.Properties["phone"].ToString();
                            try
                            {
                                ValidatePhoneNumber(phone, Resources, true);
                            }
                            catch (Exception ex)
                            {
                                return new AdapterPresentation(this, context, ex.Message, ProviderPageMode.DefinitiveError);
                            }
                            usercontext.PhoneNumber = phone;
                        }

                        usercontext.Enabled = false;
                        Registration reg = RuntimeRepository.SetUserRegistration(Config, (Registration)usercontext);
                        usercontext.Assign(reg);
                        usercontext.KeyChanged = false;

                        usercontext.UIMode = ProviderPageMode.SendKeyRequest;
                        result = new AdapterPresentation(this, context, ProviderPageMode.Invitation);
                    }
                }
                if (lnk == 0)
                {
                    int btnclicked = Convert.ToInt32(proofData.Properties["btnclicked"].ToString());
                    int select = Convert.ToInt32(proofData.Properties["selectopt"].ToString());
                    usercontext.PreferredMethod = (PreferredMethod)select;
                   
                    if (btnclicked == 1)
                    {
                        if (Config.MailEnabled)
                        {
                            email = proofData.Properties["email"].ToString();
                            try
                            {
                                ValidateEmail(email, Resources, true);
                            }
                            catch (Exception ex)
                            {
                                return new AdapterPresentation(this, context, ex.Message, ProviderPageMode.DefinitiveError);
                            }
                            usercontext.MailAddress = email;
                        }
                        if (Config.SMSEnabled)
                        {
                            phone = proofData.Properties["phone"].ToString();
                            try
                            {
                                ValidatePhoneNumber(phone, Resources, true);
                            }
                            catch (Exception ex)
                            {
                                return new AdapterPresentation(this, context, ex.Message, ProviderPageMode.DefinitiveError);
                            }
                            usercontext.PhoneNumber = phone;
                        }

                        usercontext.Enabled = false;
                        Registration reg = RuntimeRepository.SetUserRegistration(Config, (Registration)usercontext);
                        usercontext.Assign(reg);
                        usercontext.KeyChanged = false;

                        if (Config.UserFeatures.IsRegistrationRequired())
                        {
                            usercontext.UIMode = ProviderPageMode.SendAdministrativeRequest;
                            result = new AdapterPresentation(this, context);
                        }
                        else
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            if ( Config.UserFeatures.IsMFAAllowed() || Config.UserFeatures.IsRegistrationAllowed())
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass);
                            else
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNotEnabled"), ProviderPageMode.DefinitiveError);
                        }
                    }
                    else if (btnclicked == 2) //Cancel
                    {
                        if (usercontext.TargetUIMode == ProviderPageMode.Bypass)
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass);
                            
                        }
                        else 
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNoAccess"), ProviderPageMode.DefinitiveError);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;
            #endregion
        }

        /// <summary>
        /// TrySelectOptions method implementation
        /// </summary>
        private IAdapterPresentation TrySelectOptions(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Select Options
            claims = null;
            IAdapterPresentation result = null;
            try
            {
                usercontext.KeyChanged = false;
                if ((Config.UserFeatures.CanManageOptions()) || (Config.UserFeatures.CanManagePassword()))
                {
                    int lnk = Convert.ToInt32(proofData.Properties["lnk"].ToString());
                    int btnclicked = Convert.ToInt32(proofData.Properties["btnclicked"].ToString());
                    if (btnclicked != 0)
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
                else
                {
                    usercontext.UIMode = ProviderPageMode.Bypass;
                    result = new AdapterPresentation(this, context);
                }
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;
            #endregion
        }

        /// <summary>
        /// TryChooseMethod procedure implementation
        /// </summary>
        private IAdapterPresentation TryChooseMethod(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Choose method
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            claims = null;
            IAdapterPresentation result = null;
            usercontext.KeyChanged = false;
            try
            {
                usercontext.PreferredMethod = PreferredMethod.Code;
                int opt = Convert.ToInt32(proofData.Properties["opt"].ToString());
                object rem = null;
                bool remember = proofData.Properties.TryGetValue("remember", out rem);
                switch (opt)
                {
                    case 0:
                        if (Config.AppsEnabled)
                        {
                            usercontext.PreferredMethod = PreferredMethod.Code;
                            usercontext.UIMode = ProviderPageMode.Identification;
                            RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.Totp);
                            result = new AdapterPresentation(this, context);
                            if (remember)
                                RuntimeRepository.SetUserRegistration(Config, (Registration)usercontext);
                        }
                        else
                        {
                            usercontext.PreferredMethod = PreferredMethod.Choose;
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                        }
                        break;
                    case 1:
                        if (Config.SMSEnabled)
                        {
                            usercontext.PreferredMethod = PreferredMethod.Phone;
                            usercontext.UIMode = ProviderPageMode.SendCodeRequest;
                            RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.RequestExternal);
                            result = new AdapterPresentation(this, context);
                            if (remember)
                                RuntimeRepository.SetUserRegistration(Config, (Registration)usercontext);
                        }
                        else
                        {
                            usercontext.PreferredMethod = PreferredMethod.Choose;
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                        }
                        break;
                    case 2:
                        if (Config.MailEnabled)
                        {
                            usercontext.PreferredMethod = PreferredMethod.Email;
                            string stmail = proofData.Properties["stmail"].ToString();
#if softemail
                            // the email is not registered, but registration has been made, so we force to register the email. this is a little security hole, but at this point the user is authenticated. then we log an alert in the EventLog
                            if (string.IsNullOrEmpty(usercontext.MailAddress))
                            {
                                usercontext.MailAddress = stmail;
                                remember = true;
                                Log. WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorRegistrationEmptyEmail"), usercontext.UPN, usercontext.MailAddress), EventLogEntryType.Error, 9999);
                                usercontext.UIMode = ProviderPageMode.SendCodeRequest;
                                RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.RequestEmail);
                                result = new AdapterPresentation(this, context);
                                if (remember)
                                    RuntimeRepository.SetUserRegistration(Config, (Registration)usercontext);
                            }
                            else
#endif
                            {
                                string idom = MailUtilities.StripEmailDomain(usercontext.MailAddress);
                                if ((stmail.ToLower() + idom.ToLower()).Equals(usercontext.MailAddress.ToLower()))
                                {
                                    usercontext.UIMode = ProviderPageMode.SendCodeRequest;
                                    RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.RequestEmail);
                                    result = new AdapterPresentation(this, context);
                                    if (remember)
                                        RuntimeRepository.SetUserRegistration(Config, (Registration)usercontext);
                                }
                                else
                                {
                                    usercontext.PreferredMethod = PreferredMethod.Choose;
                                    usercontext.UIMode = ProviderPageMode.Locking;
                                    result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                                }
                            }
                        }
                        else
                        {
                            usercontext.PreferredMethod = PreferredMethod.Choose;
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                        }
                        break;
                    case 3:
                        usercontext.PreferredMethod = PreferredMethod.Choose;
                        usercontext.UIMode = ProviderPageMode.Locking;
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;
            #endregion
        }

        /// <summary>
        /// TryChangePassword method implementation
        /// </summary>
        private IAdapterPresentation TryChangePassword(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Change Password
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            claims = null;
            IAdapterPresentation result = null;
            usercontext.KeyChanged = false;
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
                                RuntimeRepository.ChangePassword(usercontext.UPN, oldpass, newpass);
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Informations, "InfosPasswordModified"));
                            }
                            else
                                result = new AdapterPresentation(this, context);
                        }
                        catch (Exception ex)
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, ex.Message, ProviderPageMode.DefinitiveError);
                        }
                    }
                    else
                    {
                        usercontext.UIMode = ProviderPageMode.ChangePassword;
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidPassword"), ProviderPageMode.DefinitiveError);
                    }
                }
                catch (Exception ex)
                {
                    throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
                }
            }
            else
            {
                try
                {
                }
                catch (Exception ex)
                {
                    throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
                }
            }
            return result;
            #endregion
        }

        /// <summary>
        /// TryBypass method implementation
        /// </summary>
        private IAdapterPresentation TryBypass(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Bypass Mode
            claims = null;
            try
            {
                usercontext.KeyChanged = false;
                claims = new Claim[] { GetAuthMethodClaim(usercontext.Notification) };
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return null;
            #endregion
        }

        /// <summary>
        /// TryShowQRCode method implementation
        /// </summary>
        private IAdapterPresentation TryShowQRCode(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Show QR Code
            claims = null;
            IAdapterPresentation result = null;
            try
            {
                usercontext.KeyChanged = false;
                if (usercontext.Enabled)
                {
                    usercontext.UIMode = ProviderPageMode.Registration;
                }
                else
                {
                    usercontext.UIMode = ProviderPageMode.Invitation;
                }
                result = new AdapterPresentation(this, context);
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;
            #endregion
        }

        /// <summary>
        /// TryLocking method implementation
        /// </summary>
        private IAdapterPresentation TryLocking(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Locking
            claims = null;
            IAdapterPresentation result = null;
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            try
            {
                int btnclicked = Convert.ToInt32(proofData.Properties["btnclicked"].ToString());
                ProviderPageMode lnk = usercontext.TargetUIMode;
                if (btnclicked == 1)
                {
                    switch (lnk)
                    {
                        case ProviderPageMode.DefinitiveError:
                            string msg = usercontext.UIMessage;
                            throw new Exception(msg);
                        default:
                            usercontext.UIMode = usercontext.TargetUIMode;
                            result = new AdapterPresentation(this, context, usercontext.UIMessage);
                            break;
                    }
                }
                else if (btnclicked == 2) // Invitation
                {
                    if (Config.UserFeatures.IsRegistrationRequired())
                    {
                        usercontext.UIMode = ProviderPageMode.Invitation;
                        result = new AdapterPresentation(this, context);
                    }
                    else if (usercontext.TargetUIMode == ProviderPageMode.Bypass)
                    {
                        if (Config.UserFeatures.IsAdministrative())
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAdminAuthorized"), ProviderPageMode.DefinitiveError);
                        else
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass);
                    }
                    else if (usercontext.TargetUIMode == ProviderPageMode.DefinitiveError)
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNotEnabled"), ProviderPageMode.DefinitiveError);
                }
                else if (btnclicked == 3)  // Registration
                {
                    if (Config.UserFeatures.IsRegistrationAllowed())
                    {
                        usercontext.UIMode = ProviderPageMode.Registration;
                        result = new AdapterPresentation(this, context);
                    }
                    else if (usercontext.TargetUIMode == ProviderPageMode.Bypass)
                    {
                        if (Config.UserFeatures.IsAdministrative())
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAdminAuthorized"), ProviderPageMode.DefinitiveError);
                        else
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass);
                    }
                    else if (usercontext.TargetUIMode == ProviderPageMode.DefinitiveError)
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNotEnabled"), ProviderPageMode.DefinitiveError);
                }
                else if (btnclicked == 4)  // Enable / Disable Allowed
                {
                    if (Config.UserFeatures.IsMFAAllowed() && usercontext.IsRegistered)
                    {
                        usercontext.UIMode = ProviderPageMode.Registration;
                        result = new AdapterPresentation(this, context);
                    }
                    else if (usercontext.TargetUIMode == ProviderPageMode.Bypass)
                    {
                        if (Config.UserFeatures.IsAdministrative())
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAdminAuthorized"), ProviderPageMode.DefinitiveError);
                        else
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass);
                    }
                    else if (usercontext.TargetUIMode == ProviderPageMode.DefinitiveError)
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNotEnabled"), ProviderPageMode.DefinitiveError);
                }

            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;
            #endregion
        }

        /// <summary>
        /// TrySendCodeRequest method implementation
        /// </summary>
        private IAdapterPresentation TrySendCodeRequest(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Requesting Code
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            claims = null;
            IAdapterPresentation result = null;
            try
            {
                int lnk = Convert.ToInt32(proofData.Properties["lnk"].ToString());
                if (lnk == 3)
                {
                    usercontext.UIMode = ProviderPageMode.ChooseMethod;
                    return new AdapterPresentation(this, context);
                }

                usercontext.Notification = SendNotification(usercontext, Resources);
                if (usercontext.Notification==NotificationStatus.Error)
                {
                    usercontext.UIMode = ProviderPageMode.Locking;
                    result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                }
                Notification chknotif = RuntimeRepository.CheckNotification(Config, (Registration)usercontext);
                if (chknotif != null)
                {
                    if (chknotif.OTP == (int)NotificationStatus.Error)
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.Identification);
                    }
                    else if (chknotif.OTP <= (int)NotificationStatus.Bypass)
                    {
                        CheckRequestCookie(usercontext, request);
                        if (usercontext.ShowOptions) 
                            usercontext.UIMode = ProviderPageMode.SelectOptions;  // Manage options, Access granted, Notification response OK 
                        else
                            usercontext.UIMode = ProviderPageMode.Bypass; // Grant Access, Notification response OK 
                    }
                    else if (chknotif.OTP > (int)NotificationStatus.Error)
                    {
                        CheckRequestCookie(usercontext, request);
                        usercontext.UIMode = ProviderPageMode.Identification;
                    }
                    if (chknotif.CheckDate.Value.ToUniversalTime() > chknotif.ValidityDate.ToUniversalTime())  // Always check with Universal Time
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorValidationTimeWindowElapsed"), ProviderPageMode.Identification);
                    }
                    else
                        result = new AdapterPresentation(this, context);
                }
                else
                {
                    usercontext.UIMode = ProviderPageMode.Locking;
                    result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                }
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;
            #endregion
        }

        /// <summary>
        /// CheckRequestCookie method implmentation
        /// </summary>
        private void CheckRequestCookie(AuthenticationContext usercontext, System.Net.HttpListenerRequest request)
        {
            var cook = request.Cookies["showoptions"];
            if (cook != null)
            {
                if (cook.Value == "1")
                    usercontext.ShowOptions = true;
                else
                    usercontext.ShowOptions = false;
            }
        }

        /// <summary>
        /// TrySendAdministrativeRequest method implementation
        /// </summary>
        private IAdapterPresentation TrySendAdministrativeRequest(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Invitation Request
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            claims = null;
            IAdapterPresentation result = null;
            try
            {
                usercontext.Notification = SendInscriptionToAdmins(usercontext, Resources);
                if (usercontext.Notification == NotificationStatus.Error)
                {
                    usercontext.UIMode = ProviderPageMode.Locking;
                    result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                }

                Notification invnotif = RuntimeRepository.CheckNotification(Config, (Registration)usercontext);
                if (invnotif.OTP == (int)NotificationStatus.ResponseEmailForAdminRegistration)
                {
                    if (Config.UserFeatures.IsMFANotRequired() || Config.UserFeatures.IsMFAAllowed()) 
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass);
                    }
                    else
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNotEnabled"), ProviderPageMode.DefinitiveError);
                    }
                }
                else
                {
                    usercontext.UIMode = ProviderPageMode.Locking;
                    return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);

                }
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;
            #endregion
        }

        /// <summary>
        /// TrySendKeyRequest implementation
        /// </summary>
        private IAdapterPresentation TrySendKeyRequest(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Sendkey Request
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            claims = null;
            IAdapterPresentation result = null;
            try
            {
                usercontext.Notification = SendKeyToUser(usercontext, Resources);
                if (usercontext.Notification == NotificationStatus.Error)
                {
                    usercontext.UIMode = ProviderPageMode.Locking;
                    result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                }

                Notification invnotif = RuntimeRepository.CheckNotification(Config, (Registration)usercontext);
                if (invnotif.OTP == (int)NotificationStatus.Error)
                {
                }
                else if (invnotif.OTP== (int)NotificationStatus.ResponseEmailForKey)
                {
                    usercontext.UIMode = usercontext.TargetUIMode;
                    result = new AdapterPresentation(this, context);
                }
                else
                {
                    usercontext.UIMode = ProviderPageMode.Locking;
                    return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                }
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;
            #endregion
        }

        #endregion

        #region Private methods
        /// <summary>
        /// SendNotification method implementation
        /// </summary>
        private NotificationStatus SendNotification(AuthenticationContext usercontext, ResourcesLocale resources)
        {
            if (usercontext.UIMode == ProviderPageMode.SendCodeRequest)
            {
                using (Task<NotificationStatus> tsk = new Task<NotificationStatus>(() => internalSendNotification(usercontext, resources), TaskCreationOptions.LongRunning))
                {
                    try
                    {
                        tsk.Start();
                        tsk.Wait(Config.DeliveryWindow * 1000);
                        return tsk.Result;
                    }
                    catch (Exception)
                    {
                        return NotificationStatus.Error;
                    }
                }
            }
            return NotificationStatus.Error;
        }

        /// <summary>
        /// SendInscriptionToAdmins method implementation
        /// </summary>
        private NotificationStatus SendInscriptionToAdmins(AuthenticationContext usercontext, ResourcesLocale resources)
        {
            if (usercontext.UIMode == ProviderPageMode.SendAdministrativeRequest)
            {
                using (Task<NotificationStatus> tsk = new Task<NotificationStatus>(() => internalSendInscriptionToAdmins(usercontext, resources), TaskCreationOptions.LongRunning))
                {
                    try
                    {
                        tsk.Start();
                        tsk.Wait(Config.DeliveryWindow * 1000);
                        return tsk.Result;
                    }
                    catch (Exception)
                    {
                        return NotificationStatus.Error;
                    }
                }
            }
            return NotificationStatus.Error;
        }

        /// <summary>
        /// SendKeyToUser method implementation
        /// </summary>
        private NotificationStatus SendKeyToUser(AuthenticationContext usercontext, ResourcesLocale resources)
        {
            if (usercontext.UIMode == ProviderPageMode.SendKeyRequest)
            {
                using (Task<NotificationStatus> tsk = new Task<NotificationStatus>(() => internalSendKeyToUser(usercontext, resources), TaskCreationOptions.LongRunning))
                {
                    try
                    {
                        tsk.Start();
                        tsk.Wait(Config.DeliveryWindow * 1000);
                        return tsk.Result;
                    }
                    catch (Exception)
                    {
                        return NotificationStatus.Error;
                    }
                }
            }
            return NotificationStatus.Error;
        }

        /// <summary>
        /// internalSendNotification method implementation
        /// </summary>
        private NotificationStatus internalSendNotification(AuthenticationContext usercontext, ResourcesLocale Resources) //, MFAConfig cfg) 
        {
            try
            {
                int otpres = (int)NotificationStatus.Error;
                Notification ntf = null;
                CheckUserProps(usercontext);
                switch (usercontext.PreferredMethod)
                {
                    case PreferredMethod.Email:
                        RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.RequestEmail);
                        otpres = Utilities.GetEmailOTP(usercontext, Config.SendMail, Resources.Culture);
                        ntf = RuntimeRepository.CheckNotification(Config, (Registration)usercontext);
                        if (ntf.OTP==(int)NotificationStatus.RequestEmail)
                            RuntimeRepository.SetNotification(Config, (Registration)usercontext, otpres);
                        if (otpres == (int)NotificationStatus.Error)
                            Log. WriteEntry(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation") + "\r\n" + usercontext.UPN, EventLogEntryType.Error, 800);
                        break;
                    case PreferredMethod.Phone:
                        RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.RequestExternal); 
                        otpres = Utilities.GetExternalOTP(usercontext, Config, Resources.Culture);
                        ntf = RuntimeRepository.CheckNotification(Config, (Registration)usercontext);
                        if (ntf.OTP == (int)NotificationStatus.RequestExternal)
                            RuntimeRepository.SetNotification(Config, (Registration)usercontext, otpres);
                        if (otpres == (int)NotificationStatus.Error)
                            Log. WriteEntry(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation") + "\r\n" + usercontext.UPN, EventLogEntryType.Error, 800);
                        break;
                    default:
                        usercontext.Notification = NotificationStatus.Totp;
                        break;
                }
            }
            catch (Exception ex)
            {
                usercontext.Notification = NotificationStatus.Error;
                RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.Error);
                Log. WriteEntry(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation") + "\r\n" + usercontext.UPN + " : " + "\r\n" + ex.Message + "\r\n" + ex.StackTrace, EventLogEntryType.Error, 800);
            }
            return usercontext.Notification;
        }

        /// <summary>
        /// internalSendInscriptionToAdmins method implmentation
        /// </summary>
        private NotificationStatus internalSendInscriptionToAdmins(AuthenticationContext usercontext, ResourcesLocale Resources)
        {
            try
            {
                RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.RequestIncription);
                MailUtilities.SendInscriptionMail(Config.AdminContact, (Registration)usercontext, Config.SendMail, Resources.Culture);
                RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.ResponseEmailForAdminRegistration);
                usercontext.Notification = NotificationStatus.ResponseEmailForAdminRegistration;
            }
            catch (Exception ex)
            {
                usercontext.Notification = NotificationStatus.Error;
                RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.Error);
                Log. WriteEntry(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation") + "\r\n" + usercontext.UPN + " : " + "\r\n" + ex.Message + "\r\n" + ex.StackTrace, EventLogEntryType.Error, 801);
            }
            return usercontext.Notification;
        }

        /// <summary>
        /// internalSendKeyToUser method implementation
        /// </summary>
        private NotificationStatus internalSendKeyToUser(AuthenticationContext usercontext, ResourcesLocale Resources)
        {
            try
            {
                CheckUserProps(usercontext);
                string qrcode = KeysManager.EncodedKey(usercontext.UPN);
                RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.RequestEmailForKey);
                MailUtilities.SendKeyByEmail(usercontext.MailAddress, usercontext.UPN, qrcode, Config.SendMail, Config, Resources.Culture);
                RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.ResponseEmailForKey);
                usercontext.Notification = NotificationStatus.ResponseEmailForKey;
            }
            catch (Exception ex)
            {
                usercontext.Notification = NotificationStatus.Error;
                RuntimeRepository.SetNotification(Config, (Registration)usercontext, (int)NotificationStatus.Error);
                Log. WriteEntry(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation") + "\r\n" + usercontext.UPN + " : " + "\r\n" + ex.Message + "\r\n" + ex.StackTrace, EventLogEntryType.Error, 801);
            }
            return usercontext.Notification;
        }

        /// <summary>
        /// CheckUserProps method implementation
        /// </summary>
        private void CheckUserProps(AuthenticationContext usercontext)
        {
            if (!KeysManager.ValidateKey(usercontext.UPN))
                throw new CryptographicException(string.Format("SECURTY ERROR : Invalid Key for User {0}", usercontext.UPN));
            Registration reg = RuntimeRepository.GetUserRegistration(Config, usercontext.UPN);
            if (reg==null)
                throw new Exception(string.Format("SECURTY ERROR : Invalid user {0}", usercontext.UPN));
            if (Config.MailEnabled)
            {
                if (string.IsNullOrEmpty(reg.MailAddress))
                    throw new Exception(string.Format("SECURTY ERROR : Invalid user mail address {0}", usercontext.UPN));
                if (string.IsNullOrEmpty(usercontext.MailAddress))
                    throw new Exception(string.Format("SECURTY ERROR : Invalid user mail address {0}", usercontext.UPN));
                if (!reg.MailAddress.ToLower().Equals(usercontext.MailAddress.ToLower()))
                    throw new Exception(string.Format("SECURTY ERROR : Invalid user mail address {0}", usercontext.UPN));
            }
            if (Config.SMSEnabled)
            {
                if (string.IsNullOrEmpty(reg.PhoneNumber))
                    throw new Exception(string.Format("SECURTY ERROR : Invalid user phone number {0}", usercontext.UPN));
                if (string.IsNullOrEmpty(usercontext.PhoneNumber))
                    throw new Exception(string.Format("SECURTY ERROR : Invalid user phone number {0}", usercontext.UPN));
                if (!reg.PhoneNumber.ToLower().Equals(usercontext.PhoneNumber.ToLower()))
                    throw new Exception(string.Format("SECURTY ERROR : Invalid user phone number {0}", usercontext.UPN));
            }
            return;
        }

        /// <summary>
        /// CheckPin method inplementation
        /// </summary>
        private bool CheckPin(string pin, Notification notif, AuthenticationContext usercontext)
        {
            if (notif.OTP == (int)NotificationStatus.Error)
                return false;
            else if (notif.OTP > (int)NotificationStatus.Error)
            {
                string generatedpin = notif.OTP.ToString("D6");  // eg : transmitted by email or by External System (SMS)
                return (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin));
            }
            else if (notif.OTP == (int)NotificationStatus.Totp)  // Using a TOTP Application (Microsoft Authnetication, Google Authentication, etc...)
            {
                if (!KeysManager.ValidateKey(usercontext.UPN))
                    throw new CryptographicException(string.Format("SECURTY ERROR : Invalid Key for User {0}", usercontext.UPN));
                string encodedkey = KeysManager.ProbeKey(usercontext.UPN);

                // Iterate through HashMode to check if != SHA1 / Microsoft Authnetication, Google Authentication and more support only SHA1
                foreach (HashMode algo in Enum.GetValues(typeof(HashMode)))
                {
                    if (Config.TOTPShadows <= 0)
                    {
                        DateTime call = DateTime.UtcNow;
                        OTPGenerator gen = new OTPGenerator(encodedkey, usercontext.UPN, call, algo);  // eg : TOTP code
                        gen.ComputeOTP(call);
                        string generatedpin = gen.OTP.ToString("D6");
                        if (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin))
                        {
                            return true;
                        }
                    }
                    else
                    {   // Current TOTP
                        DateTime tcall = DateTime.UtcNow;
                        OTPGenerator gen = new OTPGenerator(encodedkey, usercontext.UPN, tcall, algo);  // eg : TOTP code
                        gen.ComputeOTP(tcall);
                        string currentpin = gen.OTP.ToString("D6");
                        if (Convert.ToInt32(pin) == Convert.ToInt32(currentpin))
                        {
                            return true;
                        }
                        // TOTP with Shadow (current - x latest)
                        for (int i = 1; i <= Config.TOTPShadows; i++)
                        {
                            DateTime call = tcall.AddSeconds(-(i * OTPGenerator.TOTPDuration));
                            OTPGenerator gen2 = new OTPGenerator(encodedkey, usercontext.UPN, call, algo);  // eg : TOTP code
                            gen2.ComputeOTP(call);
                            string generatedpin = gen2.OTP.ToString("D6");
                            if (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin))
                            {
                                return true;
                            }
                        }
                        // TOTP with Shadow (current + x latest) - not possible. but can be usefull if time sync is not adequate
                        for (int i = 1; i <= Config.TOTPShadows; i++)
                        {
                            DateTime call = tcall.AddSeconds(i * OTPGenerator.TOTPDuration);
                            OTPGenerator gen3 = new OTPGenerator(encodedkey, usercontext.UPN, call, algo);  // eg : TOTP code
                            gen3.ComputeOTP(call);
                            string generatedpin = gen3.OTP.ToString("D6");
                            if (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            else if (notif.OTP == (int)NotificationStatus.Error)    // Active Request for code
                return false;
            else if (notif.OTP <= (int)NotificationStatus.Bypass)   // Magic - Validated by External ExternalOTPProvider
                return true;
            else
                return false;
        }

        /// <summary>
        /// ValidateEmail method implementation
        /// </summary>
        private void ValidateEmail(string email, ResourcesLocale Resources, bool checkempty = false)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    if (checkempty)
                        throw new Exception(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorEmailException"));
                    else
                        return;
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                    throw new Exception(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorEmailException"));
            }
            catch (Exception ex)
            {
                Log. WriteEntry(ex.Message, EventLogEntryType.Error, 800);
                throw ex;
            }
        }

        /// <summary>
        /// ValidateEmail method implementation
        /// </summary>
        private void ValidatePhoneNumber(string phone, ResourcesLocale Resources, bool checkempty = false)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    if (checkempty)
                        throw new Exception(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidPhoneException"));
                    else
                        return;
            }
            catch (Exception ex)
            {
                Log. WriteEntry(ex.Message, EventLogEntryType.Error, 800);
                throw ex;
            }
        }

        /// <summary>
        /// GetQRCodeString method implmentation
        /// </summary>
        public string GetQRCodeString(AuthenticationContext usercontext)
        {
            string displaykey = KeysManager.EncodedKey(usercontext.UPN);
            return QRUtilities.GetQRCodeString(usercontext.UPN, displaykey, this.Config);
        }

        /// <summary>
        /// OnMessageArrived method implmentation
        /// Reloads configuration
        /// </summary>
        private void OnMessageArrived(MailSlotServer maislotserver, MailSlotMessage message)
        {
            if (message.Operation == 0xAA)
            {
                Trace.TraceInformation("AuthenticationProvider:Configuration changed !");
                _config = CFGUtilities.ReadConfiguration();
                Trace.TraceInformation("AuthenticationProvider:Configuration loaded !");
                ResourcesLocale Resources = new ResourcesLocale(CultureInfo.CurrentCulture.LCID);
                Log.WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Informations, "InfosConfigurationReloaded"), message.MachineName), EventLogEntryType.Warning, 9999);

                RuntimeRepository.MailslotServer.AllowedMachines.Clear();
                foreach (ADFSServerHost svr in Config.Hosts.ADFSFarm.Servers)
                {
                    RuntimeRepository.MailslotServer.AllowedMachines.Add(svr.MachineName);
                }
            }
        }
        #endregion

        /// <summary>
        /// HookOptionParameter method implementation
        /// </summary>
        private bool HookOptionParameter(System.Net.HttpListenerRequest request)
        {
            Uri uri = new Uri(request.Url.AbsoluteUri);
            return uri.AbsoluteUri.Contains("mfaopts");
        }
    }
}
