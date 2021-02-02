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
using System.Security.Cryptography;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;
using Neos.IdentityServer.MultiFactor;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    /// <summary>
    /// Public API for parsing and veriyfing FIDO2 attestation & assertion responses.
    /// </summary>
    public partial class WebAuthNAdapter : IWebAuthN
    {
        private readonly Fido2Configuration _config;
        private readonly RandomNumberGenerator _crypto;
        private readonly IMetadataService _metadataService;

        /// <summary>
        /// WebAuthNAdapter constructor
        /// </summary>
        public WebAuthNAdapter()
        {
        }

        /// <summary>
        /// WebAuthNAdapter constructor
        /// </summary>
        public WebAuthNAdapter(Fido2Configuration config, IMetadataService metadataService = null)
        {
            _config = config;
            _crypto = RandomNumberGenerator.Create();
            _metadataService = metadataService;
        }

        #region CredentialCreateOptions
        /// <summary>
        /// GetCredentialCreateOptions method implementation
        /// </summary>
        /// <returns>CredentialCreateOptions including a challenge to be sent to the browser/authr to create new credentials</returns>
        /// <param name="excludeCredentials">Recommended. This member is intended for use by Relying Parties that wish to limit the creation of multiple credentials for the same account on a single authenticator.The client is requested to return an error if the new credential would be created on an authenticator that also contains one of the credentials enumerated in this parameter.</param>
        public RegisterCredentialOptions GetRegisterCredentialOptions(Fido2User user, List<PublicKeyCredentialDescriptor> excludeCredentials, AuthenticationExtensionsClientInputs extensions = null)
        {
            return GetRegisterCredentialOptions(user, excludeCredentials, AuthenticatorSelection.Default, AttestationConveyancePreference.None, extensions);
        }

        /// <summary>
        /// GetCredentialCreateOptions method implementation
        /// </summary>
        /// <returns>CredentialCreateOptions including a challenge to be sent to the browser/authr to create new credentials</returns>
        /// <param name="attestationPreference">This member is intended for use by Relying Parties that wish to express their preference for attestation conveyance. The default is none.</param>
        /// <param name="excludeCredentials">Recommended. This member is intended for use by Relying Parties that wish to limit the creation of multiple credentials for the same account on a single authenticator.The client is requested to return an error if the new credential would be created on an authenticator that also contains one of the credentials enumerated in this parameter.</param>
        public RegisterCredentialOptions GetRegisterCredentialOptions(Fido2User user, List<PublicKeyCredentialDescriptor> excludeCredentials, AuthenticatorSelection authenticatorSelection, AttestationConveyancePreference attestationPreference, AuthenticationExtensionsClientInputs extensions = null)
        {
            var challenge = new byte[_config.ChallengeSize];
            _crypto.GetBytes(challenge);

            var options = RegisterCredentialOptions.Create(_config, challenge, user, authenticatorSelection, attestationPreference, excludeCredentials, extensions);
            return options;
        }
        #endregion

        /// <summary>
        /// Verifies the response from the browser/authr after creating new credentials
        /// </summary>
        /// <param name="attestationResponse"></param>
        /// <param name="origChallenge"></param>
        /// <returns></returns>
        public RegisterCredentialResult SetRegisterCredentialResult(AuthenticatorAttestationRawResponse attestationResponse, RegisterCredentialOptions origChallenge, IsCredentialIdUniqueToUserDelegate isCredentialIdUniqueToUser, byte[] requestTokenBindingId = null)
        {
            var parsedResponse = AuthenticatorAttestationResponse.Parse(attestationResponse);
            var success = parsedResponse.VerifyCredentialCreateOptions(origChallenge, _config, isCredentialIdUniqueToUser, _metadataService, requestTokenBindingId);
            try
            {
                return new RegisterCredentialResult
                {
                    Status = "ok",
                    ErrorMessage = string.Empty,
                    Result = success
                };
            }
            catch (Exception e)
            {
                return new RegisterCredentialResult
                {
                    Status = "error",
                    ErrorMessage = e.Message,
                    Result = success
                };
            }
        }

        /// <summary>
        /// Returns AssertionOptions including a challenge to the browser/authr to assert existing credentials and authenticate a user.
        /// </summary>
        /// <returns></returns>
        public AssertionOptions GetAssertionOptions(IEnumerable<PublicKeyCredentialDescriptor> allowedCredentials, UserVerificationRequirement? userVerification, AuthenticationExtensionsClientInputs extensions = null)
        {
            var challenge = new byte[_config.ChallengeSize];
            _crypto.GetBytes(challenge);

            var options = AssertionOptions.Create(_config, challenge, allowedCredentials, userVerification, extensions);
            return options;
        }

        /// <summary>
        /// Verifies the assertion response from the browser/authr to assert existing credentials and authenticate a user.
        /// </summary>
        /// <returns></returns>
        public AssertionVerificationResult SetAssertionResult(AuthenticatorAssertionRawResponse assertionResponse, AssertionOptions originalOptions, byte[] storedPublicKey,
                                            uint storedSignatureCounter, IsUserHandleOwnerOfCredentialId isUserHandleOwnerOfCredentialIdCallback, byte[] requestTokenBindingId = null)
        {
            var parsedResponse = AuthenticatorAssertionResponse.Parse(assertionResponse);

            var result = parsedResponse.Verify(originalOptions,
                                                            _config.Origin,
                                                            storedPublicKey,
                                                            storedSignatureCounter,
                                                            isUserHandleOwnerOfCredentialIdCallback,
                                                            requestTokenBindingId);
            return result;
        }
    }

    /// <summary>
    /// Result of parsing and verifying attestation. Used to transport Public Key back to RP
    /// </summary>
    public class RegisterCredentialResult : Fido2ResponseBase
    {
        public AttestationVerificationSuccess Result { get; internal set; }

        #region Serialization methods
        /// <summary>
        /// ToJson method implementation
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// FromJson method implementation
        /// </summary>
        public static RegisterCredentialResult FromJson(string json)
        {
            return JsonConvert.DeserializeObject<RegisterCredentialResult>(json);
        }
        #endregion
    }

    /// <summary>
    /// Callback function used to validate that the CredentialID is unique to this User
    /// </summary>
    /// <param name="credentialIdUserParams"></param>
    /// <returns></returns>
    public delegate bool IsCredentialIdUniqueToUserDelegate(IsCredentialIdUniqueToUserParams credentialIdUserParams);
    /// <summary>
    /// Callback function used to validate that the Userhandle is indeed owned of the CrendetialId
    /// </summary>
    /// <param name="credentialIdUserHandleParams"></param>
    /// <returns></returns>
    public delegate bool IsUserHandleOwnerOfCredentialId(IsUserHandleOwnerOfCredentialIdParams credentialIdUserHandleParams);
}
