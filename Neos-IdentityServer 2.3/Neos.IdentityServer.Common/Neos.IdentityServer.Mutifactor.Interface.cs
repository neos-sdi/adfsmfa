using Neos.IdentityServer.MultiFactor.Data;
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
using System.Globalization;
using Microsoft.IdentityServer.Web.Authentication.External;
using System.Collections.Generic;

namespace Neos.IdentityServer.MultiFactor
{
    #region External Connectors
    /// <summary>
    /// IExternalOTPProvider interface declaration
    /// </summary>
    public interface IExternalOTPProvider
    {
        int GetUserCodeWithExternalSystem(string upn, string phonenumber, string email, ExternalOTPProvider externalsys, CultureInfo culture);
    }

    /// <summary>
    /// IExternalProvider interface declaration
    /// </summary>
    public interface IExternalProvider
    {
        PreferredMethod Kind { get; }
        bool Enabled { get; set; }
        bool PinRequired { get; set; }
        bool AllowDisable { get; }
        bool IsInitialized { get; }
        bool AllowOverride { get; }
        bool AllowEnrollment { get; set; }
        ForceWizardMode ForceEnrollment { get; set; }
        bool IsTwoWayByDefault  { get; }
        string Name { get; }
        string Description { get; }
        string GetUILabel(AuthenticationContext ctx);
        string GetWizardUILabel(AuthenticationContext ctx);
        string GetUICFGLabel(AuthenticationContext ctx);
        string GetUIMessage(AuthenticationContext ctx);
        string GetUIListOptionLabel(AuthenticationContext ctx);
        string GetUIListChoiceLabel(AuthenticationContext ctx);
        string GetUIConfigLabel(AuthenticationContext ctx);
        string GetUIChoiceLabel(AuthenticationContext ctx, AvailableAuthenticationMethod method = null);
        string GetUIWarningInternetLabel(AuthenticationContext ctx);
        string GetUIWarningThirdPartyLabel(AuthenticationContext ctx);
        string GetUIDefaultChoiceLabel(AuthenticationContext ctx);
        string GetUIAccountManagementLabel(AuthenticationContext ctx);
        string GetAccountManagementUrl(AuthenticationContext ctx);

        void Initialize(BaseProviderParams externalsystem);
        int PostAuthenticationRequest(AuthenticationContext ctx);
        int SetAuthenticationResult(AuthenticationContext ctx, string result);

        void GetAuthenticationContext(AuthenticationContext ctx);
        AvailableAuthenticationMethod GetSelectedAuthenticationMethod(AuthenticationContext ctx);
        bool SetSelectedAuthenticationMethod(AuthenticationContext ctx, AuthenticationResponseKind method, bool updateoverride = false);
        AvailableAuthenticationMethod GetDefaultAuthenticationMethod(AuthenticationContext ctx);
        List<AvailableAuthenticationMethod> GetAuthenticationMethods(AuthenticationContext ctx);

        bool IsAvailable(AuthenticationContext ctx);
        bool IsAvailableForUser(AuthenticationContext ctx);
        bool IsUIElementRequired(AuthenticationContext ctx, RequiredMethodElements element);

        AuthenticationResponseKind GetOverrideMethod(AuthenticationContext ctx);
        void SetOverrideMethod(AuthenticationContext ctx, AuthenticationResponseKind kind);
    }

    public interface IExternalAdminProvider
    {
        void Initialize(MFAConfig config);
        void GetInscriptionContext(AuthenticationContext ctx);
        void GetSecretKeyContext(AuthenticationContext ctx);
        int PostInscriptionRequest(AuthenticationContext ctx);
        int PostSecretKeyRequest(AuthenticationContext ctx);
        int SetInscriptionResult(AuthenticationContext ctx);
        int SetSecretKeyResult(AuthenticationContext ctx);

        string GetUIInscriptionMessageLabel(AuthenticationContext ctx);
        string GetUISecretKeyMessageLabel(AuthenticationContext ctx);

        string GetUIWarningInternetLabel(AuthenticationContext ctx);
        string GetUIWarningThirdPartyLabel(AuthenticationContext ctx);
    }
    #endregion

    #region External Keys
    /// <summary>
    /// ISecretKeyManager interface declaration
    /// </summary>
    public interface ISecretKeyManager
    {
        string Prefix { get; }
        void Initialize(MFAConfig config);
        string NewKey(string upn);
        string ReadKey(string upn);
        string EncodedKey(string upn);
        string ProbeKey(string upn);
        bool RemoveKey(string upn); 
        bool ValidateKey(string upn);
        string StripKeyPrefix(string key);
        string AddKeyPrefix(string key);
        bool HasKeyPrefix(string key);
        KeysRepositoryService KeysStorage { get; }
    }
    #endregion   
}
