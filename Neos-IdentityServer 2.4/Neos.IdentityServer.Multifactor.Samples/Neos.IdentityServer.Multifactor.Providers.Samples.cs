//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using System.Threading;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using Neos.IdentityServer.MultiFactor.Common;

namespace Neos.IdentityServer.MultiFactor.Samples
{
    ///------------------------------------------------------------------------------------------------------------------------------///
    /// Sample to demonstrate the implementation of an External MFA Provider                                                         ///
    ///                                                                                                                              ///    
    /// This can be a base of inspiration if you have specific needs about security                                                  ///   
    /// In theory, you should put all the strings in ressources, for best reading we let these string as constants in th source code ///
    ///                                                                                                                              ///
    /// -----------------------------------------------------------------------------------------------------------------------------///
    /// <summary>
    /// QuizProviderSample class implementation
    /// </summary>
    public class QuizProviderSample : BaseExternalProvider
    {
        private bool _isinitialized = false;
        private bool IsAsync;

        /// <summary>
        /// Kind property implementation
        /// 
        /// For custom implementation it' required that you return PreferredMethod.External 
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
            get { return false; }
        }

        /// <summary>
        /// CanBeDisabled property implementation
        /// 
        /// Allow the provider to be disabled in the UI, and not available to the users
        /// 
        /// </summary>
        public override bool AllowDisable
        {
            get { return true; }
        }

        /// <summary>
        /// IsInitialized property implmentation
        /// </summary>
        public override bool IsInitialized
        {
            get { return _isinitialized; }
        }

