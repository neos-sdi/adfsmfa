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
using Microsoft.IdentityServer.Web.Authentication.External;
using Neos.IdentityServer.MultiFactor.Resources;
using Neos.IdentityServer.MultiFactor;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Neos.IdentityServer.MultiFactor
{
    public class AdapterPresentation : IAdapterPresentation, IAdapterPresentationForm
    {
        private bool _isPermanentFailure;
        private bool _ismessage;
        private bool _disableoptions;
        private AuthenticationProvider _provider;
        private AuthenticationContext _context;
        private ResourcesLocale _resmgr;

        #region Constructors
        /// <summary>
        /// Constructor implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context)
        {
            this._provider = provider;
            this._context = new AuthenticationContext(context);
            this._isPermanentFailure = (this._context.TargetUIMode == ProviderPageMode.DefinitiveError);
            this._ismessage = (this._context.TargetUIMode != ProviderPageMode.DefinitiveError);
            this._disableoptions = false;
            this._resmgr = new ResourcesLocale(context.Lcid);
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, string message)
        {
            this._provider = provider;
            this._context = new AuthenticationContext(context);
            this._context.UIMessage = message;
            this._isPermanentFailure = (this._context.TargetUIMode == ProviderPageMode.DefinitiveError);
            this._ismessage = (this._context.TargetUIMode != ProviderPageMode.DefinitiveError);
            this._disableoptions = false;
            this._resmgr = new ResourcesLocale(context.Lcid);
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, string message, bool ismessage)
        {
            this._provider = provider;
            this._context = new AuthenticationContext(context);
            this._context.UIMessage = message;
            this._isPermanentFailure = (this._context.TargetUIMode == ProviderPageMode.DefinitiveError);
            this._ismessage = ismessage;
            this._disableoptions = false;
            this._resmgr = new ResourcesLocale(context.Lcid);
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, ProviderPageMode suite)
        {
            this._provider = provider;
            this._context = new AuthenticationContext(context);
            this._context.TargetUIMode = suite;
            this._isPermanentFailure = (this._context.TargetUIMode == ProviderPageMode.DefinitiveError);
            this._ismessage = (this._context.TargetUIMode != ProviderPageMode.DefinitiveError);
            this._disableoptions = false;
            this._resmgr = new ResourcesLocale(context.Lcid);
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, string message, ProviderPageMode suite, bool disableoptions = false)
        {
            this._provider = provider;
            this._context = new AuthenticationContext(context);
            this._context.TargetUIMode = suite;
            this._context.UIMessage = message;
            this._isPermanentFailure = (this._context.TargetUIMode == ProviderPageMode.DefinitiveError);
            this._ismessage = (this._context.TargetUIMode != ProviderPageMode.DefinitiveError);
            this._disableoptions = disableoptions;
            this._resmgr = new ResourcesLocale(context.Lcid);
        }
        #endregion

        #region properties
        /// <summary>
        /// IsPermanentFailure property
        /// </summary>
        public bool IsPermanentFailure
        {
            get { return _isPermanentFailure; }
        }

        /// <summary>
        /// Provider property
        /// </summary>
        public AuthenticationProvider Provider
        {
            get { return _provider; }
        }

        /// <summary>
        /// Context property
        /// </summary>
        public AuthenticationContext Context
        {
            get { return _context; }
        }

        /// <summary>
        /// Resources property
        /// </summary>
        public ResourcesLocale Resources
        {
            get { return _resmgr; }
        }

        #endregion

        /// <summary>
        /// IAdapterPresentation GetPageTitle implementation
        /// </summary>
        public string GetPageTitle(int lcid)
        {
            return Resources.GetString(ResourcesLocaleKind.Titles , "TitlePageTitle");
        }

        /// <summary>
        /// IAdapterPresentationForm GetFormHtml implementation
        /// </summary>
        public string GetFormHtml(int lcid)
        {
            string result = string.Empty;
            switch (Context.UIMode)
            {
                case ProviderPageMode.Identification:
                    result = GetFormHtmlIdentification(Context);
                    break;
                case ProviderPageMode.Registration: // User self registration and enable
                    result = GetFormHtmlRegistration(Context);
                    break;
                case ProviderPageMode.Invitation: // admministrative user registration and let disabled
                    result = GetFormHtmlInvitation(Context);
                    break;
                case ProviderPageMode.SelectOptions:
                    result = GetFormHtmlSelectOptions(Context);
                    break;
                case ProviderPageMode.ChooseMethod:
                    result = GetFormHtmlChooseMethod(Context);
                    break;
                case ProviderPageMode.ChangePassword:
                    result = GetFormHtmlChangePassword(Context);
                    break;
                case ProviderPageMode.Bypass:
                    result = GetFormHtmlBypass(Context);
                    break;
                case ProviderPageMode.Locking:
                    result = GetFormHtmlLocking(Context);
                    break;
                case ProviderPageMode.ShowQRCode:
                    result = GetFormHtmlShowQRCode(Context);
                    break;
                case ProviderPageMode.SendAuthRequest:
                    result = GetFormHtmlSendCodeRequest(Context);
                    break;
                case ProviderPageMode.SendAdministrativeRequest:
                    result = GetFormHtmlSendAdministrativeRequest(Context);
                    break;
                case ProviderPageMode.SendKeyRequest:
                    result = GetFormHtmlSendKeyRequest(Context);
                    break;
                case ProviderPageMode.EnrollOTP:
                case ProviderPageMode.EnrollOTPAndSave:
                    result = GetFormHtmlEnrollOTP(Context);
                    break;
                case ProviderPageMode.EnrollEmail:
                case ProviderPageMode.EnrollEmailAndSave:
                    result = GetFormHtmlEnrollEmail(Context);
                    break;
                case ProviderPageMode.EnrollPhone:
                case ProviderPageMode.EnrollPhoneAndSave:
                    result = GetFormHtmlEnrollPhone(Context);
                    break;
                case ProviderPageMode.EnrollBiometrics:
                case ProviderPageMode.EnrollBiometricsAndSave:
                    result = GetFormHtmlEnrollBio(Context);
                    break;
                case ProviderPageMode.EnrollPin:
                case ProviderPageMode.EnrollPinAndSave:
                    result = GetFormHtmlEnrollPinCode(Context);
                    break;
            }
            return result;
        }

        /// <summary>
        /// IAdapterPresentationForm GetFormPreRenderHtml implementation
        /// </summary>
        public string GetFormPreRenderHtml(int lcid)
        {
            string result = string.Empty;
            switch (Context.UIMode)
            {
                case ProviderPageMode.Identification:
                    result = GetFormPreRenderHtmlIdentification(Context);
                    break;
                case ProviderPageMode.Registration: // User self registration and enable
                    result = GetFormPreRenderHtmlRegistration(Context);
                    break;
                case ProviderPageMode.Invitation: // admministrative user registration and let disabled
                    result = GetFormPreRenderHtmlInvitation(Context);
                    break;
                case ProviderPageMode.SelectOptions:
                    result = GetFormPreRenderHtmlSelectOptions(Context);
                    break;
                case ProviderPageMode.ChooseMethod:
                    result = GetFormPreRenderHtmlChooseMethod(Context);
                    break;
                case ProviderPageMode.ChangePassword:
                    result = GetFormPreRenderHtmlChangePassword(Context);
                    break;
                case ProviderPageMode.Bypass:
                    result = GetFormPreRenderHtmlBypass(Context);
                    break;
                case ProviderPageMode.Locking:
                    result = GetFormPreRenderHtmlLocking(Context);
                    break;
                case ProviderPageMode.ShowQRCode:
                    result = GetFormPreRenderHtmlShowQRCode(Context);
                    break;
                case ProviderPageMode.SendAuthRequest:
                    result = GetFormPreRenderHtmlSendCodeRequest(Context);
                    break;
                case ProviderPageMode.SendAdministrativeRequest:
                    result = GetFormPreRenderHtmlSendAdministrativeRequest(Context);
                    break;
                case ProviderPageMode.SendKeyRequest:
                    result = GetFormPreRenderHtmlSendKeyRequest(Context);
                    break;
                case ProviderPageMode.EnrollOTP:
                case ProviderPageMode.EnrollOTPAndSave:
                    result = GetFormPreRenderHtmlEnrollOTP(Context);
                    break;
                case ProviderPageMode.EnrollEmail:
                case ProviderPageMode.EnrollEmailAndSave:
                    result = GetFormPreRenderHtmlEnrollEmail(Context);
                    break;
                case ProviderPageMode.EnrollPhone:
                case ProviderPageMode.EnrollPhoneAndSave:
                    result = GetFormPreRenderHtmlEnrollPhone(Context);
                    break;
                case ProviderPageMode.EnrollBiometrics:
                case ProviderPageMode.EnrollBiometricsAndSave:
                    result = GetFormPreRenderHtmlEnrollBio(Context);
                    break;
                case ProviderPageMode.EnrollPin:
                case ProviderPageMode.EnrollPinAndSave:
                    result = GetFormPreRenderHtmlEnrollPinCode(Context);
                    break;
            }
            return result;
        }

        #region Identification
        /// <summary>
        /// GetFormPreRenderHtmlIdentification implementation
        /// </summary>
        private string GetFormPreRenderHtmlIdentification(AuthenticationContext usercontext)
        {
            string result = "<script type='text/javascript'>" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

            result += "function SetLinkData(frm, data)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('lnk');" + "\r\n";
            result += "   if (lnk)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      lnk.value = data;" + "\r\n";
            result += "   }" + "\r\n";
            result += "   frm.submit();" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlIdentification implementation
        /// </summary>
        private string GetFormHtmlIdentification(AuthenticationContext usercontext)
        {
            string result = "<form method=\"post\" id=\"IdentificationForm\" autocomplete=\"off\">";

            if (_isPermanentFailure)
            {
                if (!String.IsNullOrEmpty(usercontext.UIMessage))
                    result += "<p class=\"error\">" + usercontext.UIMessage + "</p></br>";
                else
                    result += "<p class=\"error\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlErrorRestartSession") + "</p></br>";
            }
            else
            {
                if (_ismessage)
                    result += "<p class=\"text fullWidth\" style=\"color: #6FA400\">" + usercontext.UIMessage + "</p></br>";
                else
                    result += "<p class=\"error\">" + usercontext.UIMessage + "</p></br>";
            }

            IExternalProvider prov = RuntimeAuthProvider.GetProvider(usercontext.PreferredMethod);
            if ((prov != null) && (prov.IsUIElementRequired(usercontext, RequiredMethodElements.CodeInputRequired)))
            {
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUILabel(usercontext) + " : </div>";
                result += "<input id=\"totp\" name=\"totp\" type=\"password\" placeholder=\"Verification Code\" class=\"text fullWidth\" autofocus=\"autofocus\" /></br>";
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUIMessage(usercontext) + "</div>";
                if (!string.IsNullOrEmpty(prov.GetUIWarningThirdPartyLabel(usercontext)) && (usercontext.IsSendBack))
                    result += "<div class=\"error smallText\"><label for=\"\"></label>" + prov.GetUIWarningThirdPartyLabel(usercontext) + "</div>";
                result += "<br />";
            }
            if ((prov != null) && (prov.IsUIElementRequired(usercontext, RequiredMethodElements.PinParameterRequired)))
            {
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + BaseExternalProvider.GetPINLabel(usercontext) + " : </div>";
                result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" placeholder=\"PIN Number\" class=\"text fullWidth\" /></br></br>";
            }

            if (Provider.HasAccessToOptions(prov))
            {
                if (usercontext.ShowOptions)
                    result += "<input id=\"options\" type=\"checkbox\" name=\"Options\" checked=\"true\" /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMAccessOptions") + "</br></br></br>";
                else
                    result += "<input id=\"options\" type=\"checkbox\" name=\"Options\" /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMAccessOptions") + "</br></br></br>";
            }
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
            result += "<input id=\"continueButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMConnexion") + "\" /></br></br>";
            result += "<a class=\"actionLink\" href=\"#\" id=\"nocode\" name=\"nocode\" onclick=\"return SetLinkData(IdentificationForm, '3')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMNoCode") + "</a>";
            result += "</form>";
            return result;
        }
        #endregion

        #region Registration
        /// <summary>
        /// GetFormPreRenderHtmlRegistration implementation
        /// </summary>
        private string GetFormPreRenderHtmlRegistration(AuthenticationContext usercontext)
        {
            string reg = @"/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,})+$/";
            string pho = @"/^\+(?:[0-9] ?){6,14}[0-9]$/";
            string pho10 = @"/^\d{10}$/";
            string phous = @"/^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$/";

            string result = "<script type='text/javascript'>" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

            result += "function ValidateRegistration(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var mailformat = " + reg + " ;" + "\r\n";
            result += "   var phoneformat = " + pho + " ;" + "\r\n";
            result += "   var phoneformat10 = " + pho10 + " ;" + "\r\n";
            result += "   var phoneformatus = " + phous + " ;" + "\r\n";
            result += "   var email = document.getElementById('email');" + "\r\n";
            result += "   var phone = document.getElementById('phone');" + "\r\n";
            result += "   var err = document.getElementById('errorText');" + "\r\n";

            result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
            result += "   if (lnk.value=='1')" + "\r\n";
            result += "   {" + "\r\n";

            if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.EmailParameterRequired))
            {
                result += "   if ((email) && (email.value==''))" + "\r\n";
                result += "   {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectEmail") + "\";" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "   }" + "\r\n";
                result += "   if ((email) && (email.value!==''))" + "\r\n";
                result += "   {" + "\r\n";
                result += "      if (!email.value.match(mailformat))" + "\r\n";
                result += "      {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectEmail") + "\";" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "      }" + "\r\n";
                result += "   }" + "\r\n";
            }
            if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.PhoneParameterRequired))
            {
                result += "   if ((phone) && (phone.value==''))" + "\r\n";
                result += "   {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectPhoneNumber") + "\";" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "   }" + "\r\n";
                result += "   if ((phone) && (phone.value!==''))" + "\r\n";
                result += "   {" + "\r\n";
                result += "      if (!phone.value.match(phoneformat) && !phone.value.match(phoneformat10) && !phone.value.match(phoneformatus) )" + "\r\n";
                result += "      {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectPhoneNumber") + "\";" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "      }" + "\r\n";
                result += "   }" + "\r\n";
            }
            result += "   }" + "\r\n";

            result += "   err.innerHTML = \"\";" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function ChangeRegistrationTitle()" + "\r\n";
            result += "{" + "\r\n";
            result += "   var email = document.getElementById('email');" + "\r\n";
            result += "   var title = document.getElementById('mfaGreetingDescription');" + "\r\n";
            result += "   if (title)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      title.innerHTML = \"<b>" + Resources.GetString(ResourcesLocaleKind.Titles, "RegistrationPageTitle") + "</b>\";" + "\r\n";
            result += "   }" + "\r\n";
            result += "   if (email)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      email.focus();" + "\r\n";
            result += "   }" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";

            result += "function fnlinkclicked(frm, data)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
            result += "   lnk.value = data;" + "\r\n";
            result += "   frm.submit();" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";

            result += "function fnbtnclicked(id)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
            result += "   lnk.value = id;" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlRegistration implementation
        /// </summary>
        private string GetFormHtmlRegistration(AuthenticationContext usercontext)
        {
            string result = "<script type=\"text/javascript\">" + "\r\n";
            result += "if (window.addEventListener)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.addEventListener('load', ChangeRegistrationTitle, false);" + "\r\n";
            result += "}" + "\r\n";
            result += "else if (window.attachEvent)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.attachEvent('onload', ChangeRegistrationTitle);" + "\r\n";
            result += "}" + "\r\n";
            result += "</script>" + "\r\n";

            result += "<form method=\"post\" id=\"registrationForm\" autocomplete=\"off\" onsubmit=\"return ValidateRegistration(this)\" >";
            if (!String.IsNullOrEmpty(usercontext.UIMessage))
                result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div>";
            else
                result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";

            result += GetPartHtmlPagingMethod(usercontext);

            IExternalProvider prov = null;
            switch (usercontext.PageID)
            {
                case 1: // Email IsMethodElementRequired
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Email);
                    break;
                case 2: // external API
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.External);
                    break;
                case 3: // Azure MFA
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Azure);
                    break;
                case 4: // Biometrics
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics);
                    break;
                case 5: // Pin Code
                    prov = null;
                    break;
                default: // Authentication Application
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Code);
                    break;
            }

            if ((prov != null) && (prov.Enabled))
            {
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.KeyParameterRequired)))
                {
                    string displaykey = string.Empty;
                    if (usercontext.KeyStatus == SecretKeyStatus.Success)
                    {
                        displaykey = KeysManager.EncodedKey(usercontext.UPN);
                    }
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGLabelAppKey") + "</div>";
                    result += "<input id=\"secretkey\" name=\"secretkey\" type=\"text\" readonly=\"true\" placeholder=\"DisplayKey\" class=\"text fullWidth\" style=\"background-color: #C0C0C0\" value=\"" + StripDisplayKey(displaykey) + "\"/></br></br>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.EmailParameterRequired)))
                {
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUICFGLabel(usercontext) + " : </div>";
                    result += "<input id=\"email\" name=\"email\" type=\"text\" readonly=\"true\" placeholder=\"Personal Email Address\" class=\"text fullWidth\" style=\"background-color: #C0C0C0\" value=\"" + usercontext.MailAddress + "\"/></br></br>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.PhoneParameterRequired)))
                {
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUICFGLabel(usercontext) + " : </div>";
                    result += "<input id=\"phone\" name=\"phone\" type=\"text\" readonly=\"true\" placeholder=\"Phone Number\" class=\"text fullWidth\" style=\"background-color: #C0C0C0\" value=\"" + usercontext.PhoneNumber + "\"/></br></br>";
                }

                List<AvailableAuthenticationMethod> lst = prov.GetAuthenticationMethods(usercontext);
                if (lst.Count > 1)
                {
                    AuthenticationResponseKind ov = AuthenticationResponseKind.Error;
                    if (prov.AllowOverride)
                    {
                        ov = prov.GetOverrideMethod(usercontext);
                        result += "<input id=\"optiongroup\" name=\"optionitem\" type=\"radio\" value=\"Default\" checked=\"checked\" /> " + prov.GetUIDefaultChoiceLabel(usercontext) + "<br /><br />";
                    }
                    int i = 1;
                    foreach (AvailableAuthenticationMethod met in lst)
                    {
                        if (ov != AuthenticationResponseKind.Error)
                        {
                            if (met.Method == ov)
                                result += "<input id=\"optiongroup" + i.ToString() + "\" name=\"optionitem\" type=\"radio\" value=\"" + met.Method.ToString() + "\" checked=\"checked\" /> " + prov.GetUIChoiceLabel(usercontext, met) + "<br /><br />";
                            else
                                result += "<input id=\"optiongroup" + i.ToString() + "\" name=\"optionitem\" type=\"radio\" value=\"" + met.Method.ToString() + "\" /> " + prov.GetUIChoiceLabel(usercontext, met) + "<br /><br />";
                        }
                        else
                            result += "<input id=\"optiongroup" + i.ToString() + "\" name=\"optionitem\" type=\"radio\" value=\"" + met.Method.ToString() + "\" /> " + prov.GetUIChoiceLabel(usercontext, met) + "<br /><br />";
                        i++;
                    }
                }
                else if (lst.Count <=0)
                {
                    result += "<div class=\"fieldMargin error smallText\"><label for=\"\"></label>No options available for " + prov.Description + "</div></br>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.KeyParameterRequired)))
                {
                    result += "</br>";
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollopt\" name=\"enrollopt\" onclick=\"fnlinkclicked(registrationForm, 3)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollOTP") + "</a>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.EmailParameterRequired)))
                {
                    result += "</br>";
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollemail\" name=\"enrollemail\" onclick=\"fnlinkclicked(registrationForm, 4)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollEmail") + "</a>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.PhoneParameterRequired)))
                {
                    result += "</br>";
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollphone\" name=\"enrollphone\" onclick=\"fnlinkclicked(registrationForm, 5)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollPhone") + "</a>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.BiometricParameterRequired)))
                {
                    result += "</br>";
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollbio\" name=\"enrollbio\" onclick=\"fnlinkclicked(registrationForm, 6)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollBiometrics") + "</a>";
                }
                if (!string.IsNullOrEmpty(prov.GetAccountManagementUrl(usercontext)))
                {
                    result += "</br>";
                    result += "<input type=\"checkbox\" id=\"manageaccount\" name=\"manageaccount\" > " + prov.GetUIAccountManagementLabel(usercontext) + "<br />";
                }
                result += "<br />";
            }
            else
            {   // PIN Code
                prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Code);
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + BaseExternalProvider.GetPINLabel(usercontext) + " : </div>";
                if (prov.PinRequired)
                {
                    if (usercontext.PinCode <= 0)
                        result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" readonly=\"true\" placeholder=\"PIN Number\" class=\"text fullWidth\" style=\"background-color: #C0C0C0\" value=\"" + Provider.Config.DefaultPin + "\"/></br></br>";
                    else
                        result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" readonly=\"true\" placeholder=\"PIN Number\" class=\"text fullWidth\" style=\"background-color: #C0C0C0\" value=\"" + usercontext.PinCode + "\"/></br></br>";
                    result += "</br>";
                }
                result += "<a class=\"actionLink\" href=\"#\" id=\"enrollpin\" name=\"enrollpin\" onclick=\"fnlinkclicked(registrationForm, 7)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollPinCode") + "</a>";
                result += "<br />";
            }

            result += GetPartHtmlSelectMethod(usercontext);

            if (!Provider.Config.UserFeatures.IsMFARequired())
                result += "<input id=\"disablemfa\" type=\"checkbox\" name=\"disablemfa\" /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMDisableMFA") + "</br>";

            result += "</br>";
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";

            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"0\" />";

            result += "<table><tr>";
            result += "<td>";
            result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDSave") + "\" onclick=\"fnbtnclicked(1)\" />";
            result += "</td>";
            result += "<td style=\"width: 15px\" />";
            result += "<td>";
            result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(2)\" />";
            result += "</td>";
            result += "</tr></table>";
            result += "</form>";
            return result;
        }
        #endregion

        #region Invitation
        /// <summary>
        /// GetFormPreRenderHtmlInvitation implementation
        /// </summary>
        private string GetFormPreRenderHtmlInvitation(AuthenticationContext usercontext)
        {
            string reg = @"/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,})+$/";
            string pho = @"/^\+(?:[0-9] ?){6,14}[0-9]$/";
            string pho10 = @"/^\d{10}$/";
            string phous = @"/^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$/";

            string result = "<script type='text/javascript'>" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

            result += "function ValidateInvitation(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var mailformat = " + reg + " ;" + "\r\n";
            result += "   var phoneformat = " + pho + " ;" + "\r\n";
            result += "   var phoneformat10 = " + pho10 + " ;" + "\r\n";
            result += "   var phoneformatus = " + phous + " ;" + "\r\n";
            result += "   var email = document.getElementById('email');" + "\r\n";
            result += "   var phone = document.getElementById('phone');" + "\r\n";
            result += "   var err = document.getElementById('errorText');" + "\r\n";

            result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
            result += "   if (lnk.value=='1')" + "\r\n";
            result += "   {" + "\r\n";

            if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.EmailParameterRequired))
            {
                result += "   if (email)" + "\r\n";
                result += "   {" + "\r\n";
                result += "         if (email.value == '')" + "\r\n";
                result += "         {" + "\r\n";
                result += "             err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectEmail") + "\";" + "\r\n";
                result += "             return false;" + "\r\n";
                result += "         }" + "\r\n";
                result += "         if (email.value !== '')" + "\r\n";
                result += "         {" + "\r\n";
                result += "             if (!email.value.match(mailformat))" + "\r\n";
                result += "             {" + "\r\n";
                result += "                 err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectEmail") + "\";" + "\r\n";
                result += "                 return false;" + "\r\n";
                result += "             }" + "\r\n";
                result += "         }" + "\r\n";
                result += "   }" + "\r\n";
            }
            if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.PhoneParameterRequired))
            {
                result += "   if ((phone) && (phone.value==''))" + "\r\n";
                result += "   {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectPhoneNumber") + "\";" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "   }" + "\r\n";
                result += "   if ((phone) && (phone.value!==''))" + "\r\n";
                result += "   {" + "\r\n";
                result += "      if (!phone.value.match(phoneformat) && !phone.value.match(phoneformat10) && !phone.value.match(phoneformatus) )" + "\r\n";
                result += "      {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectPhoneNumber") + "\";" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "      }" + "\r\n";
                result += "   }" + "\r\n";
            }
            result += "   }" + "\r\n";
            result += "   err.innerHTML = \"\";" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";
              
            result += "function ChangeInvitationTitle()" + "\r\n";
            result += "{" + "\r\n";
            result += "   var title = document.getElementById('mfaGreetingDescription');" + "\r\n";
            result += "   if (title)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      title.innerHTML = \"<b>" + Resources.GetString(ResourcesLocaleKind.Titles, "InvitationPageTitle") + "</b>\";" + "\r\n";
            result += "   }" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";

            result += "function fnbtnclicked(id)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
            result += "   lnk.value = id;" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlInvitation implementation
        /// </summary>
        private string GetFormHtmlInvitation(AuthenticationContext usercontext)
        {
            string result = "<script type=\"text/javascript\">" + "\r\n";
            result += "if (window.addEventListener)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.addEventListener('load', ChangeInvitationTitle, false);" + "\r\n";
            result += "}" + "\r\n";
            result += "else if (window.attachEvent)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.attachEvent('onload', ChangeInvitationTitle);" + "\r\n";
            result += "}" + "\r\n";
            result += "</script>" + "\r\n";

            result += "<form method=\"post\" id=\"invitationForm\" autocomplete=\"off\" onsubmit=\"return ValidateInvitation(this)\" >";
            if (!String.IsNullOrEmpty(usercontext.UIMessage))
                 result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div>";
            else
                result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";

            result += GetPartHtmlPagingMethod(usercontext);

         
            IExternalProvider prov = null;
            switch (usercontext.PageID)
            {
                case 1: // Email IsMethodElementRequired
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Email);
                    break;
                case 2: // external API
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.External);
                    break;
                case 3: // Azure MFA
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Azure);
                    break;
                default: // Authentication Application
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Code);
                    break;
            }

            if ((prov != null) && (prov.Enabled))
            {
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.KeyParameterRequired)))
                {
                    string displaykey = string.Empty;
                    if (usercontext.KeyStatus == SecretKeyStatus.Success)
                    {
                        displaykey = KeysManager.EncodedKey(usercontext.UPN);
                    }
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGLabelAppKey") + "</div>";
                    result += "<input id=\"secretkey\" name=\"secretkey\" type=\"text\" readonly=\"true\" placeholder=\"DisplayKey\" class=\"text fullWidth\" style=\"background-color: #C0C0C0\" value=\"" + StripDisplayKey(displaykey) + "\"/></br></br>";
                    result += "</br>";
                    result += "<input id=\"nextButton\" type=\"submit\" class=\"submit\" name=\"nextButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMRecordNewKey") + "\" onclick=\"fnbtnclicked(3)\" />";                    
                    result += "</br>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.EmailParameterRequired)))
                {
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUICFGLabel(usercontext) + " : </div>";
                    result += "<input id=\"email\" name=\"email\" type=\"text\" readonly=\"true\" placeholder=\"Personal Email Address\" class=\"text fullWidth\" style=\"background-color: #C0C0C0\" value=\"" + usercontext.MailAddress + "\"/></br></br></br>";
                    result += "</br>";
                    result += "<input id=\"emailButton\" type=\"submit\" class=\"submit\" name=\"emailButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMRecordNewEmail") + "\" onclick=\"fnbtnclicked(4)\" />";
                    result += "</br>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.PhoneParameterRequired)))
                {
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUICFGLabel(usercontext) + " : </div>";
                    result += "<input id=\"phone\" name=\"phone\" readonly=\"true\" type=\"text\" placeholder=\"Phone Number\" class=\"text fullWidth\" style=\"background-color: #C0C0C0\" value=\"" + usercontext.PhoneNumber + "\"/></br></br></br>";
                    result += "</br>";
                    result += "<input id=\"phoneButton\" type=\"submit\" class=\"submit\" name=\"phoneButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIVerifyPhone") + "\" onclick=\"fnbtnclicked(5)\" />";
                    result += "</br>";
                }
                List<AvailableAuthenticationMethod> lst = prov.GetAuthenticationMethods(usercontext);
                if (lst.Count > 1)
                {
                    AuthenticationResponseKind ov = AuthenticationResponseKind.Error;
                    if (prov.AllowOverride)
                    {
                        ov = prov.GetOverrideMethod(usercontext);
                        result += "<input id=\"optiongroup\" name=\"optionitem\" type=\"radio\" value=\"Error\" checked=\"checked\" /> " + prov.GetUIDefaultChoiceLabel(usercontext) + "<br /><br />";
                    }
                    int i = 1;
                    foreach (AvailableAuthenticationMethod met in lst)
                    {
                        if (ov != AuthenticationResponseKind.Error)
                        {
                            if (met.Method == ov)
                                result += "<input id=\"optiongroup" + i.ToString() + "\" name=\"optionitem\" type=\"radio\" value=\"" + met.Method.ToString() + "\" checked=\"checked\" /> " + prov.GetUIChoiceLabel(usercontext, met) + "<br /><br />";
                            else
                                result += "<input id=\"optiongroup" + i.ToString() + "\" name=\"optionitem\" type=\"radio\" value=\"" + met.Method.ToString() + "\" /> " + prov.GetUIChoiceLabel(usercontext, met) + "<br /><br />";
                        }
                        else
                            result += "<input id=\"optiongroup" + i.ToString() + "\" name=\"optionitem\" type=\"radio\" value=\"" + met.Method.ToString() + "\" /> " + prov.GetUIChoiceLabel(usercontext, met) + "<br /><br />";
                        i++;
                    }
                }
                else if (lst.Count <= 0)
                {
                    result += "<div class=\"fieldMargin error smallText\"><label for=\"\"></label>No options available for " + prov.Description + "</div></br>";
                }
                if ((prov.PinRequired) && (prov.IsUIElementRequired(usercontext, RequiredMethodElements.PinParameterRequired)))
                {
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + BaseExternalProvider.GetPINLabel(usercontext) + " : </div>";
                    if (usercontext.PinCode <=0 )
                        result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" placeholder=\"Default PIN code\" class=\"text fullWidth\" value=\"" + Provider.Config.DefaultPin + "\"/></br></br>";
                    else
                        result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" placeholder=\"PIN code\" class=\"text fullWidth\" value=\"" + usercontext.PinCode + "\"/></br></br>";
                }
                result += "<br />";
                if (!string.IsNullOrEmpty(prov.GetAccountManagementUrl(usercontext)))
                    result += "<input type=\"checkbox\" id=\"manageaccount\" name=\"manageaccount\" > " + prov.GetUIAccountManagementLabel(usercontext) + "<br /><br />";
            }

            result += GetPartHtmlSelectMethod(usercontext);

            if (!Provider.Config.UserFeatures.IsMFARequired())
                result += "<input id=\"disablemfa\" type=\"checkbox\" name=\"disablemfa\" /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMDisableMFA") + "</br>";

            result += "</br>";
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";

            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"0\" />";
         //   result += "<input id=\"links\" type=\"hidden\" name=\"links\" value=\"0\"/>";

            result += "<table><tr>";
            result += "<td>";
            result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlINSRequest") + "\" onclick=\"fnbtnclicked(1)\" />";
            result += "</td>";
            result += "<td style=\"width: 15px\" />";
            result += "<td>";
            result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(2)\" />";
            result += "</td>";
            result += "</tr></table>";
            result += "</form>";
            return result;
        }
        #endregion

        #region SelectOptions
        /// <summary>
        /// GetFormPreRenderHtmlSelectOptions implementation
        /// </summary>
        private string GetFormPreRenderHtmlSelectOptions(AuthenticationContext usercontext)
        {
            string result = "<script type='text/javascript'>" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

            result += "function SetLinkTitle(frm, data)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('lnk');" + "\r\n";
            result += "   if (lnk)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      lnk.value = data;" + "\r\n";
            result += "   }" + "\r\n";
            result += "   var donut = document.getElementById('cookiePullWaitingWheel');" + "\r\n";
            result += "   if (donut)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      donut.style.visibility = 'visible';" + "\r\n";
            result += "   }" + "\r\n";
            result += "   frm.submit();" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";

            result += "function fnbtnclicked()" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
            result += "   lnk.value = 1;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";
            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlSelectOptions implementation
        /// </summary>
        private string GetFormHtmlSelectOptions(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            bool showdonut = false;
            result += "<form method=\"post\" id=\"selectoptionsForm\" autocomplete=\"off\" >";
            if (!String.IsNullOrEmpty(usercontext.UIMessage))
            {
                if (_ismessage)
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" style=\"color: #6FA400\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div>";
                else
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">"+ usercontext.UIMessage +"</label></div>";
            } 
            else 
                result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";

            if (Provider.Config.UserFeatures.CanManageOptions())
            {
                result += "<a class=\"actionLink\" href=\"#\" id=\"chgopt\" name=\"chgopt\" onclick=\"return SetLinkTitle(selectoptionsForm, '1')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlChangeConfiguration") + "</a>";
                showdonut = RuntimeAuthProvider.IsProviderAvailable(usercontext, PreferredMethod.Azure);
            }
            if (Provider.Config.UserFeatures.CanManagePassword())
            {
                if (!Provider.Config.CustomUpdatePassword)
                    result += "<a class=\"actionLink\" href=\"/adfs/portal/updatepassword?username=" + usercontext.UPN + "\" id=\"chgpwd\" name=\"chgpwd\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlChangePassword") + "</a>";
                else
                    result += "<a class=\"actionLink\" href=\"#\" id=\"chgpwd\" name=\"chgpwd\" onclick=\"return SetLinkTitle(selectoptionsForm, '2')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlChangePassword") + "</a>";
            }
            result += "</br>";
            if (Provider.Config.UserFeatures.CanEnrollDevices())
            {
                IExternalProvider prov = null;
                if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.OTPLinkRequired))
                {
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Code);
                    if (Provider.HasAccessToOptions(prov))
                        result += "<a class=\"actionLink\" href=\"#\" id=\"enrollopt\" name=\"enrollopt\" onclick=\"return SetLinkTitle(selectoptionsForm, '3')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollOTP") + "</a>";
                }
                if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.BiometricLinkRequired))
                {
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics);
                    if (Provider.HasAccessToOptions(prov))
                        result += "<a class=\"actionLink\" href=\"#\" id=\"enrollbio\" name=\"enrollbio\" onclick=\"return SetLinkTitle(selectoptionsForm, '4')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollBiometric") + "</a>";
                }
                if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.EmailLinkRequired))
                {
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Email);
                    if (Provider.HasAccessToOptions(prov))
                        result += "<a class=\"actionLink\" href=\"#\" id=\"enrollemail\" name=\"enrollemail\" onclick=\"return SetLinkTitle(selectoptionsForm, '5')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollEmail") + "</a>";
                }
                if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.PhoneLinkRequired))
                {
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.External);
                    if (Provider.HasAccessToOptions(prov))
                        result += "<a class=\"actionLink\" href=\"#\" id=\"enrollphone\" name=\"enrollphone\" onclick=\"return SetLinkTitle(selectoptionsForm, '6')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollPhone") + "</a>";
                }
                if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.PinLinkRequired))
                {
                    if (Provider.HasAccessToPinCode(prov))
                        result += "<a class=\"actionLink\" href=\"#\" id=\"enrollpin\" name=\"enrollpin\"  onclick=\"return SetLinkTitle(selectoptionsForm, '7')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollPinCode") + "</a>";
                }
            }
            result += "<script>";
            result += "   document.cookie = 'showoptions=;expires=Thu, 01 Jan 1970 00:00:01 GMT;path=/adfs/'";
            result += "</script>";
            result += "</br>";
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"0\" />";
            result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
            result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMConnexion") + "\" onclick=\"fnbtnclicked()\"/>";
            if (showdonut)
            {
                result += "</br></br></br>";
                result += GetPartHtmlDonut(false);
            }
            result += "</form>";
            return result;
        }
        #endregion

        #region ChooseMethod
        /// <summary>
        /// GetFormPreRenderHtmlChooseMethod implementation
        /// </summary>
        private string GetFormPreRenderHtmlChooseMethod(AuthenticationContext usercontext)
        {
            string result = "<script type='text/javascript'>" + "\r\n";
            result += "//<![CDATA[" + "\r\n";
            result += "function ChangeChooseMethodTitle()" + "\r\n";
            result += "{" + "\r\n";
            result += "   var title = document.getElementById('mfaGreetingDescription');" + "\r\n";
            result += "   if (title)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      title.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Titles, "MustUseCodePageTitle") + "\";" + "\r\n";
            result += "   }" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function ChooseMethodChanged()" + "\r\n";
            result += "{" + "\r\n";
            result += "   var data = document.getElementById('opt3');" + "\r\n";
            result += "   var edit = document.getElementById('stmail');" + "\r\n";
            result += "   if (data.checked)" + "\r\n";
            result += "   {" + "\r\n";
            result += "         edit.disabled = false;" + "\r\n";
            result += "   }" + "\r\n";
            result += "   else" + "\r\n";
            result += "   {" + "\r\n";
            result += "         edit.disabled = true;" + "\r\n";
            result += "   }" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function fnbtnclicked(id)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
            result += "   lnk.value = id;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlChooseMethod implementation
        /// </summary>
        private string GetFormHtmlChooseMethod(AuthenticationContext usercontext)
        {
            string result = "<script type=\"text/javascript\">" + "\r\n";
            result += "if (window.addEventListener)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.addEventListener('load', ChangeChooseMethodTitle, false);" + "\r\n";
            result += "}" + "\r\n";
            result += "else if (window.attachEvent)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.attachEvent('onload', ChangeChooseMethodTitle);" + "\r\n";
            result += "}" + "\r\n";
            result += "</script>" + "\r\n";

            result += "<form method=\"post\" id=\"ChooseMethodForm\" autocomplete=\"off\" \" >";
            result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";

            PreferredMethod method = GetMethod4FBUsers(usercontext);
            if (RuntimeAuthProvider.IsProviderAvailableForUser(usercontext, PreferredMethod.Code))
            {
                result += "<input id=\"opt1\" name=\"opt\" type=\"radio\" value=\"0\" onchange=\"ChooseMethodChanged()\" "+ (((method == PreferredMethod.Code) || (method == PreferredMethod.Choose)) ? "checked=\"checked\"> " : "> ") + RuntimeAuthProvider.GetProvider(PreferredMethod.Code).GetUIChoiceLabel(usercontext)  + "<br /><br/>";
            }

            if (RuntimeAuthProvider.IsProviderAvailableForUser(usercontext, PreferredMethod.Email))
            {
                result += "<input id=\"opt3\" name=\"opt\" type=\"radio\" value=\"2\" onchange=\"ChooseMethodChanged()\" " + ((method == PreferredMethod.Email) ? "checked=\"checked\"> " : "> ") + RuntimeAuthProvider.GetProvider(PreferredMethod.Email).GetUIChoiceLabel(usercontext) + "<br />";
                result += "<div style=\"text-indent: 20px;\"><i>" + usercontext.MailAddress + "</i></div><br/>";
            }

            if (RuntimeAuthProvider.IsProviderAvailableForUser(usercontext, PreferredMethod.External))
            {
               result += "<input id=\"opt2\" name=\"opt\" type=\"radio\" value=\"1\" onchange=\"ChooseMethodChanged()\" " + ((method == PreferredMethod.External) ? "checked=\"checked\"> " : "> ") + RuntimeAuthProvider.GetProvider(PreferredMethod.External).GetUIChoiceLabel(usercontext) + "<br/><br/>";
            }

            if (RuntimeAuthProvider.IsProviderAvailableForUser(usercontext, PreferredMethod.Azure))
            {
                result += "<input id=\"opt4\" name=\"opt\" type=\"radio\" value=\"3\" onchange=\"ChooseMethodChanged()\" " + ((method == PreferredMethod.Azure) ? "checked=\"checked\"> " : "> ") + RuntimeAuthProvider.GetProvider(PreferredMethod.Azure).GetUIChoiceLabel(usercontext) + "<br/><br/>";
            }

            result += "<br/>";
            if (Provider.KeepMySelectedOptionOn())
                result += "<input id=\"remember\" type=\"checkbox\" name=\"Remember\"> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlCHOOSEOptionRemember") + "<br /><br />";
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\">";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\">";
            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"0\" />";

            result += "<table><tr>";
            result += "<td>";
            result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlCHOOSEOptionSendCode") + "\" onclick=\"fnbtnclicked(0)\" />";
            result += "</td>";
            result += "<td style=\"width: 15px\" />";
            result += "<td>";
            result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
            result += "</td>";
            result += "</tr></table>";
            result += "</form>";
            return result;
        }

        /// <summary>
        /// GetMethod4FBUsers method implementation
        /// </summary>
        private PreferredMethod GetMethod4FBUsers(AuthenticationContext usercontext)
        {
            PreferredMethod method = usercontext.PreferredMethod;
            PreferredMethod idMethod = usercontext.PreferredMethod;
            do
            {
                idMethod++;
                if (idMethod > PreferredMethod.Azure)
                    idMethod = PreferredMethod.Code;
                else if (idMethod == method)
                    return method;
            } while (!RuntimeAuthProvider.IsProviderAvailableForUser(usercontext, idMethod));
            return idMethod;

        }
        #endregion

        #region ChangePassword
        /// <summary>
        /// GetFormPreRenderHtmlChangePassword implementation
        /// </summary>
        private string GetFormPreRenderHtmlChangePassword(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            if (Provider.Config.CustomUpdatePassword)
            {
                result += "<script type='text/javascript'>" + "\r\n";
                result += "//<![CDATA[" + "\r\n";

                result += "function ValidChangePwd(frm)" + "\r\n";
                result += "{" + "\r\n";
                result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
                result += "   if (lnk.value=='1')" + "\r\n";
                result += "   {" + "\r\n";
                result += "     var err = document.getElementById('errorText');" + "\r\n";
                result += "     if (frm.elements['oldpwd'].value == \"\")" + "\r\n";
                result += "     {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidPassActualError") + "\";" + "\r\n";
                result += "         frm.elements['oldpwd'].focus();" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "     }" + "\r\n";
                result += "     if (frm.elements['newpwd'].value == \"\")" + "\r\n";
                result += "     {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidPassNewError") + "\";" + "\r\n";
                result += "         frm.elements['newpwd'].focus();" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "     }" + "\r\n";
                result += "     if (frm.elements['cnfpwd'].value == \"\")" + "\r\n";
                result += "     {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidConfirmNewPassError") + "\";" + "\r\n";
                result += "         frm.elements['cnfpwd'].focus();" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "     }" + "\r\n";
                result += "     if (frm.elements['cnfpwd'].value != frm.elements['newpwd'].value)" + "\r\n";
                result += "     {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidNewPassError") + "\";" + "\r\n";
                result += "         frm.elements['cnfpwd'].focus();" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "     }" + "\r\n";
                result += "     err.innerHTML = \"\";" + "\r\n";
                result += "   }" + "\r\n";
                result += "   return true;" + "\r\n";
                result += "}" + "\r\n";
                result += "\r\n";

                result += "function fnbtnclicked(id)" + "\r\n";
                result += "{" + "\r\n";
                result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
                result += "   lnk.value = id;" + "\r\n";
                result += "}" + "\r\n";
                result += "\r\n";

                result += "function ChangePasswordTitle()" + "\r\n";
                result += "{" + "\r\n";
                result += "   var title = document.getElementById('mfaGreetingDescription');" + "\r\n";
                result += "   if (title)" + "\r\n";
                result += "   {" + "\r\n";
                result += "      title.innerHTML = \"<b>" + Resources.GetString(ResourcesLocaleKind.Titles, "PasswordPageTitle") + "</b>\";" + "\r\n";
                result += "   }" + "\r\n";
                result += "   return true;" + "\r\n";
                result += "}" + "\r\n";

                result += "//]]>" + "\r\n";
                result += "</script>" + "\r\n";
            }
            return result;
        }

        /// <summary>
        /// GetFormHtmlChangePassword implementation
        /// </summary>
        private string GetFormHtmlChangePassword(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            if (Provider.Config.CustomUpdatePassword)
            {
                result += "<script type=\"text/javascript\">" + "\r\n";
                result += "if (window.addEventListener)" + "\r\n";
                result += "{" + "\r\n";
                result += "   window.addEventListener('load', ChangePasswordTitle, false);" + "\r\n";
                result += "}" + "\r\n";
                result += "else if (window.attachEvent)" + "\r\n";
                result += "{" + "\r\n";
                result += "   window.attachEvent('onload', ChangePasswordTitle);" + "\r\n";
                result += "}" + "\r\n";
                result += "</script>" + "\r\n";

                result += "<form method=\"post\" id=\"passwordForm\" autocomplete=\"off\" onsubmit=\"return ValidChangePwd(this)\" >";
                result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDLabelActual") + "</div>";
                result += "<input id=\"oldpwd\" name=\"oldpwd\" type=\"password\" placeholder=\"Current Password\" class=\"text fullWidth\"/></br></br>";
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDLabelNew") + "</div>";
                result += "<input id=\"newpwd\" name=\"newpwd\" type=\"password\" placeholder=\"New Password\" class=\"text fullWidth\"/></br>";
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDLabelNewConfirmation") + "</div>";
                result += "<input id=\"cnfpwd\" name=\"cnfpwd\" type=\"password\" placeholder=\"Confirm New Password\" class=\"text fullWidth\"/></br></br></br>";
                result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"1\" />";
                result += "<table><tr>";
                result += "<td>";
                result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDSave") + "\" onClick=\"fnbtnclicked(1)\" />";
                result += "</td>";
                result += "<td style=\"width: 15px\" />";
                result += "<td>";
                result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onClick=\"fnbtnclicked(2)\"/>";
                result += "</td>";
                result += "</tr></table>";
                result += "</form>";
            }
            return result;
        }
        #endregion

        #region Bypass
        /// <summary>
        /// GetFormPreRenderHtmlBypass implementation
        /// </summary>
        private string GetFormPreRenderHtmlBypass(AuthenticationContext usercontext)
        {
            string result = "<script type='text/javascript'>" + "\r\n";
            result += "//<![CDATA[" + "\r\n";
            result += "function OnAutoPost(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var title = document.getElementById('mfaGreetingDescription');" + "\r\n";
            result += "   if (title)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      title.innerHTML = \"<b>" + Resources.GetString(ResourcesLocaleKind.Titles, "TitleRedirecting") + "</b>\";" + "\r\n";
            result += "   }" + "\r\n";
            result += "   document.getElementById('bypassForm').submit();" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";
            if (!string.IsNullOrEmpty(usercontext.AccountManagementUrl))
            {
                result += "\r\n";
                result += "function OnUnload(frm)" + "\r\n";
                result += "{" + "\r\n";
                result += "   window.open('" + usercontext.AccountManagementUrl+ "', '_blank');" + "\r\n";
                result += "   return true;" + "\r\n";
                result += "}" + "\r\n";
                result += "\r\n";
            }
            result += "\r\n";
            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlBypass implementation
        /// </summary>
        private string GetFormHtmlBypass(AuthenticationContext usercontext)
        {
            IExternalProvider prov = RuntimeAuthProvider.GetProvider(usercontext.PreferredMethod);
            bool needinput = ((usercontext.IsTwoWay) && (prov != null) && (prov.IsUIElementRequired(usercontext, RequiredMethodElements.PinParameterRequired)));
            string result = "<script type=\"text/javascript\">" + "\r\n";
            result += "if (window.addEventListener)" + "\r\n";
            result += "{" + "\r\n";
            if (!needinput)
                result += "   window.addEventListener('load', OnAutoPost, false);" + "\r\n";
            if (!string.IsNullOrEmpty(usercontext.AccountManagementUrl))
                result += "   window.addEventListener('unload', OnUnload, false);" + "\r\n";
            result += "}" + "\r\n";
            result += "else if (window.attachEvent)" + "\r\n";
            result += "{" + "\r\n";
            if (!needinput)
                result += "   window.attachEvent('onload', OnAutoPost);" + "\r\n";
            if (!string.IsNullOrEmpty(usercontext.AccountManagementUrl))
                result += "   window.attachEvent('unload', OnUnload);" + "\r\n";
            result += "}" + "\r\n";
            result += "</script>" + "\r\n";
            if (needinput)
            {
                result += "<form method=\"post\" id=\"bypassForm\" autocomplete=\"off\" title=\"PIN Confirmation\" >";
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + BaseExternalProvider.GetPINLabel(usercontext) + " : </div>";
                result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" placeholder=\"PIN Number\" class=\"text fullWidth\" /></br></br>";
                result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                result += "<input id=\"continueButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMConnexion") + "\" /></br></br>";
            }
            else
            {
                result += "<form method=\"post\" id=\"bypassForm\" autocomplete=\"off\" title=\"Redirecting\" >";
                result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            }
            result += "</form>";
            return result;
        }
        #endregion

        #region Locking
        /// <summary>
        /// GetFormPreRenderHtmlLocking implementation
        /// </summary>
        private string GetFormPreRenderHtmlLocking(AuthenticationContext usercontext)
        {
            string result = string.Empty;
             result += "<script type=\"text/javascript\">" + "\r\n";
             result += "//<![CDATA[" + "\r\n";

             result += "function fnbtnclicked(id)" + "\r\n";
             result += "{" + "\r\n";
             result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
             result += "   lnk.value = id;" + "\r\n";
             result += "}" + "\r\n";
             result += "\r\n";

             result += "//]]>" + "\r\n";
             result += "</script>" + "\r\n"; 
            return result;
        }

        /// <summary>
        /// GetFormHtmlLocking implementation
        /// </summary>
        private string GetFormHtmlLocking(AuthenticationContext usercontext)
        {
            string result = "<form method=\"post\" id=\"lockingForm\" autocomplete=\"off\">";
            if (_isPermanentFailure)
            {
                if (!String.IsNullOrEmpty(usercontext.UIMessage))
                    result += "<p class=\"error\">" + usercontext.UIMessage + "</p></br>";
                else
                    result += "<p class=\"error\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlErrorRestartSession") + "</p></br>";
            }
            else
            {
                if (_ismessage)
                    result += "<p class=\"text fullWidth\" style=\"color: #6FA400\">" + usercontext.UIMessage + "</p></br>";
                else
                    result += "<p class=\"error\">" + usercontext.UIMessage + "</p></br>";
            }

            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"1\" />";
            result += "<table><tr>";
            result += "<td>";
            result += "<input id=\"continueButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMOk") + "\" onClick=\"fnbtnclicked(1)\" />";
            result += "</td>";
            result += "<td style=\"width: 15px\" />";
            result += "<td>";

            if (!usercontext.IsRegistered)
            {
                if (Provider.Config.UserFeatures.IsRegistrationRequired() && (Provider.Config.AdvertisingDays.OnFire))
                {
                    result += "<input id=\"GoToRegButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMGotoInscription") + "\" onClick=\"fnbtnclicked(2)\"/>";
                }
                else if (Provider.Config.UserFeatures.IsRegistrationAllowed() && (Provider.Config.AdvertisingDays.OnFire))
                {
                    result += "<input id=\"GoToRegButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMGoToRegistration") + "\" onClick=\"fnbtnclicked(3)\"/>";
                }
            }
            else if (!usercontext.Enabled)
            {
                if (Provider.Config.UserFeatures.IsMFAAllowed() && (Provider.Config.AdvertisingDays.OnFire))
                {
                    result += "<input id=\"GoToRegButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMGoToRegistration") + "\" onClick=\"fnbtnclicked(4)\"/>";
                }
            }
            result += "</td>";
            result += "</tr></table>";
            result += "</form>";
            return result;
        }
        #endregion

        #region QRCode
        /// <summary>
        /// GetFormPreRenderHtmlShowQRCode implementation
        /// </summary>
        /// <param name="usercontext"></param>
        /// <returns></returns>
        private string GetFormPreRenderHtmlShowQRCode(AuthenticationContext usercontext)
        {
            return string.Empty;
        }

        /// <summary>
        /// GetFormHtmlShowQRCode implementation
        /// </summary>
        private string GetFormHtmlShowQRCode(AuthenticationContext usercontext)
        {
            string result = "<form method=\"post\" id=\"loginForm\" autocomplete=\"off\" >";
            result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWRQRCode") + "</div></br>";
            result += "<p style=\"text-align:center\"><img id=\"qr\" src=\"data:image/png;base64," + Provider.GetQRCodeString(usercontext) + "\"/></p></br>";
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\"OK\" />";
            result += "</form>";
            return result;
        }
        #endregion

        #region CodeRequest
        /// <summary>
        /// GetFormPreRenderHtmlSendCodeRequest implementation
        /// </summary>
        private string GetFormPreRenderHtmlSendCodeRequest(AuthenticationContext usercontext)
        {
            string dt = DateTime.Now.AddMilliseconds(Provider.Config.DeliveryWindow).ToString("R");
            string result = "<script type='text/javascript'>" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

            result += "function OnRefreshPost(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   document.getElementById('refreshForm').submit()" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";
            result += "\r\n"; 

            result += "function SetLinkTitle(frm, data)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('lnk');" + "\r\n";
            result += "   if (lnk)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      lnk.value = data;" + "\r\n";
            result += "   }" + "\r\n";
            result += "   frm.submit();" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";

            result += "function SetOptions(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var opt = document.getElementById('options');" + "\r\n";
            result += "   if (opt)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      if (opt.checked)" + "\r\n";
            result += "      {" + "\r\n";
            result += "          document.cookie = 'showoptions=1;expires=" + dt + ";path=/adfs/'" + "\r\n";
            result += "      }" + "\r\n";
            result += "      else" + "\r\n";
            result += "      {" + "\r\n";
            result += "          document.cookie = 'showoptions=;expires=Thu, 01 Jan 1970 00:00:01 GMT'" + "\r\n";
            result += "      }" + "\r\n";
            result += "   }" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlCodeRequest implementation
        /// </summary>
        private string GetFormHtmlSendCodeRequest(AuthenticationContext usercontext)
        {
            string result = "<script type=\"text/javascript\">" + "\r\n";
            result += "if (window.addEventListener)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.addEventListener('load', OnRefreshPost, false);" + "\r\n";
            result += "}" + "\r\n";
            result += "else if (window.attachEvent)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.attachEvent('onload', OnRefreshPost);" + "\r\n";
            result += "}" + "\r\n";
            result += "</script>" + "\r\n"; 

            result += "<form method=\"post\" id=\"refreshForm\" autocomplete=\"off\" \">";

            if (_isPermanentFailure)
            {
                if (!String.IsNullOrEmpty(usercontext.UIMessage))
                    result += "<p class=\"error\">" + usercontext.UIMessage + "</p></br>";
                else
                    result += "<p class=\"error\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlErrorRestartSession") + "</p></br>";
            }
            else
            {
                if (_ismessage)
                    result += "<p class=\"text fullWidth\" style=\"color: #6FA400\">" + usercontext.UIMessage + "</p></br>";
                else
                    result += "<p class=\"error\">" + usercontext.UIMessage + "</p></br>";
            }

            if (usercontext.IsRemote)
            {
                IExternalProvider prov = RuntimeAuthProvider.GetProvider(usercontext.PreferredMethod);
                result += GetPartHtmlDonut();
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUIMessage(usercontext) + "</div></br>";
                if (!string.IsNullOrEmpty(prov.GetUIWarningInternetLabel(usercontext)) && (usercontext.IsRemote))
                    result += "<div class=\"error smallText\"><label for=\"\"></label>" + prov.GetUIWarningInternetLabel(usercontext) + "</div>";
                if (!string.IsNullOrEmpty(prov.GetUIWarningThirdPartyLabel(usercontext)) && (usercontext.IsTwoWay))
                    result += "<div class=\"error smallText\"><label for=\"\"></label>" + prov.GetUIWarningThirdPartyLabel(usercontext) + "</div>";
                result += "<br />";
                if (usercontext.IsTwoWay && (Provider.Config.UserFeatures.CanAccessOptions()))
                {
                    result += "<input id=\"options\" type=\"checkbox\" name=\"options\" onclick=\"SetOptions(refreshForm)\"; /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMAccessOptions");
                    result += "<script>";
                    result += "   document.cookie = 'showoptions=;expires=Thu, 01 Jan 1970 00:00:01 GMT;path=/adfs/'";
                    result += "</script>";
                    result += "</br></br></br>";
                }
                result += "<a class=\"actionLink\" href=\"#\" id=\"nocode\" name=\"nocode\" onclick=\"return SetLinkTitle(refreshForm, '3')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMNoCode") + "</a>";
                result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
            }
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";

            result += "</form>";
            return result;
        }
        #endregion

        #region InvitationRequest
        /// <summary>
        /// GetFormPreRenderHtmlSendAdministrativeRequest implementation
        /// </summary>
        private string GetFormPreRenderHtmlSendAdministrativeRequest(AuthenticationContext usercontext)
        {
            string result = "<script type='text/javascript'>" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

            result += "function OnRefreshPost(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   document.getElementById('invitationReqForm').submit()" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";
            result += "\r\n";

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlSendAdministrativeRequest implementation
        /// </summary>
        private string GetFormHtmlSendAdministrativeRequest(AuthenticationContext usercontext)
        {
            string result = "<script type=\"text/javascript\">" + "\r\n";
            result += "if (window.addEventListener)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.addEventListener('load', OnRefreshPost, false);" + "\r\n";
            result += "}" + "\r\n";
            result += "else if (window.attachEvent)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.attachEvent('onload', OnRefreshPost);" + "\r\n";
            result += "}" + "\r\n";
            result += "</script>" + "\r\n";

            result += "<form method=\"post\" id=\"invitationReqForm\" autocomplete=\"off\" \">";
            result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlINSWaitRequest") + "</div>";

            IExternalAdminProvider admprov = RuntimeAuthProvider.GetAdministrativeProvider(Provider.Config);
            result += GetPartHtmlDonut();
            if (admprov != null)
            {
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + admprov.GetUIInscriptionMessageLabel(usercontext) + "</div></br>";
                if (!string.IsNullOrEmpty(admprov.GetUIWarningInternetLabel(usercontext)) && (usercontext.IsRemote))
                    result += "<div class=\"error smallText\"><label for=\"\"></label>" + admprov.GetUIWarningInternetLabel(usercontext) + "</div>";
            }
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "</form>";
            return result;
        }
        #endregion

        #region SendKeyRequest
        /// <summary>
        /// GetFormPreRenderHtmlSendKeyRequest method implementation
        /// </summary>
        private string GetFormPreRenderHtmlSendKeyRequest(AuthenticationContext usercontext)
        {
            string result = "<script type='text/javascript'>" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

            result += "function OnAutoRefreshPost(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   document.getElementById('sendkeyReqForm').submit()" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";
            result += "\r\n";

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlSendKeyRequest method implementation
        /// </summary>
        private string GetFormHtmlSendKeyRequest(AuthenticationContext usercontext)
        {
            string result = "<script type=\"text/javascript\">" + "\r\n";
            result += "if (window.addEventListener)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.addEventListener('load', OnAutoRefreshPost, false);" + "\r\n";
            result += "}" + "\r\n";
            result += "else if (window.attachEvent)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.attachEvent('onload', OnAutoRefreshPost);" + "\r\n";
            result += "}" + "\r\n";
            result += "</script>" + "\r\n";

            result += "<form method=\"post\" id=\"sendkeyReqForm\" autocomplete=\"off\" \">";
            result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlINSWaitRequest") + "</div>";

            IExternalAdminProvider admprov = RuntimeAuthProvider.GetAdministrativeProvider(Provider.Config);

            result += GetPartHtmlDonut();
            if (admprov != null)
            {
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + admprov.GetUISecretKeyMessageLabel(usercontext) + "</div></br>";
                if (!string.IsNullOrEmpty(admprov.GetUIWarningInternetLabel(usercontext)) && (usercontext.IsRemote))
                    result += "<div class=\"error smallText\"><label for=\"\"></label>" + admprov.GetUIWarningInternetLabel(usercontext) + "</div>";
            }
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "</form>";
            return result;
        }
        #endregion

        #region EnrollOTP
        /// <summary>
        /// GetFormPreRenderHtmlEnrollOTP method implementation
        /// </summary>
        private string GetFormPreRenderHtmlEnrollOTP(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            result += "<script type=\"text/javascript\">" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

            result += "function fnbtnclicked(id)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
            result += "   lnk.value = id;" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function ChangeTitle()" + "\r\n";
            result += "{" + "\r\n";
            result += "   var title = document.getElementById('mfaGreetingDescription');" + "\r\n";
            result += "   if (title)" + "\r\n";
            result += "   {" + "\r\n";
            if (usercontext.TargetUIMode==ProviderPageMode.Registration)
                result += "     title.innerHTML = \"<b>" + Resources.GetString(ResourcesLocaleKind.Titles, "RegistrationPageTitle") + "</b>\";" + "\r\n";
            else if (usercontext.TargetUIMode==ProviderPageMode.Invitation)
                result += "     title.innerHTML = \"<b>" + Resources.GetString(ResourcesLocaleKind.Titles, "InvitationPageTitle") + "</b>\";" + "\r\n";
            result += "   }" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";


            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlEnrollOTP method implmentation
        /// </summary>
        private string GetFormHtmlEnrollOTP(AuthenticationContext usercontext)
        {
            string result = "<script type=\"text/javascript\">" + "\r\n";
            result += "if (window.addEventListener)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.addEventListener('load', ChangeTitle, false);" + "\r\n";
            result += "}" + "\r\n";
            result += "else if (window.attachEvent)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.attachEvent('onload', ChangeTitle);" + "\r\n";
            result += "}" + "\r\n";
            result += "</script>" + "\r\n";

            result += "<form method=\"post\" id=\"enrollotpForm\" autocomplete=\"off\" \">";

            switch (usercontext.WizPageID)
            {
                case 0:
                    if (!String.IsNullOrEmpty(usercontext.UIMessage))
                        result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div>";
                    else
                        result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";

                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWREGOTP") + "</div></br>";
                    result += "<table>";
                    if (!Provider.Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoMicrosoftAuthenticator))
                    {
                        result += " <tr>";
                        result += "  <td>";
                        result += "   <img id=\"ms\" src=\"data:image/png;base64," + Convert.ToBase64String(images.microsoft.ToByteArray(ImageFormat.Png)) + "\"/>";
                        result += "  </td>";
                        result += "  <td> ";
                        result += "    <table>";
                        result += "      <tr>";
                        result += "        <td>&nbsp</td>";
                        result += "        <td> ";
                        result += "          <a href=\"https://www.microsoft.com/store/p/microsoft-authenticator/9nblgggzmcj6\" target=\"blank\">Microsoft Store</a>";
                        result += "        </td>";
                        result += "      </tr>";
                        result += "      <tr>";
                        result += "        <td>&nbsp</td>";
                        result += "        <td>";
                        result += "          <a href=\"https://play.google.com/store/apps/details?id=com.azure.authenticator\" target=\"blank\">Google Play</a>";
                        result += "        </td>";
                        result += "      </tr>";
                        result += "      <tr>";
                        result += "        <td>&nbsp</td>";
                        result += "        <td>";
                        result += "          <a href=\"https://itunes.apple.com/app/microsoft-authenticator/id983156458\" target=\"blank\">iTunes</a>";
                        result += "        </td>";
                        result += "      </tr>";
                        result += "    </table";
                        result += "  </td>";
                        result += " </tr>";
                    }
                    if (!Provider.Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoGoogleAuthenticator))
                    {
                        result += " <tr>";
                        result += "  <td>";
                        result += "    <img id=\"gl\" src=\"data:image/png;base64," + Convert.ToBase64String(images.google.ToByteArray(ImageFormat.Png)) + "\"/>";
                        result += "  </td>";
                        result += "  <td> ";
                        result += "    <table>";
                        result += "      <tr>";
                        result += "        <td>&nbsp</td>";
                        result += "        <td>&nbsp</td>";
                        result += "      </tr>";
                        result += "      <tr>";
                        result += "        <td>&nbsp</td>";
                        result += "        <td >";
                        result += "          <a href=\"https://play.google.com/store/apps/details?id=com.google.android.apps.authenticator2\" target=\"blank\">Google Play</a>";
                        result += "        </td>";
                        result += "      </tr>";
                        result += "      <tr>";
                        result += "        <td>&nbsp</td>";
                        result += "        <td>";
                        result += "          <a href=\"https://itunes.apple.com/app/google-authenticator/id388497605\" target=\"blank\">iTunes</a>";
                        result += "        </td>";
                        result += "      </tr>";
                        result += "    </table";
                        result += "  </td>";
                        result += " </tr>";
                    }
                    if (!Provider.Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoAuthyAuthenticator))
                    {
                        result += " <tr>";
                        result += "  <td>";
                        result += "    <img id=\"at\" src=\"data:image/png;base64," + Convert.ToBase64String(images.authy2.ToByteArray(ImageFormat.Png)) + "\"/>";
                        result += "  </td>";
                        result += "  <td> ";
                        result += "    <table";
                        result += "      <tr>";
                        result += "        <td>&nbsp</td>";
                        result += "        <td>&nbsp</td>";
                        result += "      </tr>";
                        result += "      <tr>";
                        result += "        <td>&nbsp</td>";
                        result += "        <td>";
                        result += "          <a href=\"https://play.google.com/store/apps/details?id=com.authy.authy\" target=\"blank\">Google Play</a>";
                        result += "        </td>";
                        result += "      </tr>";
                        result += "      <tr>";
                        result += "        <td>&nbsp</td>";
                        result += "        <td>";
                        result += "          <a href=\"https://itunes.apple.com/app/authy/id494168017\" target=\"blank\">iTunes</a>";
                        result += "        </td>";
                        result += "      </tr>";
                        result += "    </table";
                        result += "  </td>";
                        result += " </tr>";
                    }
                    result += "</table></br>";

                    if (!Provider.Config.OTPProvider.WizardOptions.HasFlag(OTPWizardOptions.NoGooglSearch))
                    {
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"><a href=\"https://www.google.fr/search?q=Authenticator+apps&oq=Authenticator+apps\" target=\"blank\">Or Search Google</a>" + "</div>";
                    }
                    result += "</br>";

                    result += "<p class=\"error\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWREGOTPWarning") + "</p></br></br>";

                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"nextButton\" type=\"submit\" class=\"submit\" name=\"nextButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMRecordNewKey") + "\" onclick=\"fnbtnclicked(2)\" />";                    
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    break;
                case 1: // Always Reset the Key
                    KeysManager.NewKey(usercontext.UPN);
                    string displaykey = KeysManager.EncodedKey(usercontext.UPN);

                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWRQRCode") + "</div></br>";
                    result += "<input id=\"secretkey\" name=\"secretkey\" type=\"text\" readonly=\"true\" placeholder=\"DisplayKey\" class=\"text fullWidth\" style=\"background-color: #C0C0C0\" value=\"" + StripDisplayKey(displaykey) + "\"/></br></br>";
                    result += "<p style=\"text-align:center\"><img id=\"qr\" src=\"data:image/png;base64," + Provider.GetQRCodeString(usercontext) + "\"/></p></br>";
                    result += "<input id=\"nextButton\" type=\"submit\" class=\"submit\" name=\"nextButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWVERIFYOTP") + "\" onclick=\"fnbtnclicked(3)\" />";                    
                    break;
                case 2: // Code verification
                    IExternalProvider prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Code);
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUILabel(usercontext) + " : </div>";
                    result += "<input id=\"totp\" name=\"totp\" type=\"password\" placeholder=\"Verification Code\" class=\"text fullWidth\" autofocus=\"autofocus\" /></br></br>";
                    result += "<input id=\"checkButton\" type=\"submit\" class=\"submit\" name=\"checkButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMCheck") + "\" onclick=\"fnbtnclicked(4)\" /></br></br>";
                    result += "<br />";
                    break;
                case 3: // Successfull test
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" style=\"color: #6FA400\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div></br>";
                    result += "<p style=\"text-align:center\"><img id=\"msgreen\" src=\"data:image/png;base64," + Convert.ToBase64String(images.cvert.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<input id=\"finishButton\" type=\"submit\" class=\"submit\" name=\"finishButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPOK") + "\" onclick=\"fnbtnclicked(1)\" />";                    
                    break;
                case 4: // Wrong result test
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div></br>";
                    result += "<p style=\"text-align:center\"><img id=\"msred\" src=\"data:image/png;base64," + Convert.ToBase64String(images.crouge.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"priorButton\" type=\"submit\" class=\"submit\" name=\"priorButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPPRIOR") + "\" onclick=\"fnbtnclicked(2)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    break;
            }

            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"1\" />";

            result += "</form>";
            return result;
        }
        #endregion

        #region EnrollEmail
        /// <summary>
        /// GetFormPreRenderHtmlEnrollEmail method implementation
        /// </summary>
        private string GetFormPreRenderHtmlEnrollEmail(AuthenticationContext usercontext)
        {
            string result = string.Empty;

            string reg = @"/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,})+$/";

            result += "<script type=\"text/javascript\">" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

            result += "function ValidateRegistration(frm)" + "\r\n";
            result += "{" + "\r\n";

            if (usercontext.WizPageID == 0)
            {
                result += "   var mailformat = " + reg + " ;" + "\r\n";
                result += "   var email = document.getElementById('email');" + "\r\n";
                result += "   var err = document.getElementById('errorText');" + "\r\n";
                result += "   var canceled = document.getElementById('btnclicked');" + "\r\n";

                result += "   if ((canceled) && (canceled.value==1))" + "\r\n";
                result += "   {" + "\r\n";
                result += "         return true;" + "\r\n";
                result += "   }" + "\r\n";
                result += "   if ((email) && (email.value==''))" + "\r\n";
                result += "   {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectEmail") + "\";" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "   }" + "\r\n";
                result += "   if ((email) && (email.value!==''))" + "\r\n";
                result += "   {" + "\r\n";
                result += "      if (!email.value.match(mailformat))" + "\r\n";
                result += "      {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectEmail") + "\";" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "      }" + "\r\n";
                result += "   }" + "\r\n";
                result += "   err.innerHTML = \"\";" + "\r\n";
            }
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function OnRefreshPost(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   ChangeTitle();" + "\r\n";
            result += "   fnbtnclicked(3);" + "\r\n";
            result += "   document.getElementById('enrollemailForm').submit();" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function fnbtnclicked(id)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
            result += "   lnk.value = id;" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function ChangeTitle()" + "\r\n";
            result += "{" + "\r\n";
            result += "   var title = document.getElementById('mfaGreetingDescription');" + "\r\n";
            result += "   if (title)" + "\r\n";
            result += "   {" + "\r\n";
            if (usercontext.TargetUIMode == ProviderPageMode.Registration)
                result += "     title.innerHTML = \"<b>" + Resources.GetString(ResourcesLocaleKind.Titles, "RegistrationPageTitle") + "</b>\";" + "\r\n";
            else if (usercontext.TargetUIMode == ProviderPageMode.Invitation)
                result += "     title.innerHTML = \"<b>" + Resources.GetString(ResourcesLocaleKind.Titles, "InvitationPageTitle") + "</b>\";" + "\r\n";
            result += "   }" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlEnrollEmail method implmentation
        /// </summary>
        private string GetFormHtmlEnrollEmail(AuthenticationContext usercontext)
        {
            string result = "<script type=\"text/javascript\">" + "\r\n";
            result += "if (window.addEventListener)" + "\r\n";
            result += "{" + "\r\n";
            if (usercontext.WizPageID==1)
                result += "   window.addEventListener('load', OnRefreshPost, false);" + "\r\n";
            else
                result += "   window.addEventListener('load', ChangeTitle, false);" + "\r\n";
            result += "}" + "\r\n";
            result += "else if (window.attachEvent)" + "\r\n";
            result += "{" + "\r\n";
            if (usercontext.WizPageID == 1)
                result += "   window.attachEvent('onload', OnRefreshPost);" + "\r\n";
            else
                result += "   window.attachEvent('onload', ChangeTitle);" + "\r\n";
            result += "}" + "\r\n";
            result += "</script>" + "\r\n";

            result += "<form method=\"post\" id=\"enrollemailForm\" autocomplete=\"off\" onsubmit=\"return ValidateRegistration(this)\" >";

            IExternalProvider prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Email);
            prov.GetAuthenticationContext(usercontext);
            switch (usercontext.WizPageID)
            {
                case 0: // Get User email
                    if (!String.IsNullOrEmpty(usercontext.UIMessage))
                        result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div>";
                    else
                        result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";

                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWREmail") + "</div></br>";
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUICFGLabel(usercontext) + " : </div>";
                    result += "<input id=\"email\" name=\"email\" type=\"text\" placeholder=\"Personal Email Address\" class=\"text fullWidth\" value=\"" + usercontext.MailAddress + "\"/></br></br>";
                    result += "</br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"nextButton\" type=\"submit\" class=\"submit\" name=\"nextButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWVERIFYOTP") + "\" onclick=\"fnbtnclicked(2)\" />";                    
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    result += "</br>";
                    break;
                case 1:
                    result += GetPartHtmlDonut();
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUIMessage(usercontext) + "</div></br>";
                    if (!string.IsNullOrEmpty(prov.GetUIWarningInternetLabel(usercontext)) && (usercontext.IsRemote))
                       result += "<div class=\"error smallText\"><label for=\"\"></label>" + prov.GetUIWarningInternetLabel(usercontext) + "</div>";
                    if (!string.IsNullOrEmpty(prov.GetUIWarningThirdPartyLabel(usercontext)) && (usercontext.IsTwoWay))
                       result += "<div class=\"error smallText\"><label for=\"\"></label>" + prov.GetUIWarningThirdPartyLabel(usercontext) + "</div>";
                    result += "<br />";
                    break;
                case 2: // Code verification
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUILabel(usercontext) + " : </div>";
                    result += "<input id=\"totp\" name=\"totp\" type=\"password\" placeholder=\"Verification Code\" class=\"text fullWidth\" autofocus=\"autofocus\" /></br></br>";
                    result += "<input id=\"checkButton\" type=\"submit\" class=\"submit\" name=\"checkButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMCheck") + "\" onclick=\"fnbtnclicked(4)\" /></br></br>";
                    result += "<br />";
                    break;
                case 3: // Successfull test
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" style=\"color: #6FA400\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div></br>";
                    result += "<p style=\"text-align:center\"><img id=\"msgreen\" src=\"data:image/png;base64," + Convert.ToBase64String(images.cvert.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<input id=\"finishButton\" type=\"submit\" class=\"submit\" name=\"finishButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPOK") + "\" onclick=\"fnbtnclicked(1)\" />";
                    break;
                case 4: // Wrong result test
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div></br>";
                    result += "<p style=\"text-align:center\"><img id=\"msred\" src=\"data:image/png;base64," + Convert.ToBase64String(images.crouge.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"priorButton\" type=\"submit\" class=\"submit\" name=\"priorButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPPRIOR") + "\" onclick=\"fnbtnclicked(0)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    break;
            }

            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"1\" />";

            result += "</form>";
            return result;
        }
        #endregion

        #region EnrollPhone
        /// <summary>
        /// GetFormPreRenderHtmlEnrollPhone method implementation
        /// </summary>
        private string GetFormPreRenderHtmlEnrollPhone(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            string pho = @"/^\+(?:[0-9] ?){6,14}[0-9]$/";
            string pho10 = @"/^\d{10}$/";
            string phous = @"/^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$/";

            result += "<script type=\"text/javascript\">" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

            result += "function ValidateRegistration(frm)" + "\r\n";
            result += "{" + "\r\n";
            if (usercontext.WizPageID == 0)
            {
                result += "   var phoneformat = " + pho + " ;" + "\r\n";
                result += "   var phoneformat10 = " + pho10 + " ;" + "\r\n";
                result += "   var phoneformatus = " + phous + " ;" + "\r\n";
                result += "   var phone = document.getElementById('phone');" + "\r\n";
                result += "   var err = document.getElementById('errorText');" + "\r\n";
                result += "   var canceled = document.getElementById('btnclicked');" + "\r\n";

                result += "   if ((canceled) && (canceled.value==1))" + "\r\n";
                result += "   {" + "\r\n";
                result += "         return true;" + "\r\n";
                result += "   }" + "\r\n";
                result += "   if ((phone) && (phone.value==''))" + "\r\n";
                result += "   {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectPhoneNumber") + "\";" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "   }" + "\r\n";
                result += "   if ((phone) && (phone.value!==''))" + "\r\n";
                result += "   {" + "\r\n";
                result += "      if (!phone.value.match(phoneformat) && !phone.value.match(phoneformat10) && !phone.value.match(phoneformatus) )" + "\r\n";
                result += "      {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectPhoneNumber") + "\";" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "      }" + "\r\n";
                result += "   }" + "\r\n";
                result += "   err.innerHTML = \"\";" + "\r\n";
            }
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function OnRefreshPost(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   ChangeTitle();" + "\r\n";
            result += "   fnbtnclicked(3);" + "\r\n";
            result += "   document.getElementById('enrollphoneForm').submit();" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function fnbtnclicked(id)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
            result += "   lnk.value = id;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function ChangeTitle()" + "\r\n";
            result += "{" + "\r\n";
            result += "   var title = document.getElementById('mfaGreetingDescription');" + "\r\n";
            result += "   if (title)" + "\r\n";
            result += "   {" + "\r\n";
            if (usercontext.TargetUIMode == ProviderPageMode.Registration)
                result += "     title.innerHTML = \"<b>" + Resources.GetString(ResourcesLocaleKind.Titles, "RegistrationPageTitle") + "</b>\";" + "\r\n";
            else if (usercontext.TargetUIMode == ProviderPageMode.Invitation)
                result += "     title.innerHTML = \"<b>" + Resources.GetString(ResourcesLocaleKind.Titles, "InvitationPageTitle") + "</b>\";" + "\r\n";
            result += "   }" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";


            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlEnrollPhone method implmentation
        /// </summary>
        private string GetFormHtmlEnrollPhone(AuthenticationContext usercontext)
        {
            string result = "<script type=\"text/javascript\">" + "\r\n";
            result += "if (window.addEventListener)" + "\r\n";
            result += "{" + "\r\n";
            if (usercontext.WizPageID == 1)
                result += "   window.addEventListener('load', OnRefreshPost, false);" + "\r\n";
            else
                result += "   window.addEventListener('load', ChangeTitle, false);" + "\r\n";
            result += "}" + "\r\n";
            result += "else if (window.attachEvent)" + "\r\n";
            result += "{" + "\r\n";
            if (usercontext.WizPageID == 1)
                result += "   window.attachEvent('onload', OnRefreshPost);" + "\r\n";
            else
                result += "   window.attachEvent('onload', ChangeTitle);" + "\r\n";
            result += "}" + "\r\n";
            result += "</script>" + "\r\n";

            result += "<form method=\"post\" id=\"enrollphoneForm\" autocomplete=\"off\" onsubmit=\"return ValidateRegistration(this)\" >";

            IExternalProvider prov = RuntimeAuthProvider.GetProvider(PreferredMethod.External);
            prov.GetAuthenticationContext(usercontext);
            switch (usercontext.WizPageID)
            {
                case 0: // Get User Phone number
                    if (!String.IsNullOrEmpty(usercontext.UIMessage))
                        result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div>";
                    else
                        result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";

                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWRPhone") + "</div></br>";
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUICFGLabel(usercontext) + " : </div>";
                    result += "<input id=\"phone\" name=\"phone\" type=\"text\" placeholder=\"Personal Phone number\" class=\"text fullWidth\" value=\"" + usercontext.PhoneNumber + "\"/></br></br>";
                    result += "</br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"nextButton\" type=\"submit\" class=\"submit\" name=\"nextButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWVERIFYOTP") + "\" onclick=\"fnbtnclicked(2)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    result += "</br>";
                    break;
                case 1:
                    result += GetPartHtmlDonut();
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUIMessage(usercontext) + "</div></br>";
                    if (!string.IsNullOrEmpty(prov.GetUIWarningInternetLabel(usercontext)) && (usercontext.IsRemote))
                        result += "<div class=\"error smallText\"><label for=\"\"></label>" + prov.GetUIWarningInternetLabel(usercontext) + "</div>";
                    if (!string.IsNullOrEmpty(prov.GetUIWarningThirdPartyLabel(usercontext)) && (usercontext.IsTwoWay))
                        result += "<div class=\"error smallText\"><label for=\"\"></label>" + prov.GetUIWarningThirdPartyLabel(usercontext) + "</div>";
                    result += "<br />";
                    break;
                case 2: // Code verification If One-Way
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + prov.GetUILabel(usercontext) + " : </div>";
                    result += "<input id=\"totp\" name=\"totp\" type=\"password\" placeholder=\"Verification Code\" class=\"text fullWidth\" autofocus=\"autofocus\" /></br></br>";
                    result += "<input id=\"checkButton\" type=\"submit\" class=\"submit\" name=\"checkButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMCheck") + "\" onclick=\"fnbtnclicked(4)\" /></br></br>";
                    result += "<br />";
                    break;
                case 3: // Successfull test
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" style=\"color: #6FA400\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div></br>";
                    result += "<p style=\"text-align:center\"><img id=\"msgreen\" src=\"data:image/png;base64," + Convert.ToBase64String(images.cvert.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<input id=\"finishButton\" type=\"submit\" class=\"submit\" name=\"finishButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPOK") + "\" onclick=\"fnbtnclicked(1)\" />";
                    break;
                case 4: // Wrong result test
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div></br>";
                    result += "<p style=\"text-align:center\"><img id=\"msred\" src=\"data:image/png;base64," + Convert.ToBase64String(images.crouge.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"priorButton\" type=\"submit\" class=\"submit\" name=\"priorButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPPRIOR") + "\" onclick=\"fnbtnclicked(0)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    break;
            }

            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"1\" />";

            result += "</form>";
            return result;
        }
        #endregion

        #region EnrollBiometrics
        /// <summary>
        /// GetFormPreRenderHtmlEnrollBio method implementation
        /// </summary>
        private string GetFormPreRenderHtmlEnrollBio(AuthenticationContext Context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetFormHtmlEnrollBio method implementation
        /// </summary>
        private string GetFormHtmlEnrollBio(AuthenticationContext Context)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region EnrollPINCode
        /// <summary>
        /// GetFormPreRenderHtmlEnrollPinCode method implementation
        /// </summary>
        private string GetFormPreRenderHtmlEnrollPinCode(AuthenticationContext usercontext)
        {
            string result = string.Empty;

            string reg = @"/([0-9]{4,4})/";

            result += "<script type=\"text/javascript\">" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

            result += "function ValidateRegistration(frm)" + "\r\n";
            result += "{" + "\r\n";

            if (usercontext.WizPageID == 0)
            {
                result += "   var mailformat = " + reg + " ;" + "\r\n";
                result += "   var email = document.getElementById('pincode');" + "\r\n";
                result += "   var err = document.getElementById('errorText');" + "\r\n";

                result += "   if ((pincode) && (pincode.value==''))" + "\r\n";
                result += "   {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectPinCode") + "\";" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "   }" + "\r\n";
                result += "   if ((pincode) && (pincode.value!==''))" + "\r\n";
                result += "   {" + "\r\n";
                result += "      if (!pincode.value.match(mailformat))" + "\r\n";
                result += "      {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectPinCode") + "\";" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "      }" + "\r\n";
                result += "   }" + "\r\n";
                result += "   err.innerHTML = \"\";" + "\r\n";
            }
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function fnbtnclicked(id)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
            result += "   lnk.value = id;" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function ChangeTitle()" + "\r\n";
            result += "{" + "\r\n";
            result += "   var title = document.getElementById('mfaGreetingDescription');" + "\r\n";
            result += "   if (title)" + "\r\n";
            result += "   {" + "\r\n";
            if (usercontext.TargetUIMode == ProviderPageMode.Registration)
                result += "     title.innerHTML = \"<b>" + Resources.GetString(ResourcesLocaleKind.Titles, "RegistrationPageTitle") + "</b>\";" + "\r\n";
            else if (usercontext.TargetUIMode == ProviderPageMode.Invitation)
                result += "     title.innerHTML = \"<b>" + Resources.GetString(ResourcesLocaleKind.Titles, "InvitationPageTitle") + "</b>\";" + "\r\n";
            result += "   }" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlEnrollEmail method implmentation
        /// </summary>
        private string GetFormHtmlEnrollPinCode(AuthenticationContext usercontext)
        {
            string result = "<script type=\"text/javascript\">" + "\r\n";
            result += "if (window.addEventListener)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.addEventListener('load', ChangeTitle, false);" + "\r\n";
            result += "}" + "\r\n";
            result += "else if (window.attachEvent)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.attachEvent('onload', ChangeTitle);" + "\r\n";
            result += "}" + "\r\n";
            result += "</script>" + "\r\n";

            result += "<form method=\"post\" id=\"enrollPinForm\" autocomplete=\"off\" onsubmit=\"return ValidateRegistration(this)\" >";

         //   IExternalProvider prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Email);
         //   prov.GetAuthenticationContext(usercontext);
            switch (usercontext.WizPageID)
            {
                case 0: // Get User Pin
                    if (!String.IsNullOrEmpty(usercontext.UIMessage))
                        result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div>";
                    else
                        result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";

                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWRPinCode") + "</div></br>";
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + BaseExternalProvider.GetPINLabel(usercontext) + " : </div>";
                    if (usercontext.PinCode <= 0)
                        result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" placeholder=\"PIN Number\" class=\"text fullWidth\" value=\"" + Provider.Config.DefaultPin + "\"/></br></br>";
                    else
                        result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" placeholder=\"PIN Number\" class=\"text fullWidth\" value=\"" + usercontext.PinCode + "\"/></br></br>";
                    result += "</br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"nextButton\" type=\"submit\" class=\"submit\" name=\"nextButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWVERIFYOTP") + "\" onclick=\"fnbtnclicked(2)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    result += "</br>";
                    break;
                case 2: // Code verification
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + BaseExternalProvider.GetPINLabel(usercontext) + " : </div>";
                    result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" placeholder=\"PIN Code\" class=\"text fullWidth\" autofocus=\"autofocus\" /></br></br>";
                    result += "<input id=\"checkButton\" type=\"submit\" class=\"submit\" name=\"checkButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMCheck") + "\" onclick=\"fnbtnclicked(3)\" /></br></br>";
                    result += "<br />";
                    break;
                case 3: // Successfull test
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" style=\"color: #6FA400\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div></br>";
                    result += "<p style=\"text-align:center\"><img id=\"msgreen\" src=\"data:image/png;base64," + Convert.ToBase64String(images.cvert.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<input id=\"finishButton\" type=\"submit\" class=\"submit\" name=\"finishButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPOK") + "\" onclick=\"fnbtnclicked(1)\" />";
                    break;
                case 4: // Wrong result test
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div></br>";
                    result += "<p style=\"text-align:center\"><img id=\"msred\" src=\"data:image/png;base64," + Convert.ToBase64String(images.crouge.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"priorButton\" type=\"submit\" class=\"submit\" name=\"priorButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPPRIOR") + "\" onclick=\"fnbtnclicked(2)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    break;
            }

            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"1\" />";

            result += "</form>";
            return result;
        }
        #endregion

        #region Html Parts
        /// <summary>
        /// GetPartHtmlPagingMethod method ipmplmentation
        /// </summary>
        private string GetPartHtmlPagingMethod(AuthenticationContext usercontext)
        {
            bool showpin = false;
            string result = string.Empty;
            result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGAccessPageMethods") + "</div>";
            result += "<select id=\"selectpage\" name=\"selectpage\" role=\"combobox\"  required=\"required\" contenteditable=\"false\" class=\"text fullWidth\" onchange=\"this.form.submit()\">";

            IExternalProvider prov = null;
            IExternalProvider lastprov = null;
            prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Code);
            if ((prov!=null) && (prov.IsAvailable(usercontext)))
            {
                result += "<option value=\"0\" " + ((usercontext.PageID == 0) ? "selected=\"true\"> " : "> ") + prov.GetUIListOptionLabel(usercontext) + "</option>";
                if (prov.PinRequired)
                {
                    showpin = true;
                    lastprov = prov;
                }
            }
            prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Email);
            if ((prov!=null) && (prov.IsAvailable(usercontext)))
            {
                result += "<option value=\"1\" " + ((usercontext.PageID == 1) ? "selected=\"true\"> " : "> ") + prov.GetUIListOptionLabel(usercontext) + "</option>";
                if (prov.PinRequired)
                {
                    showpin = true;
                    lastprov = prov;
                }
            }
            prov = RuntimeAuthProvider.GetProvider(PreferredMethod.External);
            if ((prov!=null) && (prov.IsAvailable(usercontext)))
            { 
                result += "<option value=\"2\" " + ((usercontext.PageID == 2) ? "selected=\"true\"> " : "> ") + prov.GetUIListOptionLabel(usercontext) + "</option>";
                if (prov.PinRequired)
                {
                    showpin = true;
                    lastprov = prov;
                }
            }
            prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Azure);
            if ((prov!=null) && (prov.IsAvailable(usercontext)))
            {
                result += "<option value=\"3\" " + ((usercontext.PageID == 3) ? "selected=\"true\"> " : "> ") + prov.GetUIListOptionLabel(usercontext) + "</option>";
                if (prov.PinRequired)
                {
                    showpin = true;
                    lastprov = prov;
                }
            }
            prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics);
            if ((prov!=null) && (prov.IsAvailable(usercontext)))
            {
                result += "<option value=\"4\" " + ((usercontext.PageID == 4) ? "selected=\"true\"> " : "> ") + prov.GetUIListOptionLabel(usercontext) + "</option>";
                if (prov.PinRequired)
                {
                    showpin = true;
                    lastprov = prov;
                }
            }
            if ((showpin) && (lastprov!=null))
            {
                result += "<option value=\"5\" " + ((usercontext.PageID == 5) ? "selected=\"true\"> " : "> ") + BaseExternalProvider.GetPINLabel(usercontext) + "</option>";
            }
            result += "</select></br></br>";
            return result;
        }

        /// <summary>
        /// GetPartHtmlSelectMethod method ipmplmentation
        /// </summary>
        private string GetPartHtmlSelectMethod(AuthenticationContext usercontext)
        {
            Registration reg = RuntimeRepository.GetUserRegistration(Provider.Config, usercontext.UPN);
            PreferredMethod method = PreferredMethod.None;
            if (reg == null)
                method = usercontext.PreferredMethod;
            else
                method = reg.PreferredMethod;
            string result = string.Empty;
            result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGAccessMethod") + "</div>";
            result += "<select id=\"selectopt\" name=\"selectopt\" role=\"combobox\"  required=\"required\" contenteditable=\"false\" class=\"text fullWidth\" >";

            result += "<option value=\"0\" " + ((method == PreferredMethod.Choose) ? "selected=\"true\"> " : "> ") + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionChooseBest") + "</option>";

            if (RuntimeAuthProvider.IsProviderAvailableForUser(usercontext, PreferredMethod.Code))
                result += "<option value=\"1\" " + ((method == PreferredMethod.Code) ? "selected=\"true\"> " : "> ") + RuntimeAuthProvider.GetProvider(PreferredMethod.Code).GetUIListChoiceLabel(usercontext) + "</option>";

            if (RuntimeAuthProvider.IsProviderAvailableForUser(usercontext, PreferredMethod.Email))
                result += "<option value=\"2\" " + ((method == PreferredMethod.Email) ? "selected=\"true\"> " : "> ") + RuntimeAuthProvider.GetProvider(PreferredMethod.Email).GetUIListChoiceLabel(usercontext) + "</option>";

            if (RuntimeAuthProvider.IsProviderAvailableForUser(usercontext, PreferredMethod.External))
                result += "<option value=\"3\" " + ((method == PreferredMethod.External) ? "selected=\"true\"> " : "> ") + RuntimeAuthProvider.GetProvider(PreferredMethod.External).GetUIListChoiceLabel(usercontext) + "</option>";

            if (RuntimeAuthProvider.IsProviderAvailableForUser(usercontext, PreferredMethod.Azure))
                result += "<option value=\"4\" " + ((method == PreferredMethod.Azure) ? "selected=\"true\">" : ">") + RuntimeAuthProvider.GetProvider(PreferredMethod.Azure).GetUIListChoiceLabel(usercontext) + "</option>";

            if (RuntimeAuthProvider.IsProviderAvailableForUser(usercontext, PreferredMethod.Biometrics))
                result += "<option value=\"5\" " + ((method == PreferredMethod.Biometrics) ? "selected=\"true\">" : ">") + RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics).GetUIListChoiceLabel(usercontext) + "</option>";

            result += "</select></br>";
            return result;
        }

        /// <summary>
        /// GetPartHtmlDonut method implementation
        /// </summary>
        private string GetPartHtmlDonut(bool visible = true)
        {
            string result = string.Empty;
            if (visible)
                result = "<div id=\"cookiePullWaitingWheel\" \">";
            else
                result = "<div id=\"cookiePullWaitingWheel\" style=\"visibility:hidden;\">";
            result += "<style>";
            result += "#floatingCirclesG {";
            result += "position: relative;";
            result += "width: 125px;";
            result += "height: 125px;";
            result += "margin: auto;";
            result += "transform: scale(0.4);";
            result += "-o-transform: scale(0.4);";
            result += "-ms-transform: scale(0.4);";
            result += "-webkit-transform: scale(0.4);";
            result += "-moz-transform: scale(0.4);";
            result += "}";

            result += ".f_circleG {";
            result += "position: absolute;";
            result += "height: 22px;";
            result += "width: 22px;";
            result += "border-radius: 12px;";
            result += "-o-border-radius: 12px;";
            result += "-ms-border-radius: 12px;";
            result += "-webkit-border-radius: 12px;";
            result += "-moz-border-radius: 12px;";
            result += "animation-name: f_fadeG;";
            result += "-o-animation-name: f_fadeG;";
            result += "-ms-animation-name: f_fadeG;";
            result += "-webkit-animation-name: f_fadeG;";
            result += "-moz-animation-name: f_fadeG;";
            result += "animation-duration: 1.2s;";
            result += "-o-animation-duration: 1.2s;";
            result += "-ms-animation-duration: 1.2s;";
            result += "-webkit-animation-duration: 1.2s;";
            result += "-moz-animation-duration: 1.2s;";
            result += "animation-iteration-count: infinite;";
            result += "-o-animation-iteration-count: infinite;";
            result += "-ms-animation-iteration-count: infinite;";
            result += "-webkit-animation-iteration-count: infinite;";
            result += "-moz-animation-iteration-count: infinite;";
            result += "animation-direction: normal;";
            result += "-o-animation-direction: normal;";
            result += "-ms-animation-direction: normal;";
            result += "-webkit-animation-direction: normal;";
            result += "-moz-animation-direction: normal;";
            result += "}";

            result += "#frotateG_01 {";
            result += "left: 0;";
            result += "top: 51px;";
            result += "animation-delay: 0.45s;";
            result += "-o-animation-delay: 0.45s;";
            result += "-ms-animation-delay: 0.45s;";
            result += "-webkit-animation-delay: 0.45s;";
            result += "-moz-animation-delay: 0.45s;";
            result += "}";

            result += "#frotateG_02 {";
            result += "left: 15px;";
            result += "top: 15px;";
            result += "animation-delay: 0.6s;";
            result += "-o-animation-delay: 0.6s;";
            result += "-ms-animation-delay: 0.6s;";
            result += "-webkit-animation-delay: 0.6s;";
            result += "-moz-animation-delay: 0.6s;";
            result += "}";

            result += "#frotateG_03 {";
            result += "left: 51px;";
            result += "top: 0;";
            result += "animation-delay: 0.75s;";
            result += "-o-animation-delay: 0.75s;";
            result += "-ms-animation-delay: 0.75s;";
            result += "-webkit-animation-delay: 0.75s;";
            result += "-moz-animation-delay: 0.75s;";
            result += "}";

            result += "#frotateG_04 {";
            result += "right: 15px;";
            result += "top: 15px;";
            result += "animation-delay: 0.9s;";
            result += "-o-animation-delay: 0.9s;";
            result += "-ms-animation-delay: 0.9s;";
            result += "-webkit-animation-delay: 0.9s;";
            result += "-moz-animation-delay: 0.9s;";
            result += "}";

            result += "#frotateG_05 {";
            result += "right: 0;";
            result += "top: 51px;";
            result += "animation-delay: 1.05s;";
            result += "-o-animation-delay: 1.05s;";
            result += "-ms-animation-delay: 1.05s;";
            result += "-webkit-animation-delay: 1.05s;";
            result += "-moz-animation-delay: 1.05s;";
            result += "}";

            result += "#frotateG_06 {";
            result += "right: 15px;";
            result += "bottom: 15px;";
            result += "animation-delay: 1.2s;";
            result += "-o-animation-delay: 1.2s;";
            result += "-ms-animation-delay: 1.2s;";
            result += "-webkit-animation-delay: 1.2s;";
            result += "-moz-animation-delay: 1.2s;";
            result += "}";

            result += "#frotateG_07 {";
            result += "left: 51px;";
            result += "bottom: 0;";
            result += "animation-delay: 1.35s;";
            result += "-o-animation-delay: 1.35s;";
            result += "-ms-animation-delay: 1.35s;";
            result += "-webkit-animation-delay: 1.35s;";
            result += "-moz-animation-delay: 1.35s;";
            result += "}";

            result += "#frotateG_08 {";
            result += "left: 15px;";
            result += "bottom: 15px;";
            result += "animation-delay: 1.5s;";
            result += "-o-animation-delay: 1.5s;";
            result += "-ms-animation-delay: 1.5s;";
            result += "-webkit-animation-delay: 1.5s;";
            result += "-moz-animation-delay: 1.5s;";
            result += "}";

            result += "@keyframes f_fadeG {";
            result += "0% {";
            result += "background-color: rgb(47,146,212);";
            result += "}";

            result += "100% {";
            result += "background-color: rgb(255,255,255);";
            result += "}";
            result += "}";

            result += "@-o-keyframes f_fadeG {";
            result += "0% {";
            result += "background-color: rgb(47,146,212);";
            result += "}";

            result += "100% {";
            result += "background-color: rgb(255,255,255);";
            result += "}";
            result += "}";

            result += "@-ms-keyframes f_fadeG {";
            result += "0% {";
            result += "background-color: rgb(47,146,212);";
            result += "}";

            result += "100% {";
            result += "background-color: rgb(255,255,255);";
            result += "}";
            result += "}";

            result += "@-webkit-keyframes f_fadeG {";
            result += "0% {";
            result += "background-color: rgb(47,146,212);";
            result += "}";

            result += "100% {";
            result += "background-color: rgb(255,255,255);";
            result += "}";
            result += "}";

            result += "@-moz-keyframes f_fadeG {";
            result += "0% {";
            result += "background-color: rgb(47,146,212);";
            result += "}";

            result += "100% {";
            result += "background-color: rgb(255,255,255);";
            result += "}";
            result += "}";
            result += "</style>";

            result += "<div id=\"floatingCirclesG\"\">";
            result += "<div class=\"f_circleG\" id=\"frotateG_01\"></div>";
            result += "<div class=\"f_circleG\" id=\"frotateG_02\"></div>";
            result += "<div class=\"f_circleG\" id=\"frotateG_03\"></div>";
            result += "<div class=\"f_circleG\" id=\"frotateG_04\"></div>";
            result += "<div class=\"f_circleG\" id=\"frotateG_05\"></div>";
            result += "<div class=\"f_circleG\" id=\"frotateG_06\"></div>";
            result += "<div class=\"f_circleG\" id=\"frotateG_07\"></div>";
            result += "<div class=\"f_circleG\" id=\"frotateG_08\"></div>";
            result += "</div>";
            result += "</div>";

            return result;
        }

        #endregion

        /// <summary>
        /// StripDisplayKey method implmentation
        /// </summary>
        private string StripDisplayKey(string dkey)
        {
            if ((dkey != null) && (dkey.Length>=5))
                return dkey.Substring(0, 5) + " ... (truncated for security reasons) ... ";
            else
                return " ... (invalid key) ... ";
        }
    }
}

