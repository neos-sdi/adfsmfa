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
using System.Threading.Tasks;
using Microsoft.IdentityServer.Web.Authentication.External;

namespace Neos.IdentityServer.MultiFactor
{
    public abstract class BasePresentation : IAdapterPresentation, IAdapterPresentationForm
    {
        /// <summary>
        /// Constructor implementation
        /// </summary>
        protected BasePresentation()
        {

        }

        /// <summary>
        /// Constructor implementation
        /// </summary>
        protected BasePresentation(AuthenticationProvider provider, IAuthenticationContext context)
        {
            this.Provider = provider;
            this.Context = new AuthenticationContext(context);
            this.IsPermanentFailure = (this.Context.TargetUIMode == ProviderPageMode.DefinitiveError);
            this.IsMessage = (this.Context.TargetUIMode != ProviderPageMode.DefinitiveError);
            this.DisableOptions = false;
            this.Resources = new ResourcesLocale(context.Lcid);
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        protected BasePresentation(AuthenticationProvider provider, IAuthenticationContext context, string message)
        {
            this.Provider = provider;
            this.Context = new AuthenticationContext(context);
            this.Context.UIMessage = message;
            this.IsPermanentFailure = (this.Context.TargetUIMode == ProviderPageMode.DefinitiveError);
            this.IsMessage = (this.Context.TargetUIMode != ProviderPageMode.DefinitiveError);
            this.DisableOptions = false;
            this.Resources = new ResourcesLocale(context.Lcid);
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        protected BasePresentation(AuthenticationProvider provider, IAuthenticationContext context, string message, bool ismessage)
        {
            this.Provider = provider;
            this.Context = new AuthenticationContext(context);
            this.Context.UIMessage = message;
            this.IsPermanentFailure = (this.Context.TargetUIMode == ProviderPageMode.DefinitiveError);
            this.IsMessage = ismessage;
            this.DisableOptions = false;
            this.Resources = new ResourcesLocale(context.Lcid);
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        protected BasePresentation(AuthenticationProvider provider, IAuthenticationContext context, ProviderPageMode suite)
        {
            this.Provider = provider;
            this.Context = new AuthenticationContext(context);
            this.Context.TargetUIMode = suite;
            this.IsPermanentFailure = (this.Context.TargetUIMode == ProviderPageMode.DefinitiveError);
            this.IsMessage = (this.Context.TargetUIMode != ProviderPageMode.DefinitiveError);
            this.DisableOptions = false;
            this.Resources = new ResourcesLocale(context.Lcid);
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        protected BasePresentation(AuthenticationProvider provider, IAuthenticationContext context, string message, ProviderPageMode suite, bool disableoptions = false)
        {
            this.Provider = provider;
            this.Context = new AuthenticationContext(context);
            this.Context.TargetUIMode = suite;
            this.Context.UIMessage = message;
            this.IsPermanentFailure = (this.Context.TargetUIMode == ProviderPageMode.DefinitiveError);
            this.IsMessage = (this.Context.TargetUIMode != ProviderPageMode.DefinitiveError);
            this.DisableOptions = disableoptions;
            this.Resources = new ResourcesLocale(context.Lcid);
        }

        #region Properties
        /// <summary>
        /// UiKind property 
        /// </summary>
        public virtual ADFSUserInterfaceKind UiKind
        {
            get;
            set;
        }

        /// <summary>
        /// IsPermanentFailure property
        /// </summary>
        public virtual bool IsPermanentFailure
        {
            get;
            internal set;
        }

        /// <summary>
        /// IsMessage property
        /// </summary>
        public virtual bool IsMessage
        {
            get;
            internal set;
        }

        /// <summary>
        /// DisableOptions property
        /// </summary>
        public virtual bool DisableOptions
        {
            get;
            internal set;
        }

        /// <summary>
        /// Provider property
        /// </summary>
        public virtual AuthenticationProvider Provider
        {
            get;
            internal set;
        }

        /// <summary>
        /// Context property
        /// </summary>
        public virtual AuthenticationContext Context
        {
            get; 
            internal set;
        }

        /// <summary>
        /// Resources property
        /// </summary>
        public virtual ResourcesLocale Resources
        {
            get;
            internal set;
        }

        /// <summary>
        /// UseUIPaginated property
        /// </summary>
        public virtual bool UseUIPaginated
        {
            get; 
            internal set;
        }

        #endregion

        #region ADFS Interfaces
        /// <summary>
        /// IAdapterPresentation GetPageTitle implementation
        /// </summary>
        public virtual string GetPageTitle(int lcid)
        {
            return Resources.GetString(ResourcesLocaleKind.Titles, "TitlePageTitle");
        }

        /// <summary>
        /// GetFormHtmlMessageZone method implementation
        /// </summary>
        public virtual string GetFormHtmlMessageZone(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            if (IsPermanentFailure)
            {
                result += "</br>";
                if (!String.IsNullOrEmpty(usercontext.UIMessage))
                    result += "<p class=\"error\">" + usercontext.UIMessage + "</p></br>";
                else
                    result += "<p class=\"error\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlErrorRestartSession") + "</p></br>";
            }
            else
            {
                if (!String.IsNullOrEmpty(usercontext.UIMessage))
                {
                    result += "</br>";
                    if (IsMessage)
                        result += "<p class=\"text fullWidth\" style=\"color: #6FA400\">" + usercontext.UIMessage + "</p></br>";
                    else
                        result += "<p class=\"error\">" + usercontext.UIMessage + "</p></br>";
                }
            }
            return result;
        }

        /// <summary>
        /// GetFormPreRenderHtmlCSS implementation
        /// </summary>
        protected virtual string GetFormPreRenderHtmlCSS(AuthenticationContext usercontext)
        {
            string result = "<style>" + "\r\n";
            result += "#wizardMessage, #wizardMessage2, #wizardMessage3 {" + "\r\n";
            result += "box-sizing: border-box;" + "\r\n";
            result += "color: rgb(38, 38, 38);" + "\r\n";
            result += "direction: ltr;" + "\r\n";
            result += "display: block;" + "\r\n";
            result += "font-family: \"Segoe UI Webfont\", -apple-system, \"Helvetica Neue\", \"Lucida Grande\", Roboto, Ebrima, \"Nirmala UI\", Gadugi, \"Segoe Xbox Symbol\", \"Segoe UI Symbol\", \"Meiryo UI\", \"Khmer UI\", Tunga, \"Lao UI\", Raavi, \"Iskoola Pota\", Latha, Leelawadee, \"Microsoft YaHei UI\", \"Microsoft JhengHei UI\", \"Malgun Gothic\", \"Estrangelo Edessa\", \"Microsoft Himalaya\", \"Microsoft New Tai Lue\", \"Microsoft PhagsPa\", \"Microsoft Tai Le\", \"Microsoft Yi Baiti\", \"Mongolian Baiti\", \"MV Boli\", \"Myanmar Text\", \"Cambria Math\";" + "\r\n";
            result += "font-weight: 300;" + "\r\n";
            result += "font-size: 1.7em;" + "\r\n";
            result += "height: auto;" + "\r\n";
            result += "line-height: 28px;" + "\r\n";
            result += "margin-bottom: 16px;" + "\r\n";
            result += "margin-left: -2px;" + "\r\n";
            result += "margin-right: -2px;" + "\r\n";
            result += "margin-top: 16px;" + "\r\n";
            result += "padding-bottom: 0px;" + "\r\n";
            result += "padding-left: 0px;" + "\r\n";
            result += "padding-right: 0px;" + "\r\n";
            result += "padding-top: 0px;" + "\r\n";
            result += "text-align: left;" + "\r\n";
            result += "text-size-adjust: 100%;" + "\r\n";
            result += "width: 342px;" + "\r\n";
            result += "background-color: transparent;" + "\r\n";
            result += "}" + "\r\n";
            result += "</style>" + "\r\n";
            return result;
        }


        /// <summary>
        /// IAdapterPresentationForm GetFormHtml implementation
        /// </summary>
        public virtual string GetFormHtml(int lcid)
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
                case ProviderPageMode.EnrollOTPForce:
                    result = GetFormHtmlEnrollOTP(Context);
                    break;
                case ProviderPageMode.EnrollEmail:
                case ProviderPageMode.EnrollEmailAndSave:
                case ProviderPageMode.EnrollEmailForce:
                    result = GetFormHtmlEnrollEmail(Context);
                    break;
                case ProviderPageMode.EnrollPhone:
                case ProviderPageMode.EnrollPhoneAndSave:
                case ProviderPageMode.EnrollPhoneForce:
                    result = GetFormHtmlEnrollPhone(Context);
                    break;
                case ProviderPageMode.EnrollBiometrics:
                case ProviderPageMode.EnrollBiometricsAndSave:
                case ProviderPageMode.EnrollBiometricsForce:
                    result = GetFormHtmlEnrollBio(Context);
                    break;
                case ProviderPageMode.EnrollPin:
                case ProviderPageMode.EnrollPinAndSave:
                case ProviderPageMode.EnrollPinForce:
                    result = GetFormHtmlEnrollPinCode(Context);
                    break;
            }
            return result;
        }

        /// <summary>
        /// IAdapterPresentationForm GetFormPreRenderHtml implementation
        /// </summary>
        public virtual string GetFormPreRenderHtml(int lcid)
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
                    result = GetFormPreRenderHtmlCSS(Context);
                    result += GetFormPreRenderHtmlChangePassword(Context);
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
                case ProviderPageMode.EnrollOTPForce:
                    result = GetFormPreRenderHtmlCSS(Context);
                    result += GetFormPreRenderHtmlEnrollOTP(Context);
                    break;
                case ProviderPageMode.EnrollEmail:
                case ProviderPageMode.EnrollEmailAndSave:
                case ProviderPageMode.EnrollEmailForce:
                    result = GetFormPreRenderHtmlCSS(Context);
                    result += GetFormPreRenderHtmlEnrollEmail(Context);
                    break;
                case ProviderPageMode.EnrollPhone:
                case ProviderPageMode.EnrollPhoneAndSave:
                case ProviderPageMode.EnrollPhoneForce:
                    result = GetFormPreRenderHtmlCSS(Context);
                    result += GetFormPreRenderHtmlEnrollPhone(Context);
                    break;
                case ProviderPageMode.EnrollBiometrics:
                case ProviderPageMode.EnrollBiometricsAndSave:
                case ProviderPageMode.EnrollBiometricsForce:
                    result = GetFormPreRenderHtmlCSS(Context);
                    result += GetFormPreRenderHtmlEnrollBio(Context);
                    break;
                case ProviderPageMode.EnrollPin:
                case ProviderPageMode.EnrollPinAndSave:
                case ProviderPageMode.EnrollPinForce:
                    result = GetFormPreRenderHtmlCSS(Context);
                    result += GetFormPreRenderHtmlEnrollPinCode(Context);
                    break;
            }
            return result;
        }
        #endregion

