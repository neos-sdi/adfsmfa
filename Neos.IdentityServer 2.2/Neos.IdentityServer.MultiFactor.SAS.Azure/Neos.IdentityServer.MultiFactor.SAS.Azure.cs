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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor;
using System.Globalization;
using Microsoft.IdentityServer.Web.Authentication.External;
using System.Diagnostics;
using System.IO;
using System.Threading;
using res = Neos.IdentityServer.MultiFactor.SAS.Resources;

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


        /// <summary>
        /// Kind property implementation
        /// </summary>
        public override PreferredMethod Kind
        {
            get { return PreferredMethod.Azure; }
        }

        /// <summary>
        /// IsInitialized property implementation
        /// </summary>
        public override bool IsInitialized
        {
            get { return _isinitialized; }
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
            get { return false; }
            set { }
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
            get { return "Microsoft Azure Multi-Factor Provider"; }
        }

        /// <summary>
        /// GetUILabel method implementation
        /// </summary>
        public override string GetUILabel(AuthenticationContext ctx)
        {
            Resources.az_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.az_strings.GLOBALOTPLabel;
        }

        /// <summary>
        /// GetUICFGLabel method implementation
        /// </summary>
        public override string GetUICFGLabel(AuthenticationContext ctx)
        {
            Resources.az_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.az_strings.GLOBALCFGLabel;
        }

        /// <summary>
        /// GetUIMessage method implementation
        /// </summary>
        public override string GetUIMessage(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            GetAuthenticationContext(ctx);
            Resources.az_strings.Culture = new CultureInfo(ctx.Lcid);
            switch (ctx.SelectedMethod)
            {
                case AuthenticationResponseKind.PhoneAppNotification:
                     return Resources.az_strings.NOTIFMessage;

                case AuthenticationResponseKind.PhoneAppOTP:
                     return Resources.az_strings.OTPMessage;

                case AuthenticationResponseKind.SmsOneWayOTPplusPin:
                case AuthenticationResponseKind.SmsOneWayOTP:
                case AuthenticationResponseKind.SmsTwoWayOTPplusPin:
                case AuthenticationResponseKind.SmsTwoWayOTP:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return string.Format(Resources.az_strings.SMSMessage, string.Empty);
                    else
                       return string.Format(Resources.az_strings.SMSMessage, Utilities.StripPhoneNumber(ctx.ExtraInfos));

                case AuthenticationResponseKind.VoiceTwoWayMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayMobile:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return string.Format(Resources.az_strings.VOICE1Message, string.Empty);
                    else
                        return string.Format(Resources.az_strings.VOICE1Message, Utilities.StripPhoneNumber(ctx.ExtraInfos));

                case AuthenticationResponseKind.VoiceTwoWayAlternateMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayAlternateMobile:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return string.Format(Resources.az_strings.VOICE2Message, string.Empty);
                    else
                        return string.Format(Resources.az_strings.VOICE2Message, Utilities.StripPhoneNumber(ctx.ExtraInfos));

                case AuthenticationResponseKind.VoiceTwoWayOfficePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayOffice:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return string.Format(Resources.az_strings.VOICE3Message, string.Empty);
                    else
                        return string.Format(Resources.az_strings.VOICE3Message, Utilities.StripPhoneNumber(ctx.ExtraInfos));
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// GetUIListOptionLabel method implementation
        /// </summary>
        public override string GetUIListOptionLabel(AuthenticationContext ctx)
        {
            Resources.az_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.az_strings.GLOBALListOptionLabel;
        }

        /// <summary>
        /// GetUIListChoiceLabel method implementation
        /// </summary>
        public override string GetUIListChoiceLabel(AuthenticationContext ctx)
        {
            Resources.az_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.az_strings.GLOBALListChoiceLabel;
        }

        /// <summary>
        /// GetUIConfigLabel method implementation
        /// </summary>
        public override string GetUIConfigLabel(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            GetAuthenticationContext(ctx);
            Resources.az_strings.Culture = new CultureInfo(ctx.Lcid);
            switch (ctx.SelectedMethod)
            {
                case AuthenticationResponseKind.PhoneAppNotification:
                    return Resources.az_strings.NOTIFConfigLabel;

                case AuthenticationResponseKind.PhoneAppOTP:
                    return Resources.az_strings.OTPConfigLabel;

                case AuthenticationResponseKind.SmsOneWayOTPplusPin:
                case AuthenticationResponseKind.SmsOneWayOTP:
                case AuthenticationResponseKind.SmsTwoWayOTPplusPin:
                case AuthenticationResponseKind.SmsTwoWayOTP:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return Resources.az_strings.SMSConfigLabel;
                    else
                        return string.Format(Resources.az_strings.SMSConfigLabel2, Utilities.StripPhoneNumber(ctx.ExtraInfos));
                case AuthenticationResponseKind.VoiceTwoWayMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayMobile:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return Resources.az_strings.VOICE1ConfigLabel;
                    else
                        return string.Format(Resources.az_strings.VOICE1ConfigLabel2, Utilities.StripPhoneNumber(ctx.ExtraInfos));

                case AuthenticationResponseKind.VoiceTwoWayAlternateMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayAlternateMobile:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return Resources.az_strings.VOICE2ConfigLabel;
                    else
                        return string.Format(Resources.az_strings.VOICE2ConfigLabel2, Utilities.StripPhoneNumber(ctx.ExtraInfos));

                case AuthenticationResponseKind.VoiceTwoWayOfficePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayOffice:
                    if (string.IsNullOrEmpty(ctx.ExtraInfos))
                        return Resources.az_strings.VOICE3ConfigLabel;
                    else
                        return string.Format(Resources.az_strings.VOICE3ConfigLabel2, Utilities.StripPhoneNumber(ctx.ExtraInfos));
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
            Resources.az_strings.Culture = new CultureInfo(ctx.Lcid);
            switch (mk)
            {
                case AuthenticationResponseKind.PhoneAppNotification:
                    return Resources.az_strings.NOTIFChoiceLabel;

                case AuthenticationResponseKind.PhoneAppOTP:
                    return Resources.az_strings.OTPChoiceLabel;

                case AuthenticationResponseKind.SmsOneWayOTPplusPin:
                case AuthenticationResponseKind.SmsOneWayOTP:
                case AuthenticationResponseKind.SmsTwoWayOTPplusPin:
                case AuthenticationResponseKind.SmsTwoWayOTP:
                    return Resources.az_strings.SMSChoiceLabel;

                case AuthenticationResponseKind.VoiceTwoWayMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayMobile:
                    return Resources.az_strings.VOICE1ChoiceLabel;

                case AuthenticationResponseKind.VoiceTwoWayAlternateMobilePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayAlternateMobile:
                    return Resources.az_strings.VOICE2ChoiceLabel;
                case AuthenticationResponseKind.VoiceTwoWayOfficePlusPin:
                case AuthenticationResponseKind.VoiceTwoWayOffice:
                    return Resources.az_strings.VOICE3ChoiceLabel;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// GetUIWarningInternetLabel method implementation
        /// </summary>
        public override string GetUIWarningInternetLabel(AuthenticationContext ctx)
        {
            Resources.az_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.az_strings.GLOBALWarnOverNetwork;
        }

        /// <summary>
        /// GetUIWarningThirdPartyLabel method implementation
        /// </summary>
        public override string GetUIWarningThirdPartyLabel(AuthenticationContext ctx)
        {
            Resources.az_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.az_strings.GLOBALWarnThirdParty;
        }

        /// <summary>
        /// GetUIDefaultChoiceLabel method implementation
        /// </summary>
        public override string GetUIDefaultChoiceLabel(AuthenticationContext ctx)
        {
            Resources.az_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.az_strings.GLOBALListChoiceDefaultLabel;
        }

        /// <summary>
        /// GetUIAccountManagementLabel method implementation
        /// </summary>
        public override string GetUIAccountManagementLabel(AuthenticationContext ctx)
        {
            Resources.az_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.az_strings.GLOBALManagement;
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
                Enabled = false;
                return false;
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
                return false;
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
                        PinRequired = az.PinRequired;
                        AllowEnrollment = az.EnrollWizard;
                        ForceEnrollment = az.ForceWizard;
                        _sasprovider = new NeosSasProvider(TenantId, ClientId, CertId);
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
                response = this._sasprovider.GetAvailableAuthenticationMethods(request);
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
                response = this._sasprovider.BeginTwoWayAuthentication(request);
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
                    response = this._sasprovider.EndTwoWayAuthentication(request);
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