        /// <summary>
        /// CanOverrideDefault property implmentation
        /// 
        /// When the provider propose multiple options (aka : sms, voice, otp), let the user to change the sub method at login
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
            get { return "Neos.Provider.Quiz"; }
        }

        /// <summary>
        /// Description property implementation
        /// </summary>
        public override string Description
        {
            get { return "Quiz Multi-Factor Provider Sample"; }
        }

        /// <summary>
        /// GetUILabel method implementation
        /// 
        /// Label displayed above a control (aka : TextBox) when user is in login process
        /// </summary>
        public override string GetUILabel(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            GetAuthenticationContext(ctx);
            switch (ctx.SelectedMethod)
            {
                case AuthenticationResponseKind.Sample1:
                    switch (ctx.Lcid)
                    {
                        default:
                            return "Age of a celebrity";
                        case 1036:
                            return "Age d'une personne célébre";
                        case 1034:
                        case 3082:
                            return "La edad de una persona famosa";
                    }
                case AuthenticationResponseKind.Sample2Async:
                    switch (ctx.Lcid)
                    {
                        default:
                            return "Sing a song";
                        case 1036:
                            return "Chanter une chanson";
                        case 1034:
                        case 3082:
                            return "Cantar una canción";
                    }
            }
            return string.Empty;
        }

        /// <summary>
        /// GetWizardUILabel method implementation
        /// </summary>
        public override string GetWizardUILabel(AuthenticationContext ctx)
        {
            return GetWizardLinkLabel(ctx);
        }

        /// <summary>
        /// GetWizardLinkLabel method implementation
        /// </summary>
        public override string GetWizardLinkLabel(AuthenticationContext ctx)
        {
            switch (ctx.Lcid)
            {
                default:
                    return "Register for the contest";
                case 1036:
                    return "Enregistrer vous au concours";
                case 1034:
                case 3082:
                    return "Registrarse para el concurso";
            }
        }

        /// <summary>
        /// GetUICFGLabel method implementation
        /// </summary>
        public override string GetUICFGLabel(AuthenticationContext ctx)
        {
            return GetUIListOptionLabel(ctx);
        }

        /// <summary>
        /// GetUIMessage method implmentation
        /// 
        /// Label displayed under a control (aka : TextBox) as help message when user is in login process
        /// </summary>
        public override string GetUIMessage(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            GetAuthenticationContext(ctx);
            switch (ctx.SelectedMethod)
            {
                case AuthenticationResponseKind.Sample1:
                    switch (ctx.Lcid)
                    {
                        default:
                            return "Indicate Barack Obama's age";
                        case 1036:
                            return "Indiquer l'âge de Paul Bismuth";
                        case 1034:
                        case 3082:
                            return "La edad de una persona famosa";
                    }
                case AuthenticationResponseKind.Sample2Async:
                    switch (ctx.Lcid)
                    {
                        default:
                            return "Sing Queen's \"Don't stop me now !\"";
                        case 1036:
                            return "Chanter \"Nougayork\" de Claude Nougaro";
                        case 1034:
                        case 3082:
                            return "Canta \"Waka Waka\" de Shakira";
                    }
            }
            return string.Empty;
        }

        /// <summary>
        /// GetUIListOptionLabel method implementation
        /// </summary>
        public override string GetUIListOptionLabel(AuthenticationContext ctx)
        {
            switch (ctx.Lcid)
            {
                default:
                    return "Take part in a Quiz";
                case 1036:
                    return "Participer à un Quiz";
                case 1034:
                case 3082:
                    return "Participa en un cuestionario";
            }
        }

        /// <summary>
        /// GetUIListChoiceLabel method implmentation
        /// </summary>
        public override string GetUIListChoiceLabel(AuthenticationContext ctx)
        {

            switch (ctx.Lcid)
            {
                default:
                    return "Take part in a Quiz";
                case 1036:
                    return "Participer à un Quiz";
                case 1034:
                case 3082:
                    return "Participa en un cuestionario";
            }
        }

        /// <summary>
        /// GetUIDefaultChoiceLabel method implementation
        /// </summary>
        public override string GetUIDefaultChoiceLabel(AuthenticationContext ctx)
        {
            switch (ctx.Lcid)
            {
                default:
                    return "Use Default option";
                case 1036:
                    return "Utiliser l'option par défaut";
                case 1034:
                case 3082:
                    return "Usar la opción predeterminada";
            }
        }

        /// <summary>
        /// GetUIConfigLabel method implmentation
        /// 
        /// Label displayed in the comboxbox used in configuration (registration)
        /// </summary>
        public override string GetUIConfigLabel(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            GetAuthenticationContext(ctx);
            switch (ctx.SelectedMethod)
            {
                case AuthenticationResponseKind.Sample1:
                    switch (ctx.Lcid)
                    {
                        default:
                            return "Barack Obama's age";
                        case 1036:
                            return "Age de Paul Bismuth";
                        case 1034:
                        case 3082:
                            return "La edad de Raphaël Nadal";
                    }
                case AuthenticationResponseKind.Sample2Async:
                    switch (ctx.Lcid)
                    {
                        default:
                            return "Don't stop me now !";
                        case 1036:
                            return "Nougayork";
                        case 1034:
                        case 3082:
                            return "Waka Waka";
                    }
            }
            return string.Empty;
        }

        /// <summary>
        /// GetUIChoiceLabel method ipmplmentation
        /// 
        /// Label displayed in the comboxbox used for selection of the Default Authentication method (registration, Do not have the code)
        /// </summary>
        public override string GetUIChoiceLabel(AuthenticationContext ctx, AvailableAuthenticationMethod method = null)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            GetAuthenticationContext(ctx);
            AuthenticationResponseKind mk = ctx.SelectedMethod;
            if (method != null)
                mk = method.Method;
            switch (mk)
            {
                case AuthenticationResponseKind.Sample1:
                    switch (ctx.Lcid)
                    {
                        default:
                            return "How old is Barak Obama";
                        case 1036:
                            return "Quel est l'age de Paul Bismuth";
                        case 1034:
                        case 3082:
                            return "¿ Qué edad tiene Raphael Nadal";
                    }
                case AuthenticationResponseKind.Sample2Async:
                    switch (ctx.Lcid)
                    {
                        default:
                            return "Sing Queen's \"Don't stop me now !\"";
                        case 1036:
                            return "Chanter \"Nougayork\" de Claude Nougaro";
                        case 1034:
                        case 3082:
                            return "Canta \"Waka Waka\" de Shakira";
                    }
            }
            return string.Empty;
        }

        /// <summary>
        /// GetUIWarningInternetLabel method implmentation
        /// 
        /// Warning message (optional) when data is sent over the internet
        /// </summary>
        public override string GetUIWarningInternetLabel(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            GetAuthenticationContext(ctx);
            switch (ctx.SelectedMethod)
            {
                case AuthenticationResponseKind.Sample1:
                    return string.Empty;
                case AuthenticationResponseKind.Sample2Async:
                    switch (ctx.Lcid)
                    {
                        default:
                            return "Tonight I'm gonna have myself a real good time \r" +
                                   "I feel alive and the world I'll turn it inside out - yeah \r" +
                                   "And floating around in ecstasy \r" +
                                   "So don't stop me now don't stop me \r" +
                                   "'Cause I'm having a good time having a good time";
                        case 1036:
                            return "Dès l'aérogare J'ai senti le choc \r" +
                                   "Un souffle barbare Un remous hard rock \r" +
                                   "Dès l'aérogare J'ai changé d'époque \r" +
                                   "Come on ! ça démarre Sur les starting blocks \r"+
                                   "(feat Marcus Miller on the bass)";
                        case 1034:
                        case 3082:
                            return "You're a good soldier Choosing your battles \r" +
                                   "Pick yourself up and dust yourself off and back in the saddle \r" +
                                   "You're on the front line Everyone's watching \r" +
                                   "You know it's serious we're getting closer, this isn't over";
                    }
            }
            return string.Empty;
        }

        /// <summary>
        /// GetUIWarningThirdPartyLabel method implementation
        /// 
        /// Warning mesage (optional) when the MFA validation is made by a third party
        /// </summary>
        public override string GetUIWarningThirdPartyLabel(AuthenticationContext ctx)
        {

            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            GetAuthenticationContext(ctx);
            switch (ctx.SelectedMethod)
            {
                case AuthenticationResponseKind.Sample1:
                    return string.Empty;
                case AuthenticationResponseKind.Sample2Async:
                    switch (ctx.Lcid)
                    {
                        default:
                        case 1033:
                            return "(Sing for 30 seconds)";

                        case 1036:
                            return "(Chantez durant 30 secondes)";
                        case 1034:
                        case 3082:
                            return "(Canta durante 30 segundos)";
                    }
            }
            return string.Empty;
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
            return true;
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
            switch (ctx.Lcid)
            {
                default:
                    return "Account verified<br><br>Now you can use the Quiz to login";
                case 1036:
                    return "Compte validé<br><br>Vous pouvez utiliser les Quiz pour vous connecter";
                case 1034:
                case 3082:
                    return "Cuenta verificada<br><br>Ahora puede usar el Quiz para iniciar sesión";
            }
        }

        /// <summary>
        /// GetUIAccountManagementLabel method implementation
        /// </summary>
        public override string GetUIAccountManagementLabel(AuthenticationContext ctx)
        {
            switch (ctx.Lcid)
            {
                default:
                    return "Access my Quiz configuration options";
                case 1036:
                    return "Accéder à mes options de configuration Quiz";
                case 1034:
                case 3082:
                    return "Acceda a mis opciones de configuración Quiz";
            }
        }

        /// <summary>
        /// GetAccountManagementUrl() method implmentation
        /// </summary>
        public override string GetAccountManagementUrl(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");

            GetAuthenticationContext(ctx);

            switch (ctx.SelectedMethod)
            {
                case AuthenticationResponseKind.Sample1:
                    switch (ctx.Lcid)
                    {
                        default:
                            return "https://en.wikipedia.org/wiki/Barack_Obama";
                        case 1036:
                            return "https://fr.wikipedia.org/wiki/Discussion:Nicolas_Sarkozy";
                        case 1034:
                        case 3082:
                            return "https://es.wikipedia.org/wiki/Rafael_Nadal";
                    }
                case AuthenticationResponseKind.Sample2Async:
                    switch (ctx.Lcid)
                    {
                        default:
                            return "https://www.shazam.com/track/219983/dont-stop-me-now";
                        case 1036:
                            return "https://www.shazam.com/fr/track/45655670/nougayork";
                        case 1034:
                        case 3082:
                            return "https://www.shazam.com/es/track/52114610/waka-waka-this-time-for-africa";
                    }
            }
            return string.Empty;
        }

        /// <summary>
        /// IsMethodElementRequired implementation
        /// </summary>
        public override bool IsUIElementRequired(AuthenticationContext ctx, RequiredMethodElements element)
        {
            switch (element)
            {
                case RequiredMethodElements.CodeInputRequired:
                    return (ctx.SelectedMethod==AuthenticationResponseKind.Sample1);
                case RequiredMethodElements.EmailLinkRequired:
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
                    if (externalsystem is ExternalProviderParams)
                    {
                        ExternalProviderParams param = externalsystem as ExternalProviderParams;
                        Enabled = param.Enabled;
                        PinRequired = param.PinRequired;
                        IsRequired = param.IsRequired;
                        WizardEnabled = param.EnrollWizard;
                        ForceEnrollment = param.ForceWizard;
                        IsAsync = param.Data.IsTwoWay;
                        _isinitialized = true;
                        return;
                    }
                    else
                    {
                        Enabled = externalsystem.Enabled;
                        PinRequired = externalsystem.PinRequired;
                        WizardEnabled = externalsystem.EnrollWizard;
                        ForceEnrollment = externalsystem.ForceWizard;
                        _isinitialized = true;
                        return;
                    }
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

                AvailableAuthenticationMethod item1 = GetAuthenticationMethodProperties(ctx, AuthenticationResponseKind.Sample1);
                result.Add(item1);
                AvailableAuthenticationMethod item2 = GetAuthenticationMethodProperties(ctx, AuthenticationResponseKind.Sample2Async);
                result.Add(item2);
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

            if (CheckPin(ctx, pin))
                return (int)AuthenticationResponseKind.PhoneAppOTP;
            else
                return (int)AuthenticationResponseKind.Error;
        }

        /// <summary>
        /// GetAuthenticationMethod method implementation
        /// </summary>
        private AvailableAuthenticationMethod GetAuthenticationMethodProperties(AuthenticationContext ctx, AuthenticationResponseKind method)
        {
            AvailableAuthenticationMethod result = new AvailableAuthenticationMethod();
            switch (method)
            {
                case AuthenticationResponseKind.Sample1:
                    result.IsDefault = !IsAsync;
                    result.RequiredPin = false;
                    result.IsRemote = false;
                    result.IsTwoWay = false;
                    result.IsSendBack = false;
                    result.RequiredEmail = false;
                    result.RequiredPhone = false;
                    result.RequiredCode = true;
                    result.Method = MultiFactor.AuthenticationResponseKind.Sample1;
                    break;
                case AuthenticationResponseKind.Sample2Async:
                    result.IsDefault = IsAsync;
                    result.RequiredPin = false;
                    result.IsRemote = true;
                    result.IsTwoWay = true;
                    result.IsSendBack = false;
                    result.RequiredEmail = false;
                    result.RequiredPhone = false;
                    result.RequiredCode = false;
                    result.Method = MultiFactor.AuthenticationResponseKind.Sample2Async;
                    break;
                default:
                    result.Method = AuthenticationResponseKind.Error;
                    break;
            }
            return result;
        }

        /// <summary>
        /// CheckPin method inplementation
        /// </summary>
        private bool CheckPin(AuthenticationContext ctx, string pin)
        {
            switch (ctx.SelectedMethod)
            {
                case AuthenticationResponseKind.Sample1:
                    switch (ctx.Lcid)
                    {
                        case 1033:
                            return (Convert.ToInt32(pin) == GetAgeInYears(new DateTime(1961, 08, 04), DateTime.Now));
                        case 1036:
                            return (Convert.ToInt32(pin) == GetAgeInYears(new DateTime(1955, 01, 22), DateTime.Now));
                        case 1034:
                        case 3082:
                            return (Convert.ToInt32(pin) == GetAgeInYears(new DateTime(1986, 06, 03), DateTime.Now));
                        default:
                            return (Convert.ToInt32(pin) == GetAgeInYears(new DateTime(1961, 08, 04), DateTime.Now));
                    }
                case AuthenticationResponseKind.Sample2Async:
                    Thread.Sleep(30 * 1000);
                    return true;
                default:
                    switch (ctx.Lcid)
                    {
                        case 1033:
                            return (Convert.ToInt32(pin) == GetAgeInYears(new DateTime(1961, 08, 04), DateTime.Now));
                        case 1036:
                            return (Convert.ToInt32(pin) == GetAgeInYears(new DateTime(1955, 01, 22), DateTime.Now));
                        case 1034:
                        case 3082:
                            return (Convert.ToInt32(pin) == GetAgeInYears(new DateTime(1986, 06, 03), DateTime.Now));
                        default:
                            return (Convert.ToInt32(pin) == GetAgeInYears(new DateTime(1961, 08, 04), DateTime.Now));
                    }
            }
        }

        /// <summary>
        /// GetAgeInYears method 
        /// </summary>
        private int GetAgeInYears(DateTime startDate, DateTime endDate)
        {
            int years = endDate.Year - startDate.Year;
            if (startDate.Month == endDate.Month && endDate.Day < startDate.Day || endDate.Month < startDate.Month)
                years--;
            return years;
        }
    }

    /// <summary>
    /// Old ExternalProvider API Sample
    /// </summary>
    public class ExternalLegacySample: IExternalOTPProvider
    {
        /// <summary>
        /// GetUserCodeWithExternalSystem demo method
        /// </summary>
#pragma warning disable 162
        public int GetUserCodeWithExternalSystem(string upn, string phonenumber, string email, ExternalOTPProvider externalsys, CultureInfo culture)
        {
            // Compute and send your TOTP code and return his value if everything goes right
            if (true)
                return 1230;
            else
                return (int)AuthenticationResponseKind.Error;  // return error
        }

        /// <summary>
        /// GetCodeWithExternalSystem method implementation for Azure MFA 
        /// </summary>
        public AuthenticationResponseKind GetCodeWithExternalSystem(Registration reg, ExternalOTPProvider externalsys, CultureInfo culture, out int otp)
        {
            // Compute and send your TOTP code and return his value if everything goes right
            if (true)
            {
                otp = 1230;
                return AuthenticationResponseKind.SmsOTP;
            }
            else
                return AuthenticationResponseKind.Error;  // return error
        }
#pragma warning restore 162
    }
}


