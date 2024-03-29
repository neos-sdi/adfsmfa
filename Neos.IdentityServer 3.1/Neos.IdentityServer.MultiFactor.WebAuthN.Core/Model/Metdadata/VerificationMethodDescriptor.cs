﻿//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
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

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    /// <summary>
    /// A descriptor for a specific base user verification method as implemented by the authenticator.
    /// </summary>
    /// <remarks>
    /// <see href="https://fidoalliance.org/specs/fido-v2.0-rd-20180702/fido-metadata-statement-v2.0-rd-20180702.html#verificationmethoddescriptor-dictionary"/>
    /// </remarks>
    public class VerificationMethodDescriptor
    {
        /// <summary>
        /// Gets or sets a single USER_VERIFY constant, not a bit flag combination. 
        /// </summary>
        /// <remarks>
        /// This value MUST be non-zero.
        /// </remarks>
        [JsonProperty("userVerificationMethod")]
        public string UserVerificationMethod { get; set; }
        /// <summary>
        /// Gets or sets a may optionally be used in the case of method USER_VERIFY_PASSCODE.
        /// </summary>
        [JsonProperty("caDesc")]
        public CodeAccuracyDescriptor CaDesc { get; set; }
        /// <summary>
        /// Gets or sets a may optionally be used in the case of method USER_VERIFY_FINGERPRINT, USER_VERIFY_VOICEPRINT, USER_VERIFY_FACEPRINT, USER_VERIFY_EYEPRINT, or USER_VERIFY_HANDPRINT.
        /// </summary>
        [JsonProperty("baDesc")]
        public BiometricAccuracyDescriptor BaDesc { get; set; }
        /// <summary>
        /// Gets or sets a may optionally be used in case of method USER_VERIFY_PATTERN.
        /// </summary>
        [JsonProperty("paDesc")]
        public PatternAccuracyDescriptor PaDesc { get; set; }
    }
}
