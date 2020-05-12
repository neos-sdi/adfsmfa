//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using PhoneNumbers;
using Neos.IdentityServer.MultiFactor.SMS.Resources;
using System.Globalization;
using Neos.IdentityServer.MultiFactor.Common;

//******************************************************************************************************************************************************************************************//
//                                                                                                                                                                                          //  
// This implementation was made for Azure MFA SDK, This SDK wil be removed by Microsoft in autumn 2018.                                                                                     //
// This provider is still provided, for all of you who have not yet been able to migrate to the new model provided by Microsoft.                                                            //
// For example ADFS 3.0 (Windows 2012 R2) still uses MFA components using the MFA SDK.                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
namespace Neos.IdentityServer.Multifactor.SMS
{
    /// <summary>
    /// NeosSMSProvider class implmentation
    /// Legacy Azure MFA Demo, new model for SAS providers
    /// </summary>
    public class NeosSMSProvider : BaseExternalProvider
    {
        private ExternalOTPProvider Data = null;
        private bool _isinitialized = false;
        private ForceWizardMode _forceenrollment = ForceWizardMode.Disabled;

        /// <summary>
        /// Kind property implementation
        /// </summary>
        public override PreferredMethod Kind
        {
            get { return PreferredMethod.External; }
        }

        /// <summary>
        /// IsBuiltIn property implementation
        /// </summary>
        public override bool IsBuiltIn
        {
            get { return true; }
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
        /// AllowOverride property implmentation
        /// </summary>
        public override bool AllowOverride
        {
            get
            {
                if (IsInitialized)
                    return true;
                else
                    return false;
            }
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
            get { return _forceenrollment; }
            set { _forceenrollment = value; }
        }

        /// <summary>
        /// IsTwoWayByDefault  property implementation
        /// </summary>
        public override bool IsTwoWayByDefault
        {
            get { return this.Data.IsTwoWay; }
        }

        /// <summary>
        /// Name property implementation
        /// </summary>
        public override string Name
        {
            get { return "Neos.Provider.Azure.External"; }
        }

        /// <summary>
        /// Decscription property implementation
        /// </summary>
        public override string Description
        {
            get { return "SMS Multi-Factor Provider"; }
        }

        /// <summary>
        /// GetUILabel method implementation
        /// </summary>
        public override string GetUILabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "SMSUIOTPLabel");
        }

