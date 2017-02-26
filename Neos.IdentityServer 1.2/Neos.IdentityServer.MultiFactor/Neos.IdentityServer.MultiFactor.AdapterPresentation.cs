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
//******************************************************************************************************************************************************************************************//
using System;
using Microsoft.IdentityServer.Web.Authentication.External;
using Neos.IdentityServer.MultiFactor.Resources;


namespace Neos.IdentityServer.MultiFactor
{
    public class AdapterPresentation : IAdapterPresentation, IAdapterPresentationForm
    {
        private string _message;
        private bool _isPermanentFailure;
        private bool _ismessage;
        private AuthenticationProvider _provider;
        private IAuthenticationContext _context;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context)
        {
            this._provider = provider;
            this._context = context;
            this._message = string.Empty;
            this._isPermanentFailure = false;
            this._ismessage = false;
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, string message)
        {
            this._provider = provider;
            this._context = context;
            this._message = message;
            this._isPermanentFailure = false;
            this._ismessage = true;
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, string message, bool isPermanentFailure)
        {
            this._provider = provider;
            this._context = context;
            this._message = message;
            this._isPermanentFailure = isPermanentFailure;
            this._ismessage = false;
        }

        /// <summary>
        /// SetCultureInfo method implementation
        /// </summary>
        internal static void SetCultureInfo(int lcid)
        {
            System.Globalization.CultureInfo inf = new System.Globalization.CultureInfo(lcid);
            errors_strings.Culture = inf;
            html_strings.Culture = inf;
            infos_strings.Culture = inf;
            title_strings.Culture = inf;
            valid_strings.Culture = inf;
            mail_strings.Culture = inf;
        }

        /// <summary>
        /// IAdapterPresentation GetPageTitle implementation
        /// </summary>
        public string GetPageTitle(int lcid)
        {
            SetCultureInfo(lcid);
            return title_strings.TitlePageTitle;
        }

