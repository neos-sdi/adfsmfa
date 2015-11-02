//******************************************************************************************************************************************************************************************//
// Copyright (c) 2015 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor.Resources;
using Microsoft.IdentityServer.Web.Authentication.External;

namespace Neos.IdentityServer.MultiFactor
{
    public class AdapterPresentation : IAdapterPresentation, IAdapterPresentationForm
    {
        private string _message;
        private bool _isPermanentFailure;
        private bool _ismessage;
        private AuthenticationProvider _provider;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider)
        {
            this._provider = provider;
            this._message = string.Empty;
            this._isPermanentFailure = false;
            this._ismessage = false;
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, string message)
        {
            this._provider = provider;
            this._message = message;
            this._isPermanentFailure = false;
            this._ismessage = true;
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, string message, bool isPermanentFailure)
        {
            this._provider = provider;
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
            switch (Provider.UIM)
            {
                case ProviderPageMode.locking: // Error and eventually retry
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
                        switch (Provider.UserRegistration.PreferredMethod)
                        {
                            case RegistrationPreferredMethod.ApplicationCode:
                                if (Provider.Config.AppsEnabled)
                                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlUIMUseApp + "</div><br/>";
                                break;
                            case RegistrationPreferredMethod.Email:
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
                        result += "<input id=\"uimode\" type=\"hidden\" name=\"UIMode\" value=\"" + Provider.UIM.ToString() + "\"/>";
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
                    switch (Provider.UserRegistration.PreferredMethod)
                    {
                        case RegistrationPreferredMethod.ApplicationCode:
                            if (Provider.Config.AppsEnabled)
                                result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlUIMUseApp + "</div><br/>";
                            break;
                        case RegistrationPreferredMethod.Email:
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
                    result += "<input id=\"uimode\" type=\"hidden\" name=\"UIMode\" value=\"" + Provider.UIM.ToString() + "\"/>";
                    result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
                    result += "<input id=\"continueButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\"" + html_strings.HtmlUIMConnexion + "\" /><br/><br/>";
                    result += "<a class=\"actionLink\" href=\"#\" id=\"nocode\" name=\"nocode\" onclick=\"return SetLinkTitle(loginForm, '3')\"; style=\"cursor: pointer;\">" + html_strings.HtmlUIMNoCode + "</a>";
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
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlREGLabelMail + "</div>";
                    result += "<input id=\"email\" name=\"email\" type=\"text\" placeholder=\"Personal Email Address\" class=\"text fullWidth\" value=\"" + Provider.UserRegistration.MailAddress + "\"/><br/><br/>";
                    if ((Provider.Config.AppsEnabled) && (!string.IsNullOrEmpty(Provider.UserRegistration.DisplayKey)))
                    {
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlREGLabelAppKey+ "</div>";
                        result += "<input id=\"secretkey\" name=\"secretkey\" type=\"text\" readonly=\"true\" placeholder=\"Secret Key\" class=\"text fullWidth\" style=\"background-color: #C0C0C0\" value=\"" + Provider.UserRegistration.DisplayKey + "\"/><br/>";
                        result += "<a class=\"actionLink\" href=\"#\" id=\"qrcode\" name=\"qrcode\" onclick=\"return ShowQR(loginForm)\"; style=\"cursor: pointer;\">" + html_strings.HtmlREGShowQR +"</a><br/>";
                        result += "<input id=\"lnk\" type=\"hidden\" name=\"lnk\" value=\"0\"/>";
                    }
                    if (Provider.Config.SMSEnabled)
                    {
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlREGLabelPhoneNumber+ "</div>";
                        result += "<input id=\"phone\" name=\"phone\" type=\"text\" placeholder=\"Phone Number\" class=\"text fullWidth\" value=\"" + Provider.UserRegistration.PhoneNumber + "\"/><br/><br/>";
                    }
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlREGAccessMethod + "</div>";
                    result += "<select id=\"selectopt\" name=\"selectopt\" role=\"combobox\"  required=\"required\" contenteditable=\"false\" class=\"text fullWidth\" >";
                    if (Provider.UserRegistration.PreferredMethod==RegistrationPreferredMethod.Choose)
                    {
                        result += "<option value=\"0\" selected=\"true\">" + html_strings.HtmlREGOptionChooseBest + "</option>";
                        if (Provider.Config.AppsEnabled)
                            result += "<option value=\"1\" >" + html_strings.HtmlREGOptionTOTP + "</option>";
                        result += "<option value=\"2\" >" + html_strings.HtmlREGOptionEmail + "</option>";
                        if (Provider.Config.SMSEnabled)
                            result += "<option value=\"3\" >" + html_strings.HtmlREGOptionPhone + "</option>";
                    }
                    else if (Provider.UserRegistration.PreferredMethod==RegistrationPreferredMethod.ApplicationCode)
                    {
                        result += "<option value=\"0\" >" + html_strings.HtmlREGOptionChooseBest + "</option>";
                        result += "<option value=\"1\" selected=\"true\">" + html_strings.HtmlREGOptionTOTP + "</option>";
                        result += "<option value=\"2\" >" + html_strings.HtmlREGOptionEmail + "</option>";
                        if (Provider.Config.SMSEnabled)
                            result += "<option value=\"3\" >" + html_strings.HtmlREGOptionPhone + "</option>";
                    }
                    else if (Provider.UserRegistration.PreferredMethod==RegistrationPreferredMethod.Email)
                    {
                        result += "<option value=\"0\" >" + html_strings.HtmlREGOptionChooseBest + "</option>";
                        if (Provider.Config.AppsEnabled)
                            result += "<option value=\"1\" >" + html_strings.HtmlREGOptionTOTP + "</option>";
                        result += "<option value=\"2\" selected=\"true\">" + html_strings.HtmlREGOptionEmail + "</option>";
                        if (Provider.Config.SMSEnabled)
                            result += "<option value=\"3\" >" + html_strings.HtmlREGOptionPhone + "</option>";
                    }
                    else if (Provider.UserRegistration.PreferredMethod == RegistrationPreferredMethod.Phone)
                    {
                        result += "<option value=\"0\" >" + html_strings.HtmlREGOptionChooseBest + "</option>";
                        if (Provider.Config.AppsEnabled)
                            result += "<option value=\"1\" >" + html_strings.HtmlREGOptionTOTP + "</option>";
                        result += "<option value=\"2\" >" + html_strings.HtmlREGOptionEmail + "</option>";
                        result += "<option value=\"3\" selected=\"true\">" + html_strings.HtmlREGOptionPhone + "</option>";
                    }
                    result += "</select><br/><br/><br/>";
                    result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                    result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                    result += "<input id=\"uimode\" type=\"hidden\" name=\"UIMode\" value=\"" + Provider.UIM.ToString() + "\"/>";
                    result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"1\" />";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"continue\" value=\""+ html_strings.HtmlPWDSave +"\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + html_strings.HtmlPWDCancel + "\" onclick=\"fnbtnclicked(2)\"/>";
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

                        result += "<form method=\"post\" id=\"loginForm\" autocomplete=\"off\" title=\"modification vos identifaints de connexion\" onsubmit=\"return ValidChangePwd(this)\" >";
                        result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlPWDLabelActual + "</div>";
                        result += "<input id=\"oldpwd\" name=\"oldpwd\" type=\"password\" placeholder=\"Current Password\" class=\"text fullWidth\"/><br/><br/>";
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlPWDLabelNew + "</div>";
                        result += "<input id=\"newpwd\" name=\"newpwd\" type=\"password\" placeholder=\"New Password\" class=\"text fullWidth\"/><br/>";
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlPWDLabelNewConfirmation + "</div>";
                        result += "<input id=\"cnfpwd\" name=\"cnfpwd\" type=\"password\" placeholder=\"Confirm New Password\" class=\"text fullWidth\"/><br/><br/><br/>";
                        result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                        result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                        result += "<input id=\"uimode\" type=\"hidden\" name=\"UIMode\" value=\"" + Provider.UIM.ToString() + "\"/>";
                        result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"1\" />";
                        result += "<table><tr>";
                        result += "<td>";
                        result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"continue\" value=\"" + html_strings.HtmlPWDSave + "\" onclick=\"fnbtnclicked(1)\" />";
                        result += "</td>";
                        result += "<td style=\"width: 15px\" />";
                        result += "<td>";
                        result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + html_strings.HtmlPWDCancel + "\" onclick=\"fnbtnclicked(2)\"/>";
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
                    result += "<form method=\"post\" id=\"loginForm\" autocomplete=\"off\" title=\"Configurer mes options\" >";
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";
                    result += "<a class=\"actionLink\" href=\"#\" id=\"chgopt\" name=\"chgopt\" onclick=\"return SetLinkTitle(loginForm, '1')\"; style=\"cursor: pointer;\">" + html_strings.HtmlChangeConfiguration + "</a>";
                    if (!Provider.Config.CustomUpdatePassword)
                        result += "<a class=\"actionLink\" href=\"/adfs/portal/updatepassword?username="+Provider.UserRegistration.UPN+"\" id=\"chgpwd\" name=\"chgpwd\"; style=\"cursor: pointer;\">" + html_strings.HtmlChangePassword + "</a><br/>";
                    else
                        result += "<a class=\"actionLink\" href=\"#\" id=\"chgpwd\" name=\"chgpwd\" onclick=\"return SetLinkTitle(loginForm, '2')\"; style=\"cursor: pointer;\">" + html_strings.HtmlChangePassword + "</a><br/>";
                    
                    result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                    result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                    result += "<input id=\"uimode\" type=\"hidden\" name=\"UIMode\" value=\"" + Provider.UIM.ToString() + "\"/>";
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

                    result += "<form method=\"post\" id=\"loginForm\" autocomplete=\"off\" title=\"Aidez-nos à protéger votre compte\" >";
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\"></label></div>";
                    bool notcoda = false;
                    if (!string.IsNullOrEmpty(Provider.UserRegistration.DisplayKey) && (Provider.Config.AppsEnabled))
                    {
                        result += "<input id=\"opt1\" name=\"opt\" type=\"radio\" value=\"0\" checked=\"checked\" onchange=\"ChooseMethodChanged()\" /> " + html_strings.HtmlCHOOSEOptionTOTP + "<br /><br/>";
                    }
                    else
                        notcoda = true;
                    bool notphone = false;
                    if (!string.IsNullOrEmpty(Provider.UserRegistration.PhoneNumber) && (Provider.Config.SMSEnabled))
                    {
                        if (notcoda)
                            result += "<input id=\"opt2\" name=\"opt\" type=\"radio\" value=\"1\" checked=\"checked\" onchange=\"ChooseMethodChanged()\"/> "+ html_strings.HtmlCHOOSEOptionSMS + " " + Utilities.StripPhoneNumer(Provider.UserRegistration.PhoneNumber) + "<br/><br/>";
                        else
                            result += "<input id=\"opt2\" name=\"opt\" type=\"radio\" value=\"1\" onchange=\"ChooseMethodChanged()\"/> " + html_strings.HtmlCHOOSEOptionSMS + " " + Utilities.StripPhoneNumer(Provider.UserRegistration.PhoneNumber) + "<br/><br/>";
                    }
                    else
                        notphone = true;
                    if (!string.IsNullOrEmpty(Provider.UserRegistration.MailAddress))
                    {
                        if ((notphone) && (notcoda))
                            result += "<input id=\"opt3\" name=\"opt\" type=\"radio\" value=\"2\" checked=\"checked\" onchange=\"ChooseMethodChanged()\"/> " + html_strings.HtmlCHOOSEOptionEmail + " " + Utilities.StripEmailAddress(Provider.UserRegistration.MailAddress) + "<br /><br />";
                        else
                            result += "<input id=\"opt3\" name=\"opt\" type=\"radio\" value=\"2\" onchange=\"ChooseMethodChanged()\"/> " + html_strings.HtmlCHOOSEOptionEmail + " " + Utilities.StripEmailAddress(Provider.UserRegistration.MailAddress) + "<br /><br />";
                        result += "<div>" + html_strings.HtmlCHOOSEOptionEmailWarning + "</div><br />";
                        result += "<input id=\"stmail\" name=\"stmail\" type=\"text\" class=\"text autoWidth\" disabled=\"disabled\"/><span class=\"text bigText\">" + Utilities.StripEmailDomain(Provider.UserRegistration.MailAddress) + "</span><br /><br />";
                    }
                    result += "<input id=\"opt4\" name=\"opt\" type=\"radio\" value=\"3\" onchange=\"ChooseMethodChanged()\"/> " + html_strings.HtmlCHOOSEOptionNone  + "<br /><br />";
                    result += "<input id=\"remember\" type=\"checkbox\" name=\"Remember\" /> " + html_strings.HtmlCHOOSEOptionRemember  + "<br /><br />";
                    result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                    result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                    result += "<input id=\"uimode\" type=\"hidden\" name=\"UIMode\" value=\"" + Provider.UIM.ToString() + "\"/>";
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
                    result += "<input id=\"uimode\" type=\"hidden\" name=\"UIMode\" value=\"" + Provider.UIM.ToString() + "\"/>";
                    result += "</form>";
                    break;
                case ProviderPageMode.ShowQRCode:
                    result += "<form method=\"post\" id=\"loginForm\" autocomplete=\"off\" >";
                    result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + html_strings.HtmlLabelWRQRCode + "</div>";
                    result += "<img id=\"qr\" src=\"data:image/png;base64," + Provider.GetQRCodeString() + "\"/></br>";
                    result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
                    result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
                    result += "<input id=\"uimode\" type=\"hidden\" name=\"UIMode\" value=\"" + Provider.UIM.ToString() + "\"/></br>";
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
            switch (Provider.UIM)
            {
                case ProviderPageMode.locking:
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

                case ProviderPageMode.SelectOptions:
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
                        result += "<script type=\"text/javascript\">" + "\r\n";
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


                    result += "<script type=\"text/javascript\">" + "\r\n";
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
                    result += "   if (email)" + "\r\n"; 
                    result += "   {" + "\r\n";
                    result += "      if (!email.value.match(mailformat))" + "\r\n";
                    result += "      {" + "\r\n";
                    result += "         err.innerHTML = \"" + valid_strings.ValidIncorrectEmail + "\";" + "\r\n";
                    result += "         return false;" + "\r\n";
                    result += "      }" + "\r\n";
                    result += "   }" + "\r\n";
                    result += "   if (phone)" + "\r\n"; 
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
                    result += "<script type=\"text/javascript\">" + "\r\n";
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
                    result += "<script type=\"text/javascript\">" + "\r\n";
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
        #endregion

       
    }
}

