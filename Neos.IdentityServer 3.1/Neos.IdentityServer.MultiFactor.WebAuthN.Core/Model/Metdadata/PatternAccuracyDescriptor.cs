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

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    /// <summary>
    /// The PatternAccuracyDescriptor describes relevant accuracy/complexity aspects in the case that a pattern is used as the user verification method.
    /// </summary>
    /// <remarks>
    /// <see href="https://fidoalliance.org/specs/fido-v2.0-rd-20180702/fido-metadata-statement-v2.0-rd-20180702.html#patternaccuracydescriptor-dictionary"/>
    /// </remarks>
    public class PatternAccuracyDescriptor
    {
        /// <summary>
        /// Gets or sets the number of possible patterns (having the minimum length) out of which exactly one would be the right one, i.e. 1/probability in the case of equal distribution.
        /// </summary>
        [JsonProperty("minComplexity", Required = Required.Always)]
        public ulong MinComplexity { get; set; }

        /// <summary>
        /// Gets or sets maximum number of false attempts before the authenticator will block authentication using this method (at least temporarily). 
        /// <para>Zero (0) means it will never block.</para>
        /// </summary>
        [JsonProperty("maxRetries")]
        public ushort MaxRetries { get; set; }

        /// <summary>
        /// Gets or sets the enforced minimum number of seconds wait time after blocking (due to forced reboot or similar mechanism).
        /// <para>Zero (0) means this user verification method will be blocked, either permanently or until an alternative user verification method method succeeded.</para>
        /// </summary>
        /// <remarks>
        /// All alternative user verification methods MUST be specified appropriately in the metadata under userVerificationDetails.
        /// </remarks>
        [JsonProperty("blockSlowdown")]
        public ushort BlockSlowdown { get; set; }
    }
}