        /// <summary>
        /// IAdapterPresentationForm GetFormHtml implementation
        /// </summary>
        public string GetFormHtml(int lcid)
        {
            SetCultureInfo(lcid);
            string result = "";
            if (_isPermanentFailure)
            {
                if (!String.IsNullOrEmpty(this._message))
                    result += "<p class=\"error\">" + _message + "</p><br/>";
                else
                    result += "<p class=\"error\">" + html_strings.HtmlErrorRestartSession + "</p><br/>";
                return result;
            }
            AuthenticationContext usercontext = new AuthenticationContext(Context);
            switch (usercontext.UIMode)
            {
                case ProviderPageMode.Locking: // Error and eventually retry
                    if (!String.IsNullOrEmpty(this._message))
                    {
                        if (_ismessage)
                            result += "<p class=\"text fullWidth\" style=\"color: #6FA400\">" + _message + "</p><br/>";
                        else
                            result += "<p class=\"error\">" + _message + "</p><br/>";
                    }
                    if (!this._isPermanentFailure)
                    {
                        result += "<form method=\"post\" id=\"loginForm\" autocomplete=\"off\">";
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmUIMLabelOTP + "</div>";
                        result += "<input id=\"pin\" name=\"pin\" type=\"password\" placeholder=\"One Time Password\" class=\"text fullWidth\" autofocus=\"autofocus\" /><br/>";
                        switch (usercontext.PreferredMethod)
                        {
                            case RegistrationPreferredMethod.ApplicationCode:
                                if (Provider.Config.AppsEnabled)
                                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlUIMUseApp + "</div><br/>";
                                break;
                            case RegistrationPreferredMethod.Email:
                                if (Provider.Config.MailEnabled)
                                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtlmUIMUseEmail + "</div><br/>";
                                break;

                            case RegistrationPreferredMethod.Phone:
                                if (Provider.Config.SMSEnabled)
                                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlUIMUsePhone + "</div><br/>";
                                break;
                        }
                        result += "<input id=\"options\" type=\"checkbox\" name=\"Options\" /> " + html_strings.HtmlUIMAccessOptions + "<br/><br/><br/>";
                        result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                        result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                        result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
                        result += "<input id=\"continueButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\""+ html_strings.HtmlUIMConnexion + "\" /><br/><br/>";
                        result += "<a class=\"actionLink\" href=\"#\" id=\"nocode\" name=\"nocode\" onclick=\"return SetLinkTitle(loginForm, '3')\"; style=\"cursor: pointer;\">" + html_strings.HtmlUIMNoCode +"</a>";
                        result += "</form>";
                    }
                    break;
                case ProviderPageMode.Identification:  // OTP Control
                    result += "<form method=\"post\" id=\"loginForm\" autocomplete=\"off\">";
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmUIMLabelOTP + "</div>";
                    result += "<input id=\"pin\" name=\"pin\" type=\"password\" placeholder=\"One Time Password\" class=\"text fullWidth\" autofocus=\"autofocus\" /><br/>";
                    switch (usercontext.PreferredMethod)
                    {
                        case RegistrationPreferredMethod.ApplicationCode:
                            if (Provider.Config.AppsEnabled)
                                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlUIMUseApp + "</div><br/>";
                            break;
                        case RegistrationPreferredMethod.Email:
                            if (Provider.Config.MailEnabled)
                                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtlmUIMUseEmail + "</div><br/>";
                            break;

                        case RegistrationPreferredMethod.Phone:
                            if (Provider.Config.SMSEnabled)
                                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlUIMUsePhone + "</div><br/>";
                            break;
                    }
                    result += "<input id=\"options\" type=\"checkbox\" name=\"Options\" /> " + html_strings.HtmlUIMAccessOptions + "<br/><br/><br/>";
                    result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                    result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                    result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
                    result += "<input id=\"continueButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\"" + html_strings.HtmlUIMConnexion + "\" /><br/><br/>";
                    result += "<a class=\"actionLink\" href=\"#\" id=\"nocode\" name=\"nocode\" onclick=\"return SetLinkTitle(loginForm, '3')\"; style=\"cursor: pointer;\">" + html_strings.HtmlUIMNoCode + "</a>";
                    result += "</form>";
                    break;
                case ProviderPageMode.RequestingCode:  // Async call for code (sms, email)
                    result += "<script type=\"text/javascript\">" + "\r\n";
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


                    result += "<div id=\"cookiePullWaitingWheel\" \">";
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

                 //   result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmUIMLabelOTP + "</div>";
                    switch (usercontext.PreferredMethod)
                    {
                        case RegistrationPreferredMethod.ApplicationCode:
                            if (Provider.Config.AppsEnabled)
                                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlUIMUseApp + "</div><br/>";
                            break;
                        case RegistrationPreferredMethod.Email:
                            if (Provider.Config.MailEnabled)
                                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtlmUIMUseEmail + "</div><br/>";
                            break;

                        case RegistrationPreferredMethod.Phone:
                            if (Provider.Config.SMSEnabled)
                                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlUIMUsePhone + "</div><br/>";
                            break;
                    }
                    result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                    result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                    result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
                    result += "<a class=\"actionLink\" href=\"#\" id=\"nocode\" name=\"nocode\" onclick=\"return SetLinkTitle(refreshForm, '3')\"; style=\"cursor: pointer;\">" + html_strings.HtmlUIMNoCode + "</a>";
                    result += "</form>";
                    break;
                case ProviderPageMode.Registration: // OTP Registration
                    result += "<script type=\"text/javascript\">" + "\r\n";
                    result += "if (window.addEventListener)" + "\r\n";
                    result += "{" + "\r\n";
                    result += "   window.addEventListener('load', ChangeRegistrationTitle, false);" + "\r\n";
                    result += "}" + "\r\n";
                    result += "else if (window.attachEvent)" + "\r\n";
                    result += "{" + "\r\n";
                    result += "   window.attachEvent('onload', ChangeRegistrationTitle);" + "\r\n";
                    result += "}" + "\r\n";
                    result += "</script>" + "\r\n";

                    result += "<form method=\"post\" id=\"loginForm\" autocomplete=\"off\" onsubmit=\"return ValidateRegistrationEmail(this)\">";
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";
                    if (Provider.Config.MailEnabled)
                    {
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlREGLabelMail + "</div>";
                        result += "<input id=\"email\" name=\"email\" type=\"text\" placeholder=\"Personal Email Address\" class=\"text fullWidth\" value=\"" + usercontext.MailAddress + "\"/><br/><br/>";
                    }
                    if (Provider.Config.AppsEnabled) 
                    {
                        string displaykey = string.Empty;
                        if (!string.IsNullOrEmpty(usercontext.SecretKey))  
                        {
                            displaykey = Base32.Encode(usercontext.SecretKey);
                        }
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlREGLabelAppKey+ "</div>";
                        result += "<input id=\"secretkey\" name=\"secretkey\" type=\"text\" readonly=\"true\" placeholder=\"Secret Key\" class=\"text fullWidth\" style=\"background-color: #C0C0C0\" value=\"" + displaykey + "\"/><br/>";
                        result += "<a class=\"actionLink\" href=\"#\" id=\"resetkey\" name=\"resetkey\" onclick=\"return ResetKey(loginForm)\"; style=\"cursor: pointer;\">" + html_strings.HtmlREGResetQR + "</a>";
                        if (usercontext.SecretKeyChanged)
                        {
                            result += "<div class=\"fieldMargin error smallText\"><label id=\"secretkeychanged\" for=\"\" >" + html_strings.HtmlSecretKeyChanged + "</label></div>";
                        }
                        result += "<a class=\"actionLink\" href=\"#\" id=\"qrcode\" name=\"qrcode\" onclick=\"return ShowQR(loginForm)\"; style=\"cursor: pointer;\">" + html_strings.HtmlREGShowQR +"</a><br/>";
                        result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
                    }
                    if (Provider.Config.SMSEnabled)
                    {
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlREGLabelPhoneNumber+ "</div>";
                        result += "<input id=\"phone\" name=\"phone\" type=\"text\" placeholder=\"Phone Number\" class=\"text fullWidth\" value=\"" + usercontext.PhoneNumber + "\"/><br/><br/>";
                    }
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlREGAccessMethod + "</div>";
                    result += "<select id=\"selectopt\" name=\"selectopt\" role=\"combobox\"  required=\"required\" contenteditable=\"false\" class=\"text fullWidth\" >";
                    if (usercontext.PreferredMethod == RegistrationPreferredMethod.Choose)
                    {
                        result += "<option value=\"0\" selected=\"true\">" + html_strings.HtmlREGOptionChooseBest + "</option>";

                        if (Provider.Config.AppsEnabled)
                            result += "<option value=\"1\" >" + html_strings.HtmlREGOptionTOTP + "</option>";

                        if (Provider.Config.MailEnabled)
                            result += "<option value=\"2\" >" + html_strings.HtmlREGOptionEmail + "</option>";

                        if (Provider.Config.SMSEnabled)
                            result += "<option value=\"3\" >" + html_strings.HtmlREGOptionPhone + "</option>";
                    }
                    else if (usercontext.PreferredMethod == RegistrationPreferredMethod.ApplicationCode)
                    {
                        result += "<option value=\"0\" >" + html_strings.HtmlREGOptionChooseBest + "</option>";

                        result += "<option value=\"1\" selected=\"true\">" + html_strings.HtmlREGOptionTOTP + "</option>";

                        if (Provider.Config.MailEnabled)
                            result += "<option value=\"2\" >" + html_strings.HtmlREGOptionEmail + "</option>";

                        if (Provider.Config.SMSEnabled)
                            result += "<option value=\"3\" >" + html_strings.HtmlREGOptionPhone + "</option>";
                    }
                    else if (usercontext.PreferredMethod == RegistrationPreferredMethod.Email)
                    {
                        result += "<option value=\"0\" >" + html_strings.HtmlREGOptionChooseBest + "</option>";

                        if (Provider.Config.AppsEnabled)
                            result += "<option value=\"1\" >" + html_strings.HtmlREGOptionTOTP + "</option>";
                        
                        result += "<option value=\"2\" selected=\"true\">" + html_strings.HtmlREGOptionEmail + "</option>";

                        if (Provider.Config.SMSEnabled)
                            result += "<option value=\"3\" >" + html_strings.HtmlREGOptionPhone + "</option>";
                    }
                    else if (usercontext.PreferredMethod == RegistrationPreferredMethod.Phone)
                    {
                        result += "<option value=\"0\" >" + html_strings.HtmlREGOptionChooseBest + "</option>";

                        if (Provider.Config.AppsEnabled)
                            result += "<option value=\"1\" >" + html_strings.HtmlREGOptionTOTP + "</option>";

                        if (Provider.Config.MailEnabled)
                            result += "<option value=\"2\" >" + html_strings.HtmlREGOptionEmail + "</option>";

                        result += "<option value=\"3\" selected=\"true\">" + html_strings.HtmlREGOptionPhone + "</option>";
                    }
                    result += "</select><br/><br/><br/>";
                    result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                    result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                    result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"0\" />";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"continue\" value=\""+ html_strings.HtmlPWDSave +"\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + html_strings.HtmlPWDCancel + "\" onclick=\"fnbtnclicked(2)\" />";
                    result += "</td>";
                    result += "</tr></table>";

                    result += "</form>";
                    break;

                case ProviderPageMode.ChangePassword: // Change Password
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

                        result += "<form method=\"post\" id=\"loginForm\" autocomplete=\"off\" onsubmit=\"return ValidChangePwd(this)\" >";
                        result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlPWDLabelActual + "</div>";
                        result += "<input id=\"oldpwd\" name=\"oldpwd\" type=\"password\" placeholder=\"Current Password\" class=\"text fullWidth\"/><br/><br/>";
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlPWDLabelNew + "</div>";
                        result += "<input id=\"newpwd\" name=\"newpwd\" type=\"password\" placeholder=\"New Password\" class=\"text fullWidth\"/><br/>";
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlPWDLabelNewConfirmation + "</div>";
                        result += "<input id=\"cnfpwd\" name=\"cnfpwd\" type=\"password\" placeholder=\"Confirm New Password\" class=\"text fullWidth\"/><br/><br/><br/>";
                        result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                        result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                        result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"1\" />";
                        result += "<table><tr>";
                        result += "<td>";
                        result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"continue\" value=\"" + html_strings.HtmlPWDSave + "\" onClick=\"fnbtnclicked(1)\" />";
                        result += "</td>";
                        result += "<td style=\"width: 15px\" />";
                        result += "<td>";
                        result += "<input id=\"cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + html_strings.HtmlPWDCancel + "\" onClick=\"fnbtnclicked(2)\"/>";
                        result += "</td>";
                        result += "</tr></table>";
                        result += "</form>";
                    }
                    break;
                case ProviderPageMode.SelectOptions:
                    if (!String.IsNullOrEmpty(this._message))
                    {
                        if (_ismessage)
                            result += "<p class=\"text fullWidth\" style=\"color: #6FA400\">" + _message + "</p><br/>";
                        else
                            result += "<p class=\"error\">" + _message + "</p><br/>";
                    }
                    result += "<form method=\"post\" id=\"loginForm\" autocomplete=\"off\" >";
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";
                    result += "<a class=\"actionLink\" href=\"#\" id=\"chgopt\" name=\"chgopt\" onclick=\"return SetLinkTitle(loginForm, '1')\"; style=\"cursor: pointer;\">" + html_strings.HtmlChangeConfiguration + "</a>";
                    if (!Provider.Config.CustomUpdatePassword)
                        result += "<a class=\"actionLink\" href=\"/adfs/portal/updatepassword?username=" + usercontext.UPN + "\" id=\"chgpwd\" name=\"chgpwd\"; style=\"cursor: pointer;\">" + html_strings.HtmlChangePassword + "</a><br/>";
                    else
                        result += "<a class=\"actionLink\" href=\"#\" id=\"chgpwd\" name=\"chgpwd\" onclick=\"return SetLinkTitle(loginForm, '2')\"; style=\"cursor: pointer;\">" + html_strings.HtmlChangePassword + "</a><br/>";
                    result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                    result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                    result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"0\" />";
                    result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
                    result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + html_strings.HtmlUIMConnexion + "\" onclick=\"fnbtnclicked()\"/>";
                    result += "</form>";
                    break;
                case ProviderPageMode.ChooseMethod:
                    result += "<script type=\"text/javascript\">" + "\r\n";
                    result += "if (window.addEventListener)" + "\r\n";
                    result += "{" + "\r\n";
                    result += "   window.addEventListener('load', ChangeChooseMethodTitle, false);" + "\r\n";
                    result += "}" + "\r\n";
                    result += "else if (window.attachEvent)" + "\r\n";
                    result += "{" + "\r\n";
                    result += "   window.attachEvent('onload', ChangeChooseMethodTitle);" + "\r\n";
                    result += "}" + "\r\n";
                    result += "</script>" + "\r\n";

                    result += "<form method=\"post\" id=\"loginForm\" autocomplete=\"off\" onsubmit=\"return ValidateEmail(this)\" >";
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";
                    bool notcoda = false;
                    if (Provider.Config.AppsEnabled)
                    {
                        result += "<input id=\"opt1\" name=\"opt\" type=\"radio\" value=\"0\" checked=\"checked\" onchange=\"ChooseMethodChanged()\" /> " + html_strings.HtmlCHOOSEOptionTOTP + "<br /><br/>";
                    }
                    else
                        notcoda = true;
                    bool notphone = false;
                    if (Provider.Config.SMSEnabled)
                    {
                        if (notcoda)
                            result += "<input id=\"opt2\" name=\"opt\" type=\"radio\" value=\"1\" checked=\"checked\" onchange=\"ChooseMethodChanged()\"/> " + html_strings.HtmlCHOOSEOptionSMS + " " + Utilities.StripPhoneNumer(usercontext.PhoneNumber) + "<br/><br/>";
                        else
                            result += "<input id=\"opt2\" name=\"opt\" type=\"radio\" value=\"1\" onchange=\"ChooseMethodChanged()\"/> " + html_strings.HtmlCHOOSEOptionSMS + " " + Utilities.StripPhoneNumer(usercontext.PhoneNumber) + "<br/><br/>";
                    }
                    else
                        notphone = true;
                    if (Provider.Config.MailEnabled)
                    {
                        if ((notphone) && (notcoda))
                            result += "<input id=\"opt3\" name=\"opt\" type=\"radio\" value=\"2\" checked=\"checked\" onchange=\"ChooseMethodChanged()\"/> " + html_strings.HtmlCHOOSEOptionEmail + " " + Utilities.StripEmailAddress(usercontext.MailAddress) + "<br /><br />";
                        else
                            result += "<input id=\"opt3\" name=\"opt\" type=\"radio\" value=\"2\" onchange=\"ChooseMethodChanged()\"/> " + html_strings.HtmlCHOOSEOptionEmail + " " + Utilities.StripEmailAddress(usercontext.MailAddress) + "<br /><br />";
                        result += "<div>" + html_strings.HtmlCHOOSEOptionEmailWarning + "</div><br />";
                        result += "<input id=\"stmail\" name=\"stmail\" type=\"text\" class=\"text autoWidth\" disabled=\"disabled\"/><span class=\"text bigText\">" + Utilities.StripEmailDomain(usercontext.MailAddress) + "</span><br /><br />";
                    }
                    result += "<input id=\"opt4\" name=\"opt\" type=\"radio\" value=\"3\" onchange=\"ChooseMethodChanged()\"/> " + html_strings.HtmlCHOOSEOptionNone  + "<br /><br />";
                    result += "<input id=\"remember\" type=\"checkbox\" name=\"Remember\" /> " + html_strings.HtmlCHOOSEOptionRemember  + "<br /><br />";
                    result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                    result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                    result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\""+  html_strings.HtmlCHOOSEOptionSendCode + "\" />";
                    result += "</form>";
                    break;
                case ProviderPageMode.Bypass:
                    result += "<script type=\"text/javascript\">" + "\r\n";
                    result += "if (window.addEventListener)" + "\r\n";
                    result += "{" + "\r\n";
                    result += "   window.addEventListener('load', OnAutoPost, false);" + "\r\n";
                    result += "}" + "\r\n";
                    result += "else if (window.attachEvent)" + "\r\n";
                    result += "{" + "\r\n";
                    result += "   window.attachEvent('onload', OnAutoPost);" + "\r\n";
                    result += "}" + "\r\n";
                    result += "</script>" + "\r\n";
                    result += "<form method=\"post\" id=\"loginForm\" autocomplete=\"off\" title=\"Redirecting\" >";
                    result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                    result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                    result += "</form>";
                    break;
                case ProviderPageMode.ShowQRCode:
                    result += "<form method=\"post\" id=\"loginForm\" autocomplete=\"off\" >";
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlLabelWRQRCode + "</div>";
                    result += "<img id=\"qr\" src=\"data:image/png;base64," + Provider.GetQRCodeString(usercontext) + "\"/></br>";
                    result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                    result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                    result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\"OK\" />";
                    result += "</form>";
                    break;
            }
            return result;
        }

        /// <summary>
        /// IAdapterPresentationForm GetFormPreRenderHtml implementation
        /// </summary>
        public string GetFormPreRenderHtml(int lcid)
        {
            SetCultureInfo(lcid);
            string result = "";
            AuthenticationContext _userregistration = new AuthenticationContext(Context);
            switch (_userregistration.UIMode)
            {
                case ProviderPageMode.Locking:
                    result += "<script type=\"text/javascript\">" + "\r\n";
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

                    result += "//]]>" + "\r\n";
                    result += "</script>" + "\r\n";
                    break;

                case ProviderPageMode.Identification:
                    result += "<script type='text/javascript'>" + "\r\n";
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

                    result += "//]]>" + "\r\n";
                    result += "</script>" + "\r\n";
                    break;
                    
                case ProviderPageMode.SelectOptions:
                    result += "<script type='text/javascript'>" + "\r\n";
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

                    result += "function fnbtnclicked()"+ "\r\n";
                    result += "{"+ "\r\n";
                    result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
                    result += "   lnk.value = 1;" + "\r\n";
                    result += "}" + "\r\n";
                    result += "\r\n";

                    result += "//]]>" + "\r\n";
                    result += "</script>" + "\r\n";
                    break;

                case ProviderPageMode.ChangePassword:
                    if (Provider.Config.CustomUpdatePassword)
                    {
                        result += "<script type='text/javascript'>" + "\r\n";
                        result += "//<![CDATA[" + "\r\n";

                        result += "function ValidChangePwd(frm)"+ "\r\n";
                        result += "{"+ "\r\n";
                        result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
                        result += "   if (lnk.value=='1')" + "\r\n";
                        result += "   {" + "\r\n";
                        result += "     var err = document.getElementById('errorText');" + "\r\n"; 
                        result += "     if (frm.elements['oldpwd'].value == \"\")" + "\r\n"; 
                        result += "     {" + "\r\n";
                        result += "         err.innerHTML = \"" +  valid_strings.ValidPassActualError + "\";" + "\r\n";
                        result += "         frm.elements['oldpwd'].focus();" + "\r\n";
                        result += "         return false;" + "\r\n";
                        result += "     }" + "\r\n";
                        result += "     if (frm.elements['newpwd'].value == \"\")" + "\r\n";
                        result += "     {" + "\r\n";
                        result += "         err.innerHTML = \"" + valid_strings.ValidPassNewError + "\";" + "\r\n";
                        result += "         frm.elements['newpwd'].focus();" + "\r\n";
                        result += "         return false;" + "\r\n";
                        result += "     }" + "\r\n";
                        result += "     if (frm.elements['cnfpwd'].value == \"\")" + "\r\n";
                        result += "     {" + "\r\n";
                        result += "         err.innerHTML = \"" + valid_strings.ValidConfirmNewPassError + "\";" + "\r\n";
                        result += "         frm.elements['cnfpwd'].focus();" + "\r\n";
                        result += "         return false;" + "\r\n";
                        result += "     }" + "\r\n";
                        result += "     if (frm.elements['cnfpwd'].value != frm.elements['newpwd'].value)" + "\r\n";
                        result += "     {" + "\r\n";
                        result += "         err.innerHTML = \"" + valid_strings.ValidNewPassError + "\";" + "\r\n";
                        result += "         frm.elements['cnfpwd'].focus();" + "\r\n";
                        result += "         return false;" + "\r\n";
                        result += "     }" + "\r\n";
                        result += "     err.innerHTML = \"\";" + "\r\n";
                        result += "   }" + "\r\n";
                        result += "   return true;" + "\r\n";
                        result += "}" + "\r\n";
                        result += "\r\n";

                        result += "function fnbtnclicked(id)"+ "\r\n";
                        result += "{"+ "\r\n";
                        result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
                        result += "   lnk.value = id;" + "\r\n";
                        result += "}" + "\r\n";
                        result += "\r\n";

                        result += "function ChangePasswordTitle()"+ "\r\n";
                        result += "{"+ "\r\n";
                        result += "   var title = document.getElementById('authArea').childNodes[2];" + "\r\n"; 
                        result += "   if (title)" + "\r\n"; 
                        result += "   {" + "\r\n";
                        result += "      title.innerHTML = \"" + title_strings.PasswordPageTitle + "\";" + "\r\n";
                        result += "   }" + "\r\n";
                        result += "   return true;" + "\r\n";
                        result += "}" + "\r\n";

                        result += "//]]>" + "\r\n";
                        result += "</script>" + "\r\n";
                    }
                    break;

                case ProviderPageMode.Registration:
                    string reg = @"/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/";
                    string pho = @"/^\+(?:[0-9] ?){6,14}[0-9]$/";
                    string pho10 = @"/^\d{10}$/";
                    string phous = @"/^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$/";


                    result += "<script type='text/javascript'>" + "\r\n";
                    result += "//<![CDATA[" + "\r\n";

                    result += "function ValidateRegistrationEmail(frm)" + "\r\n";
                    result += "{"+ "\r\n";
                    result += "   var mailformat = "+ reg +" ;" + "\r\n";
                    result += "   var phoneformat = " + pho + " ;" + "\r\n";
                    result += "   var phoneformat10 = " + pho10 + " ;" + "\r\n";
                    result += "   var phoneformatus = " + phous + " ;" + "\r\n"; 
                    result += "   var email = document.getElementById('email');" + "\r\n";
                    result += "   var phone = document.getElementById('phone');" + "\r\n";
                    result += "   var err = document.getElementById('errorText');" + "\r\n"; 
                    result += "   if ((email) && (email.value!==''))"+ "\r\n"; 
                    result += "   {" + "\r\n";
                    result += "      if (!email.value.match(mailformat))" + "\r\n";
                    result += "      {" + "\r\n";
                    result += "         err.innerHTML = \"" + valid_strings.ValidIncorrectEmail + "\";" + "\r\n";
                    result += "         return false;" + "\r\n";
                    result += "      }" + "\r\n";
                    result += "   }" + "\r\n";
                    result += "   if ((phone) && (phone.value!==''))" + "\r\n"; 
                    result += "   {" + "\r\n";
                    result += "      if (!phone.value.match(phoneformat) && !phone.value.match(phoneformat10) && !phone.value.match(phoneformatus) )" + "\r\n";
                    result += "      {" + "\r\n";
                    result += "         err.innerHTML = \"" + valid_strings.ValidIncorrectPhoneNumber + "\";" + "\r\n";
                    result += "         return false;" + "\r\n";
                    result += "      }" + "\r\n";
                    result += "   }" + "\r\n";
                    result += "   return true;" + "\r\n";
                    result += "}" + "\r\n";
                    result += "\r\n";


                    result += "function SelectionRegistrationChanged()" + "\r\n";
                    result += "{"+ "\r\n";
                    result += "   var data = document.getElementById('selectopt');" + "\r\n"; 
                    result += "   var sel = document.getElementById('selector');" + "\r\n";
                    result += "   sel.value = data.selectedIndex-1;" + "\r\n";
                    result += "   return true;" + "\r\n";
                    result += "}" + "\r\n";
                    result += "\r\n";

                    result += "function ChangeRegistrationTitle()"+ "\r\n";
                    result += "{"+ "\r\n";
                    result += "   var title = document.getElementById('authArea').childNodes[2];" + "\r\n"; 
                    result += "   if (title)" + "\r\n"; 
                    result += "   {" + "\r\n";
                    result += "      title.innerHTML += \"<br/><br/>" + title_strings.RegistrationPageTitle + "\";" + "\r\n";
                    result += "   }" + "\r\n";
                    result += "   return true;" + "\r\n";
                    result += "}" + "\r\n";

                    result += "function fnbtnclicked(id)"+ "\r\n";
                    result += "{"+ "\r\n";
                    result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
                    result += "   lnk.value = id;" + "\r\n";
                    result += "   return true;" + "\r\n";
                    result += "}" + "\r\n";
                    result += "\r\n";

                    result += "function ShowQR(frm)" + "\r\n";
                    result += "{"+ "\r\n";
                    result += "   var lnk = document.getElementById('lnk');" + "\r\n";
                    result += "   if (lnk)" + "\r\n";
                    result += "   {" + "\r\n";
                    result += "      lnk.value = 1;" + "\r\n";
                    result += "   }" + "\r\n";
                    result += "   frm.submit();" + "\r\n";
                    result += "   return true;" + "\r\n";
                    result += "}" + "\r\n";

                    result += "function ResetKey(frm)" + "\r\n";
                    result += "{"+ "\r\n";
                    result += "   var lnk = document.getElementById('lnk');" + "\r\n";
                    result += "   if (lnk)" + "\r\n";
                    result += "   {" + "\r\n";
                    result += "      lnk.value = 2;" + "\r\n";
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
                    break;
                case ProviderPageMode.ChooseMethod:
                    result += "<script type='text/javascript'>" + "\r\n";
                    result += "//<![CDATA[" + "\r\n";
                    result += "function ChangeChooseMethodTitle()"+ "\r\n";
                    result += "{"+ "\r\n";
                    result += "   var title = document.getElementById('authArea').childNodes[2];" + "\r\n"; 
                    result += "   if (title)" + "\r\n"; 
                    result += "   {" + "\r\n";
                    result += "      title.innerHTML += \"<br/><br/>" + title_strings.MustUseCodePageTitle + "\";" + "\r\n";
                    result += "   }" + "\r\n";
                    result += "   return true;" + "\r\n";
                    result += "}" + "\r\n";

                    result += "function ChooseMethodChanged()" + "\r\n";
                    result += "{"+ "\r\n";
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
                    result += "{"+ "\r\n";
                    result += "   var data = document.getElementById('opt3');" + "\r\n"; 
                    result += "   var email = document.getElementById('stmail');" + "\r\n";
                    result += "   var err = document.getElementById('errorText');" + "\r\n"; 
                    result += "   if (data.checked)" + "\r\n"; 
                    result += "   {" + "\r\n";
                    result += "         if ((email) && (email.value!==''))"+ "\r\n"; 
                    result += "         {" + "\r\n";
                    result += "             return true;" + "\r\n";
                    result += "         }" + "\r\n";
                    result += "         else" + "\r\n"; 
                    result += "         {" + "\r\n";
                    result += "             err.innerHTML = \"" + valid_strings.ValidIncorrectEmail + "\";" + "\r\n";
                    result += "             return false;" + "\r\n";
                    result += "         }" + "\r\n";
                    result += "   }" + "\r\n";
                    result += "   return true;" + "\r\n";
                    result += "}" + "\r\n";
                    result += "\r\n";

                    result += "function ShowQR(frm)" + "\r\n";
                    result += "{"+ "\r\n";
                    result += "   var lnk = document.getElementById('lnk');" + "\r\n";
                    result += "   if (lnk)" + "\r\n";
                    result += "   {" + "\r\n";
                    result += "      lnk.value = 1;" + "\r\n";
                    result += "   }" + "\r\n";
                    result += "   frm.submit();" + "\r\n";
                    result += "   return true;" + "\r\n";
                    result += "}" + "\r\n";
                    result += "\r\n";
                    result += "//]]>" + "\r\n";
                    result += "</script>" + "\r\n";
                    break;
                case ProviderPageMode.Bypass:
                    result += "<script type='text/javascript'>" + "\r\n";
                    result += "//<![CDATA[" + "\r\n";
                    result += "function OnAutoPost()" + "\r\n";
                    result += "{"+ "\r\n";
                    result += "   document.getElementById(\"loginForm\").submit();" + "\r\n";
                    result += "   return true;" + "\r\n";
                    result += "}" + "\r\n";
                    result += "\r\n";
                    result += "\r\n";
                    result += "//]]>" + "\r\n";
                    result += "</script>" + "\r\n";
                    break;
                case ProviderPageMode.RequestingCode:
                    result += "<script type='text/javascript'>" + "\r\n";
                    result += "//<![CDATA[" + "\r\n";

                    result += "function OnAutoRefreshPost()" + "\r\n";
                    result += "{" + "\r\n";
                    result += "   setTimeout(function() { document.getElementById(\"refreshForm\").submit() }, 3000)" + "\r\n";
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
                    break;
                default: 
                    break;
            }
            return result;
        }

        #region properties
        /// <summary>
        /// Message propertry
        /// </summary>
        public string Message
        {
            get { return _message; }
        }

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
        public IAuthenticationContext Context
        {
            get { return _context; }
        }

        #endregion

       
    }
}

