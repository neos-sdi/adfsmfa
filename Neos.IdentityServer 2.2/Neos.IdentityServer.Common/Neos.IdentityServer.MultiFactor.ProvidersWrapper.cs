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
using Neos.IdentityServer.MultiFactor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Neos.IdentityServer.MultiFactor
{
    public enum RequiredMethodElements
    {
        // Identification
        KeyInputRequired = 0,
        CodeInputRequired = 1,
        PinInputRequired = 2,
        BiometricInputRequired = 3,
        // registration
        KeyParameterRequired = 10,
        EmailParameterRequired = 11,
        PhoneParameterRequired = 12,
        BiometricParameterRequired = 13,
        PinParameterRequired = 14,
        // Enrollment
        OTPLinkRequired = 100,
        BiometricLinkRequired = 101,
        EmailLinkRequired = 102,
        PhoneLinkRequired = 103,
        PinLinkRequired = 104
    }

    /// <summary>
    /// BaseExternalProvider class implementation
    /// </summary>
    public abstract class BaseExternalProvider : IExternalProvider
    {
        private bool _enabled;
        private bool _pinenabled = false;
        public abstract PreferredMethod Kind { get; }
        public abstract bool IsInitialized { get; }
        public abstract bool AllowDisable { get; }
        public abstract bool AllowOverride { get; }
        public abstract bool AllowEnrollment { get; set; }
        public abstract bool EnrollmentNeverUseOptions { get; set; }
        public abstract string Name { get; }
        public abstract string Description { get; }

        public abstract string GetUILabel(AuthenticationContext ctx);
        public abstract string GetUICFGLabel(AuthenticationContext ctx);
        public abstract string GetUIMessage(AuthenticationContext ctx);
        public abstract string GetUIListOptionLabel(AuthenticationContext ctx);
        public abstract string GetUIListChoiceLabel(AuthenticationContext ctx);
        public abstract string GetUIConfigLabel(AuthenticationContext ctx);
        public abstract string GetUIChoiceLabel(AuthenticationContext ctx, AvailableAuthenticationMethod method = null);
        public abstract string GetUIWarningInternetLabel(AuthenticationContext ctx);
        public abstract string GetUIWarningThirdPartyLabel(AuthenticationContext ctx);
        public abstract string GetUIDefaultChoiceLabel(AuthenticationContext ctx);
        public abstract string GetUIAccountManagementLabel(AuthenticationContext ctx);
        public abstract string GetAccountManagementUrl(AuthenticationContext ctx);
        public abstract bool IsAvailable(AuthenticationContext ctx);
        public abstract bool IsAvailableForUser(AuthenticationContext ctx);
        public abstract bool IsUIElementRequired(AuthenticationContext ctx, RequiredMethodElements element);
        public abstract void Initialize(BaseProviderParams externalsystem);
        public abstract int PostAuthenticationRequest(AuthenticationContext ctx);
        public abstract int SetAuthenticationResult(AuthenticationContext ctx, string result);
        public abstract List<AvailableAuthenticationMethod> GetAuthenticationMethods(AuthenticationContext ctx);

        /// <summary>
        /// Enabled property implmentation
        /// </summary>
        public virtual bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// PinRequired  property implementation
        /// </summary>
        public virtual bool PinRequired 
        {
            get { return _pinenabled; }
            set { _pinenabled = value; }
        }

        /// <summary>
        /// IsTwoWayByDefault  property implementation
        /// </summary>
        public virtual bool IsTwoWayByDefault
        {
            get { return false; }
        }

        public static string GetPINLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.GLOBALPINLabel;
        }

        /// <summary>
        /// GetAuthenticationContext method implementation
        /// </summary>
        public virtual void GetAuthenticationContext(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

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
        /// GetDefaultAuthenticationMethod method implmentation
        /// </summary>
        public virtual AvailableAuthenticationMethod GetDefaultAuthenticationMethod(AuthenticationContext ctx)
        {
            if (!this.IsInitialized)
                throw new Exception("Provider not initialized !");

            AvailableAuthenticationMethod result = new AvailableAuthenticationMethod();
            List<AvailableAuthenticationMethod> lst = GetAuthenticationMethods(ctx);
            if (lst != null)
            {
                foreach (AvailableAuthenticationMethod current in lst)
                {
                    if (current.IsDefault)
                    {
                        result = current;
                        result.IsDefault = true;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// GetSelectedAuthenticationMethod method implmentation
        /// </summary>
        public virtual AvailableAuthenticationMethod GetSelectedAuthenticationMethod(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            AvailableAuthenticationMethod result = null;
            List<AvailableAuthenticationMethod> lst = GetAuthenticationMethods(ctx);
            AuthenticationResponseKind ov = GetOverrideMethod(ctx);
            if (lst != null)
            {
                AvailableAuthenticationMethod save = null;
                foreach (AvailableAuthenticationMethod current in lst)
                {
                    if (current.IsDefault)
                        save = current;
                    if (ov == AuthenticationResponseKind.Error)
                    {
                        if (current.IsDefault)
                        {
                            result = current;
                            result.IsDefault = true;
                            break;
                        }
                    }
                    else
                    {
                        if (current.Method == ov)
                        {
                            result = current;
                            break;
                        }
                    }
                }
                if (result==null) // Override not correct, we must have a default 
                {
                    result = save;
                    SetOverrideMethod(ctx, AuthenticationResponseKind.Error);
                }
                return result;
            }
            else
                return null;
        }

        /// <summary>
        /// SetSelectedAuthenticationMethod method implementation
        /// </summary>
        public virtual bool SetSelectedAuthenticationMethod(AuthenticationContext ctx, AuthenticationResponseKind method, bool updateoverride = false)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            List<AvailableAuthenticationMethod> lst = GetAuthenticationMethods(ctx);
            if (lst != null)
            {
                foreach (AvailableAuthenticationMethod current in lst)
                {
                    if (current.Method == method)
                    {
                        ctx.IsRemote = current.IsRemote;
                        ctx.IsSendBack = current.IsSendBack;
                        ctx.IsTwoWay = current.IsTwoWay;
                        ctx.SelectedMethod = method;

                        if (updateoverride)
                            SetOverrideMethod(ctx, method);
                        else
                            SetOverrideMethod(ctx, AuthenticationResponseKind.Default);
                        break;
                    }
                }
                return true;
            }
            else
                return false;

        }

        /// <summary>
        /// GetOverrideMethod method implmentation
        /// </summary>
        public virtual AuthenticationResponseKind GetOverrideMethod(AuthenticationContext ctx)
        {
            return GetOverrideMethodData(ctx, this.Kind);
        }

        /// <summary>
        /// SetOverrideMethod method implementation
        /// </summary>
        public virtual void SetOverrideMethod(AuthenticationContext ctx, AuthenticationResponseKind value)
        {
            SetOverrideMethodData(ctx, this.Kind, value);
        }

        /// <summary>
        /// SaveSessionData implmentation
        /// </summary>
        protected void SaveSessionData(PreferredMethod kind, AuthenticationContext ctx, List<AvailableAuthenticationMethod> methods)
        {
            Dictionary<string, List<AvailableAuthenticationMethod>> cachedmethods = GetAllSessionData(ctx);
            if (cachedmethods == null)
            {
                cachedmethods = new Dictionary<string, List<AvailableAuthenticationMethod>>();
                cachedmethods.Add(kind.ToString(), methods);
            }
            else
            {
                if (cachedmethods.ContainsKey(kind.ToString()))
                    cachedmethods[kind.ToString()] = methods;
                else
                    cachedmethods.Add(kind.ToString(), methods);
            }
            SaveAllSessionData(ctx, cachedmethods);
        }

        /// <summary>
        /// GetSessionData implmentation
        /// </summary>
        protected List<AvailableAuthenticationMethod> GetSessionData(PreferredMethod kind, AuthenticationContext ctx)
        {
            Dictionary<string, List<AvailableAuthenticationMethod>> cachedmethods = GetAllSessionData(ctx);
            if (cachedmethods != null)
            {
                List<AvailableAuthenticationMethod> methods = null;
                if (cachedmethods.TryGetValue(kind.ToString(), out methods))
                    return methods;
                else
                    return null;
            }
            else
                return null;
        }

        #region private methods
        /// <summary>
        /// GetOverrideMethodData method implmentation
        /// </summary>
        private AuthenticationResponseKind GetOverrideMethodData(AuthenticationContext ctx, PreferredMethod kind)
        {
            string json = ctx.OverrideMethod;
            try
            {
                if (!string.IsNullOrEmpty(json))
                {
                    Dictionary<string, AuthenticationResponseKind> lst = new JavaScriptSerializer().Deserialize<Dictionary<string, AuthenticationResponseKind>>(json);
                    if (lst.ContainsKey(kind.ToString()))
                    {
                        AuthenticationResponseKind res = AuthenticationResponseKind.Error;
                        if (lst.TryGetValue(kind.ToString(), out res))
                            return res;
                    }
                }
                return AuthenticationResponseKind.Error;
            }
            catch // Bad Data
            {
                return AuthenticationResponseKind.Error;
            }
        }

        /// <summary>
        /// SetOverrideMethodData method implementation
        /// </summary>
        private void SetOverrideMethodData(AuthenticationContext ctx, PreferredMethod kind, AuthenticationResponseKind value)
        {
            Dictionary<string, AuthenticationResponseKind> lst = new Dictionary<string, AuthenticationResponseKind>();
            string json = ctx.OverrideMethod;
            if (!string.IsNullOrEmpty(json))
            {
                lst = new JavaScriptSerializer().Deserialize<Dictionary<string, AuthenticationResponseKind>>(json);
                if (lst != null)
                {
                    if (lst.ContainsKey(kind.ToString()))
                    {
                        if (value != AuthenticationResponseKind.Default)
                            lst[kind.ToString()] = value;
                        else
                            lst.Remove(kind.ToString());
                    }
                    else
                    {
                        if (value != AuthenticationResponseKind.Default)
                            lst.Add(kind.ToString(), value);
                    }
                }
                else
                    return;
            }
            else
            {
                if (value != AuthenticationResponseKind.Default)
                    lst.Add(kind.ToString(), value);
                else
                    return;
            }
            json = new JavaScriptSerializer().Serialize(lst).Trim();
            ctx.OverrideMethod = json;
        }

        /// <summary>
        /// SaveAllSessionData implmentation
        /// </summary>
        private void SaveAllSessionData(AuthenticationContext ctx, Dictionary<string, List<AvailableAuthenticationMethod>> methods)
        {
            string json = new JavaScriptSerializer().Serialize(methods).Trim();
            ctx.SessionData = json;
        }

        /// <summary>
        /// GetAllSessionData implmentation
        /// </summary>
        private Dictionary<string, List<AvailableAuthenticationMethod>> GetAllSessionData(AuthenticationContext ctx)
        {
            string json = ctx.SessionData;
            try
            {
                if (string.IsNullOrEmpty(json))
                    return null;
                else
                    return new JavaScriptSerializer().Deserialize<Dictionary<string, List<AvailableAuthenticationMethod>>>(json);
            }
            catch // Bad Data
            {
                return null; 
            }
        }
        #endregion
    }

    /// <summary>
    /// NeosOTPProvider class implementation
    /// </summary>
    public class NeosOTPProvider: BaseExternalProvider
    {
        private int TOTPShadows = 2;
        private HashMode Algorithm = HashMode.SHA1;
        private bool _isinitialized = false;
        private bool _allowenrollment = true;
        private bool _enrollmentneveruseoptions = false;

        /// <summary>
        /// Kind property implementation
        /// </summary>
        public override PreferredMethod Kind 
        {
            get { return PreferredMethod.Code; }
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
            get { return _allowenrollment; }
            set { _allowenrollment = value; }
        }

        /// <summary>
        /// EnrollmentNeverUseOptions property implementation
        /// </summary>
        public override bool EnrollmentNeverUseOptions
        {
            get { return _enrollmentneveruseoptions; }
            set { _enrollmentneveruseoptions = value; }
        }

        /// <summary>
        /// Name property implementation
        /// </summary>
        public override string Name 
        {
            get { return "Neos.Provider.Totp"; }
        }

        /// <summary>
        /// Description property implementation
        /// </summary>
        public override string Description
        {
            get { return "TOTP Multi-Factor Provider"; }
        }

        /// <summary>
        /// GetUILabel method implementation
        /// </summary>
        public override string GetUILabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.OTPUIOTPLabel;
        }

        /// <summary>
        /// GetUICFGLabel method implementation
        /// </summary>
        public override string GetUICFGLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.OTPUICFGLabel;
        }

        /// <summary>
        /// GetUIMessage method implementation
        /// </summary>
        public override string GetUIMessage(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.OTPUIMessage;
        }

        /// <summary>
        /// GetUIListOptionLabel method implementation
        /// </summary>
        public override string GetUIListOptionLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.OTPUIListOptionLabel;
        }

        /// <summary>
        /// GetUIListChoiceLabel method implementation
        /// </summary>
        public override string GetUIListChoiceLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.OTPUIListChoiceLabel;
        }

        /// <summary>
        /// GetUIConfigLabel method implementation
        /// </summary>
        public override string GetUIConfigLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.OTPUIConfigLabel;
        }

        /// <summary>
        /// GetUIChoiceLabel method implementation
        /// </summary>
        public override string GetUIChoiceLabel(AuthenticationContext ctx, AvailableAuthenticationMethod method = null)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.OTPUIChoiceLabel;
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
                        AllowEnrollment = param.EnrollWizard;
                        EnrollmentNeverUseOptions = param.EnrollWizardStrict;
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
            if (usercontext.SelectedMethod == AuthenticationResponseKind.PhoneAppOTP)  // Using a TOTP Application (Microsoft Authnetication, Google Authentication, etc...)
            {
                foreach (HashMode algo in Enum.GetValues(typeof(HashMode)))
                {
                    if (algo <= Algorithm)
                    {
                        if (TOTPShadows <= 0)
                        {
                            if (!KeysManager.ValidateKey(usercontext.UPN))
                                throw new CryptographicException(string.Format("SECURTY ERROR : Invalid Key for User {0}", usercontext.UPN));
                            string encodedkey = KeysManager.ProbeKey(usercontext.UPN);
                            DateTime call = DateTime.UtcNow;
                            OTPGenerator gen = new OTPGenerator(encodedkey, usercontext.UPN, call, algo);  // eg : TOTP code
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
                            OTPGenerator gen = new OTPGenerator(encodedkey, usercontext.UPN, tcall, algo);  // eg : TOTP code
                            gen.ComputeOTP(tcall);
                            string currentpin = gen.OTP.ToString("D6");
                            if (pin == Convert.ToInt32(currentpin))
                                return true;
                            // TOTP with Shadow (current - x latest)
                            for (int i = 1; i <= TOTPShadows; i++)
                            {
                                DateTime call = tcall.AddSeconds(-(i * OTPGenerator.TOTPDuration));
                                OTPGenerator gen2 = new OTPGenerator(encodedkey, usercontext.UPN, call, algo);  // eg : TOTP code
                                gen2.ComputeOTP(call);
                                string generatedpin = gen2.OTP.ToString("D6");
                                if (pin == Convert.ToInt32(generatedpin))
                                    return true;
                            }
                            // TOTP with Shadow (current + x latest) - not possible. but can be usefull if time sync is not adequate
                            for (int i = 1; i <= TOTPShadows; i++)
                            {
                                DateTime call = tcall.AddSeconds(i * OTPGenerator.TOTPDuration);
                                OTPGenerator gen3 = new OTPGenerator(encodedkey, usercontext.UPN, call, algo);  // eg : TOTP code
                                gen3.ComputeOTP(call);
                                string generatedpin = gen3.OTP.ToString("D6");
                                if (pin == Convert.ToInt32(generatedpin))
                                    return true;
                            }
                        }
                    }
                }
                return false;
            }
            else
                return false;
        }
    }

    /// <summary>
    /// NeosLegacySMSProvider class implmentation
    /// </summary>
    public class NeosLegacySMSProvider: BaseExternalProvider
    {
        private ExternalOTPProvider Data = null;
        private IExternalOTPProvider _sasprovider;
        private bool _isinitialized = false;
        private bool _allowenrollment = true;
        private bool _enrollmentneveruseoptions = false;

        /// <summary>
        /// Kind property implementation
        /// </summary>
        public override PreferredMethod Kind
        {
            get { return PreferredMethod.External; }
        }

        /// <summary>
        /// IsInitialized property implmentation
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
            get { return _allowenrollment; }
            set { _allowenrollment = value; }
        }

        /// <summary>
        /// EnrollmentNeverUseOptions property implementation
        /// </summary>
        public override bool EnrollmentNeverUseOptions
        {
            get { return _enrollmentneveruseoptions; }
            set { _enrollmentneveruseoptions = value; }
        }

        /// <summary>
        /// IsTwoWayByDefault property implementation
        /// </summary>
        public override bool IsTwoWayByDefault
        {
            get { return Data.IsTwoWay; }
        }

        /// <summary>
        /// Name property implementation
        /// </summary>
        public override string Name
        {
            get { return "Neos.Provider.LegacyExternal"; }
        }

        /// <summary>
        /// Description property implementation
        /// </summary>
        public override string Description
        {
            get { return "Legacy Multi-Factor Provider for SMS"; }
        }

        /// <summary>
        /// GetUILabel method implementation
        /// </summary>
        public override string GetUILabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.SMSUIOTPLabel;
        }

        /// <summary>
        /// GetUICFGLabel method implementation
        /// </summary>
        public override string GetUICFGLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.SMSUICFGLabel;
        }

        /// <summary>
        /// GetUIMessage method implementation
        /// </summary>
        public override string GetUIMessage(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            if (string.IsNullOrEmpty(ctx.PhoneNumber))
                return string.Format(Resources.chtml_strings.SMSUIMessage, string.Empty);
            else
                return string.Format(Resources.chtml_strings.SMSUIMessage, Utilities.StripPhoneNumber(ctx.PhoneNumber));
        }

        /// <summary>
        /// GetUIListOptionLabel method implementation
        /// </summary>
        public override string GetUIListOptionLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.SMSUIListOptionLabel;
        }

        /// <summary>
        /// GetUIListChoiceLabel method implementation
        /// </summary>
        public override string GetUIListChoiceLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.SMSUIListChoiceLabel;
        }

        /// <summary>
        /// GetUIConfigLabel method implementation
        /// </summary>
        public override string GetUIConfigLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            if (string.IsNullOrEmpty(ctx.PhoneNumber))
                return Resources.chtml_strings.SMSUIConfigLabel;
            else
                return string.Format(Resources.chtml_strings.SMSUIConfigLabel2, Utilities.StripPhoneNumber(ctx.PhoneNumber));
        }

        /// <summary>
        /// GetUIChoiceLabel method implementation
        /// </summary>
        public override string GetUIChoiceLabel(AuthenticationContext ctx, AvailableAuthenticationMethod method = null)
        {
            if (ctx.IsTwoWay)
                return Resources.chtml_strings.SMSUIChoiceLabel2;
            else
                return Resources.chtml_strings.SMSUIChoiceLabel;
        }

        /// <summary>
        /// GetUIWarningInternetLabel method implmentation
        /// </summary>
        public override string GetUIWarningInternetLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.GLOBALWarnOverNetwork;
        }

        /// <summary>
        /// GetUIWarningThirdPartyLabel method implementation
        /// </summary>
        public override string GetUIWarningThirdPartyLabel(AuthenticationContext ctx)
        {
            if (ctx.IsTwoWay)
            {
                Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
                return Resources.chtml_strings.GLOBALWarnThirdParty;
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// GetUIDefaultChoiceLabel method implementation
        /// </summary>
        public override string GetUIDefaultChoiceLabel(AuthenticationContext ctx)
        {
            if (ctx.IsTwoWay)
            {
                Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
                return Resources.chtml_strings.GLOBALListChoiceDefaultLabel;
            }
            else
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
            return (this._sasprovider != null);
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
                        _sasprovider = LoadLegacyExternalProvider(Data.FullQualifiedImplementation);

                        Enabled = param.Enabled;
                        AllowEnrollment = param.EnrollWizard;
                        EnrollmentNeverUseOptions = param.EnrollWizardStrict;
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
        /// LoadLegacyExternalProvider method implmentation
        /// </summary>
        private IExternalOTPProvider LoadLegacyExternalProvider(string AssemblyFulldescription)
        {
            Assembly assembly = Assembly.Load(Utilities.ParseAssembly(AssemblyFulldescription));
            Type _typetoload = assembly.GetType(Utilities.ParseType(AssemblyFulldescription));

            if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IExternalOTPProvider") != null)
                return (IExternalOTPProvider)Activator.CreateInstance(_typetoload, true); // Allow Calling internal Constructors
            else
                return null;
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

            ctx.Notification = _sasprovider.GetUserCodeWithExternalSystem(ctx.UPN, ctx.PhoneNumber, ctx.MailAddress, this.Data, new CultureInfo(ctx.Lcid));
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

            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);

            List<AvailableAuthenticationMethod> result = GetSessionData(this.Kind, ctx);
            if (result != null)
                return result;
            else
            {
                result = new List<AvailableAuthenticationMethod>();
                AvailableAuthenticationMethod item = new AvailableAuthenticationMethod();
                item.IsDefault = true;
                item.IsRemote = true;
                item.IsSendBack = false;
                item.RequiredPin = false;
                item.RequiredEmail = false;
                item.ExtraInfos = ctx.PhoneNumber;
                if (this.Data.IsTwoWay)
                {
                    item.IsTwoWay = true;
                    item.RequiredPhone = true;
                    item.RequiredCode = false;
                    item.Method = AuthenticationResponseKind.SmsTwoWayOTP;
                }
                else
                {
                    item.IsTwoWay = false;
                    item.RequiredPhone = true;
                    item.RequiredCode = true;
                    item.Method = AuthenticationResponseKind.SmsOneWayOTP;
                }
                result.Add(item);
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
    }

    /// <summary>
    /// NeosMailProvider class implementation
    /// </summary>
    public class NeosMailProvider: BaseExternalProvider
    {
        private bool _isinitialized = false;
        private MailProvider Data;
        private bool _allowenrollment = true;
        private bool _enrollmentneveruseoptions = false;

        /// <summary>
        /// Kind property implementation
        /// </summary>
        public override PreferredMethod Kind
        {
            get { return PreferredMethod.Email; }
        }

        /// <summary>
        /// IsInitialized property implmentation
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
            get { return false; }
        }

        /// <summary>
        /// AllowEnrollment property implementation
        /// </summary>
        public override bool AllowEnrollment
        {
            get { return _allowenrollment; }
            set { _allowenrollment = value; }
        }

        /// <summary>
        /// EnrollmentNeverUseOptions property implementation
        /// </summary>
        public override bool EnrollmentNeverUseOptions
        {
            get { return _enrollmentneveruseoptions; }
            set { _enrollmentneveruseoptions = value; }
        }

        /// <summary>
        /// Name property implementation
        /// </summary>
        public override string Name
        {
            get { return "Neos.Provider.Email"; }
        }

        /// <summary>
        /// Description property implementation
        /// </summary>
        public override string Description
        {
            get { return "Email Multi-Factor Provider"; }
        }

        /// <summary>
        /// GetUILabel method implementation
        /// </summary>
        public override string GetUILabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.MAILUIOTPLabel;
        }

        /// <summary>
        /// GetUICFGLabel method implementation
        /// </summary>
        public override string GetUICFGLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.MAILUICFGLabel;
        }

        /// <summary>
        /// GetUIMessage method implementation
        /// </summary>
        public override string GetUIMessage(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.MAILUIMessage;
        }

        /// <summary>
        /// GetUIListOptionLabel method implementation
        /// </summary>
        public override string GetUIListOptionLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.MAILUIListOptionLabel;
        }

        /// <summary>
        /// GetUIListChoiceLabel method implementation
        /// </summary>
        public override string GetUIListChoiceLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.MAILUIListChoiceLabel;
        }

        /// <summary>
        /// GetUIConfigLabel method implementation
        /// </summary>
        public override string GetUIConfigLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            if (string.IsNullOrEmpty(ctx.MailAddress))
                return Resources.chtml_strings.MAILUIConfigLabel;
            else
                return string.Format(Resources.chtml_strings.MAILUIConfigLabel2, Utilities.StripEmailAddress(ctx.MailAddress));
        }

        /// <summary>
        /// GetUIChoiceLabel method implementation
        /// </summary>
        public override string GetUIChoiceLabel(AuthenticationContext ctx, AvailableAuthenticationMethod method = null)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.MAILUIChoiceLabel;
        }

        /// <summary>
        /// GetUIWarningInternetLabel method implmentation
        /// </summary>
        public override string GetUIWarningInternetLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.GLOBALWarnOverNetwork;
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
            return Utilities.ValidateEmail(ctx.MailAddress, true);
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
                case RequiredMethodElements.EmailParameterRequired:
                    return true;
                case RequiredMethodElements.PinParameterRequired:
                    return this.PinRequired;
                case RequiredMethodElements.PinInputRequired:
                    return this.PinRequired;
                case RequiredMethodElements.EmailLinkRequired:
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
                    if (externalsystem is MailProviderParams)
                    {
                        MailProviderParams param = externalsystem as MailProviderParams;
                        Data = param.Data;
                        Enabled = param.Enabled;
                        AllowEnrollment = param.EnrollWizard;
                        EnrollmentNeverUseOptions = param.EnrollWizardStrict;
                        PinRequired = param.PinRequired;
                        _isinitialized = true;
                        return;
                    }
                    else
                        throw new InvalidCastException("Invalid Email Provider !");
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

            ctx.Notification = Utilities.GetEmailOTP(ctx, Data, new CultureInfo(ctx.Lcid));
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

            int value = Convert.ToInt32(pin);
            if (CheckPin(ctx, value))
                return (int)AuthenticationResponseKind.EmailOTP;
            else
                return (int)AuthenticationResponseKind.Error;
        }

        /// <summary>
        /// GetAuthenticationMethods method implmentation
        /// </summary>
        public override List<AvailableAuthenticationMethod> GetAuthenticationMethods(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            List<AvailableAuthenticationMethod> result = GetSessionData(this.Kind, ctx);
            if (result != null)
                return result;
            else
            {
                result = new List<AvailableAuthenticationMethod>();
                AvailableAuthenticationMethod item = new AvailableAuthenticationMethod();
                item.IsDefault = true;
                item.IsRemote = true;
                item.IsTwoWay = false;
                item.IsSendBack = false;
                item.RequiredEmail = true;
                item.RequiredPhone = false;
                item.RequiredCode = true;
                item.Method = AuthenticationResponseKind.EmailOTP;
                item.RequiredPin = false;
                result.Add(item);
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
    }

    /// <summary>
    /// NeosAdministrationProvider class implementation
    /// </summary>
    public class NeosAdministrationProvider: IExternalAdminProvider
    {
        private bool IsInitialized = false;
        private MFAConfig Data;

        /// <summary>
        /// GetUIWarningInternetLabel method implementation
        /// </summary>
        public string GetUIWarningInternetLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.GLOBALWarnOverNetwork;
        }

        /// <summary>
        /// GetUIWarningThirdPartyLabel method implementation
        /// </summary>
        public string GetUIWarningThirdPartyLabel(AuthenticationContext ctx)
        {
            return string.Empty;
        }

        /// <summary>
        /// GetUIInscriptionMessageLabel method implementation
        /// </summary>
        public string GetUIInscriptionMessageLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.ADMINInscriptionEmail;
        }

        /// <summary>
        /// GetUISecretKeyMessageLabel method implementation
        /// </summary>
        public string GetUISecretKeyMessageLabel(AuthenticationContext ctx)
        {
            Resources.chtml_strings.Culture = new CultureInfo(ctx.Lcid);
            return Resources.chtml_strings.ADMINKeyEmail;
        }


        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(MFAConfig config)
        {
            try
            {
                if (!IsInitialized)
                {
                        Data = config;
                        IsInitialized = true;
                        return;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Administrative Email
        /// <summary>
        /// GetInscriptionContext method implementation
        /// </summary>
        public void GetInscriptionContext(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");
            CheckUser(ctx);
            ctx.Notification = (int)AuthenticationRequestKind.RequestEmailInscription;
            ctx.SessionId = Guid.NewGuid().ToString();
            ctx.SessionDate = DateTime.Now;
            ctx.IsRemote = true;
            ctx.SelectedMethod = AuthenticationResponseKind.EmailForInscription;
        }

        /// <summary>
        /// PostInscriptionRequest method implementation
        /// </summary>
        public int PostInscriptionRequest(AuthenticationContext ctx)
        {
            try
            {
                if (!IsInitialized)
                    throw new Exception("Provider not initialized !");
                CheckUser(ctx);
                MailUtilities.SendInscriptionMail(Data.AdminContact, (Registration)ctx, Data.MailProvider, new CultureInfo(ctx.Lcid));
                ctx.Notification = (int)AuthenticationResponseKind.EmailForInscription;
                return ctx.Notification;
            }
            catch (Exception ex)
            {
                ctx.Notification = (int)AuthenticationResponseKind.Error;
                Log.WriteEntry(Resources.cerrors_strings.ErrorSendingAdministrativeRequest + "\r\n" + ctx.UPN + " : " + "\r\n" + ex.Message + "\r\n" + ex.StackTrace, EventLogEntryType.Error, 801);
            }
            return ctx.Notification;
        }

        /// <summary>
        /// SetInscriptionResult method implementation
        /// </summary>
        public int SetInscriptionResult(AuthenticationContext ctx)
        {
            try
            {
                if (!IsInitialized)
                    throw new Exception("Provider not initialized !");
                ctx.Notification = (int)AuthenticationResponseKind.EmailForInscription;
            }
            catch (Exception ex)
            {
                ctx.Notification = (int)AuthenticationResponseKind.Error;
                Log.WriteEntry(Resources.cerrors_strings.ErrorSendingAdministrativeRequest + "\r\n" + ctx.UPN + " : " + "\r\n" + ex.Message + "\r\n" + ex.StackTrace, EventLogEntryType.Error, 801);
            }
            return ctx.Notification;
        }
        #endregion

        #region Secret Key
        /// <summary>
        /// GetSecretKeyContext method implementation
        /// </summary>
        public void GetSecretKeyContext(AuthenticationContext ctx)
        {

            if (!IsInitialized)
                throw new Exception("Provider not initialized !");
            CheckUser(ctx);
            ctx.Notification = (int)AuthenticationRequestKind.RequestEmailForKey;
            ctx.SessionId = Guid.NewGuid().ToString();
            ctx.SessionDate = DateTime.Now;
            ctx.IsRemote = true;
            ctx.SelectedMethod = AuthenticationResponseKind.EmailForKey;
        }

        /// <summary>
        /// PostSecretKeyRequest method implementation
        /// </summary>
        public int PostSecretKeyRequest(AuthenticationContext ctx)
        {
            try
            {
                if (!IsInitialized)
                    throw new Exception("Provider not initialized !");
                CheckUser(ctx);
                string qrcode = KeysManager.EncodedKey(ctx.UPN);
                MailUtilities.SendKeyByEmail(ctx.MailAddress, ctx.UPN, qrcode, Data.MailProvider, Data, new CultureInfo(ctx.Lcid));
                ctx.Notification = (int)AuthenticationResponseKind.EmailForKey;
            }
            catch (Exception ex)
            {
                ctx.Notification = (int)AuthenticationResponseKind.Error;
                Log.WriteEntry(Resources.cerrors_strings.ErrorSendingSecretKeyRequest + "\r\n" + ctx.UPN + " : " + "\r\n" + ex.Message + "\r\n" + ex.StackTrace, EventLogEntryType.Error, 801);
            }
            return ctx.Notification;
        }

        /// <summary>
        /// SetSecretKeyResult method implmentation
        /// </summary>
        public int SetSecretKeyResult(AuthenticationContext ctx)
        {
            try
            {
                if (!IsInitialized)
                    throw new Exception("Provider not initialized !");
                ctx.Notification = (int)AuthenticationResponseKind.EmailForKey;
            }
            catch (Exception ex)
            {
                ctx.Notification = (int)AuthenticationResponseKind.Error;
                Log.WriteEntry(Resources.cerrors_strings.ErrorSendingSecretKeyRequest + "\r\n" + ctx.UPN + " : " + "\r\n" + ex.Message + "\r\n" + ex.StackTrace, EventLogEntryType.Error, 801);
            }
            return ctx.Notification;
        }
        #endregion

        /// <summary>
        /// CheckUser method implementation
        /// </summary>
        private void CheckUser(AuthenticationContext ctx)
        {
            if (!KeysManager.ValidateKey(ctx.UPN))
                throw new CryptographicException(string.Format("SECURTY ERROR : Invalid Key for User {0}", ctx.UPN));
            Registration reg = (Registration)ctx;
            if (reg == null)
                throw new Exception(string.Format("SECURTY ERROR : Invalid user {0}", ctx.UPN));
            if (string.IsNullOrEmpty(reg.MailAddress))
                throw new Exception(string.Format("SECURTY ERROR : Invalid user mail address {0}", ctx.UPN));
        }
    }
}