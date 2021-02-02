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
// #define azuretest
using Neos.IdentityServer.MultiFactor.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Neos.IdentityServer.MultiFactor.SAS
{
    public class NeosAzureProvider: BaseExternalProvider
    {
        private string TenantId = "YOUR_TENANT_ID";
        private string ClientId = "981f26a1-7f43-403b-a875-f8b09b8cd720";
        private string CertId = Thumbprint.Demo;
        private string STSIdentifier = "stsidentifier";
        private string CompanyName = "MFA";
        private ISasProvider _sasprovider = null;

        private bool _isinitialized = false;
        private bool _isrequired = true;

        /// <summary>
        /// Kind property implementation
        /// </summary>
        public override PreferredMethod Kind
        {
            get { return PreferredMethod.Azure; }
        }

        /// <summary>
        /// IsBuiltIn property implementation
        /// </summary>
        public override bool IsBuiltIn
        {
            get { return true; }
        }

        /// <summary>
        /// IsRequired property implementation
        /// </summary>
        public override bool IsRequired
        {
            get { return _isrequired; }
            set { _isrequired = value; }
        }

        /// <summary>
        /// IsInitialized property implementation
        /// </summary>
        public override bool IsInitialized
        {
            get { return _isinitialized; }
        }

        internal ISasProvider SasProvider
        {
            get 
            { 
                if (_sasprovider==null)
                    _sasprovider = new NeosSasProvider(TenantId, ClientId, CertId);
                return _sasprovider; 
            }
        }

        /// <summary>
        /// CanBeDisabled property implementation
        /// </summary>
        public override bool AllowDisable
        {
            get { return true; }
        }

        /// <summary>
        /// CanOverrideDefault property implmentation
        /// </summary>
        public override bool AllowOverride
        {
            get { return true; }
        }

        /// <summary>
        /// AllowEnrollment property implementation
        /// </summary>
        public override bool AllowEnrollment
        {
            get { return true; }
        }

        /// <summary>
        /// ForceEnrollment property implementation
        /// </summary>
        public override ForceWizardMode ForceEnrollment
        {
            get { return ForceWizardMode.Disabled; }
            set { }
        }

        /// <summary>
        /// Name property implementation
        /// </summary>
        public override string Name
        {
            get { return "Neos.Provider.Azure"; }
        }

        /// <summary>
        /// Description property implementation
        /// </summary>
        public override string Description
        {
            get
            {
                string independent = "Microsoft Azure Multi-Factor Provider";
                ResourcesLocale Resources = null;
                if (CultureInfo.DefaultThreadCurrentUICulture != null)
                    Resources = new ResourcesLocale(CultureInfo.DefaultThreadCurrentUICulture.LCID);
                else
                    Resources = new ResourcesLocale(CultureInfo.CurrentUICulture.LCID);
                string res = Resources.GetString(ResourcesLocaleKind.Html, "PROVIDERAZURESDESCRIPTION");
                if (!string.IsNullOrEmpty(res))
                    return res;
                else
                    return independent;
            }
        }

        /// <summary>
        /// GetUILabel method implementation
        /// </summary>
        public override string GetUILabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "GLOBALOTPLabel");
        }

        /// <summary>
        /// GetWizardUILabel method implementation
        /// </summary>
        public override string GetWizardUILabel(AuthenticationContext ctx)
        {
            return string.Empty;
        }

        /// <summary>
        /// GetWizardUIComment method implementation
        /// </summary>
        public override string GetWizardUIComment(AuthenticationContext ctx)
        {
            return string.Empty;
        }

        /// <summary>
        /// GetWizardLinkLabel method implementation
        /// </summary>
        public override string GetWizardLinkLabel(AuthenticationContext ctx)
        {
            return string.Empty;
        }

        /// <summary>
        /// GetUICFGLabel method implementation
        /// </summary>
        public override string GetUICFGLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "GLOBALCFGLabel");
        }

        /// <summary>
        /// GetUIMessage method implementation
        /// </summary>
        public override string GetUIMessage(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            GetAuthenticationContext(ctx);
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            switch (ctx.SelectedMethod)
            {
                case AuthenticationResponseKind.PhoneAppNotification:
                     return Resources.GetString(ResourcesLocaleKind.Html, "NOTIFMessage");

                case AuthenticationResponseKind.PhoneAppOTP:
                     return Resources.GetString(ResourcesLocaleKind.Html, "OTPMessage");

                case AuthenticationResponseKind.SmsOneWayOTPplusPin:
                case AuthenticationResponseKind.SmsOneWayOTP:
                case AuthenticationResponseKind.SmsTwoWayOTPplusPin:
                case AuthenticationResponseKind.SmsTwoWayOTP:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "SMSMessage"), string.Empty);
                    else
                       return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "SMSMessage"), Utilities.StripPhoneNumber(ctx.ExtraInfos));

                case AuthenticationResponseKind.VoiceTwoWayMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayMobile:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "VOICE1Message"), string.Empty);
                    else
                        return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "VOICE1Message"), Utilities.StripPhoneNumber(ctx.ExtraInfos));

                case AuthenticationResponseKind.VoiceTwoWayAlternateMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayAlternateMobile:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "VOICE2Message"), string.Empty);
                    else
                        return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "VOICE2Message"), Utilities.StripPhoneNumber(ctx.ExtraInfos));

                case AuthenticationResponseKind.VoiceTwoWayOfficePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayOffice:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "VOICE3Message"), string.Empty);
                    else
                        return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "VOICE3Message"), Utilities.StripPhoneNumber(ctx.ExtraInfos));
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// GetUIListOptionLabel method implementation
        /// </summary>
        public override string GetUIListOptionLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "GLOBALListOptionLabel");
        }

        /// <summary>
        /// GetUIListChoiceLabel method implementation
        /// </summary>
        public override string GetUIListChoiceLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "GLOBALListChoiceLabel");
        }

        /// <summary>
        /// GetUIConfigLabel method implementation
        /// </summary>
        public override string GetUIConfigLabel(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            GetAuthenticationContext(ctx);
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            switch (ctx.SelectedMethod)
            {
                case AuthenticationResponseKind.PhoneAppNotification:
                    return Resources.GetString(ResourcesLocaleKind.Html, "NOTIFConfigLabel");

                case AuthenticationResponseKind.PhoneAppOTP:
                    return Resources.GetString(ResourcesLocaleKind.Html, "OTPConfigLabel");

                case AuthenticationResponseKind.SmsOneWayOTPplusPin:
                case AuthenticationResponseKind.SmsOneWayOTP:
                case AuthenticationResponseKind.SmsTwoWayOTPplusPin:
                case AuthenticationResponseKind.SmsTwoWayOTP:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return Resources.GetString(ResourcesLocaleKind.Html, "SMSConfigLabel");
                    else
                        return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "SMSConfigLabel2"), Utilities.StripPhoneNumber(ctx.ExtraInfos));
                case AuthenticationResponseKind.VoiceTwoWayMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayMobile:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return Resources.GetString(ResourcesLocaleKind.Html, "VOICE1ConfigLabel");
                    else
                        return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "VOICE1ConfigLabel2"), Utilities.StripPhoneNumber(ctx.ExtraInfos));

                case AuthenticationResponseKind.VoiceTwoWayAlternateMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayAlternateMobile:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return Resources.GetString(ResourcesLocaleKind.Html, "VOICE2ConfigLabel");
                    else
                        return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "VOICE2ConfigLabel2"), Utilities.StripPhoneNumber(ctx.ExtraInfos));

                case AuthenticationResponseKind.VoiceTwoWayOfficePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayOffice:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return Resources.GetString(ResourcesLocaleKind.Html, "VOICE3ConfigLabel");
                    else
                        return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "VOICE3ConfigLabel2"), Utilities.StripPhoneNumber(ctx.ExtraInfos));
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// GetUIChoiceLabel method implementation
        /// </summary>
        public override string GetUIChoiceLabel(AuthenticationContext ctx, AvailableAuthenticationMethod method = null)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            GetAuthenticationContext(ctx);
            AuthenticationResponseKind mk = ctx.SelectedMethod;
            if (method != null)
                mk = method.Method;
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            switch (mk)
            {
                case AuthenticationResponseKind.PhoneAppNotification:
                    return Resources.GetString(ResourcesLocaleKind.Html, "NOTIFChoiceLabel");

                case AuthenticationResponseKind.PhoneAppOTP:
                    return Resources.GetString(ResourcesLocaleKind.Html, "OTPChoiceLabel");

                case AuthenticationResponseKind.SmsOneWayOTPplusPin:
                case AuthenticationResponseKind.SmsOneWayOTP:
                case AuthenticationResponseKind.SmsTwoWayOTPplusPin:
                case AuthenticationResponseKind.SmsTwoWayOTP:
                    return Resources.GetString(ResourcesLocaleKind.Html, "SMSChoiceLabel");

                case AuthenticationResponseKind.VoiceTwoWayMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayMobile:
                    return Resources.GetString(ResourcesLocaleKind.Html, "VOICE1ChoiceLabel");

                case AuthenticationResponseKind.VoiceTwoWayAlternateMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayAlternateMobile:
                    return Resources.GetString(ResourcesLocaleKind.Html, "VOICE2ChoiceLabel");
                case AuthenticationResponseKind.VoiceTwoWayOfficePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayOffice:
                    return Resources.GetString(ResourcesLocaleKind.Html, "VOICE3ChoiceLabel");
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// GetUIWarningInternetLabel method implementation
        /// </summary>
        public override string GetUIWarningInternetLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "GLOBALWarnOverNetwork");
        }

        /// <summary>
        /// GetUIWarningThirdPartyLabel method implementation
        /// </summary>
        public override string GetUIWarningThirdPartyLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "GLOBALWarnThirdParty");
        }

        /// <summary>
        /// GetUIDefaultChoiceLabel method implementation
        /// </summary>
        public override string GetUIDefaultChoiceLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "GLOBALListChoiceDefaultLabel");
        }

        /// <summary>
        /// GetUIEnrollmentTaskLabel method implementation
        /// </summary>
        public override string GetUIEnrollmentTaskLabel(AuthenticationContext ctx)
        {
            return string.Empty;
        }

        /// <summary>
        /// GetUIEnrollValidatedLabel method implementation
        /// </summary>
        public override string GetUIEnrollValidatedLabel(AuthenticationContext ctx)
        {
            return string.Empty;
        }

        /// <summary>
        /// GetUIAccountManagementLabel method implementation
        /// </summary>
        public override string GetUIAccountManagementLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "GLOBALManagement");
        }

        /// <summary>
        /// GetAccountManagementUrl() method implmentation
        /// </summary>
        public override string GetAccountManagementUrl(AuthenticationContext ctx)
        {
            return "https://login.microsoftonline.com/login.srf?wa=wsignin1.0&whr=" + STSIdentifier + "&wreply=https://account.activedirectory.windowsazure.com/Proofup.aspx";
        }

        /// <summary>
        /// IsAvailable method implmentation
        /// </summary>
        public override bool IsAvailable(AuthenticationContext ctx)
        {
            try
            { 
                var meths = GetAuthenticationMethods(ctx);
                return true;
            }
            catch (Exception)
            {
#if !azuretest
                Enabled = false;
                return false;
#else
                Enabled = true;
                return true;
#endif
            }
        }

        /// <summary>
        /// IsAvailableForUser method implmentation
        /// </summary>
        public override bool IsAvailableForUser(AuthenticationContext ctx)
        {
            try
            {
                return GetAuthenticationMethods(ctx).Count > 0;
            }
            catch
            {
#if !azuretest
                return false;  
#else
                return true;
#endif
            }
        }

        /// <summary>
        /// IsMethodElementRequired implementation
        /// </summary>
        public override bool IsUIElementRequired(AuthenticationContext ctx, RequiredMethodElements element)
        {
            switch (element)
            {
                case RequiredMethodElements.CodeInputRequired:
                    return !ctx.IsTwoWay;
                case RequiredMethodElements.AzureInputRequired:
                    return true;
                case RequiredMethodElements.PinInputRequired:
                    return this.PinRequired;
                case RequiredMethodElements.PinParameterRequired:
                    return this.PinRequired;
                case RequiredMethodElements.PinLinkRequired:
                    return this.PinRequired;
            }
            return false;
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public override void Initialize(BaseProviderParams externalsystem)
        {
            try
            {
                if (!_isinitialized)
                {
                    if (externalsystem is AzureProviderParams)
                    {
                        AzureProviderParams az = externalsystem as AzureProviderParams;
                        TenantId = az.Data.TenantId;
                        CertId = az.Data.ThumbPrint;
                        STSIdentifier = az.ADFSIdentifier;
                        CompanyName = az.CompanyName;
                        Enabled = az.Enabled;
                        IsRequired = az.IsRequired;
                        PinRequired = az.PinRequired;
                        WizardEnabled = az.EnrollWizard;
                        ForceEnrollment = az.ForceWizard;
                        _isinitialized = true;
                        return;
                    }
                    else
                        throw new InvalidCastException("Invalid External Provider for Azue !");
                }
            }
            catch (Exception ex)
            {
                this.Enabled = false;
                _isinitialized = false;
                throw ex;
            }
        }

        /// <summary>
        /// PostAutnenticationRequest method implmentation
        /// </summary>
        public override int PostAuthenticationRequest(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            if (ctx.SelectedMethod == AuthenticationResponseKind.Error)
                GetAuthenticationContext(ctx);

            AuthenticationResponseKind stat = BeginAzureAuthentication(ctx);
            ctx.Notification = (int)stat;
           // ctx.SessionId = Guid.NewGuid().ToString(); // Provided by Microsoft
            ctx.SessionDate = DateTime.Now;
            return (int)stat;
        }

        /// <summary>
        /// SetAuthenticationResult method implmentation
        /// </summary>
        public override int SetAuthenticationResult(AuthenticationContext ctx, string secret)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            if (ctx.SelectedMethod == AuthenticationResponseKind.Error)
                GetAuthenticationContext(ctx);
            AuthenticationResponseKind stat = EndAzureAuthentication(ctx, secret);
            return (int)stat;
        }

        /// <summary>
        /// GetAuthenticationMethods method implmentation
        /// </summary>
        public override List<AvailableAuthenticationMethod> GetAuthenticationMethods(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            List<AvailableAuthenticationMethod> result = GetSessionData(this.Kind, ctx);
            if (result != null)
                return result;
            else
            {
                result = new List<AvailableAuthenticationMethod>();
#if !azuretest
                GetAvailableAuthenticationMethodsResponse authMethods = GetAzureAvailableAuthenticationMethods(ctx);
                foreach (AuthenticationMethod current in authMethods.AuthenticationMethods)
                {
                    AvailableAuthenticationMethod item = GetAuthenticationMethodProperties(current);
                    if (item.Method != AuthenticationResponseKind.Error)
                    {
                        item.IsDefault = current.IsDefault;
                        result.Add(item);
                    }
                }
#else
                AuthenticationMethod current1 = new AuthenticationMethod() { Id = "OneWaySMS", IsDefault = true};
                current1.Properties.Add("MobilePhone", "+33 123123456");
                AuthenticationMethod current2 = new AuthenticationMethod() { Id = "TwoWaySMS", IsDefault = false };
                current2.Properties.Add("MobilePhone", "+33 123123456");
                AvailableAuthenticationMethod item1 = GetAuthenticationMethodProperties(current1);
                item1.IsDefault = current1.IsDefault;
                result.Add(item1);
                AvailableAuthenticationMethod item2 = GetAuthenticationMethodProperties(current2);
                item2.IsDefault = current2.IsDefault;
                result.Add(item2);
#endif
                if (result.Count > 0)
                    SaveSessionData(this.Kind, ctx, result);

            }
            return result;
        }

