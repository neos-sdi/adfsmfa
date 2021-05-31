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
using System;
using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    /// <summary>
    /// Represents the metadata statement.
    /// </summary>
    /// <remarks>
    /// <see href="https://fidoalliance.org/specs/fido-v2.0-rd-20180702/fido-metadata-statement-v2.0-rd-20180702.html#metadata-keys"/>
    /// </remarks>
    public class MetadataStatement
    {
        /// <summary>
        /// Gets or sets the legalHeader, if present, contains a legal guide for accessing and using metadata, which itself MAY contain URL(s) pointing to further information, such as a full Terms and Conditions statement. 
        /// </summary>
        [JsonProperty("legalHeader")]
        public string LegalHeader { get; set; }

        /// <summary>
        /// Gets or set the Authenticator Attestation ID.
        /// </summary>
        /// <remarks>
        /// Note: FIDO UAF Authenticators support AAID, but they don't support AAGUID.
        /// </remarks>
        [JsonProperty("aaid")]
        public string Aaid { get; set; }

        /// <summary>
        /// Gets or sets the Authenticator Attestation GUID. 
        /// </summary>
        /// <remarks>
        /// This field MUST be set if the authenticator implements FIDO 2. 
        /// <para>Note: FIDO 2 Authenticators support AAGUID, but they don't support AAID.</para>
        /// </remarks>
        [JsonProperty("aaguid")]
        public string AaGuid { get; set; }

        /// <summary>
        /// Gets or sets a list of the attestation certificate public key identifiers encoded as hex string.
        /// </summary>
        [JsonProperty("attestationCertificateKeyIdentifiers")]
        public string[] AttestationCertificateKeyIdentifiers { get; set; }

        /// <summary>
        /// Gets or sets a human-readable, short description of the authenticator, in English. 
        /// </summary>
        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or set a list of human-readable short descriptions of the authenticator in different languages.
        /// </summary>
        [JsonProperty("alternativeDescriptions")]
        public AlternativeDescriptions IETFLanguageCodesMembers { get; set; }

        /// <summary>
        /// Gets or set earliest (i.e. lowest) trustworthy authenticatorVersion meeting the requirements specified in this metadata statement. 
        /// </summary>
        [JsonProperty("authenticatorVersion")]
        public ushort AuthenticatorVersion { get; set; }

        /// <summary>
        /// Gets or set the FIDO protocol family.
        /// <para>The values "uaf", "u2f", and "fido2" are supported.</para>
        /// </summary>
        [JsonProperty("protocolFamily")]
        public string ProtocolFamily { get; set; }

        /// <summary>
        /// Gets or sets the FIDO unified protocol version(s) (related to the specific protocol family) supported by this authenticator.
        /// </summary>
        [JsonProperty("upv")]
        public UafVersion[] Upv { get; set; }

        /// <summary>
        /// Gets or sets the assertion scheme supported by the authenticator.
        /// </summary>
        [JsonProperty("assertionScheme")]
        public string AssertionScheme { get; set; }

        /// <summary>
        /// Gets or sets the preferred authentication algorithm supported by the authenticator.
        /// </summary>
        [JsonProperty("authenticationAlgorithm")]
        public ushort AuthenticationAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the list of authentication algorithms supported by the authenticator. 
        /// </summary>
        [JsonProperty("authenticationAlgorithms")]
        public ushort[] AuthenticationAlgorithms { get; set; }

        /// <summary>
        /// Gets or sets the preferred public key format used by the authenticator during registration operations.
        /// </summary>
        [JsonProperty("publicKeyAlgAndEncoding")]
        public ushort PublicKeyAlgAndEncoding { get; set; }

        /// <summary>
        /// Gets or sets the list of public key formats supported by the authenticator during registration operations.
        /// </summary>
        [JsonProperty("publicKeyAlgAndEncodings")]
        public ushort[] PublicKeyAlgAndEncodings { get; set; }

        /// <summary>
        /// Gets or sets the supported attestation type(s).
        /// </summary>
        /// <remarks>
        /// For example: TAG_ATTESTATION_BASIC_FULL(0x3E07), TAG_ATTESTATION_BASIC_SURROGATE(0x3E08). 
        /// </remarks>
        [JsonProperty("attestationTypes")]
        public ushort[] AttestationTypes { get; set; }

        /// <summary>
        /// Gets or sets a list of alternative VerificationMethodANDCombinations.
        /// </summary>
        [JsonProperty("userVerificationDetails")]
        public VerificationMethodDescriptor[][] UserVerificationDetails { get; set; }

        /// <summary>
        /// Gets or sets a 16-bit number representing the bit fields defined by the KEY_PROTECTION constants.
        /// </summary>
        [JsonProperty("keyProtection")]
        public ushort KeyProtection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Uauth private key is restricted by the authenticator to only sign valid FIDO signature assertions.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///     <item>This entry is set to true, if the Uauth private key is restricted by the authenticator to only sign valid FIDO signature assertions.</item>
        ///     <item>This entry is set to false, if the authenticator doesn't restrict the Uauth key to only sign valid FIDO signature assertions. In this case, the calling application could potentially get any hash value signed by the authenticator.</item>
        ///     <item>If this field is missing, the assumed value is isKeyRestricted=true.</item>
        /// </list>
        /// </remarks>
        [JsonProperty("isKeyRestricted")]
        public bool IsKeyRestricted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Uauth key usage always requires a fresh user verification.
        /// </summary>
        [JsonProperty("isFreshUserVerificationRequired")]
        public bool IsFreshUserVerificationRequired { get; set; }

        /// <summary>
        /// Gets or sets a 16-bit number representing the bit fields defined by the MATCHER_PROTECTION constants.
        /// </summary>
        [JsonProperty("matcherProtection")]
        public ushort MatcherProtection { get; set; }

        /// <summary>
        /// Gets or sets the authenticator's overall claimed cryptographic strength in bits (sometimes also called security strength or security level).
        /// </summary>
        /// <remarks>If this value is absent, the cryptographic strength is unknown.</remarks>
        [JsonProperty("cryptoStrength")]
        public ushort CryptoStrength { get; set; }

        /// <summary>
        /// Gets or sets a description of the particular operating environment that is used for the Authenticator.
        /// </summary>
        [JsonProperty("operatingEnv")]
        public string OperatingEnv { get; set; }

        /// <summary>
        /// 
        /// Gets or sets a 32-bit number representing the bit fields defined by the ATTACHMENT_HINT constants.
        /// 
        /// ATTACHMENT_HINT_INTERNAL 0x0001
        /// This flag MAY be set to indicate that the authenticator is permanently attached to the FIDO
        /// user device.
        /// A device such as a smartphone may have authenticator functionality that is able to be used
        /// both locally and remotely.In such a case, the FIDO client MUST filter and exclusively report
        /// only the relevant bit during discovery and when performing policy matching.
        /// This flag cannot be combined with any other ATTACHMENT_HINT flags.
        /// 
        /// ATTACHMENT_HINT_EXTERNAL 0x0002
        /// This flag MAY be set to indicate, for a hardware-based authenticator, that it is removable or
        /// remote from the FIDO user device.
        /// A device such as a smartphone may have authenticator functionality that is able to be used
        /// both locally and remotely. In such a case, the FIDO UAF client MUST filter and exclusively
        /// report only the relevant bit during discovery and when performing policy matching.
        /// 
        /// ATTACHMENT_HINT_WIRED 0x0004
        /// This flag MAY be set to indicate that an external authenticator currently has an exclusive
        /// wired connection, e.g., through USB, Firewire or similar, to the FIDO user device.
        /// 
        /// ATTACHMENT_HINT_WIRELESS 0x0008
        /// This flag MAY be set to indicate that an external authenticator communicates with the FIDO
        /// user device through a personal area or otherwise non-routed wireless protocol, such as
        /// Bluetooth or NFC.
        /// 
        /// ATTACHMENT_HINT_NFC 0x0010
        /// This flag MAY be set to indicate that an external authenticator is able to communicate by
        /// NFC to the FIDO user device. As part of authenticator metadata, or when reporting
        /// characteristics through discovery, if this flag is set, the ATTACHMENT_HINT_WIRELESS flag
        /// SHOULD also be set as well.
        /// 
        /// ATTACHMENT_HINT_BLUETOOTH 0x0020
        /// This flag MAY be set to indicate that an external authenticator is able to communicate using
        /// Bluetooth with the FIDO user device.As part of authenticator metadata, or when reporting
        /// characteristics through discovery, if this flag is set, the ATTACHMENT_HINT_WIRELESS flag
        /// SHOULD also be set.
        /// 
        /// ATTACHMENT_HINT_NETWORK 0x0040
        /// This flag MAY be set to indicate that the authenticator is connected to the FIDO user device
        /// over a non-exclusive network (e.g., over a TCP/IP LAN or WAN, as opposed to a PAN or
        /// point-to-point connection).
        /// 
        /// ATTACHMENT_HINT_READY 0x0080
        /// This flag MAY be set to indicate that an external authenticator is in a "ready" state.This flag
        /// is set by the ASM at its discretion.
        /// 
        /// NOTE – Generally this should indicate that the device is immediately available to perform user verification
        /// without additional actions such as connecting the device or creating a new biometric profile enrollment, but
        /// the exact meaning may vary for different types of devices.For example, a USB authenticator may only report
        /// itself as ready when it is plugged in, or a Bluetooth authenticator when it is paired and connected, but an NFCbased authenticator may always report itself as ready.
        /// </summary>
        [JsonProperty("attachmentHint")]
        public ulong AttachmentHint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the authenticator is designed to be used only as a second factor, i.e. requiring some other authentication method as a first factor.
        /// </summary>
        [JsonProperty("isSecondFactorOnly")]
        public bool IsSecondFactorOnly { get; set; }

        /// <summary>
        /// Gets or sets a 16-bit number representing a combination of the bit flags defined by the TRANSACTION_CONFIRMATION_DISPLAY constants.
        /// </summary>
        [JsonProperty("tcDisplay")]
        public ushort TcDisplay { get; set; }

        /// <summary>
        /// Gets or sets the supported MIME content type [RFC2049] for the transaction confirmation display, such as text/plain or image/png. 
        /// </summary>
        [JsonProperty("tcDisplayContentType")]
        public string TcDisplayContentType { get; set; }

        /// <summary>
        /// Gets or sets a list of alternative DisplayPNGCharacteristicsDescriptor.
        /// </summary>
        [JsonProperty("tcDisplayPNGCharacteristics")]
        public DisplayPNGCharacteristicsDescriptor[] TcDisplayPNGCharacteristics { get; set; }

        /// <summary>
        /// Gets or sets a list of a PKIX [RFC5280] X.509 certificate that is a valid trust anchor for this authenticator model.
        /// </summary>
        [JsonProperty("attestationRootCertificates")]
        public string[] AttestationRootCertificates { get; set; }

        /// <summary>
        /// Gets or set a list of trust anchors used for ECDAA attestation. 
        /// </summary>
        [JsonProperty("ecdaaTrustAnchors")]
        public EcdaaTrustAnchor[] EcdaaTrustAnchors { get; set; }

        /// <summary>
        /// Gets or set a data: url [RFC2397] encoded PNG [PNG] icon for the Authenticator.
        /// </summary>
        [JsonProperty("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets a list of extensions supported by the authenticator. 
        /// </summary>
        [JsonProperty("supportedExtensions")]
        public ExtensionDescriptor[] SupportedExtensions { get; set; }

        /// <summary>
        /// Gets or sets a computed hash value of this <see cref="MetadataStatement"/>.
        /// <para>NOTE: This supports the internal infrastructure of Fido2Net and isn't intented to be used by user code.</para>
        /// </summary>
        public string Hash { get; set; }
    }
}
