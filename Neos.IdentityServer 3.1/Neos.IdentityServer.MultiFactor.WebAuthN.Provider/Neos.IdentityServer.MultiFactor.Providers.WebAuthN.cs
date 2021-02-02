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
using Microsoft.IdentityModel.Logging;
using Neos.IdentityServer.MultiFactor.Common;
using Neos.IdentityServer.MultiFactor.Data;
using Neos.IdentityServer.MultiFactor.WebAuthN.Metadata;
using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public class NeosWebAuthNProvider : BaseExternalProvider, IWebAuthNProvider
    {
        private bool _isrequired = false;
        private bool _isinitialized = false;
        private ForceWizardMode _forceenrollment = ForceWizardMode.Disabled;
        private IWebAuthN _webathn;
        private SimpleMetadataService _simplemetadataservice;

        public MFAConfig Config { get; set; }
        public bool DirectLogin { get; private set; }
        public int ChallengeSize { get; private set; }
        public string ConveyancePreference { get; private set; }
        public string Attachement { get; private set; }
        public bool Extentions { get; private set; }
        public bool Location { get; private set; }
        public bool UserVerificationIndex { get; private set; }
        public bool UserVerificationMethod { get; private set; }
        public string UserVerificationRequirement { get; private set; }
        public bool RequireResidentKey { get; private set; }
        public bool? HmacSecret { get; private set; }
        public UserVerification? CredProtect { get; private set; }
        public bool? EnforceCredProtect { get; private set; }
        public WebAuthNPinRequirements PinRequirements { get; set; } = WebAuthNPinRequirements.Null;

        /// <summary>
        /// Constructor
        /// </summary>
        public NeosWebAuthNProvider():base()
        {
            Trace.WriteLine("IMetadataService initialization");
            _simplemetadataservice = new SimpleMetadataService(new List<IMetadataRepository> { new StaticMetadataRepository(DateTime.Now.AddMinutes(1).ToUniversalTime()) });
            var result = Task.Factory.StartNew(_simplemetadataservice.Initialize);
            result.Wait(15000);
        }

        /// <summary>
        /// Kind property implementation
        /// </summary>
        public override PreferredMethod Kind
        {
            get { return PreferredMethod.Biometrics; }
        }

        /// <summary>
        /// IsBuiltIn property implementation
        /// </summary>
        public override bool IsBuiltIn
        {
            get { return true; }
        }

        /// <summary>
        /// IsRequired property implementation
        /// </summary>
        public override bool IsRequired
        {
            get { return _isrequired; }
            set { _isrequired = value; }
        }

        /// <summary>
        /// CanBeDisabled property implementation
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
        /// AllowOverride property implmentation
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
            get { return _forceenrollment; }
            set { _forceenrollment = value; }
        }

        /// <summary>
        /// Name property implementation
        /// </summary>
        public override string Name
        {
            get { return "Neos.Provider.Biometrics"; }
        }

        /// <summary>
        /// Description property implementation
        /// </summary>
        public override string Description
        {
            get
            {
                string independent = "Biometrics Multi-Factor Provider";
                ResourcesLocale Resources = null;
                if (CultureInfo.DefaultThreadCurrentUICulture != null)
                    Resources = new ResourcesLocale(CultureInfo.DefaultThreadCurrentUICulture.LCID);
                else
                    Resources = new ResourcesLocale(CultureInfo.CurrentUICulture.LCID);
                string res = Resources.GetString(ResourcesLocaleKind.Html, "PROVIDERBIODESCRIPTION");
                if (!string.IsNullOrEmpty(res))
                    return res;
                else
                    return independent;
            }
        }

        /// <summary>
        /// GetUILabel method implementation
        /// </summary>
        public override string GetUILabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOUIOTPLabel");  
        }

        /// <summary>
        /// GetWizardUILabel method implementation
        /// </summary>
        public override string GetWizardUILabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOUIWIZLabel"); 
        }

        /// <summary>
        /// GetWizardUIComment method implementation
        /// </summary>
        public override string GetWizardUIComment(AuthenticationContext ctx) 
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOUIWIZComment");
        }

        /// <summary>
        /// GetWizardLinkLabel method implementation
        /// </summary>
        public override string GetWizardLinkLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOWIZEnroll"); 
        }

        /// <summary>
        /// GetUICFGLabel method implementation
        /// </summary>
        public override string GetUICFGLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOUICFGLabel");  
        }

        /// <summary>
        /// GetUIMessage method implementation
        /// </summary>
        public override string GetUIMessage(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOUIMessage"); 
        }

        /// <summary>
        /// GetUIListOptionLabel method implementation
        /// </summary>
        public override string GetUIListOptionLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOUIListOptionLabel");  
        }

        /// <summary>
        /// GetUIListChoiceLabel method implementation
        /// </summary>
        public override string GetUIListChoiceLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOUIListChoiceLabel");  
        }

        /// <summary>
        /// GetUIConfigLabel method implementation
        /// </summary>
        public override string GetUIConfigLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOUIConfigLabel");  
        }

        /// <summary>
        /// GetUIChoiceLabel method implementation
        /// </summary>
        public override string GetUIChoiceLabel(AuthenticationContext ctx, AvailableAuthenticationMethod method = null)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOUIChoiceLabel");  
        }

        /// <summary>
        /// GetUIWarningInternetLabel method implmentation
        /// </summary>
        public override string GetUIWarningInternetLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "GLOBALWarnOverNetwork");
        }

        /// <summary>
        /// GetUIWarningThirdPartyLabel method implementation
        /// </summary>
        public override string GetUIWarningThirdPartyLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "GLOBALWarnThirdParty");
        }

        /// <summary>
        /// GetUIDefaultChoiceLabel method implementation
        /// </summary>
        public override string GetUIDefaultChoiceLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "GLOBALListChoiceDefaultLabel");
        }

        /// <summary>
        /// GetUIEnrollmentTaskLabel method implementation
        /// </summary>
        public override string GetUIEnrollmentTaskLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOUIEnrollTaskLabel");  
        }

        /// <summary>
        /// GetUIEnrollValidatedLabel method implementation
        /// </summary>
        public override string GetUIEnrollValidatedLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOUIEnrollValidatedLabel");  
        }

        /// <summary>
        /// GetUIAccountManagementLabel method implementation
        /// </summary>
        public override string GetUIAccountManagementLabel(AuthenticationContext ctx)
        {
            return string.Empty;
        }

        /// <summary>
        /// GetAccountManagementUrl method implmentation
        /// </summary>
        public override string GetAccountManagementUrl(AuthenticationContext ctx)
        {
            return null;
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
            try
            {
                List<WebAuthNCredentialInformation> wcreds = GetUserStoredCredentials(ctx);
                return (wcreds.Count > 0);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// IsMethodElementRequired implementation
        /// </summary>
        public override bool IsUIElementRequired(AuthenticationContext ctx, RequiredMethodElements element)
        {
            switch (element)
            {
                case RequiredMethodElements.BiometricInputRequired:
                    return true;
                case RequiredMethodElements.BiometricParameterRequired:
                    return true;
                case RequiredMethodElements.BiometricLinkRequired:
                    return true;
                case RequiredMethodElements.PinLinkRequired:
                    return this.PinRequired;
                case RequiredMethodElements.PinParameterRequired:
                    return this.PinRequired;
                case RequiredMethodElements.PinInputRequired:
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
                    Trace.WriteLine("WebAuthNProvider Initialize");
                    if (externalsystem is WebAuthNProviderParams)
                    {
                        WebAuthNProviderParams param = externalsystem as WebAuthNProviderParams;
                        Config = param.Config;
                        Enabled = param.Enabled;
                        IsRequired = param.IsRequired;
                        WizardEnabled = param.EnrollWizard;
                        ForceEnrollment = param.ForceWizard;
                        PinRequired = param.PinRequired;
                        PinRequirements = param.PinRequirements;
                        DirectLogin = param.DirectLogin;
                        ConveyancePreference = param.Options.AttestationConveyancePreference;
                        Attachement = param.Options.AuthenticatorAttachment;
                        Extentions = param.Options.Extensions;
                        Location = param.Options.Location;
                        UserVerificationIndex = param.Options.UserVerificationIndex;
                        UserVerificationMethod = param.Options.UserVerificationMethod;
                        UserVerificationRequirement = param.Options.UserVerificationRequirement;
                        RequireResidentKey = param.Options.RequireResidentKey;
                        ChallengeSize = param.Configuration.ChallengeSize;
                        HmacSecret = param.Options.HmacSecret;
                        CredProtect = (UserVerification?)param.Options.CredProtect;
                        EnforceCredProtect = param.Options.EnforceCredProtect;
                        IdentityModelEventSource.ShowPII = param.Configuration.ShowPII;
                        Fido2Configuration fido = new Fido2Configuration()
                        {
                            ServerDomain = param.Configuration.ServerDomain,
                            ServerName = param.Configuration.ServerName,
                            Origin = param.Configuration.Origin,
                            Timeout = param.Configuration.Timeout,
                            TimestampDriftTolerance = param.Configuration.TimestampDriftTolerance,
                            ChallengeSize = param.Configuration.ChallengeSize,
                            RequireValidAttestationRoot = param.Configuration.RequireValidAttestationRoot
                        };
                        Trace.WriteLine("WebAuthNAdapter Create");                       
                        _webathn = new WebAuthNAdapter(fido, _simplemetadataservice);
                        _isinitialized = true;
                        Trace.WriteLine("WebAuthNAdapter Created");
                        Trace.WriteLine("WebAuthNProvider Initialized");
                        return;
                    }
                    else
                        throw new InvalidCastException("Invalid WebAuthN Provider !");                   
                }
            }
            catch (Exception ex)
            {
                this.Enabled = false;
                throw ex;
            }
        }

        /// <summary>
        /// GetAuthenticationMethods method implementation
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
                AvailableAuthenticationMethod item = new AvailableAuthenticationMethod
                {
                    IsDefault = true,
                    IsRemote = true,
                    IsTwoWay = true,
                    IsSendBack = false,
                    RequiredEmail = false,
                    RequiredPhone = false,
                    RequiredCode = false,
                    Method = AuthenticationResponseKind.Biometrics,
                    RequiredPin = false,
                    RequiredBiometrics = true
                };
                result.Add(item);
                SaveSessionData(this.Kind, ctx, result);
            }
            return result;

        }

        /// <summary>
        /// GetAuthenticationContext method implementation
        /// </summary>
        public override void GetAuthenticationContext(AuthenticationContext ctx)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            AvailableAuthenticationMethod result = GetSelectedAuthenticationMethod(ctx);
            ctx.PinRequired = result.RequiredPin;
            ctx.IsRemote = result.IsRemote;
            ctx.IsTwoWay = result.IsTwoWay;
            ctx.IsSendBack = result.IsSendBack;
            ctx.PreferredMethod = ctx.PreferredMethod;
            ctx.SelectedMethod = result.Method;
            ctx.ExtraInfos = result.ExtraInfos;
            ctx.DirectLogin = this.DirectLogin;
        }

        /// <summary>
        /// GetAuthenticationData method implementation
        /// </summary>
        public override string GetAuthenticationData(AuthenticationContext ctx)
        {
            if (ctx.UIMode == ProviderPageMode.EnrollBiometrics)
                return GetRegisterCredentialOptions(ctx);
            else
                return GetLoginAssertionsOptions(ctx);
        }

        /// <summary>
        /// ReleaseAuthenticationData method implementation
        /// </summary>
        public override void ReleaseAuthenticationData(AuthenticationContext ctx)
        {
            if (ctx.UIMode == ProviderPageMode.EnrollBiometrics)
                ctx.CredentialOptions = string.Empty;
            else
                ctx.AssertionOptions = string.Empty;
        }

        /// <summary>
        /// PostAuthenticationRequest method implementation
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
        /// SetAuthenticationResult method implementation
        /// </summary>
        public override int SetAuthenticationResult(AuthenticationContext ctx, string result)
        {
            string error = string.Empty;
            return (int)SetAuthenticationResult(ctx, result, out error);
        }

        /// <summary>
        /// SetAuthenticationResult method implementation
        /// </summary>
        public override int SetAuthenticationResult(AuthenticationContext ctx, string result, out string error)
        {
            if (!IsInitialized)
                throw new Exception("Provider not initialized !");
            if (ctx.UIMode == ProviderPageMode.EnrollBiometrics)
                return (int)SetRegisterCredentialResult(ctx, result, out error);
            else
                return (int)SetLoginAssertionResult(ctx, result, out error);
        }

        #region IWebAuthNProvider
        /// <summary>
        /// GetManageLinkLabel method implementation
        /// </summary>
        public string GetManageLinkLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOUIEnrollManageLinkLabel");
        }

        public string GetDeleteLinkLabel(AuthenticationContext ctx)
        {
            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
            return Resources.GetString(ResourcesLocaleKind.Html, "BIOUIEnrollDeleteLinkLabel");
        }

        /// <summary>
        /// GetUserStoredCredentials method implementation
        /// </summary>
        public List<WebAuthNCredentialInformation> GetUserStoredCredentials(AuthenticationContext ctx)
        {
            return GetUserStoredCredentials(ctx.UPN);
        }

        /// <summary>
        /// GetUserStoredCredentials method implementation
        /// </summary>
        public List<WebAuthNCredentialInformation> GetUserStoredCredentials(string upn)
        {
            List<WebAuthNCredentialInformation> wcreds = new List<WebAuthNCredentialInformation>();
            MFAWebAuthNUser user = RuntimeRepository.GetUser(Config, upn);
            try
            {
                if (user != null)
                {

                    List<MFAUserCredential> creds = RuntimeRepository.GetCredentialsByUser(Config, user);
                    if (creds.Count == 0)
                        return wcreds;
                    foreach (MFAUserCredential st in creds)
                    {
                        WebAuthNCredentialInformation itm = new WebAuthNCredentialInformation()
                        {
                            CredentialID = HexaEncoding.GetHexStringFromByteArray(st.Descriptor.Id),
                            AaGuid = st.AaGuid,
                            CredType = st.CredType,
                            RegDate = st.RegDate,
                            SignatureCounter = st.SignatureCounter
                        };
                        if (st.Descriptor.Type != null)
                            itm.Type = EnumExtensions.ToEnumMemberValue(st.Descriptor.Type.Value);
                        wcreds.Add(itm);
                    }
                    return wcreds.OrderByDescending(c => c.RegDate).ToList();
                }
                else
                {
                    Log.WriteEntry(string.Format("{0}\r\n{1}", upn, "User does not exists !"), EventLogEntryType.Error, 5000);
                    throw new ArgumentNullException(string.Format("{0}\r\n{1}", upn, "User does not exists !")); ;
                }
            }
            catch (Exception e)
            {
                Log.WriteEntry(string.Format("{0}\r\n{1}", upn, e.Message), EventLogEntryType.Error, 5000);
                throw e;
            }
        }

        /// <summary>
        /// RemoveUserStoredCredentials method implementation
        /// </summary>
        public void RemoveUserStoredCredentials(AuthenticationContext ctx, string credentialid)
        {
            RemoveUserStoredCredentials(ctx.UPN, credentialid);
        }

        /// <summary>
        /// RemoveUserStoredCredentials method implementation
        /// </summary>
        public void RemoveUserStoredCredentials(string upn, string credentialid)
        {
            try
            {
                MFAWebAuthNUser user = RuntimeRepository.GetUser(Config, upn);
                if (user != null)
                    RuntimeRepository.RemoveUserCredential(Config, user, credentialid);
                else
                {
                    Log.WriteEntry(string.Format("{0}\r\n{1}", upn, "User does not exists !"), EventLogEntryType.Error, 5000);
                    throw new ArgumentNullException(string.Format("{0}\r\n{1}", upn, "User does not exists !")); ;
                }
            }
            catch (Exception e)
            {
                Log.WriteEntry(string.Format("{0}\r\n{1}", upn, e.Message), EventLogEntryType.Error, 5000);
                throw e;
            }
        }

        /// <summary>
        /// GetRegisterCredentialOptions method implementation
        /// </summary>
        private string GetRegisterCredentialOptions(AuthenticationContext ctx)
        {
            try
            {
                if (string.IsNullOrEmpty(ctx.UPN))
                    throw new ArgumentNullException(ctx.UPN);

                string attType = this.ConveyancePreference;                  // none, direct, indirect
                string authType = this.Attachement;                          // <empty>, platform, cross-platform
                string userVerification = this.UserVerificationRequirement;  // preferred, required, discouraged
                bool requireResidentKey = this.RequireResidentKey;           // true,false

                MFAWebAuthNUser user = RuntimeRepository.GetUser(Config, ctx.UPN);
                if (user != null)
                {
                    List<MFAPublicKeyCredentialDescriptor> existingKeys = RuntimeRepository.GetCredentialsByUser(Config, user).Select(c => c.Descriptor).ToList();

                    // 3. Create options
                    AuthenticatorSelection authenticatorSelection = new AuthenticatorSelection
                    {
                        RequireResidentKey = requireResidentKey,
                        UserVerification = userVerification.ToEnum<UserVerificationRequirement>()
                    };
                    if (!string.IsNullOrEmpty(authType))
                        authenticatorSelection.AuthenticatorAttachment = authType.ToEnum<AuthenticatorAttachment>();

                    AuthenticationExtensionsClientInputs exts = new AuthenticationExtensionsClientInputs()
                    {
                        Extensions = this.Extentions,
                        UserVerificationIndex = this.UserVerificationIndex,
                        Location = this.Location,
                        UserVerificationMethod = this.UserVerificationMethod,
                        EnforceCredProtect = this.EnforceCredProtect,
                        CredProtect = this.CredProtect,
                        HmacSecret = this.HmacSecret,
                        BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds
                        {
                            FAR = float.MaxValue,
                            FRR = float.MaxValue
                        }
                    };

                    RegisterCredentialOptions options = _webathn.GetRegisterCredentialOptions(user.ToCore(), existingKeys.ToCore(), authenticatorSelection, attType.ToEnum<AttestationConveyancePreference>(), exts);
                    string result = options.ToJson();
                    ctx.CredentialOptions = result;
                    return result;
                }
                else
                {
                    Log.WriteEntry(string.Format("{0}\r\n{1}", ctx.UPN, "User does not exists !"), EventLogEntryType.Error, 5000);
                    string result = (new RegisterCredentialOptions { Status = "error", ErrorMessage = string.Format("{0}", "User does not exists !") }).ToJson();
                    ctx.CredentialOptions = result;
                    return result;
                }
            }
            catch (Exception e)
            {
                Log.WriteEntry(string.Format("{0}\r\n{1}", ctx.UPN, e.Message), System.Diagnostics.EventLogEntryType.Error, 5000);
                string result = (new RegisterCredentialOptions { Status = "error", ErrorMessage = string.Format("{0}{1}", e.Message, e.InnerException != null ? " (" + e.InnerException.Message + ")" : "") }).ToJson();
                ctx.CredentialOptions = result;
                return result;
            }
        }

        /// <summary>
        /// SetRegisterCredentialResult method implementation
        /// </summary>
        private int SetRegisterCredentialResult(AuthenticationContext ctx, string jsonResponse, out string error)
        {
            bool isDeserialized = false;
            try
            {
                string jsonOptions = ctx.CredentialOptions;
                if (string.IsNullOrEmpty(jsonOptions))
                    throw new ArgumentNullException(jsonOptions);
                if (string.IsNullOrEmpty(jsonResponse))
                    throw new ArgumentNullException(jsonResponse);

                MFAWebAuthNUser user = RuntimeRepository.GetUser(Config, ctx.UPN);
                if (user != null)
                {
                    RegisterCredentialOptions options = RegisterCredentialOptions.FromJson(jsonOptions);

                    bool callback(IsCredentialIdUniqueToUserParams args)
                    {
                        var users = RuntimeRepository.GetUsersByCredentialId(Config, user, args.CredentialId);
                        if (users.Count > 0)
                            return false;
                        return true;
                    }

                    AuthenticatorAttestationRawResponse attestationResponse = JsonConvert.DeserializeObject<AuthenticatorAttestationRawResponse>(jsonResponse);
                    isDeserialized = true;
                    RegisterCredentialResult success = _webathn.SetRegisterCredentialResult(attestationResponse, options, callback);

                    RuntimeRepository.AddUserCredential(Config, options.User.FromCore(), new MFAUserCredential
                    {
                        Descriptor = new MFAPublicKeyCredentialDescriptor(success.Result.CredentialId),
                        PublicKey = success.Result.PublicKey,
                        UserHandle = success.Result.User.Id,
                        SignatureCounter = success.Result.Counter,
                        CredType = success.Result.CredType,
                        RegDate = DateTime.Now,
                        AaGuid = success.Result.Aaguid
                    });
                    error = string.Empty;
                    return (int)AuthenticationResponseKind.Biometrics;
                }
                else
                {
                    Log.WriteEntry(string.Format("{0}\r\n{1}", ctx.UPN, "User does not exists !"), System.Diagnostics.EventLogEntryType.Error, 5000);
                    error = string.Format("{0}\r\n{1}", ctx.UPN, "User does not exists !");
                    return (int)AuthenticationResponseKind.Error;
                }
            }
            catch (Exception e)
            {
                if (isDeserialized)
                    Log.WriteEntry(string.Format("{0}\r\n{1}", ctx.UPN, e.Message), EventLogEntryType.Error, 5000);
                else
                    Log.WriteEntry(string.Format("{0}\r\n{1}", ctx.UPN, jsonResponse), EventLogEntryType.Error, 5000);
                error = e.Message;
                return (int)AuthenticationResponseKind.Error;
            }
        }

        /// <summary>
        /// GetLoginAssertionsOptions method implementation
        /// </summary>
        private string GetLoginAssertionsOptions(AuthenticationContext ctx)
        {
            try
            {
                List<MFAPublicKeyCredentialDescriptor> existingCredentials = new List<MFAPublicKeyCredentialDescriptor>();

                if (!string.IsNullOrEmpty(ctx.UPN))
                {
                    var user = RuntimeRepository.GetUser(Config, ctx.UPN);
                    if (user == null)
                        throw new ArgumentException("Username was not registered");
                    existingCredentials = RuntimeRepository.GetCredentialsByUser(Config, user).Select(c => c.Descriptor).ToList();
                }

                AuthenticationExtensionsClientInputs exts = new AuthenticationExtensionsClientInputs()
                {
                    SimpleTransactionAuthorization = "FIDO",
                    GenericTransactionAuthorization = new TxAuthGenericArg
                    {
                        ContentType = "text/plain",
                        Content = new byte[] { 0x46, 0x49, 0x44, 0x4F } 
                    },
                    UserVerificationIndex = this.UserVerificationIndex,
                    Location = this.Location,
                    UserVerificationMethod = this.UserVerificationMethod,
                    EnforceCredProtect = this.EnforceCredProtect,
                    CredProtect = this.CredProtect,
                    HmacSecret = this.HmacSecret
                };

                UserVerificationRequirement uv = this.UserVerificationRequirement.ToEnum<UserVerificationRequirement>();
                AssertionOptions options = _webathn.GetAssertionOptions(existingCredentials.ToCore(), uv, exts);
                string result = options.ToJson();
                ctx.AssertionOptions = result;
                return result;
            }
            catch (Exception e)
            {
                Log.WriteEntry(string.Format("{0}\r\n{1}", ctx.UPN, e.Message), EventLogEntryType.Error, 5000);
                string result = (new AssertionOptions { Status = "error", ErrorMessage = string.Format("{0}{1}", e.Message, e.InnerException != null ? " (" + e.InnerException.Message + ")" : "") }).ToJson();
                ctx.AssertionOptions = result;
                return result;                
            }
        }

        /// <summary>
        /// SetLoginAssertionResult method implementation
        /// </summary>
        private int SetLoginAssertionResult(AuthenticationContext ctx, string jsonResponse, out string error)
        {
            bool isDeserialized = false;
            try
            {
                string jsonOptions = ctx.AssertionOptions;
                if (string.IsNullOrEmpty(jsonOptions))
                    throw new ArgumentNullException(jsonOptions);
                if (string.IsNullOrEmpty(jsonResponse))
                    throw new ArgumentNullException(jsonResponse);

                MFAWebAuthNUser user = RuntimeRepository.GetUser(Config, ctx.UPN);
                if (user != null)
                {
                    AssertionOptions options = AssertionOptions.FromJson(jsonOptions);
                    try
                    {
                        AuthenticatorAssertionRawResponse clientResponse = JsonConvert.DeserializeObject<AuthenticatorAssertionRawResponse>(jsonResponse);
                        isDeserialized = true;

                        MFAUserCredential creds = RuntimeRepository.GetCredentialById(Config, user, clientResponse.Id);
                        if (creds == null)
                        {
                            throw new Exception("Unknown credentials");
                        }

                        // Check Replay
                        AuthenticatorData authData = new AuthenticatorData(clientResponse.Response.AuthenticatorData);
                        uint authCounter = authData.SignCount;
                        uint storedCounter = creds.SignatureCounter;
                        if ((authCounter <= 0) || (authCounter <= storedCounter))
                        {
                            ResourcesLocale Resources = new ResourcesLocale(ctx.Lcid);
                            throw new Exception(Resources.GetString(ResourcesLocaleKind.Html, "BIOERRORAUTHREPLAY"));
                        }

                        bool callback(IsUserHandleOwnerOfCredentialIdParams args)
                        {
                            var storedCreds = RuntimeRepository.GetCredentialsByUserHandle(Config, user, args.UserHandle);
                            return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
                        }
                        AssertionVerificationResult res = _webathn.SetAssertionResult(clientResponse, options, creds.PublicKey, storedCounter, callback);
                        RuntimeRepository.UpdateCounter(Config, user, res.CredentialId, res.Counter);

                        if (!authData.UserPresent || !authData.UserVerified)
                        {
                            switch (creds.CredType)
                            {
                                case "none":
                                    ctx.PinRequirements = (this.PinRequirements.HasFlag(WebAuthNPinRequirements.None));
                                    break;
                                case "android-key":
                                    ctx.PinRequirements = (this.PinRequirements.HasFlag(WebAuthNPinRequirements.AndroidKey));
                                    break;
                                case "android-safetynet":
                                    ctx.PinRequirements = (this.PinRequirements.HasFlag(WebAuthNPinRequirements.AndroidSafetyNet));
                                    break;
                                case "fido-u2f":
                                    ctx.PinRequirements = (this.PinRequirements.HasFlag(WebAuthNPinRequirements.Fido2U2f));
                                    break;
                                case "packed":
                                    ctx.PinRequirements = (this.PinRequirements.HasFlag(WebAuthNPinRequirements.Packed));
                                    break;
                                case "tpm":
                                    ctx.PinRequirements = (this.PinRequirements.HasFlag(WebAuthNPinRequirements.TPM));
                                    break;
                                case "apple":
                                    ctx.PinRequirements = (this.PinRequirements.HasFlag(WebAuthNPinRequirements.Apple));
                                    break;
                                default:
                                    ctx.PinRequirements = false;
                                    break;
                            }
                        }
                        else
                            ctx.PinRequirements = false;
                        error = string.Empty;
                        return (int)AuthenticationResponseKind.Biometrics;
                    }
                    catch (Exception ex)
                    {
                        if (isDeserialized)
                            Log.WriteEntry(string.Format("{0}\r\n{1}", ctx.UPN, ex.Message), EventLogEntryType.Error, 5000);
                        else
                            Log.WriteEntry(string.Format("{0}\r\n{1}", ctx.UPN, jsonResponse), EventLogEntryType.Error, 5000);
                        error = ex.Message;
                        return (int)AuthenticationResponseKind.Error;
                    }
                }
                else
                {
                    Log.WriteEntry(string.Format("{0}\r\n{1}", ctx.UPN, "User does not exists !"), EventLogEntryType.Error, 5000);
                    error = string.Format("{0}\r\n{1}", ctx.UPN, "User does not exists !");
                    return (int)AuthenticationResponseKind.Error;
                }
            }
            catch (Exception e)
            {
                Log.WriteEntry(string.Format("{0}\r\n{1}", ctx.UPN, e.Message), EventLogEntryType.Error, 5000);
                error = e.Message;
                return (int)AuthenticationResponseKind.Error;
            }
        }
        #endregion
    }


    /// <summary>
    /// WebAuthNTypesExtensions class 
    /// Maps differents classes MFA/FIDO Components
    /// </summary>
    internal static class WebAuthNTypesExtensions
    {
        internal static MFAWebAuthNUser FromCore(this Fido2User user)
        {
            var usr = new MFAWebAuthNUser()
            {
                DisplayName = user.DisplayName,
                Id = user.Id,
                Name = user.Name
            };
            return usr;
        }

        internal static Fido2User ToCore(this MFAWebAuthNUser user)
        {
            var usr = new Fido2User()
            {
                DisplayName = user.DisplayName,
                Id = user.Id,
                Name = user.Name
            };
            return usr;
        }

        internal static MFAPublicKeyCredentialDescriptor FromCore(this PublicKeyCredentialDescriptor data)
        {
            var creds = new MFAPublicKeyCredentialDescriptor()
            {
                Id = data.Id,
                Type = (MFAPublicKeyCredentialType)data.Type
            };
            if (data.Transports != null)
            { 
                creds.Transports = new MFAAuthenticatorTransport[data.Transports.Length];
                for (int i = 0; i < data.Transports.Length; i++)
                {
                    creds.Transports[i] = (MFAAuthenticatorTransport)data.Transports[i];
                }
            }
            return creds;
        }

        internal static PublicKeyCredentialDescriptor ToCore(this MFAPublicKeyCredentialDescriptor data)
        {
            var creds = new PublicKeyCredentialDescriptor()
            {
                Id = data.Id,
                Type = (PublicKeyCredentialType)data.Type
            };
            if (data.Transports != null)
            {
                creds.Transports = new AuthenticatorTransport[data.Transports.Length];
                for (int i = 0; i < data.Transports.Length; i++)
                {
                    creds.Transports[i] = (AuthenticatorTransport)data.Transports[i];
                }
            }
            return creds;
        }

        internal static List<MFAPublicKeyCredentialDescriptor> FromCore(this List<PublicKeyCredentialDescriptor> data)
        {
            var creds = new List<MFAPublicKeyCredentialDescriptor>();
            foreach (PublicKeyCredentialDescriptor Desc in data)
            {
                MFAPublicKeyCredentialDescriptor res = Desc.FromCore();
                creds.Add(res);
            }
            return creds;
        }

        internal static List<PublicKeyCredentialDescriptor> ToCore(this List<MFAPublicKeyCredentialDescriptor> data)
        {
            var creds = new List<PublicKeyCredentialDescriptor>();
            foreach(MFAPublicKeyCredentialDescriptor MFADesc in data)
            {
                PublicKeyCredentialDescriptor res = MFADesc.ToCore();
                creds.Add(res);
            }
            return creds;
        }
    }
}
