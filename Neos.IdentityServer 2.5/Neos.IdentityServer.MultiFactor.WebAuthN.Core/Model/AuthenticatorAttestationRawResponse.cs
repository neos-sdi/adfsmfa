using Newtonsoft.Json;
using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public class AuthenticatorAttestationRawResponse
    {
        [JsonConverter(typeof(Base64UrlConverter))]
        public byte[] Id { get; set; }

        [JsonConverter(typeof(Base64UrlConverter))]
        public byte[] RawId { get; set; }

        public PublicKeyCredentialType? Type { get; set; }

        public ResponseData Response { get; set; }

        public AuthenticationExtensionsClientOutputs Extensions { get; set; }

        public class ResponseData
        {
            [JsonConverter(typeof(Base64UrlConverter))]
            public byte[] AttestationObject { get; set; }
            [JsonConverter(typeof(Base64UrlConverter))]
            public byte[] ClientDataJson { get; set; }
        }
    }
}
