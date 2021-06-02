//******************************************************************************************************************************************************************************************//
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
    /// Represents the the ECDAA attestation data.
    /// </summary>
    /// <remarks>
    /// <see href="https://fidoalliance.org/specs/fido-v2.0-rd-20180702/fido-metadata-statement-v2.0-rd-20180702.html#ecdaatrustanchor-dictionary"/>
    /// <para>In the case of ECDAA attestation, the ECDAA-Issuer's trust anchor MUST be specified in this field.</para>
    /// </remarks>
    public class EcdaaTrustAnchor
    {
        /// <summary>
        /// Gets or sets a base64url encoding of the result of ECPoint2ToB of the ECPoint2 X=P2​x​​.
        /// </summary>
        [JsonProperty("x", Required = Required.Always)]
        public string X { get; set; }
        /// <summary>
        /// Gets or sets a base64url encoding of the result of ECPoint2ToB of the ECPoint2.
        /// </summary>
        [JsonProperty("y", Required = Required.Always)]
        public string Y { get; set; }
        /// <summary>
        /// Gets or sets a base64url encoding of the result of BigNumberToB(c).
        /// </summary>
        [JsonProperty("c", Required = Required.Always)]
        public string C { get; set; }
        /// <summary>
        /// Gets or sets the base64url encoding of the result of BigNumberToB(sx).
        /// </summary>
        [JsonProperty("sx", Required = Required.Always)]
        public string SX { get; set; }
        /// <summary>
        /// Gets or sets the base64url encoding of the result of BigNumberToB(sy).
        /// </summary>
        [JsonProperty("sy", Required = Required.Always)]
        public string SY { get; set; }
        /// <summary>
        /// Gets or sets a name of the Barreto-Naehrig elliptic curve for G1.
        /// <para>"BN_P256", "BN_P638", "BN_ISOP256", and "BN_ISOP512" are supported.</para>
        /// </summary>
        [JsonProperty("G1Curve", Required = Required.Always)]
        public string G1Curve { get; set; }
    }
}
