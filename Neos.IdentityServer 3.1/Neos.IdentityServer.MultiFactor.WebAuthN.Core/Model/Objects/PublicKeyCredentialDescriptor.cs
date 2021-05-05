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
    /// <summary>
    /// This object contains the attributes that are specified by a caller when referring to a public key credential as an input parameter to the create() or get() methods. It mirrors the fields of the PublicKeyCredential object returned by the latter methods.
    /// Lazy implementation of https://www.w3.org/TR/webauthn/#dictdef-publickeycredentialdescriptor
    /// todo: Should add validation of values as specified in spec
    /// </summary>
    public class PublicKeyCredentialDescriptor
    {
        public PublicKeyCredentialDescriptor(byte[] credentialId)
        {
            Id = credentialId;
        }

        public PublicKeyCredentialDescriptor()
        {

        }

        /// <summary>
        /// This member contains the type of the public key credential the caller is referring to.
        /// </summary>
        [JsonProperty("type")]
        public PublicKeyCredentialType? Type { get; set; } = PublicKeyCredentialType.PublicKey;

        /// <summary>
        /// This member contains the credential ID of the public key credential the caller is referring to.
        /// </summary>
        [JsonConverter(typeof(Base64UrlConverter))]
        [JsonProperty("id")]
        public byte[] Id { get; set; }

        /// <summary>
        /// This OPTIONAL member contains a hint as to how the client might communicate with the managing authenticator of the public key credential the caller is referring to.
        /// </summary>
        [JsonProperty("transports", NullValueHandling = NullValueHandling.Ignore)]
        public AuthenticatorTransport[] Transports { get; set; }
    };
}
