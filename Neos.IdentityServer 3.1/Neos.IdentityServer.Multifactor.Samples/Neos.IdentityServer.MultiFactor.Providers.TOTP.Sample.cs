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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor.Common;
using System.Windows.Threading;
using System.Globalization;

namespace Neos.IdentityServer.MultiFactor.Samples
{
    ///----------------------------------------------------------------------------------------------------------------------------------///
    /// Sample to demonstrate the implementation of an TOTP Provider for MFA                                                             ///
    ///                                                                                                                                  ///    
    /// This can be a base of inspiration if you have specific needs about security                                                      ///   
    /// In theory, you should put all the strings in ressources, for best reading we let these string as constants in th source code     ///
    ///                                                                                                                                  ///    
    /// 4 for four digits                                                                                                                ///
    /// 30 for 30 seconds                                                                                                                ///
    ///                                                                                                                                  ///    
    /// When testing your Authenticator probably show you a 6 digit number. no matter, just put the last four digits when login          /// 
    /// Remember it's just a quick sample, but the idea, is to deal with other token like hardware dongles (6 or 8 digits and 30 or 60 s ///
    /// ---------------------------------------------------------------------------------------------------------------------------------///
   
    /// <summary>
    /// NeosOTPProvider430 class implementation
    /// </summary>
    public class NeosOTPProvider430 : BaseExternalProvider, ITOTPProviderParameters
    {
        private int TOTPShadows = 2;
        private HashMode Algorithm = HashMode.SHA1;
        private bool _isinitialized = false;
        private ForceWizardMode _forceenrollment = ForceWizardMode.Disabled;

        /// <summary>
        /// TOTP Duration in seconds
        /// </summary>
        public int Duration
        {
            get { return 30; }
        }

        /// <summary>
        /// TOTP Digits numbers count
        /// </summary>
        public int Digits
        {
            get { return 4; }
        }

        /// <summary>
        /// Kind property implementation
        /// </summary>
        public override PreferredMethod Kind
        {
            get { return PreferredMethod.Code; }
        }

        /// <summary>
        /// IsBuiltIn property implementation
        /// </summary>
        public override bool IsBuiltIn
        {
            get { return false; }
        }

        /// <summary>
        /// CanBeDisabled property implementation
        /// </summary>
        public override bool AllowDisable
        {
            get { return false; }
        }

        /// <summary>
        /// IsInitialized property implmentation
        /// </summary>
        public override bool IsInitialized
        {
            get { return _isinitialized; }
        }

        /// <summary>
        /// AllowOverride property implmentation
        /// </summary>
        public override bool AllowOverride
        {
            get { return false; }
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
        /// Name property implementation
        /// </summary>
        public override string Name
        {
            get { return "Neos.Provider.Totp.430"; }
        }

        /// <summary>
        /// Description property implementation
        /// </summary>
        public override string Description
        {
            get
            {
                string independent = "TOTP Multi-Factor Provider 430";
                ResourcesLocale Resources = null;
                if (CultureInfo.DefaultThreadCurrentUICulture != null)
                    Resources = new ResourcesLocale(CultureInfo.DefaultThreadCurrentUICulture.LCID);
                else
                    Resources = new ResourcesLocale(CultureInfo.CurrentUICulture.LCID);
                string res = Resources.GetString(ResourcesLocaleKind.Html, "PROVIDEROTPDESCRIPTION");
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
            return Resources.GetString(ResourcesLocaleKind.Html, "OTPUIOTPLabel");
        }

        /// <summary>
        /// GetWizardUILabel method implementation
        /// </summary>
        public override string GetWizardUILabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "OTPUIWIZLabel");
        }

