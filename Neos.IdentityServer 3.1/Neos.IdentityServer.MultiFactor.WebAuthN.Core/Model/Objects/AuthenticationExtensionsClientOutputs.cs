//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
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
using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Objects
{
    public class AuthenticationExtensionsClientOutputs
    {
        private bool _hmacSecret;

        /// <summary>
        /// This extension allows for passing of conformance tests
        /// </summary>
        [JsonProperty("example.extension", NullValueHandling = NullValueHandling.Ignore)]
        public object Example { get; set; }

        /// <summary>
        /// This extension allows WebAuthn Relying Parties that have previously registered a credential using the legacy FIDO JavaScript APIs to request an assertion.
        /// https://www.w3.org/TR/webauthn/#sctn-appid-extension
        /// </summary>
        [JsonProperty("appid", NullValueHandling = NullValueHandling.Ignore)]
        public bool AppID { get; set; }

        /// <summary>
        /// This extension allows for a simple form of transaction authorization. A Relying Party can specify a prompt string, intended for display on a trusted device on the authenticator.
        /// https://www.w3.org/TR/webauthn/#sctn-simple-txauth-extension
        /// </summary>
        [JsonProperty("txAuthSimple", NullValueHandling = NullValueHandling.Ignore)]
        public string SimpleTransactionAuthorization { get; set; }

        /// <summary>
        /// This extension allows images to be used as transaction authorization prompts as well. This allows authenticators without a font rendering engine to be used and also supports a richer visual appearance.
        /// https://www.w3.org/TR/webauthn/#sctn-generic-txauth-extension
        /// </summary>
        [JsonProperty("txAuthGenericArg", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] GenericTransactionAuthorization { get; set; }

        /// <summary>
        /// This extension allows a WebAuthn Relying Party to guide the selection of the authenticator that will be leveraged when creating the credential. It is intended primarily for Relying Parties that wish to tightly control the experience around credential creation.
        /// https://www.w3.org/TR/webauthn/#sctn-authenticator-selection-extension
        /// </summary>
        [JsonProperty("authnSel", NullValueHandling = NullValueHandling.Ignore)]
        public bool AuthenticatorSelection { get; set; }

        /// <summary>
        /// This extension enables the WebAuthn Relying Party to determine which extensions the authenticator supports.
        /// https://www.w3.org/TR/webauthn/#sctn-supported-extensions-extension
        /// </summary>
        [JsonProperty("exts", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Extensions { get; set; }

        /// <summary>
        /// This extension enables use of a user verification index.
        /// https://www.w3.org/TR/webauthn/#sctn-uvi-extension
        /// </summary>
        [JsonProperty("uvi", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] UserVerificationIndex { get; set; }

        /// <summary>
        /// This extension provides the authenticator's current location to the WebAuthn WebAuthn Relying Party.
        /// https://www.w3.org/TR/webauthn/#sctn-location-extension
        /// </summary>
        [JsonProperty("loc", NullValueHandling = NullValueHandling.Ignore)]
        public GeoCoordinatePortable.GeoCoordinate Location { get; set; }

        /// <summary>
        /// This extension enables use of a user verification method.
        /// https://www.w3.org/TR/webauthn/#sctn-uvm-extension
        /// </summary>
        [JsonProperty("uvm", NullValueHandling = NullValueHandling.Ignore)]
        public ulong[][] UserVerificationMethod { get; set; }

        /// <summary>
        /// This extension allows WebAuthn Relying Parties to specify the desired performance bounds for selecting biometric authenticators as candidates to be employed in a registration ceremony.
        /// https://www.w3.org/TR/webauthn/#sctn-authenticator-biometric-criteria-extension
        /// </summary>
        [JsonProperty("biometricPerfBounds", NullValueHandling = NullValueHandling.Ignore)]
        public bool BiometricAuthenticatorPerformanceBounds { get; set; }


        /// CredProtect and Others
        /// <summary>
        /// This extension is used by the platform to retrieve a symmetric secret from the authenticator when it needs to encrypt or decrypt data using that symmetric secret. This symmetric secret is scoped to a credential. The authenticator and the platform each only have the part of the complete secret to prevent offline attacks. This extension can be used to maintain different secrets on different machines.
        /// https://fidoalliance.org/specs/fido2/fido-client-to-authenticator-protocol-v2.1-rd-20191217.html#sctn-hmac-secret-extension
        /// </summary>
        [JsonProperty("hmacCreateSecret", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HmacSecret
        {
            get
            {
                // Treat false as null, so that it is not serialized.
                return _hmacSecret ? true : (bool?)null;
            }
            set
            {
                _hmacSecret = (value == true);
            }
        }

        /// <summary>
        /// This extension indicates that the authenticator supports enhanced protection mode for the credentials created on the authenticator.
        /// If present, verify that the credentialProtectionPolicy value is one of following values: userVerificationOptional, userVerificationOptionalWithCredentialIDList, userVerificationRequired
        /// https://fidoalliance.org/specs/fido2/fido-client-to-authenticator-protocol-v2.1-rd-20191217.html#sctn-credProtect-extension
        /// </summary>
        [JsonProperty("credentialProtectionPolicy", NullValueHandling = NullValueHandling.Ignore)]
        public UserVerification? CredProtect { get; set; }
    }
}
