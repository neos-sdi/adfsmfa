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
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityServer.Web.Authentication.External;
using Neos.IdentityServer.MultiFactor.Common;



namespace Neos.IdentityServer.MultiFactor
{
    /// <summary>
    /// AdapterPresentation implementation
    /// </summary>
    public class AdapterPresentation : IAdapterPresentation, IAdapterPresentationForm
    {
        private BasePresentation _adapter = null;
        private static List<PlaceHolders> _holders = new List<PlaceHolders>();

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
            InitPlaceHolders();
            switch (provider.Config.UiKind)
            {
                case ADFSUserInterfaceKind.Default2019:
                    _adapter = new AdapterPresentation2019(provider, context)
                    {
                        UseUIPaginated = provider.Config.UseUIPaginated
                    };
                    break;
                default:
                    _adapter = new AdapterPresentationDefault(provider, context)
                    {
                        UseUIPaginated = false
                    };
                    break;
            }
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, string message) : base()
        {
            if (provider == null)
                throw new ArgumentNullException("Provider");
            if (provider.Config == null)
                throw new ArgumentNullException("Config");
            InitPlaceHolders();
            switch (provider.Config.UiKind)
            {
                case ADFSUserInterfaceKind.Default2019:
                    _adapter = new AdapterPresentation2019(provider, context, message)
                    {
                        UseUIPaginated = provider.Config.UseUIPaginated
                    };
                    break;
                default:
                    _adapter = new AdapterPresentationDefault(provider, context, message)
                    {
                        UseUIPaginated = false
                    };
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
            InitPlaceHolders();
            switch (provider.Config.UiKind)
            {
                case ADFSUserInterfaceKind.Default2019:
                    _adapter = new AdapterPresentation2019(provider, context, message, ismessage)
                    {
                        UseUIPaginated = provider.Config.UseUIPaginated
                    };
                    break;
                default:
                    _adapter = new AdapterPresentationDefault(provider, context, message, ismessage)
                    {
                        UseUIPaginated = false
                    };
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
            InitPlaceHolders();
            switch (provider.Config.UiKind)
            {
                case ADFSUserInterfaceKind.Default2019:
                    _adapter = new AdapterPresentation2019(provider, context, suite)
                    {
                        UseUIPaginated = provider.Config.UseUIPaginated
                    };
                    break;
                default:
                    _adapter = new AdapterPresentationDefault(provider, context, suite)
                    {
                        UseUIPaginated = false
                    };
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
            InitPlaceHolders();
            switch (provider.Config.UiKind)
            {
                case ADFSUserInterfaceKind.Default2019:
                    _adapter = new AdapterPresentation2019(provider, context, message, suite, disableoptions)
                    {
                        UseUIPaginated = provider.Config.UseUIPaginated
                    };
                    break;
                default:
                    _adapter = new AdapterPresentationDefault(provider, context, message, suite, disableoptions)
                    {
                        UseUIPaginated = false
                    };
                    break;
            }
        }
        #endregion

        #region properties
        /// <summary>
        /// IsPermanentFailure property
        /// </summary>
        public bool IsPermanentFailure
        {
            get { return _adapter.IsPermanentFailure; }
            internal set { _adapter.IsPermanentFailure = value; }
        }

        /// <summary>
        /// IsMessage property
        /// </summary>
        public bool IsMessage
        {
            get { return _adapter.IsMessage; }
            internal set { _adapter.IsMessage = value; }
        }

        /// <summary>
        /// DisableOptions property
        /// </summary>
        public bool DisableOptions
        {
            get { return _adapter.DisableOptions; }
            internal set { _adapter.DisableOptions = value; }
        }

        /// <summary>
        /// Provider property
        /// </summary>
        public AuthenticationProvider Provider
        {
            get { return _adapter.Provider; }
            internal set { _adapter.Provider = value; }
        }

        /// <summary>
        /// Context property
        /// </summary>
        public AuthenticationContext Context
        {
            get { return _adapter.Context; }
            internal set { _adapter.Context = value; }
        }

        /// <summary>
        /// Resources property
        /// </summary>
        public ResourcesLocale Resources
        {
            get { return _adapter.Resources; }
            internal set { _adapter.Resources = value; }
        }
        #endregion

        #region Interface implementations
        /// <summary>
        /// GetPageTitle implementation
        /// </summary>
        public string GetPageTitle(int lcid)
        {
            return ReplacePlaceHolders(_adapter.GetPageTitle(lcid));
        }

        /// <summary>
        /// GetFormHtml implementation
        /// </summary>
        public string GetFormHtml(int lcid)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtml(lcid));
        }

        /// <summary>
        /// GetFormPreRenderHtml implementation
        /// </summary>
        public string GetFormPreRenderHtml(int lcid)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtml(lcid));
        }
        #endregion

        #region Custom implementations
        /// <summary>
        /// GetFormPreRenderHtmlIdentification implementation
        /// </summary>
        public string GetFormPreRenderHtmlIdentification(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlIdentification(usercontext));
        }

        /// <summary>
        /// GetFormHtmlIdentification implementation
        /// </summary>
        public string GetFormHtmlIdentification(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlIdentification(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlManageOptions implementation
        /// </summary>
        public string GetFormPreRenderHtmlManageOptions(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlManageOptions(usercontext));
        }

        /// <summary>
        /// GetFormHtmlManageOptions implementation
        /// </summary>
        public string GetFormHtmlManageOptions(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlManageOptions(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlRegistration implementation
        /// </summary>
        public string GetFormPreRenderHtmlRegistration(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlRegistration(usercontext));
        }

        /// <summary>
        /// GetFormHtmlRegistration implementation
        /// </summary>
        public string GetFormHtmlRegistration(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlRegistration(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlInvitation implementation
        /// </summary>
        public string GetFormPreRenderHtmlInvitation(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlInvitation(usercontext));
        }

        /// <summary>
        /// GetFormHtmlInvitation implementation
        /// </summary>
        public string GetFormHtmlInvitation(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlInvitation(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlActivation implementation
        /// </summary>
        public string GetFormPreRenderHtmlActivation(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlActivation(usercontext));
        }

        /// <summary>
        /// GetFormHtmlActivation implementation
        /// </summary>
        public string GetFormHtmlActivation(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlActivation(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlSelectOptions implementation
        /// </summary>
        public string GetFormPreRenderHtmlSelectOptions(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlSelectOptions(usercontext));
        }

        /// <summary>
        /// GetFormHtmlSelectOptions implementation
        /// </summary>
        public string GetFormHtmlSelectOptions(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlSelectOptions(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlChooseMethod implementation
        /// </summary>
        public string GetFormPreRenderHtmlChooseMethod(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlChooseMethod(usercontext));
        }

        /// <summary>
        /// GetFormHtmlChooseMethod implementation
        /// </summary>
        public string GetFormHtmlChooseMethod(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlChooseMethod(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlChangePassword implementation
        /// </summary>
        public string GetFormPreRenderHtmlChangePassword(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlChangePassword(usercontext));
        }

        /// <summary>
        /// GetFormHtmlChangePassword implementation
        /// </summary>
        public string GetFormHtmlChangePassword(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlChangePassword(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlBypass implementation
        /// </summary>
        public string GetFormPreRenderHtmlBypass(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlBypass(usercontext));
        }

        /// <summary>
        /// GetFormHtmlBypass implementation
        /// </summary>
        public string GetFormHtmlBypass(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlBypass(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlLocking implementation
        /// </summary>
        public string GetFormPreRenderHtmlLocking(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlLocking(usercontext));
        }

        /// <summary>
        /// GetFormHtmlLocking implementation
        /// </summary>
        public string GetFormHtmlLocking(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlLocking(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlShowQRCode implementation
        /// </summary>
        public string GetFormPreRenderHtmlShowQRCode(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlShowQRCode(usercontext));
        }

        /// <summary>
        /// GetFormHtmlShowQRCode implementation
        /// </summary>
        public string GetFormHtmlShowQRCode(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlShowQRCode(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlSendCodeRequest implementation
        /// </summary>
        public string GetFormPreRenderHtmlSendCodeRequest(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlSendCodeRequest(usercontext));
        }

        /// <summary>
        /// GetFormHtmlSendCodeRequest implementation
        /// </summary>
        public string GetFormHtmlSendCodeRequest(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlSendCodeRequest(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlSendBiometricRequest implementation
        /// </summary>
        public string GetFormPreRenderHtmlSendBiometricRequest(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlSendBiometricRequest(usercontext));
        }

        /// <summary>
        /// GetFormHtmlSendBiometricRequest implementation
        /// </summary>
        public string GetFormHtmlSendBiometricRequest(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlSendBiometricRequest(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlSendAdministrativeRequest implementation
        /// </summary>
        public string GetFormPreRenderHtmlSendAdministrativeRequest(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlSendAdministrativeRequest(usercontext));
        }

        /// <summary>
        /// GetFormHtmlSendAdministrativeRequest implementation
        /// </summary>
        public string GetFormHtmlSendAdministrativeRequest(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlSendAdministrativeRequest(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlSendKeyRequest implementation
        /// </summary>
        public string GetFormPreRenderHtmlSendKeyRequest(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlSendKeyRequest(usercontext));
        }

        /// <summary>
        /// GetFormHtmlSendKeyRequest implementation
        /// </summary>
        public string GetFormHtmlSendKeyRequest(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlSendKeyRequest(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlEnrollOTP implementation
        /// </summary>
        public string GetFormPreRenderHtmlEnrollOTP(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlEnrollOTP(usercontext));
        }

        /// <summary>
        /// GetFormHtmlEnrollOTP implementation
        /// </summary>
        public string GetFormHtmlEnrollOTP(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlEnrollOTP(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlEnrollEmail implementation
        /// </summary>
        public string GetFormPreRenderHtmlEnrollEmail(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlEnrollEmail(usercontext));
        }

        /// <summary>
        /// GetFormHtmlEnrollEmail implementation
        /// </summary>
        public string GetFormHtmlEnrollEmail(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlEnrollEmail(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlEnrollPhone implementation
        /// </summary>
        public string GetFormPreRenderHtmlEnrollPhone(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlEnrollPhone(usercontext));
        }

        /// <summary>
        /// GetFormHtmlEnrollPhone implementation
        /// </summary>
        public string GetFormHtmlEnrollPhone(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlEnrollPhone(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlEnrollBio implementation
        /// </summary>
        public string GetFormPreRenderHtmlEnrollBio(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlEnrollBio(usercontext));
        }

        /// <summary>
        /// GetFormHtmlEnrollBio implementation
        /// </summary>
        public string GetFormHtmlEnrollBio(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlEnrollBio(usercontext));
        }

        /// <summary>
        /// GetFormPreRenderHtmlEnrollPinCode implementation
        /// </summary>
        public string GetFormPreRenderHtmlEnrollPinCode(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormPreRenderHtmlEnrollPinCode(usercontext));
        }

        /// <summary>
        /// GetFormHtmlEnrollPinCode implementation
        /// </summary>
        public string GetFormHtmlEnrollPinCode(AuthenticationContext usercontext)
        {
            return ReplacePlaceHolders(_adapter.GetFormHtmlEnrollPinCode(usercontext));
        }
        #endregion

        #region PlaceHolders
        /// <summary>
        /// ReplacePlaceHolders method implmentation
        /// </summary>
        private string ReplacePlaceHolders(string html)
        {
            _holders.ForEach(C => html = html.Replace(C.TagName, C.FiledName));
            return html;
        }

        /// <summary>
        /// InitPlaceHolders  method implmentation
        /// </summary>
        private void InitPlaceHolders()
        {
            if (_holders.Count <= 0)
            {
                _holders.Add(new PlaceHolders() { TagName = "##ACCESSCODE##", FiledName = "totp" });
                _holders.Add(new PlaceHolders() { TagName = "##PINCODE##", FiledName = "pincode" });
                _holders.Add(new PlaceHolders() { TagName = "##SELECTED##", FiledName = "selected" });
                _holders.Add(new PlaceHolders() { TagName = "##EMAILADDRESS##", FiledName = "email" });
                _holders.Add(new PlaceHolders() { TagName = "##PHONENUMBER##", FiledName = "phone" });
                _holders.Add(new PlaceHolders() { TagName = "##SELECTEDLINK##", FiledName = "selectedlink" });
                _holders.Add(new PlaceHolders() { TagName = "##OPTIONS##", FiledName = "options" });
                _holders.Add(new PlaceHolders() { TagName = "##DISABLEMFA##", FiledName = "disablemfa" });
                _holders.Add(new PlaceHolders() { TagName = "##ISPROVIDER##", FiledName = "isprovider" });
                _holders.Add(new PlaceHolders() { TagName = "##SELECTOPTIONS##", FiledName = "selectopt" });
                _holders.Add(new PlaceHolders() { TagName = "##REMEMBER##", FiledName = "remember" });
                _holders.Add(new PlaceHolders() { TagName = "##SELECTEDRADIO##", FiledName = "selectedradio" });
                _holders.Add(new PlaceHolders() { TagName = "##OLDPASS##", FiledName = "oldpwdedit" });
                _holders.Add(new PlaceHolders() { TagName = "##NEWPASS##", FiledName = "newpwdedit" });
                _holders.Add(new PlaceHolders() { TagName = "##CNFPASS##", FiledName = "cnfpwdedit" });
                _holders.Add(new PlaceHolders() { TagName = "##MANAGEACCOUNT##", FiledName = "manageaccount" });
                _holders.Add(new PlaceHolders() { TagName = "##OPTIONITEM##", FiledName = "optionitem" });
                }
        }
        #endregion
    }

    /// <summary>
    /// BasePresentation implementation
    /// </summary>
    public abstract class BasePresentation
    {
        private const string CR = "\r\n";

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
            this.Context.UIMessage = string.Empty;
            this.IsPermanentFailure = false; 
            this.IsMessage = true; 
            this.DisableOptions = false;
            this.Resources = new ResourcesLocale(Context.Lcid);
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
            this.Resources = new ResourcesLocale(Context.Lcid);
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
            this.Resources = new ResourcesLocale(Context.Lcid);
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public BasePresentation(AuthenticationProvider provider, IAuthenticationContext context, ProviderPageMode suite)
        {
            this.Provider = provider;
            this.Context = new AuthenticationContext(context);
            this.Context.UIMessage = string.Empty;
            this.Context.TargetUIMode = suite;
            this.IsPermanentFailure = (this.Context.TargetUIMode == ProviderPageMode.DefinitiveError);
            this.IsMessage = (this.Context.TargetUIMode != ProviderPageMode.DefinitiveError);
            this.DisableOptions = false;
            this.Resources = new ResourcesLocale(Context.Lcid);
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
            this.Resources = new ResourcesLocale(Context.Lcid);
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
                result += "<br/>";
                if (!String.IsNullOrEmpty(usercontext.UIMessage))
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div>";
                else
                    result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlErrorRestartSession") + "</label></div>";
            }
            else
            {
                if (!String.IsNullOrEmpty(usercontext.UIMessage))
                {
                    result += "<br/>";
                    if (IsMessage)
                        result += "<div id=\"error\" class=\"fieldMargin smallText\" style=\"color: #6FA400\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div>";
                    else
                        result += "<div id=\"error\" class=\"fieldMargin error smallText\"><label id=\"errorText\" name=\"errorText\" for=\"\">" + usercontext.UIMessage + "</label></div>";
                }
            }
            return result;
        }

        /// <summary>
        /// GetFormPreRenderHtmlCSS implementation
        /// </summary>
        protected virtual string GetFormPreRenderHtmlCSS(AuthenticationContext usercontext, bool removetag = false)
        {
            string result = string.Empty;
            result += "#wizardMessage, #wizardMessage2, #wizardMessage3 {";
            result += "box-sizing: border-box;";
            result += "color: rgb(38, 38, 38);";
            result += "direction: ltr;";
            result += "display: block;";
            result += "font-family: \"Segoe UI Webfont\", -apple-system, \"Helvetica Neue\", \"Lucida Grande\", Roboto, Ebrima, \"Nirmala UI\", Gadugi, \"Segoe Xbox Symbol\", \"Segoe UI Symbol\", \"Meiryo UI\", \"Khmer UI\", Tunga, \"Lao UI\", Raavi, \"Iskoola Pota\", Latha, Leelawadee, \"Microsoft YaHei UI\", \"Microsoft JhengHei UI\", \"Malgun Gothic\", \"Estrangelo Edessa\", \"Microsoft Himalaya\", \"Microsoft New Tai Lue\", \"Microsoft PhagsPa\", \"Microsoft Tai Le\", \"Microsoft Yi Baiti\", \"Mongolian Baiti\", \"MV Boli\", \"Myanmar Text\", \"Cambria Math\";";
            result += "font-weight: 300;";
            result += "font-size: 1.7em;";
            result += "height: auto;";
            result += "line-height: 28px;";
            result += "margin-bottom: 16px;";
            result += "margin-left: -2px;";
            result += "margin-right: -2px;";
            result += "margin-top: 16px;";
            result += "padding-bottom: 0px;";
            result += "padding-left: 0px;";
            result += "padding-right: 0px;";
            result += "padding-top: 0px;";
            result += "text-align: left;";
            result += "text-size-adjust: 100%;";
            result += "width: 342px;";
            result += "background-color: transparent;";
            result += "}" + CR;
            if (!removetag)
                return "<style>" + result + "</style>"+ CR + CR;
            else
                return result + CR;
        }

        /// <summary>
        /// GetFormPreRenderHtmlHeader method implementation
        /// </summary>
        protected virtual string GetFormPreRenderHtmlHeader(AuthenticationContext usercontext)
        {
            string result = "<script type='text/javascript'>" + CR;
            result += "function RemoveADFSHtmlHeader()" + CR;
            result += "{" + CR;
            result += "   var title = document.getElementById('mfaGreetingDescription');" + CR;
            result += "   if (title)" + CR;
            result += "   {" + CR;
            result += "      title.style.display = \"none\";" + CR;
            result += "   }" + CR;
            if (WebThemeManager.HasRelyingPartyTheme(usercontext))
            {
                Dictionary<WebThemeAddressKind, string>  dic = WebThemeManager.GetAddresses(usercontext);
                if (dic != null)
                {
                    result += "   SetIllustrationImage(\"" + dic[WebThemeAddressKind.Illustration].ToString() + "\");" + CR;
                    result += "   document.getElementById('companyLogo').src = \"" + dic[WebThemeAddressKind.CompanyLogo].ToString() + "\";" + CR;
                    result += "   document.getElementsByTagName('link')[0].href = \"" + dic[WebThemeAddressKind.StyleSheet].ToString() + "\";" + CR;
                }
            } 
            result += "   return true;" + CR;
            result += "}" + CR;         
            result += "</script>" + CR + CR;
            return result;
        }   

        /// <summary>
        /// GetFormPreRenderHtmlHeader method implementation
        /// </summary>
        protected virtual string GetFormRenderHtmlHeader(AuthenticationContext usercontext)
        {
            string result = "<script type=\"text/javascript\">" + CR;
            result += "if (window.addEventListener)" + CR;
            result += "{" + CR;
            result += "   window.addEventListener('load', RemoveADFSHtmlHeader, false);" + CR;
            result += "}" + CR;
            result += "else if (window.attachEvent)" + CR;
            result += "{" + CR;
            result += "   window.attachEvent('onload', RemoveADFSHtmlHeader);" + CR;
            result += "}" + CR;
            result += "</script>" + CR;

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
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlIdentification(Context);
                    break;
                case ProviderPageMode.Registration: // User self registration and enable
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlRegistration(Context);
                    break;
                case ProviderPageMode.Invitation: // admministrative user registration and let disabled
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlInvitation(Context);
                    break;
                case ProviderPageMode.Activation: // Try to enable
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlActivation(Context);
                    break;
                case ProviderPageMode.ManageOptions:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlManageOptions(Context);
                    break;
                case ProviderPageMode.SelectOptions:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlSelectOptions(Context);
                    break;
                case ProviderPageMode.ChooseMethod:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlChooseMethod(Context);
                    break;
                case ProviderPageMode.ChangePassword:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlChangePassword(Context);
                    break;
                case ProviderPageMode.Bypass:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlBypass(Context);
                    break;
                case ProviderPageMode.Locking:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlLocking(Context);
                    break;
                case ProviderPageMode.ShowQRCode:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlShowQRCode(Context);
                    break;
                case ProviderPageMode.SendAuthRequest:
                    result += GetFormRenderHtmlHeader(Context);
                    if (Context.SelectedMethod == AuthenticationResponseKind.Biometrics)
                        result += GetFormHtmlSendBiometricRequest(Context);
                    else
                        result += GetFormHtmlSendCodeRequest(Context);
                    break;
                case ProviderPageMode.SendAdministrativeRequest:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlSendAdministrativeRequest(Context);
                    break;
                case ProviderPageMode.SendKeyRequest:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlSendKeyRequest(Context);
                    break;
                case ProviderPageMode.EnrollOTP:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlEnrollOTP(Context);
                    break;
                case ProviderPageMode.EnrollEmail:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlEnrollEmail(Context);
                    break;
                case ProviderPageMode.EnrollPhone:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlEnrollPhone(Context);
                    break;
                case ProviderPageMode.EnrollBiometrics:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlEnrollBio(Context);
                    break;
                case ProviderPageMode.EnrollPin:
                    result += GetFormRenderHtmlHeader(Context);
                    result += GetFormHtmlEnrollPinCode(Context);
                    break;
            }
            return result;
        }

        /// <summary>
        /// IAdapterPresentationForm GetFormPreRenderHtml implementation
        /// </summary>
        public virtual string GetFormPreRenderHtml(int lcid)
        {
            string result = GetFormPreRenderHtmlCSS(Context);
            switch (Context.UIMode)
            {
                case ProviderPageMode.Identification:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlIdentification(Context);
                    break;
                case ProviderPageMode.Registration: // User self registration and enable
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlRegistration(Context);
                    break;
                case ProviderPageMode.Invitation: // admministrative user registration and let disabled
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlInvitation(Context);
                    break;
                case ProviderPageMode.Activation: // Activation
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlActivation(Context);
                    break;
                case ProviderPageMode.ManageOptions:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlManageOptions(Context);
                    break;
                case ProviderPageMode.SelectOptions:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlSelectOptions(Context);
                    break;
                case ProviderPageMode.ChooseMethod:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlChooseMethod(Context);
                    break;
                case ProviderPageMode.ChangePassword:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlChangePassword(Context);
                    break;
                case ProviderPageMode.Bypass:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlBypass(Context);
                    break;
                case ProviderPageMode.Locking:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlLocking(Context);
                    break;
                case ProviderPageMode.ShowQRCode:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result = GetFormPreRenderHtmlShowQRCode(Context);
                    break;
                case ProviderPageMode.SendAuthRequest:
                    result += GetFormPreRenderHtmlHeader(Context);
                    if (Context.SelectedMethod == AuthenticationResponseKind.Biometrics)
                        result += GetFormPreRenderHtmlSendBiometricRequest(Context);
                    else
                        result += GetFormPreRenderHtmlSendCodeRequest(Context);
                    break;
                case ProviderPageMode.SendAdministrativeRequest:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlSendAdministrativeRequest(Context);
                    break;
                case ProviderPageMode.SendKeyRequest:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlSendKeyRequest(Context);
                    break;
                case ProviderPageMode.EnrollOTP:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlEnrollOTP(Context);
                    break;
                case ProviderPageMode.EnrollEmail:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlEnrollEmail(Context);
                    break;
                case ProviderPageMode.EnrollPhone:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlEnrollPhone(Context);
                    break;
                case ProviderPageMode.EnrollBiometrics:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlEnrollBio(Context);
                    break;
                case ProviderPageMode.EnrollPin:
                    result += GetFormPreRenderHtmlHeader(Context);
                    result += GetFormPreRenderHtmlEnrollPinCode(Context);
                    break;
            }
            return result;
        }
        #endregion

        #region Abstract methods
        public abstract string GetFormPreRenderHtmlIdentification(AuthenticationContext usercontext);
        public abstract string GetFormHtmlIdentification(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlRegistration(AuthenticationContext usercontext);
        public abstract string GetFormHtmlRegistration(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlInvitation(AuthenticationContext usercontext);
        public abstract string GetFormHtmlInvitation(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlActivation(AuthenticationContext usercontext);
        public abstract string GetFormHtmlActivation(AuthenticationContext usercontext);
        public abstract string GetFormPreRenderHtmlManageOptions(AuthenticationContext usercontext);
        public abstract string GetFormHtmlManageOptions(AuthenticationContext usercontext);
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
        public abstract string GetFormPreRenderHtmlSendBiometricRequest(AuthenticationContext usercontext);
        public abstract string GetFormHtmlSendBiometricRequest(AuthenticationContext usercontext);
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
        #endregion

        #region QRCode
        /// <summary>
        /// GetFormPreRenderHtmlShowQRCode implementation
        /// </summary>
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
            result += "<div class=\"fieldMargin smallText\"><label for=\"\"></label>" + Resources.GetString(ResourcesLocaleKind.Html, "HtmlLabelWRQRCode") + "</div><br/>";
            result += "<p style=\"text-align:center\"><img id=\"qr\" src=\"data:image/png;base64," + Provider.GetQRCodeString(usercontext) + "\"/></p><br/>";
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

        #region WebAuthN Support
        /// <summary>
        /// GetFormHtmlWebAuthNSupport method implementation
        /// </summary>
        public virtual string GetFormHtmlWebAuthNSupport(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            result += "function detectWebAuthNSupport()" + CR;
            result += "{" + CR;
            result += "      try" + CR;
            result += "      {" + CR;
            result += "         if (window.PublicKeyCredential === undefined || typeof window.PublicKeyCredential !== \"function\")" + CR;
            result += "             return false;" + CR;
            result += "         else" + CR;
            result += "             return true;" + CR;
            result += "      }" + CR;
            result += "      catch (e)" + CR;
            result += "      {" + CR;
            result += "         SetJsError(e.message);" + CR;
            result += "         return false;" + CR;
            result += "      }" + CR;
            result += "}" + CR;
            return result;
        }

        /// <summary>
        /// GetWebAuthNSharedScript method implementation
        /// </summary>
        public virtual string GetWebAuthNSharedScript(AuthenticationContext usercontext)
        {
            string result = string.Empty;
            result += "function coerceToArrayBuffer(thing, name)" + CR;
            result += "{" + CR;
            result += "    if (typeof thing === \"string\")" + CR;
            result += "    {" + CR;
            result += "       thing = thing.replace(/-/g, \"+\").replace(/_/g, \"/\");" + CR;
            result += "       var str = window.atob(thing);" + CR;
            result += "       var bytes = new Uint8Array(str.length);" + CR;
            result += "       for (var i = 0; i < str.length; i++)" + CR;
            result += "       {" + CR;
            result += "          bytes[i] = str.charCodeAt(i);" + CR;
            result += "       }" + CR;
            result += "       thing = bytes;" + CR;
            result += "    }" + CR;
            result += "    if (Array.isArray(thing))" + CR;
            result += "    {" + CR;
            result += "       thing = new Uint8Array(thing);" + CR;
            result += "    }" + CR;
            result += "    if (thing instanceof Uint8Array)" + CR;
            result += "    {" + CR;
            result += "       thing = thing.buffer;" + CR;
            result += "    }" + CR;
            result += "    if (!(thing instanceof ArrayBuffer))" + CR;
            result += "    {" + CR;
            result += "       throw new TypeError(\"could not coerce '\" + name + \"' to ArrayBuffer\");" + CR;
            result += "    }" + CR;
            result += "    return thing;" + CR;
            result += "}" + CR;
            result += CR;

            result += "function coerceToBase64Url(thing)" + CR;
            result += "{" + CR;
            result += "    if (Array.isArray(thing))" + CR;
            result += "    {" + CR;
            result += "       thing = Uint8Array.from(thing);" + CR;
            result += "    }" + CR;
            result += "    if (thing instanceof ArrayBuffer)" + CR;
            result += "    {" + CR;
            result += "       thing = new Uint8Array(thing);" + CR;
            result += "    }" + CR;
            result += "    if (thing instanceof Uint8Array)" + CR;
            result += "    {" + CR;
            result += "       var str = \"\";" + CR;
            result += "       var len = thing.byteLength;" + CR;
            result += "       for (var i = 0; i < len; i++)" + CR;
            result += "       {" + CR;
            result += "          str += String.fromCharCode(thing[i]);" + CR;
            result += "       }" + CR;
            result += "       thing = window.btoa(str);" + CR;
            result += "    }" + CR;
            result += "    if (typeof thing !== \"string\")" + CR;
            result += "    {" + CR;
            result += "        throw new Error(\"could not coerce to string\");" + CR;
            result += "    }" + CR;
            result += "    thing = thing.replace(/\\+/g, \"-\").replace(/\\//g, \"_\").replace(/=*$/g, \"\");" + CR;
            result += "    return thing;" + CR;
            result += "}" + CR;
            result += CR;

            return result;
        }

        /// <summary>
        /// GetWebAuthNAttestationScript method implementation
        /// </summary>
        public virtual string GetWebAuthNAttestationScript(AuthenticationContext usercontext)
        {
            string result = GetWebAuthNSharedScript(usercontext);
            result += "async function RegisterWebAuthN(frm)" + CR;
            result += "{" + CR;
            result += "   frm.preventDefault();" + CR;
            result += "   if (detectWebAuthNSupport() === true)" + CR;
            result += "   {" + CR;
            result += "      let makeCredentialOptions;" + CR;
            result += "      try" + CR;
            result += "      {" + CR;
            result += "         makeCredentialOptions = " + usercontext.CredentialOptions + ";" + CR;
            result += "         makeCredentialOptions.challenge = coerceToArrayBuffer(makeCredentialOptions.challenge);" + CR;
            result += "         makeCredentialOptions.user.id = coerceToArrayBuffer(makeCredentialOptions.user.id);" + CR;
            result += "         makeCredentialOptions.excludeCredentials = makeCredentialOptions.excludeCredentials.map((c) => {c.id = coerceToArrayBuffer(c.id); return c;});" + CR;
            result += "        if (makeCredentialOptions.authenticatorSelection.authenticatorAttachment === null)" + CR;
            result += "           makeCredentialOptions.authenticatorSelection.authenticatorAttachment = undefined;" + CR;
            result += "      }" + CR;
            result += "      catch (e)" + CR;
            result += "      {" + CR;
            result += "         SetJsError(e.message);" + CR;
            result += "         return false;" + CR;
            result += "      }" + CR;
            result += "      if (makeCredentialOptions.status !== \"ok\")" + CR;
            result += "      {" + CR;
            result += "         SetJsError(makeCredentialOptions.errorMessage);" + CR;
            result += "         return false;" + CR;
            result += "      }" + CR;
            result += "      let newCredential;" + CR;
            result += "      try" + CR;
            result += "      {" + CR;
            result += "         newCredential = await navigator.credentials.create({publicKey: makeCredentialOptions});" + CR;
            result += "         RegisterNewCredentials(newCredential);" + CR;
            result += "      }" + CR;
            result += "      catch (e)" + CR;
            result += "      {" + CR;
            result += "         SetJsError(e.message);" + CR;
            result += "         return false;" + CR;
            result += "      }" + CR;
            result += "      return true;" + CR;
            result += "   }" + CR;
            result += "   else" + CR;
            result += "   {" + CR;
            result += "      SetJsError(\"Biometric authentication not supported\");" + CR;
            result += "      return false;" + CR;
            result += "   }" + CR;
            result += "}" + CR;
            result += CR;

            result += "async function RegisterNewCredentials(newCredential)" + CR;
            result += "{" + CR;
            result += "   try" + CR;
            result += "   {" + CR;
            result += "      let attestationObject = new Uint8Array(newCredential.response.attestationObject);" + CR;
            result += "      let clientDataJSON = new Uint8Array(newCredential.response.clientDataJSON);" + CR;
            result += "      let rawId = new Uint8Array(newCredential.rawId);" + CR;
            result += "      const data = {id: newCredential.id, rawId: coerceToBase64Url(rawId), type: newCredential.type, " + CR;
            result += "         extensions: newCredential.getClientExtensionResults()," + CR;
            result += "         response: { AttestationObject: coerceToBase64Url(attestationObject), clientDataJson: coerceToBase64Url(clientDataJSON) }" + CR;
            result += "      };" + CR;
            result += "      OnRefreshPost(3, data);" + CR;
            result += "   }" + CR;
            result += "   catch (e)" + CR;
            result += "   {" + CR;
            result += "      SetJsError(e.message);" + CR;
            result += "      return false;" + CR;
            result += "   }" + CR;
            result += "   return true;" + CR;
            result += "}" + CR;
            result += CR;

            return result;
        }

        /// <summary>
        /// GetWebAuthNAssertionScript method implementation
        /// </summary>
        public virtual string GetWebAuthNAssertionScript(AuthenticationContext usercontext)
        {
            string result = GetWebAuthNSharedScript(usercontext);
            result += "async function LoginWebAuthN(frm)" + CR;
            result += "{" + CR;
            result += "   frm.preventDefault();" + CR;
            result += "   if (detectWebAuthNSupport() === true)" + CR;
            result += "   {" + CR;
            result += "      let makeAssertionOptions;" + CR;
            result += "      try" + CR;
            result += "      {" + CR;
            result += "         makeAssertionOptions = " + usercontext.AssertionOptions + ";" + CR;
            result += "         const challenge = makeAssertionOptions.challenge.replace(/-/g, \"+\").replace(/_/g, \"/\");" + CR;
            result += "         makeAssertionOptions.challenge = Uint8Array.from(atob(challenge), c => c.charCodeAt(0));" + CR;
            result += "         makeAssertionOptions.allowCredentials.forEach(function (listItem)" + CR;
            result += "         {" + CR;
            result += "            var fixedId = listItem.id.replace(/_/g, \"/\").replace(/-/g, \"+\");" + CR;
            result += "            listItem.id = Uint8Array.from(atob(fixedId), c => c.charCodeAt(0));" + CR;
            result += "         });" + CR;
            result += "      }" + CR;
            result += "      catch (e)" + CR;
            result += "      {" + CR;
            result += "         SetJsError(e.message);" + CR;
            result += "         return false;" + CR;
            result += "      }" + CR;
            result += "      if (makeAssertionOptions.status !== \"ok\")" + CR;
            result += "      {" + CR;
            result += "         SetJsError(makeAssertionOptions.errorMessage);" + CR;
            result += "         return false;" + CR;
            result += "      }" + CR;
            result += "      let credential;" + CR;
            result += "      try" + CR;
            result += "      {" + CR;
            result += "         credential = await navigator.credentials.get({ publicKey: makeAssertionOptions });" + CR;
            result += "         LoginAssertionCredentials(credential);" + CR;
            result += "      }" + CR;
            result += "      catch (e)" + CR;
            result += "      {" + CR;
            result += "         SetJsError(e.message);" + CR;
            result += "         return false;" + CR;
            result += "      }" + CR;
            result += "      return true;" + CR;
            result += "   }" + CR;
            result += "   else" + CR;
            result += "   {" + CR;
            result += "      SetJsError(\"Biometric authentication not supported\");" + CR;
            result += "      return false;" + CR;
            result += "   }" + CR;
            result += "}" + CR;
            result += CR;
            result += "async function LoginAssertionCredentials(assertedCredential)" + CR;
            result += "{" + CR;
            result += "   try" + CR;
            result += "   {" + CR;
            result += "      let authData = new Uint8Array(assertedCredential.response.authenticatorData);" + CR;
            result += "      let clientDataJSON = new Uint8Array(assertedCredential.response.clientDataJSON);" + CR;
            result += "      let rawId = new Uint8Array(assertedCredential.rawId);" + CR;
            result += "      let sig = new Uint8Array(assertedCredential.response.signature);" + CR;
            result += "      const data = " + CR;
            result += "      {" + CR;
            result += "         id: assertedCredential.id," + CR;
            result += "         rawId: coerceToBase64Url(rawId)," + CR;
            result += "         type: assertedCredential.type," + CR;
            result += "         extensions: assertedCredential.getClientExtensionResults()," + CR;
            result += "         response: { authenticatorData: coerceToBase64Url(authData)," + CR;
            result += "         clientDataJson: coerceToBase64Url(clientDataJSON)," + CR;
            result += "         signature: coerceToBase64Url(sig) }" + CR;
            result += "      };" + CR;
            result += "      OnRefreshPost(data);" + CR;
            result += "   }" + CR;
            result += "   catch (e)" + CR;
            result += "   {" + CR;
            result += "      SetJsError(e.message);" + CR;
            result += "      return false;" + CR;
            result += "   }" + CR;
            result += "   return true;" + CR;
            result += "}" + CR;
            result += CR;

            return result;
        }
        #endregion
    }
}
