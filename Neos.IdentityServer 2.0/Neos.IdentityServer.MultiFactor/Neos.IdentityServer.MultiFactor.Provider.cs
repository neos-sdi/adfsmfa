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

        /// <summary>
        /// Constructor override
        /// </summary>
        public AuthenticationProvider()
        {

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
                            case RegistrationPreferredMethod.Code:
                                usercontext.UIMode = ProviderPageMode.Identification;
                                SendNotification(usercontext, Resources);
                                break;
                            case RegistrationPreferredMethod.Email:
                                usercontext.UIMode = ProviderPageMode.CodeRequest;
                                SendNotification(usercontext, Resources);
                                break;
                            case RegistrationPreferredMethod.Phone:
                                usercontext.UIMode = ProviderPageMode.CodeRequest;
                                SendNotification(usercontext, Resources);
                                break;
                        }
                        result = new AdapterPresentation(this, context);
                        break;
                    case ProviderPageMode.Locking:  // Only for locking mode
                        if (usercontext.TargetUIMode == ProviderPageMode.DefinitiveError)
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNoAccess"), ProviderPageMode.DefinitiveError);
                        else if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AdministrativeMode))
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNoAccess"), ProviderPageMode.DefinitiveError);
                        else if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowDisabled) || Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowUnRegistered))
                        {
                            if (Config.AdvertisingDays.OnFire) 
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
                        if ((HookOptionParameter(request)) && (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowManageOptions)))
                            usercontext.UIMode = ProviderPageMode.Registration;
                        result = new AdapterPresentation(this, context);
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                Log. WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAuthenticating"), ex.Message), EventLogEntryType.Error, 802);
                throw new ExternalAuthenticationException(ex.Message, context);
            }
        }
		/// <summary>
        /// IsAvailableForUser method implementation
		/// </summary>
        public bool IsAvailableForUser(System.Security.Claims.Claim identityClaim, IAuthenticationContext context)
        {
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            try
            {
                string upn = identityClaim.Value;
                Registration reg = RepositoryService.GetUserRegistration(upn, Config);
                if (reg != null) // User Is Registered
                {
                    AuthenticationContext usercontext = new AuthenticationContext(context, reg);
                    if (usercontext.Enabled)
                    {
                        if (usercontext.KeyStatus == SecretKeyStatus.NoKey)
                        {
                            if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowManageOptions))
                            {
                                KeysManager.NewKey(usercontext.UPN);
                                usercontext.KeyStatus = SecretKeyStatus.Success;
                                usercontext.KeyChanged = true;
                                usercontext.UIMode = ProviderPageMode.Registration;
                            }
                        }
                        else if (usercontext.PreferredMethod == RegistrationPreferredMethod.Choose)
                            usercontext.UIMode = ProviderPageMode.ChooseMethod;
                        else
                            usercontext.UIMode = ProviderPageMode.Identification;
                        return true;
                    }
                    else // Not enabled
                    {
                        if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AdministrativeMode))
                            usercontext.UIMode = ProviderPageMode.Locking;
                        else if (Config.UserFeatures.HasFlag(UserFeaturesOptions.BypassDisabled))
                            usercontext.UIMode = ProviderPageMode.Bypass;
                        else if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowManageOptions))
                        {
                            if (usercontext.KeyStatus == SecretKeyStatus.NoKey)
                            {
                                KeysManager.NewKey(usercontext.UPN);
                                usercontext.KeyStatus = SecretKeyStatus.Success;
                                usercontext.KeyChanged = true;
                            }
                            usercontext.Enabled = true;
                            usercontext.UIMode = ProviderPageMode.Registration;
                        } 
                        else if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowProvideInformations))
                        {
                            if (usercontext.KeyStatus == SecretKeyStatus.NoKey)
                            {
                                KeysManager.NewKey(usercontext.UPN);
                                usercontext.KeyStatus = SecretKeyStatus.Success;
                                usercontext.KeyChanged = true;
                            }
                            usercontext.Enabled = false;
                            usercontext.UIMode = ProviderPageMode.Locking;
                            usercontext.TargetUIMode = ProviderPageMode.None;
                        }
                        else if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowDisabled))
                            usercontext.UIMode = ProviderPageMode.Bypass;
                        else
                        {
                            usercontext.TargetUIMode = ProviderPageMode.DefinitiveError;
                            usercontext.UIMode = ProviderPageMode.Locking;
                        }
                        return true;
                    }
                }
                else //Not registered
                {
                    AuthenticationContext usercontext = new AuthenticationContext(context);
                    usercontext.UPN = upn;
                    if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AdministrativeMode))
                        usercontext.UIMode = ProviderPageMode.Locking;
                    else if (Config.UserFeatures.HasFlag(UserFeaturesOptions.BypassUnRegistered))
                        usercontext.UIMode = ProviderPageMode.Bypass;
                    else if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowManageOptions))
                    {
                        KeysManager.NewKey(usercontext.UPN);
                        usercontext.KeyStatus = SecretKeyStatus.Success;
                        usercontext.KeyChanged = true;
                        usercontext.Enabled = true;
                        usercontext.UIMode = ProviderPageMode.Registration;
                    } 
                    else if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowProvideInformations))
                    {
                        KeysManager.NewKey(usercontext.UPN);
                        usercontext.KeyStatus = SecretKeyStatus.Success;
                        usercontext.KeyChanged = true;
                        usercontext.PreferredMethod = RegistrationPreferredMethod.Choose;
                        usercontext.Enabled = false;
                        usercontext.UIMode = ProviderPageMode.Locking;
                        usercontext.TargetUIMode = ProviderPageMode.None;
                    }
                    else if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowUnRegistered))
                        usercontext.UIMode = ProviderPageMode.Bypass;
                    else
                    {
                        usercontext.TargetUIMode = ProviderPageMode.DefinitiveError;
                        usercontext.UIMode = ProviderPageMode.Locking;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
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
                                 _config.MailEnabled = true;  // always let an active option eg : email in this case
                             KeysManager.Initialize(_config);
                         }

                         RepositoryService.MailslotServer.MailSlotMessageArrived += this.OnMessageArrived;
                         RepositoryService.MailslotServer.AllowedMachines.Clear();
                         foreach(ADFSServerHost svr in Config.Hosts.ADFSFarm.Servers)
                         {
                             RepositoryService.MailslotServer.AllowedMachines.Add(svr.MachineName);
                         }
                         RepositoryService.MailslotServer.Start();
                     }
                     catch (Exception EX)
                     {
                         Log.WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorLoadingConfigurationFile"), EX.Message), EventLogEntryType.Error, 900);
                         throw new ExternalAuthenticationException();
                     }
                 }
             }
             else
             {
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
                case ProviderPageMode.CodeRequest:
                    result = TryCodeRequest(usercontext, context, proofData, request, out claims);
                    break;
                case ProviderPageMode.InvitationRequest:
                    result = TryInvitationRequest(usercontext, context, proofData, request, out claims);
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
                    Notification notif = RepositoryService.CheckNotification((Registration)usercontext, Config);
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
                                        case RegistrationPreferredMethod.Code:
                                            claim1 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpmode", "totp");
                                            break;
                                        case RegistrationPreferredMethod.Face:
                                            claim1 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpmode", "faceid");
                                            break;
                                    }
                                    TimeSpan duration = notif.CheckDate.Value.Subtract(notif.CreationDate);  // We Want only the difference no need UTC
                                    System.Security.Claims.Claim claim2 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpduration", duration.ToString());
                                    claims = new System.Security.Claims.Claim[] { claim0, claim1, claim2 };
                                }
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
                        usercontext.UIMode = ProviderPageMode.ShowQRCode;
                        result = new AdapterPresentation(this, context);
                    }
                    else if (lnk == 2)
                    {
                        RepositoryService.SetUserRegistration((Registration)usercontext, Config);
                        KeysManager.NewKey(usercontext.UPN);
                        usercontext.KeyStatus = SecretKeyStatus.Success;
                        usercontext.KeyChanged = true;
                        usercontext.UIMode = ProviderPageMode.Registration;
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Informations, "InfosConfigurationModified"));
                    }
                    else if (lnk==3)
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

                        usercontext.UIMode = ProviderPageMode.SendKeyRequest;
                        SendKeyToUser(usercontext, Resources, NotificationStatus.ResponseEmailForKeyRegistration);
                        result = new AdapterPresentation(this, context);
                    }
                }
                if (lnk == 0)
                {
                    int select = Convert.ToInt32(proofData.Properties["selectopt"].ToString());
                    usercontext.PreferredMethod = (RegistrationPreferredMethod)select;
                    usercontext.UIMode = ProviderPageMode.SelectOptions;
                    int btnclicked = Convert.ToInt32(proofData.Properties["btnclicked"].ToString());
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

                        object opt = null;
                        bool options = proofData.Properties.TryGetValue("disablemfa", out opt);

                        usercontext.Enabled = !options;
                        RepositoryService.SetUserRegistration((Registration)usercontext, Config);
                        usercontext.UIMode = ProviderPageMode.SelectOptions;
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Informations, "InfosConfigurationModified"));
                    }
                    else
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
        /// TryInvitation method implementation
        /// </summary>
        private IAdapterPresentation TryInvitation(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Invitation
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            claims = null;
            IAdapterPresentation result = null;
            usercontext.KeyChanged = true;
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
                        usercontext.UIMode = ProviderPageMode.ShowQRCode;
                        result = new AdapterPresentation(this, context);
                    }
                    else if (lnk == 3)
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

                        usercontext.UIMode = ProviderPageMode.SendKeyRequest;
                        SendKeyToUser(usercontext, Resources, NotificationStatus.ResponseEmailForKeyInvitation);
                        result = new AdapterPresentation(this, context);
                    }
                }
                if (lnk == 0)
                {
                    int btnclicked = Convert.ToInt32(proofData.Properties["btnclicked"].ToString());
                    int select = Convert.ToInt32(proofData.Properties["selectopt"].ToString());
                    usercontext.PreferredMethod = (RegistrationPreferredMethod)select;
                   
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
                        RepositoryService.SetUserRegistration((Registration)usercontext, Config);
                        Registration reg = RepositoryService.GetUserRegistration(usercontext.UPN, Config);
                        usercontext.Assign(reg);

                        if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowProvideInformations))
                        {
                            usercontext.UIMode = ProviderPageMode.InvitationRequest;
                            SendInscriptionToAdmins(usercontext, Resources);
                            if (usercontext.TargetUIMode == ProviderPageMode.DefinitiveError)
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNotEnabled"), ProviderPageMode.DefinitiveError);
                            else if (usercontext.TargetUIMode == ProviderPageMode.Bypass)
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass);
                        }
                        else
                        {
                            usercontext.UIMode = ProviderPageMode.Locking;
                            if ( Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowDisabled) || Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowUnRegistered) )
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass);
                            else
                                result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNotEnabled"), ProviderPageMode.DefinitiveError);
                        }
                    }
                    else if (btnclicked == 2) //Cancel
                    {
                        
                        if (usercontext.TargetUIMode == ProviderPageMode.Bypass)
                        {
                            usercontext.UIMode = ProviderPageMode.Bypass;
                            result = new AdapterPresentation(this, context);
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
                if ((Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowManageOptions)) || (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowChangePassword)))
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
                usercontext.PreferredMethod = RegistrationPreferredMethod.Code;
                int opt = Convert.ToInt32(proofData.Properties["opt"].ToString());
                object rem = null;
                bool remember = proofData.Properties.TryGetValue("remember", out rem);
                switch (opt)
                {
                    case 0:
                        if (Config.AppsEnabled)
                        {
                            usercontext.PreferredMethod = RegistrationPreferredMethod.Code;
                            usercontext.UIMode = ProviderPageMode.Identification;
                            SendNotification(usercontext, Resources);
                            result = new AdapterPresentation(this, context);
                            if (remember)
                                RepositoryService.SetUserRegistration((Registration)usercontext, Config);
                        }
                        else
                        {
                            usercontext.PreferredMethod = RegistrationPreferredMethod.Choose;
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                        }
                        break;
                    case 1:
                        if (Config.SMSEnabled)
                        {
                            usercontext.PreferredMethod = RegistrationPreferredMethod.Phone;
                            usercontext.UIMode = ProviderPageMode.CodeRequest;
                            SendNotification(usercontext, Resources);
                            result = new AdapterPresentation(this, context);
                            if (remember)
                                RepositoryService.SetUserRegistration((Registration)usercontext, Config);
                        }
                        else
                        {
                            usercontext.PreferredMethod = RegistrationPreferredMethod.Choose;
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                        }
                        break;
                    case 2:
                        if (Config.MailEnabled)
                        {
                            usercontext.PreferredMethod = RegistrationPreferredMethod.Email;
                            string stmail = proofData.Properties["stmail"].ToString();
#if softemail
                            // the email is not registered, but registration has been made, so we force to register the email. this is a little security hole, but at this point the user is authenticated. then we log an alert in the EventLog
                            if (string.IsNullOrEmpty(usercontext.MailAddress))
                            {
                                usercontext.MailAddress = stmail;
                                remember = true;
                                Log. WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorRegistrationEmptyEmail"), usercontext.UPN, usercontext.MailAddress), EventLogEntryType.Error, 9999);
                                usercontext.UIMode = ProviderPageMode.CodeRequest;
                                SendNotification(usercontext, Resources);
                                result = new AdapterPresentation(this, context);
                                if (remember)
                                    RepositoryService.SetUserRegistration((Registration)usercontext, Config);
                            }
                            else
#endif
                            {
                                string idom = MailUtilities.StripEmailDomain(usercontext.MailAddress);
                                if ((stmail.ToLower() + idom.ToLower()).Equals(usercontext.MailAddress.ToLower()))
                                {
                                    usercontext.UIMode = ProviderPageMode.CodeRequest;
                                    SendNotification(usercontext, Resources);
                                    result = new AdapterPresentation(this, context);
                                    if (remember)
                                        RepositoryService.SetUserRegistration((Registration)usercontext, Config);
                                }
                                else
                                {
                                    usercontext.PreferredMethod = RegistrationPreferredMethod.Choose;
                                    usercontext.UIMode = ProviderPageMode.Locking;
                                    result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                                }
                            }
                        }
                        else
                        {
                            usercontext.PreferredMethod = RegistrationPreferredMethod.Choose;
                            usercontext.UIMode = ProviderPageMode.Locking;
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorInvalidIdentificationRestart"), ProviderPageMode.DefinitiveError);
                        }
                        break;
                    case 3:
                        usercontext.PreferredMethod = RegistrationPreferredMethod.Choose;
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
                                RepositoryService.ChangePassword(usercontext.UPN, oldpass, newpass);
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
                System.Security.Claims.Claim claimx1 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", "http://schemas.microsoft.com/ws/2012/12/authmethod/otp");
                System.Security.Claims.Claim claimx2 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpmode", "bypass");
                System.Security.Claims.Claim claimx3 = new System.Security.Claims.Claim("http://schemas.microsoft.com/ws/2015/02/identity/claims/otpduration", "0");
                claims = new System.Security.Claims.Claim[] { claimx1, claimx2, claimx3 };
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
                           // string msg = proofData.Properties["msg"].ToString();
                            string msg = usercontext.UIMessage;
                            throw new Exception(msg);
                        default:
                            usercontext.UIMode = usercontext.TargetUIMode;
                            result = new AdapterPresentation(this, context, usercontext.UIMessage);
                            break;
                    }
                }
                else if (btnclicked == 2)
                {
                    if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowProvideInformations))
                    {
                        usercontext.UIMode = ProviderPageMode.Invitation;
                        result = new AdapterPresentation(this, context);
                    }
                    else if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowManageOptions))
                    {
                        usercontext.UIMode = ProviderPageMode.Registration;
                        result = new AdapterPresentation(this, context);
                    }
                    else if (usercontext.TargetUIMode == ProviderPageMode.Bypass)
                    {
                        if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AdministrativeMode))
                            result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAdminAuthorized"), ProviderPageMode.Bypass);
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
        /// TryCodeRequest method implementation
        /// </summary>
        private IAdapterPresentation TryCodeRequest(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Requesting Code
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            claims = null;
            IAdapterPresentation result = null;
            try
            {
                int lnk = Convert.ToInt32(proofData.Properties["lnk"].ToString());
                object opt = null;
                bool options = proofData.Properties.TryGetValue("options", out opt);
                usercontext.ShowOptions = options;
                if (lnk == 3)
                {
                    usercontext.UIMode = ProviderPageMode.ChooseMethod;
                    return new AdapterPresentation(this, context);
                }
                Notification chknotif = RepositoryService.CheckNotification((Registration)usercontext, Config);
                if (chknotif != null)
                {
                    if (chknotif.OTP == NotificationStatus.Error)
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.Identification);
                    }
                    else if (chknotif.OTP == NotificationStatus.RequestEmail)
                        usercontext.UIMode = ProviderPageMode.CodeRequest;
                    else if (chknotif.OTP == NotificationStatus.RequestSMS)
                        usercontext.UIMode = ProviderPageMode.CodeRequest;
                    else if (chknotif.OTP == NotificationStatus.Bypass)
                    {
                        if (options)
                            usercontext.UIMode = ProviderPageMode.SelectOptions;  // Manage options, Access granted
                        else
                            usercontext.UIMode = ProviderPageMode.Bypass; // Grant Access
                    }
                    else
                        usercontext.UIMode = ProviderPageMode.Identification;
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
        /// TryInvitationRequest method implementation
        /// </summary>
        private IAdapterPresentation TryInvitationRequest(AuthenticationContext usercontext, IAuthenticationContext context, IProofData proofData, System.Net.HttpListenerRequest request, out System.Security.Claims.Claim[] claims)
        {
            #region Invitation Request
            ResourcesLocale Resources = new ResourcesLocale(context.Lcid);
            claims = null;
            IAdapterPresentation result = null;
            try
            {
                Notification invnotif = RepositoryService.CheckNotification((Registration)usercontext, Config);
                if (invnotif.OTP == NotificationStatus.Error)
                {
                    usercontext.UIMode = ProviderPageMode.Locking;
                    return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                }
                else if (invnotif.CheckDate.Value.ToUniversalTime() > invnotif.ValidityDate.ToUniversalTime())  // Always check with Universal Time
                {
                    usercontext.UIMode = ProviderPageMode.Locking;
                    result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorValidationTimeWindowElapsed"), ProviderPageMode.DefinitiveError);
                }
                else if (invnotif.OTP == NotificationStatus.Bypass)
                {
                    if (Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowDisabled) || Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowUnRegistered))
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized"), ProviderPageMode.Bypass, true);
                    }
                    else
                    {
                        usercontext.UIMode = ProviderPageMode.Locking;
                        result = new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNotEnabled"), ProviderPageMode.DefinitiveError, true);
                    }
                }
                else
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
                Notification invnotif = RepositoryService.CheckNotification((Registration)usercontext, Config);
                if (invnotif.OTP == NotificationStatus.Error)
                {
                    usercontext.UIMode = ProviderPageMode.Locking;
                    return new AdapterPresentation(this, context, Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation"), ProviderPageMode.DefinitiveError);
                }
                else if (invnotif.OTP== NotificationStatus.ResponseEmailForKeyInvitation)
                {
                    usercontext.UIMode = ProviderPageMode.Invitation;
                    result = new AdapterPresentation(this, context);
                }
                else if (invnotif.OTP== NotificationStatus.ResponseEmailForKeyRegistration)
                {
                    usercontext.UIMode = ProviderPageMode.Registration;
                    result = new AdapterPresentation(this, context);
                }
                else
                    result = new AdapterPresentation(this, context);

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
        private void SendNotification(AuthenticationContext usercontext, ResourcesLocale resources )
        {
            if (usercontext.UIMode == ProviderPageMode.CodeRequest)
            {
                Task tsk = new Task(() => internalSendNotification(usercontext, resources, Config), TaskCreationOptions.LongRunning);
                tsk.Start();
            }
            else if (usercontext.UIMode == ProviderPageMode.Identification)
                RepositoryService.SetNotification((Registration)usercontext, Config, NotificationStatus.Totp);
            return;
        }

        /// <summary>
        /// SendInscriptionToAdmins method implementation
        /// </summary>
        private void SendInscriptionToAdmins(AuthenticationContext usercontext, ResourcesLocale resources)
        {
            if (usercontext.UIMode == ProviderPageMode.InvitationRequest)
            {
                Task tsk = new Task(() => internalSendInscriptionToAdmins(usercontext, resources, Config), TaskCreationOptions.LongRunning);
                tsk.Start();
            }
            return;
        }

        /// <summary>
        /// SendKeyToUser method implementation
        /// </summary>
        private void SendKeyToUser(AuthenticationContext usercontext, ResourcesLocale resources, int ret)
        {
            if (usercontext.UIMode == ProviderPageMode.SendKeyRequest)
            {
                Task tsk = new Task(() => internalSendKeyToUser(usercontext, resources, ret), TaskCreationOptions.LongRunning);
                tsk.Start();
            }
            return;
        }

        /// <summary>
        /// internalSendNotification method implementation
        /// </summary>
        private void internalSendNotification(AuthenticationContext usercontext, ResourcesLocale Resources, MFAConfig cfg) 
        {
            try
            {
                int otpres = NotificationStatus.Error;
                Notification ntf = null;
                CheckUserProps(usercontext);
                switch (usercontext.PreferredMethod)
                {
                    case RegistrationPreferredMethod.Email:
                        RepositoryService.SetNotification((Registration)usercontext, Config, NotificationStatus.RequestEmail);
                        otpres = Utilities.GetEmailOTP((Registration)usercontext, Config.SendMail, Resources.Culture);
                        ntf = RepositoryService.CheckNotification((Registration)usercontext, cfg);
                        if (ntf.OTP==NotificationStatus.RequestEmail)
                           RepositoryService.SetNotification((Registration)usercontext, Config, otpres);
                        if (otpres == NotificationStatus.Error)
                            Log. WriteEntry(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation") + "\r\n" + usercontext.UPN, EventLogEntryType.Error, 800);
                        break;
                    case RegistrationPreferredMethod.Phone:
                        RepositoryService.SetNotification((Registration)usercontext, Config, NotificationStatus.RequestSMS); 
                        otpres = Utilities.GetPhoneOTP((Registration)usercontext, Config, Resources.Culture);
                        ntf = RepositoryService.CheckNotification((Registration)usercontext, cfg);
                        if (ntf.OTP == NotificationStatus.RequestSMS)
                            RepositoryService.SetNotification((Registration)usercontext, Config, otpres);
                        if (otpres == NotificationStatus.Error)
                            Log. WriteEntry(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation") + "\r\n" + usercontext.UPN, EventLogEntryType.Error, 800);
                        break;
                }
            }
            catch (Exception ex)
            {
                RepositoryService.SetNotification((Registration)usercontext, Config, NotificationStatus.Error);
                Log. WriteEntry(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation") + "\r\n" + usercontext.UPN + " : " + "\r\n" + ex.Message + "\r\n" + ex.StackTrace, EventLogEntryType.Error, 800);
            }
        }

        /// <summary>
        /// internalSendInscriptionToAdmins method implmentation
        /// </summary>
        private void internalSendInscriptionToAdmins(AuthenticationContext usercontext, ResourcesLocale Resources, MFAConfig Config)
        {
            try
            {
                RepositoryService.SetNotification((Registration)usercontext, Config, NotificationStatus.RequestIncription);
                MailUtilities.SendInscriptionMail(Config.AdminContact, (Registration)usercontext, Config.SendMail, Resources.Culture);
                RepositoryService.SetNotification((Registration)usercontext, Config, NotificationStatus.Bypass);
            }
            catch (Exception ex)
            {
                RepositoryService.SetNotification((Registration)usercontext, Config, NotificationStatus.Error);
                Log. WriteEntry(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation") + "\r\n" + usercontext.UPN + " : " + "\r\n" + ex.Message + "\r\n" + ex.StackTrace, EventLogEntryType.Error, 801);
            }
        }

        /// <summary>
        /// internalSendKeyToUser method implementation
        /// </summary>
        private void internalSendKeyToUser(AuthenticationContext usercontext, ResourcesLocale Resources, int ret)
        {
            try
            {
                CheckUserProps(usercontext);
                string qrcode = KeysManager.EncodedKey(usercontext.UPN);
                RepositoryService.SetNotification((Registration)usercontext, Config, NotificationStatus.RequestEmailForKey);
                MailUtilities.SendKeyByEmail(usercontext.MailAddress, usercontext.UPN, qrcode, Config.SendMail, Config, Resources.Culture);
                RepositoryService.SetNotification((Registration)usercontext, Config, ret);
            }
            catch (Exception ex)
            {
                RepositoryService.SetNotification((Registration)usercontext, Config, NotificationStatus.Error);
                Log. WriteEntry(Resources.GetString(ResourcesLocaleKind.Errors, "ErrorSendingToastInformation") + "\r\n" + usercontext.UPN + " : " + "\r\n" + ex.Message + "\r\n" + ex.StackTrace, EventLogEntryType.Error, 801);
            }
        }

        /// <summary>
        /// CheckUserProps method implementation
        /// </summary>
        private void CheckUserProps(AuthenticationContext usercontext)
        {
            if (!KeysManager.ValidateKey(usercontext.UPN))
                throw new CryptographicException(string.Format("SECURTY ERROR : Invalid Key for User {0}", usercontext.UPN));
            Registration reg = RepositoryService.GetUserRegistration(usercontext.UPN, Config);
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
            if (notif.OTP == NotificationStatus.Error)
                return false;
            else if (notif.OTP > NotificationStatus.Error)
            {
                string generatedpin = notif.OTP.ToString("D6");  // eg : transmitted by email or by External System (SMS)
                return (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin));
            }
            else if (notif.OTP == NotificationStatus.Totp)  // Using a TOTP Application (Microsoft Authnetication, Google Authentication, etc...)
            {
                if (Config.TOTPShadows <= 0)
                {
                    if (!KeysManager.ValidateKey(usercontext.UPN))
                        throw new CryptographicException(string.Format("SECURTY ERROR : Invalid Key for User {0}", usercontext.UPN));
                    string encodedkey = KeysManager.ProbeKey(usercontext.UPN);
                    DateTime call = DateTime.UtcNow;
                    OTPGenerator gen = new OTPGenerator(encodedkey, usercontext.UPN, call, Config.Algorithm);  // eg : TOTP code
                    gen.ComputeOTP(call);
                    string generatedpin = gen.OTP.ToString("D6");
                    return (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin));
                }
                else
                {   // Current TOTP
                    if (!KeysManager.ValidateKey(usercontext.UPN))
                        throw new CryptographicException(string.Format("SECURTY ERROR : Invalid Key for User {0}", usercontext.UPN));
                    string encodedkey = KeysManager.ProbeKey(usercontext.UPN);
                    DateTime tcall = DateTime.UtcNow;
                    OTPGenerator gen = new OTPGenerator(encodedkey, usercontext.UPN, tcall, Config.Algorithm);  // eg : TOTP code
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
                        OTPGenerator gen2 = new OTPGenerator(encodedkey, usercontext.UPN, call, Config.Algorithm);  // eg : TOTP code
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
                        OTPGenerator gen3 = new OTPGenerator(encodedkey, usercontext.UPN, call, Config.Algorithm);  // eg : TOTP code
                        gen3.ComputeOTP(call);
                        string generatedpin = gen3.OTP.ToString("D6");
                        if (Convert.ToInt32(pin) == Convert.ToInt32(generatedpin))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            else if (notif.OTP == NotificationStatus.Error)    // Active Request for code
                return false;
            else if (notif.OTP == NotificationStatus.Bypass)   // Magic - Validated by External ExternalOTPProvider
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
                Trace.WriteLine("Configuration changed !", "Information");
                _config = CFGUtilities.ReadConfiguration();
                Trace.WriteLine("Configuration loaded !", "Warning");
                ResourcesLocale Resources = new ResourcesLocale(CultureInfo.CurrentCulture.LCID);
                Log.WriteEntry(string.Format(Resources.GetString(ResourcesLocaleKind.Informations, "InfosConfigurationReloaded"), message.MachineName), EventLogEntryType.Warning, 9999);

                RepositoryService.MailslotServer.AllowedMachines.Clear();
                foreach (ADFSServerHost svr in Config.Hosts.ADFSFarm.Servers)
                {
                    RepositoryService.MailslotServer.AllowedMachines.Add(svr.MachineName);
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