#region Private methods
        /// <summary>
        /// GetAuthenticationMethod method implementation
        /// </summary>
        private AvailableAuthenticationMethod GetAuthenticationMethodProperties(AuthenticationMethod method)
        {
            AvailableAuthenticationMethod result = new AvailableAuthenticationMethod();
            result.IsDefault = false;
            result.RequiredPin = false;
            result.IsRemote = true;
            result.IsTwoWay = false;
            result.IsSendBack = true;
            result.RequiredEmail = false;
            result.RequiredPhone = false;
            result.RequiredCode = false;
            if (method.Id == "PhoneAppNotification")
            {
                result.IsTwoWay = true;
                result.RequiredEmail = false;
                result.RequiredPhone = false;
                result.RequiredCode = false;
                result.Method = AuthenticationResponseKind.PhoneAppNotification;
            }
            else if (method.Id == "PhoneAppOTP")
            {
                result.RequiredEmail = false;
                result.RequiredPhone = false;
                result.RequiredCode = true;
                result.Method = AuthenticationResponseKind.PhoneAppOTP;
            }
            else if (method.Id == "OneWaySMS")
            {
                result.RequiredEmail = false;
                result.RequiredPhone = false;
                result.RequiredCode = true;
                if (method.Properties.ContainsKey("MobilePhone"))
                    result.ExtraInfos = method.Properties["MobilePhone"];
                if (method.Properties.ContainsKey("PinEnabled"))
                {
                    result.RequiredPin = true;
                    result.Method = AuthenticationResponseKind.SmsOneWayOTPplusPin;
                }
                else
                    result.Method = AuthenticationResponseKind.SmsOneWayOTP;
            }
            else if (method.Id == "TwoWaySMS")
            {
                result.RequiredEmail = false;
                result.RequiredPhone = false;
                result.RequiredCode = false;
                result.IsTwoWay = true;
                if (method.Properties.ContainsKey("MobilePhone"))
                    result.ExtraInfos = method.Properties["MobilePhone"];
                if (method.Properties.ContainsKey("PinEnabled"))
                {
                    result.RequiredPin = true;
                    result.Method = AuthenticationResponseKind.SmsTwoWayOTPplusPin;
                }
                else
                    result.Method = AuthenticationResponseKind.SmsTwoWayOTP;
            }
            else if (method.Id == "TwoWayVoiceMobile")
            {
                result.RequiredEmail = false;
                result.RequiredPhone = false;
                result.RequiredCode = false;
                result.IsTwoWay = true;
                if (method.Properties.ContainsKey("MobilePhone"))
                    result.ExtraInfos = method.Properties["MobilePhone"];
                if (method.Properties.ContainsKey("PinEnabled"))
                {
                    result.RequiredPin = true;
                    result.Method = AuthenticationResponseKind.VoiceTwoWayMobilePlusPin;
                }
                else
                    result.Method = AuthenticationResponseKind.VoiceTwoWayMobile;
            }
            else if (method.Id == "TwoWayVoiceOffice")
            {
                result.RequiredEmail = false;
                result.RequiredPhone = false;
                result.RequiredCode = false;
                result.IsTwoWay = true;
                if (method.Properties.ContainsKey("PhoneNumber"))
                    result.ExtraInfos = method.Properties["PhoneNumber"];
                if (method.Properties.ContainsKey("PhoneNumber"))
                {
                    result.RequiredPin = true;
                    result.Method = AuthenticationResponseKind.VoiceTwoWayOfficePlusPin;
                }
                else
                    result.Method = AuthenticationResponseKind.VoiceTwoWayOffice;
            }
            else if (method.Id == "TwoWayVoiceAlternateMobile")
            {
                result.RequiredEmail = false;
                result.RequiredPhone = false;
                result.RequiredCode = false;
                result.IsTwoWay = true;
                if (method.Properties.ContainsKey("AlternateMobilePhone"))
                    result.ExtraInfos = method.Properties["AlternateMobilePhone"];
                if (method.Properties.ContainsKey("PinEnabled"))
                {
                    result.RequiredPin = true;
                    result.Method = AuthenticationResponseKind.VoiceTwoWayAlternateMobilePlusPin;
                }
                else
                    result.Method = AuthenticationResponseKind.VoiceTwoWayAlternateMobile;
            }
            else
                result.Method = AuthenticationResponseKind.Error;
            return result;
        }

        /// <summary>
        /// AuthenticationMethodToString method implementation
        /// </summary>
        private string AuthenticationMethodToString(AuthenticationResponseKind method)
        {
            switch (method)
            {
                case AuthenticationResponseKind.PhoneAppNotification:
                    return "PhoneAppNotification";
                case AuthenticationResponseKind.PhoneAppOTP:
                    return "PhoneAppOTP";
                case AuthenticationResponseKind.SmsOneWayOTPplusPin:
                    return "OneWaySMS";
                case AuthenticationResponseKind.SmsOneWayOTP:
                    return "OneWaySMS";
                case AuthenticationResponseKind.SmsTwoWayOTPplusPin:
                    return "TwoWaySMS";
                case AuthenticationResponseKind.SmsTwoWayOTP:
                    return "TwoWaySMS";
                case AuthenticationResponseKind.VoiceTwoWayMobilePlusPin:
                    return "TwoWayVoiceMobile";
                case AuthenticationResponseKind.VoiceTwoWayMobile:
                    return "TwoWayVoiceMobile";
                case AuthenticationResponseKind.VoiceTwoWayOfficePlusPin:
                    return "TwoWayVoiceOffice";
                case AuthenticationResponseKind.VoiceTwoWayOffice:
                    return "TwoWayVoiceOffice";
                case AuthenticationResponseKind.VoiceTwoWayAlternateMobilePlusPin:
                    return "TwoWayVoiceAlternateMobile";
                case AuthenticationResponseKind.VoiceTwoWayAlternateMobile:
                    return "TwoWayVoiceAlternateMobile";
                default:
                    return string.Empty;
            }
        }