        /// <summary>
        /// GetWizardUILabel method implementation
        /// </summary>
        public override string GetWizardUILabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "SMSUIWIZLabel");
        }

        /// <summary>
        /// GetWizardLinkLabel method implementation
        /// </summary>
        public override string GetWizardLinkLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "SMSWIZEnroll");
        }

        /// <summary>
        /// GetUICFGLabel method implementation
        /// </summary>
        public override string GetUICFGLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "SMSUICFGLabel");
        }

        /// <summary>
        /// GetUIMessage method implementation
        /// </summary>
        public override string GetUIMessage(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            if (string.IsNullOrEmpty(ctx.PhoneNumber))
                return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "SMSUIMessage"), string.Empty);
            else
                return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "SMSUIMessage"), Utilities.StripPhoneNumber(ctx.PhoneNumber));
        }

        /// <summary>
        /// GetUIListOptionLabel method implementation
        /// </summary>
        public override string GetUIListOptionLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "SMSUIListOptionLabel");
        }

        /// <summary>
        /// GetUIListChoiceLabel method implementation
        /// </summary>
        public override string GetUIListChoiceLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "SMSUIListChoiceLabel");
        }


        /// <summary>
        /// GetUIConfigLabel method implementation
        /// </summary>
        public override string GetUIConfigLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            if (string.IsNullOrEmpty(ctx.ExtraInfos))
                return Resources.GetString(ResourcesLocaleKind.Html, "SMSUIConfigLabel");
            else
                return string.Format(Resources.GetString(ResourcesLocaleKind.Html, "SMSUIConfigLabel2"), Utilities.StripPhoneNumber(ctx.ExtraInfos));
        }

        /// <summary>
        /// GetUIChoiceLabel method implementation
        /// </summary>
        public override string GetUIChoiceLabel(AuthenticationContext ctx, AvailableAuthenticationMethod method)
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
                case AuthenticationResponseKind.SmsOTP:
                case AuthenticationResponseKind.SmsOneWayOTP:
                    return Resources.GetString(ResourcesLocaleKind.Html, "SMSUIChoiceLabel");
                case AuthenticationResponseKind.SmsTwoWayOTP:
                    return Resources.GetString(ResourcesLocaleKind.Html, "SMSUIChoiceLabel2");
                default:
                    return Resources.GetString(ResourcesLocaleKind.Html, "SMSUIChoiceLabel");
            }
        }

        /// <summary>
        /// GetUIWarningInternetLabel method implmentation
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
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "SMSUIEnrollTaskLabel");
        }

        /// <summary>
        /// GetUIEnrollValidatedLabel method implementation
        /// </summary>
        public override string GetUIEnrollValidatedLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "SMSUIEnrollValidatedLabel");
        }

        /// <summary>
        /// GetUIAccountManagementLabel method implementation
        /// </summary>
        public override string GetUIAccountManagementLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "SMSUIManagement");
        }

        /// <summary>
        /// GetAccountManagementUrl() method implmentation
        /// </summary>
        public override string GetAccountManagementUrl(AuthenticationContext ctx)
        {
            return null;
        }

        /// <summary>
        /// IsAvailable method implmentation
        /// </summary>
        public override bool IsAvailable(AuthenticationContext ctx)
        {
            return true;
        }

        /// <summary>
        /// IsAvailableForUser method implmentation
        /// </summary>
        public override bool IsAvailableForUser(AuthenticationContext ctx)
        {
            return Utilities.ValidatePhoneNumber(ctx.PhoneNumber, true);
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
                case RequiredMethodElements.PhoneParameterRequired:
                    return true;
                case RequiredMethodElements.PinInputRequired:
                    return this.PinRequired;
                case RequiredMethodElements.PinParameterRequired:
                    return this.PinRequired;
                case RequiredMethodElements.PhoneLinkRequired:
                    return true;
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
                    if (externalsystem is ExternalProviderParams)
                    {
                        ExternalProviderParams param = externalsystem as ExternalProviderParams;
                        Data = param.Data;

                        Enabled = param.Enabled;
                        IsRequired = param.IsRequired;
                        WizardEnabled = param.EnrollWizard;
                        ForceEnrollment = param.ForceWizard;
                        PinRequired = param.PinRequired;
                        _isinitialized = true;
                        return;
                    }
                    else
                        throw new InvalidCastException("Invalid SMS/External Provider !");
                }
            }
            catch (Exception ex)
            {
                this.Enabled = false;
                throw ex;
            }
        }

        /// <summary>
        /// GetAuthenticationContext method implementation
        /// </summary>
        public override void GetAuthenticationContext(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            AvailableAuthenticationMethod result = GetSelectedAuthenticationMethod(ctx);
            ctx.PinRequired = result.RequiredPin;
            ctx.IsRemote = result.IsRemote;
            ctx.IsTwoWay = result.IsTwoWay;
            ctx.IsSendBack = result.IsSendBack;
            ctx.PreferredMethod = ctx.PreferredMethod;
            ctx.SelectedMethod = result.Method;
            ctx.ExtraInfos = result.ExtraInfos;
        }

        /// <summary>
        /// PostAutnenticationRequest method implmentation
        /// </summary>
        public override int PostAuthenticationRequest(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            if (ctx.SelectedMethod == AuthenticationResponseKind.Error)
                GetAuthenticationContext(ctx);

            ctx.Notification = GetUserCodeWithExternalSystem(ctx, this.Data, new CultureInfo(ctx.Lcid));
            ctx.SessionId = Guid.NewGuid().ToString();
            ctx.SessionDate = DateTime.Now;
            if (ctx.Notification == (int)AuthenticationResponseKind.Error)
                return (int)(int)AuthenticationResponseKind.Error;
            else
                return (int)ctx.SelectedMethod;
        }

        /// <summary>
        /// SetAuthenticationResult method implmentation
        /// </summary>
        public override int SetAuthenticationResult(AuthenticationContext ctx, string pin)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            if (ctx.IsTwoWay)
                return (int)AuthenticationResponseKind.SmsTwoWayOTP;
            else
            {
                if (!string.IsNullOrEmpty(pin))
                {
                    int value = Convert.ToInt32(pin);
                    if (CheckPin(ctx, value))
                        return (int)AuthenticationResponseKind.SmsOneWayOTP;
                    else
                        return (int)AuthenticationResponseKind.Error;
                }
                else
                    return (int)AuthenticationResponseKind.Error;
            }
        }

        /// <summary>
        /// GetAuthenticationMethods method implmentation
        /// </summary>
        public override List<AvailableAuthenticationMethod> GetAuthenticationMethods(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            List<AvailableAuthenticationMethod> result = GetSessionData(this.Kind, ctx);
            if (result != null)
                return result;
            else
            {
                result = new List<AvailableAuthenticationMethod>();
                AvailableAuthenticationMethod item1 = new AvailableAuthenticationMethod();
                item1.IsDefault = !this.Data.IsTwoWay;
                item1.IsRemote = true;
                item1.IsTwoWay = false;
                item1.IsSendBack = false;
                item1.RequiredPin = false;
                item1.RequiredEmail = false;
                item1.RequiredPhone = true;
                item1.RequiredCode = true;
                item1.ExtraInfos = ctx.PhoneNumber;
                item1.Method = AuthenticationResponseKind.SmsOneWayOTP;
                result.Add(item1);

                if (this.Data.IsTwoWay)
                {
                    AvailableAuthenticationMethod item2 = new AvailableAuthenticationMethod();
                    item2.IsDefault = this.Data.IsTwoWay;
                    item2.IsRemote = true;
                    item2.IsTwoWay = true;
                    item2.IsSendBack = false;
                    item2.RequiredPin = false;
                    item2.RequiredEmail = false;
                    item2.RequiredPhone = true;
                    item2.RequiredCode = false;
                    item2.ExtraInfos = ctx.PhoneNumber;
                    item2.Method = AuthenticationResponseKind.SmsTwoWayOTP;
                    result.Add(item2);
                }
                SaveSessionData(this.Kind, ctx, result);
            }
            return result;

        }

        /// <summary>
        /// CheckPin method implementation
        /// </summary>
        private bool CheckPin(AuthenticationContext ctx, int pin)
        {
            if (ctx.Notification > (int)AuthenticationResponseKind.Error)
            {
                string generatedpin = ctx.Notification.ToString("D6");  // eg : transmitted by email or by External System (SMS)
                return (pin == Convert.ToInt32(generatedpin));
            }
            else
                return false;
        }

        /// <summary>
        /// GetUserCodeWithExternalSystem method implementation for Azure MFA 
        /// </summary>
        private int GetUserCodeWithExternalSystem(AuthenticationContext ctx, ExternalOTPProvider externalsys, CultureInfo culture)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            String NumberStr = ctx.PhoneNumber;
            int CountryCode = 0;
            ulong NationalNumber = 0;
            string extension = string.Empty;

            PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();
            PhoneNumber NumberProto = phoneUtil.Parse(NumberStr, culture.TwoLetterISOLanguageName.ToUpper());
            CountryCode = NumberProto.CountryCode;
            NationalNumber = NumberProto.NationalNumber;
            if (NumberProto.HasExtension)
                extension = NumberProto.Extension;

            PhoneFactor.Initialize(externalsys);
            PhoneFactorParams Params = new PhoneFactorParams();
            Params.Username = ctx.UPN;

            Params.CountryCode = CountryCode.ToString();
            Params.Phone = NationalNumber.ToString();
            Params.Extension = extension;
            Params.ApplicationName = "IdentityServer";
            Params.Sha1Salt = externalsys.Sha1Salt;

            if (ctx.IsTwoWay)
            {
                Params.SmsText = string.Format(Resources.GetString(ResourcesLocaleKind.Azure, "SMSTwoWayMessage"), externalsys.Company);
                Params.Mode = PhoneFactor.MODE_SMS_TWO_WAY_OTP;               
            }
            else
            {
                Params.SmsText = string.Format(Resources.GetString(ResourcesLocaleKind.Azure, "SMSMessage"), externalsys.Company);
                Params.Mode = PhoneFactor.MODE_SMS_ONE_WAY_OTP;
            }

            int callStatus;
            int errorId;
            string otp = string.Empty;
            if (PhoneFactor.Authenticate(Params, out otp, out callStatus, out errorId, externalsys.Timeout))
                if (ctx.IsTwoWay)
                    return (int)AuthenticationResponseKind.SmsTwoWayOTP;                   
                else
                    return Convert.ToInt32(otp);
            else
                return (int)AuthenticationResponseKind.Error;
        }
    }

    /// <summary>
    /// Legacy Azure MFA Demo, Legacy Interface
    /// </summary>
    public class SMSCall : IExternalOTPProvider
    {
        /// <summary>
        /// GetUserCodeWithExternalSystem method implementation for Azure MFA 
        /// </summary>
        public int GetUserCodeWithExternalSystem(string upn, string phonenumber, string smstext, ExternalOTPProvider externalsys, CultureInfo culture)
        {
            ResourcesLocale Resources = new ResourcesLocale(culture.LCID);
            String NumberStr = phonenumber;
            int CountryCode = 0;
            ulong NationalNumber = 0;
            string extension = string.Empty;

            PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();
            PhoneNumber NumberProto = phoneUtil.Parse(NumberStr, culture.TwoLetterISOLanguageName.ToUpper());
            CountryCode = NumberProto.CountryCode;
            NationalNumber = NumberProto.NationalNumber;
            if (NumberProto.HasExtension)
                extension = NumberProto.Extension;

            PhoneFactor.Initialize(externalsys);
            PhoneFactorParams Params = new PhoneFactorParams();
            Params.Username = upn;

            Params.CountryCode = CountryCode.ToString();
            Params.Phone = NationalNumber.ToString();
            Params.Extension = extension;
            Params.ApplicationName = "IdentityServer";
            Params.Sha1Salt = externalsys.Sha1Salt;

            if (externalsys.IsTwoWay)
            {
                Params.SmsText = string.Format(Resources.GetString(ResourcesLocaleKind.Azure, "SMSTwoWayMessage"), externalsys.Company);
                Params.Mode = PhoneFactor.MODE_SMS_TWO_WAY_OTP;
            }
            else
            {
                Params.SmsText = string.Format(Resources.GetString(ResourcesLocaleKind.Azure, "SMSMessage"), externalsys.Company);
                Params.Mode = PhoneFactor.MODE_SMS_ONE_WAY_OTP;
            }

            int callStatus;
            int errorId;
            string otp = string.Empty;
            if (PhoneFactor.Authenticate(Params, out otp, out callStatus, out errorId, externalsys.Timeout))
                if (externalsys.IsTwoWay)
                    return (int)AuthenticationResponseKind.SmsTwoWayOTP;
                else
                    return Convert.ToInt32(otp);
            else
                return (int)AuthenticationResponseKind.Error;
        }

        /// <summary>
        /// GetCodeWithExternalSystem method implementation for Azure MFA 
        /// </summary>
        public AuthenticationResponseKind GetCodeWithExternalSystem(MFAUser reg, ExternalOTPProvider externalsys, CultureInfo culture, out int otp)
        {
            throw new NotImplementedException();
        }
    }
}