        /// <summary>
        /// GetWizardUIComment method implementation
        /// </summary>
        public override string GetWizardUIComment(AuthenticationContext ctx) 
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "OTPUIWIZComment");
        }

        /// <summary>
        /// GetWizardLinkLabel method implementation
        /// </summary>
        public override string GetWizardLinkLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "OTPUIWIZLabel");
        }

        /// <summary>
        /// GetUICFGLabel method implementation
        /// </summary>
        public override string GetUICFGLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "OTPUICFGLabel");
        }

        /// <summary>
        /// GetUIMessage method implementation
        /// </summary>
        public override string GetUIMessage(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "OTPUIMessage");
        }

        /// <summary>
        /// GetUIListOptionLabel method implementation
        /// </summary>
        public override string GetUIListOptionLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "OTPUIListOptionLabel");
        }

        /// <summary>
        /// GetUIListChoiceLabel method implementation
        /// </summary>
        public override string GetUIListChoiceLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "OTPUIListChoiceLabel");
        }

        /// <summary>
        /// GetUIConfigLabel method implementation
        /// </summary>
        public override string GetUIConfigLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "OTPUIConfigLabel");
        }

        /// <summary>
        /// GetUIChoiceLabel method implementation
        /// </summary>
        public override string GetUIChoiceLabel(AuthenticationContext ctx, AvailableAuthenticationMethod method = null)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "OTPUIChoiceLabel");
        }

        /// <summary>
        /// GetUIWarningInternetLabel method implmentation
        /// </summary>
        public override string GetUIWarningInternetLabel(AuthenticationContext ctx)
        {
            return string.Empty;
        }

        /// <summary>
        /// GetUIWarningThirdPartyLabel method implementation
        /// </summary>
        public override string GetUIWarningThirdPartyLabel(AuthenticationContext ctx)
        {
            return string.Empty;
        }

        /// <summary>
        /// GetUIDefaultChoiceLabel method implementation
        /// </summary>
        public override string GetUIDefaultChoiceLabel(AuthenticationContext ctx)
        {
            return string.Empty;
        }

        /// <summary>
        /// GetUIAccountManagementLabel method implementation
        /// </summary>
        public override string GetUIAccountManagementLabel(AuthenticationContext ctx)
        {
            return string.Empty;
        }

        /// <summary>
        /// GetUIEnrollmentTaskLabel method implementation
        /// </summary>
        public override string GetUIEnrollmentTaskLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "OTPUIEnrollTaskLabel");
        }

        /// <summary>
        /// GetUIEnrollValidatedLabel method implementation
        /// </summary>
        public override string GetUIEnrollValidatedLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "OTPUIEnrollValidatedLabel");
        }

        /// <summary>
        /// GetAccountManagementUrl method implmentation
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
            return (ctx.KeyStatus == SecretKeyStatus.Success);
        }

        /// <summary>
        /// IsMethodElementRequired implementation
        /// </summary>
        public override bool IsUIElementRequired(AuthenticationContext ctx, RequiredMethodElements element)
        {
            switch (element)
            {
                case RequiredMethodElements.CodeInputRequired:
                    return true;
                case RequiredMethodElements.KeyParameterRequired:
                    return true;
                case RequiredMethodElements.OTPLinkRequired:
                    return true;
                case RequiredMethodElements.PinLinkRequired:
                    return this.PinRequired;
                case RequiredMethodElements.PinParameterRequired:
                    return this.PinRequired;
                case RequiredMethodElements.PinInputRequired:
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
                    if (externalsystem is OTPProviderParams)
                    {
                        OTPProviderParams param = externalsystem as OTPProviderParams;
                        TOTPShadows = param.TOTPShadows;
                        Algorithm = param.Algorithm;
                        Enabled = param.Enabled;
                        WizardEnabled = param.EnrollWizard;
                        ForceEnrollment = param.ForceWizard;
                        PinRequired = param.PinRequired;
                        _isinitialized = true;
                        return;
                    }
                    else
                        throw new InvalidCastException("Invalid OTP Provider !");
                }
            }
            catch (Exception ex)
            {
                this.Enabled = false;
                throw ex;
            }
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
                AvailableAuthenticationMethod item = new AvailableAuthenticationMethod();
                item.IsDefault = true;
                item.IsRemote = false;
                item.IsTwoWay = false;
                item.IsSendBack = false;
                item.RequiredEmail = false;
                item.RequiredPhone = false;
                item.RequiredCode = true;
                item.Method = AuthenticationResponseKind.PhoneAppOTP;
                item.RequiredPin = false;
                result.Add(item);
                SaveSessionData(this.Kind, ctx, result);
            }
            return result;
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
            ctx.Notification = (int)ctx.SelectedMethod;
            ctx.SessionId = Guid.NewGuid().ToString();
            ctx.SessionDate = DateTime.Now;
            return (int)ctx.SelectedMethod;
        }

        /// <summary>
        /// SetAuthenticationResult method implmentation
        /// </summary>
        public override int SetAuthenticationResult(AuthenticationContext ctx, string pin)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");
            int value = Convert.ToInt32(pin);
            if (CheckPin(ctx, value))
                return (int)AuthenticationResponseKind.PhoneAppOTP;
            else
                return (int)AuthenticationResponseKind.Error;
        }

        /// <summary>
        /// CheckPin method inplementation
        /// </summary>
        private bool CheckPin(AuthenticationContext usercontext, int pin)
        {
            foreach (HashMode algo in Enum.GetValues(typeof(HashMode)))
            {
                if (algo <= Algorithm)
                {
                    if (TOTPShadows <= 0)
                    {
                        if (!KeysManager.ValidateKey(usercontext.UPN))
                            throw new CryptographicException(string.Format("SECURTY ERROR : Invalid Key for User {0}", usercontext.UPN));
                        byte[] encodedkey = KeysManager.ProbeKey(usercontext.UPN);
                        DateTime call = DateTime.UtcNow;
                        TOTP gen = new TOTP(encodedkey, usercontext.UPN, call, algo, this.Duration, this.Digits);  // eg : TOTP code
                        gen.Compute(call);
                        return (pin == gen.OTP);
                    }
                    else
                    {   // Current TOTP
                        if (!KeysManager.ValidateKey(usercontext.UPN))
                            throw new CryptographicException(string.Format("SECURTY ERROR : Invalid Key for User {0}", usercontext.UPN));
                        byte[] encodedkey = KeysManager.ProbeKey(usercontext.UPN);
                        DateTime tcall = DateTime.UtcNow;
                        TOTP gen = new TOTP(encodedkey, usercontext.UPN, tcall, algo, this.Duration, this.Digits);  // eg : TOTP code
                        gen.Compute(tcall);
                        if (pin == gen.OTP)
                            return true;
                        // TOTP with Shadow (current - x latest)
                        for (int i = 1; i <= TOTPShadows; i++)
                        {
                            DateTime call = tcall.AddSeconds(-(i * this.Duration));
                            TOTP gen2 = new TOTP(encodedkey, usercontext.UPN, call, algo, this.Duration, this.Digits);  // eg : TOTP code
                            gen2.Compute(call);
                            if (pin == gen2.OTP)
                                return true;
                        }
                        // TOTP with Shadow (current + x latest) - not possible. but can be usefull if time sync is not adequate
                        for (int i = 1; i <= TOTPShadows; i++)
                        {
                            DateTime call = tcall.AddSeconds(i * this.Duration);
                            TOTP gen3 = new TOTP(encodedkey, usercontext.UPN, call, algo, this.Duration, this.Digits);  // eg : TOTP code
                            gen3.Compute(call);
                            if (pin == gen3.OTP)
                                return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
