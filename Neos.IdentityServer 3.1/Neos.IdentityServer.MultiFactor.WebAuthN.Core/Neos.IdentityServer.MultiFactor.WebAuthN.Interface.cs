//******************************************************************************************************************************************************************************************//
// Copyright (c) 2022 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//

using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public interface IWebAuthN
    {
        AssertionOptions GetAssertionOptions(IEnumerable<PublicKeyCredentialDescriptor> allowedCredentials, UserVerificationRequirement? userVerification, AuthenticationExtensionsClientInputs extensions = null);
        Task<AssertionVerificationResult> SetAssertionResult(AuthenticatorAssertionRawResponse assertionResponse, AssertionOptions originalOptions, byte[] storedPublicKey, uint storedSignatureCounter, IsUserHandleOwnerOfCredentialIdAsync isUserHandleOwnerOfCredentialIdCallback, byte[] requestTokenBindingId = null);
        Task<CredentialMakeResult> SetRegisterCredentialResult(AuthenticatorAttestationRawResponse attestationResponse, CredentialCreateOptions origChallenge, IsCredentialIdUniqueToUserAsyncDelegate isCredentialIdUniqueToUser, byte[] requestTokenBindingId = null);
        CredentialCreateOptions GetRegisterCredentialOptions(Fido2User user, List<PublicKeyCredentialDescriptor> excludeCredentials, AuthenticationExtensionsClientInputs extensions = null);
        CredentialCreateOptions GetRegisterCredentialOptions(Fido2User user, List<PublicKeyCredentialDescriptor> excludeCredentials, AuthenticatorSelection authenticatorSelection, AttestationConveyancePreference attestationPreference, AuthenticationExtensionsClientInputs extensions = null);
    }
}
