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
                case ProviderPageMode.CodeRequest:
                    result = GetFormHtmlCodeRequest(Context);
                    break;
                case ProviderPageMode.InvitationRequest:
                    result = GetFormHtmlInvitationRequest(Context);
                    break;
                case ProviderPageMode.SendKeyRequest:
                    result = GetFormHtmlSendKeyRequest(Context);
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
                case ProviderPageMode.CodeRequest:
                    result = GetFormPreRenderHtmlCodeRequest(Context);
                    break;
                case ProviderPageMode.InvitationRequest:
                    result = GetFormPreRenderHtmlInvitationRequest(Context);
                    break;
                case ProviderPageMode.SendKeyRequest:
                    result = GetFormPreRenderHtmlSendKeyRequest(Context);
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
            result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmUIMLabelOTP") + "</div>";
            result += "<input id=\"pin\" name=\"pin\" type=\"password\" placeholder=\"One Time Password\" class=\"text fullWidth\" autofocus=\"autofocus\" /><br/>";
            switch (usercontext.PreferredMethod)
            {
                case RegistrationPreferredMethod.Code:
                    if (Provider.Config.AppsEnabled)
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMUseApp") + "</div><br/>";
                    break;
                case RegistrationPreferredMethod.Email:
                    if (Provider.Config.MailEnabled)
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtlmUIMUseEmail") + "</div><br/>";
                    break;

                case RegistrationPreferredMethod.Phone:
                    if (Provider.Config.SMSEnabled)
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMUsePhone") + "</div><br/>";
                    break;
            }
            if ((Provider.Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowManageOptions)) || (Provider.Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowChangePassword)))
            {
                if (usercontext.ShowOptions)
                    result += "<input id=\"options\" type=\"checkbox\" name=\"Options\" checked=\"true\" /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMAccessOptions") + "<br/><br/><br/>";
                else
                    result += "<input id=\"options\" type=\"checkbox\" name=\"Options\" /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMAccessOptions") + "<br/><br/><br/>";
            }
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
            result += "<input id=\"continueButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMConnexion") + "\" /><br/><br/>";
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
            string reg = @"/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/";
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

            if (Provider.Config.MailEnabled == true)
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
            if (Provider.Config.SMSEnabled == true)
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

            result += "function SelectionRegistrationChanged()" + "\r\n";
            result += "{" + "\r\n";
            result += "   var data = document.getElementById('selectopt');" + "\r\n";
            result += "   var sel = document.getElementById('selector');" + "\r\n";
            result += "   sel.value = data.selectedIndex-1;" + "\r\n";
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

            result += "function fnbtnclicked(id)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
            result += "   lnk.value = id;" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function ShowQR(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('lnk');" + "\r\n";
            result += "   if (lnk)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      lnk.value = 1;" + "\r\n";
            result += "   }" + "\r\n";
            result += "   frm.submit();" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";

            result += "function ResetKey(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('lnk');" + "\r\n";
            result += "   if (lnk)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      lnk.value = 2;" + "\r\n";
            result += "   }" + "\r\n";
            result += "   frm.submit();" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";

            result += "function SendQR(frm)" + "\r\n";
            result += "{" + "\r\n";

            result += "   var lnk = document.getElementById('lnk');" + "\r\n";
            if (Provider.Config.MailEnabled == true)
            {
                result += "   var mailformat = " + reg + " ;" + "\r\n";
                result += "   var err = document.getElementById('errorText');" + "\r\n";
                result += "   var email = document.getElementById('email');" + "\r\n";

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
            result += "   if (lnk)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      lnk.value = 3;" + "\r\n";
            result += "   }" + "\r\n";
            result += "   frm.submit();" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";

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
            result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";
            if (Provider.Config.AppsEnabled)
            {
                string displaykey = string.Empty;
                if (usercontext.KeyStatus == SecretKeyStatus.Success)
                {
                    displaykey = KeysManager.EncodedKey(usercontext.UPN);
                }
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGLabelAppKey") + "</div>";
                result += "<input id=\"secretkey\" name=\"secretkey\" type=\"text\" readonly=\"true\" placeholder=\"DisplayKey\" class=\"text fullWidth\" style=\"background-color: #C0C0C0\" value=\"" + StripDisplayKey(displaykey) + "\"/>"; //<br/>";
                result += "<a class=\"actionLink\" href=\"#\" id=\"resetkey\" name=\"resetkey\" onclick=\"return ResetKey(registrationForm)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGResetQR") + "</a>";
                if (usercontext.KeyChanged)
                {
                    result += "<div class=\"fieldMargin error smallText\"><label id=\"secretkeychanged\" for=\"\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlSecretKeyChanged") + "</label></div>";
                }
                result += "<a class=\"actionLink\" href=\"#\" id=\"qrcode\" name=\"qrcode\" onclick=\"return ShowQR(registrationForm)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGShowQR") + "</a>";
                if (Provider.Config.MailEnabled == true)
                    result += "<a class=\"actionLink\" href=\"#\" id=\"sendcode\" name=\"sendcode\" onclick=\"return SendQR(registrationForm)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGSendQR") + "</a>";
                result += "<br/>";
                result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
            }
            if (Provider.Config.MailEnabled)
            {
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGLabelMail") + "</div>";
                result += "<input id=\"email\" name=\"email\" type=\"text\" placeholder=\"Personal Email Address\" class=\"text fullWidth\" value=\"" + usercontext.MailAddress + "\"/><br/><br/>";
            }
            if (Provider.Config.SMSEnabled)
            {
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGLabelPhoneNumber") + "</div>";
                result += "<input id=\"phone\" name=\"phone\" type=\"text\" placeholder=\"Phone Number\" class=\"text fullWidth\" value=\"" + usercontext.PhoneNumber + "\"/><br/><br/>";
            }
            result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGAccessMethod") + "</div>";
            result += "<select id=\"selectopt\" name=\"selectopt\" role=\"combobox\"  required=\"required\" contenteditable=\"false\" class=\"text fullWidth\" >";
            if (usercontext.PreferredMethod == RegistrationPreferredMethod.Choose)
            {
                result += "<option value=\"0\" selected=\"true\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionChooseBest") + "</option>";

                if (Provider.Config.AppsEnabled)
                    result += "<option value=\"1\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionTOTP") + "</option>";

                if (Provider.Config.MailEnabled)
                    result += "<option value=\"2\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionEmail") + "</option>";

                if (Provider.Config.SMSEnabled)
                    result += "<option value=\"3\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionPhone") + "</option>";
            }
            else if (usercontext.PreferredMethod == RegistrationPreferredMethod.Code)
            {
                result += "<option value=\"0\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionChooseBest") + "</option>";

                result += "<option value=\"1\" selected=\"true\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionTOTP") + "</option>";

                if (Provider.Config.MailEnabled)
                    result += "<option value=\"2\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionEmail") + "</option>";

                if (Provider.Config.SMSEnabled)
                    result += "<option value=\"3\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionPhone") + "</option>";
            }
            else if (usercontext.PreferredMethod == RegistrationPreferredMethod.Email)
            {
                result += "<option value=\"0\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionChooseBest") + "</option>";

                if (Provider.Config.AppsEnabled)
                    result += "<option value=\"1\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionTOTP") + "</option>";

                result += "<option value=\"2\" selected=\"true\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionEmail") + "</option>";

                if (Provider.Config.SMSEnabled)
                    result += "<option value=\"3\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionPhone") + "</option>";
            }
            else if (usercontext.PreferredMethod == RegistrationPreferredMethod.Phone)
            {
                result += "<option value=\"0\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionChooseBest") + "</option>";

                if (Provider.Config.AppsEnabled)
                    result += "<option value=\"1\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionTOTP") + "</option>";

                if (Provider.Config.MailEnabled)
                    result += "<option value=\"2\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionEmail") + "</option>";

                result += "<option value=\"3\" selected=\"true\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionPhone") + "</option>";
            }
            result += "</select><br/>";
            if (usercontext.Enabled==true)
                result += "<input id=\"disablemfa\" type=\"checkbox\" name=\"disablemfa\" /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMDisableMFA") + "<br/>";
            else
                result += "<input id=\"disablemfa\" type=\"checkbox\" name=\"disablemfa\" checked=\"true\" /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMDisableMFA") + "<br/>";
            result += "<br/><br/>";
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"0\" />";
            result += "<table><tr>";
            result += "<td>";
            result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDSave") + "\" onclick=\"fnbtnclicked(1)\" />";
            result += "</td>";
            result += "<td style=\"width: 15px\" />";
            result += "<td>";
            result += "<input id=\"cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(2)\" />";
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
            string reg = @"/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/";
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

            if (Provider.Config.MailEnabled == true)
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
            if (Provider.Config.SMSEnabled == true)
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

            result += "function SelectionInvitationChanged()" + "\r\n";
            result += "{" + "\r\n";
            result += "   var data = document.getElementById('selectopt');" + "\r\n";
            result += "   var sel = document.getElementById('selector');" + "\r\n";
            result += "   sel.value = data.selectedIndex-1;" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";

            result += "function ChangeInvitationTitle()" + "\r\n";
            result += "{" + "\r\n";
            result += "   var email = document.getElementById('email');" + "\r\n";
            result += "   var title = document.getElementById('mfaGreetingDescription');" + "\r\n";
            result += "   if (title)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      title.innerHTML = \"<b>" + Resources.GetString(ResourcesLocaleKind.Titles, "InvitationPageTitle") + "</b>\";" + "\r\n";
            result += "   }" + "\r\n";
            result += "   if (email)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      email.focus();" + "\r\n";
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

            result += "function ShowQR(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('lnk');" + "\r\n";
            result += "   if (lnk)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      lnk.value = 1;" + "\r\n";
            result += "   }" + "\r\n";
            result += "   frm.submit();" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";

            result += "function SendQR(frm)" + "\r\n";
            result += "{" + "\r\n";

            result += "   var lnk = document.getElementById('lnk');" + "\r\n";
            if (Provider.Config.MailEnabled == true)
            {
                result += "   var mailformat = " + reg + " ;" + "\r\n";
                result += "   var err = document.getElementById('errorText');" + "\r\n";
                result += "   var email = document.getElementById('email');" + "\r\n";

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
            result += "   if (lnk)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      lnk.value = 3;" + "\r\n";
            result += "   }" + "\r\n";
            result += "   frm.submit();" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";

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
            result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";
            
            if (Provider.Config.AppsEnabled)
            {
                string displaykey = string.Empty;
                if (usercontext.KeyStatus == SecretKeyStatus.Success)
                {
                    displaykey = KeysManager.EncodedKey(usercontext.UPN);
                }

                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGLabelAppKey") + "</div>";
                result += "<input id=\"secretkey\" name=\"secretkey\" type=\"text\" readonly=\"true\" placeholder=\"Secret Key\" class=\"text fullWidth\" style=\"background-color: #C0C0C0\" value=\"" + StripDisplayKey(displaykey) + "\"/><br/>";
                result += "<a class=\"actionLink\" href=\"#\" id=\"qrcode\" name=\"qrcode\" onclick=\"return ShowQR(invitationForm)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGShowQR") + "</a>";
                if (Provider.Config.MailEnabled)
                    result += "<a class=\"actionLink\" href=\"#\" id=\"sendcode\" name=\"sendcode\" onclick=\"return SendQR(invitationForm)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGSendQR") + "</a>";
                result += "<br/>";
                result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
            }
            if (Provider.Config.MailEnabled)
            {
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGLabelMail") + "</div>";
                result += "<input id=\"email\" name=\"email\" type=\"text\" placeholder=\"Personal Email Address (Required)\" class=\"text fullWidth\" value=\"" + usercontext.MailAddress + "\"/><br/><br/>";
            }
            if (Provider.Config.SMSEnabled)
            {
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGLabelPhoneNumber") + "</div>";
                result += "<input id=\"phone\" name=\"phone\" type=\"text\" placeholder=\"Phone Number (Required)\" class=\"text fullWidth\" value=\"" + usercontext.PhoneNumber + "\"/><br/><br/>";
            }
            result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGAccessMethod") + "</div>";
            result += "<select id=\"selectopt\" name=\"selectopt\" role=\"combobox\"  required=\"required\" contenteditable=\"false\" class=\"text fullWidth\" >";
            if (usercontext.PreferredMethod == RegistrationPreferredMethod.Choose)
            {
                result += "<option value=\"0\" selected=\"true\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionChooseBest") + "</option>";

                if (Provider.Config.AppsEnabled)
                    result += "<option value=\"1\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionTOTP") + "</option>";

                if (Provider.Config.MailEnabled)
                    result += "<option value=\"2\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionEmail") + "</option>";

                if (Provider.Config.SMSEnabled)
                    result += "<option value=\"3\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionPhone") + "</option>";
            }
            else if (usercontext.PreferredMethod == RegistrationPreferredMethod.Code)
            {
                result += "<option value=\"0\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionChooseBest") + "</option>";

                result += "<option value=\"1\" selected=\"true\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionTOTP") + "</option>";

                if (Provider.Config.MailEnabled)
                    result += "<option value=\"2\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionEmail") + "</option>";

                if (Provider.Config.SMSEnabled)
                    result += "<option value=\"3\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionPhone") + "</option>";
            }
            else if (usercontext.PreferredMethod == RegistrationPreferredMethod.Email)
            {
                result += "<option value=\"0\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionChooseBest") + "</option>";

                if (Provider.Config.AppsEnabled)
                    result += "<option value=\"1\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionTOTP") + "</option>";

                result += "<option value=\"2\" selected=\"true\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionEmail") + "</option>";

                if (Provider.Config.SMSEnabled)
                    result += "<option value=\"3\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionPhone") + "</option>";
            }
            else if (usercontext.PreferredMethod == RegistrationPreferredMethod.Phone)
            {
                result += "<option value=\"0\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionChooseBest") + "</option>";

                if (Provider.Config.AppsEnabled)
                    result += "<option value=\"1\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionTOTP") + "</option>";

                if (Provider.Config.MailEnabled)
                    result += "<option value=\"2\" >" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionEmail") + "</option>";

                result += "<option value=\"3\" selected=\"true\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionPhone") + "</option>";
            }
            result += "</select><br/><br/><br/>";
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"0\" />";
            result += "<table><tr>";
            result += "<td>";
            result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlINSRequest") + "\" onclick=\"fnbtnclicked(1)\" />";
            result += "</td>";
            result += "<td style=\"width: 15px\" />";
            result += "<td>";
            result += "<input id=\"cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(2)\" />";
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
            if (!String.IsNullOrEmpty(usercontext.UIMessage))
            {
                if (_ismessage)
                    result += "<p class=\"text fullWidth\" style=\"color: #6FA400\">" + usercontext.UIMessage + "</p><br/>";
                else
                    result += "<p class=\"error\">" + usercontext.UIMessage + "</p><br/>";
            }
            result += "<form method=\"post\" id=\"selectoptionsForm\" autocomplete=\"off\" >";
            result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";
            if (Provider.Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowManageOptions))
                result += "<a class=\"actionLink\" href=\"#\" id=\"chgopt\" name=\"chgopt\" onclick=\"return SetLinkTitle(selectoptionsForm, '1')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlChangeConfiguration") + "</a>";
            if (Provider.Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowChangePassword))
            {
                if (!Provider.Config.CustomUpdatePassword)
                    result += "<a class=\"actionLink\" href=\"/adfs/portal/updatepassword?username=" + usercontext.UPN + "\" id=\"chgpwd\" name=\"chgpwd\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlChangePassword") + "</a><br/>";
                else
                    result += "<a class=\"actionLink\" href=\"#\" id=\"chgpwd\" name=\"chgpwd\" onclick=\"return SetLinkTitle(selectoptionsForm, '2')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlChangePassword") + "</a><br/>";
            }
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"0\" />";
            result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
            result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMConnexion") + "\" onclick=\"fnbtnclicked()\"/>";
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
            result += "   var title = document.getElementById('authArea').childNodes[2];" + "\r\n";
            result += "   if (title)" + "\r\n";
            result += "   {" + "\r\n";
            result += "      title.innerHTML += \"<br/><br/>" + Resources.GetString(ResourcesLocaleKind.Titles, "MustUseCodePageTitle") + "\";" + "\r\n";
            result += "   }" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";

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

            result += "function ValidateEmail(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var data = document.getElementById('opt3');" + "\r\n";
            result += "   var email = document.getElementById('stmail');" + "\r\n";
            result += "   var err = document.getElementById('errorText');" + "\r\n";
            result += "   if (data.checked)" + "\r\n";
            result += "   {" + "\r\n";
            result += "         if ((email) && (email.value!==''))" + "\r\n";
            result += "         {" + "\r\n";
            result += "             return true;" + "\r\n";
            result += "         }" + "\r\n";
            result += "         else" + "\r\n";
            result += "         {" + "\r\n";
            result += "             err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidIncorrectEmail") + "\";" + "\r\n";
            result += "             return false;" + "\r\n";
            result += "         }" + "\r\n";
            result += "   }" + "\r\n";
            result += "   return true;" + "\r\n";
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

            result += "<form method=\"post\" id=\"ChooseMethodForm\" autocomplete=\"off\" onsubmit=\"return ValidateEmail(this)\" >";
            result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";
            bool notcoda = false;
            if (Provider.Config.AppsEnabled)
            {
                result += "<input id=\"opt1\" name=\"opt\" type=\"radio\" value=\"0\" checked=\"checked\" onchange=\"ChooseMethodChanged()\" /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlCHOOSEOptionTOTP") + "<br /><br/>";
            }
            else
                notcoda = true;
            bool notphone = false;
            if (Provider.Config.SMSEnabled)
            {
                if (notcoda)
                    result += "<input id=\"opt2\" name=\"opt\" type=\"radio\" value=\"1\" checked=\"checked\" onchange=\"ChooseMethodChanged()\"/> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlCHOOSEOptionSMS") + " " + MailUtilities.StripPhoneNumber(usercontext.PhoneNumber) + "<br/><br/>";
                else
                    result += "<input id=\"opt2\" name=\"opt\" type=\"radio\" value=\"1\" onchange=\"ChooseMethodChanged()\"/> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlCHOOSEOptionSMS") + " " + MailUtilities.StripPhoneNumber(usercontext.PhoneNumber) + "<br/><br/>";
            }
            else
                notphone = true;
            if (Provider.Config.MailEnabled)
            {
                if ((notphone) && (notcoda))
                    result += "<input id=\"opt3\" name=\"opt\" type=\"radio\" value=\"2\" checked=\"checked\" onchange=\"ChooseMethodChanged()\"/> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlCHOOSEOptionEmail") + " " + MailUtilities.StripEmailAddress(usercontext.MailAddress) + "<br /><br />";
                else
                    result += "<input id=\"opt3\" name=\"opt\" type=\"radio\" value=\"2\" onchange=\"ChooseMethodChanged()\"/> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlCHOOSEOptionEmail") + " " + MailUtilities.StripEmailAddress(usercontext.MailAddress) + "<br /><br />";
                result += "<div>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlCHOOSEOptionEmailWarning") + "</div><br />";
                result += "<input id=\"stmail\" name=\"stmail\" type=\"text\" class=\"text autoWidth\" disabled=\"disabled\"/><span class=\"text bigText\">" + MailUtilities.StripEmailDomain(usercontext.MailAddress) + "</span><br /><br />";
            }
            result += "<input id=\"opt4\" name=\"opt\" type=\"radio\" value=\"3\" onchange=\"ChooseMethodChanged()\"/> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlCHOOSEOptionNone") + "<br /><br />";
            result += "<input id=\"remember\" type=\"checkbox\" name=\"Remember\" /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlCHOOSEOptionRemember") + "<br /><br />";
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlCHOOSEOptionSendCode") + "\" />";
            result += "</form>";
            return result;
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
                result += "<input id=\"oldpwd\" name=\"oldpwd\" type=\"password\" placeholder=\"Current Password\" class=\"text fullWidth\"/><br/><br/>";
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDLabelNew") + "</div>";
                result += "<input id=\"newpwd\" name=\"newpwd\" type=\"password\" placeholder=\"New Password\" class=\"text fullWidth\"/><br/>";
                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDLabelNewConfirmation") + "</div>";
                result += "<input id=\"cnfpwd\" name=\"cnfpwd\" type=\"password\" placeholder=\"Confirm New Password\" class=\"text fullWidth\"/><br/><br/><br/>";
                result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"1\" />";
                result += "<table><tr>";
                result += "<td>";
                result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDSave") + "\" onClick=\"fnbtnclicked(1)\" />";
                result += "</td>";
                result += "<td style=\"width: 15px\" />";
                result += "<td>";
                result += "<input id=\"cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onClick=\"fnbtnclicked(2)\"/>";
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
            string result = "<script type=\"text/javascript\">" + "\r\n";
            result += "if (window.addEventListener)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.addEventListener('load', OnAutoPost, false);" + "\r\n";
            result += "}" + "\r\n";
            result += "else if (window.attachEvent)" + "\r\n";
            result += "{" + "\r\n";
            result += "   window.attachEvent('onload', OnAutoPost);" + "\r\n";
            result += "}" + "\r\n";
            result += "</script>" + "\r\n";
            result += "<form method=\"post\" id=\"bypassForm\" autocomplete=\"off\" title=\"Redirecting\" >";
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
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
                    result += "<p class=\"error\">" + usercontext.UIMessage + "</p><br/>";
                else
                    result += "<p class=\"error\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlErrorRestartSession") + "</p><br/>";
            }
            else
            {
                if (_ismessage)
                    result += "<p class=\"text fullWidth\" style=\"color: #6FA400\">" + usercontext.UIMessage + "</p><br/>";
                else
                    result += "<p class=\"error\">" + usercontext.UIMessage + "</p><br/>";
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
            if (Provider.Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowProvideInformations) && (!_disableoptions))
           // if (Provider.Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowProvideInformations) )
            {
                result += "<input id=\"cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMGoToRegistration") + "\" onClick=\"fnbtnclicked(2)\"/>";
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
            result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWRQRCode") + "</div>";
            result += "<img id=\"qr\" src=\"data:image/png;base64," + Provider.GetQRCodeString(usercontext) + "\"/></br>";
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\"OK\" />";
            result += "</form>";
            return result;
        }
        #endregion

        #region CodeRequest
        /// <summary>
        /// GetFormPreRenderHtmlCodeRequest implementation
        /// </summary>
        private string GetFormPreRenderHtmlCodeRequest(AuthenticationContext usercontext)
        {
            string result = "<script type='text/javascript'>" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

            result += "function OnAutoRefreshPost(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   setTimeout(function() { document.getElementById('refreshForm').submit() }, 3000)" + "\r\n";
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

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlCodeRequest implementation
        /// </summary>
        private string GetFormHtmlCodeRequest(AuthenticationContext usercontext)
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

            result += "<form method=\"post\" id=\"refreshForm\" autocomplete=\"off\" \">";
            result += GetFormHtmlDonut();
            switch (usercontext.PreferredMethod)
            {
                case RegistrationPreferredMethod.Code:
                    if (Provider.Config.AppsEnabled)
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMUseApp") + "</div><br/>";
                    break;
                case RegistrationPreferredMethod.Email:
                    if (Provider.Config.MailEnabled)
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtlmUIMUseEmail") + "</div><br/>";
                    break;

                case RegistrationPreferredMethod.Phone:
                    if (Provider.Config.SMSEnabled)
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMUsePhone") + "</div><br/>";
                    break;
            }
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
            if ((Provider.Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowManageOptions)) || (Provider.Config.UserFeatures.HasFlag(UserFeaturesOptions.AllowChangePassword)))
            {
                if (usercontext.ShowOptions)
                    result += "<input id=\"options\" type=\"checkbox\" name=\"Options\" checked=\"true\" /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMAccessOptions") + "<br/><br/><br/>";
                else
                    result += "<input id=\"options\" type=\"checkbox\" name=\"Options\" /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMAccessOptions") + "<br/><br/><br/>";
            }
            result += "<a class=\"actionLink\" href=\"#\" id=\"nocode\" name=\"nocode\" onclick=\"return SetLinkTitle(refreshForm, '3')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMNoCode") + "</a>";
            result += "</form>";
            return result;
        }
        #endregion

        #region InvitationRequest
        /// <summary>
        /// GetFormPreRenderHtmlInvitationRequest implementation
        /// </summary>
        private string GetFormPreRenderHtmlInvitationRequest(AuthenticationContext usercontext)
        {
            string result = "<script type='text/javascript'>" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

            result += "function OnAutoRefreshPost(frm)" + "\r\n";
            result += "{" + "\r\n";
            result += "   setTimeout(function() { document.getElementById('invitationReqForm').submit() }, 3000)" + "\r\n";
            result += "   return true;" + "\r\n";
            result += "}" + "\r\n";
            result += "\r\n";
            result += "\r\n";

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlInvitationRequest implementation
        /// </summary>
        private string GetFormHtmlInvitationRequest(AuthenticationContext usercontext)
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

            result += "<form method=\"post\" id=\"invitationReqForm\" autocomplete=\"off\" \">";
            result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlINSWaitRequest") + "</div>";

            result += GetFormHtmlDonut();

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
            result += "   setTimeout(function() { document.getElementById('sendkeyReqForm').submit() }, 3000)" + "\r\n";
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

            result += GetFormHtmlDonut();

            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "</form>";
            return result;
        }

        #endregion

        /// <summary>
        /// GetFormHtmlDonut method implementation
        /// </summary>
        private string GetFormHtmlDonut()
        {
            string result = "<div id=\"cookiePullWaitingWheel\" \">";
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

        /// <summary>
        /// StripDisplayKey method implmentation
        /// </summary>
        private string StripDisplayKey(string dkey)
        {
            return dkey.Substring(0, 5) + " ... (truncated for security reasons) ...";
        }
    }
}

