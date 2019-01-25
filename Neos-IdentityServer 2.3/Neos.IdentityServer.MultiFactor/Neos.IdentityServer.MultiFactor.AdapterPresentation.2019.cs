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
using Microsoft.IdentityServer.Web.Authentication.External;
using Neos.IdentityServer.MultiFactor.Resources;
using Neos.IdentityServer.MultiFactor;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Neos.IdentityServer.MultiFactor.Common;

namespace Neos.IdentityServer.MultiFactor
{
    public class AdapterPresentation2019 : BasePresentation
    {
        #region Constructors
        /// <summary>
        /// Constructor implementation
        /// </summary>
        public AdapterPresentation2019(AuthenticationProvider provider, IAuthenticationContext context)
            : base(provider, context)
        {
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation2019(AuthenticationProvider provider, IAuthenticationContext context, string message)
            : base(provider, context, message)
        {
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation2019(AuthenticationProvider provider, IAuthenticationContext context, string message, bool ismessage)
            : base(provider, context, message, ismessage)
        {
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation2019(AuthenticationProvider provider, IAuthenticationContext context, ProviderPageMode suite)
            : base(provider, context, suite)
        {
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation2019(AuthenticationProvider provider, IAuthenticationContext context, string message, ProviderPageMode suite, bool disableoptions = false)
            : base(provider, context, message, suite, disableoptions)
        {
        }
        #endregion

        #region CSS
        /// <summary>
        /// GetFormPreRenderHtmlCSS implementation
        /// </summary>
        protected override string GetFormPreRenderHtmlCSS(AuthenticationContext usercontext, bool removetag = false)
        {
            string result = string.Empty;
            if (!removetag)
                result = "<style>" + "\r\n";
            result += "\r\n";
            result += base.GetFormPreRenderHtmlCSS(usercontext, true);
            result += "\r\n";

            result += "#buttonquit {" + "\r\n";
            result += "align-items: flex-start;" + "\r\n";
            result += "background-color: rgb(232, 17, 35);" + "\r\n";
            result += "border-bottom-color: rgb(232, 17, 35);" + "\r\n";
            result += "border-bottom-style: solid;" + "\r\n";
            result += "border-bottom-width: 1px;" + "\r\n";
            result += "border-image-outset: 0px;" + "\r\n";
            result += "border-image-repeat: stretch;" + "\r\n";
            result += "border-image-slice: 100%;" + "\r\n";
            result += "border-image-source: none;" + "\r\n";
            result += "border-image-width: 1;" + "\r\n";
            result += "border-left-color: rgb(232, 17, 35);" + "\r\n";
            result += "border-left-style: solid;" + "\r\n";
            result += "border-left-width: 1px;" + "\r\n";
            result += "border-right-color: rgb(232, 17, 35);" + "\r\n";
            result += "border-right-style: solid;" + "\r\n";
            result += "border-right-width: 1px;" + "\r\n";
            result += "border-top-color: rgb(232, 17, 35);" + "\r\n";
            result += "border-top-style: solid;" + "\r\n";
            result += "border-top-width: 1px;" + "\r\n";
            result += "box-sizing: border-box;" + "\r\n";
            result += "color: rgb(255, 255, 255);" + "\r\n";
            result += "cursor: pointer;" + "\r\n";
            result += "direction: ltr;" + "\r\n";
            result += "display: inline-block;" + "\r\n";
            result += "font-family: \"Segoe UI Webfont\", -apple-system, \"Helvetica Neue\", \"Lucida Grande\", Roboto, Ebrima, \"Nirmala UI\", Gadugi, \"Segoe Xbox Symbol\", \"Segoe UI Symbol\", \"Meiryo UI\", \"Khmer UI\", Tunga, \"Lao UI\", Raavi, \"Iskoola Pota\", Latha, Leelawadee, \"Microsoft YaHei UI\", \"Microsoft JhengHei UI\", \"Malgun Gothic\", \"Estrangelo Edessa\", \"Microsoft Himalaya\", \"Microsoft New Tai Lue\", \"Microsoft PhagsPa\", \"Microsoft Tai Le\", \"Microsoft Yi Baiti\", \"Mongolian Baiti\", \"MV Boli\", \"Myanmar Text\", \"Cambria Math\";" + "\r\n";
            result += "font-size: 15px;" + "\r\n";
            result += "font-stretch: normal;" + "\r\n";
            result += "font-style: normal;" + "\r\n";
            result += "font-variant-caps: normal;" + "\r\n";
            result += "font-variant-ligatures: normal;" + "\r\n";
            result += "font-variant-numeric: normal;" + "\r\n";
            result += "font-weight: normal;" + "\r\n";
            result += "height: 36px;" + "\r\n";
            result += "letter-spacing: normal;" + "\r\n";
            result += "line-height: 25px;" + "\r\n";
            result += "margin-bottom: 0px;" + "\r\n";
            result += "margin-left: 0px;" + "\r\n";
            result += "margin-right: 0px;" + "\r\n";
            result += "margin-top: 0px;" + "\r\n";
            result += "max-width: 100%;" + "\r\n";
            result += "min-width: 165px;" + "\r\n";
            result += "overflow-x: hidden;" + "\r\n";
            result += "overflow-y: hidden;" + "\r\n";
            result += "padding-bottom: 4px;" + "\r\n";
            result += "padding-left: 12px;" + "\r\n";
            result += "padding-right: 12px;" + "\r\n";
            result += "padding-top: 4px;" + "\r\n";
            result += "position: relative;" + "\r\n";
            result += "text-align: center;" + "\r\n";
            result += "text-indent: 0px;" + "\r\n";
            result += "text-overflow: ellipsis;" + "\r\n";
            result += "text-rendering: auto;" + "\r\n";
            result += "text-shadow: none;" + "\r\n";
            result += "text-size-adjust: 100%;" + "\r\n";
            result += "touch-action: manipulation;" + "\r\n";
            result += "user-select: none;" + "\r\n";
            result += "vertical-align: middle;" + "\r\n";
            result += "white-space: nowrap;" + "\r\n";
            result += "width: 100%;" + "\r\n";
            result += "word-spacing: 0px;" + "\r\n";
            result += "writing-mode: horizontal-tb;" + "\r\n";
            result += "-webkit-appearance: none;" + "\r\n";
            result += "-webkit-rtl-ordering: logical;" + "\r\n";
            result += "-webkit-border-image: none;" + "\r\n";
            result += "}" + "\r\n";

            result += "#buttonquit:hover {" + "\r\n";
            result += "background: rgb(170, 0, 0);" + "\r\n";
            result += "border: 1px solid rgb(232, 17, 35);" + "\r\n";
            result += "}" + "\r\n";

            if (!removetag)
                result += "</style>" + "\r\n";
            else
                result += "\r\n";
            return result;
        }
        #endregion

        #region Identification
        /// <summary>
        /// GetFormPreRenderHtmlIdentification implementation
        /// </summary>
        public override string GetFormPreRenderHtmlIdentification(AuthenticationContext usercontext)
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
        public override string GetFormHtmlIdentification(AuthenticationContext usercontext)
        {
            string result = "<form method=\"post\" id=\"IdentificationForm\" autocomplete=\"off\">";
            IExternalProvider prov = RuntimeAuthProvider.GetProvider(usercontext.PreferredMethod);
            if ((prov != null) && (prov.IsUIElementRequired(usercontext, RequiredMethodElements.CodeInputRequired)))
            {
                result += "<div id=\"wizardMessage\" class=\"groupMargin\">" + prov.GetUILabel(usercontext) + "</div>";
                result += "<input id=\"totp\" name=\"totp\" type=\"password\" placeholder=\"Code\" class=\""+(UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") +"\" autofocus=\"autofocus\" /></br>";
                result += "<div class=\"fieldMargin smallText\">" + prov.GetUIMessage(usercontext) + "</div></br>";
                if (!string.IsNullOrEmpty(prov.GetUIWarningThirdPartyLabel(usercontext)) && (usercontext.IsSendBack))
                    result += "<div class=\"error smallText\">" + prov.GetUIWarningThirdPartyLabel(usercontext) + "</div></br>";
            }
            if ((prov != null) && (prov.IsUIElementRequired(usercontext, RequiredMethodElements.PinParameterRequired)))
            {
                result += "<div id=\"wizardMessage2\" class=\"groupMargin\">" + BaseExternalProvider.GetPINLabel(usercontext) + "</div>";
                result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" placeholder=\"PIN Number\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" /></br>";
                result += "<div class=\"fieldMargin smallText\">" + BaseExternalProvider.GetPINMessage(usercontext) + "</div></br>";
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
            result += GetFormHtmlMessageZone(usercontext);
            result += "</form>";
            return result;
        }
        #endregion

        #region Registration
        /// <summary>
        /// GetFormPreRenderHtmlRegistration implementation
        /// </summary>
        public override string GetFormPreRenderHtmlRegistration(AuthenticationContext usercontext)
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
        public override string GetFormHtmlRegistration(AuthenticationContext usercontext)
        {
            string result = string.Empty;

            result += "<form method=\"post\" id=\"registrationForm\" autocomplete=\"off\" onsubmit=\"return ValidateRegistration(this)\" >";
            result += "<div id=\"wizardMessage\" class=\"groupMargin\">" + Resources.GetString(ResourcesLocaleKind.Titles, "RegistrationPageTitle") + "</div>";

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
                    result += "<b><div class=\"fieldMargin\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGLabelAppKey") + "</div></b>";
                    result += "<input id=\"secretkey\" name=\"secretkey\" type=\"text\" readonly=\"true\" placeholder=\"DisplayKey\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" style=\"background-color: #F0F0F0\" value=\"" + StripDisplayKey(displaykey) + "\"/></br>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.EmailParameterRequired)))
                {
                    result += "<b><div class=\"fieldMargin\">" + prov.GetUICFGLabel(usercontext) + "</div></b>";
                    result += "<input id=\"email\" name=\"email\" type=\"text\" readonly=\"true\" placeholder=\"Personal Email Address\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" style=\"background-color: #F0F0F0\" value=\"" + usercontext.MailAddress + "\"/></br>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.PhoneParameterRequired)))
                {
                    result += "<b><div class=\"fieldMargin\">" + prov.GetUICFGLabel(usercontext) + "</div></b>";
                    result += "<input id=\"phone\" name=\"phone\" type=\"text\" readonly=\"true\" placeholder=\"Phone Number\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" style=\"background-color: #F0F0F0\" value=\"" + usercontext.PhoneNumber + "\"/></br>";
                }

                List<AvailableAuthenticationMethod> lst = prov.GetAuthenticationMethods(usercontext);
                if (lst.Count > 1)
                {
                    AuthenticationResponseKind ov = AuthenticationResponseKind.Error;
                    if (prov.AllowOverride)
                    {
                        ov = prov.GetOverrideMethod(usercontext);
                        result += "<input id=\"optiongroup\" name=\"optionitem\" type=\"radio\" value=\"Default\" checked=\"checked\" /> " + prov.GetUIDefaultChoiceLabel(usercontext) + "</br>";
                    }
                    int i = 1;
                    foreach (AvailableAuthenticationMethod met in lst)
                    {
                        if (ov != AuthenticationResponseKind.Error)
                        {
                            if (met.Method == ov)
                                result += "<input id=\"optiongroup" + i.ToString() + "\" name=\"optionitem\" type=\"radio\" value=\"" + met.Method.ToString() + "\" checked=\"checked\" /> " + prov.GetUIChoiceLabel(usercontext, met) + "</br>";
                            else
                                result += "<input id=\"optiongroup" + i.ToString() + "\" name=\"optionitem\" type=\"radio\" value=\"" + met.Method.ToString() + "\" /> " + prov.GetUIChoiceLabel(usercontext, met) + "</br>";
                        }
                        else
                            result += "<input id=\"optiongroup" + i.ToString() + "\" name=\"optionitem\" type=\"radio\" value=\"" + met.Method.ToString() + "\" /> " + prov.GetUIChoiceLabel(usercontext, met) + "</br>";
                        i++;
                    }
                }
                else if (lst.Count <= 0)
                {
                    result += "<div class=\"fieldMargin error smallText\">No options available for " + prov.Description + "</div></br>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.KeyParameterRequired)))
                {
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollopt\" name=\"enrollopt\" onclick=\"fnlinkclicked(registrationForm, 3)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollOTP") + "</a>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.EmailParameterRequired)))
                {
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollemail\" name=\"enrollemail\" onclick=\"fnlinkclicked(registrationForm, 4)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollEmail") + "</a>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.PhoneParameterRequired)))
                {
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollphone\" name=\"enrollphone\" onclick=\"fnlinkclicked(registrationForm, 5)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollPhone") + "</a>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.BiometricParameterRequired)))
                {
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollbio\" name=\"enrollbio\" onclick=\"fnlinkclicked(registrationForm, 6)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollBiometrics") + "</a>";
                }
                if (!string.IsNullOrEmpty(prov.GetAccountManagementUrl(usercontext)))
                {
                    result += "</br>";
                    result += "<input type=\"checkbox\" id=\"manageaccount\" name=\"manageaccount\" > " + prov.GetUIAccountManagementLabel(usercontext)+ "/>";
                }
                result += "<br />";
            }
            else
            {   // PIN Code
                if (RuntimeAuthProvider.IsPinCodeRequired(usercontext))
                {
                    result += "<b><div class=\"fieldMargin\">" + BaseExternalProvider.GetPINLabel(usercontext) + " : </div></b>";
                    if (usercontext.PinCode > 0)
                        result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" readonly=\"true\" placeholder=\"Pin Code\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" style=\"background-color: #F0F0F0\" value=\"" + usercontext.PinCode + "\"/></br>";
                    else
                        result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" readonly=\"true\" placeholder=\"PIN Code\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" style=\"background-color: #F0F0F0\" value=\"" + Provider.Config.DefaultPin + "\"/></br>";
                    result += "</br>";
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollpin\" name=\"enrollpin\" onclick=\"fnlinkclicked(registrationForm, 7)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollPinCode") + "</a>";
                    result += "</br>";
                }
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
            result += GetFormHtmlMessageZone(usercontext);
            result += "</form>";
            return result;
        }
        #endregion

        #region Invitation
        /// <summary>
        /// GetFormPreRenderHtmlInvitation implementation
        /// </summary>
        public override string GetFormPreRenderHtmlInvitation(AuthenticationContext usercontext)
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

            result += "function fnlinkclicked(frm, id)" + "\r\n";
            result += "{" + "\r\n";
            result += "   var lnk = document.getElementById('btnclicked');" + "\r\n";
            result += "   lnk.value = id;" + "\r\n";
            result += "   frm.submit();" + "\r\n";
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

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlInvitation implementation
        /// </summary>
        public override string GetFormHtmlInvitation(AuthenticationContext usercontext)
        {
            string result = string.Empty;

            result += "<form method=\"post\" id=\"invitationForm\" autocomplete=\"off\" onsubmit=\"return ValidateInvitation(this)\" >";
            result += "<div id=\"wizardMessage\" class=\"groupMargin\">" + Resources.GetString(ResourcesLocaleKind.Titles, "InvitationPageTitle") + "</div>";

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
                    result += "<b><div class=\"fieldMargin\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGLabelAppKey") + "</div></b>";
                    result += "<input id=\"secretkey\" name=\"secretkey\" type=\"text\" readonly=\"true\" placeholder=\"DisplayKey\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" style=\"background-color: #F0F0F0\" value=\"" + StripDisplayKey(displaykey) + "\"/></br>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.EmailParameterRequired)))
                {
                    result += "<b><div class=\"fieldMargin\">" + prov.GetUICFGLabel(usercontext) + "</div></b>";
                    result += "<input id=\"email\" name=\"email\" type=\"text\" readonly=\"true\" placeholder=\"Personal Email Address\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" style=\"background-color: #F0F0F0\" value=\"" + usercontext.MailAddress + "\"/></br>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.PhoneParameterRequired)))
                {
                    result += "<b><div class=\"fieldMargin\">" + prov.GetUICFGLabel(usercontext) + "</div></b>";
                    result += "<input id=\"phone\" name=\"phone\" type=\"text\" readonly=\"true\" placeholder=\"Phone Number\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" style=\"background-color: #F0F0F0\" value=\"" + usercontext.PhoneNumber + "\"/></br>";
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
                                result += "<input id=\"optiongroup" + i.ToString() + "\" name=\"optionitem\" type=\"radio\" value=\"" + met.Method.ToString() + "\" checked=\"checked\" /> " + prov.GetUIChoiceLabel(usercontext, met) + "</br>";
                            else
                                result += "<input id=\"optiongroup" + i.ToString() + "\" name=\"optionitem\" type=\"radio\" value=\"" + met.Method.ToString() + "\" /> " + prov.GetUIChoiceLabel(usercontext, met) + "</br>";
                        }
                        else
                            result += "<input id=\"optiongroup" + i.ToString() + "\" name=\"optionitem\" type=\"radio\" value=\"" + met.Method.ToString() + "\" /> " + prov.GetUIChoiceLabel(usercontext, met) + "</br>";
                        i++;
                    }
                }
                else if (lst.Count <= 0)
                {
                    result += "<div class=\"fieldMargin error smallText\">No options available for " + prov.Description + "</div></br>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.KeyParameterRequired)))
                {
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollopt\" name=\"enrollopt\" onclick=\"fnlinkclicked(invitationForm, 3)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollOTP") + "</a>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.EmailParameterRequired)))
                {
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollemail\" name=\"enrollemail\" onclick=\"fnlinkclicked(invitationForm, 4)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollEmail") + "</a>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.PhoneParameterRequired)))
                {
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollphone\" name=\"enrollphone\" onclick=\"fnlinkclicked(invitationForm, 5)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollPhone") + "</a>";
                }
                if ((prov.IsUIElementRequired(usercontext, RequiredMethodElements.BiometricParameterRequired)))
                {
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollbio\" name=\"enrollbio\" onclick=\"fnlinkclicked(invitationForm, 6)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollBiometrics") + "</a>";
                }
                if (!string.IsNullOrEmpty(prov.GetAccountManagementUrl(usercontext)))
                {
                    result += "<input type=\"checkbox\" id=\"manageaccount\" name=\"manageaccount\" > " + prov.GetUIAccountManagementLabel(usercontext) + "<br />";
                }
                result += "<br />";
            }
            else
            {
                if (RuntimeAuthProvider.IsPinCodeRequired(usercontext))
                {
                    result += "<b><div class=\"fieldMargin smallText\">" + BaseExternalProvider.GetPINLabel(usercontext) + " : </div></b>";
                    if (usercontext.PinCode > 0)
                        result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" readonly=\"true\" placeholder=\"Pin Code\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" style=\"background-color: #F0F0F0\" value=\"" + usercontext.PinCode + "\"/></br>";
                    else
                        result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" readonly=\"true\" placeholder=\"PIN Code\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" style=\"background-color: #F0F0F0\" value=\"" + Provider.Config.DefaultPin + "\"/></br>";
                    result += "</br>";
                    result += "<a class=\"actionLink\" href=\"#\" id=\"enrollpin\" name=\"enrollpin\" onclick=\"fnlinkclicked(invitationForm, 7)\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollPinCode") + "</a>";
                    result += "</br>";
                }
            }
            result += GetPartHtmlSelectMethod2(usercontext);

            if (!Provider.Config.UserFeatures.IsMFARequired())
                result += "<input id=\"disablemfa\" type=\"checkbox\" name=\"disablemfa\" /> " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMDisableMFA") + "</br>";

            result += "</br>";
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";

            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"0\" />";

            result += "<table><tr>";
            result += "<td>";
            result += "<input id=\"saveButton\" type=\"submit\" class=\"submit\" name=\"continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlINSRequest") + "\" onclick=\"fnbtnclicked(1)\" />";
            result += "</td>";
            result += "<td style=\"width: 15px\" />";
            result += "<td>";
            result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(2)\" />";
            result += "</td>";
            result += "</tr></table>";
            result += GetFormHtmlMessageZone(usercontext);
            result += "</form>";
            return result;
        }
        #endregion

