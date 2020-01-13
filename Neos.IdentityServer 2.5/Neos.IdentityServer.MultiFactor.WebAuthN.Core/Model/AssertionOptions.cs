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
using System.Collections.Generic;
using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;
using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    /// <summary>
    /// Sent to the browser when we want to Assert credentials and authenticate a user
    /// </summary>
    public class AssertionOptions : Fido2ResponseBase
    {
        /// <summary>
        /// This member represents a challenge that the selected authenticator signs, along with other data, when producing an authentication assertion.See the §13.1 Cryptographic Challenges security consideration.
        /// </summary>
        [JsonProperty("challenge")]
        [JsonConverter(typeof(Base64UrlConverter))]
        public byte[] Challenge { get; set; }

        /// <summary>
        /// This member specifies a time, in milliseconds, that the caller is willing to wait for the call to complete. This is treated as a hint, and MAY be overridden by the client.
        /// </summary>
        [JsonProperty("timeout")]
        public uint Timeout { get; set; }

        /// <summary>
        /// This OPTIONAL member specifies the relying party identifier claimed by the caller.If omitted, its value will be the CredentialsContainer object’s relevant settings object's origin's effective domain
        /// </summary>
        [JsonProperty("rpId")]
        public string RpId { get; set; }

        /// <summary>
        /// This OPTIONAL member contains a list of PublicKeyCredentialDescriptor objects representing public key credentials acceptable to the caller, in descending order of the caller’s preference(the first item in the list is the most preferred credential, and so on down the list)
        /// </summary>
        [JsonProperty("allowCredentials")]
        public IEnumerable<PublicKeyCredentialDescriptor> AllowCredentials { get; set; }

        /// <summary>
        /// This member describes the Relying Party's requirements regarding user verification for the get() operation. Eligible authenticators are filtered to only those capable of satisfying this requirement
        /// </summary>
        [JsonProperty("userVerification")]
        public UserVerificationRequirement? UserVerification { get; set; }

        /// <summary>
        /// This OPTIONAL member contains additional parameters requesting additional processing by the client and authenticator. For example, if transaction confirmation is sought from the user, then the prompt string might be included as an extension.
        /// </summary>
        [JsonProperty("extensions", NullValueHandling = NullValueHandling.Ignore)]
        public AuthenticationExtensionsClientInputs Extensions { get; set; }

        public static AssertionOptions Create(Fido2Configuration config, byte[] challenge, IEnumerable<PublicKeyCredentialDescriptor> allowedCredentials, UserVerificationRequirement? userVerification, AuthenticationExtensionsClientInputs extensions)
        {
            return new AssertionOptions()
            {
                Status = "ok",
                ErrorMessage = string.Empty,
                Challenge = challenge,
                Timeout = config.Timeout,
                RpId = config.ServerDomain,
                AllowCredentials = allowedCredentials ?? new List<PublicKeyCredentialDescriptor>(),
                UserVerification = userVerification,
                Extensions = extensions
            };
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static AssertionOptions FromJson(string json)
        {
            return JsonConvert.DeserializeObject<AssertionOptions>(json);
        }
    }
}
