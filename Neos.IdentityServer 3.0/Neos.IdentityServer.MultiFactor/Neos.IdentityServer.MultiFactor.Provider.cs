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
// #define softemail
using Microsoft.IdentityServer.Web.Authentication.External;
using Neos.IdentityServer.MultiFactor.Common;
using Neos.IdentityServer.MultiFactor.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;


namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// AuthenticationProvider class
    /// </summary>
    public class AuthenticationProvider : IAuthenticationAdapter
    {
        private static MFAConfig _config;

        /// <summary>
        /// Constructor override
        /// </summary>
        static AuthenticationProvider()
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
        public IAdapterPresentation BeginAuthentication(Claim identityClaim, HttpListenerRequest request, IAuthenticationContext context)
        {
            DateTime st = DateTime.Now;
            AuthenticationContext usercontext = new AuthenticationContext(context);
            usercontext.IPAddress = request.RemoteEndPoint.Address.ToString();
            Utilities.PatchLanguageIfNeeded(Config, usercontext, request.UserLanguages);
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);


            IAdapterPresentation result = null;
            try
            {
                WebThemeManager.Initialize(Config, usercontext, request.Url);
                ClaimsUtilities.SetIdentityClaim(identityClaim);
                if ((Config.IsPrimaryAuhentication) && (!Config.PrimaryAuhenticationOptions.HasFlag(PrimaryAuthOptions.Register)))
                {
                    if ((!usercontext.Enabled) || (!usercontext.IsRegistered))
                        usercontext.UIMode = ProviderPageMode.Locking;
                }
                switch (usercontext.UIMode)
                {
                    case ProviderPageMode.PreSet:
                        usercontext.UIMode = GetAuthenticationContextRequest(usercontext);
                        GetAuthenticationData(usercontext);
                        result = new AdapterPresentation(this, context);
                        break;
                    case ProviderPageMode.Locking:  
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNoAccess"), ProviderPageMode.DefinitiveError);
                        break;
                    default:
                        // Do not Select method if only one provider
                        if ((usercontext.UIMode == ProviderPageMode.ChooseMethod) && (usercontext.PreferredMethod==PreferredMethod.Choose)) 
                        {
                            if (RuntimeAuthProvider.GetActiveProvidersCount()<=1)
                            {
                                IExternalProvider pp = RuntimeAuthProvider.GetFirstActiveProvider();
                                usercontext.UIMode = ProviderPageMode.SendAuthRequest;
                                if (pp == null)
                                {
                                    if (Config.DefaultProviderMethod != PreferredMethod.Choose)
                                        usercontext.PreferredMethod = Config.DefaultProviderMethod;
                                    else
                                        usercontext.PreferredMethod = PreferredMethod.Code;
                                }
                                else
                                    usercontext.PreferredMethod = pp.Kind;
                            }
                            else
                            {
                                if (Config.DefaultProviderMethod != PreferredMethod.Choose)
                                {
                                    usercontext.UIMode = ProviderPageMode.SendAuthRequest;
                                    usercontext.PreferredMethod = Config.DefaultProviderMethod;
                                }
                            }
                            PatchUserContextWithSelectedMethod(usercontext);
                            GetAuthenticationData(usercontext);
                        }
                        else if ((HookOptionParameter(request)) && (Config.UserFeatures.CanAccessOptions()))
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
        public bool IsAvailableForUser(Claim identityClaim, IAuthenticationContext context)
        {
            DateTime st = DateTime.Now;
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            try
            {
                ClaimsUtilities.SetIdentityClaim(identityClaim);
                string upn = identityClaim.Value;
                MFAUser reg = RuntimeRepository.GetMFAUser(Config, upn);
                if (reg != null) // User Is Registered
                {
                    AuthenticationContext usercontext = new AuthenticationContext(context, reg);
                    if (usercontext.Enabled) // Enabled
                    {
                        usercontext.LogonDate = DateTime.Now;
                        usercontext.CurrentRetries = 0;
                        usercontext.NotificationSent = false;
                        if (usercontext.KeyStatus == SecretKeyStatus.NoKey)
                        {
                            if (Config.UserFeatures.CanManageOptions())
                                usercontext.UIMode = ProviderPageMode.PreSet;
                        }
                        else if (usercontext.PreferredMethod == PreferredMethod.Choose)
                            usercontext.UIMode = ProviderPageMode.ChooseMethod;
                        else
                            usercontext.UIMode = ProviderPageMode.PreSet;
                        Trace.TraceInformation(String.Format("AuthenticationProvider:IsAvailableForUser (Standard) Duration : {0}", (DateTime.Now - st).ToString()));
                        return true;
                    }
                    else // Not enabled
                    {
                        usercontext.LogonDate = DateTime.Now;
                        usercontext.CurrentRetries = 0;
                        usercontext.NotificationSent = false;
                        usercontext.UPN = upn;
                        usercontext.Enabled = false;
                        usercontext.PreferredMethod = Config.DefaultProviderMethod;

                        if (Config.UserFeatures.IsMFANotRequired())
                        {
                            usercontext.UIMode = ProviderPageMode.Bypass;
                            usercontext.TargetUIMode = ProviderPageMode.None;
                        }
                        else if (Config.UserFeatures.IsMFAAllowed())
                        {
                            if (Config.AdvertisingDays.OnFire)
                            {
                                usercontext.UIMode = ProviderPageMode.Activation;
                                usercontext.TargetUIMode = ProviderPageMode.None;
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.Bypass;
                                usercontext.TargetUIMode = ProviderPageMode.None;
                            }
                        }
                        else if (Config.UserFeatures.IsMFARequired())
                        {
                            usercontext.TargetUIMode = ProviderPageMode.DefinitiveError;
                            usercontext.UIMode = ProviderPageMode.Locking;
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
                    AuthenticationContext usercontext = new AuthenticationContext(context)
                    {
                        LogonDate = DateTime.Now,
                        CurrentRetries = 0,
                        NotificationSent = false,
                        UPN = upn,
                        Enabled = false,
                        PreferredMethod = Config.DefaultProviderMethod
                    };

                    if (Config.UserFeatures.IsAdministrative())                   // Error : administrative Only Registration
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        usercontext.TargetUIMode = ProviderPageMode.DefinitiveError;
                    }
                    else if (Config.UserFeatures.IsStrict())                      // Provide Information : Invitation
                    { 
                        usercontext.UIMode = ProviderPageMode.Invitation;
                        usercontext.TargetUIMode = ProviderPageMode.None;
                    }
                    else if (Config.UserFeatures.IsManaged())                      // Provide Information : Invitation
                    {
                        if (Config.UserFeatures.IsAdvertisable() && (Config.AdvertisingDays.OnFire))
                        {
                            usercontext.UIMode = ProviderPageMode.Invitation;
                            usercontext.TargetUIMode = ProviderPageMode.None;
                        }
                        else
                        {
                            usercontext.UIMode = ProviderPageMode.Bypass;
                            usercontext.TargetUIMode = ProviderPageMode.None;
                        }
                    }
                    else if (Config.UserFeatures.IsRegistrationRequired())        // Provide Information : Invitation
                    {
                        usercontext.UIMode = ProviderPageMode.Invitation;
                        usercontext.TargetUIMode = ProviderPageMode.None;
                    }
                    else if (Config.UserFeatures.IsRegistrationMixed())           // Provide Registration
                    {
                        usercontext.UIMode = ProviderPageMode.Registration;
                        usercontext.TargetUIMode = ProviderPageMode.None;
                    }
                    else if (Config.UserFeatures.IsRegistrationAllowed())         // Provide Registration
                    {
                        if (Config.UserFeatures.IsAdvertisable() && (Config.AdvertisingDays.OnFire))
                        {
                            usercontext.UIMode = ProviderPageMode.Registration;
                            usercontext.TargetUIMode = ProviderPageMode.None;
                        }
                        else
                        {
                            usercontext.UIMode = ProviderPageMode.Bypass;
                            usercontext.TargetUIMode = ProviderPageMode.None;
                        }
                    }
                    else if (Config.UserFeatures.IsRegistrationNotRequired())     // Bypass 
                    {
                        usercontext.UIMode = ProviderPageMode.Bypass;
                        usercontext.TargetUIMode = ProviderPageMode.None;
                    }
                    else                                                          // Error
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        usercontext.TargetUIMode = ProviderPageMode.DefinitiveError;
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
            ResourcesLocale Resources = new ResourcesLocale(CultureInfo.InstalledUICulture.LCID);
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
                             if ((!_config.OTPProvider.Enabled) && (!_config.MailProvider.Enabled) && (!_config.ExternalProvider.Enabled) && (!_config.AzureProvider.Enabled))
                                 _config.OTPProvider.Enabled = true;   // always let an active option eg : aplication in this case
                            using (AESSystemEncryption MSIS = new AESSystemEncryption())
                            {
                                _config.KeysConfig.XORSecret = MSIS.Decrypt(_config.KeysConfig.XORSecret);
                                _config.Hosts.ActiveDirectoryHost.Password = MSIS.Decrypt(_config.Hosts.ActiveDirectoryHost.Password);
                                _config.MailProvider.Password = MSIS.Decrypt(_config.MailProvider.Password);
                            };
                            Certs.InitializeAccountsSID(_config.Hosts.ActiveDirectoryHost.DomainName, _config.Hosts.ActiveDirectoryHost.Account, _config.Hosts.ActiveDirectoryHost.Password);
                            KeysManager.Initialize(_config);  // Always Bind KeysManager Otherwise this is made in CFGUtilities.ReadConfiguration
                            RuntimeAuthProvider.LoadProviders(_config); // Load Available providers
                         }
                         RuntimeRepository.MailslotServer.MailSlotMessageArrived += this.OnMessageArrived;
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
        public IAdapterPresentation OnError(HttpListenerRequest request, ExternalAuthenticationException ex)
        {
            return null; // Do not present an adapter let default adfs error handling
        }

        /// <summary>
        /// TryEndAuthentication method implementation
        /// </summary>
        public IAdapterPresentation TryEndAuthentication(IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            claims = null;
            IAdapterPresentation result = null;
            AuthenticationContext usercontext = new AuthenticationContext(context)
            {
                UIMessage = string.Empty
            };

            ProviderPageMode ui = usercontext.UIMode;        
            switch (ui)
            {
                case ProviderPageMode.Identification:
                    result = TryIdentification(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.Registration: // User self registration and enable
                    usercontext.WizContext = WizardContextMode.Registration;
                    result = TryRegistration(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.Invitation: // admministrative user registration and let disabled
                    usercontext.WizContext = WizardContextMode.Invitation;
                    result = TryInvitation(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.Activation: // Ask to enable
                    result = TryActivation(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.ManageOptions: // Manage Options
                    usercontext.WizContext = WizardContextMode.ManageOptions;
                    result = TryManageOptions(usercontext, context, proofData, request, out claims);
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
                case ProviderPageMode.SendAuthRequest:
                    result = TrySendCodeRequest(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.SendAdministrativeRequest:
                    result = TrySendAdministrativeRequest(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.SendKeyRequest:
                    result = TrySendKeyRequest(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.EnrollOTP:
                    result = TryEnrollOTP(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.EnrollEmail:
                    result = TryEnrollEmail(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.EnrollPhone:
                    result = TryEnrollPhone(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.EnrollBiometrics:
                    result = TryEnrollBio(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.EnrollPin:
                    result = TryEnrollPinCode(usercontext, context, proofData, request, out claims);
                    break;
            }
            return result;
        }
        #endregion

        #region IAuthenticationAdapter custom implementation
        /// <summary>
        /// TryIdentification method implementation
        /// </summary>
        private IAdapterPresentation TryIdentification(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = null;
            IAdapterPresentation result = null;
            usercontext.KeyChanged = false;
            try
            {
                string totp = proofData.Properties["totp"].ToString();
                bool pincode = proofData.Properties.TryGetValue("pincode", out object pin);
                int lnk = Convert.ToInt32(proofData.Properties["selected"].ToString());
                if (lnk == 0)
                {
                    if (usercontext.CurrentRetries >= Config.MaxRetries)
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                    }
                    if(DateTime.Now.ToUniversalTime() > usercontext.LogonDate.AddSeconds(Convert.ToDouble(Config.DeliveryWindow)).ToUniversalTime())
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorValidationTimeWindowElapsed"), ProviderPageMode.DefinitiveError);
                    }
                   /* if (!Utilities.CheckForReplay(Config, usercontext, request, Convert.ToInt32(totp)))
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorReplayToken"), ProviderPageMode.DefinitiveError);
                    } */
                    try
                    {
                        string error = string.Empty;
                        if ((int)AuthenticationResponseKind.Error != SetAuthenticationResult(usercontext, totp, out error))
                        {
                            if (pincode)
                            {
                                if (Convert.ToInt32(pin) < 0)
                                    pin = Config.DefaultPin;
                                if (Convert.ToInt32(pin) != usercontext.PinCode)
                                {
                                    if (usercontext.CurrentRetries >= Config.MaxRetries)
                                    {
                                        usercontext.UIMode = ProviderPageMode.Locking;
                                        return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                                    }
                                    else
                                    {
                                        usercontext.UIMode = ProviderPageMode.Identification;
                                        return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRetry"), false);
                                    }
                                }
                                usercontext.PinDone = true;
                            }
                            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };

                            bool options = proofData.Properties.TryGetValue("options", out object opt);
                            usercontext.ShowOptions = options;
                            if (options)
                            {
                                usercontext.UIMode = ProviderPageMode.Bypass;
                                return new AdapterPresentation(this, context);
                            }
                            else if ((usercontext.FirstChoiceMethod != PreferredMethod.Choose) && (usercontext.FirstChoiceMethod != PreferredMethod.None))
                            {
                                IExternalProvider prov = RuntimeAuthProvider.GetProvider(usercontext.FirstChoiceMethod);
                                if ((prov != null) && (prov.ForceEnrollment != ForceWizardMode.Disabled))
                                {
                                    if (usercontext.FirstChoiceMethod != usercontext.PreferredMethod)
                                    {
                                        switch (usercontext.FirstChoiceMethod)
                                        {
                                            case PreferredMethod.Code:
                                                usercontext.WizContext = WizardContextMode.ForceWizard;
                                                usercontext.UIMode = ProviderPageMode.EnrollOTP;
                                                break;
                                            case PreferredMethod.Email:
                                                usercontext.WizContext = WizardContextMode.ForceWizard;
                                                usercontext.UIMode = ProviderPageMode.EnrollEmail;
                                                break;
                                            case PreferredMethod.External:
                                                usercontext.WizContext = WizardContextMode.ForceWizard;
                                                usercontext.UIMode = ProviderPageMode.EnrollPhone;
                                                break;
                                            case PreferredMethod.Biometrics:
                                                usercontext.WizContext = WizardContextMode.ForceWizard;
                                                usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                                                break;
                                        }
                                        return new AdapterPresentation(this, context);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (usercontext.CurrentRetries >= Config.MaxRetries)
                            {
                                usercontext.UIMode = ProviderPageMode.Locking;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.Identification;
                                if (!string.IsNullOrEmpty(error))
                                    return new AdapterPresentation(this, context, error, false);
                                    // usercontext.UIMode = ProviderPageMode.Locking;
                                    // return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorReplayToken"), ProviderPageMode.DefinitiveError);
                                else
                                    return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRetry"), false);
                            }
                        }
                    }
                    catch (CryptographicException cex)
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        Log.WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAuthenticating"), cex.Message), EventLogEntryType.Error, 10000);
                        return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                    }
                }
                else
                {
                    if (usercontext.FirstChoiceMethod==PreferredMethod.Choose)
                        usercontext.FirstChoiceMethod = usercontext.PreferredMethod;
                    usercontext.UIMode = ProviderPageMode.ChooseMethod;
                    return new AdapterPresentation(this, context);
                }
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;
        }

        /// <summary>
        /// TryManageOptions method implementation
        /// </summary>
        private IAdapterPresentation TryManageOptions(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
            IAdapterPresentation result = null;
            usercontext.KeyChanged = false;
            try
            {
                int btnclicked = Convert.ToInt32(proofData.Properties["selected"].ToString());
                bool options = proofData.Properties.TryGetValue("disablemfa", out object opt);
                usercontext.Enabled = !options;
                usercontext.KeyChanged = false;

                switch (btnclicked)
                {
                    case 1:  // OK
                        {
                            string error = string.Empty;
                            if (proofData.Properties.ContainsKey("selectopt"))
                            {
                                PreferredMethod m = (PreferredMethod)Convert.ToInt32(proofData.Properties["selectopt"].ToString());
                                usercontext.PreferredMethod = m;
                            }
                            try
                            {
                                ValidateUserOptions(usercontext, context, proofData, Resources, true);
                            }
                            catch (Exception ex)
                            {
                                error = ex.Message;
                                return new AdapterPresentation(this, context, ex.Message, false);
                            }
                            RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, usercontext.KeyStatus != SecretKeyStatus.Success);
                            usercontext.UIMode = ProviderPageMode.SelectOptions;
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Informations, "InfosConfigurationModified"));
                            break;
                        }
                    case 2:  // Cancel   
                        {
                            usercontext.UIMode = ProviderPageMode.SelectOptions;
                            result = new AdapterPresentation(this, context);
                            break;
                        }
                    case 3:
                        {
                            usercontext.WizPageID = 0;
                            usercontext.WizContext = WizardContextMode.ManageOptions;
                            usercontext.UIMode = ProviderPageMode.EnrollOTP;
                            GetAuthenticationData(usercontext, PreferredMethod.Code);
                            result = new AdapterPresentation(this, context, ProviderPageMode.ManageOptions);
                            break;
                        }
                    case 4:
                        {
                            usercontext.WizPageID = 0;
                            usercontext.WizContext = WizardContextMode.ManageOptions;
                            usercontext.UIMode = ProviderPageMode.EnrollEmail;
                            GetAuthenticationData(usercontext, PreferredMethod.Email);
                            result = new AdapterPresentation(this, context, ProviderPageMode.ManageOptions);
                            break;
                        }
                    case 5:
                        {
                            usercontext.WizPageID = 0;
                            usercontext.WizContext = WizardContextMode.ManageOptions;
                            usercontext.UIMode = ProviderPageMode.EnrollPhone;
                            GetAuthenticationData(usercontext, PreferredMethod.External);
                            result = new AdapterPresentation(this, context, ProviderPageMode.ManageOptions);
                            break;
                        }
                    case 6:
                        {
                            usercontext.WizPageID = 0;
                            usercontext.WizContext = WizardContextMode.ManageOptions;
                            usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                            GetAuthenticationData(usercontext, PreferredMethod.Biometrics);
                            result = new AdapterPresentation(this, context, ProviderPageMode.ManageOptions);
                            break;
                        }
                    case 7:
                        {
                            usercontext.WizPageID = 0;
                            usercontext.WizContext = WizardContextMode.ManageOptions;
                            usercontext.UIMode = ProviderPageMode.EnrollPin;
                            GetAuthenticationData(usercontext, PreferredMethod.Pin);
                            result = new AdapterPresentation(this, context, ProviderPageMode.ManageOptions);
                            break;
                        }
                    default: // Option frame changed
                        {
                            string error = string.Empty;
                            try
                            {
                                ValidateUserOptions(usercontext, context, proofData, Resources, true);
                            }
                            catch (Exception ex)
                            {
                                error = ex.Message;
                            }
                            usercontext.UIMode = ProviderPageMode.ManageOptions;
                            if (string.IsNullOrEmpty(error))
                                result = new AdapterPresentation(this, context);
                            else
                                result = new AdapterPresentation(this, context, error, false);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;
        }

        /// <summary>
        /// TryRegistration method implementation
        /// </summary>
        private IAdapterPresentation TryRegistration(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
            IAdapterPresentation result = null;
            usercontext.KeyChanged = false;
            try
            {
                int isprovider = Convert.ToInt32(proofData.Properties["isprovider"].ToString());
                int btnclicked = Convert.ToInt32(proofData.Properties["selected"].ToString());
                bool skipwizard = (btnclicked==6);
                usercontext.KeyChanged = false;
                switch (isprovider)
                {
                    case (int)PreferredMethod.Choose:
                        {
                            if (btnclicked == 1) // Initial Cancel Registration
                            {
                                usercontext.UIMode = ProviderPageMode.Bypass;
                                result = new AdapterPresentation(this, context);
                            }
                            else if (btnclicked == 2)
                            {
                                usercontext.EnrollPageStatus = EnrollPageStatus.Run;
                                usercontext.UIMode = ProviderPageMode.Registration;
                                result = new AdapterPresentation(this, context);
                            }
                            else if (btnclicked == 3) // OK
                            {
                                usercontext.UIMode = ProviderPageMode.Registration;
                                usercontext.EnrollPageStatus = EnrollPageStatus.Stop;
                                result = new AdapterPresentation(this, context);
                            }
                            else if (btnclicked == 5)
                            {
                                usercontext.UIMode = ProviderPageMode.Bypass;
                                usercontext.Enabled = true;
                                RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, usercontext.KeyStatus != SecretKeyStatus.Success);
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Informations, "InfosConfigurationModified"));
                            }
                            else if (btnclicked == 6)
                            {
                                usercontext.UIMode = ProviderPageMode.Registration;
                                usercontext.EnrollPageStatus = EnrollPageStatus.Run;
                                result = new AdapterPresentation(this, context);
                            }
                            else if (btnclicked == 7)
                            {
                                usercontext.UIMode = ProviderPageMode.Locking;
                                result = new AdapterPresentation(this, context, ProviderPageMode.DefinitiveError);
                            }
                            break;
                        }
                    case (int)PreferredMethod.Code:
                        {
                            usercontext.WizPageID = 0;
                            if (skipwizard) // Skip
                            {
                                usercontext.UIMode = ProviderPageMode.Registration;
                                usercontext.EnrollPageID = PreferredMethod.Code;
                                result = new AdapterPresentation(this, context);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollOTP;
                                result = new AdapterPresentation(this, context, ProviderPageMode.Registration);
                            }
                           // usercontext.UIMode = ProviderPageMode.EnrollOTP;
                           // result = new AdapterPresentation(this, context, ProviderPageMode.Registration);
                            break;
                        }
                    case (int)PreferredMethod.Email:
                        {
                            usercontext.WizPageID = 0;
                            if (skipwizard) // Skip
                            {
                                usercontext.UIMode = ProviderPageMode.Registration;
                                usercontext.EnrollPageID = PreferredMethod.Email;
                                result = new AdapterPresentation(this, context);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollEmail;
                                result = new AdapterPresentation(this, context, ProviderPageMode.Registration);
                            }
                            break;
                        }
                    case (int)PreferredMethod.External:
                        {
                            usercontext.WizPageID = 0;
                            if (skipwizard) // Skip
                            {
                                usercontext.UIMode = ProviderPageMode.Registration;
                                usercontext.EnrollPageID = PreferredMethod.External;
                                result = new AdapterPresentation(this, context);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollPhone;
                                result = new AdapterPresentation(this, context, ProviderPageMode.Registration);
                            }
                            break;
                        }
                    case (int)PreferredMethod.Biometrics:
                        {
                            usercontext.WizPageID = 0;
                            if (skipwizard) // Skip
                            {
                                usercontext.UIMode = ProviderPageMode.Registration;
                                usercontext.EnrollPageID = PreferredMethod.Biometrics;
                                result = new AdapterPresentation(this, context);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                                result = new AdapterPresentation(this, context, ProviderPageMode.Registration);
                            }
                            break;
                        }
                    case (int)PreferredMethod.Pin:
                        {
                            usercontext.WizPageID = 0;
                            if (skipwizard) // Skip
                            {
                                usercontext.UIMode = ProviderPageMode.Registration;
                                usercontext.EnrollPageID = PreferredMethod.Pin;
                                result = new AdapterPresentation(this, context);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollPin;
                                result = new AdapterPresentation(this, context, ProviderPageMode.Registration);
                            }
                            break;
                        }
                    case (int)PreferredMethod.None:
                        usercontext.UIMode = ProviderPageMode.Registration;
                        usercontext.EnrollPageStatus = EnrollPageStatus.Stop;
                        result = new AdapterPresentation(this, context);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;
        }

        /// <summary>
        /// TryInvitation method implementation
        /// </summary>
        private IAdapterPresentation TryInvitation(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
            IAdapterPresentation result = null;
            usercontext.KeyChanged = false;
            try
            {
                int isprovider = Convert.ToInt32(proofData.Properties["isprovider"].ToString());
                int btnclicked = Convert.ToInt32(proofData.Properties["selected"].ToString());
                bool skipwizard = (btnclicked == 6);
                usercontext.KeyChanged = false;
                switch (isprovider)
                {
                    case (int)PreferredMethod.Choose:
                        {
                            if (btnclicked == 1) // Continue 
                            {
                                usercontext.UIMode = ProviderPageMode.Bypass;
                                result = new AdapterPresentation(this, context, ProviderPageMode.DefinitiveError);
                            }
                            else if (btnclicked == 2)
                            {
                                usercontext.EnrollPageStatus = EnrollPageStatus.Run;
                                usercontext.UIMode = ProviderPageMode.Invitation;
                                result = new AdapterPresentation(this, context);
                            }
                            else if (btnclicked == 3) // OK
                            {
                                usercontext.UIMode = ProviderPageMode.Invitation;
                                usercontext.EnrollPageStatus = EnrollPageStatus.Stop;
                                result = new AdapterPresentation(this, context);
                            }
                            else if (btnclicked == 5)
                            {
                                usercontext.UIMode = ProviderPageMode.Bypass;
                                usercontext.Enabled = false;
                                RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, usercontext.KeyStatus != SecretKeyStatus.Success);

                                if (Config.UserFeatures.InformationsRequired())
                                {
                                    usercontext.UIMode = ProviderPageMode.SendAdministrativeRequest;
                                    result = new AdapterPresentation(this, context);
                                }
                                else
                                {
                                    usercontext.UIMode = ProviderPageMode.Locking;
                                    result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNotEnabled"), ProviderPageMode.DefinitiveError);
                                }
                            }
                            else if (btnclicked == 6)
                            {
                                usercontext.UIMode = ProviderPageMode.Invitation;
                                usercontext.EnrollPageStatus = EnrollPageStatus.Run;
                                result = new AdapterPresentation(this, context);
                            }
                            else if (btnclicked == 7) // Quit Invitation 
                            {
                                usercontext.UIMode = ProviderPageMode.Locking;
                                result = new AdapterPresentation(this, context, ProviderPageMode.DefinitiveError);
                            }
                            break;
                        }
                    case (int)PreferredMethod.Code:
                        {
                            usercontext.WizPageID = 0;
                            if (skipwizard) // Skip
                            {
                                usercontext.UIMode = ProviderPageMode.Invitation;
                                usercontext.EnrollPageID = PreferredMethod.Code;
                                result = new AdapterPresentation(this, context);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollOTP;
                                result = new AdapterPresentation(this, context, ProviderPageMode.Invitation);
                            }

                           // usercontext.UIMode = ProviderPageMode.EnrollOTP;
                           // result = new AdapterPresentation(this, context, ProviderPageMode.Invitation);
                            break;
                        }
                    case (int)PreferredMethod.Email:
                        {
                            usercontext.WizPageID = 0;
                            if (skipwizard) // Skip
                            {
                                usercontext.UIMode = ProviderPageMode.Invitation;
                                usercontext.EnrollPageID = PreferredMethod.Email;
                                result = new AdapterPresentation(this, context);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollEmail;
                                result = new AdapterPresentation(this, context, ProviderPageMode.Invitation);
                            }
                            break;
                        }
                    case (int)PreferredMethod.External:
                        {
                            usercontext.WizPageID = 0;
                            if (skipwizard) // Skip
                            {
                                usercontext.UIMode = ProviderPageMode.Invitation;
                                usercontext.EnrollPageID = PreferredMethod.External;
                                result = new AdapterPresentation(this, context);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollPhone;
                                result = new AdapterPresentation(this, context, ProviderPageMode.Invitation);
                            }
                            break;
                        }
                    case (int)PreferredMethod.Biometrics:
                        {
                            usercontext.WizPageID = 0;
                            if (skipwizard) // Skip
                            {
                                usercontext.UIMode = ProviderPageMode.Invitation;
                                usercontext.EnrollPageID = PreferredMethod.Biometrics;
                                result = new AdapterPresentation(this, context);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                                result = new AdapterPresentation(this, context, ProviderPageMode.Invitation);
                            }
                            break;
                        }
                    case (int)PreferredMethod.Pin:
                        {
                            usercontext.WizPageID = 0;
                            if (skipwizard) // Skip
                            {
                                usercontext.UIMode = ProviderPageMode.Invitation;
                                usercontext.EnrollPageID = PreferredMethod.Pin;
                                result = new AdapterPresentation(this, context);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollPin;
                                result = new AdapterPresentation(this, context, ProviderPageMode.Invitation);
                            }
                            break;
                        }
                    case (int)PreferredMethod.None:
                        usercontext.UIMode = ProviderPageMode.Invitation;
                        usercontext.EnrollPageStatus = EnrollPageStatus.Stop;
                        result = new AdapterPresentation(this, context);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return result;

        }

        /// <summary>
        /// TryInvitation method implementation
        /// </summary>
        private IAdapterPresentation TryActivation(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
#region Invitation
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
            IAdapterPresentation result = null;
            usercontext.KeyChanged = false;
            try
            {
                int btnclicked = Convert.ToInt32(proofData.Properties["selected"].ToString());
                usercontext.UIMode = ProviderPageMode.Bypass;
                if (btnclicked == 2) // Enable account 
                    RuntimeRepository.EnableMFAUser(Config, (MFAUser)usercontext);
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
        /// TrySelectOptions method implementation
        /// </summary>
        private IAdapterPresentation TrySelectOptions(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
#region Select Options
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
            IAdapterPresentation result = null;
            try
            {
                usercontext.KeyChanged = false;
                usercontext.ShowOptions = false;
                if (Config.UserFeatures.CanAccessOptions())
                {
                    int lnk = Convert.ToInt32(proofData.Properties["selectedlink"].ToString());
                    int btnclicked = Convert.ToInt32(proofData.Properties["selected"].ToString());
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
                                usercontext.WizContext = WizardContextMode.ManageOptions;
                                usercontext.UIMode = ProviderPageMode.ManageOptions;
                                break;
                            case 2:
                                usercontext.UIMode = ProviderPageMode.ChangePassword;
                                break;
                            case 3:
                                usercontext.WizContext = WizardContextMode.DirectWizards;
                                usercontext.UIMode = ProviderPageMode.EnrollOTP;
                                GetAuthenticationData(usercontext, PreferredMethod.Code);
                                break;
                            case 4:
                                usercontext.WizContext = WizardContextMode.DirectWizards;
                                usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                                GetAuthenticationData(usercontext, PreferredMethod.Biometrics);
                                break;
                            case 5:
                                usercontext.WizContext = WizardContextMode.DirectWizards;
                                usercontext.UIMode = ProviderPageMode.EnrollEmail;
                                GetAuthenticationData(usercontext, PreferredMethod.Email);
                                break;
                            case 6:
                                usercontext.WizContext = WizardContextMode.DirectWizards;
                                usercontext.UIMode = ProviderPageMode.EnrollPhone;
                                GetAuthenticationData(usercontext, PreferredMethod.External);
                                break;
                            case 7:
                                usercontext.WizContext = WizardContextMode.DirectWizards;
                                usercontext.UIMode = ProviderPageMode.EnrollPin;
                                GetAuthenticationData(usercontext, PreferredMethod.Pin);
                                break;
                        }
                        usercontext.WizPageID = 0;
                        result = new AdapterPresentation(this, context, ProviderPageMode.SelectOptions);
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
        private IAdapterPresentation TryChooseMethod(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
#region Choose method
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = null;
            IAdapterPresentation result = null;
            usercontext.KeyChanged = false;
            try
            {
                int btnclicked = Convert.ToInt32(proofData.Properties["selected"].ToString());
                int opt = Convert.ToInt32(proofData.Properties["selectedradio"].ToString());
                bool remember = proofData.Properties.TryGetValue("remember", out object rem);
                if (btnclicked == 0)
                {
                    switch (opt)
                    {
                        case 0:
                            if (RuntimeAuthProvider.IsProviderAvailable(usercontext, PreferredMethod.Code))
                            {
                                usercontext.PreferredMethod = PreferredMethod.Code;
                                usercontext.UIMode = GetAuthenticationContextRequest(usercontext);
                                GetAuthenticationData(usercontext, PreferredMethod.Code);
                                result = new AdapterPresentation(this, context);
                                if (remember)
                                    RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, false);
                            }
                            else
                            {
                                usercontext.PreferredMethod = PreferredMethod.Choose;
                                usercontext.UIMode = ProviderPageMode.Locking;
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                            }
                            break;
                        case 1:
                            if (RuntimeAuthProvider.IsProviderAvailable(usercontext, PreferredMethod.External))
                            {
                                usercontext.PreferredMethod = PreferredMethod.External;
                                usercontext.UIMode = GetAuthenticationContextRequest(usercontext);
                                GetAuthenticationData(usercontext, PreferredMethod.External);
                                result = new AdapterPresentation(this, context);
                                if (remember)
                                    RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, false);
                            }
                            else
                            {
                                usercontext.PreferredMethod = PreferredMethod.Choose;
                                usercontext.UIMode = ProviderPageMode.Locking;
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                            }
                            break;
                        case 2:
                            if (RuntimeAuthProvider.IsProviderAvailable(usercontext, PreferredMethod.Email))
                            {
                                usercontext.PreferredMethod = PreferredMethod.Email;
                                if (Utilities.ValidateEmail(usercontext.MailAddress, true))
                                {
                                    usercontext.UIMode = GetAuthenticationContextRequest(usercontext);
                                    GetAuthenticationData(usercontext, PreferredMethod.Email);
                                    result = new AdapterPresentation(this, context);
                                    if (remember)
                                        RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, false);
                                }
                                else
                                {
                                    usercontext.PreferredMethod = PreferredMethod.Choose;
                                    usercontext.UIMode = ProviderPageMode.Locking;
                                    result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
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
                            if (RuntimeAuthProvider.IsProviderAvailable(usercontext, PreferredMethod.Azure))
                            {
                                usercontext.PreferredMethod = PreferredMethod.Azure;
                                usercontext.UIMode = GetAuthenticationContextRequest(usercontext);
                                GetAuthenticationData(usercontext, PreferredMethod.Azure);
                                result = new AdapterPresentation(this, context);
                                if (remember)
                                    RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, false);
                            }
                            else
                            {
                                usercontext.PreferredMethod = PreferredMethod.Choose;
                                usercontext.UIMode = ProviderPageMode.Locking;
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                            }
                            break;
                        case 4:
                            if (RuntimeAuthProvider.IsProviderAvailable(usercontext, PreferredMethod.Biometrics))
                            {
                                usercontext.PreferredMethod = PreferredMethod.Biometrics;
                                usercontext.UIMode = GetAuthenticationContextRequest(usercontext);
                                GetAuthenticationData(usercontext, PreferredMethod.Biometrics);
                                result = new AdapterPresentation(this, context);
                                if (remember)
                                    RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, false);
                            }
                            else
                            {
                                usercontext.PreferredMethod = PreferredMethod.Choose;
                                usercontext.UIMode = ProviderPageMode.Locking;
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                            }
                            break;

                        default:
                            usercontext.PreferredMethod = PreferredMethod.Choose;
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                            break;
                    }
                }
                else
                {
                    usercontext.PreferredMethod = PreferredMethod.Choose;
                    usercontext.UIMode = ProviderPageMode.Locking;
                    result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
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
        private IAdapterPresentation TryChangePassword(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
#region Change Password
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
            IAdapterPresentation result = null;
            usercontext.KeyChanged = false;
            if (_config.CustomUpdatePassword)
            {
                try
                {
                    string oldpass = proofData.Properties["oldpwdedit"].ToString();
                    string newpass = proofData.Properties["newpwdedit"].ToString();
                    string cnfpass = proofData.Properties["cnfpwdedit"].ToString();
                    string btnclick = proofData.Properties["selected"].ToString();
                    if (btnclick == "1")
                    {
                        if (!usercontext.NotificationSent)
                        {
                            MailUtilities.SendNotificationByEmail(Config, (MFAUser)usercontext, Config.MailProvider, Resources.Culture);
                            usercontext.NotificationSent = true;
                        }
                        if (newpass.Equals(cnfpass))
                        {
                            try
                            {
                                usercontext.UIMode = ProviderPageMode.SelectOptions;
                                RuntimeRepository.ChangePassword(this.Config, usercontext.UPN, oldpass, newpass);
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Informations, "InfosPasswordModified"));
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
                    else if (btnclick == "2")
                    {
                        usercontext.UIMode = ProviderPageMode.SelectOptions;
                        result = new AdapterPresentation(this, context);
                    }
                }
                catch (Exception ex)
                {
                    throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
                }
            }
            else
            {
                usercontext.UIMode = ProviderPageMode.SelectOptions;
                result = new AdapterPresentation(this, context);
            }
            return result;
#endregion
        }

        /// <summary>
        /// TryBypass method implementation
        /// </summary>
        private IAdapterPresentation TryBypass(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
#region TryBypass
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
            CheckOptionsCookie(usercontext, request);
            try
            {
                bool pincode = proofData.Properties.TryGetValue("pincode", out object pin);
                if (pincode)
                {
                    if (Convert.ToInt32(pin) <= 0)
                        pin = Config.DefaultPin;
                    if (Convert.ToInt32(pin) != usercontext.PinCode)
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                    }
                    usercontext.PinDone = true;
                }

                usercontext.KeyChanged = false;
                claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
                if (usercontext.ShowOptions)  // Launch user options
                {
                    usercontext.ShowOptions = false;
                    usercontext.UIMode = ProviderPageMode.SelectOptions;
                    return new AdapterPresentation(this, context);
                }
                else if ((usercontext.FirstChoiceMethod != PreferredMethod.Choose) && (usercontext.FirstChoiceMethod != PreferredMethod.None))
                {
                    IExternalProvider prov = RuntimeAuthProvider.GetProvider(usercontext.FirstChoiceMethod);
                    if ((prov != null) && ((prov.WizardEnabled) && (prov.ForceEnrollment!=ForceWizardMode.Disabled)))
                    {
                        if (usercontext.FirstChoiceMethod != usercontext.PreferredMethod)
                        {
                            switch (usercontext.FirstChoiceMethod)
                            {
                                case PreferredMethod.Code:
                                    usercontext.WizContext = WizardContextMode.ForceWizard;
                                    usercontext.UIMode = ProviderPageMode.EnrollOTP;
                                    break;
                                case PreferredMethod.Email:
                                    usercontext.WizContext = WizardContextMode.ForceWizard;
                                    usercontext.UIMode = ProviderPageMode.EnrollEmail;
                                    break;
                                case PreferredMethod.External:
                                    usercontext.WizContext = WizardContextMode.ForceWizard;
                                    usercontext.UIMode = ProviderPageMode.EnrollPhone;
                                    break;
                                case PreferredMethod.Biometrics:
                                    usercontext.WizContext = WizardContextMode.ForceWizard;
                                    usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                                    break;
                            }
                            return new AdapterPresentation(this, context);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ExternalAuthenticationException(usercontext.UPN + " : " + ex.Message, context);
            }
            return null;
#endregion
        }

        /// <summary>
        /// TryLocking method implementation
        /// </summary>
        private IAdapterPresentation TryLocking(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
#region Locking
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
            IAdapterPresentation result = null;
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            try
            {
                ProviderPageMode lnk = usercontext.TargetUIMode;
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
        private IAdapterPresentation TrySendCodeRequest(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
#region Requesting Code
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
            usercontext.DirectLogin = false;
            IAdapterPresentation result = null;
            try
            {
                if (usercontext.CurrentRetries >= Config.MaxRetries)
                {
                    usercontext.UIMode = ProviderPageMode.Locking;
                    return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                }
                if (DateTime.Now.ToUniversalTime() > usercontext.LogonDate.AddSeconds(Convert.ToDouble(Config.DeliveryWindow)).ToUniversalTime())
                {
                    usercontext.UIMode = ProviderPageMode.Locking;
                    return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorValidationTimeWindowElapsed"), ProviderPageMode.DefinitiveError);
                }
                if (proofData.Properties.ContainsKey("selectedlink"))
                {
                    int lnk = Convert.ToInt32(proofData.Properties["selectedlink"].ToString());
                    if (lnk == 3)
                    {
                        if (usercontext.FirstChoiceMethod==PreferredMethod.Choose)
                            usercontext.FirstChoiceMethod = usercontext.PreferredMethod;
                        usercontext.UIMode = ProviderPageMode.ChooseMethod;
                        return new AdapterPresentation(this, context);
                    }
                }
                usercontext.ShowOptions = proofData.Properties.TryGetValue("options", out object opt);
                if (!usercontext.IsRemote)
                {
                    if ((int)AuthenticationResponseKind.Error == PostAuthenticationRequest(usercontext))
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                    }
                    usercontext.UIMode = ProviderPageMode.Identification;
                }
                else if (!usercontext.IsTwoWay)
                {
                    if ((int)AuthenticationResponseKind.Error == PostAuthenticationRequest(usercontext))
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                    }
                    switch (usercontext.SelectedMethod)
                    {
                        case AuthenticationResponseKind.SmsOneWayOTP:
                        case AuthenticationResponseKind.SmsOneWayOTPplusPin:
                        case AuthenticationResponseKind.SmsOTP:
                            usercontext.UIMode = ProviderPageMode.Identification;
                            break;
                        case AuthenticationResponseKind.EmailOTP:
                            usercontext.UIMode = ProviderPageMode.Identification;
                            break;
                        case AuthenticationResponseKind.PhoneAppOTP:
                            usercontext.UIMode = ProviderPageMode.Identification;
                            break;
                        case AuthenticationResponseKind.Sample1:
                        case AuthenticationResponseKind.Sample2:
                        case AuthenticationResponseKind.Sample3:
                            usercontext.UIMode = ProviderPageMode.Identification;
                            break;
                        default:
                            usercontext.UIMode = ProviderPageMode.Locking;
                            return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                    }
                }
                else if (usercontext.IsTwoWay)
                {
                    if ((int)AuthenticationResponseKind.Error == PostAuthenticationRequest(usercontext))
                    {
                        if (usercontext.CurrentRetries >= Config.MaxRetries)
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                        }
                        else
                        {
                            usercontext.UIMode = ProviderPageMode.SendAuthRequest;
                            return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformationRetry"), false);
                        }
                    }
                    string valuetopass = string.Empty;
                    string error = string.Empty;
                    bool cango = true;
                    if (usercontext.SelectedMethod == AuthenticationResponseKind.Biometrics)
                    {
                        if (proofData.Properties.ContainsKey("selected"))
                        {
                            int lnk = Convert.ToInt32(proofData.Properties["selected"].ToString());
                            switch (lnk)
                            {
                                case 1:
                                    valuetopass = proofData.Properties["assertionResponse"].ToString();
                                    break;
                                case 2:
                                    error = proofData.Properties["jsError"].ToString();
                                    cango = false;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    if (cango && (int)AuthenticationResponseKind.Error != SetAuthenticationResult(usercontext, valuetopass, out error))
                    {
                        switch (usercontext.SelectedMethod)
                        {
                            case AuthenticationResponseKind.SmsTwoWayOTP:
                            case AuthenticationResponseKind.SmsTwoWayOTPplusPin:
                                usercontext.UIMode = ProviderPageMode.Bypass;
                                break;
                            case AuthenticationResponseKind.PhoneAppNotification:
                                usercontext.UIMode = ProviderPageMode.Bypass;
                                break;
                            case AuthenticationResponseKind.PhoneAppConfirmation:
                            case AuthenticationResponseKind.VoiceTwoWayMobile:
                            case AuthenticationResponseKind.VoiceTwoWayMobilePlusPin:
                            case AuthenticationResponseKind.VoiceTwoWayOffice:
                            case AuthenticationResponseKind.VoiceTwoWayOfficePlusPin:
                            case AuthenticationResponseKind.VoiceTwoWayAlternateMobile:
                            case AuthenticationResponseKind.VoiceTwoWayAlternateMobilePlusPin:
                                usercontext.UIMode = ProviderPageMode.Bypass;
                                break;
                            case AuthenticationResponseKind.Sample1Async:
                            case AuthenticationResponseKind.Sample2Async:
                            case AuthenticationResponseKind.Sample3Async:
                                usercontext.UIMode = ProviderPageMode.Bypass;
                                break;
                            case AuthenticationResponseKind.Biometrics:
                                usercontext.UIMode = ProviderPageMode.Bypass;
                                break;
                            default:
                                usercontext.UIMode = ProviderPageMode.Locking;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                        }
                    }
                    else
                    {
                        if (DateTime.Now.ToUniversalTime() > usercontext.LogonDate.AddSeconds(Convert.ToDouble(Config.DeliveryWindow)).ToUniversalTime())
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorValidationTimeWindowElapsed"), ProviderPageMode.DefinitiveError);
                        }
                        if (usercontext.CurrentRetries >= Config.MaxRetries)
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                        }
                        else
                        {
                            usercontext.UIMode = ProviderPageMode.SendAuthRequest;
                            if (!string.IsNullOrEmpty(error))
                                return new AdapterPresentation(this, context, error, false);
                            else
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformationRetry"), false);
                        }                        
                    }
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
        /// TrySendAdministrativeRequest method implementation
        /// </summary>
        private IAdapterPresentation TrySendAdministrativeRequest(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
#region Invitation Request
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
            IAdapterPresentation result = null;
            try
            {
                usercontext.Notification = (int)PostInscriptionRequest(usercontext, (MFAUser)usercontext);
                if (usercontext.Notification == (int)AuthenticationResponseKind.Error)
                {
                    usercontext.UIMode = ProviderPageMode.Locking;
                    return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                }

                usercontext.Notification = (int)this.SetInscriptionResult(usercontext, (MFAUser)usercontext);
                if (usercontext.Notification == (int)AuthenticationResponseKind.EmailForInscription)
                {
                    // Store Account as disabled and reload it
                    usercontext.Enabled = false;
                    MFAUser reg = RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, false);
                    usercontext.Assign(reg);

                    if (Config.UserFeatures.IsMFANotRequired() || Config.UserFeatures.IsMFAAllowed()) // Bypass
                    {
                        usercontext.UIMode = ProviderPageMode.Bypass;
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass);
                    }
                    else  // Error Not Enabled
                    {
                        usercontext.UIMode = ProviderPageMode.Locking; 
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "ErrorAccountNotEnabled"), ProviderPageMode.DefinitiveError);
                    }
                }
                else // Error
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
        private IAdapterPresentation TrySendKeyRequest(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
#region Sendkey Request
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
            IAdapterPresentation result = null;
            try
            {
                usercontext.Notification = (int)this.PostSecretKeyRequest(usercontext);
                if (usercontext.Notification == (int)AuthenticationResponseKind.Error)
                {
                    usercontext.UIMode = ProviderPageMode.Locking;
                    result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                }

                usercontext.Notification = (int)this.SetSecretKeyResult(usercontext);
                if (usercontext.Notification == (int)AuthenticationResponseKind.EmailForKey)
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

        /// <summary>
        /// TryShowQRCode method implementation
        /// </summary>
        private IAdapterPresentation TryShowQRCode(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
#region Show QR Code
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
            IAdapterPresentation result = null;
            try
            {
                usercontext.KeyChanged = false;
                if (usercontext.Enabled)
                {
                    usercontext.UIMode = ProviderPageMode.ManageOptions;
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
        /// TryEnrollOTP implementation
        /// </summary>
        private IAdapterPresentation TryEnrollOTP(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            string error = string.Empty;
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };

            usercontext.FirstChoiceMethod = PreferredMethod.Choose;
            try
            {
                int btnclicked = Convert.ToInt32(proofData.Properties["selected"].ToString());
                bool remember = proofData.Properties.TryGetValue("remember", out object rem);
                usercontext.KeyChanged = false;

                switch (btnclicked)
                {
                    case 1:  // Cancel
                        if (usercontext.TargetUIMode == ProviderPageMode.Registration)
                        {
                            usercontext.UIMode = ProviderPageMode.Registration;
                        }
                        else if (usercontext.TargetUIMode == ProviderPageMode.Invitation)
                        {
                            usercontext.UIMode = ProviderPageMode.Invitation;
                        }
                        else if (usercontext.TargetUIMode == ProviderPageMode.ManageOptions)
                        {
                            usercontext.UIMode = ProviderPageMode.ManageOptions;
                        }
                        else
                            usercontext.UIMode = ProviderPageMode.SelectOptions;

                        return new AdapterPresentation(this, context);
                    case 2: // Next Button
                        usercontext.WizPageID = 1;
                        usercontext.KeyStatus = SecretKeyStatus.Success;
                        if (remember)
                            usercontext.PreferredMethod = PreferredMethod.Code;
                        SetProviderOverrideOption(usercontext, context, proofData, PreferredMethod.Code);
                        ValidateProviderManagementUrl(usercontext, context, proofData, PreferredMethod.Code);
                        if (!usercontext.NotificationSent)
                        {
                            MailUtilities.SendNotificationByEmail(Config, (MFAUser)usercontext, Config.MailProvider, Resources.Culture);
                            usercontext.NotificationSent = true;
                        }
                        return new AdapterPresentation(this, context);
                    case 3: // Code verification
                        try
                        {
                            if ((int)AuthenticationResponseKind.Error == PostAuthenticationRequest(usercontext, PreferredMethod.Code))
                            {
                                usercontext.WizPageID = 4;
                                usercontext.UIMode = ProviderPageMode.EnrollOTP;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                            }
                            else
                            {
                                usercontext.WizPageID = 2;
                                usercontext.UIMode = ProviderPageMode.EnrollOTP;
                                return new AdapterPresentation(this, context);
                            }
                        }
                        catch (Exception ex)
                        {
                            usercontext.UIMode = ProviderPageMode.EnrollOTP;
                            usercontext.WizPageID = 0;
                            return new AdapterPresentation(this, context, ex.Message, false);
                        }
                    case 4: // Code validation
                        string totp = proofData.Properties["totp"].ToString();
                        if ((int)AuthenticationResponseKind.Error != SetAuthenticationResult(usercontext, totp, out error, PreferredMethod.Code))
                        {
                            try
                            {
                                ValidateUserKey(usercontext, context, proofData, Resources, true);
                                if (usercontext.WizContext==WizardContextMode.DirectWizards)
                                    RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, usercontext.KeyStatus != SecretKeyStatus.Success);
                                else if (usercontext.WizContext == WizardContextMode.Registration)
                                {
                                    usercontext.EnrollPageID = PreferredMethod.Code;
                                    usercontext.UIMode = ProviderPageMode.Registration;
                                }
                                else if (usercontext.WizContext == WizardContextMode.Invitation)
                                {
                                    usercontext.EnrollPageID = PreferredMethod.Code;
                                    usercontext.UIMode = ProviderPageMode.Invitation;
                                }
                                else
                                    usercontext.UIMode = ProviderPageMode.EnrollOTP;
                                usercontext.WizPageID = 3;
                                return new AdapterPresentation(this, context, RuntimeAuthProvider.GetProvider(PreferredMethod.Code).GetUIEnrollValidatedLabel(usercontext), true);
                            }
                            catch (Exception)
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollOTP;
                                usercontext.WizPageID = 4;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                            }
                        }
                        else
                        {
                            usercontext.WizPageID = 4;
                            usercontext.UIMode = ProviderPageMode.EnrollOTP;
                            return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                        }
                }
            }
            catch (Exception ex)
            {
                if (usercontext.TargetUIMode == ProviderPageMode.Registration)
                {
                    usercontext.UIMode = ProviderPageMode.Registration;
                    usercontext.TargetUIMode = ProviderPageMode.Bypass;
                }
                else if (usercontext.TargetUIMode == ProviderPageMode.Invitation)
                {
                    usercontext.UIMode = ProviderPageMode.Invitation;
                    if (Config.UserFeatures.IsMFARequired())
                        usercontext.TargetUIMode = ProviderPageMode.Locking;
                    else
                        usercontext.TargetUIMode = ProviderPageMode.Bypass;
                }
                else
                {
                    usercontext.UIMode = ProviderPageMode.EnrollOTP;
                    usercontext.WizPageID = 0;
                }
                return new AdapterPresentation(this, context, ex.Message, false);
            }
            return null;
        }

        /// <summary>
        /// TryEnrollEmail implementation
        /// </summary>
        private IAdapterPresentation TryEnrollEmail(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            string error = string.Empty;
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };

            usercontext.FirstChoiceMethod = PreferredMethod.Choose;
            try
            {                               
                int btnclicked = Convert.ToInt32(proofData.Properties["selected"].ToString());
                bool remember = proofData.Properties.TryGetValue("remember", out object rem);
                usercontext.KeyChanged = false;

                switch (btnclicked)
                {
                    case 0:
                        usercontext.WizPageID = 0; // Get Email
                        return new AdapterPresentation(this, context);
                    case 1:  // Cancel
                        if (usercontext.TargetUIMode == ProviderPageMode.Registration)
                        {
                            usercontext.UIMode = ProviderPageMode.Registration;
                        }
                        else if (usercontext.TargetUIMode == ProviderPageMode.Invitation)
                        {
                            usercontext.UIMode = ProviderPageMode.Invitation;
                        }
                        else if (usercontext.TargetUIMode == ProviderPageMode.ManageOptions)
                        {
                            usercontext.UIMode = ProviderPageMode.ManageOptions;
                        }
                        else
                            usercontext.UIMode = ProviderPageMode.SelectOptions;
                        return new AdapterPresentation(this, context);
                    case 2: // Next Button
                        try
                        {
                            usercontext.WizPageID = 1; // Goto Donut
                            ValidateUserEmail(usercontext, context, proofData, Resources, true);
                            if (remember)
                                usercontext.PreferredMethod = PreferredMethod.Email;
                            SetProviderOverrideOption(usercontext, context, proofData, PreferredMethod.Email);
                            ValidateProviderManagementUrl(usercontext, context, proofData, PreferredMethod.Email);
                            if (!usercontext.NotificationSent)
                            {
                                MailUtilities.SendNotificationByEmail(Config, (MFAUser)usercontext, Config.MailProvider, Resources.Culture);
                                usercontext.NotificationSent = true;
                            }
                            return new AdapterPresentation(this, context);
                        }
                        catch (Exception ex)
                        {
                            usercontext.UIMode = ProviderPageMode.EnrollEmail;
                            usercontext.WizPageID = 0;
                            return new AdapterPresentation(this, context, ex.Message, false);
                        }
                    case 3: // Code Validation Donut
                        try
                        {
                            ValidateUserEmail(usercontext, context, proofData, Resources, true);
                            IExternalProvider prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Email);
                            AuthenticationResponseKind kd = prov.GetOverrideMethod(usercontext);
                            
                            if (prov.SetSelectedAuthenticationMethod(usercontext, kd, true))
                            {
                                if ((int)AuthenticationResponseKind.Error == PostAuthenticationRequest(usercontext, PreferredMethod.Email))
                                {
                                    usercontext.WizPageID = 4;
                                    usercontext.UIMode = ProviderPageMode.EnrollEmail;
                                    return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                                }
                                else
                                {
                                    if (!usercontext.IsRemote) // ???
                                    {
                                        usercontext.WizPageID = 2;
                                    }
                                    else if (!usercontext.IsTwoWay)
                                    {
                                        usercontext.WizPageID = 2;
                                    }
                                    else
                                    {
                                        // Not very usefull for email provider, because OneWay transmission, but if you implement an IExternalProvider of Email Kind, you can make it TwoWay 
                                        if ((int)AuthenticationResponseKind.Error != SetAuthenticationResult(usercontext, string.Empty, out error, PreferredMethod.Email))
                                        {
                                            if (usercontext.WizContext == WizardContextMode.DirectWizards)
                                                RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, false);
                                            else if (usercontext.WizContext == WizardContextMode.Registration)
                                            {
                                                usercontext.EnrollPageID = PreferredMethod.Email;
                                                usercontext.UIMode = ProviderPageMode.Registration;
                                            }
                                            else if (usercontext.WizContext == WizardContextMode.Invitation)
                                            {
                                                usercontext.EnrollPageID = PreferredMethod.Email;
                                                usercontext.UIMode = ProviderPageMode.Invitation;
                                            }
                                            else
                                                usercontext.UIMode = ProviderPageMode.EnrollEmail;
                                            usercontext.WizPageID = 3;
                                        }
                                        else
                                        {
                                            usercontext.UIMode = ProviderPageMode.EnrollEmail;
                                            usercontext.WizPageID = 4;
                                        }
                                    }
                                    return new AdapterPresentation(this, context, RuntimeAuthProvider.GetProvider(PreferredMethod.Email).GetUIEnrollValidatedLabel(usercontext), true);
                                }
                            }
                            else
                            {
                                usercontext.WizPageID = 4;
                                usercontext.UIMode = ProviderPageMode.EnrollEmail;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                            }
                        }
                        catch (Exception ex)
                        {
                            usercontext.UIMode = ProviderPageMode.EnrollEmail;
                            usercontext.WizPageID = 0;
                            return new AdapterPresentation(this, context, ex.Message, false);
                        }
                    case 4: // Code Validation
                        string totp = proofData.Properties["totp"].ToString();
                        if ((int)AuthenticationResponseKind.Error != SetAuthenticationResult(usercontext, totp, out error, PreferredMethod.Email))
                        {
                            try
                            {
                                ValidateUserEmail(usercontext, context, proofData, Resources, true);
                                if (usercontext.WizContext == WizardContextMode.DirectWizards)
                                    RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, false);
                                else if (usercontext.WizContext == WizardContextMode.Registration)
                                {
                                    usercontext.EnrollPageID = PreferredMethod.Email;
                                    usercontext.UIMode = ProviderPageMode.Registration;
                                }
                                else if (usercontext.WizContext == WizardContextMode.Invitation)
                                {
                                    usercontext.EnrollPageID = PreferredMethod.Email;
                                    usercontext.UIMode = ProviderPageMode.Invitation;
                                }
                                else
                                    usercontext.UIMode = ProviderPageMode.EnrollEmail;
                                usercontext.WizPageID = 3;
                                return new AdapterPresentation(this, context, RuntimeAuthProvider.GetProvider(PreferredMethod.Email).GetUIEnrollValidatedLabel(usercontext), true);
                            }
                            catch (Exception)
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollEmail;
                                usercontext.WizPageID = 4;
                                MFAUser reg = RuntimeRepository.GetMFAUser(Config, usercontext.UPN); // Rollback
                                usercontext.MailAddress = reg.MailAddress;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                            }
                        }
                        else
                        {
                            usercontext.WizPageID = 4;
                            usercontext.UIMode = ProviderPageMode.EnrollEmail;
                            MFAUser reg = RuntimeRepository.GetMFAUser(Config, usercontext.UPN); // Rollback
                            usercontext.MailAddress = reg.MailAddress;
                            return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                        }
                }
            }
            catch (Exception ex)
            {
                if (usercontext.TargetUIMode == ProviderPageMode.Registration)
                {
                    usercontext.UIMode = ProviderPageMode.Registration;
                    usercontext.TargetUIMode = ProviderPageMode.Bypass;
                }
                else if (usercontext.TargetUIMode == ProviderPageMode.Invitation)
                {
                    usercontext.UIMode = ProviderPageMode.Invitation;
                    if (Config.UserFeatures.IsMFARequired())
                        usercontext.TargetUIMode = ProviderPageMode.Locking;
                    else
                        usercontext.TargetUIMode = ProviderPageMode.Bypass;
                }
                else
                {
                    usercontext.UIMode = ProviderPageMode.EnrollEmail;
                    usercontext.WizPageID = 0;
                }
                return new AdapterPresentation(this, context, ex.Message, false);
            }
            return null;
        }

        /// <summary>
        /// TryEnrollPhone implementation
        /// </summary>
        private IAdapterPresentation TryEnrollPhone(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            string error = string.Empty;
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };

            usercontext.KeyChanged = false;
            usercontext.FirstChoiceMethod = PreferredMethod.Choose;
            try
            {
                int btnclicked = Convert.ToInt32(proofData.Properties["selected"].ToString());
                bool remember = proofData.Properties.TryGetValue("remember", out object rem);
                usercontext.KeyChanged = false;

                switch (btnclicked)
                {
                    case 0: // Next Button
                        usercontext.WizPageID = 0; 
                        return new AdapterPresentation(this, context);
                    case 1:  // Cancel
                        if (usercontext.TargetUIMode == ProviderPageMode.Registration)
                        {
                            usercontext.UIMode = ProviderPageMode.Registration;
                        }
                        else if (usercontext.TargetUIMode == ProviderPageMode.Invitation)
                        {
                            usercontext.UIMode = ProviderPageMode.Invitation;
                        }
                        else if (usercontext.TargetUIMode == ProviderPageMode.ManageOptions)
                        {
                            usercontext.UIMode = ProviderPageMode.ManageOptions;
                        }
                        else
                            usercontext.UIMode = ProviderPageMode.SelectOptions;                        
                        return new AdapterPresentation(this, context);
                    case 2: // Next Button
                        try
                        {
                            usercontext.WizPageID = 1; // Goto Donut
                            ValidateUserPhone(usercontext, context, proofData, Resources, true);
                            if (remember)
                                usercontext.PreferredMethod = PreferredMethod.External;
                            SetProviderOverrideOption(usercontext, context, proofData, PreferredMethod.External);
                            ValidateProviderManagementUrl(usercontext, context, proofData, PreferredMethod.External);
                            if (!usercontext.NotificationSent) 
                            {
                                MailUtilities.SendNotificationByEmail(Config, (MFAUser)usercontext, Config.MailProvider, Resources.Culture);
                                usercontext.NotificationSent = true;
                            }
                            return new AdapterPresentation(this, context);
                        }
                        catch (Exception ex)
                        {
                            usercontext.UIMode = ProviderPageMode.EnrollPhone;
                            usercontext.WizPageID = 0;
                            return new AdapterPresentation(this, context, ex.Message, false);
                        }
                    case 3: // Code Validation Donut
                        try
                        {
                            ValidateUserPhone(usercontext, context, proofData, Resources, true);
                            IExternalProvider prov = RuntimeAuthProvider.GetProvider(PreferredMethod.External);
                            AuthenticationResponseKind kd = prov.GetOverrideMethod(usercontext);
                            if (prov.SetSelectedAuthenticationMethod(usercontext, kd, true))
                            { 
                                if ((int)AuthenticationResponseKind.Error == PostAuthenticationRequest(usercontext, PreferredMethod.External))
                                {
                                    usercontext.WizPageID = 4;
                                    usercontext.UIMode = ProviderPageMode.EnrollPhone;
                                    return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                                }
                                else
                                {
                                    if (!usercontext.IsRemote)
                                    {
                                        usercontext.WizPageID = 2;
                                    }
                                    else if (!usercontext.IsTwoWay)
                                    {
                                        usercontext.WizPageID = 2;
                                    }
                                    else
                                    {                                        
                                        if ((int)AuthenticationResponseKind.Error != SetAuthenticationResult(usercontext, string.Empty, out error, PreferredMethod.External))
                                        {
                                            if (usercontext.WizContext == WizardContextMode.DirectWizards)
                                                RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, false);
                                            else if (usercontext.WizContext == WizardContextMode.Registration)
                                            {
                                                usercontext.EnrollPageID = PreferredMethod.External;
                                                usercontext.UIMode = ProviderPageMode.Registration;
                                            }
                                            else if (usercontext.WizContext == WizardContextMode.Invitation)
                                            {
                                                usercontext.EnrollPageID = PreferredMethod.External;
                                                usercontext.UIMode = ProviderPageMode.Invitation;
                                            }
                                            else
                                                usercontext.UIMode = ProviderPageMode.EnrollPhone;
                                            usercontext.WizPageID = 3;
                                        }
                                        else
                                        {
                                            usercontext.UIMode = ProviderPageMode.EnrollPhone;
                                            usercontext.WizPageID = 4;
                                        }
                                    }
                                    return new AdapterPresentation(this, context, RuntimeAuthProvider.GetProvider(PreferredMethod.External).GetUIEnrollValidatedLabel(usercontext), true);
                                }
                            }
                            else
                            {
                                usercontext.WizPageID = 4;
                                usercontext.UIMode = ProviderPageMode.EnrollPhone;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                            }
                        }
                        catch (Exception ex)
                        {
                            usercontext.UIMode = ProviderPageMode.EnrollPhone;
                            usercontext.WizPageID = 0;
                            return new AdapterPresentation(this, context, ex.Message, false);
                        }
                    case 4: // Code Validation
                        string totp = proofData.Properties["totp"].ToString();
                        if ((int)AuthenticationResponseKind.Error != SetAuthenticationResult(usercontext, totp, out error, PreferredMethod.External))
                        {
                            try
                            {
                                ValidateUserPhone(usercontext, context, proofData, Resources, true);
                                if (usercontext.WizContext==WizardContextMode.DirectWizards)
                                   RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, false);
                                else if (usercontext.WizContext == WizardContextMode.Registration)
                                {
                                    usercontext.EnrollPageID = PreferredMethod.External;
                                    usercontext.UIMode = ProviderPageMode.Registration;
                                }
                                else if (usercontext.WizContext == WizardContextMode.Invitation)
                                {
                                    usercontext.EnrollPageID = PreferredMethod.External;
                                    usercontext.UIMode = ProviderPageMode.Invitation;
                                }
                                else
                                    usercontext.UIMode = ProviderPageMode.EnrollPhone;
                                usercontext.WizPageID = 3;
                                return new AdapterPresentation(this, context, RuntimeAuthProvider.GetProvider(PreferredMethod.External).GetUIEnrollValidatedLabel(usercontext), true);
                            }
                            catch (Exception)
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollPhone;
                                usercontext.WizPageID = 4;
                                MFAUser reg = RuntimeRepository.GetMFAUser(Config, usercontext.UPN); // Rollback
                                usercontext.PhoneNumber = reg.PhoneNumber;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                            }
                        }
                        else
                        {
                            usercontext.WizPageID = 4;
                            usercontext.UIMode = ProviderPageMode.EnrollPhone;
                            MFAUser reg = RuntimeRepository.GetMFAUser(Config, usercontext.UPN); // Rollback
                            usercontext.PhoneNumber = reg.PhoneNumber;
                            return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                        }
                }
            }
            catch (Exception ex)
            {
                if (usercontext.TargetUIMode == ProviderPageMode.Registration)
                {
                    usercontext.UIMode = ProviderPageMode.Registration;
                    usercontext.TargetUIMode = ProviderPageMode.Bypass;
                }
                else if (usercontext.TargetUIMode == ProviderPageMode.Invitation)
                {
                    usercontext.UIMode = ProviderPageMode.Invitation;
                    if (Config.UserFeatures.IsMFARequired())
                        usercontext.TargetUIMode = ProviderPageMode.Locking;
                    else
                        usercontext.TargetUIMode = ProviderPageMode.Bypass;
                }
                else
                {
                    usercontext.UIMode = ProviderPageMode.EnrollPhone;
                    usercontext.WizPageID = 0;
                }
                return new AdapterPresentation(this, context, ex.Message, false);
            }
            return null;
        }

        /// <summary>
        /// TryEnrollBio implementation
        /// </summary>
        private IAdapterPresentation TryEnrollBio(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            string error = string.Empty;
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };

            usercontext.KeyChanged = false;
            usercontext.FirstChoiceMethod = PreferredMethod.Choose;
            IExternalProvider prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics);
            IWebAuthNProvider web = prov as IWebAuthNProvider;
            try
            {
                int btnclicked = Convert.ToInt32(proofData.Properties["selected"].ToString());
                bool remember = proofData.Properties.TryGetValue("remember", out object rem);
                usercontext.KeyChanged = false;

                switch (btnclicked)
                {
                    case 0: // Next Button
                        usercontext.WizPageID = 0;
                        return new AdapterPresentation(this, context);
                    case 1:  // Cancel
                        ReleaseAuthenticationData(usercontext, PreferredMethod.Biometrics);
                        if (usercontext.TargetUIMode == ProviderPageMode.Registration)
                        {
                            usercontext.UIMode = ProviderPageMode.Registration;
                        }
                        else if (usercontext.TargetUIMode == ProviderPageMode.Invitation)
                        {
                            usercontext.UIMode = ProviderPageMode.Invitation;
                        }
                        else if (usercontext.TargetUIMode == ProviderPageMode.ManageOptions)
                        {
                            usercontext.UIMode = ProviderPageMode.ManageOptions;
                        }
                        else
                            usercontext.UIMode = ProviderPageMode.SelectOptions;
                        return new AdapterPresentation(this, context);
                    case 2: // Next Button
                        try
                        {
                            usercontext.WizPageID = 1; // Goto Donut
                            if (remember)
                                usercontext.PreferredMethod = PreferredMethod.Biometrics;

                            SetBiometricProviderKeyManagementOption(usercontext, context, proofData);
                            ValidateProviderManagementUrl(usercontext, context, proofData, PreferredMethod.Biometrics);
                            GetAuthenticationData(usercontext, PreferredMethod.Biometrics);
                            if ((int)AuthenticationResponseKind.Error != PostAuthenticationRequest(usercontext, PreferredMethod.Biometrics))
                            {
                                if (!usercontext.NotificationSent)
                                {
                                    MailUtilities.SendNotificationByEmail(Config, (MFAUser)usercontext, Config.MailProvider, Resources.Culture);
                                    usercontext.NotificationSent = true;
                                }
                                return new AdapterPresentation(this, context);
                            }
                            else
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                                usercontext.WizPageID = 4;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                            }
                        }
                        catch (Exception ex)
                        {
                            usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                            usercontext.WizPageID = 0;
                            return new AdapterPresentation(this, context, ex.Message, false);
                        }
                    case 3: // Code Validation Donut
                        try
                        {
                            AuthenticationResponseKind kd = prov.GetOverrideMethod(usercontext);
                            if (prov.SetSelectedAuthenticationMethod(usercontext, kd, true))
                            {
                                string jsResponse = proofData.Properties["attestationResponse"].ToString();
                                if ((int)AuthenticationResponseKind.Error == SetAuthenticationResult(usercontext, jsResponse, out error, PreferredMethod.Biometrics))
                                {
                                    usercontext.WizPageID = 4;
                                    usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                                    return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                                }
                                else
                                {
                                    if (usercontext.WizContext == WizardContextMode.DirectWizards)
                                        RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, false);
                                    else if (usercontext.WizContext == WizardContextMode.Registration)
                                    {
                                        usercontext.EnrollPageID = PreferredMethod.Biometrics;
                                        usercontext.UIMode = ProviderPageMode.Registration;
                                    }
                                    else if (usercontext.WizContext == WizardContextMode.Invitation)
                                    {
                                        usercontext.EnrollPageID = PreferredMethod.Biometrics;
                                        usercontext.UIMode = ProviderPageMode.Invitation;
                                    }
                                    else
                                        usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                                    usercontext.WizPageID = 3;
                                    return new AdapterPresentation(this, context, RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics).GetUIEnrollValidatedLabel(usercontext), true);
                                }
                            }
                            else
                            {
                                usercontext.WizPageID = 4;
                                usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                            }
                        }
                        catch (Exception ex)
                        {
                            usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                            usercontext.WizPageID = 4;
                            return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError") + "<br><br><i>" + ex.Message + "</i>", false);
                        }
                    case 4: // Code Validation
                        try
                        {
                            if ((int)AuthenticationResponseKind.Error != SetAuthenticationResult(usercontext, string.Empty, out error, PreferredMethod.Biometrics))
                            {
                                try
                                {
                                    if (usercontext.WizContext == WizardContextMode.DirectWizards)
                                        RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, false);
                                    else if (usercontext.WizContext == WizardContextMode.Registration)
                                    {
                                        usercontext.EnrollPageID = PreferredMethod.Biometrics;
                                        usercontext.UIMode = ProviderPageMode.Registration;
                                    }
                                    else if (usercontext.WizContext == WizardContextMode.Invitation)
                                    {
                                        usercontext.EnrollPageID = PreferredMethod.Biometrics;
                                        usercontext.UIMode = ProviderPageMode.Invitation;
                                    }
                                    else
                                        usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                                    usercontext.WizPageID = 3;
                                    return new AdapterPresentation(this, context, RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics).GetUIEnrollValidatedLabel(usercontext), true);
                                }
                                catch (Exception)
                                {
                                    usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                                    usercontext.WizPageID = 4;
                                    MFAUser reg = RuntimeRepository.GetMFAUser(Config, usercontext.UPN); // Rollback
                                    return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                                }
                            }
                            else
                            {
                                usercontext.WizPageID = 4;
                                usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                                MFAUser reg = RuntimeRepository.GetMFAUser(Config, usercontext.UPN); // Rollback
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                            }
                        }
                        finally
                        {
                            ReleaseAuthenticationData(usercontext, PreferredMethod.Biometrics);
                        }
                    case 5:
                        try
                        {
                            string jserror = proofData.Properties["jserror"].ToString();
                            usercontext.WizPageID = 4;
                            usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                            return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError") + "<br><br><i>" + jserror + "</i>", false);
                        }
                        finally
                        {
                            ReleaseAuthenticationData(usercontext, PreferredMethod.Biometrics);
                        }
                    case 7:
                        usercontext.WizPageID = 5;
                        return new AdapterPresentation(this, context);
                    case 8:
                        SetBiometricProviderKeyManagementOption(usercontext, context, proofData);
                        usercontext.WizPageID = 0;
                        return new AdapterPresentation(this, context);
                }
            }
            catch (Exception ex)
            {
                ReleaseAuthenticationData(usercontext, PreferredMethod.Biometrics);
                if (usercontext.TargetUIMode == ProviderPageMode.Registration)
                {
                    usercontext.UIMode = ProviderPageMode.Registration;
                    usercontext.TargetUIMode = ProviderPageMode.Bypass;
                }
                else if (usercontext.TargetUIMode == ProviderPageMode.Invitation)
                {
                    usercontext.UIMode = ProviderPageMode.Invitation;
                    if (Config.UserFeatures.IsMFARequired())
                        usercontext.TargetUIMode = ProviderPageMode.Locking;
                    else
                        usercontext.TargetUIMode = ProviderPageMode.Bypass;
                }
                else
                {
                    usercontext.UIMode = ProviderPageMode.EnrollBiometrics;
                    usercontext.WizPageID = 0;
                }
                return new AdapterPresentation(this, context, ex.Message, false);
            }
            return null;
        }

        /// <summary>
        /// TryEnrollPinCode implementation
        /// </summary>
        private IAdapterPresentation TryEnrollPinCode(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            ResourcesLocale Resources = new ResourcesLocale(usercontext.Lcid);
            claims = new Claim[] { GetAuthMethodClaim(usercontext.SelectedMethod) };
            try
            {
                int btnclicked = Convert.ToInt32(proofData.Properties["selected"].ToString());
                usercontext.KeyChanged = false;

                switch (btnclicked)
                {
                    case 1:  // Cancel
                        if (usercontext.TargetUIMode == ProviderPageMode.Registration)
                        {
                            usercontext.UIMode = ProviderPageMode.Registration;
                        }
                        else if (usercontext.TargetUIMode == ProviderPageMode.Invitation)
                        {
                            usercontext.UIMode = ProviderPageMode.Invitation;
                        }
                        else if (usercontext.TargetUIMode == ProviderPageMode.ManageOptions)
                        {
                            usercontext.UIMode = ProviderPageMode.ManageOptions;
                        }
                        else
                            usercontext.UIMode = ProviderPageMode.SelectOptions;
                        return new AdapterPresentation(this, context);
                    case 2: // Next Button
                        try
                        {
                            if (!usercontext.NotificationSent)
                            {
                                MailUtilities.SendNotificationByEmail(Config, (MFAUser)usercontext, Config.MailProvider, Resources.Culture);
                                usercontext.NotificationSent = true;
                            }
                            usercontext.WizPageID = 2;
                            if (usercontext.PinCode <= 0)
                                usercontext.PinCode = Config.DefaultPin;
                            ValidateUserPin(usercontext, context, proofData, Resources, true);
                            return new AdapterPresentation(this, context);
                        }
                        catch (Exception ex)
                        {
                            usercontext.UIMode = ProviderPageMode.EnrollPin;
                            usercontext.WizPageID = 0;
                            return new AdapterPresentation(this, context, ex.Message, false);
                        }
                    case 3: // Code verification
                        try
                        {
                            int totp = Convert.ToInt32(proofData.Properties["pincode"]);                           
                            if (usercontext.PinCode != totp)
                            {
                                usercontext.WizPageID = 4;
                                usercontext.UIMode = ProviderPageMode.EnrollPin;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                            }
                            else
                            {
                                usercontext.WizPageID = 3;
                                usercontext.UIMode = ProviderPageMode.EnrollPin;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPSuccess"), true);
                            }
                        }
                        catch (Exception ex)
                        {
                            return new AdapterPresentation(this, context, ex.Message, false);
                        }
                    case 4: // Code validation
                        int totp2 = Convert.ToInt32(proofData.Properties["pincode"]);                           
                        if (usercontext.PinCode == totp2)
                        {
                            try
                            {
                               // ValidatePINCode(totp2.ToString(), Resources, true);
                                ValidateUserPin(usercontext, context, proofData, Resources, true);
                                if (usercontext.WizContext == WizardContextMode.DirectWizards)
                                    RuntimeRepository.SetMFAUser(Config, (MFAUser)usercontext, false);
                                else if (usercontext.WizContext == WizardContextMode.Registration)
                                {
                                    usercontext.EnrollPageID = PreferredMethod.Pin;
                                    usercontext.UIMode = ProviderPageMode.Registration;
                                }
                                else if (usercontext.WizContext == WizardContextMode.Invitation)
                                {
                                    usercontext.EnrollPageID = PreferredMethod.Pin;
                                    usercontext.UIMode = ProviderPageMode.Invitation;
                                }
                                else
                                    usercontext.UIMode = ProviderPageMode.EnrollPin;
                                usercontext.WizPageID = 3;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPSuccess"), true);
                            }
                            catch (Exception)
                            {
                                usercontext.UIMode = ProviderPageMode.EnrollPin;
                                MFAUser reg = RuntimeRepository.GetMFAUser(Config, usercontext.UPN); // Rollback
                                usercontext.PinCode = reg.PIN;
                                return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                            }
                        }
                        else
                        {
                            usercontext.WizPageID = 4;
                            usercontext.UIMode = ProviderPageMode.EnrollPin;
                            MFAUser reg = RuntimeRepository.GetMFAUser(Config, usercontext.UPN); // Rollback
                            usercontext.PinCode = reg.PIN;
                            return new  AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPError"), false);
                        }
                }
            }
            catch (Exception ex)
            {
                if (usercontext.TargetUIMode == ProviderPageMode.Registration)
                {
                    usercontext.UIMode = ProviderPageMode.Registration;
                    usercontext.TargetUIMode = ProviderPageMode.Bypass;
                }
                else if (usercontext.TargetUIMode == ProviderPageMode.Invitation)
                {
                    usercontext.UIMode = ProviderPageMode.Invitation;
                    if (Config.UserFeatures.IsMFARequired())
                        usercontext.TargetUIMode = ProviderPageMode.Locking;
                    else
                        usercontext.TargetUIMode = ProviderPageMode.Bypass;
                }
                else
                {
                    usercontext.UIMode = ProviderPageMode.EnrollPin;
                    usercontext.WizPageID = 1;
                }
                return new AdapterPresentation(this, context, ex.Message, false);
            }
            return null;
        }
#endregion

        #region private methods
        /// <summary>
        /// ValidateUserOptions method implementation
        /// </summary>
        private void ValidateUserOptions(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, ResourcesLocale Resources, bool checkempty = false)
        {
            ValidateUserKey(usercontext, context, proofData, Resources, checkempty);
            ValidateUserEmail(usercontext, context, proofData, Resources, checkempty);
            ValidateUserPhone(usercontext, context, proofData, Resources, checkempty);
            ValidateUserPin(usercontext, context, proofData, Resources, checkempty);
        }

        /// <summary>
        /// ValidateUserKey method implementation
        /// </summary>
        private void ValidateUserKey(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, ResourcesLocale Resources, bool checkempty = false)
        {
            IExternalProvider prov = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Code);
            if ((prov.Enabled) && (prov.IsRequired) && (prov.IsUIElementRequired(usercontext, RequiredMethodElements.KeyParameterRequired)))
            {
                string displaykey = string.Empty;

                if (usercontext.KeyStatus != SecretKeyStatus.Success)
                    throw new Exception(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidKey"));
            }
        }

        /// <summary>
        /// ValidateUserEmail method implementation
        /// </summary>
        private void ValidateUserEmail(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, ResourcesLocale Resources, bool checkempty = false)
        {
            IExternalProvider prov = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.Email);
            if ((prov.Enabled) && (prov.IsUIElementRequired(usercontext, RequiredMethodElements.EmailParameterRequired)))
            {
                string email = string.Empty;
                if (proofData.Properties.ContainsKey("email"))
                {
                    email = proofData.Properties["email"].ToString();
                    if (string.IsNullOrEmpty(email))
                        email = usercontext.MailAddress;
                }
                else
                    email = usercontext.MailAddress;
                if ((prov.Enabled) && (prov.IsRequired))
                    ValidateEmail(email, Resources, checkempty);
                usercontext.MailAddress = email;
            }
        }

        /// <summary>
        /// ValidateUserPhone method implementation
        /// </summary>
        private void ValidateUserPhone(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, ResourcesLocale Resources, bool checkempty = false)
        {
            string phone = string.Empty;
            IExternalProvider prov = RuntimeAuthProvider.GetProviderInstance(PreferredMethod.External);
            if ((prov.Enabled) && (prov.IsUIElementRequired(usercontext, RequiredMethodElements.PhoneParameterRequired)))
            {
                if (proofData.Properties.ContainsKey("phone"))
                {
                    phone = proofData.Properties["phone"].ToString();
                    if (string.IsNullOrEmpty(phone))
                        phone = usercontext.PhoneNumber;
                }
                else
                    phone = usercontext.PhoneNumber;
                if ((prov.Enabled) && (prov.IsRequired))
                    ValidatePhoneNumber(phone, Resources, checkempty);
                usercontext.PhoneNumber = phone;
            }
            if ((prov.Enabled) && (prov.IsUIElementRequired(usercontext, RequiredMethodElements.ExternalParameterRequired)))
            {
                if (proofData.Properties.ContainsKey("phone"))
                {
                    phone = proofData.Properties["phone"].ToString();
                    if (string.IsNullOrEmpty(phone))
                        if (prov.Enabled && prov.IsRequired)
                            throw new ArgumentException("Invalid value !");
                }
                usercontext.PhoneNumber = phone;
            }
        }

        /// <summary>
        /// ValidateUserPin method implementation
        /// </summary>
        private void ValidateUserPin(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, ResourcesLocale Resources, bool checkempty = false)
        {
            if (RuntimeAuthProvider.IsPinCodeRequired(usercontext))
            {
                string strpin = string.Empty;
                if (proofData.Properties.ContainsKey("pincode"))
                {
                    strpin = proofData.Properties["pincode"].ToString();
                    if (string.IsNullOrEmpty(strpin))
                        strpin = usercontext.PinCode.ToString();
                }
                else
                    strpin = usercontext.PinCode.ToString();
                ValidatePINCode(strpin, Resources, checkempty);
                usercontext.PinCode = Convert.ToInt32(strpin);
            }
        }

        /// <summary>
        /// ValidateAzureUrl method implementation
        /// </summary>
        private void ValidateProviderManagementUrl(AuthenticationContext ctx, IAuthenticationContext context, IProofData proofData, PreferredMethod pageid)
        {
            if (proofData.Properties.ContainsKey("manageaccount"))
            {
                bool mgtaccount = proofData.Properties.TryGetValue("manageaccount", out object rem);
                switch (pageid)
                {
                    case PreferredMethod.Code: // TOTP
                        ctx.AccountManagementUrl = RuntimeAuthProvider.GetProvider(PreferredMethod.Code).GetAccountManagementUrl(ctx);
                        break;
                    case PreferredMethod.Email: // Email IsMethodElementRequired
                        ctx.AccountManagementUrl = RuntimeAuthProvider.GetProvider(PreferredMethod.Email).GetAccountManagementUrl(ctx);
                        break;
                    case PreferredMethod.External: // external API
                        ctx.AccountManagementUrl = RuntimeAuthProvider.GetProvider(PreferredMethod.External).GetAccountManagementUrl(ctx);
                        break;
                    case PreferredMethod.Azure: // Azure MFA
                        ctx.AccountManagementUrl = RuntimeAuthProvider.GetProvider(PreferredMethod.Azure).GetAccountManagementUrl(ctx);
                        break;
                }
            }
            else
                ctx.AccountManagementUrl = null;
            return;
        }

        /// <summary>
        /// SetProviderOverrideOption method implementation
        /// </summary>
        private void SetProviderOverrideOption(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, PreferredMethod method)
        {

            if (proofData.Properties.ContainsKey("optionitem"))
            {
                bool chk = proofData.Properties.TryGetValue("optionitem", out object rem);
                if (chk)
                {
                    AuthenticationResponseKind kind = (AuthenticationResponseKind)Enum.Parse(typeof(AuthenticationResponseKind), rem.ToString());
                    switch (method)
                    {
                        case PreferredMethod.Code: // Otp
                            RuntimeAuthProvider.GetProvider(PreferredMethod.Code).SetOverrideMethod(usercontext, kind);
                            break;
                        case PreferredMethod.Email: // Email 
                            RuntimeAuthProvider.GetProvider(PreferredMethod.Email).SetOverrideMethod(usercontext, kind);
                            break;
                        case PreferredMethod.External: // External API
                            switch (kind)
                            {
                                case AuthenticationResponseKind.SmsOTP:
                                case AuthenticationResponseKind.SmsOneWayOTP:
                                case AuthenticationResponseKind.SmsOneWayOTPplusPin:
                                    kind = AuthenticationResponseKind.SmsOneWayOTP;
                                    break;
                                case AuthenticationResponseKind.SmsTwoWayOTP:
                                case AuthenticationResponseKind.SmsTwoWayOTPplusPin:
                                    kind = AuthenticationResponseKind.SmsTwoWayOTP;
                                    break;
                                case AuthenticationResponseKind.Sample1:
                                    kind = AuthenticationResponseKind.Sample1;
                                    break;
                                case AuthenticationResponseKind.Sample2:
                                    kind = AuthenticationResponseKind.Sample2;
                                    break;
                                case AuthenticationResponseKind.Sample3:
                                    kind = AuthenticationResponseKind.Sample3;
                                    break;
                                case AuthenticationResponseKind.Sample1Async:
                                    kind = AuthenticationResponseKind.Sample1Async;
                                    break;
                                case AuthenticationResponseKind.Sample2Async:
                                    kind = AuthenticationResponseKind.Sample2Async;
                                    break;
                                case AuthenticationResponseKind.Sample3Async:
                                    kind = AuthenticationResponseKind.Sample3Async;
                                    break;
                                default:
                                    if (RuntimeAuthProvider.GetProvider(PreferredMethod.External).IsTwoWayByDefault)
                                        kind = AuthenticationResponseKind.SmsTwoWayOTP;
                                    else
                                        kind = AuthenticationResponseKind.SmsOneWayOTP;
                                    break;
                            }
                            RuntimeAuthProvider.GetProvider(PreferredMethod.External).SetOverrideMethod(usercontext, kind);
                            break;
                        case PreferredMethod.Azure: // Azure MFA
                            RuntimeAuthProvider.GetProvider(PreferredMethod.Azure).SetOverrideMethod(usercontext, kind);
                            break;
                    }
                }
            }
            return;
        }

        /// <summary>
        /// SetBiometricProviderKeyManagementOption method implementation
        /// </summary>
        private void SetBiometricProviderKeyManagementOption(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData)
        {

            if (proofData.Properties.ContainsKey("optionitem"))
            {
                bool chk = proofData.Properties.TryGetValue("optionitem", out object optionitem);
                if (chk)
                {
                    if (!optionitem.Equals(Guid.Empty.ToString()))
                    {
                        IExternalProvider provider = RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics);
                        IWebAuthNProvider web = provider as IWebAuthNProvider;
                        web.RemoveUserStoredCredentials(usercontext, optionitem.ToString());
                    }
                }
            }
        }
        #endregion

        #region Providers private methods
        /// <summary>
        /// GetAuthenticationContextRequest method implmentation
        /// </summary>
        private ProviderPageMode GetAuthenticationContextRequest(AuthenticationContext usercontext)
        {
            IExternalProvider provider = RuntimeAuthProvider.GetAuthenticationProvider(Config, usercontext);
            if ((provider != null) && (provider.Enabled))
            {
                provider.GetAuthenticationContext(usercontext);                
                usercontext.UIMode = ProviderPageMode.SendAuthRequest;              
            }
            else
                usercontext.UIMode = ProviderPageMode.ChooseMethod;
            return usercontext.UIMode;
        }

        /// <summary>
        /// PostAuthenticationRequest method implementation
        /// </summary>
        private int PostAuthenticationRequest(AuthenticationContext usercontext, PreferredMethod method = PreferredMethod.None)
        {
            usercontext.CurrentRetries++;
            try
            {
                IExternalProvider provider = null;
                if (method == PreferredMethod.None)
                    provider = RuntimeAuthProvider.GetAuthenticationProvider(Config, usercontext);
                else
                    provider = RuntimeAuthProvider.GetProvider(method);

                if ((provider != null) && (provider.Enabled))
                    return provider.PostAuthenticationRequest(usercontext);
                else
                    return (int)AuthenticationResponseKind.Error;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("PostAuthenticationRequest Error {0} \r\n {1} \r\n {2}", usercontext.UPN, ex.Message, ex.StackTrace), EventLogEntryType.Error, 800);
                return (int)AuthenticationResponseKind.Error;
            }
        }

        /// <summary>
        /// SetAuthenticationResult method implementation
        /// </summary>
        public int SetAuthenticationResult(AuthenticationContext usercontext, string credentials, PreferredMethod method = PreferredMethod.None)
        {
            try
            {
                IExternalProvider provider = null;
                if (method == PreferredMethod.None)
                    provider = RuntimeAuthProvider.GetAuthenticationProvider(Config, usercontext);
                else
                    provider = RuntimeAuthProvider.GetProvider(method);
                if ((provider != null) && (provider.Enabled))
                    return provider.SetAuthenticationResult(usercontext, credentials);
                else
                    return (int)AuthenticationResponseKind.Error;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("SetAuthenticationResult Error {0} \r\n {1} \r\n {2}", usercontext.UPN, ex.Message, ex.StackTrace), EventLogEntryType.Error, 800);
                return (int)AuthenticationResponseKind.Error;
            }
        }

        /// <summary>
        /// SetAuthenticationResult method implementation
        /// </summary>
        public int SetAuthenticationResult(AuthenticationContext usercontext, string credentials, out string error, PreferredMethod method = PreferredMethod.None)
        {
            try
            {
                IExternalProvider provider = null;
                if (method == PreferredMethod.None)
                    provider = RuntimeAuthProvider.GetAuthenticationProvider(Config, usercontext);
                else
                    provider = RuntimeAuthProvider.GetProvider(method);
                if ((provider != null) && (provider.Enabled))
                    return provider.SetAuthenticationResult(usercontext, credentials, out error);
                else
                {
                    error = "Provider not availlable !";
                    return (int)AuthenticationResponseKind.Error;
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("SetAuthenticationResult Error {0} \r\n {1} \r\n {2}", usercontext.UPN, ex.Message, ex.StackTrace), EventLogEntryType.Error, 800);
                error = ex.Message;
                return (int)AuthenticationResponseKind.Error;
            }
        }

        /// <summary>
        /// GetAuthenticationData method implementation
        /// </summary>
        public void GetAuthenticationData(AuthenticationContext ctx, PreferredMethod method = PreferredMethod.None)
        {
            IExternalProvider provider = null;
            if (method == PreferredMethod.None)
                provider = RuntimeAuthProvider.GetAuthenticationProvider(Config, ctx);
            else
                provider = RuntimeAuthProvider.GetProvider(method);

            if ((provider != null) && (provider.Enabled))
                provider.GetAuthenticationData(ctx);
        }

        /// <summary>
        /// ReleaseAuthenticationData method implementation
        /// </summary>
        public void ReleaseAuthenticationData(AuthenticationContext ctx, PreferredMethod method = PreferredMethod.None)
        {
            IExternalProvider provider = null;
            if (method == PreferredMethod.None)
                provider = RuntimeAuthProvider.GetAuthenticationProvider(Config, ctx);
            else
                provider = RuntimeAuthProvider.GetProvider(method);

            if ((provider != null) && (provider.Enabled))
                provider.ReleaseAuthenticationData(ctx);
        }
        #endregion

        #region Administrative Provider methods
        /// <summary>
        /// GetAuthenticationContextRequest method implmentation
        /// </summary>
        private ProviderPageMode GetInscriptionContextRequest(AuthenticationContext usercontext)
        {
            IExternalAdminProvider _provider = RuntimeAuthProvider.GetAdministrativeProvider(Config);
            if (_provider != null)
            {
                _provider.GetInscriptionContext(usercontext);
                usercontext.UIMode = ProviderPageMode.SendAdministrativeRequest;
            }
            else
                usercontext.UIMode = ProviderPageMode.Locking;
            return usercontext.UIMode;
        }

        /// <summary>
        /// PostInscriptionRequest method implementation
        /// </summary>
        private int PostInscriptionRequest(AuthenticationContext usercontext, MFAUser registration)
        {
            try
            {
                IExternalAdminProvider _provider = RuntimeAuthProvider.GetAdministrativeProvider(Config);
                if (_provider != null)
                    return _provider.PostInscriptionRequest(usercontext);
                else
                    return (int)AuthenticationResponseKind.Error;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("PostInscriptionRequest Error {0} \r\n {1} \r\n {2}", usercontext.UPN, ex.Message, ex.StackTrace), EventLogEntryType.Error, 800);
                return (int)AuthenticationResponseKind.Error;
            }
        }

        /// <summary>
        /// SetInscriptionResult method implementation
        /// </summary>
        public int SetInscriptionResult(AuthenticationContext usercontext, MFAUser registration)
        {
            try
            {
                IExternalAdminProvider _provider = RuntimeAuthProvider.GetAdministrativeProvider(Config);
                if (_provider != null)
                    return _provider.SetInscriptionResult(usercontext);
                else
                    return (int)AuthenticationResponseKind.Error;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("SetInscriptionResult Error {0} \r\n {1} \r\n {2}", usercontext.UPN, ex.Message, ex.StackTrace), EventLogEntryType.Error, 800);
                return (int)AuthenticationResponseKind.Error;
            }
        }
        #endregion

        #region Secret Key Provider
        /// <summary>
        /// GetAuthenticationContextRequest method implmentation
        /// </summary>
        private ProviderPageMode GetSecretKeyContextRequest(AuthenticationContext usercontext)
        {
            IExternalAdminProvider _provider = RuntimeAuthProvider.GetAdministrativeProvider(Config);
            if (_provider != null)
            {
                _provider.GetSecretKeyContext(usercontext);
                usercontext.UIMode = ProviderPageMode.SendKeyRequest;
            }
            else
                usercontext.UIMode = ProviderPageMode.Locking;
            return usercontext.UIMode;
        }

        /// <summary>
        /// PostSecretKeyRequest method implementation
        /// </summary>
        private int PostSecretKeyRequest(AuthenticationContext usercontext)
        {
            try
            {
                IExternalAdminProvider _provider = RuntimeAuthProvider.GetAdministrativeProvider(Config);
                if (_provider != null)
                    return _provider.PostSecretKeyRequest(usercontext);
                else
                    return (int)AuthenticationResponseKind.Error;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("PostSecretKeyRequest Error {0} \r\n {1} \r\n {2}", usercontext.UPN, ex.Message, ex.StackTrace), EventLogEntryType.Error, 800);
                return (int)AuthenticationResponseKind.Error;
            }
        }

        /// <summary>
        /// SetSecretKeyResult method implementation
        /// </summary>
        public int SetSecretKeyResult(AuthenticationContext usercontext)
        {
            try
            {
                IExternalAdminProvider _provider = RuntimeAuthProvider.GetAdministrativeProvider(Config);
                if (_provider != null)
                    return _provider.SetSecretKeyResult(usercontext);
                else
                    return (int)AuthenticationResponseKind.Error;
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("SetSecretKeyResult Error {0} \r\n {1} \r\n {2}", usercontext.UPN, ex.Message, ex.StackTrace), EventLogEntryType.Error, 800);
                return (int)AuthenticationResponseKind.Error;
            }
        }
        #endregion

        #region Other Private Methods
        /// <summary>
        /// GetAuthMethodClaim method implementation
        /// </summary>
        private Claim GetAuthMethodClaim(AuthenticationResponseKind notificationStatus)
        {
            switch (notificationStatus)
            {
                case AuthenticationResponseKind.PhoneAppOTP:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");
                case AuthenticationResponseKind.EmailOTP:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/email");
                case AuthenticationResponseKind.SmsOTP:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/sms");
                case AuthenticationResponseKind.VoiceBiometric:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/voicebiometrics");
                case AuthenticationResponseKind.PhoneAppNotification:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/phoneappnotification");
                case AuthenticationResponseKind.PhoneAppConfirmation:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/phoneconfirmation");
                case AuthenticationResponseKind.Kba:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/kba");
                case AuthenticationResponseKind.FaceID:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/facebiometrics");
                case AuthenticationResponseKind.WindowsHello:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/windowshello");
                case AuthenticationResponseKind.FIDO:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/fido");
                case AuthenticationResponseKind.Biometrics:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/webauthN");


                // Azure
                case AuthenticationResponseKind.SmsOneWayOTPplusPin:
                case AuthenticationResponseKind.SmsOneWayOTP:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/smsotp");
                case AuthenticationResponseKind.SmsTwoWayOTPplusPin:
                case AuthenticationResponseKind.SmsTwoWayOTP:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/smsreply");
                case AuthenticationResponseKind.VoiceTwoWayMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayMobile:
                case AuthenticationResponseKind.VoiceTwoWayOfficePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayOffice:
                case AuthenticationResponseKind.VoiceTwoWayAlternateMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayAlternateMobile:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/phoneconfirmation");
                // Quiz for fun
                case AuthenticationResponseKind.Sample1: 
                case AuthenticationResponseKind.Sample2:
                case AuthenticationResponseKind.Sample3:
                case AuthenticationResponseKind.Sample1Async:
                case AuthenticationResponseKind.Sample2Async:
                case AuthenticationResponseKind.Sample3Async:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");
                default:
                    return new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/none");
            }
        }

        /// <summary>
        /// ValidateEmail method implementation
        /// </summary>
        private void ValidateEmail(string email, ResourcesLocale Resources, bool checkempty = false)
        {
            try
            {
                if (!Utilities.ChedkEmailValidity(email, Config.MailProvider.AllowedDomains, Config.MailProvider.BlockedDomains, checkempty))
                    throw new Exception(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorEmailException"));
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
        /// ValidateEmail method implementation
        /// </summary>
        private void ValidatePhoneNumber(string phone, ResourcesLocale Resources, bool checkempty = false)
        {
            try
            {
                if (!Utilities.ValidatePhoneNumber(phone, checkempty))
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
        /// ValidatePINCode method implementation
        /// </summary>
        private void ValidatePINCode(string strpin, ResourcesLocale Resources, bool checkempty = false)
        {
            try
            {
                if (checkempty)
                {
                    if (string.IsNullOrEmpty(strpin))
                        throw new Exception(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorPinValue"));
                    if (strpin.Length > 9)
                        throw new Exception(string.Format(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorPinLength"), 9));
                    if (strpin.Length < 4)
                        throw new Exception(string.Format(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorPinLength"), 4));
                    if (strpin.Length != Config.PinLength)
                        throw new Exception(string.Format(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorPinLength"), Config.PinLength));
                }
                if (Convert.ToInt32(strpin) < 0)
                   throw new Exception(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorPinValue"));
            }
            catch (Exception ex)
            {
                Log.WriteEntry(ex.Message, EventLogEntryType.Error, 800);
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
            if ((message.Operation == (byte)NotificationsKind.ConfigurationReload) || (message.Operation == (byte)NotificationsKind.ConfigurationCreated) || (message.Operation == (byte)NotificationsKind.ConfigurationDeleted))
            {
                Trace.TraceInformation("AuthenticationProvider:Configuration changed !");
                _config = CFGUtilities.ReadConfiguration();
                string computer = message.Text;
                if (string.IsNullOrEmpty(computer))
                    computer = Environment.MachineName;
                else
                    computer = computer.Replace("$", "");
                Trace.TraceInformation("AuthenticationProvider:Configuration loaded !");
                ResourcesLocale Resources = new ResourcesLocale(CultureInfo.InstalledUICulture.LCID);
                switch (message.Operation)
                {
                    case (byte)NotificationsKind.ConfigurationReload:
                    default:
                        Log.WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Informations, "InfosConfigurationReloaded"), computer), EventLogEntryType.Warning, 9999);
                        break;
                    case (byte)NotificationsKind.ConfigurationCreated:
                        Log.WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Informations, "InfosConfigurationCacheCreated"), computer), EventLogEntryType.Warning, 9999);
                        break;
                    case (byte)NotificationsKind.ConfigurationDeleted:
                        Log.WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Informations, "InfosConfigurationCacheDeleted"), computer), EventLogEntryType.Warning, 9999);
                        break;
                }
            }
        }


        /// <summary>
        /// PatchUserContextWithSelectedMethod method implementation
        /// </summary>
        private void PatchUserContextWithSelectedMethod(AuthenticationContext usercontext)
        {
            IExternalProvider prov = RuntimeAuthProvider.GetProvider(usercontext.PreferredMethod);
            if (prov != null)
            {
                List<AvailableAuthenticationMethod> lst = prov.GetAuthenticationMethods(usercontext);
                if (lst.Count > 0)
                {
                    AvailableAuthenticationMethod itm = lst[0];
                    usercontext.IsRemote = itm.IsRemote;
                    usercontext.IsTwoWay = itm.IsTwoWay;
                    usercontext.IsSendBack = itm.IsSendBack;
                    usercontext.SelectedMethod = itm.Method;
                }
            }
        }

        /// <summary>
        /// HookOptionParameter method implementation
        /// when MFA is disabled, allow the user to access his configuration
        /// </summary>
        private bool HookOptionParameter(HttpListenerRequest request)
        {
            Uri uri = new Uri(request.Url.AbsoluteUri);
            return uri.AbsoluteUri.Contains("mfaopts");
        }

        /// <summary>
        /// CheckOptionsCookie method implmentation
        /// </summary>
        private void CheckOptionsCookie(AuthenticationContext usercontext, HttpListenerRequest request)
        {
            if (usercontext.ShowOptions)
                return;
            var cook = request.Cookies["showoptions"];
            if (cook != null)
            {
                if (cook.Value == "1")
                    usercontext.ShowOptions = true;
                else
                    usercontext.ShowOptions = false;
            }
            else
                usercontext.ShowOptions = false;
        }

        /// <summary>
        /// HasAccessToOptions method
        /// </summary>
        internal bool HasAccessToOptions(IExternalProvider prov)
        {
            if (prov == null)
                return false;
            if (!prov.Enabled)
                return false;
            if (!Config.UserFeatures.CanAccessOptions())
                return false;
            if (Config.UserFeatures.CanManageOptions() || Config.UserFeatures.CanManagePassword())
                return true;
            if (Config.UserFeatures.CanEnrollDevices() && (prov.WizardEnabled))
               return true;
            return false;
        }

        /// <summary>
        /// HasStrictAccessToOptions method
        /// </summary>
        internal bool HasStrictAccessToOptions(IExternalProvider prov)
        {
            if (prov == null)
                return false;
            if (!prov.Enabled)
                return false;
            if (!Config.UserFeatures.CanAccessOptions())
                return false;
            if (Config.UserFeatures.CanEnrollDevices() && (prov.WizardEnabled))
               return true;
            return false;
        }

        /// <summary>
        /// HasAccessToPinCode method
        /// </summary>
        internal bool HasAccessToPinCode(IExternalProvider prov)
        {
            if (prov == null)
                return false;
            if (!prov.Enabled)
                return false;
            if (!prov.PinRequired)
                return false;
            return true;
        }

        /// <summary>
        /// KeepMySelectedOptionOn method
        /// </summary>
        internal bool KeepMySelectedOptionOn()
        {
            return Config.KeepMySelectedOptionOn;
        }
        #endregion
    }
}