        public abstract string GetFormPreRenderHtmlIdentification(AuthenticationContext usercontext);
        public abstract string GetFormHtmlIdentification(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlRegistration(AuthenticationContext usercontext);
        public abstract string GetFormHtmlRegistration(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlInvitation(AuthenticationContext usercontext);
        public abstract string GetFormHtmlInvitation(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlSelectOptions(AuthenticationContext usercontext);
        public abstract string GetFormHtmlSelectOptions(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlChooseMethod(AuthenticationContext usercontext);
        public abstract string GetFormHtmlChooseMethod(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlChangePassword(AuthenticationContext usercontext);
        public abstract string GetFormHtmlChangePassword(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlBypass(AuthenticationContext usercontext);
        public abstract string GetFormHtmlBypass(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlLocking(AuthenticationContext usercontext);
        public abstract string GetFormHtmlLocking(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlSendCodeRequest(AuthenticationContext usercontext);
        public abstract string GetFormHtmlSendCodeRequest(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlSendAdministrativeRequest(AuthenticationContext usercontext);
        public abstract string GetFormHtmlSendAdministrativeRequest(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlSendKeyRequest(AuthenticationContext usercontext);
        public abstract string GetFormHtmlSendKeyRequest(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlEnrollOTP(AuthenticationContext usercontext);
        public abstract string GetFormHtmlEnrollOTP(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlEnrollEmail(AuthenticationContext usercontext);
        public abstract string GetFormHtmlEnrollEmail(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlEnrollPhone(AuthenticationContext usercontext);
        public abstract string GetFormHtmlEnrollPhone(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlEnrollBio(AuthenticationContext Context);
        public abstract string GetFormHtmlEnrollBio(AuthenticationContext Context);
        public abstract string GetFormPreRenderHtmlEnrollPinCode(AuthenticationContext usercontext);
        public abstract string GetFormHtmlEnrollPinCode(AuthenticationContext usercontext);

        #region QRCode
        /// <summary>
        /// GetFormPreRenderHtmlShowQRCode implementation
        /// </summary>
        /// <param name="usercontext"></param>
        /// <returns></returns>
        public virtual string GetFormPreRenderHtmlShowQRCode(AuthenticationContext usercontext)
        {
            return string.Empty;
        }

        /// <summary>
        /// GetFormHtmlShowQRCode implementation
        /// </summary>
        public virtual string GetFormHtmlShowQRCode(AuthenticationContext usercontext)
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

        #region Donut
        /// <summary>
        /// GetPartHtmlDonut method implementation
        /// </summary>
        protected virtual string GetPartHtmlDonut(bool visible = true)
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

        #region Utils
        /// <summary>
        /// StripDisplayKey method implmentation
        /// </summary>
        internal string StripDisplayKey(string dkey)
        {
            if ((dkey != null) && (dkey.Length >= 5))
                return dkey.Substring(0, 5) + " ... (truncated for security reasons) ... ";
            else
                return " ... (invalid key) ... ";
        }
        #endregion
    }

    public class AdapterPresentation : BasePresentation
    {
        private BasePresentation _adapter = null;
        #region Constructors
        /// <summary>
        /// Constructor implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context)
        {
            if (provider == null)
                throw new ArgumentNullException("Provider");
            if (provider.Config == null)
                throw new ArgumentNullException("Config");
            switch (provider.Config.UiKind)
            {
                case ADFSUserInterfaceKind.Default2019:
                    _adapter = new AdapterPresentation2019(provider, context);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
                case ADFSUserInterfaceKind.Custom:
                    _adapter = new AdapterPresentationDefault(provider, context);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
                default:
                    _adapter = new AdapterPresentationDefault(provider, context);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
            }
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, string message)
        {
            if (provider == null)
                throw new ArgumentNullException("Provider");
            if (provider.Config == null)
                throw new ArgumentNullException("Config");
            switch (provider.Config.UiKind)
            {
                case ADFSUserInterfaceKind.Default2019:
                    _adapter = new AdapterPresentation2019(provider, context, message);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
                case ADFSUserInterfaceKind.Custom:
                    _adapter = new AdapterPresentationDefault(provider, context, message);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
                default:
                    _adapter = new AdapterPresentationDefault(provider, context, message);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
            }
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, string message, bool ismessage)
        {
            if (provider == null)
                throw new ArgumentNullException("Provider");
            if (provider.Config == null)
                throw new ArgumentNullException("Config");
            switch (provider.Config.UiKind)
            {
                case ADFSUserInterfaceKind.Default2019:
                    _adapter = new AdapterPresentation2019(provider, context, message, ismessage);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
                case ADFSUserInterfaceKind.Custom:
                    _adapter = new AdapterPresentationDefault(provider, context, message, ismessage);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
                default:
                    _adapter = new AdapterPresentationDefault(provider, context, message, ismessage);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
            }
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, ProviderPageMode suite)
        {
            if (provider == null)
                throw new ArgumentNullException("Provider");
            if (provider.Config == null)
                throw new ArgumentNullException("Config");
            switch (provider.Config.UiKind)
            {
                case ADFSUserInterfaceKind.Default2019:
                    _adapter = new AdapterPresentation2019(provider, context, suite);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
                case ADFSUserInterfaceKind.Custom:
                    _adapter = new AdapterPresentationDefault(provider, context, suite);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
                default:
                    _adapter = new AdapterPresentationDefault(provider, context, suite);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
            }
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, string message, ProviderPageMode suite, bool disableoptions = false)
        {
            if (provider == null)
                throw new ArgumentNullException("Provider");
            if (provider.Config == null)
                throw new ArgumentNullException("Config");
            switch (provider.Config.UiKind)
            {
                case ADFSUserInterfaceKind.Default2019:
                    _adapter = new AdapterPresentation2019(provider, context, message, suite, disableoptions);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
                case ADFSUserInterfaceKind.Custom:
                    _adapter = new AdapterPresentationDefault(provider, context, message, suite, disableoptions);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
                default:
                    _adapter = new AdapterPresentationDefault(provider, context, message, suite, disableoptions);
                    _adapter.UseUIPaginated = provider.Config.UseUIPaginated;
                    break;
            }
        }
        #endregion

        #region properties
        /// <summary>
        /// IsPermanentFailure property
        /// </summary>
        public override bool IsPermanentFailure
        {
            get { return _adapter.IsPermanentFailure; }
            internal set { _adapter.IsPermanentFailure = value; }
        }

        /// <summary>
        /// IsMessage property
        /// </summary>
        public override bool IsMessage
        {
            get { return _adapter.IsMessage; }
            internal set { _adapter.IsMessage = value; }
        }

        /// <summary>
        /// DisableOptions property
        /// </summary>
        public override bool DisableOptions
        {
            get { return _adapter.DisableOptions; }
            internal set { _adapter.DisableOptions = value; }
        }

        /// <summary>
        /// Provider property
        /// </summary>
        public override AuthenticationProvider Provider
        {
            get { return _adapter.Provider; }
            internal set { _adapter.Provider = value; }
        }

        /// <summary>
        /// Context property
        /// </summary>
        public override AuthenticationContext Context
        {
            get { return _adapter.Context; }
            internal set { _adapter.Context = value; }
        }

        /// <summary>
        /// Resources property
        /// </summary>
        public override ResourcesLocale Resources
        {
            get { return _adapter.Resources; }
            internal set { _adapter.Resources = value; }
        }
        #endregion

        /// <summary>
        /// GetPageTitle implementation
        /// </summary>
        public override string GetPageTitle(int lcid)
        {
            return _adapter.GetPageTitle(lcid);
        }

        /// <summary>
        /// GetFormHtml implementation
        /// </summary>
        public override string GetFormHtml(int lcid)
        {
            return _adapter.GetFormHtml(lcid);
        }

        /// <summary>
        /// GetFormPreRenderHtml implementation
        /// </summary>
        public override string GetFormPreRenderHtml(int lcid)
        {
            return _adapter.GetFormPreRenderHtml(lcid);
        }

        /// <summary>
        /// GetFormPreRenderHtmlIdentification implementation
        /// </summary>
        public override string GetFormPreRenderHtmlIdentification(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlIdentification(usercontext);
        }

        /// <summary>
        /// GetFormHtmlIdentification implementation
        /// </summary>
        public override string GetFormHtmlIdentification(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlIdentification(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlRegistration implementation
        /// </summary>
        public override string GetFormPreRenderHtmlRegistration(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlRegistration(usercontext);
        }

        /// <summary>
        /// GetFormHtmlRegistration implementation
        /// </summary>
        public override string GetFormHtmlRegistration(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlRegistration(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlInvitation implementation
        /// </summary>
        public override string GetFormPreRenderHtmlInvitation(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlInvitation(usercontext);
        }

        /// <summary>
        /// GetFormHtmlInvitation implementation
        /// </summary>
        public override string GetFormHtmlInvitation(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlInvitation(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlSelectOptions implementation
        /// </summary>
        public override string GetFormPreRenderHtmlSelectOptions(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlSelectOptions(usercontext);
        }

        /// <summary>
        /// GetFormHtmlSelectOptions implementation
        /// </summary>
        public override string GetFormHtmlSelectOptions(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlSelectOptions(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlChooseMethod implementation
        /// </summary>
        public override string GetFormPreRenderHtmlChooseMethod(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlChooseMethod(usercontext);
        }

        /// <summary>
        /// GetFormHtmlChooseMethod implementation
        /// </summary>
        public override string GetFormHtmlChooseMethod(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlChooseMethod(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlChangePassword implementation
        /// </summary>
        public override string GetFormPreRenderHtmlChangePassword(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlChangePassword(usercontext);
        }

        /// <summary>
        /// GetFormHtmlChangePassword implementation
        /// </summary>
        public override string GetFormHtmlChangePassword(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlChangePassword(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlBypass implementation
        /// </summary>
        public override string GetFormPreRenderHtmlBypass(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlBypass(usercontext);
        }

        /// <summary>
        /// GetFormHtmlBypass implementation
        /// </summary>
        public override string GetFormHtmlBypass(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlBypass(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlLocking implementation
        /// </summary>
        public override string GetFormPreRenderHtmlLocking(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlLocking(usercontext);
        }

        /// <summary>
        /// GetFormHtmlLocking implementation
        /// </summary>
        public override string GetFormHtmlLocking(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlLocking(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlShowQRCode implementation
        /// </summary>
        public override string GetFormPreRenderHtmlShowQRCode(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlShowQRCode(usercontext);
        }

        /// <summary>
        /// GetFormHtmlShowQRCode implementation
        /// </summary>
        public override string GetFormHtmlShowQRCode(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlShowQRCode(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlSendCodeRequest implementation
        /// </summary>
        public override string GetFormPreRenderHtmlSendCodeRequest(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlSendCodeRequest(usercontext);
        }

        /// <summary>
        /// GetFormHtmlSendCodeRequest implementation
        /// </summary>
        public override string GetFormHtmlSendCodeRequest(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlSendCodeRequest(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlSendAdministrativeRequest implementation
        /// </summary>
        public override string GetFormPreRenderHtmlSendAdministrativeRequest(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlSendAdministrativeRequest(usercontext);
        }

        /// <summary>
        /// GetFormHtmlSendAdministrativeRequest implementation
        /// </summary>
        public override string GetFormHtmlSendAdministrativeRequest(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlSendAdministrativeRequest(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlSendKeyRequest implementation
        /// </summary>
        public override string GetFormPreRenderHtmlSendKeyRequest(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlSendKeyRequest(usercontext);
        }

        /// <summary>
        /// GetFormHtmlSendKeyRequest implementation
        /// </summary>
        public override string GetFormHtmlSendKeyRequest(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlSendKeyRequest(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlEnrollOTP implementation
        /// </summary>
        public override string GetFormPreRenderHtmlEnrollOTP(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlEnrollOTP(usercontext);
        }

        /// <summary>
        /// GetFormHtmlEnrollOTP implementation
        /// </summary>
        public override string GetFormHtmlEnrollOTP(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlEnrollOTP(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlEnrollEmail implementation
        /// </summary>
        public override string GetFormPreRenderHtmlEnrollEmail(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlEnrollEmail(usercontext);
        }

        /// <summary>
        /// GetFormHtmlEnrollEmail implementation
        /// </summary>
        public override string GetFormHtmlEnrollEmail(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlEnrollEmail(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlEnrollPhone implementation
        /// </summary>
        public override string GetFormPreRenderHtmlEnrollPhone(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlEnrollPhone(usercontext);
        }

        /// <summary>
        /// GetFormHtmlEnrollPhone implementation
        /// </summary>
        public override string GetFormHtmlEnrollPhone(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlEnrollPhone(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlEnrollBio implementation
        /// </summary>
        public override string GetFormPreRenderHtmlEnrollBio(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlEnrollBio(usercontext);
        }

        /// <summary>
        /// GetFormHtmlEnrollBio implementation
        /// </summary>
        public override string GetFormHtmlEnrollBio(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlEnrollBio(usercontext);
        }

        /// <summary>
        /// GetFormPreRenderHtmlEnrollPinCode implementation
        /// </summary>
        public override string GetFormPreRenderHtmlEnrollPinCode(AuthenticationContext usercontext)
        {
            return _adapter.GetFormPreRenderHtmlEnrollPinCode(usercontext);
        }

        /// <summary>
        /// GetFormHtmlEnrollPinCode implementation
        /// </summary>
        public override string GetFormHtmlEnrollPinCode(AuthenticationContext usercontext)
        {
            return _adapter.GetFormHtmlEnrollPinCode(usercontext);
        }
    }
}