        #region SelectOptions
        /// <summary>
        /// GetFormPreRenderHtmlSelectOptions implementation
        /// </summary>
        public override string GetFormPreRenderHtmlSelectOptions(AuthenticationContext usercontext)
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
        public override string GetFormHtmlSelectOptions(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            bool showdonut = false;
            result += "<form method=\"post\" id=\"selectoptionsForm\" autocomplete=\"off\" >";
            result += "</br>";
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
                bool WantPin = false;
                if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.OTPLinkRequired))
                {
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Code);
                    if (prov.PinRequired)
                        WantPin = true;
                    if (Provider.HasStrictAccessToOptions(prov))
                        result += "<a class=\"actionLink\" href=\"#\" id=\"enrollopt\" name=\"enrollopt\" onclick=\"return SetLinkTitle(selectoptionsForm, '3')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollOTP") + "</a>";
                }
                if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.BiometricLinkRequired))
                {
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics);
                    if (prov.PinRequired)
                        WantPin = true;
                    if (Provider.HasStrictAccessToOptions(prov))
                        result += "<a class=\"actionLink\" href=\"#\" id=\"enrollbio\" name=\"enrollbio\" onclick=\"return SetLinkTitle(selectoptionsForm, '4')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollBiometric") + "</a>";
                }
                if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.EmailLinkRequired))
                {
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Email);
                    if (prov.PinRequired)
                        WantPin = true;
                    if (Provider.HasStrictAccessToOptions(prov))
                        result += "<a class=\"actionLink\" href=\"#\" id=\"enrollemail\" name=\"enrollemail\" onclick=\"return SetLinkTitle(selectoptionsForm, '5')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollEmail") + "</a>";
                }
                if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.PhoneLinkRequired))
                {
                    prov = RuntimeAuthProvider.GetProvider(PreferredMethod.External);
                    if (prov.PinRequired)
                        WantPin = true;
                    if (Provider.HasStrictAccessToOptions(prov))
                        result += "<a class=\"actionLink\" href=\"#\" id=\"enrollphone\" name=\"enrollphone\" onclick=\"return SetLinkTitle(selectoptionsForm, '6')\"; style=\"cursor: pointer;\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlEnrollPhone") + "</a>";
                }
                if (RuntimeAuthProvider.IsUIElementRequired(usercontext, RequiredMethodElements.PinLinkRequired))
                {
                    if (WantPin)
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
            result += GetFormHtmlMessageZone(usercontext);
            result += "</form>";
            return result;
        }
        #endregion

        #region ChooseMethod
        /// <summary>
        /// GetFormPreRenderHtmlChooseMethod implementation
        /// </summary>
        public override string GetFormPreRenderHtmlChooseMethod(AuthenticationContext usercontext)
        {
            string result = "<script type='text/javascript'>" + "\r\n";
            result += "//<![CDATA[" + "\r\n";

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
        public override string GetFormHtmlChooseMethod(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            result += "<form method=\"post\" id=\"ChooseMethodForm\" autocomplete=\"off\" \" >";
            result += "</br><b><div id=\"pwdTitle\" class=\"groupMargin\">" + Resources.GetString(ResourcesLocaleKind.Titles, "MustUseCodePageTitle") + "</div></b></br>";
            PreferredMethod method = GetMethod4FBUsers(usercontext);
            if (RuntimeAuthProvider.IsProviderAvailableForUser(usercontext, PreferredMethod.Code))
            {
                result += "<input id=\"opt1\" name=\"opt\" type=\"radio\" value=\"0\" onchange=\"ChooseMethodChanged()\" " + (((method == PreferredMethod.Code) || (method == PreferredMethod.Choose)) ? "checked=\"checked\"> " : "> ") + RuntimeAuthProvider.GetProvider(PreferredMethod.Code).GetUIChoiceLabel(usercontext) + "<br /><br/>";
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
                result += "<input id=\"remember\" type=\"checkbox\" name=\"Remember\" checked=\"true\" > " + Resources.GetString(ResourcesLocaleKind.Html, "HtmlCHOOSEOptionRemember") + "<br /><br />";
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
            result += GetFormHtmlMessageZone(usercontext);
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
        public override string GetFormPreRenderHtmlChangePassword(AuthenticationContext usercontext)
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
                result += "     var oldpwd = document.getElementById('oldpwdedit');" + "\r\n";
                result += "     var newpwd = document.getElementById('newpwdedit');" + "\r\n";
                result += "     var cnfpwd = document.getElementById('cnfpwdedit');" + "\r\n";
                result += "     if (oldpwd.value == \"\")" + "\r\n";
                result += "     {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidPassActualError") + "\";" + "\r\n";
                result += "         oldpwd.focus();" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "     }" + "\r\n";
                result += "     if (newpwd.value == \"\")" + "\r\n";
                result += "     {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidPassNewError") + "\";" + "\r\n";
                result += "         newpwd.focus();" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "     }" + "\r\n";
                result += "     if (cnfpwd.value == \"\")" + "\r\n";
                result += "     {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidConfirmNewPassError") + "\";" + "\r\n";
                result += "         cnfpwd.focus();" + "\r\n";
                result += "         return false;" + "\r\n";
                result += "     }" + "\r\n";
                result += "     if (cnfpwd.value != newpwd.value)" + "\r\n";
                result += "     {" + "\r\n";
                result += "         err.innerHTML = \"" + Resources.GetString(ResourcesLocaleKind.Validation, "ValidNewPassError") + "\";" + "\r\n";
                result += "         cnfpwd.focus();" + "\r\n";
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

                result += "//]]>" + "\r\n";
                result += "</script>" + "\r\n";
            }
            return result;
        }

        /// <summary>
        /// GetFormHtmlChangePassword implementation
        /// </summary>
        public override string GetFormHtmlChangePassword(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            if (Provider.Config.CustomUpdatePassword)
            {
                result += "<form method=\"post\" id=\"passwordForm\" autocomplete=\"off\" onsubmit=\"return ValidChangePwd(this)\" >";
                result += "<div id=\"wizardMessage\" class=\"groupMargin\">" + Resources.GetString(ResourcesLocaleKind.Titles, "PasswordPageTitle") + "</div>";
                result += "</br>";

                result += "<b><div id=\"oldpwd\" class=\"groupMargin\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDLabelActual") + "</div></b>";
                result += "<input id=\"oldpwdedit\" name=\"oldpwdedit\" type=\"password\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\"/></br>";

                result += "<b><div id=\"newpwd\" class=\"groupMargin\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDLabelNew") + "</div></b>";
                result += "<input id=\"newpwdedit\" name=\"newpwdedit\" type=\"password\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\"/></br>";

                result += "<b><div id=\"cnfpwd\" class=\"groupMargin\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDLabelNewConfirmation") + "</div></b>";
                result += "<input id=\"cnfpwdedit\" name=\"cnfpwdedit\" type=\"password\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\"/></br></br>";

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
                result += GetFormHtmlMessageZone(usercontext);
                result += "</form>";
            }
            return result;
        }
        #endregion

        #region Bypass
        /// <summary>
        /// GetFormPreRenderHtmlBypass implementation
        /// </summary>
        public override string GetFormPreRenderHtmlBypass(AuthenticationContext usercontext)
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
                result += "   window.open('" + usercontext.AccountManagementUrl + "', '_blank');" + "\r\n";
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
        public override string GetFormHtmlBypass(AuthenticationContext usercontext)
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
                result += "<div class=\"fieldMargin smallText\">" + BaseExternalProvider.GetPINLabel(usercontext) + " : </div>";
                result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" placeholder=\"PIN Number\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" /></br>";
                result += "<div class=\"fieldMargin smallText\">" + BaseExternalProvider.GetPINMessage(usercontext) + "</div></br>";
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
        public override string GetFormPreRenderHtmlLocking(AuthenticationContext usercontext)
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
        public override string GetFormHtmlLocking(AuthenticationContext usercontext)
        {
            string result = "<form method=\"post\" id=\"lockingForm\" autocomplete=\"off\">";
            result += "</br>";
            result += "<input id=\"context\" type=\"hidden\" name=\"Context\" value=\"%Context%\"/>";
            result += "<input id=\"authMethod\" type=\"hidden\" name=\"AuthMethod\" value=\"%AuthMethod%\"/>";
            result += "<input id=\"btnclicked\" type=\"hidden\" name=\"btnclicked\" value=\"1\" />";

            if (!usercontext.IsRegistered)
            {
                if (Provider.Config.UserFeatures.IsRegistrationRequired())
                {
                    result += "<div id=\"pwdMessage\" class=\"groupMargin\">" + Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountNotEnabled") + "</div></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"buttonquit\" type=\"submit\"  value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMQuit") + "\" onClick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"GoToRegButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMGotoInscription") + "\" onClick=\"fnbtnclicked(2)\"/>";
                    result += "</td>";
                    result += "</tr></table>";            
                }
                else if (Provider.Config.UserFeatures.IsRegistrationAllowed() && (Provider.Config.AdvertisingDays.OnFire))
                {
                    result += "<div id=\"pwdMessage\" class=\"groupMargin\">" + Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized") + "</div></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"continueButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMConnexion") + "\" onClick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"GoToRegButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMGoToRegistration") + "\" onClick=\"fnbtnclicked(3)\"/>";
                    result += "</td>";
                    result += "</tr></table>";            
                }
            }
            else if (!usercontext.Enabled)
            {
                if (Provider.Config.UserFeatures.IsMFAAllowed() && (Provider.Config.AdvertisingDays.OnFire))
                {
                    result += "<div id=\"pwdMessage\" class=\"groupMargin\">" + Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAuthorized") + "</div></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"continueButton\" type=\"submit\" class=\"submit\" name=\"Continue\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMConnexion") + "\" onClick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"GoToRegButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMGoToRegistration") + "\" onClick=\"fnbtnclicked(4)\"/>";
                }
                if (Provider.Config.UserFeatures.IsMFARequired())
                {
                    result += "<div id=\"pwdMessage\" class=\"groupMargin\">" + Resources.GetString(ResourcesLocaleKind.Errors, "ErrorAccountAdminNotEnabled") + "</div></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"buttonquit\" type=\"submit\"  value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMQuit") + "\" onClick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "</td>";
                    result += "</tr></table>";            
                }
            }
            else
            {
                result += GetFormHtmlMessageZone(usercontext);
                result += "<table><tr>";
                result += "<td>";
                result += "<input id=\"buttonquit\" type=\"submit\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMQuit") + "\" onClick=\"fnbtnclicked(1)\" />";
                result += "</td>";
                result += "<td style=\"width: 15px\" />";
                result += "<td>";
                result += "</td>";
                result += "</tr></table>";            

            }
            result += "</form>";
            return result;
        }
        #endregion

        #region CodeRequest
        /// <summary>
        /// GetFormPreRenderHtmlSendCodeRequest implementation
        /// </summary>
        public override string GetFormPreRenderHtmlSendCodeRequest(AuthenticationContext usercontext)
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
        public override string GetFormHtmlSendCodeRequest(AuthenticationContext usercontext)
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

            if (usercontext.IsRemote)
            {
                IExternalProvider prov = RuntimeAuthProvider.GetProvider(usercontext.PreferredMethod);
                result += GetPartHtmlDonut();
                result += "<div class=\"fieldMargin smallText\">" + prov.GetUIMessage(usercontext) + "</div></br>";
                if (!string.IsNullOrEmpty(prov.GetUIWarningInternetLabel(usercontext)) && (usercontext.IsRemote))
                    result += "<div class=\"error smallText\">" + prov.GetUIWarningInternetLabel(usercontext) + "</div>";
                if (!string.IsNullOrEmpty(prov.GetUIWarningThirdPartyLabel(usercontext)) && (usercontext.IsTwoWay))
                    result += "<div class=\"error smallText\">" + prov.GetUIWarningThirdPartyLabel(usercontext) + "</div>";
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
            result += GetFormHtmlMessageZone(usercontext);

            result += "</form>";
            return result;
        }
        #endregion

        #region InvitationRequest
        /// <summary>
        /// GetFormPreRenderHtmlSendAdministrativeRequest implementation
        /// </summary>
        public override string GetFormPreRenderHtmlSendAdministrativeRequest(AuthenticationContext usercontext)
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
        public override string GetFormHtmlSendAdministrativeRequest(AuthenticationContext usercontext)
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
            result += "<div class=\"fieldMargin smallText\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlINSWaitRequest") + "</div>";

            IExternalAdminProvider admprov = RuntimeAuthProvider.GetAdministrativeProvider(Provider.Config);
            result += GetPartHtmlDonut();
            if (admprov != null)
            {
                result += "<div class=\"fieldMargin smallText\">" + admprov.GetUIInscriptionMessageLabel(usercontext) + "</div></br>";
                if (!string.IsNullOrEmpty(admprov.GetUIWarningInternetLabel(usercontext)) && (usercontext.IsRemote))
                    result += "<div class=\"error smallText\">" + admprov.GetUIWarningInternetLabel(usercontext) + "</div>";
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
        public override string GetFormPreRenderHtmlSendKeyRequest(AuthenticationContext usercontext)
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
        public override string GetFormHtmlSendKeyRequest(AuthenticationContext usercontext)
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
            result += "<div class=\"fieldMargin smallText\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlINSWaitRequest") + "</div>";

            IExternalAdminProvider admprov = RuntimeAuthProvider.GetAdministrativeProvider(Provider.Config);

            result += GetPartHtmlDonut();
            if (admprov != null)
            {
                result += "<div class=\"fieldMargin smallText\">" + admprov.GetUISecretKeyMessageLabel(usercontext) + "</div></br>";
                if (!string.IsNullOrEmpty(admprov.GetUIWarningInternetLabel(usercontext)) && (usercontext.IsRemote))
                    result += "<div class=\"error smallText\">" + admprov.GetUIWarningInternetLabel(usercontext) + "</div>";
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
        public override string GetFormPreRenderHtmlEnrollOTP(AuthenticationContext usercontext)
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

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlEnrollOTP method implmentation
        /// </summary>
        public override string GetFormHtmlEnrollOTP(AuthenticationContext usercontext)
        {
            string result = "<form method=\"post\" id=\"enrollotpForm\" autocomplete=\"off\" \">";
            IExternalProvider prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Code);
            switch (usercontext.WizPageID)
            {
                case 0:
                    result += "<div id=\"wizardMessage\" class=\"groupMargin\">" + prov.GetWizardUILabel(usercontext) + "</div>";
                    result += "<div class=\"fieldMargin smallText\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWREGOTP") + "</div></br>";
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
                        result += "<div class=\"fieldMargin smallText\"><label for=\"\"><a href=\"https://www.google.fr/search?q=Authenticator+apps&oq=Authenticator+apps\" target=\"blank\">Or Search Google</a>" + "</div>";

                    result += "<p class=\"error\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWREGOTPWarning") + "</p></br>";

                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"nextButton\" type=\"submit\" class=\"submit\" name=\"nextButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMRecordNewKey") + "\" onclick=\"fnbtnclicked(2)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    if ((usercontext.UIMode != ProviderPageMode.EnrollOTPForce) || ((usercontext.UIMode == ProviderPageMode.EnrollOTPForce) && (prov.ForceEnrollment != ForceWizardMode.Strict)))
                        result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    // result += GetFormHtmlMessageZone(usercontext);
                    break;
                case 1: // Always Reset the Key
                    KeysManager.NewKey(usercontext.UPN);
                    string displaykey = KeysManager.EncodedKey(usercontext.UPN);

                    result += "<div id=\"wizardMessage\" class=\"groupMargin\">" + prov.GetWizardUILabel(usercontext) + "</div>";
                    result += "<div class=\"fieldMargin smallText\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWRQRCode") + "</div></br>";
                    result += "<input id=\"secretkey\" name=\"secretkey\" type=\"text\" readonly=\"true\" placeholder=\"DisplayKey\" class=\"text fullWidth\" style=\"background-color: #F0F0F0\" value=\"" + StripDisplayKey(displaykey) + "\"/></br></br>";
                    result += "<p style=\"text-align:center\"><img id=\"qr\" src=\"data:image/png;base64," + Provider.GetQRCodeString(usercontext) + "\"/></p></br>";
                    result += "<input id=\"nextButton\" type=\"submit\" class=\"submit\" name=\"nextButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWVERIFYOTP") + "\" onclick=\"fnbtnclicked(3)\" />";
                    break;
                case 2: // Code verification
                    result += "<div id=\"wizardMessage\" class=\"groupMargin\">" + prov.GetUILabel(usercontext) + "</div>";
                    result += "<input id=\"totp\" name=\"totp\" type=\"password\" placeholder=\"Code\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" autofocus=\"autofocus\" /></br>";
                    result += "<div class=\"fieldMargin smallText\">" + prov.GetUIMessage(usercontext) + "</div></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"checkButton\" type=\"submit\" class=\"submit\" name=\"checkButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMCheck") + "\" onclick=\"fnbtnclicked(4)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    if ((usercontext.UIMode != ProviderPageMode.EnrollOTPForce) || ((usercontext.UIMode == ProviderPageMode.EnrollOTPForce) && (prov.ForceEnrollment != ForceWizardMode.Strict)))
                        result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    break;
                case 3: // Successfull test
                    result += "</br>";
                    result += "<p style=\"text-align:center\"><img id=\"msgreen\" src=\"data:image/png;base64," + Convert.ToBase64String(images.cvert.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<input id=\"finishButton\" type=\"submit\" class=\"submit\" name=\"finishButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPOK") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</br>";
                    result += GetFormHtmlMessageZone(usercontext);
                    break;
                case 4: // Wrong result test
                    result += "</br>";
                    result += "<p style=\"text-align:center\"><img id=\"msred\" src=\"data:image/png;base64," + Convert.ToBase64String(images.crouge.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"priorButton\" type=\"submit\" class=\"submit\" name=\"priorButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPPRIOR") + "\" onclick=\"fnbtnclicked(2)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    if ((usercontext.UIMode != ProviderPageMode.EnrollOTPForce) || ((usercontext.UIMode == ProviderPageMode.EnrollOTPForce) && (prov.ForceEnrollment != ForceWizardMode.Strict)))
                        result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    result += "</br>";
                    result += GetFormHtmlMessageZone(usercontext);
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
        public override string GetFormPreRenderHtmlEnrollEmail(AuthenticationContext usercontext)
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

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";

            return result;
        }

        /// <summary>
        /// GetFormHtmlEnrollEmail method implmentation
        /// </summary>
        public override string GetFormHtmlEnrollEmail(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            if (usercontext.WizPageID == 1)
            {
                result = "<script type=\"text/javascript\">" + "\r\n";
                result += "if (window.addEventListener)" + "\r\n";
                result += "{" + "\r\n";
                result += "   window.addEventListener('load', OnRefreshPost, false);" + "\r\n";
                result += "}" + "\r\n";
                result += "else if (window.attachEvent)" + "\r\n";
                result += "{" + "\r\n";
                result += "   window.attachEvent('onload', OnRefreshPost);" + "\r\n";
                result += "}" + "\r\n";
                result += "</script>" + "\r\n";
            }

            result += "<form method=\"post\" id=\"enrollemailForm\" autocomplete=\"off\" onsubmit=\"return ValidateRegistration(this)\" >";

            IExternalProvider prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Email);
            prov.GetAuthenticationContext(usercontext);
            switch (usercontext.WizPageID)
            {
                case 0: // Get User email
                    result += "<div id=\"wizardMessage\" class=\"groupMargin\">" + prov.GetWizardUILabel(usercontext) + "</div>";
                    result += "<div class=\"fieldMargin smallText\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWREmail") + "</div></br>";
                    result += "<input id=\"email\" name=\"email\" type=\"text\" placeholder=\"Personal Email Address\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" value=\"" + usercontext.MailAddress + "\"/></br>";
                    result += "</br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"nextButton\" type=\"submit\" class=\"submit\" name=\"nextButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWVERIFYOTP") + "\" onclick=\"fnbtnclicked(2)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    if ((usercontext.UIMode != ProviderPageMode.EnrollEmailForce) || ((usercontext.UIMode == ProviderPageMode.EnrollEmailForce) && (prov.ForceEnrollment != ForceWizardMode.Strict)))
                        result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    result += GetFormHtmlMessageZone(usercontext);
                    break;
                case 1:
                    result += GetPartHtmlDonut();
                    result += "<div class=\"fieldMargin smallText\">" + prov.GetUIMessage(usercontext) + "</div></br>";
                    if (!string.IsNullOrEmpty(prov.GetUIWarningInternetLabel(usercontext)) && (usercontext.IsRemote))
                        result += "<div class=\"error smallText\">" + prov.GetUIWarningInternetLabel(usercontext) + "</div>";
                    if (!string.IsNullOrEmpty(prov.GetUIWarningThirdPartyLabel(usercontext)) && (usercontext.IsTwoWay))
                        result += "<div class=\"error smallText\">" + prov.GetUIWarningThirdPartyLabel(usercontext) + "</div>";
                    result += "<br />";
                    break;
                case 2: // Code verification
                    result += "<div id=\"wizardMessage\" class=\"groupMargin\">" + prov.GetUILabel(usercontext) + "</div>";
                    result += "<input id=\"totp\" name=\"totp\" type=\"password\" placeholder=\"Code\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" autofocus=\"autofocus\" /></br>";
                    result += "<div class=\"fieldMargin smallText\">" + prov.GetUIMessage(usercontext) + "</div></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"checkButton\" type=\"submit\" class=\"submit\" name=\"checkButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMCheck") + "\" onclick=\"fnbtnclicked(4)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    if ((usercontext.UIMode != ProviderPageMode.EnrollEmailForce) || ((usercontext.UIMode == ProviderPageMode.EnrollEmailForce) && (prov.ForceEnrollment != ForceWizardMode.Strict)))
                        result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    break;
                case 3: // Successfull test
                    result += "</br>";
                    result += "<p style=\"text-align:center\"><img id=\"msgreen\" src=\"data:image/png;base64," + Convert.ToBase64String(images.cvert.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<input id=\"finishButton\" type=\"submit\" class=\"submit\" name=\"finishButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPOK") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</br>";
                    result += GetFormHtmlMessageZone(usercontext);
                    break;
                case 4: // Wrong result test
                    result += "</br>";
                    result += "<p style=\"text-align:center\"><img id=\"msred\" src=\"data:image/png;base64," + Convert.ToBase64String(images.crouge.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"priorButton\" type=\"submit\" class=\"submit\" name=\"priorButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPPRIOR") + "\" onclick=\"fnbtnclicked(0)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    if ((usercontext.UIMode != ProviderPageMode.EnrollEmailForce) || ((usercontext.UIMode == ProviderPageMode.EnrollEmailForce) && (prov.ForceEnrollment != ForceWizardMode.Strict)))
                        result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    result += "</br>";
                    result += GetFormHtmlMessageZone(usercontext);
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
        public override string GetFormPreRenderHtmlEnrollPhone(AuthenticationContext usercontext)
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


            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlEnrollPhone method implmentation
        /// </summary>
        public override string GetFormHtmlEnrollPhone(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            if (usercontext.WizPageID == 1)
            {
                result += "<script type=\"text/javascript\">" + "\r\n";
                result += "if (window.addEventListener)" + "\r\n";
                result += "{" + "\r\n";
                result += "   window.addEventListener('load', OnRefreshPost, false);" + "\r\n";
                result += "}" + "\r\n";
                result += "else if (window.attachEvent)" + "\r\n";
                result += "{" + "\r\n";
                result += "   window.attachEvent('onload', OnRefreshPost);" + "\r\n";
                result += "}" + "\r\n";
                result += "</script>" + "\r\n";
            }

            result += "<form method=\"post\" id=\"enrollphoneForm\" autocomplete=\"off\" onsubmit=\"return ValidateRegistration(this)\" >";

            IExternalProvider prov = RuntimeAuthProvider.GetProvider(PreferredMethod.External);
            prov.GetAuthenticationContext(usercontext);
            switch (usercontext.WizPageID)
            {
                case 0: // Get User Phone number
                    result += "<div id=\"wizardMessage\" class=\"groupMargin\">" + prov.GetWizardUILabel(usercontext) + "</div>";
                    result += "<div class=\"fieldMargin smallText\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWRPhone") + "</div></br>";
                    result += "<input id=\"phone\" name=\"phone\" type=\"text\" placeholder=\"Personal Phone number\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" value=\"" + usercontext.PhoneNumber + "\"/></br>";
                    result += "</br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"nextButton\" type=\"submit\" class=\"submit\" name=\"nextButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWVERIFYOTP") + "\" onclick=\"fnbtnclicked(2)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    if ((usercontext.UIMode != ProviderPageMode.EnrollPhoneForce) || ((usercontext.UIMode == ProviderPageMode.EnrollPhoneForce) && (prov.ForceEnrollment != ForceWizardMode.Strict)))
                        result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    result += GetFormHtmlMessageZone(usercontext);
                    break;
                case 1:
                    result += GetPartHtmlDonut();
                    result += "<div class=\"fieldMargin smallText\">" + prov.GetUIMessage(usercontext) + "</div></br>";
                    if (!string.IsNullOrEmpty(prov.GetUIWarningInternetLabel(usercontext)) && (usercontext.IsRemote))
                        result += "<div class=\"error smallText\">" + prov.GetUIWarningInternetLabel(usercontext) + "</div>";
                    if (!string.IsNullOrEmpty(prov.GetUIWarningThirdPartyLabel(usercontext)) && (usercontext.IsTwoWay))
                        result += "<div class=\"error smallText\">" + prov.GetUIWarningThirdPartyLabel(usercontext) + "</div>";
                    result += "<br />";
                    break;
                case 2: // Code verification If One-Way
                    result += "<div id=\"wizardMessage\" class=\"groupMargin\">" + prov.GetUILabel(usercontext) + "</div>";
                    result += "<input id=\"totp\" name=\"totp\" type=\"password\" placeholder=\"Code\" class=\""+(UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") +"\" autofocus=\"autofocus\" /></br>";
                    result += "<div class=\"fieldMargin smallText\">" + prov.GetUIMessage(usercontext) + "</div></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"checkButton\" type=\"submit\" class=\"submit\" name=\"checkButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMCheck") + "\" onclick=\"fnbtnclicked(4)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    if ((usercontext.UIMode != ProviderPageMode.EnrollPhoneForce) || ((usercontext.UIMode == ProviderPageMode.EnrollPhoneForce) && (prov.ForceEnrollment != ForceWizardMode.Strict)))
                        result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    break;
                case 3: // Successfull test
                    result += "</br>";
                    result += "<p style=\"text-align:center\"><img id=\"msgreen\" src=\"data:image/png;base64," + Convert.ToBase64String(images.cvert.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<input id=\"finishButton\" type=\"submit\" class=\"submit\" name=\"finishButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPOK") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</br>";
                    result += GetFormHtmlMessageZone(usercontext);
                    break;
                case 4: // Wrong result test
                    result += "</br>";
                    result += "<p style=\"text-align:center\"><img id=\"msred\" src=\"data:image/png;base64," + Convert.ToBase64String(images.crouge.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"priorButton\" type=\"submit\" class=\"submit\" name=\"priorButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPPRIOR") + "\" onclick=\"fnbtnclicked(0)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    if ((usercontext.UIMode != ProviderPageMode.EnrollOTPForce) || ((usercontext.UIMode == ProviderPageMode.EnrollOTPForce) && (prov.ForceEnrollment != ForceWizardMode.Strict)))
                        result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    result += "</br>";
                    result += GetFormHtmlMessageZone(usercontext);
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
        public override string GetFormPreRenderHtmlEnrollBio(AuthenticationContext Context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetFormHtmlEnrollBio method implementation
        /// </summary>
        public override string GetFormHtmlEnrollBio(AuthenticationContext Context)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region EnrollPINCode
        /// <summary>
        /// GetFormPreRenderHtmlEnrollPinCode method implementation
        /// </summary>
        public override string GetFormPreRenderHtmlEnrollPinCode(AuthenticationContext usercontext)
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

            result += "//]]>" + "\r\n";
            result += "</script>" + "\r\n";
            return result;
        }

        /// <summary>
        /// GetFormHtmlEnrollEmail method implmentation
        /// </summary>
        public override string GetFormHtmlEnrollPinCode(AuthenticationContext usercontext)
        {
            string result = "<form method=\"post\" id=\"enrollPinForm\" autocomplete=\"off\" onsubmit=\"return ValidateRegistration(this)\" >";

            switch (usercontext.WizPageID)
            {
                case 0: // Get User Pin
                    result += "<div id=\"wizardMessage\" class=\"groupMargin\">" + BaseExternalProvider.GetPINWizardUILabel(usercontext) + "</div>";
                    result += "<div class=\"fieldMargin smallText\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWRPinCode") + "</div></br>";
                    if (usercontext.PinCode <= 0)
                        result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" placeholder=\"PIN Number\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" value=\"" + Provider.Config.DefaultPin + "\"/></br>";
                    else
                        result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" placeholder=\"PIN Number\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" value=\"" + usercontext.PinCode + "\"/></br>";
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
                    result += GetFormHtmlMessageZone(usercontext);
                    break;
                case 2: // Code verification
                    result += "<div id=\"wizardMessage\" class=\"groupMargin\">" + BaseExternalProvider.GetPINLabel(usercontext) + "</div>";
                    result += "<input id=\"pincode\" name=\"pincode\" type=\"password\" placeholder=\"PIN Code\" class=\"" + (UseUIPaginated ? "text textPaginated fullWidth" : "text fullWidth") + "\" autofocus=\"autofocus\" /></br>";
                    result += "<div class=\"fieldMargin smallText\">" + BaseExternalProvider.GetPINMessage(usercontext) + "</div></br>";
                    result += "<table><tr>";
                    result += "<td>";
                    result += "<input id=\"checkButton\" type=\"submit\" class=\"submit\" name=\"checkButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlUIMCheck") + "\" onclick=\"fnbtnclicked(3)\" />";
                    result += "</td>";
                    result += "<td style=\"width: 15px\" />";
                    result += "<td>";
                    result += "<input id=\"mfa-cancelButton\" type=\"submit\" class=\"submit\" name=\"cancel\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlPWDCancel") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</td>";
                    result += "</tr></table>";
                    break;
                case 3: // Successfull test
                    result += "</br>";
                    result += "<p style=\"text-align:center\"><img id=\"msgreen\" src=\"data:image/png;base64," + Convert.ToBase64String(images.cvert.ToByteArray(ImageFormat.Jpeg)) + "\"/></p></br></br>";
                    result += "<input id=\"finishButton\" type=\"submit\" class=\"submit\" name=\"finishButton\" value=\"" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelVERIFYOTPOK") + "\" onclick=\"fnbtnclicked(1)\" />";
                    result += "</br>";
                    result += GetFormHtmlMessageZone(usercontext);
                    break;
                case 4: // Wrong result test
                    result += "</br>";
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
                    result += "</br>";
                    result += GetFormHtmlMessageZone(usercontext);
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
            result += "<b><div class=\"fieldMargin\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGAccessPageMethods") + "</div></b>";
            result += "<select id=\"selectpage\" name=\"selectpage\" role=\"combobox\"  required=\"required\" contenteditable=\"false\" class=\"text fullWidth\" onchange=\"this.form.submit()\">";

            IExternalProvider prov = null;
            IExternalProvider lastprov = null;
            prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Code);
            if ((prov != null) && (prov.IsAvailable(usercontext)))
            {
                result += "<option value=\"0\" " + ((usercontext.PageID == 0) ? "selected=\"true\"> " : "> ") + prov.GetUIListOptionLabel(usercontext) + "</option>";
                if (prov.PinRequired)
                {
                    showpin = true;
                    lastprov = prov;
                }
            }
            prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Email);
            if ((prov != null) && (prov.IsAvailable(usercontext)))
            {
                result += "<option value=\"1\" " + ((usercontext.PageID == 1) ? "selected=\"true\"> " : "> ") + prov.GetUIListOptionLabel(usercontext) + "</option>";
                if (prov.PinRequired)
                {
                    showpin = true;
                    lastprov = prov;
                }
            }
            prov = RuntimeAuthProvider.GetProvider(PreferredMethod.External);
            if ((prov != null) && (prov.IsAvailable(usercontext)))
            {
                result += "<option value=\"2\" " + ((usercontext.PageID == 2) ? "selected=\"true\"> " : "> ") + prov.GetUIListOptionLabel(usercontext) + "</option>";
                if (prov.PinRequired)
                {
                    showpin = true;
                    lastprov = prov;
                }
            }
            prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Azure);
            if ((prov != null) && (prov.IsAvailable(usercontext)))
            {
                result += "<option value=\"3\" " + ((usercontext.PageID == 3) ? "selected=\"true\"> " : "> ") + prov.GetUIListOptionLabel(usercontext) + "</option>";
                if (prov.PinRequired)
                {
                    showpin = true;
                    lastprov = prov;
                }
            }
            prov = RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics);
            if ((prov != null) && (prov.IsAvailable(usercontext)))
            {
                result += "<option value=\"4\" " + ((usercontext.PageID == 4) ? "selected=\"true\"> " : "> ") + prov.GetUIListOptionLabel(usercontext) + "</option>";
                if (prov.PinRequired)
                {
                    showpin = true;
                    lastprov = prov;
                }
            }
            if ((showpin) && (lastprov != null))
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
            result += "<b><div class=\"fieldMargin\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGAccessMethod") + "</div></b>";
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
        /// GetPartHtmlSelectMethod2 method ipmplmentation
        /// </summary>
        private string GetPartHtmlSelectMethod2(AuthenticationContext usercontext)
        {
            Registration reg = RuntimeRepository.GetUserRegistration(Provider.Config, usercontext.UPN);
            PreferredMethod method = PreferredMethod.None;
            if (reg == null)
                method = usercontext.PreferredMethod;
            else
                method = reg.PreferredMethod;
            string result = string.Empty;
            result += "<b><div class=\"fieldMargin\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGAccessMethod") + "</div></b>";
            result += "<select id=\"selectopt\" name=\"selectopt\" role=\"combobox\"  required=\"required\" contenteditable=\"false\" class=\"text fullWidth\" >";

            result += "<option value=\"0\" " + ((method == PreferredMethod.Choose) ? "selected=\"true\"> " : "> ") + Resources.GetString(ResourcesLocaleKind.Html, "HtmlREGOptionChooseBest") + "</option>";

            if (RuntimeAuthProvider.IsProviderAvailable(usercontext, PreferredMethod.Code))
                result += "<option value=\"1\" " + ((method == PreferredMethod.Code) ? "selected=\"true\"> " : "> ") + RuntimeAuthProvider.GetProvider(PreferredMethod.Code).GetUIListChoiceLabel(usercontext) + "</option>";

            if (RuntimeAuthProvider.IsProviderAvailable(usercontext, PreferredMethod.Email))
                result += "<option value=\"2\" " + ((method == PreferredMethod.Email) ? "selected=\"true\"> " : "> ") + RuntimeAuthProvider.GetProvider(PreferredMethod.Email).GetUIListChoiceLabel(usercontext) + "</option>";

            if (RuntimeAuthProvider.IsProviderAvailable(usercontext, PreferredMethod.External))
                result += "<option value=\"3\" " + ((method == PreferredMethod.External) ? "selected=\"true\"> " : "> ") + RuntimeAuthProvider.GetProvider(PreferredMethod.External).GetUIListChoiceLabel(usercontext) + "</option>";

            if (RuntimeAuthProvider.IsProviderAvailable(usercontext, PreferredMethod.Azure))
                result += "<option value=\"4\" " + ((method == PreferredMethod.Azure) ? "selected=\"true\">" : ">") + RuntimeAuthProvider.GetProvider(PreferredMethod.Azure).GetUIListChoiceLabel(usercontext) + "</option>";

            if (RuntimeAuthProvider.IsProviderAvailable(usercontext, PreferredMethod.Biometrics))
                result += "<option value=\"5\" " + ((method == PreferredMethod.Biometrics) ? "selected=\"true\">" : ">") + RuntimeAuthProvider.GetProvider(PreferredMethod.Biometrics).GetUIListChoiceLabel(usercontext) + "</option>";

            result += "</select></br>";
            return result;
        }
        #endregion
    }
}

