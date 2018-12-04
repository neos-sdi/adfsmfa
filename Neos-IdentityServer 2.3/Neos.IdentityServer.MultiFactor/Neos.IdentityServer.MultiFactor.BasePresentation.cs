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

        public abstract bool IsPermanentFailure { get; internal set; }
        public abstract bool IsMessage { get; internal set; }
        public abstract bool DisableOptions { get; internal set; }
        public abstract AuthenticationProvider Provider { get; internal set; }
        public abstract AuthenticationContext Context { get; internal set; }
        public abstract ResourcesLocale Resources { get; internal set; }

        public abstract string GetPageTitle(int lcid);
        public abstract string GetFormHtml(int lcid);
        public abstract string GetFormPreRenderHtml(int lcid);

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
        public abstract string GetFormPreRenderHtmlShowQRCode(AuthenticationContext usercontext);
        public abstract string GetFormHtmlShowQRCode(AuthenticationContext usercontext);
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
    }

    public class AdapterPresentation : BasePresentation
    {
        private DefaultAdapterPresentation _adapter = null;
        #region Constructors
        /// <summary>
        /// Constructor implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context)
        {
            _adapter = new DefaultAdapterPresentation(provider, context);
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, string message)
        {
            _adapter = new DefaultAdapterPresentation(provider, context, message);
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, string message, bool ismessage)
        {
            _adapter = new DefaultAdapterPresentation(provider, context, message, ismessage);
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, ProviderPageMode suite)
        {
            _adapter = new DefaultAdapterPresentation(provider, context, suite);
        }

        /// <summary>
        /// Constructor overload implementation
        /// </summary>
        public AdapterPresentation(AuthenticationProvider provider, IAuthenticationContext context, string message, ProviderPageMode suite, bool disableoptions = false)
        {
            _adapter = new DefaultAdapterPresentation(provider, context, message, suite, disableoptions);
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
