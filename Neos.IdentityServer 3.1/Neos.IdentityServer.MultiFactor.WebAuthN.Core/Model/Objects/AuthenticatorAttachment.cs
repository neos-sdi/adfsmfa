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
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Objects
{
    /// <summary>
    /// This enumeration’s values describe authenticators' attachment modalities. Relying Parties use this for two purposes:
    /// to express a preferred authenticator attachment modality when calling navigator.credentials.create() to create a credential, and
    /// to inform the client of the Relying Party's best belief about how to locate the managing authenticators of the credentials listed in allowCredentials when calling navigator.credentials.get().
    /// </summary>
    /// <remarks>
    /// Note: An authenticator attachment modality selection option is available only in the [[Create]](origin, options, sameOriginWithAncestors) operation. The Relying Party may use it to, for example, ensure the user has a roaming credential for authenticating on another client device; or to specifically register a platform credential for easier reauthentication using a particular client device. The [[DiscoverFromExternalSource]](origin, options, sameOriginWithAncestors) operation has no authenticator attachment modality selection option, so the Relying Party SHOULD accept any of the user’s registered credentials. The client and user will then use whichever is available and convenient at the time.
    /// https://w3c.github.io/webauthn/#attachment
    /// </remarks>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuthenticatorAttachment
    {
        /// <summary>
        /// This value indicates platform attachment
        /// </summary>
        [EnumMember(Value = "platform")]
        Platform,

        /// <summary>
        /// This value indicates cross-platform attachment.
        /// </summary>
        [EnumMember(Value = "cross-platform")]
        CrossPlatform
    }
}
