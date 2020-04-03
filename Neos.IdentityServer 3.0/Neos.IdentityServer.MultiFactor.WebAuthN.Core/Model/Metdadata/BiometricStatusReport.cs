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
    /// Contains the current BiometricStatusReport of one of the authenticator's biometric component.
    /// </summary>
    /// <remarks>
    /// <see href="https://fidoalliance.org/specs/fido-v2.0-rd-20180702/fido-metadata-service-v2.0-rd-20180702.html#biometricstatusreport-dictionary"/>
    /// </remarks>
    public class BiometricStatusReport
    {
        /// <summary>
        /// Gets or sets the level of the biometric certification of this biometric component of the authenticator.
        /// </summary>
        [JsonProperty("certLevel", Required = Required.Always)]
        public ushort CertLevel { get; set; }
        /// <summary>
        /// Gets or sets a single USER_VERIFY constant indicating the modality of the biometric component.
        /// </summary>
        /// <remarks>
        /// This is not a bit flag combination. 
        /// This value MUST be non-zero and this value MUST correspond to one or more entries in field userVerificationDetails in the related Metadata Statement.
        /// </remarks>
        [JsonProperty("modality", Required = Required.Always)]
        public ulong Modality { get; set; }
        /// <summary>
        /// Gets or sets a ISO-8601 formatted date since when the certLevel achieved, if applicable. 
        /// <para>If no date is given, the status is assumed to be effective while present.</para>
        /// </summary>
        [JsonProperty("effectiveDate")]
        public string EffectiveDate { get; set; }
        /// <summary>
        /// Gets or sets the externally visible aspects of the Biometric Certification evaluation.
        /// </summary>
        [JsonProperty("certificationDescriptor")]
        public string CertificationDescriptor { get; set; }
        /// <summary>
        /// Gets or sets the unique identifier for the issued Biometric Certification.
        /// </summary>
        [JsonProperty("certificateNumber")]
        public string CertificateNumber { get; set; }
        /// <summary>
        /// Gets or sets the  version of the Biometric Certification Policy the implementation is Certified to.
        /// </summary>
        /// <remarks>
        /// For example: "1.0.0".
        /// </remarks>
        [JsonProperty("certificationPolicyVersion")]
        public string CertificationPolicyVersion { get; set; }
        /// <summary>
        /// Gets or sets the version of the Biometric Requirements the implementation is certified to.
        /// </summary>
        /// <remarks>
        /// For example: "1.0.0".
        /// </remarks>
        [JsonProperty("certificationRequirementsVersion")]
        public string CertificationRequirementsVersion { get; set; }
    }
}
