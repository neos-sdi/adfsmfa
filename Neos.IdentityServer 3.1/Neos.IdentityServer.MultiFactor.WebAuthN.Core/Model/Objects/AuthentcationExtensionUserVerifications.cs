using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Objects
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserVerification
    {
        /// <summary>
        /// This reflects "FIDO_2_0" semantics. In this configuration, user verification is optional with or without credentialID list. This is the default state of the credential if the extension is not specified and the authenticator does not report a defaultCredProtect value in the authenticatorGetInfo response.
        /// </summary>
        [EnumMember(Value = "userVerificationOptional")]
        Optional = 0x1,

        /// <summary>
        /// In this configuration, credential is discovered only when its credentialID is provided by the platform or when user verification is performed.
        /// </summary>
        [EnumMember(Value = "userVerificationOptionalWithCredentialIDList")]
        OptionalWithCredentialIDList = 0x2,

        /// <summary>
        /// This reflects that discovery and usage of the credential MUST be preceeded by user verification.
        /// </summary>
        [EnumMember(Value = "userVerificationRequired")]
        Required = 0x3
    }
}