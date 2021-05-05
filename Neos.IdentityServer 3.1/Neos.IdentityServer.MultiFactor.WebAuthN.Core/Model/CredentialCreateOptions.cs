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
using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    /// <summary>
    /// RegisterCredentialOptions class implementation
    /// </summary>
    public class RegisterCredentialOptions : Fido2ResponseBase
    {
        /// <summary>
        /// Create static method implementation
        /// </summary>
        public static RegisterCredentialOptions Create(Fido2Configuration config, byte[] challenge, Fido2User user, AuthenticatorSelection authenticatorSelection, AttestationConveyancePreference attestationConveyancePreference, List<PublicKeyCredentialDescriptor> excludeCredentials, AuthenticationExtensionsClientInputs extensions)
        {
            return new RegisterCredentialOptions
            {
                Status = "ok",
                ErrorMessage = string.Empty,
                Challenge = challenge,
                Rp = new PublicKeyCredentialRpEntity(config.ServerDomain, config.ServerName, config.ServerIcon),
                Timeout = config.Timeout,
                User = user,
                PubKeyCredParams = new List<PubKeyCredParam>()
                {
                    // Add additional as appropriate
                    ES256,
                    RS256,
                    PS256,
                    ES384,
                    RS384,
                    PS384,
                    ES512,
                    RS512,
                    PS512,
                },
                AuthenticatorSelection = authenticatorSelection,
                Attestation = attestationConveyancePreference,
                ExcludeCredentials = excludeCredentials ?? new List<PublicKeyCredentialDescriptor>(),
                Extensions = extensions
            };
        }

        #region Json Properties
        /// <summary>
        /// This member contains data about the Relying Party responsible for the request.
        /// Its value’s name member is required.
        /// Its value’s id member specifies the relying party identifier with which the credential should be associated.If omitted, its value will be the CredentialsContainer object’s relevant settings object's origin's effective domain.
        /// </summary>
        [JsonProperty("rp")]
        public PublicKeyCredentialRpEntity Rp { get; set; }

        /// <summary>
        /// This member contains data about the user account for which the Relying Party is requesting attestation. 
        /// Its value’s name, displayName and id members are required.
        /// </summary>
        [JsonProperty("user")]
        public Fido2User User { get; set; }

        /// <summary>
        /// Must be generated by the Server (Relying Party)
        /// </summary>
        [JsonProperty("challenge")]
        [JsonConverter(typeof(Base64UrlConverter))]
        public byte[] Challenge { get; set; }

        /// <summary>
        /// This member contains information about the desired properties of the credential to be created. The sequence is ordered from most preferred to least preferred. The platform makes a best-effort to create the most preferred credential that it can.
        /// </summary>
        [JsonProperty("pubKeyCredParams")]
        public List<PubKeyCredParam> PubKeyCredParams { get; set; }

        /// <summary>
        /// This member specifies a time, in milliseconds, that the caller is willing to wait for the call to complete. This is treated as a hint, and MAY be overridden by the platform.
        /// </summary>
        [JsonProperty("timeout")]
        public uint Timeout { get; set; }

        /// <summary>
        /// This member is intended for use by Relying Parties that wish to express their preference for attestation conveyance.The default is none.
        /// </summary>
        [JsonProperty("attestation")]
        public AttestationConveyancePreference Attestation { get; set; } = AttestationConveyancePreference.None;
        
        /// <summary>
        /// This member is intended for use by Relying Parties that wish to select the appropriate authenticators to participate in the create() operation.
        /// </summary>
        [JsonProperty("authenticatorSelection")]
        public AuthenticatorSelection AuthenticatorSelection { get; set; }

        /// <summary>
        /// This member is intended for use by Relying Parties that wish to limit the creation of multiple credentials for the same account on a single authenticator.The client is requested to return an error if the new credential would be created on an authenticator that also contains one of the credentials enumerated in this parameter.
        /// </summary>
        [JsonProperty("excludeCredentials")]
        public List<PublicKeyCredentialDescriptor> ExcludeCredentials { get; set; }

        /// <summary>
        /// This OPTIONAL member contains additional parameters requesting additional processing by the client and authenticator. For example, if transaction confirmation is sought from the user, then the prompt string might be included as an extension.
        /// </summary>
        [JsonProperty("extensions", NullValueHandling = NullValueHandling.Ignore)]
        public AuthenticationExtensionsClientInputs Extensions { get; set; }
        #endregion       

        #region Serialization methods
        /// <summary>
        /// ToJson method implementation
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// FromJson method implementation
        /// </summary>
        public static RegisterCredentialOptions FromJson(string json)
        {
            return JsonConvert.DeserializeObject<RegisterCredentialOptions>(json);
        }
        #endregion

        #region PublicKeys Credential Params
        /// <summary>
        /// ES256
        /// </summary>
        private static readonly PubKeyCredParam ES256 = new PubKeyCredParam()
        {
            // External authenticators support the ES256 algorithm
            Type = PublicKeyCredentialType.PublicKey,
            Alg = -7
        };

        /// <summary>
        /// ES384
        /// </summary>
        private static readonly PubKeyCredParam ES384 = new PubKeyCredParam()
        {
            Type = PublicKeyCredentialType.PublicKey,
            Alg = -35
        };

        /// <summary>
        /// ES512
        /// </summary>
        private static readonly PubKeyCredParam ES512 = new PubKeyCredParam()
        {
            Type = PublicKeyCredentialType.PublicKey,
            Alg = -36
        };

        /// <summary>
        /// RS256
        /// </summary>
        private static readonly PubKeyCredParam RS256 = new PubKeyCredParam()
        {
            // Windows Hello supports the RS256 algorithm
            Type = PublicKeyCredentialType.PublicKey,
            Alg = -257
        };

        /// <summary>
        /// RS384
        /// </summary>
        private static readonly PubKeyCredParam RS384 = new PubKeyCredParam()
        {
            Type = PublicKeyCredentialType.PublicKey,
            Alg = -258
        };

        /// <summary>
        /// RS512
        /// </summary>
        private static readonly PubKeyCredParam RS512 = new PubKeyCredParam()
        {
            Type = PublicKeyCredentialType.PublicKey,
            Alg = -259
        };

        /// <summary>
        /// PS256
        /// </summary>
        private static readonly PubKeyCredParam PS256 = new PubKeyCredParam()
        {
            Type = PublicKeyCredentialType.PublicKey,
            Alg = -37
        };

        /// <summary>
        /// PS384
        /// </summary>
        private static readonly PubKeyCredParam PS384 = new PubKeyCredParam()
        {
            Type = PublicKeyCredentialType.PublicKey,
            Alg = -38
        };

        /// <summary>
        /// PS512
        /// </summary>
        private static readonly PubKeyCredParam PS512 = new PubKeyCredParam()
        {
            Type = PublicKeyCredentialType.PublicKey,
            Alg = -39
        };
        #endregion
    }

    /// <summary>
    /// PubKeyCredParam class implementation
    /// </summary>
    public class PubKeyCredParam
    {
        /// <summary>
        /// The type member specifies the type of credential to be created.
        /// </summary>
        [JsonProperty("type")]
        public PublicKeyCredentialType Type { get; set; }

        /// <summary>
        /// The alg member specifies the cryptographic signature algorithm with which the newly generated credential will be used, and thus also the type of asymmetric key pair to be generated, e.g., RSA or Elliptic Curve.
        /// </summary>
        [JsonProperty("alg")]
        public long Alg { get; set; }
    }

    /// <summary>
    /// PublicKeyCredentialRpEntity class implementation
    /// </summary>
    public class PublicKeyCredentialRpEntity
    {
        /// <summary>
        /// PublicKeyCredentialRpEntity constructor
        /// </summary>
        public PublicKeyCredentialRpEntity(string id, string name, string icon)
        {
            Name = name;
            Id = id;
            Icon = icon;
        }

        /// <summary>
        /// A unique identifier for the Relying Party entity, which sets the RP ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// A human-readable name for the entity. Its function depends on what the PublicKeyCredentialEntity represents:
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("icon", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Icon { get; set; }
    }

    /// <summary>
    /// AuthenticatorSelection class implementation
    /// </summary>
    /// <Param>WebAuthn Relying Parties may use the AuthenticatorSelectionCriteria dictionary to specify their requirements regarding authenticator attributes.</Param>
    public class AuthenticatorSelection
    {
        /// <summary>
        /// AuthenticatorSelection Default property
        /// </summary>
        public static AuthenticatorSelection Default => new AuthenticatorSelection
        {
            AuthenticatorAttachment = null,
            RequireResidentKey = false,
            UserVerification = UserVerificationRequirement.Preferred
        };

        /// <summary>
        /// If this member is present, eligible authenticators are filtered to only authenticators attached with the specified §5.4.5 Authenticator Attachment enumeration (enum AuthenticatorAttachment).
        /// </summary>
        [JsonProperty("authenticatorAttachment", NullValueHandling = NullValueHandling.Ignore)]
        public AuthenticatorAttachment? AuthenticatorAttachment { get; set; }

        /// <summary>
        /// This member describes the Relying Parties' requirements regarding resident credentials. If the parameter is set to true, the authenticator MUST create a client-side-resident public key credential source when creating a public key credential.
        /// </summary>
        [JsonProperty("requireResidentKey")]
        public bool RequireResidentKey { get; set; }

        /// <summary>
        /// This member describes the Relying Party's requirements regarding user verification for the create() operation. Eligible authenticators are filtered to only those capable of satisfying this requirement.
        /// </summary>
        [JsonProperty("userVerification")]
        public UserVerificationRequirement UserVerification { get; set; }
    }

    public class Fido2User
    {

        /// <summary>
        /// Required. A human-friendly identifier for a user account. It is intended only for display, i.e., aiding the user in determining the difference between user accounts with similar displayNames. For example, "alexm", "alex.p.mueller@example.com" or "+14255551234". https://w3c.github.io/webauthn/#dictdef-publickeycredentialentity
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The user handle of the user account entity. To ensure secure operation, authentication and authorization decisions MUST be made on the basis of this id member, not the displayName nor name members
        /// </summary>
        [JsonProperty("id")]
        [JsonConverter(typeof(Base64UrlConverter))]
        public byte[] Id { get; set; }

        /// <summary>
        /// A human-friendly name for the user account, intended only for display. For example, "Alex P. Müller" or "田中 倫". The Relying Party SHOULD let the user choose this, and SHOULD NOT restrict the choice more than necessary.
        /// </summary>
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }
}