#endregion

#region Azure Calls
        /// <summary>
        /// GetAzureAvailableAuthenticationMethods method implementation
        /// </summary>
        private GetAvailableAuthenticationMethodsResponse GetAzureAvailableAuthenticationMethods(AuthenticationContext ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException("ctx");
            if (string.IsNullOrEmpty(ctx.UPN))
                throw new InvalidDataException("No user identity was provided.");

            GetAvailableAuthenticationMethodsRequest request = new GetAvailableAuthenticationMethodsRequest() { UserPrincipalName = ctx.UPN, ContextId = ctx.ActivityId };
            GetAvailableAuthenticationMethodsResponse response;
            try
            {
                response = SasProvider.GetAvailableAuthenticationMethods(request);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception calling SAS.", ex);
            }

            if (response.Result.Value != "Success")
                throw new Exception(string.Format("Unexpected SAS response status code : {0}", response.Result.Value));
            return response;
        }

        /// <summary>
        /// BeginAzureAuthentication method implementation
        /// </summary>
        private AuthenticationResponseKind BeginAzureAuthentication(AuthenticationContext ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException("authContext");
            if (string.IsNullOrEmpty(ctx.UPN))
                throw new InvalidDataException("No user identity was provided.");

            BeginTwoWayAuthenticationRequest request = new BeginTwoWayAuthenticationRequest
            {
                Lcid = CultureInfo.GetCultureInfo(ctx.Lcid).Name,
                UserPrincipalName = ctx.UPN,
                
                CompanyName = this.CompanyName,
                AuthenticationMethodId = AuthenticationMethodToString(ctx.SelectedMethod),
                ReplicationScope = null,
                ContextId = ctx.ActivityId,
                
            };
 
            BeginTwoWayAuthenticationResponse response;
            try
            {
                response = SasProvider.BeginTwoWayAuthentication(request);
            }
            catch (Exception ex2)
            {
                throw new Exception("Exception calling SAS.", ex2);
            }
            if (response.Result.Value != "Success")
                return AuthenticationResponseKind.Error;
            ctx.SessionId = response.SessionId;
            return ctx.SelectedMethod;
        }

        /// <summary>
        /// EndAzureAuthentication method implementation
        /// </summary>
        private AuthenticationResponseKind EndAzureAuthentication(AuthenticationContext ctx, string code)
        {
            if (ctx == null)
                throw new ArgumentNullException("authContext");
            if (string.IsNullOrEmpty(ctx.UPN))
                throw new InvalidDataException("No user identity was provided.");

            EndTwoWayAuthenticationRequest request = new EndTwoWayAuthenticationRequest
            {
                ContextId = ctx.ActivityId,
                SessionId = ctx.SessionId,
                UserPrincipalName = ctx.UPN,     
            };
            if (!string.IsNullOrEmpty(code))
                request.AdditionalAuthData = code;
            EndTwoWayAuthenticationResponse response;
            try
            {
                do
                {
                    response = SasProvider.EndTwoWayAuthentication(request);
                    if (response.Result.Value.Equals("AuthenticationPending"))
                        Thread.Sleep(1000);
                } while (response.Result.Value.Equals("AuthenticationPending"));
            }
            catch (Exception ex)
            {
                throw new Exception("Exception calling SAS.", ex);
            }
            if (response.Result.Value != "Success")
                return AuthenticationResponseKind.Error;
            return ctx.SelectedMethod;
        }

#endregion
    }
}
